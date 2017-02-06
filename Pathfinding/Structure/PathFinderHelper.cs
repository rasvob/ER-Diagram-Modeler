using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Pathfinding.Structure
{
	public static class PathFinderHelper
	{
		public static int RoundToNearestPowerOfTwo(int n) => (int)Math.Pow(2, Math.Ceiling(Math.Log(n) / Math.Log(2)));

		public static Grid CreateNewGrid(int width, int height)
		{
			Grid grid = new Grid(width, width);
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
			}

			for (int i = 0; i < grid.Height; i++)
			{
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
				grid[i, height-1].State = NodeState.Obstacle;
				grid[i, 0].State = NodeState.Obstacle;
			}

			for(int i = 0; i < grid.Height; i++)
			{
				grid[width-1, i].State = NodeState.Obstacle;
				grid[0, i].State = NodeState.Obstacle;
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

		public static byte[,] CreateGridStruct(int width, int height, IEnumerable<Rectangle> obstacles)
		{
			var res = new byte[height, width];

			for(int i = 0; i < width; i++)
			{
				res[height - 1, i] = 1;
			}

			for(int i = 0; i < height; i++)
			{
				res[i, width - 1] = 1;
			}

			foreach(Rectangle rect in obstacles)
			{
				for(int i = rect.Left; i < (rect.Right > width ? width : rect.Right); i++)
				{
					for(int j = rect.Top; j < (rect.Bottom > height ? height : rect.Bottom); j++)
					{
						try
						{
							res[j, i] = 1;
						}
						catch (Exception exception)
						{
							Trace.WriteLine(exception.Message);
						}
					}
				}
			}

			return res;
		}
	}
}