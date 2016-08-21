using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private DatabaseModelDesignerViewModel _databaseModelDesignerViewModel;
		
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
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<MouseMode> MouseModeChanged;  

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnMouseModeChanged(MouseMode e)
		{
			MouseModeChanged?.Invoke(this, e);
		}
	}
}