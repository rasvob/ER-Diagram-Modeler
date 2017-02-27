using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;

namespace ER_Diagram_Modeler.Views.Canvas.TableItem
{
	/// <summary>
	/// Wrapper for tableview control resize/move on canvas
	/// </summary>
	public class TableContent: ContentControl
	{
		/// <summary>
		/// Viewmodel of control
		/// </summary>
		public TableViewModel TableViewModel { get; set; }

		/// <summary>
		/// Add column clicked
		/// </summary>
		public event EventHandler<TableModel> AddNewRow;

		/// <summary>
		/// Selected row double clicked
		/// </summary>
		public event EventHandler<EditRowEventArgs> EditSelectedRow;

		/// <summary>
		/// Rename menu item clicked
		/// </summary>
		public event EventHandler<TableModel> RenameTable;

		/// <summary>
		/// Delete pressed with selected row
		/// </summary>
		public event EventHandler<EditRowEventArgs> RemoveSelectedRow;

		/// <summary>
		/// Drop table menu item clicked
		/// </summary>
		public event EventHandler<TableModel> DropTable;

		/// <summary>
		/// Primary key manager menu item clicked
		/// </summary>
		public event EventHandler<TableModel> UpdatePrimaryKeyConstraint;

		/// <summary>
		/// Selected item Z Index
		/// </summary>
		public static readonly int ZIndexSelectedValue = DesignerCanvas.TableSelectedZIndex;

		/// <summary>
		/// Unselected item Z Index
		/// </summary>
		public static readonly int ZIndexUnSelectedValue = DesignerCanvas.TableUnselectedZIndex;

		private double _oldHeight;

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
				TableViewModel.IsSelected = value;
			}
		}

		/// <summary>
		/// Minimum item width
		/// </summary>
		public static double TableItemMinWidth { get; } = (double)Application.Current.FindResource("TableMinWidth");

		/// <summary>
		/// Minimum item height
		/// </summary>
		public static double TableItemMinHeight { get; } = (double)Application.Current.FindResource("TableMinHeight");

		/// <summary>
		/// Is item selected
		/// </summary>
		/// <remarks>XAML</remarks>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty
			.Register(
				"IsSelected", 
				typeof(bool), 
				typeof(TableContent), 
				new FrameworkPropertyMetadata(false)
			);

		/// <summary>
		/// Thumb template
		/// </summary>
		public static readonly DependencyProperty MoveThumbTemplateProperty =
			DependencyProperty.RegisterAttached("MoveThumbTemplate", typeof(ControlTemplate), typeof(TableContent));

		/// <summary>
		/// Get thumb template
		/// </summary>
		/// <param name="element">UI element</param>
		/// <returns>Control template of element</returns>
		public static ControlTemplate GetMoveThumbTemplate(UIElement element)
		{
			return (ControlTemplate) element.GetValue(MoveThumbTemplateProperty);
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

		static TableContent()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TableContent), new FrameworkPropertyMetadata(typeof(TableContent)));
		}

		/// <summary>
		/// For XAML Creation
		/// </summary>
		public TableContent() { }

		public TableContent(TableViewModel tableViewModel)
		{
			TableViewModel = tableViewModel;
			TableViewModel.PropertyChanged += TableViewModelOnPropertyChanged;
			Style = Application.Current.FindResource("TableItemStyle") as Style;
			var control = new TableViewControl(TableViewModel);
			control.AddNewRow += ControlOnAddNewRow;
			control.EditSelectedRow += ControlOnEditSelectedRow;
			control.RenameTable += ControlOnRenameTable;
			control.RemoveSelectedRow += ControlOnRemoveSelectedRow;
			control.DropTable += ControlOnDropTable;
			control.UpdatePrimaryKeyConstraint += ControlOnUpdatePrimaryKeyConstraint;
			Content = control;
		}

		private void ControlOnUpdatePrimaryKeyConstraint(object sender, TableModel tableModel)
		{
			OnUpdatePrimaryKeyConstraint(tableModel);
		}

		private void ControlOnDropTable(object sender, TableModel tableModel)
		{
			OnDropTable(tableModel);
		}

		private void ControlOnRemoveSelectedRow(object sender, EditRowEventArgs editRowEventArgs)
		{
			OnRemoveSelectedRow(editRowEventArgs);
		}

		private void ControlOnRenameTable(object sender, TableModel tableModel)
		{
			OnRenameTable(tableModel);
		}

		private void ControlOnEditSelectedRow(object sender, EditRowEventArgs editRowEventArgs)
		{
			OnEditSelectedRow(editRowEventArgs);
		}

		private void ControlOnAddNewRow(object sender, TableModel tableModel)
		{
			OnAddNewRow(tableModel);
		}

		/// <summary>
		/// View mode changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void TableViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName.Equals("ViewMode"))
			{
				switch (TableViewModel.ViewMode)
				{
					case TableViewMode.Standard:
						Height = _oldHeight;
						TableViewModel.Height = _oldHeight;
						TableViewModel.OnTableViewModeChanged();
						break;
					case TableViewMode.NameOnly:
						_oldHeight = TableViewModel.Height;
						Height = TableItemMinHeight;
						Width = ActualWidth;
						TableViewModel.Height = Height;
						TableViewModel.Width = Width;
						TableViewModel.OnTableViewModeChanged();
						break;
				}
			}
		}

		/// <summary>
		/// Item clicked
		/// </summary>
		/// <param name="e"></param>
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
					canvas.DeselectLabels();
				}
				else
				{
					if (!IsSelected)
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

		protected virtual void OnAddNewRow(TableModel e)
		{
			AddNewRow?.Invoke(this, e);
		}

		protected virtual void OnEditSelectedRow(EditRowEventArgs e)
		{
			EditSelectedRow?.Invoke(this, e);
		}

		protected virtual void OnRenameTable(TableModel e)
		{
			RenameTable?.Invoke(this, e);
		}

		protected virtual void OnRemoveSelectedRow(EditRowEventArgs e)
		{
			RemoveSelectedRow?.Invoke(this, e);
		}

		protected virtual void OnDropTable(TableModel e)
		{
			DropTable?.Invoke(this, e);
		}

		protected virtual void OnUpdatePrimaryKeyConstraint(TableModel e)
		{
			UpdatePrimaryKeyConstraint?.Invoke(this, e);
		}
	}
}