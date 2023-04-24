using System;
using System.Windows.Forms;
using System.Threading;

namespace MainInterface
{
	public partial class FormDocPQPReportSaveDialog : Form
	{
		private bool enableNumber_;

		public FormDocPQPReportSaveDialog(System.Globalization.CultureInfo culture, bool enableNumber)
		{
			Thread.CurrentThread.CurrentUICulture = culture;
			InitializeComponent();

			enableNumber_ = enableNumber;
		}

		private void FormDocPQPReportSaveDialog_Shown(object sender, EventArgs e)
		{
			txtFileName.Focus();
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			if (fd.ShowDialog(this) != DialogResult.OK) return;
			txtFileName.Text = fd.FileName;
		}

		private void txtFileName_TextChanged(object sender, EventArgs e)
		{
			btnSave.Enabled = txtFileName.Text.Length > 0;
		}

		private void FormDocPQPReportSaveDialog_Load(object sender, EventArgs e)
		{
			txtReportNumber.Enabled = enableNumber_;
			txtAppendixNumber.Enabled = enableNumber_;
		}
	}
}