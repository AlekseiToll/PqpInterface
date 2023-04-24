using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using CustomControls;
using WeifenLuo.WinFormsUI.Docking;
using PulseButtonLib;
using RangeControl;
using EmServiceLib;
using FileAnalyzerLib;

namespace MainInterface
{
	public partial class FormArchivesList : DockContent
	{
		private FormMain frmMain_;
		private EmSettings settings_;

		Morell.GroupPanel.TabPage tpDevice_;
		Morell.GroupPanel.TabPage tpCalendar_;
		Morell.GroupPanel.TabPage tpReg_;
		Morell.GroupPanel.TabPage tpArchive_;

		private Panel panelSerNumbers_ = new Panel();
		private Panel panelCalendar_ = new Panel();
		private SphMonthCalendar calendar_ = new SphMonthCalendar();
		private Panel panelRegs_ = new Panel();
		private Panel panelArchives_ = new Panel();
		private GroupBox gbPqpArchives_ = new GroupBox();
		private GroupBox gbDnsArchive_ = new GroupBox();
		private List<CheckBox> listChbPqpArchives_ = new List<CheckBox>();
		private CheckBox chbAvgArchive3sec_;
		private CheckBox chbAvgArchive10min_;
		private CheckBox chbAvgArchive2hour_;
		private EmRangeBar rangeBarAvg3sec_ = null;
		private EmRangeBar rangeBarAvg10min_ = null;
		private EmRangeBar rangeBarAvg2hour_ = null;
		private CheckBox chbDnsArchive_;

		private DateTime prevSelectedDate_ = DateTime.MinValue;

		private StoredArchivesInfo storedArchivesInfo_ = null;
		private long selectedSerialNumber_ = -1;
		private long selectedRegId_ = -1;

		internal FormArchivesList(ref EmSettings settings, FormMain frmMain)
		{
			InitializeComponent();

			settings_ = settings;
			frmMain_ = frmMain;
		}

		internal void SetStoredDevicesInfo(ref StoredArchivesInfo devicesInfo)
		{
			storedArchivesInfo_ = devicesInfo;
			LoadListOfArchives();
		}

		private void FormArchivesList_Load(object sender, EventArgs e)
		{
			try
			{
				panelArchives_.AutoScroll = true;
				panelCalendar_.AutoScroll = true;
				panelRegs_.AutoScroll = true;
				panelSerNumbers_.AutoScroll = true;

				tpDevice_ = new Morell.GroupPanel.TabPage("Device");
				tpCalendar_ = new Morell.GroupPanel.TabPage("Calendar");
				tpReg_ = new Morell.GroupPanel.TabPage("Registration");
				tpArchive_ = new Morell.GroupPanel.TabPage("Archive");

				tpDevice_.AddControl(panelSerNumbers_);
				tpCalendar_.AddControl(panelCalendar_);
				tpReg_.AddControl(panelRegs_);
				tpArchive_.AddControl(panelArchives_);

				//panelCalendar_.MinimumSize = new Size(180, calendar_.Height);
				//panelCalendar_.Height = calendar_.Height + 100;
				//calendar_.CalendarDimensions = new Size(1, 3);
				//calendar_.Location = new Point(2, 2);
				//calendar_.Parent = panelCalendar_;
				//calendar_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
				//calendar_.ColorTable.DayActiveGradientBegin = Color.Lime;
				//calendar_.ColorTable.DayActiveGradientEnd = Color.Transparent;
				//calendar_.MouseClick += new System.Windows.Forms.MouseEventHandler(this.monthCalendar_MouseClick);
				//calendar_.SelectionRange = new SelectionRange(DateTime.MinValue,
				//                                DateTime.MinValue);

				// Configure the icons to use
				groupPanel.UpImage = Image.FromFile("up.bmp");
				groupPanel.DownImage = Image.FromFile("down.bmp");

				// Create a color gradient
				groupPanel.ColorLeft = SystemColors.ControlLightLight;
				groupPanel.ColorRight = SystemColors.Control;

				// Add the tabpages to the group panel control
				groupPanel.TabPages.AddRange(new[] { tpDevice_, tpCalendar_, tpReg_, tpArchive_ });
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in FormArchivesList_Load");
				throw;
			}
		}

