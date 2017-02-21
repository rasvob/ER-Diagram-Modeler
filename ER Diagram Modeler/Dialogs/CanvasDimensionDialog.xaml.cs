using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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

		/// <summary>
		/// Width of canvas
		/// </summary>
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
				OnPropertyChanged(nameof(Item));
			}
		}

		public CanvasDimensionDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Close dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Proceed_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		/// <summary>
		/// Are data valid - check
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		/// <summary>
		/// Check input values
		/// </summary>
		/// <param name="columnName">Name of input</param>
		/// <returns>Error string</returns>
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

		/// <summary>
		/// For data binding
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
