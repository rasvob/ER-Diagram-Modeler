using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionInfo
	{
		public RelationshipModel RelationshipModel { get; set; }
		public ObservableCollection<ConnectionLine> Lines { get; } = new ObservableCollection<ConnectionLine>();
		public ObservableCollection<ConnectionPoint> Points { get; } = new ObservableCollection<ConnectionPoint>();
		public ObservableCollection<ConnectionPointMark> Marks { get; } = new ObservableCollection<ConnectionPointMark>();
		public Connector SourceConnector { get; } = new Connector();
		public Connector DestinationConnector { get; } = new Connector();

		private ConnectionLine _moveLine1 = null;
		private ConnectionLine _moveLine2 = null;
		private ConnectionPointMark _bendPoint1 = null;
		private ConnectionPointMark _bendPoint2 = null;
		private ConnectionPoint _sourcePoint = null;
		private ConnectionPoint _destinationPoint = null;
		private bool _isSelected;
		private readonly List<ConnectionPoint> _newBendPoints = new List<ConnectionPoint>();
		private TableViewModel _sourceViewModel;
		private TableViewModel _destinationViewModel;

		public event EventHandler<bool> SelectionChange;
		public event EventHandler<Connector> ConnectorChange;

		public TableViewModel SourceViewModel
		{
			get { return _sourceViewModel; }
			set
			{
				_sourceViewModel = value;
				SourceViewModel.PositionAndMeasureChanged += SourceViewModelOnPositionAndMeasureChanged;
				SourceViewModel.PositionAndMeasureChangesCompleted += OnPositionAndMeasureChangesCompleted;
			}
		}

		public TableViewModel DestinationViewModel
		{
			get { return _destinationViewModel; }
			set
			{
				_destinationViewModel = value;
				DestinationViewModel.PositionAndMeasureChanged += DestinationViewModelOnPositionAndMeasureChanged;
				DestinationViewModel.PositionAndMeasureChangesCompleted += OnPositionAndMeasureChangesCompleted;
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				foreach (ConnectionLine connectionLine in Lines)
				{
					connectionLine.IsSelected = value;
				}
				foreach (ConnectionPointMark mark in Marks)
				{
					mark.Mark.Visibility = value ? Visibility.Visible : Visibility.Hidden;
				}
				SourceConnector.IsSelected = value;
				DestinationConnector.IsSelected = value;
				OnSelectionChange(value);
			}
		}

		public ConnectionInfo()
		{
			Points.CollectionChanged += PointsOnCollectionChanged;
			SourceConnector.ConnectorSelected += OnConnectorSelected;
			DestinationConnector.ConnectorSelected += OnConnectorSelected;
		}

		private void OnConnectorSelected(object sender, System.EventArgs eventArgs)
		{
			IsSelected = true;
		}

		private void SourceViewModelOnPositionAndMeasureChanged(object sender, TablePositionAndMeasureEventArgs e)
		{
			switch (SourceConnector.Orientation)
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

		private void DestinationViewModelOnPositionAndMeasureChanged(object sender, TablePositionAndMeasureEventArgs e)
		{
			var line = Lines.LastOrDefault();
			var prevLineIdx = Lines.IndexOf(line) - 1;
			ConnectionLine prevLine = null;

			if (prevLineIdx >= 0)
			{
				prevLine = Lines[prevLineIdx];
			}

			if (line == null || prevLine == null)
			{
				return;
			}

			switch(DestinationConnector.Orientation)
			{
				case ConnectorOrientation.Up:
					break;
				case ConnectorOrientation.Down:
					break;
				case ConnectorOrientation.Left:
					line.EndPoint.Y += e.TopDelta;
					line.StartPoint.Y += e.TopDelta;
					prevLine.EndPoint.Y += e.TopDelta;

					line.EndPoint.X += e.LeftDelta;
					break;
				case ConnectorOrientation.Right:
					break;
			}
		}

		private void OnPositionAndMeasureChangesCompleted(object sender, System.EventArgs eventArgs)
		{
			SynchronizeBendingPoints();
		}

		private void PointsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (ConnectionPoint point in args.NewItems)
					{
						var mark = new ConnectionPointMark {Point = point};
						Marks.Add(mark);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (ConnectionPoint point in args.OldItems)
					{
						var item = Marks.FirstOrDefault(t => t.Point.Equals(point));
						Marks.Remove(item);
					}
					break;
			}
		}

		public void BuildLinesFromPoints()
		{
			if (Points.Count < 2)
			{
				return;
			}

			ClearLines();

			for (int i = 0; i < Points.Count - 1; i++)
			{
				ConnectionLine line = new ConnectionLine();

				line.StartPoint.X = Points[i].X;
				line.StartPoint.Y = Points[i].Y;

				line.EndPoint.X = Points[i + 1].X;
				line.EndPoint.Y = Points[i + 1].Y;

				if (line.StartPoint.X == line.EndPoint.X)
				{
					line.Orientation = LineOrientation.Vertical;
				}
				else
				{
					line.Orientation = LineOrientation.Horizontal;
				}

				ConfigureSplitLine(line);

				line.BeforeLineMove += LineOnBeforeLineMove;
				line.LineMoving += LineOnLineMoving;
				line.LineMoved += LineOnLineMoved;
				line.LineSplit += LineOnLineSplit;
				line.LineSelected += LineOnLineSelected;

				Lines.Add(line);
			}

			SourceConnector.EndPoint = Lines[0].StartPoint;
			DestinationConnector.EndPoint = Lines[Lines.Count - 1].EndPoint;
			SourceConnector.UpdateConnector();
			DestinationConnector.UpdateConnector();
		}

		private void LineOnLineSelected(object sender, System.EventArgs eventArgs)
		{
			IsSelected = true;
		}

		private void LineOnLineSplit(object sender, ConnectionPoint connectionPoint)
		{
			var line = sender as ConnectionLine;
			SplitLine(line, connectionPoint);
		}

		public void BuildPointsFromLines()
		{
			if (Lines.Count < 1)
			{
				return;
			}

			ClearPoints();
			Points.Add(Lines[0].StartPoint);

			foreach (ConnectionLine line in Lines)
			{
				Points.Add(line.EndPoint);
			}

			SourceConnector.EndPoint = Points[0];
			DestinationConnector.EndPoint = Points[Points.Count - 1];
			SourceConnector.UpdateConnector();
			DestinationConnector.UpdateConnector();
		}

		private void LineOnBeforeLineMove(object sender, ConnectionLineBeforeMoveEventArgs args)
		{
			var line = sender as ConnectionLine;
			var idx = Lines.IndexOf(line);
			var moveLine1Idx = idx - 1;
			var moveLine2Idx = idx + 1;

			if (moveLine1Idx >= 0)
			{
				_moveLine1 = Lines[moveLine1Idx];
			}
			else
			{
				_sourcePoint = Points[0];
			}

			if (moveLine2Idx < Lines.Count)
			{
				_moveLine2 = Lines[moveLine2Idx];
			}
			else
			{
				_destinationPoint = Points[Points.Count - 1];
			}

			_bendPoint1 = Marks.FirstOrDefault(t => t.Point.Equals(line?.StartPoint));
			_bendPoint2 = Marks.FirstOrDefault(t => t.Point.Equals(line?.EndPoint));

			//_moveLine1 = Lines.FirstOrDefault(t => t.EndPoint.Equals(args.OriginalStartPoint));
			//_moveLine2 = Lines.FirstOrDefault(t => t.StartPoint.Equals(args.OriginalEndPoint));
		}

		private void LineOnLineMoving(object sender, System.EventArgs args)
		{
			var line = sender as ConnectionLine;

			if (line == null)
			{
				return;
			}

			if (_moveLine1 != null)
			{
				_moveLine1.EndPoint.X = line.StartPoint.X;
				_moveLine1.EndPoint.Y = line.StartPoint.Y;
			}

			if (_moveLine2 != null)
			{
				_moveLine2.StartPoint.X = line.EndPoint.X;
				_moveLine2.StartPoint.Y = line.EndPoint.Y;
			}

			if (_bendPoint1 != null)
			{
				_bendPoint1.Point.X = line.StartPoint.X;
				_bendPoint1.Point.Y = line.StartPoint.Y;
			}

			if (_bendPoint2 != null)
			{
				_bendPoint2.Point.X = line.EndPoint.X;
				_bendPoint2.Point.Y = line.EndPoint.Y;
			}

			if (_sourcePoint != null)
			{
				_sourcePoint.X = line.StartPoint.X;
				_sourcePoint.Y = line.StartPoint.Y;
			}

			if (_destinationPoint != null)
			{
				_destinationPoint.X = line.EndPoint.X;
				_destinationPoint.Y = line.EndPoint.Y;
			}

			_newBendPoints.Clear();
		}

		private void LineOnLineMoved(object sender, System.EventArgs eventArgs)
		{
			var line = sender as ConnectionLine;

			if (_moveLine1 != null)
			{
				_moveLine1.EndPoint.X = line.StartPoint.X;
				_moveLine1.EndPoint.Y = line.StartPoint.Y;
			}

			if (_moveLine2 != null)
			{
				_moveLine2.StartPoint.X = line.EndPoint.X;
				_moveLine2.StartPoint.Y = line.EndPoint.Y;
			}

			if (_bendPoint1 != null)
			{
				_bendPoint1.Point.X = line.StartPoint.X;
				_bendPoint1.Point.Y = line.StartPoint.Y;
			}

			if (_bendPoint2 != null)
			{
				_bendPoint2.Point.X = line.EndPoint.X;
				_bendPoint2.Point.Y = line.EndPoint.Y;
			}

			if (_sourcePoint != null)
			{
				_sourcePoint.X = line.StartPoint.X;
				_sourcePoint.Y = line.StartPoint.Y;
			}

			if (_destinationPoint != null)
			{
				_destinationPoint.X = line.EndPoint.X;
				_destinationPoint.Y = line.EndPoint.Y;
			}

			_moveLine1 = null;
			_moveLine2 = null;
			_bendPoint1 = null;
			_bendPoint2 = null;
			_sourcePoint = null;
			_destinationPoint = null;

			SynchronizeBendingPoints();
		}

		private void ClearPoints()
		{
			var len = Points.Count;
			for (int i = 0; i < len; i++)
			{
				Points.RemoveAt(0);
			}
			Points.Clear();
		}

		private void ClearLines()
		{
			int len = Lines.Count;
			for (int i = 0; i < len; i++)
			{
				Lines.RemoveAt(0);
			}
			Lines.Clear();
		}

		private void RemoveRedundandBendPoints()
		{
			var duplicate = Points.Skip(1).Take(Points.Count - 2).GroupBy(t => t).Where(s => s.Count() > 1).Select(u => u.Key);
			var forRemove = Points.Where(t => duplicate.Any(s => s.Equals(t))).Where(s => !_newBendPoints.Any(r => r.Equals(s))).ToList();

			//if (forRemove.Contains(Points[0]))
			//{
			//	var item = forRemove.FirstOrDefault(t => t.Equals(Points[0]));
			//	forRemove.Remove(item);
			//}

			//if (forRemove.Contains(Points[Points.Count - 1]))
			//{
			//	var item = forRemove.FirstOrDefault(t => t.Equals(Points[Points.Count - 1]));
			//	forRemove.Remove(item);
			//}

			foreach (ConnectionPoint connectionPoint in forRemove)
			{
				Points.Remove(connectionPoint);
			}
		}

		public void SynchronizeBendingPoints()
		{
			BuildPointsFromLines();
			RemoveRedundandBendPoints();
			BuildLinesFromPoints();
			AdjustBendPointMarks();
		}

		private void AdjustBendPointMarks()
		{
			Marks.RemoveAt(0);
			Marks.RemoveAt(Marks.Count - 1);
		}

		private void SplitLine(ConnectionLine line, ConnectionPoint point)
		{
			var startPoint = Points.FirstOrDefault(t => t.Equals(line.StartPoint));
			var idx = Points.IndexOf(startPoint);

			if (!Points.Contains(point))
			{
				Points.Insert(idx + 1, point);
				Points.Insert(idx + 1, point);
			}

			_newBendPoints.Add(point);

			BuildLinesFromPoints();
		}

		private void ConfigureSplitLine(ConnectionLine line)
		{
			if (line.StartPoint.Equals(line.EndPoint))
			{
				if (Lines.Count > 0)
				{
					var previous = Lines[Lines.Count - 1];
					line.Orientation = previous.Orientation == LineOrientation.Vertical ? LineOrientation.Horizontal : LineOrientation.Vertical;
				}
			}
		}

		protected virtual void OnSelectionChange(bool e)
		{
			SelectionChange?.Invoke(this, e);
		}

		protected virtual void OnConnectorChange(Connector e)
		{
			ConnectorChange?.Invoke(this, e);
		}
	}
}