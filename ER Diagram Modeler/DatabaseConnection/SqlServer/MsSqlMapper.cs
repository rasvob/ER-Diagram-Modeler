using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public class MsSqlMapper: IMapper
	{
		private static string SqlListDatabases = @"SELECT database_id, name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');";
		private static string SqlListTables = @"SELECT object_id, name FROM sys.tables; ";
		
		public MsSqlDatabase Database { get; set; }

		public MsSqlMapper()
		{
			Database = new MsSqlDatabase();
			Database.Connect(SessionProvider.Instance.ConnectionString);
		}

		public void Dispose()
		{
			if (Database.Connection.State == ConnectionState.Open)
			{
				Database.Close();
			}
		}

		public int CreateTable(string name)
		{
			return 0;
		}

		public IEnumerable<MsSqlDatabaseInfo> ListDatabases()
		{
			SqlCommand command = Database.CreateCommand(SqlListDatabases);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadListDatabases(reader);
			reader.Close();
			return res;
		}

		public IEnumerable<TableModel> ListTables()
		{
			SqlCommand command = Database.CreateCommand(SqlListTables);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadListTables(reader);
			reader.Close();
			return res;
		}

		private IEnumerable<TableModel> ReadListTables(SqlDataReader reader)
		{
			List<TableModel> res = new List<TableModel>();

			while (reader.Read())
			{
				var table = new TableModel();
				int i = 0;
				table.Id = reader.GetInt32(i++).ToString();
				table.Title = reader.GetString(i++);
				res.Add(table);
			}

			return res;
		}

		public IEnumerable<MsSqlDatabaseInfo> ReadListDatabases(SqlDataReader reader)
		{
			List<MsSqlDatabaseInfo> res = new List<MsSqlDatabaseInfo>();

			while (reader.Read())
			{
				int i = 0;
				var info = new MsSqlDatabaseInfo();
				info.Id = reader.GetInt32(i++);
				info.Name = reader.GetString(i++);
				res.Add(info);
			}

			return res;
		}
	}
}