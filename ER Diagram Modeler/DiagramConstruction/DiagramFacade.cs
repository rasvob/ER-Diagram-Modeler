using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Extintions;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Canvas.TableItem;
using MahApps.Metro.Controls.Dialogs;
using Pathfinding;
using Pathfinding.Structure;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.DiagramConstruction
{
	public class DiagramFacade
	{
		public DatabaseModelDesignerViewModel ViewModel { get; set; }
		public Action<TableModel> AddTableCallbackAction { get; set; }

		public DiagramFacade(DatabaseModelDesignerViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public DiagramFacade(DatabaseModelDesigner designer)
		{
			ViewModel = designer.ViewModel;
		}

		public bool AddTable(TableModel source)
		{
			return AddTable(source, -1, -1);
		}

		public bool AddTable(string name, int x, int y)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			if(ViewModel.TableViewModels.Any(t => t.Model.Title.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
			{
				return false;
			}

			ctx.CreateTable(name);

			TableModel model = ctx.ListTables().FirstOrDefault(t => t.Title.Equals(name, StringComparison.InvariantCultureIgnoreCase));

			if (model == null)
			{
				return false;
			}

			return AddTable(model, x, y);
		}

		public bool AddTable(TableModel source, int x, int y)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			if(ViewModel.TableViewModels.Any(t => t.Model.Title.Equals(source.Title)))
			{
				return false;
			}

			TableViewModel vm = CreateTableViewModel(source, ctx);
			AddTableViewModel(vm, x, y);
			return true;
		}

		public async Task AddRelationShipsForTable(TableModel model, DesignerCanvas canvas)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			var relationships = ctx.ListRelationshipsForTable(model.Title, ViewModel.TableViewModels.Select(t => t.Model)).Where(t => !ViewModel.ConnectionInfoViewModels.Any(s => s.RelationshipModel.Name.Equals(t.Name)));

			foreach (RelationshipModel relationship in relationships)
			{
				await AddRelationship(relationship, canvas);
			}
		}

		public async Task AddRelationship(RelationshipModel relationship, DesignerCanvas canvas)
		{
			ConnectionInfoViewModel vm = new ConnectionInfoViewModel();
			vm.DesignerCanvas = canvas;
			vm.RelationshipModel = relationship;
			vm.SourceViewModel = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Equals(relationship.Source));
			vm.DestinationViewModel = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Equals(relationship.Destination));
			await vm.BuildConnection3(ViewModel);
			ViewModel.ConnectionInfoViewModels.Add(vm);
		}

		public void RemoveTable(TableModel model)
		{
			var table = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Equals(model));

			if (table != null)
			{
				ViewModel.TableViewModels.Remove(table);
			}
		}

		private TableViewModel CreateTableViewModel(TableModel source, DatabaseContext ctx)
		{
			TableModel model = ctx.ReadTableDetails(source.Id, source.Title);
			model.Title = source.Title;
			model.Id = source.Id;
			TableViewModel vm = new TableViewModel()
			{
				Model = model
			};
			return vm;
		}

		private void AddTableViewModel(TableViewModel vm, int x, int y)
		{
			vm.Left = x > -1 ? x : 0;
			vm.Top = y > -1 ? y : 0;

			vm.TableLoaded += (sender, content) =>
			{
				if (x < 0 || y < 0)
				{
					UpdateTablePosition(content);
				}
				AddTableCallbackAction?.Invoke(vm.Model);
			};

			ViewModel.TableViewModels.Add(vm);
		}

		public async Task RefreshDiagram(DesignerCanvas canvas)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			IEnumerable<TableModel> tables = ctx.ListTables();
			IEnumerable<string> foreignKeys = ctx.ListAllForeignKeys();

			var tablesForDelete = ViewModel.TableViewModels.Where(t => !tables.Any(s => s.Id.Equals(t.Model.Id))).ToList();
			tablesForDelete.ForEach(t => ViewModel.TableViewModels.Remove(t));

			var relationsForDelete =
				ViewModel.ConnectionInfoViewModels.Where(t => !foreignKeys.Any(s => s.Equals(t.RelationshipModel.Name))).ToList();
			relationsForDelete.ForEach(t => ViewModel.ConnectionInfoViewModels.Remove(t));

			foreach (TableModel table in tables)
			{
				var ft = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Id.Equals(table.Id));

				if (ft == null)
				{
					continue;
				}

				ft.Model.Title = table.Title;
			}


			foreach(TableViewModel viewModel in ViewModel.TableViewModels)
			{
				TableModel model = ctx.ReadTableDetails(viewModel.Model.Id, viewModel.Model.Title);
				viewModel.Model.RefreshModel(model);

				IEnumerable<RelationshipModel> relationshipModels = ctx.ListRelationshipsForTable(viewModel.Model.Title, ViewModel.TableViewModels.Select(t => t.Model));
				IEnumerable<RelationshipModel> filtered = relationshipModels.Where(t => !ViewModel.ConnectionInfoViewModels.Any(s => s.RelationshipModel.Name.Equals(t.Name)));

				foreach (RelationshipModel relationshipModel in filtered)
				{
					await AddRelationship(relationshipModel, canvas);
				}
			}
		}

		public void LoadDiagram(DesignerCanvas canvas, XDocument data)
		{
			var root = data.Root;
			ViewModel.LoadFromElement(data.Root);

			if (ViewModel.CanvasWidth != DatabaseModelDesignerViewModel.DefaultWidth || ViewModel.CanvasHeight != DatabaseModelDesignerViewModel.DefaultWidth)
			{
				StreamGeometry geometry = DesignerCanvas.CreateGridWithStreamGeometry(ViewModel.CanvasHeight, ViewModel.CanvasWidth,
					DesignerCanvas.GridCellWidth);
				canvas.RefreshGuideLines(geometry);
			}
			
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			var tablesInDb = ctx.ListTables();
			var tableElements = root?.XPathSelectElements("TableViewModels/TableViewModel")
				.Select(t =>
				{
					var vm = new TableViewModel();
					vm.LoadFromElement(t);
					return vm;
				})
				.Where(s => tablesInDb.Any(t => t.Id.Equals(s.Model.Id)))
				.ToList();

			if (tableElements == null || !tableElements.Any())
			{
				return;	
			}

			tableElements?.ForEach(t => t.Model.RefreshModel(ctx.ReadTableDetails(t.Model.Id, t.Model.Title)));
			tableElements?.ForEach(t => ViewModel.TableViewModels.Add(t));

			var relationShipsInDb = ctx.ListAllForeignKeys();
			var allRelationshipsDetails = tableElements?
				.SelectMany(t => ctx.ListRelationshipsForTable(t.Model.Title, tableElements.Select(s => s.Model)))
				.GroupBy(t => t.Name)
				.Select(t => t.FirstOrDefault())
				.ToList();

			var relationElements = root?.XPathSelectElements("ConnectionInfoViewModels/ConnectionInfoViewModel")
				.Select(t =>
				{
					var vm = new ConnectionInfoViewModel {DesignerCanvas = canvas};
					vm.LoadFromElement(t);
					return vm;
				})
				.Where(t => relationShipsInDb.Any(s => s.Equals(t.RelationshipModel.Name)))
				.Where(t => allRelationshipsDetails.Any(s => ctx.AreRelationshipModelsTheSame(t.RelationshipModel, s)))
				.ToList();

			
			relationElements.ForEach(t =>
			{
				t.SourceViewModel = tableElements.FirstOrDefault(s => s.Model.Id.Equals(t.RelationshipModel.Source.Id));
				t.DestinationViewModel = tableElements.FirstOrDefault(s => s.Model.Id.Equals(t.RelationshipModel.Destination.Id));
				t.RelationshipModel.RefreshModel(allRelationshipsDetails.FirstOrDefault(s => ctx.AreRelationshipModelsTheSame(t.RelationshipModel, s)));
				ViewModel.ConnectionInfoViewModels.Add(t);
				t.BuildLoadedConnection();
			});

			var newRelations =
				allRelationshipsDetails
					.Where(t => !relationElements.Any(s => s.RelationshipModel.Name.Equals(t.Name)))
					.Select(t =>
					{
						var vm = new ConnectionInfoViewModel
						{
							DesignerCanvas = canvas,
							SourceViewModel = tableElements.FirstOrDefault(s => s.Model.Id.Equals(t.Source.Id)),
							DestinationViewModel = tableElements.FirstOrDefault(s => s.Model.Id.Equals(t.Destination.Id))
						};
						vm.RelationshipModel.RefreshModel(t);
						return vm;
					})
					.ToList();

			newRelations.ForEach(async t =>
			{
				ViewModel.ConnectionInfoViewModels.Add(t);
				await t.BuildConnection3(ViewModel);
			});
		}

		public int SaveDiagram()
		{
			XDocument doc = XDocument.Parse(ViewModel.CreateElement().ToString());
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			return ctx.SaveDiagram(ViewModel.DiagramTitle, doc);
		}

