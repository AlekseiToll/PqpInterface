namespace MainInterface
{
	partial class FormArchivesList
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
			Morell.GroupPanel.TabPageCollection tabPageCollection1 = new Morell.GroupPanel.TabPageCollection();
			this.groupPanel = new Morell.GroupPanel.GroupPanel();
			this.SuspendLayout();
			// 
			// groupPanel
			// 
			this.groupPanel.BackColor = System.Drawing.SystemColors.Control;
			this.groupPanel.ColorLeft = System.Drawing.Color.Empty;
			this.groupPanel.ColorRight = System.Drawing.Color.Empty;
			this.groupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupPanel.DownImage = null;
			this.groupPanel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.groupPanel.HotTrack = true;
			this.groupPanel.HotTrackColor = System.Drawing.SystemColors.HotTrack;
			this.groupPanel.ImageList = null;
			this.groupPanel.Location = new System.Drawing.Point(0, 0);
			//this.groupPanel.MinimumSize = new System.Drawing.Size(180, 300);
			this.groupPanel.Name = "groupPanel";
			this.groupPanel.SelectedIndex = -1;
			this.groupPanel.Size = new System.Drawing.Size(287, 407);
			this.groupPanel.TabHeight = 40;
			this.groupPanel.TabIndex = 0;
			//this.groupPanel.TabPages = tabPageCollection1;
			this.groupPanel.TransparentColor = System.Drawing.Color.Magenta;
			this.groupPanel.UpImage = null;
			this.groupPanel.Load += new System.EventHandler(this.FormArchivesList_Load);
			// 
			// FormArchivesList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(287, 407);
			this.CloseButton = false;
			this.CloseButtonVisible = false;
			this.Controls.Add(this.groupPanel);
			this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft;
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.Name = "FormArchivesList";
			this.ResumeLayout(false);

		}

		#endregion

		private Morell.GroupPanel.GroupPanel groupPanel;
	}
}