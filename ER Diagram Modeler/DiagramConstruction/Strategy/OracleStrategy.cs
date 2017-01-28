using System.Collections.Generic;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.Models.Designer;
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
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
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
	}
}