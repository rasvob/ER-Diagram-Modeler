using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	/// <summary>
	/// Ms Sql database connection operations
	/// </summary>
	public class MsSqlDatabase: IDatabase<SqlCommand>
	{
		/// <summary>
		/// Current connection
		/// </summary>
		public SqlConnection Connection { get; set; }

		/// <summary>
		/// Connection string
		/// </summary>
		public string ConnectionString { get; set; }

		public MsSqlDatabase()
		{
			Connection = new SqlConnection();
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
			if (Connection.State != ConnectionState.Open)
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
		/// Execute command and return number of rows affected
		/// </summary>
		/// <param name="command">SQL Command</param>
		/// <returns>Number of rows affected</returns>
		public int ExecuteNonQuery(SqlCommand command)
		{
			int rowNumber = 0;
			try
			{
				rowNumber = command.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				throw e;
			}
			return rowNumber;
		}

		/// <summary>
		/// Create SqlCommand
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>SqlCommand object</returns>
		public SqlCommand CreateCommand(string sql)
		{
			return new SqlCommand(sql, Connection);
		}

		/// <summary>
		/// Execute reader for command
		/// </summary>
		/// <param name="command">Sql Command</param>
		/// <returns>Sql reader object</returns>
		public SqlDataReader Select(SqlCommand command)
		{
			return command.ExecuteReader();
		}

		/// <summary>
		/// Try connection with credentials
		/// </summary>
		/// <param name="connStreing">Connection string</param>
		/// <returns>Task for async execution</returns>
		private async Task TryToConnectToServer(string connStreing)
		{
			SqlConnection connection = new SqlConnection(connStreing);
			await connection.OpenAsync();
			connection.Close();
		}

		/// <summary>
		/// Build MS Sql Server session
		/// </summary>
		/// <param name="server">Hostname</param>
		/// <param name="integratedSecurity">True if you are using Windows authentication</param>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		/// <returns>Task for async execution</returns>
		public async Task BuildSession(string server, bool integratedSecurity, string username = null, string password = null)
		{
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
			{
				DataSource = server,
				IntegratedSecurity = integratedSecurity
			};

			if(!builder.IntegratedSecurity)
			{
				builder.UserID = username;
				builder.Password = password;
			}

			await TryToConnectToServer(builder.ConnectionString);

			SessionProvider.Instance.ServerName = builder.DataSource;
			SessionProvider.Instance.UseWinAuth = builder.IntegratedSecurity;

			if(SessionProvider.Instance.UseWinAuth)
			{
				SessionProvider.Instance.Username = builder.UserID;
				SessionProvider.Instance.Password = builder.Password;
			}

			SessionProvider.Instance.ConnectionType = ConnectionType.SqlServer;
		}
	}
}