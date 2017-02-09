using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Extintions;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Canvas.Connection;
using Pathfinding;
using Pathfinding.Structure;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Point = System.Drawing.Point;

namespace ER_Diagram_Modeler.ViewModels
{
	public class ConnectionInfoViewModel
	{
		public RelationshipModel RelationshipModel { get; set; } = new RelationshipModel();
		public ObservableCollection<ConnectionLine> Lines { get; } = new ObservableCollection<ConnectionLine>();
		public ObservableCollection<ConnectionPoint> Points { get; } = new ObservableCollection<ConnectionPoint>();
		public ObservableCollection<ConnectionPointMark> Marks { get; } = new ObservableCollection<ConnectionPointMark>();
		public Connector SourceConnector { get; } = new Connector();
		public Connector DestinationConnector { get; } = new Connector();
		public bool? IsSourceConnectorAtStartPoint { get; set; }

		public DesignerCanvas DesignerCanvas
		{
			get { return _canvas; }
			set { _canvas = value; }
		}

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
		private DesignerCanvas _canvas;

		public event EventHandler<bool> SelectionChange;
		public event EventHandler<Connector> ConnectorChange;

		public static readonly double PointsDistaceTolerance = 8.0;
		public static readonly double DefaultConnectionLineLength = 80;
		public static readonly int GraphTransformStep = 60;

		public TableViewModel SourceViewModel
		{
			get { return _sourceViewModel; }
			set
			{
				if (_sourceViewModel != null)
				{
					_sourceViewModel.PositionAndMeasureChanged -= SourceViewModelOnPositionAndMeasureChanged;
					_sourceViewModel.PositionAndMeasureChangesCompleted -= SourceViewModelOnPositionAndMeasureChangesCompleted;
					_sourceViewModel.PositionAndMeasureChangesStarted -= SourceViewModelOnPositionAndMeasureChangesStarted;
					_sourceViewModel.TableViewModeChanged -= ViewModelOnTableViewModeChanged;
				}
				_sourceViewModel = value;
				if (value == null) return;
				SourceViewModel.PositionAndMeasureChanged += SourceViewModelOnPositionAndMeasureChanged;
				SourceViewModel.PositionAndMeasureChangesCompleted += SourceViewModelOnPositionAndMeasureChangesCompleted;
				SourceViewModel.PositionAndMeasureChangesStarted += SourceViewModelOnPositionAndMeasureChangesStarted;
				SourceViewModel.TableViewModeChanged += ViewModelOnTableViewModeChanged;
			}
		}

		public TableViewModel DestinationViewModel
		{
			get { return _destinationViewModel; }
			set
			{
				if (_destinationViewModel != null)
				{
					_destinationViewModel.PositionAndMeasureChanged -= DestinationViewModelOnPositionAndMeasureChanged;
					_destinationViewModel.PositionAndMeasureChangesCompleted -= DestinationViewModelOnPositionAndMeasureChangesCompleted;
					_destinationViewModel.PositionAndMeasureChangesStarted -= DestinationViewModelOnPositionAndMeasureChangesStarted;
					_destinationViewModel.TableViewModeChanged -= ViewModelOnTableViewModeChanged;
				}
				_destinationViewModel = value;
				if (value == null) return;
				DestinationViewModel.PositionAndMeasureChanged += DestinationViewModelOnPositionAndMeasureChanged;
				DestinationViewModel.PositionAndMeasureChangesCompleted += DestinationViewModelOnPositionAndMeasureChangesCompleted;
				DestinationViewModel.PositionAndMeasureChangesStarted += DestinationViewModelOnPositionAndMeasureChangesStarted;
				DestinationViewModel.TableViewModeChanged += ViewModelOnTableViewModeChanged;
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

		public ConnectionInfoViewModel()
		{
			Points.CollectionChanged += PointsOnCollectionChanged;
			SourceConnector.ConnectorSelected += OnConnectorSelected;
			DestinationConnector.ConnectorSelected += OnConnectorSelected;
		}

		private void OnConnectorSelected(object sender, System.EventArgs eventArgs)
		{
			IsSelected = true;
		}


		private void ViewModelOnTableViewModeChanged(object sender, System.EventArgs eventArgs)
		{
			var table = sender as TableViewModel;

			if (table?.ViewMode == TableViewMode.NameOnly)
			{
				if (IsSelfConnection())
				{
					MoveConnectorOnTableViewModeChange(SourceConnector, SourceViewModel);
					MoveConnectorOnTableViewModeChange(DestinationConnector, DestinationViewModel);
				}
				else if (table.Equals(SourceViewModel))
				{
					MoveConnectorOnTableViewModeChange(SourceConnector, table);
				}
				else if (table.Equals(DestinationViewModel))
				{
					MoveConnectorOnTableViewModeChange(DestinationConnector, table);
				}
			}
		}

		private void SourceViewModelOnPositionAndMeasureChanged(object sender, TablePositionAndMeasureEventArgs e)
		{
			var table = sender as TableViewModel;

			if(IsSourceAndDestinationSelected() && !IsSelfConnection())
			{
				if (SourceViewModel.IsMoving)
				{
					MoveAllLines(e);
					return;
				}
				if (DestinationViewModel.IsMoving)
				{
					return;
				}
			}

			if (table != null && IsSelfConnection() && (table.IsMoving || IsAnySelectedTableMoving()))
			{
				return;
			}

			if (Lines.Count == 1 || Lines.Count == 0)
			{
				return;
			}

			switch (SourceConnector.Orientation)
			{
				case ConnectorOrientation.Up:
				{
						ConnectionLine endLine = null;
						if(SourceConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.Y += e.TopDelta;

							AdjustConnectorMarkToHorizontalLimits(SourceConnector, table, endLine, nextLine, e, true);
						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;

							AdjustConnectorMarkToHorizontalLimits(SourceConnector, table, endLine, nextLine, e, false);
						}
						break;
				}

				case ConnectorOrientation.Down:
				{
						ConnectionLine endLine = null;
						if(SourceConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.Y += e.TopDelta;
							endLine.StartPoint.Y += e.HeightDelta;

							AdjustConnectorMarkToHorizontalLimits(SourceConnector, table, endLine, nextLine, e, true);
						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;
							endLine.EndPoint.Y += e.HeightDelta;

							AdjustConnectorMarkToHorizontalLimits(SourceConnector, table, endLine, nextLine, e, false);
						}
						break;
				}
				case ConnectorOrientation.Left:
				{
						ConnectionLine endLine = null;
						if(SourceConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.X += e.LeftDelta;

							AdjustConnectorMarkToVerticalLimits(SourceConnector, table, endLine, nextLine, e, true);
						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;

							AdjustConnectorMarkToVerticalLimits(SourceConnector, table, endLine, nextLine, e, false);
						}
						break;
				}
				case ConnectorOrientation.Right:
				{
						ConnectionLine endLine = null;
						if(SourceConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.X += e.LeftDelta;
							endLine.StartPoint.X += e.WidthDelta;

							AdjustConnectorMarkToVerticalLimits(SourceConnector, table, endLine, nextLine, e, true);
						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;
							endLine.EndPoint.X += e.WidthDelta;

							AdjustConnectorMarkToVerticalLimits(SourceConnector, table, endLine, nextLine, e, false);
						}
						break;
				}
			}
		}
		private void DestinationViewModelOnPositionAndMeasureChanged(object sender, TablePositionAndMeasureEventArgs e)
		{
			var table = DestinationViewModel;

			if(IsSourceAndDestinationSelected() && !IsSelfConnection())
			{
				if(DestinationViewModel.IsMoving)
				{
					MoveAllLines(e);
					return;
				}
				if(SourceViewModel.IsMoving)
				{
					return;
				}
			}

			if(table != null && IsSelfConnection() && (table.IsMoving || IsAnySelectedTableMoving()))
			{
				MoveAllLines(e);
				return;
			}

			if(Lines.Count == 1 || Lines.Count == 0)
			{
				return;
			}

			if (DestinationConnector.EndPoint == null)
			{
				return;
			}

			switch(DestinationConnector.Orientation)
			{
				case ConnectorOrientation.Up:
				{
						ConnectionLine endLine = null;
						if(DestinationConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.Y += e.TopDelta;

							AdjustConnectorMarkToHorizontalLimits(DestinationConnector, table, endLine, nextLine, e, true);
						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;

							AdjustConnectorMarkToHorizontalLimits(DestinationConnector, table, endLine, nextLine, e, false);
						}
						break;
				}
				case ConnectorOrientation.Down:
				{
						ConnectionLine endLine = null;
						if(DestinationConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.Y += e.TopDelta;
							endLine.StartPoint.Y += e.HeightDelta;

							AdjustConnectorMarkToHorizontalLimits(DestinationConnector, table, endLine, nextLine, e, true);
						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;
							endLine.EndPoint.Y += e.HeightDelta;

							AdjustConnectorMarkToHorizontalLimits(DestinationConnector, table, endLine, nextLine, e, false);
						}
						break;
				}
				case ConnectorOrientation.Left:
				{
						ConnectionLine endLine = null;
						if(DestinationConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.X += e.LeftDelta;

							AdjustConnectorMarkToVerticalLimits(DestinationConnector, table, endLine, nextLine, e, true);
						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;

							AdjustConnectorMarkToVerticalLimits(DestinationConnector, table, endLine, nextLine, e, false);
						}
						break;
					}
				case ConnectorOrientation.Right:
					{
						ConnectionLine endLine = null;
						if (DestinationConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint))
						{
							endLine = Lines.FirstOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) + 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.StartPoint.X += e.LeftDelta;
							endLine.StartPoint.X += e.WidthDelta;

							AdjustConnectorMarkToVerticalLimits(DestinationConnector, table, endLine, nextLine, e, true);
						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;
							endLine.EndPoint.X += e.WidthDelta;

							AdjustConnectorMarkToVerticalLimits(DestinationConnector, table, endLine, nextLine, e, false);
						}
						break;
					}
			}
		}

