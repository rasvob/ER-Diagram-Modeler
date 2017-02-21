namespace ER_Diagram_Modeler.EventArgs
{
	/// <summary>
	/// Scale change EventArgs
	/// </summary>
	public class ScaleEventArgs: System.EventArgs
	{
		public double OldScale { get; set; }
		public double OldVerticalOffset { get; set; }
		public double OldHorizontalOffset { get; set; }
		public double OldViewportWidth { get; set; }
		public double OldViewportHeight { get; set; }
	}
}