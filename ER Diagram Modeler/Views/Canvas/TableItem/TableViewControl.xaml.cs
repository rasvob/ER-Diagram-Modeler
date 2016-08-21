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
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;

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
			ViewModel = viewModel;
			DataContext = viewModel;
		}

		private void UncheckViewModeMenuItems()
		{
			foreach (MenuItem item in MenuItemViewModeList.Items)
			{
				item.IsChecked = false;
			}
		}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			ViewModel.ViewMode = TableViewMode.Standard;
			UncheckViewModeMenuItems();
			var item = sender as MenuItem;
			if (item != null) item.IsChecked = true;
		}

		private void MenuItem_NameOnly_OnClick(object sender, RoutedEventArgs e)
		{
			ViewModel.ViewMode = TableViewMode.NameOnly;
			UncheckViewModeMenuItems();
			var item = sender as MenuItem;
			if(item != null) item.IsChecked = true;
		}

		private void ChangeTableNameItem_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new TableNameDialog()
			{
				Model = ViewModel.Model,
				Owner = Window.GetWindow(this)
			};
			dialog.ShowDialog();
		}
	}
}
