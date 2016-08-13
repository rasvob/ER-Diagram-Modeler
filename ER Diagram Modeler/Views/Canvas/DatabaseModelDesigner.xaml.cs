using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
	/// Interaction logic for DatabaseModelDesigner.xaml
	/// </summary>
	public partial class DatabaseModelDesigner : UserControl
	{
		private DatabaseModelDesignerViewModel _viewModel;

		public DatabaseModelDesignerViewModel ViewModel
		{
			get { return _viewModel; }
			set
			{
				_viewModel = value;
				DataContext = value;
				ViewModel.TableViewModels.CollectionChanged += TableViewModelsOnCollectionChanged;
			}
		}

		private void TableViewModelsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (TableViewModel item in args.NewItems)
					{
						AddElement(item);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					break;
			}
		}

		public DatabaseModelDesigner()
		{
			InitializeComponent();
		}

		public void AddElement(TableViewModel viewModel)
		{
			var content = new TableContent(viewModel);
			ModelDesignerCanvas.Children.Add(content);
			DesignerCanvas.SetTop(content, viewModel.Top);
			DesignerCanvas.SetLeft(content, viewModel.Left);
		}
	}
}
