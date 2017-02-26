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
		/// Use raw connection string instead of built one
		/// </summary>
		public bool UseOwnConnectionString { get; set; } = false;

		/// <summary>
		/// Raw connection string provided by user
		/// </summary>
		public string OwnConnectionString { get; set; }

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
						if (UseOwnConnectionString)
						{
							SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
							sb.ConnectionString = OwnConnectionString;
							sb.InitialCatalog = Database;
							return sb.ConnectionString;
						}

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
						if (UseOwnConnectionString)
						{
							return OwnConnectionString;
						}

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

		public string GetConnectionStringForMsSqlDatabase(string database)
		{
			if(UseOwnConnectionString)
			{
				SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
				sb.ConnectionString = OwnConnectionString;
				sb.InitialCatalog = database;
				sb.ConnectTimeout = 1;
				return sb.ConnectionString;
			}

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
			builder.DataSource = ServerName;
			builder.IntegratedSecurity = UseWinAuth;
			builder.ConnectTimeout = 1;
			if(!UseWinAuth)
			{
				builder.UserID = Username;
				builder.Password = Password;
			}
			builder.InitialCatalog = database;
			return builder.ConnectionString;
		}

		/// <summary>
		/// Cancel current session
		/// </summary>
		public void Disconnect()
		{
			ConnectionType = ConnectionType.None;
			Database = string.Empty;
			ServerName = string.Empty;
			OwnConnectionString = string.Empty;
			UseOwnConnectionString = false;
		}
	}
}