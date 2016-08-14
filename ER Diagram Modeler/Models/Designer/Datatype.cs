using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using ER_Diagram_Modeler.Annotations;

namespace ER_Diagram_Modeler.Models.Designer
{
	public class Datatype: INotifyPropertyChanged
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
		private int _maxLenght;

		public int MaxLenght
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

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Name);
			if (Lenght.HasValue)
			{
				sb.Append($"({Lenght.Value})");
			}
			else if (Scale.HasValue && !Precision.HasValue)
			{
				sb.Append($"({Scale.Value})");
			}
			else if (Scale.HasValue && Precision.HasValue)
			{
				sb.Append($"({Scale.Value},{Precision.Value})");
			}
			return sb.ToString();
		}

		public static Datatype LoadFromXmlNode(XElement node)
		{
			return null;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}