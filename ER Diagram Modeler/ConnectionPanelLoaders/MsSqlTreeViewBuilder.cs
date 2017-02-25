using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ConnectionPanelLoaders
{
	public class MsSqlTreeViewBuilder: TreeViewBuilder
	{
		/// <summary>
		/// Drop DB callback
		/// </summary>
		private Action<string> _dropDatabaseAction;

		///// <summary>
		///// Build treeview for MS Sql server
		///// </summary>
		/// <remarks>DEPRECATED</remarks>
		///// <returns>Collection of treeview items</returns>
		//public override List<TreeViewItem> BuildTreeView()
		//{
		//	string origdb = SessionProvider.Instance.Database;
		//	List<TreeViewItem> res = new List<TreeViewItem>();
		//	var ctx = new DatabaseContext(ConnectionType.SqlServer);
		//	foreach(DatabaseInfo databaseInfo in Infos)
		//	{
		//		SessionProvider.Instance.Database = databaseInfo.Name;
		//		TreeViewItem root = new TreeViewItem();
		//		root.Header = databaseInfo.Name;
		//		SetupDatabaseItemContextMenu(root);
		//		databaseInfo.Tables.Clear();

		//		TreeViewItem tables = new TreeViewItem();
		//		tables.Header = "Tables";

		//		foreach(TableModel model in ctx.ListTables())
		//		{
		//			databaseInfo.Tables.Add(model);

		//			TreeViewItem item = new TreeViewItem
		//			{
		//				Header = model.Title,
		//				IsEnabled = databaseInfo.Name.Equals(origdb)
		//			};

		//			SetupTreeViewItemContextMenu(item, model);
		//			tables.Items.Add(item);
		//		}
		//		root.Items.Add(tables);

		//		databaseInfo.Diagrams.Clear();

		//		TreeViewItem diagrams = new TreeViewItem();
		//		diagrams.Header = "Diagrams";

		//		IEnumerable<DiagramModel> diagramModels = ctx.SelectDiagrams();

		//		foreach(DiagramModel model in diagramModels)
		//		{
		//			databaseInfo.Diagrams.Add(model);

		//			TreeViewItem item = new TreeViewItem
		//			{
		//				Header = model.Name,
		//				IsEnabled = databaseInfo.Name.Equals(origdb)
		//			};

		//			SetupTreeViewItemDiagramContextMenu(item, model);
		//			diagrams.Items.Add(item);
		//		}

		//		root.Items.Add(diagrams);

		//		res.Add(root);
		//	}

		//	SessionProvider.Instance.Database = origdb;

		//	return res;
		//}

		/// <summary>
		/// Build treeview for MS Sql server
		/// </summary>
		/// <returns>Collection of treeview items</returns>
		public override List<TreeViewItem> BuildTreeView()
		{
			List<TreeViewItem> res = new List<TreeViewItem>();
			foreach(DatabaseInfo databaseInfo in Infos)
			{
				TreeViewItem root = new TreeViewItem();
				root.Header = databaseInfo.Name;
				SetupDatabaseItemContextMenu(root);

				TreeViewItem tables = new TreeViewItem();
				tables.Header = "Tables";

				foreach(TableModel model in databaseInfo.Tables.ToList())
				{
					databaseInfo.Tables.Add(model);

					TreeViewItem item = new TreeViewItem
					{
						Header = model.Title,
						IsEnabled = databaseInfo.Name.Equals(SessionProvider.Instance.Database)
					};

					SetupTreeViewItemContextMenu(item, model);
					tables.Items.Add(item);
				}
				root.Items.Add(tables);

				TreeViewItem diagrams = new TreeViewItem();
				diagrams.Header = "Diagrams";

				foreach(DiagramModel model in databaseInfo.Diagrams.ToList())
				{
					databaseInfo.Diagrams.Add(model);

					TreeViewItem item = new TreeViewItem
					{
						Header = model.Name,
						IsEnabled = databaseInfo.Name.Equals(SessionProvider.Instance.Database)
					};

					SetupTreeViewItemDiagramContextMenu(item, model);
					diagrams.Items.Add(item);
				}

				root.Items.Add(diagrams);

				res.Add(root);
			}
			return res;
		}

		/// <summary>
		/// Setup DB treeview item
		/// </summary>
		/// <param name="item">Current item</param>
		private void SetupDatabaseItemContextMenu(TreeViewItem item)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem dropDatabaseItem = new MenuItem { Header = $"Drop {item.Header}" };
			dropDatabaseItem.Click += (sender, args) => _dropDatabaseAction(item.Header as string);
			menu.Items.Add(dropDatabaseItem);
			item.ContextMenu = menu;
		}

		public MsSqlTreeViewBuilder(Action<TableModel> addTableAction, Action<string> dropDatabaseAction, IEnumerable<DatabaseInfo> infos, Action<DiagramModel> addDiagramAction, Action<DiagramModel> dropDiagramAction) : base(addTableAction, infos, addDiagramAction, dropDiagramAction)
		{
			_dropDatabaseAction = dropDatabaseAction;
		}
	}
}