using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
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
				_scale = value;
				OnPropertyChanged();
				OnComputedPropertyChanged();
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
			ChangeZoomCommand = new SimpleCommand(ChangeZoomCommand_Executed);
		}

		private void ChangeZoomCommand_Executed(object o)
		{
			double scale;
			bool res = double.TryParse((string) o, out scale);
			if (res)
			{
				Scale = scale;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnComputedPropertyChanged()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbWidth"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbHeight"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbLeft"));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ZoomBoxThumbTop"));
		}
	}
}