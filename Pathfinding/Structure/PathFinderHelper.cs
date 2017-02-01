using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pathfinding.Structure
{
	public static class PathFinderHelper
	{
		public static int RoundToNearestPowerOfTwo(int n) => (int)Math.Pow(2, Math.Ceiling(Math.Log(n) / Math.Log(2)));

		public static Grid CreateGrid(int width, int height, IEnumerable<Rectangle> obstacles)
		{
			int newWidth = RoundToNearestPowerOfTwo(width);
			int newHeight = RoundToNearestPowerOfTwo(height);

			Grid grid = new Grid(newWidth, newHeight);

			for(var i = 0; i < grid.InnerGrid.Length; i++)
			{
				grid.InnerGrid[i] = new Node();
			}

			for(int i = 0; i < grid.Width; i++)
			{
				grid[i, height].State = NodeState.Obstacle;
				grid[i, height].InitState = NodeState.Obstacle;
			}

			for(int i = 0; i < grid.Height; i++)
			{
				grid[width, i].State = NodeState.Obstacle;
				grid[width, i].InitState = NodeState.Obstacle;
			}

			foreach(Rectangle rect in obstacles)
			{
				for(int i = rect.Left; i < rect.Right; i++)
				{
					for(int j = rect.Top; j < rect.Bottom; j++)
					{
						grid[i, j].State = NodeState.Obstacle;
						grid[i, j].InitState = NodeState.Obstacle;

					}
				}
			}

			return grid;
		}
	}
}