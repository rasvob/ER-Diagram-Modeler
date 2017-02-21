using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ValueConverters
{
	/// <summary>
	/// TableViewMode To Visibility
	/// </summary>
	public class TableViewModeToVisibilityConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var mode = (TableViewMode) value;
			switch (mode)
			{
				case TableViewMode.NameOnly:
					return Visibility.Collapsed;
				case TableViewMode.Standard:
					return Visibility.Visible;
				default:
					return Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//No need for back conversion
			return null;
		}
	}
}