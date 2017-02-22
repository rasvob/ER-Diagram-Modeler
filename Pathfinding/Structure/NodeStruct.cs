using System;

namespace Pathfinding.Structure
{
	/// <summary>
	/// Struct for Node in Graph
	/// </summary>
	/// <remarks>DEPRECATED</remarks>
	public struct NodeStruct: IComparable<NodeStruct>
	{
		public short ParentX;
		public short ParentY;
		public short X;
		public short Y;
		public short F;
		public short G;
		public short H;
		public NodeState State;

		public int CompareTo(NodeStruct other)
		{
			if(F > other.F)
				return 1;
			if(F < other.F)
				return -1;
			return 0;
		}
	}
}