using System;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	public static class MapperFactory
	{
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