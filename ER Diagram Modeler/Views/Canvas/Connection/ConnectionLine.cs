using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionLine: Control
	{
		private ConnectionPoint _dragStartPointStart = new ConnectionPoint();
		private ConnectionPoint _dragStartPointEnd = new ConnectionPoint();
		private Point? _dragStartPoint;

		private LineOrientation _orientation;
		public ConnectionPoint StartPoint { get; }
		public ConnectionPoint EndPoint { get; }
		public Line Line { get; }

		public event EventHandler LineMoved;
		public event EventHandler<ConnectionLineMovingEventArgs> LineMoving;
		public event EventHandler<ConnectionLineMovingEventArgs> BeforeLineMove; 

		public LineOrientation Orientation
		{
			get { return _orientation; }
			set
			{
				_orientation = value;
				if (_orientation == LineOrientation.Horizontal)
				{
					Line.Cursor = Cursors.SizeNS;
				}
				else
				{
					Line.Cursor = Cursors.SizeWE;
				}
			}
		}

		public ConnectionLine()
		{
			Line = new Line();
			StartPoint = new ConnectionPoint();
			EndPoint = new ConnectionPoint();

			Line.Stroke = Application.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
			Line.StrokeThickness = 4;
			Line.SnapsToDevicePixels = true;

			StartPoint.CoordinatesChanged += StartPointOnCoordinatesChanged;
			EndPoint.CoordinatesChanged += EndPointOnCoordinatesChanged;

			Line.MouseLeftButtonUp += LineOnMouseLeftButtonUp;
			Line.MouseLeftButtonDown += LineOnMouseLeftButtonDown;
			Line.MouseMove += LineOnMouseMove;
		}

		private void LineOnMouseMove(object sender, MouseEventArgs args)
		{
			if (_dragStartPoint.HasValue)
			{
				Vector offset = args.GetPosition(sender as Line) - _dragStartPoint.Value;

				if (Orientation == LineOrientation.Vertical)
				{
					StartPoint.X = _dragStartPointStart.X + offset.X;
					EndPoint.X = _dragStartPointEnd.X + offset.X;
				}
				else
				{
					StartPoint.Y = _dragStartPointStart.Y + offset.Y;
					EndPoint.Y = _dragStartPointEnd.Y + offset.Y;
				}

				var evArgs = new ConnectionLineMovingEventArgs()
				{
					Offset = offset
				};

				OnLineMoving(evArgs);
			}
		}

		private void LineOnMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
		{
			var snd = sender as Line;
			_dragStartPoint = null;
			snd?.ReleaseMouseCapture();
			OnLineMoved();
			args.Handled = true;
		}

		private void LineOnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
		{
			var snd = sender as Line;

			_dragStartPoint = args.GetPosition(snd);
			_dragStartPointStart.X = StartPoint.X;
			_dragStartPointStart.Y = StartPoint.Y;
			_dragStartPointEnd.X = EndPoint.X;
			_dragStartPointEnd.Y = EndPoint.Y;

			snd?.CaptureMouse();

			var evArgs = new ConnectionLineMovingEventArgs()
			{
				OriginalStartPoint = _dragStartPointStart,
				OriginalEndPoint = _dragStartPointEnd
			};
			OnBeforeLineMove(evArgs);

			args.Handled = true;
		}

		private void EndPointOnCoordinatesChanged(object sender, ConnectionPointEventArgs args)
		{
			Line.X2 = args.X;
			Line.Y2 = args.Y;
		}

		private void StartPointOnCoordinatesChanged(object sender, ConnectionPointEventArgs args)
		{
			Line.X1 = args.X;
			Line.Y1 = args.Y;
		}

		public Point GetMiddle()
		{
			Point res = new Point()
			{
				X = (StartPoint.X + EndPoint.X)/2,
				Y = (EndPoint.X + EndPoint.X)/2
			};

			return res;
		}

		public override string ToString()
		{
			return $"{StartPoint} | {EndPoint}";
		}

		protected virtual void OnLineMoved()
		{
			LineMoved?.Invoke(this, System.EventArgs.Empty);
		}

		protected virtual void OnLineMoving(ConnectionLineMovingEventArgs e)
		{
			LineMoving?.Invoke(this, e);
		}

		protected virtual void OnBeforeLineMove(ConnectionLineMovingEventArgs e)
		{
			BeforeLineMove?.Invoke(this, e);
		}
	}
}