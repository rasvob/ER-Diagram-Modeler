using System;
using System.Collections.Generic;
using System.Linq;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels.Enums;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler.DiagramConstruction.Strategy
{
	public class OracleStrategy: IDatabseStrategy
	{
		public TableModel ReadTableDetails(string id, string name)
		{
			using (IMapper mapper = new OracleMapper())
			{
				return mapper.SelectTableDetails(id, name);
			}
		}

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

					model.Optionality = model.Attributes.All(t => t.Destination.AllowNull) ? Optionality.Optional : Optionality.Mandatory;
					res.Add(model);
				}
				return res;
			}
		}

		public TableRowModel PlaceholderRowModel()
		{
			return new TableRowModel { Name = "Id", Datatype = DatatypeProvider.Instance.FindDatatype("integer", ConnectionType.Oracle) };
		}

		public void CreateTable(string name)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.CreateTable(name);
			}
		}

		public IEnumerable<TableModel> ListTables()
		{
			using(IMapper mapper = new OracleMapper())
			{
				return mapper.ListTables();
			}
		}

		public void RenameTable(string oldName, string newName)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.RenameTable(oldName, newName);
			}
		}

		public void RenameColumn(string table, string oldName, string newName)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.RenameColumn(table, oldName, newName);
			}
		}

		public void AddColumn(string table, TableRowModel model)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.AddNewColumn(table, model);
			}
		}

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

		public void RemoveRelationship(RelationshipModel model)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.DropForeignKey(model.Destination.Title, model.Name);
			}
		}

		public void AddRelationship(RelationshipModel model)
		{
			using(IMapper mapper = new OracleMapper())
			{
				mapper.CreateForeignKey(model.Destination.Title, model.Source.Title, model.Attributes, model.Name);
			}
		}
	}
}