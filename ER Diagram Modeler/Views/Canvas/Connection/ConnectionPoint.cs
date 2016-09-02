using System;
using System.Windows;
using ER_Diagram_Modeler.EventArgs;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionPoint
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

		public void MoveByOffset(double xOffset, double yOffset)
		{
			X += xOffset;
			Y += yOffset;
		}

		public override string ToString()
		{
			return $"[{X}|{Y}]";
		}

		public bool IsEqual(ConnectionPoint compared)
		{
			return X == compared.X && Y == compared.Y;
		}

		public event EventHandler<ConnectionPointEventArgs> CoordinatesChanged;

		protected virtual void OnCoordinatesChanged(ConnectionPointEventArgs e)
		{
			CoordinatesChanged?.Invoke(this, e);
		}
	}
}