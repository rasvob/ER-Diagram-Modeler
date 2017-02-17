using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.ViewModels.Enums;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public class OracleDatabase: IDatabase<OracleCommand>
	{
		public OracleConnection Connection { get; set; }
		public string ConnectionString { get; set; }

		public OracleDatabase()
		{
			Connection = new OracleConnection();
		}

		public  bool Connect()
		{
			return Connect(ConnectionString);
		}

		public bool Connect(string conString)
		{
			if(Connection.State != ConnectionState.Open)
			{
				Connection.ConnectionString = conString;
				ConnectionString = conString;
				Connection.Open();
				return true;
			}
			return false;
		}

		public void Close()
		{
			Connection.Close();
		}

		public OracleCommand CreateCommand(string sql)
		{
			return new OracleCommand(sql, Connection)
			{
				BindByName = true
			};
		}

		private async Task TryToConnectToServer(string connStreing)
		{
			OracleConnection connection = new OracleConnection(connStreing);
			await connection.OpenAsync();
			connection.Close();
		}

		public async Task BuildSession(string server, string port, string sid,string username = null, string password = null)
		{
			OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder
			{
				DataSource = $"(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={server})(PORT={port})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={sid})))",
				UserID = username,
				Password = password
			};

			await TryToConnectToServer(builder.ConnectionString);

			SessionProvider.Instance.ServerName = builder.DataSource;
			SessionProvider.Instance.Username = builder.UserID;
			SessionProvider.Instance.Password = builder.Password;
			SessionProvider.Instance.ConnectionType = ConnectionType.Oracle;
		}
	}
}