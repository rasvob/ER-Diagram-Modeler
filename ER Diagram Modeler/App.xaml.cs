﻿using System.Diagnostics;
using System.Globalization;
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
		/// <summary>
		/// Init point of app
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>Exception logging only in avalon dock isn!t revisited</remarks>
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			SetEnglighCultureInfo();
			//TODO: Uncomment only in case of issues with docking in debug mode
			//RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			//this.DispatcherUnhandledException += OnDispatcherUnhandledException;
		}

		/// <summary>
		/// Trace write exceptions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="dispatcherUnhandledExceptionEventArgs"></param>
		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
		{
			Trace.WriteLine(dispatcherUnhandledExceptionEventArgs.Exception.Message);
			dispatcherUnhandledExceptionEventArgs.Handled = true;
		}

		/// <summary>
		/// En-Us culture set on start
		/// </summary>
		private void SetEnglighCultureInfo()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US");
			CultureInfo.CurrentUICulture = new CultureInfo("en-US");
		}
	}
}
