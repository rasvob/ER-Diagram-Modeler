using System.Drawing;
using Pathfinding.Structure;

namespace Pathfinding
{
	public class AStarPathFinder: AbstractPathFinder
	{
		public AStarPathFinder(Grid grid) : base(grid)
		{
		}

		public override Point[] FindPath(Point startPoint, Point endPoint)
		{
			throw new System.NotImplementedException();
		}
	}
}