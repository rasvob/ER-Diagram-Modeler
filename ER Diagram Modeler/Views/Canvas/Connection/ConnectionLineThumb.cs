using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionLineThumb: Thumb
	{
		private ConnectionLine _line;
		private DesignerCanvas _canvas;

		public ConnectionLineThumb()
		{
			DragStarted += OnDragStarted;
			DragDelta += OnDragDelta;
			DragCompleted += OnDragCompleted;
		}

		private void OnDragCompleted(object sender, DragCompletedEventArgs dragCompletedEventArgs)
		{
			Trace.WriteLine("Complete");
		}

		private void OnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
		{
			Trace.WriteLine(dragDeltaEventArgs.HorizontalChange);
		}

		private void OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			_line = DataContext as ConnectionLine;

			if(_line != null)
			{
				_canvas = VisualTreeHelper.GetParent(_line) as DesignerCanvas;
			}
		}
	}
}