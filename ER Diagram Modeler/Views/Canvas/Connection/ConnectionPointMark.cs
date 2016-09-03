using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ER_Diagram_Modeler.EventArgs;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionPointMark
	{
		private ConnectionPoint _point;
		private SolidColorBrush _color = Application.Current.FindResource("PrimaryColorDarkBrush") as SolidColorBrush;
		private readonly EllipseGeometry _geometry;
		private const int Radius = 5;

		public ConnectionPoint Point
		{
			get { return _point; }
			set
			{
				_point = value; 
				_point.CoordinatesChanged += PointOnCoordinatesChanged;
				SetEllipsePosition();
			}
		}

		private void PointOnCoordinatesChanged(object sender, ConnectionPointEventArgs connectionPointEventArgs)
		{
			SetEllipsePosition();
		}

		public Path Mark { get; }

		public ConnectionPointMark()
		{
			Mark = new Path();
			_geometry = new EllipseGeometry();
			_geometry.RadiusX = Radius;
			_geometry.RadiusY = Radius;
			Mark.Data = _geometry;
			Mark.Fill = _color;
			Mark.Visibility = Visibility.Hidden;
		}

		private void SetEllipsePosition()
		{
			_geometry.Center = new Point(_point.X, _point.Y);
		}
	}
}