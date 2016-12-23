using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public class OracleMapper: IMapper
	{
		private readonly OracleDatabase _database = new OracleDatabase();

		public OracleMapper()
		{
			_database.Connect();
		}

		public void Dispose()
		{
			if(_database.Connection.State == ConnectionState.Open)
			{
				_database.Close();
			}
		}

		public int CreateTable(string name)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<MsSqlDatabaseInfo> ListDatabases()
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<TableModel> ListTables()
		{
			throw new System.NotImplementedException();
		}
	}
}