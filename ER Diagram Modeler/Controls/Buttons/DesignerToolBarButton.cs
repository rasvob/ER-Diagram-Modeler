using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ER_Diagram_Modeler.Controls.Buttons
{
	/// <summary>
	/// Toolbar button control
	/// </summary>
	class DesignerToolBarButton: Button
	{
		/// <summary>
		/// Icon
		/// </summary>
		public Geometry Icon
		{
			get { return (Geometry) GetValue(IconProperty); }
			set
			{
				SetValue(IconProperty, value);
			}
		}

		/// <summary>
		/// Is button checked
		/// </summary>
		public bool IsChecked
		{
			get { return (bool) GetValue(IsCheckedProperty); }
			set
			{
				SetValue(IsCheckedProperty, value);
			}
		}

		/// <summary>
		/// XAML Property
		/// </summary>
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(DesignerToolBarButton), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// XAML Property
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(DesignerToolBarButton), new FrameworkPropertyMetadata(false));

		public DesignerToolBarButton()
		{
			Style = Application.Current.TryFindResource("DesignerToolBarButtonStyle") as Style;
		}
	}
}
