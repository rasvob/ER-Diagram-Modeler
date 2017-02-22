using System.Threading.Tasks;

namespace Pathfinding.Structure
{
	/// <summary>
	/// Graph represented area with obstacles
	/// </summary>
	public class Grid
	{
		/// <summary>
		/// Number of rows
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Number of columns
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Array of nodes
		/// </summary>
		public Node[] InnerGrid { get; }

		public Grid(int width, int height)
		{
			InnerGrid = new Node[width * height];
			Width = width;
			Height = height;
			Parallel.For(0, InnerGrid.Length, i =>
			{
				InnerGrid[i] = new Node();
			});
		}

		/// <summary>
		/// Indexer for getting nodes
		/// </summary>
		/// <param name="x">Column</param>
		/// <param name="y">Row</param>
		/// <returns>Node from position or node in Obstacle state</returns>
		public Node this[int x, int y]
		{
			set { InnerGrid[x + y * Width] = value; }

			get
			{
				if(x < 0 || y < 0 || x >= Width || y >= Height)
				{
					return new Node()
					{
						State = NodeState.Obstacle
					};
				}
				InnerGrid[x + y * Width].X = (short)x;
				InnerGrid[x + y * Width].Y = (short)y;
				return InnerGrid[x + y * Width];
			}
		}
	}
}