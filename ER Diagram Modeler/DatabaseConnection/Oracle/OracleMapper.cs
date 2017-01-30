using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using Oracle.ManagedDataAccess.Client;
using Xceed.Wpf.DataGrid;

namespace ER_Diagram_Modeler.DatabaseConnection.Oracle
{
	public class OracleMapper: IOracleMapper
	{
		private static string SqlListTables = @"SELECT OBJECT_NAME, OBJECT_ID FROM ALL_OBJECTS WHERE OBJECT_TYPE = 'TABLE' AND OWNER = :Owner AND OBJECT_NAME <> '__ERDiagramModelerData'";
		private static string SqlTableDetails = @"SELECT COLUMN_NAME, COLUMN_ID, DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE FROM ALL_TAB_COLUMNS WHERE TABLE_NAME = :TableName";
		private static string SqlPrimaryKey = @"SELECT CONSTRAINT_NAME FROM ALL_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'P' AND Owner = :Owner AND TABLE_NAME = :TableName";
		private static string SqlConsColumns = @"SELECT COLUMN_NAME FROM ALL_CONS_COLUMNS WHERE CONSTRAINT_NAME = :Cons";
		private static string SqlForeignKeys = @"SELECT a.CONSTRAINT_NAME, c1.TABLE_NAME, c1.COLUMN_NAME, c2.TABLE_NAME, c2.COLUMN_NAME, c1.POSITION, c2.POSITION 
FROM ALL_CONSTRAINTS a
  JOIN ALL_CONS_COLUMNS c1 ON a.CONSTRAINT_NAME = c1.CONSTRAINT_NAME
  JOIN ALL_CONS_COLUMNS c2 ON a.R_CONSTRAINT_NAME = c2.CONSTRAINT_NAME
WHERE a.CONSTRAINT_TYPE = 'R'
  AND a.Owner = :Owner
  AND (c1.TABLE_NAME = :TableName1 OR c2.TABLE_NAME = :TableName2)
  AND c1.POSITION = c2.POSITION";

		

		public OracleDatabase Database { get; set; }
		public string Owner { get; set; }

		public OracleMapper()
		{
			Database = new OracleDatabase();
			Database.Connect(SessionProvider.Instance.ConnectionString);
			Owner = SessionProvider.Instance.Username.ToUpper();
		}

		public OracleMapper(string connString)
		{
			Database = new OracleDatabase();
			Database.Connect(connString);
		}

		public OracleMapper(string connString, string owner)
		{
			Database = new OracleDatabase();
			Owner = owner;
			Database.Connect(connString);
		}

		public void Dispose()
		{
			if(Database.Connection.State == ConnectionState.Open)
			{
				Database.Close();
			}
		}

		public IEnumerable<ForeignKeyDto> ListForeignKeys(string tableName)
		{
			OracleCommand command = Database.CreateCommand(SqlForeignKeys);
			command.Parameters.Add("Owner", OracleDbType.Varchar2, Owner, ParameterDirection.Input);
			command.Parameters.Add("TableName1", OracleDbType.Varchar2, tableName, ParameterDirection.Input);
			command.Parameters.Add("TableName2", OracleDbType.Varchar2, tableName, ParameterDirection.Input);
			OracleDataReader reader = command.ExecuteReader();
			var res = ReadForeignKeys(reader);
			reader.Close();
			return res;
		}

		private IEnumerable<ForeignKeyDto> ReadForeignKeys(OracleDataReader reader)
		{
			var res = new List<ForeignKeyDto>();

			while (reader.Read())
			{
				var dto = new ForeignKeyDto();
				int i = 0;
				dto.Name = reader.GetString(i++);
				dto.ForeignKeyTable = reader.GetString(i++);
				dto.ForeignKeyCollumn = reader.GetString(i++);
				dto.PrimaryKeyTable = reader.GetString(i++);
				dto.PrimaryKeyCollumn = reader.GetString(i++);

				var pos1 = reader.GetInt32(i++);
				var pos2 = reader.GetInt32(i++);

				res.Add(dto);
			}

			return res;
		}

