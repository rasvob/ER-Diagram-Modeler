using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ER_Diagram_Modeler.Dialogs
{
	/// <summary>
	/// Interaction logic for ForeignKeysDialog.xaml
	/// </summary>
	public partial class ForeignKeysDialog : MetroWindow, INotifyPropertyChanged
	{
		/// <summary>
		/// Viewmodel for ConnectionInfoViewModel 
		/// </summary>
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

		/// <summary>
		/// Viewmodel for DatabaseModelDesignerViewModel
		/// </summary>
		public DatabaseModelDesignerViewModel DatabaseModelDesignerViewModel { get; set; }

		
		private ObservableCollection<RowModelPair> _gridData = new ObservableCollection<RowModelPair>();
		private ConnectionInfoViewModel _infoViewModel;

		/// <summary>
		/// Collection of RowModelPair
		/// </summary>
		public ObservableCollection<RowModelPair> GridData
		{
			get { return _gridData; }
			set
			{
				if (Equals(value, _gridData)) return;
				_gridData = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Canvas for visualization
		/// </summary>
		public DesignerCanvas Canvas { get; set; }

		public ForeignKeysDialog(DatabaseModelDesignerViewModel viewModel, ConnectionInfoViewModel selected = null)
		{
			InitializeComponent();
			DatabaseModelDesignerViewModel = viewModel;

			int index = 0;

			if (selected != null)
			{
				index = DatabaseModelDesignerViewModel.ConnectionInfoViewModels.IndexOf(selected);
			}

			SetupListBoxData(index);
			SetupFlyout();
		}

		/// <summary>
		/// Fill grid data
		/// </summary>
		/// <param name="index">Selected index</param>
		private void SetupListBoxData(int index)
		{
			RelationshipsListBox.ItemsSource =
				DatabaseModelDesignerViewModel.ConnectionInfoViewModels;
			RelationshipsListBox.DisplayMemberPath = "RelationshipModel.Name";
			RelationshipsListBox.SelectionChanged += RelationshipsListBoxOnSelectionChanged;
			RelationshipsListBox.SelectedIndex = index;
		}

		/// <summary>
		/// Setup flyout
		/// </summary>
		private void SetupFlyout()
		{
			FlyoutPrimaryTableComboBox.ItemsSource = DatabaseModelDesignerViewModel.TableViewModels;
			FlyoutPrimaryTableComboBox.DisplayMemberPath = "Model.Title";
			FlyoutPrimaryTableComboBox.SelectedIndex = 0;

			FlyoutForeignTableComboBox.ItemsSource = DatabaseModelDesignerViewModel.TableViewModels;
			FlyoutForeignTableComboBox.DisplayMemberPath = "Model.Title";
			FlyoutForeignTableComboBox.SelectedIndex = 0;
		}

		/// <summary>
		/// Update gridview when foreign key is selected
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="selectionChangedEventArgs"></param>
		private void RelationshipsListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
		{
			if (selectionChangedEventArgs.AddedItems.Count == 0)
			{
				GridData.Clear();
				return;
			}

			InfoViewModel = selectionChangedEventArgs.AddedItems[0] as ConnectionInfoViewModel;

			if (InfoViewModel?.RelationshipModel != null)
			{
				GridData.Clear();
				foreach (RowModelPair pair in InfoViewModel.RelationshipModel.Attributes)
				{
					GridData.Add(pair);
				}
			}
		}

		/// <summary>
		/// Open flyout for FK creation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddNew_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var flyout = Flyouts.Items[0] as Flyout;

			if (flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}
		}

		/// <summary>
		/// Drop foreign key
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Remove_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var updater = new DatabaseUpdater();
			var res = updater.RemoveRelationship(InfoViewModel.RelationshipModel);

			if (res != null)
			{
				await this.ShowMessageAsync("Drop foreign key", res);
				return;
			}

			DatabaseModelDesignerViewModel.ConnectionInfoViewModels.Remove(InfoViewModel);
			InfoViewModel = null;
			RelationshipsListBox.SelectedIndex = 0;
		}

		/// <summary>
		/// Can drop check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Remove_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (InfoViewModel == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = true;
		}

		/// <summary>
		/// Close dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cancel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Create new FK - open dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateRelationship_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var source = FlyoutPrimaryTableComboBox.SelectedItem as TableViewModel;
			var dest = FlyoutForeignTableComboBox.SelectedItem as TableViewModel;

			var dialog = new ForeignKeyCreatorDialog(NewRelationshipTextBox.Text, source, dest, DatabaseModelDesignerViewModel, Canvas);
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

		/// <summary>
		/// Is create new flyout data valid - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
