using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.ViewModels
{
	public class TableContentViewModel: INotifyPropertyChanged
	{
		public static readonly int ZIndexSelectedValue = 1000;
		public static readonly int ZIndexUnSelectedValue = 0;

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected) return;
				ZIndex = value ? ZIndexSelectedValue : ZIndexUnSelectedValue;
				_isSelected = value;
				OnPropertyChanged();
			}
		}

		private int _zIndex;
		private bool _isSelected;

		public int ZIndex
		{
			get { return _zIndex; }
			set
			{
				if (value == _zIndex) return;
				_zIndex = value;
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