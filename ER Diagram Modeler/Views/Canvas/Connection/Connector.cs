using System.IO;
using Path = System.Windows.Shapes.Path;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class Connector
	{
		public Path ConnectorPath { get; }
		public ConnectionPoint EndPoint { get; set; }
		public ConnectionPoint TableRelatedPoint { get; set; }
		public Orien Type { get; set; }

		public Connector()
		{
			ConnectorPath = new Path();
		}

	}
}