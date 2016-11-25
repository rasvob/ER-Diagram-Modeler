using System.Drawing;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.Models.Helpers
{
	public class RowModelPair
	{
		public TableRowModel Source { get; set; }
		public TableRowModel Destination { get; set; }

		public bool CanBeForeignKey(ref string errorMessage)
		{
			if (Source == null)
			{
				errorMessage = "Primary key table attributte is null";
				return false;
			}

			if (Destination == null)
			{
				errorMessage = "Foreign key table attributte is null";
				return false;
			}

			if(!Source.Datatype.Name.Equals(Destination.Datatype.Name))
			{
				errorMessage = $"{Source.Datatype.Name} can't be used as foreign key with {Destination.Datatype.Name}";
				return false;
			}

			if(Source.Datatype.HasLenght && Source.Datatype.Lenght != Destination.Datatype.Lenght)
			{
				errorMessage = $"Lenght of {Source.Name} and {Destination.Name} is different";
				return false;
			}

			if(Source.Datatype.HasScale && Source.Datatype.Scale != Destination.Datatype.Scale)
			{
				errorMessage = $"Scale of {Source.Name} and {Destination.Name} is different";
				return false;
			}

			if(Source.Datatype.HasPrecision && Source.Datatype.Precision != Destination.Datatype.Precision)
			{
				errorMessage = $"Precision of {Source.Name} and {Destination.Name} is different";
				return false;
			}

			return true;
		}
	}
}