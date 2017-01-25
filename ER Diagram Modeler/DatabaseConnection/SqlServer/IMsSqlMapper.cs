using System.Collections.Generic;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public interface IMsSqlMapper: IMapper
	{
		IEnumerable<DatabaseInfo> ListDatabases();
		IEnumerable<MsSqlForeignKeyDto> ListForeignKeys(string tableName);
		void CreateDatabase(string name);
	}
}