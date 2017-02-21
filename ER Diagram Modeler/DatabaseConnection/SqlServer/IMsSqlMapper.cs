using System.Collections.Generic;
using ER_Diagram_Modeler.DatabaseConnection.Dto;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;

namespace ER_Diagram_Modeler.DatabaseConnection.SqlServer
{
	/// <summary>
	/// MsSqlMapper specific operations
	/// </summary>
	public interface IMsSqlMapper: IMapper
	{
		/// <summary>
		/// List all databases of given instance
		/// </summary>
		/// <returns>Collection of databases</returns>
		IEnumerable<DatabaseInfo> ListDatabases();

		/// <summary>
		/// Create new database
		/// </summary>
		/// <param name="name">DB name</param>
		void CreateDatabase(string name);

		/// <summary>
		/// Drop database
		/// </summary>
		/// <param name="name">DB Name</param>
		void DropDatabase(string name);

		/// <summary>
		/// Create new foreign key constraint
		/// </summary>
		/// <param name="table">Table name</param>
		/// <param name="referencedTable">Referenced table name</param>
		/// <param name="collumns">Collection of constraint columns</param>
		/// <param name="fkName">Name of constraint, generated if NULL</param>
		/// <param name="onUpdate">Action on update, all standard MS Sql  actions</param>
		/// <param name="onDelete">Action on delete, all standard MS Sql actions</param>
		void CreateForeignKey(string table, string referencedTable, IEnumerable<RowModelPair> collumns, string fkName = null, string onUpdate = "NO ACTION", string onDelete = "NO ACTION");
	}
}