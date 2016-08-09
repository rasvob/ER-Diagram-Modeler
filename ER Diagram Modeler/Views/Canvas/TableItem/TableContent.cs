using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	public class TableContent: ContentControl
	{
		public bool IsSelected
		{
			get
			{
				return (bool)GetValue(IsSelectedProperty);
			}
			set
			{
				SetValue(IsSelectedProperty, value);
			}
		}

		public TableContentViewModel ViewModel { get; set; }

		public static readonly int ZIndexSelectedValue = 1000;
		public static readonly int ZIndexUnSelectedValue = 0;

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty
			.Register(
				"IsSelected", 
				typeof(bool), 
				typeof(TableContent), 
				new FrameworkPropertyMetadata(false)
			);

		public static readonly DependencyProperty MoveThumbTemplateProperty =
			DependencyProperty.RegisterAttached("MoveThumbTemplate", typeof(ControlTemplate), typeof(TableContent));

		public static ControlTemplate GetMoveThumbTemplate(UIElement element)
		{
			return (ControlTemplate) element.GetValue(MoveThumbTemplateProperty);
		}

		public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
		{
			element.SetValue(MoveThumbTemplateProperty, value);
		}

		static TableContent()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TableContent), new FrameworkPropertyMetadata(typeof(TableContent)));
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);
			DesignerCanvas canvas = VisualTreeHelper.GetParent(this) as DesignerCanvas;

			if (canvas != null)
			{
				if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
				{
					IsSelected = !IsSelected;
					DesignerCanvas.SetZIndex(this, !IsSelected ? ZIndexUnSelectedValue : ZIndexSelectedValue);
				}
				else
				{
					if (!IsSelected)
					{
						canvas.ResetZIndexes();
						canvas.DeselectAll();
						IsSelected = true;
						DesignerCanvas.SetZIndex(this, ZIndexSelectedValue);
					}
				}
			}
			e.Handled = false;
		}

		//public void LoadThumb()
		//{
		//	if(Template != null)
		//	{
		//		ContentPresenter contentPresenter = Template.FindName("ContentPresenter", this) as ContentPresenter;
		//		MoveThumb moveThumb = Template.FindName("MoveThumb", this) as MoveThumb;
		//		if(contentPresenter != null && moveThumb != null)
		//		{
		//			UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;

		//			if(contentVisual != null)
		//			{
		//				ControlTemplate template = GetMoveThumbTemplate(contentVisual);

		//				if(template != null)
		//				{
		//					moveThumb.Template = template;
		//				}
		//			}
		//		}
		//	}
		//}
	}
}