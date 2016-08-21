using System;
using System.Globalization;
using System.Windows.Data;

namespace ER_Diagram_Modeler.ValueConverters
{
	public class ScaleToPercentConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (int)((double) value*100);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (double)((double)value/100.0);
		}
	}
}