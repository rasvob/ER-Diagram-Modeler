using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pathfinding.DataStructures;
using Pathfinding.Structure;

namespace Pathfinding
{
	public class FreeRectangleFinder
	{
		private readonly Grid _grid;

		public FreeRectangleFinder(Grid grid)
		{
			_grid = grid;
		}

		public Point? FindFreeRectangle(Rectangle rect)
		{
			PreprocessGrid();
			var nodes = GetListOfFreePoints(rect);

			if (!nodes.Any())
			{
				return null;
			}

			var center = new Point(_grid.Width/2, _grid.Height/2);
			var avg = (int)nodes.Select(t => Manhattan(t, center)).Average(t => t);
			var res = nodes.FirstOrDefault(t => Manhattan(t, center) <= avg);

			if (res != null) 
				return new Point(res.X, res.Y);

			return null;
		}

		private Node[] GetNeighbors(Node node)
		{
			var res = new Node[8];

			res[0] = _grid[(short)(node.X - 1), node.Y];
			res[1] = _grid[(short)(node.X + 1), node.Y];
			res[2] = _grid[node.X, (short)(node.Y - 1)];
			res[3] = _grid[node.X, (short)(node.Y + 1)];
			res[4] = _grid[(short)(node.X - 1), (short)(node.Y - 1)];
			res[5] = _grid[(short)(node.X + 1), (short)(node.Y + 1)];
			res[6] = _grid[(short)(node.X - 1), (short)(node.Y + 1)];
			res[7] = _grid[(short)(node.X + 1), (short)(node.Y - 1)];

			return res;
		}

		private List<Node> GetListOfFreePoints(Rectangle rectangle)
		{
			var nodes = new List<Node>();
			var width = rectangle.Width + 2;
			var minimum = rectangle.Height + 2;

			for(int i = 0; i < _grid.Height; i++)
			{
				for(int j = 0; j < _grid.Width; j++)
				{
					var line = GetLineOfPoints(_grid[j, i], width);

					if(line.All(t => t.State == NodeState.Free) && line.All(t => t.F >= minimum))
					{
						nodes.Add(_grid[j, i]);
					}
				}
			}
			return nodes;
		}

		private List<Node> GetListOfFreePointsWithBfs(Rectangle rectangle)
		{
			var nodes = new List<Node>();
			var width = rectangle.Width + 2;
			var minimum = rectangle.Height + 2;

			var q = new Queue<Node>();
			int x = _grid.Width-2, y = 1;
			_grid[x, y].State = NodeState.Open;
			_grid[x, y].X = (short)x;
			_grid[x, y].Y = (short)y;

			q.Enqueue(_grid[x, y]);

			while(q.Count > 0)
			{
				var node = q.Dequeue();
				node.State = NodeState.Close;

				var line = GetLineOfPoints(node, width);

				if(line.All(t => t.State != NodeState.Obstacle) && line.All(t => t.F >= minimum))
				{
					nodes.Add(node);
				}

				var buffer = GetNeighbors(node);

				foreach(Node buffered in buffer)
				{
					if(buffered.State != NodeState.Free)
					{
						continue;
					}

					buffered.State = NodeState.Open;
					q.Enqueue(buffered);
				}
			}

			return nodes;
		}

		private Node[] GetLineOfPoints(Node from, int count)
		{
			var res = new Node[count];

			int pom = 0;
			for (int i = from.X; i < from.X + count; i++)
			{
				res[pom++] = _grid[i, from.Y];
			}

			return res;
		}

		private short Manhattan(Node point1, Point point2) => (short)(Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));
		private short DistanceToCenter(Node point1, Point point2) => (short) (Math.Pow(Math.Abs(point1.X - point2.X), 2) + Math.Pow(Math.Abs(point1.Y - point2.Y), 2));

		private void PreprocessGrid()
		{
			for (int i = 0; i < _grid.Width; i++)
			{
				short counter = 1;
				for (int j = _grid.Height - 1; j >= 0; j--)
				{
					Node node = _grid[i, j];

					if (node.State == NodeState.Obstacle)
					{
						counter = 1;
						continue;
					}

					node.F = counter++;
				}
			}
		}
	}
}