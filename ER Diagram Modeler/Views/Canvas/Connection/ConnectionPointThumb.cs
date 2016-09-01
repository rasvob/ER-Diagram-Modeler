using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionPointThumb: Thumb
	{
		public ConnectionPoint Point { get; set; }
		public ConnectionLine Line1 { get; set; }
		public ConnectionLine Line2 { get; set; }

		public ConnectionPointThumb()
		{
			Style = Application.Current.FindResource("ConnectionPointStyle") as Style;

			DragStarted += OnDragStarted;
			DragDelta += OnDragDelta;
		}

		private void OnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
		{
			
		}

		private void OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			
		}
	}
}