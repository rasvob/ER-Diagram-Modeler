using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.EventArgs
{
	/// <summary>
	/// Row edit EventArgs
	/// </summary>
	public class EditRowEventArgs: System.EventArgs
	{
		public TableModel TableModel { get; set; }
		public TableRowModel RowModel { get; set; }
	}
}