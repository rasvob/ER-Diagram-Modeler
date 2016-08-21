using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas
{
	public class DesignerCanvas: System.Windows.Controls.Canvas
	{
		public IEnumerable<TableContent> SelectedItems => Children.OfType<TableContent>().Where(t => t.IsSelected);
		public IEnumerable<TableContent> Tables => Children.OfType<TableContent>();

		public void DeselectAll()
		{
			foreach (TableContent item in SelectedItems)
			{
				item.IsSelected = false;
			}
		}

		public void ResetZIndexes()
		{
			foreach (TableContent item in Tables)
			{
				SetZIndex(item, 0);
			}
		}
	}
}