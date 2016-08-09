using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

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
		}

		private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
		{
			_item = DataContext as TableContent;

			if (_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
			}
		}

		private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if(_item != null && _canvas != null && _item.IsSelected)
			{
				double minLeft = double.MaxValue;
				double minTop = double.MaxValue;

				double maxTop = double.MinValue;
				double maxLeft = double.MinValue;

				double deltaVertical, deltaHorizontal;
				double minDeltaVertical = double.MaxValue; 
				double minDeltaHorizontal = double.MaxValue;

				foreach(TableContent item in _canvas.SelectedItems)
				{
					minLeft = Math.Min(DesignerCanvas.GetLeft(item), minLeft);
					minTop = Math.Min(DesignerCanvas.GetTop(item), minTop);

					minDeltaVertical = Math.Min(item.ActualHeight - item.MinHeight, minDeltaVertical);
					minDeltaHorizontal = Math.Min(item.ActualWidth - item.MinWidth, minDeltaHorizontal);
				}

				foreach(TableContent item in _canvas.SelectedItems)
				{
					switch (VerticalAlignment)
					{
						case VerticalAlignment.Bottom:
							deltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
							item.Height = item.ActualHeight - deltaVertical;
							break;
						case VerticalAlignment.Top:
							deltaVertical = Math.Min(e.VerticalChange, _item.ActualHeight - _item.MinHeight);
							System.Windows.Controls.Canvas.SetTop(_item, System.Windows.Controls.Canvas.GetTop(_item) + deltaVertical);
							_item.Height -= deltaVertical;
							break;
					}

					switch (HorizontalAlignment)
					{
						case HorizontalAlignment.Left:
							deltaHorizontal = Math.Min(e.HorizontalChange, _item.ActualWidth - _item.MinWidth);
							System.Windows.Controls.Canvas.SetLeft(_item, System.Windows.Controls.Canvas.GetLeft(_item) + deltaHorizontal);
							_item.Width -= deltaHorizontal;
							break;
						case HorizontalAlignment.Right:
							deltaHorizontal = Math.Min(-e.HorizontalChange, _item.ActualWidth - _item.MinWidth);
							_item.Width -= deltaHorizontal;
							break;
					}
				}
			}

			e.Handled = true;
		}

	}
}

