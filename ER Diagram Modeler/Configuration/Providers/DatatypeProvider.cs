using System;
using System.Collections.Generic;
using System.Linq;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Configuration.Providers
{
	public class DatatypeProvider
	{
		private static readonly Lazy<DatatypeProvider> _instance = new Lazy<DatatypeProvider>(() => new DatatypeProvider());

		private DatatypeProvider()
		{
			
		}

		public static DatatypeProvider Instance => _instance.Value;

		public IEnumerable<Datatype> SqlServerDatatypes => Datatype.LoadDatatypesFromResource(ConnectionType.SqlServer);
		public IEnumerable<Datatype> OracleDatatypes => Datatype.LoadDatatypesFromResource(ConnectionType.Oracle);
		public IEnumerable<Datatype> SessionBasedDatatypes => SessionProvider.Instance.ConnectionType == ConnectionType.Oracle ? OracleDatatypes : SqlServerDatatypes;

		public Datatype FindDatatype(string name, ConnectionType connType = ConnectionType.None)
		{
			if (connType == ConnectionType.None)
			{
				connType = SessionProvider.Instance.ConnectionType;
			}

			if (connType == ConnectionType.Oracle)
			{
				return
					OracleDatatypes.FirstOrDefault(t => t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
			}
			return
				SqlServerDatatypes.FirstOrDefault(t => t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
		}
	}
}