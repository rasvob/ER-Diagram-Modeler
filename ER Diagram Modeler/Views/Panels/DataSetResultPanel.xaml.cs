using System;
using System.Collections.Generic;
using System.Data;
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
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for DataSetResultPanel.xaml
	/// </summary>
	public partial class DataSetResultPanel : UserControl
	{
		public string Title { get; set; } = "Query result";
		public LayoutAnchorable Anchorable { get; set; }

		public DataSetResultPanel()
		{
			InitializeComponent();
		}

		public void BuildNewQueryPanel(MainWindow window, string title)
		{
			Anchorable = new LayoutAnchorable()
			{
				CanClose = true,
				CanHide = true,
				CanFloat = true,
				CanAutoHide = true,
				Title = title
			};

			Title = title;
			Anchorable.Content = this;
		}

		public void RefreshData(DataSet data)
		{
			GridStackPanel.Children.Clear();
			foreach (DataTable table in data.Tables)
			{
				DataGrid grid = CreateDataGrid(table.DefaultView);
				grid.IsReadOnly = true;
				GridStackPanel.Children.Add(grid);
			}
		}

		private DataGrid CreateDataGrid(DataView view)
		{
			return new DataGrid
			{
				CanUserAddRows = false,
				CanUserDeleteRows = false,
				CanUserReorderColumns = true,
				CanUserResizeColumns = true,
				CanUserResizeRows = true,
				CanUserSortColumns = true,
				ItemsSource = view,
			};
		}

		private void DatasetScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
			e.Handled = true;
		}
	}
}
