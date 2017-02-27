using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Extintions;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Canvas.TableItem;
using MahApps.Metro.Controls.Dialogs;
using Oracle.ManagedDataAccess.Client;
using Pathfinding;
using Pathfinding.Structure;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.DiagramConstruction
{
	/// <summary>
	/// Facade for DatabaseModelDesigner operations
	/// </summary>
	public class DiagramFacade
	{
		/// <summary>
		/// Viewmodel of working diagram
		/// </summary>
		public DatabaseModelDesignerViewModel ViewModel { get; set; }

		/// <summary>
		/// Callback action after adding new table
		/// </summary>
		public Action<TableModel> AddTableCallbackAction { get; set; }

		public static string DiagramSaved = "DIAGRAM SAVED";
		public static string DiagramLoaded = "DIAGRAM LOADED";
		public static string DiagramCreated = "DIAGRAM CREATED";

		public DiagramFacade(DatabaseModelDesignerViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public DiagramFacade(DatabaseModelDesigner designer)
		{
			ViewModel = designer.ViewModel;
		}

		/// <summary>
		/// Add table to diagram
		/// </summary>
		/// <param name="source">Table for add</param>
		/// <returns>True if added, false if not</returns>
		public bool AddTable(TableModel source)
		{
			return AddTable(source, -1, -1);
		}

		/// <summary>
		/// Add table to digram on given coordinates
		/// </summary>
		/// <param name="name">Name of new table</param>
		/// <param name="x">Left property</param>
		/// <param name="y">Top property</param>
		/// <returns>True if added, false if not</returns>
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

		/// <summary>
		/// Add table (existing) to digram on given coordinates
		/// </summary>
		/// <param name="source">Table for add</param>
		/// <param name="x">Left property</param>
		/// <param name="y">Top property</param>
		/// <returns>True if added, false if not</returns>
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

		/// <summary>
		/// Add existing realtionships for given table
		/// </summary>
		/// <param name="model">Table</param>
		/// <param name="canvas">Canvas for relationship add</param>
		/// <returns>Task for async execution</returns>
		public async Task AddRelationShipsForTable(TableModel model, DesignerCanvas canvas)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			var relationships =
				await Task.Run(
					() =>
						ctx.ListRelationshipsForTable(model.Title, ViewModel.TableViewModels.Select(t => t.Model))
							.Where(t => !ViewModel.ConnectionInfoViewModels.Any(s => s.RelationshipModel.Name.Equals(t.Name))));

			//var relationships = ctx.ListRelationshipsForTable(model.Title, ViewModel.TableViewModels.Select(t => t.Model)).Where(t => !ViewModel.ConnectionInfoViewModels.Any(s => s.RelationshipModel.Name.Equals(t.Name)));

			foreach (RelationshipModel relationship in relationships)
			{
				await AddRelationship(relationship, canvas);
			}
		}

		/// <summary>
		/// Add given relationship to diagrram
		/// </summary>
		/// <param name="relationship">Relationship</param>
		/// <param name="canvas">Canvas for relationship add</param>
		/// <returns>Task for async execution</returns>
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

		/// <summary>
		/// Remove table from diagram
		/// </summary>
		/// <param name="model">Table for remove</param>
		public void RemoveTable(TableModel model)
		{
			var table = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Equals(model));

			if (table != null)
			{
				ViewModel.TableViewModels.Remove(table);
			}
		}

		/// <summary>
		/// Create viewmodel for new table
		/// </summary>
		/// <param name="source">Table model</param>
		/// <param name="ctx">Strategy context</param>
		/// <returns>Viewmodel for table</returns>
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

		/// <summary>
		/// Add viewmodel to diagram on given coordinations
		/// </summary>
		/// <param name="vm">Viewmodel for add</param>
		/// <param name="x">Left property</param>
		/// <param name="y">Top property</param>
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

		/// <summary>
		/// Synchronize current diagram with DB
		/// </summary>
		/// <param name="canvas">Canvas with diagram</param>
		/// <returns>Task for async execution</returns>
		public async Task RefreshDiagram(DesignerCanvas canvas)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			IEnumerable<TableModel> tables = await Task.Run(() => ctx.ListTables());
			IEnumerable<string> foreignKeys = await Task.Run(() => ctx.ListAllForeignKeys());

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
				TableModel model = await Task.Run(() => ctx.ReadTableDetails(viewModel.Model.Id, viewModel.Model.Title));
				viewModel.Model.RefreshModel(model);

				IEnumerable<RelationshipModel> relationshipModels = await Task.Run(() => ctx.ListRelationshipsForTable(viewModel.Model.Title, ViewModel.TableViewModels.Select(t => t.Model)));
				IEnumerable<RelationshipModel> filtered = relationshipModels.Where(t => !ViewModel.ConnectionInfoViewModels.Any(s => s.RelationshipModel.Name.Equals(t.Name)));

				foreach (RelationshipModel relationshipModel in filtered)
				{
					await AddRelationship(relationshipModel, canvas);
				}
			}
		}

		/// <summary>
		/// Load diagram from DB
		/// </summary>
		/// <param name="canvas">Canvas for diagram</param>
		/// <param name="data">XML attribute from DB</param>
		public async Task LoadDiagram(DesignerCanvas canvas, XDocument data)
		{
			var root = data.Root;
			ViewModel.LoadFromElement(data.Root);

			if (ViewModel.CanvasWidth != DatabaseModelDesignerViewModel.DefaultWidth || ViewModel.CanvasHeight != DatabaseModelDesignerViewModel.DefaultWidth)
			{
				StreamGeometry geometry = DesignerCanvas.CreateGridWithStreamGeometry(ViewModel.CanvasHeight, ViewModel.CanvasWidth,
					DesignerCanvas.GridCellWidth);
				canvas.RefreshGuideLines(geometry);
			}

			var labels = root?.XPathSelectElements("LabelViewModels/LabelViewModel")
				.Select(t =>
				{
					var vm = new LabelViewModel();
					vm.LoadFromElement(t);
					return vm;
				}).ToList();

			if (labels != null && labels.Any())
			{
				foreach(LabelViewModel label in labels)
				{
					ViewModel.LabelViewModels.Add(label);
				}
			}

			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			var tablesInDb = await Task.Run(() => ctx.ListTables());
			var tableElements = root?.XPathSelectElements("TableViewModels/TableViewModel")
				.Select(t =>
				{
					var vm = new TableViewModel();
					vm.LoadFromElement(t);
					return vm;
				})
				.Where(s => tablesInDb.Any(t => t.Id.Equals(s.Model.Id)))
				.ToList();

			Output.WriteLine(DiagramLoaded);

			if(tableElements == null || !tableElements.Any())
			{
				return;	
			}

			var relationShipsInDb = await Task.Run(() => ctx.ListAllForeignKeys());
			var allRelationshipsDetailsPom = new List<RelationshipModel>();
			foreach (TableViewModel model in tableElements)
			{
				TableModel tab = ctx.ReadTableDetails(model.Model.Id, model.Model.Title);
				model.Model.RefreshModel(tab);
				ViewModel.TableViewModels.Add(model);
			}

			foreach (TableViewModel model in tableElements)
			{
				IEnumerable<RelationshipModel> models = await Task.Run(() => ctx.ListRelationshipsForTable(model.Model.Title, tableElements.Select(s => s.Model)));
				allRelationshipsDetailsPom.AddRange(models);
			}

			var allRelationshipsDetails = allRelationshipsDetailsPom
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

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <returns>Returns 1 if saved, 0 if not</returns>
		public int SaveDiagram()
		{
			XDocument doc = XDocument.Parse(ViewModel.CreateElement().ToString());
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			int saveDiagram = 0;
			try
			{
				saveDiagram = ctx.SaveDiagram(ViewModel.DiagramTitle, doc);
			}
			catch(Exception exception) when(exception is SqlException || exception is OracleException)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
			}
			return saveDiagram;
		}

