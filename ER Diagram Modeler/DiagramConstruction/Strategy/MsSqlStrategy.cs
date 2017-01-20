using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public class MsSqlStrategy: IDatabseStrategy
	{
		public TableModel ReadTableDetails(string id, string name)
		{
			using (IMapper mapper = new MsSqlMapper())
			{
				return mapper.SelectTableDetails(id, name);
			}
		}
	}
}