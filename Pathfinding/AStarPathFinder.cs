using System.Drawing;
using Pathfinding.DataStructures;
using Pathfinding.Structure;

namespace Pathfinding
{
	/// <summary>
	/// A* Pathfinding algorithm
	/// </summary>
	public class AStarPathFinder: AbstractPathFinder
	{
		/// <summary>
		/// Weight of heuristic
		/// </summary>
		public static int Weight = 100;

		public AStarPathFinder(Grid grid) : base(grid)
		{

		}

		/// <summary>
		/// Find path between points
		/// </summary>
		/// <param name="startPoint">Path start</param>
		/// <param name="endPoint">Path end</param>
		/// <returns>All points in path</returns>
		public override Point[] FindPath(Point startPoint, Point endPoint)
		{
			var q = new PriorityQueue<Node>();
			short x = (short) startPoint.X, y = (short) startPoint.Y;

			Grid[x, y].ParentX = -1;
			Grid[x, y].ParentY = -1;
			Grid[x, y].State = NodeState.Open;
			Grid[x, y].X = x;
			Grid[x, y].Y = y;
			Grid[x, y].G = 0;
			Grid[x, y].H = 0;

			q.Enqueue(Grid[x, y]);

			while (q.Count > 0)
			{
				var node = q.Dequeue();
				node.State = NodeState.Close;

				if (EndpointReached(node, endPoint))
				{
					return BacktrackNodes(node);
				}

				var buffer = GetNeighbors(node);

				foreach (Node buffered in buffer)
				{
					if (buffered.State == NodeState.Obstacle || buffered.State == NodeState.Close)
					{
						continue;
					}

					var ng = (short) (node.G + 1);

					if (buffered.State == NodeState.Free || ng < buffered.G)
					{
						buffered.G = ng;
						buffered.H = (short) (buffered.H > 0 ? buffered.H : Manhattan(buffered, endPoint)*Weight);
						buffered.F = (short) (buffered.G + buffered.H);
						buffered.ParentX = node.X;
						buffered.ParentY = node.Y;

						if (buffered.State != NodeState.Open)
						{
							buffered.State = NodeState.Open;
							q.Enqueue(buffered);
							continue;
						}
						
						q.Update(buffered);
					}
				}
			}
			return null;
		}
	}
}