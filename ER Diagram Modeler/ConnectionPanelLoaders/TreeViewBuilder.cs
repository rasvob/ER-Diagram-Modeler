using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ConnectionPanelLoaders
{
	public abstract class TreeViewBuilder
	{
		private readonly Action<TableModel> _addTableAction;
		protected readonly IEnumerable<DatabaseInfo> Infos;

		protected TreeViewBuilder(Action<TableModel> addTableAction, IEnumerable<DatabaseInfo> infos)
		{
			_addTableAction = addTableAction;
			Infos = infos;
		}

		protected void SetupTreeViewItemContextMenu(TreeViewItem item, TableModel model)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem addTableToDiagramItem = new MenuItem { Header = $"Add {model.Title} to diagram" };
			addTableToDiagramItem.Click += (sender, args) => _addTableAction(model);
			menu.Items.Add(addTableToDiagramItem);
			item.ContextMenu = menu;
		}

		public abstract List<TreeViewItem> BuildTreeView();
	}
}