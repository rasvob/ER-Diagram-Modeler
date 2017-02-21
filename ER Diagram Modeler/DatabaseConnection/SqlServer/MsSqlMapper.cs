using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	/// <summary>
	/// Mapper implementation for MS Sql Server
	/// </summary>
	public class MsSqlMapper: IMsSqlMapper
	{
		private static string SqlListDatabases = @"SELECT database_id, name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');";
		private static string SqlListTables = @"SELECT object_id, name FROM sys.tables WHERE name <> 'sysdiagrams' AND name <> 'ERDIAGRAMS';";
		private static string SqlTableDetails = @"SELECT s.column_id, s.name, s.is_nullable, t.name, s.max_length, s.precision, s.scale FROM sys.columns s JOIN sys.types t ON s.system_type_id = t.system_type_id WHERE s.object_id = @Id;";
		private static string SqlPrimaryKeys = @"sp_pkeys";
		private static string SqlForeignKeys = @"sp_fkeys";
		private static string SqlForeignKeysReferentialAction = @"SELECT f.name, f.delete_referential_action_desc, f.update_referential_action_desc, f.object_id, f.modify_date FROM sys.foreign_keys f";
		private static string SqlCreateDatabase = @"CREATE DATABASE";
		private static string SqlDropDatabase = @"DROP DATABASE";
		private static string SqlCreateTable = @"CREATE TABLE {0} (Id{0} INT PRIMARY KEY);";
		private static string SqlRenameTable = @"sp_rename";
		private static string SqlAddColumn = @"ALTER TABLE [{0}] ADD {1}";
		private static string SqlAlterColumn = @"ALTER TABLE [{0}] ALTER COLUMN {1}";
		private static string SqlDropColumn = @"ALTER TABLE [{0}] DROP COLUMN {1}";
		private static string SqlDropTable = @"DROP TABLE [{0}]";
		private static string SqlDropConstraint = @"ALTER TABLE [{0}] DROP CONSTRAINT {1}";
		private static string SqlAddPrimaryKeyConstraint = @"ALTER TABLE [{0}] ADD CONSTRAINT {1} PRIMARY KEY CLUSTERED ({2})";
		private static string SqlAddForeignKeyConstraint = @"ALTER TABLE [{0}] ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES [{3}] ({4})";
		private static string SqlAddForeignKeyConstraint2 = @"ALTER TABLE [{0}] ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES [{3}] ({4}) ON DELETE {5} ON UPDATE {6}";
		private static string SqlCreateDiagramTable = @"IF (NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ERDIAGRAMS'))
BEGIN
	CREATE TABLE ERDIAGRAMS (NAME VARCHAR(100) PRIMARY KEY, DATA XML NOT NULL)
END";
		private static string SqlInsertDiagram = @"INSERT INTO ERDIAGRAMS(NAME, DATA) VALUES(@Name, @Data)";
		private static string SqlUpdateDiagram = @"UPDATE ERDIAGRAMS SET DATA = @Data WHERE NAME = @Name";
		private static string SqlSelectDiagrams = @"SELECT NAME, DATA FROM ERDIAGRAMS";
		private static string SqlDeleteDiagram = @"DELETE FROM ERDIAGRAMS WHERE NAME = @Name";

		/// <summary>
		/// Current DB Connection
		/// </summary>
		public MsSqlDatabase Database { get; set; }

		/// <summary>
		/// Connect to session based DB
		/// </summary>
		public MsSqlMapper()
		{
			Database = new MsSqlDatabase();
			Database.Connect(SessionProvider.Instance.ConnectionString);
		}

		/// <summary>
		/// Connect to DB
		/// </summary>
		/// <param name="connString">Connection string</param>
		public MsSqlMapper(string connString)
		{
			Database = new MsSqlDatabase();
			Database.Connect(connString);
		}

		/// <summary>
		/// Close DB Connection
		/// </summary>
		public void Dispose()
		{
			if (Database.Connection.State == ConnectionState.Open)
			{
				Database.Close();
			}
		}

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		public IEnumerable<string> ListAllForeignKeys()
		{
			SqlCommand command = Database.CreateCommand(SqlForeignKeysReferentialAction);
			SqlDataReader refReader = command.ExecuteReader();
			var res = ReadForeignKeyNames(refReader);
			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="refReader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		private IEnumerable<string> ReadForeignKeyNames(SqlDataReader refReader)
		{
			var res = new List<string>();

			while (refReader.Read())
			{
				res.Add(refReader.GetString(0));
			}

			return res;
		}

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		public void CreateTable(string name)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlCreateTable, name));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// List all databases of given instance
		/// </summary>
		/// <returns>Collection of databases</returns>
		public IEnumerable<DatabaseInfo> ListDatabases()
		{
			SqlCommand command = Database.CreateCommand(SqlListDatabases);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadListDatabases(reader);
			reader.Close();
			return res;
		}

		/// <summary>
		/// List foreign key coninstraints for table
		/// </summary>
		/// <param name="tableName">Name of table</param>
		/// <returns>Collection of constraint</returns>
		public IEnumerable<ForeignKeyDto> ListForeignKeys(string tableName)
		{
			SqlCommand commandPrimaryKeys = Database.CreateCommand(SqlForeignKeys);
			commandPrimaryKeys.CommandType = CommandType.StoredProcedure;
			commandPrimaryKeys.Parameters.AddWithValue("pktable_name", tableName);
			var pkReader = commandPrimaryKeys.ExecuteReader();

			var list1 = ReadForeignKeys(pkReader);
			pkReader.Close();

			SqlCommand commandForeignKeys = Database.CreateCommand(SqlForeignKeys);
			commandForeignKeys.CommandType = CommandType.StoredProcedure;
			commandForeignKeys.Parameters.AddWithValue("fktable_name", tableName);
			var fkReader = commandForeignKeys.ExecuteReader();

			var list2 = ReadForeignKeys(fkReader);
			fkReader.Close();

			var msSqlForeignKeyDtos = list1.ToList();
			msSqlForeignKeyDtos.AddRange(list2);

			SqlCommand commandReferentialActions = Database.CreateCommand(SqlForeignKeysReferentialAction);
			SqlDataReader refReader = commandReferentialActions.ExecuteReader();
			ReadReferentialActions(refReader, msSqlForeignKeyDtos);
			refReader.Close();

			return msSqlForeignKeyDtos;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <param name="dtos">Collection of FK Constraint DTO</param>
		public void ReadReferentialActions(SqlDataReader reader, IEnumerable<ForeignKeyDto> dtos)
		{
			while (reader.Read())
			{
				int i = 0;
				string name = reader.GetString(i++);
				string delete = reader.GetString(i++);
				string update = reader.GetString(i++);
				string id = reader.GetInt32(i++).ToString();
				DateTime date = reader.GetDateTime(i++);

				var dto = dtos.Where(t => t.Name.Equals(name));

				foreach (ForeignKeyDto keyDto in dto)
				{
					keyDto.DeleteAction = delete;
					keyDto.UpdateAction = update;
					keyDto.Id = id;
					keyDto.LastModified = date;
				}
			}
		}

		/// <summary>
		/// Create new database
		/// </summary>
		/// <param name="name">DB name</param>
		public void CreateDatabase(string name)
		{
			SqlCommand command = Database.CreateCommand($"{SqlCreateDatabase} [{name}]");
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop database
		/// </summary>
		/// <param name="name">DB Name</param>
		public void DropDatabase(string name)
		{
			SqlCommand commandAlter = Database.CreateCommand($"alter database [{name}] set single_user with rollback immediate");
			Output.WriteLine(commandAlter.CommandText);
			commandAlter.ExecuteNonQuery();

			SqlCommand command = Database.CreateCommand($"{SqlDropDatabase} [{name}]");
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// List tables in DB
		/// </summary>
		/// <returns>Collection of tables with ID and name</returns>
		public IEnumerable<TableModel> ListTables()
		{
			SqlCommand command = Database.CreateCommand(SqlListTables);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadListTables(reader);
			reader.Close();
			return res;
		}

		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
		public TableModel SelectTableDetails(string id, string name)
		{
			SqlCommand command = Database.CreateCommand(SqlTableDetails);
			command.Parameters.AddWithValue("Id", id);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadTableDetails(reader);
			reader.Close();

			SqlCommand commandPrimaryKeys = Database.CreateCommand(SqlPrimaryKeys);
			commandPrimaryKeys.CommandType = CommandType.StoredProcedure;
			commandPrimaryKeys.Parameters.AddWithValue("table_name", name);
			var pkReader = commandPrimaryKeys.ExecuteReader();
			ReadPrimaryKeys(pkReader, res);
			pkReader.Close();

			res.Id = id;
			res.Title = name;

			return res;
		}

		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		public void RenameTable(string oldName, string newName)
		{
			SqlCommand command = Database.CreateCommand(SqlRenameTable);
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue("objname", oldName);
			command.Parameters.AddWithValue("newname", newName);
			Output.WriteLine(OutputPanelListener.PrepareSql(command, command.Parameters));
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void AddNewColumn(string table, TableRowModel model)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlAddColumn, table, model));
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
			SqlCommand command = Database.CreateCommand(string.Format(SqlAlterColumn, table, model));
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
			SqlCommand command = Database.CreateCommand(SqlRenameTable);
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue("objname", $"{table}.{oldName}");
			command.Parameters.AddWithValue("newname", newName);
			command.Parameters.AddWithValue("objtype", "COLUMN");
			Output.WriteLine(OutputPanelListener.PrepareSql(command, command.Parameters));
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		public void DropColumn(string table, string column)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropColumn, table, column));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		public void DropTable(string table)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropTable, table));
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
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropConstraint, table, primaryKeyConstraintName));
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
			SqlCommand command = Database.CreateCommand(string.Format(SqlAddPrimaryKeyConstraint, table, $"PK_{table}_{columns[0]}", string.Join(",", columns)));
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

			SqlCommand command = Database.CreateCommand(string.Format(SqlAddForeignKeyConstraint, table, name, tableColumns, referencedTable, referencedColumns));
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
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropConstraint, table, name));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Create table for saving diagrams
		/// </summary>
		public void CreateDiagramTable()
		{
			SqlCommand command = Database.CreateCommand(SqlCreateDiagramTable);
			command.ExecuteNonQuery();
		}

		/// <summary>
		/// Save new diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		public int InsertDiagram(string name, XDocument data)
		{
			SqlCommand command = Database.CreateCommand(SqlInsertDiagram);
			command.Parameters.AddWithValue("Name", name);
			command.Parameters.AddWithValue("Data", data.ToString());
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
			SqlCommand command = Database.CreateCommand(SqlUpdateDiagram);
			command.Parameters.AddWithValue("Name", name);
			command.Parameters.AddWithValue("Data", data.ToString());
			return command.ExecuteNonQuery();
		}

		/// <summary>
		/// Delete diagram from DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <returns>One if successful, zero if not</returns>
		public int DeleteDiagram(string name)
		{
			SqlCommand command = Database.CreateCommand(SqlDeleteDiagram);
			command.Parameters.AddWithValue("Name", name);
			return command.ExecuteNonQuery();
		}

		/// <summary>
		/// Select existing diagrams
		/// </summary>
		/// <returns>Collections of diagrams</returns>
		public IEnumerable<DiagramModel> SelectDiagrams()
		{
			SqlCommand command = Database.CreateCommand(SqlSelectDiagrams);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadDiagramDetails(reader);
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
			SqlCommand command = Database.CreateCommand(sql);
			Output.WriteLine(command.CommandText);
			var dataset = new DataSet("Result");
			var adapter = new SqlDataAdapter(command);
			adapter.Fill(dataset);
			return dataset;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		private IEnumerable<DiagramModel> ReadDiagramDetails(SqlDataReader reader)
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
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		private IEnumerable<TableModel> ReadListTables(SqlDataReader reader)
		{
			List<TableModel> res = new List<TableModel>();

			while (reader.Read())
			{
				var table = new TableModel();
				int i = 0;
				table.Id = reader.GetInt32(i++).ToString();
				table.Title = reader.GetString(i++);
				res.Add(table);
			}

			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		private IEnumerable<ForeignKeyDto> ReadForeignKeys(SqlDataReader reader)
		{
			var res = new List<ForeignKeyDto>();

			while (reader.Read())
			{
				var dto = new ForeignKeyDto();

				dto.PrimaryKeyTable = reader.GetString(2);
				dto.PrimaryKeyCollumn = reader.GetString(3);
				dto.ForeignKeyTable = reader.GetString(6);
				dto.ForeignKeyCollumn = reader.GetString(7);
				dto.Name = reader.GetString(11);

				res.Add(dto);
			}

			return res;
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <param name="model">Table model</param>
		/// <returns>Collection of read data</returns>
		private void ReadPrimaryKeys(SqlDataReader reader, TableModel model)
		{
			while (reader.Read())
			{
				var name = reader.GetString(3);
				var constraintName = reader.GetString(5);

				TableRowModel rowModel = model.Attributes.FirstOrDefault(t => t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
				if (rowModel != null)
				{
					rowModel.PrimaryKey = true;
					rowModel.PrimaryKeyConstraintName = constraintName;
				}
			}
		}

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		private TableModel ReadTableDetails(SqlDataReader reader)
		{
			var res = new TableModel();

			while (reader.Read())
			{
				int i = 0;
				var row = new TableRowModel();
				row.Id = reader.GetInt32(i++).ToString();
				row.Name = reader.GetString(i++);
				row.AllowNull = reader.GetBoolean(i++);

				var typeName = reader.GetString(i++);
				var len = reader.GetInt16(i++);
				var precision = reader.GetByte(i++);
				var scale = reader.GetByte(i++);

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

		/// <summary>
		/// Read data from reader
		/// </summary>
		/// <param name="reader">Sql data reader</param>
		/// <returns>Collection of read data</returns>
		public IEnumerable<DatabaseInfo> ReadListDatabases(SqlDataReader reader)
		{
			List<DatabaseInfo> res = new List<DatabaseInfo>();

			while (reader.Read())
			{
				int i = 0;
				var info = new DatabaseInfo();
				info.Id = reader.GetInt32(i++);
				info.Name = reader.GetString(i++);
				res.Add(info);
			}

			return res;
		}

		/// <summary>
		/// Create new foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="referencedTable">Referenced table name</param>
		/// <param name="collumns">Collection of constraint columns</param>
		/// <param name="fkName">Name of constraint, generated if NULL</param>
		/// <param name="onUpdate">Action on update, all standard MS Sql  actions</param>
		/// <param name="onDelete">Action on delete, all standard MS Sql actions</param>
		public void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null,
			string onUpdate = "NO ACTION", string onDelete = "NO ACTION")
		{
			string name = fkName ?? $"{table}_{referencedTable}_FK";
			string tableColumns = string.Join(",", collumns.Select(t => t.Destination.Name));
			string referencedColumns = string.Join(",", collumns.Select(t => t.Source.Name));

			SqlCommand command = Database.CreateCommand(string.Format(SqlAddForeignKeyConstraint2, table, name, tableColumns, referencedTable, referencedColumns, onDelete, onUpdate));
			Output.WriteLine(command.CommandText);
			command.ExecuteNonQuery();
		}
	}
}