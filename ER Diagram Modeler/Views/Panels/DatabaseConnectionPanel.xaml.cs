﻿using System;
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
		public event EventHandler<TableModel> AddTable;
		public event EventHandler CreateMsSqlDatabase;
		public event EventHandler<string> DropMsSqlDatabase;
		public List<DatabaseInfo> DatabaseInfos { get; set; }

		public DatabaseConnectionPanel()
		{
			InitializeComponent();
		}

		private void Disconnect_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SessionProvider.Instance.ConnectionType = ConnectionType.None;
			HideDatabaseStackPanels();
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

		private void ConnectToSqlServerMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnConnectionClick(ConnectionType.SqlServer);
		}

		private void ConnectToOracleMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			OnConnectionClick(ConnectionType.Oracle);
		}

		protected virtual void OnConnectionClick(ConnectionType e)
		{
			ConnectionClick?.Invoke(this, e);
		}

		public void LoadMsSqlData()
		{
			using (MsSqlMapper mapper = new MsSqlMapper())
			{
				DatabaseInfos = mapper.ListDatabases().ToList();
			}

			if (DatabaseInfos.Any())
			{
				MsSqlDatabaseComboBox.ItemsSource = DatabaseInfos;
				MsSqlDatabaseComboBox.DisplayMemberPath = "Name";
				MsSqlDatabaseComboBox.SelectedIndex = 0;
			}

			MsSqlServerGrid.Visibility = Visibility.Visible;
		}

		public void LoadOracleData()
		{
			using(IOracleMapper mapper = new OracleMapper())
			{
				DatabaseInfos = new List<DatabaseInfo>();
				DatabaseInfo info = new DatabaseInfo()
				{
					Name = "Tables"
				};

				IEnumerable<TableModel> tables = mapper.ListTables();

				foreach (TableModel model in tables)
				{
					info.Tables.Add(model);
				}

				DatabaseInfos.Add(info);
			}
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
			throw new NotImplementedException();
		}

		private void MsSqlDatabaseComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var cb = (sender as ComboBox)?.SelectedItem as DatabaseInfo;

			if (cb == null)
			{
				return;
			}

			SessionProvider.Instance.Database = cb.Name;

			LoadMsSqlTreeViewData();
		}

		private void LoadMsSqlTreeViewData()
		{
			TreeViewBuilder builder = new MsSqlTreeViewBuilder(OnAddTable, DropDatabaseAction, DatabaseInfos);
			List<TreeViewItem> item = builder.BuildTreeView();
			MsSqlTreeView.Items.Clear();
			item.ForEach(t => MsSqlTreeView.Items.Add(t));
		}

		private void LoadOracleTreeData()
		{
			TreeViewBuilder builder = new OracleTreeViewBuilder(OnAddTable, DatabaseInfos);
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
	}
}
