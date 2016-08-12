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
using ER_Diagram_Modeler.ViewModels;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	/// <summary>
	/// Interaction logic for TableViewControl.xaml
	/// </summary>
	public partial class TableViewControl : UserControl
	{
		public TableViewModel ViewModel { get; set; }

		public TableViewControl()
		{
			InitializeComponent();
		}

		public TableViewControl(TableViewModel viewModel)
		{
			InitializeComponent();
			DataContext = viewModel;
		}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			Trace.WriteLine("STD");
		}

		private void MenuItem_NameOnly_OnClick(object sender, RoutedEventArgs e)
		{
			Trace.WriteLine("Name only");
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			TableContextMenu.IsOpen = true;
		}
	}
}
