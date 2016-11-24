using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels;
using MahApps.Metro.Controls;

namespace ER_Diagram_Modeler.Dialogs
{
	/// <summary>
	/// Interaction logic for ForeignKeysDialog.xaml
	/// </summary>
	public partial class ForeignKeysDialog : MetroWindow, INotifyPropertyChanged
	{
		public ConnectionInfoViewModel InfoViewModel
		{
			get { return _infoViewModel; }
			set
			{
				if (Equals(value, _infoViewModel)) return;
				_infoViewModel = value;
				OnPropertyChanged();
			}
		}

		public DatabaseModelDesignerViewModel DatabaseModelDesignerViewModel { get; set; }
		private List<RowModelPair> _gridData = new List<RowModelPair>();
		private ConnectionInfoViewModel _infoViewModel;

		public List<RowModelPair> GridData
		{
			get { return _gridData; }
			set
			{
				if (Equals(value, _gridData)) return;
				_gridData = value;
				OnPropertyChanged();
			}
		}

		public ForeignKeysDialog(DatabaseModelDesignerViewModel viewModel)
		{
			InitializeComponent();
			DatabaseModelDesignerViewModel = viewModel;


			RelationshipsListBox.ItemsSource =
				DatabaseModelDesignerViewModel.ConnectionInfoViewModels;
			RelationshipsListBox.DisplayMemberPath = "RelationshipModel.Name";
			RelationshipsListBox.SelectionChanged += RelationshipsListBoxOnSelectionChanged;
			RelationshipsListBox.SelectedIndex = 0;
		}

		private void RelationshipsListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
		{
			if (selectionChangedEventArgs.AddedItems.Count == 0) return;
			InfoViewModel = selectionChangedEventArgs.AddedItems[0] as ConnectionInfoViewModel;

			if (InfoViewModel?.RelationshipModel != null)
			{
				GridData.Clear();
				GridData.AddRange(InfoViewModel.RelationshipModel.Attributes);
				OnPropertyChanged(nameof(GridData));
			}
		}

		private void AddNew_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Debug.WriteLine("Add test");
		}

		private void Remove_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{

		}

		private void Remove_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (InfoViewModel == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		private void Cancel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
