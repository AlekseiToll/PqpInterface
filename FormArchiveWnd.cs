using System;
using System.Collections.Generic;
using System.Windows.Forms;

using EmServiceLib;
using FileAnalyzerLib;

namespace MainInterface
{
	public partial class FormArchiveWnd : Form
	{
		private RegistrationInfo regInfo_;
		private EmSettings settings_;
		private FormMain frmMain_;

		private List<TabPage> listPqpPages_ = new List<TabPage>();
		private List<UserControlPqpPage> listPqpControls_ = new List<UserControlPqpPage>();

		public FormArchiveWnd(ref RegistrationInfo regInfo, ref EmSettings settings, ref FormMain frmMain)
		{
			InitializeComponent();

			regInfo_ = regInfo;
			settings_ = settings;
			frmMain_ = frmMain;

			listPqpPages_.Add(tpPqp1);
			listPqpPages_.Add(tpPqp2);
			listPqpPages_.Add(tpPqp3);
			listPqpPages_.Add(tpPqp4);
			listPqpPages_.Add(tpPqp5);
			listPqpPages_.Add(tpPqp6);
			listPqpPages_.Add(tpPqp7);

			listPqpControls_.Add(userControlPqpPage1);
			listPqpControls_.Add(userControlPqpPage2);
			listPqpControls_.Add(userControlPqpPage3);
			listPqpControls_.Add(userControlPqpPage4);
			listPqpControls_.Add(userControlPqpPage5);
			listPqpControls_.Add(userControlPqpPage6);
			listPqpControls_.Add(userControlPqpPage7);
		}

		private void FormArchiveWnd_Load(object sender, EventArgs e)
		{
			try
			{
				// set visability of pages
				for (int iPage = 0; iPage < listPqpPages_.Count; iPage++)
				{
					// we can do so because tabpages were placed to list in according 
					// to their numbers
					listPqpPages_[iPage].Visible =
						(regInfo_.PqpArchives.Count > iPage) && regInfo_.PqpArchives[iPage].Selected;

					if (!listPqpPages_[iPage].Visible)
						listPqpPages_[iPage].Parent = null;	// hide tabPage
					else
					{
						// show data
						listPqpControls_[iPage].ShowData(regInfo_.PqpArchives[iPage], ref regInfo_,
							ref frmMain_, ref settings_);

						// add path to ListView
						ListViewItem itm = new ListViewItem("PQP " + iPage.ToString());
						itm.SubItems.Add(regInfo_.PqpArchives[iPage].Path);
						lvArchiveLocation.Items.Add(itm);			
					}
				}

				if (regInfo_.AvgArchive3Sec == null || !regInfo_.AvgArchive3Sec.Selected)
					tpAvg3sec.Parent = null;		// hide tabPage
				else userControlAvgPage3sec.ShowData(regInfo_.AvgArchive3Sec, ref regInfo_, ref frmMain_, ref settings_);

				if (regInfo_.AvgArchive10Min == null || !regInfo_.AvgArchive10Min.Selected)
					tpAvg10min.Parent = null;
				else userControlAvgPage10min.ShowData(regInfo_.AvgArchive10Min, ref regInfo_, ref frmMain_, ref settings_);

				if (regInfo_.AvgArchive2Hour == null || !regInfo_.AvgArchive2Hour.Selected)
					tpAvg2hour.Parent = null;
				else userControlAvgPage2hour.ShowData(regInfo_.AvgArchive2Hour, ref regInfo_, ref frmMain_, ref settings_);

				//if (regInfo_.DnsArchive == null || !regInfo_.DnsArchive.Selected)			uncomment
				//	tpDns.Parent = null;
				tpDns.Parent = null;			//?????????????? dummy

				// show registration info
				labelSerNumberData.Text = regInfo_.SerialNumber.ToString();
				labelObjNameData.Text = regInfo_.RegistrationName;
				labelDateStartData.Text = regInfo_.DtStart.ToString();
				labelDateEndData.Text = regInfo_.DtEnd.ToString();
				labelConnSchemeData.Text = EmService.GetConnSchemeAsStringFull(regInfo_.ConnectionScheme);
				labelConstrTypeData.Text = EmService.GetConstraintsAsStringFull(regInfo_.ConstraintType);
				labelFirmwareData.Text = regInfo_.DeviceVersion;
				if(!regInfo_.MarkedOnOff)
					labelMarkingDataOnOff.Text = "off";

				labelLatitudeData.Text = regInfo_.GpsLatitude.ToString();
				labelLongitudeData.Text = regInfo_.GpsLongitude.ToString();

				labelNominalFData.Text = regInfo_.Fnominal.ToString();
				labelNominalUphData.Text = regInfo_.UnominalPhase.ToString();
				labelNominalUlinData.Text = regInfo_.UnominalLinear.ToString();
				labelLimitUData.Text = regInfo_.Ulimit.ToString();
				labelLimitIData.Text = regInfo_.Ilimit.ToString();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormArchiveWnd_Load");
				throw;
			}
		}

		private void FormArchiveWnd_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Left || e.KeyData == Keys.Right)
			{
				int selectedTab = tabControlMain.SelectedIndex;
				selectedTab--;		// beacuse 0 is Registration tab; PQP tabs start from 1
				if (selectedTab < listPqpControls_.Count && selectedTab >= 0)
				{
					if (listPqpControls_[selectedTab].IsNonsinusPageActive())
					{
						listPqpControls_[selectedTab].Form_KeyDown(e);
						e.Handled = true;
					}
				}
			}
		}
	}
}
