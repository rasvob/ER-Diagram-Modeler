namespace ER_Diagram_Modeler.CommandOutput
{
	public interface IOutputListener
	{
		void Write(string text);
		void WriteLine(string line);
	}
}