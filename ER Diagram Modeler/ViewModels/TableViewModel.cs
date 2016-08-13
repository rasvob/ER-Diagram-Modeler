using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class TableViewModel: INotifyPropertyChanged
	{
		private TableModel _model;
		private TableViewMode _viewMode;
		private double _left;
		private double _top;
		private bool _isSelected;

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected) return;
				_isSelected = value;
				OnPropertyChanged();
			}
		}

		public TableViewMode ViewMode
		{
			get { return _viewMode; }
			set
			{
				if (value == _viewMode) return;
				_viewMode = value;
				OnPropertyChanged();
			}
		}

		public TableModel Model
		{
			get { return _model; }
			set
			{
				if (Equals(value, _model)) return;
				_model = value;
				OnPropertyChanged();
			}
		}

		public double Left
		{
			get { return _left; }
			set
			{
				if (value.Equals(_left)) return;
				_left = value;
				OnPropertyChanged();
			}
		}

		public double Top
		{
			get { return _top; }
			set
			{
				if (value.Equals(_top)) return;
				_top = value;
				OnPropertyChanged();
			}
		}

		public TableViewModel()
		{
			ViewMode = TableViewMode.Standard;
		}

		public TableViewModel(TableModel model):this()
		{
			_model = model;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}