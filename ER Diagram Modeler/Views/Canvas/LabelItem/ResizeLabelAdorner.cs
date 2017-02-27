using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas.LabelItem
{
	/// <summary>
	/// Resize overlay item
	/// </summary>
	public class ResizeLabelAdorner: Adorner
	{
		private readonly VisualCollection _visuals;
		private readonly ResizeDecoratorLabelFlat _flat;

		protected override int VisualChildrenCount => _visuals.Count;

		public ResizeLabelAdorner(ContentControl designerItem)
            : base(designerItem)
        {
			SnapsToDevicePixels = true;
			_flat = new ResizeDecoratorLabelFlat { DataContext = designerItem };
			_visuals = new VisualCollection(this) { _flat };
		}

		/// <summary>
		/// Arange for overlay item size
		/// </summary>
		/// <param name="arrangeBounds"></param>
		/// <returns></returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			_flat.Arrange(new Rect(arrangeBounds));
			return arrangeBounds;
		}

		/// <summary>
		/// Get child from visual collection
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override Visual GetVisualChild(int index)
		{
			return _visuals[index];
		}
	}
}