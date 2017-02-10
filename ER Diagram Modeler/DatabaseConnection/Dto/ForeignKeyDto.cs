using System;

namespace ER_Diagram_Modeler.DatabaseConnection.Dto
{
	public class ForeignKeyDto
	{
		public string PrimaryKeyTable { get; set; }
		public string PrimaryKeyCollumn { get; set; }
		public string ForeignKeyTable { get; set; }
		public string ForeignKeyCollumn { get; set; }
		public string Name { get; set; }
		public string UpdateAction { get; set; } = "NO ACTION";
		public string DeleteAction { get; set; } = "NO ACTION";
		public DateTime LastModified { get; set; }
		public string Id { get; set; }
	}
}