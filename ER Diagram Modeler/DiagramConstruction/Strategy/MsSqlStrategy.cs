﻿using System;
using System.Collections.Generic;
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
	public class MsSqlStrategy: IDatabseStrategy
	{
		public IComparer<RelationshipModel> Comparer { get; set; } = new MsSqlComparer();

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
			using(IMsSqlMapper mapper = new MsSqlMapper())
			{
				mapper.CreateForeignKey(model.Destination.Title, model.Source.Title, model.Attributes, model.Name, model.UpdateAction, model.DeleteAction);
			}
		}

		public IEnumerable<string> ListAllForeignKeys()
		{
			using(IMsSqlMapper mapper = new MsSqlMapper())
			{
				return mapper.ListAllForeignKeys();
			}
		}

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

		public TableRowModel PlaceholderRowModel()
		{
			return new TableRowModel {Name = "Id", Datatype = DatatypeProvider.Instance.FindDatatype("int", ConnectionType.SqlServer)};
		}


		public int SaveDiagram(string name, XDocument data)
		{
			using(IMapper mapper = new MsSqlMapper())
			{
				IEnumerable<DiagramModel> diagrams = mapper.SelectDiagrams();
				mapper.CreateDiagramTable();
				return diagrams.Any(t => t.Name.Equals(name)) ? mapper.UpdateDiagram(name, data) : mapper.InsertDiagram(name, data);
			}
		}

		public int DeleteDiagram(string name)
		{
			using (IMapper mapper = new MsSqlMapper())
			{
				return mapper.DeleteDiagram(name);
			}

		}

		public IEnumerable<DiagramModel> SelectDiagrams()
		{
			using (IMapper mapper = new MsSqlMapper())
			{
				return mapper.SelectDiagrams();
			}
		}
	}

	public class MsSqlComparer : IComparer<RelationshipModel>
	{
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