		private void SourceViewModelOnPositionAndMeasureChangesStarted(object sender, System.EventArgs eventArgs)
		{
			SplitLinesIfNeeded();
		}

		private void SourceViewModelOnPositionAndMeasureChangesCompleted(object sender, System.EventArgs eventArgs)
		{
			_newBendPoints.Clear();
			SynchronizeBendingPoints();
		}

		private void DestinationViewModelOnPositionAndMeasureChangesStarted(object sender, System.EventArgs eventArgs)
		{
			SplitLinesIfNeeded();
		}

		private void DestinationViewModelOnPositionAndMeasureChangesCompleted(object sender, System.EventArgs eventArgs)
		{
			_newBendPoints.Clear();
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

			var firstLine = Lines.FirstOrDefault();
			var lastLine = Lines.LastOrDefault();

			if(IsSourceConnectorAtStartPoint.HasValue)
			{
				if(IsSourceConnectorAtStartPoint.Value)
				{
					SourceConnector.EndPoint = firstLine?.StartPoint;
					DestinationConnector.EndPoint = lastLine?.EndPoint;
				}
				else
				{
					SourceConnector.EndPoint = lastLine?.EndPoint;
					DestinationConnector.EndPoint = firstLine?.StartPoint;
				}
				SourceConnector.UpdateConnector();
				DestinationConnector.UpdateConnector();
				return;
			}

			if (firstLine != null && firstLine.StartPoint.Equals(SourceConnector.EndPoint))
			{
				SourceConnector.EndPoint = firstLine.StartPoint;
			}
			else if (firstLine != null && firstLine.StartPoint.Equals(DestinationConnector.EndPoint))
			{
				DestinationConnector.EndPoint = firstLine.StartPoint;
			}

			if (lastLine != null && lastLine.EndPoint.Equals(SourceConnector.EndPoint))
			{
				SourceConnector.EndPoint = lastLine.EndPoint;
			}
			else if(lastLine != null && lastLine.EndPoint.Equals(DestinationConnector.EndPoint))
			{
				DestinationConnector.EndPoint = lastLine.EndPoint;
			}

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
		}

		private void LineOnBeforeLineMove(object sender, ConnectionLineBeforeMoveEventArgs args)
		{
			var line = sender as ConnectionLine;
			var idx = Lines.IndexOf(line);
			var moveLine1Idx = idx - 1;
			var moveLine2Idx = idx + 1;

			_canvas = VisualTreeHelper.GetParent(line.Line) as DesignerCanvas;

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
		}

		private void LineOnLineMoving(object sender, System.EventArgs args)
		{
			var line = sender as ConnectionLine;

			if (line == null)
			{
				return;
			}

			if(Lines.Count == 3 && DestinationConnector.Orientation == SourceConnector.Orientation)
			{
				if(Lines.IndexOf(line) == 1)
				{
					if(EnsureBoundsForMiddleLine(line)) return;
				}
			}
			else
			{
				if(Lines.IndexOf(line) == 1)
				{
					if(EnsureBoundsForSecondLine(line)) return;
				}

				if(Lines.IndexOf(line) == Lines.Count - 2)
				{
					if(EnsureBoundsForLastButOneLine(line)) return;
				}
			}

			if (EnsureBoundsToCanvasDimensions(line)) return;


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

		private bool EnsureBoundsForMiddleLine(ConnectionLine line)
		{
			Connector connector = null;
			switch (SourceConnector.Orientation)
			{
				case ConnectorOrientation.Up:
					connector = SourceConnector.EndPoint.Y < DestinationConnector.EndPoint.Y ? SourceConnector : DestinationConnector;
					break;
				case ConnectorOrientation.Down:
					connector = SourceConnector.EndPoint.Y > DestinationConnector.EndPoint.Y ? SourceConnector : DestinationConnector;
					break;
				case ConnectorOrientation.Left:
					connector = SourceConnector.EndPoint.X < DestinationConnector.EndPoint.X ? SourceConnector : DestinationConnector;
					break;
				case ConnectorOrientation.Right:
					connector = SourceConnector.EndPoint.X > DestinationConnector.EndPoint.X ? SourceConnector : DestinationConnector;
					break;

			}
			return EnsureBoundsForLineByConnectorPosition(line, connector);
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
			_canvas = null;

			CorrectConnectorPosition(line);

			SynchronizeBendingPoints();
			_newBendPoints.Clear();
		}

		private bool EnsureBoundsForSecondLine(ConnectionLine line)
		{
			var connector = SourceConnector.EndPoint.Equals(Lines.FirstOrDefault()?.StartPoint) ? SourceConnector : DestinationConnector;
			return EnsureBoundsForLineByConnectorPosition(line, connector);
		}

		private bool EnsureBoundsForLastButOneLine(ConnectionLine line)
		{
			var connector = SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint) ? SourceConnector : DestinationConnector;
			return EnsureBoundsForLineByConnectorPosition(line, connector);
		}

		private bool EnsureBoundsForLineByConnectorPosition(ConnectionLine line, Connector connector)
		{
			if (line.Orientation == LineOrientation.Vertical)
			{
				switch (connector.Orientation)
				{
					case ConnectorOrientation.Left:
						if (line.StartPoint.X >= connector.EndPoint.X)
						{
							line.StartPoint.X = connector.EndPoint.X;
							line.EndPoint.X = connector.EndPoint.X;
							return true;
						}
						break;
					case ConnectorOrientation.Right:
						if (line.StartPoint.X <= connector.EndPoint.X)
						{
							line.StartPoint.X = connector.EndPoint.X;
							line.EndPoint.X = connector.EndPoint.X;
							return true;
						}
						break;
				}
			}
			else
			{
				switch (connector.Orientation)
				{
					case ConnectorOrientation.Up:
						if (line.StartPoint.Y >= connector.EndPoint.Y)
						{
							line.StartPoint.Y = connector.EndPoint.Y;
							line.EndPoint.Y = connector.EndPoint.Y;
							return true;
						}
						break;
					case ConnectorOrientation.Down:
						if (line.StartPoint.Y <= connector.EndPoint.Y)
						{
							line.StartPoint.Y = connector.EndPoint.Y;
							line.EndPoint.Y = connector.EndPoint.Y;
							return true;
						}
						break;
				}
			}
			return false;
		}

		private bool EnsureBoundsToCanvasDimensions(ConnectionLine line)
		{
			switch (line.Orientation)
			{
				case LineOrientation.Vertical:
					if (line.StartPoint.X <= 0)
					{
						line.StartPoint.X = 1;
						line.EndPoint.X = 1;
						return true;
					}
					if (line.StartPoint.X >= _canvas.Width)
					{
						line.StartPoint.X = _canvas.Width - 1;
						line.EndPoint.X = _canvas.Width - 1;
						return true;
					}
					break;
				case LineOrientation.Horizontal:
					if (line.StartPoint.Y <= 0)
					{
						line.StartPoint.Y = 1;
						line.EndPoint.Y = 1;
						return true;
					}
					if (line.StartPoint.Y >= _canvas.Height)
					{
						line.StartPoint.Y = _canvas.Height - 1;
						line.EndPoint.Y = _canvas.Height - 1;
						return true;
					}
					break;
			}
			return false;
		}

