using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Pathfinding.Structure
{
	public class Grid
	{
		public int Height { get; set; }
		public int Width { get; set; }
		public Node[] InnerGrid { get; }

		public Grid(int width, int height)
		{
			InnerGrid = new Node[width*height];
			Width = width;
			Height = height;
		}

		public Node this[int x, int y]
		{
			set { InnerGrid[x + y * Width] = value; }

			get
			{
				if (x < 0 || y < 0 || x >= Width || y >= Height)
				{
					return new Node()
					{
						State = NodeState.Obstacle
					};
				}
				InnerGrid[x + y * Width].Location = new Point(x, y);
				return InnerGrid[x + y * Width];
			}
		}
	}
}