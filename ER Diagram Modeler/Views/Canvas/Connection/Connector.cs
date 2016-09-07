using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using Path = System.Windows.Shapes.Path;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class Connector
	{
		public Path Symbol { get; }
		public Path ConnectionPath { get; set; }

		public ConnectionPoint EndPoint
		{
			get { return _endPoint; }
			set
			{
				_endPoint = value;
				EndPoint.CoordinatesChanged += EndPointOnCoordinatesChanged;
			}
		}

		private void EndPointOnCoordinatesChanged(object sender, ConnectionPointEventArgs connectionPointEventArgs)
		{
			UpdateConnector();
		}

		public TableViewModel TableViewModel { get; set; }

		public ConnectorOrientation Orientation
		{
			get { return _orientation; }
			set
			{
				_orientation = value;
				UpdateConnector();
			}
		}

		private ConnectorOrientation _orientation;
		private readonly SolidColorBrush _unselectedColor = Application.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
		private readonly SolidColorBrush _selectedColor = Application.Current.FindResource("PrimaryColorDarkBrush") as SolidColorBrush;
		private ConnectionPoint _endPoint;
		private PathGeometry _symbolGeometry;
		private PathGeometry _connectionGeometry;

		public Connector()
		{
			Symbol = new Path();
			ConnectionPath = new Path();
			_symbolGeometry = new PathGeometry();
			_connectionGeometry = new PathGeometry();

			Symbol.SnapsToDevicePixels = true;
			Symbol.Stroke = _unselectedColor;
			Symbol.StrokeThickness = ConnectionLine.StrokeThickness;
			Symbol.StrokeEndLineCap = PenLineCap.Flat;
			Symbol.Data = _symbolGeometry;

			ConnectionPath.SnapsToDevicePixels = true;
			ConnectionPath.Stroke = _unselectedColor;
			ConnectionPath.StrokeThickness = ConnectionLine.StrokeThickness;
			ConnectionPath.StrokeEndLineCap = PenLineCap.Round;
			ConnectionPath.StrokeStartLineCap = PenLineCap.Round;
			ConnectionPath.Data = _connectionGeometry;
		}

		public void UpdateConnector()
		{
			switch(Orientation)
			{
				case ConnectorOrientation.Up:
					
					break;
				case ConnectorOrientation.Down:

					break;
				case ConnectorOrientation.Left:
					
					break;
				case ConnectorOrientation.Right:

					break;
			}
		}
	}
}