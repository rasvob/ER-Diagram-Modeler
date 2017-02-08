using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public interface IOracleMapper: IMapper
	{
		void AlterColumn(string table, TableRowModel model, bool modifyNull = false);
		void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null, string onDelete = "NO ACTION");

	}
}