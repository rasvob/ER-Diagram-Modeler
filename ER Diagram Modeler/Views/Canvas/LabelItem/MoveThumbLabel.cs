using System;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace ER_Diagram_Modeler.Views.Canvas.LabelItem
{
	/// <summary>
	/// Move thumb for label
	/// </summary>
	public class MoveThumbLabel: Thumb
	{
		private DesignerCanvas _canvas;
		private LabelContent _item;

		public MoveThumbLabel()
		{
			DragDelta += OnDragDelta;
			DragCompleted += OnDragCompleted;
			DragStarted += OnDragStarted;
		}

		/// <summary>
		/// Drag started
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dragStartedEventArgs"></param>
		private void OnDragStarted(object sender, DragStartedEventArgs dragStartedEventArgs)
		{
			_item = DataContext as LabelContent;

			if(_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
			}
		}

		/// <summary>
		/// Drag ended
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDragCompleted(object sender, DragCompletedEventArgs e)
		{
			SnapToGrid();
		}

		/// <summary>
		/// Dragging
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDragDelta(object sender, DragDeltaEventArgs e)
		{
			if (_item != null && _canvas != null && _item.IsSelected)
			{
				double minLeft = double.MaxValue;
				double minTop = double.MaxValue;

				double maxTop = double.MinValue;
				double maxLeft = double.MinValue;

				LabelContent[] selected = _canvas.SelectedLabels.ToArray();
				foreach(LabelContent item in selected)
				{
					minLeft = Math.Min(DesignerCanvas.GetLeft(item), minLeft);
					minTop = Math.Min(DesignerCanvas.GetTop(item), minTop);
					maxLeft = Math.Max(DesignerCanvas.GetLeft(item) + item.ActualWidth, maxLeft);
					maxTop = Math.Max(DesignerCanvas.GetTop(item) + item.ActualHeight, maxTop);
				}

				double deltaHorizontal = (int)Math.Max(-minLeft, e.HorizontalChange);
				double deltaVertical = (int)Math.Max(-minTop, e.VerticalChange);

				if (deltaHorizontal + maxLeft > _canvas.ActualWidth)
				{
					deltaHorizontal = 0;
				}

				if(deltaVertical + maxTop > _canvas.ActualHeight)
				{
					deltaVertical = 0;
				}

				foreach(LabelContent item in selected)
				{
					var leftPos = DesignerCanvas.GetLeft(item) + deltaHorizontal;
					var topPos = DesignerCanvas.GetTop(item) + deltaVertical;
					DesignerCanvas.SetLeft(item, leftPos);
					DesignerCanvas.SetTop(item, topPos);
					item.ViewModel.Left = leftPos;
					item.ViewModel.Top = topPos;
				}

				e.Handled = true;
			}
		}

		/// <summary>
		/// If is grid enabled, snap to intersects
		/// </summary>
		private void SnapToGrid()
		{
			if(_canvas.IsGridEnabled)
			{
				int cellWidth = DesignerCanvas.GridCellWidth;

				var selected = _canvas.SelectedLabels.ToArray();

				foreach(LabelContent item in selected)
				{
					double top = item.ViewModel.Top;
					double left = item.ViewModel.Left;
					double bottom = top + item.ViewModel.Height;
					double right = left + item.ViewModel.Width;

					double approxCellTop = Math.Round(top / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;
					double approxCellLeft = Math.Round(left / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;

					
					DesignerCanvas.SetLeft(item, approxCellLeft);
					DesignerCanvas.SetTop(item, approxCellTop);
					item.ViewModel.Left = approxCellLeft;
					item.ViewModel.Top = approxCellTop;
				}
			}
		}
	}
}