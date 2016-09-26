using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ER_Diagram_Modeler
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		//FOR DEBUG ONLY - AVALONDOCK ISSUE
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			//RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			//this.DispatcherUnhandledException += OnDispatcherUnhandledException;
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
		{
			Trace.WriteLine(dispatcherUnhandledExceptionEventArgs.Exception.Message);
			dispatcherUnhandledExceptionEventArgs.Handled = true;
		}
	}
}
