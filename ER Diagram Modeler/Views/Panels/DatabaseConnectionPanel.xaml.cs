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
		public event EventHandler<ConnectionType> ConnectionClick;
		public event EventHandler DisconnectClick;
		public event EventHandler<string> MsSqlDatabaseChanged; 
		public event EventHandler<TableModel> AddTable;
		public event EventHandler<DiagramModel> AddDiagram;
		public event EventHandler<DiagramModel> DropDiagram;
		public event EventHandler CreateMsSqlDatabase;
		public event EventHandler<string> DropMsSqlDatabase;
		public List<DatabaseInfo> DatabaseInfos { get; set; }

		public DatabaseConnectionPanel()
		{
			InitializeComponent();
		}

		private void Disconnect_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			OnDisconnectClick();
		}

		private void Disconnect_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType != ConnectionType.None;
		}

		private void ConnectoToServerButton_OnClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as DesignerToolBarButton;

			if (btn != null) btn.ContextMenu.IsOpen = true;
		}

		protected virtual void OnConnectionClick(ConnectionType e)
		{
			ConnectionClick?.Invoke(this, e);
		}

		public void LoadMsSqlData(bool loadPrev = false)
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

			using (MsSqlMapper mapper = new MsSqlMapper())
			{
				DatabaseInfos = mapper.ListDatabases().ToList();
			}

			if (loadPrev)
			{
				int indexOf = DatabaseInfos.IndexOf(DatabaseInfos.FirstOrDefault(t => t.Name.Equals(name)));
				selected = indexOf > 0 ? indexOf : 0;
			}

			if(DatabaseInfos.Any())
			{
				MsSqlDatabaseComboBox.ItemsSource = DatabaseInfos;
				MsSqlDatabaseComboBox.DisplayMemberPath = "Name";
				MsSqlDatabaseComboBox.SelectedIndex = selected;
			}

			LoadMsSqlTreeViewData();

			MsSqlServerGrid.Visibility = Visibility.Visible;
		}

		public void LoadOracleData()
		{
			var ctx = new DatabaseContext(ConnectionType.Oracle);
			
			DatabaseInfos = new List<DatabaseInfo>();
			DatabaseInfo info = new DatabaseInfo()
			{
				Name = "Tables"
			};

			IEnumerable<TableModel> tables = ctx.ListTables();

			foreach (TableModel model in tables)
			{
				info.Tables.Add(model);
			}

			IEnumerable<DiagramModel> diagrams = ctx.SelectDiagrams();

			foreach (DiagramModel diagram in diagrams)
			{
				info.Diagrams.Add(diagram);
			}

			DatabaseInfos.Add(info);
			
			LoadOracleTreeData();
			OracleStackPanel.Visibility = Visibility.Visible;
		}

		public void HideDatabaseStackPanels()
		{
			MsSqlServerGrid.Visibility = Visibility.Collapsed;
			OracleStackPanel.Visibility = Visibility.Collapsed;
		}

		public void RefreshTreeData()
		{
			switch (SessionProvider.Instance.ConnectionType)
			{
				case ConnectionType.None:
					break;
				case ConnectionType.SqlServer:
					using(MsSqlMapper mapper = new MsSqlMapper())
					{
						DatabaseInfos = mapper.ListDatabases().ToList();
					}
					LoadMsSqlTreeViewData();
					ExpandMsSqlTreeItem();
					break;
				case ConnectionType.Oracle:
					LoadOracleData();
					TreeViewItem item = OracleTreeView.Items.Cast<TreeViewItem>().FirstOrDefault();
					if (item != null)
						item.IsExpanded = true;
					break;
			}
		}

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
				TreeViewItem firstOrDefault = itemMsSql.Items.Cast<TreeViewItem>().FirstOrDefault();
				if(firstOrDefault != null)
					firstOrDefault.IsExpanded = true;
			}
		}

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

		public void LoadMsSqlTreeViewData()
		{
			TreeViewBuilder builder = new MsSqlTreeViewBuilder(OnAddTable, DropDatabaseAction, DatabaseInfos, OnAddDiagram, OnDropDiagram);
			List<TreeViewItem> item = builder.BuildTreeView();
			MsSqlTreeView.Items.Clear();
			item.ForEach(t => MsSqlTreeView.Items.Add(t));
		}

		public void LoadOracleTreeData()
		{
			TreeViewBuilder builder = new OracleTreeViewBuilder(OnAddTable, DatabaseInfos, OnAddDiagram, OnDropDiagram);
			List<TreeViewItem> item = builder.BuildTreeView();
			OracleTreeView.Items.Clear();
			item.ForEach(t => OracleTreeView.Items.Add(t));
		}

		private void DropDatabaseAction(string dbName)
		{
			OnDropMsSqlDatabase(dbName);
		}

		protected virtual void OnAddTable(TableModel e)
		{
			AddTable?.Invoke(this, e);
		}

		private void CreateNewMsSqlDatabase_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			OnCreateMsSqlDatabase();
		}

		private void CreateNewMsSqlDatabase_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType == ConnectionType.SqlServer;
		}

		protected virtual void OnCreateMsSqlDatabase()
		{
			CreateMsSqlDatabase?.Invoke(this, System.EventArgs.Empty);
		}

		protected virtual void OnDropMsSqlDatabase(string e)
		{
			DropMsSqlDatabase?.Invoke(this, e);
		}

		protected virtual void OnAddDiagram(DiagramModel e)
		{
			AddDiagram?.Invoke(this, e);
		}

		protected virtual void OnDropDiagram(DiagramModel e)
		{
			DropDiagram?.Invoke(this, e);
		}

		protected virtual void OnDisconnectClick()
		{
			DisconnectClick?.Invoke(this, System.EventArgs.Empty);
		}

		protected virtual void OnMsSqlDatabaseChanged(string e)
		{
			MsSqlDatabaseChanged?.Invoke(this, e);
		}

		private void ConnectToSqlServerMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnConnectionClick(ConnectionType.SqlServer);
		}

		private void ConnectToOracleMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnConnectionClick(ConnectionType.Oracle);
		}
	}
}
