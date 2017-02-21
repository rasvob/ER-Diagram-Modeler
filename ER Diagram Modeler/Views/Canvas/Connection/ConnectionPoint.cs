using System;
using System.Windows;
using System.Xml.Linq;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.EventArgs;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	/// <summary>
	/// Point with X and Y coordinates and value change events
	/// </summary>
	public class ConnectionPoint: IEquatable<ConnectionPoint>, IDiagramSerializable
	{
		private double _x;
		private double _y;

		/// <summary>
		/// X coordinate
		/// </summary>
		public double X
		{
			get { return _x; }
			set
			{
				_x = value;
				OnCoordinatesChanged(new ConnectionPointEventArgs() { X = _x, Y = _y });
			}
		}

		/// <summary>
		/// Y coordinate
		/// </summary>
		public double Y
		{
			get { return _y; }
			set
			{
				_y = value;
				OnCoordinatesChanged(new ConnectionPointEventArgs() {X = _x, Y = _y});
			}
		}

		/// <summary>
		/// Tolerance for IEquatable.Equals()
		/// </summary>
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

		/// <summary>
		/// Is point equal to other
		/// </summary>
		/// <param name="other">Other point</param>
		/// <returns>True if is equal, false if not</returns>
		public bool Equals(ConnectionPoint other)
		{
			return (Math.Abs(X - other.X) < EqualityTolerance) && (Math.Abs(Y - other.Y) < EqualityTolerance);
		}

		/// <summary>
		/// Distance to other point
		/// </summary>
		/// <param name="other">Other point</param>
		/// <returns>Distance between points</returns>
		public double GetDistanceToPoint(ConnectionPoint other)
		{
			return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
		}

		/// <summary>
		/// Get hash code
		/// </summary>
		/// <returns>Get hash code</returns>
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

		/// <summary>
		/// Coords as vector
		/// </summary>
		/// <returns>String with both coordination</returns>
		public override string ToString()
		{
			return $"[{X}|{Y}]";
		}

		/// <summary>
		/// Coordinations changed event
		/// </summary>
		public event EventHandler<ConnectionPointEventArgs> CoordinatesChanged;

		protected virtual void OnCoordinatesChanged(ConnectionPointEventArgs e)
		{
			CoordinatesChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Create XML element
		/// </summary>
		/// <returns>XML serialized data</returns>
		public XElement CreateElement()
		{
			return new XElement("ConnectionPoint", new XAttribute("X", X), new XAttribute("Y", Y));
		}

		/// <summary>
		/// Load property values from XML element
		/// </summary>
		/// <param name="element">XML serialized data from CreateElement()</param>
		public void LoadFromElement(XElement element)
		{
			X = Convert.ToDouble(element.Attribute("X")?.Value);
			Y = Convert.ToDouble(element.Attribute("Y")?.Value);
		}
	}
}