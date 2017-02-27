using System.Windows;
using System.Windows.Controls;

namespace ER_Diagram_Modeler.Views.Canvas.LabelItem
{
	/// <summary>
	/// Decorator placeholder
	/// </summary>
	public class ResizeDecoratorLabelFlat: Control
	{
		static ResizeDecoratorLabelFlat()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeDecoratorLabelFlat), new FrameworkPropertyMetadata(typeof(ResizeDecoratorLabelFlat)));
		}
	}
}