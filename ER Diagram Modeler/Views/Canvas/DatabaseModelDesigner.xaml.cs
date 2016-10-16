using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.Connection;
using ER_Diagram_Modeler.Views.Canvas.TableItem;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace ER_Diagram_Modeler.Views.Canvas
{
	/// <summary>
	/// Interaction logic for DatabaseModelDesigner.xaml
	/// </summary>
	public partial class DatabaseModelDesigner : UserControl
	{
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
				ViewModel.ConnectionInfoViewModels.CollectionChanged += ConnectionsOnCollectionChanged;
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
						item.PropertyChanged += ItemOnPropertyChanged;
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

		private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("IsSelected", StringComparison.CurrentCultureIgnoreCase))
			{
				var table = sender as TableViewModel;
				if (table != null && table.IsSelected)
				{
					DeselectConnections();
				}
			}
		}

		public DatabaseModelDesigner()
		{
			InitializeComponent();
			DataContextChanged += OnDataContextChanged;
		}

		private void ConnectionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (ConnectionInfoViewModel item in args.NewItems)
					{
						AddConnectionElement(item);
						item.Lines.CollectionChanged += LinesOnCollectionChanged;
						item.Points.CollectionChanged += PointsOnCollectionChanged;
						item.Marks.CollectionChanged += MarksOnCollectionChanged;
						item.SelectionChange += ItemOnSelectionChange;
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach(ConnectionInfoViewModel item in args.OldItems)
					{
						RemoveConnectionElement(item);
					}
					break;
			}
		}

		private void MarksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (ConnectionPointMark mark in args.NewItems)
					{
						ModelDesignerCanvas.Children.Add(mark.Mark);
						DesignerCanvas.SetZIndex(mark.Mark, DesignerCanvas.ConnectionPointZIndex);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach(ConnectionPointMark mark in args.OldItems)
					{
						ModelDesignerCanvas.Children.Remove(mark.Mark);
					}
					break;
			}
		}

		private void ItemOnSelectionChange(object sender, bool val)
		{
			var conn = sender as ConnectionInfoViewModel;
			if (val)
			{
				foreach (ConnectionInfoViewModel info in ViewModel.ConnectionInfoViewModels.Where(t => !t.Equals(conn)))
				{
					info.IsSelected = false;
				}
				ModelDesignerCanvas.DeselectTables();

			}
		}

		private void PointsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					//Trace.WriteLine("Point added");
					break;
				case NotifyCollectionChangedAction.Remove:
					//Trace.WriteLine("Point removed");
					break;
			}
		}

		private void LinesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (ConnectionLine newItem in args.NewItems)
					{
						ModelDesignerCanvas.Children.Add(newItem.Line);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (ConnectionLine newItem in args.OldItems)
					{
						ModelDesignerCanvas.Children.Remove(newItem.Line);
					}
					break;
			}
		}

		private void ModelDesignerCanvasOnMouseDown(object sender, MouseButtonEventArgs e)
		{
			switch (ViewModel.MouseMode)
			{
				case MouseMode.Select:
					ModelDesignerCanvas.DeselectTables();
					DeselectConnections();
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
				content.Height = content.ActualHeight + 30;
				content.Width = content.ActualWidth + 30;
				viewModel.Height = content.Height;
				viewModel.Width = content.Width;
			};
		}

		private void RemoveTableElement(TableViewModel viewModel)
		{
			var table = ModelDesignerCanvas.Children.Cast<TableContent>().FirstOrDefault(t => t.TableViewModel.Equals(viewModel));
			ModelDesignerCanvas.Children.Remove(table);
		}

		private void AddConnectionElement(ConnectionInfoViewModel connection)
		{
			foreach (ConnectionLine connectionLine in connection.Lines)
			{
				ModelDesignerCanvas.Children.Add(connectionLine.Line);
			}
			ModelDesignerCanvas.Children.Add(connection.SourceConnector.ConnectionPath);
			ModelDesignerCanvas.Children.Add(connection.SourceConnector.Symbol);
			ModelDesignerCanvas.Children.Add(connection.DestinationConnector.ConnectionPath);
			ModelDesignerCanvas.Children.Add(connection.DestinationConnector.Symbol);
		}

		private void RemoveConnectionElement(ConnectionInfoViewModel connection)
		{
			foreach (ConnectionLine connectionLine in connection.Lines)
			{
				ModelDesignerCanvas.Children.Remove(connectionLine.Line);
			}
			ModelDesignerCanvas.Children.Remove(connection.SourceConnector.ConnectionPath);
			ModelDesignerCanvas.Children.Remove(connection.SourceConnector.Symbol);
			ModelDesignerCanvas.Children.Remove(connection.DestinationConnector.ConnectionPath);
			ModelDesignerCanvas.Children.Remove(connection.DestinationConnector.Symbol);
		}

		public void DeleteSelectedTables()
		{
			var delete = ViewModel.TableViewModels.Where(t => t.IsSelected).ToList();
			foreach (TableViewModel item in delete)
			{
				ViewModel.TableViewModels.Remove(item);
				var connectionsForRemove =
					ViewModel.ConnectionInfoViewModels.Where(t => t.RelationshipModel.Destination.Equals(item.Model) || t.RelationshipModel.Source.Equals(item.Model));
				foreach (ConnectionInfoViewModel connectionInfo in connectionsForRemove)
				{
					ViewModel.ConnectionInfoViewModels.Remove(connectionInfo);
				}
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

		private void DeselectConnections()
		{
			foreach (ConnectionInfoViewModel info in ViewModel.ConnectionInfoViewModels)
			{
				info.IsSelected = false;
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

		private ConnectionInfoViewModel TestNewConnectionCreate()
		{
			var info = new ConnectionInfoViewModel();

			//info.Points.Add(new ConnectionPoint()
			//{
			//	X = 100, Y = 75
			//});

			//info.Points.Add(new ConnectionPoint()
			//{
			//	X = 100, Y = 80
			//});

			//info.Points.Add(new ConnectionPoint()
			//{
			//	X = 210, Y = 80
			//});

			info.Points.Add(new ConnectionPoint()
			{
				X = 400, Y = 300
			});

			info.Points.Add(new ConnectionPoint()
			{
				X = 210, Y = 300
			});

			info.BuildLinesFromPoints();

			return info;
		}

		//Test command F4
		private void TestCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var sourceTable = MainWindow.SeedDataTable();
			var destTable = MainWindow.SeedDataTable();

			sourceTable.Left = 50;
			sourceTable.Top = 350;
			destTable.Left = 700;
			destTable.Top = 350;

			ViewModel.TableViewModels.Add(sourceTable);
			ViewModel.TableViewModels.Add(destTable);

		}

		//Test command F5
		private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var info = new ConnectionInfoViewModel();
			info.SourceViewModel = ViewModel.TableViewModels.FirstOrDefault();
			info.DestinationViewModel = ViewModel.TableViewModels.LastOrDefault();

			//Right
			//var point1 =
			//	new ConnectionPoint(info.SourceViewModel.Left + info.SourceViewModel.Width + Connector.ConnectorLenght, 450);
			//var point2 =
			//	new ConnectionPoint(info.DestinationViewModel.Left - Connector.ConnectorLenght
			//		, 450);
			//info.Points.Add(point2);
			//info.Points.Add(point1);
			//info.SourceConnector.Orientation = ConnectorOrientation.Right;
			//info.DestinationConnector.Orientation = ConnectorOrientation.Left;
			//info.SourceConnector.EndPoint = point1;
			//info.DestinationConnector.EndPoint = point2;

			//Up
			//var point1 =
			//	new ConnectionPoint(info.SourceViewModel.Left + 30, info.SourceViewModel.Top - Connector.ConnectorLenght);
			//var point2 =
			//	new ConnectionPoint(info.SourceViewModel.Left + 30, info.SourceViewModel.Top - Connector.ConnectorLenght - 30);
			//var point3 = new ConnectionPoint(info.SourceViewModel.Left + +info.SourceViewModel.Width + 50, info.SourceViewModel.Top - Connector.ConnectorLenght - 30);
			//var point4 = new ConnectionPoint(info.SourceViewModel.Left + +info.SourceViewModel.Width + 50, info.DestinationViewModel.Top + 50);
			//var point5 = new ConnectionPoint(info.DestinationViewModel.Left - Connector.ConnectorLenght, info.DestinationViewModel.Top + 50);
			//info.Points.Add(point1);
			//info.Points.Add(point2);
			//info.Points.Add(point3);
			//info.Points.Add(point4);
			//info.Points.Add(point5);
			//info.SourceConnector.Orientation = ConnectorOrientation.Up;
			//info.DestinationConnector.Orientation = ConnectorOrientation.Left;
			//info.SourceConnector.EndPoint = point1;
			//info.DestinationConnector.EndPoint = point5;

			//Down
			var point1 =
				new ConnectionPoint(info.SourceViewModel.Left + 30, info.SourceViewModel.Top + info.SourceViewModel.Height + Connector.ConnectorLenght);
			var point2 =
				new ConnectionPoint(info.SourceViewModel.Left + 30, info.SourceViewModel.Top + info.SourceViewModel.Height + Connector.ConnectorLenght + 30);
			var point3 = new ConnectionPoint(info.SourceViewModel.Left + +info.SourceViewModel.Width + 50, info.SourceViewModel.Top + info.SourceViewModel.Height + Connector.ConnectorLenght + 30);
			var point4 = new ConnectionPoint(info.SourceViewModel.Left + +info.SourceViewModel.Width + 50, info.DestinationViewModel.Top + 50);
			var point5 = new ConnectionPoint(info.DestinationViewModel.Left - Connector.ConnectorLenght, info.DestinationViewModel.Top + 50);
			info.Points.Add(point1);
			info.Points.Add(point2);
			info.Points.Add(point3);
			info.Points.Add(point4);
			info.Points.Add(point5);
			info.SourceConnector.Orientation = ConnectorOrientation.Down;
			info.DestinationConnector.Orientation = ConnectorOrientation.Left;
			info.SourceConnector.EndPoint = point1;
			info.DestinationConnector.EndPoint = point5;

			//Left
			//var point1 =
			//new ConnectionPoint(info.SourceViewModel.Left - Connector.ConnectorLenght, info.SourceViewModel.Top + 30);
			//var point2 =
			//	new ConnectionPoint(info.SourceViewModel.Left - Connector.ConnectorLenght - 3, info.SourceViewModel.Top + 30);
			//var point3 = new ConnectionPoint(info.SourceViewModel.Left - Connector.ConnectorLenght - 3, info.SourceViewModel.Top - 50);
			//var point4 = new ConnectionPoint(info.SourceViewModel.Left + +info.SourceViewModel.Width + 50, info.SourceViewModel.Top - 50);
			//var point5 = new ConnectionPoint(info.SourceViewModel.Left + +info.SourceViewModel.Width + 50, info.DestinationViewModel.Top + 50);
			//var point6 = new ConnectionPoint(info.DestinationViewModel.Left - Connector.ConnectorLenght, info.DestinationViewModel.Top + 50);
			//info.Points.Add(point1);
			//info.Points.Add(point2);
			//info.Points.Add(point3);
			//info.Points.Add(point4);
			//info.Points.Add(point5);
			//info.Points.Add(point6);
			//info.SourceConnector.Orientation = ConnectorOrientation.Left;
			//info.DestinationConnector.Orientation = ConnectorOrientation.Left;
			//info.SourceConnector.EndPoint = point1;
			//info.DestinationConnector.EndPoint = point6;

			//Self
			//info.SourceViewModel = null;
			//info.SourceViewModel = info.DestinationViewModel;
			//var point1 =
			//	new ConnectionPoint(info.DestinationViewModel.Left - Connector.ConnectorLenght, info.DestinationViewModel.Top + 30);
			//var point2 =
			//	new ConnectionPoint(info.DestinationViewModel.Left - Connector.ConnectorLenght - 3, info.DestinationViewModel.Top + 30);
			//var point3 = new ConnectionPoint(info.SourceViewModel.Left - Connector.ConnectorLenght - 3, info.DestinationViewModel.Top - 50);
			//var point4 = new ConnectionPoint(info.SourceViewModel.Left + 50, info.DestinationViewModel.Top - 50);
			//var point5 = new ConnectionPoint(info.SourceViewModel.Left + 50, info.DestinationViewModel.Top - Connector.ConnectorLenght);
			//info.Points.Add(point1);
			//info.Points.Add(point2);
			//info.Points.Add(point3);
			//info.Points.Add(point4);
			//info.Points.Add(point5);
			//info.SourceConnector.Orientation = ConnectorOrientation.Left;
			//info.DestinationConnector.Orientation = ConnectorOrientation.Up;
			//info.SourceConnector.EndPoint = point1;
			//info.DestinationConnector.EndPoint = point5;


			info.SourceConnector.Cardinality = Cardinality.One;
			info.SourceConnector.Optionality = Optionality.Mandatory;
			info.DestinationConnector.Cardinality = Cardinality.Many;
			info.DestinationConnector.Optionality = Optionality.Mandatory;

			info.BuildLinesFromPoints();
			ViewModel.ConnectionInfoViewModels.Add(info);
			info.SynchronizeBendingPoints();
		}

		//Test command F6
		private void CommandBinding3_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			
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
					try
					{
						var grid = viewControl.TableDataGrid;
						var item = grid.SelectedItem;
						var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
						var cbox = VisualTreeHelperEx.FindDescendantByName(row, "DataTypeComboBox") as ComboBox;
						if (cbox != null) cbox.IsDropDownOpen = false;
					}
					catch (Exception exc)
					{
						Trace.WriteLine(exc.Message);
					}
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


		private void NewTableCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.MouseMode = MouseMode.NewTable;
		}

		private void NewTableCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var parent = VisualTreeHelperEx.FindAncestorByType<MainWindow>(this);
			if (parent == null)
			{
				return;
			}
			var focus = FocusManager.GetFocusedElement(parent);
			var isSenderAllowed = focus is ScrollViewer || focus is Button;
			if(!isSenderAllowed)
			{
				e.ContinueRouting = true;
				e.CanExecute = false;
			}
			else
			{
				e.ContinueRouting = false;
				e.CanExecute = true;
			}
		}

		private void DeleteTablesCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DeleteSelectedTables();
		}

		private void DeleteTablesCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if(ViewModel != null)
				e.CanExecute = ViewModel.TableViewModels.Any(t => t.IsSelected);
		}


		private void ShowGuideLinesCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.AreGuideLinesVisible = !ViewModel.AreGuideLinesVisible;
			ModelDesignerCanvas.SetGuideLinesVisible(ViewModel.AreGuideLinesVisible);
		}

		private void AddForeignKeyCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			
		}

		private void SelectionModeCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.MouseMode = MouseMode.Select;
		}

		public async void CanvasDimensionsChanged()
		{
			var settings = new MetroDialogSettings()
			{
				AnimateHide = true,
				AnimateShow = false
			};

			var parent = VisualTreeHelperEx.FindAncestorByType<MetroWindow>(this);

			var progressController = await parent.ShowProgressAsync("Please wait...", "Guidelines are updating", false, settings);
			await UpdateLines();
			await progressController.CloseAsync();
			ViewModel.OnComputedPropertyChanged();
		}

		private async Task UpdateLines()
		{
			double cellWidth = DesignerCanvas.GridCellWidth;
			double w = ModelDesignerCanvas.Width;
			double h = ModelDesignerCanvas.Height;

			//For progress dialog glitch-free opening
			await Task.Delay(500);

			var geometry = await Task.Run(() => DesignerCanvas.CreateGridWithStreamGeometry(h, w, cellWidth));
			ModelDesignerCanvas.RefreshGuideLines(geometry);
		}
	}
}
