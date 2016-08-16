using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Xml.Linq;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using MahApps.Metro.Controls;

namespace ER_Diagram_Modeler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public List<TableViewModel> Models { get; set; }

		public MainWindowViewModel MainWindowViewModel { get; set; }

		public MainWindow()
		{
			SessionProvider.Instance.ConnectionType = ConnectionType.SqlServer;
			InitializeComponent();
			MainWindowViewModel = new MainWindowViewModel();
			DatabaseModelDesigner.ViewModel = MainWindowViewModel.DatabaseModelDesignerViewModel;
			DataContext = MainWindowViewModel;
		}

		private TableViewModel SeedDataTable()
		{
			var attrs = new List<TableRowModel>();

			var row1 = new TableRowModel("Id", DatatypeProvider.Instance.FindDatatype("int"));
			var row2 = new TableRowModel("FirstName", DatatypeProvider.Instance.FindDatatype("varchar"));
			var row3 = new TableRowModel("LastName", DatatypeProvider.Instance.FindDatatype("varchar"));
			var row4 = new TableRowModel("Salary", DatatypeProvider.Instance.FindDatatype("numeric"));

			row2.Datatype.Lenght = 100;
			row3.Datatype.Lenght = 200;
			row4.Datatype.Scale = 20;
			row4.Datatype.Precision = 15;

			row1.PrimaryKey = true;

			attrs.Add(row1);
			attrs.Add(row2);
			attrs.Add(row3);
			attrs.Add(row4);

			var model = new TableModel();
			model.Title = "Employee";
			model.Attributes = attrs;
			var vm = new TableViewModel(model);
			vm.Left = 100;
			vm.Top = 100;

			return vm;
			//return new TableViewModel()
			//{
			//	Left = 100,
			//	Top = 50,
			//	Model = new TableModel()
			//	{
			//		Title = "Employee",
			//		Attributes = new List<TableRowModel>()
			//		{
			//			new TableRowModel()
			//			{
			//				AllowNull = false,
			//				Datatype = DatatypeProvider.Instance.FindDatatype("Int"),
			//				Name = "Id",
			//				PrimaryKey = true
			//			},
			//			new TableRowModel()
			//			{
			//				AllowNull = false,
			//				Datatype = DatatypeProvider.Instance.FindDatatype("Varchar"),
			//				Name = "FirstName",
			//				PrimaryKey = false
			//			},
			//			new TableRowModel()
			//			{
			//				AllowNull = false,
			//				Datatype = new Datatype()
			//				{
			//					Name = "Varchar",
			//					Lenght = 100
			//				},
			//				Name = "LastName",
			//				PrimaryKey = true
			//			}
			//		}
			//	}
			//};
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			MainWindowViewModel.DatabaseModelDesignerViewModel.TableViewModels.Add(SeedDataTable());
		}

		private void ButtonBase_OnClickDelete(object sender, RoutedEventArgs e)
		{
			
		}

		private void MenuItemTest_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (TableViewModel model in MainWindowViewModel.DatabaseModelDesignerViewModel.TableViewModels)
			{
				Trace.WriteLine(model.Model);
			}
		}
	}
}
