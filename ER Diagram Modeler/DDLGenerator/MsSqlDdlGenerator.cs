using System.Collections.Generic;
using System.Linq;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DDLGenerator
{
	public class MsSqlDdlGenerator: BaseGenerator
	{
		public MsSqlDdlGenerator(IEnumerable<TableModel> tables, IEnumerable<RelationshipModel> foreignKeys) : base(tables, foreignKeys)
		{
		}

		protected override string GenerateForeignKey(RelationshipModel model)
		{
			string sql = @"ALTER TABLE [{0}] ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES [{3}] ({4}) ON DELETE {5} ON UPDATE {6}";
			string tableColumns = string.Join(",", model.Attributes.Select(t => t.Destination.Name));
			string referencedColumns = string.Join(",", model.Attributes.Select(t => t.Source.Name));
			return string.Format(sql, GetTableName(model.Destination.Title), model.Name, tableColumns, model.Source.Title,
				referencedColumns, model.DeleteAction.Replace('_', ' '), model.UpdateAction.Replace('_', ' '));
		}

		protected override string GetTableName(string name) => name;
	}
}