using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;

using DeviceIO;
using EmServiceLib;
using FileAnalyzerLib;
using MainInterface.SavingInterface;
using MainInterface.SavingInterface.CheckTreeView;

namespace MainInterface
{
	public enum CurrentOperation
	{
		NONE = 0,
		READ_LIST_OF_ARCHIVES = 1,
		READ_ARCHIVES = 2
	}

	public partial class FormMain : Form
	{
		//private FormArchiveWnd[] childWindowList_;
		//private const short MaxWindowsCnt = 6;
		private EmSettings settings_;

		private FormArchivesList frmArchiveList_;
		private BackgroundWorker bwSearchFiles_;
		private FormWaitSearchArchives frmWait_ = null;
		private StoredArchivesInfo storedArchivesInfo_;

		private DeviceCommonInfo devInfo_;
		private EtDataReader readerEtPQP_A_ = null; 
		private BackgroundWorker bwReaderArchivesList_;
		private BackgroundWorker bwReaderArchives_;
		private ReaderProcessManager readerProcessManager_;
		private CurrentOperation curOperation_ = CurrentOperation.NONE;

		public FormMain(ref EmSettings settings)
		{
			InitializeComponent();

			settings_ = settings;
			//childWindowList_ = new FormArchiveWnd[MaxWindowsCnt];
			frmArchiveList_ = new FormArchivesList(ref settings_, this);
			storedArchivesInfo_ = new StoredArchivesInfo(ref settings_);
		}

		#region Form event handlers

		private void tpbRussian_Click(object sender, EventArgs e)
		{
			try
			{
				tpbRussian.Enabled = false;
				tpbEnglish.Enabled = true;
				settings_.Language = "Русский";
				settings_.SaveSettings();
				MessageBox.Show("Язык изменится только после перезапуска приложения",
					"К сведению", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in tpbRussian_Click");
				throw;
			}
		}

		private void tpbEnglish_Click(object sender, EventArgs e)
		{
			try
			{
				tpbRussian.Enabled = true;
				tpbEnglish.Enabled = false;
				settings_.Language = "English";
				settings_.SaveSettings();
				MessageBox.Show("Interface language will be changed only after application restarts",
					"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in tpbEnglish_Click");
				throw;
			}
		}

		private void tsmiAbortReading_Click(object sender, EventArgs e)
		{
			try
			{
				if (curOperation_ == CurrentOperation.READ_ARCHIVES)
				{
					if (readerProcessManager_ != null)
						readerProcessManager_.KillReader();
					StoredArchivesInfo.DeleteTemporaryArchiveFiles(settings_.LastPathToStoreArchives);
				}
				else if(curOperation_ == CurrentOperation.READ_LIST_OF_ARCHIVES)
				{
					if (bwReaderArchivesList_ != null)
					{
						if (bwReaderArchivesList_.IsBusy)
						{
							bwReaderArchivesList_.CancelAsync();
							
							//readerEtPQP_A_.SetCancelReading();
							readerEtPQP_A_.DisconnectDevice();

							//if (threadInfo == null)
							//{
							//	threadInfo = new ClosingInfoThread(this, mess);
							//	thread = new Thread(new ThreadStart(threadInfo.ThreadEntry));
							//	thread.Start();
							//}

							//while (bwReaderArchivesList_.IsBusy)
							//{
							//	Thread.Sleep(1000);
							//	Application.DoEvents();
							//}
						}
					}
				}

				TurnControlsForReading(false, false,
					new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in tsmiAbortReading_Click");
				throw;
			}
		}

		private void FormMain_Load(object sender, EventArgs e)
		{
			try
			{
				try
				{
					if (File.Exists(EmService.LogGeneralName))
						File.Delete(EmService.LogGeneralName);
					if (File.Exists(EmService.LogFailedName))
						File.Delete(EmService.LogFailedName);
					if (File.Exists(EmService.LogDebugName))
						File.Delete(EmService.LogDebugName);

					if (File.Exists("LogFailedCpp.txt")) File.Delete("LogFailedCpp.txt");
					if (File.Exists("LogGeneralCpp.txt")) File.Delete("LogGeneralCpp.txt");

					Process[] procs = Process.GetProcessesByName("ReaderProc");
					foreach (Process proc in procs)
						proc.Kill();
				}
				catch (Exception) {}

				frmArchiveList_.Show(dockPanelMain);

				StartLoadingListOfArchives();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormMain_Load");
				throw;
			}
		}

		private void tpbFile_Click(object sender, EventArgs e)
		{
			cmsMenuFile.Show(tpbFile.Location.X + this.Location.X + 10,
				tpbFile.Location.Y + this.Location.Y + toolStripMain.Height + 25);
		}

		private void tpbService_Click(object sender, EventArgs e)
		{
			cmsMenuService.Show(tpbService.Location.X + this.Location.X + 10,
				tpbService.Location.Y + this.Location.Y + toolStripMain.Height + 25);
		}

		private void tpbWindow_Click(object sender, EventArgs e)
		{
			cmsMenuWindow.Show(tpbWindow.Location.X + this.Location.X + 10,
				tpbWindow.Location.Y + this.Location.Y + toolStripMain.Height + 25);
		}

		private void tpbHelp_Click(object sender, EventArgs e)
		{
			cmsMenuHelp.Show(tpbHelp.Location.X + this.Location.X + 10,
				tpbHelp.Location.Y + this.Location.Y + toolStripMain.Height + 25);
		}

		//private void cmiCreateWin_Click(object sender, EventArgs e)
		//{
			//for (int iWin = 0; iWin < MaxWindowsCnt; ++iWin)
			//{
			//    if (childWindowList[iWin] == null)
			//    {
			//        childWindowList[iWin] = new FormArchiveWnd();
			//        childWindowList[iWin].MdiParent = this;
			//        childWindowList[iWin].Show();
			//        return;
			//    }
			//}
			//MessageBox.Show(string.Format("You can create only {0} windows!", MaxWindowsCnt));
		//}

		private void cmiCascade_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.Cascade);
		}

