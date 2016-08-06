using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
		}

		public void AddElement(Rectangle rect)
		{
			var contentControl = new ContentControl();
			contentControl.Content = rect;
			Canvas.Children.Add(rect);
		}
	}
}
