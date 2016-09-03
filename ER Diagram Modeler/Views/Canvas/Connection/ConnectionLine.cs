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
		private SolidColorBrush _unselectedColor = Application.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
		private SolidColorBrush _selectedColor = Application.Current.FindResource("PrimaryColorDarkBrush") as SolidColorBrush;

		private LineOrientation _orientation;
		private bool _isSelected;
		public ConnectionPoint StartPoint { get; }
		public ConnectionPoint EndPoint { get; }
		public Line Line { get; }

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				Line.Stroke = value ? _selectedColor : _unselectedColor;
			}
		}

		public event EventHandler LineSelected;
		public event EventHandler LineMoved;
		public event EventHandler LineMoving;
		public event EventHandler<ConnectionLineBeforeMoveEventArgs> BeforeLineMove;
		public event EventHandler<ConnectionPoint> LineSplit; 

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

			Line.Stroke = _unselectedColor;
			Line.StrokeThickness = 4;
			Line.SnapsToDevicePixels = true;
			Line.StrokeEndLineCap = PenLineCap.Round;
			Line.StrokeStartLineCap = PenLineCap.Round;

			StartPoint.CoordinatesChanged += StartPointOnCoordinatesChanged;
			EndPoint.CoordinatesChanged += EndPointOnCoordinatesChanged;

			Line.MouseLeftButtonUp += LineOnMouseLeftButtonUp;
			Line.MouseLeftButtonDown += LineOnMouseLeftButtonDown;
			Line.MouseMove += LineOnMouseMove;
			Line.PreviewMouseDown += LineOnPreviewMouseDown;
			Line.PreviewMouseUp += LineOnPreviewMouseUp;
		}

		private void LineOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			OnLineSelected();
		}

		private void LineOnPreviewMouseDown(object sender, MouseButtonEventArgs args)
		{
			OnLineSelected();
			if (args.MiddleButton == MouseButtonState.Pressed)
			{
				Line line = sender as Line;
				var point = args.GetPosition(line);
				var splitPoint = new ConnectionPoint();

				if (Orientation == LineOrientation.Horizontal)
				{
					splitPoint.X = (int) point.X;
					splitPoint.Y = StartPoint.Y;
				}
				else
				{
					splitPoint.X = StartPoint.X;
					splitPoint.Y = (int) point.Y;
				}

				OnLineSplit(splitPoint);

				args.Handled = true;
			}
		}

		private void LineOnMouseMove(object sender, MouseEventArgs args)
		{
			if (_dragStartPoint.HasValue)
			{
				Vector offset = args.GetPosition(sender as Line) - _dragStartPoint.Value;

				if (Orientation == LineOrientation.Vertical)
				{
					StartPoint.X = _dragStartPointStart.X + (int)offset.X;
					EndPoint.X = _dragStartPointEnd.X + (int)offset.X;
				}
				else
				{
					StartPoint.Y = _dragStartPointStart.Y + (int)offset.Y;
					EndPoint.Y = _dragStartPointEnd.Y + (int)offset.Y;
				}

				OnLineMoving();
			}
		}

		private void LineOnMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
		{
			var snd = sender as Line;
			_dragStartPoint = null;
			snd?.ReleaseMouseCapture();
			OnLineMoved();
			OnLineSelected();
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
			OnBeforeLineMove(new ConnectionLineBeforeMoveEventArgs()
			{
				OriginalEndPoint = _dragStartPointEnd,
				OriginalStartPoint = _dragStartPointStart
			});

			OnLineSelected();
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

		protected virtual void OnLineMoving()
		{
			LineMoving?.Invoke(this, System.EventArgs.Empty);
		}

		protected virtual void OnBeforeLineMove(ConnectionLineBeforeMoveEventArgs e)
		{
			BeforeLineMove?.Invoke(this, e);
		}

		protected virtual void OnLineSplit(ConnectionPoint e)
		{
			LineSplit?.Invoke(this, e);
		}

		protected virtual void OnLineSelected()
		{
			LineSelected?.Invoke(this, System.EventArgs.Empty);
		}
	}
}