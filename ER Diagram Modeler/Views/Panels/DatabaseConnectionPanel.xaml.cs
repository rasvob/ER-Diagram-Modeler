using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
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
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.ConnectionPanelLoaders;
using ER_Diagram_Modeler.Controls.Buttons;
using ER_Diagram_Modeler.DatabaseConnection.Oracle;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using MahApps.Metro.Controls.Dialogs;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for DatabaseConnectionPanel.xaml
	/// </summary>
	public partial class DatabaseConnectionPanel : UserControl
	{
		/// <summary>
		/// Connection item clicked
		/// </summary>
		public event EventHandler<ConnectionType> ConnectionClick;

		/// <summary>
		/// Disconnect item clicked
		/// </summary>
		public event EventHandler DisconnectClick;

		/// <summary>
		/// Database selected
		/// </summary>
		public event EventHandler<string> MsSqlDatabaseChanged; 

		/// <summary>
		/// Add table to diagram clicked
		/// </summary>
		public event EventHandler<TableModel> AddTable;

		/// <summary>
		/// Add diagram to window
		/// </summary>
		public event EventHandler<DiagramModel> AddDiagram;

		/// <summary>
		/// Delete diagram from DB
		/// </summary>
		public event EventHandler<DiagramModel> DropDiagram;

		/// <summary>
		/// Create new database
		/// </summary>
		/// <remarks>MS Sql server only</remarks>
		public event EventHandler CreateMsSqlDatabase;

		/// <summary>
		/// Drop database
		/// </summary>
		/// <remarks>MS Sql Server only</remarks>
		public event EventHandler<string> DropMsSqlDatabase;

		/// <summary>
		/// Treeview data
		/// </summary>
		public List<DatabaseInfo> DatabaseInfos { get; set; }

		public DatabaseConnectionPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// End session
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Disconnect_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			OnDisconnectClick();
		}

		/// <summary>
		/// Can disconnect
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Disconnect_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType != ConnectionType.None;
		}

		/// <summary>
		/// Connect to server open menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectoToServerButton_OnClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as DesignerToolBarButton;

			if (btn != null) btn.ContextMenu.IsOpen = true;
		}

		/// <summary>
		/// Clicked on menu item
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnConnectionClick(ConnectionType e)
		{
			ConnectionClick?.Invoke(this, e);
		}

		/// <summary>
		/// Load Ms Sql tree data
		/// </summary>
		/// <param name="loadPrev">Select previous DB</param>
		public async Task LoadMsSqlData(bool loadPrev = false)
		{
			int selected = 0;
			string name = string.Empty;
			
			if (loadPrev)
			{
				DatabaseInfo info = DatabaseInfos.FirstOrDefault(t => t.Name.Equals(SessionProvider.Instance.Database));
				if (info != null)
				{
					name = info.Name;
				}
			}

			await Task.Run(() =>
			{
				var pomInfos = new List<DatabaseInfo>();
				DatabaseInfos = new List<DatabaseInfo>();
				using(MsSqlMapper mapper = new MsSqlMapper())
				{
					pomInfos = mapper.ListDatabases().ToList();
				}

				foreach(DatabaseInfo info in pomInfos)
				{
					try
					{
						using(MsSqlMapper mapper = new MsSqlMapper(SessionProvider.Instance.GetConnectionStringForMsSqlDatabase(info.Name)))
						{
							try
							{
								mapper.ListTables().ToList().ForEach(t => info.Tables.Add(t));
							}
							catch(SqlException)
							{
								Debug.WriteLine("Can't read table");
								continue;
							}

							try
							{
								mapper.SelectDiagrams().ToList().ForEach(t => info.Diagrams.Add(t));
							}
							catch(SqlException)
							{
								Debug.WriteLine("Can't read diagrams");
							}

							DatabaseInfos.Add(info);
						}
					}
					catch (SqlException e)
					{
						Debug.WriteLine(e.Message);
					}
					
				}

			});

			if(loadPrev)
			{
				int indexOf = DatabaseInfos.IndexOf(DatabaseInfos.FirstOrDefault(t => t.Name.Equals(name)));
				selected = indexOf > 0 ? indexOf : 0;
			}

			if(DatabaseInfos.Any())
			{
				if(SessionProvider.Instance.Database.Equals(string.Empty))
				{
					SessionProvider.Instance.Database = DatabaseInfos[selected].Name;
				}
				MsSqlDatabaseComboBox.ItemsSource = DatabaseInfos;
				MsSqlDatabaseComboBox.DisplayMemberPath = "Name";
				MsSqlDatabaseComboBox.SelectedIndex = selected;
			}

			LoadMsSqlTreeViewData();

			MsSqlServerGrid.Visibility = Visibility.Visible;
		}

		/// <summary>
		/// Load Oracle tree data
		/// </summary>
		public async Task LoadOracleData()
		{
			await Task.Run(() =>
			{
				var ctx = new DatabaseContext(ConnectionType.Oracle);
				DatabaseInfos = new List<DatabaseInfo>();
				DatabaseInfo info = new DatabaseInfo()
				{
					Name = "Tables"
				};

				try
				{
					IEnumerable<TableModel> tables = ctx.ListTables();

					foreach(TableModel model in tables)
					{
						info.Tables.Add(model);
					}
				}
				catch (SqlException e)
				{
					Debug.WriteLine(e.Message);
				}

				try
				{
					IEnumerable<DiagramModel> diagrams = ctx.SelectDiagrams();

					foreach(DiagramModel diagram in diagrams)
					{
						info.Diagrams.Add(diagram);
					}
				}
				catch(SqlException e)
				{
					Debug.WriteLine(e.Message);
				}

				DatabaseInfos.Add(info);
			});

			LoadOracleTreeData();
			OracleStackPanel.Visibility = Visibility.Visible;
		}

		/// <summary>
		/// Hide controls
		/// </summary>
		public void HideDatabaseStackPanels()
		{
			MsSqlServerGrid.Visibility = Visibility.Collapsed;
			OracleStackPanel.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		/// Refresh views
		/// </summary>
		public async Task RefreshTreeData()
		{
			switch (SessionProvider.Instance.ConnectionType)
			{
				case ConnectionType.None:
					break;
				case ConnectionType.SqlServer:
					await LoadMsSqlData(true);
					ExpandMsSqlTreeItem();
					break;
				case ConnectionType.Oracle:
					await LoadOracleData();
					ExpandOracleTreeItem();
					break;
			}
		}

		/// <summary>
		/// Expand treeview item
		/// </summary>
		private void ExpandMsSqlTreeItem()
		{
			TreeViewItem itemMsSql = MsSqlTreeView.Items.Cast<TreeViewItem>().FirstOrDefault(t =>
			{
				string s = t.Header as string;
				return s != null && s.Equals(SessionProvider.Instance.Database);
			});

			if(itemMsSql != null)
			{
				itemMsSql.IsExpanded = true;
				IEnumerable<TreeViewItem> items = itemMsSql.Items.Cast<TreeViewItem>();
				IEnumerable<TreeViewItem> viewItems = items as IList<TreeViewItem> ?? items.ToList();
				TreeViewItem firstOrDefault = viewItems.FirstOrDefault();
				if(firstOrDefault != null)
					firstOrDefault.IsExpanded = true;

				TreeViewItem diagrams = viewItems.Skip(1).FirstOrDefault();

				if (diagrams != null)
					diagrams.IsExpanded = true;
			}
		}

		/// <summary>
		/// Expand treeview item
		/// </summary>
		private void ExpandOracleTreeItem()
		{
			IEnumerable<TreeViewItem> items = OracleTreeView.Items.Cast<TreeViewItem>();
			IEnumerable<TreeViewItem> treeViewItems = items as IList<TreeViewItem> ?? items.ToList();
			TreeViewItem item = treeViewItems.FirstOrDefault();
			if (item != null)
			{
				item.IsExpanded = true;
				TreeViewItem viewItem = treeViewItems.Skip(1).FirstOrDefault();

				if (viewItem != null) viewItem.IsExpanded = true;
			}
		}

		/// <summary>
		/// DB selected from combobox
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MsSqlDatabaseComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var cb = (sender as ComboBox)?.SelectedItem as DatabaseInfo;

			if (cb == null)
			{
				return;
			}

			if (SessionProvider.Instance.Database.Equals(cb.Name))
			{
				return;
			}

			OnMsSqlDatabaseChanged(cb.Name);
		}

		/// <summary>
		/// Load treeview data
		/// </summary>
		/// <remarks>MS Sql Server</remarks>
		public void LoadMsSqlTreeViewData()
		{
			TreeViewBuilder builder = new MsSqlTreeViewBuilder(OnAddTable, DropDatabaseAction, DatabaseInfos, OnAddDiagram, OnDropDiagram);
			List<TreeViewItem> item = builder.BuildTreeView();
			MsSqlTreeView.Items.Clear();
			item.ForEach(t => MsSqlTreeView.Items.Add(t));
		}

		/// <summary>
		/// Load treeview data
		/// </summary>
		/// <remarks>Oracle</remarks>
		public void LoadOracleTreeData()
		{
			TreeViewBuilder builder = new OracleTreeViewBuilder(OnAddTable, DatabaseInfos, OnAddDiagram, OnDropDiagram);
			List<TreeViewItem> item = builder.BuildTreeView();
			OracleTreeView.Items.Clear();
			item.ForEach(t => OracleTreeView.Items.Add(t));
		}

		/// <summary>
		/// Invoke Drop DB event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DropDatabaseAction(string dbName)
		{
			OnDropMsSqlDatabase(dbName);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnAddTable(TableModel e)
		{
			AddTable?.Invoke(this, e);
		}

		/// <summary>
		/// Invoke Create DB event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateNewMsSqlDatabase_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			OnCreateMsSqlDatabase();
		}

		/// <summary>
		/// Can create new DB
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateNewMsSqlDatabase_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType == ConnectionType.SqlServer;
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnCreateMsSqlDatabase()
		{
			CreateMsSqlDatabase?.Invoke(this, System.EventArgs.Empty);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnDropMsSqlDatabase(string e)
		{
			DropMsSqlDatabase?.Invoke(this, e);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnAddDiagram(DiagramModel e)
		{
			AddDiagram?.Invoke(this, e);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnDropDiagram(DiagramModel e)
		{
			DropDiagram?.Invoke(this, e);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnDisconnectClick()
		{
			DisconnectClick?.Invoke(this, System.EventArgs.Empty);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		protected virtual void OnMsSqlDatabaseChanged(string e)
		{
			MsSqlDatabaseChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		private void ConnectToSqlServerMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnConnectionClick(ConnectionType.SqlServer);
		}

		/// <summary>
		/// Event invoker
		/// </summary>
		private void ConnectToOracleMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnConnectionClick(ConnectionType.Oracle);
		}
	}
}
