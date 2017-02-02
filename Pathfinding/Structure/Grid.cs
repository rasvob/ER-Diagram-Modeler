using System.Threading.Tasks;

namespace Pathfinding.Structure
{
	public class Grid
	{
		public int Height { get; set; }
		public int Width { get; set; }
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
				InnerGrid[x + y * Width].X = (short) x;
				InnerGrid[x + y * Width].Y = (short) y;
				return InnerGrid[x + y * Width];
			}
		}

	}
}