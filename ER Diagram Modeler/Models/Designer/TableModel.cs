using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableModel
	{
		public string Title { get; set; }
		public List<TableRowModel> Attributes { get; set; } = new List<TableRowModel>();
	}
}
