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
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}

		public void RenameColumn(string table, string oldName, string newName)
		{
			throw new System.NotImplementedException();
		}

		public void AddColumn(string table, TableRowModel model)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateColumn(string table, TableRowModel model)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveColumn(string table, string column)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveTable(TableModel table)
		{
			throw new System.NotImplementedException();
		}

		public void UpdatePrimaryKeyConstraint(TableModel table)
		{
			throw new System.NotImplementedException();
		}

		public void RemoveRelationship(RelationshipModel model)
		{
			throw new System.NotImplementedException();
		}

		public void AddRelationship(RelationshipModel model)
		{
			throw new System.NotImplementedException();
		}
	}
}