using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Pathfinding.Structure
{
	public class Node : IComparable<Node>
	{
		public Point? Parent { get; set; } = null;
		public NodeState State { get; set; } = NodeState.Free;
		public NodeState InitState { get; set; } = NodeState.Free;
		public Point Location { get; set; }
		public int F { get; set; }
		public int G { get; set; }
		public int H { get; set; }

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