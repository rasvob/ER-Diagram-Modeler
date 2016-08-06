using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace ER_Diagram_Modeler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Item1Command(object sender, ExecutedRoutedEventArgs e)
		{
			Trace.WriteLine("Executed");
		}


		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			DatabaseModelCanvas.AddElement(new Rectangle()
			{
				Width = 100,
				Height = 200,
				Fill = new SolidColorBrush(Color.FromRgb(150, 30, 20)),
				Stretch = Stretch.Fill
			});
		}
	}
}
