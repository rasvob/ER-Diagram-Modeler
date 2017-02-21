using System.Collections.Generic;

namespace ER_Diagram_Modeler.CommandOutput
{
	/// <summary>
	/// Static class for triggering output listeners
	/// </summary>
	public static class Output
	{
		/// <summary>
		/// Listeners collection
		/// </summary>
		public static List<IOutputListener> OutputListeners { get; set; } = new List<IOutputListener>();

		/// <summary>
		/// Call write for all listeners
		/// </summary>
		/// <param name="text">Text for write</param>
		public static void Write(string text)
		{
			OutputListeners.ForEach(t => t.Write(text));
		}

		/// <summary>
		/// Call write line for all listeners
		/// </summary>
		/// <param name="text">Text for write</param>
		public static void WriteLine(string text)
		{
			OutputListeners.ForEach(t => t.WriteLine(text));
		}
	}
}