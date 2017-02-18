using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ER_Diagram_Modeler.CommandOutput;
using ER_Diagram_Modeler.Configuration.Providers;
using ER_Diagram_Modeler.DiagramConstruction.Strategy;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using Oracle.ManagedDataAccess.Client;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler.Views.Panels
{
	/// <summary>
	/// Interaction logic for QueryPanel.xaml
	/// </summary>
	public partial class QueryPanel : UserControl
	{
		public string Title { get; set; } = "SQL Query";
		public LayoutAnchorable Anchorable { get; set; }
		public event EventHandler<DataSet> QueryResultReady;

		public string FilePath { get; set; }
		public string Text
		{
			get { return QueryEditor.Text; }
			set { QueryEditor.Text = value; }
		}

		//TODO: Only debug
		private string TestSql = @"SELECT * 
FROM sys.tables

--389576426

SELECT s.column_id, s.name, s.is_nullable, t.name, s.max_length, s.precision, s.scale
FROM sys.columns s JOIN sys.types t ON s.system_type_id = t.system_type_id
WHERE s.object_id = 389576426
ORDER BY s.column_id


SELECT *
FROM sys.columns s JOIN sys.types t ON s.system_type_id = t.system_type_id
WHERE s.object_id = 389576426
ORDER BY s.column_id


SELECT database_id, name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb'); 

SELECT * FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb'); 

SELECT * FROM sys.foreign_key_columns

exec sp_pkeys @table_name = 'hodnoceni_uzivatel'"
			;
		public QueryPanel()
		{
			InitializeComponent();
			QueryEditor.Text = TestSql;
			Loaded += OnLoaded;
		}

		public void BuildNewQueryPanel(MainWindow window, string title)
		{
			Anchorable = new LayoutAnchorable()
			{
				CanClose = true,
				CanHide = false,
				CanFloat = true,
				CanAutoHide = false,
				Title = title
			};

			Title = title;
			Anchorable.Content = this;
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			using(var reader = XmlReader.Create(new StringReader(Properties.Resources.SQLSyntaxHL)))
			{
				QueryEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
			}
		}

		private void RunQuery_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			try
			{
				var data = ctx.ExecuteRawQuery(QueryEditor.Text);
				OnQueryResultReady(data);
			}
			catch (Exception exception)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
			}
		}

		private void RunSelectedQuery_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var ctx = new DatabaseContext(SessionProvider.Instance.ConnectionType);
			try
			{
				var data = ctx.ExecuteRawQuery(QueryEditor.SelectedText);
				OnQueryResultReady(data);
			}
			catch(Exception exception)
			{
				Output.WriteLine(OutputPanelListener.PrepareException(exception.Message));
			}
		}

		private void Save_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (FilePath == null)
			{
				ShowSaveDialog();
			}
			SaveFile();
		}

		private void SaveAs_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ShowSaveDialog();
			SaveFile();
		}

		private void SaveFile()
		{
			if (QueryEditor.Text != null && FilePath != null)
			{
				File.WriteAllText(FilePath, QueryEditor.Text);
				Output.WriteLine("FILE SAVED");
			}
		}

		private void ShowSaveDialog()
		{
			var dialog = new SaveFileDialog
			{
				Filter = "SQL Files|*.sql|Text Files|*.txt|All files|*.*",
				CreatePrompt = true,
				OverwritePrompt = true,
				AddExtension = true,
				DefaultExt = ".sql",
			};
			bool? b = dialog.ShowDialog(Window.GetWindow(this));

			if(b.Value)
			{
				FilePath = dialog.FileName;
				Anchorable.Title = Path.GetFileNameWithoutExtension(FilePath);
			}
		}

		protected virtual void OnQueryResultReady(DataSet e)
		{
			QueryResultReady?.Invoke(this, e);
		}
	}
}
