using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	public class MoveThumb: Thumb
	{
		private DesignerCanvas _canvas;
		private TableContent _item;

		public MoveThumb()
		{
			DragDelta += OnDragDelta;
			DragStarted += OnDragStarted;
			DragCompleted += OnDragCompleted;
		}

		private void OnDragCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
		{
			_item?.TableViewModel.OnPositionAndMeasureChangesCompleted();
		}

		private void OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			_item = DataContext as TableContent;

			if (_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
				_item.TableViewModel.OnPositionAndMeasureChangesStarted();
			}
		}

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

				double deltaHorizontal = Math.Max(-minLeft, dragDeltaEventArgs.HorizontalChange);
				double deltaVertical = Math.Max(-minTop, dragDeltaEventArgs.VerticalChange);

				if (maxLeft >= _canvas.ActualWidth && dragDeltaEventArgs.HorizontalChange > 0)
				{
					deltaHorizontal = 0;
				}

				if (maxTop >= _canvas.ActualHeight && dragDeltaEventArgs.VerticalChange > 0)
				{
					deltaVertical = 0;
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
	}
}