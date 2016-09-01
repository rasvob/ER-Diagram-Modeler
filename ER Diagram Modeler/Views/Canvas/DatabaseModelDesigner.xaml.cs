using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.Connection;
using ER_Diagram_Modeler.Views.Canvas.TableItem;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Size = System.Drawing.Size;

namespace ER_Diagram_Modeler.Views.Canvas
{
	/// <summary>
	/// Interaction logic for DatabaseModelDesigner.xaml
	/// </summary>
	public partial class DatabaseModelDesigner : UserControl
	{
		private ObservableCollection<ConnectionInfo> _connections; 

		private DatabaseModelDesignerViewModel _viewModel;
		private Point? _dragStartPoint = null;
		private double _capturedVerticalOffset;
		private double _capturedHorizontalOffset;

		public DatabaseModelDesignerViewModel ViewModel
		{
			get { return _viewModel; }
			set
			{
				_viewModel = value;
				DataContext = value;
				ViewModel.TableViewModels.CollectionChanged += TableViewModelsOnCollectionChanged;
				ViewModel.ScaleChanged += ViewModelOnScaleChanged;
			}
		}

		private void ViewModelOnScaleChanged(object sender, ScaleEventArgs args)
		{
			double contentHorizontalMiddle = (args.OldHorizontalOffset + args.OldViewportWidth/2)/args.OldScale;
			double contentVerticalMiddle = (args.OldVerticalOffset + args.OldViewportHeight/2)/args.OldScale;

			if ((int)DesignerScrollViewer.ScrollableWidth == 0)
			{
				contentHorizontalMiddle = ViewModel.CanvasWidth/2;
			}

			if ((int)DesignerScrollViewer.ScrollableHeight == 0)
			{
				contentVerticalMiddle = ViewModel.CanvasHeight/2;
			}

			double newContentHorizontalOffset = contentHorizontalMiddle - (ViewModel.ViewportWidth / 2)/ViewModel.Scale;
			double newContentVerticalOffset = contentVerticalMiddle - (ViewModel.ViewportHeight/2)/ViewModel.Scale;

			double newHorizontalOffset = newContentHorizontalOffset*ViewModel.Scale;
			double newVerticalOffset = newContentVerticalOffset * ViewModel.Scale;

			DesignerScrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
			DesignerScrollViewer.ScrollToVerticalOffset(newVerticalOffset);
		}

