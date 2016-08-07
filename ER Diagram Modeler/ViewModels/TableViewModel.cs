using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ViewModels
{
	public class TableViewModel: INotifyPropertyChanged
	{
		private TableModel _model;
		private double _left;
		private double _top;

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

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}