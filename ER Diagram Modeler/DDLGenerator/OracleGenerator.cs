using System.Collections.Generic;
using System.Linq;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DDLGenerator
{
	public class OracleGenerator: BaseGenerator
	{
		private readonly string _owner;

		public OracleGenerator(IEnumerable<TableModel> tables, IEnumerable<RelationshipModel> foreignKeys, string owner) : base(tables, foreignKeys)
		{
			_owner = owner;
		}

		protected override string GenerateForeignKey(RelationshipModel model)
		{
			string sql = @"ALTER TABLE [{0}] ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES [{3}] ({4})";
			string tableColumns = string.Join(",", model.Attributes.Select(t => t.Destination.Name));
			string referencedColumns = string.Join(",", model.Attributes.Select(t => t.Source.Name));

			if (!model.DeleteAction.ToUpper().Equals("NO ACTION"))
			{
				sql += $" ON DELETE {model.DeleteAction}";
			}
			return string.Format(sql, GetTableName(model.Destination.Title), model.Name, tableColumns, model.Source.Title,
				referencedColumns);
		}

		protected override string GetTableName(string name) => $"{_owner}.\"{name}\"";
	}
}