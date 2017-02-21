using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ER_Diagram_Modeler.CommandOutput;
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
	/// <summary>
	/// Mapper implementation for Oracle
	/// </summary>
	public class OracleMapper: IOracleMapper
	{
		private static string SqlListTables = @"SELECT OBJECT_NAME, OBJECT_ID FROM SYS.ALL_OBJECTS WHERE OBJECT_TYPE = 'TABLE' AND OWNER = :Owner AND OBJECT_NAME <> '__ERDIAGRAMS'";
		private static string SqlTableDetails = @"SELECT COLUMN_NAME, COLUMN_ID, DATA_TYPE, DATA_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE FROM SYS.ALL_TAB_COLUMNS WHERE TABLE_NAME = :TableName";
		private static string SqlPrimaryKey = @"SELECT CONSTRAINT_NAME FROM SYS.ALL_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'P' AND Owner = :Owner AND TABLE_NAME = :TableName";
		private static string SqlConsColumns = @"SELECT COLUMN_NAME FROM SYS.ALL_CONS_COLUMNS WHERE CONSTRAINT_NAME = :Cons";
		private static string SqlForeignKeys = @"SELECT a.CONSTRAINT_NAME, c1.TABLE_NAME, c1.COLUMN_NAME, c2.TABLE_NAME, c2.COLUMN_NAME, a.DELETE_RULE, a.LAST_CHANGE 
FROM SYS.ALL_CONSTRAINTS a
  JOIN SYS.ALL_CONS_COLUMNS c1 ON a.CONSTRAINT_NAME = c1.CONSTRAINT_NAME
  JOIN SYS.ALL_CONS_COLUMNS c2 ON a.R_CONSTRAINT_NAME = c2.CONSTRAINT_NAME
WHERE a.CONSTRAINT_TYPE = 'R'
  AND a.Owner = :Owner
  AND (c1.TABLE_NAME = :TableName1 OR c2.TABLE_NAME = :TableName2)
  AND c1.POSITION = c2.POSITION";

		private static string SqlAllForeignKeys = @"SELECT a.CONSTRAINT_NAME FROM SYS.ALL_CONSTRAINTS a WHERE a.CONSTRAINT_TYPE = 'R' AND a.Owner = :Owner";
		private static string SqlCreateTable = "CREATE TABLE {0} (Id{1} NUMBER PRIMARY KEY)";
		private static string SqlRenameTable = "RENAME \"{0}\" TO \"{1}\"";
		private static string SqlDropTable = "DROP TABLE {0}";

		private static string SqlAddColumn = "ALTER TABLE {0} ADD {1}";
		private static string SqlModifyColumn = "ALTER TABLE {0} MODIFY {1}";
		private static string SqlDropColumn = "ALTER TABLE {0} DROP COLUMN {1}";
		private static string SqlRenameColumn = "ALTER TABLE {0} RENAME COLUMN {1} TO {2}";

		private static string SqlDropConstraint = "ALTER TABLE {0} DROP CONSTRAINT {1}";
		private static string SqlAddPrimaryKeyConstraint = "ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})";
		private static string SqlAddForeignKeyConstraint = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})";
		private static string SqlAddForeignKeyConstraint2 = "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}) {5}";

		private static string SqlCreateDiagramTable = @"CREATE TABLE {0} (NAME VARCHAR2(100) PRIMARY KEY, DATA CLOB NOT NULL)";
		private static string SqlDoesDiagramTableExist = @"SELECT count(*) FROM ALL_TABLES a WHERE a.OWNER = :Owner AND a.TABLE_NAME = 'ERDIAGRAMS'";

		private static string SqlInsertDiagram = @"INSERT INTO {0}(NAME, DATA) VALUES(:Name, :Data)";
		private static string SqlUpdateDiagram = @"UPDATE {0} SET DATA = :Data WHERE NAME = :Name";
		private static string SqlSelectDiagrams = @"SELECT NAME, DATA FROM {0}";
		private static string SqlDeleteDiagram = @"DELETE FROM {0} WHERE NAME = :Name";

		/// <summary>
		/// Current DB Connection
		/// </summary>
		public OracleDatabase Database { get; set; }

		/// <summary>
		/// Owner of current schema
		/// </summary>
		public string Owner { get; set; }

		/// <summary>
		/// Connect to session based DB
		/// </summary>
		public OracleMapper()
		{
			Database = new OracleDatabase();
			Database.Connect(SessionProvider.Instance.ConnectionString);
			Owner = SessionProvider.Instance.Username.ToUpper();
		}

		/// <summary>
		/// Connect to DB
		/// </summary>
		/// <param name="connString">Connection string</param>
		public OracleMapper(string connString)
		{
			Database = new OracleDatabase();
			Database.Connect(connString);
		}

		/// <summary>
		/// Connect to DB
		/// </summary>
		/// <param name="connString">Connection string</param>
		/// <param name="owner">Schema owner</param>
		public OracleMapper(string connString, string owner)
		{
			Database = new OracleDatabase();
			Owner = owner;
			Database.Connect(connString);
		}

		/// <summary>
		/// Close DB Connection
		/// </summary>
		public void Dispose()
		{
			if(Database.Connection.State == ConnectionState.Open)
			{
				Database.Close();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
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

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		public IEnumerable<string> ListAllForeignKeys()
		{
			OracleCommand command = Database.CreateCommand(SqlAllForeignKeys);
			command.Parameters.Add("Owner", OracleDbType.Varchar2, Owner, ParameterDirection.Input);
			OracleDataReader reader = command.ExecuteReader();
			var res = ReadForeignKeyNames(reader);
			reader.Close();
			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Oracle data reader</param>
		/// <returns>Collection of read data</returns>
		private IEnumerable<string> ReadForeignKeyNames(OracleDataReader reader)
		{
			var res = new List<string>();

			while (reader.Read())
			{
				res.Add(reader.GetString(0));
			}

			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Oracle data reader</param>
		/// <returns>Collection of read data</returns>
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
				dto.DeleteAction = reader.GetString(i++);
				dto.LastModified = reader.GetDateTime(i++);
				dto.UpdateAction = dto.DeleteAction;
				res.Add(dto);
			}

			return res;
		}

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		public void CreateTable(string name)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlCreateTable, TableNameWithOwnerCaseSensitve(name), name));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
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
			bool read = readerPk.Read();

			if (read)
			{
				string pkConstraint = readerPk.GetString(0);
				OracleCommand commandPkCols = Database.CreateCommand(SqlConsColumns);
				commandPkCols.Parameters.Add("Cons", OracleDbType.Varchar2, pkConstraint, ParameterDirection.Input);
				OracleDataReader readerPkCons = commandPkCols.ExecuteReader();
				ReadTablePrimaryKey(readerPkCons, res, pkConstraint);
				readerPkCons.Close();
			}
			readerPk.Close();
			res.Id = id;
			res.Title = name;
			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Oracle data reader</param>
		/// <param name="table">Table model</param>
		/// <param name="consName">PK Constraint name</param>
		/// <returns>Collection of read data</returns>
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

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Read data fro table</returns>
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

				if (type == null)
				{
					var regex = new Regex("^timestamp(\\((\\d)\\))?");
					if (regex.IsMatch(typeName.ToLower()))
					{
						type =
							row.DatatypesItemSource.FirstOrDefault(
								t => t.Name.Equals("timestamp", StringComparison.InvariantCultureIgnoreCase));

						
					}
					if(type == null)
					{
						continue;
					}
				}

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

		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		public void RenameTable(string oldName, string newName)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlRenameTable, oldName, newName));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void AddNewColumn(string table, TableRowModel model)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlAddColumn, TableNameWithOwnerCaseSensitve(table), model));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void AlterColumn(string table, TableRowModel model)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlModifyColumn, TableNameWithOwnerCaseSensitve(table), model));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Rename column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		public void RenameColumn(string table, string oldName, string newName)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlRenameColumn, TableNameWithOwnerCaseSensitve(table), oldName, newName));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		public void DropColumn(string table, string column)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlDropColumn, TableNameWithOwnerCaseSensitve(table), column));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		public void DropTable(string table)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlDropTable, TableNameWithOwnerCaseSensitve(table)));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop primary key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="primaryKeyConstraintName">Constraint name</param>
		public void DropPrimaryKey(string table, string primaryKeyConstraintName)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlDropConstraint, TableNameWithOwnerCaseSensitve(table), primaryKeyConstraintName));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Create new primary key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="columns">Name of primary key columns</param>
		public void CreatePrimaryKey(string table, string[] columns)
		{
			string cols = string.Join(",", columns);
			OracleCommand command = Database.CreateCommand(string.Format(SqlAddPrimaryKeyConstraint, TableNameWithOwnerCaseSensitve(table), $"PK_{table}", cols));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Create new foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="referencedTable">Referenced table name</param>
		/// <param name="collumns">Collection of column models</param>
		/// <param name="fkName">Name of constraint, generated if null</param>
		public void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null)
		{
			string name = fkName ?? $"{table}_{referencedTable}_FK";
			string tableColumns = string.Join(",", collumns.Select(t => t.Destination.Name));
			string referencedColumns = string.Join(",", collumns.Select(t => t.Source.Name));

			OracleCommand command = Database.CreateCommand(string.Format(SqlAddForeignKeyConstraint, TableNameWithOwnerCaseSensitve(table), name, tableColumns, TableNameWithOwnerCaseSensitve(referencedTable), referencedColumns));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="name">Constraint name</param>
		public void DropForeignKey(string table, string name)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlDropConstraint, TableNameWithOwnerCaseSensitve(table), name));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Create table for saving diagrams
		/// </summary>
		public void CreateDiagramTable()
		{
			OracleCommand command = Database.CreateCommand(SqlDoesDiagramTableExist);
			command.Parameters.Add("Owner", OracleDbType.Varchar2, Owner, ParameterDirection.Input);
			var count = (decimal) command.ExecuteScalar();

			if(count == 0)
			{
				OracleCommand commandCreate = Database.CreateCommand(string.Format(SqlCreateDiagramTable, DiagramTable));
				commandCreate.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Save new diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		public int InsertDiagram(string name, XDocument data)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlInsertDiagram, DiagramTable));
			command.Parameters.Add("Name", OracleDbType.Varchar2, name, ParameterDirection.Input);
			command.Parameters.Add("Data", OracleDbType.Varchar2, data.ToString(), ParameterDirection.Input);
			return command.ExecuteNonQuery();
		}

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		public int UpdateDiagram(string name, XDocument data)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlUpdateDiagram, DiagramTable));
			command.Parameters.Add("Data", OracleDbType.Varchar2, data.ToString(), ParameterDirection.Input);
			command.Parameters.Add("Name", OracleDbType.Varchar2, name, ParameterDirection.Input);
			return command.ExecuteNonQuery();
		}

		/// <summary>
		/// Delete diagram from DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <returns>One if successful, zero if not</returns>
		public int DeleteDiagram(string name)
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlDeleteDiagram, DiagramTable));
			command.Parameters.Add("Name", OracleDbType.Varchar2, name, ParameterDirection.Input);
			return command.ExecuteNonQuery();
		}

		/// <summary>
		/// Select existing diagrams
		/// </summary>
		/// <returns>Collections of diagrams</returns>
		public IEnumerable<DiagramModel> SelectDiagrams()
		{
			OracleCommand command = Database.CreateCommand(string.Format(SqlSelectDiagrams, DiagramTable));
			OracleDataReader reader = command.ExecuteReader();
			var res = ReadDiagrams(reader);
			reader.Close();
			return res;
		}

		/// <summary>
		/// Execute raw query
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>Dataset with results</returns>
		public DataSet ExecuteRawQuery(string sql)
		{
			OracleCommand command = Database.CreateCommand(sql);
			Output.WriteLine(command.CommandText);
			var dataset = new DataSet("Result");
			var adapter = new OracleDataAdapter(command);
			adapter.Fill(dataset);
			return dataset;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		private IEnumerable<DiagramModel> ReadDiagrams(OracleDataReader reader)
		{
			var res = new List<DiagramModel>();

			while (reader.Read())
			{
				var diagram = new DiagramModel();
				int i = 0;
				diagram.Name = reader.GetString(i++);
				diagram.Xml = reader.GetString(i++);
				res.Add(diagram);
			}

			return res;
		}

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void AlterColumn(string table, TableRowModel model, bool modifyNull = true)
		{
			if (!modifyNull)
			{
				OracleCommand command = Database.CreateCommand(string.Format(SqlModifyColumn, TableNameWithOwnerCaseSensitve(table), $"{model.Name} {model.Datatype}"));
				command.ExecuteNonQuery();
				return;
			}
			AlterColumn(table, model);
		}

		/// <summary>
		/// List tables in DB
		/// </summary>
		/// <returns>Collection of tables with ID and name</returns>
		public IEnumerable<TableModel> ListTables()
		{
			OracleCommand command = Database.CreateCommand(SqlListTables);
			command.Parameters.Add("Owner", OracleDbType.Varchar2, Owner, ParameterDirection.Input);
			OracleDataReader reader = command.ExecuteReader();
			var res = ReadTableList(reader);
			reader.Close();
			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
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

		private string TableNameWithOwner(string table) => $"{Owner}.{table}";
		private string TableNameWithOwnerCaseSensitve(string table) => $"{Owner}.\"{table}\"";
		private string DiagramTable => $"{Owner}.\"ERDIAGRAMS\"";

		/// <summary>
		/// Create new foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="referencedTable">Referenced table name</param>
		/// <param name="collumns">Collection of constraint columns</param>
		/// <param name="fkName">Name of constraint, generated if NULL</param>
		/// <param name="onDelete">Action on delete, all standard MS Sql actions</param>
		public void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null, string onDelete = "NO ACTION")
		{
			string name = fkName ?? $"{table}_{referencedTable}_FK";
			string tableColumns = string.Join(",", collumns.Select(t => t.Destination.Name));
			string referencedColumns = string.Join(",", collumns.Select(t => t.Source.Name));
			string commandStr;

			if (onDelete.ToUpper().Equals("NO ACTION"))
			{
				commandStr = string.Format(SqlAddForeignKeyConstraint, TableNameWithOwnerCaseSensitve(table), name,
				tableColumns, TableNameWithOwnerCaseSensitve(referencedTable), referencedColumns);
			}
			else
			{
				commandStr = string.Format(SqlAddForeignKeyConstraint2, TableNameWithOwnerCaseSensitve(table), name,
				tableColumns, TableNameWithOwnerCaseSensitve(referencedTable), referencedColumns, $"ON DELETE {onDelete}");
			}
			
			OracleCommand command = Database.CreateCommand(commandStr);
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}
	}
}