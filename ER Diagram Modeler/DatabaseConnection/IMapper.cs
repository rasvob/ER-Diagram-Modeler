using System;
using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	public interface IMapper: IDisposable
	{
		void CreateTable(string name);
		IEnumerable<TableModel> ListTables();
		TableModel SelectTableDetails(string id, string name);
		void RenameTable(string oldName, string newName);
		void AddNewColumn(string table, TableRowModel model);
		void AlterColumn(string table, TableRowModel model);
		void RenameColumn(string table, string oldName, string newName);
		void DropColumn(string table, string column);
		void DropTable(string table);
		void DropPrimaryKey(string table ,string primaryKeyConstraintName);
		void CreatePrimaryKey(string table, string[] columns);
	}
}