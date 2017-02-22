using System;

namespace Pathfinding.DataStructures
{
	/// <summary>
	/// Interface for priority queue data structure
	/// </summary>
	/// <typeparam name="T">Queue item</typeparam>
	public interface IPriorityQueue<T> where T: IComparable<T>
	{
		/// <summary>
		/// Add item to queue
		/// </summary>
		/// <param name="item">Item for enque</param>
		void Enqueue(T item);

		/// <summary>
		/// Deque item from queue
		/// </summary>
		/// <returns>Item with highest priority</returns>
		T Dequeue();

		/// <summary>
		/// Peek at queue head value
		/// </summary>
		/// <returns>Item with highest priority</returns>
		T Peek();

		/// <summary>
		/// Update item in queue
		/// </summary>
		/// <param name="item">Item for update</param>
		void Update(T item);

		/// <summary>
		/// Count of items in queue
		/// </summary>
		int Count { get; }
	}
}