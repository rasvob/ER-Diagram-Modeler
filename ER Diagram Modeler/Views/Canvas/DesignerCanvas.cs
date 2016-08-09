using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas
{
	public class DesignerCanvas: System.Windows.Controls.Canvas
	{
		private Point? _dragStartPoint;

		public IEnumerable<TableContent> SelectedItems => Children.OfType<TableContent>().Where(t => t.IsSelected);

		public void DeselectAll()
		{
			foreach (TableContent item in SelectedItems)
			{
				item.IsSelected = false;
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Source.Equals(this))
			{
				_dragStartPoint = e.GetPosition(this);
				ResetZIndexes();
				DeselectAll();
				e.Handled = true;
			}
		}

		public void ResetZIndexes()
		{
			foreach (TableContent item in Children.OfType<TableContent>())
			{
				SetZIndex(item, 0);
			}
		}
	}
}