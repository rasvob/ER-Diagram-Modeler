using System;
using System.Windows;
using ER_Diagram_Modeler.EventArgs;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionPoint: IEquatable<ConnectionPoint>
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
			return X == other.X && Y == other.Y;
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
	}
}