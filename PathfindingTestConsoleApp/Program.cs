using System;
using System.Collections.Generic;
using System.Diagnostics;
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

			//var queue = new PriorityQueue<Node>();
			//var node1 = new Node() {F = 10};
			//var node2 = new Node() {F = 5};
			//var node3 = new Node() {F = 3};
			//var node4 = new Node() {F = 20};
			//var node5 = new Node() {F = 15};

			//queue.Enqueue(node1);
			//queue.Enqueue(node2);
			//queue.Enqueue(node3);
			//queue.Enqueue(node4);
			//queue.Enqueue(node5);

			//node1.F = 1;
			//queue.Update(node1);

			//node2.F = 1;
			//queue.Update(node2);

			//node3.F = 17;
			//queue.Update(node3);

			//node4.F = 0;
			//queue.Update(node4);

			//Console.WriteLine(queue.Dequeue().F);
			//Console.WriteLine(queue.Dequeue().F);
			//Console.WriteLine(queue.Dequeue().F);
			//Console.WriteLine(queue.Dequeue().F);
			//Console.WriteLine(queue.Dequeue().F);

			int n = 5000;
			var sw = new Stopwatch();
			sw.Start();

			var arr = new Node[n];
			for (var i = 0; i < arr.Length; i++)
			{
				arr[i] = new Node();
			}
			sw.Stop();
			Console.WriteLine(sw.ElapsedMilliseconds/1000.0);

			Console.ReadLine();
		}
	}
}
