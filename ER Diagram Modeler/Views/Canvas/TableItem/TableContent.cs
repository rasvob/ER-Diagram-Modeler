﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	public class TableContent: ContentControl
	{
		public TableViewModel TableViewModel { get; set; }

		public static readonly int ZIndexSelectedValue = DesignerCanvas.TableSelectedZIndex;
		public static readonly int ZIndexUnSelectedValue = DesignerCanvas.TableUnselectedZIndex;

		private double _oldHeight;

		public bool IsSelected
		{
			get
			{
				return (bool)GetValue(IsSelectedProperty);
			}
			set
			{
				SetValue(IsSelectedProperty, value);
				TableViewModel.IsSelected = value;
			}
		}

		public static double TableItemMinWidth { get; } = (double)Application.Current.FindResource("TableMinWidth");
		public static double TableItemMinHeight { get; } = (double)Application.Current.FindResource("TableMinHeight");

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

		//For XAML Creation
		public TableContent() { }

		public TableContent(TableViewModel tableViewModel)
		{
			TableViewModel = tableViewModel;
			TableViewModel.PropertyChanged += TableViewModelOnPropertyChanged;
			Style = Application.Current.FindResource("TableItemStyle") as Style;
			Content = new TableViewControl(TableViewModel);
		}

		private void TableViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("ViewMode"))
			{
				switch (TableViewModel.ViewMode)
				{
					case TableViewMode.Standard:
						Height = _oldHeight;
						TableViewModel.Height = _oldHeight;
						break;
					case TableViewMode.NameOnly:
						_oldHeight = TableViewModel.Height;
						Height = TableItemMinHeight;
						Width = ActualWidth;
						TableViewModel.Height = Height;
						TableViewModel.Width = Width;
						break;
				}
			}
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
	}
}