namespace ER_Diagram_Modeler.DatabaseConnection.Dto
{
	public class ForeignKeyDto
	{
		public string PrimaryKeyTable { get; set; }
		public string PrimaryKeyCollumn { get; set; }
		public string ForeignKeyTable { get; set; }
		public string ForeignKeyCollumn { get; set; }
		public string Name { get; set; }
	}
}