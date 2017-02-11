using System.Collections.Generic;
using System.Xml.Linq;
using ER_Diagram_Modeler.Models.Database;
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
		void RemoveRelationship(RelationshipModel model);
		void AddRelationship(RelationshipModel model);
		IEnumerable<string> ListAllForeignKeys();
		int SaveDiagram(string name, XDocument data);
		int DeleteDiagram(string name);
		IEnumerable<DiagramModel> SelectDiagrams();
		IComparer<RelationshipModel> Comparer { get; set; }
	}
}