using System;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public class DatabaseContext
	{
		private IDatabseStrategy _strategy;

		public DatabaseContext(ConnectionType connectionType)
		{
			SetStrategy(connectionType);
		}

		public void SetStrategy(ConnectionType connectionType)
		{
			switch(connectionType)
			{
				case ConnectionType.None:
					break;
				case ConnectionType.SqlServer:
					_strategy = new MsSqlStrategy();
					break;
				case ConnectionType.Oracle:
					_strategy = new OracleStrategy();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
			}
		}

		public TableModel ReadTableDetails(string id, string name)
		{
			return _strategy.ReadTableDetails(id, name);
		}
	}
}