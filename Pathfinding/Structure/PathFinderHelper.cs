using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace Pathfinding.Structure
{
	/// <summary>
	/// Helper methods for preprocessing data for pathfinding
	/// </summary>
	public static class PathFinderHelper
	{
		/// <summary>
		/// Round to nearest power of two
		/// </summary>
		/// <param name="n">Number for round</param>
		/// <returns>Nearest power of two</returns>
		public static int RoundToNearestPowerOfTwo(int n) => (int)Math.Pow(2, Math.Ceiling(Math.Log(n) / Math.Log(2)));

		/// <summary>
		/// Create grid of given dimensions
		/// </summary>
		/// <param name="width">Count of columns</param>
		/// <param name="height">Count of rows</param>
		/// <returns>Grid of given dimensions</returns>
		public static Grid CreateNewGrid(int width, int height)
		{
			Grid grid = new Grid(width, width);
			return grid;
		}

		/// <summary>
		/// Update state of nodes in grid
		/// </summary>
		/// <param name="width">Count of columns</param>
		/// <param name="height">Count of rows</param>
		/// <param name="obstacles">Filled rectangle areas</param>
		/// <param name="grid">Existing grid</param>
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

		/// <summary>
		/// Create new grid
		/// </summary>
		/// <param name="width">Count of columns</param>
		/// <param name="height">Count of rows</param>
		/// <param name="obstacles">Filled rectangle areas</param>
		/// <returns></returns>
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

		/// <summary>
		/// Create grid for struct represented nodes
		/// </summary>
		/// <param name="width">Count of columns</param>
		/// <param name="height">Count of rows</param>
		/// <param name="obstacles">Filled rectangle areas</param>
		/// <returns>Byte array of free/closed nodes</returns>
		/// <remarks>DEPRECATED</remarks>
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