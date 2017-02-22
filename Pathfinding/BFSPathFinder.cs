using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Pathfinding.DataStructures;
using Pathfinding.Structure;

namespace Pathfinding
{
	/// <summary>
	/// BFS Pathfinding algorithm
	/// </summary>
	public class BfsPathFinder: AbstractPathFinder
	{
		public BfsPathFinder(Grid grid): base(grid){ }

		/// <summary>
		/// Find path between points
		/// </summary>
		/// <param name="startPoint">Path start</param>
		/// <param name="endPoint">Path end</param>
		/// <returns>All points in path</returns>
		public override Point[] FindPath(Point startPoint, Point endPoint)
		{
			var q = new PriorityQueue<Node>();
			int x = startPoint.X, y = startPoint.Y;
			Grid[x, y].ParentX = -1;
			Grid[x, y].ParentY = -1;
			Grid[x, y].State = NodeState.Open;
			Grid[x, y].X = (short) x;
			Grid[x, y].Y = (short) y;

			q.Enqueue(Grid[x, y]);

			while(q.Count > 0)
			{
				var node = q.Dequeue();
				node.State = NodeState.Close;

				if(EndpointReached(node, endPoint))
				{
					return BacktrackNodes(node);
				}

				var buffer = GetNeighbors(node);

				foreach(Node buffered in buffer)
				{
					if(buffered.State != NodeState.Free)
					{
						continue;
					}

					buffered.ParentX = node.X;
					buffered.ParentY = node.Y;
					buffered.State = NodeState.Open;
					buffered.F = Manhattan(buffered, endPoint);
					q.Enqueue(buffered);
				}
			}

			return null;
		}
	}
}