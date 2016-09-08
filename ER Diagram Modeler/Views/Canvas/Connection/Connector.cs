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
		public Optionality Optionality { get; set; } = Optionality.Mandatory;
		public Cardinality Cardinality { get; set; }

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

		public ConnectorOrientation Orientation
		{
			get { return _orientation; }
			set
			{
				_orientation = value;
				UpdateConnector();
			}
		}

		public static readonly double ConnectionPathLength = 25;
		public static readonly double SymbolLinesLength = 20;
		public static readonly double SymbolLineEndsDiff = 8;
		public static readonly DoubleCollection DashArray = new DoubleCollection() { 2 };

		private ConnectorOrientation _orientation;
		private readonly SolidColorBrush _unselectedColor = Application.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
		private readonly SolidColorBrush _selectedColor = Application.Current.FindResource("PrimaryColorDarkBrush") as SolidColorBrush;
		private ConnectionPoint _endPoint;
		private readonly PathGeometry _symbolGeometry;
		private readonly PathGeometry _connectionGeometry;

		public Connector()
		{
			Symbol = new Path();
			ConnectionPath = new Path();
			_symbolGeometry = new PathGeometry();
			_connectionGeometry = new PathGeometry();

			Symbol.SnapsToDevicePixels = true;
			Symbol.Stroke = _unselectedColor;
			Symbol.StrokeThickness = ConnectionLine.StrokeThickness;
			Symbol.StrokeEndLineCap = PenLineCap.Round;
			Symbol.StrokeStartLineCap = PenLineCap.Round;
			Symbol.StrokeDashCap = PenLineCap.Round;
			Symbol.Data = _symbolGeometry;

			ConnectionPath.SnapsToDevicePixels = true;
			ConnectionPath.Stroke = _unselectedColor;
			ConnectionPath.StrokeThickness = ConnectionLine.StrokeThickness;
			ConnectionPath.StrokeEndLineCap = PenLineCap.Round;
			ConnectionPath.StrokeStartLineCap = PenLineCap.Round;
			ConnectionPath.StrokeDashCap = PenLineCap.Round;
			ConnectionPath.Data = _connectionGeometry;
		}

		public void UpdateConnector()
		{
			if (EndPoint == null)
			{
				return;
			}
			_connectionGeometry.Figures.Clear();
			_symbolGeometry.Figures.Clear();
			switch(Orientation)
			{
				case ConnectorOrientation.Up:
					
					break;
				case ConnectorOrientation.Down:

					break;
				case ConnectorOrientation.Left:
					BuildLeftConnector();
					break;
				case ConnectorOrientation.Right:

					break;
			}
		}

		private void BuildLeftConnector()
		{
			var connectionPathEndPoint = new Point(EndPoint.X + ConnectionPathLength, EndPoint.Y);
			var connectionFigure = new PathFigure(new Point(EndPoint.X, EndPoint.Y),
				new[] { new LineSegment(connectionPathEndPoint, true) }, false);
			_connectionGeometry.Figures.Add(connectionFigure);

			var symbolEndPoint1 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y);
			var symbolLine1 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint1, true) }, false);

			_symbolGeometry.Figures.Add(symbolLine1);

			if(Cardinality == Cardinality.Many)
			{
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var symbolLine2 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint2, true) }, false);
				var symbolLine3 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint3, true) }, false);

				_symbolGeometry.Figures.Add(symbolLine2);
				_symbolGeometry.Figures.Add(symbolLine3);
			}

			if(Optionality == Optionality.Optional)
			{
				ConnectionPath.StrokeDashArray = DashArray;
				if(Cardinality == Cardinality.One)
				{
					Symbol.StrokeDashArray = DashArray;
				}
			}
		}
	}
}