using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Linq;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	/// <summary>
	/// Implementation of IDatabseStrategy for MS Sql Server
	/// </summary>
	public class MsSqlStrategy: IDatabseStrategy
	{
		/// <summary>
		/// Execute raw query
		/// </summary>
		/// <param name="sql">SQL Command text</param>
		/// <returns>Dataset with results</returns>
		public DataSet ExecuteRawQuery(string sql)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				return mapper.ExecuteRawQuery(sql);
			}
		}

		/// <summary>
		/// Comparer for RelationshipModel objects
		/// </summary>
		public IComparer<RelationshipModel> Comparer { get; set; } = new MsSqlComparer();

		/// <summary>
		/// Get table info with all collumns from DB
		/// </summary>
		/// <param name="id">Object ID</param>
		/// <param name="name">Table name</param>
		/// <returns>Table model</returns>
		public TableModel ReadTableDetails(string id, string name)
		{
			using (IMapper mapper = new MsSqlMapper())
			{
				return mapper.SelectTableDetails(id, name);
			}
		}

		/// <summary>
		/// Create new table in DB
		/// </summary>
		/// <param name="name">Table name</param>
		public void CreateTable(string name)
		{
			using(IMapper mapper = new MsSqlMapper())
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
			using(IMapper mapper = new MsSqlMapper())
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
			using(IMapper mapper = new MsSqlMapper())
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
			using(IMapper mapper = new MsSqlMapper())
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
			using(IMapper mapper = new MsSqlMapper())
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
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.AlterColumn(table, model);
			}
		}

		/// <summary>
		/// Drop column in table
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="column">Column name</param>
		public void RemoveColumn(string table, string column)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.DropColumn(table, column);
			}
		}

		/// <summary>
		/// Drop table from DB
		/// </summary>
		/// <param name="table">Table name</param>
		public void RemoveTable(TableModel table)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.DropTable(table.Title);
			}
		}

		/// <summary>
		/// Update PK constraint
		/// </summary>
		/// <param name="table">Table model</param>
		public void UpdatePrimaryKeyConstraint(TableModel table)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				string constraintName = table.Attributes.FirstOrDefault(t => t.PrimaryKeyConstraintName != null)?.PrimaryKeyConstraintName;
				string[] columns = table.Attributes.Where(t => t.PrimaryKey).Select(s => s.Name).ToArray();

				if (constraintName != null)
				{
					mapper.DropPrimaryKey(table.Title, constraintName);
				}

				if (columns.Length != 0)
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
			using (IMapper mapper = new MsSqlMapper())
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
			using(IMsSqlMapper mapper = new MsSqlMapper())
			{
				mapper.CreateForeignKey(model.Destination.Title, model.Source.Title, model.Attributes, model.Name, model.UpdateAction, model.DeleteAction);
			}
		}

		/// <summary>
		/// List name of all foreign key coninstraints
		/// </summary>
		/// <returns>Names of all foreign key coninstraints</returns>
		public IEnumerable<string> ListAllForeignKeys()
		{
			using(IMsSqlMapper mapper = new MsSqlMapper())
			{
				return mapper.ListAllForeignKeys();
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
			using (MsSqlMapper mapper = new MsSqlMapper())
			{
				IEnumerable<ForeignKeyDto> keyDtos = mapper.ListForeignKeys(table);

				var grouped = keyDtos.Where(t =>
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

				foreach (IGrouping<string, ForeignKeyDto> dtos in grouped)
				{
					var model = new RelationshipModel();
					model.Name = dtos.Key;

					var first = dtos.FirstOrDefault();

					model.Source = tables.FirstOrDefault(t => t.Title.Equals(first.PrimaryKeyTable));
					model.Destination = tables.FirstOrDefault(t => t.Title.Equals(first.ForeignKeyTable));

					foreach (ForeignKeyDto keyDto in dtos)
					{
						RowModelPair pair = new RowModelPair();

						pair.Source = model.Source.Attributes.FirstOrDefault(t => t.Name.Equals(keyDto.PrimaryKeyCollumn));
						pair.Destination = model.Destination.Attributes.FirstOrDefault(t => t.Name.Equals(keyDto.ForeignKeyCollumn));

						model.Attributes.Add(pair);
					}

					model.Optionality = model.Attributes.All(t => t.Destination.AllowNull) ? Optionality.Optional : Optionality.Mandatory;
					model.DeleteAction = first.DeleteAction;
					model.UpdateAction = first.UpdateAction;
					model.Id = first.Id;
					model.LastModified = first.LastModified;
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
			return new TableRowModel {Name = "Id", Datatype = DatatypeProvider.Instance.FindDatatype("int", ConnectionType.SqlServer)};
		}

		/// <summary>
		/// Save diagram to DB
		/// </summary>
		/// <param name="name">Diagram name</param>
		/// <param name="data">XML data</param>
		/// <returns>One if successful, zero if not</returns>
		public int SaveDiagram(string name, XDocument data)
		{
			using(IMapper mapper = new MsSqlMapper())
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
			using (IMapper mapper = new MsSqlMapper())
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
			using (IMapper mapper = new MsSqlMapper())
			{
				try
				{
					IEnumerable<DiagramModel> diagrams = mapper.SelectDiagrams();
					return diagrams;
				}
				catch (SqlException)
				{
					return new List<DiagramModel>();
				}
			}
		}
	}

	/// <summary>
	/// Comparer for RelationshipModel - MS Sql implementation
	/// </summary>
	public class MsSqlComparer : IComparer<RelationshipModel>
	{
		/// <summary>
		/// Compare by Name and Id
		/// </summary>
		/// <param name="x">Relationship model</param>
		/// <param name="y">Relationship model</param>
		/// <returns>One if equal, zero if not</returns>
		public int Compare(RelationshipModel x, RelationshipModel y)
		{
			if (x.Name.Equals(y.Name) && x.Id.Equals(y.Id))
			{
				return 1;
			}
			return 0;
		}
	}
}