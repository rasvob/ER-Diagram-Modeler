using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	/// <summary>
	/// Decorator for resize thumb
	/// </summary>
	class ResizeDecoratorFlat: Control
	{
		static ResizeDecoratorFlat()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeDecoratorFlat), new FrameworkPropertyMetadata(typeof(ResizeDecoratorFlat)));
		}
	}
}
