using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.ViewModels
{
	public class TableViewModel: INotifyPropertyChanged, IDiagramSerializable
	{
		private TableModel _model;
		private TableViewMode _viewMode;
		private double _left;
		private double _top;
		private bool _isSelected;
		private double _width;
		private double _height;
		private bool _isMoving;
		private bool _areLimitsEnabled = true;

		public bool AreLimitsEnabled
		{
			get { return _areLimitsEnabled; }
			set
			{
				if (value == _areLimitsEnabled) return;
				_areLimitsEnabled = value;
				OnPropertyChanged();
			}
		}

		public bool IsMoving
		{
			get { return _isMoving; }
			set
			{
				if (value == _isMoving) return;
				_isMoving = value;
				OnPropertyChanged();
			}
		}

		public double Height
		{
			get { return _height; }
			set
			{
				if (value.Equals(_height)) return;
				var args = new TablePositionAndMeasureEventArgs()
				{
					HeightDelta = value - _height
				};
				_height = value;
				OnPositionAndMeasureChanged(args);
				OnPropertyChanged();
			}
		}

		public double Width
		{
			get { return _width; }
			set
			{
				if (value.Equals(_width)) return;
				var args = new TablePositionAndMeasureEventArgs()
				{
					WidthDelta = value - _width
				};
				_width = value;
				OnPositionAndMeasureChanged(args);
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

		public TableViewMode ViewMode
		{
			get { return _viewMode; }
			set
			{
				if (value == _viewMode) return;
				_viewMode = value;
				OnPropertyChanged();
			}
		}

		public TableModel Model
		{
			get { return _model; }
			set
			{
				if (Equals(value, _model)) return;
				_model = value;
				OnPropertyChanged();
			}
		}

		public double Left
		{
			get { return _left; }
			set
			{
				if (value.Equals(_left)) return;
				var args = new TablePositionAndMeasureEventArgs()
				{
					LeftDelta = value - _left
				};
				_left = value;
				OnPositionAndMeasureChanged(args);
				OnPropertyChanged();
			}
		}

		public double Top
		{
			get { return _top; }
			set
			{
				if (value.Equals(_top)) return;
				var args = new TablePositionAndMeasureEventArgs()
				{
					TopDelta = value - _top
				};
				_top = value;
				OnPositionAndMeasureChanged(args);
				OnPropertyChanged();
			}
		}

		public TableViewModel()
		{
			ViewMode = TableViewMode.Standard;
		}

		public TableViewModel(TableModel model):this()
		{
			_model = model;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<TablePositionAndMeasureEventArgs> PositionAndMeasureChanged;
		public event EventHandler PositionAndMeasureChangesCompleted;
		public event EventHandler PositionAndMeasureChangesStarted;
		public event EventHandler TableViewModeChanged;
		public event EventHandler<TableContent> TableLoaded; 

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPositionAndMeasureChanged(TablePositionAndMeasureEventArgs e)
		{
			PositionAndMeasureChanged?.Invoke(this, e);
		}

		public void OnPositionAndMeasureChangesCompleted()
		{
			PositionAndMeasureChangesCompleted?.Invoke(this, System.EventArgs.Empty);
		}

		public void OnPositionAndMeasureChangesStarted()
		{
			PositionAndMeasureChangesStarted?.Invoke(this, System.EventArgs.Empty);
		}

		public void OnTableViewModeChanged()
		{
			TableViewModeChanged?.Invoke(this, System.EventArgs.Empty);
		}

		public void OnTableLoaded(TableContent e)
		{
			TableLoaded?.Invoke(this, e);
		}

		public XElement CreateElement()
		{
			return new XElement("TableViewModel",
				Model.CreateElement(),
				new XAttribute("ViewMode", ViewMode),
				new XAttribute("Top", Top),
				new XAttribute("Left", Left),
				new XAttribute("Width", Width),
				new XAttribute("Height", Height));
		}

		public void LoadFromElement(XElement element)
		{
			Model = new TableModel();
			Model.LoadFromElement(element.Element("TableModel"));
			Top = Convert.ToDouble(element.Attribute("Top")?.Value);
			Left = Convert.ToDouble(element.Attribute("Left")?.Value);
			Height = Convert.ToDouble(element.Attribute("Height")?.Value);
			Width = Convert.ToDouble(element.Attribute("Width")?.Value);
			string value = element.Attribute("ViewMode")?.Value;
			if (value != null)
				ViewMode = (TableViewMode) Enum.Parse(typeof(TableViewMode), value, true);
		}
	}
}