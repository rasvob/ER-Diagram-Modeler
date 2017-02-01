using System;

namespace Pathfinding.DataStructures
{
	public interface IPriorityQueue<T> where T: IComparable<T>
	{
		void Enqueue(T item);
		T Dequeue();
		T Peek();
		int Count { get; }
	}
}