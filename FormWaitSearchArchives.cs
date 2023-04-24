using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MainInterface
{
	public partial class FormWaitSearchArchives : Form
	{
		private static FormWaitSearchArchives instance_;

		public FormWaitSearchArchives(bool cancelEnable)
		{
			InitializeComponent();

			pbtnCancel.Visible = cancelEnable;
		}

		public static FormWaitSearchArchives Instance(bool cancelEnable)
		{
			if (instance_ == null)
			{
				instance_ = new FormWaitSearchArchives(cancelEnable);
			}
			return instance_;
		}

		public void DeleteInstance()
		{
			instance_ = null;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
			instance_ = null;
		}

		private void FormWait_FormClosing(object sender, FormClosingEventArgs e)
		{
			instance_ = null;
		}

		public void CloseWithResultOK()
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
			instance_ = null;
		}

		public void SetProgressBarValue(int value)
		{
			progressBar.Value = value;
		}
	}
}
