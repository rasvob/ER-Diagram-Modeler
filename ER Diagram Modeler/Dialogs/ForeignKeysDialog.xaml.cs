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

			SetupFlyout();
		}

		private void SetupFlyout()
		{
			FlyoutPrimaryTableComboBox.ItemsSource = DatabaseModelDesignerViewModel.TableViewModels;
			FlyoutPrimaryTableComboBox.DisplayMemberPath = "Model.Title";
			FlyoutPrimaryTableComboBox.SelectedIndex = 0;

			FlyoutForeignTableComboBox.ItemsSource = DatabaseModelDesignerViewModel.TableViewModels;
			FlyoutForeignTableComboBox.DisplayMemberPath = "Model.Title";
			FlyoutForeignTableComboBox.SelectedIndex = 0;
		}

		private void RelationshipsListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
		{
			if (selectionChangedEventArgs.AddedItems.Count == 0) return;
			InfoViewModel = selectionChangedEventArgs.AddedItems[0] as ConnectionInfoViewModel;

			if (InfoViewModel?.RelationshipModel != null)
			{
				GridData = new List<RowModelPair>();
				GridData.AddRange(InfoViewModel.RelationshipModel.Attributes);
				OnPropertyChanged(nameof(GridData));
			}
		}

		private void AddNew_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var flyout = Flyouts.Items[0] as Flyout;

			if (flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}
		}

		private void Remove_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseModelDesignerViewModel.ConnectionInfoViewModels.Remove(InfoViewModel);
			InfoViewModel = null;
			GridData = new List<RowModelPair>();
			OnPropertyChanged(nameof(GridData));
			RelationshipsListBox.SelectedIndex = 0;
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

		private void CreateRelationship_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var source = FlyoutPrimaryTableComboBox.SelectedItem as TableViewModel;
			var dest = FlyoutForeignTableComboBox.SelectedItem as TableViewModel;

			var dialog = new ForeignKeyCreatorDialog(NewRelationshipTextBox.Text, source, dest, DatabaseModelDesignerViewModel);
			dialog.Owner = this;

			var flyout = Flyouts.Items[0] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = false;
			}

			bool? res = dialog.ShowDialog();

			if (res.HasValue && res.Value)
			{
				RelationshipsListBox.SelectedIndex = RelationshipsListBox.Items.Count - 1;
			}
		}

		private void CreateRelationship_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var source = FlyoutPrimaryTableComboBox.SelectedItem as TableViewModel;
			var dest = FlyoutForeignTableComboBox.SelectedItem as TableViewModel;
			var text = NewRelationshipTextBox.Text;

			if (source == null || dest == null)
			{
				e.CanExecute = false;
				return;
			}

			if (text.Length == 0)
			{
				NewRelationshipTextBox.Text = $"{source.Model.Title}_{dest.Model.Title}_FK";
				e.CanExecute = true;
				return;
			}

			e.CanExecute = !DatabaseModelDesignerViewModel.ConnectionInfoViewModels.Any(t => t.RelationshipModel.Name.Equals(text));
		}
	}
}
