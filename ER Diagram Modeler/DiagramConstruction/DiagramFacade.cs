using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.DiagramConstruction
{
	public class DiagramFacade
	{
		public DatabaseModelDesignerViewModel ViewModel { get; set; }

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
			int x, y;
			FindFreePosition(out x, out y);
			return AddTable(source, x, y);
		}

		public void RefreshTableModel(TableViewModel vm)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			var model = ctx.ReadTableDetails(vm.Model.Id, vm.Model.Title);

			vm.Model.Attributes.Clear();

			foreach (TableRowModel attribute in model.Attributes)
			{
				vm.Model.Attributes.Add(attribute);
			}
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
			vm.Left = x;
			vm.Top = y;
			ViewModel.TableViewModels.Add(vm);
		}

#region HELPERS
		private void FindFreePosition(out int x, out int y)
		{
			x = 50;
			y = 50;

			bool isFree = false;
			var rnd = new Random();

			int xMin = (int)(ViewModel.HorizontalScrollOffset/ViewModel.Scale);
			int yMin = (int)(ViewModel.VeticalScrollOffset/ViewModel.Scale);

			int xMax = xMin + (int)(ViewModel.ViewportWidth / ViewModel.Scale);
			int yMax = yMin + (int)(ViewModel.ViewportHeight / ViewModel.Scale);

			if (xMax > ViewModel.CanvasWidth)
			{
				xMax = (int)ViewModel.CanvasWidth;
			}

			if(yMax > ViewModel.CanvasHeight)
			{
				yMax = (int)ViewModel.CanvasHeight;
			}

			while (!isFree)
			{
				x = rnd.Next(xMin, xMax);
				y = rnd.Next(yMin, yMax);

				isFree = true;
				foreach (TableViewModel viewModel in ViewModel.TableViewModels)
				{
					if (x >= viewModel.Left && x <= viewModel.Left+viewModel.Width && y >= viewModel.Top && y <= viewModel.Top+viewModel.Height)
					{
						isFree = false;
						break;
					}
				}
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

			anchorable.Content = new DatabaseModelDesigner()
			{
				ViewModel = designerViewModel
			};
			window.MainDocumentPane.Children.Add(anchorable);
			int indexOf = window.MainDocumentPane.Children.IndexOf(anchorable);
			window.MainDocumentPane.SelectedContentIndex = indexOf;
		}

#endregion
	}
}