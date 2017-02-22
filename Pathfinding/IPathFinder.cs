using System.Collections.Generic;
using System.Drawing;
using Pathfinding.Structure;

namespace Pathfinding
{
	/// <summary>
	/// Interface for pathfinding algorithms
	/// </summary>
	public interface IPathFinder
	{
		/// <summary>
		/// Find path between points
		/// </summary>
		/// <param name="startPoint">Path start</param>
		/// <param name="endPoint">Path end</param>
		/// <returns>All points in path</returns>
		Point[] FindPath(Point startPoint, Point endPoint);

		/// <summary>
		/// Find only filtered bending points from full path
		/// </summary>
		/// <param name="startPoint">Path start</param>
		/// <param name="endPoint">Path end</param>
		/// <returns>Filtered bending points</returns>
		Point[] FindPathBendingPointsOnly(Point startPoint, Point endPoint);
	}
}