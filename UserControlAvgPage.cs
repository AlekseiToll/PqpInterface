using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Windows.Forms;

using EmServiceLib;
using FileAnalyzerLib;

namespace MainInterface
{
	public partial class UserControlAvgPage : UserControl
	{
		private RegistrationInfo regInfo_;
		private AvgArchiveInfo archiveInfo_;
		private List<AvgRecordFields> listRecords_;
		private FormMain formMain_;
		private EmSettings settings_;

		public UserControlAvgPage()
		{
			InitializeComponent();
		}

		public void InitTables()
		{
			try
			{
				#region Grid Colors

				DataGridViewCellStyle dgvCellStylePhaseA = new DataGridViewCellStyle();
				dgvCellStylePhaseA.BackColor = DataGridColors.ColorAvgPhaseA;
				DataGridViewCellStyle dgvCellStylePhaseB = new DataGridViewCellStyle();
				dgvCellStylePhaseB.BackColor = DataGridColors.ColorAvgPhaseB;
				DataGridViewCellStyle dgvCellStylePhaseC = new DataGridViewCellStyle();
				dgvCellStylePhaseC.BackColor = DataGridColors.ColorAvgPhaseC;
				DataGridViewCellStyle dgvCellStyleCommon = new DataGridViewCellStyle();
				dgvCellStyleCommon.BackColor = DataGridColors.ColorCommon;
				dgvUI.Columns[(int)DgvUIColumns.MARKED].DefaultCellStyle = dgvCellStyleCommon;
				dgvUI.Columns[(int)DgvUIColumns.UA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.UB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.UC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.UAB].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.UBC].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.UCA].DefaultCellStyle = dgvCellStylePhaseA;

