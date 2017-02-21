namespace ER_Diagram_Modeler.CommandOutput
{
	/// <summary>
	/// Console-like output interface
	/// </summary>
	public interface IOutputListener
	{
		/// <summary>
		/// Write text
		/// </summary>
		/// <param name="text">Text for write</param>
		void Write(string text);

		/// <summary>
		/// Write line of text
		/// </summary>
		/// <param name="line">Text for write</param>
		void WriteLine(string line);
	}
}