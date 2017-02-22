using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Pathfinding.Structure
{
	/// <summary>
	/// Represent one node in graph
	/// </summary>
	public class Node : IComparable<Node>
	{
		/// <summary>
		/// State of node
		/// </summary>
		public NodeState State { get; set; }

		/// <summary>
		/// X coordinate of parent node
		/// </summary>
		public short ParentX { get; set; }

		/// <summary>
		/// Y coordinate of parent node
		/// </summary>
		public short ParentY { get; set; }

		/// <summary>
		/// X coordinate
		/// </summary>
		public short X { get; set; }

		/// <summary>
		/// Y coordinate
		/// </summary>
		public short Y { get; set; }

		/// <summary>
		/// Sum of G and H (Cost)
		/// </summary>
		/// <remarks>Priority in queue</remarks>
		public short F { get; set; }

		/// <summary>
		/// Cost of path from parent
		/// </summary>
		public short G { get; set; }

		/// <summary>
		/// Heuristic value
		/// </summary>
		public short H { get; set; }

		/// <summary>
		/// Compare nodes by F (Cost)
		/// </summary>
		/// <param name="other">Other node</param>
		/// <returns>Minus one if other.F is greater, one if smaller and zero if equal</returns>
		public int CompareTo(Node other)
		{
			if(F > other.F)
				return 1;
			if(F < other.F)
				return -1;
			return 0;
		}
	}
}