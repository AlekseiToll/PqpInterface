using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

using EmServiceLib;
using FileAnalyzerLib;
using MainInterface.SavingInterface.CheckTreeView;
using DeviceIO;

namespace MainInterface.SavingInterface
{
	/// <summary>
	/// Summary description for FormDeviceExchange.
	/// </summary>
	public class FormDeviceExchange : Form
	{
		private IContainer components;

		private EmSettings settings_;

		//private DeviceCommonInfo devInfo_;
		private SplitContainer scMain;
		private GroupBox grbArchivesTreeView;
		private DeviceTreeView tvDeviceData;
		private GroupBox gbPathToStore;
		private TextBox tbStoragePath;
		private PulseButtonLib.PulseButton pbtnCancel;
		private PulseButtonLib.PulseButton pbtnOk;
		private PulseButtonLib.PulseButton pbtnBrowse;
		private FolderBrowserDialog dlgFolderBrowser;
		private ImageList imageListCheckbox;

        private bool bStopClose_ = false;

		//public FormDeviceExchange() {}

		public FormDeviceExchange(ref DeviceCommonInfo devInfo, ref EmSettings settings, ref StoredArchivesInfo storedArchivesInfo)
		{
			try
			{
				EmService.WriteToLogDebug("FormDeviceExchange::FormDeviceExchange enter");
				//
				// Required for Windows Form Designer support
				//
				InitializeComponent();
				EmService.WriteToLogDebug("FormDeviceExchange::FormDeviceExchange step 1");

				//
				// TO DO: Add any constructor code after InitializeComponent call
				//
				CheckForIllegalCrossThreadCalls = false;

				CheckTreeView.DeviceTreeView.NothingChecked +=
					new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_NothingChecked);
				CheckTreeView.DeviceTreeView.SomethingChecked +=
					new CheckTreeView.DeviceTreeView.CheckedHandler(DeviceTreeView_SomethingChecked);
				EmService.WriteToLogDebug("FormDeviceExchange::FormDeviceExchange step 2");

				//devInfo_ = devInfo;
				settings_ = settings;
				DeviceData.ImportDataFromContents(ref devInfo, ref storedArchivesInfo);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormDeviceExchange::FormDeviceExchange");
				throw;
			}
		}

