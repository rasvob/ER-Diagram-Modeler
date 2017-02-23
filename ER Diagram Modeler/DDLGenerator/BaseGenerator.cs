using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DDLGenerator
{
	/// <summary>
	/// DDL Script generator
	/// </summary>
	public abstract class BaseGenerator
	{
		/// <summary>
		/// Tables
		/// </summary>
		protected readonly IEnumerable<TableModel> Tables;

		/// <summary>
		/// Foreign keys
		/// </summary>
		protected readonly IEnumerable<RelationshipModel> ForeignKeys;

		protected BaseGenerator(IEnumerable<TableModel> tables, IEnumerable<RelationshipModel> foreignKeys)
		{
			Tables = tables;
			ForeignKeys = foreignKeys;
		}

		/// <summary>
		/// Generate Create table statement
		/// </summary>
		/// <param name="table">Table model</param>
		/// <returns>Create table statement</returns>
		protected string GenerateCreateTable(TableModel table)
		{
			var sb = new StringBuilder();
			sb.Append($"CREATE TABLE {GetTableName(table.Title)} ( {Environment.NewLine}");
			string attr = string.Join("," + Environment.NewLine, table.Attributes.ToList().Select(t => t.ToString().TrimEnd()));
			sb.Append(attr).Append(Environment.NewLine).Append(")");
			return sb.ToString();
		}

		/// <summary>
		/// Generate Alter table statement
		/// </summary>
		/// <param name="table">Table model</param>
		/// <returns>Primary key statement</returns>
		protected string GeneratePrimaryKey(TableModel table)
		{
			var pk = table.Attributes.Where(t => t.PrimaryKey);
			IEnumerable<TableRowModel> rowModels = pk as IList<TableRowModel> ?? pk.ToList();
			var first = rowModels.FirstOrDefault();
			var str = "ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})";

			return string.Format(str, GetTableName(table.Title), first?.PrimaryKeyConstraintName,
				string.Join(",", rowModels.Select(t => t.Name)));
		}

		/// <summary>
		/// Generate Alter table statement
		/// </summary>
		/// <param name="model">Foreign model</param>
		/// <returns>Foreign key statement</returns>
		protected abstract string GenerateForeignKey(RelationshipModel model);

		/// <summary>
		/// Generate complete DDL for diagram
		/// </summary>
		/// <returns>DDL script</returns>
		public string GenerateDdl()
		{
			var sb = new StringBuilder();
			var csb = new StringBuilder();
			var pksb = new StringBuilder();
			var fksb = new StringBuilder();

			foreach (TableModel table in Tables)
			{
				string createTable = GenerateCreateTable(table);
				string primaryKey = GeneratePrimaryKey(table);
				csb.Append(createTable).Append(Environment.NewLine).Append(Environment.NewLine);
				pksb.Append(primaryKey).Append(Environment.NewLine);
			}

			foreach (RelationshipModel foreignKey in ForeignKeys)
			{
				string key = GenerateForeignKey(foreignKey);
				fksb.Append(key).Append(Environment.NewLine);
			}

			return sb
				.Append("--DDL Script")
				.Append(Environment.NewLine)
				.Append("--CREATE TABLES")
				.Append(Environment.NewLine)
				.Append(csb)
				.Append(Environment.NewLine)
				.Append("--PRIMARY KEYS")
				.Append(Environment.NewLine)
				.Append(pksb)
				.Append(Environment.NewLine)
				.Append("--FOREIGN KEYS")
				.Append(Environment.NewLine)
				.Append(fksb)
				.Append(Environment.NewLine)
				.ToString();
		}

		/// <summary>
		/// Return table name
		/// </summary>
		/// <param name="name">Name of table</param>
		/// <returns>Table name</returns>
		protected abstract string GetTableName(string name);

		/// <summary>
		/// Factory method
		/// </summary>
		/// <param name="tables">Table models</param>
		/// <param name="foreignKeys">Foreign kys models</param>
		/// <param name="connection">Type of connection</param>
		/// <param name="owner">DB Owner - Oracle only</param>
		/// <returns>Instance of BaseGenerator subtype</returns>
		public static BaseGenerator CreateGenerator(IEnumerable<TableModel> tables, IEnumerable<RelationshipModel> foreignKeys,
			ConnectionType connection, string owner = null)
		{
			switch (connection)
			{
				case ConnectionType.SqlServer:
					return new MsSqlDdlGenerator(tables, foreignKeys);
				case ConnectionType.Oracle:
					return new OracleGenerator(tables, foreignKeys, owner);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}