using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ConnectionPanelLoaders
{
	public class OracleTreeViewBuilder: TreeViewBuilder
	{
		public OracleTreeViewBuilder(Action<TableModel> addTableAction, IEnumerable<DatabaseInfo> infos) : base(addTableAction, infos)
		{
		}

		public override List<TreeViewItem> BuildTreeView()
		{
			throw new NotImplementedException();
		}
	}
}