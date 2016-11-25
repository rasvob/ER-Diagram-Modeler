﻿using System;
using System.Collections.Generic;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace ER_Diagram_Modeler.Dialogs
{
	/// <summary>
	/// Interaction logic for ForeignKeyCreatorDialog.xaml
	/// </summary>
	public partial class ForeignKeyCreatorDialog : MetroWindow, INotifyPropertyChanged
	{
		private string _relationshipName;
		private List<RowModelPair> _gridData = new List<RowModelPair>();
		private TableViewModel _sourceTableVm;
		private DatabaseModelDesignerViewModel _designerViewModel;
		private TableViewModel _destinationTableVm;

		public DatabaseModelDesignerViewModel DesignerViewModel
		{
			get { return _designerViewModel; }
			set
			{
				if (Equals(value, _designerViewModel)) return;
				_designerViewModel = value;
				OnPropertyChanged();
			}
		}

		public TableViewModel SourceTableVm
		{
			get { return _sourceTableVm; }
			set
			{
				if (Equals(value, _sourceTableVm)) return;
				_sourceTableVm = value;
				OnPropertyChanged();
			}
		}

		public TableViewModel DestinationTableVm
		{
			get { return _destinationTableVm; }
			set
			{
				if (Equals(value, _destinationTableVm)) return;
				_destinationTableVm = value;
				OnPropertyChanged();
			}
		}

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

		public string RelationshipName
		{
			get { return _relationshipName; }
			set
			{
				if (value == _relationshipName) return;
				_relationshipName = value;
				OnPropertyChanged();
			}
		}

		public ForeignKeyCreatorDialog(string relationShipName, TableViewModel source, TableViewModel dest, DatabaseModelDesignerViewModel designerViewModel)
		{
			InitializeComponent();
			RelationshipName = relationShipName;
			SourceTableVm = source;
			DestinationTableVm = dest;
			DesignerViewModel = designerViewModel;

			PrimaryAttributes.ItemsSource = SourceTableVm.Model.Attributes;
			PrimaryAttributes.DisplayMemberPath = "Name";

			ForeignAttributes.ItemsSource = DestinationTableVm.Model.Attributes;
			ForeignAttributes.DisplayMemberPath = "Name";
		}

		private async void Confirm_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var isOk = await CheckIntegrity();

			if (isOk)
			{
				ConnectionInfoViewModel model = new ConnectionInfoViewModel()
				{
					DestinationViewModel = DestinationTableVm,
					SourceViewModel = SourceTableVm,
				};

				model.RelationshipModel.Name = RelationshipName;
				model.RelationshipModel.Source = SourceTableVm.Model;
				model.RelationshipModel.Destination = DestinationTableVm.Model;
				model.RelationshipModel.Attributes.AddRange(GridData);
				model.BuildConnection();

				DesignerViewModel.ConnectionInfoViewModels.Add(model);
				DialogResult = true;
				Close();
			}
		}

		private void Cancel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		private async Task<bool> CheckIntegrity()
		{
			string err = "";

			var mySettings = new MetroDialogSettings()
			{
				AnimateShow = true,
				AffirmativeButtonText = "Ok",
				ColorScheme = MetroDialogOptions.ColorScheme
			};

			if (GridData.Any(pair => !pair.CanBeForeignKey(ref err)))
			{
				await this.ShowMessageAsync("Datatype problem", err, MessageDialogStyle.Affirmative, mySettings);
				return false;
			}

			if (RelationshipName.Length == 0)
			{
				err = "Name of foreign key constraint is mandatory";
				await this.ShowMessageAsync("Name problem", err, MessageDialogStyle.Affirmative, mySettings);
				return false;
			}

			if(DesignerViewModel.ConnectionInfoViewModels.Any(t => t.RelationshipModel.Name.Equals(RelationshipName)))
			{
				err = "Name of foreign key constraint is already in use";
				await this.ShowMessageAsync("Name problem", err, MessageDialogStyle.Affirmative, mySettings);
				return false;
			}

			if (GridData.Count == 0)
			{
				err = "You have to specify at least one attribute pair";
				await this.ShowMessageAsync("Attribute problem", err, MessageDialogStyle.Affirmative, mySettings);
				return false;
			}

			return true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}