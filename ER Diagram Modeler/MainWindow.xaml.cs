using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Oracle.ManagedDataAccess.Client;

namespace ER_Diagram_Modeler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindowViewModel MainWindowViewModel { get; set; }
		private TableModel _flyoutTableModel;
		private EditRowEventArgs _flyoutRowEventArgs = null;
		private readonly DatabaseUpdater _updater;

		public MainWindow()
		{
			SessionProvider.Instance.ConnectionType = ConnectionType.None;
			InitializeComponent();
			_updater = new DatabaseUpdater();
			MainWindowViewModel = new MainWindowViewModel();
			DataContext = MainWindowViewModel;
			DatabaseConnectionSidebar.ConnectionClick += DatabaseConnectionSidebarOnConnectionClick;
			DatabaseConnectionSidebar.AddTable += DatabaseConnectionSidebarOnAddTable;
			DatabaseConnectionSidebar.CreateMsSqlDatabase += DatabaseConnectionSidebarOnCreateMsSqlDatabase;
			DatabaseConnectionSidebar.DropMsSqlDatabase += DatabaseConnectionSidebarOnDropMsSqlDatabase;
		}

		private async void DatabaseConnectionSidebarOnDropMsSqlDatabase(object sender, string dbName)
		{
			var progress = await this.ShowProgressAsync($"Drop database {dbName}", "Please wait...");
			progress.SetIndeterminate();

			try
			{
				using(IMsSqlMapper mapper = new MsSqlMapper())
				{
					await Task.Factory.StartNew(() => mapper.DropDatabase(dbName));
					DatabaseConnectionSidebar.LoadMsSqlData(true);
					await progress.CloseAsync();
					await this.ShowMessageAsync("Drop database", $"Database {dbName} dropped successfully");
				}
			}
			catch(SqlException ex)
			{
				await progress.CloseAsync();
				await this.ShowMessageAsync("Drop database", ex.Message);
			}
		}

		private async void DatabaseConnectionSidebarOnCreateMsSqlDatabase(object sender, System.EventArgs eventArgs)
		{
			string name = await this.ShowInputAsync("Create database", "Database name");

			if (name == null || name.Equals(string.Empty))
			{
				return;
			}

			try
			{
				using (IMsSqlMapper mapper = new MsSqlMapper())
				{
					mapper.CreateDatabase(name);
					DatabaseConnectionSidebar.LoadMsSqlData(true);
				}
			}
			catch (SqlException exc)
			{
				await this.ShowMessageAsync("Create database", exc.Message);
			}
		}

		private async void DatabaseConnectionSidebarOnAddTable(object sender, TableModel model)
		{
			var idx = MainDocumentPane.SelectedContentIndex;

			if (idx < 0)
			{
				var title = await ShowNewDiagramDialog();

				if (title == null)
				{
					return;
				}

				DiagramFacade.CreateNewDiagram(this, title);

				idx = MainDocumentPane.SelectedContentIndex;
			}

			var content = MainDocumentPane.Children[idx].Content;

			var diagram = content as DatabaseModelDesigner;
			if (diagram == null)
			{
				return;
			}

			var facade = new DiagramFacade(diagram.ViewModel);
			facade.AddTableCallbackAction += async tableModel =>
			{
				await facade.AddRelationShipsForTable(model, diagram.ModelDesignerCanvas);
			};
			facade.AddTable(model);
		}

		private bool TryGetSelectedDesigner(out DatabaseModelDesigner designer)
		{
			var idx = MainDocumentPane.SelectedContentIndex;
			designer = null;

			if (idx < 0)
			{
				return false;
			}

			var content = MainDocumentPane.Children[idx].Content;
			designer = content as DatabaseModelDesigner;

			if (designer == null)
			{
				return false;
			}

			return true;
		}

		private void DatabaseConnectionSidebarOnConnectionClick(object sender, ConnectionType connectionType)
		{
			switch (connectionType)
			{
				case ConnectionType.SqlServer:
					var flyoutMsSql = Flyouts.Items[0] as Flyout;

					if(flyoutMsSql != null)
					{
						flyoutMsSql.IsOpen = !flyoutMsSql.IsOpen;
					}
					break;
				case ConnectionType.Oracle:
					var flyoutOracle = Flyouts.Items[2] as Flyout;

					if(flyoutOracle != null)
					{
						flyoutOracle.IsOpen = !flyoutOracle.IsOpen;
					}
					break;
			}
		}

		private void MenuItemTest_OnClick(object sender, RoutedEventArgs e)
		{
			
		}

		private void ChangeCanvasSize_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var activeDiagramModeler = MainDocumentPane.SelectedContent.Content as DatabaseModelDesigner;
			if (activeDiagramModeler != null)
			{
				var dialog = new CanvasDimensionDialog
				{
					Owner = this,
					CanvasWidth = activeDiagramModeler.ViewModel.CanvasWidth,
					CanvasHeight = activeDiagramModeler.ViewModel.CanvasWidth
				};
				dialog.ShowDialog();

				if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
				{
					activeDiagramModeler.ViewModel.CanvasWidth = dialog.CanvasWidth;
					activeDiagramModeler.ViewModel.CanvasHeight = dialog.CanvasHeight;
					activeDiagramModeler.CanvasDimensionsChanged();
				}
			}
		}

		private void ChangeCanvasSize_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var activeDiagramModeler = MainDocumentPane.SelectedContent?.Content as DatabaseModelDesigner;
			e.CanExecute = activeDiagramModeler != null;
		}

		private async void ConnectToMsSql_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ProgressDialogController progressDialogController = null;

			Func<ProgressDialogController, Task> closeProgress = async t =>
			{
				if(t != null)
				{
					if(t.IsOpen)
					{
						await t.CloseAsync();
					}
				}
			};

			try
			{
				progressDialogController = await this.ShowProgressAsync("Please wait", "Connecting to server...", false, new MetroDialogSettings()
				{
					AnimateShow = false, 
					AnimateHide = false
				});
				progressDialogController.SetIndeterminate();

				MsSqlDatabase db = new MsSqlDatabase();
				string name = null, pass = null;
				bool integratedSecurity = true;

				var server = MsSqlServerNameTextBox.Text;
				if(WinAuthSwitch.IsChecked != null) integratedSecurity = !WinAuthSwitch.IsChecked.Value;

				if(!integratedSecurity)
				{
					name = MsSqlUsernameTextBox.Text;
					pass = MsSqlPasswordBox.Password;
				}

				await db.BuildSession(server, integratedSecurity, name, pass);

				await closeProgress(progressDialogController);
				await this.ShowMessageAsync("Connected", $"Successfuly connected to {SessionProvider.Instance.ServerName}");

				var flyout = Flyouts.Items[0] as Flyout;

				if(flyout != null)
				{
					flyout.IsOpen = !flyout.IsOpen;
				}

				DatabaseConnectionSidebar.LoadMsSqlData();
			}
			catch (SqlException exception)
			{
				await closeProgress(progressDialogController);
				await this.ShowMessageAsync("Connection error", exception.Message);
				SessionProvider.Instance.ConnectionType = ConnectionType.None;
			}
		}

		private void ShowDatabaseConnectionLayout_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseConnectionLayoutAnchorable.Show();
		}

		private void ShowDatabaseConnectionLayout_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = DatabaseConnectionLayoutAnchorable.IsHidden;
		}

		private void ConnectToMsSql_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!MsSqlServerNameTextBox.Text.Any())
			{
				e.CanExecute = false;
				return;
			}

			if (WinAuthSwitch.IsChecked != null && (bool) WinAuthSwitch.IsChecked)
			{
				if (!MsSqlUsernameTextBox.Text.Any() || !MsSqlPasswordBox.Password.Any())
				{
					e.CanExecute = false;
					return;
				}
			}

			e.CanExecute = true;
		}

		private async void NewDiagram_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var result = await ShowNewDiagramDialog();

			if (result == null)
			{
				return;
			}

			DiagramFacade.CreateNewDiagram(this, result);
		}

		private async Task<string> ShowNewDiagramDialog()
		{
			var result = await this.ShowInputAsync("Diagram title", "Enter diagram title", new MetroDialogSettings()
			{
				DefaultText = $"Diagram_{Guid.NewGuid()}"
			});

			return result;
		}

		private void NewDiagram_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType != ConnectionType.None;
		}

		private async void ApplyAttributeEdit_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			TableModel refreshed;
			var res = _updater.AddOrUpdateCollumn(_flyoutTableModel, MainWindowViewModel.FlyoutRowModel, out refreshed, ref _flyoutRowEventArgs);

			if (res != null)
			{
				await this.ShowMessageAsync("Column error", res);
			}
			else
			{
				_flyoutTableModel.RefreshModel(refreshed);
			}
			
			ToggleRowFlyout();
		}

		private void ApplyAttributeEdit_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var row = MainWindowViewModel.FlyoutRowModel;

			if (row == null)
			{
				e.CanExecute = false;
				return;
			}

			e.CanExecute = row.IsValid();
		}

		public void AddNewRowHandler(object sender, TableModel args)
		{
			ToggleRowFlyout();
			MainWindowViewModel.FlyoutRowModel = new TableRowModel();
			_flyoutTableModel = args;
			_flyoutRowEventArgs = null;
		}

		public void EditRowHandler(object sender, EditRowEventArgs args)
		{
			ToggleRowFlyout();
			MainWindowViewModel.FlyoutRowModel = new TableRowModel(args.RowModel.Name, args.RowModel.Datatype);
			MainWindowViewModel.FlyoutRowModel.AllowNull = args.RowModel.AllowNull;
			_flyoutRowEventArgs = args;
			_flyoutTableModel = args.TableModel;
		}

		private void ToggleRowFlyout()
		{
			var flyout = Flyouts.Items[1] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}
		}

		public async void RenameTableHandler(object sender, TableModel e)
		{
			var originalName = e.Title;
			var dialog = new TableNameDialog()
			{
				Model = e,
				Owner = this
			};
			dialog.ShowDialog();

			string res = _updater.RenameTable(originalName, e);

			if (res != null)
			{
				await this.ShowMessageAsync("Rename table", res);
				return;
			}

			DatabaseConnectionSidebar.RefreshTreeData();
		}

		private async void RemoveColumn_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var res = _updater.RemoveColumn(_flyoutTableModel, ref _flyoutRowEventArgs);

			if(res != null)
			{
				await this.ShowMessageAsync("Drop column", res);
			}

			ToggleRowFlyout();
		}

		public async void RemoveRowHandler(object sender, EditRowEventArgs e)
		{
			_flyoutTableModel = e.TableModel;
			_flyoutRowEventArgs = e;
			var res = _updater.RemoveColumn(_flyoutTableModel, ref _flyoutRowEventArgs);

			if(res != null)
			{
				await this.ShowMessageAsync("Drop column", res);
			}
		}

		private void RemoveColumn_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _flyoutRowEventArgs != null;
		}

		public async void DropTableHandler(object sender, TableModel e)
		{
			var dialog = await this.ShowMessageAsync("Drop table", $"Do you really want to drop {e.Title} ? Changes can't be undone!",
				MessageDialogStyle.AffirmativeAndNegative);

			if (dialog == MessageDialogResult.Affirmative)
			{
				var res = _updater.DropTable(e);

				if (res != null)
				{
					await this.ShowMessageAsync("Drop table", res);
					return;
				}

				DatabaseModelDesigner designer;
				if (TryGetSelectedDesigner(out designer))
				{
					var facade = new DiagramFacade(designer);
					facade.RemoveTable(e);
				}
				DatabaseConnectionSidebar.RefreshTreeData();
			}
		}

		public async void UpdatePrimaryKeyConstraintHandler(object sender, TableModel e)
		{
			var res = _updater.UpdatePrimaryKeyConstraint(e);

			if(res != null)
			{
				await this.ShowMessageAsync("Primary key constraint", res);
			}
		}

		private async void ConnectToOracle_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			//TODO: REMOVE LOGIN CREDS
			ProgressDialogController progressDialogController = null;

			Func<ProgressDialogController, Task> closeProgress = async t =>
			{
				if(t != null)
				{
					if(t.IsOpen)
					{
						await t.CloseAsync();
					}
				}
			};

			try
			{
				progressDialogController = await this.ShowProgressAsync("Please wait", "Connecting to server...", false, new MetroDialogSettings()
				{
					AnimateShow = false,
					AnimateHide = false
				});
				progressDialogController.SetIndeterminate();

				var db = new OracleDatabase();
				await db.BuildSession(OracleServerNameTextBox.Text, OraclePortTextBox.Text, OracleSidTextBox.Text,
					OracleUsernameTextBox.Text, OraclePasswordBox.Password);
				using(IOracleMapper mapper = new OracleMapper())
				{
					TableModel model = mapper.ListTables().FirstOrDefault();
					if(model != null)
					{
						mapper.ListForeignKeys(model.Title);
					}
				}

				//Init work
				await Task.Factory.StartNew(() =>
				{
					
				});

				await closeProgress(progressDialogController);
				await this.ShowMessageAsync("Connected", $"Successfuly connected to {SessionProvider.Instance.ServerName}");

				var flyout = Flyouts.Items[2] as Flyout;

				if(flyout != null)
				{
					flyout.IsOpen = !flyout.IsOpen;
				}

				DatabaseConnectionSidebar.LoadOracleData();
			}
			catch (OracleException exception)
			{
				await closeProgress(progressDialogController);
				await this.ShowMessageAsync("Connection error", exception.Message);
				SessionProvider.Instance.ConnectionType = ConnectionType.None;
			}
		}

		private void ConnectToOracle_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			string[] inputBoxes = {
				OracleServerNameTextBox.Text, OraclePortTextBox.Text, OracleSidTextBox.Text, OracleUsernameTextBox.Text, OraclePasswordBox.Password
			};

			e.CanExecute = inputBoxes.All(t => t.Length > 0) && OraclePortTextBox.Text.All(char.IsDigit);
		}

		public void CreateTableHandler(object sender, System.EventArgs e)
		{
			DatabaseConnectionSidebar.RefreshTreeData();
		}
	}
}
