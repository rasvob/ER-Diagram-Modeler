using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Annotations;
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ER_Diagram_Modeler.Dialogs
{
	/// <summary>
	/// Interaction logic for CanvasDimensionDialog.xaml
	/// </summary>
	public partial class CanvasDimensionDialog : MetroWindow, IDataErrorInfo, INotifyPropertyChanged
	{
		private double _canvasWidth = 2500;
		private double _canvasHeight = 2500;

		public double CanvasWidth
		{
			get { return _canvasWidth; }
			set
			{
				if (value.Equals(_canvasWidth)) return;
				_canvasWidth = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Item));
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
				OnPropertyChanged(nameof(Item));
			}
		}

		public CanvasDimensionDialog()
		{
			InitializeComponent();
		}

		private void Proceed_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void Proceed_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (this["CanvasWidth"].Equals(string.Empty) && this["CanvasHeight"].Equals(string.Empty))
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}

			double w, h;
			if (!double.TryParse(WidthTextBox.Text, out w) || !double.TryParse(HeightTextBox.Text, out h))
			{
				e.CanExecute = false;
			}
		}

		private void Cancel_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = false;
			Trace.WriteLine(Error);
			Close();
		}

		public string this[string columnName]
		{
			get
			{
				switch (columnName)
				{
					case "CanvasWidth":
						if (CanvasWidth < 1000 || CanvasWidth > 8000)
						{
							return $"Width must be between {1000} and {8000}";
						}
						break;
					case "CanvasHeight":
						if(CanvasHeight < 1000 || CanvasHeight > 8000)
						{
							return $"Height must be between {1000} and {8000}";
						}
						break;
					default:
						return string.Empty;
				}
				return string.Empty;
			}
		}

		public string Error => string.Empty;
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
