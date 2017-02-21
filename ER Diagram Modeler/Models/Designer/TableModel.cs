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
	/// <summary>
	/// Table representation
	/// </summary>
	public class TableModel: INotifyPropertyChanged, IDataErrorInfo, IDiagramSerializable
	{
		/// <summary>
		/// ObjectId or GUID if not in DB yet
		/// </summary>
		public string Id { get; set; } = Guid.NewGuid().ToString();
		private string _title = "Table1";
		private ObservableCollection<TableRowModel> _attributes = new ObservableCollection<TableRowModel>();

		/// <summary>
		/// Table name
		/// </summary>
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

		/// <summary>
		/// Table columns
		/// </summary>
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

		/// <summary>
		/// Seralize attributes
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Error indexer
		/// </summary>
		/// <param name="columnName">Input name</param>
		/// <returns></returns>
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

		/// <summary>
		/// Update attribute from loaded
		/// </summary>
		/// <param name="old">Updating</param>
		/// <param name="row">Updated</param>
		public void UpdateAttributes(TableRowModel old, TableRowModel row)
		{
			int indexOf = Attributes.IndexOf(old);
			Attributes[indexOf] = row;
		}

		/// <summary>
		/// Add default column for CREATE TABLE statement
		/// </summary>
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

		/// <summary>
		/// Refresh data from DB
		/// </summary>
		/// <param name="fresh">Updated model</param>
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

		/// <summary>
		/// Create XML element
		/// </summary>
		/// <returns>XML serialized element</returns>
		public XElement CreateElement()
		{
			return new XElement("TableModel", new XAttribute("Title", Title), new XAttribute("Id", Id));
		}

		/// <summary>
		/// Load data from XML
		/// </summary>
		/// <param name="element">Data in XML</param>
		public void LoadFromElement(XElement element)
		{
			Title = element.Attribute("Title")?.Value;
			Id = element.Attribute("Id")?.Value;
		}
	}
}
