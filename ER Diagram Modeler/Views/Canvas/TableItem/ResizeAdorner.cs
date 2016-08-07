using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	class ResizeAdorner: Adorner
	{
		private readonly VisualCollection _visuals;
		private readonly ResizeDecoratorFlat _flat;

		protected override int VisualChildrenCount => _visuals.Count;

		public ResizeAdorner(ContentControl designerItem)
            : base(designerItem)
        {
			SnapsToDevicePixels = true;
			_flat = new ResizeDecoratorFlat {DataContext = designerItem};
			_visuals = new VisualCollection(this) {_flat};
        }

		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			_flat.Arrange(new Rect(arrangeBounds));
			return arrangeBounds;
		}

		protected override Visual GetVisualChild(int index)
		{
			return _visuals[index];
		}
	}
}
