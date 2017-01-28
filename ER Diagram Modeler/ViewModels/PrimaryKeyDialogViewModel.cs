using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.ViewModels
{
	public class PrimaryKeyDialogViewModel: INotifyPropertyChanged
	{
		public string TableName
		{
			get { return _tableName; }
			set
			{
				if (value == _tableName) return;
				_tableName = value;
				OnPropertyChanged();
			}
		}

		private ObservableCollection<TableRowModel> _rowModels = new ObservableCollection<TableRowModel>();
		private string _tableName;

		public ObservableCollection<TableRowModel> RowModels
		{
			get { return _rowModels; }
			set
			{
				if (Equals(value, _rowModels)) return;
				_rowModels = value;
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