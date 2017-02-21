using System;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	/// <summary>
	/// Static factory for mappers
	/// </summary>
	public static class MapperFactory
	{
		/// <summary>
		/// Get mapper instance based on connection type
		/// </summary>
		/// <param name="type">Connection type</param>
		/// <returns>Mapper with IMapper interface</returns>
		public static IMapper GetMapper(ConnectionType type)
		{
			switch (type)
			{
				case ConnectionType.SqlServer:
					return new MsSqlMapper();
				case ConnectionType.Oracle:
					return new OracleMapper();
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		/// <summary>
		/// Get mapper instance based on connection type for connection string
		/// </summary>
		/// <param name="type">Connection type</param>
		/// <param name="connectionString">Connection string</param>
		/// <returns>Mapper with IMapper interface</returns>
		public static IMapper GetMapper(ConnectionType type, string connectionString)
		{
			switch(type)
			{
				case ConnectionType.SqlServer:
					return new MsSqlMapper(connectionString);
				case ConnectionType.Oracle:
					return new OracleMapper(connectionString);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}
}