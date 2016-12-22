using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public class MsSqlMapper: IMapper
	{
		private readonly MsSqlDatabase _database;

		public MsSqlMapper()
		{
			_database = new MsSqlDatabase();
		}

		public void Dispose()
		{
			if (_database.Connection.State == ConnectionState.Open)
			{
				_database.Close();
			}
		}

		public int CreateTable(string name)
		{
			return 0;
		}

		public IEnumerable<DatabaseInfo> ListDatabases()
		{
			return null;
		}

		public IEnumerable<TableModel> ListTables()
		{
			return null;
		}
	}
}