				dgvUI.Columns[(int)DgvUIColumns.U1A].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.U1B].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.U1C].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.U1AB].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.U1BC].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.U1CA].DefaultCellStyle = dgvCellStylePhaseA;

				dgvUI.Columns[(int)DgvUIColumns.U0A].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.U0B].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.U0C].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.U0AB].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.U0BC].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.U0CA].DefaultCellStyle = dgvCellStylePhaseA;

				dgvUI.Columns[(int)DgvUIColumns.UavrA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.UavrB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.UavrC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.UavrAB].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.UavrBC].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.UavrCA].DefaultCellStyle = dgvCellStylePhaseA;

				dgvUI.Columns[(int)DgvUIColumns.IA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.IB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.IC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.IN].DefaultCellStyle = dgvCellStyleCommon;

				dgvUI.Columns[(int)DgvUIColumns.I1A].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.I1B].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.I1C].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.I1N].DefaultCellStyle = dgvCellStyleCommon;

				dgvUI.Columns[(int)DgvUIColumns.I0A].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.I0B].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.I0C].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.I0N].DefaultCellStyle = dgvCellStyleCommon;

				dgvUI.Columns[(int)DgvUIColumns.IavrA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvUI.Columns[(int)DgvUIColumns.IavrB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvUI.Columns[(int)DgvUIColumns.IavrC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvUI.Columns[(int)DgvUIColumns.IavrN].DefaultCellStyle = dgvCellStyleCommon;

				dgvPower.Columns[(int)DgvPowerColumns.MARKED].DefaultCellStyle = dgvCellStyleCommon;

				dgvPower.Columns[(int)DgvPowerColumns.PA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPower.Columns[(int)DgvPowerColumns.PB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPower.Columns[(int)DgvPowerColumns.PC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPower.Columns[(int)DgvPowerColumns.P1].DefaultCellStyle = dgvCellStyleCommon;
				dgvPower.Columns[(int)DgvPowerColumns.P2].DefaultCellStyle = dgvCellStyleCommon;
				dgvPower.Columns[(int)DgvPowerColumns.PSUM].DefaultCellStyle = dgvCellStyleCommon;

				dgvPower.Columns[(int)DgvPowerColumns.SA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPower.Columns[(int)DgvPowerColumns.SB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPower.Columns[(int)DgvPowerColumns.SC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPower.Columns[(int)DgvPowerColumns.SSUM].DefaultCellStyle = dgvCellStyleCommon;

				dgvPower.Columns[(int)DgvPowerColumns.QA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPower.Columns[(int)DgvPowerColumns.QB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPower.Columns[(int)DgvPowerColumns.QC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPower.Columns[(int)DgvPowerColumns.QSUM].DefaultCellStyle = dgvCellStyleCommon;

				dgvPower.Columns[(int)DgvPowerColumns.TANP].DefaultCellStyle = dgvCellStyleCommon;

				dgvPower.Columns[(int)DgvPowerColumns.KPA].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPower.Columns[(int)DgvPowerColumns.KPB].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPower.Columns[(int)DgvPowerColumns.KPC].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPower.Columns[(int)DgvPowerColumns.KPSUM].DefaultCellStyle = dgvCellStyleCommon;

				dgvPqp.Columns[(int)DgvPqpColumns.MARKED].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.U1].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.U2].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.U0].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.K2U].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.K0U].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.I1].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.I2].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.I0].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.P1].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.P2].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.P0].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.AngleP1].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.AngleP2].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.AngleP0].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelS].DefaultCellStyle = dgvCellStyleCommon;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrel1A].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrel1B].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrel1C].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrel1AB].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrel1BC].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrel1CA].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUApos].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUBpos].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUCpos].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUABpos].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUBCpos].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUCApos].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUAneg].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUBneg].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUCneg].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUABneg].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUBCneg].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUCAneg].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelApos].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelBpos].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelCpos].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelABpos].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelBCpos].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelCApos].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelAneg].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelBneg].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelCneg].DefaultCellStyle = dgvCellStylePhaseC;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelABneg].DefaultCellStyle = dgvCellStylePhaseA;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelBCneg].DefaultCellStyle = dgvCellStylePhaseB;
				dgvPqp.Columns[(int)DgvPqpColumns.DUrelCAneg].DefaultCellStyle = dgvCellStylePhaseC;

				dgvAngles.Columns[(int)DgvAnglesColumns.MARKED].DefaultCellStyle = dgvCellStyleCommon;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1AU1B].DefaultCellStyle = dgvCellStylePhaseA;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1BU1C].DefaultCellStyle = dgvCellStylePhaseB;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1CU1A].DefaultCellStyle = dgvCellStylePhaseC;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1ABU1CB].DefaultCellStyle = dgvCellStyleCommon;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1AI1A].DefaultCellStyle = dgvCellStylePhaseA;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1BI1B].DefaultCellStyle = dgvCellStylePhaseB;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1CI1C].DefaultCellStyle = dgvCellStylePhaseC;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1ABI1A].DefaultCellStyle = dgvCellStyleCommon;
				dgvAngles.Columns[(int)DgvAnglesColumns.U1CBI1C].DefaultCellStyle = dgvCellStyleCommon;

				dgvcolIHarmMarked.DefaultCellStyle = dgvCellStyleCommon;
				dgvcolIInterharmMarked.DefaultCellStyle = dgvCellStyleCommon;
				dgvcolUphHarmMarked.DefaultCellStyle = dgvCellStyleCommon;
				dgvcolUphInterharmMarked.DefaultCellStyle = dgvCellStyleCommon;
				dgvcolUlinHarmMarked.DefaultCellStyle = dgvCellStyleCommon;
				dgvcolUlinInterharmMarked.DefaultCellStyle = dgvCellStyleCommon;

				#endregion

				EmService.WriteToLogGeneral("UserControlAvgPage::InitTables step 1");

				#region Columns Visibility

				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
					regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc ||
					regInfo_.ConnectionScheme == ConnectScheme.Ph1W2)
				{
					dgvAngles.Columns[(int)DgvAnglesColumns.U1AU1B].Visible = false;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1BU1C].Visible = false;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1CU1A].Visible = false;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1AI1A].Visible = regInfo_.ConnectionScheme == ConnectScheme.Ph1W2;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1BI1B].Visible = false;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1CI1C].Visible = false;

					dgvAngles.Columns[(int)DgvAnglesColumns.U1ABU1CB].Visible = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1ABI1A].Visible = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1CBI1C].Visible = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				}
				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W4)
				{
					dgvAngles.Columns[(int)DgvAnglesColumns.U1ABU1CB].Visible = false;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1ABI1A].Visible = false;
					dgvAngles.Columns[(int)DgvAnglesColumns.U1CBI1C].Visible = false;
				}

				if (regInfo_.ConnectionScheme == ConnectScheme.Ph1W2)
				{
					dgvPqp.Columns[(int)DgvPqpColumns.U1].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.U2].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.K2U].Visible = false;
				}

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W4)
				{
					dgvPqp.Columns[(int)DgvPqpColumns.U0].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.K0U].Visible = false;
				}

				if (regInfo_.Ilimit == 0)
				{
					dgvPqp.Columns[(int)DgvPqpColumns.I1].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.I2].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.I0].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.P1].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.P2].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.P0].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.AngleP1].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.AngleP2].Visible = false;
					dgvPqp.Columns[(int)DgvPqpColumns.AngleP0].Visible = false;
				}

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
					regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					dgvPower.Columns[(int)DgvPowerColumns.P1].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.P2].Visible = false;
				}
				else
				{
					dgvPower.Columns[(int)DgvPowerColumns.PA].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.PB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.PC].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.SA].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.SB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.SC].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.QA].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.QB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.QC].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.KPA].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.KPB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.KPC].Visible = false;
				}

				if (regInfo_.ConnectionScheme == ConnectScheme.Ph1W2)
				{
					dgvPower.Columns[(int)DgvPowerColumns.PB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.PC].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.SB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.SC].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.QB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.QC].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.KPB].Visible = false;
					dgvPower.Columns[(int)DgvPowerColumns.KPC].Visible = false;
				}

				#endregion

				EmService.WriteToLogGeneral("UserControlAvgPage::InitTables step 2");

				#region Add Columns

				string[] phases;
				if (regInfo_.ConnectionScheme == ConnectScheme.Ph1W2)
					phases = new string[] { "A" };
				else if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 || regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
					phases = new string[] { "A", "B", "C" };
				else // 3 ph 4 w
					phases = new string[] { "A", "B", "C", "N" };
				Dictionary<string, DataGridViewCellStyle> cellStyles = new Dictionary<string, DataGridViewCellStyle>();
				cellStyles.Add("A", dgvCellStylePhaseA);
				cellStyles.Add("B", dgvCellStylePhaseB);
				cellStyles.Add("C", dgvCellStylePhaseC);
				cellStyles.Add("N", dgvCellStyleCommon);
				cellStyles.Add("AB", dgvCellStylePhaseA);
				cellStyles.Add("BC", dgvCellStylePhaseB);
				cellStyles.Add("CA", dgvCellStylePhaseC);

				// I harmonics
				foreach (var phase in phases)
				{
					DataGridViewTextBoxColumn dgvcolTmp = new DataGridViewTextBoxColumn();
					dgvcolTmp.HeaderText = string.Format("Σgpgr>1 {0}, A", phase);
					dgvcolTmp.Name = string.Format("dgvcolIHarmSum{0}", phase);
					dgvcolTmp.ReadOnly = true;
					dgvcolTmp.DefaultCellStyle = cellStyles[phase];
					dgvIHarm.Columns.Add(dgvcolTmp);

					for (int iCol = 1; iCol <= 50; iCol++)
					{
						dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("Gsg{0} {1}, A", iCol, phase);
						dgvcolTmp.Name = string.Format("dgvcolIHarmGsg{0}{1}", iCol, phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvcolTmp.Tag = iCol;
						//dgvcolTmp.Width = 90;
						dgvIHarm.Columns.Add(dgvcolTmp);
					}

					dgvcolTmp = new DataGridViewTextBoxColumn();
					dgvcolTmp.HeaderText = string.Format("THDS {0}, %", phase);
					dgvcolTmp.Name = string.Format("dgvcolIHarmThds{0}", phase);
					dgvcolTmp.ReadOnly = true;
					dgvcolTmp.DefaultCellStyle = cellStyles[phase];
					dgvIHarm.Columns.Add(dgvcolTmp);

					for (int iCol = 2; iCol <= 50; iCol++)
					{
						dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("C ord({0}) {1}, %", iCol, phase);
						dgvcolTmp.Name = string.Format("dgvcolIHarmCOrd{0}{1}", iCol, phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvcolTmp.Tag = iCol;
						//dgvcolTmp.Width = 90;
						dgvIHarm.Columns.Add(dgvcolTmp);
					}
				}

				// I interharmonics
				foreach (var phase in phases)
				{
					for (int iCol = 0; iCol <= 50; iCol++)
					{
						DataGridViewTextBoxColumn dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("Cig{0} {1}, A", iCol, phase);
						dgvcolTmp.Name = string.Format("dgvcolIInterHarm{0}{1}", iCol, phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvcolTmp.Tag = iCol;
						dgvcolTmp.Width = 90;
						dgvIInterharm.Columns.Add(dgvcolTmp);
					}
				}

				// U harmonics and interharmonics (phase and lin)
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
					regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					// ??????????????????? hide U lin page

					if (regInfo_.ConnectionScheme == ConnectScheme.Ph1W2)
						phases = new string[] { "A" };
					else // 3 ph 4 w
						phases = new string[] { "A", "B", "C" };
					foreach (var phase in phases)
					{
						DataGridViewTextBoxColumn dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("Σgpgr>1 {0}, V", phase);
						dgvcolTmp.Name = string.Format("dgvcolUphHarmSum{0}", phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvUphHarm.Columns.Add(dgvcolTmp);

						for (int iCol = 1; iCol <= 50; iCol++)
						{
							dgvcolTmp = new DataGridViewTextBoxColumn();
							dgvcolTmp.HeaderText = string.Format("Gsg{0} {1}, V", iCol, phase);
							dgvcolTmp.Name = string.Format("dgvcolUphHarmGsg{0}{1}", iCol, phase);
							dgvcolTmp.ReadOnly = true;
							dgvcolTmp.DefaultCellStyle = cellStyles[phase];
							dgvcolTmp.Tag = iCol;
							//dgvcolTmp.Width = 90;
							dgvUphHarm.Columns.Add(dgvcolTmp);
						}

						dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("THDS {0}, %", phase);
						dgvcolTmp.Name = string.Format("dgvcolUphHarmThds{0}", phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvUphHarm.Columns.Add(dgvcolTmp);

						for (int iCol = 2; iCol <= 50; iCol++)
						{
							dgvcolTmp = new DataGridViewTextBoxColumn();
							dgvcolTmp.HeaderText = string.Format("C ord({0}) {1}, %", iCol, phase);
							dgvcolTmp.Name = string.Format("dgvcolUphHarmCOrd{0}{1}", iCol, phase);
							dgvcolTmp.ReadOnly = true;
							dgvcolTmp.DefaultCellStyle = cellStyles[phase];
							dgvcolTmp.Tag = iCol;
							//dgvcolTmp.Width = 90;
							dgvUphHarm.Columns.Add(dgvcolTmp);
						}
					}

					// U phase interharmonics
					foreach (var phase in phases)
					{
						for (int iCol = 0; iCol <= 50; iCol++)
						{
							DataGridViewTextBoxColumn dgvcolTmp = new DataGridViewTextBoxColumn();
							dgvcolTmp.HeaderText = string.Format("Cig{0} {1}, V", iCol, phase);
							dgvcolTmp.Name = string.Format("dgvcolUphInterHarm{0}{1}", iCol, phase);
							dgvcolTmp.ReadOnly = true;
							dgvcolTmp.DefaultCellStyle = cellStyles[phase];
							dgvcolTmp.Tag = iCol;
							dgvcolTmp.Width = 90;
							dgvUphInterharm.Columns.Add(dgvcolTmp);
						}
					}
				}
				else
				{
					// hide Uph page ????????????????????

					phases = new string[] { "AB", "BC", "CA" };
					foreach (var phase in phases)
					{
						DataGridViewTextBoxColumn dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("Σgpgr>1 {0}, V", phase);
						dgvcolTmp.Name = string.Format("dgvcolUlinHarmSum{0}", phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvUlinHarm.Columns.Add(dgvcolTmp);

						for (int iCol = 1; iCol <= 50; iCol++)
						{
							dgvcolTmp = new DataGridViewTextBoxColumn();
							dgvcolTmp.HeaderText = string.Format("Gsg{0} {1}, V", iCol, phase);
							dgvcolTmp.Name = string.Format("dgvcolUlinHarmGsg{0}{1}", iCol, phase);
							dgvcolTmp.ReadOnly = true;
							dgvcolTmp.DefaultCellStyle = cellStyles[phase];
							dgvcolTmp.Tag = iCol;
							//dgvcolTmp.Width = 90;
							dgvUlinHarm.Columns.Add(dgvcolTmp);
						}

						dgvcolTmp = new DataGridViewTextBoxColumn();
						dgvcolTmp.HeaderText = string.Format("THDS {0}, %", phase);
						dgvcolTmp.Name = string.Format("dgvcolUlinHarmThds{0}", phase);
						dgvcolTmp.ReadOnly = true;
						dgvcolTmp.DefaultCellStyle = cellStyles[phase];
						dgvUlinHarm.Columns.Add(dgvcolTmp);

						for (int iCol = 2; iCol <= 50; iCol++)
						{
							dgvcolTmp = new DataGridViewTextBoxColumn();
							dgvcolTmp.HeaderText = string.Format("C ord({0}) {1}, %", iCol, phase);
							dgvcolTmp.Name = string.Format("dgvcolUlinHarmCOrd{0}{1}", iCol, phase);
							dgvcolTmp.ReadOnly = true;
							dgvcolTmp.DefaultCellStyle = cellStyles[phase];
							dgvcolTmp.Tag = iCol;
							//dgvcolTmp.Width = 90;
							dgvUlinHarm.Columns.Add(dgvcolTmp);
						}
					}

					// U phase interharmonics
					foreach (var phase in phases)
					{
						for (int iCol = 0; iCol <= 50; iCol++)
						{
							DataGridViewTextBoxColumn dgvcolTmp = new DataGridViewTextBoxColumn();
							dgvcolTmp.HeaderText = string.Format("Cig{0} {1}, V", iCol, phase);
							dgvcolTmp.Name = string.Format("dgvcolUlinInterHarm{0}{1}", iCol, phase);
							dgvcolTmp.ReadOnly = true;
							dgvcolTmp.DefaultCellStyle = cellStyles[phase];
							dgvcolTmp.Tag = iCol;
							dgvcolTmp.Width = 90;
							dgvUlinInterharm.Columns.Add(dgvcolTmp);
						}
					}
				}

				#endregion

				EmService.WriteToLogGeneral("UserControlAvgPage::InitTables step 3");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::InitTables");
				throw;
			}
		}

		public void ShowData(AvgArchiveInfo archiveInfo, ref RegistrationInfo regInfo,
		                     ref FormMain formMain, ref EmSettings settings)
		{
			try
			{
				regInfo_ = regInfo;
				archiveInfo_ = archiveInfo;
				formMain_ = formMain;
				settings_ = settings;

				if (regInfo_ == null || archiveInfo_ == null)
				{
					EmService.WriteToLogFailed("UserControlAvgPage_Load: null error");
					MessageBox.Show("Inner error: AVG data is null", "Error",
					                MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				formMain_.Cursor = Cursors.WaitCursor;

				InitTables();

				ResourceManager rm = new ResourceManager("MainInterface.emstrings", formMain_.GetType().Assembly);
				labelArchiveInfo.Text = String.Format(rm.GetString("avg_archive_info"),
				                                      archiveInfo_.DtStart, archiveInfo_.DtEnd,
				                                      EmService.GetAvgTypeAsString(archiveInfo_.AvgType),
				                                      "-", "-");
				//strCtrValue, strVtrValue);// КТТ, КТН ??????????????????????????

				//CultureInfo ci_enUS = new CultureInfo("en-US");

				if (!FileAnalyzer.GetAvgRecordFields(archiveInfo_.Path, out listRecords_, ref regInfo_,
				                                     archiveInfo_.DtStartSelected, archiveInfo_.DtEndSelected))
				{
					EmService.WriteToLogFailed("UserControlAvgPage_Load: error");
					MessageBox.Show("Error while analyzing AVG archive", "Error",
					                MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				((ISupportInitialize) dgvUI).BeginInit();
				dgvUI.VirtualMode = true;
				dgvUI.CellValueNeeded += dgvUI_CellValueNeeded;
				dgvUI.RowCount = listRecords_.Count;
				((ISupportInitialize) dgvUI).EndInit();

				((ISupportInitialize) dgvPower).BeginInit();
				dgvPower.VirtualMode = true;
				dgvPower.CellValueNeeded += dgvPower_CellValueNeeded;
				dgvPower.RowCount = listRecords_.Count;
				((ISupportInitialize) dgvPower).EndInit();

				((ISupportInitialize) dgvPqp).BeginInit();
				dgvPqp.VirtualMode = true;
				dgvPqp.CellValueNeeded += dgvPqp_CellValueNeeded;
				dgvPqp.RowCount = listRecords_.Count;
				((ISupportInitialize) dgvPqp).EndInit();

				((ISupportInitialize) dgvAngles).BeginInit();
				dgvAngles.VirtualMode = true;
				dgvAngles.CellValueNeeded += dgvAngles_CellValueNeeded;
				dgvAngles.RowCount = listRecords_.Count;
				((ISupportInitialize) dgvAngles).EndInit();

				((ISupportInitialize) dgvIHarm).BeginInit();
				dgvIHarm.VirtualMode = true;
				dgvIHarm.CellValueNeeded += dgvIHarm_CellValueNeeded;
				dgvIHarm.RowCount = listRecords_.Count;
				((ISupportInitialize) dgvIHarm).EndInit();

				((ISupportInitialize) dgvIInterharm).BeginInit();
				dgvIInterharm.VirtualMode = true;
				dgvIInterharm.CellValueNeeded += dgvIInterharm_CellValueNeeded;
				dgvIInterharm.RowCount = listRecords_.Count;
				((ISupportInitialize) dgvIInterharm).EndInit();

				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 || regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					((ISupportInitialize) dgvUlinHarm).BeginInit();
					dgvUlinHarm.VirtualMode = true;
					dgvUlinHarm.CellValueNeeded += dgvUlinHarm_CellValueNeeded;
					dgvUlinHarm.RowCount = listRecords_.Count;
					((ISupportInitialize) dgvUlinHarm).EndInit();

					((ISupportInitialize) dgvUlinInterharm).BeginInit();
					dgvUlinInterharm.VirtualMode = true;
					dgvUlinInterharm.CellValueNeeded += dgvUlinInterharm_CellValueNeeded;
					dgvUlinInterharm.RowCount = listRecords_.Count;
					((ISupportInitialize) dgvUlinInterharm).EndInit();

					tpUPhHarmonics.Parent = null;
					tpUPhInterHarmonics.Parent = null;
				}
				else
				{
					((ISupportInitialize) dgvUphHarm).BeginInit();
					dgvUphHarm.VirtualMode = true;
					dgvUphHarm.CellValueNeeded += dgvUphHarm_CellValueNeeded;
					dgvUphHarm.RowCount = listRecords_.Count;
					((ISupportInitialize) dgvUphHarm).EndInit();

					((ISupportInitialize) dgvUphInterharm).BeginInit();
					dgvUphInterharm.VirtualMode = true;
					dgvUphInterharm.CellValueNeeded += dgvUphInterharm_CellValueNeeded;
					dgvUphInterharm.RowCount = listRecords_.Count;
					((ISupportInitialize) dgvUphInterharm).EndInit();

					tpULinHarmonics.Parent = null;
					tpULinInterHarmonics.Parent = null;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::ShowData");
				throw;
			}
			finally
			{
				formMain_.Cursor = Cursors.Default;
			}
		}

		// Voltage and current (U and I)
		private void dgvUI_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];
				switch (e.ColumnIndex)
				{
					case (int) DgvUIColumns.TIME: e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss"); break;

					case (int)DgvUIColumns.MARKED: e.Value = record.BRecordIsMarked ? "yes" : "no"; break;

					// напряжение - действующие значения
					case (int)DgvUIColumns.UA: e.Value = record.Ua.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UB: e.Value = record.Ub.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UC: e.Value = record.Uc.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UAB: e.Value = record.Uab.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UBC: e.Value = record.Ubc.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UCA: e.Value = record.Uca.ToString(settings_.FloatFormat); break;

					// напряжение - 1-ая гармоника
					case (int)DgvUIColumns.U1A: e.Value = record.Ua1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U1B: e.Value = record.Ub1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U1C: e.Value = record.Uc1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U1AB: e.Value = record.Uab1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U1BC: e.Value = record.Ubc1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U1CA: e.Value = record.Uca1harm.ToString(settings_.FloatFormat); break;

					// напряжение - постоянная составляющая
					case (int)DgvUIColumns.U0A: e.Value = record.UaConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U0B: e.Value = record.UbConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U0C: e.Value = record.UcConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U0AB: e.Value = record.UabConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U0BC: e.Value = record.UbcConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.U0CA: e.Value = record.UcaConst.ToString(settings_.FloatFormat); break;

					// напряжение - средневыпрямленное значение
					case (int)DgvUIColumns.UavrA: e.Value = record.UaAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UavrB: e.Value = record.UbAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UavrC: e.Value = record.UcAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UavrAB: e.Value = record.UabAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UavrBC: e.Value = record.UbcAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.UavrCA: e.Value = record.UcaAvdirect.ToString(settings_.FloatFormat); break;

					// ток - действующие значения
					case (int)DgvUIColumns.IA: e.Value = record.Ia.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.IB: e.Value = record.Ib.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.IC: e.Value = record.Ic.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.IN: e.Value = record.In.ToString(settings_.FloatFormat); break;

					// ток - постоянная составляющая
					case (int)DgvUIColumns.I0A: e.Value = record.IaConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.I0B: e.Value = record.IbConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.I0C: e.Value = record.IcConst.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.I0N: e.Value = record.InConst.ToString(settings_.FloatFormat); break;

					// ток - средневыпрямленное значение
					case (int)DgvUIColumns.IavrA: e.Value = record.IaAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.IavrB: e.Value = record.IbAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.IavrC: e.Value = record.IcAvdirect.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.IavrN: e.Value = record.InAvdirect.ToString(settings_.FloatFormat); break;

					// ток - 1-ая гармоника
					case (int)DgvUIColumns.I1A: e.Value = record.Ia1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.I1B: e.Value = record.Ib1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.I1C: e.Value = record.Ic1harm.ToString(settings_.FloatFormat); break;
					case (int)DgvUIColumns.I1N: e.Value = record.In1harm.ToString(settings_.FloatFormat); break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvUI_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		// Power
		//private void FillPowerData(AvgRecordFields record, DateTime dt)
		private void dgvPower_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];
				switch (e.ColumnIndex)
				{
					case (int) DgvPowerColumns.TIME: e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss"); break;
					case (int)DgvPowerColumns.MARKED: e.Value = record.BRecordIsMarked ? "yes" : "no"; break;

					case (int)DgvPowerColumns.PA: e.Value = record.Pa.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.PB: e.Value = record.Pb.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.PC: e.Value = record.Pc.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.P1: e.Value = record.P1.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.P2: e.Value = record.P2.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.PSUM:
						if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
						    regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
						{
							
							e.Value = record.Psum.ToString(settings_.FloatFormat);
						}
						else
						{
							
							e.Value = record.P12sum.ToString(settings_.FloatFormat);
						}
						break;

					case (int)DgvPowerColumns.SA: e.Value = record.Sa.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.SB: e.Value = record.Sb.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.SC: e.Value = record.Sc.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.SSUM:
						if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
							regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
						{

							e.Value = record.Ssum.ToString(settings_.FloatFormat);
						}
						else
						{

							e.Value = record.S12sum.ToString(settings_.FloatFormat);
						}
						break;

					case (int)DgvPowerColumns.QA: e.Value = record.Qa.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.QB: e.Value = record.Qb.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.QC: e.Value = record.Qc.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.QSUM:
						if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
							regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
						{

							e.Value = record.Qsum.ToString(settings_.FloatFormat);
						}
						else
						{

							e.Value = record.Q12sum.ToString(settings_.FloatFormat);
						}
						break;

					case (int)DgvPowerColumns.TANP: e.Value = record.TanP.ToString(settings_.FloatFormat); break;

					case (int)DgvPowerColumns.KPA: e.Value = record.Kpa.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.KPB: e.Value = record.Kpb.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.KPC: e.Value = record.Kpc.ToString(settings_.FloatFormat); break;
					case (int)DgvPowerColumns.KPSUM:
						if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 &&
							regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
						{

							e.Value = record.Kpabc.ToString(settings_.FloatFormat);
						}
						else
						{

							e.Value = record.Kp12.ToString(settings_.FloatFormat);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvPower_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		// PQP
		private void dgvPqp_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];
				switch (e.ColumnIndex)
				{
					case (int) DgvPqpColumns.TIME: e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss"); break;
					case (int) DgvPqpColumns.MARKED: e.Value = record.BRecordIsMarked ? "yes" : "no"; break;

					case (int) DgvPqpColumns.U1: e.Value = record.U1.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.U2: e.Value = record.U2.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.U0: e.Value = record.U0.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.K2U: e.Value = record.K2u.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.K0U: e.Value = record.K0u.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.I1: e.Value = record.I1.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.I2: e.Value = record.I2.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.I0: e.Value = record.I0.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.P1: e.Value = record.P1pqp.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.P2: e.Value = record.P2pqp.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.P0: e.Value = record.P0pqp.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.AngleP1: e.Value = record.AngleP1.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.AngleP2: e.Value = record.AngleP2.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.AngleP0: e.Value = record.AngleP0.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.DUrelS: e.Value = record.RdUY.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrel1A: e.Value = record.RdU1harmA.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrel1B: e.Value = record.RdU1harmB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrel1C: e.Value = record.RdU1harmC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrel1AB: e.Value = record.RdU1harmAB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrel1BC: e.Value = record.RdU1harmBC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrel1CA: e.Value = record.RdU1harmCA.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.DUApos: e.Value = record.DUposA.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUBpos: e.Value = record.DUposB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUCpos: e.Value = record.DUposC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUABpos: e.Value = record.DUposAB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUBCpos: e.Value = record.DUposBC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUCApos: e.Value = record.DUposCA.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.DUAneg: e.Value = record.DUnegA.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUBneg: e.Value = record.DUnegB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUCneg: e.Value = record.DUnegC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUABneg: e.Value = record.DUnegAB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUBCneg: e.Value = record.DUnegBC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUCAneg: e.Value = record.DUnegCA.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.DUrelApos: e.Value = record.RdUposA.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelBpos: e.Value = record.RdUposB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelCpos: e.Value = record.RdUposC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelABpos: e.Value = record.RdUposAB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelBCpos: e.Value = record.RdUposBC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelCApos: e.Value = record.RdUposCA.ToString(settings_.FloatFormat); break;

					case (int) DgvPqpColumns.DUrelAneg: e.Value = record.RdUnegA.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelBneg: e.Value = record.RdUnegB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelCneg: e.Value = record.RdUnegC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelABneg: e.Value = record.RdUnegAB.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelBCneg: e.Value = record.RdUnegBC.ToString(settings_.FloatFormat); break;
					case (int) DgvPqpColumns.DUrelCAneg: e.Value = record.RdUnegCA.ToString(settings_.FloatFormat); break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvPqp_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		// Angles
		private void dgvAngles_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];
				switch (e.ColumnIndex)
				{
					case (int)DgvAnglesColumns.TIME: e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss"); break;
					case (int)DgvAnglesColumns.MARKED: e.Value = record.BRecordIsMarked ? "yes" : "no"; break;

					case (int)DgvAnglesColumns.U1AU1B: e.Value = record.AngleUaUb.ToString(settings_.FloatFormat); break;
					case (int)DgvAnglesColumns.U1BU1C: e.Value = record.AngleUbUc.ToString(settings_.FloatFormat); break;
					case (int)DgvAnglesColumns.U1CU1A: e.Value = record.AngleUcUa.ToString(settings_.FloatFormat); break;
					case (int)DgvAnglesColumns.U1AI1A: e.Value = record.AngleUaIa.ToString(settings_.FloatFormat); break;
					case (int)DgvAnglesColumns.U1BI1B: e.Value = record.AngleUbIb.ToString(settings_.FloatFormat); break;
					case (int)DgvAnglesColumns.U1CI1C: e.Value = record.AngleUcIc.ToString(settings_.FloatFormat); break;

					case (int) DgvAnglesColumns.U1ABU1CB: e.Value = record.AngleUabUcb.ToString(settings_.FloatFormat); break;
					case (int) DgvAnglesColumns.U1ABI1A: e.Value = record.AngleUabIa.ToString(settings_.FloatFormat); break;
					case (int)DgvAnglesColumns.U1CBI1C: e.Value = record.AngleUcbIc.ToString(settings_.FloatFormat); break;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvAngles_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		//private void FillIHarmonicsData(AvgRecordFields record, DateTime dt)
		private void dgvIHarm_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				int curIndex;
				AvgRecordFields record = listRecords_[e.RowIndex];
				
				if (e.ColumnIndex == DgvHarmColumns.TIME)
					e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss");
				else if (e.ColumnIndex == DgvHarmColumns.MARKED)
					e.Value = record.BRecordIsMarked ? "yes" : "no";
				// phase A
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.SUM)
					e.Value = record.IHarmSummForOrderMore1A.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.SUM) &&
				         e.ColumnIndex < (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS))
				{
					curIndex = e.ColumnIndex - (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.ORDER_VALUE1);
					//if (curIndex >= record.IHarmOrderValueA.Length)
					//	return;
					e.Value = record.IHarmOrderValueA[curIndex].ToString(settings_.FloatFormat);
				}

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS)
					e.Value = record.IHarmOrderCoefA[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS) &&
				         e.ColumnIndex < DgvHarmColumns.BLOCKB)
				{
					curIndex = e.ColumnIndex - (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.ORDER_COEF1) + 1;
					//if (curIndex >= record.IHarmOrderCoefA.Length)
					//	return;
					e.Value = record.IHarmOrderCoefA[curIndex].ToString(settings_.FloatFormat);
				}

					// phase B
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.SUM)
					e.Value = record.IHarmSummForOrderMore1B.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.SUM) &&
				         e.ColumnIndex < (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS))
					e.Value = record.IHarmOrderValueB[e.ColumnIndex -
					                                  (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(
						                                  settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS)
					e.Value = record.IHarmOrderCoefB[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS) &&
				         e.ColumnIndex < DgvHarmColumns.BLOCKC)
					e.Value = record.IHarmOrderCoefB[e.ColumnIndex -
					                                 (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(
						                                 settings_.FloatFormat);

					// phase C
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.SUM)
					e.Value = record.IHarmSummForOrderMore1C.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.SUM) &&
				         e.ColumnIndex < (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS))
					e.Value = record.IHarmOrderValueC[e.ColumnIndex -
					                                  (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(
						                                  settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS)
					e.Value = record.IHarmOrderCoefC[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS) &&
				         e.ColumnIndex < DgvHarmColumns.BLOCKN)
					e.Value = record.IHarmOrderCoefC[e.ColumnIndex -
					                                 (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(
						                                 settings_.FloatFormat);

					// phase N
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.SUM)
					e.Value = record.IHarmSummForOrderMore1N.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.SUM) &&
				         e.ColumnIndex < (DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.THDS))
					e.Value = record.IHarmOrderValueN[e.ColumnIndex -
					                                  (DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(
						                                  settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.THDS)
					e.Value = record.IHarmOrderCoefN[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.THDS))
					e.Value = record.IHarmOrderCoefN[e.ColumnIndex -
					                                 (DgvHarmColumns.BLOCKN + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(
						                                 settings_.FloatFormat);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvIHarm_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		private void dgvIInterharm_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];

				if (e.ColumnIndex == DgvInterHarmColumns.TIME)
					e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss");
				else if (e.ColumnIndex == DgvInterHarmColumns.MARKED)
					e.Value = record.BRecordIsMarked ? "yes" : "no";
				// phase A
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKA)
					e.Value = record.IInterHarmAvgSquareA.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKA && e.ColumnIndex < DgvInterHarmColumns.BLOCKB)
					e.Value = record.IInterHarmAvgSquareOrderA[e.ColumnIndex - DgvInterHarmColumns.BLOCKA - 1].
						ToString(settings_.FloatFormat);

				// phase B
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKB)
					e.Value = record.IInterHarmAvgSquareB.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKB && e.ColumnIndex < DgvInterHarmColumns.BLOCKC)
					e.Value = record.IInterHarmAvgSquareOrderB[e.ColumnIndex - DgvInterHarmColumns.BLOCKB - 1].
						ToString(settings_.FloatFormat);

				// phase C
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKC)
					e.Value = record.IInterHarmAvgSquareC.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKC && e.ColumnIndex < DgvInterHarmColumns.BLOCKN)
					e.Value = record.IInterHarmAvgSquareOrderC[e.ColumnIndex - DgvInterHarmColumns.BLOCKC - 1].
						ToString(settings_.FloatFormat);

				// phase N
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKN)
					e.Value = record.IInterHarmAvgSquareN.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKN)
					e.Value = record.IInterHarmAvgSquareOrderN[e.ColumnIndex - DgvInterHarmColumns.BLOCKN - 1].
						ToString(settings_.FloatFormat);	
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvIInterharm_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				//throw;
			}
		}

		private void dgvUphHarm_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];

				if (e.ColumnIndex == DgvHarmColumns.TIME)
					e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss");
				else if (e.ColumnIndex == DgvHarmColumns.MARKED)
					e.Value = record.BRecordIsMarked ? "yes" : "no";
				// phase A
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.SUM)
					e.Value = record.UHarmSummForOrderMore1A.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.SUM) &&
					e.ColumnIndex < (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS))
					e.Value = record.UHarmOrderValueA[e.ColumnIndex -
						(DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS)
					e.Value = record.UHarmOrderCoefA[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS) &&
									e.ColumnIndex < DgvHarmColumns.BLOCKB)
					e.Value = record.UHarmOrderCoefA[e.ColumnIndex -
						(DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(settings_.FloatFormat);

				// phase B
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.SUM)
					e.Value = record.UHarmSummForOrderMore1B.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.SUM) &&
					e.ColumnIndex < (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS))
					e.Value = record.UHarmOrderValueB[e.ColumnIndex -
						(DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS)
					e.Value = record.UHarmOrderCoefB[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS) &&
									e.ColumnIndex < DgvHarmColumns.BLOCKC)
					e.Value = record.UHarmOrderCoefB[e.ColumnIndex -
						(DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(settings_.FloatFormat);

				// phase C
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.SUM)
					e.Value = record.UHarmSummForOrderMore1C.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.SUM) &&
					e.ColumnIndex < (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS))
					e.Value = record.UHarmOrderValueC[e.ColumnIndex -
						(DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS)
					e.Value = record.UHarmOrderCoefC[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS) &&
									e.ColumnIndex < DgvHarmColumns.BLOCKN)
					e.Value = record.UHarmOrderCoefC[e.ColumnIndex -
						(DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(settings_.FloatFormat);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvUphHarm_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		private void dgvUphInterharm_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];

				if (e.ColumnIndex == DgvInterHarmColumns.TIME)
					e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss");
				else if (e.ColumnIndex == DgvInterHarmColumns.MARKED)
					e.Value = record.BRecordIsMarked ? "yes" : "no";
				// phase A
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKA)
					e.Value = record.UInterHarmAvgSquareA.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKA && e.ColumnIndex < DgvInterHarmColumns.BLOCKB)
					e.Value = record.UInterHarmAvgSquareOrderA[e.ColumnIndex - DgvInterHarmColumns.BLOCKA - 1].
						ToString(settings_.FloatFormat);

				// phase B
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKB)
					e.Value = record.UInterHarmAvgSquareB.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKB && e.ColumnIndex < DgvInterHarmColumns.BLOCKC)
					e.Value = record.UInterHarmAvgSquareOrderB[e.ColumnIndex - DgvInterHarmColumns.BLOCKB - 1].
						ToString(settings_.FloatFormat);

				// phase C
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKC)
					e.Value = record.UInterHarmAvgSquareC.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKC && e.ColumnIndex < DgvInterHarmColumns.BLOCKN)
					e.Value = record.UInterHarmAvgSquareOrderC[e.ColumnIndex - DgvInterHarmColumns.BLOCKC - 1].
						ToString(settings_.FloatFormat);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvUphInterharm_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		private void dgvUlinHarm_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];

				if (e.ColumnIndex == DgvHarmColumns.TIME)
					e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss");
				else if (e.ColumnIndex == DgvHarmColumns.MARKED)
					e.Value = record.BRecordIsMarked ? "yes" : "no";
				// phase A
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.SUM)
					e.Value = record.UHarmSummForOrderMore1AB.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.SUM) &&
					e.ColumnIndex < (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS))
					e.Value = record.UHarmOrderValueAB[e.ColumnIndex -
						(DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS)
					e.Value = record.UHarmOrderCoefAB[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.THDS) &&
									e.ColumnIndex < DgvHarmColumns.BLOCKB)
					e.Value = record.UHarmOrderCoefAB[e.ColumnIndex -
						(DgvHarmColumns.BLOCKA + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(settings_.FloatFormat);

				// phase B
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.SUM)
					e.Value = record.UHarmSummForOrderMore1BC.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.SUM) &&
					e.ColumnIndex < (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS))
					e.Value = record.UHarmOrderValueBC[e.ColumnIndex -
						(DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS)
					e.Value = record.UHarmOrderCoefBC[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.THDS) &&
									e.ColumnIndex < DgvHarmColumns.BLOCKC)
					e.Value = record.UHarmOrderCoefBC[e.ColumnIndex -
						(DgvHarmColumns.BLOCKB + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(settings_.FloatFormat);

				// phase C
				else if (e.ColumnIndex == DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.SUM)
					e.Value = record.UHarmSummForOrderMore1CA.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.SUM) &&
					e.ColumnIndex < (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS))
					e.Value = record.UHarmOrderValueCA[e.ColumnIndex -
						(DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.ORDER_VALUE1)].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex == DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS)
					e.Value = record.UHarmOrderCoefCA[0].ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > (DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.THDS) &&
									e.ColumnIndex < DgvHarmColumns.BLOCKN)
					e.Value = record.UHarmOrderCoefCA[e.ColumnIndex -
						(DgvHarmColumns.BLOCKC + DgvHarmBlockColumns.ORDER_COEF1) + 1].ToString(settings_.FloatFormat);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::dgvUlinHarm_CellValueNeeded");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}

		private void dgvUlinInterharm_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
		{
			try
			{
				AvgRecordFields record = listRecords_[e.RowIndex];

				if (e.ColumnIndex == DgvInterHarmColumns.TIME)
					e.Value = record.DtStart.ToString("dd.MM.yyyy HH:mm:ss");
				else if (e.ColumnIndex == DgvInterHarmColumns.MARKED)
					e.Value = record.BRecordIsMarked ? "yes" : "no";
				// phase A
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKA)
					e.Value = record.UInterHarmAvgSquareAB.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKA && e.ColumnIndex < DgvInterHarmColumns.BLOCKB)
					e.Value = record.UInterHarmAvgSquareOrderAB[e.ColumnIndex - DgvInterHarmColumns.BLOCKA - 1].
						ToString(settings_.FloatFormat);

				// phase B
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKB)
					e.Value = record.UInterHarmAvgSquareBC.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKB && e.ColumnIndex < DgvInterHarmColumns.BLOCKC)
					e.Value = record.UInterHarmAvgSquareOrderBC[e.ColumnIndex - DgvInterHarmColumns.BLOCKB - 1].
						ToString(settings_.FloatFormat);

				// phase C
				else if (e.ColumnIndex == DgvInterHarmColumns.BLOCKC)
					e.Value = record.UInterHarmAvgSquareCA.ToString(settings_.FloatFormat);

				else if (e.ColumnIndex > DgvInterHarmColumns.BLOCKC && e.ColumnIndex < DgvInterHarmColumns.BLOCKN)
					e.Value = record.UInterHarmAvgSquareOrderCA[e.ColumnIndex - DgvInterHarmColumns.BLOCKC - 1].
						ToString(settings_.FloatFormat);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlAvgPage::FillUlinInterHarmonicsData");
				EmService.WriteToLogFailed(string.Format("row: {0}, column: {1}", e.RowIndex, e.ColumnIndex));
				throw;
			}
		}
	}
}
