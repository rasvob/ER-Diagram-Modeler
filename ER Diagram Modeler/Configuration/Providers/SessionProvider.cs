using System;
using System.Data.SqlClient;
using ER_Diagram_Modeler.ViewModels.Enums;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.Configuration.Providers
{
	/// <summary>
	/// Singleton for session data providing
	/// </summary>
	public class SessionProvider
	{
		/// <summary>
		/// Instance with lazy load
		/// </summary>
		private static readonly Lazy<SessionProvider> _instance = new Lazy<SessionProvider>(() => new SessionProvider());

		/// <summary>
		/// Prevent creation
		/// </summary>
		private SessionProvider()
		{

		}

		/// <summary>
		/// Current instance
		/// </summary>
		public static SessionProvider Instance => _instance.Value;

		/// <summary>
		/// Session type - MS Sql Server, Oracle or none
		/// </summary>
		public ConnectionType ConnectionType { get; set; }

		/// <summary>
		/// Host
		/// </summary>
		public string ServerName { get; set; }

		/// <summary>
		/// Username
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Password
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// MS Sql database
		/// </summary>
		public string Database { get; set; } = string.Empty;

		/// <summary>
		/// Intance or win based auth
		/// </summary>
		public bool UseWinAuth { get; set; }

		/// <summary>
		/// Build connection string based on session parameters
		/// </summary>
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

		/// <summary>
		/// Cancel current session
		/// </summary>
		public void Disconnect()
		{
			ConnectionType = ConnectionType.None;
			Database = string.Empty;
			ServerName = string.Empty;
		}
	}
}