using System;
using System.Globalization;
using System.Windows.Data;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ValueConverters
{
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

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}