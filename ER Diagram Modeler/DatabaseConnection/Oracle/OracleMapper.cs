﻿using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public class OracleMapper: IOracleMapper
	{
		private readonly OracleDatabase _database = new OracleDatabase();

		public OracleMapper()
		{
			_database.Connect();
		}

		public void Dispose()
		{
			if(_database.Connection.State == ConnectionState.Open)
			{
				_database.Close();
			}
		}


		public void CreateTable(string name)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<TableModel> ListTables()
		{
			throw new System.NotImplementedException();
		}

		public TableModel SelectTableDetails(string id, string name)
		{
			throw new System.NotImplementedException();
		}

		public void RenameTable(string oldName, string newName)
		{
			throw new System.NotImplementedException();
		}

		public void AddNewColumn(string table, TableRowModel model)
		{
			throw new System.NotImplementedException();
		}

		public void AlterColumn(string table, TableRowModel model)
		{
			throw new System.NotImplementedException();
		}

		public void RenameColumn(string table, string oldName, string newName)
		{
			throw new System.NotImplementedException();
		}

		public void DropColumn(string table, string column)
		{
			throw new System.NotImplementedException();
		}

		public void DropTable(string table)
		{
			throw new System.NotImplementedException();
		}

		public void DropPrimaryKey(string table, string primaryKeyConstraintName)
		{
			throw new System.NotImplementedException();
		}

		public void CreatePrimaryKey(string table, string[] columns)
		{
			throw new System.NotImplementedException();
		}

		public void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null)
		{
			throw new System.NotImplementedException();
		}

		public void DropForeignKey(string table, string name)
		{
			throw new System.NotImplementedException();
		}
	}
}