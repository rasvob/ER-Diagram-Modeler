using System;
using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	public interface IMapper: IDisposable
	{
		TableModel SelectTableDetails(string id, string name);
		IEnumerable<TableModel> ListTables();
		IEnumerable<ForeignKeyDto> ListForeignKeys(string tableName);
		void CreateTable(string name);
		void RenameTable(string oldName, string newName);
		void AddNewColumn(string table, TableRowModel model);
		void AlterColumn(string table, TableRowModel model);
		void RenameColumn(string table, string oldName, string newName);
		void DropColumn(string table, string column);
		void DropTable(string table);
		void DropPrimaryKey(string table ,string primaryKeyConstraintName);
		void CreatePrimaryKey(string table, string[] columns);
		void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null);
		void DropForeignKey(string table ,string name);
	}
}