using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public class OracleMapper: IOracleMapper
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

		public void CreateTable(string name)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<TableModel> ListTables()
		{
			throw new System.NotImplementedException();
		}

		public TableModel SelectTableDetails(string id, string name)
		{
			throw new System.NotImplementedException();
		}
	}
}