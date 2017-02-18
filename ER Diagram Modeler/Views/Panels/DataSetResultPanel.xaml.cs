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
		public string Title { get; set; } = "Query result";
		public LayoutAnchorable Anchorable { get; set; }
		public DataSetResultViewModel ViewModel { get; set; } = new DataSetResultViewModel();

		public DataSetResultPanel()
		{
			InitializeComponent();
			DataContext = ViewModel;
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
			ViewModel.Views.Clear();
			foreach (DataTable table in data.Tables)
			{
				ViewModel.Views.Add(table.DefaultView);
			}
		}

		private void DatasetScrollViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			ScrollViewer scv = (ScrollViewer)sender;
			scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
			e.Handled = true;
		}
	}

	public class DataSetResultViewModel: INotifyPropertyChanged
	{
		public DataSetResultViewModel()
		{
			Views = new ObservableCollection<DataView>();
		}

		private ObservableCollection<DataView> _views;

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

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
