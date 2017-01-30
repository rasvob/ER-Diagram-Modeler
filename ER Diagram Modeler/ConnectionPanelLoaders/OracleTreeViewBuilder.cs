using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
			List<TreeViewItem> res = new List<TreeViewItem>();
			TreeViewItem tables = new TreeViewItem();
			tables.Header = "Tables";

			ObservableCollection<TableModel> models = Infos.FirstOrDefault()?.Tables;

			if (models != null)
				foreach (TableModel model in models)
				{
					var item = new TreeViewItem {Header = model.Title};
					SetupTreeViewItemContextMenu(item, model);
					tables.Items.Add(item);
				}

			res.Add(tables);
			return res;
		}
	}
}