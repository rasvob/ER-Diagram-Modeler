using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Properties;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class Datatype: INotifyPropertyChanged,IDataErrorInfo
	{
		private string _name;
		private int? _lenght;
		private int? _scale;
		private int? _precision;
		private int? _defaultLenght;
		private int? _defaultScale;
		private int? _defaultPrecision;
		private bool _hasLenght;
		private bool _hasScale;
		private bool _hasPrecision;
		private bool _isNullable;
		private int? _maxLenght;
		private int? _maxScale;
		private int? _maxPrecision;

		public int? MaxScale
		{
			get { return _maxScale; }
			set
			{
				if (value == _maxScale) return;
				_maxScale = value;
				OnPropertyChanged();
			}
		}

		public int? MaxPrecision
		{
			get { return _maxPrecision; }
			set
			{
				if (value == _maxPrecision) return;
				_maxPrecision = value;
				OnPropertyChanged();
			}
		}

		public int? MaxLenght
		{
			get { return _maxLenght; }
			set
			{
				if(value == _maxLenght) return;
				_maxLenght = value;
				OnPropertyChanged();
			}
		}

		public bool IsNullable
		{
			get { return _isNullable; }
			set
			{
				if (value == _isNullable) return;
				_isNullable = value;
				OnPropertyChanged();
			}
		}

		public bool HasPrecision
		{
			get { return _hasPrecision; }
			set
			{
				if (value == _hasPrecision) return;
				_hasPrecision = value;
				OnPropertyChanged();
			}
		}

		public bool HasScale
		{
			get { return _hasScale; }
			set
			{
				if (value == _hasScale) return;
				_hasScale = value;
				OnPropertyChanged();
			}
		}

		public bool HasLenght
		{
			get { return _hasLenght; }
			set
			{
				if (value == _hasLenght) return;
				_hasLenght = value;
				OnPropertyChanged();
			}
		}

		public int? DefaultPrecision
		{
			get { return _defaultPrecision; }
			set
			{
				if (value == _defaultPrecision) return;
				_defaultPrecision = value;
				OnPropertyChanged();
			}
		}

		public int? DefaultScale
		{
			get { return _defaultScale; }
			set
			{
				if (value == _defaultScale) return;
				_defaultScale = value;
				OnPropertyChanged();
			}
		}

		public int? DefaultLenght
		{
			get { return _defaultLenght; }
			set
			{
				if (value == _defaultLenght) return;
				_defaultLenght = value;
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

		public int? Lenght
		{
			get { return _lenght; }
			set
			{
				if (value == _lenght) return;
				_lenght = value;
				OnPropertyChanged();
			}
		}

		public int? Scale
		{
			get { return _scale; }
			set
			{
				if (value == _scale) return;
				_scale = value;
				OnPropertyChanged();
			}
		}

		public int? Precision
		{
			get { return _precision; }
			set
			{
				if (value == _precision) return;
				_precision = value;
				OnPropertyChanged();
			}
		}

		public Datatype()
		{
			
		}

		public Datatype(Datatype datatype)
		{
			Name = datatype.Name;
			DefaultLenght = datatype.DefaultLenght;
			DefaultPrecision = datatype.DefaultPrecision;
			DefaultScale = datatype.DefaultScale;
			HasLenght = datatype.HasLenght;
			HasScale = datatype.HasScale;
			HasPrecision = datatype.HasPrecision;
			MaxLenght = datatype.MaxLenght;
			MaxScale = datatype.MaxScale;
			MaxPrecision = datatype.MaxPrecision;
			IsNullable = datatype.IsNullable;
			Lenght = datatype.Lenght;
			Scale = datatype.Scale;
			Precision = datatype.Precision;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Name);
			if (Lenght.HasValue && Lenght.Value != 0)
			{
				sb.Append($"({Lenght.Value})");
			}
			else if (Scale.HasValue && !Precision.HasValue && Scale.Value != 0)
			{
				sb.Append($"({Scale.Value})");
			}
			else if (Scale.HasValue && Precision.HasValue && Scale.Value != 0 && Precision.Value != 0)
			{
				sb.Append($"({Precision.Value},{Scale.Value})");
			}
			return sb.ToString();
		}


		public static Datatype LoadFromXmlNode(XElement node)
		{
			var res = new Datatype();
			res.Name = node.Attribute("Name").Value;
			res.IsNullable = node.Attribute("IsNullable").Value.Equals("1");
			res.HasLenght = node.Attribute("HasLenght").Value.Equals("1");
			res.HasScale = node.Attribute("HasScale").Value.Equals("1");
			res.HasPrecision = node.Attribute("HasPrecision").Value.Equals("1");

			if (res.HasLenght)
			{
				res.MaxLenght = int.Parse(node.Attribute("MaxLenght").Value);
				res.DefaultLenght = int.Parse(node.Attribute("DefaultLenght").Value);
				res.Lenght = res.DefaultLenght;
			}

			if (res.HasPrecision)
			{
				res.MaxPrecision = int.Parse(node.Attribute("Precision").Value);
				res.DefaultPrecision = int.Parse(node.Attribute("DefaultPrecision").Value);
				res.Precision = res.DefaultPrecision;
			}

			if (res.HasScale)
			{
				res.MaxScale = int.Parse(node.Attribute("Scale").Value);
				res.DefaultScale = int.Parse(node.Attribute("DefaultScale").Value);
				res.Scale = res.DefaultScale;
			}

			return res;
		}

		public static List<Datatype> LoadDatatypesFromResource(ConnectionType connectionType)
		{
			var res = new List<Datatype>();

			if (connectionType == ConnectionType.None)
			{
				return res;
			}

			var resource = connectionType == ConnectionType.SqlServer ? Resources.DataTypesMicrosoft : Resources.DataTypesOracle;

			XDocument document = XDocument.Parse(resource);
			var nodes = document.Root.Elements();

			res.AddRange(nodes.Select(LoadFromXmlNode));

			return res;
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
					case "Scale":
						if(Scale < 0 || Scale > MaxScale)
						{
							return $"Scale value has to be between {0} and {MaxScale}";
						}
						break;

					case "Precision":
						if(Precision < 0 || Precision > MaxPrecision)
						{
							return $"Precision value has to be between {0} and {MaxPrecision}";
						}
						break;

					case "Lenght":
						if(Lenght < -1 || Lenght > MaxLenght)
						{
							return $"Lenght value has to be between {-1} and {MaxLenght}";
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