		private void CorrectConnectorPosition(ConnectionLine line)
		{
			if (Lines.Count == 1)
			{
				if (line.StartPoint.Equals(SourceConnector.EndPoint))
				{
					CorrectPosition(SourceConnector, line, SourceViewModel, true);
				}
				else if (line.StartPoint.Equals(DestinationConnector.EndPoint))
				{
					CorrectPosition(DestinationConnector, line, DestinationViewModel, true);
				}

				if (line.EndPoint.Equals(SourceConnector.EndPoint))
				{
					CorrectPosition(SourceConnector, line, SourceViewModel, false);
				}
				else if (line.EndPoint.Equals(DestinationConnector.EndPoint))
				{
					CorrectPosition(DestinationConnector, line, DestinationViewModel, false);
				}
			}
			else
			{
				if (IsFirstLine(line))
				{
					if (line.StartPoint.Equals(SourceConnector.EndPoint))
					{
						CorrectPosition(SourceConnector, line, SourceViewModel, true);
					}
					else if (line.StartPoint.Equals(DestinationConnector.EndPoint))
					{
						CorrectPosition(DestinationConnector, line, DestinationViewModel, true);
					}
				}
				else if (IsLastLine(line))
				{
					if (line.EndPoint.Equals(SourceConnector.EndPoint))
					{
						CorrectPosition(SourceConnector, line, SourceViewModel, false);
					}
					else if (line.EndPoint.Equals(DestinationConnector.EndPoint))
					{
						CorrectPosition(DestinationConnector, line, DestinationViewModel, false);
					}
				}
			}
		}

