using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.Properties;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ViewModels
{
	public class DatabaseModelDesignerViewModel: INotifyPropertyChanged
	{
		private ObservableCollection<TableViewModel> _tableViewModels;
		private double _scale = 1;
		private MouseMode _mouseMode = MouseMode.Select;
		private double _viewportHeight;
		private double _viewportWidth;
		private double _canvasWidth = 2500;
		private double _canvasHeight = 2500;
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

		public ICommand ChangeZoomCommand { get; private set; }

		public DatabaseModelDesignerViewModel()
		{
			TableViewModels = new ObservableCollection<TableViewModel>();
			ConnectionInfoViewModels = new ObservableCollection<ConnectionInfoViewModel>();
			ChangeZoomCommand = new SimpleCommand(ChangeZoomCommand_Executed);
		}

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

		public event PropertyChangedEventHandler PropertyChanged;
		public event EventHandler<ScaleEventArgs> ScaleChanged;
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
	}
}