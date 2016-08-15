using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Properties;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelDesignerViewModel: INotifyPropertyChanged
	{
		private ObservableCollection<TableViewModel> _tableViewModels;
		private List<Datatype> _datatypes;
		private ConnectionType _connectionType;

		public ConnectionType ConnectionType
		{
			get { return _connectionType; }
			set
			{
				if (value == _connectionType) return;
				_connectionType = value;
				OnPropertyChanged();
			}
		}

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

		public void LoadDatatypes()
		{
			Datatypes = Datatype.LoadDatatypesFromResource(ConnectionType);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}