#region HELPERS

		private void UpdateTablePosition(TableContent table)
		{
			int step = 100;
			var grid = CreateMinifiedGridForPathFindingSync(ViewModel, step);
			var finder = new FreeRectangleFinder(grid);

			var rect = GetTableRectangles(new[] {table.TableViewModel}, step).Select(s =>
			{
				var t = s.Y / step;
				var l = s.X / step;
				var r = s.Right / step;
				var b = s.Bottom / step;
				return new Rectangle(l, t, r - l, b - t);
			}).FirstOrDefault();

			var res = finder.FindFreeRectangle(rect);

			if (res.HasValue)
			{
				var realPoint = res.Value.FromMinified(step);
				table.TableViewModel.Top = realPoint.Y;
				table.TableViewModel.Left = realPoint.X;

				DesignerCanvas.SetTop(table, realPoint.Y);
				DesignerCanvas.SetLeft(table, realPoint.X);
			}
		}

		public static void CreateNewDiagram(MainWindow window, string title)
		{
			LayoutAnchorable anchorable = new LayoutAnchorable()
			{
				CanClose = true,
				CanHide = false,
				CanFloat = true,
				CanAutoHide = false,
				Title = title,
				ContentId = $"{title}_ID"
			};
			
			DatabaseModelDesignerViewModel designerViewModel = new DatabaseModelDesignerViewModel()
			{
				DiagramTitle = title
			};

			window.MainWindowViewModel.DatabaseModelDesignerViewModels.Add(designerViewModel);

			DatabaseModelDesigner designer = new DatabaseModelDesigner()
			{
				ViewModel = designerViewModel,
			};

			designer.TableCreated += window.CreateTableHandler;
			anchorable.Content = designer;
			anchorable.IsActiveChanged += window.AnchorableDesignerActiveChangedHandler;
			window.MainDocumentPane.Children.Add(anchorable);
			int indexOf = window.MainDocumentPane.Children.IndexOf(anchorable);
			window.MainDocumentPane.SelectedContentIndex = indexOf;
		}

		public static IEnumerable<Rectangle> GetTableRectangles(IEnumerable<TableViewModel> tables, int step = 1)
		{
			return tables.Select(t =>
			{
				int top = (int)t.Top;
				int left = (int)t.Left;
				int width = (int)t.Width;
				int height = (int)t.Height;

				int right = left + width;
				int bottom = top + height;

				top = top / step * step;
				left = left / step * step;

				if(step > 1)
				{
					right = (right / step + 1) * step;
					bottom = (bottom / step + 1) * step;
				}

				return new Rectangle(left, top, right - left, bottom - top);
			});
		}

		public static async Task<Grid> CreateMinifiedGridForPathFinding(DatabaseModelDesignerViewModel designer, int step)
		{
			var rects = GetTableRectangles(designer.TableViewModels, step).Select(s =>
			{
				var t = s.Y / step;
				var l = s.X / step;
				var r = s.Right / step;
				var b = s.Bottom / step;
				return new Rectangle(l, t, r - l, b - t);
			});
			var res = await Task.Factory.StartNew(() => PathFinderHelper.CreateGrid((int)(designer.CanvasWidth / step), (int)designer.CanvasHeight / step, rects));
			return res;
		}

		public static Grid CreateMinifiedGridForPathFindingSync(DatabaseModelDesignerViewModel designer, int step)
		{
			var rects = GetTableRectangles(designer.TableViewModels, step).Select(s =>
			{
				var t = s.Y / step;
				var l = s.X / step;
				var r = s.Right / step;
				var b = s.Bottom / step;
				return new Rectangle(l, t, r - l, b - t);
			});

			return PathFinderHelper.CreateGrid((int) (designer.CanvasWidth / step), (int) designer.CanvasHeight / step, rects);
		}

		public static bool DoesPointIntersectWithRectangle(Rectangle area, Point point)
		{
			return point.X >= area.Left && point.X <= area.Right && point.Y >= area.Top && point.Y <= area.Bottom;
		}

		public static async Task CloseDiagramsOnDisconnect(MainWindow window)
		{
			var forSave = new Queue<DatabaseModelDesignerViewModel>();
			IEnumerable<LayoutContent> contents =
				window.MainDocumentPane.ChildrenSorted.Where(t => t.Content is DatabaseModelDesigner).ToList();

			bool yesToAll = false;

			foreach (LayoutContent content in contents)
			{
				DatabaseModelDesigner designer = content.Content as DatabaseModelDesigner;

				if (designer == null)
				{
					continue;
				}

				content.Close();

				if (yesToAll)
				{
					forSave.Enqueue(designer.ViewModel);
					continue;
				}

				var res = await window.ShowMessageAsync("Save diagram",
					$"Do you want to save diagram ? ({designer.ViewModel.DiagramTitle})",
					MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings()
					{
						AnimateShow = true,
						AnimateHide = false,
						AffirmativeButtonText = "Yes",
						FirstAuxiliaryButtonText = "Yes to all",
						NegativeButtonText = "No"
					});

				switch (res)
				{
					case MessageDialogResult.Affirmative:
						forSave.Enqueue(designer.ViewModel);
						break;
					case MessageDialogResult.FirstAuxiliary:
						yesToAll = true;
						forSave.Enqueue(designer.ViewModel);
						break;
				}
			}

			var facade = new DiagramFacade(new DatabaseModelDesigner());
			while(forSave.Count > 0)
			{
				var vm = forSave.Dequeue();
				facade.ViewModel = vm;
				facade.SaveDiagram();
			}			
		}
		#endregion
	}
}