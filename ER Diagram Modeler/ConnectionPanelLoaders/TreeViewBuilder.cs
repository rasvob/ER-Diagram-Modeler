using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ConnectionPanelLoaders
{
	/// <summary>
	/// Abstract class for treeview creation
	/// </summary>
	public abstract class TreeViewBuilder
	{
		/// <summary>
		/// Add table callback
		/// </summary>
		private readonly Action<TableModel> _addTableAction;

		/// <summary>
		/// Data for treeview
		/// </summary>
		protected readonly IEnumerable<DatabaseInfo> Infos;

		/// <summary>
		/// Add diagram callback
		/// </summary>
		private readonly Action<DiagramModel> _addDiagramAction;

		/// <summary>
		/// Delete diagram callback
		/// </summary>
		private readonly Action<DiagramModel> _dropDiagramAction;

		private Point _startPoint;

		protected TreeViewBuilder(Action<TableModel> addTableAction, IEnumerable<DatabaseInfo> infos, Action<DiagramModel> addDiagramAction, Action<DiagramModel> dropDiagramAction)
		{
			_addTableAction = addTableAction;
			Infos = infos;
			_addDiagramAction = addDiagramAction;
			_dropDiagramAction = dropDiagramAction;
		}

		/// <summary>
		/// Setup context menu and event listeners for table item
		/// </summary>
		/// <param name="item">Current item</param>
		/// <param name="model">Table model</param>
		protected void SetupTreeViewItemContextMenu(TreeViewItem item, TableModel model)
		{
			ContextMenu menu = new ContextMenu();
			MenuItem addTableToDiagramItem = new MenuItem { Header = $"Add {model.Title} to diagram" };
			addTableToDiagramItem.Click += (sender, args) => _addTableAction(model);
			menu.Items.Add(addTableToDiagramItem);
			item.MouseDoubleClick += (sender, args) => _addTableAction(model);
			item.PreviewMouseLeftButtonDown += (sender, args) =>
			{
				_startPoint = args.GetPosition(null);
			};
			item.PreviewMouseMove += (sender, args) =>
			{
				var pos = args.GetPosition(null);
				Vector vector = _startPoint - pos;
				TreeViewItem ti = args.Source as TreeViewItem;
				if (ti != null && args.LeftButton == MouseButtonState.Pressed && (Math.Abs(vector.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(vector.Y) > SystemParameters.MinimumVerticalDragDistance))
				{
					DataObject data = new DataObject("ConnectionPanelTableDragFormat", model);
					DragDrop.DoDragDrop(ti, data, DragDropEffects.Copy);
				}
			};
			item.ContextMenu = menu;
		}

		/// <summary>
		/// Setup context menu and event listeners for diagram item
		/// </summary>
		/// <param name="item">Current item</param>
		/// <param name="model">Diagram model</param>
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

		/// <summary>
		/// Build treeview
		/// </summary>
		/// <returns>Collection of treeview items</returns>
		public abstract List<TreeViewItem> BuildTreeView();
	}
}