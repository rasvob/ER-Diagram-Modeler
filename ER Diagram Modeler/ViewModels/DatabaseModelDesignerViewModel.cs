using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.DiagramConstruction.Serialization;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelDesignerViewModel: INotifyPropertyChanged, IDiagramSerializable
	{
		/// <summary>
		/// Default canvas size
		/// </summary>
		public static readonly double DefaultWidth = 2500;

		private ObservableCollection<TableViewModel> _tableViewModels;
		private double _scale = 1;
		private MouseMode _mouseMode = MouseMode.Select;
		private double _viewportHeight;
		private double _viewportWidth;
		private double _canvasWidth = DefaultWidth;
		private double _canvasHeight = DefaultWidth;
		private double _zoomBoxCanvasWidth = 250;
		private double _zoomBoxCanvasHeight = 250;
		private double _veticalScrollOffset;
		private double _horizontalScrollOffset;
		private double _maxScale = 3;
		private double _minScale = 0.25;
		private Visibility _zoomBoxVisibility = Visibility.Visible;
		private ObservableCollection<ConnectionInfoViewModel> _connectionInfoViewModels;
		private bool _areGuideLinesVisible = true;
		private string _diagramTitle;
		private bool _areTableLimitsEnabled = true;
		private string _database = "NO_DB";

		/// <summary>
		/// DB Name
		/// </summary>
		public string Database
		{
			get { return _database; }
			set
			{
				if (value == _database) return;
				_database = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Is table movement enabled
		/// </summary>
		public bool AreTableLimitsEnabled
		{
			get { return _areTableLimitsEnabled; }
			set
			{
				if (value == _areTableLimitsEnabled) return;
				_areTableLimitsEnabled = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Title
		/// </summary>
		public string DiagramTitle
		{
			get { return _diagramTitle; }
			set
			{
				if (value == _diagramTitle) return;
				_diagramTitle = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Are canvas guideline visible
		/// </summary>
		public bool AreGuideLinesVisible
		{
			get { return _areGuideLinesVisible; }
			set
			{
				if (value == _areGuideLinesVisible) return;
				_areGuideLinesVisible = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Visualization of foreign key constraint
		/// </summary>
		public ObservableCollection<ConnectionInfoViewModel> ConnectionInfoViewModels
		{
			get { return _connectionInfoViewModels; }
			set
			{
				if (Equals(value, _connectionInfoViewModels)) return;
				_connectionInfoViewModels = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Show/hide zoombox control
		/// </summary>
		public Visibility ZoomBoxVisibility
		{
			get { return _zoomBoxVisibility; }
			set
			{
				if (value == _zoomBoxVisibility) return;
				_zoomBoxVisibility = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Minimum scale limit
		/// </summary>
		public double MinScale
		{
			get { return _minScale; }
			set
			{
				if (value.Equals(_minScale)) return;
				_minScale = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Maximum scale limit
		/// </summary>
		public double MaxScale
		{
			get { return _maxScale; }
			set
			{
				if (value.Equals(_maxScale)) return;
				_maxScale = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Hozizontal scroll offset
		/// </summary>
		public double HorizontalScrollOffset
		{
			get { return _horizontalScrollOffset; }
			set
			{
				if (value.Equals(_horizontalScrollOffset)) return;
				_horizontalScrollOffset = value;
				OnPropertyChanged();
				OnComputedPropertyChanged();
			}
		}

		/// <summary>
		/// Vertical scroll offset
		/// </summary>
		public double VeticalScrollOffset
		{
			get { return _veticalScrollOffset; }
			set
			{
				if (value.Equals(_veticalScrollOffset)) return;
				_veticalScrollOffset = value;
				OnPropertyChanged();
				OnComputedPropertyChanged();
			}
		}

		/// <summary>
		/// Height of zoombox canvas
		/// </summary>
		public double ZoomBoxCanvasHeight
		{
			get { return _zoomBoxCanvasHeight; }
			set
			{
				if (value.Equals(_zoomBoxCanvasHeight)) return;
				_zoomBoxCanvasHeight = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Width of zoombox canvas
		/// </summary>
		public double ZoomBoxCanvasWidth
		{
			get { return _zoomBoxCanvasWidth; }
			set
			{
				if (value.Equals(_zoomBoxCanvasWidth)) return;
				_zoomBoxCanvasWidth = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Height of canvas
		/// </summary>
		public double CanvasHeight
		{
			get { return _canvasHeight; }
			set
			{
				if (value.Equals(_canvasHeight)) return;
				_canvasHeight = value;
				OnPropertyChanged();
				OnCanvasDimensionsChanged();
			}
		}

		/// <summary>
		/// Width of zoombox canvas
		/// </summary>
		public double CanvasWidth
		{
			get { return _canvasWidth; }
			set
			{
				if (value.Equals(_canvasWidth)) return;
				_canvasWidth = value;
				OnPropertyChanged();
				OnCanvasDimensionsChanged();
			}
		}

		/// <summary>
		/// Width of viewport
		/// </summary>
		public double ViewportWidth
		{
			get { return _viewportWidth; }
			set
			{
				if (value.Equals(_viewportWidth)) return;
				_viewportWidth = value;
				OnPropertyChanged();
				OnComputedPropertyChanged();
			}
		}

		/// <summary>
		/// Height of viewport
		/// </summary>
		public double ViewportHeight
		{
			get { return _viewportHeight; }
			set
			{
				if (value.Equals(_viewportHeight)) return;
				_viewportHeight = value;
				OnPropertyChanged();
				OnComputedPropertyChanged();
			}
		}

		/// <summary>
		/// Mouse mode
		/// </summary>
		public MouseMode MouseMode
		{
			get { return _mouseMode; }
			set
			{
				if (value == _mouseMode) return;
				_mouseMode = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Scale (zoom level)
		/// </summary>
		public double Scale
		{
			get { return _scale; }
			set
			{
				if (value.Equals(_scale)) return;
				var args = new ScaleEventArgs()
				{
					OldScale = _scale,
					OldHorizontalOffset = HorizontalScrollOffset,
					OldVerticalOffset = VeticalScrollOffset,
					OldViewportHeight = ViewportHeight,
					OldViewportWidth = ViewportWidth
				};
				_scale = value;
				OnPropertyChanged();
				OnComputedPropertyChanged();
				OnScaleChanged(args);
			}
		}

		/// <summary>
		/// Viewport width in zoombox
		/// </summary>
		public double ZoomBoxThumbWidth
		{
			get
			{
				double ratio = CanvasWidth/ZoomBoxCanvasWidth;
				double val = (ViewportWidth/ratio)/Scale;
				if (val > ZoomBoxCanvasHeight)
				{
					return ZoomBoxCanvasWidth;
				}
				return val;
			}
		}

		/// <summary>
		/// Viewport height in zoombox
		/// </summary>
		public double ZoomBoxThumbHeight
		{
			get
			{
				double ratio = CanvasHeight / ZoomBoxCanvasHeight;
				double val = (ViewportHeight / ratio) / Scale;
				if (val > ZoomBoxCanvasHeight)
				{
					return ZoomBoxCanvasHeight;
				}
				return val;
			}
		}

		/// <summary>
		/// Viewport offset from top in zoombox
		/// </summary>
		public double ZoomBoxThumbTop
		{
			get
			{
				double ratio = CanvasHeight / ZoomBoxCanvasHeight;
				double val = (VeticalScrollOffset/ratio) / Scale;
				if(ZoomBoxThumbHeight >= ZoomBoxCanvasHeight)
				{
					return 0;
				}
				return val;
			}
		}

		/// <summary>
		/// Viewport offset from left in zoombox
		/// </summary>
		public double ZoomBoxThumbLeft
		{
			get
			{
				double ratio = CanvasHeight / ZoomBoxCanvasHeight;
				double val = (HorizontalScrollOffset / ratio) / Scale;
				if(ZoomBoxThumbWidth >= ZoomBoxCanvasWidth)
				{
					return 0;
				}
				return val;
			}
		}

		/// <summary>
		/// Tables visualization
		/// </summary>
		public ObservableCollection<TableViewModel> TableViewModels
		{
			get { return _tableViewModels; }
			set
			{
				if (Equals(value, _tableViewModels)) return;
				_tableViewModels = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Command for change of scale
		/// </summary>
		public ICommand ChangeZoomCommand { get; private set; }

		public DatabaseModelDesignerViewModel()
		{
			TableViewModels = new ObservableCollection<TableViewModel>();
			ConnectionInfoViewModels = new ObservableCollection<ConnectionInfoViewModel>();
			ChangeZoomCommand = new SimpleCommand(ChangeZoomCommand_Executed);
			Database = "NO_DB";
		}

		/// <summary>
		/// Change scale
		/// </summary>
		/// <param name="o">Scale</param>
		private void ChangeZoomCommand_Executed(object o)
		{
			double scale;
			bool res = double.TryParse((string) o, out scale);
			if (res)
			{
				SnapScaleToRange(ref scale);
				Scale = scale;
			}
		}

		/// <summary>
		/// Limit scale in bounds
		/// </summary>
		/// <param name="scale">Final scale</param>
		private void SnapScaleToRange(ref double scale)
		{
			if (scale > MaxScale)
			{
				scale = MaxScale;
			}
			if (scale < MinScale)
			{
				scale = MinScale;
			}
		}

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Scale is changed
		/// </summary>
		public event EventHandler<ScaleEventArgs> ScaleChanged;

		/// <summary>
		/// Canvas dimensions changed
		/// </summary>
		public event EventHandler CanvasDimensionsChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public virtual void OnComputedPropertyChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbWidth"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbHeight"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbLeft"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbTop"));
		}

		protected virtual void OnScaleChanged(ScaleEventArgs args)
		{
			ScaleChanged?.Invoke(this, args);
		}

		protected virtual void OnCanvasDimensionsChanged()
		{
			CanvasDimensionsChanged?.Invoke(this, System.EventArgs.Empty);
		}

		/// <summary>
		/// Create XML element
		/// </summary>
		/// <returns>XML serialized data</returns>
		public XElement CreateElement()
		{
			var elem = new XElement("DatabaseModelDesignerViewModel", 
				new XAttribute("DiagramTitle", DiagramTitle),
				new XAttribute("CanvasWidth", CanvasWidth),
				new XAttribute("CanvasHeight", CanvasHeight));

			var tables = new XElement("TableViewModels");
			TableViewModels.ToList().ForEach(t => tables.Add(t.CreateElement()));

			var lines = new XElement("ConnectionInfoViewModels");
			ConnectionInfoViewModels.ToList().ForEach(t => lines.Add(t.CreateElement()));
			elem.Add(tables);
			elem.Add(lines);
			return elem;
		}

		/// <summary>
		/// Load property values from XML element
		/// </summary>
		/// <param name="element">XML serialized data from CreateElement()</param>
		public void LoadFromElement(XElement element)
		{
			DiagramTitle = element.Attribute("DiagramTitle").Value;
			CanvasWidth = Convert.ToDouble(element.Attribute("CanvasWidth")?.Value);
			CanvasHeight = Convert.ToDouble(element.Attribute("CanvasHeight")?.Value);
			//Database = element.Attribute("Database")?.Value;
		}
	}
}