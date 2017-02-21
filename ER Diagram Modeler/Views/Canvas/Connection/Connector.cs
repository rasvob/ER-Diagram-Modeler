using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using Path = System.Windows.Shapes.Path;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	/// <summary>
	/// Connector visual element
	/// </summary>
	public class Connector
	{
		/// <summary>
		/// Symbol One or Many
		/// </summary>
		public Path Symbol { get; }

		/// <summary>
		/// Short line at end of connector
		/// </summary>
		public Path ConnectionPath { get; set; }

		/// <summary>
		/// Optionality of relationship
		/// </summary>
		public Optionality Optionality { get; set; } = Optionality.Mandatory;

		/// <summary>
		/// Cadrinality of relationship
		/// </summary>
		public Cardinality Cardinality { get; set; }

		/// <summary>
		/// Is connector selected
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				Symbol.Fill = value ? _selectedColor : _unselectedColor;
				Symbol.Stroke = value ? _selectedColor : _unselectedColor;
				ConnectionPath.Stroke = value ? _selectedColor : _unselectedColor;
			}
		}

		/// <summary>
		/// End point
		/// </summary>
		public ConnectionPoint EndPoint
		{
			get { return _endPoint; }
			set
			{
				_endPoint = value;
				EndPoint.CoordinatesChanged += EndPointOnCoordinatesChanged;
			}
		}

		/// <summary>
		/// End point X or Y changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="connectionPointEventArgs"></param>
		private void EndPointOnCoordinatesChanged(object sender, ConnectionPointEventArgs connectionPointEventArgs)
		{
			UpdateConnector();
		}

		/// <summary>
		/// Orientation of symbol
		/// </summary>
		public ConnectorOrientation Orientation
		{
			get { return _orientation; }
			set
			{
				_orientation = value;
				UpdateConnector();
			}
		}

		/// <summary>
		/// Lenght of path
		/// </summary>
		public static readonly double ConnectionPathLength = 25;

		/// <summary>
		/// Line of lines of symbol
		/// </summary>
		public static readonly double SymbolLinesLength = 20;

		/// <summary>
		/// Difference between ends of lines
		/// </summary>
		public static readonly double SymbolLineEndsDiff = 8;

		/// <summary>
		/// Connctor lenght including line for connection
		/// </summary>
		public static readonly double ConnectorLenght = SymbolLinesLength + ConnectionPathLength;

		public static readonly DoubleCollection DashArray = new DoubleCollection() { 2 };

		private ConnectorOrientation _orientation;
		private readonly SolidColorBrush _unselectedColor = Application.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
		private readonly SolidColorBrush _selectedColor = Application.Current.FindResource("PrimaryColorDarkBrush") as SolidColorBrush;
		private ConnectionPoint _endPoint;
		private readonly PathGeometry _symbolGeometry;
		private readonly PathGeometry _connectionGeometry;
		private bool _isSelected;

		/// <summary>
		/// Connector selected
		/// </summary>
		public event EventHandler ConnectorSelected;

		public Connector()
		{
			Symbol = new Path();
			ConnectionPath = new Path();
			_symbolGeometry = new PathGeometry();
			_connectionGeometry = new PathGeometry();

			Symbol.SnapsToDevicePixels = true;
			Symbol.Stroke = _unselectedColor;
			Symbol.StrokeThickness = ConnectionLine.StrokeThickness;
			Symbol.Fill = _unselectedColor;
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

			Symbol.PreviewMouseLeftButtonDown += SymbolOnPreviewMouseLeftButtonDown;
			ConnectionPath.PreviewMouseLeftButtonDown += SymbolOnPreviewMouseLeftButtonDown;
		}

		/// <summary>
		/// Left mouse button down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SymbolOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
		{
			OnConnectorSelected();
			args.Handled = true;
		}

		/// <summary>
		/// Update connector by orientation
		/// </summary>
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
					BuildTopConnector();
					break;
				case ConnectorOrientation.Down:
					BuildBottomConnector();
					break;
				case ConnectorOrientation.Left:
					BuildLeftConnector();
					break;
				case ConnectorOrientation.Right:
					BuildRightConnector();
					break;
			}
		}

		/// <summary>
		/// Build paths for visualization
		/// </summary>
		private void BuildBottomConnector()
		{
			var connectionPathEndPoint = new Point(EndPoint.X, EndPoint.Y - ConnectionPathLength);
			var connectionFigure = new PathFigure(new Point(EndPoint.X, EndPoint.Y),
				new[] { new LineSegment(connectionPathEndPoint, true) }, false);
			_connectionGeometry.Figures.Add(connectionFigure);

			if(Cardinality == Cardinality.One)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X - SymbolLineEndsDiff, connectionPathEndPoint.Y + ConnectionLine.StrokeThickness / 2);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X, connectionPathEndPoint.Y - SymbolLinesLength);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X + SymbolLineEndsDiff, connectionPathEndPoint.Y + ConnectionLine.StrokeThickness / 2);

				var shape = new PathFigure(connectionPathEndPoint, new[]
				{
					new PolyLineSegment(new List<Point>()
					{
						symbolEndPoint1, symbolEndPoint2, symbolEndPoint3
					}, true),
				}, true);

				_symbolGeometry.Figures.Add(shape);
			}
			else if(Cardinality == Cardinality.Many)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X, connectionPathEndPoint.Y - SymbolLinesLength);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X + SymbolLineEndsDiff, connectionPathEndPoint.Y - SymbolLinesLength);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X - SymbolLineEndsDiff, connectionPathEndPoint.Y - SymbolLinesLength);

				var symbolLine1 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint1, true) }, false);
				var symbolLine2 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint2, true) }, false);
				var symbolLine3 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint3, true) }, false);

				var symbolEndPoint4 = new Point(connectionPathEndPoint.X - SymbolLineEndsDiff, connectionPathEndPoint.Y + ConnectionLine.StrokeThickness / 2);
				var symbolEndPoint5 = new Point(connectionPathEndPoint.X + SymbolLineEndsDiff, connectionPathEndPoint.Y + ConnectionLine.StrokeThickness / 2);

				var symbolLine4 = new PathFigure(symbolEndPoint4, new[] { new LineSegment(symbolEndPoint5, true) }, false);

				_symbolGeometry.Figures.Add(symbolLine1);
				_symbolGeometry.Figures.Add(symbolLine2);
				_symbolGeometry.Figures.Add(symbolLine3);
				_symbolGeometry.Figures.Add(symbolLine4);
			}

			if(Optionality == Optionality.Optional)
			{
				ConnectionPath.StrokeDashArray = DashArray;
			}
		}

		/// <summary>
		/// Build paths for visualization
		/// </summary>
		private void BuildTopConnector()
		{
			var connectionPathEndPoint = new Point(EndPoint.X, EndPoint.Y + ConnectionPathLength);
			var connectionFigure = new PathFigure(new Point(EndPoint.X, EndPoint.Y),
				new[] { new LineSegment(connectionPathEndPoint, true) }, false);
			_connectionGeometry.Figures.Add(connectionFigure);

			if(Cardinality == Cardinality.One)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X - SymbolLineEndsDiff, connectionPathEndPoint.Y - ConnectionLine.StrokeThickness / 2);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X, connectionPathEndPoint.Y + SymbolLinesLength);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X + SymbolLineEndsDiff, connectionPathEndPoint.Y - ConnectionLine.StrokeThickness / 2);

				var shape = new PathFigure(connectionPathEndPoint, new[]
				{
					new PolyLineSegment(new List<Point>()
					{
						symbolEndPoint1, symbolEndPoint2, symbolEndPoint3
					}, true),
				}, true);

				_symbolGeometry.Figures.Add(shape);
			}
			else if(Cardinality == Cardinality.Many)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X, connectionPathEndPoint.Y + SymbolLinesLength);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X + SymbolLineEndsDiff, connectionPathEndPoint.Y + SymbolLinesLength);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X - SymbolLineEndsDiff, connectionPathEndPoint.Y + SymbolLinesLength);

				var symbolLine1 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint1, true) }, false);
				var symbolLine2 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint2, true) }, false);
				var symbolLine3 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint3, true) }, false);

				var symbolEndPoint4 = new Point(connectionPathEndPoint.X - SymbolLineEndsDiff, connectionPathEndPoint.Y - ConnectionLine.StrokeThickness / 2);
				var symbolEndPoint5 = new Point(connectionPathEndPoint.X + SymbolLineEndsDiff, connectionPathEndPoint.Y - ConnectionLine.StrokeThickness / 2);

				var symbolLine4 = new PathFigure(symbolEndPoint4, new[] { new LineSegment(symbolEndPoint5, true) }, false);

				_symbolGeometry.Figures.Add(symbolLine1);
				_symbolGeometry.Figures.Add(symbolLine2);
				_symbolGeometry.Figures.Add(symbolLine3);
				_symbolGeometry.Figures.Add(symbolLine4);
			}

			if(Optionality == Optionality.Optional)
			{
				ConnectionPath.StrokeDashArray = DashArray;
			}
		}

		/// <summary>
		/// Build paths for visualization
		/// </summary>
		private void BuildLeftConnector()
		{
			var connectionPathEndPoint = new Point(EndPoint.X + ConnectionPathLength, EndPoint.Y);
			var connectionFigure = new PathFigure(new Point(EndPoint.X, EndPoint.Y),
				new[] { new LineSegment(connectionPathEndPoint, true) }, false);
			_connectionGeometry.Figures.Add(connectionFigure);

			if (Cardinality == Cardinality.One)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X - ConnectionLine.StrokeThickness / 2, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X - ConnectionLine.StrokeThickness / 2, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var shape = new PathFigure(connectionPathEndPoint, new []
				{
					new PolyLineSegment(new List<Point>()
					{
						symbolEndPoint1, symbolEndPoint2, symbolEndPoint3
					}, true), 
				}, true);

				_symbolGeometry.Figures.Add(shape);
			}
			else if(Cardinality == Cardinality.Many)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X + SymbolLinesLength, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var symbolLine1 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint1, true) }, false);
				var symbolLine2 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint2, true) }, false);
				var symbolLine3 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint3, true) }, false);

				var symbolEndPoint4 = new Point(connectionPathEndPoint.X - ConnectionLine.StrokeThickness / 2, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint5 = new Point(connectionPathEndPoint.X - ConnectionLine.StrokeThickness / 2, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var symbolLine4 = new PathFigure(symbolEndPoint4, new[] { new LineSegment(symbolEndPoint5, true) }, false);

				_symbolGeometry.Figures.Add(symbolLine1);
				_symbolGeometry.Figures.Add(symbolLine2);
				_symbolGeometry.Figures.Add(symbolLine3);
				_symbolGeometry.Figures.Add(symbolLine4);
			}

			if(Optionality == Optionality.Optional)
			{
				ConnectionPath.StrokeDashArray = DashArray;
			}
		}

		/// <summary>
		/// Build paths for visualization
		/// </summary>
		private void BuildRightConnector()
		{
			var connectionPathEndPoint = new Point(EndPoint.X - ConnectionPathLength, EndPoint.Y);
			var connectionFigure = new PathFigure(new Point(EndPoint.X, EndPoint.Y),
				new[] { new LineSegment(connectionPathEndPoint, true) }, false);
			_connectionGeometry.Figures.Add(connectionFigure);

			if(Cardinality == Cardinality.One)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X + ConnectionLine.StrokeThickness, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X - SymbolLinesLength, connectionPathEndPoint.Y);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X + ConnectionLine.StrokeThickness, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var shape = new PathFigure(connectionPathEndPoint, new[]
				{
					new PolyLineSegment(new List<Point>()
					{
						symbolEndPoint1, symbolEndPoint2, symbolEndPoint3
					}, true),
				}, true);

				_symbolGeometry.Figures.Add(shape);
			}
			else if(Cardinality == Cardinality.Many)
			{
				var symbolEndPoint1 = new Point(connectionPathEndPoint.X - SymbolLinesLength, connectionPathEndPoint.Y);
				var symbolEndPoint2 = new Point(connectionPathEndPoint.X - SymbolLinesLength, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint3 = new Point(connectionPathEndPoint.X - SymbolLinesLength, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var symbolLine1 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint1, true) }, false);
				var symbolLine2 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint2, true) }, false);
				var symbolLine3 = new PathFigure(connectionPathEndPoint, new[] { new LineSegment(symbolEndPoint3, true) }, false);

				var symbolEndPoint4 = new Point(connectionPathEndPoint.X + ConnectionLine.StrokeThickness / 2, connectionPathEndPoint.Y + SymbolLineEndsDiff);
				var symbolEndPoint5 = new Point(connectionPathEndPoint.X + ConnectionLine.StrokeThickness / 2, connectionPathEndPoint.Y - SymbolLineEndsDiff);

				var symbolLine4 = new PathFigure(symbolEndPoint4, new[] { new LineSegment(symbolEndPoint5, true) }, false);

				_symbolGeometry.Figures.Add(symbolLine1);
				_symbolGeometry.Figures.Add(symbolLine2);
				_symbolGeometry.Figures.Add(symbolLine3);
				_symbolGeometry.Figures.Add(symbolLine4);
			}

			if(Optionality == Optionality.Optional)
			{
				ConnectionPath.StrokeDashArray = DashArray;
			}
		}

		protected virtual void OnConnectorSelected()
		{
			ConnectorSelected?.Invoke(this, System.EventArgs.Empty);
		}
	}
}