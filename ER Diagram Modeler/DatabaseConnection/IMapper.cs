using System;
using System.Collections.Generic;
using System.Data;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.DatabaseConnection
{
	public interface IMapper: IDisposable
	{
		void CreateTable(string name);
		IEnumerable<TableModel> ListTables();
		TableModel SelectTableDetails(string id, string name);
	}
}