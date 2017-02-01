using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathfinding;
using Pathfinding.DataStructures;
using Pathfinding.Structure;

namespace PathfindingTestConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			//var pf = new BfsPathFinder(PathFinderHelper.CreateGrid(2500, 2500, new Rectangle[]
			//{
			//	new Rectangle(1,1, 40, 20), 
			//	new Rectangle(50,50, 40, 40), 
			//	new Rectangle(100,100, 20, 40), 
			//	new Rectangle(150,10, 40, 40), 
			//	new Rectangle(10,100, 40, 40)
			//}));

			//Point[] path = pf.FindPath(new Point(41, 10), new Point(120, 140));

			//string[] grid = pf.CreateDebugGrid(path);

			//using (FileStream fs = new FileStream("debugGrid.txt", FileMode.Create))
			//{
			//	using (StreamWriter sw = new StreamWriter(fs))
			//	{
			//		foreach (string line in grid)
			//		{
			//			sw.WriteLine(line);
			//		}
			//	}
			//}

			var queue = new PriorityQueue<Node>();

			queue.Enqueue(new Node()
			{
				F = 10
			});
			queue.Enqueue(new Node()
			{
				F = 20
			});
			queue.Enqueue(new Node()
			{
				F = 5
			});
			queue.Enqueue(new Node()
			{
				F = 15
			});

			Console.WriteLine(queue.Dequeue().F);
			Console.WriteLine(queue.Dequeue().F);
			Console.WriteLine(queue.Dequeue().F);
			Console.WriteLine(queue.Dequeue().F);
		}
	}
}
