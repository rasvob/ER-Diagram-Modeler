﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Models.Helpers;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas;
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
		private Visibility _onUpdateVisibility = Visibility.Visible;
		private string[] _referentialAction;

		/// <summary>
		/// Array of actions: "NO ACTION", "CASCADE", "SET NULL", "SET DEFAULT"
		/// </summary>
		public string[] ReferentialAction
		{
			get { return _referentialAction; }
			set
			{
				if (Equals(value, _referentialAction)) return;
				_referentialAction = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Hide/Show OnUpdate action combobox
		/// </summary>
		public Visibility OnUpdateVisibility
		{
			get { return _onUpdateVisibility; }
			set
			{
				if (value == _onUpdateVisibility) return;
				_onUpdateVisibility = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Viewmodel for DatabaseModelDesignerViewModel
		/// </summary>
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

		/// <summary>
		/// Canvas for visualization
		/// </summary>
		public DesignerCanvas Canvas { get; set; }

		/// <summary>
		/// Table with primary key
		/// </summary>
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

		/// <summary>
		/// Table with foreign key
		/// </summary>
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

		/// <summary>
		/// Data for grid
		/// </summary>
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

		/// <summary>
		/// Name of constraint
		/// </summary>
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

		public ForeignKeyCreatorDialog(string relationShipName, TableViewModel source, TableViewModel dest, DatabaseModelDesignerViewModel designerViewModel, DesignerCanvas canvas)
		{
			InitializeComponent();
			RelationshipName = relationShipName;
			SourceTableVm = source;
			DestinationTableVm = dest;
			DesignerViewModel = designerViewModel;
			Canvas = canvas;

			PrimaryAttributes.ItemsSource = SourceTableVm.Model.Attributes.Where(t => t.PrimaryKey);
			PrimaryAttributes.DisplayMemberPath = "Name";

			ForeignAttributes.ItemsSource = DestinationTableVm.Model.Attributes;
			ForeignAttributes.DisplayMemberPath = "Name";

			switch (SessionProvider.Instance.ConnectionType)
			{
				case ConnectionType.None:
					break;
				case ConnectionType.SqlServer:
					ReferentialAction = new string[] { "NO ACTION", "CASCADE", "SET NULL", "SET DEFAULT" };
					break;
				case ConnectionType.Oracle:
					OnUpdateVisibility = Visibility.Collapsed;
					ReferentialAction = new string[] { "NO ACTION", "CASCADE", "SET NULL" };
					break;
			}
		}

		/// <summary>
		/// Create new
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Confirm_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var isOk = await CheckIntegrity();

			if (isOk)
			{
				var relModel = new RelationshipModel
				{
					Name = RelationshipName,
					Source = SourceTableVm.Model,
					Destination = DestinationTableVm.Model
				};
				relModel.Attributes.AddRange(GridData);
				relModel.Optionality = relModel.Attributes.All(t => t.Destination.AllowNull)
				? Optionality.Optional
				: Optionality.Mandatory;
				relModel.DeleteAction = OnDeleteComboBox.SelectedValue as string;
				relModel.UpdateAction = OnUpdateComboBox.SelectedValue as string;

				var updater = new DatabaseUpdater();
				string res = updater.AddRelationship(relModel);

				if (res != null)
				{
					await this.ShowMessageAsync("Add foreign key", res);
				}
				else
				{
					ConnectionInfoViewModel model = new ConnectionInfoViewModel()
					{
						DestinationViewModel = DestinationTableVm,
						SourceViewModel = SourceTableVm,
						DesignerCanvas = Canvas
					};
					model.RelationshipModel.RefreshModel(relModel);
					await model.BuildConnection3(DesignerViewModel);
					DesignerViewModel.ConnectionInfoViewModels.Add(model);
					DialogResult = true;
					Close();
				}
			}
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
		/// Are data valid - check
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
