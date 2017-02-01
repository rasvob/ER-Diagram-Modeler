using System.Collections.Generic;
using System.Drawing;
using Pathfinding.Structure;

namespace Pathfinding
{
	public interface IPathFinder
	{
		Point[] FindPath(Point startPoint, Point endPoint);
		Point[] FindPathBendingPointsOnly(Point startPoint, Point endPoint);
	}
}