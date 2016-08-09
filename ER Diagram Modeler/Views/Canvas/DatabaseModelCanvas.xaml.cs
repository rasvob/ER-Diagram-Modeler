using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.Views.Canvas.TableItem;

namespace ER_Diagram_Modeler.Views.Canvas
{
	/// <summary>
	/// Interaction logic for DatabaseModelCanvas.xaml
	/// </summary>
	public partial class DatabaseModelCanvas : UserControl
	{
		public DatabaseModelCanvas()
		{
			InitializeComponent();
			var vm = new TableViewModel()
			{
				Left = 120,
				Top = 150,
				Model = new TableModel()
				{
					Title = "TABULKA 1"
				}
			};
			DataContext = vm;
			//AddElement(vm);
		}

		public void AddElement(TableViewModel viewModel)
		{
			var label = new TableViewControl(viewModel);
			label.TableTitle.Text = "Employees";
			var tab = new TableContent();
			tab.Content = label;
			tab.Style = FindResource("TableItemStyle") as Style;
			ModelDesignerCanvas.Children.Add(tab);
			DesignerCanvas.SetTop(tab, 120);
			DesignerCanvas.SetLeft(tab, 120);
		}
	}
}
