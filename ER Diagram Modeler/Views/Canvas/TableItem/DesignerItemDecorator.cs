using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
    public class DesignerItemDecorator : Control
    {
        private Adorner _adorner;

        public bool ShowDecorator
        {
	        get
	        {
		        return (bool)GetValue(ShowDecoratorProperty);
	        }
	        set
	        {
		        SetValue(ShowDecoratorProperty, value);
	        }
        }

        public static readonly DependencyProperty ShowDecoratorProperty =
            DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(DesignerItemDecorator),
            new FrameworkPropertyMetadata(false, ShowDecoratorProperty_Changed));

        public DesignerItemDecorator()
        {
            Unloaded += UnloadedHandler;
        }

        private void HideAdorner()
        {
            if (_adorner != null)
            {
                _adorner.Visibility = Visibility.Hidden;
            }
        }

        private void ShowAdorner()
        {
            if (this._adorner == null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

	            if (adornerLayer == null) return;
	            ContentControl designerItem = this.DataContext as ContentControl;
	            System.Windows.Controls.Canvas canvas = VisualTreeHelper.GetParent(designerItem) as System.Windows.Controls.Canvas;
	            _adorner = new ResizeAdorner(designerItem);
	            adornerLayer.Add(this._adorner);

	            _adorner.Visibility = ShowDecorator ? Visibility.Visible : Visibility.Hidden;
            }
            else
            {
                _adorner.Visibility = Visibility.Visible;
            }
        }

        private void UnloadedHandler(object sender, RoutedEventArgs e)
        {
            if (_adorner != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
	            adornerLayer?.Remove(_adorner);
	            _adorner = null;
            }
        }

        private static void ShowDecoratorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DesignerItemDecorator decorator = (DesignerItemDecorator)d;
            bool showDecorator = (bool)e.NewValue;

            if (showDecorator)
            {
                decorator.ShowAdorner();
            }
            else
            {
                decorator.HideAdorner();
            }
        }
    }
}
