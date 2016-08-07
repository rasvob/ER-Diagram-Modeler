using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelCanvasViewModel: INotifyPropertyChanged
	{
		private List<TableViewModel> _tables;

		public List<TableViewModel> Tables
		{
			get { return _tables; }
			set
			{
				if (Equals(value, _tables)) return;
				_tables = value;
				OnPropertyChanged();
			}
		}

		public DatabaseModelCanvasViewModel()
		{
			_tables = new List<TableViewModel>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}