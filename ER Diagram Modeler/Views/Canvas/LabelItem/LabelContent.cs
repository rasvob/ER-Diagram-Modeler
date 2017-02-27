using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas.LabelItem
{
	/// <summary>
	/// Canvas item for label
	/// </summary>
	public class LabelContent: ContentControl
	{
		/// <summary>
		/// ViewModel
		/// </summary>
		public LabelViewModel ViewModel { get; set; }
		
		/// <summary>
		/// Selected item Z Index
		/// </summary>
		public static readonly int ZIndexSelectedValue = DesignerCanvas.TableSelectedZIndex;

		/// <summary>
		/// Unselected item Z Index
		/// </summary>
		public static readonly int ZIndexUnSelectedValue = DesignerCanvas.LabelUnselectedZIndex;

		/// <summary>
		/// Is item selected
		/// </summary>
		public bool IsSelected
		{
			get
			{
				return (bool)GetValue(IsSelectedProperty);
			}
			set
			{
				SetValue(IsSelectedProperty, value);
				ViewModel.IsSelected = value;
			}
		}

		/// <summary>
		/// Minimum item width
		/// </summary>
		public static double TableItemMinWidth { get; } = (double)Application.Current.FindResource("LabelMinWidth");

		/// <summary>
		/// Minimum item height
		/// </summary>
		public static double TableItemMinHeight { get; } = (double)Application.Current.FindResource("LabelMinHeight");

		/// <summary>
		/// Is item selected
		/// </summary>
		/// <remarks>XAML</remarks>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty
			.Register(
				"IsSelected",
				typeof(bool),
				typeof(LabelContent),
				new FrameworkPropertyMetadata(false)
			);

		/// <summary>
		/// Thumb template
		/// </summary>
		public static readonly DependencyProperty MoveThumbTemplateProperty =
			DependencyProperty.RegisterAttached("MoveThumbLabelTemplate", typeof(ControlTemplate), typeof(LabelContent));

		/// <summary>
		/// Get thumb template
		/// </summary>
		/// <param name="element">UI element</param>
		/// <returns>Control template of element</returns>
		public static ControlTemplate GetMoveThumbTemplate(UIElement element)
		{
			return (ControlTemplate)element.GetValue(MoveThumbTemplateProperty);
		}

		/// <summary>
		/// Set thumb template
		/// </summary>
		/// <param name="element"></param>
		/// <param name="value"></param>
		public static void SetMoveThumbTemplate(UIElement element, ControlTemplate value)
		{
			element.SetValue(MoveThumbTemplateProperty, value);
		}

		static LabelContent()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelContent), new FrameworkPropertyMetadata(typeof(LabelContent)));
		}

		public LabelContent()
		{
			
		}

		public LabelContent(LabelViewModel viewModel)
		{
			ViewModel = viewModel;
			Style = Application.Current.FindResource("LabelItemStyle") as Style;
			var control = new LabelViewControl(ViewModel);
			Content = control;
		}

		/// <summary>
		/// Mouse clicked for selection
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseDown(e);

			DesignerCanvas canvas = VisualTreeHelper.GetParent(this) as DesignerCanvas;

			if(canvas != null)
			{
				if((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
				{
					IsSelected = !IsSelected;
					DesignerCanvas.SetZIndex(this, !IsSelected ? ZIndexUnSelectedValue : ZIndexSelectedValue);
					canvas.DeselectTables();
				}
				else
				{
					if(!IsSelected)
					{
						canvas.ResetZIndexes();
						canvas.DeselectLabels();
						canvas.DeselectTables();
						IsSelected = true;
						DesignerCanvas.SetZIndex(this, ZIndexSelectedValue);
					}
				}
			}
			e.Handled = false;
		}
	}
}