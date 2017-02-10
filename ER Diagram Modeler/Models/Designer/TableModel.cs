using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class TableModel: INotifyPropertyChanged, IDataErrorInfo, IDiagramSerializable
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
			foreach (TableRowModel model in fresh.Attributes)
			{
				Attributes.Add(model);
			}
		}

		public XElement CreateElement()
		{
			return new XElement("TableModel", new XAttribute("Title", Title), new XAttribute("Id", Id));
		}

		public void LoadFromElement(XElement element)
		{
			Title = element.Attribute("Title")?.Value;
			Id = element.Attribute("Id")?.Value;
		}
	}
}
