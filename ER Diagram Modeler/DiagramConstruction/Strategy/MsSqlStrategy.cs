using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public class MsSqlStrategy: IDatabseStrategy
	{
		public TableModel ReadTableDetails(string id, string name)
		{
			using (IMapper mapper = new MsSqlMapper())
			{
				return mapper.SelectTableDetails(id, name);
			}
		}

		public void CreateTable(string name)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.CreateTable(name);
			}
		}

		public IEnumerable<TableModel> ListTables()
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				return mapper.ListTables();
			}
		}

		public void RenameTable(string oldName, string newName)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.RenameTable(oldName, newName);
			}
		}

		public void RenameColumn(string table, string oldName, string newName)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.RenameColumn(table, oldName, newName);
			}
		}

		public void AddColumn(string table, TableRowModel model)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.AddNewColumn(table, model);
			}
		}

		public void UpdateColumn(string table, TableRowModel model)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.AlterColumn(table, model);
			}
		}

		public void RemoveColumn(string table, string column)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.DropColumn(table, column);
			}
		}

		public void RemoveTable(TableModel table)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.DropTable(table.Title);
			}
		}

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

		public void RemoveRelationship(RelationshipModel model)
		{
			using (IMapper mapper = new MsSqlMapper())
			{
				mapper.DropForeignKey(model.Destination.Title, model.Name);
			}
		}

		public void AddRelationship(RelationshipModel model)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				mapper.CreateForeignKey(model.Destination.Title, model.Source.Title, model.Attributes, model.Name);
			}
		}

		public IEnumerable<RelationshipModel> ReadRelationshipModels(string table, IEnumerable<TableModel> tables)
		{
			using (MsSqlMapper mapper = new MsSqlMapper())
			{
				IEnumerable<MsSqlForeignKeyDto> keyDtos = mapper.ListForeignKeys(table);

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

				foreach (IGrouping<string, MsSqlForeignKeyDto> dtos in grouped)
				{
					var model = new RelationshipModel();
					model.Name = dtos.Key;

					var first = dtos.FirstOrDefault();

					model.Source = tables.FirstOrDefault(t => t.Title.Equals(first.PrimaryKeyTable));
					model.Destination = tables.FirstOrDefault(t => t.Title.Equals(first.ForeignKeyTable));

					foreach (MsSqlForeignKeyDto keyDto in dtos)
					{
						RowModelPair pair = new RowModelPair();

						pair.Source = model.Source.Attributes.FirstOrDefault(t => t.Name.Equals(keyDto.PrimaryKeyCollumn));
						pair.Destination = model.Destination.Attributes.FirstOrDefault(t => t.Name.Equals(keyDto.ForeignKeyCollumn));

						model.Attributes.Add(pair);
					}

					model.Optionality = model.Attributes.All(t => t.Destination.AllowNull) ? Optionality.Optional : Optionality.Mandatory;
					res.Add(model);
				}
				return res;
			}
		}

		public TableRowModel PlaceholderRowModel()
		{
			return new TableRowModel {Name = "Id", Datatype = DatatypeProvider.Instance.FindDatatype("int", ConnectionType.SqlServer)};
		}
	}
}