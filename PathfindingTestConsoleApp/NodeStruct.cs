using Pathfinding.Structure;

namespace PathfindingTestConsoleApp
{
	public struct NodeStruct
	{
		public short ParentX;
		public short ParentY;
		public short X;
		public short Y;
		public short F;
		public short G;
		public short H;
		public NodeState State;
	}
}