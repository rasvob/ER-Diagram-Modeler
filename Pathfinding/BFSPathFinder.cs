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
			//var q = new PriorityQueue<Node>();
			//int x = startPoint.X, y = startPoint.Y;
			////Grid[x, y].Parent = null;
			//Grid[x, y].ParentX = -1;
			//Grid[x, y].ParentY = -1;
			//Grid[x, y].State = NodeState.Open;
			////Grid[x, y].Location = new Point(x, y);
			//Grid[x, y].X = x;
			//Grid[x, y].Y = y;

			//q.Enqueue(Grid[x, y]);

			//while (q.Count > 0)
			//{
			//	var node = q.Dequeue();
			//	node.State = NodeState.Close;

			//	if (EndpointReached(node.Location, endPoint))
			//	{
			//		return BacktrackNodes(node);
			//	}

			//	var buffer = GetNeighbors(node);

			//	foreach (Node buffered in buffer)
			//	{
			//		if (buffered.State != NodeState.Free)
			//		{
			//			continue;
			//		}

			//		buffered.Parent = new Point(node.Location.X, node.Location.Y);
			//		buffered.State = NodeState.Open;
			//		buffered.F = Manhattan(buffered.Location, endPoint);
			//		q.Enqueue(buffered);
			//	}
			//}

			return null;
		}
	}
}