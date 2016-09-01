using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class ConnectionInfo
	{
		public RelationshipModel RelationshipModel { get; set; }
		public ObservableCollection<ConnectionLine> Lines { get; } = new ObservableCollection<ConnectionLine>();
		public ObservableCollection<ConnectionPoint> Points { get; } = new ObservableCollection<ConnectionPoint>();

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

				Lines.Add(line);
			}
		}

		public void BuildPointsFromLines()
		{
			if (Lines.Count < 1)
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

		private void ClearPoints()
		{
			for (int i = 0; i < Points.Count; i++)
			{
				Points.RemoveAt(i);
			}
			Points.Clear();
		}

		private void ClearLines()
		{
			for(int i = 0; i < Lines.Count; i++)
			{
				Lines.RemoveAt(i);
			}
			Lines.Clear();
		}

		public void SplitLine(ConnectionLine line)
		{
			
		}

		public void SplitLine(ConnectionLine line, Point point)
		{

		}
	}
}