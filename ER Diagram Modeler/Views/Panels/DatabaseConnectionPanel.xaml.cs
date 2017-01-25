using System;
using System.Collections.Generic;
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
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
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
			//TODO: Oracle data load
		}

		public void HideDatabaseStackPanels()
		{
			MsSqlServerGrid.Visibility = Visibility.Collapsed;
			OracleStackPanel.Visibility = Visibility.Collapsed;
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
			
			TreeViewBuilder builder = new MsSqlTreeViewBuilder(OnAddTable, DatabaseInfos);
			List<TreeViewItem> item = builder.BuildTreeView();
			MsSqlTreeView.Items.Clear();
			item.ForEach(t => MsSqlTreeView.Items.Add(t));
		}

		protected virtual void OnAddTable(TableModel e)
		{
			AddTable?.Invoke(this, e);
		}
	}
}
