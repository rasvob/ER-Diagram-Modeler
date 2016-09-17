using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.ViewModels;

namespace ER_Diagram_Modeler.Views.Canvas.ZoomBox
{
	/// <summary>
	/// Interaction logic for ZoomBox.xaml
	/// </summary>
	public partial class ZoomBox : UserControl
	{
		public DatabaseModelDesignerViewModel ViewModel { get; set; }

		public ScrollViewer ScrollOwner
		{
			get { return (ScrollViewer) GetValue(ScrollOwnerProperty); }
			set
			{
				SetValue(ScrollOwnerProperty, value);
			}
		}

		public DesignerCanvas DesignerCanvas
		{
			get { return (DesignerCanvas)GetValue(DesignerCanvasProperty); }
			set
			{
				SetValue(DesignerCanvasProperty, value);
			}
		}

		public static readonly DependencyProperty ScrollOwnerProperty = 
			DependencyProperty.Register("ScrollOwner", typeof(ScrollViewer), typeof(ZoomBox), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty DesignerCanvasProperty =
			DependencyProperty.Register("DesignerCanvas", typeof(DesignerCanvas), typeof(ZoomBox), new FrameworkPropertyMetadata(null));


		public ZoomBox()
		{
			InitializeComponent();
		}

		private void ZoomBoxThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
		{
			ScrollOwner.ScrollToVerticalOffset(ScrollOwner.VerticalOffset + e.VerticalChange);
			ScrollOwner.ScrollToHorizontalOffset(ScrollOwner.HorizontalOffset + e.HorizontalChange);
		}

		private void ShowHideButton_OnClick(object sender, RoutedEventArgs e)
		{
			ViewModel.ZoomBoxVisibility = ViewModel.ZoomBoxVisibility ==  Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
		}
	}
}
