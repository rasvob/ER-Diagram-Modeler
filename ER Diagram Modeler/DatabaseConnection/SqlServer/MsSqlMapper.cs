using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	public class MsSqlMapper: IMsSqlMapper
	{
		private static string SqlListDatabases = @"SELECT database_id, name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');";
		private static string SqlListTables = @"SELECT object_id, name FROM sys.tables WHERE name <> 'sysdiagrams' AND name <> '__ERDiagramModelerData';";
		private static string SqlTableDetails = @"SELECT s.column_id, s.name, s.is_nullable, t.name, s.max_length, s.precision, s.scale FROM sys.columns s JOIN sys.types t ON s.system_type_id = t.system_type_id WHERE s.object_id = @Id;";
		private static string SqlPrimaryKeys = @"sp_pkeys";
		private static string SqlForeignKeys = @"sp_fkeys";
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

		public MsSqlDatabase Database { get; set; }

		public MsSqlMapper()
		{
			Database = new MsSqlDatabase();
			Database.Connect(SessionProvider.Instance.ConnectionString);
		}

		public MsSqlMapper(string connString)
		{
			Database = new MsSqlDatabase();
			Database.Connect(connString);
		}

		public void Dispose()
		{
			if (Database.Connection.State == ConnectionState.Open)
			{
				Database.Close();
			}
		}

		public void CreateTable(string name)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlCreateTable, name));
			command.ExecuteNonQuery();
		}

		public IEnumerable<DatabaseInfo> ListDatabases()
		{
			SqlCommand command = Database.CreateCommand(SqlListDatabases);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadListDatabases(reader);
			reader.Close();
			return res;
		}

		public IEnumerable<MsSqlForeignKeyDto> ListForeignKeys(string tableName)
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

			return msSqlForeignKeyDtos;
		}

		public void CreateDatabase(string name)
		{
			SqlCommand command = Database.CreateCommand($"{SqlCreateDatabase} [{name}]");
			command.ExecuteNonQuery();
		}

		public void DropDatabase(string name)
		{
			SqlCommand commandAlter = Database.CreateCommand($"alter database [{name}] set single_user with rollback immediate");
			commandAlter.ExecuteNonQuery();

			SqlCommand command = Database.CreateCommand($"{SqlDropDatabase} [{name}]");
			command.ExecuteNonQuery();
		}

		public IEnumerable<TableModel> ListTables()
		{
			SqlCommand command = Database.CreateCommand(SqlListTables);
			SqlDataReader reader = command.ExecuteReader();
			var res = ReadListTables(reader);
			reader.Close();
			return res;
		}

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

		public void RenameTable(string oldName, string newName)
		{
			SqlCommand command = Database.CreateCommand(SqlRenameTable);
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue("objname", oldName);
			command.Parameters.AddWithValue("newname", newName);
			command.ExecuteNonQuery();
		}

		public void AddNewColumn(string table, TableRowModel model)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlAddColumn, table, model));
			command.ExecuteNonQuery();
		}

		public void AlterColumn(string table, TableRowModel model)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlAlterColumn, table, model));
			command.ExecuteNonQuery();
		}

		public void RenameColumn(string table, string oldName, string newName)
		{
			SqlCommand command = Database.CreateCommand(SqlRenameTable);
			command.CommandType = CommandType.StoredProcedure;
			command.Parameters.AddWithValue("objname", $"{table}.{oldName}");
			command.Parameters.AddWithValue("newname", newName);
			command.Parameters.AddWithValue("objtype", "COLUMN");
			command.ExecuteNonQuery();
		}

		public void DropColumn(string table, string column)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropColumn, table, column));
			command.ExecuteNonQuery();
		}

		public void DropTable(string table)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropTable, table));
			command.ExecuteNonQuery();
		}

		public void DropPrimaryKey(string table, string primaryKeyConstraintName)
		{
			SqlCommand command = Database.CreateCommand(string.Format(SqlDropConstraint, table, primaryKeyConstraintName));
			command.ExecuteNonQuery();
		}

		public void CreatePrimaryKey(string table, string[] columns)
		{
			if (columns.Length == 0)
			{
				return;
			}
			SqlCommand command = Database.CreateCommand(string.Format(SqlAddPrimaryKeyConstraint, table, $"PK_{table}_{columns[0]}", string.Join(",", columns)));
			command.ExecuteNonQuery();
		}

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

		private IEnumerable<MsSqlForeignKeyDto> ReadForeignKeys(SqlDataReader reader)
		{
			var res = new List<MsSqlForeignKeyDto>();

			while (reader.Read())
			{
				var dto = new MsSqlForeignKeyDto();

				dto.PrimaryKeyTable = reader.GetString(2);
				dto.PrimaryKeyCollumn = reader.GetString(3);
				dto.ForeignKeyTable = reader.GetString(6);
				dto.ForeignKeyCollumn = reader.GetString(7);
				dto.Name = reader.GetString(11);

				res.Add(dto);
			}

			return res;
		}

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
	}
}