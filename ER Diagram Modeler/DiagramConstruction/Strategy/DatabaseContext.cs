﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public class DatabaseContext
	{
		private IDatabseStrategy _strategy;

		public DatabaseContext(ConnectionType connectionType)
		{
			SetStrategy(connectionType);
		}

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

		public TableModel ReadTableDetails(string id, string name)
		{
			return _strategy.ReadTableDetails(id, name);
		}

		public IEnumerable<RelationshipModel> ListRelationshipsForTable(string name, IEnumerable<TableModel> tables)
		{
			return _strategy.ReadRelationshipModels(name, tables);
		}

		public TableRowModel GetPlaceholderRowModel()
		{
			return _strategy.PlaceholderRowModel();
		}

		public void CreateTable(string name)
		{
			_strategy.CreateTable(name);
		}

		public IEnumerable<TableModel> ListTables()
		{
			return _strategy.ListTables();
		}

		public IEnumerable<string> ListAllForeignKeys()
		{
			return _strategy.ListAllForeignKeys();
		}

		public void RenameTable(string oldName, string newName)
		{
			_strategy.RenameTable(oldName, newName);
		}

		public void AddColumn(string table, TableRowModel model)
		{
			_strategy.AddColumn(table, model);
		}

		public void UpdateColumn(string table, TableRowModel model)
		{
			_strategy.UpdateColumn(table, model);
		}

		public void RenameColumn(string table, string oldName, string newName)
		{
			_strategy.RenameColumn(table, oldName, newName);
		}

		public void RemoveColumn(string table, string column)
		{
			_strategy.RemoveColumn(table, column);
		}

		public void RemoveTable(TableModel table)
		{
			_strategy.RemoveTable(table);
		}

		public void UpdatePrimaryKeyConstraint(TableModel table)
		{
			_strategy.UpdatePrimaryKeyConstraint(table);
		}

		public void RemoveRelationship(RelationshipModel model)
		{
			_strategy.RemoveRelationship(model);
		}

		public void AddRelationship(RelationshipModel model)
		{
			_strategy.AddRelationship(model);
		}

		public bool AreRelationshipModelsTheSame(RelationshipModel model1, RelationshipModel model2)
		{
			return _strategy.Comparer.Compare(model1, model2) == 1;
		}

		public int SaveDiagram(string name, XDocument data)
		{
			return _strategy.SaveDiagram(name, data);
		}

		public int DeleteDiagram(string name)
		{
			return _strategy.DeleteDiagram(name);
		}

		public IEnumerable<DiagramModel> SelectDiagrams()
		{
			return _strategy.SelectDiagrams();
		}

		public DataSet ExecuteRawQuery(string sql)
		{
			return _strategy.ExecuteRawQuery(sql);
		}
	}
}