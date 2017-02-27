using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas.LabelItem
{
	/// <summary>
	/// Resize thumb for label
	/// </summary>
	public class ResizeThumbLabel: Thumb
	{
		private LabelContent _item;
		private DesignerCanvas _canvas;

		public ResizeThumbLabel()
		{
			DragDelta += OnDragDelta;
			DragCompleted += OnDragCompleted;
			DragStarted += OnDragStarted;
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
		/// Drag started
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDragStarted(object sender, DragStartedEventArgs e)
		{
			_item = DataContext as LabelContent;

			if (_item != null)
			{
				_canvas = VisualTreeHelper.GetParent(_item) as DesignerCanvas;
			}
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
				double maxDeltaVertical = _canvas.ActualHeight - (DesignerCanvas.GetTop(_item) + _item.ActualHeight);
				double maxDeltaHorizontal = _canvas.ActualWidth - (DesignerCanvas.GetLeft(_item) + _item.ActualWidth);

				var minDeltaVertical = _item.ActualHeight - _item.MinHeight;
				var minDeltaHorizontal = _item.ActualWidth - _item.MinWidth;

				double deltaVertical;
				switch (VerticalAlignment)
				{
					case VerticalAlignment.Bottom:
						deltaVertical = e.VerticalChange > 0 && e.VerticalChange >= maxDeltaVertical
							? 0
							: Math.Min(-e.VerticalChange, minDeltaVertical);
						_item.Height = _item.ActualHeight - (int) deltaVertical;
						_item.ViewModel.Height = _item.Height;
						break;
					case VerticalAlignment.Top:
						deltaVertical = (int) Math.Min(Math.Max(-DesignerCanvas.GetTop(_item), e.VerticalChange), minDeltaVertical);
						var topPos = DesignerCanvas.GetTop(_item) + deltaVertical;
						DesignerCanvas.SetTop(_item, topPos);
						_item.Height = _item.ActualHeight - deltaVertical;
						_item.ViewModel.Height = _item.Height;
						_item.ViewModel.Top = topPos;
						break;
				}

				double deltaHorizontal;
				switch(HorizontalAlignment)
				{
					case HorizontalAlignment.Left:
						deltaHorizontal = (int)Math.Min(Math.Max(-DesignerCanvas.GetLeft(_item), e.HorizontalChange), minDeltaHorizontal);
						var leftPos = DesignerCanvas.GetLeft(_item) + deltaHorizontal;
						DesignerCanvas.SetLeft(_item, leftPos);
						_item.Width = _item.ActualWidth - deltaHorizontal;
						_item.ViewModel.Width = _item.Width;
						_item.ViewModel.Left = leftPos;
						break;
					case HorizontalAlignment.Right:
						deltaHorizontal = e.HorizontalChange > 0 && e.HorizontalChange >= maxDeltaHorizontal ? 0 : (int)Math.Min(-e.HorizontalChange, minDeltaHorizontal);
						_item.Width = _item.ActualWidth - deltaHorizontal;
						_item.ViewModel.Width = _item.Width;
						break;
				}
			}
		}

		/// <summary>
		/// If is grid enabled, snap to intersects
		/// </summary>
		private void SnapToGrid()
		{
			if(_canvas.IsGridEnabled)
			{
				double top = _item.ViewModel.Top;
				double left = _item.ViewModel.Left;
				double bottom = top + _item.ViewModel.Height;
				double right = left + _item.ViewModel.Width;
				int cellWidth = DesignerCanvas.GridCellWidth;

				double approxCell;

				switch(VerticalAlignment)
				{
					case VerticalAlignment.Top:
						approxCell = Math.Round(top / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;
						DesignerCanvas.SetTop(_item, approxCell);
						_item.ViewModel.Top = approxCell;
						break;
					case VerticalAlignment.Bottom:
						approxCell = Math.Round(bottom / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;
						_item.ViewModel.Height = approxCell - _item.ViewModel.Top;
						_item.Height = _item.ViewModel.Height;
						break;
				}

				switch(HorizontalAlignment)
				{
					case HorizontalAlignment.Left:
						approxCell = Math.Round(left / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;
						DesignerCanvas.SetLeft(_item, approxCell);
						_item.ViewModel.Left = approxCell;
						break;
					case HorizontalAlignment.Right:
						approxCell = Math.Round(right / cellWidth, MidpointRounding.AwayFromZero) * cellWidth;
						_item.ViewModel.Width = approxCell - _item.ViewModel.Left;
						_item.Width = _item.ViewModel.Width;
						break;
				}
			}
		}
	}
}