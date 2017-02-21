using System.Windows;
using ER_Diagram_Modeler.Views.Canvas.Connection;

namespace ER_Diagram_Modeler.EventArgs
{
	/// <summary>
	/// Connection line before move EventArgs
	/// </summary>
	public class ConnectionLineBeforeMoveEventArgs
	{
		public ConnectionPoint OriginalStartPoint { get; set; }
		public ConnectionPoint OriginalEndPoint { get; set; }
	}
}