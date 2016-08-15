using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private DatabaseModelDesignerViewModel _databaseModelDesignerViewModel;
		private MouseMode _mouseMode;
		private ConnectionType _connectionType = ConnectionType.None;

		public MouseMode MouseMode
		{
			get { return _mouseMode; }
			set
			{
				if (value == _mouseMode) return;
				_mouseMode = value;
				OnPropertyChanged();
			}
		}

		public ConnectionType ConnectionType
		{
			get { return _connectionType; }
			set
			{
				if (value == _connectionType) return;
				_connectionType = value;
				DatabaseModelDesignerViewModel.ConnectionType = value;
				OnPropertyChanged();
			}
		}

		public DatabaseModelDesignerViewModel DatabaseModelDesignerViewModel
		{
			get { return _databaseModelDesignerViewModel; }
			set
			{
				if (Equals(value, _databaseModelDesignerViewModel)) return;
				_databaseModelDesignerViewModel = value;
				OnPropertyChanged();
			}
		}

		public MainWindowViewModel()
		{
			DatabaseModelDesignerViewModel = new DatabaseModelDesignerViewModel();
			MouseMode = MouseMode.Select;
			//For Debug
			ConnectionType = ConnectionType.SqlServer;
			//DatabaseModelDesignerViewModel.LoadDatatypes();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}