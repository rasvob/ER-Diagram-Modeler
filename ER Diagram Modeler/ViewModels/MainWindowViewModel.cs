using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.ViewModels
{
	public class MainWindowViewModel: INotifyPropertyChanged
	{
		private DatabaseModelDiagramViewModel _databaseModelCanvasViewModel;

		public DatabaseModelDiagramViewModel DatabaseModelCanvasViewModel
		{
			get { return _databaseModelCanvasViewModel; }
			set
			{
				if (Equals(value, _databaseModelCanvasViewModel)) return;
				_databaseModelCanvasViewModel = value;
				OnPropertyChanged();
			}
		}

		public MainWindowViewModel()
		{
			_databaseModelCanvasViewModel = new DatabaseModelDiagramViewModel();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}