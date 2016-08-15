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
			InitializeComponent();
			MainWindowViewModel = new MainWindowViewModel();
			DatabaseModelDesigner.ViewModel = MainWindowViewModel.DatabaseModelDesignerViewModel;
			DataContext = MainWindowViewModel;
		}

		private TableViewModel SeedDataTable2()
		{
			return new TableViewModel()
			{
				Left = 250,
				Top = 100,
				Model = new TableModel()
				{
					Title = "Employee2",
					Attributes = new List<TableRowModel>()
					{
						new TableRowModel()
						{
							AllowNull = false,
							Datatype = new Datatype()
							{
								Name = "Int"
							},
							Name = "Id",
							PrimaryKey = true
						},
						new TableRowModel()
						{
							AllowNull = false,
							Datatype = new Datatype()
							{
								Name = "Varchar",
								Lenght = 100
							},
							Name = "FirstName",
							PrimaryKey = false
						},
						new TableRowModel()
						{
							AllowNull = false,
							Datatype = new Datatype()
							{
								Name = "Varchar",
								Lenght = 100
							},
							Name = "LastName",
							PrimaryKey = false
						}
					}
				}
			};
		}

		private TableViewModel SeedDataTable()
		{
			return new TableViewModel()
			{
				Left = 100,
				Top = 50,
				Model = new TableModel()
				{
					Title = "Employee",
					Attributes = new List<TableRowModel>()
					{
						new TableRowModel()
						{
							AllowNull = false,
							Datatype = new Datatype()
							{
								Name = "Int"
							},
							Name = "Id",
							PrimaryKey = true
						},
						new TableRowModel()
						{
							AllowNull = false,
							Datatype = new Datatype()
							{
								Name = "Varchar",
								Lenght = 100
							},
							Name = "FirstName",
							PrimaryKey = false
						},
						new TableRowModel()
						{
							AllowNull = false,
							Datatype = new Datatype()
							{
								Name = "Varchar",
								Lenght = 100
							},
							Name = "LastName",
							PrimaryKey = true
						}
					}
				}
			};
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
			Datatype.LoadDatatypesFromResource(ConnectionType.SqlServer);
		}
	}
}
