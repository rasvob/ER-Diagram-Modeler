using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	/// <summary>
	/// Main Window viewmodel
	/// </summary>
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private List<DatabaseModelDesignerViewModel> _databaseModelDesignerViewModels;
		private TableRowModel _flyoutRowModel;

		/// <summary>
		/// Column model for edit
		/// </summary>
		public TableRowModel FlyoutRowModel
		{
			get { return _flyoutRowModel; }
			set
			{
				if (Equals(value, _flyoutRowModel)) return;
				_flyoutRowModel = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Opened designers viewmodels
		/// </summary>
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
			FlyoutRowModel = new TableRowModel();
		}

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Change of mouse mode
		/// </summary>
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