using System;
using System.Globalization;
using System.Windows.Data;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ValueConverters
{
	/// <summary>
	/// Button IsChecked state by command state
	/// </summary>
	public class MouseModeToButtonCheckedConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var mode = (MouseMode) value;
			var name = parameter.ToString();

			if (mode == MouseMode.Select && name.Equals("SelectionModeButon"))
			{
				return true;
			}

			if (mode == MouseMode.NewTable && name.Equals("NewTableButon"))
			{
				return true;
			}

			if (mode == MouseMode.NewLabel && name.Equals("NewLabelButon"))
			{
				return true;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}