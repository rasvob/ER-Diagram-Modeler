using System.Runtime.InteropServices;
using System.Text;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableRowModel
	{
		public string Name { get; set; }
		public Datatype Datatype { get; set; }
		public bool AllowNull { get; set; } = true;
		public bool PrimaryKey { get; set; } = false;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Name).Append(' ').Append(Datatype.ToString()).Append(' ');
			if (AllowNull)
			{
				sb.Append("NULL ");
			}
			else
			{
				sb.Append("NOT NULL ");
			}
			if (PrimaryKey)
			{
				sb.Append("PRIMARY KEY");
			}
			return sb.ToString();
		}
	}
}