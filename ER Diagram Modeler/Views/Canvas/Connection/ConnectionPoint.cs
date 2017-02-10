using System;
using System.Windows;
using System.Xml.Linq;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.EventArgs;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionPoint: IEquatable<ConnectionPoint>, IDiagramSerializable
	{
		private double _x;
		private double _y;

		public double X
		{
			get { return _x; }
			set
			{
				_x = value;
				OnCoordinatesChanged(new ConnectionPointEventArgs() { X = _x, Y = _y });
			}
		}

		public double Y
		{
			get { return _y; }
			set
			{
				_y = value;
				OnCoordinatesChanged(new ConnectionPointEventArgs() {X = _x, Y = _y});
			}
		}

		public static readonly double EqualityTolerance = 0.005;

		public ConnectionPoint()
		{
			
		}

		public ConnectionPoint(double x, double y)
		{
			X = x;
			Y = y;
		}

		public void MoveByOffset(double xOffset, double yOffset)
		{
			X += xOffset;
			Y += yOffset;
		}

		public bool Equals(ConnectionPoint other)
		{
			return (Math.Abs(X - other.X) < EqualityTolerance) && (Math.Abs(Y - other.Y) < EqualityTolerance);
		}

		public double GetDistanceToPoint(ConnectionPoint other)
		{
			return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 13;
				hash = 31*hash + X.GetHashCode();
				hash = 31*hash + Y.GetHashCode();
				return hash;
			}
		}

		public override string ToString()
		{
			return $"[{X}|{Y}]";
		}

		public event EventHandler<ConnectionPointEventArgs> CoordinatesChanged;

		protected virtual void OnCoordinatesChanged(ConnectionPointEventArgs e)
		{
			CoordinatesChanged?.Invoke(this, e);
		}

		public XElement CreateElement()
		{
			return new XElement("ConnectionPoint", new XAttribute("X", X), new XAttribute("Y", Y));
		}

		public void LoadFromElement(XElement element)
		{
			X = Convert.ToDouble(element.Attribute("X")?.Value);
			Y = Convert.ToDouble(element.Attribute("Y")?.Value);
		}
	}
}