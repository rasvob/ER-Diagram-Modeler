using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableRowModel: INotifyPropertyChanged, IDataErrorInfo
	{
		private string _name;
		private Datatype _datatype;
		private bool _allowNull = true;
		private bool _primaryKey;
		private Datatype _loadeDatatypeFromDb;

		private IEnumerable<Datatype> _datatypesItemSource = DatatypeProvider.Instance.SessionBasedDatatypes;
		private Datatype _selectedDatatype;

		public Datatype SelectedDatatype
		{
			get { return _selectedDatatype; }
			set
			{
				if(Equals(value, _selectedDatatype)) return;
				_selectedDatatype = value;
				//Datatype = new Datatype(value);
				Datatype = SelectedDatatype;
				OnPropertyChanged();
			}
		}

		public IEnumerable<Datatype> DatatypesItemSource
		{
			get { return _datatypesItemSource; }
			set
			{
				if(Equals(value, _datatypesItemSource)) return;
				_datatypesItemSource = value;
				OnPropertyChanged();
			}
		}

		public Datatype LoadeDatatypeFromDb
		{
			get { return _loadeDatatypeFromDb; }
			set
			{
				if (Equals(value, _loadeDatatypeFromDb)) return;
				_loadeDatatypeFromDb = value;
				OnPropertyChanged();
			}
		}

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

		public TableRowModel()
		{
			SelectedDatatype = DatatypesItemSource.FirstOrDefault();
		}

		public TableRowModel(string name, Datatype datatype)
		{
			Name = name;
			Datatype = datatype;
			SelectedDatatype =
				DatatypesItemSource.FirstOrDefault(t => t.Name.Equals(datatype.Name, StringComparison.CurrentCultureIgnoreCase));

			if (SelectedDatatype == null)
			{
				return;
			}

			if (Datatype.HasLenght)
			{
				SelectedDatatype.Lenght = Datatype.Lenght;
			}

			if (Datatype.HasScale)
			{
				SelectedDatatype.Scale = Datatype.Scale;
			}

			if (Datatype.HasPrecision)
			{
				SelectedDatatype.Precision = Datatype.Precision;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string this[string columnName]
		{
			get
			{
				switch(columnName)
				{
					case "Name":
						if (Name.Length == 0)
						{
							return "Name can't be empty";
						}
						break;
					default:
						return null;
				}

				return Error;
			}
		}

		public string Error => string.Empty;
	}
}