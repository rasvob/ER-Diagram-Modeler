using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableModel: INotifyPropertyChanged, IDataErrorInfo
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		private string _title = "Table";
		private ObservableCollection<TableRowModel> _attributes = new ObservableCollection<TableRowModel>();

		public string Title
		{
			get { return _title; }
			set
			{
				if (value == _title) return;
				_title = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<TableRowModel> Attributes
		{
			get { return _attributes; }
			set
			{
				if (Equals(value, _attributes)) return;
				_attributes = value;
				OnPropertyChanged();
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Title);
			foreach (TableRowModel attribute in Attributes)
			{
				sb.AppendLine(attribute.ToString());
			}
			return sb.ToString();
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
				if (columnName.Equals("Title"))
				{
					if (Title.Length == 0)
					{
						return "Title can't be empty";
					}
				}

				return string.Empty;
			}
		}

		public string Error => string.Empty;
	}
}
