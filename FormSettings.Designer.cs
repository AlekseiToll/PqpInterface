using System.Drawing;

namespace MainInterface
{
	partial class FormSettings
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
			this.dlgFolderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			this.tabControlMain = new System.Windows.Forms.TabControl();
			this.tpGeneral = new System.Windows.Forms.TabPage();
			this.gbDecimalType = new System.Windows.Forms.GroupBox();
			this.cmbFloatSigns = new System.Windows.Forms.ComboBox();
			this.txtFloatFormatExample = new System.Windows.Forms.TextBox();
			this.tpPath = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelArchiveDir = new System.Windows.Forms.Label();
			this.pbtnSearchAllComp = new PulseButtonLib.PulseButton();
			this.pbtnDeletePath = new PulseButtonLib.PulseButton();
			this.lbArchiveStorePath = new System.Windows.Forms.ListBox();
			this.pbtnBrowse = new PulseButtonLib.PulseButton();
			this.pbtnCancel = new PulseButtonLib.PulseButton();
			this.pbtnOk = new PulseButtonLib.PulseButton();
			this.tabControlMain.SuspendLayout();
			this.tpGeneral.SuspendLayout();
			this.gbDecimalType.SuspendLayout();
			this.tpPath.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlMain
			// 
			this.tabControlMain.Controls.Add(this.tpGeneral);
			this.tabControlMain.Controls.Add(this.tpPath);
			resources.ApplyResources(this.tabControlMain, "tabControlMain");
			this.tabControlMain.Name = "tabControlMain";
			this.tabControlMain.SelectedIndex = 0;
			// 
			// tpGeneral
			// 
			this.tpGeneral.BackColor = System.Drawing.Color.Transparent;
			this.tpGeneral.Controls.Add(this.gbDecimalType);
			resources.ApplyResources(this.tpGeneral, "tpGeneral");
			this.tpGeneral.Name = "tpGeneral";
			// 
			// gbDecimalType
			// 
			this.gbDecimalType.Controls.Add(this.cmbFloatSigns);
			this.gbDecimalType.Controls.Add(this.txtFloatFormatExample);
			resources.ApplyResources(this.gbDecimalType, "gbDecimalType");
			this.gbDecimalType.Name = "gbDecimalType";
			this.gbDecimalType.TabStop = false;
			// 
			// cmbFloatSigns
			// 
			this.cmbFloatSigns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbFloatSigns.FormattingEnabled = true;
			this.cmbFloatSigns.Items.AddRange(new object[] {
            resources.GetString("cmbFloatSigns.Items"),
            resources.GetString("cmbFloatSigns.Items1"),
            resources.GetString("cmbFloatSigns.Items2"),
            resources.GetString("cmbFloatSigns.Items3"),
            resources.GetString("cmbFloatSigns.Items4"),
            resources.GetString("cmbFloatSigns.Items5"),
            resources.GetString("cmbFloatSigns.Items6")});
			resources.ApplyResources(this.cmbFloatSigns, "cmbFloatSigns");
			this.cmbFloatSigns.Name = "cmbFloatSigns";
			this.cmbFloatSigns.SelectedIndexChanged += new System.EventHandler(this.cmbFloatSigns_SelectedIndexChanged);
			// 
			// txtFloatFormatExample
			// 
			this.txtFloatFormatExample.BackColor = System.Drawing.Color.AliceBlue;
			resources.ApplyResources(this.txtFloatFormatExample, "txtFloatFormatExample");
			this.txtFloatFormatExample.Name = "txtFloatFormatExample";
			this.txtFloatFormatExample.ReadOnly = true;
			// 
			// tpPath
			// 
			this.tpPath.BackColor = System.Drawing.Color.Transparent;
			this.tpPath.Controls.Add(this.panel1);
			resources.ApplyResources(this.tpPath, "tpPath");
			this.tpPath.Name = "tpPath";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Control;
			this.panel1.Controls.Add(this.labelArchiveDir);
			this.panel1.Controls.Add(this.pbtnSearchAllComp);
			this.panel1.Controls.Add(this.pbtnDeletePath);
			this.panel1.Controls.Add(this.lbArchiveStorePath);
			this.panel1.Controls.Add(this.pbtnBrowse);
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Name = "panel1";
			// 
			// labelArchiveDir
			// 
			resources.ApplyResources(this.labelArchiveDir, "labelArchiveDir");
			this.labelArchiveDir.Name = "labelArchiveDir";
			// 
			// pbtnSearchAllComp
			// 
			this.pbtnSearchAllComp.ButtonColorBottom = System.Drawing.SystemColors.Control;
			this.pbtnSearchAllComp.ButtonColorTop = System.Drawing.SystemColors.HighlightText;
			this.pbtnSearchAllComp.ForeColor = System.Drawing.SystemColors.ControlText;
			resources.ApplyResources(this.pbtnSearchAllComp, "pbtnSearchAllComp");
			this.pbtnSearchAllComp.Name = "pbtnSearchAllComp";
			this.pbtnSearchAllComp.PulseColor = System.Drawing.SystemColors.ControlLightLight;
			this.pbtnSearchAllComp.PulseSpeed = 0.3F;
			this.pbtnSearchAllComp.PulseWidth = 7;
			this.pbtnSearchAllComp.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnSearchAllComp.UseVisualStyleBackColor = true;
			this.pbtnSearchAllComp.Click += new System.EventHandler(this.pbtnSearchAllComp_Click);
			// 
			// pbtnDeletePath
			// 
			this.pbtnDeletePath.ButtonColorBottom = System.Drawing.SystemColors.Control;
			this.pbtnDeletePath.ButtonColorTop = System.Drawing.SystemColors.HighlightText;
			resources.ApplyResources(this.pbtnDeletePath, "pbtnDeletePath");
			this.pbtnDeletePath.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pbtnDeletePath.Name = "pbtnDeletePath";
			this.pbtnDeletePath.PulseColor = System.Drawing.SystemColors.ControlLightLight;
			this.pbtnDeletePath.PulseSpeed = 0.3F;
			this.pbtnDeletePath.PulseWidth = 8;
			this.pbtnDeletePath.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnDeletePath.UseVisualStyleBackColor = true;
			this.pbtnDeletePath.Click += new System.EventHandler(this.pbtnDeletePath_Click);
			// 
			// lbArchiveStorePath
			// 
			this.lbArchiveStorePath.FormattingEnabled = true;
			resources.ApplyResources(this.lbArchiveStorePath, "lbArchiveStorePath");
			this.lbArchiveStorePath.Name = "lbArchiveStorePath";
			this.lbArchiveStorePath.SelectedIndexChanged += new System.EventHandler(this.lbArchiveStorePath_SelectedIndexChanged);
			// 
			// pbtnBrowse
			// 
			this.pbtnBrowse.ButtonColorBottom = System.Drawing.SystemColors.Control;
			this.pbtnBrowse.ButtonColorTop = System.Drawing.SystemColors.HighlightText;
			this.pbtnBrowse.ForeColor = System.Drawing.SystemColors.ControlText;
			resources.ApplyResources(this.pbtnBrowse, "pbtnBrowse");
			this.pbtnBrowse.Name = "pbtnBrowse";
			this.pbtnBrowse.PulseColor = System.Drawing.SystemColors.ControlLightLight;
			this.pbtnBrowse.PulseSpeed = 0.3F;
			this.pbtnBrowse.PulseWidth = 8;
			this.pbtnBrowse.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnBrowse.UseVisualStyleBackColor = true;
			this.pbtnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
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
			// FormSettings
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ControlDark;
			this.Controls.Add(this.tabControlMain);
			this.Controls.Add(this.pbtnCancel);
			this.Controls.Add(this.pbtnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormSettings";
			this.Load += new System.EventHandler(this.FormSettings_Load);
			this.tabControlMain.ResumeLayout(false);
			this.tpGeneral.ResumeLayout(false);
			this.gbDecimalType.ResumeLayout(false);
			this.gbDecimalType.PerformLayout();
			this.tpPath.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FolderBrowserDialog dlgFolderBrowser;
		private PulseButtonLib.PulseButton pbtnOk;
		private PulseButtonLib.PulseButton pbtnCancel;
		private System.Windows.Forms.TabControl tabControlMain;
		private System.Windows.Forms.TabPage tpGeneral;
		private System.Windows.Forms.TabPage tpPath;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelArchiveDir;
		private PulseButtonLib.PulseButton pbtnSearchAllComp;
		private PulseButtonLib.PulseButton pbtnDeletePath;
		private System.Windows.Forms.ListBox lbArchiveStorePath;
		private PulseButtonLib.PulseButton pbtnBrowse;
		private System.Windows.Forms.GroupBox gbDecimalType;
		private System.Windows.Forms.ComboBox cmbFloatSigns;
		private System.Windows.Forms.TextBox txtFloatFormatExample;
	}
}