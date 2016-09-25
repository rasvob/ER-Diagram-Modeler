using System;
using System.Globalization;
using System.Windows.Data;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ValueConverters
{
	public class SelectionModeToBoolValueConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var val = (MouseMode) value;
			return val == MouseMode.Select;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}