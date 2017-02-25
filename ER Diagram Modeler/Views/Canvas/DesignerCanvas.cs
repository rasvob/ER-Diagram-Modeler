using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
		/// <summary>
		/// Selected table item Z index
		/// </summary>
		public static int TableSelectedZIndex = 100;

		/// <summary>
		/// Unselected table item Z index
		/// </summary>
		public static int TableUnselectedZIndex = 10;

		/// <summary>
		/// Line Z index
		/// </summary>
		public static int ConnectionLineZIndex = 5;

		/// <summary>
		/// Bending point Z index
		/// </summary>
		public static int ConnectionPointZIndex = 6;

		/// <summary>
		/// Grid Z index
		/// </summary>
		public static int CanvasGridZIndex = 2;

		/// <summary>
		/// Grid cell width
		/// </summary>
		public static int GridCellWidth = 30;

		private readonly Path _canvasGrid;

		/// <summary>
		/// Selected table item
		/// </summary>
		public IEnumerable<TableContent> SelectedTables => Children.OfType<TableContent>().Where(t => t.IsSelected);

		/// <summary>
		/// All tables
		/// </summary>
		public IEnumerable<TableContent> Tables => Children.OfType<TableContent>();

		/// <summary>
		/// Is grid visible
		/// </summary>
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

		/// <summary>
		/// Create grid for the first time
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="routedEventArgs"></param>
		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			var data = CreateGridWithStreamGeometry(Height, Width, GridCellWidth);
			RefreshGuideLines(data);
			Loaded -= OnLoaded;
		}

		/// <summary>
		/// Deselect all tables
		/// </summary>
		public void DeselectTables()
		{
			foreach (TableContent item in SelectedTables)
			{
				item.IsSelected = false;
			}
		}

		/// <summary>
		/// Reset Z indexes of items
		/// </summary>
		public void ResetZIndexes()
		{
			foreach (TableContent item in Tables)
			{
				SetZIndex(item, TableUnselectedZIndex);
			}
		}

		/// <summary>
		/// Refresh grid
		/// </summary>
		/// <param name="geometry">Grid lines</param>
		public void RefreshGuideLines(StreamGeometry geometry)
		{
			_canvasGrid.Data = geometry;
		}

		/// <summary>
		/// Create stream geometry for grid
		/// </summary>
		/// <param name="height"></param>
		/// <param name="width"></param>
		/// <param name="cellWidth"></param>
		/// <returns></returns>
		public static StreamGeometry CreateGridWithStreamGeometry(double height, double width, double cellWidth)
		{
			StreamGeometry geometry = new StreamGeometry();

			using (StreamGeometryContext context = geometry.Open())
			{
				for(int i = 1; i < height / cellWidth; i++)
				{
					context.BeginFigure(new Point(0, i * cellWidth), true, false);
					context.LineTo(new Point(width, i * cellWidth), true, false);
				}

				for(int i = 1; i < width / cellWidth; i++)
				{
					context.BeginFigure(new Point(i * cellWidth, 0), true, false);
					context.LineTo(new Point(i * cellWidth, height), true, false);
				}
			}

			geometry.Freeze();
			return geometry;
		}

		/// <summary>
		/// Set guidelines visible
		/// </summary>
		/// <param name="areGuideLinesVisible"></param>
		public void SetGuideLinesVisible(bool areGuideLinesVisible)
		{
			_canvasGrid.Visibility = areGuideLinesVisible ? Visibility.Visible : Visibility.Hidden;
		}
	}
}