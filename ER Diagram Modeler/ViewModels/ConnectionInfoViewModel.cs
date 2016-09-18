using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Canvas.Connection;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace ER_Diagram_Modeler.ViewModels
{
	public class ConnectionInfoViewModel
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
		private DesignerCanvas _canvas;

		public event EventHandler<bool> SelectionChange;
		public event EventHandler<Connector> ConnectorChange;

		public static readonly double PointsDistaceTolerance = 4.0;

		public TableViewModel SourceViewModel
		{
			get { return _sourceViewModel; }
			set
			{
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
				if (table.Equals(SourceViewModel))
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

			if (table != null && (IsSelfConnection() && table.IsMoving))
			{
				return;
			}

			if (Lines.Count == 1)
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

			if(table != null && (IsSelfConnection() && table.IsMoving))
			{
				MoveAllLines(e);
				return;
			}

			if(Lines.Count == 1)
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

			if (Lines.IndexOf(line) == 1)
			{
				if (EnsureBoundsForSecondLine(line)) return;
			}

			if (Lines.IndexOf(line) == Lines.Count - 2)
			{
				if(EnsureBoundsForLastButOneLine(line)) return;
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
			switch(line.Orientation)
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
					if(line.StartPoint.Y <= 0)
					{
						line.StartPoint.Y = 1;
						line.EndPoint.Y = 1;
						return true;
					}
					if(line.StartPoint.Y >= _canvas.Height)
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
			var forSelection = Points.Skip(1).Take(Points.Count - 2);
			var duplicate = forSelection.GroupBy(t => t).Where(s => s.Count() > 1).Select(u => u.Key);
			var forRemove = Points.Where(t => duplicate.Any(s => s.Equals(t))).Where(s => !_newBendPoints.Any(r => r.Equals(s))).ToList();

			foreach (ConnectionPoint connectionPoint in forRemove)
			{
				Points.Remove(connectionPoint);
			}
		}

		private void RemoveBendPointsWithTolerance()
		{
			if (Points.Count < 4)
			{
				return;
			}

			for (int i = 1; i < Points.Count - 1; i++)
			{
				var point = Points[i];
				var nextPoint = Points[i + 1];
			}
		}

		public void SynchronizeBendingPoints()
		{
			BuildPointsFromLines();
			RemoveRedundandBendPoints();
			RemoveBendPointsWithTolerance();
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

		private void MergeLines(ConnectionLine line1, ConnectionLine line2)
		{
			//TODO: Unfinished
			line1.EndPoint.X = line2.EndPoint.X;
			line1.EndPoint.Y = line2.EndPoint.Y;
			Lines.Remove(line2);
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