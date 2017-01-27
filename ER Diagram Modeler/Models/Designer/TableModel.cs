using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableModel: INotifyPropertyChanged, IDataErrorInfo
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		private string _title = "Table1";
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
		public event EventHandler<EditRowEventArgs> ColumnDropped; 

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

		public void UpdateAttributes(TableRowModel old, TableRowModel row)
		{
			int indexOf = Attributes.IndexOf(old);
			Attributes[indexOf] = row;
		}

		public void AddDefaultAttribute()
		{
			switch (SessionProvider.Instance.ConnectionType)
			{
				case ConnectionType.None:
					break;
				case ConnectionType.SqlServer:
					TableRowModel msrow = new TableRowModel($"Id{Title}", DatatypeProvider.Instance.FindDatatype("int", ConnectionType.SqlServer));
					Attributes.Add(msrow);
					break;
				case ConnectionType.Oracle:
					TableRowModel oraclerow = new TableRowModel($"Id{Title}", DatatypeProvider.Instance.FindDatatype("integer", ConnectionType.Oracle));
					Attributes.Add(oraclerow);
					break;
			}
		}

		public void RefreshModel(TableModel fresh)
		{
			Id = fresh.Id;
			Title = fresh.Title;

			Attributes.Clear();

			foreach(TableRowModel model in fresh.Attributes)
			{
				Attributes.Add(model);
			}
		}

		protected virtual void OnColumnDropped(EditRowEventArgs e)
		{
			ColumnDropped?.Invoke(this, e);
		}
	}
}
