using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Pathfinding.Structure
{
	public static class PathFinderHelper
	{
		public static int RoundToNearestPowerOfTwo(int n) => (int)Math.Pow(2, Math.Ceiling(Math.Log(n) / Math.Log(2)));

		public static Grid CreateNewGrid(int width, int height)
		{
			int newWidth = RoundToNearestPowerOfTwo(width);
			int newHeight = RoundToNearestPowerOfTwo(height);
			Grid grid = new Grid(newWidth, newHeight);
			return grid;
		}

		public static void UpdateGrid(int width, int height, IEnumerable<Rectangle> obstacles, Grid grid)
		{
			grid.Width = width;
			grid.Height = height;

			Parallel.For(0, height * width, i =>
			{
				grid.InnerGrid[i].State = NodeState.Free;
			});

			for(int i = 0; i < grid.Width; i++)
			{
				grid[i, height].State = NodeState.Obstacle;
				grid[width, i].State = NodeState.Obstacle;
			}

			foreach(Rectangle rect in obstacles)
			{
				for(int i = rect.Left; i < rect.Right; i++)
				{
					for(int j = rect.Top; j < rect.Bottom; j++)
					{
						grid[i, j].State = NodeState.Obstacle;

					}
				}
			}
		}

		public static Grid CreateGrid(int width, int height, IEnumerable<Rectangle> obstacles)
		{
			Grid grid = CreateNewGrid(width, height);

			for(int i = 0; i < grid.Width; i++)
			{
				grid[i, height].State = NodeState.Obstacle;
				grid[width, i].State = NodeState.Obstacle;
			}

			foreach(Rectangle rect in obstacles)
			{
				for(int i = rect.Left; i < rect.Right; i++)
				{
					for(int j = rect.Top; j < rect.Bottom; j++)
					{
						grid[i, j].State = NodeState.Obstacle;

					}
				}
			}

			return grid;
		}
	}
}