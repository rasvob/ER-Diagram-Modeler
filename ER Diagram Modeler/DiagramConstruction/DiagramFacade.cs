﻿using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ER_Diagram_Modeler.Annotations;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas;

namespace ER_Diagram_Modeler.DiagramConstruction
{
	public class DiagramFacade
	{
		public DatabaseModelDesignerViewModel ViewModel { get; set; }

		public DiagramFacade(DatabaseModelDesignerViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public DiagramFacade(DatabaseModelDesigner designer)
		{
			ViewModel = designer.ViewModel;
		}

		public void AddTable(TableModel source)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);

			if (ViewModel.TableViewModels.Any(t => t.Model.Title.Equals(source.Title)))
			{
				return;
			}

			TableModel model = ctx.ReadTableDetails(source.Id, source.Title);
			model.Title = source.Title;
			model.Id = source.Id;
			TableViewModel vm = new TableViewModel()
			{
				Model = model
			};
			int x, y;
			FindFreePosition(out x, out y);
			vm.Left = x;
			vm.Top = y;
			ViewModel.TableViewModels.Add(vm);
		}

		private void FindFreePosition(out int x, out int y)
		{
			x = 50;
			y = 50;

			bool isFree = false;
			var rnd = new Random();

			int xMin = (int)(ViewModel.HorizontalScrollOffset/ViewModel.Scale);
			int yMin = (int)(ViewModel.VeticalScrollOffset/ViewModel.Scale);

			int xMax = xMin + (int)(ViewModel.ViewportWidth / ViewModel.Scale);
			int yMax = yMin + (int)(ViewModel.ViewportHeight / ViewModel.Scale);

			if (xMax > ViewModel.CanvasWidth)
			{
				xMax = (int)ViewModel.CanvasWidth;
			}

			if(yMax > ViewModel.CanvasHeight)
			{
				yMax = (int)ViewModel.CanvasHeight;
			}

			while (!isFree)
			{
				x = rnd.Next(xMin, xMax);
				y = rnd.Next(yMin, yMax);

				isFree = true;
				foreach (TableViewModel viewModel in ViewModel.TableViewModels)
				{
					if (x >= viewModel.Left && x <= viewModel.Left+viewModel.Width && y >= viewModel.Top && y <= viewModel.Top+viewModel.Height)
					{
						isFree = false;
						break;
					}
				}
			}
		}
	}
}