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

		public IEnumerable<TableContent> SelectedTables => Children.OfType<TableContent>().Where(t => t.IsSelected);
		public IEnumerable<TableContent> Tables => Children.OfType<TableContent>();
		public Path CanvasGrid { get; set; }

		public DesignerCanvas()
		{
			CanvasGrid = new Path();
			CanvasGrid.Stroke = new SolidColorBrush(Colors.DimGray);
			CanvasGrid.StrokeThickness = 0.5;
			CanvasGrid.SnapsToDevicePixels = true;
			CanvasGrid.Opacity = 0.1;
			CanvasGrid.CacheMode = new BitmapCache(1);
			SetZIndex(CanvasGrid, CanvasGridZIndex);
			Children.Add(CanvasGrid);

			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			//Very bad performance
			CanvasGrid.Data = CreateGridWithStreamGeometry();
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
	}
}