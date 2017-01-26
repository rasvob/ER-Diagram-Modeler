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
		private Action<string> _dropDatabaseAction; 

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
					SetupDatabaseItemContextMenu(root);
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

		private void SetupDatabaseItemContextMenu(TreeViewItem item)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem dropDatabaseItem = new MenuItem { Header = $"Drop {item.Header}" };
			dropDatabaseItem.Click += (sender, args) => _dropDatabaseAction(item.Header as string);
			menu.Items.Add(dropDatabaseItem);
			item.ContextMenu = menu;
		}

		public MsSqlTreeViewBuilder(Action<TableModel> addTableAction, Action<string> dropDatabaseAction,IEnumerable<DatabaseInfo> infos) : base(addTableAction, infos)
		{
			_dropDatabaseAction = dropDatabaseAction;
		}
	}
}