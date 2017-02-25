using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ConnectionPanelLoaders
{
	/// <summary>
	/// Oracle treeviw builder
	/// </summary>
	public class OracleTreeViewBuilder: TreeViewBuilder
	{
		public OracleTreeViewBuilder(Action<TableModel> addTableAction, IEnumerable<DatabaseInfo> infos, Action<DiagramModel> addDiagramAction, Action<DiagramModel> dropDiagramAction) : base(addTableAction, infos, addDiagramAction, dropDiagramAction)
		{
		}

		/// <summary>
		/// Build treeview for Oracle
		/// </summary>
		/// <returns>Collection of treeview items</returns>
		public override List<TreeViewItem> BuildTreeView()
		{
			List<TreeViewItem> res = new List<TreeViewItem>();
			TreeViewItem tables = new TreeViewItem();
			tables.Header = "Tables";

			List<TableModel> models = Infos.FirstOrDefault()?.Tables;

			if (models != null)
				foreach (TableModel model in models)
				{
					var item = new TreeViewItem {Header = model.Title};
					SetupTreeViewItemContextMenu(item, model);
					tables.Items.Add(item);
				}

			res.Add(tables);

			TreeViewItem diagrams = new TreeViewItem();
			diagrams.Header = "Diagrams";

			List<DiagramModel> diagramModels = Infos.FirstOrDefault()?.Diagrams;
			if (diagramModels != null)
				foreach (DiagramModel model in diagramModels)
				{
					var item = new TreeViewItem { Header = model.Name };
					SetupTreeViewItemDiagramContextMenu(item, model);
					diagrams.Items.Add(item);
				}

			res.Add(diagrams);
			return res;
		}
	}
}