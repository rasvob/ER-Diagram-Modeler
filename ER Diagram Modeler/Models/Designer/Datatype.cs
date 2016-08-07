using System.Text;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class Datatype
	{
		public string Name { get; set; }
		public int? Lenght { get; set; }
		public int? Scale { get; set; } 
		public int? Precision { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Name).Append(' ');
			if (Lenght.HasValue)
			{
				sb.Append($"({Lenght.Value})");
			}
			else if (Scale.HasValue && Precision.HasValue)
			{
				sb.Append($"({Scale.Value},{Precision.Value})");
			}
			return sb.ToString();
		}
	}
}