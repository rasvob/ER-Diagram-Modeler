using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ER_Diagram_Modeler.ValueConverters
{
	public class GuideLinesVisibilityToBoolValueConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var visibility = (Visibility) value;
			return visibility == Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}