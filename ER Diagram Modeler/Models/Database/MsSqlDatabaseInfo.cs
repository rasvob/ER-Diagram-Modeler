using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.Models.Database
{
	public class MsSqlDatabaseInfo: INotifyPropertyChanged
	{
		private string _name;
		private int _id;
		private ObservableCollection<TableModel> _tables;
		private ObservableCollection<DiagramModel> _diagrams;

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == _name) return;
				_name = value;
				OnPropertyChanged();
			}
		}

		public int Id
		{
			get { return _id; }
			set
			{
				if (value == _id) return;
				_id = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<TableModel> Tables
		{
			get { return _tables; }
			set
			{
				if (Equals(value, _tables)) return;
				_tables = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<DiagramModel> Diagrams
		{
			get { return _diagrams; }
			set
			{
				if (Equals(value, _diagrams)) return;
				_diagrams = value;
				OnPropertyChanged();
			}
		}

		public MsSqlDatabaseInfo()
		{
			Diagrams = new ObservableCollection<DiagramModel>();
			Tables = new ObservableCollection<TableModel>();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}