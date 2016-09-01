using System.Windows;
using System.Windows.Controls;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas.Connection
{
	public class LineContent: ContentControl
	{
		public ConnectionLine ConnectionLine { get; set; }

		public static readonly DependencyProperty MoveThumbTemplateProperty =
			DependencyProperty.RegisterAttached("LineMoveThumbTemplate", typeof(ControlTemplate), typeof(LineContent));

		public static ControlTemplate GetMoveThumbTemplate(UIElement element)
		{
			return (ControlTemplate)element.GetValue(MoveThumbTemplateProperty);
		}

		public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
		{
			element.SetValue(MoveThumbTemplateProperty, value);
		}

		static LineContent()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LineContent), new FrameworkPropertyMetadata(typeof(LineContent)));
		}

		public LineContent()
		{
			
		}

		public LineContent(ConnectionLine connectionLine)
		{
			ConnectionLine = connectionLine;
			Content = connectionLine.Line;
		}
	}
}