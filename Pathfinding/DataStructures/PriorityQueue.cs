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

		public void Update(T item)
		{
			int current = _items.IndexOf(item);
			int start = current;

			if (current < 0)
			{
				return;
			}

			int parent;
			while (true)
			{
				if (current == 0)
				{
					break;
				}

				parent = (current - 1)/2;

				if (_items[current].CompareTo(_items[parent]) >= 0)
				{
					break;
				}

				Swap(current, parent);
				current = parent;
			}

			if (current < start)
			{
				return;
			}

			while (true)
			{
				parent = current;
				var leftChild = 2*current + 1;
				var rightChild = 2*current + 2;

				if (_items.Count > leftChild && _items[parent].CompareTo(_items[leftChild]) <= 0)
				{
					current = leftChild;
				}

				if(_items.Count > leftChild && _items[parent].CompareTo(_items[rightChild]) <= 0)
				{
					current = rightChild;
				}

				if (current == parent)
				{
					break;
				}

				Swap(parent, current);
			}

		}

		public int Count => _items.Count;

		private void Swap(int i, int j)
		{
			T tmp = _items[i];
			_items[i] = _items[j];
			_items[j] = tmp;
		}
	}
}