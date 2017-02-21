using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using ER_Diagram_Modeler.Annotations;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for DataSetResultPanel.xaml
	/// </summary>
	public partial class DataSetResultPanel : UserControl
	{
		/// <summary>
		/// Panel title
		/// </summary>
		public string Title { get; set; } = "Query result";

		/// <summary>
		/// Dockable panel
		/// </summary>
		public LayoutAnchorable Anchorable { get; set; }

		/// <summary>
		/// Control viewmodel
		/// </summary>
		public DataSetResultViewModel ViewModel { get; set; } = new DataSetResultViewModel();

		public DataSetResultPanel()
		{
			InitializeComponent();
			DataContext = ViewModel;
		}

		/// <summary>
		/// Build dockable panel
		/// </summary>
		/// <param name="window">App window</param>
		/// <param name="title">Title of panel</param>
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

		/// <summary>
		/// Refresh data in grid
		/// </summary>
		/// <param name="data"></param>
		public void RefreshData(DataSet data)
		{
			ViewModel.Views.Clear();
			foreach (DataTable table in data.Tables)
			{
				ViewModel.Views.Add(table.DefaultView);
			}
		}


		/// <summary>
		/// Scroll hook
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DatasetScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
			e.Handled = true;
		}
	}

	/// <summary>
	/// Viewmodel of  DataSetResultPanel
	/// </summary>
	public class DataSetResultViewModel: INotifyPropertyChanged
	{
		public DataSetResultViewModel()
		{
			Views = new ObservableCollection<DataView>();
		}

		private ObservableCollection<DataView> _views;

		/// <summary>
		/// Dataset DefaultView results
		/// </summary>
		public ObservableCollection<DataView> Views
		{
			get { return _views; }
			set
			{
				if (Equals(value, _views)) return;
				_views = value;
				OnPropertyChanged();
			}
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
