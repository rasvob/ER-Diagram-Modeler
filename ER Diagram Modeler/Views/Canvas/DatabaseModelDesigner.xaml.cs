using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.Connection;
using ER_Diagram_Modeler.Views.Canvas.TableItem;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Oracle.ManagedDataAccess.Client;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Image = System.Drawing.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

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

		private TableViewModel _sourceModel;
		private TableViewModel _destinationModel;

		/// <summary>
		/// New table added
		/// </summary>
		public event EventHandler TableCreated;

		/// <summary>
		/// Viewmodel for designer
		/// </summary>
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

		/// <summary>
		/// Scale changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Table added or removed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void TableViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TableViewModel item in args.NewItems)
					{
						AddTableElement(item);
						item.PropertyChanged += ItemOnPropertyChanged;
						item.AreLimitsEnabled = ViewModel.AreTableLimitsEnabled;
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

		/// <summary>
		/// Item is selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("IsSelected", StringComparison.CurrentCultureIgnoreCase))
			{
				var table = sender as TableViewModel;
				if (table != null && table.IsSelected)
				{
					DeselectConnections();
				}

				var selectedCount = ViewModel.TableViewModels.Count(t => t.IsSelected);

				switch (selectedCount)
				{
					case 1:
						_sourceModel = table;
						break;
					case 2:
						_destinationModel = table;
						break;
				}
			}
		}

		public DatabaseModelDesigner()
		{
			InitializeComponent();
			DataContextChanged += OnDataContextChanged;
		}

		/// <summary>
		/// Relationship added or removed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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
						item.ClearLines();
						item.ClearPoints();
						item.ClearMarks();
						item.SourceViewModel = null;
						item.DestinationViewModel = null;
					}
					break;
			}
		}

		/// <summary>
		/// Bending point mark added or removed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Deselect not selected element
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="val"></param>
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
				_sourceModel = null;
				_destinationModel = null;
			}
		}

		/// <summary>
		/// Only for testing
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// New line added to relationship visualization
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Mouse clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ModelDesignerCanvasOnMouseDown(object sender, MouseButtonEventArgs e)
		{
			switch (ViewModel.MouseMode)
			{
				case MouseMode.Select:
					ModelDesignerCanvas.DeselectTables();
					_sourceModel = null;
					_destinationModel = null;
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
							MainWindow window = null;
							try
							{
								window = Window.GetWindow(this) as MainWindow;
								var facade = new DiagramFacade(ViewModel);

								bool addRes = facade.AddTable(table.Model.Title, (int) origin.X, (int) origin.Y);

								if (!addRes)
								{
									await window.ShowMessageAsync("Add new table", $"Table {table.Model.Title} already exists");
								}
								else
								{
									OnTableCreated();
								}
							}
							catch(Exception exception) when (exception is SqlException || exception is OracleException)
							{
								await window.ShowMessageAsync("Add new table", exception.Message);
							}
						}
					}
					ViewModel.MouseMode = MouseMode.Select;
					break;
			}
		}

		/// <summary>
		/// Data context changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dependencyPropertyChangedEventArgs"></param>
		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			ZoomBox.ViewModel = ViewModel;
		}

		/// <summary>
		/// Add table to canvas
		/// </summary>
		/// <param name="viewModel">Viewmodel of table</param>
		private void AddTableElement(TableViewModel viewModel)
		{
			var content = new TableContent(viewModel);
			var owner = Window.GetWindow(this) as MainWindow;
			content.AddNewRow += owner.AddNewRowHandler;
			content.EditSelectedRow += owner.EditRowHandler;
			content.RenameTable += owner.RenameTableHandler;
			content.RemoveSelectedRow += owner.RemoveRowHandler;
			content.DropTable += owner.DropTableHandler;
			content.UpdatePrimaryKeyConstraint += owner.UpdatePrimaryKeyConstraintHandler;

			RoutedEventHandler loadedEventHandler = null;
			loadedEventHandler = (sender, args) =>
			{
				MeasureToFit(content);
				DesignerCanvas.SetZIndex(content, DesignerCanvas.TableUnselectedZIndex);
				DesignerCanvas.SetTop(content, viewModel.Top);
				DesignerCanvas.SetLeft(content, viewModel.Left);
				content.Height = content.ActualHeight + 30;
				content.Width = content.ActualWidth + 30;
				viewModel.Height = content.Height;
				viewModel.Width = content.Width;
				viewModel.OnTableLoaded(content);
				content.Loaded -= loadedEventHandler;
			};

			content.Loaded += loadedEventHandler;
			ModelDesignerCanvas.Children.Add(content);
		}

		/// <summary>
		/// Remove table from canvas
		/// </summary>
		/// <param name="viewModel">Viewmodel of table</param>
		private void RemoveTableElement(TableViewModel viewModel)
		{
			var table = ModelDesignerCanvas.Children.OfType<TableContent>().FirstOrDefault(t => t.TableViewModel.Equals(viewModel));
			var owner = Window.GetWindow(this) as MainWindow;
			table.AddNewRow -= owner.AddNewRowHandler;
			table.EditSelectedRow -= owner.EditRowHandler;
			table.RenameTable -= owner.RenameTableHandler;
			table.RemoveSelectedRow -= owner.RemoveRowHandler;
			table.DropTable -= owner.DropTableHandler;
			table.UpdatePrimaryKeyConstraint -= owner.UpdatePrimaryKeyConstraintHandler;
			ModelDesignerCanvas.Children.Remove(table);
		}

		/// <summary>
		/// Add lines to canvas
		/// </summary>
		/// <param name="connection">Viewmodel of relationship</param>
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

		/// <summary>
		/// Remove lines from canvas
		/// </summary>
		/// <param name="connection">Viewmodel of relationship</param>
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

		/// <summary>
		/// Deselect tables
		/// </summary>
		public void DeleteSelectedTables()
		{
			var delete = ViewModel.TableViewModels.Where(t => t.IsSelected).ToList();
			foreach (TableViewModel item in delete)
			{
				
				var connectionsForRemove =
					ViewModel.ConnectionInfoViewModels.Where(t => t.RelationshipModel.Destination.Equals(item.Model) || t.RelationshipModel.Source.Equals(item.Model)).ToList();

				foreach (ConnectionInfoViewModel connectionInfoViewModel in connectionsForRemove)
				{
					ViewModel.ConnectionInfoViewModels.Remove(connectionInfoViewModel);
				}

				ViewModel.TableViewModels.Remove(item);
			}
		}

		/// <summary>
		/// Deselect connections
		/// </summary>
		private void DeleteSelectedConnections()
		{
			var connectionsForRemove = ViewModel.ConnectionInfoViewModels.Where(t => t.IsSelected).ToList();
			foreach(ConnectionInfoViewModel connectionInfoViewModel in connectionsForRemove)
			{
				ViewModel.ConnectionInfoViewModels.Remove(connectionInfoViewModel);
			}
		}

		/// <summary>
		/// Ensure bounds
		/// </summary>
		/// <param name="content">Item on canvas</param>
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

		/// <summary>
		/// Deselect connections
		/// </summary>
		private void DeselectConnections()
		{
			foreach (ConnectionInfoViewModel info in ViewModel.ConnectionInfoViewModels)
			{
				info.IsSelected = false;
			}
		}

		/// <summary>
		/// Scroll caused viewport offset change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Test command F4
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TestCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			
		}

		/// <summary>
		/// Test command F5
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{

		}

		/// <summary>
		/// Test command F6
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CommandBinding3_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			
		}

		#endregion

		/// <summary>
		/// Left mouse button down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Left mouse button up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DesignerScrollViewer_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (ViewModel.MouseMode == MouseMode.Panning || _dragStartPoint.HasValue)
			{
				DesignerScrollViewer.ReleaseMouseCapture();
				_dragStartPoint = null;
				e.Handled = true;
			}
		}

		/// <summary>
		/// Mouse move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Key pressed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Key released
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Mouse wheel used
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Change mouse mode for table add
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NewTableCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.MouseMode = MouseMode.NewTable;
		}

		/// <summary>
		/// New table
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Delete selected from diagram not from DB
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteTablesCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DeleteSelectedTables();
			//DeleteSelectedConnections();
		}

		/// <summary>
		/// Delete selected from diagram not from DB - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteTablesCommand_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (ViewModel != null)
				e.CanExecute = ViewModel.TableViewModels.Any(t => t.IsSelected);
		}

		/// <summary>
		/// Show guidelines as grid in canvas
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowGuideLinesCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.AreGuideLinesVisible = !ViewModel.AreGuideLinesVisible;
			ModelDesignerCanvas.SetGuideLinesVisible(ViewModel.AreGuideLinesVisible);
		}

		/// <summary>
		/// Add new foreign key
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddForeignKeyCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (ViewModel.TableViewModels.Count(t => t.IsSelected) == 2)
			{
				var dialogCreate = new ForeignKeyCreatorDialog($"{_sourceModel.Model.Title}_{_destinationModel.Model.Title}_FK", _sourceModel, _destinationModel, ViewModel, ModelDesignerCanvas);
				dialogCreate.Owner = Application.Current.MainWindow;
				dialogCreate.ShowDialog();
				return;
			}

			ForeignKeysDialog dialog = new ForeignKeysDialog(ViewModel, ViewModel.ConnectionInfoViewModels.FirstOrDefault(t => t.IsSelected));
			dialog.Owner = Application.Current.MainWindow;
			dialog.Canvas = ModelDesignerCanvas;
			dialog.ShowDialog();
		}

		/// <summary>
		/// Mouse mode changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectionModeCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.MouseMode = MouseMode.Select;
		}

		/// <summary>
		/// Dimensions changed
		/// </summary>
		/// <returns></returns>
		public async Task CanvasDimensionsChanged()
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

			StreamGeometry geometry = DesignerCanvas.CreateGridWithStreamGeometry(ViewModel.CanvasHeight, ViewModel.CanvasWidth,
					DesignerCanvas.GridCellWidth);
			ModelDesignerCanvas.RefreshGuideLines(geometry);

			ViewModel.OnComputedPropertyChanged();
		}

		/// <summary>
		/// Update guidelines
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Create table in DB
		/// </summary>
		protected virtual void OnTableCreated()
		{
			TableCreated?.Invoke(this, System.EventArgs.Empty);
		}

		/// <summary>
		/// Enable/disable table move limits
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EnableTableLimits_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.AreTableLimitsEnabled = !ViewModel.AreTableLimitsEnabled;

			foreach (TableViewModel viewModel in ViewModel.TableViewModels)
			{
				viewModel.AreLimitsEnabled = ViewModel.AreTableLimitsEnabled;
			}
		}

		/// <summary>
		/// Redraw selected line
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void RefreshOneLine_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ConnectionInfoViewModel viewModel = ViewModel.ConnectionInfoViewModels.FirstOrDefault(t => t.IsSelected);

			if (viewModel != null)
			{
				await viewModel.RebuildVisual(ViewModel);
			}
		}

		/// <summary>
		/// Redraw all lines
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void RefeshLines_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (ConnectionInfoViewModel vm in ViewModel.ConnectionInfoViewModels)
			{
				await vm.RebuildVisual(ViewModel);
			}
		}

		/// <summary>
		/// Can redraw line - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RefreshOneLine_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (ViewModel == null)
			{
				return;
			}

			e.CanExecute = ViewModel.ConnectionInfoViewModels.Any(t => t.IsSelected);
		}

		/// <summary>
		/// Export canvas to png
		/// </summary>
		/// <param name="filepath">Path to file</param>
		public void ExportToPng(string filepath)
		{
			RenderTargetBitmap bitmap = RenderCanvas();
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			int offset = 30;
			int left = 0, right = (int)ViewModel.CanvasWidth, top = 0, bot = (int)ViewModel.CanvasHeight;

			if (ViewModel.TableViewModels.Any())
			{
				left = (int)ViewModel.TableViewModels.Select(t => t.Left - offset).Min();
				top = (int)ViewModel.TableViewModels.Select(t => t.Top - offset).Min();
				right = (int)ViewModel.TableViewModels.Select(t => t.Left + t.Width + offset).Max();
				bot = (int)ViewModel.TableViewModels.Select(t => t.Top + t.Height + offset).Max();
			}

			if (ViewModel.ConnectionInfoViewModels.Any())
			{
				left = (int)Math.Min(left, ViewModel.ConnectionInfoViewModels.SelectMany(t => t.Points).Select(t => t.X - offset).Min());
				right = (int)Math.Max(right,ViewModel.ConnectionInfoViewModels.SelectMany(t => t.Points).Select(t => t.X + offset).Max());
				bot = (int)Math.Max(bot, ViewModel.ConnectionInfoViewModels.SelectMany(t => t.Points).Select(t => t.Y + offset).Max());
				top = (int)Math.Min(top,ViewModel.ConnectionInfoViewModels.SelectMany(t => t.Points).Select(t => t.Y - offset).Min());
			}

			left = left < 0 ? 0 : left;
			top = top < 0 ? 0 : top;
			right = right > ViewModel.CanvasWidth ? (int)ViewModel.CanvasWidth : right;
			bot = bot > ViewModel.CanvasHeight ? (int)ViewModel.CanvasHeight : bot;

			BitmapFrame frame = BitmapFrame.Create(bitmap);
			encoder.Frames.Add(frame);

			using (MemoryStream ms = new MemoryStream())
			{
				encoder.Save(ms);
				ms.Seek(0, SeekOrigin.Begin);

				var crop = new System.Drawing.Rectangle(left, top, right - left, bot - top);
				var cropped = new Bitmap(crop.Width, crop.Height);
				var img = Image.FromStream(ms);
				using (Graphics g = Graphics.FromImage(cropped))
				{
					g.DrawImage(img, -crop.X, -crop.Y);
					cropped.Save(filepath);
				}
			}

			Output.WriteLine("FILE SAVED");
		}

		/// <summary>
		/// Export whole canva to png
		/// </summary>
		/// <param name="filepath">Path to file</param>
		public async Task ExportToPngFullSize(string filepath)
		{
			await Dispatcher.BeginInvoke(new Action(() =>
			{
				RenderTargetBitmap bitmap = RenderCanvas();
				PngBitmapEncoder encoder = new PngBitmapEncoder();
				BitmapFrame frame = BitmapFrame.Create(bitmap);
				encoder.Frames.Add(frame);

				using(var fs = File.Create(filepath))
				{
					encoder.Save(fs);
				}

				Output.WriteLine("FILE SAVED");
			}), DispatcherPriority.Background);
		}

		/// <summary>
		/// Render canvas to bitmap
		/// </summary>
		/// <returns>Rendered canvas</returns>
		private RenderTargetBitmap RenderCanvas()
		{
			Transform layoutTransform = ModelDesignerCanvas.LayoutTransform;
			ModelDesignerCanvas.LayoutTransform = null;

			double dpi = 300;
			double scale = dpi / 96;

			var size = new Size((int)ViewModel.CanvasWidth, (int)ViewModel.CanvasHeight);
			ModelDesignerCanvas.Measure(size);
			ModelDesignerCanvas.Arrange(new Rect(size));

			RenderTargetBitmap render = new RenderTargetBitmap((int)(ViewModel.CanvasWidth*scale), (int)(ViewModel.CanvasHeight*scale), dpi, dpi, PixelFormats.Default);
			render.Render(ModelDesignerCanvas);

			ModelDesignerCanvas.LayoutTransform = layoutTransform;
			return render;
		}

		/// <summary>
		/// Export whole canva to XPS
		/// </summary>
		/// <param name="fileName">Path to file</param>
		public void ExportToXps(string fileName)
		{
			
			Transform layoutTransform = ModelDesignerCanvas.LayoutTransform;
			ModelDesignerCanvas.LayoutTransform = null;

			var size = new Size(ViewModel.CanvasWidth, ViewModel.CanvasHeight);
			ModelDesignerCanvas.Measure(size);
			ModelDesignerCanvas.Arrange(new Rect(size));

			Package package = Package.Open(fileName, FileMode.Create);
			XpsDocument document = new XpsDocument(package);

			XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(document);
			writer.Write(ModelDesignerCanvas);

			document.Close();
			package.Close();

			ModelDesignerCanvas.LayoutTransform = layoutTransform;
			Output.WriteLine("FILE SAVED");
		}
	}
}