		/// <summary>Clean up any resources being used</summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDeviceExchange));
			this.scMain = new System.Windows.Forms.SplitContainer();
			this.grbArchivesTreeView = new System.Windows.Forms.GroupBox();
			this.pbtnCancel = new PulseButtonLib.PulseButton();
			this.pbtnOk = new PulseButtonLib.PulseButton();
			this.gbPathToStore = new System.Windows.Forms.GroupBox();
			this.pbtnBrowse = new PulseButtonLib.PulseButton();
			this.tbStoragePath = new System.Windows.Forms.TextBox();
			this.dlgFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.imageListCheckbox = new System.Windows.Forms.ImageList(this.components);
			this.tvDeviceData = new MainInterface.SavingInterface.CheckTreeView.DeviceTreeView();
			((System.ComponentModel.ISupportInitialize)(this.scMain)).BeginInit();
			this.scMain.Panel1.SuspendLayout();
			this.scMain.Panel2.SuspendLayout();
			this.scMain.SuspendLayout();
			this.grbArchivesTreeView.SuspendLayout();
			this.gbPathToStore.SuspendLayout();
			this.SuspendLayout();
			// 
			// scMain
			// 
			resources.ApplyResources(this.scMain, "scMain");
			this.scMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.scMain.Name = "scMain";
			// 
			// scMain.Panel1
			// 
			this.scMain.Panel1.Controls.Add(this.grbArchivesTreeView);
			// 
			// scMain.Panel2
			// 
			this.scMain.Panel2.Controls.Add(this.pbtnCancel);
			this.scMain.Panel2.Controls.Add(this.pbtnOk);
			this.scMain.Panel2.Controls.Add(this.gbPathToStore);
			// 
			// grbArchivesTreeView
			// 
			resources.ApplyResources(this.grbArchivesTreeView, "grbArchivesTreeView");
			this.grbArchivesTreeView.Controls.Add(this.tvDeviceData);
			this.grbArchivesTreeView.Name = "grbArchivesTreeView";
			this.grbArchivesTreeView.TabStop = false;
			// 
			// pbtnCancel
			// 
			resources.ApplyResources(this.pbtnCancel, "pbtnCancel");
			this.pbtnCancel.ButtonColorBottom = System.Drawing.SystemColors.Control;
			this.pbtnCancel.ButtonColorTop = System.Drawing.SystemColors.HighlightText;
			this.pbtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.pbtnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pbtnCancel.Name = "pbtnCancel";
			this.pbtnCancel.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.pbtnCancel.PulseSpeed = 0.3F;
			this.pbtnCancel.PulseWidth = 7;
			this.pbtnCancel.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnCancel.UseVisualStyleBackColor = true;
			// 
			// pbtnOk
			// 
			resources.ApplyResources(this.pbtnOk, "pbtnOk");
			this.pbtnOk.ButtonColorBottom = System.Drawing.SystemColors.Control;
			this.pbtnOk.ButtonColorTop = System.Drawing.SystemColors.HighlightText;
			this.pbtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.pbtnOk.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pbtnOk.Name = "pbtnOk";
			this.pbtnOk.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.pbtnOk.PulseSpeed = 0.3F;
			this.pbtnOk.PulseWidth = 7;
			this.pbtnOk.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnOk.UseVisualStyleBackColor = true;
			this.pbtnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// gbPathToStore
			// 
			resources.ApplyResources(this.gbPathToStore, "gbPathToStore");
			this.gbPathToStore.Controls.Add(this.pbtnBrowse);
			this.gbPathToStore.Controls.Add(this.tbStoragePath);
			this.gbPathToStore.Name = "gbPathToStore";
			this.gbPathToStore.TabStop = false;
			// 
			// pbtnBrowse
			// 
			resources.ApplyResources(this.pbtnBrowse, "pbtnBrowse");
			this.pbtnBrowse.ButtonColorBottom = System.Drawing.SystemColors.Control;
			this.pbtnBrowse.ButtonColorTop = System.Drawing.SystemColors.HighlightText;
			this.pbtnBrowse.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pbtnBrowse.Name = "pbtnBrowse";
			this.pbtnBrowse.PulseColor = System.Drawing.SystemColors.ControlLightLight;
			this.pbtnBrowse.PulseSpeed = 0.3F;
			this.pbtnBrowse.PulseWidth = 8;
			this.pbtnBrowse.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnBrowse.UseVisualStyleBackColor = true;
			this.pbtnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// tbStoragePath
			// 
			resources.ApplyResources(this.tbStoragePath, "tbStoragePath");
			this.tbStoragePath.Name = "tbStoragePath";
			this.tbStoragePath.ReadOnly = true;
			// 
			// imageListCheckbox
			// 
			this.imageListCheckbox.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListCheckbox.ImageStream")));
			this.imageListCheckbox.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListCheckbox.Images.SetKeyName(0, "chbUnChecked.JPG");
			this.imageListCheckbox.Images.SetKeyName(1, "chbChecked.jpg");
			this.imageListCheckbox.Images.SetKeyName(2, "chbChecked2.JPG");
			this.imageListCheckbox.Images.SetKeyName(3, "chbDisabled.JPG");
			// 
			// tvDeviceData
			// 
			resources.ApplyResources(this.tvDeviceData, "tvDeviceData");
			this.tvDeviceData.HideSelection = false;
			this.tvDeviceData.ImageList = this.imageListCheckbox;
			this.tvDeviceData.Name = "tvDeviceData";
			// 
			// FormDeviceExchange
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			resources.ApplyResources(this, "$this");
			this.ControlBox = false;
			this.Controls.Add(this.scMain);
			this.KeyPreview = true;
			this.Name = "FormDeviceExchange";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDeviceExchange_FormClosing);
			this.Load += new System.EventHandler(this.FormDeviceExchange_Load);
			this.scMain.Panel1.ResumeLayout(false);
			this.scMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.scMain)).EndInit();
			this.scMain.ResumeLayout(false);
			this.grbArchivesTreeView.ResumeLayout(false);
			this.gbPathToStore.ResumeLayout(false);
			this.gbPathToStore.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		public new DialogResult ShowDialog()
		{
			DialogResult res = base.ShowDialog();
			Environment.CurrentDirectory = EmService.AppDirectory;
			return res;
		}

		private void DeviceTreeView_NothingChecked()
		{
			pbtnOk.Enabled = false;
		}

		private void DeviceTreeView_SomethingChecked()
		{
			pbtnOk.Enabled = true;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			try
			{
				dlgFolderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
				if (dlgFolderBrowser.ShowDialog() == DialogResult.OK)
				{
					tbStoragePath.Text = dlgFolderBrowser.SelectedPath;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormDevExchange::btnBrowse_Click");
				throw;
			}
		}

		//private void chbShowExisting_Click(object sender, EventArgs e)
		//{
		//    if (device_ == null) return;
		//}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
				if (settings_.LastPathToStoreArchives != tbStoragePath.Text)
				{
					settings_.LastPathToStoreArchives = tbStoragePath.Text;

					List<string> listPaths = new List<string>(settings_.PathToStoredArchives);
					if (!listPaths.Contains(settings_.LastPathToStoreArchives))
					{
						listPaths.Add(settings_.LastPathToStoreArchives);
						settings_.PathToStoredArchives = listPaths.ToArray();
					}
					settings_.SaveSettings();
				}

				#region Check if AVG archive too large (not used so far)

				//DeviceTreeView devTree = tvDeviceData;
                //if (devTree.Nodes.Count < 1) return;

				//int limitAVGHours = 2;

				//for (int iObj = 0; iObj < devTree.Nodes[0].Nodes.Count; iObj++)
				//{
				//    if (((RegistrationTreeNode)devTree.Nodes[0].Nodes[iObj]).CheckState !=
				//                CheckState.Unchecked)
				//    {
				//        RegistrationTreeNode objNodeTmp = (RegistrationTreeNode)devTree.Nodes[0].Nodes[iObj];
				//        foreach (MeasureTypeTreeNode typeNode in objNodeTmp.Nodes)
				//        {
				//            if (typeNode.MeasureType == MeasureType.AVG)
				//            {
                                //for (int iAvg = 0; iAvg < typeNode.Nodes.Count; iAvg++)
                                //{
                                    //MeasureTreeNode curMeasureNode =
                                    //    (typeNode.Nodes[iAvg] as MeasureTreeNode);
									// предупреждение, что архив слишком большой
									//if (curMeasureNode.Text.Contains("3 sec") && 
									//    curMeasureNode.CheckState != CheckState.Unchecked)
									//{
									//    TimeSpan ts = curMeasureNode.EndDateTime -
									//        curMeasureNode.StartDateTime;
									//    if (ts > new TimeSpan(limitAVGHours, 0, 0))
									//    {
									//        if (MessageBoxes.MsgArchiveMoreThanLimit(this, this, limitAVGHours) ==
									//            DialogResult.No)
									//        {
									//            bStopClose_ = true;
									//        }
									//        //реакцию юзера рассматриваем как действительную для
									//        //всех длинных архивов, поэтому выходим
									//        return;
									//    }
									//}
                                //}
                            //}
						//}
					//}
				//}	

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in FormDeviceExchange::btnOk_Click ");
				throw;
			}
		}

        private void frmDeviceExchange_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bStopClose_)
            {
                e.Cancel = true;
                bStopClose_ = false;
            }
		}

		private void FormDeviceExchange_Load(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(settings_.LastPathToStoreArchives))
				{
					EmService.WriteToLogDebug("FormDeviceExchange_Load var 1: " + settings_.LastPathToStoreArchives);
					tbStoragePath.Text = settings_.LastPathToStoreArchives;
				}
				else if ((settings_.PathToStoredArchives.Length > 0) &&
					(!string.IsNullOrEmpty(settings_.PathToStoredArchives[0])))
				{
					EmService.WriteToLogDebug("FormDeviceExchange_Load var 2: " + settings_.PathToStoredArchives[0]);
					tbStoragePath.Text = settings_.PathToStoredArchives[0];
				}
				else
				{
					string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
					md += "\\Archives";
					if (!Directory.Exists(md))
					{
						Directory.CreateDirectory(md);
					}
					EmService.WriteToLogDebug("FormDeviceExchange_Load var 3: " + md);
					tbStoragePath.Text = md;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in FormDeviceExchange_Load");
				throw;
			}
		}

		public DeviceTreeView DeviceData
		{
			get { return tvDeviceData; }
		}

		public string PathToStoreArchives
		{
			get { return tbStoragePath.Text; }
		}
	}
}
