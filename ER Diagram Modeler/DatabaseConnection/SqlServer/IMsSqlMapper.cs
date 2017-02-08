using System.Collections.Generic;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public interface IMsSqlMapper: IMapper
	{
		IEnumerable<DatabaseInfo> ListDatabases();
		void CreateDatabase(string name);
		void DropDatabase(string name);
		void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null, string onUpdate = "NO ACTION", string onDelete = "NO ACTION");
	}
}