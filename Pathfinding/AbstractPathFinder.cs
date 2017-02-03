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
			res.Add(new Point(n.X, n.Y));

			while(n.ParentX != -1)
			{
				n = Grid[n.ParentX, n.ParentY];
				res.Add(new Point(n.X, n.Y));
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

		protected bool EndpointReached(Node current, Point endPoint) => current.X == endPoint.X && current.Y == endPoint.Y;

		protected short Manhattan(Node point1, Point point2) => (short) (Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));

		protected Node[] GetNeighbors(Node node)
		{
			var res = new Node[4];

			res[0] = Grid[(short)(node.X - 1), node.Y];
			res[1] = Grid[(short)(node.X + 1), node.Y];
			res[2] = Grid[node.X, (short)(node.Y - 1)];
			res[3] = Grid[node.X, (short)(node.Y + 1)];

			return res;
		}

		public abstract Point[] FindPath(Point startPoint, Point endPoint);
	}
}