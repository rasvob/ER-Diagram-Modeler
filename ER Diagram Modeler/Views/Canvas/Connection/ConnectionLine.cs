using System;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	/// <summary>
	/// Line in relationship visualization
	/// </summary>
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

		/// <summary>
		/// Is line selected
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				Line.Stroke = value ? _selectedColor : _unselectedColor;
			}
		}

		/// <summary>
		/// Line stroke thickness
		/// </summary>
		public static double StrokeThickness = 3;

		/// <summary>
		/// Line selected
		/// </summary>
		public event EventHandler LineSelected;

		/// <summary>
		/// Line move finished
		/// </summary>
		public event EventHandler LineMoved;

		/// <summary>
		/// Line moving
		/// </summary>
		public event EventHandler LineMoving;

		/// <summary>
		/// Line move not started yet
		/// </summary>
		public event EventHandler<ConnectionLineBeforeMoveEventArgs> BeforeLineMove;

		/// <summary>
		/// Line was splitted
		/// </summary>
		public event EventHandler<ConnectionPoint> LineSplit; 

		/// <summary>
		/// Orientation of line
		/// </summary>
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
			Line.StrokeThickness = StrokeThickness;
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

		/// <summary>
		/// Mouse button up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="mouseButtonEventArgs"></param>
		private void LineOnPreviewMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			OnLineSelected();
		}

		/// <summary>
		/// Mouse button down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Mouse moved
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// Mouse left button up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void LineOnMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
		{
			var snd = sender as Line;
			_dragStartPoint = null;
			snd?.ReleaseMouseCapture();
			OnLineMoved();
			OnLineSelected();
			args.Handled = true;
		}

		/// <summary>
		/// Mouse left button down
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
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

		/// <summary>
		/// End point coordinated changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void EndPointOnCoordinatesChanged(object sender, ConnectionPointEventArgs args)
		{
			Line.X2 = args.X;
			Line.Y2 = args.Y;
		}

		/// <summary>
		/// Start point coordinated changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void StartPointOnCoordinatesChanged(object sender, ConnectionPointEventArgs args)
		{
			Line.X1 = args.X;
			Line.Y1 = args.Y;
		}

		/// <summary>
		/// Get lenght of line
		/// </summary>
		/// <returns>Lenght of line</returns>
		public double GetLenght()
		{
			if (Orientation == LineOrientation.Horizontal)
			{
				return Math.Abs(EndPoint.X - StartPoint.X);
			}
			return Math.Abs(EndPoint.Y - StartPoint.Y);
		}

		/// <summary>
		/// Get coordinations of middle
		/// </summary>
		/// <returns>Middle point</returns>
		public ConnectionPoint GetMiddlePoint()
		{
			ConnectionPoint middle = new ConnectionPoint();
			if(Orientation == LineOrientation.Horizontal)
			{
				middle.Y = StartPoint.Y;
				var sum = StartPoint.X + EndPoint.X;
				middle.X = sum / 2.0;
			}
			else
			{
				middle.X = StartPoint.X;
				var sum = StartPoint.Y + EndPoint.Y;
				middle.Y = sum / 2.0;
			}
			return middle;
		}

		/// <summary>
		/// Start and end point coordinates
		/// </summary>
		/// <returns></returns>
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