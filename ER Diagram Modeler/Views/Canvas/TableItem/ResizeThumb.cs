using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	class ResizeThumb: Thumb
	{
		private TableContent _item;
		private DesignerCanvas _canvas;

		public ResizeThumb()
		{
			DragStarted += ResizeThumb_DragStarted;
			DragDelta += ResizeThumb_DragDelta;
			DragCompleted += OnDragCompleted;
		}

		private void OnDragCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
		{
			_item.TableViewModel.OnPositionAndMeasureChangesCompleted();
		}

		private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
		{
			_item = DataContext as TableContent;

			if (_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
				_item.TableViewModel.OnPositionAndMeasureChangesStarted();
			}
		}

		private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if(_item != null && _canvas != null && _item.IsSelected)
			{
				//double minLeft = double.MaxValue;
				//double minTop = double.MaxValue;

				//double maxTop = double.MinValue;
				//double maxLeft = double.MinValue;

				double maxDeltaVertical = _canvas.ActualHeight - (DesignerCanvas.GetTop(_item) + _item.ActualHeight);
				double maxDeltaHorizontal = _canvas.ActualWidth - (DesignerCanvas.GetLeft(_item) + _item.ActualWidth);

				var minDeltaVertical = _item.ActualHeight - _item.MinHeight;
				var minDeltaHorizontal = _item.ActualWidth - _item.MinWidth;

				//Bad approach
				//foreach(TableContent item in _canvas.SelectedTables)
				//{
				//	minLeft = Math.Min(DesignerCanvas.GetLeft(item), minLeft);
				//	minTop = Math.Min(DesignerCanvas.GetTop(item), minTop);

				//	maxLeft = Math.Max(DesignerCanvas.GetLeft(item) + item.ActualWidth, maxLeft);
				//	maxTop = Math.Max(DesignerCanvas.GetTop(item) + item.ActualHeight, maxTop);

				//	if (item.TableViewModel.ViewMode != TableViewMode.NameOnly)
				//	{
				//		minDeltaVertical = Math.Min(item.ActualHeight - item.MinHeight, minDeltaVertical);
				//	}
				//	minDeltaHorizontal = Math.Min(item.ActualWidth - item.MinWidth, minDeltaHorizontal);

				//	maxDeltaVertical = Math.Min(_canvas.ActualHeight - (DesignerCanvas.GetTop(item) + item.ActualHeight), maxDeltaVertical);
				//	maxDeltaHorizontal = Math.Min(_canvas.ActualWidth - (DesignerCanvas.GetLeft(item) + item.ActualWidth), maxDeltaHorizontal);
				//}

					if (_item.TableViewModel.ViewMode != TableViewMode.NameOnly)
					{
						double deltaVertical;
						switch (VerticalAlignment)
						{
							case VerticalAlignment.Bottom:
								deltaVertical = e.VerticalChange > 0 && e.VerticalChange >= maxDeltaVertical ? 0 : Math.Min(-e.VerticalChange, minDeltaVertical);
								_item.Height = _item.ActualHeight - (int)deltaVertical;
								_item.TableViewModel.Height = _item.Height;
								break;
							case VerticalAlignment.Top:
								deltaVertical = (int)Math.Min(Math.Max(-DesignerCanvas.GetTop(_item), e.VerticalChange), minDeltaVertical);
								var topPos = DesignerCanvas.GetTop(_item) + deltaVertical;
								DesignerCanvas.SetTop(_item, topPos);
								_item.Height = _item.ActualHeight - deltaVertical;
								_item.TableViewModel.Height = _item.Height;
								_item.TableViewModel.Top = topPos;
								break;
						}
					}

					double deltaHorizontal;
					switch (HorizontalAlignment)
					{
						case HorizontalAlignment.Left:
							deltaHorizontal = (int)Math.Min(Math.Max(-DesignerCanvas.GetLeft(_item), e.HorizontalChange), minDeltaHorizontal);
							var leftPos = DesignerCanvas.GetLeft(_item) + deltaHorizontal;
							DesignerCanvas.SetLeft(_item, leftPos);
							_item.Width = _item.ActualWidth - deltaHorizontal;
							_item.TableViewModel.Width = _item.Width;
							_item.TableViewModel.Left = leftPos;
							break;
						case HorizontalAlignment.Right:
							deltaHorizontal = e.HorizontalChange > 0 && e.HorizontalChange >= maxDeltaHorizontal ? 0 : (int)Math.Min(-e.HorizontalChange, minDeltaHorizontal);
							_item.Width = _item.ActualWidth - deltaHorizontal;
							_item.TableViewModel.Width = _item.Width;
							break;
					}
				}
			e.Handled = true;
		}
	}
}

