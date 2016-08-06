using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	public class MoveThumb: Thumb
	{
		public MoveThumb()
		{
			DragDelta += OnDragDelta;
		}

		private void OnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
		{
			var item = DataContext as ContentControl;

			if (item != null)
			{
				Point dragDelta = new Point((int)dragDeltaEventArgs.HorizontalChange, (int)dragDeltaEventArgs.VerticalChange);

				System.Windows.Controls.Canvas.SetLeft(item, System.Windows.Controls.Canvas.GetLeft(item) + dragDelta.X);
				System.Windows.Controls.Canvas.SetTop(item, System.Windows.Controls.Canvas.GetTop(item) + dragDelta.Y);
			}
		}
	}
}