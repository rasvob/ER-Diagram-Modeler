using System.Collections.Generic;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class RelationshipModel
	{
		//Source:Destination => 1:N
		public TableModel Source { get; set; }
		public TableModel Destination { get; set; }
		public Optionality Optionality { get; set; } = Optionality.Optional;
		public Dictionary<TableRowModel, TableRowModel> Attributes { get; set; }
	}
}