using System.Collections.Generic;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public interface IMsSqlMapper: IMapper
	{
		IEnumerable<DatabaseInfo> ListDatabases();
		void CreateDatabase(string name);
		void DropDatabase(string name);
	}
}