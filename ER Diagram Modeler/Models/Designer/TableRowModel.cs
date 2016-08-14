using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableRowModel: INotifyPropertyChanged
	{
		private string _name;
		private Datatype _datatype = new Datatype();
		private bool _allowNull = true;
		private bool _primaryKey;

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

		public Datatype Datatype
		{
			get { return _datatype; }
			set
			{
				if (Equals(value, _datatype)) return;
				_datatype = value;
				OnPropertyChanged();
			}
		}

		public bool AllowNull
		{
			get { return _allowNull; }
			set
			{
				if (value == _allowNull) return;
				_allowNull = value;
				OnPropertyChanged();
			}
		}

		public bool PrimaryKey
		{
			get { return _primaryKey; }
			set
			{
				if (value == _primaryKey) return;
				_primaryKey = value;
				if (value)
				{
					AllowNull = false;
				}
				OnPropertyChanged();
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Name).Append(' ').Append(Datatype);
			sb.Append(AllowNull ? " NULL " : " NOT NULL ");
			if (PrimaryKey)
			{
				sb.Append("PRIMARY KEY");
			}
			return sb.ToString();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}