using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ER_Diagram_Modeler.Extintions
{
	/// <summary>
	/// Extintion methods
	/// </summary>
	public static class ExtintionsHelpers
	{
		/// <summary>
		/// Shuffle IEnumerable
		/// </summary>
		/// <typeparam name="T">Collection item</typeparam>
		/// <param name="items">Collection</param>
		/// <param name="random">Randomizer</param>
		/// <returns>Shuffled collection</returns>
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

		/// <summary>
		/// Transform point coordinates
		/// </summary>
		/// <param name="point">Input</param>
		/// <param name="step">Divider</param>
		/// <returns>Transformed point</returns>
		public static Point ToMinified(this Point point, int step)
		{
			return new Point(point.X/step, point.Y/step);
		}

		/// <summary>
		/// Transform point coordinates
		/// </summary>
		/// <param name="point">Input</param>
		/// <param name="step">Multiplier</param>
		/// <returns>Transformed point</returns>
		public static Point FromMinified(this Point point, int step)
		{
			return new Point(point.X * step, point.Y * step);
		}
	}
}