using System.Windows;
using ER_Diagram_Modeler.Views.Canvas.Connection;

namespace ER_Diagram_Modeler.EventArgs
{
	public class ConnectionLineMovingEventArgs
	{
		public ConnectionPoint OriginalStartPoint { get; set; }
		public ConnectionPoint OriginalEndPoint { get; set; }
		public Vector Offset { get; set; }
	}
}