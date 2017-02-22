using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Pathfinding.Structure;

namespace Pathfinding
{
	/// <summary>
	/// Base class for pathfinding algorithms
	/// </summary>
	public abstract class AbstractPathFinder: IPathFinder
	{
		/// <summary>
		/// Graph as a grid
		/// </summary>
		public Grid Grid { get; set; }

		protected AbstractPathFinder(Grid grid)
		{
			Grid = grid;
		}

		/// <summary>
		/// Backtrack path from final node to start
		/// </summary>
		/// <param name="start">Final node</param>
		/// <returns>Path from end to start</returns>
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

		/// <summary>
		/// Find only filtered bending points from full path
		/// </summary>
		/// <param name="startPoint">Path start</param>
		/// <param name="endPoint">Path end</param>
		/// <returns>Filtered bending points</returns>
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

		/// <summary>
		/// Is endpoint reached
		/// </summary>
		/// <param name="current">Current node</param>
		/// <param name="endPoint">End point</param>
		/// <returns>True if reached, false otherwise</returns>
		protected bool EndpointReached(Node current, Point endPoint) => current.X == endPoint.X && current.Y == endPoint.Y;

		/// <summary>
		/// Heuristic function
		/// </summary>
		/// <param name="point1">Current node</param>
		/// <param name="point2">End point</param>
		/// <returns>Result of heuristic</returns>
		protected short Manhattan(Node point1, Point point2) => (short) (Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));

		/// <summary>
		/// Get neighbor nodes
		/// </summary>
		/// <param name="node">Current node</param>
		/// <returns>Array of 4 nodes</returns>
		protected Node[] GetNeighbors(Node node)
		{
			var res = new Node[4];

			res[0] = Grid[(short)(node.X - 1), node.Y];
			res[1] = Grid[(short)(node.X + 1), node.Y];
			res[2] = Grid[node.X, (short)(node.Y - 1)];
			res[3] = Grid[node.X, (short)(node.Y + 1)];

			return res;
		}

		/// <summary>
		/// Find path between points
		/// </summary>
		/// <param name="startPoint">Path start</param>
		/// <param name="endPoint">Path end</param>
		/// <returns>All points in path</returns>
		public abstract Point[] FindPath(Point startPoint, Point endPoint);
	}
}