using System.Drawing;

namespace MainInterface
{
	partial class FormMain
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.dockPanelMain = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.tsLabelOperation = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsProgressReading = new System.Windows.Forms.ToolStripProgressBar();
			this.tsddbtnCancel = new System.Windows.Forms.ToolStripDropDownButton();
			this.tsmiAbortReading = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMain = new System.Windows.Forms.ToolStrip();
			this.cmsMenuFile = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cmiOpenArchive = new System.Windows.Forms.ToolStripMenuItem();
			this.cmiLoadFromDevice = new System.Windows.Forms.ToolStripMenuItem();
			this.cmiSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.cmiExit = new System.Windows.Forms.ToolStripMenuItem();
			this.cmsMenuService = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cmiSettings = new System.Windows.Forms.ToolStripMenuItem();
			this.cmiReloadArchiveInfoPanel = new System.Windows.Forms.ToolStripMenuItem();
			this.cmsMenuWindow = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cmiCascade = new System.Windows.Forms.ToolStripMenuItem();
			this.cmiTileHorizontal = new System.Windows.Forms.ToolStripMenuItem();
			this.cmiTileVertical = new System.Windows.Forms.ToolStripMenuItem();
			this.cmsMenuHelp = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cmiAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.tpbFile = new PulseButtonLib.PulseButton();
			this.tpbService = new PulseButtonLib.PulseButton();
			this.tpbWindow = new PulseButtonLib.PulseButton();
			this.tpbHelp = new PulseButtonLib.PulseButton();
			this.tpbUsb = new PulseButtonLib.PulseButton();
			this.tpbInternet = new PulseButtonLib.PulseButton();
			this.tpbWifi = new PulseButtonLib.PulseButton();
			this.tpbEnglish = new PulseButtonLib.PulseButton();
			this.tpbRussian = new PulseButtonLib.PulseButton();
			this.statusStrip.SuspendLayout();
			this.cmsMenuFile.SuspendLayout();
			this.cmsMenuService.SuspendLayout();
			this.cmsMenuWindow.SuspendLayout();
			this.cmsMenuHelp.SuspendLayout();
			this.SuspendLayout();
			// 
			// dockPanelMain
			// 
			resources.ApplyResources(this.dockPanelMain, "dockPanelMain");
			this.dockPanelMain.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.dockPanelMain.DockBackColor = System.Drawing.SystemColors.AppWorkspace;
			this.dockPanelMain.DockBottomPortion = 150D;
			this.dockPanelMain.DockLeftPortion = 230D;
			this.dockPanelMain.DockRightPortion = 230D;
			this.dockPanelMain.DockTopPortion = 150D;
			this.dockPanelMain.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.SystemMdi;
			this.dockPanelMain.Name = "dockPanelMain";
			this.dockPanelMain.RightToLeftLayout = true;
			// 
			// statusStrip
			// 
			resources.ApplyResources(this.statusStrip, "statusStrip");
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsLabelOperation,
            this.tsProgressReading,
            this.tsddbtnCancel});
			this.statusStrip.Name = "statusStrip";
			// 
			// tsLabelOperation
			// 
			resources.ApplyResources(this.tsLabelOperation, "tsLabelOperation");
			this.tsLabelOperation.Name = "tsLabelOperation";
			// 
			// tsProgressReading
			// 
			resources.ApplyResources(this.tsProgressReading, "tsProgressReading");
			this.tsProgressReading.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.tsProgressReading.ForeColor = System.Drawing.Color.Lime;
			this.tsProgressReading.Name = "tsProgressReading";
			// 
			// tsddbtnCancel
			// 
			resources.ApplyResources(this.tsddbtnCancel, "tsddbtnCancel");
			this.tsddbtnCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsddbtnCancel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAbortReading});
			this.tsddbtnCancel.Name = "tsddbtnCancel";
			// 
			// tsmiAbortReading
			// 
			resources.ApplyResources(this.tsmiAbortReading, "tsmiAbortReading");
			this.tsmiAbortReading.Name = "tsmiAbortReading";
			this.tsmiAbortReading.Click += new System.EventHandler(this.tsmiAbortReading_Click);
			// 
			// toolStripMain
			// 
			resources.ApplyResources(this.toolStripMain, "toolStripMain");
			this.toolStripMain.BackColor = System.Drawing.SystemColors.ButtonFace;
			this.toolStripMain.Name = "toolStripMain";
			// 
			// cmsMenuFile
			// 
			resources.ApplyResources(this.cmsMenuFile, "cmsMenuFile");
			this.cmsMenuFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiOpenArchive,
            this.cmiLoadFromDevice,
            this.cmiSeparator1,
            this.cmiExit});
			this.cmsMenuFile.Name = "cmsMenuFile";
			// 
			// cmiOpenArchive
			// 
			resources.ApplyResources(this.cmiOpenArchive, "cmiOpenArchive");
			this.cmiOpenArchive.Name = "cmiOpenArchive";
			this.cmiOpenArchive.Click += new System.EventHandler(this.cmiOpenArchive_Click);
			// 
			// cmiLoadFromDevice
			// 
			resources.ApplyResources(this.cmiLoadFromDevice, "cmiLoadFromDevice");
			this.cmiLoadFromDevice.Name = "cmiLoadFromDevice";
			this.cmiLoadFromDevice.Click += new System.EventHandler(this.cmiLoadFromDevice_Click);
			// 
			// cmiSeparator1
			// 
			resources.ApplyResources(this.cmiSeparator1, "cmiSeparator1");
			this.cmiSeparator1.Name = "cmiSeparator1";
			// 
			// cmiExit
			// 
			resources.ApplyResources(this.cmiExit, "cmiExit");
			this.cmiExit.Name = "cmiExit";
			this.cmiExit.Click += new System.EventHandler(this.cmiExit_Click);
			// 
			// cmsMenuService
			// 
			resources.ApplyResources(this.cmsMenuService, "cmsMenuService");
			this.cmsMenuService.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiSettings,
            this.cmiReloadArchiveInfoPanel});
			this.cmsMenuService.Name = "cmsMenuService";
			// 
			// cmiSettings
			// 
			resources.ApplyResources(this.cmiSettings, "cmiSettings");
			this.cmiSettings.Name = "cmiSettings";
			this.cmiSettings.Click += new System.EventHandler(this.cmiSettings_Click);
			// 
			// cmiReloadArchiveInfoPanel
			// 
			resources.ApplyResources(this.cmiReloadArchiveInfoPanel, "cmiReloadArchiveInfoPanel");
			this.cmiReloadArchiveInfoPanel.Name = "cmiReloadArchiveInfoPanel";
			this.cmiReloadArchiveInfoPanel.Click += new System.EventHandler(this.cmiReloadArchiveInfoPanel_Click);
			// 
			// cmsMenuWindow
			// 
			resources.ApplyResources(this.cmsMenuWindow, "cmsMenuWindow");
			this.cmsMenuWindow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiCascade,
            this.cmiTileHorizontal,
            this.cmiTileVertical});
			this.cmsMenuWindow.Name = "cmsMenuWindow";
			// 
			// cmiCascade
			// 
			resources.ApplyResources(this.cmiCascade, "cmiCascade");
			this.cmiCascade.Name = "cmiCascade";
			this.cmiCascade.Click += new System.EventHandler(this.cmiCascade_Click);
			// 
			// cmiTileHorizontal
			// 
			resources.ApplyResources(this.cmiTileHorizontal, "cmiTileHorizontal");
			this.cmiTileHorizontal.Name = "cmiTileHorizontal";
			this.cmiTileHorizontal.Click += new System.EventHandler(this.cmiTileHorizontal_Click);
			// 
			// cmiTileVertical
			// 
			resources.ApplyResources(this.cmiTileVertical, "cmiTileVertical");
			this.cmiTileVertical.Name = "cmiTileVertical";
			this.cmiTileVertical.Click += new System.EventHandler(this.cmiTileVertical_Click);
			// 
			// cmsMenuHelp
			// 
			resources.ApplyResources(this.cmsMenuHelp, "cmsMenuHelp");
			this.cmsMenuHelp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiAbout});
			this.cmsMenuHelp.Name = "cmsMenuHelp";
			// 
			// cmiAbout
			// 
			resources.ApplyResources(this.cmiAbout, "cmiAbout");
			this.cmiAbout.Name = "cmiAbout";
			// 
			// tpbFile
			// 
			resources.ApplyResources(this.tpbFile, "tpbFile");
			this.tpbFile.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbFile.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbFile.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbFile.Name = "tpbFile";
			this.tpbFile.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbFile.PulseSpeed = 0.3F;
			this.tpbFile.PulseWidth = 5;
			this.tpbFile.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbFile.UseVisualStyleBackColor = true;
			this.tpbFile.Click += new System.EventHandler(this.tpbFile_Click);
			// 
			// tpbService
			// 
			resources.ApplyResources(this.tpbService, "tpbService");
			this.tpbService.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbService.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbService.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbService.Name = "tpbService";
			this.tpbService.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbService.PulseSpeed = 0.3F;
			this.tpbService.PulseWidth = 5;
			this.tpbService.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbService.UseVisualStyleBackColor = true;
			this.tpbService.Click += new System.EventHandler(this.tpbService_Click);
			// 
			// tpbWindow
			// 
			resources.ApplyResources(this.tpbWindow, "tpbWindow");
			this.tpbWindow.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbWindow.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbWindow.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbWindow.Name = "tpbWindow";
			this.tpbWindow.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbWindow.PulseSpeed = 0.3F;
			this.tpbWindow.PulseWidth = 5;
			this.tpbWindow.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbWindow.UseVisualStyleBackColor = true;
			this.tpbWindow.Click += new System.EventHandler(this.tpbWindow_Click);
			// 
			// tpbHelp
			// 
			resources.ApplyResources(this.tpbHelp, "tpbHelp");
			this.tpbHelp.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbHelp.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbHelp.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbHelp.Name = "tpbHelp";
			this.tpbHelp.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbHelp.PulseSpeed = 0.3F;
			this.tpbHelp.PulseWidth = 5;
			this.tpbHelp.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbHelp.UseVisualStyleBackColor = true;
			this.tpbHelp.Click += new System.EventHandler(this.tpbHelp_Click);
			// 
			// tpbUsb
			// 
			resources.ApplyResources(this.tpbUsb, "tpbUsb");
			this.tpbUsb.ButtonColorBottom = System.Drawing.Color.Red;
			this.tpbUsb.ButtonColorTop = System.Drawing.Color.Pink;
			this.tpbUsb.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbUsb.Name = "tpbUsb";
			this.tpbUsb.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbUsb.PulseSpeed = 0.3F;
			this.tpbUsb.PulseWidth = 5;
			this.tpbUsb.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbUsb.UseVisualStyleBackColor = true;
			// 
			// tpbInternet
			// 
			resources.ApplyResources(this.tpbInternet, "tpbInternet");
			this.tpbInternet.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbInternet.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbInternet.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbInternet.Name = "tpbInternet";
			this.tpbInternet.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbInternet.PulseSpeed = 0.3F;
			this.tpbInternet.PulseWidth = 5;
			this.tpbInternet.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbInternet.UseVisualStyleBackColor = true;
			// 
			// tpbWifi
			// 
			resources.ApplyResources(this.tpbWifi, "tpbWifi");
			this.tpbWifi.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbWifi.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbWifi.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbWifi.Name = "tpbWifi";
			this.tpbWifi.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbWifi.PulseSpeed = 0.3F;
			this.tpbWifi.PulseWidth = 5;
			this.tpbWifi.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbWifi.UseVisualStyleBackColor = true;
			// 
			// tpbEnglish
			// 
			resources.ApplyResources(this.tpbEnglish, "tpbEnglish");
			this.tpbEnglish.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbEnglish.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbEnglish.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbEnglish.Image = global::MainInterface.Properties.Resources.eng;
			this.tpbEnglish.Name = "tpbEnglish";
			this.tpbEnglish.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbEnglish.PulseSpeed = 0.3F;
			this.tpbEnglish.PulseWidth = 5;
			this.tpbEnglish.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbEnglish.UseVisualStyleBackColor = true;
			this.tpbEnglish.Click += new System.EventHandler(this.tpbEnglish_Click);
			// 
			// tpbRussian
			// 
			resources.ApplyResources(this.tpbRussian, "tpbRussian");
			this.tpbRussian.ButtonColorBottom = System.Drawing.SystemColors.ButtonFace;
			this.tpbRussian.ButtonColorTop = System.Drawing.SystemColors.ButtonHighlight;
			this.tpbRussian.ForeColor = System.Drawing.SystemColors.ControlText;
			this.tpbRussian.Image = global::MainInterface.Properties.Resources.rus;
			this.tpbRussian.Name = "tpbRussian";
			this.tpbRussian.PulseColor = System.Drawing.SystemColors.ControlDark;
			this.tpbRussian.PulseSpeed = 0.3F;
			this.tpbRussian.PulseWidth = 5;
			this.tpbRussian.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.tpbRussian.UseVisualStyleBackColor = true;
			this.tpbRussian.Click += new System.EventHandler(this.tpbRussian_Click);
			// 
			// FormMain
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tpbEnglish);
			this.Controls.Add(this.tpbRussian);
			this.Controls.Add(this.tpbWifi);
			this.Controls.Add(this.tpbInternet);
			this.Controls.Add(this.tpbUsb);
			this.Controls.Add(this.tpbHelp);
			this.Controls.Add(this.tpbWindow);
			this.Controls.Add(this.tpbService);
			this.Controls.Add(this.tpbFile);
			this.Controls.Add(this.toolStripMain);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.dockPanelMain);
			this.IsMdiContainer = true;
			this.Name = "FormMain";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
			this.Load += new System.EventHandler(this.FormMain_Load);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.cmsMenuFile.ResumeLayout(false);
			this.cmsMenuService.ResumeLayout(false);
			this.cmsMenuWindow.ResumeLayout(false);
			this.cmsMenuHelp.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanelMain;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStrip toolStripMain;
		private System.Windows.Forms.ContextMenuStrip cmsMenuFile;
		private System.Windows.Forms.ToolStripMenuItem cmiOpenArchive;
		private System.Windows.Forms.ToolStripSeparator cmiSeparator1;
		private System.Windows.Forms.ToolStripMenuItem cmiExit;
		private System.Windows.Forms.ContextMenuStrip cmsMenuService;
		private System.Windows.Forms.ToolStripMenuItem cmiSettings;
		private System.Windows.Forms.ContextMenuStrip cmsMenuWindow;
		private System.Windows.Forms.ToolStripMenuItem cmiCascade;
		private System.Windows.Forms.ToolStripMenuItem cmiTileHorizontal;
		private System.Windows.Forms.ToolStripMenuItem cmiTileVertical;
		private System.Windows.Forms.ContextMenuStrip cmsMenuHelp;
		private System.Windows.Forms.ToolStripMenuItem cmiAbout;
		private PulseButtonLib.PulseButton tpbFile;
		private PulseButtonLib.PulseButton tpbService;
		private PulseButtonLib.PulseButton tpbWindow;
		private PulseButtonLib.PulseButton tpbHelp;
		private PulseButtonLib.PulseButton tpbUsb;
		private PulseButtonLib.PulseButton tpbInternet;
		private PulseButtonLib.PulseButton tpbWifi;
		private System.Windows.Forms.ToolStripMenuItem cmiReloadArchiveInfoPanel;
		private System.Windows.Forms.ToolStripMenuItem cmiLoadFromDevice;
		private System.Windows.Forms.ToolStripStatusLabel tsLabelOperation;
		private System.Windows.Forms.ToolStripProgressBar tsProgressReading;
		private System.Windows.Forms.ToolStripDropDownButton tsddbtnCancel;
		private System.Windows.Forms.ToolStripMenuItem tsmiAbortReading;
		private PulseButtonLib.PulseButton tpbEnglish;
		private PulseButtonLib.PulseButton tpbRussian;
	}
}