		private void RemovePreviousControls(string type)
		{
			try
			{
				if (type == "all")
				{
					foreach (Control control in panelSerNumbers_.Controls)
					{
						control.Dispose();
					}
					panelSerNumbers_.Controls.Clear();
				}
				if (type == "all" || type == "calendar")
				{
					calendar_.Dispose();
					panelCalendar_.Controls.Clear();

					//panelCalendar_.MinimumSize = new Size(180, calendar_.Height);
					calendar_ = new SphMonthCalendar();
					calendar_.CalendarDimensions = new Size(1, 3);
					calendar_.Location = new Point(2, 2);
					calendar_.Parent = panelCalendar_;
					calendar_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
					calendar_.ColorTable.DayActiveGradientBegin = Color.Lime;
					calendar_.ColorTable.DayActiveGradientEnd = Color.Transparent;
					calendar_.MouseClick += new System.Windows.Forms.MouseEventHandler(this.monthCalendar_MouseClick);
					calendar_.SelectionRange = new SelectionRange(DateTime.MinValue,
					                                              DateTime.MinValue);
				}
				if (type == "all" || type == "calendar" || type == "reg")
				{
					foreach (Control control in panelRegs_.Controls)
					{
						control.Dispose();
					}
					panelRegs_.Controls.Clear();
				}
				if (type == "all" || type == "calendar" || type == "reg" || type == "archives")
				{
					foreach (Control control in panelArchives_.Controls)
					{
						control.Dispose();
					}
					panelArchives_.Controls.Clear();
					listChbPqpArchives_.Clear();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in RemovePreviousControls");
				throw;
			}
		}

		private void monthCalendar_MouseClick(object sender, MouseEventArgs e)
		{
			try
			{
				if (calendar_.UserSelectedDate != DateTime.MinValue &&
				    prevSelectedDate_ != calendar_.UserSelectedDate)
				{
					prevSelectedDate_ = calendar_.UserSelectedDate;
					//Thread.Sleep(1000);
					// Set the registration tab to be shown
					groupPanel.SelectedIndex = 2;

					RegistrationInfo[] regs = 
						storedArchivesInfo_.GetRegistrationsForDate(calendar_.UserSelectedDate);
					//panelRegs_.MinimumSize = new Size(180, 300);
					//panelRegs_.BackColor = Color.BlueViolet;
					panelRegs_.Height = regs.Length * 50 + 50;

					RemovePreviousControls("reg");

					for (int iReg = 0; iReg < regs.Length; ++iReg)
					{
						PulseButton pbtn = new PulseButton();
						pbtn.SerialNumber = regs[iReg].SerialNumber;
						pbtn.RegistrationId = regs[iReg].RegId;
						pbtn.ButtonColorBottom = SystemColors.ButtonFace; //Color.DodgerBlue;
						pbtn.ButtonColorTop = SystemColors.ButtonHighlight;
						pbtn.Text = string.Format("{0}\n{1}",
						                regs[iReg].DtStart.ToString("dd-MM-yy hh:mm:ss"),
						                regs[iReg].DtEnd.ToString("dd- MM-yy  hh:mm:ss"));
						pbtn.Parent = panelRegs_;
						pbtn.Location = new Point(10, 5 + iReg * 70);
						//pbtn.Width = 160;
						pbtn.Width = panelRegs_.Width - 20;
						pbtn.Height = 75;
						pbtn.CornerRadius = 20;
						pbtn.PulseColor = SystemColors.ControlLightLight;
						pbtn.PulseWidth = 10;
						pbtn.ShapeType = PulseButton.Shape.Rectangle;
						pbtn.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
						pbtn.Click += pulseButtonReg_Click;
					}
					//tpReg_.AddControl(panelRegs_);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in monthCalendar_MouseClick");
				throw;
			}
		}

		private void pulseButtonReg_Click(object sender, EventArgs e)
		{
			try
			{
				selectedRegId_ = ((PulseButton)sender).RegistrationId;
				foreach (var ctrl in panelRegs_.Controls)
				{
					PulseButton pbtn = null;
					try { pbtn = (PulseButton)ctrl; }
					catch { }
					if (pbtn != null)
					{
						if (pbtn.RegistrationId == selectedRegId_)
						{
							pbtn.ButtonColorBottom = Color.Lime;
						}
						else pbtn.ButtonColorBottom = SystemColors.ControlDark;
					}
				}

				if (selectedRegId_ != -1)
				{
					RemovePreviousControls("archives");

					RegistrationInfo reg = storedArchivesInfo_.GetRegistration(selectedSerialNumber_,
																		selectedRegId_);
					//panelArchives_.MinimumSize = new Size(180, 300);

					// PQP
					gbPqpArchives_ = new GroupBox();
					gbPqpArchives_.Location = new Point(2, 10);
					gbPqpArchives_.Width = panelArchives_.Width - 4;
					gbPqpArchives_.Height = reg.PqpArchives.Count * 40 + 10 + 10;
					gbPqpArchives_.BackColor = SystemColors.ControlLightLight;
					gbPqpArchives_.Text = "PQP Archives";
					gbPqpArchives_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
					gbPqpArchives_.Parent = panelArchives_;

					panelArchives_.Height = 10 + gbPqpArchives_.Height + 20 + // pqp
						3 * (70 + 20) + 15 + // avg
						60 + 20 + // dns
						75 + 15 + // btn
						75 + 15;  // btn

					for (int iPqp = 0; iPqp < reg.PqpArchives.Count; ++iPqp)
					{
						CheckBox chbPqp = new CheckBox();
						chbPqp.Tag = reg.PqpArchives[iPqp].Id;
						chbPqp.AutoSize = true;
						chbPqp.Text = string.Format("{0}\n{1}",
								reg.PqpArchives[iPqp].DtStart.ToString("dd-MM-yy hh:mm:ss"),
								reg.PqpArchives[iPqp].DtEnd.ToString("dd- MM-yy  hh:mm:ss"));
						//chbPqp.CheckedChanged += chbPqp_CheckedChanged;
						chbPqp.Parent = gbPqpArchives_;
						chbPqp.Location = new Point(15, 20 + iPqp * 40);
						listChbPqpArchives_.Add(chbPqp);
					}

					// AVG
					int shiftVertical = gbPqpArchives_.Top + gbPqpArchives_.Height + 20;
					if (reg.AvgArchive3Sec != null)
					{
						chbAvgArchive3sec_ = new CheckBox();
						chbAvgArchive3sec_.Text = "AVG 3 sec";
						chbAvgArchive3sec_.Location = new Point(5, shiftVertical);
						chbAvgArchive3sec_.Parent = panelArchives_;
						chbAvgArchive3sec_.AutoSize = true;
						chbAvgArchive3sec_.Tag = AvgTypes.ThreeSec;
						chbAvgArchive3sec_.CheckedChanged += chbAvgArchive_CheckedChanged;
						shiftVertical += chbAvgArchive3sec_.Height;

						rangeBarAvg3sec_ = new EmRangeBar(
							reg.AvgArchive3Sec.DtStart, reg.AvgArchive3Sec.DtEnd);
						rangeBarAvg3sec_.Location = new Point(5, shiftVertical);
						rangeBarAvg3sec_.Parent = panelArchives_;
						rangeBarAvg3sec_.Size = new Size(panelArchives_.Width - 10, 70);
						rangeBarAvg3sec_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
						rangeBarAvg3sec_.Enabled = false;
						shiftVertical += rangeBarAvg3sec_.Height + 5;
					}
					else rangeBarAvg3sec_ = null;

					if (reg.AvgArchive10Min != null)
					{
						chbAvgArchive10min_ = new CheckBox();
						chbAvgArchive10min_.Text = "AVG 10 min";
						chbAvgArchive10min_.Location = new Point(5, shiftVertical);
						chbAvgArchive10min_.Parent = panelArchives_;
						chbAvgArchive10min_.AutoSize = true;
						chbAvgArchive10min_.Tag = AvgTypes.TenMin;
						chbAvgArchive10min_.CheckedChanged += chbAvgArchive_CheckedChanged;
						shiftVertical += chbAvgArchive10min_.Height;

						rangeBarAvg10min_ = new EmRangeBar(
							reg.AvgArchive10Min.DtStart, reg.AvgArchive10Min.DtEnd);
						rangeBarAvg10min_.Location = new Point(5, shiftVertical);
						rangeBarAvg10min_.Parent = panelArchives_;
						rangeBarAvg10min_.Size = new Size(panelArchives_.Width - 10, 70);
						rangeBarAvg10min_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
						rangeBarAvg10min_.Enabled = false;
						shiftVertical += rangeBarAvg10min_.Height + 5;
					}
					else rangeBarAvg10min_ = null;

					if (reg.AvgArchive2Hour != null)
					{
						chbAvgArchive2hour_ = new CheckBox();
						chbAvgArchive2hour_.Text = "AVG 2 hour";
						chbAvgArchive2hour_.Location = new Point(5, shiftVertical);
						chbAvgArchive2hour_.Parent = panelArchives_;
						chbAvgArchive2hour_.AutoSize = true;
						chbAvgArchive2hour_.Tag = AvgTypes.TwoHours;
						chbAvgArchive2hour_.CheckedChanged += chbAvgArchive_CheckedChanged;
						shiftVertical += chbAvgArchive2hour_.Height;

						rangeBarAvg2hour_ = new EmRangeBar(
							reg.AvgArchive2Hour.DtStart, reg.AvgArchive2Hour.DtEnd);
						rangeBarAvg2hour_.Location = new Point(5, shiftVertical);
						rangeBarAvg2hour_.Parent = panelArchives_;
						rangeBarAvg2hour_.Size = new Size(panelArchives_.Width - 10, 70);
						rangeBarAvg2hour_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
						rangeBarAvg2hour_.Enabled = false;
						shiftVertical += rangeBarAvg2hour_.Height + 5;
					}
					else rangeBarAvg2hour_ = null;

					shiftVertical += 15;

					// DNS
					if (reg.DnsArchive != null)
					{
						gbDnsArchive_ = new GroupBox();
						gbDnsArchive_.Location = new Point(2, shiftVertical);
						gbDnsArchive_.Width = panelArchives_.Width - 4;
						gbDnsArchive_.Height = 60;
						gbDnsArchive_.BackColor = SystemColors.ControlLightLight;
						gbDnsArchive_.Text = "DNS Archive";
						gbDnsArchive_.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
						gbDnsArchive_.Parent = panelArchives_;

						chbDnsArchive_ = new CheckBox();
						chbDnsArchive_.AutoSize = true;
						chbDnsArchive_.Text = string.Format("{0}\n{1}",
								reg.DtStart.ToString("dd-MM-yy hh:mm:ss"),
								reg.DtEnd.ToString("dd- MM-yy  hh:mm:ss"));
						chbDnsArchive_.Parent = gbDnsArchive_;
						chbDnsArchive_.Location = new Point(15, 20);

						shiftVertical += gbDnsArchive_.Height + 15;
					}

					PulseButton pbtnOpenArchive = new PulseButton();
					pbtnOpenArchive.ButtonColorBottom = SystemColors.ButtonFace; //Color.DodgerBlue;
					pbtnOpenArchive.ButtonColorTop = SystemColors.ButtonHighlight;
					pbtnOpenArchive.Text = "Open Archive";
					pbtnOpenArchive.Parent = panelArchives_;
					pbtnOpenArchive.Location = new Point(20, shiftVertical);
					pbtnOpenArchive.Width = panelArchives_.Width - 40;
					pbtnOpenArchive.Height = 75;
					pbtnOpenArchive.CornerRadius = 20;
					pbtnOpenArchive.PulseColor = SystemColors.ControlLightLight;
					pbtnOpenArchive.PulseWidth = 10;
					pbtnOpenArchive.ShapeType = PulseButton.Shape.Rectangle;
					pbtnOpenArchive.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
					pbtnOpenArchive.Click += pulseButtonOpenArchive_Click;
					shiftVertical += pbtnOpenArchive.Height + 10;

					PulseButton pbtnSelectAll = new PulseButton();
					pbtnSelectAll.ButtonColorBottom = SystemColors.ButtonFace; 
					pbtnSelectAll.ButtonColorTop = SystemColors.ButtonHighlight;
					pbtnSelectAll.Text = "Select All";
					pbtnSelectAll.Parent = panelArchives_;
					pbtnSelectAll.Location = new Point(20, shiftVertical);
					pbtnSelectAll.Width = panelArchives_.Width - 40;
					pbtnSelectAll.Height = 75;
					pbtnSelectAll.CornerRadius = 20;
					pbtnSelectAll.PulseColor = SystemColors.ControlLightLight;
					pbtnSelectAll.PulseWidth = 10;
					pbtnSelectAll.ShapeType = PulseButton.Shape.Rectangle;
					pbtnSelectAll.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
					pbtnSelectAll.Click += pulseButtonSelectAll_Click;
					//shiftVertical += pbtnSelectAll.Height + 10;		

					//tpArchive_.AddControl(panelArchives_);

					//Thread.Sleep(1000);
					// Set the archives tab to be shown
					groupPanel.SelectedIndex = 3;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pulseButtonReg_Click");
				throw;
			}
		}

		private void pulseButtonOpenArchive_Click(object sender, EventArgs e)
		{
			try
			{
				if (selectedRegId_ != -1 && selectedSerialNumber_ != -1)
				{
					RegistrationInfo regInfo = storedArchivesInfo_.GetRegistration(selectedSerialNumber_,
					                                                    selectedRegId_);
					foreach (var control in gbPqpArchives_.Controls)
					{
						if (control is CheckBox)
						{
							for (int iPqp = 0; iPqp < regInfo.PqpArchives.Count; ++iPqp)
							{
								if (regInfo.PqpArchives[iPqp].Id == (int)((CheckBox)control).Tag)
								{
									regInfo.PqpArchives[iPqp].Selected = ((CheckBox)control).Checked;
									break;
								}
							}
						}
					}

					if (chbAvgArchive3sec_ != null && regInfo.AvgArchive3Sec != null)
					{
						regInfo.AvgArchive3Sec.Selected = chbAvgArchive3sec_.Checked;
						if (chbAvgArchive3sec_.Checked)
						{
							regInfo.AvgArchive3Sec.DtStartSelected = 
								new DateTime(rangeBarAvg3sec_.RangeMinimum);
							regInfo.AvgArchive3Sec.DtEndSelected =
								new DateTime(rangeBarAvg3sec_.RangeMaximum);
						}
					}
					if (chbAvgArchive10min_ != null && regInfo.AvgArchive10Min != null)
					{
						regInfo.AvgArchive10Min.Selected = chbAvgArchive10min_.Checked;
						if (chbAvgArchive10min_.Checked)
						{
							regInfo.AvgArchive10Min.DtStartSelected =
								new DateTime(rangeBarAvg10min_.RangeMinimum);
							regInfo.AvgArchive10Min.DtEndSelected =
								new DateTime(rangeBarAvg10min_.RangeMaximum);
						}
					}
					if (chbAvgArchive2hour_ != null && regInfo.AvgArchive2Hour != null)
					{
						regInfo.AvgArchive2Hour.Selected = chbAvgArchive2hour_.Checked;
						if (chbAvgArchive2hour_.Checked)
						{
							regInfo.AvgArchive2Hour.DtStartSelected =
								new DateTime(rangeBarAvg2hour_.RangeMinimum);
							regInfo.AvgArchive2Hour.DtEndSelected =
								new DateTime(rangeBarAvg2hour_.RangeMaximum);
						}
					}

					if (regInfo.DnsArchive != null)
					{
						foreach (var control in gbDnsArchive_.Controls)
						{
							if (control is CheckBox)
							{
								regInfo.DnsArchive.Selected = ((CheckBox) control).Checked;
								break;
							}
						}
					}

					//childWindowList[iWin] = new FormArchiveWnd();
					//childWindowList[iWin].MdiParent = frmMain_;
					//childWindowList[iWin].Show();
					settings_.LoadSettings();
					FormArchiveWnd wnd = new FormArchiveWnd(ref regInfo, ref settings_, ref frmMain_);
					wnd.MdiParent = frmMain_;
					wnd.Show();
				}
				else EmService.WriteToLogFailed("pulseButtonOpenArchive_Click error!");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pulseButtonOpenArchive_Click");
				throw;
			}
		}

		private void chbAvgArchive_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				AvgTypes type = (AvgTypes) ((CheckBox) sender).Tag;
				bool enabled = ((CheckBox) sender).Checked;
				Color backColor = SystemColors.Control;
				if (enabled) backColor = SystemColors.ControlLightLight;
				switch (type)
				{
					case AvgTypes.ThreeSec:
						rangeBarAvg3sec_.Enabled = enabled;
						rangeBarAvg3sec_.BackColor = backColor;
						break;
					case AvgTypes.TenMin:
						rangeBarAvg10min_.Enabled = enabled;
						rangeBarAvg10min_.BackColor = backColor;
						break;
					case AvgTypes.TwoHours:
						rangeBarAvg2hour_.Enabled = enabled;
						rangeBarAvg2hour_.BackColor = backColor;
						break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in chbAvgArchive_CheckedChanged");
				throw;
			}
		}

		private void pulseButtonSelectAll_Click(object sender, EventArgs e)
		{
			try
			{
				foreach (var control in panelArchives_.Controls)
				{
					if (control is CheckBox)
					{
						CheckBox curChb = (CheckBox) control;
						curChb.Checked = true;
					}

					if (control is GroupBox)
					{
						GroupBox gb = (GroupBox) control;
						foreach (var gbControl in gb.Controls)
						{
							if (gbControl is CheckBox)
							{
								CheckBox curChb = (CheckBox) gbControl;
								curChb.Checked = true;
							}
						}
					}

					//if (panelArchives_.Controls[iControl] is EmRangeBar)
					//{
					//    EmRangeBar curRb = (EmRangeBar)panelArchives_.Controls[iControl];
					//    curRb.Set
					//}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pulseButtonSelectAll_Click");
				throw;
			}
		}

		//private void chbPqp_CheckedChanged(object sender, EventArgs e)
		//{

		//}

		private void pulseButtonDevice_Click(object sender, EventArgs e)
		{
			try
			{
				selectedSerialNumber_ = ((PulseButton) sender).SerialNumber;
				foreach (var ctrl in panelSerNumbers_.Controls)
				{
					PulseButton pbtn = null;
					try { pbtn = (PulseButton) ctrl; }
					catch {}
					if (pbtn != null)
					{
						if (pbtn.SerialNumber == selectedSerialNumber_)
						{
							pbtn.ButtonColorBottom = Color.Lime;
						}
						else pbtn.ButtonColorBottom = SystemColors.ControlDark;
					}
				}

				if (selectedSerialNumber_ != -1)
				{
					RemovePreviousControls("calendar");

					RegistrationInfo[] regs = storedArchivesInfo_.GetRegistrations(selectedSerialNumber_);
					List<SelectionRange> listRanges = new List<SelectionRange>();
					DateTime maxDtEnd = DateTime.MinValue;	// determine which page of the calendar
															// must be shown
					foreach (var reg in regs)
					{
						SelectionRange range = new SelectionRange(reg.DtStart, reg.DtEnd);
						listRanges.Add(range);
						if (reg.DtEnd > maxDtEnd) maxDtEnd = reg.DtEnd;
					}
					calendar_.SelectionRanges = listRanges;
					calendar_.ViewStart = maxDtEnd.AddMonths(-1);

					// Set the calendar tab to be shown
					//Thread.Sleep(1000);
					groupPanel.SelectedIndex = 1;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pulseButtonDevice_Click");
				throw;
			}
		}

		private void LoadListOfArchives()
		{
			try
			{
				RemovePreviousControls("all");

				if (storedArchivesInfo_ == null)
					return;

				//storedArchivesInfo_ = new StoredArchivesInfo(ref settings_);
				//storedArchivesInfo_.LoadArchivesInfo();

				long[] devSerNumbers = storedArchivesInfo_.GetDeviceSerialNumbers();
				if (devSerNumbers == null || devSerNumbers.Length == 0)
				{
					EmService.WriteToLogGeneral("No stored archives!");
					return;
				}

				//panelSerNumbers_.MinimumSize = new Size(180, 300);
				panelSerNumbers_.Height = devSerNumbers.Length * 50 + 50;

				for (int iDev = 0; iDev < devSerNumbers.Length; ++iDev)
				{
					PulseButton pbtn = new PulseButton();
					pbtn.SerialNumber = devSerNumbers[iDev];
					pbtn.ButtonColorBottom = SystemColors.ButtonFace; //Color.DodgerBlue;
					pbtn.ButtonColorTop = SystemColors.ButtonHighlight;
					pbtn.Text = "Device " + devSerNumbers[iDev].ToString();
					pbtn.Parent = panelSerNumbers_;
					pbtn.Location = new Point(10, 5 + iDev * 60);
					pbtn.Width = panelSerNumbers_.Width - 20;
					pbtn.Height = 65;
					pbtn.CornerRadius = 20;
					pbtn.PulseColor = SystemColors.ControlLightLight;
					pbtn.PulseWidth = 10;
					pbtn.ShapeType = PulseButton.Shape.Rectangle;
					pbtn.Anchor = ((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right);
					pbtn.Click += pulseButtonDevice_Click;
				}
				//tpDevice_.AddControl(panelSerNumbers_);

				//tpCalendar_.AddControl(panelCalendar_);

				// Set the first tab to be shown
				groupPanel.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in LoadListOfArchives");
				throw;
			}
		}
	}
}
