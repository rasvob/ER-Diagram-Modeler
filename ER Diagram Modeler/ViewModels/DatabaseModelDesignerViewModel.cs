using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Properties;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelDesignerViewModel: INotifyPropertyChanged
	{
		private ObservableCollection<TableViewModel> _tableViewModels;
		private double _scale = 1;
		private MouseMode _mouseMode = MouseMode.Select;

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

		public double Scale
		{
			get { return _scale; }
			set
			{
				if (value.Equals(_scale)) return;
				_scale = value;
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