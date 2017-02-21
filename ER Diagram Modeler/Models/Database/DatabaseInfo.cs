using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Models.Designer;

namespace ER_Diagram_Modeler.Models.Database
{
	/// <summary>
	/// Database tables and diagrams
	/// </summary>
	public class DatabaseInfo: INotifyPropertyChanged
	{
		private string _name;
		private int _id;
		private ObservableCollection<TableModel> _tables;
		private ObservableCollection<DiagramModel> _diagrams;

		/// <summary>
		/// DB name
		/// </summary>
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

		/// <summary>
		/// DB ObjectId
		/// </summary>
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

		/// <summary>
		/// Tables in DB
		/// </summary>
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

		/// <summary>
		/// Diagrams in DB
		/// </summary>
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

		public DatabaseInfo()
		{
			Diagrams = new ObservableCollection<DiagramModel>();
			Tables = new ObservableCollection<TableModel>();
		}

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}