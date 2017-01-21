using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public interface IDatabseStrategy
	{
		TableModel ReadTableDetails(string id, string name);
		IEnumerable<RelationshipModel> ReadRelationshipModels(string table, IEnumerable<TableModel> tables);
	}
}