using System;
using ER_Diagram_Modeler.ViewModels.Enums;

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
		public string ConnectionString { get; set; }
	}
}