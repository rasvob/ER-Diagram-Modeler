namespace ER_Diagram_Modeler.EventArgs
{
	public class TablePositionAndMeasureEventArgs
	{
		public double LeftDelta { get; set; } = 0;
		public double TopDelta { get; set; } = 0;
		public double WidthDelta { get; set; } = 0;
		public double HeightDelta { get; set; } = 0;
		public bool Handled { get; set; } = false;
	}
}