using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelDesignerViewModel: INotifyPropertyChanged
	{
		private ObservableCollection<TableViewModel> _tableViewModels;

		public ObservableCollection<TableViewModel> TableViewModels
		{
			get { return _tableViewModels; }
			set
			{
				if (Equals(value, _tableViewModels)) return;
				_tableViewModels = value;
				OnPropertyChanged();
			}
		}

		public DatabaseModelDesignerViewModel()
		{
			TableViewModels = new ObservableCollection<TableViewModel>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}