		private void CorrectPosition(Connector connector, ConnectionLine line, TableViewModel vm, bool isFirstLine)
		{
			const int correctionLenght1 = 10;
			int correctionLenght2 = vm.ViewMode == TableViewMode.NameOnly ? (int) vm.Height/2 : 20;
			switch (connector.Orientation)
			{
				case ConnectorOrientation.Up:
					if (connector.EndPoint.X < vm.Left + Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.Y = vm.Top + correctionLenght2;
							if (line.StartPoint.X >= vm.Left - Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								prevLine.StartPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = vm.Left - Connector.ConnectorLenght;
							newLine.StartPoint.Y = line.StartPoint.Y;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.Y = vm.Top + correctionLenght2;
							if (line.EndPoint.X >= vm.Left - Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								prevLine.EndPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = vm.Left - Connector.ConnectorLenght;
							newLine.EndPoint.Y = line.EndPoint.Y;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Left;
					}
					else if (connector.EndPoint.X > vm.Left + vm.Width - Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.Y = vm.Top + correctionLenght2;
							if (line.StartPoint.X <= vm.Left + vm.Width + Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								prevLine.StartPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght;
							newLine.StartPoint.Y = line.StartPoint.Y;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.Y = vm.Top + correctionLenght2;
							if (line.EndPoint.X <= vm.Left + vm.Width + Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								prevLine.EndPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght;
							newLine.EndPoint.Y = line.EndPoint.Y;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Right;
					}
					break;
				case ConnectorOrientation.Down:
					if (connector.EndPoint.X < vm.Left + Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.Y = vm.Top + vm.Height - correctionLenght2;
							if (line.StartPoint.X >= vm.Left - Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								prevLine.StartPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = vm.Left - Connector.ConnectorLenght;
							newLine.StartPoint.Y = line.StartPoint.Y;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.Y = vm.Top + vm.Height - correctionLenght2;
							if (line.EndPoint.X >= vm.Left - Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.X = vm.Left - Connector.ConnectorLenght - correctionLenght1;
								prevLine.EndPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = vm.Left - Connector.ConnectorLenght;
							newLine.EndPoint.Y = line.EndPoint.Y;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Left;
					}
					else if (connector.EndPoint.X > vm.Left + vm.Width - Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.Y = vm.Top + vm.Height - correctionLenght2;
							if (line.StartPoint.X <= vm.Left + vm.Width + Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								prevLine.StartPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght;
							newLine.StartPoint.Y = line.StartPoint.Y;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.Y = vm.Top + vm.Height - correctionLenght2;
							if (line.EndPoint.X <= vm.Left + vm.Width + Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght + correctionLenght1;
								prevLine.EndPoint.X = line.StartPoint.X;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = vm.Left + vm.Width + Connector.ConnectorLenght;
							newLine.EndPoint.Y = line.EndPoint.Y;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Right;
					}
					break;
				case ConnectorOrientation.Left:
					if (connector.EndPoint.Y < vm.Top + Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.X = vm.Left + correctionLenght2;
							if (line.StartPoint.Y >= vm.Top - Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								prevLine.StartPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = line.StartPoint.X;
							newLine.StartPoint.Y = vm.Top - Connector.ConnectorLenght;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.X = vm.Left + correctionLenght2;
							if (line.EndPoint.Y >= vm.Top - Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								prevLine.EndPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = newLine.StartPoint.X;
							newLine.EndPoint.Y = vm.Top - Connector.ConnectorLenght;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Up;
					}
					else if (connector.EndPoint.Y > vm.Top + vm.Height - Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.X = vm.Left + correctionLenght2;
							if (line.StartPoint.Y <= vm.Top + vm.Height + Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								prevLine.StartPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = line.StartPoint.X;
							newLine.StartPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.X = vm.Left + correctionLenght2;
							if (line.EndPoint.Y <= vm.Top + vm.Height + Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								prevLine.EndPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = newLine.StartPoint.X;
							newLine.EndPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Down;
					}
					break;
				case ConnectorOrientation.Right:
					if (connector.EndPoint.Y < vm.Top + Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.X = vm.Left + vm.Width - correctionLenght2;
							if (line.StartPoint.Y >= vm.Top - Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								prevLine.StartPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = line.StartPoint.X;
							newLine.StartPoint.Y = vm.Top - Connector.ConnectorLenght;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.X = vm.Left + vm.Width - correctionLenght2;
							if (line.EndPoint.Y >= vm.Top - Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								line.EndPoint.Y = vm.Top - Connector.ConnectorLenght - correctionLenght1;
								prevLine.EndPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = newLine.StartPoint.X;
							newLine.EndPoint.Y = vm.Top - Connector.ConnectorLenght;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Up;
					}
					else if (connector.EndPoint.Y > vm.Top + vm.Height - Connector.SymbolLineEndsDiff)
					{
						if (isFirstLine)
						{
							line.StartPoint.X = vm.Left + vm.Width - correctionLenght2;
							if (line.StartPoint.Y <= vm.Top + vm.Height + Connector.ConnectorLenght)
							{
								var prevLine = Lines[1];
								line.StartPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								prevLine.StartPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.EndPoint.X = line.StartPoint.X;
							newLine.EndPoint.Y = line.StartPoint.Y;
							newLine.StartPoint.X = line.StartPoint.X;
							newLine.StartPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght;
							connector.EndPoint = newLine.StartPoint;
							Lines.Insert(0, newLine);
						}
						else
						{
							line.EndPoint.X = vm.Left + vm.Width - correctionLenght2;
							if (line.EndPoint.Y <= vm.Top + vm.Height + Connector.ConnectorLenght)
							{
								var prevLine = Lines[Lines.Count - 2];
								line.StartPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								line.EndPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght + correctionLenght1;
								prevLine.EndPoint.Y = line.StartPoint.Y;
							}

							ConnectionLine newLine = new ConnectionLine();
							newLine.StartPoint.X = line.EndPoint.X;
							newLine.StartPoint.Y = line.EndPoint.Y;
							newLine.EndPoint.X = newLine.StartPoint.X;
							newLine.EndPoint.Y = vm.Top + vm.Height + Connector.ConnectorLenght;
							connector.EndPoint = newLine.EndPoint;
							Lines.Add(newLine);
						}
						connector.Orientation = ConnectorOrientation.Down;
					}
					break;
			}
		}

		private bool IsFirstLine(ConnectionLine line)
		{
			return Lines.IndexOf(line) == 0;
		}

		private bool IsLastLine(ConnectionLine line)
		{
			return Lines.IndexOf(line) == Lines.Count - 1;
		}

		public void ClearPoints()
		{
			var len = Points.Count;
			for (int i = 0; i < len; i++)
			{
				Points.RemoveAt(0);
			}
			Points.Clear();
		}

		public void ClearLines()
		{
			int len = Lines.Count;
			for (int i = 0; i < len; i++)
			{
				Lines.RemoveAt(0);
			}
			Lines.Clear();
		}

		public void ClearMarks()
		{
			int len = Marks.Count;
			for (int i = 0; i < len; i++)
			{
				Marks.RemoveAt(0);
			}
			Marks.Clear();
		}

		private void RemoveRedundandBendPoints()
		{
			var forSelection = Points.Skip(1).Take(Points.Count - 2);
			var duplicate = forSelection.GroupBy(t => t).Where(s => s.Count() > 1).Select(u => u.Key);
			var forRemove = Points.Where(t => duplicate.Any(s => s.Equals(t))).Where(s => !_newBendPoints.Any(r => r.Equals(s))).ToList();

			foreach (ConnectionPoint connectionPoint in forRemove)
			{
				Points.Remove(connectionPoint);
			}
		}

		private void NormalizeLinesWithTolerance(double tolerance)
		{
			if (Lines.Count < 3)
			{
				return;
			}

			for (int i = 1; i < Lines.Count - 1; i++)
			{
				var line = Lines[i];

				if (line.GetLenght() < tolerance)
				{
					var prevLine = Lines[i - 1];
					var nextLine = Lines[i + 1];
					if (line.Orientation == LineOrientation.Vertical)
					{
						line.StartPoint.Y = prevLine.EndPoint.Y;
						line.EndPoint.Y = prevLine.EndPoint.Y;
						nextLine.StartPoint.Y = prevLine.EndPoint.Y;
						nextLine.EndPoint.Y = prevLine.EndPoint.Y;
					}
					else
					{
						line.StartPoint.X = prevLine.EndPoint.X;
						line.EndPoint.X = prevLine.EndPoint.X;
						nextLine.StartPoint.X = prevLine.EndPoint.X;
						nextLine.EndPoint.X = prevLine.EndPoint.X;
					}
				}
			}
		}

		public void SynchronizeBendingPoints()
		{
			NormalizeLinesWithTolerance(PointsDistaceTolerance);
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

		private bool IsSelfConnection()
		{
			return DestinationViewModel.Equals(SourceViewModel);
		}

		private bool IsSourceAndDestinationSelected()
		{
			return SourceViewModel.IsSelected && DestinationViewModel.IsSelected;
		}

		private void MoveAllLines(TablePositionAndMeasureEventArgs args)
		{
			foreach (ConnectionLine line in Lines)
			{
				line.EndPoint.X += args.LeftDelta;
				line.EndPoint.Y += args.TopDelta;

				line.StartPoint.X += args.LeftDelta;
				line.StartPoint.Y += args.TopDelta;
			}
		}

		private void AdjustConnectorMarkToHorizontalLimits(Connector connector, TableViewModel table, ConnectionLine endLine, ConnectionLine nextLine, TablePositionAndMeasureEventArgs e, bool isFirstLine)
		{
			if (connector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
			{
				endLine.StartPoint.X += e.LeftDelta;
				endLine.EndPoint.X += e.LeftDelta;

				if (nextLine != null)
				{
					if (isFirstLine)
					{
						nextLine.StartPoint.X += e.LeftDelta;
					}
					else
					{
						nextLine.EndPoint.X += e.LeftDelta;
					}
				}
			}

			if (connector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
			{
				endLine.StartPoint.X += e.WidthDelta;
				endLine.EndPoint.X += e.WidthDelta;

				if (nextLine != null)
				{
					if (isFirstLine)
					{
						nextLine.StartPoint.X += e.WidthDelta;
					}
					else
					{
						nextLine.EndPoint.X += e.WidthDelta;
					}
				}
			}
		}

		private void AdjustConnectorMarkToVerticalLimits(Connector connector, TableViewModel table, ConnectionLine endLine, ConnectionLine nextLine, TablePositionAndMeasureEventArgs e, bool isFirstLine)
		{
			if (connector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
			{
				endLine.StartPoint.Y += e.TopDelta;
				endLine.EndPoint.Y += e.TopDelta;

				if (nextLine != null)
				{
					if (isFirstLine)
					{
						nextLine.StartPoint.Y += e.TopDelta;
					}
					else
					{
						nextLine.EndPoint.Y += e.TopDelta;
					}
				}
			}

			if (connector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
			{
				endLine.StartPoint.Y += e.HeightDelta;
				endLine.EndPoint.Y += e.HeightDelta;

				if (nextLine != null)
				{
					if (isFirstLine)
					{
						nextLine.StartPoint.Y += e.HeightDelta;
					}
					else
					{
						nextLine.EndPoint.Y += e.HeightDelta;
					}
				}
			}
		}

		private void SplitLinesIfNeeded()
		{
			if (Lines.Count == 1)
			{
				SplitLine(Lines.FirstOrDefault(), Lines.FirstOrDefault()?.GetMiddlePoint());
				SynchronizeBendingPoints();
			}
		}

		private bool IsAnySelectedTableMoving()
		{
			ConnectionLine line = Lines.FirstOrDefault();
			var canvas = VisualTreeHelper.GetParent(line.Line) as DesignerCanvas;

			if (canvas == null)
			{
				return false;
			}

			var itemsForCheck = canvas.SelectedTables.Where(t => !t.TableViewModel.Equals(SourceViewModel) && !t.TableViewModel.Equals(DestinationViewModel)).Where(s => s.TableViewModel.IsMoving);
			return itemsForCheck.Any();
		}

		private void MoveConnectorOnTableViewModeChange(Connector connector, TableViewModel table)
		{
			if (connector.Orientation == ConnectorOrientation.Right || connector.Orientation == ConnectorOrientation.Left)
			{
				SplitLinesIfNeeded();
				var endLine = Lines.FirstOrDefault(t => t.EndPoint.Equals(connector.EndPoint) || t.StartPoint.Equals(connector.EndPoint));
				int endLineIdx = Lines.IndexOf(endLine);
				endLine.StartPoint.Y = table.Top + table.Height/2;
				endLine.EndPoint.Y = table.Top + table.Height/2;

				ConnectionLine nextLine = null;
				if (endLineIdx == 0)
				{
					nextLine = Lines[endLineIdx + 1];
					nextLine.StartPoint.Y = table.Top + table.Height/2;
				}
				else
				{
					nextLine = Lines[endLineIdx - 1];
					nextLine.EndPoint.Y = table.Top + table.Height/2;
				}
			}
		}

		public static void GetConnectionLimits(ref double top, ref double bot, ref double left, ref double right, Connector connector, ConnectionInfoViewModel connection)
		{
			if (connection.Lines.Count <= 1)
			{
				return;
			}

			var line = connection.Lines.FirstOrDefault(t => t.EndPoint.Equals(connector.EndPoint) || t.StartPoint.Equals(connector.EndPoint));

			if (line == null)
			{
				return;
			}

			var len = line.GetLenght();

			switch (connector.Orientation)
			{
				case ConnectorOrientation.Up:
					top = Math.Min(len, top);
					break;
				case ConnectorOrientation.Down:
					bot = Math.Min(len, bot);
					break;
				case ConnectorOrientation.Left:
					left = Math.Min(len, left);
					break;
				case ConnectorOrientation.Right:
					right = Math.Min(len, right);
					break;
			}
		}

		public void BuildConnection()
		{
			if (SourceViewModel == null)
			{
				throw new ApplicationException("SourceViewModel is null");
			}

			if (DestinationViewModel == null)
			{
				throw new ApplicationException("DestinationViewModel is null");
			}

			if (IsSelfConnection())
			{
				BuildSelfConnection();
				return;
			}

			BuildConnectionBetweenViewModels();
		}

		public async Task BuildConnection2(DatabaseModelDesignerViewModel designer, Grid grid = null)
		{
			if(SourceViewModel == null)
			{
				throw new ApplicationException("SourceViewModel is null");
			}

			if(DestinationViewModel == null)
			{
				throw new ApplicationException("DestinationViewModel is null");
			}

			await BuildConnectionBetweenViewModelsUsingPathFinding(designer, grid);
		}

		public async Task BuildConnection3(DatabaseModelDesignerViewModel designer)
		{
			if(SourceViewModel == null)
			{
				throw new ApplicationException("SourceViewModel is null");
			}

			if(DestinationViewModel == null)
			{
				throw new ApplicationException("DestinationViewModel is null");
			}

			await BuildConnectionBetweenViewModelsUsingReducedGraphPathFinding(designer);
		}

		private async Task BuildConnectionBetweenViewModelsUsingReducedGraphPathFinding(DatabaseModelDesignerViewModel designer)
		{
			int step = GraphTransformStep;
			if(IsSelfConnection())
			{
				BuildSelfConnection();
				return;
			}

			if(AreTablesOverlaping())
			{
				BuildOverlapConnection();
				return;
			}

			var source = SetupConnector(SourceViewModel, SourceConnector, designer,step);
			var destination = SetupConnector(DestinationViewModel, DestinationConnector, designer,step);

			if (source == null || destination == null)
			{
				BuildOverlapConnection();
				return;
			}

			var grid = await DiagramFacade.CreateMinifiedGridForPathFinding(designer, step);
			AbstractPathFinder pathFinder = new AStarPathFinder(grid);
			Point[] points = await Task.Factory.StartNew(() => pathFinder.FindPathBendingPointsOnly(source[1].ToMinified(step), destination[1].ToMinified(step)));

			if (points == null)
			{
				BuildOverlapConnection();
				return;
			}

			var allPoints = new List<Point>();
			allPoints.Add(source[0]);
			allPoints.AddRange(points.Select(s => s.FromMinified(step)).Reverse());
			allPoints.Add(destination[0]);

			var tempCollection = new List<ConnectionPoint> {new ConnectionPoint(source[0].X, source[0].Y)};
			if (allPoints.Count > 3)
			{
				for (int i = 1; i < allPoints.Count-1; i++)
				{
					var prev = allPoints[i - 1];
					var curr = allPoints[i];
					var next = allPoints[i + 1];

					if(curr.Y == prev.Y && curr.Y == next.Y)
					{
						continue;
					}

					if(curr.X == prev.X && curr.X == next.X)
					{
						continue;
					}

					tempCollection.Add(new ConnectionPoint(curr.X, curr.Y));
				}
			}
			tempCollection.Add(new ConnectionPoint(destination[0].X, destination[0].Y));

			tempCollection.ForEach(s => Points.Add(s));

			SourceConnector.Cardinality = Cardinality.One;
			DestinationConnector.Cardinality = Cardinality.Many;
			DestinationConnector.Optionality = RelationshipModel.Optionality;
			SourceConnector.EndPoint = Points.FirstOrDefault();
			DestinationConnector.EndPoint = Points.LastOrDefault();
			IsSourceConnectorAtStartPoint = true;
			BuildLinesFromPoints();
		}

		private Point[] SetupConnector(TableViewModel vm, Connector connector, DatabaseModelDesignerViewModel designer,int step)
		{
			var horizontalLines = new List<int>();
			var verticalLines = new List<int>();

			int off = (int)Connector.ConnectorLenght;
			int l = (int)vm.Left;
			int r = (int)(vm.Left + vm.Width);
			int t = (int)vm.Top;
			int b = (int)(vm.Top + vm.Height);

			int limit1 = (l / step) + 1;
			int limit2 = (r / step);

			for (int i = limit1; i <= limit2; i++)
			{
				verticalLines.Add(i*step);
			}

			if (vm.ViewMode != TableViewMode.NameOnly)
			{
				limit1 = (t / step) + 1;
				limit2 = b / step;

				for(int i = limit1; i <= limit2; i++)
				{
					horizontalLines.Add(i * step);
				}
			}

			var rnd = new Random();
			var rects = DiagramFacade.GetTableRectangles(designer.TableViewModels, step);
			var canvas = new Rectangle(0,0, (int) designer.CanvasWidth, (int) designer.CanvasHeight);
			bool vertical = !horizontalLines.Any() || (rnd.Next(2) == 0);

			if (vertical)
			{
				var offsetPointsTop = verticalLines.Select(s => new Point[] { new Point(s, t - off), new Point(s, (t-off)/step * step) });
				var offsetPointsBot = verticalLines.Select(s => new Point[] { new Point(s, b + off), new Point(s, ((b+off)/step+2) * step) });

				IEnumerable<Point[]> validTop = offsetPointsTop.Where(s => rects.All(m => !DiagramFacade.DoesPointIntersectWithRectangle(m, s[1]))).Where(n => DiagramFacade.DoesPointIntersectWithRectangle(canvas,n[1]));
				IEnumerable<Point[]> validBot = offsetPointsBot.Where(s => rects.All(m => !DiagramFacade.DoesPointIntersectWithRectangle(m, s[1]))).Where(n => DiagramFacade.DoesPointIntersectWithRectangle(canvas, n[1]));

				var top = validTop as IList<Point[]> ?? validTop.ToList();
				bool bottom = !top.Any() || rnd.Next(2) == 0;
				IEnumerable<Point[]> bot = validBot as IList<Point[]> ?? validBot.ToList();

				if (bot.Any() && bottom)
				{
					IEnumerable<Point[]> shuffle = bot.Shuffle(rnd);
					connector.Orientation = ConnectorOrientation.Down;
					return shuffle.FirstOrDefault();
				}
				if (top.Any())
				{
					IEnumerable<Point[]> shuffle = top.Shuffle(rnd);
					connector.Orientation = ConnectorOrientation.Up;
					return shuffle.FirstOrDefault();
				}
			}
			else
			{
				var offsetPointsLeft = horizontalLines.Select(s => new Point[] { new Point(l - off, s), new Point((l - off)/step * step, s) });
				var offsetPointsRight = horizontalLines.Select(s => new Point[] { new Point(r + off, s), new Point(((r + off) / step + 2) * step, s) });

				IEnumerable<Point[]> validLeft = offsetPointsLeft.Where(s => rects.All(m => !DiagramFacade.DoesPointIntersectWithRectangle(m, s[1]))).Where(n => DiagramFacade.DoesPointIntersectWithRectangle(canvas, n[1]));
				IEnumerable<Point[]> validRight = offsetPointsRight.Where(s => rects.All(m => !DiagramFacade.DoesPointIntersectWithRectangle(m, s[1]))).Where(n => DiagramFacade.DoesPointIntersectWithRectangle(canvas, n[1]));

				var left = validLeft as Point[][] ?? validLeft.ToArray();
				bool right = !left.Any() || rnd.Next(2) == 0;
				IEnumerable<Point[]> rig = validRight as IList<Point[]> ?? validRight.ToList();

				if(rig.Any() && right)
				{
					IEnumerable<Point[]> shuffle = rig.Shuffle(rnd);
					connector.Orientation = ConnectorOrientation.Right;
					return shuffle.FirstOrDefault();
				}

				if (left.Any())
				{
					IEnumerable<Point[]> shuffle1 = left.Shuffle(rnd);
					connector.Orientation = ConnectorOrientation.Left;
					return shuffle1.FirstOrDefault();
				}
			}

			return null;
		}

		private async Task BuildConnectionBetweenViewModelsUsingPathFinding(DatabaseModelDesignerViewModel designer, Grid grid = null)
		{
			Point start = new Point((int)SourceViewModel.Left + 20, (int)(SourceViewModel.Top + SourceViewModel.Height) + (int)Connector.ConnectorLenght);
			Point end = new Point((int)DestinationViewModel.Left + 20, (int)(DestinationViewModel.Top + DestinationViewModel.Height) + (int)Connector.ConnectorLenght);

			int off = (int)Connector.ConnectorLenght;

			int sl = (int)SourceViewModel.Left;
			int sr = (int)(SourceViewModel.Left + SourceViewModel.Width);
			int st = (int)SourceViewModel.Top;
			int sb = (int)(SourceViewModel.Top + SourceViewModel.Height);
			int sw = (int)SourceViewModel.Width;
			int sw2 = sw / 2;
			int sh = (int)SourceViewModel.Height;
			int sh2 = sh / 2;

			int dl = (int)DestinationViewModel.Left;
			int dr = (int)(DestinationViewModel.Left + DestinationViewModel.Width);
			int dt = (int)DestinationViewModel.Top;
			int db = (int)(DestinationViewModel.Top + DestinationViewModel.Height);
			int dw = (int)DestinationViewModel.Width;
			int dw2 = dw / 2;
			int dh = (int)DestinationViewModel.Height;
			int dh2 = sh / 2;

			if (IsSelfConnection())
			{
				start = new Point(sl + sw2, sb + off);
				end = new Point(sr + off, st + sh2);
				SourceConnector.Orientation = ConnectorOrientation.Down;
				DestinationConnector.Orientation = ConnectorOrientation.Right;
			}
			else if (AreTablesOverlaping())
			{
				BuildOverlapConnection();
				return;
			}
			else
			{
				var position = GetRelativePositionOfDestinationTable();
				switch(position)
				{
					case RelativeTablePosition.LeftTop:
						start = new Point(sl - off, st + sh2);
						end = new Point(dl + dw2, db + off);
						SourceConnector.Orientation = ConnectorOrientation.Left;
						DestinationConnector.Orientation = ConnectorOrientation.Down;
						break;
					case RelativeTablePosition.Top:
						start = new Point(sl + sw2, st - off);
						end = new Point(dl + dw2, db + off);
						SourceConnector.Orientation = ConnectorOrientation.Up;
						DestinationConnector.Orientation = ConnectorOrientation.Down;
						break;
					case RelativeTablePosition.RightTop:
						start = new Point(sr + off, st + sh2);
						end = new Point(dl + dw2, db + off);
						SourceConnector.Orientation = ConnectorOrientation.Right;
						DestinationConnector.Orientation = ConnectorOrientation.Down;
						break;
					case RelativeTablePosition.Right:
						start = new Point(sr + off, st + sh2);
						end = new Point(dl - off, dt + dh2);
						SourceConnector.Orientation = ConnectorOrientation.Right;
						DestinationConnector.Orientation = ConnectorOrientation.Left;
						break;
					case RelativeTablePosition.RightBottom:
						start = new Point(sr + off, st + sh2);
						end = new Point(dl + dw2, dt - off);
						SourceConnector.Orientation = ConnectorOrientation.Right;
						DestinationConnector.Orientation = ConnectorOrientation.Up;
						break;
					case RelativeTablePosition.Bottom:
						start = new Point(sl + sw2, sb + off);
						end = new Point(dl + dw2, dt - off);
						SourceConnector.Orientation = ConnectorOrientation.Down;
						DestinationConnector.Orientation = ConnectorOrientation.Up;
						break;
					case RelativeTablePosition.LeftBottom:
						start = new Point(sl - off, st + sh2);
						end = new Point(dl + dw2, dt - off);
						SourceConnector.Orientation = ConnectorOrientation.Left;
						DestinationConnector.Orientation = ConnectorOrientation.Up;
						break;
					case RelativeTablePosition.Left:
						start = new Point(sl - off, st + sh2);
						end = new Point(dr + off, dt + dh2);
						SourceConnector.Orientation = ConnectorOrientation.Left;
						DestinationConnector.Orientation = ConnectorOrientation.Right;
						break;
					case RelativeTablePosition.TopR:
						start = new Point(sl + sw2, st - off);
						end = new Point(dl + dw2, db + off);
						SourceConnector.Orientation = ConnectorOrientation.Up;
						DestinationConnector.Orientation = ConnectorOrientation.Down;
						break;
					case RelativeTablePosition.LeftB:
						start = new Point(sl - off, st + sh2);
						end = new Point(dr + off, dt + dh2);
						SourceConnector.Orientation = ConnectorOrientation.Left;
						DestinationConnector.Orientation = ConnectorOrientation.Right;
						break;
					case RelativeTablePosition.BottomR:
						start = new Point(sl + sw2, sb + off);
						end = new Point(dl + dw2, dt - off);
						SourceConnector.Orientation = ConnectorOrientation.Down;
						DestinationConnector.Orientation = ConnectorOrientation.Up;
						break;
					case RelativeTablePosition.RightB:
						start = new Point(sr + off, st + sh2);
						end = new Point(dl - off, dt + dh2);
						SourceConnector.Orientation = ConnectorOrientation.Right;
						DestinationConnector.Orientation = ConnectorOrientation.Left;
						break;
				}
			}
			
			SourceConnector.Cardinality = Cardinality.One;
			DestinationConnector.Cardinality = Cardinality.Many;
			DestinationConnector.Optionality = RelationshipModel.Optionality;

			int offset = 10;
			Point startFind = new Point();
			switch(SourceConnector.Orientation)
			{
				case ConnectorOrientation.Up:
					startFind = new Point(start.X, start.Y - offset);
					break;
				case ConnectorOrientation.Down:
					startFind = new Point(start.X, start.Y + offset);
					break;
				case ConnectorOrientation.Left:
					startFind = new Point(start.X - offset, start.Y);
					break;
				case ConnectorOrientation.Right:
					startFind = new Point(start.X + offset, start.Y);
					break;
			}

			Point endFind = new Point();
			switch(DestinationConnector.Orientation)
			{
				case ConnectorOrientation.Up:
					endFind = new Point(end.X, end.Y - offset);
					break;
				case ConnectorOrientation.Down:
					endFind = new Point(end.X, end.Y + offset);
					break;
				case ConnectorOrientation.Left:
					endFind = new Point(end.X - offset, end.Y);
					break;
				case ConnectorOrientation.Right:
					endFind = new Point(end.X + offset, end.Y);
					break;
			}

			//OOP A*
			if(grid == null)
			{
				grid = await Task.Factory.StartNew(() => CreateGridForPathFinding(designer));
				//grid = await designer.GetGrid();
				//await Task.Factory.StartNew(() => PathFinderHelper.UpdateGrid((int)designer.CanvasWidth, (int)designer.CanvasHeight, GetRectangleAreas(designer), grid));
			}

			AbstractPathFinder pathFinder = new AStarPathFinder(grid);
			Point[] points = await Task.Factory.StartNew(() => pathFinder.FindPathBendingPointsOnly(startFind, endFind));


			//var byteGrid = await Task.Factory.StartNew(() => PathFinderHelper.CreateGridStruct((int)designer.CanvasWidth, (int)designer.CanvasHeight, GetRectangleAreas(designer)));

			//AStarStructPathFinder pathFinder = new AStarStructPathFinder(byteGrid, (int)designer.CanvasWidth, (int)designer.CanvasHeight);
			//Point[] points = await Task.Factory.StartNew(() => pathFinder.FindPathBendingPointsOnly(startFind, endFind));

			if(points == null)
			{
				BuildOverlapConnection();
				return;
			}

			var reverse = points.Reverse();
			List<ConnectionPoint> all = new List<ConnectionPoint>();

			all.Add(new ConnectionPoint(start.X, start.Y));
			all.AddRange(reverse.Select(p => new ConnectionPoint(p.X, p.Y)));
			all.Add(new ConnectionPoint(end.X, end.Y));

			Points.Add(new ConnectionPoint(start.X, start.Y));
			if (all.Count > 3)
			{
				for (var i = 1; i < all.Count - 1; i++)
				{
					var prev = all[i - 1];
					var curr = all[i];
					var next = all[i + 1];

					if(curr.Y == prev.Y && curr.Y == next.Y)
					{
						continue;
					}

					if(curr.X == prev.X && curr.X == next.X)
					{
						continue;
					}

					Points.Add(curr);
				}
			}
			Points.Add(new ConnectionPoint(end.X, end.Y));

			SourceConnector.EndPoint = Points.FirstOrDefault();
			DestinationConnector.EndPoint = Points.LastOrDefault();

			IsSourceConnectorAtStartPoint = true;

			BuildLinesFromPoints();
		}

		private Grid CreateGridForPathFinding(DatabaseModelDesignerViewModel designer)
		{
			var obs = GetRectangleAreas(designer);
			Grid grid = PathFinderHelper.CreateGrid((int) designer.CanvasWidth, (int) designer.CanvasHeight, obs);
			return grid;
		}

		private IEnumerable<Rectangle> GetRectangleAreas(DatabaseModelDesignerViewModel designer)
		{
			var obs = new List<Rectangle>();

			IEnumerable<TableViewModel> models = designer.TableViewModels.Where(t => !(t.Equals(SourceViewModel) || t.Equals(DestinationViewModel)));

			foreach(TableViewModel model in models)
			{
				int l, t, w, h;

				l = (int)(model.Left - 4 * Connector.ConnectorLenght);
				t = (int)(model.Top - 4 * Connector.ConnectorLenght);
				w = (int)(model.Width + 4 * Connector.ConnectorLenght);
				h = (int)(model.Height + 4 * Connector.ConnectorLenght);

				var rect = new Rectangle(l < 0 ? 0 : l, t < 0 ? 0 : t, (int)(w), (int)(h));
				obs.Add(rect);
			}

			foreach (TableViewModel model in designer.TableViewModels.Where(t => t.Equals(SourceViewModel) || t.Equals(DestinationViewModel)))
			{
				int l, t, w, h;

				l = (int)(model.Left);
				t = (int)(model.Top);
				w = (int)(model.Width);
				h = (int)(model.Height);

				var rect = new Rectangle(l < 0 ? 0 : l, t < 0 ? 0 : t, w, h);
				obs.Add(rect);
			}

			return obs;
		}

		private void BuildConnectionBetweenViewModels()
		{
			if (AreTablesOverlaping())
			{
				BuildOverlapConnection();
				return;
			}

			var position = GetRelativePositionOfDestinationTable();

			switch (position)
			{
				case RelativeTablePosition.LeftTop:
					BuildLeftTopConnection();
					break;
				case RelativeTablePosition.Top:
					if (!IsSufficientDistance(position))
					{
						BuildVerticalConnection();
						break;
					}
					BuildTopConnection(true);
					break;
				case RelativeTablePosition.RightTop:
					BuildRightTopConnection();
					break;
				case RelativeTablePosition.Right:
					if (!IsSufficientDistance(position))
					{
						BuildHorizontalConnection();
						break;
					}
					BuildRightConnection(false);
					break;
				case RelativeTablePosition.RightBottom:
					BuildRightBottomConnection();
					break;
				case RelativeTablePosition.Bottom:
					if (!IsSufficientDistance(position))
					{
						BuildVerticalConnection();
						break;
					}
					BuildBottomConnection(true);
					break;
				case RelativeTablePosition.LeftBottom:
					BuildLeftBottomConnection();
					break;
				case RelativeTablePosition.Left:
					if (!IsSufficientDistance(position))
					{
						BuildHorizontalConnection();
						break;
					}
					BuildLeftConnection(false);
					break;
				case RelativeTablePosition.TopR:
					if (!IsSufficientDistance(position))
					{
						BuildVerticalConnection();
						break;
					}
					BuildTopConnection(false);
					break;
				case RelativeTablePosition.LeftB:
					if (!IsSufficientDistance(position))
					{
						BuildHorizontalConnection();
						break;
					}
					BuildLeftConnection(true);
					break;
				case RelativeTablePosition.BottomR:
					if (!IsSufficientDistance(position))
					{
						BuildVerticalConnection();
						break;
					}
					BuildBottomConnection(false);
					break;
				case RelativeTablePosition.RightB:
					if (!IsSufficientDistance(position))
					{
						BuildHorizontalConnection();
						break;
					}
					BuildRightConnection(true);
					break;
			}


			SourceConnector.Cardinality = Cardinality.One;
			DestinationConnector.Cardinality = Cardinality.Many;
			DestinationConnector.Optionality = RelationshipModel.Optionality;

			SourceConnector.EndPoint = Points.FirstOrDefault();
			DestinationConnector.EndPoint = Points.LastOrDefault();

			IsSourceConnectorAtStartPoint = true;

			BuildLinesFromPoints();
		}

		private double GetTableDistance(RelativeTablePosition position)
		{
			switch (position)
			{
				case RelativeTablePosition.Top:
					return SourceViewModel.Top - (DestinationViewModel.Top + DestinationViewModel.Height);
				case RelativeTablePosition.Right:
					return DestinationViewModel.Left - (SourceViewModel.Left + SourceViewModel.Width);
				case RelativeTablePosition.Bottom:
					return DestinationViewModel.Top - (SourceViewModel.Top + SourceViewModel.Height);
				case RelativeTablePosition.Left:
					return SourceViewModel.Left - (DestinationViewModel.Left + DestinationViewModel.Width);
				case RelativeTablePosition.TopR:
					return SourceViewModel.Top - (DestinationViewModel.Top + DestinationViewModel.Height);
				case RelativeTablePosition.LeftB:
					return SourceViewModel.Left - (DestinationViewModel.Left + DestinationViewModel.Width);
				case RelativeTablePosition.BottomR:
					return DestinationViewModel.Top - (SourceViewModel.Top + SourceViewModel.Height);
				case RelativeTablePosition.RightB:
					return DestinationViewModel.Left - (SourceViewModel.Left + SourceViewModel.Width);
			}

			return 0;
		}

		private bool IsSufficientDistance(RelativeTablePosition position)
		{
			//Less tolerace
			return GetTableDistance(position) > Connector.ConnectorLenght + 3;
		}

		private void BuildHorizontalConnection()
		{
			var y = SourceViewModel.Top > DestinationViewModel.Top ? SourceViewModel.Top + SourceViewModel.Height + Connector.ConnectorLenght*2 : DestinationViewModel.Top + DestinationViewModel.Height + Connector.ConnectorLenght*2;

			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, SourceViewModel.Top + SourceViewModel.Height + Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, y);
			var point3 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width/2, y);
			var point4 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width/2, (int) (DestinationViewModel.Top + DestinationViewModel.Height + Connector.ConnectorLenght));

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);
			Points.Add(point4);

			SourceConnector.Orientation = ConnectorOrientation.Down;
			DestinationConnector.Orientation = ConnectorOrientation.Down;
		}

		private void BuildVerticalConnection()
		{
			var x = SourceViewModel.Left > DestinationViewModel.Left ? SourceViewModel.Left + SourceViewModel.Width + Connector.ConnectorLenght*2 : DestinationViewModel.Left + DestinationViewModel.Width + Connector.ConnectorLenght*2;

			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width + Connector.ConnectorLenght, SourceViewModel.Top + SourceViewModel.Height/2);
			var point2 = new ConnectionPoint(x, SourceViewModel.Top + SourceViewModel.Height/2);
			var point3 = new ConnectionPoint(x, DestinationViewModel.Top + DestinationViewModel.Height/2);
			var point4 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + Connector.ConnectorLenght, DestinationViewModel.Top + DestinationViewModel.Height/2);

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);
			Points.Add(point4);

			SourceConnector.Orientation = ConnectorOrientation.Right;
			DestinationConnector.Orientation = ConnectorOrientation.Right;
		}

		private void BuildLeftTopConnection()
		{
			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, SourceViewModel.Top - Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));
			var point3 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + Connector.ConnectorLenght, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);

			SourceConnector.Orientation = ConnectorOrientation.Up;
			DestinationConnector.Orientation = ConnectorOrientation.Right;
		}

		private void BuildRightTopConnection()
		{
			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, SourceViewModel.Top - Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));
			var point3 = new ConnectionPoint(DestinationViewModel.Left - Connector.ConnectorLenght, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);

			SourceConnector.Orientation = ConnectorOrientation.Up;
			DestinationConnector.Orientation = ConnectorOrientation.Left;
		}

		private void BuildLeftBottomConnection()
		{
			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, SourceViewModel.Top + SourceViewModel.Height + Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));
			var point3 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + Connector.ConnectorLenght, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);

			SourceConnector.Orientation = ConnectorOrientation.Down;
			DestinationConnector.Orientation = ConnectorOrientation.Right;
		}

		private void BuildRightBottomConnection()
		{
			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, SourceViewModel.Top + SourceViewModel.Height + Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width/2, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));
			var point3 = new ConnectionPoint(DestinationViewModel.Left - Connector.ConnectorLenght, (int) (DestinationViewModel.Top + DestinationViewModel.Height/2));

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);

			SourceConnector.Orientation = ConnectorOrientation.Down;
			DestinationConnector.Orientation = ConnectorOrientation.Left;
		}

		private void BuildTopConnection(bool fromRight = false)
		{
			int x;

			if (fromRight)
			{
				x = (int) (DestinationViewModel.Left + Connector.SymbolLineEndsDiff + 5);
			}
			else
			{
				x = (int) (DestinationViewModel.Left + DestinationViewModel.Width - Connector.SymbolLineEndsDiff - 5);
			}

			var point1 = new ConnectionPoint(x, SourceViewModel.Top - Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(x, (int) (DestinationViewModel.Top + DestinationViewModel.Height + Connector.ConnectorLenght));

			Points.Add(point1);
			Points.Add(point2);

			SourceConnector.Orientation = ConnectorOrientation.Up;
			DestinationConnector.Orientation = ConnectorOrientation.Down;
		}

		private void BuildBottomConnection(bool fromRight = false)
		{
			int x;

			if (fromRight)
			{
				x = (int) (DestinationViewModel.Left + Connector.SymbolLineEndsDiff + 5);
			}
			else
			{
				x = (int) (DestinationViewModel.Left + DestinationViewModel.Width - Connector.SymbolLineEndsDiff - 5);
			}

			var point1 = new ConnectionPoint(x, SourceViewModel.Top + SourceViewModel.Height + Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(x, (int) (DestinationViewModel.Top - Connector.ConnectorLenght));

			Points.Add(point1);
			Points.Add(point2);

			SourceConnector.Orientation = ConnectorOrientation.Down;
			DestinationConnector.Orientation = ConnectorOrientation.Up;
		}

		private void BuildLeftConnection(bool fromBottom = false)
		{
			int y;

			if (fromBottom)
			{
				y = (int) (DestinationViewModel.Top + DestinationViewModel.Height - Connector.SymbolLineEndsDiff - 5);
			}
			else
			{
				y = (int) (DestinationViewModel.Top + Connector.SymbolLineEndsDiff + 5);
			}

			var point1 = new ConnectionPoint(SourceViewModel.Left - Connector.ConnectorLenght, y);
			var point2 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + Connector.ConnectorLenght, y);

			Points.Add(point1);
			Points.Add(point2);

			SourceConnector.Orientation = ConnectorOrientation.Left;
			DestinationConnector.Orientation = ConnectorOrientation.Right;
		}

		private void BuildRightConnection(bool fromBottom = false)
		{
			int y;

			if (fromBottom)
			{
				y = (int) (DestinationViewModel.Top + DestinationViewModel.Height - Connector.SymbolLineEndsDiff - 5);
			}
			else
			{
				y = (int) (DestinationViewModel.Top + Connector.SymbolLineEndsDiff + 5);
			}

			var point1 = new ConnectionPoint(SourceViewModel.Left + SourceViewModel.Width + Connector.ConnectorLenght, y);
			var point2 = new ConnectionPoint(DestinationViewModel.Left - Connector.ConnectorLenght, y);

			Points.Add(point1);
			Points.Add(point2);

			SourceConnector.Orientation = ConnectorOrientation.Right;
			DestinationConnector.Orientation = ConnectorOrientation.Left;
		}

		private RelativeTablePosition GetRelativePositionOfDestinationTable()
		{
			var rectangles = new Rectangle[8];
			rectangles[0] = new Rectangle(0, 0, (int) SourceViewModel.Left, (int) SourceViewModel.Top);
			rectangles[1] = new Rectangle((int) SourceViewModel.Left, 0, (int) SourceViewModel.Width, (int) SourceViewModel.Top);
			rectangles[2] = new Rectangle((int) (SourceViewModel.Left + SourceViewModel.Width), 0, (int) (_canvas.ActualWidth - SourceViewModel.Left - SourceViewModel.Width), (int) SourceViewModel.Top);
			rectangles[3] = new Rectangle((int) (SourceViewModel.Left + SourceViewModel.Width), (int) SourceViewModel.Top, (int) (_canvas.ActualWidth - SourceViewModel.Left - SourceViewModel.Width), (int) SourceViewModel.Height);
			rectangles[4] = new Rectangle((int) (SourceViewModel.Left + SourceViewModel.Width), (int) (SourceViewModel.Top + SourceViewModel.Height), (int) (_canvas.ActualWidth - SourceViewModel.Left - SourceViewModel.Width), (int) (_canvas.ActualHeight - SourceViewModel.Height - SourceViewModel.Top));
			rectangles[5] = new Rectangle((int) (SourceViewModel.Left), (int) (SourceViewModel.Top + SourceViewModel.Height), (int) SourceViewModel.Width, (int) (_canvas.ActualWidth - SourceViewModel.Left - SourceViewModel.Width));
			rectangles[6] = new Rectangle(0, (int) (SourceViewModel.Top + SourceViewModel.Height), (int) SourceViewModel.Left, (int) (_canvas.ActualHeight - SourceViewModel.Height - SourceViewModel.Top));
			rectangles[7] = new Rectangle(0, (int) (SourceViewModel.Top), (int) SourceViewModel.Left, (int) (SourceViewModel.Height));

			var points = new Point[4];
			points[0] = new Point((int) DestinationViewModel.Left, (int) (DestinationViewModel.Top));
			points[1] = new Point((int) (DestinationViewModel.Left + DestinationViewModel.Width), (int) (DestinationViewModel.Top));
			points[2] = new Point((int) (DestinationViewModel.Left + DestinationViewModel.Width), (int) (DestinationViewModel.Top + DestinationViewModel.Height));
			points[3] = new Point((int) DestinationViewModel.Left, (int) (DestinationViewModel.Top + DestinationViewModel.Height));

			if (rectangles[0].Contains(points[2]))
			{
				return RelativeTablePosition.LeftTop;
			}

			if (rectangles[1].Contains(points[2]))
			{
				return RelativeTablePosition.TopR;
			}

			if (rectangles[1].Contains(points[3]))
			{
				return RelativeTablePosition.Top;
			}

			if (rectangles[2].Contains(points[3]))
			{
				return RelativeTablePosition.RightTop;
			}

			if (rectangles[3].Contains(points[0]))
			{
				return RelativeTablePosition.Right;
			}

			if (rectangles[3].Contains(points[3]))
			{
				return RelativeTablePosition.RightB;
			}

			if (rectangles[4].Contains(points[0]))
			{
				return RelativeTablePosition.RightBottom;
			}

			if (rectangles[5].Contains(points[0]))
			{
				return RelativeTablePosition.Bottom;
			}

			if (rectangles[5].Contains(points[1]))
			{
				return RelativeTablePosition.BottomR;
			}

			if (rectangles[6].Contains(points[1]))
			{
				return RelativeTablePosition.LeftBottom;
			}

			if (rectangles[7].Contains(points[1]))
			{
				return RelativeTablePosition.Left;
			}

			if (rectangles[7].Contains(points[2]))
			{
				return RelativeTablePosition.LeftB;
			}

			return RelativeTablePosition.LeftTop;
		}

		private void BuildOverlapConnection()
		{
			int off = (int)Connector.ConnectorLenght;

			int sl = (int)SourceViewModel.Left;
			int sr = (int)(SourceViewModel.Left + SourceViewModel.Width);
			int st = (int)SourceViewModel.Top;
			int sb = (int)(SourceViewModel.Top + SourceViewModel.Height);
			int sw = (int)SourceViewModel.Width;
			int sw2 = sw / 2;
			int sh = (int)SourceViewModel.Height;
			int sh2 = sh / 2;

			int dl = (int)DestinationViewModel.Left;
			int dr = (int)(DestinationViewModel.Left + DestinationViewModel.Width);
			int dt = (int)DestinationViewModel.Top;
			int db = (int)(DestinationViewModel.Top + DestinationViewModel.Height);
			int dw = (int)DestinationViewModel.Width;
			int dw2 = dw / 2;
			int dh = (int)DestinationViewModel.Height;
			int dh2 = sh / 2;

			//From bottom
			var point1 = new ConnectionPoint(sl + sw2, sb + off);
			var point2 = new ConnectionPoint(sl + sw2, sb + off + DefaultConnectionLineLength);
			var point3 = new ConnectionPoint(dr + off + DefaultConnectionLineLength, sb + off + DefaultConnectionLineLength);
			var point4 = new ConnectionPoint(dr + off + DefaultConnectionLineLength, dt + dh2);
			var point5 = new ConnectionPoint(dr + off, dt + dh2);

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);
			Points.Add(point4);
			Points.Add(point5);

			SourceConnector.Orientation = ConnectorOrientation.Down;
			DestinationConnector.Orientation = ConnectorOrientation.Right;

			SourceConnector.Cardinality = Cardinality.One;
			DestinationConnector.Cardinality = Cardinality.Many;
			DestinationConnector.Optionality = RelationshipModel.Optionality;

			SourceConnector.EndPoint = point1;
			DestinationConnector.EndPoint = point5;

			IsSourceConnectorAtStartPoint = true;

			BuildLinesFromPoints();
		}

		private bool AreTablesOverlaping()
		{
			if (SourceViewModel.Left < DestinationViewModel.Left + DestinationViewModel.Width && SourceViewModel.Left + SourceViewModel.Width + Connector.ConnectorLenght > DestinationViewModel.Left && SourceViewModel.Top < DestinationViewModel.Top + DestinationViewModel.Height && SourceViewModel.Height + SourceViewModel.Top > DestinationViewModel.Top)
			{
				return true;
			}

			return false;
		}

		private void BuildSelfConnection()
		{
			var point1 = new ConnectionPoint(SourceViewModel.Left + (SourceViewModel.Width/2), SourceViewModel.Top - Connector.ConnectorLenght);
			var point2 = new ConnectionPoint(SourceViewModel.Left + (SourceViewModel.Width/2), SourceViewModel.Top - Connector.ConnectorLenght - DefaultConnectionLineLength);
			var point3 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + DefaultConnectionLineLength + Connector.ConnectorLenght, SourceViewModel.Top - Connector.ConnectorLenght - DefaultConnectionLineLength);
			var point4 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + DefaultConnectionLineLength + Connector.ConnectorLenght, DestinationViewModel.Top + (DestinationViewModel.Height/2));
			var point5 = new ConnectionPoint(DestinationViewModel.Left + DestinationViewModel.Width + Connector.ConnectorLenght, DestinationViewModel.Top + (DestinationViewModel.Height/2));

			Points.Add(point1);
			Points.Add(point2);
			Points.Add(point3);
			Points.Add(point4);
			Points.Add(point5);

			SourceConnector.Orientation = ConnectorOrientation.Up;
			DestinationConnector.Orientation = ConnectorOrientation.Right;

			SourceConnector.Cardinality = Cardinality.One;
			DestinationConnector.Cardinality = Cardinality.Many;
			DestinationConnector.Optionality = RelationshipModel.Optionality;

			SourceConnector.EndPoint = point1;
			DestinationConnector.EndPoint = point5;

			IsSourceConnectorAtStartPoint = true;

			BuildLinesFromPoints();
		}

		public void ClearAll()
		{
			ClearLines();
			ClearPoints();
			ClearMarks();
		}

		public async Task RebuildVisual(DatabaseModelDesignerViewModel vm)
		{
			ClearAll();
			await BuildConnection3(vm);
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