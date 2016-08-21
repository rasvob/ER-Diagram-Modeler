using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
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

		private void Proceed_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
		}

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
