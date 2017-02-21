using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	/// <summary>
	/// Interface implemented by MsSqlStrategy and OracleStrategy
	/// </summary>
	public interface IDatabseStrategy
	{
		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
		TableModel ReadTableDetails(string id, string name);

		/// <summary>
		/// List foreign key coninstraints for table
		/// </summary>
		/// <param name="table">Name of table</param>
		/// <param name="tables">Tables in designer</param>
		/// <returns>Collection of FK constraints</returns>
		IEnumerable<RelationshipModel> ReadRelationshipModels(string table, IEnumerable<TableModel> tables);

		/// <summary>
		/// Return default column by connection type
		/// </summary>
		/// <returns>Default column</returns>
		TableRowModel PlaceholderRowModel();

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		void CreateTable(string name);

		/// <summary>
		/// List tables in DB
		/// </summary>
		/// <returns>Collection of tables with ID and name</returns>
		IEnumerable<TableModel> ListTables();

		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		void RenameTable(string oldName, string newName);

		/// <summary>
		/// Rename column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		void RenameColumn(string table, string oldName, string newName);

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		void AddColumn(string table, TableRowModel model);

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		void UpdateColumn(string table, TableRowModel model);

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		void RemoveColumn(string table, string column);

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		void RemoveTable(TableModel table);

		/// <summary>
		/// Update PK constraint
		/// </summary>
		/// <param name="table">Table model</param>
		void UpdatePrimaryKeyConstraint(TableModel table);

		/// <summary>
		/// Drop foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		void RemoveRelationship(RelationshipModel model);

		/// <summary>
		/// Create foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		void AddRelationship(RelationshipModel model);

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		IEnumerable<string> ListAllForeignKeys();

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		int SaveDiagram(string name, XDocument data);

		/// <summary>
		/// Delete diagram from DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <returns>One if successful, zero if not</returns>
		int DeleteDiagram(string name);

		/// <summary>
		/// Select existing diagrams
		/// </summary>
		/// <returns>Collections of diagrams</returns>
		IEnumerable<DiagramModel> SelectDiagrams();

		/// <summary>
		/// Execute raw query
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>Dataset with results</returns>
		DataSet ExecuteRawQuery(string sql);

		/// <summary>
		/// Comparer for RelationshipModel objects
		/// </summary>
		IComparer<RelationshipModel> Comparer { get; set; }
	}
}