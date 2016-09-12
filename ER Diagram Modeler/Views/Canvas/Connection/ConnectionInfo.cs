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
		//TODO: Name only mode connector position
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
			if (SourceViewModel.Equals(DestinationViewModel))
			{
				return;
			}
			var table = sender as TableViewModel;
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

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.LeftDelta;
								}
							}

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.WidthDelta;
								}
							}

						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.X += e.LeftDelta;
								}
							}

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.X += e.WidthDelta;
								}
							}
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

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.LeftDelta;
								}
							}

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.WidthDelta;
								}
							}
						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;
							endLine.EndPoint.Y += e.HeightDelta;

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.X += e.LeftDelta;
								}
							}

							if(SourceConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.X += e.WidthDelta;
								}
							}
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

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.Y += e.TopDelta;
								}
							}

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.Y += e.HeightDelta;
								}
							}
						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.Y += e.TopDelta;
								}
							}

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.Y += e.HeightDelta;
								}
							}
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

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.Y += e.TopDelta;
								}
							}

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.Y += e.HeightDelta;
								}
							}

						}
						else if(SourceConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;
							endLine.EndPoint.X += e.WidthDelta;

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.Y += e.TopDelta;
								}
							}

							if(SourceConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.Y += e.HeightDelta;
								}
							}
						}
						break;
				}
			}
		}

		private void DestinationViewModelOnPositionAndMeasureChanged(object sender, TablePositionAndMeasureEventArgs e)
		{
			//TODO: Self connection movement update
			var table = DestinationViewModel;
			if (DestinationViewModel.Equals(SourceViewModel))
			{
				foreach (ConnectionLine line in Lines)
				{
					line.EndPoint.X += e.WidthDelta;
					line.EndPoint.X += e.LeftDelta;
					line.EndPoint.Y += e.TopDelta;
					line.EndPoint.Y += e.HeightDelta;

					line.StartPoint.X += e.WidthDelta;
					line.StartPoint.X += e.LeftDelta;
					line.StartPoint.Y += e.TopDelta;
					line.StartPoint.Y += e.HeightDelta;
				}
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

							if (DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if (nextLine != null)
								{
									nextLine.StartPoint.X += e.LeftDelta;
								}
							}

							if(DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.WidthDelta;
								}
							}

						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;

							if (DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if (nextLine != null)
								{
									nextLine.EndPoint.X += e.LeftDelta;
								}
							}

							if(DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.X += e.WidthDelta;
								}
							}
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

							if (DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.LeftDelta;
								}
							}

							if (DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.X += e.WidthDelta;
								}
							}
						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.Y += e.TopDelta;
							endLine.EndPoint.Y += e.HeightDelta;

							if (DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff < table.Left + table.Width || e.LeftDelta < 0)
							{
								endLine.StartPoint.X += e.LeftDelta;
								endLine.EndPoint.X += e.LeftDelta;

								if (nextLine != null)
								{
									nextLine.EndPoint.X += e.LeftDelta;
								}
							}

							if(DestinationConnector.EndPoint.X + Connector.SymbolLineEndsDiff >= table.Left + table.Width - 10 && e.WidthDelta <= 0)
							{
								endLine.StartPoint.X += e.WidthDelta;
								endLine.EndPoint.X += e.WidthDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.X += e.WidthDelta;
								}
							}
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

							if(DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if (nextLine != null)
								{
									nextLine.StartPoint.Y += e.TopDelta;
								}
							}

							if(DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.Y += e.HeightDelta;
								}
							}
						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;

							if (DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if (nextLine != null)
								{
									nextLine.EndPoint.Y += e.TopDelta;
								}
							}

							if(DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.Y += e.HeightDelta;
								}
							}
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

							if (DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if (nextLine != null)
								{
									nextLine.StartPoint.Y += e.TopDelta;
								}
							}

							if(DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.StartPoint.Y += e.HeightDelta;
								}
							}

						}
						else if(DestinationConnector.EndPoint.Equals(Lines.LastOrDefault()?.EndPoint))
						{
							endLine = Lines.LastOrDefault();
							int nextLineIdx = Lines.IndexOf(endLine) - 1;
							ConnectionLine nextLine = Lines[nextLineIdx];

							endLine.EndPoint.X += e.LeftDelta;
							endLine.EndPoint.X += e.WidthDelta;

							if (DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff < table.Top + table.Height || e.TopDelta < 0)
							{
								endLine.StartPoint.Y += e.TopDelta;
								endLine.EndPoint.Y += e.TopDelta;

								if (nextLine != null)
								{
									nextLine.EndPoint.Y += e.TopDelta;
								}
							}

							if(DestinationConnector.EndPoint.Y + Connector.SymbolLineEndsDiff >= table.Top + table.Height - 4 && e.HeightDelta <= 0)
							{
								endLine.StartPoint.Y += e.HeightDelta;
								endLine.EndPoint.Y += e.HeightDelta;

								if(nextLine != null)
								{
									nextLine.EndPoint.Y += e.HeightDelta;
								}
							}
						}
						break;
					}
			}
		}

		private void SourceViewModelOnPositionAndMeasureChangesStarted(object sender, System.EventArgs eventArgs)
		{
			if(Lines.Count == 1)
			{
				SplitLine(Lines.FirstOrDefault(), Lines.FirstOrDefault()?.GetMiddlePoint());
				SynchronizeBendingPoints();
			}
		}

		private void SourceViewModelOnPositionAndMeasureChangesCompleted(object sender, System.EventArgs eventArgs)
		{
			_newBendPoints.Clear();
			SynchronizeBendingPoints();
		}

		private void DestinationViewModelOnPositionAndMeasureChangesStarted(object sender, System.EventArgs eventArgs)
		{
			if(Lines.Count == 1)
			{
				SplitLine(Lines.FirstOrDefault(), Lines.FirstOrDefault()?.GetMiddlePoint());
				SynchronizeBendingPoints();
			}
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
			_newBendPoints.Clear();
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

		private void NormalizePoints()
		{
			for (int i = 0; i < Points.Count; i++)
			{
				Points[i].X = (int) Points[i].X;
				Points[i].Y = (int)Points[i].Y;
			}
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

		private void SplitLineInMiddleNonDestructive(ConnectionLine line)
		{
			var endPoint = Points.LastOrDefault();

			ConnectionPoint middle = line.GetMiddlePoint();
			var newLine = new ConnectionLine();

			newLine.StartPoint.X = middle.X;
			newLine.StartPoint.Y = middle.Y;
			newLine.EndPoint.X = line.EndPoint.X;
			newLine.EndPoint.Y = line.EndPoint.Y;
			newLine.Orientation = line.Orientation;

			newLine.BeforeLineMove += LineOnBeforeLineMove;
			newLine.LineMoving += LineOnLineMoving;
			newLine.LineMoved += LineOnLineMoved;
			newLine.LineSplit += LineOnLineSplit;
			newLine.LineSelected += LineOnLineSelected;

			line.EndPoint.X = middle.X;
			line.EndPoint.Y = middle.Y;

			endPoint.X = middle.X;
			endPoint.Y = middle.Y;

			var splitLine = new ConnectionLine();
			splitLine.StartPoint.X = middle.X;
			splitLine.StartPoint.Y = middle.Y;
			splitLine.EndPoint.X = middle.X;
			splitLine.EndPoint.Y = middle.Y;

			splitLine.BeforeLineMove += LineOnBeforeLineMove;
			splitLine.LineMoving += LineOnLineMoving;
			splitLine.LineMoved += LineOnLineMoved;
			splitLine.LineSplit += LineOnLineSplit;
			splitLine.LineSelected += LineOnLineSelected;
			
			ConfigureSplitLine(splitLine);
			
			Lines.Add(splitLine);
			Lines.Add(newLine);
			Points.Add(splitLine.StartPoint);
			Points.Add(newLine.EndPoint);

			//TODO: CONNECTOR ERROR
			if (DestinationConnector.EndPoint.Equals(newLine.EndPoint))
			{
				DestinationConnector.EndPoint = newLine.EndPoint;
				DestinationConnector.EndPoint.X = newLine.EndPoint.X;
				DestinationConnector.EndPoint.Y = newLine.EndPoint.Y;
			}
			else if (DestinationConnector.EndPoint.Equals(line.StartPoint))
			{
				DestinationConnector.EndPoint = line.StartPoint;
				DestinationConnector.EndPoint.X = line.StartPoint.X;
				DestinationConnector.EndPoint.Y = line.StartPoint.Y;
			}
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