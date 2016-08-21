using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas
{
	/// <summary>
	/// Interaction logic for DatabaseModelDesigner.xaml
	/// </summary>
	public partial class DatabaseModelDesigner : UserControl
	{
		private DatabaseModelDesignerViewModel _viewModel;

		public DatabaseModelDesignerViewModel ViewModel
		{
			get { return _viewModel; }
			set
			{
				_viewModel = value;
				DataContext = value;
				ViewModel.TableViewModels.CollectionChanged += TableViewModelsOnCollectionChanged;
			}
		}

		private void TableViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TableViewModel item in args.NewItems)
					{
						AddElement(item);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (TableViewModel item in args.OldItems)
					{
						RemoveElement(item);
					}
					break;
			}
		}

		public DatabaseModelDesigner()
		{
			InitializeComponent();
			DataContextChanged += OnDataContextChanged;
			ModelDesignerCanvas.MouseDown += ModelDesignerCanvasOnMouseDown;
		}

		private void ModelDesignerCanvasOnMouseDown(object sender, MouseButtonEventArgs e)
		{
			switch (ViewModel.MouseMode)
			{
				case MouseMode.Select:
					ModelDesignerCanvas.DeselectAll();
					ModelDesignerCanvas.ResetZIndexes();
					break;
				case MouseMode.NewTable:
					var table = MainWindow.SeedDataTable();
					table.Left = e.GetPosition(sender as DesignerCanvas).X;
					table.Top = e.GetPosition(sender as DesignerCanvas).Y;
					ViewModel.TableViewModels.Add(table);
					ViewModel.MouseMode = MouseMode.Select;
					break;
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			ZoomBox.ViewModel = ViewModel;
		}

		private void AddElement(TableViewModel viewModel)
		{
			var content = new TableContent(viewModel);
			ModelDesignerCanvas.Children.Add(content);
			DesignerCanvas.SetTop(content, viewModel.Top);
			DesignerCanvas.SetLeft(content, viewModel.Left);
		}

		private void RemoveElement(TableViewModel viewModel)
		{
			var table = ModelDesignerCanvas.Children.Cast<TableContent>().FirstOrDefault(t => t.TableViewModel.Equals(viewModel));
			ModelDesignerCanvas.Children.Remove(table);
		}

		public void DeleteSelectedTables()
		{
			var delete = ViewModel.TableViewModels.Where(t => t.IsSelected).ToList();
			foreach(TableViewModel item in delete)
			{
				ViewModel.TableViewModels.Remove(item);
			}
		}
	}
}
