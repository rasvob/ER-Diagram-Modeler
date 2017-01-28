using System.Collections.Generic;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public interface IDatabseStrategy
	{
		TableModel ReadTableDetails(string id, string name);
		IEnumerable<RelationshipModel> ReadRelationshipModels(string table, IEnumerable<TableModel> tables);
		TableRowModel PlaceholderRowModel();
		void CreateTable(string name);
		IEnumerable<TableModel> ListTables();
		void RenameTable(string oldName, string newName);
		void RenameColumn(string table, string oldName, string newName);
		void AddColumn(string table, TableRowModel model);
		void UpdateColumn(string table, TableRowModel model);
		void RemoveColumn(string table, string column);
		void RemoveTable(TableModel table);
		void UpdatePrimaryKeyConstraint(TableModel table);
	}
}