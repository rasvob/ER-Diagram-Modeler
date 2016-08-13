using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.EventArgs
{
	public class TableViewModelArgs: System.EventArgs
	{
		public TableViewModel ViewModel { get; set; }
	}
}