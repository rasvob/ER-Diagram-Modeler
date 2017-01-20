using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public class OracleStrategy: IDatabseStrategy
	{
		public TableModel ReadTableDetails(string id, string name)
		{
			using (IMapper mapper = new OracleMapper())
			{
				return mapper.SelectTableDetails(id, name);
			}
		}
	}
}