using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	/// <summary>
	/// Adorner for resize of item
	/// </summary>
	public class DesignerItemDecorator : Control
    {
        private Adorner _adorner;

		/// <summary>
		/// Show overlay
		/// </summary>
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

		/// <summary>
		/// Show overlay
		/// </summary>
		/// <remarks>XAML</remarks>
        public static readonly DependencyProperty ShowDecoratorProperty =
            DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(DesignerItemDecorator),
            new FrameworkPropertyMetadata(false, ShowDecoratorProperty_Changed));

        public DesignerItemDecorator()
        {
            Unloaded += UnloadedHandler;
        }

		/// <summary>
		/// Hide overlay
		/// </summary>
        private void HideAdorner()
        {
            if (_adorner != null)
            {
                _adorner.Visibility = Visibility.Hidden;
            }
        }

		/// <summary>
		/// Show overlay
		/// </summary>
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

		/// <summary>
		/// Unload overlay
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void UnloadedHandler(object sender, RoutedEventArgs e)
        {
            if (_adorner != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
	            adornerLayer?.Remove(_adorner);
	            _adorner = null;
            }
        }

		/// <summary>
		/// Show or hide overlay
		/// </summary>
		/// <param name="d"></param>
		/// <param name="e"></param>
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
