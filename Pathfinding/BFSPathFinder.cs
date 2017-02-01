using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Pathfinding.DataStructures;
using Pathfinding.Structure;

namespace Pathfinding
{
	public class BfsPathFinder: AbstractPathFinder
	{
		public BfsPathFinder(Grid grid): base(grid){ }

		public override Point[] FindPath(Point startPoint, Point endPoint)
		{
			//var q = new Queue<Node>();
			var q = new PriorityQueue<Node>();
			int x = startPoint.X, y = startPoint.Y;
			Grid[x, y].Parent = null;
			Grid[x, y].State = NodeState.Open;
			Grid[x, y].Location = new Point(x, y);
			q.Enqueue(Grid[x, y]);
			var buffer = new Node[4];

			while (q.Count > 0)
			{
				var node = q.Dequeue();
				node.State = NodeState.Close;

				if (node.Location.X == endPoint.X && node.Location.Y == endPoint.Y)
				{
					return BacktrackNodes(node);
				}

				buffer = GetNeighbors(node);

				//buffer[0] = Grid[node.Location.X - 1, node.Location.Y];
				//buffer[1] = Grid[node.Location.X + 1, node.Location.Y];
				//buffer[2] = Grid[node.Location.X, node.Location.Y - 1];
				//buffer[3] = Grid[node.Location.X, node.Location.Y + 1];

				foreach (Node buffered in buffer)
				{
					if (buffered.State != NodeState.Free)
					{
						continue;
					}

					buffered.Parent = new Point(node.Location.X, node.Location.Y);
					buffered.State = NodeState.Open;
					buffered.F = Manhattan(buffered.Location, endPoint);
					q.Enqueue(buffered);
				}

				//var nodes = buffer.Where(t => t.State == NodeState.Free).ToArray();
				//foreach (Node buffered in nodes)
				//{
				//	buffered.Parent = new Point(node.Location.X, node.Location.Y);
				//	buffered.State = NodeState.Open;
				//	buffered.F = Manhattan(buffered.Location, endPoint);
				//}

				//Array.Sort(nodes);

				//foreach (Node n in nodes)
				//{
				//	q.Enqueue(n);
				//}

			}

			return null;
		}
	}
}