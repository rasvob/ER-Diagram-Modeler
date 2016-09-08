using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class RelationshipModel
	{
		public Cardinality SourdeCardinality { get; set; }
		public Cardinality DestinationCardinality { get; set; }
		public Optionality DestinationOptionality { get; set; }
		public TableModel Source { get; set; }
		public TableModel Destination { get; set; }
		public TableRowModel SourceAttribute { get; set; }
		public TableRowModel DestinationAttribute { get; set; }
	}
}