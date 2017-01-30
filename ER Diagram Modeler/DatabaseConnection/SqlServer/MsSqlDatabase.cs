using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public class MsSqlDatabase: IDatabase<SqlCommand>
	{
		public SqlConnection Connection { get; set; }
		public string ConnectionString { get; set; }

		public MsSqlDatabase()
		{
			Connection = new SqlConnection();
		}

		public bool Connect()
		{
			return Connect(ConnectionString);
		}

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

		public void Close()
		{
			Connection.Close();
		}

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

		public SqlCommand CreateCommand(string sql)
		{
			return new SqlCommand(sql, Connection);
		}

		public SqlDataReader Select(SqlCommand command)
		{
			return command.ExecuteReader();
		}

		private async Task TryToConnectToServer(string connStreing)
		{
			SqlConnection connection = new SqlConnection(connStreing);
			await connection.OpenAsync();
			connection.Close();
		}

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