using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas
{
	public class DesignerCanvas: System.Windows.Controls.Canvas
	{
		public static int TableSelectedZIndex = 100;
		public static int TableUnselectedZIndex = 10;
		public static int ConnectionLineZIndex = 5;
		public static int ConnectionPointZIndex = 6;
		public static int CanvasGridZIndex = 2;
		public static int GridCellWidth = 30;
		private readonly Path _canvasGrid;

		public IEnumerable<TableContent> SelectedTables => Children.OfType<TableContent>().Where(t => t.IsSelected);
		public IEnumerable<TableContent> Tables => Children.OfType<TableContent>();
		public bool IsGridEnabled => _canvasGrid.Visibility == Visibility.Visible;

		public DesignerCanvas()
		{
			_canvasGrid = new Path
			{
				Stroke = new SolidColorBrush(Colors.DimGray),
				StrokeThickness = 0.5,
				SnapsToDevicePixels = true,
				Opacity = 0.3,
				CacheMode = new BitmapCache(1)
			};
			SetZIndex(_canvasGrid, CanvasGridZIndex);
			Children.Add(_canvasGrid);

			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			RefreshGuideLines();
		}

		public void DeselectTables()
		{
			foreach (TableContent item in SelectedTables)
			{
				item.IsSelected = false;
			}
		}

		public void ResetZIndexes()
		{
			foreach (TableContent item in Tables)
			{
				SetZIndex(item, TableUnselectedZIndex);
			}
		}

		public void RefreshGuideLines()
		{
			_canvasGrid.Data = CreateGridWithStreamGeometry();
		}

		private StreamGeometry CreateGridWithStreamGeometry()
		{
			StreamGeometry geometry = new StreamGeometry();

			using (StreamGeometryContext context = geometry.Open())
			{
				for(int i = 1; i < Height / GridCellWidth; i++)
				{
					context.BeginFigure(new Point(0, i * GridCellWidth), true, false);
					context.LineTo(new Point(Width, i * GridCellWidth), true, false);
				}

				for(int i = 1; i < Width / GridCellWidth; i++)
				{
					context.BeginFigure(new Point(i * GridCellWidth, 0), true, false);
					context.LineTo(new Point(i * GridCellWidth, Height), true, false);
				}
			}

			geometry.Freeze();
			return geometry;
		}

		public void SetGuideLinesVisible(bool areGuideLinesVisible)
		{
			_canvasGrid.Visibility = areGuideLinesVisible ? Visibility.Visible : Visibility.Hidden;
		}
	}
}