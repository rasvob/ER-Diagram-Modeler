using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ER_Diagram_Modeler.Extintions
{
	public static class ExtintionsHelpers
	{
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items, Random random)
		{
			T[] arr = items.ToArray();

			for (var i = arr.Length - 1; i >= 0; i--)
			{
				int swapIndex = random.Next(i + 1);
				yield return arr[swapIndex];
				arr[swapIndex] = arr[i];
			}
		}

		public static Point ToMinified(this Point point, int step)
		{
			return new Point(point.X/step, point.Y/step);
		}

		public static Point FromMinified(this Point point, int step)
		{
			return new Point(point.X * step, point.Y * step);
		}
	}
}