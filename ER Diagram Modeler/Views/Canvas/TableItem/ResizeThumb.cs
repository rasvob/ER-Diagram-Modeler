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
		private Adorner _adorner;
		private Point _transformOrigin;
		private ContentControl _designerItem;
		private System.Windows.Controls.Canvas _canvas;

		public ResizeThumb()
		{
			DragStarted += ResizeThumb_DragStarted;
			DragDelta += ResizeThumb_DragDelta;
			DragCompleted += ResizeThumb_DragCompleted;
		}

		private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
		{
			_designerItem = DataContext as ContentControl;

			if(_designerItem != null)
			{
				_canvas = VisualTreeHelper.GetParent(_designerItem) as System.Windows.Controls.Canvas;

				if(_canvas != null)
				{
					_transformOrigin = _designerItem.RenderTransformOrigin;
				}
			}
		}

		private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if(_designerItem != null)
			{
				double deltaVertical, deltaHorizontal;

				switch(VerticalAlignment)
				{
					case VerticalAlignment.Bottom:
						deltaVertical = Math.Min(-e.VerticalChange, _designerItem.ActualHeight - _designerItem.MinHeight);
						_designerItem.Height -= deltaVertical;
						break;
					case VerticalAlignment.Top:
						deltaVertical = Math.Min(e.VerticalChange, _designerItem.ActualHeight - _designerItem.MinHeight);
						System.Windows.Controls.Canvas.SetTop(_designerItem, System.Windows.Controls.Canvas.GetTop(_designerItem) + deltaVertical);
						_designerItem.Height -= deltaVertical;
						break;
				}

				switch(HorizontalAlignment)
				{
					case HorizontalAlignment.Left:
						deltaHorizontal = Math.Min(e.HorizontalChange, _designerItem.ActualWidth - _designerItem.MinWidth);
						System.Windows.Controls.Canvas.SetLeft(_designerItem, System.Windows.Controls.Canvas.GetLeft(_designerItem) + deltaHorizontal);
						_designerItem.Width -= deltaHorizontal;
						break;
					case HorizontalAlignment.Right:
						deltaHorizontal = Math.Min(-e.HorizontalChange, _designerItem.ActualWidth - _designerItem.MinWidth);
						_designerItem.Width -= deltaHorizontal;
						break;
				}
			}

			e.Handled = true;
		}

		private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			if (_adorner == null) return;
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
			adornerLayer?.Remove(_adorner);
			_adorner = null;
		}
	}
}

