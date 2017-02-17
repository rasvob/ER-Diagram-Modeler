using System.Collections.Generic;

namespace ER_Diagram_Modeler.CommandOutput
{
	public static class Output
	{
		public static List<IOutputListener> OutputListeners { get; set; } = new List<IOutputListener>();

		public static void Write(string text)
		{
			OutputListeners.ForEach(t => t.Write(text));
		}

		public static void WriteLine(string text)
		{
			OutputListeners.ForEach(t => t.WriteLine(text));
		}
	}
}