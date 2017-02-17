using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ER_Diagram_Modeler.CommandOutput;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for OutputPanel.xaml
	/// </summary>
	public partial class OutputPanel : UserControl
	{
		public OutputPanel()
		{
			InitializeComponent();
			Output.OutputListeners.Add(new OutputPanelListener(OutputTextBox));
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			using(var reader =  XmlReader.Create(new StringReader(Properties.Resources.SQLSyntaxHL)))
			{
				OutputTextBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
			}
		}

		private void ClearOutputCommand_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			OutputTextBox.Clear();
		}
	}
}
