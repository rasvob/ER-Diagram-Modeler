using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas
{
	public class DesignerCanvas: System.Windows.Controls.Canvas
	{
		public static int TableSelectedZIndex = 100;
		public static int TableUnselectedZIndex = 10;
		public static int ConnectionLineZIndex = 5;

		public IEnumerable<TableContent> SelectedTables => Children.OfType<TableContent>().Where(t => t.IsSelected);
		public IEnumerable<TableContent> Tables => Children.OfType<TableContent>();

		public void DeselectAll()
		{
			foreach (TableContent item in SelectedTables)
			{
				item.IsSelected = false;
			}
		}

		public void ResetZIndexes()
		{
			foreach (TableContent item in Tables)
			{
				SetZIndex(item, TableUnselectedZIndex);
			}
		}
	}
}