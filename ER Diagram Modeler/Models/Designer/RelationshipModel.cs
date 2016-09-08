using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class RelationshipModel
	{
		public Cardinality SourdeCardinality { get; set; } = Cardinality.One;
		public Cardinality DestinationCardinality { get; set; } = Cardinality.Many;
		public Optionality DestinationOptionality { get; set; } = Optionality.Optional;
		public Optionality SourceOptionality { get; set; } = Optionality.Mandatory;
		public TableModel Source { get; set; }
		public TableModel Destination { get; set; }
		public TableRowModel SourceAttribute { get; set; }
		public TableRowModel DestinationAttribute { get; set; }
	}
}