		private void TableViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TableViewModel item in args.NewItems)
					{
						AddTableElement(item);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (TableViewModel item in args.OldItems)
					{
						RemoveTableElement(item);
					}
					break;
			}
		}

		public DatabaseModelDesigner()
		{
			InitializeComponent();
			_connections = new ObservableCollection<ConnectionInfo>();

			DataContextChanged += OnDataContextChanged;
			_connections.CollectionChanged += ConnectionsOnCollectionChanged;
		}

		private void ConnectionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (ConnectionInfo item in args.NewItems)
					{
						AddConnectionElement(item);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach(ConnectionInfo item in args.OldItems)
					{
						RemoveConnectionElement(item);
					}
					break;
			}
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
					var origin = e.GetPosition(ModelDesignerCanvas);
					var table = new TableViewModel(new TableModel());

					var nameDialog = new TableNameDialog
					{
						Owner = Window.GetWindow(this), Model = table.Model
					};
					var res = nameDialog.ShowDialog();

					if (res.HasValue)
					{
						if (res.Value)
						{
							table.Left = origin.X;
							table.Top = origin.Y;
							ViewModel.TableViewModels.Add(table);
						}
					}
					ViewModel.MouseMode = MouseMode.Select;
					break;
			}
		}

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			ZoomBox.ViewModel = ViewModel;
		}

		private void AddTableElement(TableViewModel viewModel)
		{
			var content = new TableContent(viewModel);
			ModelDesignerCanvas.Children.Add(content);
			content.Loaded += (sender, args) =>
			{
				MeasureToFit(content);
				DesignerCanvas.SetZIndex(content, DesignerCanvas.TableUnselectedZIndex);
				DesignerCanvas.SetTop(content, viewModel.Top);
				DesignerCanvas.SetLeft(content, viewModel.Left);
				viewModel.Height = content.ActualHeight;
				viewModel.Width = content.ActualWidth;
			};
		}

		private void RemoveTableElement(TableViewModel viewModel)
		{
			var table = ModelDesignerCanvas.Children.Cast<TableContent>().FirstOrDefault(t => t.TableViewModel.Equals(viewModel));
			ModelDesignerCanvas.Children.Remove(table);
		}

		private void AddConnectionElement(ConnectionInfo connection)
		{
			foreach (ConnectionLine connectionLine in connection.Lines)
			{
				ModelDesignerCanvas.Children.Add(connectionLine.Line);
			}
		}

		private void RemoveConnectionElement(ConnectionInfo connection)
		{
			foreach (ConnectionLine connectionLine in connection.Lines)
			{
				ModelDesignerCanvas.Children.Remove(connectionLine.Line);
			}
		}

		public void DeleteSelectedTables()
		{
			var delete = ViewModel.TableViewModels.Where(t => t.IsSelected).ToList();
			foreach (TableViewModel item in delete)
			{
				ViewModel.TableViewModels.Remove(item);
			}
		}

		private void MeasureToFit(TableContent content)
		{
			if (content.ActualWidth + content.TableViewModel.Left >= ModelDesignerCanvas.ActualWidth)
			{
				content.TableViewModel.Left = ModelDesignerCanvas.ActualWidth - content.ActualWidth - 10;
			}

			if (content.ActualHeight + content.TableViewModel.Top >= ModelDesignerCanvas.ActualHeight)
			{
				content.TableViewModel.Top = ModelDesignerCanvas.ActualHeight - content.ActualHeight - 10;
			}
		}

		private void DesignerScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs args)
		{
			//Extent size glith-stop
			if (args.ExtentWidth < ViewModel.CanvasWidth*ViewModel.Scale || args.ExtentHeight < ViewModel.CanvasHeight*ViewModel.Scale)
			{
				return;
			}
			ViewModel.ViewportWidth = args.ViewportWidth;
			ViewModel.ViewportHeight = args.ViewportHeight;
			ViewModel.VeticalScrollOffset = args.VerticalOffset;
			ViewModel.HorizontalScrollOffset = args.HorizontalOffset;
		}

		#region TestRegion

		private ConnectionInfo TestNewConnectionCreate()
		{
			var info = new ConnectionInfo();

			info.Points.Add(new ConnectionPoint()
			{
				X = 5,
				Y = 5
			});

			info.Points.Add(new ConnectionPoint()
			{
				X = 100,
				Y = 5
			});

			info.Points.Add(new ConnectionPoint()
			{
				X = 100,
				Y = 80
			});

			info.Points.Add(new ConnectionPoint()
			{
				X = 210,
				Y = 80
			});

			info.Points.Add(new ConnectionPoint()
			{
				X = 210,
				Y = 300
			});

			info.BuildLinesFromPoints();

			return info;
		}

		//Test command F4
		private void TestCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var info = TestNewConnectionCreate();
			_connections.Add(info);
		}

		//Test command F5
		private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var info = _connections[0];

			_connections.Remove(info);

			foreach(ConnectionPoint connectionPoint in info.Points)
			{
				Trace.WriteLine($"{connectionPoint.X}:{connectionPoint.Y}");
			}
			info.BuildPointsFromLines();

			Trace.WriteLine("---");
			foreach(ConnectionPoint connectionPoint in info.Points)
			{
				Trace.WriteLine($"{connectionPoint.X}:{connectionPoint.Y}");
			}

			info.Lines.Clear();
			info.BuildLinesFromPoints();

			_connections.Add(info);
		}

		#endregion

		private void DesignerScrollViewer_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ViewModel.MouseMode == MouseMode.Panning)
			{
				_dragStartPoint = e.GetPosition(DesignerScrollViewer);
				_capturedHorizontalOffset = DesignerScrollViewer.HorizontalOffset;
				_capturedVerticalOffset = DesignerScrollViewer.VerticalOffset;
				DesignerScrollViewer.CaptureMouse();
				Keyboard.Focus(DesignerScrollViewer);
				e.Handled = true;
			}
		}

		private void DesignerScrollViewer_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (ViewModel.MouseMode == MouseMode.Panning || _dragStartPoint.HasValue)
			{
				DesignerScrollViewer.ReleaseMouseCapture();
				_dragStartPoint = null;
				e.Handled = true;
			}
		}

		private void DesignerScrollViewer_OnPreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (ViewModel.MouseMode == MouseMode.Panning)
			{
				if (_dragStartPoint.HasValue)
				{
					Vector delta = e.GetPosition(DesignerScrollViewer) - _dragStartPoint.Value;
					DesignerScrollViewer.ScrollToHorizontalOffset(_capturedHorizontalOffset - delta.X);
					DesignerScrollViewer.ScrollToVerticalOffset(_capturedVerticalOffset - delta.Y);
					e.Handled = true;
				}
			}
		}

		private void DesignerScrollViewer_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Source is ScrollViewer)
			{
				if (e.Key == Key.Space)
				{
					ViewModel.MouseMode = MouseMode.Panning;
					e.Handled = true;
				}
			}
		}

		private void DesignerScrollViewer_OnPreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Source is ScrollViewer)
			{
				if (e.Key == Key.Space)
				{
					ViewModel.MouseMode = MouseMode.Select;
					e.Handled = true;
				}
			}
		}

		private void DesignerScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.Modifiers != 0)
			{
				TableViewControl viewControl = e.Source as TableViewControl;
				if (viewControl != null)
				{
					var grid = viewControl.TableDataGrid;
					var item = grid.SelectedItem;
					var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
					var cbox = VisualTreeHelperEx.FindDescendantByName(row, "DataTypeComboBox") as ComboBox;
					if (cbox != null) cbox.IsDropDownOpen = false;
				}
			}

			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
			{
				double newScale = ViewModel.Scale;
				if (e.Delta > 0)
				{
					newScale += 0.05;
				}
				else if (e.Delta < 0)
				{
					newScale -= 0.05;
				}
				ViewModel.ChangeZoomCommand.Execute(newScale.ToString("G"));
				e.Handled = true;
			}
			else if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
			{
				DesignerScrollViewer.ScrollToHorizontalOffset(DesignerScrollViewer.HorizontalOffset + e.Delta);
				e.Handled = true;
			}
		}

		
	}
}
