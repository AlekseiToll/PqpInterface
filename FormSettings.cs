using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using EmServiceLib;

namespace MainInterface
{
	public partial class FormSettings : Form
	{
		private EmSettings settings_;
		private bool bSettingsWasChanged_ = false;
		// make the separate variable for directories list to know if it was changed.
		// reloading of archives info takes a lot of time so we will do it only
		// if it's really necessary
		private bool bDirsListWasChanged_ = false;

		private BackgroundWorker bwSearchFiles_ = new BackgroundWorker();
		private FormWaitSearchArchives frmWait_ = null;
		// new directories in which we've found archive files
		private List<string> listAdditionalDirs_ = new List<string>(); 

		public FormSettings(ref EmSettings settings)
		{
			InitializeComponent();

			settings_ = settings.Clone();
		}

		#region Form event handlers

		private void cmbFloatSigns_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (settings_.FloatSigns == Convert.ToInt32(cmbFloatSigns.SelectedItem.ToString()))
				{
					return;
				}

				if (cmbFloatSigns.SelectedIndex > -1)
				{
					settings_.FloatSigns = Convert.ToInt32(cmbFloatSigns.SelectedItem.ToString());
					float f = 12.345678F;
					txtFloatFormatExample.Text = f.ToString(settings_.FloatFormat);

					if (settings_.SettingsChanged)
					{
						//if (this.settings_.CurrentLanguage == "en")
						//    MessageBox.Show("Resolution of displaying fractions will be changed\nonly after restart of the application.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
						//else if (this.settings_.CurrentLanguage == "ru")
						//    MessageBox.Show("Точность отображения дробных величин изменится\nтолько после повторного открытия архива", "К сведению", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					bSettingsWasChanged_ = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in cmbFloatSigns_SelectedIndexChanged():");
				throw;
			}
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			try
			{
				dlgFolderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
				if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
				{
					lbArchiveStorePath.Items.Add(dlgFolderBrowser.SelectedPath);
					bSettingsWasChanged_ = true;
					bDirsListWasChanged_ = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings::btnBrowse_Click");
				throw;
			}
		}

		private void FormSettings_Load(object sender, EventArgs e)
		{
			try
			{
				settings_.LoadSettings();

				// general
				try
				{
					cmbFloatSigns.SelectedIndex = cmbFloatSigns.FindString(settings_.FloatSigns.ToString());
					float f = 12.345678F;
					txtFloatFormatExample.Text = f.ToString(settings_.FloatFormat);
				}
				catch
				{
					cmbFloatSigns.SelectedIndex = -1;
				}

				foreach (var path in settings_.PathToStoredArchives)
				{
					lbArchiveStorePath.Items.Add(path);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings_Load");
				throw;
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
				if (bSettingsWasChanged_)
				{
					if (bDirsListWasChanged_)
					{
						settings_.PathToStoredArchives = new string[lbArchiveStorePath.Items.Count];
						for (int iItem = 0; iItem < lbArchiveStorePath.Items.Count; ++iItem)
						{
							settings_.PathToStoredArchives[iItem] =
								lbArchiveStorePath.Items[iItem].ToString();
						}
					}

					settings_.SaveSettings();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings::btnOk_Click");
				throw;
			}
		}

		private void pbtnDeletePath_Click(object sender, EventArgs e)
		{
			try
			{
				if (lbArchiveStorePath.SelectedIndex != -1)
				{
					lbArchiveStorePath.Items.RemoveAt(lbArchiveStorePath.SelectedIndex);
					bSettingsWasChanged_ = true;
					bDirsListWasChanged_ = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings::pbtnDeletePath_Click");
				throw;
			}
		}

		private void pbtnSearchAllComp_Click(object sender, EventArgs e)
		{
			StartLoadingListOfArchives();
		}

		private void lbArchiveStorePath_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lbArchiveStorePath.SelectedIndex != -1)
				pbtnDeletePath.Enabled = true;
			else pbtnDeletePath.Enabled = false;
		}

		#endregion

		#region Search new directories

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

				bwSearchFiles_.RunWorkerAsync();

				frmWait_ = FormWaitSearchArchives.Instance(true);
				if (frmWait_.ShowDialog() == DialogResult.Cancel)
				{
					bwSearchFiles_.CancelAsync();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings::StartLoadingListOfArchives");
				throw;
			}
		}

		private void bwSearchFiles_ProgressChanged(object sender,
													ProgressChangedEventArgs e)
		{
			frmWait_.SetProgressBarValue(e.ProgressPercentage);
		}

		private void bwSearchFiles_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				listAdditionalDirs_.Clear();

				DriveInfo[] allDrives = DriveInfo.GetDrives();
				for (int iDisk = 0; iDisk < allDrives.Length; ++iDisk)
				{
					if (allDrives[iDisk].IsReady)
					{
						EmService.SearchAdditionalDirs(allDrives[iDisk].RootDirectory.Name,
						           new string[] {"*.pqp", "*.avg", "*.dns"},
						           ref listAdditionalDirs_, ref e,
								   ref bwSearchFiles_, 
								   settings_.PathToStoredArchives);
					}
					else
					{
						EmService.WriteToLogFailed("Disk is not ready: " + allDrives[iDisk].Name);
					}
					bwSearchFiles_.ReportProgress((iDisk + 1) * 100 / allDrives.Length);
				}

				if (bwSearchFiles_.CancellationPending)
				{
					e.Cancel = true;
					e.Result = false;
					return;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings::bwSearchFiles_DoWork");
				throw;
			}
		}

		private void bwSearchFiles_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				if(!e.Cancelled)
					frmWait_.CloseWithResultOK();

				if (listAdditionalDirs_.Count > 0)
				{
					string[] tmpDirs = new string[settings_.PathToStoredArchives.Length +
					                              listAdditionalDirs_.Count];
					Array.Copy(settings_.PathToStoredArchives, tmpDirs, settings_.PathToStoredArchives.Length);
					Array.Copy(listAdditionalDirs_.ToArray(), 0, tmpDirs, settings_.PathToStoredArchives.Length,
					           listAdditionalDirs_.Count);
					settings_.PathToStoredArchives = tmpDirs;

					bSettingsWasChanged_ = true;
					bDirsListWasChanged_ = true;

					lbArchiveStorePath.Items.Clear();
					foreach (var path in settings_.PathToStoredArchives)
					{
						lbArchiveStorePath.Items.Add(path);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormSettings::bwSearchFiles_RunWorkerCompleted");
				throw;
			}
		}

		#endregion

		#region Properties

		public bool DirsListWasChanged
		{
			get { return bDirsListWasChanged_; }
		}

		#endregion
	}
}