#region HELPERS
		
		/// <summary>
		/// Helper method for update of table position
		/// </summary>
		/// <param name="table">Table</param>
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

		/// <summary>
		/// Create new diagram panel in Main Window
		/// </summary>
		/// <param name="window">App window</param>
		/// <param name="title">Title of diagram</param>
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
			Output.WriteLine(DiagramCreated);
		}

		/// <summary>
		/// Rectangular areas of tables - for pathfinding
		/// </summary>
		/// <param name="tables">Table viewmodels</param>
		/// <param name="step">How coarse size of rectangle is</param>
		/// <returns>Rectangular areas</returns>
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
					right = (right / step + 2) * step;
					bottom = (bottom / step + 2) * step;
				}

				return new Rectangle(left, top, right - left, bottom - top);
			});
		}

		/// <summary>
		/// Create smaller grid for pathfinding
		/// </summary>
		/// <param name="designer">From this viewmodel is grid created</param>
		/// <param name="step">Grid row/col step</param>
		/// <returns>Task for async execution => Created grid</returns>
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

		/// <summary>
		/// Create smaller grid for pathfinding - for non-async calls
		/// </summary>
		/// <param name="designer">From this viewmodel is grid created</param>
		/// <param name="step">Grid row/col step</param>
		/// <returns>Created grid</returns>
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

		/// <summary>
		/// Check if point lies within rectangular area
		/// </summary>
		/// <param name="area">Rectangle</param>
		/// <param name="point">Point</param>
		/// <returns>True if point is within, false if not</returns>
		public static bool DoesPointIntersectWithRectangle(Rectangle area, Point point)
		{
			return point.X >= area.Left && point.X <= area.Right && point.Y >= area.Top && point.Y <= area.Bottom;
		}

		/// <summary>
		/// Cloase and save diagrams after session end
		/// </summary>
		/// <param name="window">Main app window</param>
		/// <returns>Task for async execution</returns>
		public static async Task CloseDiagramsOnDisconnect(MainWindow window)
		{
			var forSave = new Queue<DatabaseModelDesignerViewModel>();
			IEnumerable<LayoutContent> contents =
				window.MainDocumentPane.ChildrenSorted.Where(t => t.Content is DatabaseModelDesigner).ToList();

			var facade = new DiagramFacade(new DatabaseModelDesigner());

			bool yesToAll = false;

			foreach (LayoutContent content in contents)
			{
				DatabaseModelDesigner designer = content.Content as DatabaseModelDesigner;

				if (designer == null)
				{
					continue;
				}

				if (yesToAll)
				{
					forSave.Enqueue(designer.ViewModel);
					content.Close();
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
						facade.ViewModel = designer.ViewModel;
						facade.SaveDiagram();
						break;
					case MessageDialogResult.FirstAuxiliary:
						yesToAll = true;
						forSave.Enqueue(designer.ViewModel);
						break;
				}

				content.Close();
			}

			while(forSave.Count > 0)
			{
				var vm = forSave.Dequeue();
				facade.ViewModel = vm;
				facade.SaveDiagram();
			}			
		}

		/// <summary>
		/// Save all opened diagrams to DB
		/// </summary>
		/// <param name="window">Main app window</param>
		public static void SaveAllDiagrams(MainWindow window)
		{
			IEnumerable<DatabaseModelDesignerViewModel> viewModels = window.MainDocumentPane.ChildrenSorted
				.Where(t => t.Content is DatabaseModelDesigner)
				.Select(t => t.Content as DatabaseModelDesigner)
				.Select(t => t?.ViewModel);
			var facade = new DiagramFacade(new DatabaseModelDesigner());

			foreach (DatabaseModelDesignerViewModel viewModel in viewModels)
			{
				facade.ViewModel = viewModel;
				facade.SaveDiagram();
			}
		}
		#endregion
	}
}