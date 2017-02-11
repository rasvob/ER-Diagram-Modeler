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
		private readonly Action<DiagramModel> _addDiagramAction;
		private readonly Action<DiagramModel> _dropDiagramAction;

		protected TreeViewBuilder(Action<TableModel> addTableAction, IEnumerable<DatabaseInfo> infos, Action<DiagramModel> addDiagramAction, Action<DiagramModel> dropDiagramAction)
		{
			_addTableAction = addTableAction;
			Infos = infos;
			_addDiagramAction = addDiagramAction;
			_dropDiagramAction = dropDiagramAction;
		}

		protected void SetupTreeViewItemContextMenu(TreeViewItem item, TableModel model)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem addTableToDiagramItem = new MenuItem { Header = $"Add {model.Title} to diagram" };
			addTableToDiagramItem.Click += (sender, args) => _addTableAction(model);
			menu.Items.Add(addTableToDiagramItem);
			item.MouseDoubleClick += (sender, args) => _addTableAction(model);
			item.ContextMenu = menu;
		}

		protected void SetupTreeViewItemDiagramContextMenu(TreeViewItem item, DiagramModel model)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem addDiagramItem = new MenuItem { Header = $"Add {model.Name} to workspace" };
			MenuItem dropDiagramItem = new MenuItem { Header = $"Delete {model.Name} from database" };
			addDiagramItem.Click += (sender, args) => _addDiagramAction(model);
			dropDiagramItem.Click += (sender, args) => _dropDiagramAction(model);
			menu.Items.Add(addDiagramItem);
			menu.Items.Add(dropDiagramItem);
			item.MouseDoubleClick += (sender, args) => _addDiagramAction(model);
			item.ContextMenu = menu;
		}

		public abstract List<TreeViewItem> BuildTreeView();
	}
}