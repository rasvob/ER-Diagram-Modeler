using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	/// <summary>
	/// Strategy operations context
	/// </summary>
	public class DatabaseContext
	{
		private IDatabseStrategy _strategy;

		public DatabaseContext(ConnectionType connectionType)
		{
			SetStrategy(connectionType);
		}

		/// <summary>
		/// Strategy setter
		/// </summary>
		/// <param name="connectionType">Conncection type</param>
		public void SetStrategy(ConnectionType connectionType)
		{
			switch(connectionType)
			{
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

		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
		public TableModel ReadTableDetails(string id, string name)
		{
			return _strategy.ReadTableDetails(id, name);
		}

		/// <summary>
		/// List foreign key coninstraints for table
		/// </summary>
		/// <param name="name">Name of table</param>
		/// <param name="tables">Tables in designer</param>
		/// <returns>Collection of FK constraints</returns>
		public IEnumerable<RelationshipModel> ListRelationshipsForTable(string name, IEnumerable<TableModel> tables)
		{
			return _strategy.ReadRelationshipModels(name, tables);
		}

		/// <summary>
		/// Return default column by connection type
		/// </summary>
		/// <returns>Default column</returns>
		public TableRowModel GetPlaceholderRowModel()
		{
			return _strategy.PlaceholderRowModel();
		}

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		public void CreateTable(string name)
		{
			_strategy.CreateTable(name);
		}

		/// <summary>
		/// List tables in DB
		/// </summary>
		/// <returns>Collection of tables with ID and name</returns>
		public IEnumerable<TableModel> ListTables()
		{
			return _strategy.ListTables();
		}

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		public IEnumerable<string> ListAllForeignKeys()
		{
			return _strategy.ListAllForeignKeys();
		}

		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		public void RenameTable(string oldName, string newName)
		{
			_strategy.RenameTable(oldName, newName);
		}

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void AddColumn(string table, TableRowModel model)
		{
			_strategy.AddColumn(table, model);
		}

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void UpdateColumn(string table, TableRowModel model)
		{
			_strategy.UpdateColumn(table, model);
		}

		/// <summary>
		/// Rename column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		public void RenameColumn(string table, string oldName, string newName)
		{
			_strategy.RenameColumn(table, oldName, newName);
		}

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		public void RemoveColumn(string table, string column)
		{
			_strategy.RemoveColumn(table, column);
		}

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		public void RemoveTable(TableModel table)
		{
			_strategy.RemoveTable(table);
		}

		/// <summary>
		/// Update PK constraint
		/// </summary>
		/// <param name="table">Table model</param>
		public void UpdatePrimaryKeyConstraint(TableModel table)
		{
			_strategy.UpdatePrimaryKeyConstraint(table);
		}

		/// <summary>
		/// Drop foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		public void RemoveRelationship(RelationshipModel model)
		{
			_strategy.RemoveRelationship(model);
		}

		/// <summary>
		/// Create foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		public void AddRelationship(RelationshipModel model)
		{
			_strategy.AddRelationship(model);
		}

		/// <summary>
		/// Compare two given foreign key constraints by values
		/// </summary>
		/// <param name="model1">Relationship model</param>
		/// <param name="model2">Relationship model</param>
		/// <returns>True if models are the same, false if not</returns>
		public bool AreRelationshipModelsTheSame(RelationshipModel model1, RelationshipModel model2)
		{
			return _strategy.Comparer.Compare(model1, model2) == 1;
		}

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		public int SaveDiagram(string name, XDocument data)
		{
			return _strategy.SaveDiagram(name, data);
		}

		/// <summary>
		/// Delete diagram from DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <returns>One if successful, zero if not</returns>
		public int DeleteDiagram(string name)
		{
			return _strategy.DeleteDiagram(name);
		}

		/// <summary>
		/// Select existing diagrams
		/// </summary>
		/// <returns>Collections of diagrams</returns>
		public IEnumerable<DiagramModel> SelectDiagrams()
		{
			return _strategy.SelectDiagrams();
		}

		/// <summary>
		/// Execute raw query
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>Dataset with results</returns>
		public DataSet ExecuteRawQuery(string sql)
		{
			return _strategy.ExecuteRawQuery(sql);
		}
	}
}