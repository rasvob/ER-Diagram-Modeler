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
using ER_Diagram_Modeler.Controls.Buttons;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.Models.Database;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for DatabaseConnectionPanel.xaml
	/// </summary>
	public partial class DatabaseConnectionPanel : UserControl
	{
		public event EventHandler<ConnectionType> ConnectionClick;
		public List<MsSqlDatabaseInfo> MsSqlDatabaseInfos { get; set; }

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
				MsSqlDatabaseInfos = mapper.ListDatabases().ToList();
			}

			if (MsSqlDatabaseInfos.Any())
			{
				MsSqlDatabaseComboBox.ItemsSource = MsSqlDatabaseInfos;
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
			var cb = (sender as ComboBox)?.SelectedItem as MsSqlDatabaseInfo;

			if (cb == null)
			{
				return;
			}

			SessionProvider.Instance.Database = cb.Name;

			LoadMsSqlTreeViewData();
		}

		private void LoadMsSqlTreeViewData()
		{
			//TODO: Load diagrams
			MsSqlTreeView.Items.Clear();
			string origdb = SessionProvider.Instance.Database;
			foreach (MsSqlDatabaseInfo databaseInfo in MsSqlDatabaseInfos)
			{
				SessionProvider.Instance.Database = databaseInfo.Name;
				using(MsSqlMapper mapper = new MsSqlMapper())
				{
					TreeViewItem root = new TreeViewItem();
					root.Header = databaseInfo.Name;

					databaseInfo.Tables.Clear();

					TreeViewItem tables = new TreeViewItem();
					tables.Header = "Tables";

					foreach(TableModel model in mapper.ListTables())
					{
						databaseInfo.Tables.Add(model);

						tables.Items.Add(new TreeViewItem()
						{
							Header = model.Title
						});
					}

					root.Items.Add(tables);
					MsSqlTreeView.Items.Add(root);
				}
			}

			SessionProvider.Instance.Database = origdb;
		}
	}
}
