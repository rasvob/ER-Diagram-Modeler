using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public interface IOracleMapper: IMapper
	{
		void AlterColumn(string table, TableRowModel model, bool modifyNull = false);
	}
}