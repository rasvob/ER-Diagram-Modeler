using System.Linq;
using System.Windows.Input;
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

		/// <summary>
		/// Table model for primary key configuration
		/// </summary>
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

		/// <summary>
		/// Viewmodel for dialog
		/// </summary>
		public PrimaryKeyDialogViewModel ViewModel { get; set; } = new PrimaryKeyDialogViewModel();

		public PrimaryKeyDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Update primary key constraint
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Confirm_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			foreach (TableRowModel model in TableModel.Attributes)
			{
				model.PrimaryKey = ViewModel.RowModels.Where(t => t.PrimaryKey).Any(s => s.Name.Equals(model.Name));
			}

			DialogResult = true;
		}

		/// <summary>
		/// Check if config is valid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Confirm_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (TableModel == null)
			{
				return;
			}

			e.CanExecute = true;
		}

		/// <summary>
		/// DialogResult = false;
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cancel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
