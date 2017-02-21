using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	/// <summary>
	/// Implementation of IDatabseStrategy for Oracle
	/// </summary>
	public class OracleStrategy: IDatabseStrategy
	{
		/// <summary>
		/// Execute raw query
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>Dataset with results</returns>
		public DataSet ExecuteRawQuery(string sql)
		{
			using(IMapper mapper = new OracleMapper())
			{
				return mapper.ExecuteRawQuery(sql);
			}
		}

		/// <summary>
		/// Comparer for RelationshipModel objects
		/// </summary>
		public IComparer<RelationshipModel> Comparer { get; set; } = new OracleComparer();

		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
		public TableModel ReadTableDetails(string id, string name)
		{
			using (IMapper mapper = new OracleMapper())
			{
				return mapper.SelectTableDetails(id, name);
			}
		}

		/// <summary>
		/// List foreign key coninstraints for table
		/// </summary>
		/// <param name="table">Name of table</param>
		/// <param name="tables">Tables in designer</param>
		/// <returns>Collection of FK constraints</returns>
		public IEnumerable<RelationshipModel> ReadRelationshipModels(string table, IEnumerable<TableModel> tables)
		{
			using(IMapper mapper = new OracleMapper())
			{
				IEnumerable<ForeignKeyDto> keys = mapper.ListForeignKeys(table);

				var grouped = keys.Where(t =>
				{
					if(t.PrimaryKeyTable.Equals(table, StringComparison.CurrentCultureIgnoreCase))
					{
						if(tables.Any(s => s.Title.Equals(t.ForeignKeyTable, StringComparison.CurrentCultureIgnoreCase)))
						{
							return true;
						}
					}

					if(t.ForeignKeyTable.Equals(table, StringComparison.CurrentCultureIgnoreCase))
					{
						if(tables.Any(s => s.Title.Equals(t.PrimaryKeyTable, StringComparison.CurrentCultureIgnoreCase)))
						{
							return true;
						}
					}

					return false;
				}).GroupBy(t => t.Name);

				var res = new List<RelationshipModel>();

				foreach(IGrouping<string, ForeignKeyDto> dtos in grouped)
				{
					var model = new RelationshipModel();
					model.Name = dtos.Key;

					var first = dtos.FirstOrDefault();

					model.Source = tables.FirstOrDefault(t => t.Title.Equals(first.PrimaryKeyTable));
					model.Destination = tables.FirstOrDefault(t => t.Title.Equals(first.ForeignKeyTable));

					foreach(ForeignKeyDto keyDto in dtos)
					{
						RowModelPair pair = new RowModelPair();

						pair.Source = model.Source.Attributes.FirstOrDefault(t => t.Name.Equals(keyDto.PrimaryKeyCollumn));
						pair.Destination = model.Destination.Attributes.FirstOrDefault(t => t.Name.Equals(keyDto.ForeignKeyCollumn));

						model.Attributes.Add(pair);
					}
					model.DeleteAction = first.DeleteAction;
					model.UpdateAction = first.UpdateAction;
					model.LastModified = first.LastModified;
					model.Optionality = model.Attributes.All(t => t.Destination.AllowNull) ? Optionality.Optional : Optionality.Mandatory;
					res.Add(model);
				}
				return res;
			}
		}

		/// <summary>
		/// Return default column for Ms Sql Connection
		/// </summary>
		/// <returns>Default column</returns>
		public TableRowModel PlaceholderRowModel()
		{
			return new TableRowModel { Name = "Id", Datatype = DatatypeProvider.Instance.FindDatatype("integer", ConnectionType.Oracle) };
		}

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		public void CreateTable(string name)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.CreateTable(name);
			}
		}

		/// <summary>
		/// List tables in DB
		/// </summary>
		/// <returns>Collection of tables with ID and name</returns>
		public IEnumerable<TableModel> ListTables()
		{
			using(IMapper mapper = new OracleMapper())
			{
				return mapper.ListTables();
			}
		}

		/// <summary>
		/// Rename table in DB
		/// </summary>
		/// <param name="oldName">Old table name</param>
		/// <param name="newName">New table name</param>
		public void RenameTable(string oldName, string newName)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.RenameTable(oldName, newName);
			}
		}

		/// <summary>
		/// Rename column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		public void RenameColumn(string table, string oldName, string newName)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.RenameColumn(table, oldName, newName);
			}
		}

		/// <summary>
		/// Add columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void AddColumn(string table, TableRowModel model)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.AddNewColumn(table, model);
			}
		}

		/// <summary>
		/// Alter columnd to table in DB
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="model">Column model</param>
		public void UpdateColumn(string table, TableRowModel model)
		{
			using(IOracleMapper mapper = new OracleMapper())
			{
				TableModel details = mapper.SelectTableDetails(string.Empty, table);
				TableRowModel row = details.Attributes.FirstOrDefault(t => t.Name.Equals(model.Name));
				bool modifyNull = true;

				if (row != null)
				{
					modifyNull = row.AllowNull != model.AllowNull;
				}

				mapper.AlterColumn(table, model, modifyNull);
			}
		}

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		public void RemoveColumn(string table, string column)
		{
			using(IMapper mapper = new OracleMapper())
			{
				IEnumerable<ForeignKeyDto> keyDtos = mapper.ListForeignKeys(table).Where(t => t.ForeignKeyTable.Equals(table));
				IEnumerable<ForeignKeyDto> dtos = keyDtos as IList<ForeignKeyDto> ?? keyDtos.ToList();
				bool fkExists = dtos.Any(s => s.ForeignKeyCollumn.Equals(column));

				if (fkExists)
				{
					throw new ApplicationException($"{column} is referenced by foreign key constraint ({dtos.FirstOrDefault(t => t.ForeignKeyTable.Equals(table))?.Name})");
				}
				mapper.DropColumn(table, column);
			}
		}

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		public void RemoveTable(TableModel table)
		{
			using(IMapper mapper = new OracleMapper())
			{
				IEnumerable<ForeignKeyDto> dtos = mapper.ListForeignKeys(table.Title);
				var keyDtos = dtos as IList<ForeignKeyDto> ?? dtos.ToList();
				bool fkExists = keyDtos.Any(t => t.ForeignKeyTable.Equals(table.Title));

				if(fkExists)
				{
					throw new ApplicationException($"{table.Title} can't be dropped due to the foreign key constraint ({keyDtos.FirstOrDefault(t => t.ForeignKeyTable.Equals(table.Title))?.Name})");
				}

				mapper.DropTable(table.Title);
			}
		}

		/// <summary>
		/// Update PK constraint
		/// </summary>
		/// <param name="table">Table model</param>
		public void UpdatePrimaryKeyConstraint(TableModel table)
		{
			using(IMapper mapper = new OracleMapper())
			{
				string constraintName = table.Attributes.FirstOrDefault(t => t.PrimaryKeyConstraintName != null)?.PrimaryKeyConstraintName;
				string[] columns = table.Attributes.Where(t => t.PrimaryKey).Select(s => s.Name).ToArray();

				if(constraintName != null)
				{
					mapper.DropPrimaryKey(table.Title, constraintName);
				}

				if(columns.Length != 0)
				{
					mapper.CreatePrimaryKey(table.Title, columns);
				}
			}
		}

		/// <summary>
		/// Drop foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		public void RemoveRelationship(RelationshipModel model)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.DropForeignKey(model.Destination.Title, model.Name);
			}
		}

		/// <summary>
		/// Create foreign key constraint
		/// </summary>
		/// <param name="model">Relationship model</param>
		public void AddRelationship(RelationshipModel model)
		{
			using(IOracleMapper mapper = new OracleMapper())
			{
				mapper.CreateForeignKey(model.Destination.Title, model.Source.Title, model.Attributes, model.Name, model.DeleteAction);
			}
		}

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		public IEnumerable<string> ListAllForeignKeys()
		{
			using(IOracleMapper mapper = new OracleMapper())
			{
				return mapper.ListAllForeignKeys();
			}
		}

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		public int SaveDiagram(string name, XDocument data)
		{
			using(IMapper mapper = new OracleMapper())
			{
				IEnumerable<DiagramModel> diagrams = SelectDiagrams();
				mapper.CreateDiagramTable();
				return diagrams.Any(t => t.Name.Equals(name)) ? mapper.UpdateDiagram(name, data) : mapper.InsertDiagram(name, data);
			}
		}

		/// <summary>
		/// Delete diagram from DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <returns>One if successful, zero if not</returns>
		public int DeleteDiagram(string name)
		{
			using(IMapper mapper = new OracleMapper())
			{
				return mapper.DeleteDiagram(name);
			}
		}

		/// <summary>
		/// Select existing diagrams
		/// </summary>
		/// <returns>Collections of diagrams</returns>
		public IEnumerable<DiagramModel> SelectDiagrams()
		{
			using(IMapper mapper = new OracleMapper())
			{
				try
				{
					IEnumerable<DiagramModel> diagrams = mapper.SelectDiagrams();
					return diagrams;
				}
				catch (OracleException)
				{
					return new List<DiagramModel>();
				}
			}
		}
	}

	/// <summary>
	/// Comparer for RelationshipModel - Oracle implementation
	/// </summary>
	public class OracleComparer : IComparer<RelationshipModel>
	{
		/// <summary>
		/// Compare by Name and LastModified
		/// </summary>
		/// <param name="x">Relationship model</param>
		/// <param name="y">Relationship model</param>
		/// <returns>One if equal, zero if not</returns>
		public int Compare(RelationshipModel x, RelationshipModel y)
		{
			if (x.Name.Equals(y.Name) && x.LastModified.Equals(y.LastModified))
			{
				return 1;
			}
			return 0;
		}
	}
}