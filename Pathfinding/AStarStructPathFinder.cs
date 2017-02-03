using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Pathfinding.DataStructures;
using Pathfinding.Structure;

namespace Pathfinding
{
	public class AStarStructPathFinder
	{
		private readonly int _width;
		private readonly int _height;
		private NodeStruct[] _grid;
		private readonly PriorityQueue<NodeStruct> _queue;

		public AStarStructPathFinder(byte[,] grid, int width, int height)
		{
			_width = width;
			_height = height;
			int newWidth = PathFinderHelper.RoundToNearestPowerOfTwo(width);
			int newHeight = PathFinderHelper.RoundToNearestPowerOfTwo(height);

			_grid = new NodeStruct[newWidth*newHeight];
			_queue = new PriorityQueue<NodeStruct>();

			for (int i = 0; i < _width; i++)
			{
				_grid[GetPosition(i, _width)].State = NodeState.Obstacle;
				_grid[GetPosition(_width, i)].State = NodeState.Obstacle;
			}


		}

		private int GetPosition(int x, int y) => x + y * _width;

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

		public Point[] FindPath(Point startPoint, Point endPoint)
		{
			short x = (short)startPoint.X, y = (short)startPoint.Y;

			int pos = x + y * _width;
			_grid[pos].ParentX = -1;
			_grid[pos].ParentY = -1;
			_grid[pos].State = NodeState.Open;
			_grid[pos].X = x;
			_grid[pos].Y = y;
			_grid[pos].G = 0;
			_grid[pos].H = 0;

			_queue.Enqueue(_grid[pos]);

			var buffer = new List<int>();
			short ng;
			int neighPos;

			while(_queue.Count > 0)
			{
				var node = _queue.Dequeue();
				node.State = NodeState.Close;

				if(node.X == endPoint.X && node.Y == endPoint.Y)
				{
					return BacktrackNodes(node);
				}

				buffer.Clear();

				neighPos = node.X + 1 + node.Y * _width;
				if (neighPos >= 0 && neighPos < _width*_height)
				{
					_grid[neighPos].X = (short) (node.X + 1);
					_grid[neighPos].Y = (short) (node.Y);
					buffer.Add(neighPos);
				}

				neighPos = node.X - 1 + node.Y * _width;
				if(neighPos >= 0 && neighPos < _width * _height)
				{
					_grid[neighPos].X = (short)(node.X - 1);
					_grid[neighPos].Y = (short)(node.Y);
					buffer.Add(neighPos);
				}

				neighPos = node.X + (node.Y-1) * _width;
				if(neighPos >= 0 && neighPos < _width * _height)
				{
					_grid[neighPos].X = (short)(node.X);
					_grid[neighPos].Y = (short)(node.Y-1);
					buffer.Add(neighPos);
				}

				neighPos = node.X + (node.Y+1) * _width;
				if(neighPos >= 0 && neighPos < _width * _height)
				{
					_grid[neighPos].X = (short)(node.X);
					_grid[neighPos].Y = (short)(node.Y+1);
					buffer.Add(neighPos);
				}

				foreach(int position in buffer)
				{
					if(_grid[position].State == NodeState.Obstacle || _grid[position].State == NodeState.Close)
					{
						continue;
					}

					ng = (short)(node.G + 1);

					if(_grid[position].State == NodeState.Free || ng < _grid[position].G)
					{
						_grid[position].G = ng;
						_grid[position].H = (short)(_grid[position].H > 0 ? _grid[position].H : Manhattan(_grid[position], endPoint) * 10);
						_grid[position].F = (short)(_grid[position].G + _grid[position].H);
						_grid[position].ParentX = node.X;
						_grid[position].ParentY = node.Y;

						if(_grid[position].State != NodeState.Open)
						{
							_grid[position].State = NodeState.Open;
							_queue.Enqueue(_grid[position]);
							continue;
						}

						_queue.Update(_grid[position]);
					}
				}
			}
			return null;
		}

		protected short Manhattan(NodeStruct point1, Point point2)
		{
			return (short)(Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));
		}

		protected Point[] BacktrackNodes(NodeStruct start)
		{
			var res = new List<Point>();
			var n = start;
			res.Add(new Point(n.X, n.Y));

			while(n.ParentX != -1)
			{
				n = _grid[n.ParentX + n.ParentY * _width];
				res.Add(new Point(n.X, n.Y));
			}

			return res.ToArray();
		}
	}
}