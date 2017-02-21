using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	/// <summary>
	/// Layer-supertype for mappers
	/// </summary>
	public interface IMapper: IDisposable
	{
		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
		TableModel SelectTableDetails(string id, string name);

		/// <summary>
		/// List tables in DB
		/// </summary>
		/// <returns>Collection of tables with ID and name</returns>
		IEnumerable<TableModel> ListTables();

		/// <summary>
		/// List foreign key coninstraints for table
		/// </summary>
		/// <param name="tableName">Name of table</param>
		/// <returns>Collection of constraint</returns>
		IEnumerable<ForeignKeyDto> ListForeignKeys(string tableName);

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		IEnumerable<string> ListAllForeignKeys();

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		void CreateTable(string name);

		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		void RenameTable(string oldName, string newName);

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		void AddNewColumn(string table, TableRowModel model);

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		void AlterColumn(string table, TableRowModel model);

		/// <summary>
		/// Rename column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		void RenameColumn(string table, string oldName, string newName);

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		void DropColumn(string table, string column);

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		void DropTable(string table);

		/// <summary>
		/// Drop primary key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="primaryKeyConstraintName">Constraint name</param>
		void DropPrimaryKey(string table ,string primaryKeyConstraintName);

		/// <summary>
		/// Create new primary key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="columns">Name of primary key columns</param>
		void CreatePrimaryKey(string table, string[] columns);

		/// <summary>
		/// Create new foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="referencedTable">Referenced table name</param>
		/// <param name="collumns">Collection of column models</param>
		/// <param name="fkName">Name of constraint, generated if null</param>
		void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null);

		/// <summary>
		/// Drop foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="name">Constraint name</param>
		void DropForeignKey(string table ,string name);

		/// <summary>
		/// Create table for saving diagrams
		/// </summary>
		void CreateDiagramTable();

		/// <summary>
		/// Save new diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		int InsertDiagram(string name, XDocument data);

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		int UpdateDiagram(string name, XDocument data);

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
	}
}