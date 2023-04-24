namespace MainInterface
{
	partial class FormWaitSearchArchives
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWaitSearchArchives));
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.labelMain = new System.Windows.Forms.Label();
			this.pbtnCancel = new PulseButtonLib.PulseButton();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			resources.ApplyResources(this.progressBar, "progressBar");
			this.progressBar.Name = "progressBar";
			// 
			// labelMain
			// 
			resources.ApplyResources(this.labelMain, "labelMain");
			this.labelMain.BackColor = System.Drawing.Color.Transparent;
			this.labelMain.ForeColor = System.Drawing.Color.Yellow;
			this.labelMain.Name = "labelMain";
			// 
			// pbtnCancel
			// 
			resources.ApplyResources(this.pbtnCancel, "pbtnCancel");
			this.pbtnCancel.ButtonColorBottom = System.Drawing.Color.MediumBlue;
			this.pbtnCancel.ButtonColorTop = System.Drawing.Color.LightSkyBlue;
			this.pbtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.pbtnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.pbtnCancel.Name = "pbtnCancel";
			this.pbtnCancel.PulseColor = System.Drawing.Color.LightBlue;
			this.pbtnCancel.PulseSpeed = 0.3F;
			this.pbtnCancel.PulseWidth = 15;
			this.pbtnCancel.ShapeType = PulseButtonLib.PulseButton.Shape.Rectangle;
			this.pbtnCancel.UseVisualStyleBackColor = true;
			this.pbtnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// FormWaitSearchArchives
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(49)))), ((int)(((byte)(81)))));
			this.BackgroundImage = global::MainInterface.Properties.Resources.title;
			this.Controls.Add(this.pbtnCancel);
			this.Controls.Add(this.labelMain);
			this.Controls.Add(this.progressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormWaitSearchArchives";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWait_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label labelMain;
		private PulseButtonLib.PulseButton pbtnCancel;
	}
}