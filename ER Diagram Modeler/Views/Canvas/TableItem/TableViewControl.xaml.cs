using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
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
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
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
		public event EventHandler<EditRowEventArgs> RemoveSelectedRow;
		public event EventHandler<TableModel> RenameTable; 
		public event EventHandler<TableModel> DropTable;
		public event EventHandler<TableModel> UpdatePrimaryKeyConstraint;
		public event EventHandler<TableModel> RefreshTableData;

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
			EditRowEventArgs args;
			if(TryGetSelectedRow(out args))
			{
				OnEditSelectedRow(args);
			}
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

		private void TableDataGrid_OnPreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Delete) return;

			EditRowEventArgs args;

			if (TryGetSelectedRow(out args))
			{
				OnRemoveSelectedRow(args);
				Trace.WriteLine(args.RowModel);
				e.Handled = true;
			}
		}

		private bool TryGetSelectedRow(out EditRowEventArgs args)
		{
			int index = TableDataGrid.SelectedIndex;

			if(index < 0)
			{
				args = null;
				return false;
			}

			args = new EditRowEventArgs()
			{
				RowModel = ViewModel.Model.Attributes[index],
				TableModel = ViewModel.Model
			};

			return true;
		}

		protected virtual void OnRemoveSelectedRow(EditRowEventArgs e)
		{
			RemoveSelectedRow?.Invoke(this, e);
		}

		private void DropTableItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnDropTable(ViewModel.Model);
		}

		protected virtual void OnDropTable(TableModel e)
		{
			DropTable?.Invoke(this, e);
		}

		private void ModPrimaryKeyItem_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new PrimaryKeyDialog
			{
				Owner = Window.GetWindow(this),
				TableModel = ViewModel.Model
			};

			bool? res = dialog.ShowDialog();

			if (res.HasValue)
			{
				if (res.Value)
				{
					OnUpdatePrimaryKeyConstraint(ViewModel.Model);
				}
			}
		}

		protected virtual void OnUpdatePrimaryKeyConstraint(TableModel e)
		{
			UpdatePrimaryKeyConstraint?.Invoke(this, e);
		}

		protected virtual void OnRefreshTableData(TableModel e)
		{
			RefreshTableData?.Invoke(this, e);
		}
	}
}
