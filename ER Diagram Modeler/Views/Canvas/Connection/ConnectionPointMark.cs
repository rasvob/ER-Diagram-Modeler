using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using ER_Diagram_Modeler.EventArgs;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	/// <summary>
	/// Bending point mark visualization
	/// </summary>
	public class ConnectionPointMark
	{
		private ConnectionPoint _point;
		private SolidColorBrush _color = Application.Current.FindResource("PrimaryColorDarkBrush") as SolidColorBrush;
		private readonly EllipseGeometry _geometry;
		private const int Radius = 5;

		/// <summary>
		/// Coordinates
		/// </summary>
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

		/// <summary>
		/// Coordinates changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="connectionPointEventArgs"></param>
		private void PointOnCoordinatesChanged(object sender, ConnectionPointEventArgs connectionPointEventArgs)
		{
			SetEllipsePosition();
		}

		/// <summary>
		/// Circle mark
		/// </summary>
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

		/// <summary>
		/// Set circle position
		/// </summary>
		private void SetEllipsePosition()
		{
			_geometry.Center = new Point(_point.X, _point.Y);
		}
	}
}