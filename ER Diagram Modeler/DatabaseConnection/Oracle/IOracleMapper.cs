using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	/// <summary>
	/// Specific operations for Oracle mapper
	/// </summary>
	public interface IOracleMapper: IMapper
	{
		/// <summary>
		/// Alter column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		/// <param name="modifyNull">Modify also (NOT) NULL constraint</param>
		void AlterColumn(string table, TableRowModel model, bool modifyNull = false);

		/// <summary>
		/// Create new foreign key
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="referencedTable">Referenced table name</param>
		/// <param name="collumns">Collection of constraint columns</param>
		/// <param name="fkName">Name of constraint, generated if NULL</param>
		/// <param name="onDelete">Action on delete, all standard Oracle actions</param>
		void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null, string onDelete = "NO ACTION");

	}
}