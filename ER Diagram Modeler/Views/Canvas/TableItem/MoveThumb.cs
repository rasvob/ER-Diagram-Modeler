using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.Connection;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	/// <summary>
	/// Wrapper for moving items on canvas
	/// </summary>
	public class MoveThumb: Thumb
	{
		private DesignerCanvas _canvas;
		private TableContent _item;
		private List<ConnectionInfoViewModel> _connections; 

		public MoveThumb()
		{
			DragDelta += OnDragDelta;
			DragStarted += OnDragStarted;
			DragCompleted += OnDragCompleted;
		}

		/// <summary>
		/// Mouse drag completed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dragCompletedEventArgs"></param>
		private void OnDragCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
		{
			SnapToGrid();
			_item?.TableViewModel.OnPositionAndMeasureChangesCompleted();
			_item.TableViewModel.IsMoving = false;
		}

		/// <summary>
		/// Mouse drag started
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dragStartedEventArgs"></param>
		private void OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			_item = DataContext as TableContent;

			if (_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
				if (_canvas != null)
				{
					_connections = VisualTreeHelperEx.FindAncestorByType<DatabaseModelDesigner>(_canvas).ViewModel.ConnectionInfoViewModels
						.Where(t => !(t.DestinationViewModel.IsSelected && t.SourceViewModel.IsSelected))
						.ToList();
				}
				_item.TableViewModel.OnPositionAndMeasureChangesStarted();
				_item.TableViewModel.IsMoving = true;
			}
		}

		/// <summary>
		/// Mouse drag in progress
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dragDeltaEventArgs"></param>
		private void OnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
		{
			if (_item != null && _canvas != null && _item.IsSelected)
			{
				double minLeft = double.MaxValue;
				double minTop = double.MaxValue;

				double maxTop = double.MinValue;
				double maxLeft = double.MinValue;

				foreach (TableContent item in _canvas.SelectedTables)
				{
					minLeft = Math.Min(DesignerCanvas.GetLeft(item), minLeft);
					minTop = Math.Min(DesignerCanvas.GetTop(item), minTop);
					maxLeft = Math.Max(DesignerCanvas.GetLeft(item) + item.ActualWidth, maxLeft);
					maxTop = Math.Max(DesignerCanvas.GetTop(item) + item.ActualHeight, maxTop);
				}

				double deltaHorizontal = (int)Math.Max(-minLeft, dragDeltaEventArgs.HorizontalChange);
				double deltaVertical = (int)Math.Max(-minTop, dragDeltaEventArgs.VerticalChange);

				//Limit movement by connection position
				double topConnectionLimit = double.MaxValue;
				double bottomConnectionLimit = double.MaxValue;
				double leftConnectionLimit = double.MaxValue;
				double rightConnectionLimit = double.MaxValue;

				foreach(TableContent item in _canvas.SelectedTables)
				{
					foreach(ConnectionInfoViewModel model in _connections.Where(t => t.SourceViewModel.Equals(item.TableViewModel)))
					{
						ConnectionInfoViewModel.GetConnectionLimits(ref topConnectionLimit, ref bottomConnectionLimit, ref leftConnectionLimit, ref rightConnectionLimit, model.SourceConnector, model);
					}

					foreach(ConnectionInfoViewModel model in _connections.Where(t => t.DestinationViewModel.Equals(item.TableViewModel)))
					{
						ConnectionInfoViewModel.GetConnectionLimits(ref topConnectionLimit, ref bottomConnectionLimit, ref leftConnectionLimit, ref rightConnectionLimit, model.DestinationConnector, model);
					}
				}

				if (_item.TableViewModel.AreLimitsEnabled)
				{
					if (topConnectionLimit < double.MaxValue)
					{
						if (deltaVertical < 0 && minTop + deltaVertical <= minTop - topConnectionLimit)
						{
							deltaVertical = 0;
						}
					}

					if (bottomConnectionLimit < double.MaxValue)
					{
						if (deltaVertical > 0 && maxTop + deltaVertical >= maxTop + bottomConnectionLimit)
						{
							deltaVertical = 0;
						}
					}

					if (leftConnectionLimit < double.MaxValue)
					{
						if (deltaHorizontal < 0 && minLeft + deltaHorizontal <= minLeft - leftConnectionLimit)
						{
							deltaHorizontal = 0;
						}
					}

					if (rightConnectionLimit < double.MaxValue)
					{
						if (deltaHorizontal > 0 && maxLeft + deltaHorizontal >= maxLeft + rightConnectionLimit)
						{
							deltaHorizontal = 0;
						}
					}

					if (maxLeft >= _canvas.ActualWidth && dragDeltaEventArgs.HorizontalChange > 0)
					{
						deltaHorizontal = 0;
					}

					if (maxTop >= _canvas.ActualHeight && dragDeltaEventArgs.VerticalChange > 0)
					{
						deltaVertical = 0;
					}
				}

				var selected = _canvas.SelectedTables.ToArray();

				foreach (TableContent item in selected)
				{
					var leftPos = DesignerCanvas.GetLeft(item) + deltaHorizontal;
					var topPos = DesignerCanvas.GetTop(item) + deltaVertical;
					DesignerCanvas.SetLeft(item, leftPos);
					DesignerCanvas.SetTop(item, topPos);
					item.TableViewModel.Left = leftPos;
					item.TableViewModel.Top = topPos;
				}

				dragDeltaEventArgs.Handled = true;
			}
		}

		/// <summary>
		/// If is grid enabled, snap to intersects
		/// </summary>
		private void SnapToGrid()
		{
			if (_canvas.IsGridEnabled)
			{
				int cellWidth = DesignerCanvas.GridCellWidth;

				var selected = _canvas.SelectedTables.ToArray();

				double topConnectionLimit = double.MaxValue;
				double bottomConnectionLimit = double.MaxValue;
				double leftConnectionLimit = double.MaxValue;
				double rightConnectionLimit = double.MaxValue;

				foreach(TableContent item in _canvas.SelectedTables)
				{
					foreach(ConnectionInfoViewModel model in _connections.Where(t => t.SourceViewModel.Equals(item.TableViewModel)))
					{
						ConnectionInfoViewModel.GetConnectionLimits(ref topConnectionLimit, ref bottomConnectionLimit, ref leftConnectionLimit, ref rightConnectionLimit, model.SourceConnector, model);
					}

					foreach(ConnectionInfoViewModel model in _connections.Where(t => t.DestinationViewModel.Equals(item.TableViewModel)))
					{
						ConnectionInfoViewModel.GetConnectionLimits(ref topConnectionLimit, ref bottomConnectionLimit, ref leftConnectionLimit, ref rightConnectionLimit, model.DestinationConnector, model);
					}
				}

				foreach(TableContent item in selected)
				{
					double top = item.TableViewModel.Top;
					double left = item.TableViewModel.Left;
					double bottom = top + item.TableViewModel.Height;
					double right = left + item.TableViewModel.Width;

					double approxCellTop = Math.Round(top / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;
					double approxCellLeft = Math.Round(left / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;

					if (WillSnapToGridBrokeConnections(topConnectionLimit, leftConnectionLimit, bottomConnectionLimit, rightConnectionLimit, top, left, bottom, right, approxCellTop, approxCellLeft))
					{
						continue;
					}
					
					DesignerCanvas.SetLeft(item, approxCellLeft);
					DesignerCanvas.SetTop(item, approxCellTop);
					item.TableViewModel.Left = approxCellLeft;
					item.TableViewModel.Top = approxCellTop;
				}
			}
		}

		/// <summary>
		/// Are connections breaking connections - check
		/// </summary>
		/// <param name="topLimit">Top offset limit</param>
		/// <param name="leftLimit">Left offset limit</param>
		/// <param name="botLimit">Bottom offset limit</param>
		/// <param name="rightLimit">Right offset limit</param>
		/// <param name="top">Top offset</param>
		/// <param name="left">Left offset</param>
		/// <param name="bottom">Bottom offset</param>
		/// <param name="right">Right offset</param>
		/// <param name="approxTop">Cell top offset</param>
		/// <param name="approxLeft">Cell left offset</param>
		/// <returns>True of is out of bounds, false if not</returns>
		private bool WillSnapToGridBrokeConnections(double topLimit, double leftLimit, double botLimit, double rightLimit, double top, double left, double bottom, double right, double approxTop, double approxLeft)
		{
			if (topLimit < double.MaxValue && approxTop < top - topLimit)
			{
				return true;
			}

			if(leftLimit < double.MaxValue && approxLeft < left - leftLimit)
			{
				return true;
			}

			if(botLimit < double.MaxValue && approxTop + (bottom - top) > bottom + botLimit)
			{
				return true;
			}

			if(rightLimit < double.MaxValue && approxLeft + (right - left) > right + rightLimit)
			{
				return true;
			}

			return false;
		}
	}
}