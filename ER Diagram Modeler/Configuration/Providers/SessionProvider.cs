using System;
using System.Data.SqlClient;
using ER_Diagram_Modeler.ViewModels.Enums;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.Configuration.Providers
{
	public class SessionProvider
	{
		private static readonly Lazy<SessionProvider> _instance = new Lazy<SessionProvider>(() => new SessionProvider());

		private SessionProvider()
		{

		}

		public static SessionProvider Instance => _instance.Value;

		public ConnectionType ConnectionType { get; set; }

		public string ServerName { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Database { get; set; } = string.Empty;
		public bool UseWinAuth { get; set; }

		public string ConnectionString
		{
			get
			{
				switch (ConnectionType)
				{
					case ConnectionType.None:
						return string.Empty;
					case ConnectionType.SqlServer:
						SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
						builder.DataSource = ServerName;
						builder.IntegratedSecurity = UseWinAuth;
						if (!UseWinAuth)
						{
							builder.UserID = Username;
							builder.Password = Password;
						}

						if (Database != string.Empty)
						{
							builder.InitialCatalog = Database;
						}

						return builder.ConnectionString;
					case ConnectionType.Oracle:
						OracleConnectionStringBuilder oracleConnectionStringBuilder = new OracleConnectionStringBuilder
						{
							DataSource = ServerName,
							UserID = Username,
							Password = Password
						};
						return oracleConnectionStringBuilder.ConnectionString;
				}

				return string.Empty;
			}
		}

		public void Disconnect()
		{
			ConnectionType = ConnectionType.None;
			Database = string.Empty;
			ServerName = string.Empty;
		}
	}
}