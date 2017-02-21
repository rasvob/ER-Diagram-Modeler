using System;
using System.Collections.Generic;
using System.Linq;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Configuration.Providers
{
	/// <summary>
	/// Singleton for holding datatypes
	/// </summary>
	public class DatatypeProvider
	{
		/// <summary>
		/// Datatypes holder instance with lazy load
		/// </summary>
		private static readonly Lazy<DatatypeProvider> _instance = new Lazy<DatatypeProvider>(() => new DatatypeProvider());

		/// <summary>
		/// Prevent creation
		/// </summary>
		private DatatypeProvider()
		{
			
		}

		/// <summary>
		/// Current instance
		/// </summary>
		public static DatatypeProvider Instance => _instance.Value;

		/// <summary>
		/// Sql server datatypes
		/// </summary>
		public IEnumerable<Datatype> SqlServerDatatypes => Datatype.LoadDatatypesFromResource(ConnectionType.SqlServer);

		/// <summary>
		/// Oracle datatypes
		/// </summary>
		public IEnumerable<Datatype> OracleDatatypes => Datatype.LoadDatatypesFromResource(ConnectionType.Oracle);

		/// <summary>
		/// Datatypes based on current session
		/// </summary>
		public IEnumerable<Datatype> SessionBasedDatatypes => SessionProvider.Instance.ConnectionType == ConnectionType.Oracle ? OracleDatatypes : SqlServerDatatypes;

		/// <summary>
		/// Find datatype by name
		/// </summary>
		/// <param name="name">Name of datatype</param>
		/// <param name="connType">Session connection type</param>
		/// <returns>Found datatype</returns>
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