using System;
using System.Collections.Generic;
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
		private List<DatabaseModelDesignerViewModel> _databaseModelDesignerViewModels;

		public List<DatabaseModelDesignerViewModel> DatabaseModelDesignerViewModels
		{
			get { return _databaseModelDesignerViewModels; }
			set
			{
				if (Equals(value, _databaseModelDesignerViewModels)) return;
				_databaseModelDesignerViewModels = value;
				OnPropertyChanged();
			}
		}

		public MainWindowViewModel()
		{
			DatabaseModelDesignerViewModels = new List<DatabaseModelDesignerViewModel>();
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