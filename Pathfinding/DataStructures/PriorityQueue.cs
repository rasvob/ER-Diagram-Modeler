using System;
using System.Collections.Generic;

namespace Pathfinding.DataStructures
{
	public class PriorityQueue<T>: IPriorityQueue<T> where T : IComparable<T>
	{
		private readonly List<T> _items;

		public PriorityQueue()
		{
			_items = new List<T>();
		} 

		public void Enqueue(T item)
		{
			_items.Add(item);
			int childIndex = _items.Count - 1;

			while (childIndex > 0)
			{
				int parentIndex = (childIndex - 1)/2;

				if (_items[childIndex].CompareTo(_items[parentIndex]) >= 0)
				{
					break;
				}

				Swap(parentIndex, childIndex);
				childIndex = parentIndex;
			}
		}

		public T Dequeue()
		{
			int lastIndex = _items.Count - 1;
			T retItem = _items[0];
			_items[0] = _items[lastIndex];
			_items.RemoveAt(lastIndex);

			lastIndex--;
			int parentIndex = 0;

			while (true)
			{
				int childIndex = 2*parentIndex + 1;

				if (childIndex > lastIndex)
				{
					break;
				}

				int rightChildIndex = childIndex + 1;

				if (rightChildIndex <= lastIndex && _items[rightChildIndex].CompareTo(_items[childIndex]) < 0)
				{
					childIndex = rightChildIndex;
				}

				if (_items[parentIndex].CompareTo(_items[childIndex]) <= 0)
				{
					break;
				}

				Swap(parentIndex, childIndex);
				parentIndex = childIndex;
			}

			return retItem;
		}

		public T Peek() => _items[0];

		public int Count => _items.Count;

		private void Swap(int i, int j)
		{
			T tmp = _items[i];
			_items[i] = _items[j];
			_items[j] = tmp;
		}
	}
}