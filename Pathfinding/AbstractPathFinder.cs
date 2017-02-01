using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Pathfinding.Structure;

namespace Pathfinding
{
	public abstract class AbstractPathFinder
	{
		public Grid Grid { get; set; }

		protected AbstractPathFinder(Grid grid)
		{
			Grid = grid;
		}

		protected Point[] BacktrackNodes(Node start)
		{
			var res = new List<Point>();
			var n = start;
			res.Add(n.Location);

			while(n.Parent.HasValue)
			{
				n = Grid[n.Parent.Value.X, n.Parent.Value.Y];
				res.Add(n.Location);
			}

			return res.ToArray();
		}

		public Point[] FindPathBendingPointsOnly(Point startPoint, Point endPoint)
		{
			var points = FindPath(startPoint, endPoint);

			if(points == null)
			{
				return null;
			}

			var res = new List<Point> { points[0] };

			for(var i = 1; i < points.Length - 1; i++)
			{
				var prev = points[i - 1];
				var curr = points[i];
				var next = points[i + 1];

				if(curr.Y == prev.Y && curr.Y == next.Y)
				{
					continue;
				}

				if(curr.X == prev.X && curr.X == next.X)
				{
					continue;
				}

				res.Add(curr);
			}

			res.Add(points[points.Length - 1]);
			return res.ToArray();
		}

		public string[] CreateDebugGrid(Point[] path)
		{
			var res = new List<string>();

			if(path != null)
			{
				foreach(Point p in path)
				{
					Grid[p.X, p.Y].InitState = NodeState.Open;
				}
			}

			for(int i = 0; i < Grid.Height; i++)
			{
				var str = new StringBuilder();
				for(int j = 0; j < Grid.Width; j++)
				{
					switch(Grid[j, i].InitState)
					{
						case NodeState.Free:
							str.Append("-");
							break;
						case NodeState.Obstacle:
							str.Append("X");
							break;
						case NodeState.Open:
							str.Append("O");
							break;
					}
				}
				res.Add(str.ToString());
			}

			return res.ToArray();
		}

		protected int Manhattan(Point point1, Point point2)
		{
			return (Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));
		}

		protected Node[] GetNeighbors(Node node)
		{
			var res = new Node[4];

			res[0] = Grid[node.Location.X - 1, node.Location.Y];
			res[1] = Grid[node.Location.X + 1, node.Location.Y];
			res[2] = Grid[node.Location.X, node.Location.Y - 1];
			res[3] = Grid[node.Location.X, node.Location.Y + 1];

			return res;
		}

		public abstract Point[] FindPath(Point startPoint, Point endPoint);
	}
}