using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	public class TableContent: ContentControl
	{
		public bool IsSelected
		{
			get { return (bool)GetValue(IsSelectedProperty); }
			set { SetValue(IsSelectedProperty, value); }
		}

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

		public TableContent()
		{
			Loaded += OnLoaded;
		}

		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);
			System.Windows.Controls.Canvas content = VisualTreeHelper.GetParent(this) as System.Windows.Controls.Canvas;

			if (content != null)
			{
				if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
				{
					IsSelected = !IsSelected;
				}
				else
				{
					if (!IsSelected)
					{
						IsSelected = true;
					}
				}
			}
		}

		public void LoadThumb()
		{
			if(Template != null)
			{
				ContentPresenter contentPresenter = Template.FindName("ContentPresenter", this) as ContentPresenter;
				MoveThumb moveThumb = Template.FindName("MoveThumb", this) as MoveThumb;
				if(contentPresenter != null && moveThumb != null)
				{
					UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;

					if(contentVisual != null)
					{
						ControlTemplate template = GetMoveThumbTemplate(contentVisual);

						if(template != null)
						{
							moveThumb.Template = template;
						}
					}
				}
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			if (Template != null)
			{
				ContentPresenter contentPresenter = Template.FindName("ContentPresenter", this) as ContentPresenter;
				MoveThumb moveThumb = Template.FindName("MoveThumb", this) as MoveThumb;
				if (contentPresenter != null && moveThumb != null)
				{
					UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;

					if (contentVisual != null)
					{
						ControlTemplate template = GetMoveThumbTemplate(contentVisual);

						if (template != null)
						{
							moveThumb.Template = template;
						}
					}
				}
			}
		}
	}
}