using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using EmServiceLib;

namespace MainInterface
{
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.UnhandledException +=
					new UnhandledExceptionEventHandler(FormMain.LastExceptionHandler);

				EmService.Init();

				// trying to load settings			
				EmSettings settings = new EmSettings();
				settings.LoadSettings();

				string locale;
				switch (settings.Language)
				{
				    case "Русский":
				        locale = "ru-RU";
				        settings.CurrentLanguage = "ru";
				        break;
				    case "English":
				        locale = "en-US";
				        settings.CurrentLanguage = "en";
				        break;
				    default:
				        //locale = string.Empty;
				        locale = "ru-RU";
				        EmService.WriteToLogFailed("no language detected! russian was set");
				        settings.CurrentLanguage = "ru";
				        settings.Language = "Русский";
				        break;
				}

				// setting up current culture
				Thread.CurrentThread.CurrentCulture =
				    Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale, false);
				Application.CurrentCulture = Thread.CurrentThread.CurrentCulture;

				// enabling visual styles
				Application.EnableVisualStyles();
				// enabling imagelist			
				Application.DoEvents();

				Application.Run(new FormMain(ref settings));
			}
			catch (OutOfMemoryException mex)
			{
				ResourceManager rm = new ResourceManager("MainInterface.emstrings",
														 System.Reflection.Assembly.GetExecutingAssembly());
				string msg = rm.GetString("msg_outofmemory");
				string cap = rm.GetString("caption_unfortunately");

				MessageBox.Show(msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);
				EmService.DumpException(mex, "Exception in Main():");
				//throw;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Exception in Main():  " + ex.Message);
				EmService.DumpException(ex, "Exception in Main():");

				//if (EmService.ShowWndFeedback)
				//{
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//EmService.ShowWndFeedback = false;
				//}
				//throw;
			}
		}
	}
}
