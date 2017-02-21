namespace ER_Diagram_Modeler.EventArgs
{
	/// <summary>
	/// EventArgs for table position or measure change
	/// </summary>
	public class TablePositionAndMeasureEventArgs
	{
		public double LeftDelta { get; set; } = 0;
		public double TopDelta { get; set; } = 0;
		public double WidthDelta { get; set; } = 0;
		public double HeightDelta { get; set; } = 0;
	}
}