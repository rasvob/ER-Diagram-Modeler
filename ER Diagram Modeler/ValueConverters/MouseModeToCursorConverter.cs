using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.ValueConverters
{
	/// <summary>
	/// Cursor converter
	/// </summary>
	public class MouseModeToCursorConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var mode = (MouseMode) value;
			switch (mode)
			{
				case MouseMode.Select:
					return Cursors.Arrow;
				case MouseMode.NewTable:
					return Cursors.Cross;
				case MouseMode.Panning:
					return Cursors.Hand;
				default:
					return Cursors.Arrow;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}