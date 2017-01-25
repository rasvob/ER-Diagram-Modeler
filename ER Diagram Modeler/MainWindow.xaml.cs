﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
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
using ER_Diagram_Modeler.DatabaseConnection;
using ER_Diagram_Modeler.DatabaseConnection.SqlServer;
using ER_Diagram_Modeler.DiagramConstruction;
using ER_Diagram_Modeler.Dialogs;
using ER_Diagram_Modeler.EventArgs;
using ER_Diagram_Modeler.Models.Designer;
using ER_Diagram_Modeler.ViewModels;
using ER_Diagram_Modeler.ViewModels.Enums;
using ER_Diagram_Modeler.Views.Canvas;
using ER_Diagram_Modeler.Views.Panels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Xceed.Wpf.AvalonDock.Layout;

namespace ER_Diagram_Modeler
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public MainWindowViewModel MainWindowViewModel { get; set; }
		private TableModel _flyoutTableModel;
		private EditRowEventArgs _flyoutRowEventArgs = null;

		public MainWindow()
		{
			SessionProvider.Instance.ConnectionType = ConnectionType.None;
			InitializeComponent();
			MainWindowViewModel = new MainWindowViewModel();
			DataContext = MainWindowViewModel;
			DatabaseConnectionSidebar.ConnectionClick += DatabaseConnectionSidebarOnConnectionClick;
			DatabaseConnectionSidebar.AddTable += DatabaseConnectionSidebarOnAddTable;
		}

		private async void DatabaseConnectionSidebarOnAddTable(object sender, TableModel model)
		{
			var idx = MainDocumentPane.SelectedContentIndex;

			if (idx < 0)
			{
				var title = await ShowNewDiagramDialog();

				if (title == null)
				{
					return;
				}

				AddNewDiagramDocument(title);

				idx = MainDocumentPane.SelectedContentIndex;
			}

			var content = MainDocumentPane.Children[idx].Content;

			var diagram = content as DatabaseModelDesigner;
			if (diagram == null)
			{
				return;
			}

			var facade = new DiagramFacade(diagram.ViewModel);
			bool addTable = facade.AddTable(model);
			if (addTable)
			{
				await Task.Delay(100);
				facade.AddRelationShipsForTable(model, diagram.ModelDesignerCanvas);
			}
		}

		private void AddNewDiagramDocument(string title)
		{
			LayoutAnchorable anchorable = new LayoutAnchorable()
			{
				CanClose = true,
				CanHide = false,
				CanFloat = true,
				CanAutoHide = false,
				Title = title,
				ContentId = $"{title}_ID"
			};

			DatabaseModelDesignerViewModel designerViewModel = new DatabaseModelDesignerViewModel()
			{
				DiagramTitle = title
			};

			MainWindowViewModel.DatabaseModelDesignerViewModels.Add(designerViewModel);

			anchorable.Content = new DatabaseModelDesigner()
			{
				ViewModel = designerViewModel
			};
			MainDocumentPane.Children.Add(anchorable);
			int indexOf = MainDocumentPane.Children.IndexOf(anchorable);
			MainDocumentPane.SelectedContentIndex = indexOf;
		}

		private void DatabaseConnectionSidebarOnConnectionClick(object sender, ConnectionType connectionType)
		{
			switch (connectionType)
			{
				case ConnectionType.SqlServer:
					var flyout = Flyouts.Items[0] as Flyout;

					if(flyout != null)
					{
						flyout.IsOpen = !flyout.IsOpen;
					}
					break;
				case ConnectionType.Oracle:
					//TODO: Flyout add 
					break;
			}
		}

		private void MenuItemTest_OnClick(object sender, RoutedEventArgs e)
		{
			var flyout = Flyouts.Items[1] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}
		}

		private void ChangeCanvasSize_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var activeDiagramModeler = MainDocumentPane.SelectedContent.Content as DatabaseModelDesigner;
			if (activeDiagramModeler != null)
			{
				var dialog = new CanvasDimensionDialog
				{
					Owner = this,
					CanvasWidth = activeDiagramModeler.ViewModel.CanvasWidth,
					CanvasHeight = activeDiagramModeler.ViewModel.CanvasWidth
				};
				dialog.ShowDialog();

				if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
				{
					activeDiagramModeler.ViewModel.CanvasWidth = dialog.CanvasWidth;
					activeDiagramModeler.ViewModel.CanvasHeight = dialog.CanvasHeight;
					activeDiagramModeler.CanvasDimensionsChanged();
				}
			}
		}

		private void ChangeCanvasSize_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var activeDiagramModeler = MainDocumentPane.SelectedContent?.Content as DatabaseModelDesigner;

			if (activeDiagramModeler != null)
			{
				e.CanExecute = true;
			}
			else
			{
				e.CanExecute = false;
			}
		}

		private async void ConnectToMsSql_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			ProgressDialogController progressDialogController = null;

			Func<ProgressDialogController, Task> closeProgress = async t =>
			{
				if(t != null)
				{
					if(t.IsOpen)
					{
						await t.CloseAsync();
					}
				}
			};

			try
			{
				progressDialogController = await this.ShowProgressAsync("Please wait", "Connecting to server...", false, new MetroDialogSettings()
				{
					AnimateShow = false, 
					AnimateHide = false
				});
				progressDialogController.SetIndeterminate();

				MsSqlDatabase db = new MsSqlDatabase();
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
				builder.DataSource = MsSqlServerNameTextBox.Text;
				if (WinAuthSwitch.IsChecked != null) builder.IntegratedSecurity = !WinAuthSwitch.IsChecked.Value;

				if (!builder.IntegratedSecurity)
				{
					builder.UserID = MsSqlUsernameTextBox.Text;
					builder.Password = MsSqlPasswordBox.Password;
				}

				string str = builder.ConnectionString;
				
				SqlConnection connection = new SqlConnection(str);
				await connection.OpenAsync();
				connection.Close();

				SessionProvider.Instance.ServerName = builder.DataSource;
				SessionProvider.Instance.UseWinAuth = builder.IntegratedSecurity;

				if (SessionProvider.Instance.UseWinAuth)
				{
					SessionProvider.Instance.Username = builder.UserID;
					SessionProvider.Instance.Password = builder.Password;
				}
				await closeProgress(progressDialogController);
				await this.ShowMessageAsync("Connected", $"Successfuly connected to {SessionProvider.Instance.ServerName}");

				SessionProvider.Instance.ConnectionType = ConnectionType.SqlServer;

				var flyout = Flyouts.Items[0] as Flyout;

				if(flyout != null)
				{
					flyout.IsOpen = !flyout.IsOpen;
				}

				DatabaseConnectionSidebar.LoadMsSqlData();
			}
			catch (SqlException exception)
			{
				await closeProgress(progressDialogController);
				await this.ShowMessageAsync("Connection error", exception.Message);
				SessionProvider.Instance.ConnectionType = ConnectionType.None;
			}
		}

		private void ShowDatabaseConnectionLayout_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			DatabaseConnectionLayoutAnchorable.Show();
		}

		private void ShowDatabaseConnectionLayout_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = DatabaseConnectionLayoutAnchorable.IsHidden;
		}

		private void ConnectToMsSql_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!MsSqlServerNameTextBox.Text.Any())
			{
				e.CanExecute = false;
				return;
			}

			if (WinAuthSwitch.IsChecked != null && (bool) WinAuthSwitch.IsChecked)
			{
				if (!MsSqlUsernameTextBox.Text.Any() || !MsSqlPasswordBox.Password.Any())
				{
					e.CanExecute = false;
					return;
				}
			}

			e.CanExecute = true;
		}

		private async void NewDiagram_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var result = await ShowNewDiagramDialog();

			if (result == null)
			{
				return;
			}

			AddNewDiagramDocument(result);
		}

		private async Task<string> ShowNewDiagramDialog()
		{
			var result = await this.ShowInputAsync("Diagram title", "Enter diagram title", new MetroDialogSettings()
			{
				DefaultText = $"Diagram_{Guid.NewGuid()}"
			});

			return result;
		}

		private void NewDiagram_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = SessionProvider.Instance.ConnectionType != ConnectionType.None;
		}

		private void ApplyAttributeEdit_OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (_flyoutRowEventArgs == null)
			{
				_flyoutTableModel.Attributes.Add(MainWindowViewModel.FlyoutRowModel);
			}
			else
			{
				int indexOf = _flyoutRowEventArgs.TableModel.Attributes.IndexOf(_flyoutRowEventArgs.RowModel);
				_flyoutRowEventArgs.TableModel.Attributes[indexOf] = MainWindowViewModel.FlyoutRowModel;
				_flyoutRowEventArgs = null;
			}

			var flyout = Flyouts.Items[1] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}
		}

		private void ApplyAttributeEdit_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var row = MainWindowViewModel.FlyoutRowModel;

			if (row == null)
			{
				e.CanExecute = false;
				return;
			}

			if (!row["Name"].Equals(string.Empty))
			{
				e.CanExecute = false;
				return;
			}

			var props = new string[] { "Scale", "Precision", "Lenght" };
			foreach (string prop in props)
			{
				if (!row.Datatype[prop].Equals(string.Empty))
				{
					e.CanExecute = false;
					return;
				}
			}

			e.CanExecute = true;
		}

		public void AddNewRowHandler(object sender, TableModel args)
		{
			var flyout = Flyouts.Items[1] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}

			MainWindowViewModel.FlyoutRowModel = new TableRowModel();
			_flyoutTableModel = args;
		}

		public void EditRowHandler(object sender, EditRowEventArgs args)
		{
			var flyout = Flyouts.Items[1] as Flyout;

			if(flyout != null)
			{
				flyout.IsOpen = !flyout.IsOpen;
			}

			MainWindowViewModel.FlyoutRowModel = new TableRowModel(args.RowModel.Name, args.RowModel.Datatype);
			_flyoutRowEventArgs = args;
		}
	}
}
