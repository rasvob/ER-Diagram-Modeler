﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using MahApps.Metro.Controls.Dialogs;
using MenuItem = System.Windows.Controls.MenuItem;
using UserControl = System.Windows.Controls.UserControl;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	/// <summary>
	/// Interaction logic for TableViewControl.xaml
	/// </summary>
	public partial class TableViewControl : UserControl
	{
		public TableViewModel ViewModel { get; set; }
		public event EventHandler<TableModel> AddNewRow;
		public event EventHandler<EditRowEventArgs> EditSelectedRow;
		public event EventHandler<TableModel> RenameTable; 

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
			OnRenameTable(ViewModel.Model);
		}

		private void MenuItemShowDatatype_OnClick(object sender, RoutedEventArgs e)
		{
			var item = sender as MenuItem;

			if (item != null)
			{
				if (item.IsChecked)
				{
					TableDataGrid.Columns[2].Visibility = Visibility.Collapsed;
				}
				else
				{
					TableDataGrid.Columns[2].Visibility = Visibility.Visible;
				}
				item.IsChecked = !item.IsChecked;
			}
		}

		private void TableDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			int index = TableDataGrid.SelectedIndex;

			if (index < 0)
			{
				return;
			}

			OnEditSelectedRow(new EditRowEventArgs()
			{
				RowModel = ViewModel.Model.Attributes[index],
				TableModel = ViewModel.Model
			});			
		}

		private void AddNewRow_OnClick(object sender, RoutedEventArgs e)
		{
			OnAddNewRow(ViewModel.Model);
		}

		protected virtual void OnAddNewRow(TableModel e)
		{
			AddNewRow?.Invoke(this, e);
		}

		protected virtual void OnEditSelectedRow(EditRowEventArgs e)
		{
			EditSelectedRow?.Invoke(this, e);
		}

		protected virtual void OnRenameTable(TableModel e)
		{
			RenameTable?.Invoke(this, e);
		}
	}
}
