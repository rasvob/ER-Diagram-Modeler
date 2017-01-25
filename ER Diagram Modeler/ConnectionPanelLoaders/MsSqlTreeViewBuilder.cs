using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ConnectionPanelLoaders
{
	public class MsSqlTreeViewBuilder: TreeViewBuilder
	{
		public override List<TreeViewItem> BuildTreeView()
		{
			//TODO: Load diagrams
			string origdb = SessionProvider.Instance.Database;
			List<TreeViewItem> res = new List<TreeViewItem>();
			foreach(DatabaseInfo databaseInfo in Infos)
			{
				SessionProvider.Instance.Database = databaseInfo.Name;
				using(MsSqlMapper mapper = new MsSqlMapper())
				{
					TreeViewItem root = new TreeViewItem();
					root.Header = databaseInfo.Name;

					databaseInfo.Tables.Clear();

					TreeViewItem tables = new TreeViewItem();
					tables.Header = "Tables";

					foreach(TableModel model in mapper.ListTables())
					{
						databaseInfo.Tables.Add(model);

						TreeViewItem item = new TreeViewItem
						{
							Header = model.Title,
							IsEnabled = databaseInfo.Name.Equals(origdb)
						};

						SetupTreeViewItemContextMenu(item, model);
						tables.Items.Add(item);
					}

					root.Items.Add(tables);
					res.Add(root);
				}
			}

			SessionProvider.Instance.Database = origdb;

			return res;
		}

		public MsSqlTreeViewBuilder(Action<TableModel> addTableAction, IEnumerable<DatabaseInfo> infos) : base(addTableAction, infos)
		{
		}
	}
}