using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public interface IDatabseStrategy
	{
		TableModel ReadTableDetails(string id, string name);
	}
}