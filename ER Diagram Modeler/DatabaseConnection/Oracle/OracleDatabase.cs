using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.ViewModels.Enums;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	/// <summary>
	/// Oracle database connection operations
	/// </summary>
	public class OracleDatabase: IDatabase<OracleCommand>
	{
		/// <summary>
		/// Current connection
		/// </summary>
		public OracleConnection Connection { get; set; }

		/// <summary>
		/// Connection string
		/// </summary>
		public string ConnectionString { get; set; }

		public OracleDatabase()
		{
			Connection = new OracleConnection();
		}

		/// <summary>
		/// Connect to DB - Session connection string
		/// </summary>
		/// <returns>True if success, false if not</returns>
		public bool Connect()
		{
			return Connect(ConnectionString);
		}

		/// <summary>
		/// Connect to DB
		/// </summary>
		/// <param name="conString">Connection string</param>
		/// <returns>rue if success, false if not</returns>
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

		/// <summary>
		/// Close DB connection
		/// </summary>
		public void Close()
		{
			Connection.Close();
		}

		/// <summary>
		/// Create OracleCommand
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>OracleCommand object</returns>
		public OracleCommand CreateCommand(string sql)
		{
			return new OracleCommand(sql, Connection)
			{
				BindByName = true
			};
		}

		/// <summary>
		/// Try connection with credentials
		/// </summary>
		/// <param name="connStreing">Connection string</param>
		/// <returns>Task for async execution</returns>
		public async Task TryToConnectToServer(string connStreing)
		{
			OracleConnection connection = new OracleConnection(connStreing);
			await connection.OpenAsync();
			connection.Close();
		}

		/// <summary>
		/// Build Oracle session
		/// </summary>
		/// <param name="server">Hostname</param>
		/// <param name="port">Port (default 1521)</param>
		/// <param name="sid">SID</param>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		/// <returns>Task for async execution</returns>
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