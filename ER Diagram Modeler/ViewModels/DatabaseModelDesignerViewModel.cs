using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelDesignerViewModel: INotifyPropertyChanged
	{
		private ObservableCollection<TableViewModel> _tableViewModels;
		private List<Datatype> _datatypes;

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

		public List<Datatype> Datatypes
		{
			get { return _datatypes; }
			set
			{
				if(Equals(value, _datatypes)) return;
				_datatypes = value;
				OnPropertyChanged();
			}
		}

		public DatabaseModelDesignerViewModel()
		{
			TableViewModels = new ObservableCollection<TableViewModel>();
			Datatypes = new List<Datatype>();
		}

		private void LoadDatatypesFromResource(string res)
		{
			
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}