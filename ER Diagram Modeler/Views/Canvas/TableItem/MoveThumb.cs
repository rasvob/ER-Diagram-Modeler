using System;
using System.Diagnostics;
using System.Drawing;
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
		}

		private void OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			_item = DataContext as TableContent;

			if (_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
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

				foreach (TableContent item in _canvas.SelectedItems)
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

				foreach (TableContent item in _canvas.SelectedItems)
				{
					item.TableViewModel.Left = DesignerCanvas.GetLeft(item) + deltaHorizontal;
					item.TableViewModel.Top = DesignerCanvas.GetTop(item) + deltaVertical;
					DesignerCanvas.SetLeft(item, item.TableViewModel.Left);
					DesignerCanvas.SetTop(item, item.TableViewModel.Top);
				}

				dragDeltaEventArgs.Handled = true;
			}


			//var item = DataContext as ContentControl;
			//if (item == null) return;
			//Point dragDelta = new Point((int)dragDeltaEventArgs.HorizontalChange, (int)dragDeltaEventArgs.VerticalChange);
			//System.Windows.Controls.Canvas.SetLeft(item, System.Windows.Controls.Canvas.GetLeft(item) + dragDelta.X);
			//System.Windows.Controls.Canvas.SetTop(item, System.Windows.Controls.Canvas.GetTop(item) + dragDelta.Y);
		}
	}
}