		public void CreateTable(string name)
		{
			throw new System.NotImplementedException();
		}

		public TableModel SelectTableDetails(string id, string name)
		{
			OracleCommand command = Database.CreateCommand(SqlTableDetails);
			command.Parameters.Add("TableName", OracleDbType.Varchar2, name, ParameterDirection.Input);
			OracleDataReader reader = command.ExecuteReader();
			var res = ReadTableDetails(reader);
			reader.Close();

			OracleCommand commandPk = Database.CreateCommand(SqlPrimaryKey);
			commandPk.Parameters.Add("Owner", OracleDbType.Varchar2, Owner, ParameterDirection.Input);
			commandPk.Parameters.Add("TableName", OracleDbType.Varchar2, name, ParameterDirection.Input);
			OracleDataReader readerPk = commandPk.ExecuteReader(CommandBehavior.SingleRow);
			readerPk.Read();
			string pkConstraint = readerPk.GetString(0);
			readerPk.Close();

			OracleCommand commandPkCols = Database.CreateCommand(SqlConsColumns);
			commandPkCols.Parameters.Add("Cons", OracleDbType.Varchar2, pkConstraint, ParameterDirection.Input);
			OracleDataReader readerPkCons = commandPkCols.ExecuteReader();
			ReadTablePrimaryKey(readerPkCons, res, pkConstraint);
			readerPkCons.Close();
			return res;
		}

		private void ReadTablePrimaryKey(OracleDataReader reader, TableModel table, string consName)
		{
			while (reader.Read())
			{
				var column = reader.GetString(0);

				TableRowModel model = table.Attributes.FirstOrDefault(t => t.Name.Equals(column, StringComparison.InvariantCultureIgnoreCase));

				if (model != null)
				{
					model.PrimaryKey = true;
					model.PrimaryKeyConstraintName = consName;
				}
			}
		}

		private TableModel ReadTableDetails(OracleDataReader reader)
		{
			var res = new TableModel();

			while(reader.Read())
			{
				int i = 0;
				var row = new TableRowModel();
				row.Name = reader.GetString(i++);
				row.Id = reader.GetInt32(i++).ToString();

				var typeName = reader.GetString(i++);
				var len = reader.IsDBNull(i) ? 0 : reader.GetInt32(i);
				i++;
				var precision = reader.IsDBNull(i) ? 0 : reader.GetInt32(i);
				i++;
				var scale = reader.IsDBNull(i) ? 0 : reader.GetInt32(i);
				i++;

				row.AllowNull = !reader.GetString(i++).Equals("N");

				var type =
					row.DatatypesItemSource.FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));

				type.Lenght = type.HasLenght ? (int)len : type.Lenght;
				type.Precision = type.HasPrecision ? (int)precision : type.Precision;
				type.Scale = type.HasScale ? (int)scale : type.Scale;

				row.Datatype = type;
				row.LoadeDatatypeFromDb = type;
				row.SelectedDatatype = type;
				res.Attributes.Add(row);
			}

			return res;
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

		public IEnumerable<TableModel> ListTables()
		{
			OracleCommand command = Database.CreateCommand(SqlListTables);
			command.Parameters.Add("Owner", OracleDbType.Varchar2, Owner, ParameterDirection.Input);
			OracleDataReader reader = command.ExecuteReader();
			var res = ReadTableList(reader);
			reader.Close();
			return res;
		}

		private IEnumerable<TableModel> ReadTableList(OracleDataReader reader)
		{
			var res = new List<TableModel>();

			while (reader.Read())
			{
				var table = new TableModel();
				int i = 0;
				table.Title = reader.GetString(i++);
				table.Id = reader.GetInt32(i++).ToString();
				res.Add(table);
			}

			return res;
		}
	}
}