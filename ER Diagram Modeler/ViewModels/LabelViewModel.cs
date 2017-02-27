using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;

namespace ER_Diagram_Modeler.ViewModels
{
	public class LabelViewModel: INotifyPropertyChanged, IDiagramSerializable
	{
		private double _top;
		private double _left;
		private double _width;
		private double _height;
		private bool _isSelected;
		private string _labelText;

		public double Top
		{
			get { return _top; }
			set
			{
				if (value.Equals(_top)) return;
				_top = value;
				OnPropertyChanged();
			}
		}

		public double Left
		{
			get { return _left; }
			set
			{
				if (value.Equals(_left)) return;
				_left = value;
				OnPropertyChanged();
			}
		}

		public double Width
		{
			get { return _width; }
			set
			{
				if (value.Equals(_width)) return;
				_width = value;
				OnPropertyChanged();
			}
		}

		public double Height
		{
			get { return _height; }
			set
			{
				if (value.Equals(_height)) return;
				_height = value;
				OnPropertyChanged();
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected) return;
				_isSelected = value;
				OnPropertyChanged();
			}
		}

		public string LabelText
		{
			get { return _labelText; }
			set
			{
				if (value == _labelText) return;
				_labelText = value;
				OnPropertyChanged();
			}
		}

		public LabelViewModel()
		{
			LabelText = string.Empty;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Create XML element
		/// </summary>
		/// <returns>XML serialized data</returns>
		public XElement CreateElement()
		{
			return new XElement("LabelViewModel",
				new XAttribute("LabelText", LabelText),
				new XAttribute("Top", Top),
				new XAttribute("Left", Left),
				new XAttribute("Width", Width),
				new XAttribute("Height", Height));
		}

		/// <summary>
		/// Load property values from XML element
		/// </summary>
		/// <param name="element">XML serialized data from CreateElement()</param>
		public void LoadFromElement(XElement element)
		{
			LabelText = element.Attribute("LabelText")?.Value;
			Top = Convert.ToDouble(element.Attribute("Top")?.Value);
			Left = Convert.ToDouble(element.Attribute("Left")?.Value);
			Height = Convert.ToDouble(element.Attribute("Height")?.Value);
			Width = Convert.ToDouble(element.Attribute("Width")?.Value);
		}
	}
}