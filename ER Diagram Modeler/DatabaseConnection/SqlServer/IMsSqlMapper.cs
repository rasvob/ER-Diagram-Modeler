using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Database;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public interface IMsSqlMapper: IMapper
	{
		IEnumerable<MsSqlDatabaseInfo> ListDatabases();
	}
}