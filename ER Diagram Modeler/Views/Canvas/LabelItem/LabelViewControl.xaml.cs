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
using MahApps.Metro.Controls.Dialogs;

namespace ER_Diagram_Modeler.Views.Canvas.LabelItem
{
	/// <summary>
	/// Interaction logic for LabelViewControl.xaml
	/// </summary>
	public partial class LabelViewControl : UserControl
	{
		public LabelViewModel ViewModel { get; set; }

		private string _originalText = string.Empty;

		public LabelViewControl()
		{
			InitializeComponent();
		}

		public LabelViewControl(LabelViewModel viewModel)
		{
			InitializeComponent();
			ViewModel = viewModel;
			DataContext = viewModel;
		}

		private void LabelTextBox_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var txt = sender as TextBox;

			if (txt != null)
			{
				txt.IsReadOnly = false;
				_originalText = ViewModel.LabelText;
				txt.SelectAll();
			}
		}

		private void LabelTextBox_OnLostFocus(object sender, RoutedEventArgs e)
		{
			var txt = sender as TextBox;
			txt.IsReadOnly = true;
		}

		private void LabelTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			var txt = sender as TextBox;

			if (txt != null)
			{
				if(e.Key == Key.Escape)
				{
					ViewModel.LabelText = _originalText;
					txt.IsReadOnly = true;
				}
			}
		}
	}
}
