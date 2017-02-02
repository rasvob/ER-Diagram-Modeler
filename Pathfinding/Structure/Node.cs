using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Pathfinding.Structure
{
	public class Node : IComparable<Node>
	{
		public NodeState State { get; set; }
		public short ParentX { get; set; }
		public short ParentY { get; set; }
		public short X { get; set; }
		public short Y { get; set; }
		public short F { get; set; }
		public short G { get; set; }
		public short H { get; set; }

		public int CompareTo(Node other)
		{
			if(F > other.F)
				return 1;
			if(F < other.F)
				return -1;
			return 0;
		}

		public static implicit operator Lazy<object>(Node v)
		{
			throw new NotImplementedException();
		}
	}
}