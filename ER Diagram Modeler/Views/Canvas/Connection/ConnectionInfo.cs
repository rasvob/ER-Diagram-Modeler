using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionInfo
	{
		public RelationshipModel RelationshipModel { get; set; }
		public ObservableCollection<ConnectionLine> Lines { get; } = new ObservableCollection<ConnectionLine>();
		public ObservableCollection<ConnectionPoint> Points { get; } = new ObservableCollection<ConnectionPoint>();

		private ConnectionLine _moveLine1 = null;
		private ConnectionLine _moveLine2 = null;

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

				line.BeforeLineMove += LineOnBeforeLineMove;
				line.LineMoving += LineOnLineMoving;
				line.LineMoved += LineOnLineMoved;

				Lines.Add(line);
			}
		}

		public void BuildPointsFromLines()
		{
			if(Lines.Count < 1)
			{
				return;
			}

			ClearPoints();
			Points.Add(Lines[0].StartPoint);

			foreach(ConnectionLine line in Lines)
			{
				Points.Add(line.EndPoint);
			}
		}

		private void LineOnBeforeLineMove(object sender, ConnectionLineMovingEventArgs args)
		{
			_moveLine1 = Lines.FirstOrDefault(t => t.EndPoint.Equals(args.OriginalStartPoint));
			_moveLine2 = Lines.FirstOrDefault(t => t.StartPoint.Equals(args.OriginalEndPoint));
		}

		private void LineOnLineMoving(object sender, ConnectionLineMovingEventArgs args)
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
		}

		private void LineOnLineMoved(object sender, System.EventArgs eventArgs)
		{
			var line = sender as ConnectionLine;

			if(_moveLine1 != null)
			{
				_moveLine1.EndPoint.X = line.StartPoint.X;
				_moveLine1.EndPoint.Y = line.StartPoint.Y;
			}

			if(_moveLine2 != null)
			{
				_moveLine2.StartPoint.X = line.EndPoint.X;
				_moveLine2.StartPoint.Y = line.EndPoint.Y;
			}

			_moveLine1 = null;
			_moveLine2 = null;

			foreach (ConnectionLine connectionLine in Lines)
			{
				Trace.WriteLine(connectionLine);
			}

			SynchronizeBendingPoints();

			Trace.WriteLine("---");
			foreach (ConnectionPoint point in Points)
			{
				Trace.WriteLine(point);
			}
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
			for(int i = 0; i < len; i++)
			{
				Lines.RemoveAt(0);
			}
			Lines.Clear();
		}

		private void RemoveRedundandBendPoints()
		{
			Trace.WriteLine("Before");
			foreach (ConnectionPoint point in Points)
			{
				Trace.WriteLine(point);
			}
			Trace.WriteLine("before");


			var duplicate = Points.GroupBy(t => t).Where(s => s.Count() > 1).Select(u => u.Key);
			var forRemove = Points.Where(t => duplicate.Any(s => s.Equals(t))).ToList();

			if (forRemove.Contains(Points[0]))
			{
				var item = forRemove.FirstOrDefault(t => t.Equals(Points[0]));
				forRemove.Remove(item);
			}

			if(forRemove.Contains(Points[Points.Count - 1]))
			{
				var item = forRemove.FirstOrDefault(t => t.Equals(Points[Points.Count - 1]));
				forRemove.Remove(item);
			}

			foreach (ConnectionPoint connectionPoint in forRemove)
			{
				Points.Remove(connectionPoint);
			}
		}

		private void SynchronizeBendingPoints()
		{
			BuildPointsFromLines();
			RemoveRedundandBendPoints();
			BuildLinesFromPoints();
		}

		public void SplitLine(ConnectionLine line)
		{
			
		}

		public void SplitLine(ConnectionLine line, Point point)
		{

		}
	}
}