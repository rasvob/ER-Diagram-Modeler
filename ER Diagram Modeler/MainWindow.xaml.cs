using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Panels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Oracle.ManagedDataAccess.Client;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace ER_Diagram_Modeler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		/// <summary>
		/// ViewModels for Main window
		/// </summary>
		public MainWindowViewModel MainWindowViewModel { get; set; }

		/// <summary>
		/// Modify row helper event args
		/// </summary>
		private TableModel _flyoutTableModel;

		/// <summary>
		/// Modify row helper event args
		/// </summary>
		private EditRowEventArgs _flyoutRowEventArgs = null;

		/// <summary>
		/// DB Structure updated
		/// </summary>
		private readonly DatabaseUpdater _updater;

		/// <summary>
		/// Application initialization with subscribtion for connection panel events
		/// </summary>
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
			DatabaseConnectionSidebar.AddDiagram += DatabaseConnectionSidebarOnAddDiagram;
			DatabaseConnectionSidebar.DropDiagram += DatabaseConnectionSidebarOnDropDiagram;
			DatabaseConnectionSidebar.DisconnectClick += DatabaseConnectionSidebarOnDisconnectClick;
			DatabaseConnectionSidebar.MsSqlDatabaseChanged += DatabaseConnectionSidebarOnMsSqlDatabaseChanged;
		}

		/// <summary>
		/// Changed DB in Connection panel - MS Sql Server only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="s">Selected DB</param>
		private async void DatabaseConnectionSidebarOnMsSqlDatabaseChanged(object sender, string s)
		{
			await DiagramFacade.CloseDiagramsOnDisconnect(this);
			SessionProvider.Instance.Database = s;
			DatabaseConnectionSidebar.LoadMsSqlTreeViewData();
		}

		/// <summary>
		/// End current session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private async void DatabaseConnectionSidebarOnDisconnectClick(object sender, System.EventArgs eventArgs)
		{
			await DiagramFacade.CloseDiagramsOnDisconnect(this);
			SessionProvider.Instance.Disconnect();
			DatabaseConnectionSidebar.HideDatabaseStackPanels();
		}

		/// <summary>
		/// Delete saved diagram
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="diagramModel">Diagram for deletion</param>
		private void DatabaseConnectionSidebarOnDropDiagram(object sender, DiagramModel diagramModel)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			var res = ctx.DeleteDiagram(diagramModel.Name);
			DatabaseConnectionSidebar.RefreshTreeData();
		}

		/// <summary>
		/// Open saved diagram
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="diagramModel">Diagram for opening</param>
		private void DatabaseConnectionSidebarOnAddDiagram(object sender, DiagramModel diagramModel)
		{
			DiagramFacade.CreateNewDiagram(this, diagramModel.Name);
			DatabaseModelDesigner designer;
			if (TryGetSelectedDesigner(out designer))
			{
				var facade = new DiagramFacade(designer.ViewModel);
				facade.LoadDiagram(designer.ModelDesignerCanvas, XDocument.Parse(diagramModel.Xml));
			}
			DatabaseConnectionSidebar.RefreshTreeData();
		}

		/// <summary>
		/// Drop selected DB - MS Sql Server only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dbName">DB for drop</param>
		private async void DatabaseConnectionSidebarOnDropMsSqlDatabase(object sender, string dbName)
		{
			if (dbName.Equals(SessionProvider.Instance.Database))
			{
				await this.ShowMessageAsync("Drop database", $"Database {dbName} is currently in use");
				return;
			}

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

		/// <summary>
		/// Create new DB - MS Sql Server only
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
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

		/// <summary>
		/// Add table to active diagram
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="model">Added table</param>
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
				var title = await ShowNewDiagramDialog();

				if(title == null)
				{
					return;
				}

				DiagramFacade.CreateNewDiagram(this, title);

				idx = MainDocumentPane.SelectedContentIndex;
			}

			content = MainDocumentPane.Children[idx].Content;
			diagram = content as DatabaseModelDesigner;

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

		/// <summary>
		/// Try to get active diagram
		/// </summary>
		/// <param name="designer">Active digram, null if there is none</param>
		/// <returns></returns>
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

			return designer != null;
		}

		/// <summary>
		/// Try to get active panel
		/// </summary>
		/// <param name="panel">Selected panel, null if there is no active panel</param>
		/// <returns></returns>
		private bool TryGetSelectedPanel(out LayoutAnchorable panel)
		{
			var idx = MainDocumentPane.SelectedContentIndex;
			panel = null;

			if(idx < 0)
			{
				return false;
			}

			panel = MainDocumentPane.Children[idx] as LayoutAnchorable;

			return panel != null;
		}

		/// <summary>
		/// Open flyout for connection
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="connectionType">Type of session</param>
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

		/// <summary>
		/// Change diagram canvas size
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ChangeCanvasSize_OnExecuted(object sender, ExecutedRoutedEventArgs e)
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

				if(dialog.DialogResult.HasValue && dialog.DialogResult.Value)
				{
					activeDiagramModeler.ViewModel.CanvasWidth = dialog.CanvasWidth;
					activeDiagramModeler.ViewModel.CanvasHeight = dialog.CanvasHeight;
					await activeDiagramModeler.CanvasDimensionsChanged();
				}
			}
		}

		/// <summary>
		/// Can execute diagram canvas size change - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ChangeCanvasSize_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var activeDiagramModeler = MainDocumentPane.SelectedContent?.Content as DatabaseModelDesigner;
			e.CanExecute = activeDiagramModeler != null;
		}

		/// <summary>
		/// Connect to MS SQL Server
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ConnectToMsSql_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			await DiagramFacade.CloseDiagramsOnDisconnect(this);
			SessionProvider.Instance.Disconnect();
			DatabaseConnectionSidebar.HideDatabaseStackPanels();

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

		/// <summary>
		/// Show hidden connection panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDatabaseConnectionLayout_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseConnectionLayoutAnchorable.Show();
		}

		/// <summary>
		/// Is connection panel hidden - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDatabaseConnectionLayout_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = DatabaseConnectionLayoutAnchorable.IsHidden;
		}

		/// <summary>
		/// Are MS Sql connection parameters valid - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Create new diagram
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void NewDiagram_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var result = await ShowNewDiagramDialog();

			if (result == null)
			{
				return;
			}

			DiagramFacade.CreateNewDiagram(this, result);
		}

		/// <summary>
		/// Show name dialog for new diagram
		/// </summary>
		/// <returns></returns>
		private async Task<string> ShowNewDiagramDialog()
		{
			var result = await this.ShowInputAsync("Diagram title", "Enter diagram title", new MetroDialogSettings()
			{
				DefaultText = $"Diagram_{Guid.NewGuid()}"
			});

			return result;
		}

		/// <summary>
		/// Can create new digram check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NewDiagram_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType != ConnectionType.None;
		}

		/// <summary>
		/// Alter column in table
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ApplyAttributeEdit_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			TableModel refreshed;
			var res = _updater.AddOrUpdateCollumn(_flyoutTableModel, MainWindowViewModel.FlyoutRowModel, out refreshed, ref _flyoutRowEventArgs);

			if (res != null)
			{
				await this.ShowMessageAsync("Column error", res);
				Output.WriteLine(OutputPanelListener.PrepareException(res));
			}
			else
			{
				_flyoutTableModel.RefreshModel(refreshed);
			}
			
			ToggleRowFlyout();
		}

		/// <summary>
		/// Can alter table column check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Add new row event handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void AddNewRowHandler(object sender, TableModel args)
		{
			ToggleRowFlyout();
			MainWindowViewModel.FlyoutRowModel = new TableRowModel();
			_flyoutTableModel = args;
			_flyoutRowEventArgs = null;
		}

		/// <summary>
		/// Edit row event handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void EditRowHandler(object sender, EditRowEventArgs args)
		{
			ToggleRowFlyout();
			MainWindowViewModel.FlyoutRowModel = new TableRowModel(args.RowModel.Name, args.RowModel.Datatype);
			MainWindowViewModel.FlyoutRowModel.AllowNull = args.RowModel.AllowNull;
			_flyoutRowEventArgs = args;
			_flyoutTableModel = args.TableModel;
		}

		/// <summary>
		/// Helper method for showing row edit flyout
		/// </summary>
		private void ToggleRowFlyout()
		{
			var flyout = Flyouts.Items[1] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}
		}

		/// <summary>
		/// Rename selected table and refresh treeview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
				Output.WriteLine(OutputPanelListener.PrepareException(res));
				return;
			}

			DatabaseConnectionSidebar.RefreshTreeData();
		}

		/// <summary>
		/// Drop column from DB table
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void RemoveColumn_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var res = _updater.RemoveColumn(_flyoutTableModel, ref _flyoutRowEventArgs);

			if(res != null)
			{
				await this.ShowMessageAsync("Drop column", res);
				Output.WriteLine(OutputPanelListener.PrepareException(res));
			}

			ToggleRowFlyout();
		}

		/// <summary>
		/// Drop column handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public async void RemoveRowHandler(object sender, EditRowEventArgs e)
		{
			_flyoutTableModel = e.TableModel;
			_flyoutRowEventArgs = e;
			var res = _updater.RemoveColumn(_flyoutTableModel, ref _flyoutRowEventArgs);

			if(res != null)
			{
				await this.ShowMessageAsync("Drop column", res);
				Output.WriteLine(OutputPanelListener.PrepareException(res));
			}
		}

		/// <summary>
		/// Drop column - Can execute method
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveColumn_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _flyoutRowEventArgs != null;
		}

		/// <summary>
		/// Drop table from Db
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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
					Output.WriteLine(OutputPanelListener.PrepareException(res));
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

		/// <summary>
		/// Update primary key constraint
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public async void UpdatePrimaryKeyConstraintHandler(object sender, TableModel e)
		{
			var res = _updater.UpdatePrimaryKeyConstraint(e);

			if(res != null)
			{
				await this.ShowMessageAsync("Primary key constraint", res);
				Output.WriteLine(OutputPanelListener.PrepareException(res));
			}
		}

		/// <summary>
		/// Connect to Oracle
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ConnectToOracle_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			await DiagramFacade.CloseDiagramsOnDisconnect(this);
			SessionProvider.Instance.Disconnect();
			DatabaseConnectionSidebar.HideDatabaseStackPanels();

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
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
				SessionProvider.Instance.ConnectionType = ConnectionType.None;
			}
		}

		/// <summary>
		/// Are Oracle connection parameters valid - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectToOracle_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			string[] inputBoxes = {
				OracleServerNameTextBox.Text, OraclePortTextBox.Text, OracleSidTextBox.Text, OracleUsernameTextBox.Text, OraclePasswordBox.Password
			};

			e.CanExecute = inputBoxes.All(t => t.Length > 0) && OraclePortTextBox.Text.All(char.IsDigit);
		}

		/// <summary>
		/// Refresh treeview after table creation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CreateTableHandler(object sender, System.EventArgs e)
		{
			DatabaseConnectionSidebar.RefreshTreeData();
		}

		/// <summary>
		/// Synchronize diagram after activation of panel with DB structure
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public async void AnchorableDesignerActiveChangedHandler(object sender, System.EventArgs e)
		{
			var anchorable = sender as LayoutAnchorable;
			
			DatabaseModelDesigner designer = anchorable?.Content as DatabaseModelDesigner;

			if (designer == null || !anchorable.IsActive)
			{
				return;
			}

			var facade = new DiagramFacade(designer.ViewModel);
			await facade.RefreshDiagram(designer.ModelDesignerCanvas);
		}

		/// <summary>
		/// Save diagram and refresh treeview in panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveDiagram_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseModelDesigner designer;
			if(TryGetSelectedDesigner(out designer))
			{
				SaveDiagramAndRefresh(designer.ViewModel);
			}
		}

		/// <summary>
		/// Save current diagram - Can execute method
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveDiagram_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			DatabaseModelDesigner designer;
			e.CanExecute = TryGetSelectedDesigner(out designer);
		}

		/// <summary>
		/// Save current diagram with new name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void SaveDiagramAs_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseModelDesigner designer;
			if(TryGetSelectedDesigner(out designer))
			{
				LayoutAnchorable panel;
				if (TryGetSelectedPanel(out panel))
				{
					string result = await this.ShowInputAsync("Save as...", "Diagram name", new MetroDialogSettings()
					{
						DefaultText = designer.ViewModel.DiagramTitle,
						AnimateShow = true
					});

					if(!string.IsNullOrEmpty(result))
					{
						designer.ViewModel.DiagramTitle = result;
						panel.Title = result;
					}
				}
				
				SaveDiagramAndRefresh(designer.ViewModel);
			}
		}

		/// <summary>
		/// Save diagram to DB and refresh treeview in panel
		/// </summary>
		/// <param name="vm"></param>
		private void SaveDiagramAndRefresh(DatabaseModelDesignerViewModel vm)
		{
			var facade = new DiagramFacade(vm);
			var res = facade.SaveDiagram();
			Output.WriteLine(DiagramFacade.DiagramSaved);
			DatabaseConnectionSidebar.RefreshTreeData();
		}

		/// <summary>
		/// Show hidden Output panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowOutputLayout_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			OutputLayoutAnchorable.Show();
		}

		/// <summary>
		/// Show hidden Output panel - Can execute method
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowOutputLayout_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = OutputLayoutAnchorable.IsHidden;
		}

		/// <summary>
		/// Window closing
		/// </summary>
		/// <param name="sender">Current window</param>
		/// <param name="args">CancelEventArgs</param>
		private void MainWindow_OnClosing(object sender, CancelEventArgs args)
		{
			try
			{
				DiagramFacade.SaveAllDiagrams(this);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		/// <summary>
		/// Can execute new query command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NewQuery_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType != ConnectionType.None;
		}

		/// <summary>
		/// Add new query panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void NewQuery_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var title = await this.ShowInputAsync("New query", "Query name", new MetroDialogSettings { DefaultText = "SQL Query" });

			if (title != string.Empty)
			{
				var panel = new QueryPanel();
				panel.QueryResultReady += PanelOnQueryResultReady;
				panel.BuildNewQueryPanel(this, title);

				MainDocumentPane.Children.Add(panel.Anchorable);
				int indexOf = MainDocumentPane.Children.IndexOf(panel.Anchorable);
				MainDocumentPane.SelectedContentIndex = indexOf;
			}
		}

		private void PanelOnQueryResultReady(object sender, DataSet dataSet)
		{
			QueryResultPanel.RefreshData(dataSet);
			QueryResultLayoutAnchorable.Show();
		}

		/// <summary>
		/// Show hidden query result panel - Can execute method
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDatasetLayout_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = QueryResultLayoutAnchorable.IsHidden;
		}

		/// <summary>
		/// Show hidden query result panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowDatasetLayout_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			QueryResultLayoutAnchorable.Show();
		}

		/// <summary>
		/// Open *.sql/*.txt file in new query window
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OpenQueryFile_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				Filter = "SQL Files|*.sql|Text Files|*.txt|All files|*.*",
				Multiselect = false
			};
			bool? showDialog = dialog.ShowDialog(this);

			if (!showDialog.Value) return;

			string text = File.ReadAllText(dialog.FileName);

			var panel = new QueryPanel {Text = text, FilePath = dialog.FileName};
			panel.QueryResultReady += PanelOnQueryResultReady;
			panel.BuildNewQueryPanel(this, Path.GetFileNameWithoutExtension(dialog.FileName));

			MainDocumentPane.Children.Add(panel.Anchorable);
			int indexOf = MainDocumentPane.Children.IndexOf(panel.Anchorable);
			MainDocumentPane.SelectedContentIndex = indexOf;
		}

		private async void ShowAbout_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			await this.ShowMessageAsync("About", "Created by Radek Svoboda (rasvob14@gmail.com)");
		}

		/// <summary>
		/// Show MS SQL Server connection options
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowMsSqlConnectionPanel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var flyoutMsSql = Flyouts.Items[0] as Flyout;

			if(flyoutMsSql != null)
			{
				flyoutMsSql.IsOpen = !flyoutMsSql.IsOpen;
			}
		}

		/// <summary>
		/// Show Oracle connection options
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShowOracleConnectionPanel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var flyoutMsSql = Flyouts.Items[2] as Flyout;

			if(flyoutMsSql != null)
			{
				flyoutMsSql.IsOpen = !flyoutMsSql.IsOpen;
			}
		}

		/// <summary>
		/// Menu item - About
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Exit_OnClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Export current diagram to PNG
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExportToPng_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseModelDesigner designer;
			if (TryGetSelectedDesigner(out designer))
			{
				var dialog = new SaveFileDialog
				{
					OverwritePrompt = true,
					AddExtension = true,
					DefaultExt = "png",
					Filter = "Image Files|*.png"
				};

				bool? showDialog = dialog.ShowDialog(this);

				if (showDialog.Value)
				{
					designer.ExportToPng(dialog.FileName);
				}
			}
		}

		/// <summary>
		/// Export current diagram to PNG - check if it's possible
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ExportToPng_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			DatabaseModelDesigner designer;
			e.CanExecute = TryGetSelectedDesigner(out designer);
		}

		/// <summary>
		/// Export current diagram to PNG - Full size of canvas
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void ExportToPngFull_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseModelDesigner designer;
			if(TryGetSelectedDesigner(out designer))
			{
				var dialog = new SaveFileDialog
				{
					OverwritePrompt = true,
					AddExtension = true,
					DefaultExt = "png",
					Filter = "Image Files|*.png"
				};

				bool? showDialog = dialog.ShowDialog(this);

				if(showDialog.Value)
				{
					designer.ExportToPngFullSize(dialog.FileName);
				}
			}
		}
	}
}
