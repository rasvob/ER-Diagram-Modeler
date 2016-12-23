using System;
using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	public interface IMapper: IDisposable
	{
		int CreateTable(string name);
		IEnumerable<MsSqlDatabaseInfo> ListDatabases();
		IEnumerable<TableModel> ListTables();
	}
}