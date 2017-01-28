using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using MahApps.Metro.Controls;

namespace ER_Diagram_Modeler.Dialogs
{
	/// <summary>
	/// Interaction logic for PrimaryKeyDialog.xaml
	/// </summary>
	public partial class PrimaryKeyDialog : MetroWindow
	{
		private TableModel _tableModel;

		public TableModel TableModel
		{
			get { return _tableModel; }
			set
			{
				_tableModel = value;
				ViewModel.TableName = value.Title;
				foreach (TableRowModel model in value.Attributes.Where(t => !t.AllowNull))
				{
					ViewModel.RowModels.Add(new TableRowModel(model.Name, model.Datatype)
					{
						PrimaryKey = model.PrimaryKey
					});
				}
				DataContext = ViewModel;
			}
		}

		public PrimaryKeyDialogViewModel ViewModel { get; set; } = new PrimaryKeyDialogViewModel();

		public PrimaryKeyDialog()
		{
			InitializeComponent();
		}

		private void Confirm_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (TableRowModel model in TableModel.Attributes)
			{
				model.PrimaryKey = ViewModel.RowModels.Where(t => t.PrimaryKey).Any(s => s.Name.Equals(model.Name));
			}

			DialogResult = true;
		}

		private void Confirm_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (TableModel == null)
			{
				return;
			}

			e.CanExecute = true;
		}

		private void Cancel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
