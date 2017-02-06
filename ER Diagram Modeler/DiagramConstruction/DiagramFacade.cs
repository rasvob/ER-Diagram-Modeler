﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Extintions;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Canvas.TableItem;
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

			var relationships = ctx.ListRelationshipsForTable(model.Title, ViewModel.TableViewModels.Select(t => t.Model));

			foreach (RelationshipModel relationship in relationships)
			{
				ConnectionInfoViewModel vm = new ConnectionInfoViewModel();
				vm.DesignerCanvas = canvas;
				vm.RelationshipModel = relationship;
				vm.SourceViewModel = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Equals(relationship.Source));
				vm.DestinationViewModel = ViewModel.TableViewModels.FirstOrDefault(t => t.Model.Equals(relationship.Destination));

				await vm.BuildConnection3(ViewModel);
				ViewModel.ConnectionInfoViewModels.Add(vm);
			}
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

		#endregion
	}
}