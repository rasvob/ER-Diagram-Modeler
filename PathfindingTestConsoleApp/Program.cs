using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace PathfindingTestConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			var regex = new Regex("^timestamp(\\((\\d)\\))?");
			Console.WriteLine(regex.IsMatch("timestamp".ToLower()));

			Console.ReadLine();
		}
	}
}