		private void cmiTileHorizontal_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileHorizontal);
		}

		private void cmiTileVertical_Click(object sender, EventArgs e)
		{
			this.LayoutMdi(MdiLayout.TileVertical);
		}

		private void cmiExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void cmiSettings_Click(object sender, EventArgs e)
		{
			try
			{
				FormSettings frm = new FormSettings(ref settings_);
				if (frm.ShowDialog() == DialogResult.OK)
				{
					if (frm.DirsListWasChanged)
					{
						settings_.LoadSettings();
						//frmArchiveList_.LoadListOfArchives();
						//storedArchivesInfo_.LoadArchivesInfo(ref e);
						StartLoadingListOfArchives();
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in cmiSettings_Click");
				throw;
			}
		}

		private void cmiReloadArchiveInfoPanel_Click(object sender, EventArgs e)
		{
			StartLoadingListOfArchives();
		}

		private void cmiOpenArchive_Click(object sender, EventArgs e)
		{

		}

		private void cmiLoadFromDevice_Click(object sender, EventArgs e)
		{
			try
			{
				Process[] procs = Process.GetProcessesByName("ReaderProc");
				foreach (Process proc in procs)
					proc.Kill();

				settings_.LoadSettings();

				bwReaderArchivesList_ = new BackgroundWorker();
				bwReaderArchivesList_.WorkerReportsProgress = true;
				bwReaderArchivesList_.WorkerSupportsCancellation = true;
				bwReaderArchivesList_.DoWork += bwReaderArchivesList_DoWork;
				bwReaderArchivesList_.ProgressChanged += bwReaderArchivesList_ProgressChanged;
				bwReaderArchivesList_.RunWorkerCompleted += bwReaderArchivesList_RunWorkerCompleted;

				readerEtPQP_A_ = new EtDataReader(settings_, bwReaderArchivesList_, this.Handle);

				bwReaderArchivesList_.RunWorkerAsync();

				TurnControlsForReading(false, false, 
					new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_waiting_answer"));
				curOperation_ = CurrentOperation.READ_LIST_OF_ARCHIVES;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in cmiLoadFromDevice_Click(): ");
				throw;
			}
		}

		private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if(readerProcessManager_ != null)
					readerProcessManager_.KillReader();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in FormMain_FormClosing(): ");
			}
		}

		#endregion

		#region Exchange with device

		private void bwReaderArchivesList_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			try
			{
				if (e.ProgressPercentage == 1)
				{
					TurnControlsForReading(true, false, 
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_read_archive_list"));
				}
				tsProgressReading.Value = e.ProgressPercentage;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwReaderArchivesList_ProgressChanged");
				tsProgressReading.Value = tsProgressReading.Maximum;
			}
		}

		private void bwReaderArchivesList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				tsLabelOperation.Text = new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready");
				tsProgressReading.Style = ProgressBarStyle.Blocks;

				ExchangeResult res = ExchangeResult.NONE;
				if (e.Error != null)
				{
					EmService.WriteToLogFailed("Error in bwReader_RunWorkerCompleted() 1: " + e.Error.Message);
				}
				else if (e.Cancelled)
				{
					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					return;
				}
				else if (e.Result != null) res = (ExchangeResult)e.Result;

				// если завершили неуспешно
				if (res == ExchangeResult.DEVICE_EMPTY_EXCEPTION)
				{
					MessageBoxes.DeviceHasNoData(this);
					return;
				}
				if (res == ExchangeResult.DEVICE_OLD_VERSION_EXCEPTION)
				{
					MessageBoxes.DeviceOldVersion(this);
					return;
				}
				if (res == ExchangeResult.DISCONNECT)
				{
					MessageBoxes.DeviceConnectionError(this, settings_.IOInterface,
					            settings_.IOParameters);
					return;
				}
				if (res != ExchangeResult.OK)
				{
					MessageBoxes.ReadDevInfoError(this);

					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					return;
				}

				devInfo_ = readerEtPQP_A_.DeviceInfo;
				FormDeviceExchange wndDeviceExchange = new FormDeviceExchange(ref devInfo_, ref settings_, ref storedArchivesInfo_);
				// дерево выбора архивов
				if (wndDeviceExchange.ShowDialog(this) != DialogResult.OK)
				{
					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					return;
				}

				DeviceTreeView devTree = wndDeviceExchange.DeviceData;
				if (devTree.Nodes.Count < 1)
				{
					MessageBoxes.DeviceHasNoData(this);
					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					return;
				}

				// считаем количество отмеченных архивов
				int cntArchives = 0;
				for (int i = 0; i < devTree.Nodes[0].Nodes.Count; i++)
				{
					if (((RegistrationTreeNode)devTree.Nodes[0].Nodes[i]).IsChecked())    //имя объекта
					{
						cntArchives++;
					}
				}
				if (cntArchives < 1)
				{
					MessageBox.Show("You have selected no archives", "", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					return;
				}

				// we make pipe name uniq because cpp process can be killed and wouldn't close pipe,
				// so previous pipe name can be busy
				string pipeName = "readeretpqpa" + DateTime.Now.ToString("ddHHmmss"); 

				// forming the file for the reader
				StreamWriter sw = null;
				try
				{
					sw = new StreamWriter(
						EmService.AppDirectory + EmService.FileNameForReader, 
						false);
					sw.WriteLine("SELECTED");
					sw.WriteLine(wndDeviceExchange.PathToStoreArchives);
					sw.WriteLine(pipeName);

					// devTree.Nodes[0] is a device node. It's always alone
					//int curArchive = 1;
					for (int iReg = 0; iReg < devTree.Nodes[0].Nodes.Count; iReg++)	// registrations
					{
						RegistrationTreeNode regNode = (RegistrationTreeNode)devTree.Nodes[0].Nodes[iReg];
						if (!regNode.IsChecked())
							continue;

						sw.WriteLine(regNode.RegistrationId.ToString());

						string pqpSelectedArchives = string.Empty;
						string avgSelectedArchives = string.Empty;
						string dnsSelectedArchives = string.Empty;

						foreach (MeasureTypeTreeNode typeNode in regNode.Nodes)
						{
							if ((!typeNode.IsChecked()) || typeNode.Nodes.Count < 1) continue;

							switch (typeNode.NodeMeasureType)
							{
								case MeasureType.PQP:
									for (int iPqp = 0; iPqp < typeNode.Nodes.Count; iPqp++)
									{
										if ((typeNode.Nodes[iPqp] as CheckTreeNode).IsChecked())
										{
											if (pqpSelectedArchives == string.Empty)
												pqpSelectedArchives +=
													(typeNode.Nodes[iPqp] as MeasureTreeNodePqp).PqpIndex;
											else pqpSelectedArchives += ("|" +
													(typeNode.Nodes[iPqp] as MeasureTreeNodePqp).PqpIndex);
										}
									}
									break;

								case MeasureType.AVG:
									for (int iAvg = 0; iAvg < typeNode.Nodes.Count; iAvg++)
									{
										if ((typeNode.Nodes[iAvg] as CheckTreeNode).IsChecked())
										{
											MeasureTreeNodeAvg curNode = typeNode.Nodes[iAvg] as MeasureTreeNodeAvg;
											// such numeration of types is determined in the file for reader
											int typeNumber = 1;
											if (curNode.AvgType == AvgTypes.TenMin) typeNumber = 2;
											else if (curNode.AvgType == AvgTypes.TwoHours) typeNumber = 3;

											if (avgSelectedArchives == string.Empty)
												avgSelectedArchives += typeNumber;
											else avgSelectedArchives += ("|" + typeNumber);
										}
									}

									break;
								case MeasureType.DNS:  // there can be only one DNS archive
									if ((typeNode.Nodes[0 /*iDns*/] as CheckTreeNode).IsChecked())
									{
										dnsSelectedArchives = "1";
									}
									else dnsSelectedArchives = "0";
									break;
							}
						}

						if (pqpSelectedArchives == string.Empty) pqpSelectedArchives = "EMPTY";
						if (avgSelectedArchives == string.Empty) avgSelectedArchives = "EMPTY";
						if (dnsSelectedArchives == string.Empty) dnsSelectedArchives = "0";

						sw.WriteLine(pqpSelectedArchives);
						sw.WriteLine(avgSelectedArchives);
						sw.WriteLine(dnsSelectedArchives);
					}
				}
				catch (IOException ioex)
				{
					EmService.DumpException(ioex, "Error in bwReader_RunWorkerCompleted() IO:");
					MessageBox.Show("Unable to write archive list to the cofiguration file", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					return;
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error in bwReader_RunWorkerCompleted() 2:");
					TurnControlsForReading(false, false,
						new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
					MessageBox.Show("Error 1234", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					throw;
				}
				finally
				{
					if (sw != null) sw.Close();
				}

				bwReaderArchives_ = new BackgroundWorker();
				bwReaderArchives_.WorkerReportsProgress = true;
				bwReaderArchives_.WorkerSupportsCancellation = true;
				bwReaderArchives_.DoWork += bwReaderArchives_DoWork;
				bwReaderArchives_.ProgressChanged += bwReaderArchives_ProgressChanged;
				bwReaderArchives_.RunWorkerCompleted += bwReaderArchives_RunWorkerCompleted;

				readerProcessManager_ = new ReaderProcessManager(settings_, bwReaderArchives_, this.Handle, pipeName);

				bwReaderArchives_.RunWorkerAsync();
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in bwReader_RunWorkerCompleted() 2: " + emx.Message);
				TurnControlsForReading(false, false,
					new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
				MessageBox.Show("Error 1235", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bwReader_RunWorkerCompleted() 3: ");
				TurnControlsForReading(false, false,
					new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));
				MessageBox.Show("Error 1236", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				throw;
			}
		}

		private void bwReaderArchivesList_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				//if (settings_.CurrentLanguage == "ru")
				//{
				//    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
				//    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
				//}
				//else
				//{
				//    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				//    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
				//}

				readerEtPQP_A_.Run(ref e);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwReader_DoWork():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//throw;  unhandled exception
			}
		}

		private void bwReaderArchives_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			byte cntArchives = (byte)(e.ProgressPercentage & 0xFF);
			byte curArchive = (byte)((e.ProgressPercentage >> 8) & 0xFF);
			byte percent = (byte)((e.ProgressPercentage >> 16) & 0xFF);

			tsLabelOperation.Text = string.Format("Reading {0} archive from {1}   ", curArchive, cntArchives);
			tsProgressReading.Value = percent;
		}

		private void bwReaderArchives_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				TurnControlsForReading(false, false,
					new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_ready"));

				ExchangeResult res = ExchangeResult.NONE;
				if (e.Error != null)
				{
					EmService.WriteToLogFailed("Error in bwReader_RunWorkerCompleted() 1: " + e.Error.Message);
				}
				else if (e.Cancelled)
				{
					MessageBox.Show("Reading was aborted by user", "Abort",
						MessageBoxButtons.OK, MessageBoxIcon.Stop);
					return;
				}
				else if (e.Result != null) res = (ExchangeResult)e.Result;

				settings_.LoadSettings();
				// если завершили неуспешно
				if (res == ExchangeResult.DISCONNECT)
				{
					MessageBox.Show("Connection with the device was interrupted", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				if (res != ExchangeResult.OK)
				{
					MessageBox.Show("There was some error while reading archives", "Error", 
						MessageBoxButtons.OK, MessageBoxIcon.Error);

					StoredArchivesInfo.DeleteTemporaryArchiveFiles(settings_.LastPathToStoreArchives);
					return;
				}
				else
				{
					EmService.WriteToLogGeneral("Archives were read successfully");
					StoredArchivesInfo.RenameTemporaryArchiveFiles(settings_.LastPathToStoreArchives);
				}

				StartLoadingListOfArchives();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in bwReaderArchives_RunWorkerCompleted() 2: ");
				throw;
			}
		}

		private void bwReaderArchives_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				//if (settings_.CurrentLanguage == "ru")
				//{
				//    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
				//    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
				//}
				//else
				//{
				//    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				//    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
				//}

				curOperation_ = CurrentOperation.READ_ARCHIVES;
				TurnControlsForReading(true, false, 
					new ResourceManager("MainInterface.emstrings", this.GetType().Assembly).GetString("str_read_archives"));
				tsProgressReading.Value = 0;
				readerProcessManager_.Run(ref e);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in bwReaderArchives_DoWork():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//throw;  unhandled exception
			}
		}

		private void TurnControlsForReading(bool readingExecute, bool progressMarquee, string text)
		{
			tsLabelOperation.Text = text;

			if (readingExecute)
			{
				tpbUsb.ButtonColorTop = Color.LightCyan;
				tpbUsb.ButtonColorBottom = Color.Lime;
				tsProgressReading.Visible = true;
				if(progressMarquee)
					tsProgressReading.Style = ProgressBarStyle.Marquee;
				else tsProgressReading.Style = ProgressBarStyle.Blocks;
				tsddbtnCancel.Visible = true;
				cmiLoadFromDevice.Enabled = false;
			}
			else
			{
				tpbUsb.ButtonColorTop = Color.LightPink;
				tpbUsb.ButtonColorBottom = Color.Red;
				tsProgressReading.Style = ProgressBarStyle.Blocks;
				tsProgressReading.Visible = false;
				tsddbtnCancel.Visible = false;
				cmiLoadFromDevice.Enabled = true;

				curOperation_ = CurrentOperation.NONE;
			}
		}

		#endregion

		#region Search archive files

		// search archives stored on the computer
		private void bwSearchFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			storedArchivesInfo_.LoadArchivesInfo(ref e);
		}

		private void bwSearchFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(!e.Cancelled)
				frmWait_.CloseWithResultOK();
			frmArchiveList_.SetStoredDevicesInfo(ref storedArchivesInfo_);
		}

		private void bwSearchFiles_ProgressChanged(object sender,
													ProgressChangedEventArgs e)
		{
			frmWait_.SetProgressBarValue(e.ProgressPercentage);
		}

		private void StartLoadingListOfArchives()
		{
			try
			{
				bwSearchFiles_ = new BackgroundWorker();
				bwSearchFiles_.WorkerReportsProgress = true;
				bwSearchFiles_.WorkerSupportsCancellation = true;
				bwSearchFiles_.DoWork += bwSearchFiles_DoWork;
				bwSearchFiles_.ProgressChanged += bwSearchFiles_ProgressChanged;
				bwSearchFiles_.RunWorkerCompleted += bwSearchFiles_RunWorkerCompleted;
				storedArchivesInfo_.SetBackgroundWorker(ref bwSearchFiles_);

				bwSearchFiles_.RunWorkerAsync();

				frmWait_ = FormWaitSearchArchives.Instance(true);
				if (frmWait_.ShowDialog() == DialogResult.Cancel)
				{
					bwSearchFiles_.CancelAsync();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in StartLoadingListOfArchives");
				throw;
			}
		}

		#endregion

		#region Service methods

		public static void LastExceptionHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception ex = (Exception)args.ExceptionObject;
			EmService.DumpException(ex, "Information from LastExceptionHandler:");
		}

		#endregion
	}
}
