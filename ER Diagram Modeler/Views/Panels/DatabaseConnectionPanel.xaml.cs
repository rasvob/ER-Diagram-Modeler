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
using ER_Diagram_Modeler.ViewModels.Enums;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for DatabaseConnectionPanel.xaml
	/// </summary>
	public partial class DatabaseConnectionPanel : UserControl
	{
		public DatabaseConnectionPanel()
		{
			InitializeComponent();
		}

		private void Disconnect_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SessionProvider.Instance.ConnectionType = ConnectionType.None;
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
			
		}

		private void ConnectToOracleMenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			//TODO: Open dialog
		}
	}
}
