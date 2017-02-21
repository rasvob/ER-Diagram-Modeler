using System.Windows.Input;
using ER_Diagram_Modeler.Models.Designer;
using MahApps.Metro.Controls;

namespace ER_Diagram_Modeler.Dialogs
{
	/// <summary>
	/// Interaction logic for TableNameDialog.xaml
	/// </summary>
	public partial class TableNameDialog : MetroWindow
	{
		private TableModel _model;

		/// <summary>
		/// Table model
		/// </summary>
		public TableModel Model
		{
			get { return _model; }
			set
			{
				_model = value;
				DataContext = value;
			}
		}

		public TableNameDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Dialog result = true
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Proceed_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
		}

		/// <summary>
		/// Check if name is valid
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Proceed_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (Model == null)
			{
				return;
			}

			if (Model["Title"].Equals(string.Empty))
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}
	}
}
