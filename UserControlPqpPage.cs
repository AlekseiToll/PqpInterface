using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

using EmServiceLib;
using ExportToExcel;
using FileAnalyzerLib;
using ZedGraph;
using AxisType = ZedGraph.AxisType;

namespace MainInterface
{
	public partial class UserControlPqpPage : UserControl
	{
		private static readonly string ConstraintsFormat = "0.00";

		enum WayToShowStatistics
		{
			NUMBER,
			PERCENTAGE,
			TIME
		}

		enum StaticticsColumns
		{
			PARAM = 0,
			NORM = 1,
			BETWEEN = 2,
			OUT = 3,
			NORM_PERC = 4,
			BETW_PERC = 5,
			OUT_PERC = 6,
			NORM_TIME = 7,
			BETW_TIME = 8,
			OUT_TIME = 9,
		}

		enum UStatisticRows
		{
			A_PLUS = 0,
			A_MINUS = 1,
			B_PLUS = 2,
			B_MINUS = 3,
			C_PLUS = 4,
			C_MINUS = 5
		}

		private RegistrationInfo regInfo_;
		private PqpArchiveInfo archiveInfo_;
		private PqpArchiveFields archiveFields_;
		private FormMain formMain_;
		private EmSettings settings_;

		private WayToShowStatistics curWayToShowSamples_ = WayToShowStatistics.NUMBER;

		// peak load time values
		private DateTime dtMaxModeStart1_ = DateTime.MinValue;
		private DateTime dtMaxModeEnd1_ = DateTime.MinValue;
		private DateTime dtMaxModeStart2_ = DateTime.MinValue;
		private DateTime dtMaxModeEnd2_ = DateTime.MinValue;
		// уставки для той же цели
		private float constrNPLtopMax_ = Single.NaN;		// Max - для наибольших нагрузок
		private float constrNPLtopMin_ = Single.NaN;		// Min - для наименьших
		private float constrNPLbottomMax_ = Single.NaN;
		private float constrNPLbottomMin_ = Single.NaN;
		private float constrUPLtopMax_ = Single.NaN;
		private float constrUPLtopMin_ = Single.NaN;
		private float constrUPLbottomMax_ = Single.NaN;
		private float constrUPLbottomMin_ = Single.NaN;
		// переменная указывает надо ли считать эти режимы
		private bool bNeedMaxModeForEtPQP_A_ = false;

		private short currentHarmonic_ = 1;		// for nonsinus: harmonic which is shown now

		public UserControlPqpPage()
		{
			InitializeComponent();

			zgcF.GraphPane.XAxis.Type = AxisType.Date;
			zgcUA.GraphPane.XAxis.Type = AxisType.Date;
			zgcUB.GraphPane.XAxis.Type = AxisType.Date;
			zgcUC.GraphPane.XAxis.Type = AxisType.Date;
			zgcFlicker.GraphPane.XAxis.Type = AxisType.Date;
		}

		public void ShowData(PqpArchiveInfo archiveInfo, ref RegistrationInfo regInfo, 
							ref FormMain formMain, ref EmSettings settings)
		{
			regInfo_ = regInfo;
			//if(pqpArchiveIndex < regInfo.PqpArchives.Count)
			//	archiveInfo_ = regInfo.PqpArchives[pqpArchiveIndex];
			//else EmService.WriteToLogFailed("UserControlPqpPage() index error");
			archiveInfo_ = archiveInfo;
			formMain_ = formMain;
			settings_ = settings;

			labelPqpStartData.Text = archiveInfo_.DtStart.ToString("dd-MM-yyyy HH:mm:ss");
			labelPqpEndData.Text = archiveInfo_.DtEnd.ToString("dd-MM-yyyy HH:mm:ss");

			try
			{
				//CultureInfo ci_enUS = new CultureInfo("en-US");

				if (regInfo_ == null || archiveInfo_ == null)
				{
					EmService.WriteToLogFailed("UserControlPqpPage_Load: null error");
					return;
				}

				if (!FileAnalyzer.GetPqpArchiveFields(archiveInfo_.Path, out archiveFields_, ref regInfo_))
				{
					EmService.WriteToLogFailed("UserControlPqpPage_Load: error");
					MessageBox.Show("Error while analyzing PQP archive", "Error",
										MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// frequency
				FillFrequencyData();

				// voltage
				FillVoltageData(false);

				// nonsinus
				rbNonsinusChartMaxB.Enabled = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				rbNonsinusChartUpperB.Enabled = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				rbNonsinusChartMaxC.Enabled = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				rbNonsinusChartUpperC.Enabled = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				FillNonsinusData(false, false);

				// nonsymmetry
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					FillNonsymmetryData();
				else tpNonsymmetry.Visible = false;

				// ficker
				FillFlickerData();

				// interharmonics
				tsbInterharmB.Enabled = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				tsbInterharmC.Enabled = regInfo_.ConnectionScheme != ConnectScheme.Ph1W2;
				FillInterharmonicsData();

				// events (dip and swell)
				FillEventsData();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::ShowData");
				throw;
			}
		}

		private void FillNonsymmetryData()
		{
			try
			{
				#region Statistics

				dgvNonsymmStatistics.Rows.Clear();

				float percentNorm = 0, percentBetween = 0, percentOut = 0;
				Int64 coefTime = 600 * TimeSpan.TicksPerSecond; // 60 seconds

				dgvNonsymmStatistics.Rows.Add(2);	// 2 row: k2u and k0u
				int K_2U = 0, K_0U = 1;		// row indexes

				// K 2U
				// numbers
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.PARAM].Value = "K 2U";
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.K2UcounterNonflaggedNormal.ToString();
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.K2UcounterNonflaggedT1.ToString();
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.K2UcounterNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.K2UcounterNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.K2UcounterNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.K2UcounterNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.K2UcounterNonflaggedNormal * coefTime).ToString();
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.K2UcounterNonflaggedT1 * coefTime).ToString();
				dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.K2UcounterNonflaggedT2 * coefTime).ToString();
				// colors
				if (archiveFields_.K2UcounterNonflaggedNormal > 0)
				{
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.NORM].Style.BackColor =
						Color.LightGreen;
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
						Color.LightGreen;
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
						Color.LightGreen;
				}
				if (archiveFields_.K2UcounterNonflaggedT1 > 0)
				{
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor =
						Color.Yellow;
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor =
						Color.Yellow;
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor =
						Color.Yellow;
				}
				if (archiveFields_.K2UcounterNonflaggedT1 > 0)
				{
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.OUT].Style.BackColor =
						Color.Salmon;
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor =
						Color.Salmon;
					dgvNonsymmStatistics.Rows[K_2U].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor =
						Color.Salmon;
				}	

				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W4)
				{
					// K 0U
					// numbers
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.PARAM].Value = "K 0U";
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.NORM].Value =
						archiveFields_.K0UcounterNonflaggedNormal.ToString();
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.BETWEEN].Value =
						archiveFields_.K0UcounterNonflaggedT1.ToString();
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.OUT].Value =
						archiveFields_.K0UcounterNonflaggedT2.ToString();
					// percent
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						percentNorm = archiveFields_.K0UcounterNonflaggedNormal * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentBetween = archiveFields_.K0UcounterNonflaggedT1 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentOut = archiveFields_.K0UcounterNonflaggedT2 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
					}
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.NORM_PERC].Value =
						percentNorm.ToString(settings_.FloatFormat);
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.BETW_PERC].Value =
						percentBetween.ToString(settings_.FloatFormat);
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.OUT_PERC].Value =
						percentOut.ToString(settings_.FloatFormat);
					// time
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.NORM_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.K0UcounterNonflaggedNormal * coefTime).ToString();
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.BETW_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.K0UcounterNonflaggedT1 * coefTime).ToString();
					dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.OUT_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.K0UcounterNonflaggedT2 * coefTime).ToString();
					// colors
					if (archiveFields_.K0UcounterNonflaggedNormal > 0)
					{
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.NORM].Style.BackColor =
							Color.LightGreen;
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
							Color.LightGreen;
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
							Color.LightGreen;
					}
					if (archiveFields_.K0UcounterNonflaggedT1 > 0)
					{
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor =
							Color.Yellow;
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor =
							Color.Yellow;
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor =
							Color.Yellow;
					}
					if (archiveFields_.K0UcounterNonflaggedT1 > 0)
					{
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.OUT].Style.BackColor =
							Color.Salmon;
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor =
							Color.Salmon;
						dgvNonsymmStatistics.Rows[K_0U].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor =
							Color.Salmon;
					}
				}

				#endregion

				// sample
				lvNonsymmSample.Items.Clear();

				ListViewItem itm = new ListViewItem("All");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
				lvNonsymmSample.Items.Add(itm);
				itm = new ListViewItem("Marked");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
				lvNonsymmSample.Items.Add(itm);
				itm = new ListViewItem("Not Marked");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
				lvNonsymmSample.Items.Add(itm);

				#region Values (max and min)

				lvNonsymmValues.Items.Clear();

				itm = new ListViewItem("K 2U");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.K2U95.ToString(settings_.FloatFormat));
				itm.SubItems.Add(archiveFields_.K2U100.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.K2u95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.K2u100.ToString(ConstraintsFormat));
				lvNonsymmValues.Items.Add(itm);

				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W4)
				{
					itm = new ListViewItem("K 0U");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.K0U95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.K0U100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.K0u95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.K0u100.ToString(ConstraintsFormat));
					lvNonsymmValues.Items.Add(itm);
				}

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillNonsymmetryData");
				throw;
			}
		}

		private void FillFrequencyData()
		{
			try
			{
				// Clear all
				dgvFStatistics.Rows.Clear();
				lvFSample.Items.Clear();
				lvFValues.Items.Clear();
				dgvFreq.Rows.Clear();
				zgcF.GraphPane.CurveList.Clear();
				// if no valid data then return
				if (!archiveFields_.BoolFreqDeviationStatistics) return;

				float percentNorm = 0, percentBetween = 0, percentOut = 0;
				Int64 coefTime = 10 * TimeSpan.TicksPerSecond; // 10 seconds

				dgvFStatistics.Rows.Add(1);		// there is only one string in the table
				// numbers
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.PARAM].Value = "ΔF";
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.FreqDeviationCounterLockedNormal.ToString();
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.FreqDeviationCounterLockedT1.ToString();
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.FreqDeviationCounterLockedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.FreqDeviationCounterLockedNormal * 100.0F /
								  archiveFields_.FreqDeviationCounterLocked;
					percentBetween = archiveFields_.FreqDeviationCounterLockedT1 * 100.0F /
								  archiveFields_.FreqDeviationCounterLocked;
					percentOut = archiveFields_.FreqDeviationCounterLockedT2 * 100.0F /
								  archiveFields_.FreqDeviationCounterLocked;
				}
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.FreqDeviationCounterLockedNormal * coefTime).ToString();
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.FreqDeviationCounterLockedT1 * coefTime).ToString();
				dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.FreqDeviationCounterLockedT2 * coefTime).ToString();

				if (archiveFields_.FreqDeviationCounterLockedNormal > 0)
				{
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.NORM].Style.BackColor = Color.LightGreen;
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor = Color.LightGreen;
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.FreqDeviationCounterLockedT1 > 0)
				{
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor = Color.Yellow;
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor = Color.Yellow;
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.FreqDeviationCounterLockedT2 > 0)
				{
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor = Color.Salmon;
					dgvFStatistics.Rows[0].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor = Color.Salmon;
				}

				// sample
				ListViewItem itm = new ListViewItem("All");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationCounterTotal.ToString());
				lvFSample.Items.Add(itm);
				itm = new ListViewItem("Synchro");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationCounterLocked.ToString());
				lvFSample.Items.Add(itm);
				itm = new ListViewItem("Not Synchro");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationCounterNonlocked.ToString());
				lvFSample.Items.Add(itm);

				#region Values

				// values (max and min)
				itm = new ListViewItem("Max");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationUp100.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.FSynchroUp100.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.FIsolateUp100.ToString(ConstraintsFormat));
				lvFValues.Items.Add(itm);
				itm = new ListViewItem("Upper");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationUp95.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.FSynchroUp95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.FIsolateUp95.ToString(ConstraintsFormat));
				lvFValues.Items.Add(itm);
				itm = new ListViewItem("Lower");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationDown95.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.FSynchroDown95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.FIsolateDown95.ToString(ConstraintsFormat));
				lvFValues.Items.Add(itm);
				itm = new ListViewItem("Min");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.FreqDeviationDown100.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.FSynchroDown100.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.FIsolateDown100.ToString(ConstraintsFormat));
				lvFValues.Items.Add(itm);

				// values table
				DateTime dtCurTime = archiveInfo_.DtStart;
				if (archiveFields_.FreqDeviationCounterTotal > 0)
				{
					dgvFreq.Rows.Add(archiveFields_.FreqDeviationCounterTotal);
					for (int iRow = 0; iRow < archiveFields_.FreqDeviationCounterTotal; ++iRow)
					{
						dgvFreq.Rows[iRow].Cells[0].Value = dtCurTime.ToString("dd.MM.yyyy HH:mm:ss");

						if (archiveFields_.FreqValuesValid[iRow])	// if value is valid
							dgvFreq.Rows[iRow].Cells[1].Value =
								archiveFields_.FreqDeviation[iRow].ToString(settings_.FloatFormat);
						else dgvFreq.Rows[iRow].Cells[1].Value = string.Empty;	// if not valid

						dtCurTime = dtCurTime.AddSeconds(10);
					}
				}

				// graph
				AddUFCurver(ref zgcF, ref dgvFreq, 0, 1, Color.DarkBlue, 1.5f, "Frequency Values", archiveFields_.FreqValuesValid);

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillIFrequencyData");
				throw;
			}
		}

		private void FillInterharmonicsData()
		{
			try
			{
				dgvInterharm.Rows.Clear();

				if (PqpArchiveFields.VoltageInterhartCount > 0)
				{
					short curParam = 0;
					dgvInterharm.Rows.Add(PqpArchiveFields.VoltageInterhartCount);
					for (int iRow = 0; iRow < PqpArchiveFields.VoltageInterhartCount; ++iRow)
					{
						dgvInterharm.Rows[iRow].Cells[0].Value = curParam.ToString();
						dgvInterharm.Rows[iRow].Cells[1].Value =
							archiveFields_.UAABInterharm[iRow].ToString(settings_.FloatFormat);

						if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
						{
							dgvInterharm.Rows[iRow].Cells[2].Value =
								archiveFields_.UBBCInterharm[iRow].ToString(settings_.FloatFormat);
							dgvInterharm.Rows[iRow].Cells[3].Value =
								archiveFields_.UCCAInterharm[iRow].ToString(settings_.FloatFormat);
						}

						curParam += 1;
					}
				}

				zgcInterharm.GraphPane.CurveList.Clear();
				DrawInterharmChart(ref zgcInterharm, ref archiveFields_.UAABInterharm,
				                   ref archiveFields_.UBBCInterharm, ref archiveFields_.UCCAInterharm);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillInterharmonicsData");
				throw;
			}
		}

		private void DrawInterharmChart(ref ZedGraphControl zg, ref float[] valuesA, ref float[] valuesB,
									ref float[] valuesC)
		{
			try
			{
				List<double> listValuesA = new List<double>();
				List<double> listValuesB = new List<double>();
				List<double> listValuesC = new List<double>();

				foreach (var v in valuesA) listValuesA.Add(v);
				foreach (var v in valuesB) listValuesB.Add(v);
				foreach (var v in valuesC) listValuesC.Add(v);

				// get a reference to the GraphPane
				GraphPane myPane = zg.GraphPane;
				//listValuesA[4] = 4;//  for test
				//listValuesB[4] = 10;
				//listValuesC[7] = 3;
				//listValuesA[7] = 10;
				//listValuesB[26] = 9;
				//listValuesC[26] = 10;
				//listValuesA[7] = 10;
				//listValuesB[26] = 5;
				//listValuesC[26] = 10;
				//listValuesA[7] = 10;
				//listValuesB[26] = 7;
				//listValuesC[26] = 10;
				//listValuesA[7] = 10;
				//listValuesB[26] = 6;
				//listValuesC[26] = 10;

				myPane.CurveList.Clear();

				// Set the Titles
				myPane.Title.Text = "Interharmonics";
				myPane.XAxis.Title.Text = "";
				myPane.YAxis.Title.Text = "";

				// Make up some random data points
				string[] labels =
					{
						"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10",
						"11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
						"21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
						"31", "32", "33", "34", "35", "36", "37", "38", "39", "40"
					};
				//double[] y = {100, 115, 75, 22, 98, 40};
				//double[] y2 = {90, 100, 95, 35, 80, 35};
				//double[] y3 = {80, 110, 65, 15, 54, 67};
				//double[] y4 = {120, 125, 100, 40, 105, 75};

				BarItem myBar;
				if (tsbInterharmA.Checked)
				{
					myBar = myPane.AddBar("Phase A", null, listValuesA.ToArray(), Color.Orange);
					myBar.Bar.Fill = new Fill(Color.Orange, Color.Yellow, Color.Orange);
				}

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (tsbInterharmB.Checked)
					{
						myBar = myPane.AddBar("Phase B", null, listValuesB.ToArray(), Color.Green);
						myBar.Bar.Fill = new Fill(Color.Green, Color.Lime, Color.LimeGreen);
					}

					if (tsbInterharmC.Checked)
					{
						myBar = myPane.AddBar("Phase C", null, listValuesC.ToArray(), Color.Red);
						myBar.Bar.Fill = new Fill(Color.Red, Color.LightPink, Color.Fuchsia);
					}
				}

				// Fix up the curve attributes a little
				//myCurve.Symbol.Size = 8.0F;
				//myCurve.Symbol.Fill = new Fill(Color.White);
				//myCurve.Line.Width = 2.0F;

				// Draw the X tics between the labels instead of at the labels
				myPane.XAxis.MajorTic.IsBetweenLabels = false;

				// Set the XAxis labels
				myPane.XAxis.Scale.TextLabels = labels;
				// Set the XAxis to Text type
				myPane.XAxis.Type = AxisType.Text;
				// Fill the Axis and Pane backgrounds
				myPane.Chart.Fill = new Fill(Color.AntiqueWhite, Color.White, 90F);
				myPane.Fill = new Fill(Color.White);

				// Tell ZedGraph to refigure the axes since the data have changed
				zg.AxisChange();
				zg.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::DrawInterharmChart");
				throw;
			}
		}

		private void FillEventsData()
		{
			try
			{
				#region Dips

				lvDips.Items.Clear();

				ListViewItem itm = new ListViewItem("90>u≥85%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_90_till_85,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_90_till_85,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_90_till_85,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_90_till_85,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_90_till_85,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_90_till_85,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvDips.Items.Add(itm);

				itm = new ListViewItem("85>u≥70%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_85_till_70,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_85_till_70,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_85_till_70,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_85_till_70,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_85_till_70,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_85_till_70,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvDips.Items.Add(itm);

				itm = new ListViewItem("70>u≥40%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_70_till_40,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_70_till_40,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_70_till_40,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_70_till_40,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_70_till_40,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_70_till_40,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvDips.Items.Add(itm);

				itm = new ListViewItem("40>u≥10%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_40_till_10,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_40_till_10,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_40_till_10,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_40_till_10,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_40_till_10,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_40_till_10,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvDips.Items.Add(itm);

				itm = new ListViewItem("10>u≥0%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_10_till_0,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_10_till_0,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_10_till_0,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_10_till_0,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_10_till_0,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.UD_10_till_0,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvDips.Items.Add(itm);

				#endregion

				#region Swells

				lvSwells.Items.Clear();

				itm = new ListViewItem("110<u≤120%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_110_till_120,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_110_till_120,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_110_till_120,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_110_till_120,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_110_till_120,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_110_till_120,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvSwells.Items.Add(itm);

				itm = new ListViewItem("120<u≤140%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_120_till_140,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_120_till_140,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_120_till_140,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_120_till_140,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_120_till_140,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_120_till_140,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvSwells.Items.Add(itm);

				itm = new ListViewItem("140<u≤160%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_140_till_160,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_140_till_160,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_140_till_160,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_140_till_160,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_140_till_160,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_140_till_160,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvSwells.Items.Add(itm);

				itm = new ListViewItem("160<u≤180%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_160_till_180,
														(int)DurationDipSwell.T_0_01_till_0_2].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_160_till_180,
														(int)DurationDipSwell.T_0_2_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_160_till_180,
														(int)DurationDipSwell.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_160_till_180,
														(int)DurationDipSwell.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_160_till_180,
														(int)DurationDipSwell.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.DipSwell[(int)DeviationDipSwell.US_160_till_180,
														(int)DurationDipSwell.T_20_till_60].ToString());
				lvSwells.Items.Add(itm);

				#endregion

				lvInterrupt.Items.Clear();

				itm = new ListViewItem("5>u≥0%");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_0_till_0_5].ToString());
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_0_5_till_1].ToString());
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_1_till_5].ToString());
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_5_till_20].ToString());
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_20_till_60].ToString());
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_60_till_180].ToString());
				itm.SubItems.Add(archiveFields_.Interrupt[(int)DurationInterrupt.T_Over_180].ToString());
				itm.SubItems.Add(archiveFields_.InterruptionMaxDuration.ToString());
				lvInterrupt.Items.Add(itm);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillEventsData");
				throw;
			}
		}

		private void FillFlickerData()
		{
			try
			{
				#region Main Tables

				#region Statistics

				dgvFlikStatST.Rows.Clear();
				dgvFlikStatLT.Rows.Clear();

				float percentNorm = 0, percentBetween = 0, percentOut = 0;
				Int64 coefTime = 600 * TimeSpan.TicksPerSecond; // 60 seconds
				int rowA = 0, rowB = 1, rowC = 2;

				// ST //////////////////////
				dgvFlikStatST.Rows.Add(3);		// 3 phases: A, B, C
				// numbers
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.PARAM].Value = "A";
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.UAABFlickerPstCounterNonflaggedNormal.ToString();
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.UAABFlickerPstCounterNonflaggedT1.ToString();
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.UAABFlickerPstCounterNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UAABFlickerPstCounterNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UAABFlickerPstCounterNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UAABFlickerPstCounterNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABFlickerPstCounterNonflaggedNormal * coefTime).ToString();
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABFlickerPstCounterNonflaggedT1 * coefTime).ToString();
				dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABFlickerPstCounterNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UAABFlickerPstCounterNonflaggedNormal > 0)
				{
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.NORM].Style.BackColor = Color.LightGreen;
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor = Color.LightGreen;
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UAABFlickerPstCounterNonflaggedT1 > 0)
				{
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor = Color.Yellow;
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor = Color.Yellow;
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UAABFlickerPstCounterNonflaggedT2 > 0)
				{
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor = Color.Salmon;
					dgvFlikStatST.Rows[rowA].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor = Color.Salmon;
				}

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					// numbers
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.PARAM].Value = "B";
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.NORM].Value =
						archiveFields_.UBBCFlickerPstCounterNonflaggedNormal.ToString();
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.BETWEEN].Value =
						archiveFields_.UBBCFlickerPstCounterNonflaggedT1.ToString();
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.OUT].Value =
						archiveFields_.UBBCFlickerPstCounterNonflaggedT2.ToString();
					// percent
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						percentNorm = archiveFields_.UBBCFlickerPstCounterNonflaggedNormal * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentBetween = archiveFields_.UBBCFlickerPstCounterNonflaggedT1 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentOut = archiveFields_.UBBCFlickerPstCounterNonflaggedT2 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
					}
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.NORM_PERC].Value =
						percentNorm.ToString(settings_.FloatFormat);
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.BETW_PERC].Value =
						percentBetween.ToString(settings_.FloatFormat);
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.OUT_PERC].Value =
						percentOut.ToString(settings_.FloatFormat);
					// time
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.NORM_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UBBCFlickerPstCounterNonflaggedNormal * coefTime).ToString();
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.BETW_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UBBCFlickerPstCounterNonflaggedT1 * coefTime).ToString();
					dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.OUT_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UBBCFlickerPstCounterNonflaggedT2 * coefTime).ToString();

					if (archiveFields_.UBBCFlickerPstCounterNonflaggedNormal > 0)
					{
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.NORM].Style.BackColor = Color.LightGreen;
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
							Color.LightGreen;
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
							Color.LightGreen;
					}
					if (archiveFields_.UBBCFlickerPstCounterNonflaggedT1 > 0)
					{
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor = Color.Yellow;
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor = Color.Yellow;
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor = Color.Yellow;
					}
					if (archiveFields_.UBBCFlickerPstCounterNonflaggedT2 > 0)
					{
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor = Color.Salmon;
						dgvFlikStatST.Rows[rowB].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor = Color.Salmon;
					}

					// numbers
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.PARAM].Value = "C";
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.NORM].Value =
						archiveFields_.UCCAFlickerPstCounterNonflaggedNormal.ToString();
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.BETWEEN].Value =
						archiveFields_.UCCAFlickerPstCounterNonflaggedT1.ToString();
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.OUT].Value =
						archiveFields_.UCCAFlickerPstCounterNonflaggedT2.ToString();
					// percent
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						percentNorm = archiveFields_.UCCAFlickerPstCounterNonflaggedNormal * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentBetween = archiveFields_.UCCAFlickerPstCounterNonflaggedT1 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentOut = archiveFields_.UCCAFlickerPstCounterNonflaggedT2 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
					}
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.NORM_PERC].Value =
						percentNorm.ToString(settings_.FloatFormat);
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.BETW_PERC].Value =
						percentBetween.ToString(settings_.FloatFormat);
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.OUT_PERC].Value =
						percentOut.ToString(settings_.FloatFormat);
					// time
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.NORM_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UCCAFlickerPstCounterNonflaggedNormal * coefTime).ToString();
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.BETW_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UCCAFlickerPstCounterNonflaggedT1 * coefTime).ToString();
					dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.OUT_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UCCAFlickerPstCounterNonflaggedT2 * coefTime).ToString();

					if (archiveFields_.UCCAFlickerPstCounterNonflaggedNormal > 0)
					{
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.NORM].Style.BackColor = Color.LightGreen;
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
							Color.LightGreen;
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
							Color.LightGreen;
					}
					if (archiveFields_.UCCAFlickerPstCounterNonflaggedT1 > 0)
					{
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor = Color.Yellow;
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor = Color.Yellow;
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor = Color.Yellow;
					}
					if (archiveFields_.UCCAFlickerPstCounterNonflaggedT2 > 0)
					{
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor = Color.Salmon;
						dgvFlikStatST.Rows[rowC].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor = Color.Salmon;
					}
				}

				// LT //////////////////////
				if (archiveFields_.FlickerPltCounterNonflagged > 0)
				{
					dgvFlikStatLT.Rows.Add(3);		// 3 phases: A, B, C
					// numbers
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.PARAM].Value = "A";
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.NORM].Value =
						archiveFields_.UAABFlickerPltCounterNonflaggedNormal.ToString();
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.BETWEEN].Value =
						archiveFields_.UAABFlickerPltCounterNonflaggedT1.ToString();
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.OUT].Value =
						archiveFields_.UAABFlickerPltCounterNonflaggedT2.ToString();
					// percent
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						percentNorm = archiveFields_.UAABFlickerPltCounterNonflaggedNormal * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentBetween = archiveFields_.UAABFlickerPltCounterNonflaggedT1 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
						percentOut = archiveFields_.UAABFlickerPltCounterNonflaggedT2 * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged;
					}
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.NORM_PERC].Value =
						percentNorm.ToString(settings_.FloatFormat);
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.BETW_PERC].Value =
						percentBetween.ToString(settings_.FloatFormat);
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.OUT_PERC].Value =
						percentOut.ToString(settings_.FloatFormat);
					// time
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.NORM_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UAABFlickerPltCounterNonflaggedNormal * coefTime).ToString();
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.BETW_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UAABFlickerPltCounterNonflaggedT1 * coefTime).ToString();
					dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.OUT_TIME].Value =
						TimeSpan.FromTicks(archiveFields_.UAABFlickerPltCounterNonflaggedT2 * coefTime).ToString();

					if (archiveFields_.UAABFlickerPltCounterNonflaggedNormal > 0)
					{
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.NORM].Style.BackColor = Color.LightGreen;
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
							Color.LightGreen;
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
							Color.LightGreen;
					}
					if (archiveFields_.UAABFlickerPltCounterNonflaggedT1 > 0)
					{
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor = Color.Yellow;
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor = Color.Yellow;
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor = Color.Yellow;
					}
					if (archiveFields_.UAABFlickerPltCounterNonflaggedT2 > 0)
					{
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor = Color.Salmon;
						dgvFlikStatLT.Rows[rowA].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor = Color.Salmon;
					}

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						// numbers
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.PARAM].Value = "B";
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.NORM].Value =
							archiveFields_.UBBCFlickerPltCounterNonflaggedNormal.ToString();
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.BETWEEN].Value =
							archiveFields_.UBBCFlickerPltCounterNonflaggedT1.ToString();
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.OUT].Value =
							archiveFields_.UBBCFlickerPltCounterNonflaggedT2.ToString();
						// percent
						if (archiveFields_.TenMinuteCounterNonflagged > 0)
						{
							percentNorm = archiveFields_.UBBCFlickerPltCounterNonflaggedNormal * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged;
							percentBetween = archiveFields_.UBBCFlickerPltCounterNonflaggedT1 * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged;
							percentOut = archiveFields_.UBBCFlickerPltCounterNonflaggedT2 * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged;
						}
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.NORM_PERC].Value =
							percentNorm.ToString(settings_.FloatFormat);
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.BETW_PERC].Value =
							percentBetween.ToString(settings_.FloatFormat);
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.OUT_PERC].Value =
							percentOut.ToString(settings_.FloatFormat);
						// time
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.NORM_TIME].Value =
							TimeSpan.FromTicks(archiveFields_.UBBCFlickerPltCounterNonflaggedNormal * coefTime).
							ToString();
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.BETW_TIME].Value =
							TimeSpan.FromTicks(archiveFields_.UBBCFlickerPltCounterNonflaggedT1 * coefTime).ToString();
						dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.OUT_TIME].Value =
							TimeSpan.FromTicks(archiveFields_.UBBCFlickerPltCounterNonflaggedT2 * coefTime).ToString();

						if (archiveFields_.UBBCFlickerPltCounterNonflaggedNormal > 0)
						{
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.NORM].Style.BackColor =
								Color.LightGreen;
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
								Color.LightGreen;
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
								Color.LightGreen;
						}
						if (archiveFields_.UBBCFlickerPltCounterNonflaggedT1 > 0)
						{
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor =
								Color.Yellow;
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor =
								Color.Yellow;
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor =
								Color.Yellow;
						}
						if (archiveFields_.UBBCFlickerPltCounterNonflaggedT2 > 0)
						{
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor =
								Color.Salmon;
							dgvFlikStatLT.Rows[rowB].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor =
								Color.Salmon;
						}

						// numbers
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.PARAM].Value = "C";
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.NORM].Value =
							archiveFields_.UCCAFlickerPltCounterNonflaggedNormal.ToString();
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.BETWEEN].Value =
							archiveFields_.UCCAFlickerPltCounterNonflaggedT1.ToString();
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.OUT].Value =
							archiveFields_.UCCAFlickerPltCounterNonflaggedT2.ToString();
						// percent
						if (archiveFields_.TenMinuteCounterNonflagged > 0)
						{
							percentNorm = archiveFields_.UCCAFlickerPltCounterNonflaggedNormal * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged;
							percentBetween = archiveFields_.UCCAFlickerPltCounterNonflaggedT1 * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged;
							percentOut = archiveFields_.UCCAFlickerPltCounterNonflaggedT2 * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged;
						}
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.NORM_PERC].Value =
							percentNorm.ToString(settings_.FloatFormat);
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.BETW_PERC].Value =
							percentBetween.ToString(settings_.FloatFormat);
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.OUT_PERC].Value =
							percentOut.ToString(settings_.FloatFormat);
						// time
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.NORM_TIME].Value =
							TimeSpan.FromTicks(archiveFields_.UCCAFlickerPltCounterNonflaggedNormal * coefTime).
							ToString();
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.BETW_TIME].Value =
							TimeSpan.FromTicks(archiveFields_.UCCAFlickerPltCounterNonflaggedT1 * coefTime).ToString();
						dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.OUT_TIME].Value =
							TimeSpan.FromTicks(archiveFields_.UCCAFlickerPltCounterNonflaggedT2 * coefTime).ToString();

						if (archiveFields_.UCCAFlickerPltCounterNonflaggedNormal > 0)
						{
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.NORM].Style.BackColor =
								Color.LightGreen;
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.NORM_PERC].Style.BackColor =
								Color.LightGreen;
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.NORM_TIME].Style.BackColor =
								Color.LightGreen;
						}
						if (archiveFields_.UCCAFlickerPltCounterNonflaggedT1 > 0)
						{
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.BETWEEN].Style.BackColor =
								Color.Yellow;
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.BETW_PERC].Style.BackColor =
								Color.Yellow;
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.BETW_TIME].Style.BackColor =
								Color.Yellow;
						}
						if (archiveFields_.UCCAFlickerPltCounterNonflaggedT2 > 0)
						{
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.OUT].Style.BackColor = Color.Salmon;
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.OUT_PERC].Style.BackColor =
								Color.Salmon;
							dgvFlikStatLT.Rows[rowC].Cells[(int)StaticticsColumns.OUT_TIME].Style.BackColor =
								Color.Salmon;
						}
					}
				}

				#endregion

				// sample
				lvFlickSample.Items.Clear();

				ListViewItem itm = new ListViewItem("ST");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
				itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
				itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
				lvFlickSample.Items.Add(itm);
				if (archiveFields_.FlickerPltCounterNonflagged > 0)
				{
					itm = new ListViewItem("LT");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.FlickerPltCounterTotal.ToString());
					itm.SubItems.Add(archiveFields_.FlickerPltCounterFlagged.ToString());
					itm.SubItems.Add(archiveFields_.FlickerPltCounterNonflagged.ToString());
					lvFlickSample.Items.Add(itm);
				}

				#region Values (max and min)

				lvFlickValuesST.Items.Clear();
				lvFlickValuesLT.Items.Clear();

				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					itm = new ListViewItem("A");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UAABFlickerPst95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UAABFlickerPst100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.FlickShortUp95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.FlickShortUp100.ToString(ConstraintsFormat));
					lvFlickValuesST.Items.Add(itm);

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						itm = new ListViewItem("B");
						itm.UseItemStyleForSubItems = false;
						itm.SubItems.Add(archiveFields_.UBBCFlickerPst95.ToString(settings_.FloatFormat));
						itm.SubItems.Add(archiveFields_.UBBCFlickerPst100.ToString(settings_.FloatFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickShortUp95.ToString(ConstraintsFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickShortUp100.ToString(ConstraintsFormat));
						lvFlickValuesST.Items.Add(itm);

						itm = new ListViewItem("C");
						itm.UseItemStyleForSubItems = false;
						itm.SubItems.Add(archiveFields_.UCCAFlickerPst95.ToString(settings_.FloatFormat));
						itm.SubItems.Add(archiveFields_.UCCAFlickerPst100.ToString(settings_.FloatFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickShortUp95.ToString(ConstraintsFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickShortUp100.ToString(ConstraintsFormat));
						lvFlickValuesST.Items.Add(itm);
					}
				}

				if (archiveFields_.FlickerPltCounterNonflagged > 0)
				{
					itm = new ListViewItem("A");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UAABFlickerPlt95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UAABFlickerPlt100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.FlickLongUp95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.FlickLongUp100.ToString(ConstraintsFormat));
					lvFlickValuesLT.Items.Add(itm);

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						itm = new ListViewItem("B");
						itm.UseItemStyleForSubItems = false;
						itm.SubItems.Add(archiveFields_.UBBCFlickerPlt95.ToString(settings_.FloatFormat));
						itm.SubItems.Add(archiveFields_.UBBCFlickerPlt100.ToString(settings_.FloatFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickLongUp95.ToString(ConstraintsFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickLongUp100.ToString(ConstraintsFormat));
						lvFlickValuesLT.Items.Add(itm);

						itm = new ListViewItem("C");
						itm.UseItemStyleForSubItems = false;
						itm.SubItems.Add(archiveFields_.UCCAFlickerPlt95.ToString(settings_.FloatFormat));
						itm.SubItems.Add(archiveFields_.UCCAFlickerPlt100.ToString(settings_.FloatFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickLongUp95.ToString(ConstraintsFormat));
						itm.SubItems.Add(regInfo_.Constraints.FlickLongUp100.ToString(ConstraintsFormat));
						lvFlickValuesLT.Items.Add(itm);
					}
				}

				#endregion

				#endregion

				#region Values Tables

				dgvFlickValST.Rows.Clear();
				dgvFlickValLT.Rows.Clear();

				DateTime dtCurTime = archiveInfo_.DtStart;
				if (archiveFields_.TenMinuteCounterTotal > 0)
				{
					dgvFlickValST.Rows.Add(archiveFields_.TenMinuteCounterTotal);
					for (int iRow = 0; iRow < archiveFields_.TenMinuteCounterTotal; ++iRow)
					{
						dgvFlickValST.Rows[iRow].Cells[0].Value = dtCurTime.ToString("HH:mm:ss") + " - " +
							dtCurTime.AddMinutes(10).ToString("HH:mm:ss");
						if (archiveFields_.TenMinuteNotMarked[iRow])	// not marked
						{
							dgvFlickValST.Rows[iRow].Cells[1].Value =
								archiveFields_.UAABFlickerPst[iRow].ToString(settings_.FloatFormat);
							if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
							{
								dgvFlickValST.Rows[iRow].Cells[2].Value =
									archiveFields_.UBBCFlickerPst[iRow].ToString(settings_.FloatFormat);
								dgvFlickValST.Rows[iRow].Cells[3].Value =
									archiveFields_.UCCAFlickerPst[iRow].ToString(settings_.FloatFormat);
							}
						}
						else    // marked
						{
							dgvFlickValST.Rows[iRow].Cells[1].Value = string.Empty;
							if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
							{
								dgvFlickValST.Rows[iRow].Cells[2].Value = string.Empty;
								dgvFlickValST.Rows[iRow].Cells[3].Value = string.Empty;
							}
						}

						dtCurTime = dtCurTime.AddMinutes(10);
					}
				}

				dtCurTime = archiveInfo_.DtStart;
				if (archiveFields_.FlickerPltCounterTotal > 0)
				{
					dgvFlickValLT.Rows.Add(archiveFields_.FlickerPltCounterTotal);
					for (int iRow = 0; iRow < archiveFields_.FlickerPltCounterTotal; ++iRow)
					{
						dgvFlickValLT.Rows[iRow].Cells[0].Value = dtCurTime.ToString("HH:mm:ss") + " - " +
							dtCurTime.AddMinutes(10).ToString("HH:mm:ss");
						if (archiveFields_.FlickerPltValid[iRow] == 0)
						{
							dgvFlickValLT.Rows[iRow].Cells[1].Value =
								archiveFields_.UAABFlickerPlt[iRow].ToString(settings_.FloatFormat);
							if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
							{
								dgvFlickValLT.Rows[iRow].Cells[2].Value =
									archiveFields_.UBBCFlickerPlt[iRow].ToString(settings_.FloatFormat);
								dgvFlickValLT.Rows[iRow].Cells[3].Value =
									archiveFields_.UCCAFlickerPlt[iRow].ToString(settings_.FloatFormat);
							}
						}
						else
						{
							dgvFlickValLT.Rows[iRow].Cells[1].Value = string.Empty;
							if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
							{
								dgvFlickValLT.Rows[iRow].Cells[2].Value = string.Empty;
								dgvFlickValLT.Rows[iRow].Cells[3].Value = string.Empty;
							}
						}

						dtCurTime = dtCurTime.AddHours(2);
					}
				}

				#endregion

				//zgcFlicker.GraphPane.CurveList.Clear(); // in PaintFlickerCurves method
				PaintFlickerCurves(tsbFlickPhaseA.Checked, tsbFlickPhaseALong.Checked,
									tsbFlickPhaseB.Checked, tsbFlickPhaseBLong.Checked,
									tsbFlickPhaseC.Checked, tsbFlickPhaseCLong.Checked);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillFlickerData");
				throw;
			}
		}

		private void PaintFlickerCurves(bool phaseA, bool phaseALong,
										bool phaseB, bool phaseBLong,
										bool phaseC, bool phaseCLong)
		{
			try
			{
				zgcFlicker.GraphPane.CurveList.Clear();
				zgcFlicker.Refresh();

				if (phaseA)
					AddFlickerCurve(ref zgcFlicker, ref dgvFlickValST, 1, Color.Orange, 1.0F, 0, "A st");
				if (phaseALong)
					AddFlickerCurve(ref zgcFlicker, ref dgvFlickValLT, 1, Color.Orange, 2.0F, 1, "A lt");

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (phaseB)
						AddFlickerCurve(ref zgcFlicker, ref dgvFlickValST, 2, Color.Green, 1.0F, 0, "B st");
					if (phaseBLong)
						AddFlickerCurve(ref zgcFlicker, ref dgvFlickValLT, 2, Color.Green, 2.0F, 1, "B lt");

					if (phaseC)
						AddFlickerCurve(ref zgcFlicker, ref dgvFlickValST, 3, Color.Red, 1.0F, 0, "C st");
					if (phaseCLong)
						AddFlickerCurve(ref zgcFlicker, ref dgvFlickValLT, 3, Color.Red, 2.0F, 1, "C lt");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in flikker PaintFlickerCurves():");
				throw;
			}
		}

		private void AddFlickerCurve(ref ZedGraphControl zgc, ref DataGridView dg, int column,
								Color curveColor, float width, byte type, string legend)
		{
			try
			{
				if (dg.Rows.Count <= 0) return;

				// starting build point list
				GraphPane gPane = zgc.GraphPane;
				gPane.YAxis.MajorGrid.IsZeroLine = true;
				gPane.YAxis.MajorGrid.IsVisible = true;
				gPane.XAxis.MajorGrid.IsVisible = true;
				PointPairList list = new PointPairList();

				DateTime dt2;
				double y, x1;
				int startRow = 0;

				short curTFliker = Constants.TFlickerST;
				if (type == 1) curTFliker = Constants.TFlickerLT;

				//dt1 = ExtractTimeFromString(dg[startRow, 0].ToString(), 0);
				dt2 = ExtractTimeFromString(dg[0, startRow].Value.ToString(), 1);
				//dt1 = dt1.AddMinutes(-curTFliker);
				//dt2 = dt2.AddMinutes(-cur_t_fliker);
				short rule = -1;
				if (!String.IsNullOrEmpty(dg[column, startRow].Value.ToString()) &&
					dg[column, startRow].Value.ToString() != " " && dg[column, startRow].Value.ToString() != "-")
				{
					try
					{
						if (rule != -1)
							y = GetFlickerValue(dg[column, startRow].Value, rule);
						else
						{
							y = Convert.ToSingle(dg[column, startRow].Value);
							rule = 0;
						}
					}
					catch (FormatException)
					{
						y = Single.Parse(dg[column, startRow].Value.ToString(), new CultureInfo("en-US"));
						rule = 1;
					}

					//x0 = (double)new XDate(Convert.ToDateTime(dg[startRow, 0]).AddMinutes(-t_fliker_));
					//x1 = (double)new XDate(Convert.ToDateTime(dg[startRow, 0]));
					//x0 = (double)new XDate(dt1);
					x1 = (double)new XDate(dt2);
					//list.Add(x0, y);
					list.Add(x1, y);
				}
				double tmp_prev_value = Double.NaN;
				for (int iRow = startRow; iRow < (dg.Rows.Count - 1); iRow++)
				{
					//x0 = (double)new XDate(Convert.ToDateTime(dg[i, 0]));
					//dt1 = dt2;
					dt2 = dt2.AddMinutes(curTFliker);
					//x0 = (double)new XDate(dt1);
					//x1 = (double)new XDate(Convert.ToDateTime(dg[i + 1, 0]));
					x1 = (double)new XDate(dt2);

					if (!String.IsNullOrEmpty(dg[column, iRow + 1].Value.ToString()) &&
						dg[column, iRow + 1].Value.ToString() != " " && dg[column, iRow + 1].Value.ToString() != "-")
					{
						try
						{
							if (rule != -1)
								y = GetFlickerValue(dg[column, iRow + 1].Value, rule);
							else
							{
								y = Convert.ToSingle(dg[column, iRow + 1].Value);
								rule = 0;
							}
						}
						catch (FormatException)
						{
							y = Single.Parse(dg[column, iRow + 1].Value.ToString(), new CultureInfo("en-US"));
							rule = 1;
						}

						//list.Add(x0, y);
						list.Add(x1, y);
						tmp_prev_value = y;
					}
					else
					{
						if (!Double.IsNaN(tmp_prev_value))
						{
							//list.Add(x0, tmp_prev_value);
							list.Add(x1, tmp_prev_value);
						}
					}
				}

				// restoring sorting rule
				//SetSortRule(dg, SortRule);

				LineItem myCurve = gPane.AddCurve(legend, list, curveColor, SymbolType.None);
				myCurve.IsY2Axis = false;
				myCurve.Line.Width = width;

				//gPane.AxisChange(this.CreateGraphics());

				// Axis X, Y and Y2
				gPane.XAxis.IsVisible = tsbFlickXGridLine.Checked;
				//gPane.XAxis.IsShowGrid = tsXGridLine.Checked;
				//gPane.XAxis.IsShowMinorGrid = tsXMinorGridLine.Checked;

				gPane.YAxis.IsVisible = tsbFlickYGridLine.Checked;
				//gPane.YAxis.IsShowGrid = tsYGridLine.Checked;
				//gPane.YAxis.IsShowMinorGrid = tsYMinorGridLine.Checked;

				gPane.Y2Axis.IsVisible = tsbFlickY2GridLine.Checked;
				//gPane.Y2Axis.IsShowGrid = tsY2GridLine.Checked;
				//gPane.Y2Axis.IsShowMinorGrid = tsY2MinorGridLine.Checked;

				// Zoom set defalult
				Graphics g = zgc.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				zgc.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddFlickerCurve():");
				throw;
			}
		}

		private double GetFlickerValue(object s, short rule)
		{
			try
			{
				if (rule == 0)
				{
					try
					{
						return Convert.ToSingle(s, new CultureInfo("en-US"));
					}
					catch { return Convert.ToSingle(s, new CultureInfo("ru-RU")); }
				}
				else
					return Single.Parse(s.ToString(), new CultureInfo("en-US"));
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetFlikkerValue(): ");
				throw;
			}
		}

		/// <summary>
		/// Extract time from fliker string
		/// </summary>
		/// <param name="str"></param>
		/// <param name="type">0 - start time, 1 - end time</param>
		/// <returns></returns>
		private DateTime ExtractTimeFromString(string str, int type)
		{
			try
			{
				int i = 0;
				if (type == 1)
				{
					for (i = str.Length - 1; i >= 0; --i)
					{
						if ((!Char.IsDigit(str[i])) && (str[i] != ':')) break;
					}
					return Convert.ToDateTime(str.Substring(i + 1));
				}
				else
				{
					for (i = 0; i < str.Length; ++i)
					{
						if ((!Char.IsDigit(str[i])) && (str[i] != ':')) break;
					}
					return Convert.ToDateTime(str.Substring(0, i));
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in flikker ExtractTimeFromString(): ");
				throw;
			}
		}

		private void FillNonsinusData(bool newOrderOnly, bool statisticsOnly)
		{
			try
			{
				#region Statistics

				ListViewItem itm;
				int rowNorm = 0, rowBetween = 1, rowOut = 2;
				int columnParam = 0, columnA = 1, columnB = 2, columnC = 3;

				string valueA, valueB, valueC;
				valueA = valueB = valueC = string.Empty;
				Int64 coefTime = 600 * TimeSpan.TicksPerSecond; // 60 seconds

				// summary
				if (!newOrderOnly)
				{
					dgvNonsinusStat1.Rows.Clear();
					dgvNonsinusStat1.Rows.Add(3);	// 3: norm, between and out

					// Norm
					if (curWayToShowSamples_ == WayToShowStatistics.NUMBER)
					{
						valueA = archiveFields_.UAABKHarmCounterNonflaggedNormal[0].ToString();
						valueB = archiveFields_.UBBCKHarmCounterNonflaggedNormal[0].ToString();
						valueC = archiveFields_.UCCAKHarmCounterNonflaggedNormal[0].ToString();
					}
					else if (curWayToShowSamples_ == WayToShowStatistics.PERCENTAGE)
					{
						if (archiveFields_.TenMinuteCounterNonflagged > 0)
						{
							valueA = (archiveFields_.UAABKHarmCounterNonflaggedNormal[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
							valueB = (archiveFields_.UBBCKHarmCounterNonflaggedNormal[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
							valueC = (archiveFields_.UCCAKHarmCounterNonflaggedNormal[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						}
					}
					else  // time
					{
						valueA = TimeSpan.FromTicks(archiveFields_.UAABKHarmCounterNonflaggedNormal[0] * 
							coefTime).ToString();
						valueB = TimeSpan.FromTicks(archiveFields_.UBBCKHarmCounterNonflaggedNormal[0] * 
							coefTime).ToString();
						valueC = TimeSpan.FromTicks(archiveFields_.UCCAKHarmCounterNonflaggedNormal[0] * 
							coefTime).ToString();
					}

					dgvNonsinusStat1.Rows[rowNorm].Cells[columnParam].Value = "Within NPL";
					dgvNonsinusStat1.Rows[rowNorm].Cells[columnA].Value = valueA;
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						dgvNonsinusStat1.Rows[rowNorm].Cells[columnB].Value = valueB;
						dgvNonsinusStat1.Rows[rowNorm].Cells[columnC].Value = valueC;
					}
					
					if (archiveFields_.UAABKHarmCounterNonflaggedNormal[0] > 0)
					{
						dgvNonsinusStat1.Rows[rowNorm].Cells[columnA].Style.BackColor = Color.LightGreen;
					}
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						if (archiveFields_.UBBCKHarmCounterNonflaggedNormal[0] > 0)
						{
							dgvNonsinusStat1.Rows[rowNorm].Cells[columnB].Style.BackColor = Color.LightGreen;
						}
						if (archiveFields_.UCCAKHarmCounterNonflaggedNormal[0] > 0)
						{
							dgvNonsinusStat1.Rows[rowNorm].Cells[columnC].Style.BackColor = Color.LightGreen;
						}
					}

					// Between
					if (curWayToShowSamples_ == WayToShowStatistics.NUMBER)
					{
						valueA = archiveFields_.UAABKHarmCounterNonflaggedT1[0].ToString();
						valueB = archiveFields_.UBBCKHarmCounterNonflaggedT1[0].ToString();
						valueC = archiveFields_.UCCAKHarmCounterNonflaggedT1[0].ToString();
					}
					else if (curWayToShowSamples_ == WayToShowStatistics.PERCENTAGE)
					{
						if (archiveFields_.TenMinuteCounterNonflagged > 0)
						{
							valueA = (archiveFields_.UAABKHarmCounterNonflaggedT1[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
							valueB = (archiveFields_.UBBCKHarmCounterNonflaggedT1[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
							valueC = (archiveFields_.UCCAKHarmCounterNonflaggedT1[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						}
					}
					else  // time
					{
						valueA = TimeSpan.FromTicks(archiveFields_.UAABKHarmCounterNonflaggedT1[0] *
							coefTime).ToString();
						valueB = TimeSpan.FromTicks(archiveFields_.UBBCKHarmCounterNonflaggedT1[0] *
							coefTime).ToString();
						valueC = TimeSpan.FromTicks(archiveFields_.UCCAKHarmCounterNonflaggedT1[0] *
							coefTime).ToString();
					}

					dgvNonsinusStat1.Rows[rowBetween].Cells[columnParam].Value = "Betw. NPL and UPL";
					dgvNonsinusStat1.Rows[rowBetween].Cells[columnA].Value = valueA;
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						dgvNonsinusStat1.Rows[rowBetween].Cells[columnB].Value = valueB;
						dgvNonsinusStat1.Rows[rowBetween].Cells[columnC].Value = valueC;
					}

					if (archiveFields_.UAABKHarmCounterNonflaggedT1[0] > 0)
					{
						dgvNonsinusStat1.Rows[rowBetween].Cells[columnA].Style.BackColor = Color.Yellow;
					}
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						if (archiveFields_.UBBCKHarmCounterNonflaggedT1[0] > 0)
						{
							dgvNonsinusStat1.Rows[rowBetween].Cells[columnB].Style.BackColor = Color.Yellow;
						}
						if (archiveFields_.UCCAKHarmCounterNonflaggedT1[0] > 0)
						{
							dgvNonsinusStat1.Rows[rowBetween].Cells[columnC].Style.BackColor = Color.Yellow;
						}
					}

					// Out
					if (curWayToShowSamples_ == WayToShowStatistics.NUMBER)
					{
						valueA = archiveFields_.UAABKHarmCounterNonflaggedT2[0].ToString();
						valueB = archiveFields_.UBBCKHarmCounterNonflaggedT2[0].ToString();
						valueC = archiveFields_.UCCAKHarmCounterNonflaggedT2[0].ToString();
					}
					else if (curWayToShowSamples_ == WayToShowStatistics.PERCENTAGE)
					{
						if (archiveFields_.TenMinuteCounterNonflagged > 0)
						{
							valueA = (archiveFields_.UAABKHarmCounterNonflaggedT2[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
							valueB = (archiveFields_.UBBCKHarmCounterNonflaggedT2[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
							valueC = (archiveFields_.UCCAKHarmCounterNonflaggedT2[0] * 100.0F /
										  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						}
					}
					else  // time
					{
						valueA = TimeSpan.FromTicks(archiveFields_.UAABKHarmCounterNonflaggedT2[0] *
							coefTime).ToString();
						valueB = TimeSpan.FromTicks(archiveFields_.UBBCKHarmCounterNonflaggedT2[0] *
							coefTime).ToString();
						valueC = TimeSpan.FromTicks(archiveFields_.UCCAKHarmCounterNonflaggedT2[0] *
							coefTime).ToString();
					}

					dgvNonsinusStat1.Rows[rowOut].Cells[columnParam].Value = "Outside UPL";
					dgvNonsinusStat1.Rows[rowOut].Cells[columnA].Value = valueA;
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						dgvNonsinusStat1.Rows[rowOut].Cells[columnB].Value = valueB;
						dgvNonsinusStat1.Rows[rowOut].Cells[columnC].Value = valueC;
					}

					if (archiveFields_.UAABKHarmCounterNonflaggedT2[0] > 0)
					{
						dgvNonsinusStat1.Rows[rowOut].Cells[columnA].Style.BackColor = Color.Salmon;
					}
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						if (archiveFields_.UBBCKHarmCounterNonflaggedT2[0] > 0)
						{
							dgvNonsinusStat1.Rows[rowOut].Cells[columnB].Style.BackColor = Color.Salmon;
						}
						if (archiveFields_.UCCAKHarmCounterNonflaggedT2[0] > 0)
						{
							dgvNonsinusStat1.Rows[rowOut].Cells[columnC].Style.BackColor = Color.Salmon;
						}
					}
				}

				// orders /////////////////////////////////////
				dgvNonsinusStat2.Rows.Clear();
				dgvNonsinusStat2.Rows.Add(3);	// 3: norm, between and out
				columnA = 0;
				columnB = 1;
				columnC = 2;
				// Norm
				if (rbShowNumber.Checked)
				{
					valueA = archiveFields_.UAABKHarmCounterNonflaggedNormal[currentHarmonic_].ToString();
					valueB = archiveFields_.UBBCKHarmCounterNonflaggedNormal[currentHarmonic_].ToString();
					valueC = archiveFields_.UCCAKHarmCounterNonflaggedNormal[currentHarmonic_].ToString();
				}
				else if (rbShowPercentage.Checked)
				{
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						valueA = (archiveFields_.UAABKHarmCounterNonflaggedNormal[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						valueB = (archiveFields_.UBBCKHarmCounterNonflaggedNormal[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						valueC = (archiveFields_.UCCAKHarmCounterNonflaggedNormal[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
					}
				}
				else  // time
				{
					valueA = TimeSpan.FromTicks(archiveFields_.UAABKHarmCounterNonflaggedNormal[currentHarmonic_] *
						coefTime).ToString();
					valueB = TimeSpan.FromTicks(archiveFields_.UBBCKHarmCounterNonflaggedNormal[currentHarmonic_] *
						coefTime).ToString();
					valueC = TimeSpan.FromTicks(archiveFields_.UCCAKHarmCounterNonflaggedNormal[currentHarmonic_] *
						coefTime).ToString();
				}

				dgvNonsinusStat2.Rows[rowNorm].Cells[columnA].Value = valueA;
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					dgvNonsinusStat2.Rows[rowNorm].Cells[columnB].Value = valueB;
					dgvNonsinusStat2.Rows[rowNorm].Cells[columnC].Value = valueC;
				}

				if (archiveFields_.UAABKHarmCounterNonflaggedNormal[currentHarmonic_] > 0)
				{
					dgvNonsinusStat2.Rows[rowNorm].Cells[columnA].Style.BackColor = Color.LightGreen;
				}
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (archiveFields_.UBBCKHarmCounterNonflaggedNormal[currentHarmonic_] > 0)
					{
						dgvNonsinusStat2.Rows[rowNorm].Cells[columnB].Style.BackColor = Color.LightGreen;
					}
					if (archiveFields_.UCCAKHarmCounterNonflaggedNormal[currentHarmonic_] > 0)
					{
						dgvNonsinusStat2.Rows[rowNorm].Cells[columnC].Style.BackColor = Color.LightGreen;
					}
				}

				// Between
				if (curWayToShowSamples_ == WayToShowStatistics.NUMBER)
				{
					valueA = archiveFields_.UAABKHarmCounterNonflaggedT1[currentHarmonic_].ToString();
					valueB = archiveFields_.UBBCKHarmCounterNonflaggedT1[currentHarmonic_].ToString();
					valueC = archiveFields_.UCCAKHarmCounterNonflaggedT1[currentHarmonic_].ToString();
				}
				else if (curWayToShowSamples_ == WayToShowStatistics.PERCENTAGE)
				{
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						valueA = (archiveFields_.UAABKHarmCounterNonflaggedT1[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						valueB = (archiveFields_.UBBCKHarmCounterNonflaggedT1[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						valueC = (archiveFields_.UCCAKHarmCounterNonflaggedT1[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
					}
				}
				else  // time
				{
					valueA = TimeSpan.FromTicks(archiveFields_.UAABKHarmCounterNonflaggedT1[currentHarmonic_] *
						coefTime).ToString();
					valueB = TimeSpan.FromTicks(archiveFields_.UBBCKHarmCounterNonflaggedT1[currentHarmonic_] *
						coefTime).ToString();
					valueC = TimeSpan.FromTicks(archiveFields_.UCCAKHarmCounterNonflaggedT1[currentHarmonic_] *
						coefTime).ToString();
				}

				dgvNonsinusStat2.Rows[rowBetween].Cells[columnA].Value = valueA;
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					dgvNonsinusStat2.Rows[rowBetween].Cells[columnB].Value = valueB;
					dgvNonsinusStat2.Rows[rowBetween].Cells[columnC].Value = valueC;
				}

				if (archiveFields_.UAABKHarmCounterNonflaggedT1[currentHarmonic_] > 0)
				{
					dgvNonsinusStat2.Rows[rowBetween].Cells[columnA].Style.BackColor = Color.Yellow;
				}
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (archiveFields_.UBBCKHarmCounterNonflaggedT1[currentHarmonic_] > 0)
					{
						dgvNonsinusStat2.Rows[rowBetween].Cells[columnB].Style.BackColor = Color.Yellow;
					}
					if (archiveFields_.UCCAKHarmCounterNonflaggedT1[currentHarmonic_] > 0)
					{
						dgvNonsinusStat2.Rows[rowBetween].Cells[columnC].Style.BackColor = Color.Yellow;
					}
				}

				// Out
				if (rbShowNumber.Checked)
				{
					valueA = archiveFields_.UAABKHarmCounterNonflaggedT2[currentHarmonic_].ToString();
					valueB = archiveFields_.UBBCKHarmCounterNonflaggedT2[currentHarmonic_].ToString();
					valueC = archiveFields_.UCCAKHarmCounterNonflaggedT2[currentHarmonic_].ToString();
				}
				else if (rbShowPercentage.Checked)
				{
					if (archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						valueA = (archiveFields_.UAABKHarmCounterNonflaggedT2[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						valueB = (archiveFields_.UBBCKHarmCounterNonflaggedT2[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
						valueC = (archiveFields_.UCCAKHarmCounterNonflaggedT2[currentHarmonic_] * 100.0F /
									  archiveFields_.TenMinuteCounterNonflagged).ToString(settings_.FloatFormat);
					}
				}
				else  // time
				{
					valueA = TimeSpan.FromTicks(archiveFields_.UAABKHarmCounterNonflaggedT2[currentHarmonic_] *
						coefTime).ToString();
					valueB = TimeSpan.FromTicks(archiveFields_.UBBCKHarmCounterNonflaggedT2[currentHarmonic_] *
						coefTime).ToString();
					valueC = TimeSpan.FromTicks(archiveFields_.UCCAKHarmCounterNonflaggedT2[currentHarmonic_] *
						coefTime).ToString();
				}

				dgvNonsinusStat2.Rows[rowOut].Cells[columnA].Value = valueA;
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					dgvNonsinusStat2.Rows[rowOut].Cells[columnB].Value = valueB;
					dgvNonsinusStat2.Rows[rowOut].Cells[columnC].Value = valueC;
				}

				if (archiveFields_.UAABKHarmCounterNonflaggedT2[currentHarmonic_] > 0)
				{
					dgvNonsinusStat2.Rows[rowOut].Cells[columnA].Style.BackColor = Color.Salmon;
				}
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (archiveFields_.UBBCKHarmCounterNonflaggedT2[currentHarmonic_] > 0)
					{
						dgvNonsinusStat2.Rows[rowOut].Cells[columnB].Style.BackColor = Color.Salmon;
					}
					if (archiveFields_.UCCAKHarmCounterNonflaggedT2[currentHarmonic_] > 0)
					{
						dgvNonsinusStat2.Rows[rowOut].Cells[columnC].Style.BackColor = Color.Salmon;
					}
				}

				if(statisticsOnly) return;

				#endregion

				// sample
				if (!newOrderOnly)
				{
					lvNonsinusSample.Items.Clear();

					itm = new ListViewItem("All");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
					lvNonsinusSample.Items.Add(itm);
					itm = new ListViewItem("Marked");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
					lvNonsinusSample.Items.Add(itm);
					itm = new ListViewItem("Not Marked");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
					lvNonsinusSample.Items.Add(itm);
				}

				#region Values

				// summary
				if (!newOrderOnly)
				{
					lvNonsinusValues1.Items.Clear();

					itm = new ListViewItem("Max");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UAABKHarm100[0].ToString(settings_.FloatFormat));
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						itm.SubItems.Add(archiveFields_.UBBCKHarm100[0].ToString(settings_.FloatFormat));
						itm.SubItems.Add(archiveFields_.UCCAKHarm100[0].ToString(settings_.FloatFormat));
					}
					itm.SubItems.Add(regInfo_.Constraints.KHarmTotal100.ToString(ConstraintsFormat));

					if (archiveFields_.UAABKHarm100[0] < regInfo_.Constraints.KHarmTotal95)
						itm.SubItems[1].BackColor = Color.LightGreen;
					else if (archiveFields_.UAABKHarm100[0] > regInfo_.Constraints.KHarmTotal100)
						itm.SubItems[1].BackColor = Color.Salmon;
					else itm.SubItems[1].BackColor = Color.Yellow;	// between

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						if (archiveFields_.UBBCKHarm100[0] < regInfo_.Constraints.KHarmTotal95)
							itm.SubItems[2].BackColor = Color.LightGreen;
						else if (archiveFields_.UBBCKHarm100[0] > regInfo_.Constraints.KHarmTotal100)
							itm.SubItems[2].BackColor = Color.Salmon;
						else itm.SubItems[2].BackColor = Color.Yellow; // between

						if (archiveFields_.UCCAKHarm100[0] < regInfo_.Constraints.KHarmTotal95)
							itm.SubItems[3].BackColor = Color.LightGreen;
						else if (archiveFields_.UCCAKHarm100[0] > regInfo_.Constraints.KHarmTotal100)
							itm.SubItems[3].BackColor = Color.Salmon;
						else itm.SubItems[3].BackColor = Color.Yellow; // between
					}
					lvNonsinusValues1.Items.Add(itm);

					itm = new ListViewItem("Upper");
					itm.SubItems.Add(archiveFields_.UAABKHarm95[0].ToString(settings_.FloatFormat));
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						itm.SubItems.Add(archiveFields_.UBBCKHarm95[0].ToString(settings_.FloatFormat));
						itm.SubItems.Add(archiveFields_.UCCAKHarm95[0].ToString(settings_.FloatFormat));
					}
					itm.SubItems.Add(regInfo_.Constraints.KHarmTotal95.ToString(ConstraintsFormat));

					if (archiveFields_.UAABKHarm95[0] < regInfo_.Constraints.KHarmTotal95)
						itm.SubItems[1].BackColor = Color.LightGreen;
					else if (archiveFields_.UAABKHarm95[0] > regInfo_.Constraints.KHarmTotal100)
						itm.SubItems[1].BackColor = Color.Salmon;
					else itm.SubItems[1].BackColor = Color.Yellow;	// between

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						if (archiveFields_.UBBCKHarm95[0] < regInfo_.Constraints.KHarmTotal95)
							itm.SubItems[2].BackColor = Color.LightGreen;
						else if (archiveFields_.UBBCKHarm95[0] > regInfo_.Constraints.KHarmTotal100)
							itm.SubItems[2].BackColor = Color.Salmon;
						else itm.SubItems[2].BackColor = Color.Yellow; // between

						if (archiveFields_.UCCAKHarm95[0] < regInfo_.Constraints.KHarmTotal95)
							itm.SubItems[3].BackColor = Color.LightGreen;
						else if (archiveFields_.UCCAKHarm100[0] > regInfo_.Constraints.KHarmTotal100)
							itm.SubItems[3].BackColor = Color.Salmon;
						else itm.SubItems[3].BackColor = Color.Yellow; // between
					}
					lvNonsinusValues1.Items.Add(itm);
				}

				// orders
				lvNonsinusValues2.Items.Clear();

				itm = new ListViewItem(archiveFields_.UAABKHarm100[currentHarmonic_].ToString(settings_.FloatFormat));
				itm.UseItemStyleForSubItems = false;
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					itm.SubItems.Add(archiveFields_.UBBCKHarm100[currentHarmonic_].ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UCCAKHarm100[currentHarmonic_].ToString(settings_.FloatFormat));
				}
				itm.SubItems.Add(regInfo_.Constraints.KHarmTotal100.ToString(settings_.FloatFormat));

				if (archiveFields_.UAABKHarm100[currentHarmonic_] < regInfo_.Constraints.KHarmTotal95)
					itm.SubItems[1].BackColor = Color.LightGreen;
				else if (archiveFields_.UAABKHarm100[currentHarmonic_] > regInfo_.Constraints.KHarmTotal100)
					itm.SubItems[1].BackColor = Color.Salmon;
				else itm.SubItems[1].BackColor = Color.Yellow;	// between

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (archiveFields_.UBBCKHarm100[currentHarmonic_] < regInfo_.Constraints.KHarmTotal95)
						itm.SubItems[2].BackColor = Color.LightGreen;
					else if (archiveFields_.UBBCKHarm100[currentHarmonic_] > regInfo_.Constraints.KHarmTotal100)
						itm.SubItems[2].BackColor = Color.Salmon;
					else itm.SubItems[2].BackColor = Color.Yellow; // between

					if (archiveFields_.UCCAKHarm100[currentHarmonic_] < regInfo_.Constraints.KHarmTotal95)
						itm.SubItems[3].BackColor = Color.LightGreen;
					else if (archiveFields_.UCCAKHarm100[currentHarmonic_] > regInfo_.Constraints.KHarmTotal100)
						itm.SubItems[3].BackColor = Color.Salmon;
					else itm.SubItems[3].BackColor = Color.Yellow; // between
				}
				lvNonsinusValues2.Items.Add(itm);

				itm = new ListViewItem(archiveFields_.UAABKHarm95[currentHarmonic_].ToString(settings_.FloatFormat));
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					itm.SubItems.Add(archiveFields_.UBBCKHarm95[currentHarmonic_].ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UCCAKHarm95[currentHarmonic_].ToString(settings_.FloatFormat));
				}
				itm.SubItems.Add(regInfo_.Constraints.KHarmTotal95.ToString(ConstraintsFormat));

				if (archiveFields_.UAABKHarm95[currentHarmonic_] < regInfo_.Constraints.KHarmTotal95)
					itm.SubItems[1].BackColor = Color.LightGreen;
				else if (archiveFields_.UAABKHarm95[currentHarmonic_] > regInfo_.Constraints.KHarmTotal100)
					itm.SubItems[1].BackColor = Color.Salmon;
				else itm.SubItems[1].BackColor = Color.Yellow;	// between

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (archiveFields_.UBBCKHarm95[currentHarmonic_] < regInfo_.Constraints.KHarmTotal95)
						itm.SubItems[2].BackColor = Color.LightGreen;
					else if (archiveFields_.UBBCKHarm95[currentHarmonic_] > regInfo_.Constraints.KHarmTotal100)
						itm.SubItems[2].BackColor = Color.Salmon;
					else itm.SubItems[2].BackColor = Color.Yellow; // between

					if (archiveFields_.UCCAKHarm95[currentHarmonic_] < regInfo_.Constraints.KHarmTotal95)
						itm.SubItems[3].BackColor = Color.LightGreen;
					else if (archiveFields_.UCCAKHarm100[currentHarmonic_] > regInfo_.Constraints.KHarmTotal100)
						itm.SubItems[3].BackColor = Color.Salmon;
					else itm.SubItems[3].BackColor = Color.Yellow; // between
				}
				lvNonsinusValues2.Items.Add(itm);

				#endregion

				// charts
				if (!newOrderOnly)
				{
					//zgcNonsinusA.GraphPane.CurveList.Clear();  in DrawNonsinusCharts method
					//zgcNonsinusB.GraphPane.CurveList.Clear();
					//zgcNonsinusC.GraphPane.CurveList.Clear();
					DrawNonsinusCharts();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillNonsinusData");
				throw;
			}
		}

		private void ChangeNonsinusOrder(Keys key)
		{
			try
			{
				if (tabControlMain.SelectedTab == tpNonsinus)
				{
					int currentHarmonicToShow = currentHarmonic_ + 1;
					if (key == Keys.Left)
					{
						if (currentHarmonicToShow > 2)
						{
							currentHarmonic_--;
							currentHarmonicToShow = currentHarmonic_ + 1;
							if (currentHarmonicToShow > 2)
							{
								btnCapNonsinusLeft1.Text = "<\n" + (currentHarmonicToShow - 1).ToString();
								btnCapNonsinusLeft2.Text = "<\n" + (currentHarmonicToShow - 1).ToString();
							}
							else
							{
								btnCapNonsinusLeft1.Text = string.Empty;
								btnCapNonsinusLeft2.Text = string.Empty;
							}
							btnCapNonsinusRight1.Text = ">\n" + (currentHarmonicToShow + 1).ToString();
							btnCapNonsinusRight2.Text = ">\n" + (currentHarmonicToShow + 1).ToString();

							FillNonsinusData(true, false);
						}
					}
					if (key == Keys.Right)
					{
						if (currentHarmonicToShow < 40)
						{
							currentHarmonic_++;
							currentHarmonicToShow = currentHarmonic_ + 1;
							if (currentHarmonicToShow < 40)
							{
								btnCapNonsinusRight1.Text = ">\n" + (currentHarmonicToShow + 1).ToString();
								btnCapNonsinusRight2.Text = ">\n" + (currentHarmonicToShow + 1).ToString();
							}
							else
							{
								btnCapNonsinusRight1.Text = string.Empty;
								btnCapNonsinusRight2.Text = string.Empty;
							}
							btnCapNonsinusLeft1.Text = "<\n" + (currentHarmonicToShow - 1).ToString();
							btnCapNonsinusLeft2.Text = "<\n" + (currentHarmonicToShow - 1).ToString();

							FillNonsinusData(true, false);
						}
					}
					btnCapNonsinusCoef17.Text = "Nonsinusoidality coeff. of order " + currentHarmonicToShow;
					btnCapNonsinusCoef172.Text = "Nonsinusoidality coeff. of order " + currentHarmonicToShow;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ChangeNonsinusOrder()");
				throw;
			}
		}

		private void DrawNonsinusCharts()
		{
			zgcNonsinusA.GraphPane.CurveList.Clear();
			zgcNonsinusB.GraphPane.CurveList.Clear();
			zgcNonsinusC.GraphPane.CurveList.Clear();

			if (rbNonsinusChartMaxA.Checked)
				DrawOneNonsinusChart(ref zgcNonsinusA, ref archiveFields_.UAABKHarm100,
								regInfo_.Constraints.KHarmTotal100,
								ref regInfo_.Constraints.KHarm100, "Phase A, Max values");
			else
				DrawOneNonsinusChart(ref zgcNonsinusA, ref archiveFields_.UAABKHarm95,
								regInfo_.Constraints.KHarmTotal95,
								ref regInfo_.Constraints.KHarm95, "Phase A, Upper values");

			if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
			{
				if (rbNonsinusChartMaxB.Checked)
					DrawOneNonsinusChart(ref zgcNonsinusB, ref archiveFields_.UBBCKHarm100,
					                     regInfo_.Constraints.KHarmTotal100,
					                     ref regInfo_.Constraints.KHarm100, "Phase B, Max values");
				else
					DrawOneNonsinusChart(ref zgcNonsinusB, ref archiveFields_.UBBCKHarm95,
					                     regInfo_.Constraints.KHarmTotal95,
					                     ref regInfo_.Constraints.KHarm95, "Phase B, Upper values");

				if (rbNonsinusChartMaxC.Checked)
					DrawOneNonsinusChart(ref zgcNonsinusC, ref archiveFields_.UCCAKHarm100,
					                     regInfo_.Constraints.KHarmTotal100,
					                     ref regInfo_.Constraints.KHarm100, "Phase C, Max values");
				else
					DrawOneNonsinusChart(ref zgcNonsinusC, ref archiveFields_.UCCAKHarm95,
					                     regInfo_.Constraints.KHarmTotal95,
					                     ref regInfo_.Constraints.KHarm95, "Phase C, Upper values");
			}
		}

		private void DrawOneNonsinusChart(ref ZedGraphControl zg, ref float[] values, float constrTotal,
									ref float[] constraints, string title)
		{
			try
			{
				List<double> listValueNormal = new List<double>();
				List<double> listValueExceed = new List<double>();

				List<double> listConstraints = new List<double>();
				listConstraints.Add(constrTotal);
				listConstraints.Add(0);			// there is no the 1st harmonic
				foreach (var constr in constraints)
				{
					listConstraints.Add(constr);
				}

				//Series serGreen = new Series();
				//serGreen.Color = Color.LimeGreen;
				//serGreen.Name = "Within NPL";
				//Series serYellow = new Series();
				//serYellow.Color = Color.Gold;
				//serYellow.Name = "Betw. NPL and UPL";
				//Series serRed = new Series();
				//serRed.Color = Color.Red;
				//serRed.Name = "Outside UPL";

				if (values[0] <= constrTotal)
				{
					listValueNormal.Add(values[0]);
					listValueExceed.Add(0);
				}
				else
				{
					listValueNormal.Add(0);
					listValueExceed.Add(values[0]);
				}
				listValueNormal.Add(0);			// there is no the 1st harmonic	
				listValueExceed.Add(0);

				for (int iOrder = 1; iOrder < 40; ++iOrder)
				{
					if (values[iOrder] <= regInfo_.Constraints.KHarm95[iOrder - 1])
					{
						//listValueNormal.Add(values[iOrder] + iOrder/10);//for debug
						listValueExceed.Add(0);
					}
					else
					{
						listValueNormal.Add(0);
						listValueExceed.Add(values[iOrder]);
					}
				}

				// get a reference to the GraphPane
				GraphPane myPane = zg.GraphPane;
				//listValueExceed[4] = 4;//for debug
				//listValueNormal[4] = 0;
				//listValueExceed[7] = 3;
				//listValueNormal[7] = 0;
				//listValueExceed[26] = 2;
				//listValueNormal[26] = 0;

				myPane.CurveList.Clear();

				// Set the Titles
				myPane.Title.Text = title;
				myPane.XAxis.Title.Text = "Order";
				myPane.YAxis.Title.Text = "%";

				// Make up some random data points
				string[] labels =
					{
						"THD", "", "2", "3", "4", "5", "6", "7", "8", "9", "10",
						"11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
						"21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
						"31", "32", "33", "34", "35", "36", "37", "38", "39", "40"
					};
				//double[] y = {100, 115, 75, 22, 98, 40};
				//double[] y2 = {90, 100, 95, 35, 80, 35};
				//double[] y3 = {80, 110, 65, 15, 54, 67};
				//double[] y4 = {120, 125, 100, 40, 105, 75};//for debug

				BarItem myBar = myPane.AddBar("Upper Normal", null, listValueNormal.ToArray(), Color.LimeGreen);
				myBar.Bar.Fill = new Fill(Color.LimeGreen, Color.PaleGreen, Color.Green);

				myBar = myPane.AddBar("Upper Exceeding", null, listValueExceed.ToArray(), Color.Red);
				myBar.Bar.Fill = new Fill(Color.Red, Color.LightPink, Color.Fuchsia);

				// Generate a black line with "Curve 4" in the legend
				LineItem myCurve = myPane.AddCurve("Nominal values", null, listConstraints.ToArray(), 
													Color.Red, SymbolType.Circle);
				myCurve.Line.Fill = new Fill(Color.White, Color.LightGreen, -45F);

				// Fix up the curve attributes a little
				//myCurve.Symbol.Size = 8.0F;
				//myCurve.Symbol.Fill = new Fill(Color.White);
				//myCurve.Line.Width = 2.0F;

				// Draw the X tics between the labels instead of at the labels
				myPane.XAxis.MajorTic.IsBetweenLabels = true;

				// Set the XAxis labels
				myPane.XAxis.Scale.TextLabels = labels;
				// Set the XAxis to Text type
				myPane.XAxis.Type = AxisType.Text;
				// Fill the Axis and Pane backgrounds
				myPane.Chart.Fill = new Fill(Color.FromArgb(255, 223, 226), Color.White, 90F);
				myPane.Fill = new Fill(Color.White);

				// Tell ZedGraph to refigure the axes since the data have changed
				zg.AxisChange();
				zg.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::AddNonsinusChart");
				throw;
			}
		}

		// allTables = true at the first start
		// allTables = false when we reload 3 tables with new peak load values
		private void FillVoltageData(bool minMaxModeOnly)
		{
			try
			{
				string phaseA = "A", phaseB = "B", phaseC = "C";
				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
				    regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					phaseA = "AB"; phaseB = "BC"; phaseC = "CA";
				}

				#region Statistics

				dgvUStatistics.Rows.Clear();
				float percentNorm = 0, percentBetween = 0, percentOut = 0;
				Int64 coefTime = 600 * TimeSpan.TicksPerSecond; // 600 seconds

				//ListViewItem itm = new ListViewItem("δU " + phaseA + "+");
				//itm.UseItemStyleForSubItems = false;
				// numbers
				//itm.SubItems.Add(archiveFields_.UAABDeviationPosNonflaggedNormal.ToString());
				//itm.SubItems.Add(archiveFields_.UAABDeviationPosNonflaggedT1.ToString());
				//itm.SubItems.Add(archiveFields_.UAABDeviationPosNonflaggedT2.ToString());
				dgvUStatistics.Rows.Add(6);		// 6 phases

				dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].
					Cells[(int)StaticticsColumns.PARAM].Value = "δU " + phaseA + "+";
				dgvUStatistics.Rows[(int) UStatisticRows.A_PLUS].Cells[(int) StaticticsColumns.NORM].Value =
					archiveFields_.UAABDeviationPosNonflaggedNormal.ToString();
				dgvUStatistics.Rows[(int) UStatisticRows.A_PLUS].Cells[(int) StaticticsColumns.BETWEEN].Value =
					archiveFields_.UAABDeviationPosNonflaggedT1.ToString();
				dgvUStatistics.Rows[(int) UStatisticRows.A_PLUS].Cells[(int) StaticticsColumns.OUT].Value =
					archiveFields_.UAABDeviationPosNonflaggedT2.ToString();

				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UAABDeviationPosNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UAABDeviationPosNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UAABDeviationPosNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				//itm.SubItems.Add(percentNorm.ToString(settings_.FloatFormat));
				//itm.SubItems.Add(percentBetween.ToString(settings_.FloatFormat));
				//itm.SubItems.Add(percentOut.ToString(settings_.FloatFormat));
				dgvUStatistics.Rows[(int) UStatisticRows.A_PLUS].Cells[(int) StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				//itm.SubItems.Add(TimeSpan.FromTicks(
				//    (archiveFields_.UAABDeviationPosNonflaggedNormal * coefTime)).ToString());
				//itm.SubItems.Add(TimeSpan.FromTicks(
				//    (archiveFields_.UAABDeviationPosNonflaggedT1 * coefTime)).ToString());
				//itm.SubItems.Add(TimeSpan.FromTicks(
				//    (archiveFields_.UAABDeviationPosNonflaggedT2 * coefTime)).ToString());
				dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABDeviationPosNonflaggedNormal * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABDeviationPosNonflaggedT1 * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABDeviationPosNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UAABDeviationPosNonflaggedNormal > 0)
				{
					//itm.SubItems[1].BackColor = Color.LightGreen;	// numbers
					//itm.SubItems[4].BackColor = Color.LightGreen;	// percent
					//itm.SubItems[7].BackColor = Color.LightGreen;	// time
					dgvUStatistics.Rows[(int) UStatisticRows.A_PLUS].Cells[(int) StaticticsColumns.NORM].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.NORM_PERC].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.NORM_TIME].
						Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UAABDeviationPosNonflaggedT1 > 0)
				{
					//itm.SubItems[2].BackColor = Color.Yellow;
					//itm.SubItems[5].BackColor = Color.Yellow;
					//itm.SubItems[8].BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.BETWEEN].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.BETW_PERC].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.BETW_TIME].
						Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UAABDeviationPosNonflaggedT2 > 0)
				{
					//itm.SubItems[3].BackColor = Color.Salmon;
					//itm.SubItems[6].BackColor = Color.Salmon;
					//itm.SubItems[9].BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.OUT].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.OUT_PERC].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.A_PLUS].Cells[(int)StaticticsColumns.OUT_TIME].
						Style.BackColor = Color.Salmon;
				}
				//lvUStatistics.Items.Add(itm);

				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].
					Cells[(int)StaticticsColumns.PARAM].Value = "δU " + phaseA + "-";
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.UAABDeviationNegNonflaggedNormal.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.UAABDeviationNegNonflaggedT1.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.UAABDeviationNegNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UAABDeviationNegNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UAABDeviationNegNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UAABDeviationNegNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABDeviationNegNonflaggedNormal * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABDeviationNegNonflaggedT1 * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UAABDeviationNegNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UAABDeviationNegNonflaggedNormal > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.NORM].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.NORM_PERC].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.NORM_TIME].
						Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UAABDeviationNegNonflaggedT1 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.BETWEEN].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.BETW_PERC].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.BETW_TIME].
						Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UAABDeviationNegNonflaggedT2 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.OUT].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.OUT_PERC].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.A_MINUS].Cells[(int)StaticticsColumns.OUT_TIME].
						Style.BackColor = Color.Salmon;
				}
		
				////////// phase B ///////////////////////////////////
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].
					Cells[(int)StaticticsColumns.PARAM].Value = "δU " + phaseB + "+";
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.UBBCDeviationPosNonflaggedNormal.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.UBBCDeviationPosNonflaggedT1.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.UBBCDeviationPosNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UBBCDeviationPosNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UBBCDeviationPosNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UBBCDeviationPosNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UBBCDeviationPosNonflaggedNormal * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UBBCDeviationPosNonflaggedT1 * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UBBCDeviationPosNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UBBCDeviationPosNonflaggedNormal > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.NORM].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.NORM_PERC].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.NORM_TIME].
						Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UBBCDeviationPosNonflaggedT1 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.BETWEEN].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.BETW_PERC].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.BETW_TIME].
						Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UBBCDeviationPosNonflaggedT2 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.OUT].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.OUT_PERC].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.B_PLUS].Cells[(int)StaticticsColumns.OUT_TIME].
						Style.BackColor = Color.Salmon;
				}

				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].
					Cells[(int)StaticticsColumns.PARAM].Value = "δU " + phaseB + "-";
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.UBBCDeviationNegNonflaggedNormal.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.UBBCDeviationNegNonflaggedT1.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.UBBCDeviationNegNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UBBCDeviationNegNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UBBCDeviationNegNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UBBCDeviationNegNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UBBCDeviationNegNonflaggedNormal * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UBBCDeviationNegNonflaggedT1 * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UBBCDeviationNegNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UBBCDeviationNegNonflaggedNormal > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.NORM].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.NORM_PERC].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.NORM_TIME].
						Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UBBCDeviationNegNonflaggedT1 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.BETWEEN].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.BETW_PERC].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.BETW_TIME].
						Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UBBCDeviationNegNonflaggedT2 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.OUT].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.OUT_PERC].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.B_MINUS].Cells[(int)StaticticsColumns.OUT_TIME].
						Style.BackColor = Color.Salmon;
				}

				////////// phase C ///////////////////////////////////
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.PARAM].Value = 
					"δU " + phaseC + "+";
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.UCCADeviationPosNonflaggedNormal.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.UCCADeviationPosNonflaggedT1.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.UCCADeviationPosNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UCCADeviationPosNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UCCADeviationPosNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UCCADeviationPosNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UCCADeviationPosNonflaggedNormal * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UCCADeviationPosNonflaggedT1 * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UCCADeviationPosNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UCCADeviationPosNonflaggedNormal > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.NORM].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.NORM_PERC].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.NORM_TIME].
						Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UCCADeviationPosNonflaggedT1 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.BETWEEN].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.BETW_PERC].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.BETW_TIME].
						Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UCCADeviationPosNonflaggedT2 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.OUT].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.OUT_PERC].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.C_PLUS].Cells[(int)StaticticsColumns.OUT_TIME].
						Style.BackColor = Color.Salmon;
				}

				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].
					Cells[(int)StaticticsColumns.PARAM].Value = "δU " + phaseC + "-";
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.NORM].Value =
					archiveFields_.UCCADeviationNegNonflaggedNormal.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.BETWEEN].Value =
					archiveFields_.UCCADeviationNegNonflaggedT1.ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.OUT].Value =
					archiveFields_.UCCADeviationNegNonflaggedT2.ToString();
				// percent
				if (archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					percentNorm = archiveFields_.UCCADeviationNegNonflaggedNormal * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentBetween = archiveFields_.UCCADeviationNegNonflaggedT1 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
					percentOut = archiveFields_.UCCADeviationNegNonflaggedT2 * 100.0F /
								  archiveFields_.TenMinuteCounterNonflagged;
				}
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.NORM_PERC].Value =
					percentNorm.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.BETW_PERC].Value =
					percentBetween.ToString(settings_.FloatFormat);
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.OUT_PERC].Value =
					percentOut.ToString(settings_.FloatFormat);
				// time
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.NORM_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UCCADeviationNegNonflaggedNormal * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.BETW_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UCCADeviationNegNonflaggedT1 * coefTime).ToString();
				dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.OUT_TIME].Value =
					TimeSpan.FromTicks(archiveFields_.UCCADeviationNegNonflaggedT2 * coefTime).ToString();

				if (archiveFields_.UCCADeviationNegNonflaggedNormal > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.NORM].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.NORM_PERC].
						Style.BackColor = Color.LightGreen;
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.NORM_TIME].
						Style.BackColor = Color.LightGreen;
				}
				if (archiveFields_.UCCADeviationNegNonflaggedT1 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.BETWEEN].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.BETW_PERC].
						Style.BackColor = Color.Yellow;
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.BETW_TIME].
						Style.BackColor = Color.Yellow;
				}
				if (archiveFields_.UCCADeviationNegNonflaggedT2 > 0)
				{
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.OUT].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.OUT_PERC].
						Style.BackColor = Color.Salmon;
					dgvUStatistics.Rows[(int)UStatisticRows.C_MINUS].Cells[(int)StaticticsColumns.OUT_TIME].
						Style.BackColor = Color.Salmon;
				}			

				#endregion

				#region Sample

				lvUSample.Items.Clear();
				// sample
				ListViewItem itm = new ListViewItem("δU " + phaseA + "+");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
				itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
				itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
				lvUSample.Items.Add(itm);
				itm = new ListViewItem("δU " + phaseA + "-");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
				itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
				itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
				lvUSample.Items.Add(itm);
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					itm = new ListViewItem("δU " + phaseB + "+");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
					itm = new ListViewItem("δU " + phaseB + "-");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
					itm = new ListViewItem("δU " + phaseC + "+");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
					itm = new ListViewItem("δU " + phaseC + "-");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.TenMinuteCounterTotal.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterFlagged.ToString());
					itm.SubItems.Add(archiveFields_.TenMinuteCounterNonflagged.ToString());
					lvUSample.Items.Add(itm);
				}

				#endregion

				#region Values (max and min)

				lvUValues.Items.Clear();

				itm = new ListViewItem("δU " + phaseA + "+");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.UAABDeviationPos95.ToString(settings_.FloatFormat));
				itm.SubItems.Add(archiveFields_.UAABDeviationPos100.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationUp95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationUp100.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationDown95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationDown100.ToString(ConstraintsFormat));
				lvUValues.Items.Add(itm);
				itm = new ListViewItem("δU " + phaseA + "-");
				itm.UseItemStyleForSubItems = false;
				itm.SubItems.Add(archiveFields_.UAABDeviationNeg95.ToString(settings_.FloatFormat));
				itm.SubItems.Add(archiveFields_.UAABDeviationNeg100.ToString(settings_.FloatFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationUp95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationUp100.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationDown95.ToString(ConstraintsFormat));
				itm.SubItems.Add(regInfo_.Constraints.UDeviationDown100.ToString(ConstraintsFormat));
				lvUValues.Items.Add(itm);

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					itm = new ListViewItem("δU " + phaseB + "+");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UBBCDeviationPos95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UBBCDeviationPos100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp100.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown100.ToString(ConstraintsFormat));
					lvUValues.Items.Add(itm);
					itm = new ListViewItem("δU " + phaseB + "-");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UBBCDeviationNeg95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UBBCDeviationNeg100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp100.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown100.ToString(ConstraintsFormat));
					lvUValues.Items.Add(itm);

					itm = new ListViewItem("δU " + phaseC + "+");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UCCADeviationPos95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UCCADeviationPos100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp100.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown100.ToString(ConstraintsFormat));
					lvUValues.Items.Add(itm);
					itm = new ListViewItem("δU " + phaseC + "-");
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add(archiveFields_.UCCADeviationNeg95.ToString(settings_.FloatFormat));
					itm.SubItems.Add(archiveFields_.UCCADeviationNeg100.ToString(settings_.FloatFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationUp100.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown95.ToString(ConstraintsFormat));
					itm.SubItems.Add(regInfo_.Constraints.UDeviationDown100.ToString(ConstraintsFormat));
					lvUValues.Items.Add(itm);
				}

				#endregion

				#region  Values table

				if (!minMaxModeOnly && archiveFields_.TenMinuteCounterNonflagged > 0)
				{
					dgvUValues.Rows.Clear();

					DateTime dtCurTime = archiveInfo_.DtStart;
					if (archiveFields_.TenMinuteCounterTotal > 0)
					{
						dgvUValues.Rows.Add(archiveFields_.TenMinuteCounterTotal);
						for (int iRow = 0; iRow < archiveFields_.TenMinuteCounterTotal; ++iRow)
						{
							dgvUValues.Rows[iRow].Cells[0].Value = dtCurTime.ToString("dd.MM.yyyy HH:mm:ss");
							if (archiveFields_.TenMinuteNotMarked[iRow])
							{
								dgvUValues.Rows[iRow].Cells[1].Value =
									archiveFields_.UAABDeviationPos[iRow].ToString(settings_.FloatFormat);
								dgvUValues.Rows[iRow].Cells[2].Value =
									archiveFields_.UAABDeviationNeg[iRow].ToString(settings_.FloatFormat);
								if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
								{
									dgvUValues.Rows[iRow].Cells[3].Value =
										archiveFields_.UBBCDeviationPos[iRow].ToString(settings_.FloatFormat);
									dgvUValues.Rows[iRow].Cells[4].Value =
										archiveFields_.UBBCDeviationNeg[iRow].ToString(settings_.FloatFormat);
									dgvUValues.Rows[iRow].Cells[5].Value =
										archiveFields_.UCCADeviationPos[iRow].ToString(settings_.FloatFormat);
									dgvUValues.Rows[iRow].Cells[6].Value =
										archiveFields_.UCCADeviationNeg[iRow].ToString(settings_.FloatFormat);
								}
							}
							else
							{
								dgvUValues.Rows[iRow].Cells[1].Value = string.Empty;
								dgvUValues.Rows[iRow].Cells[2].Value = string.Empty;
								if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
								{
									dgvUValues.Rows[iRow].Cells[3].Value = string.Empty;
									dgvUValues.Rows[iRow].Cells[4].Value = string.Empty;
									dgvUValues.Rows[iRow].Cells[5].Value = string.Empty;
									dgvUValues.Rows[iRow].Cells[6].Value = string.Empty;
								}
							}

							dtCurTime = dtCurTime.AddMinutes(10);
						}
					}
				}

				#endregion

				try
				{
					// если нужно, добавляем режимы макс. и мин. нагрузок
					if (bNeedMaxModeForEtPQP_A_ && archiveFields_.TenMinuteCounterNonflagged > 0)
					{
						if (dgvUValues != null && dgvUValues.Rows.Count > 0)
						{
							AddMinMaxModeString("A+");
							AddMinMaxModeString("A-");
							if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
							{
								AddMinMaxModeString("B+");
								AddMinMaxModeString("B-");
								AddMinMaxModeString("C+");
								AddMinMaxModeString("C-");
							}
						}
						else
						{
							MessageBoxes.MsgErrorGetVolValues(this);
							return;
						}
					}
				}
				catch (Exception exc)
				{
					EmService.DumpException(exc, "Error in FillVoltageData while calculating max mode");
					//if (EmService.ShowWndFeedback)
					//{
					//    EmServiceLib.SavingInterface.frmSentLogs frmLogs = 
					//			new EmServiceLib.SavingInterface.frmSentLogs();
					//    frmLogs.ShowDialog();
					//    EmService.ShowWndFeedback = false;
					//}
				}

				// graph
				if (!minMaxModeOnly)
				{
					zgcUA.GraphPane.CurveList.Clear();
					zgcUB.GraphPane.CurveList.Clear();
					zgcUC.GraphPane.CurveList.Clear();

					AddUFCurver(ref zgcUA, ref dgvUValues, 0, 1, Color.Red, 1.5f, "Voltage Deviation, Phase A",
																	archiveFields_.TenMinuteNotMarked);
					AddUFCurver(ref zgcUA, ref dgvUValues, 0, 2, Color.Blue, 1.5f, "Voltage Deviation, Phase A",
																	archiveFields_.TenMinuteNotMarked);

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						AddUFCurver(ref zgcUB, ref dgvUValues, 0, 3, Color.Red, 1.5f, "Voltage Deviation, Phase B",
																	archiveFields_.TenMinuteNotMarked);
						AddUFCurver(ref zgcUB, ref dgvUValues, 0, 4, Color.Blue, 1.5f, "Voltage Deviation, Phase B",
																	archiveFields_.TenMinuteNotMarked);

						AddUFCurver(ref zgcUC, ref dgvUValues, 0, 5, Color.Red, 1.5f, "Voltage Deviation, Phase C",
																	archiveFields_.TenMinuteNotMarked);
						AddUFCurver(ref zgcUC, ref dgvUValues, 0, 6, Color.Blue, 1.5f, "Voltage Deviation, Phase C",
																	archiveFields_.TenMinuteNotMarked);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in UserControlPqpPage::FillVoltageData");
				throw;
			}
		}

		#region MinMaxMode

		private void AddMinMaxModeString(string phase)
		{
			bool positivePhase = true;		// positive or negative phase
			try
			{
				// максимальное и верхнее (для наиб. и наим. нагрузок)
				float maxMaxMode = -1, maxMinMode = -1, upMaxMode = -1, upMinMode = -1;	
				int markedMax = 0, notMarkedMax = 0, markedMin = 0, notMarkedMin = 0;
				bool minModeValid = false, maxModeValid = false, upValueValidMax = false, upValueValidMin = false;

				#region Calculate values

				// сначала нужно получить все значения
				List<Trio<DateTime, float, bool>> listUVal = new List<Trio<DateTime, float, bool>>();
				short curColumn = -1;
				switch (phase)
				{
					case "A+": curColumn = 1;
						if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
						    regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
							phase = "AB+";
						break;
					case "A-": curColumn = 2; positivePhase = false;
						if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
							regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
							phase = "AB-";
						break;
					case "B+": curColumn = 3;
						if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
							regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
							phase = "BC+";
						break;
					case "B-": curColumn = 4; positivePhase = false;
						if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
							regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
							phase = "BC-";
						break;
					case "C+": curColumn = 5;
						if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
							regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
							phase = "CA+";
						break;
					case "C-": curColumn = 6; positivePhase = false;
						if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
							regInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
							phase = "CA-";
						break;
				}

				//string query = string.Format("SELECT event_datetime, case when {0} = -1 then ' ' else cast({1} as text) end, record_marked FROM pqp_du_val WHERE datetime_id = " + curDatetimeId_ + " ORDER BY event_datetime;", curColumn, curColumn);
				float curValue;
				//while (dbService.DataReaderRead())
				DateTime dtCurrent = archiveInfo_.DtStart;
				for(int iRow = 0; iRow < dgvUValues.Rows.Count; ++iRow)
				{
					if (archiveFields_.TenMinuteNotMarked[iRow])
					{
						object oCurVal = dgvUValues.Rows[iRow].Cells[curColumn].Value;
						if (!Conversions.object_2_float_en_ru(oCurVal, out curValue))
							continue;
						listUVal.Add(new Trio<DateTime, float, bool>(dtCurrent, curValue, archiveFields_.TenMinuteNotMarked[iRow]));
					}
					else listUVal.Add(new Trio<DateTime, float, bool>(dtCurrent, -1, false));

					dtCurrent = dtCurrent.AddMinutes(10);
				}

				if (listUVal.Count == 0)
				{
					EmService.WriteToLogFailed("AddMinMaxModeString: no dU values for phase " + phase);
					return;
				}
				
				// затем надо разделить по режимам наибольших и наименьших:
				// создаем новые списки и туда отложим отсчеты, которые относятся к режиму наибольших,
				// а в исходных списках останутся значения режима наименьших
				List<Trio<DateTime, float, bool>> listUValMax = new List<Trio<DateTime, float, bool>>();
				SeparateMinMaxMode(ref listUVal, ref listUValMax);
				// теперь считаем сколько маркированных и немаркированных
				for (int iItem = 0; iItem < listUVal.Count; ++iItem)
				{
					if (!listUVal[iItem].Third)
					{
						markedMin++;
						listUVal.RemoveAt(iItem);
						iItem--;
					}
				}
				notMarkedMin = listUVal.Count;
				for (int iItem = 0; iItem < listUValMax.Count; ++iItem)
				{
					if (!listUValMax[iItem].Third)
					{
						markedMax++;
						listUValMax.RemoveAt(iItem);
						iItem--;
					}
				}
				notMarkedMax = listUValMax.Count;

				// если что-то еще осталось, значит режим можно обработать :)
				if (listUVal.Count > 0) minModeValid = true;
				if (listUValMax.Count > 0) maxModeValid = true;

				if (float.IsNaN(constrNPLtopMax_)) constrNPLtopMax_ = regInfo_.Constraints.UDeviationUp95;
				if (float.IsNaN(constrNPLtopMin_)) constrNPLtopMin_ = regInfo_.Constraints.UDeviationUp95;
				if (float.IsNaN(constrUPLtopMax_)) constrUPLtopMax_ = regInfo_.Constraints.UDeviationUp100;
				if (float.IsNaN(constrUPLtopMin_)) constrUPLtopMin_ = regInfo_.Constraints.UDeviationUp100;
				if (float.IsNaN(constrNPLbottomMax_)) constrNPLbottomMax_ = regInfo_.Constraints.UDeviationDown95;
				if (float.IsNaN(constrNPLbottomMin_)) constrNPLbottomMin_ = regInfo_.Constraints.UDeviationDown95;
				if (float.IsNaN(constrUPLbottomMax_)) constrUPLbottomMax_ = regInfo_.Constraints.UDeviationDown100;
				if (float.IsNaN(constrUPLbottomMin_)) constrUPLbottomMin_ = regInfo_.Constraints.UDeviationDown100;

				// теперь считаем сколько в НДП и ПДП (наиб.нагрузки)
				int betweenNPLandUPLmax = 0, overUPLmax = 0, inNPLmax = 0;
				// для положит.фаз используем уставки top, для отриц.фаз используем bottom
				float curConstrNPLMax = constrNPLtopMax_;
				float curConstrUPLMax = constrUPLtopMax_;
				if (!positivePhase)
				{
					curConstrNPLMax = constrNPLbottomMax_;
					curConstrUPLMax = constrUPLbottomMax_;
				}
				if (maxModeValid)
				{
					for (int iItem = 0; iItem < listUValMax.Count; ++iItem)
					{
						// in NPL
						if (listUValMax[iItem].Second <= curConstrNPLMax)
						{
							inNPLmax++;
						}
						// если уставка НДП = 0, то ее как бы нет
						else if (curConstrNPLMax == 0 && listUValMax[iItem].Second < curConstrUPLMax)
						{
							inNPLmax++;
						}
						// over UPL
						else if (listUValMax[iItem].Second > curConstrUPLMax)
						{
							overUPLmax++;
						}
						else // between NPL and UPL
						{
							betweenNPLandUPLmax++;
						}
					}
				}

				// теперь считаем сколько в НДП и ПДП (наим.нагрузки)
				int betweenNPLandUPLmin = 0, overUPLmin = 0, inNPLmin = 0;
				// для положит.фаз используем уставки top, для отриц.фаз используем bottom
				float curConstrNPLMin = constrNPLtopMin_;
				float curConstrUPLMin = constrUPLtopMin_;
				if (!positivePhase)
				{
					curConstrNPLMin = constrNPLbottomMin_;
					curConstrUPLMin = constrUPLbottomMin_;
				}
				if (minModeValid)
				{
					for (int iItem = 0; iItem < listUVal.Count; ++iItem)
					{
						// in NPL
						if (listUVal[iItem].Second <= curConstrNPLMin)
						{
							inNPLmin++;
						}
						// если уставка НДП = 0, то ее как бы нет
						else if (curConstrNPLMin == 0 && listUVal[iItem].Second < curConstrUPLMin)
						{
							inNPLmin++;
						}
						// over UPL
						else if (listUVal[iItem].Second > curConstrUPLMin)
						{
							overUPLmin++;
						}
						else // between NPL and UPL
						{
							betweenNPLandUPLmin++;
						}
					}
				}

				// sort values
				if (minModeValid) listUVal.Sort(CompareUValues);
				if (maxModeValid) listUValMax.Sort(CompareUValues);

				// максимальные
				if (minModeValid) maxMinMode = listUVal[listUVal.Count - 1].Second;
				if (maxModeValid) maxMaxMode = listUValMax[listUValMax.Count - 1].Second;

				// верхние
				// удаляем 5% наибольших значений
				if (minModeValid)
				{
					int perc5 = listUVal.Count * 5 / 100;
					if (perc5 > 0)
					{
						upValueValidMin = true;
						listUVal.RemoveRange(listUVal.Count - perc5, perc5);
						upMinMode = listUVal[listUVal.Count - 1].Second;
					}
				}
				if (maxModeValid)
				{
					int perc5 = listUValMax.Count * 5 / 100;
					if (perc5 > 0)
					{
						upValueValidMax = true;
						listUValMax.RemoveRange(listUValMax.Count - perc5, perc5);
						upMaxMode = listUValMax[listUValMax.Count - 1].Second;
					}
				}

				#endregion

				#region Add to table

				float percentNorm = 0, percentBetween = 0, percentOut = 0;
				Int64 coefTime = 600 * TimeSpan.TicksPerSecond; // 600 seconds
				// добавляем строку в таблицу (наиб. нагрузки)
				if (maxModeValid)
				{
					string sign = " '";
					// sample //////////////////////////////
					ListViewItem itm = new ListViewItem("δU " + phase + ' ' + sign);
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add((markedMax + notMarkedMax).ToString());
					itm.SubItems.Add(markedMax.ToString());
					itm.SubItems.Add(notMarkedMax.ToString());
					lvUSample.Items.Add(itm);

					// statistics /////////////////////////
					int rowIndex = dgvUStatistics.Rows.Add();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.PARAM].Value =
					"δU " + phase + ' ' + sign;
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM].Value =
						inNPLmax.ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETWEEN].Value =
						betweenNPLandUPLmax.ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT].Value =
						overUPLmax.ToString();
					// percent
					if (notMarkedMax > 0)
					{
						percentNorm = inNPLmax * 100.0F / notMarkedMax;
						percentBetween = betweenNPLandUPLmax * 100.0F / notMarkedMax;
						percentOut = overUPLmax * 100.0F / notMarkedMax;
					}
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_PERC].Value =
						percentNorm.ToString(settings_.FloatFormat);
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_PERC].Value =
						percentBetween.ToString(settings_.FloatFormat);
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_PERC].Value =
						percentOut.ToString(settings_.FloatFormat);
					// time
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_TIME].Value =
						TimeSpan.FromTicks(inNPLmax * coefTime).ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_TIME].Value =
						TimeSpan.FromTicks(betweenNPLandUPLmax * coefTime).ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_TIME].Value =
						TimeSpan.FromTicks(overUPLmax * coefTime).ToString();

					if (inNPLmax > 0)
					{
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM].
							Style.BackColor = Color.LightGreen;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_PERC].
							Style.BackColor = Color.LightGreen;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_TIME].
							Style.BackColor = Color.LightGreen;
					}
					if (betweenNPLandUPLmax > 0)
					{
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETWEEN].
							Style.BackColor = Color.Yellow;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_PERC].
							Style.BackColor = Color.Yellow;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_TIME].
							Style.BackColor = Color.Yellow;
					}
					if (overUPLmax > 0)
					{
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT].
							Style.BackColor = Color.Salmon;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_PERC].
							Style.BackColor = Color.Salmon;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_TIME].
							Style.BackColor = Color.Salmon;
					}

					// values ////////////////////////////////
					itm = new ListViewItem("δU " + phase + ' ' + sign);
					itm.UseItemStyleForSubItems = false;
					if (upValueValidMax)
						itm.SubItems.Add(Math.Round(upMaxMode, settings_.FloatSigns).ToString(settings_.FloatFormat));
					else itm.SubItems.Add("-");
					itm.SubItems.Add(Math.Round(maxMaxMode, settings_.FloatSigns).ToString(settings_.FloatFormat));
					itm.SubItems.Add(constrNPLtopMax_.ToString(ConstraintsFormat));
					itm.SubItems.Add(constrUPLtopMax_.ToString(ConstraintsFormat));
					itm.SubItems.Add(constrNPLbottomMax_.ToString(ConstraintsFormat));
					itm.SubItems.Add(constrUPLbottomMax_.ToString(ConstraintsFormat));
					lvUValues.Items.Add(itm);
				}

				// добавляем строку в таблицу (наим. нагрузки)
				if (minModeValid)
				{
					string sign = " \"";
					// sample //////////////////////////////
					ListViewItem itm = new ListViewItem("δU " + phase + ' ' + sign);
					itm.UseItemStyleForSubItems = false;
					itm.SubItems.Add((markedMin + notMarkedMin).ToString());
					itm.SubItems.Add(markedMin.ToString());
					itm.SubItems.Add(notMarkedMin.ToString());
					lvUSample.Items.Add(itm);

					// statistics /////////////////////////
					int rowIndex = dgvUStatistics.Rows.Add();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.PARAM].Value =
					"δU " + phase + ' ' + sign;
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM].Value =
						inNPLmin.ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETWEEN].Value =
						betweenNPLandUPLmin.ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT].Value =
						overUPLmin.ToString();
					// percent
					if (notMarkedMin > 0)
					{
						percentNorm = inNPLmin * 100.0F / notMarkedMin;
						percentBetween = betweenNPLandUPLmin * 100.0F / notMarkedMin;
						percentOut = overUPLmin * 100.0F / notMarkedMin;
					}
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_PERC].Value =
						percentNorm.ToString(settings_.FloatFormat);
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_PERC].Value =
						percentBetween.ToString(settings_.FloatFormat);
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_PERC].Value =
						percentOut.ToString(settings_.FloatFormat);
					// time
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_TIME].Value =
						TimeSpan.FromTicks(inNPLmin * coefTime).ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_TIME].Value =
						TimeSpan.FromTicks(betweenNPLandUPLmin * coefTime).ToString();
					dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_TIME].Value =
						TimeSpan.FromTicks(overUPLmin * coefTime).ToString();

					if (inNPLmin > 0)
					{
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM].
							Style.BackColor = Color.LightGreen;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_PERC].
							Style.BackColor = Color.LightGreen;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.NORM_TIME].
							Style.BackColor = Color.LightGreen;
					}
					if (betweenNPLandUPLmin > 0)
					{
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETWEEN].
							Style.BackColor = Color.Yellow;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_PERC].
							Style.BackColor = Color.Yellow;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.BETW_TIME].
							Style.BackColor = Color.Yellow;
					}
					if (overUPLmin > 0)
					{
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT].
							Style.BackColor = Color.Salmon;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_PERC].
							Style.BackColor = Color.Salmon;
						dgvUStatistics.Rows[rowIndex].Cells[(int)StaticticsColumns.OUT_TIME].
							Style.BackColor = Color.Salmon;
					}

					// values ////////////////////////////////
					itm = new ListViewItem("δU " + phase + ' ' + sign);
					itm.UseItemStyleForSubItems = false;
					if (upValueValidMin)
						itm.SubItems.Add(Math.Round(upMinMode, settings_.FloatSigns).ToString(settings_.FloatFormat));
					else itm.SubItems.Add("-");
					itm.SubItems.Add(Math.Round(maxMinMode, settings_.FloatSigns).ToString(settings_.FloatFormat));
					itm.SubItems.Add(constrNPLtopMin_.ToString(ConstraintsFormat));
					itm.SubItems.Add(constrUPLtopMin_.ToString(ConstraintsFormat));
					itm.SubItems.Add(constrNPLbottomMin_.ToString(ConstraintsFormat));
					itm.SubItems.Add(constrUPLbottomMin_.ToString(ConstraintsFormat));
					lvUValues.Items.Add(itm);
				}

				#endregion

				#region Add to archive info

				regInfo_.Constraints.MaxModeUDeviationUp100 = constrUPLtopMax_;
				regInfo_.Constraints.MaxModeUDeviationDown100 = constrUPLbottomMax_;
				regInfo_.Constraints.MinModeUDeviationUp100 = constrUPLtopMin_;
				regInfo_.Constraints.MinModeUDeviationDown100 = constrUPLbottomMin_;

				archiveFields_.MaxModeExists = maxModeValid;
				if (maxModeValid)
				{
					switch (phase)
					{
						case "A+":
							archiveFields_.MaxModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MaxModeUAABDeviationPosNonflaggedT2 = (ushort)overUPLmax;
							archiveFields_.MaxModeUAABDeviationPos100 = maxMaxMode;
							break;
						case "A-":
							archiveFields_.MaxModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MaxModeUAABDeviationNegNonflaggedT2 = (ushort)overUPLmax;
							archiveFields_.MaxModeUAABDeviationNeg100 = maxMaxMode;
							break;
						case "B+":
							archiveFields_.MaxModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MaxModeUBBCDeviationPosNonflaggedT2 = (ushort)overUPLmax;
							archiveFields_.MaxModeUBBCDeviationPos100 = maxMaxMode;
							break;
						case "B-":
							archiveFields_.MaxModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MaxModeUBBCDeviationNegNonflaggedT2 = (ushort)overUPLmax;
							archiveFields_.MaxModeUBBCDeviationNeg100 = maxMaxMode;
							break;
						case "C+":
							archiveFields_.MaxModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MaxModeUCCADeviationPosNonflaggedT2 = (ushort)overUPLmax;
							archiveFields_.MaxModeUCCADeviationPos100 = maxMaxMode;
							break;
						case "C-":
							archiveFields_.MaxModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MaxModeUCCADeviationNegNonflaggedT2 = (ushort)overUPLmax;
							archiveFields_.MaxModeUCCADeviationNeg100 = maxMaxMode;
							break;
					}
				}

				archiveFields_.MinModeExists = minModeValid;
				if (minModeValid)
				{
					switch (phase)
					{
						case "A+":
							archiveFields_.MinModeTenMinuteCounterNonflagged = (ushort)notMarkedMin;
							archiveFields_.MinModeUAABDeviationPosNonflaggedT2 = (ushort)overUPLmin;
							archiveFields_.MinModeUAABDeviationPos100 = maxMinMode;
							break;
						case "A-":
							archiveFields_.MinModeTenMinuteCounterNonflagged = (ushort)notMarkedMin;
							archiveFields_.MinModeUAABDeviationNegNonflaggedT2 = (ushort)overUPLmin;
							archiveFields_.MinModeUAABDeviationNeg100 = maxMinMode;
							break;
						case "B+":
							archiveFields_.MinModeTenMinuteCounterNonflagged = (ushort)notMarkedMin;
							archiveFields_.MinModeUBBCDeviationPosNonflaggedT2 = (ushort)overUPLmin;
							archiveFields_.MinModeUBBCDeviationPos100 = maxMinMode;
							break;
						case "B-":
							archiveFields_.MinModeTenMinuteCounterNonflagged = (ushort)notMarkedMin;
							archiveFields_.MinModeUBBCDeviationNegNonflaggedT2 = (ushort)overUPLmin;
							archiveFields_.MinModeUBBCDeviationNeg100 = maxMinMode;
							break;
						case "C+":
							archiveFields_.MinModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MinModeUCCADeviationPosNonflaggedT2 = (ushort)overUPLmin;
							archiveFields_.MinModeUCCADeviationPos100 = maxMinMode;
							break;
						case "C-":
							archiveFields_.MinModeTenMinuteCounterNonflagged = (ushort)notMarkedMax;
							archiveFields_.MinModeUCCADeviationNegNonflaggedT2 = (ushort)overUPLmin;
							archiveFields_.MinModeUCCADeviationNeg100 = maxMinMode;
							break;
					}
				}

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in AddMinMaxModeString()");
				throw;
			}
		}

		private void SeparateMinMaxMode(ref List<Trio<DateTime, float, bool>> listUValpos,
										ref List<Trio<DateTime, float, bool>> listUValposMax)
		{
			try
			{
				// проверяем заданы ли вообще режимы (если времена равны, то считается, что не задан)
				bool validPeriod1 = true, validPeriod2 = true;
				if (dtMaxModeStart1_.TimeOfDay == dtMaxModeEnd1_.TimeOfDay) validPeriod1 = false;
				if (dtMaxModeStart2_.TimeOfDay == dtMaxModeEnd2_.TimeOfDay) validPeriod2 = false;

				// эти переменные нужны, чтобы обработать ситуацию когда например режим с 23:50 до 00:10
				bool timeIsConsistent1 = true, timeIsConsistent2 = true;
				if (dtMaxModeStart1_.TimeOfDay > dtMaxModeEnd1_.TimeOfDay) timeIsConsistent1 = false;
				if (dtMaxModeStart2_.TimeOfDay > dtMaxModeEnd2_.TimeOfDay) timeIsConsistent2 = false;

				for (int iItem = 0; iItem < listUValpos.Count; ++iItem)
				{
					bool isMax = false;
					if (validPeriod1)
					{
						if (timeIsConsistent1)
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart1_.TimeOfDay &&
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd1_.TimeOfDay)
								isMax = true;
						}
						else
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart1_.TimeOfDay ||
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd1_.TimeOfDay)
								isMax = true;
						}
					}

					if (validPeriod2)
					{
						if (timeIsConsistent2)
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart2_.TimeOfDay &&
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd2_.TimeOfDay)
								isMax = true;
						}
						else
						{
							if (listUValpos[iItem].First.TimeOfDay >= dtMaxModeStart2_.TimeOfDay ||
								listUValpos[iItem].First.TimeOfDay <= dtMaxModeEnd2_.TimeOfDay)
								isMax = true;
						}
					}

					if (isMax)
					{
						listUValposMax.Add(listUValpos[iItem]);
						listUValpos.RemoveAt(iItem);
						iItem--;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SeparateMinMaxMode()");
				throw;
			}
		}

		private static int CompareUValues(Trio<DateTime, float, bool> v1, Trio<DateTime, float, bool> v2)
		{
			if (v1.Second > v2.Second) return 1;
			else if(v1.Second < v2.Second) return -1;
			return 0;
		}

		#endregion

		private void AddUFCurver(ref ZedGraphControl zedGraph, ref DataGridView dgv,
								int columnTime, int columnValue,
								Color curveColor, float width, string title,
								bool[] valid)
		{
			try
			{
				GraphPane gPane = zedGraph.GraphPane;
				gPane.YAxis.MajorGrid.IsZeroLine = true;
				gPane.YAxis.MajorGrid.IsVisible = true;
				gPane.XAxis.MajorGrid.IsVisible = true;
				gPane.Title.Text = title;
				PointPairList list = new PointPairList();

				int startIndex = 0;
				while (!valid[startIndex])
				{
					startIndex++;
					if (startIndex >= valid.Length)
					{
						EmService.WriteToLogFailed("AddUFCurver: no valid values!");
						return;
					}
				}

				double _y = Conversions.object_2_double(dgv[columnValue, startIndex].Value);
				double _x1 = new XDate(Convert.ToDateTime(dgv[columnTime, startIndex].Value));
				list.Add(_x1, _y);

				for (int i = startIndex; i < dgv.Rows.Count - 1; i++)
				{
					double y = Conversions.object_2_double(dgv[columnValue, i + 1].Value);
					double x0 = new XDate(Convert.ToDateTime(dgv[columnTime, i].Value));
					list.Add(x0, y);
					double x1 = new XDate(Convert.ToDateTime(dgv[columnTime, i + 1].Value));
					list.Add(x1, y);
				}

				string legend = dgv.Columns[columnValue].HeaderText;
				//legend += " (L)";

				LineItem myCurve = gPane.AddCurve(legend, list, curveColor, SymbolType.None);
				myCurve.Line.Width = width;

				//gPane.AxisChange(this.CreateGraphics());

				// Axis X, Y and Y2
				gPane.XAxis.IsVisible = true;
				//gPane.XAxis.IsVisible = tsXGridLine.Checked;
				//gPane.XAxis.IsShowGrid = tsXGridLine.Checked;
				//gPane.XAxis.IsShowMinorGrid = tsXMinorGridLine.Checked;

				gPane.YAxis.IsVisible = true;
				//gPane.YAxis.IsVisible = tsYGridLine.Checked;
				//gPane.YAxis.IsShowGrid = tsYGridLine.Checked;
				//gPane.YAxis.IsShowMinorGrid = tsYMinorGridLine.Checked;

				//gPane.Y2Axis.IsVisible = tsY2GridLine.Checked;
				//gPane.Y2Axis.IsShowGrid = tsY2GridLine.Checked;
				//gPane.Y2Axis.IsShowMinorGrid = tsY2MinorGridLine.Checked;

				// Zoom set defalult
				Graphics g = zedGraph.CreateGraphics();
				gPane.XAxis.ResetAutoScale(gPane, g);
				gPane.YAxis.ResetAutoScale(gPane, g);
				gPane.Y2Axis.ResetAutoScale(gPane, g);
				g.Dispose();
				zedGraph.Refresh();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error AddGraphCurver():");
				//throw;
			}
		}

		public bool IsNonsinusPageActive()
		{
			return tabControlMain.SelectedTab == tpNonsinus;
		}

		private void CreatePQPReport(PQPProtocolType protocolType)
		{
			try
			{
				//EmService.ShowWndFeedback = true;

				//EmTreeNodeRegistration reg = (this.wndToolbox.ActiveNodePQP.ParentObject as EmTreeNodeRegistration);

				FormDocPQPReportSaveDialog wndPqpRepSettings = new FormDocPQPReportSaveDialog(
					Thread.CurrentThread.CurrentUICulture, true);
				if (wndPqpRepSettings.ShowDialog() != DialogResult.OK) return;

				string reportFileName = wndPqpRepSettings.txtFileName.Text;
				string reportNumber = wndPqpRepSettings.txtReportNumber.Text;
				string appendixNumber = wndPqpRepSettings.txtAppendixNumber.Text;
				bool bOpenReportAfterSaving = wndPqpRepSettings.chkOpenAfterSaving.Checked;

				if (Constants.AnalyseDeviceVersion(regInfo_.DeviceVersion) == DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
				{
					FormTemperatureReport frm = new FormTemperatureReport();
					if (frm.ShowDialog() == DialogResult.Cancel)
						return;
					//this.wndDocPQP.wndDocPQPMain.ExportPQPReport(
					//				regInfo_.ConnectionScheme, regInfo_.SerialNumber,
					//				version, regInfo_.GpsLatitude, regInfo_.GpsLongitude, regInfo_.AutocorrectTimeGpsEnable,
					//				frm.TemperatureMin, frm.TemperatureMax);

					PQPReportEtPQPA exporter = new PQPReportEtPQPA(ref settings_,
														ref archiveFields_, ref regInfo_,
														ref archiveInfo_, protocolType,
														reportFileName, reportNumber, appendixNumber,
														bOpenReportAfterSaving,
														DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073,
														frm.TemperatureMin, frm.TemperatureMax);

					exporter.ExportReport();
				}
				else
				{
					//this.wndDocPQP.wndDocPQPMain.ExportPQPReport(regInfo_.ConnectionScheme, regInfo_.SerialNumber,
					//		  version, regInfo_.GpsLatitude, regInfo_.GpsLongitude,
					//		  Constants.AnalyseDeviceVersion(regInfo_.DeviceVersion));

					PQPReportEtPQPA exporter;
					if (Constants.AnalyseDeviceVersion(regInfo_.DeviceVersion) != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					{
						exporter = new PQPReportEtPQPA(ref settings_,
														ref archiveFields_, ref regInfo_,
														ref archiveInfo_, protocolType,
														reportFileName, reportNumber, appendixNumber,
														bOpenReportAfterSaving,
														Constants.AnalyseDeviceVersion(regInfo_.DeviceVersion));
					}
					else
					{
						exporter = new PQPReportEtPQPA(ref settings_,
														ref archiveFields_, ref regInfo_,
														ref archiveInfo_, protocolType,
														reportFileName, reportNumber, appendixNumber,
														bOpenReportAfterSaving,
														Constants.AnalyseDeviceVersion(regInfo_.DeviceVersion));
					}
					exporter.ExportReport();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in CreatePQPReport()");
				throw;
			}
		}

		#region Form event handlers

		private void tsbFlickPhaseButton_Click(object sender, EventArgs e)
		{
			PaintFlickerCurves(tsbFlickPhaseA.Checked, tsbFlickPhaseALong.Checked,
				tsbFlickPhaseB.Checked, tsbFlickPhaseBLong.Checked,
				tsbFlickPhaseC.Checked, tsbFlickPhaseCLong.Checked);
		}

		private void chbGraphUPhase_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				string phase = ((CheckBox) sender).Text;
				bool check = ((CheckBox) sender).Checked;
				ZedGraphControl zedGraph = null;
				Color color = Color.Red;
				int columnValue = -1;
				string legend = string.Empty;

				switch (phase)
				{
					case "A+":
						zedGraph = zgcUA;
						color = Color.Red;
						columnValue = 1;
						legend = "δU(A+), %";
						break;
					case "A-":
						zedGraph = zgcUA;
						color = Color.Blue;
						columnValue = 2;
						legend = "δU(A-), %";
						break;
					case "B+":
						zedGraph = zgcUB;
						color = Color.Red;
						columnValue = 3;
						legend = "δU(B+), %";
						break;
					case "B-":
						zedGraph = zgcUB;
						color = Color.Blue;
						columnValue = 4;
						legend = "δU(B-), %";
						break;
					case "C+":
						zedGraph = zgcUC;
						color = Color.Red;
						columnValue = 5;
						legend = "δU(C+), %";
						break;
					case "C-":
						zedGraph = zgcUC;
						color = Color.Blue;
						columnValue = 6;
						legend = "δU(C-), %";
						break;
				}

				if (check)
				{
					AddUFCurver(ref zedGraph, ref dgvUValues, 0, columnValue, color, 1.5f, 
								"Voltage Deviation, Phase " + phase[0], archiveFields_.TenMinuteNotMarked);
				}
				else
				{
					// delete curve
					GraphPane gPane = zedGraph.GraphPane;
					if (gPane.CurveList.IndexOf(legend) >= 0)
					{
						gPane.CurveList.Remove(gPane.CurveList[legend]);
					}

					// Zoom set defalult
					Graphics g = zedGraph.CreateGraphics();
					gPane.XAxis.ResetAutoScale(gPane, g);
					gPane.YAxis.ResetAutoScale(gPane, g);
					gPane.Y2Axis.ResetAutoScale(gPane, g);
					g.Dispose();
					zedGraph.Refresh();
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in chbGraphUPhase_CheckedChanged");
				//throw;
			}
		}

		private void pbtnPeakLoad_Click(object sender, EventArgs e)
		{
			try
			{
				FormPeakLoadTime wnd = new FormPeakLoadTime(
					dtMaxModeStart1_, dtMaxModeEnd1_, dtMaxModeStart2_, dtMaxModeEnd2_,
					constrNPLtopMax_, constrUPLtopMax_, constrNPLbottomMax_, constrUPLbottomMax_,
					constrNPLtopMin_, constrUPLtopMin_, constrNPLbottomMin_, constrUPLbottomMin_);
				if (wnd.ShowDialog(formMain_) != DialogResult.OK)
					return;
				dtMaxModeStart1_ = wnd.TimeMaxStart1;
				dtMaxModeEnd1_ = wnd.TimeMaxEnd1;
				if (dtMaxModeStart1_ == dtMaxModeEnd1_)
				{
					dtMaxModeStart1_ = DateTime.MinValue;
					dtMaxModeEnd1_ = DateTime.MinValue;
				}

				dtMaxModeStart2_ = wnd.TimeMaxStart2;
				dtMaxModeEnd2_ = wnd.TimeMaxEnd2;
				if (dtMaxModeStart2_ == dtMaxModeEnd2_)
				{
					dtMaxModeStart2_ = DateTime.MinValue;
					dtMaxModeEnd2_ = DateTime.MinValue;
				}

				if (!wnd.GetConstrNPLtopMaxMode(out constrNPLtopMax_)) constrNPLtopMax_ = float.NaN;
				if (!wnd.GetConstrUPLtopMaxMode(out constrUPLtopMax_)) constrUPLtopMax_ = float.NaN;
				if (!wnd.GetConstrNPLbottomMaxMode(out constrNPLbottomMax_)) constrNPLbottomMax_ = float.NaN;
				if (!wnd.GetConstrUPLbottomMaxMode(out constrUPLbottomMax_)) constrUPLbottomMax_ = float.NaN;
				if (!wnd.GetConstrNPLtopMinMode(out constrNPLtopMin_)) constrNPLtopMin_ = float.NaN;
				if (!wnd.GetConstrUPLtopMinMode(out constrUPLtopMin_)) constrUPLtopMin_ = float.NaN;
				if (!wnd.GetConstrNPLbottomMinMode(out constrNPLbottomMin_)) constrNPLbottomMin_ = float.NaN;
				if (!wnd.GetConstrUPLbottomMinMode(out constrUPLbottomMin_)) constrUPLbottomMin_ = float.NaN;

				bNeedMaxModeForEtPQP_A_ = true;
				if (dtMaxModeStart1_ == DateTime.MinValue && dtMaxModeEnd1_ == DateTime.MinValue &&
					dtMaxModeStart2_ == DateTime.MinValue && dtMaxModeEnd2_ == DateTime.MinValue)
					bNeedMaxModeForEtPQP_A_ = false;

				FillVoltageData(true);

				labelUPeakTime1.Text = dtMaxModeStart1_.ToString("HH:mm") + " - " + dtMaxModeEnd1_.ToString("HH:mm");
				labelUPeakTime2.Text = dtMaxModeStart2_.ToString("HH:mm") + " - " + dtMaxModeEnd2_.ToString("HH:mm");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pbtnPeakLoad_Click()");
				throw;
			}
		}

		public void Form_KeyDown(KeyEventArgs e)
		{
			ChangeNonsinusOrder(e.KeyData);
		}

		private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControlMain.SelectedTab == tpNonsinus)
			{
				// otherwise user wouldn't be able to use arrows to leaf harmonics
				// because arrows would leaf pages of the main tabControl.
				// so we have to set focus to any control of Nonsinus table page
				//btnCapNonsinusEmpty.Focus();
				//panelNonsinus2.Focus();
				dgvNonsinusStat2.Focus();
			}
		}

		private void tabControlNonsinus_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (tabControlMain.SelectedTab == tpNonsinusTables)
			{
				// otherwise user wouldn't be able to use arrows to leaf harmonics
				// because arrows would leaf pages of the Nonsinus tabControl.
				// so we have to set focus to any control of Nonsinus table page
				//btnCapNonsinusEmpty.Focus();
				//panelNonsinus2.Focus();
				dgvNonsinusStat2.Focus();
			}
		}

		private void rbNonsinusChart_CheckedChanged(object sender, EventArgs e)
		{
			DrawNonsinusCharts();
		}

		private void tsbFlickYGridLine_Click(object sender, EventArgs e)
		{
			zgcFlicker.GraphPane.YAxis.MajorGrid.IsVisible = tsbFlickYGridLine.Checked;
			zgcFlicker.Refresh();
		}

		private void tsbFlickYMinorGridLine_Click(object sender, EventArgs e)
		{
			zgcFlicker.GraphPane.YAxis.MinorGrid.IsVisible = tsbFlickYMinorGridLine.Checked;
			zgcFlicker.Refresh();
		}

		private void tsbFlickXGridLine_Click(object sender, EventArgs e)
		{
			zgcFlicker.GraphPane.XAxis.MajorGrid.IsVisible = tsbFlickXGridLine.Checked;
			zgcFlicker.Refresh();
		}

		private void tsbFlickXMinorGridLine_Click(object sender, EventArgs e)
		{
			zgcFlicker.GraphPane.XAxis.MinorGrid.IsVisible = tsbFlickXMinorGridLine.Checked;
			zgcFlicker.Refresh();
		}

		private void tsbFlickY2GridLine_Click(object sender, EventArgs e)
		{
			zgcFlicker.GraphPane.Y2Axis.MajorGrid.IsVisible = tsbFlickY2GridLine.Checked;
			zgcFlicker.Refresh();
		}

		private void tsbFlickY2MinorGridLine_Click(object sender, EventArgs e)
		{
			zgcFlicker.GraphPane.Y2Axis.MinorGrid.IsVisible = tsbFlickY2MinorGridLine.Checked;
			zgcFlicker.Refresh();
		}

		private void rbWayToShowStatistics_CheckedChanged(object sender, EventArgs e)
		{
			if (rbShowNumber.Checked)
				curWayToShowSamples_ = WayToShowStatistics.NUMBER;
			else if (rbShowPercentage.Checked)
				curWayToShowSamples_ = WayToShowStatistics.PERCENTAGE;
			else if (rbShowTime.Checked)
				curWayToShowSamples_ = WayToShowStatistics.TIME;

			List<DataGridView> dgvs = new List<DataGridView>();
			dgvs.Add(dgvFStatistics);
			dgvs.Add(dgvUStatistics);
			dgvs.Add(dgvNonsymmStatistics);
			dgvs.Add(dgvFlikStatST);
			dgvs.Add(dgvFlikStatLT);

			switch (curWayToShowSamples_)
			{
				case WayToShowStatistics.NUMBER:
					gbFStatistics.Text = gbUStatistics.Text = "Statistics";
					gbNonsymmStatistics.Text = gbFlickStatST.Text = gbFlickStatLT.Text = "Statistics";

					foreach (var dataGridView in dgvs)
					{
						dataGridView.Columns[(int)StaticticsColumns.NORM].Visible = true;
						dataGridView.Columns[(int)StaticticsColumns.BETWEEN].Visible = true;
						dataGridView.Columns[(int)StaticticsColumns.OUT].Visible = true;

						dataGridView.Columns[(int)StaticticsColumns.NORM_PERC].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.BETW_PERC].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.OUT_PERC].Visible = false;

						dataGridView.Columns[(int)StaticticsColumns.NORM_TIME].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.BETW_TIME].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.OUT_TIME].Visible = false;
					}
					break;
				case WayToShowStatistics.PERCENTAGE:
					gbFStatistics.Text = gbUStatistics.Text = "Statistics, %";
					gbNonsymmStatistics.Text = gbFlickStatST.Text = gbFlickStatLT.Text = "Statistics, %";

					foreach (var dataGridView in dgvs)
					{
						dataGridView.Columns[(int)StaticticsColumns.NORM].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.BETWEEN].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.OUT].Visible = false;

						dataGridView.Columns[(int)StaticticsColumns.NORM_PERC].Visible = true;
						dataGridView.Columns[(int)StaticticsColumns.BETW_PERC].Visible = true;
						dataGridView.Columns[(int)StaticticsColumns.OUT_PERC].Visible = true;

						dataGridView.Columns[(int)StaticticsColumns.NORM_TIME].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.BETW_TIME].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.OUT_TIME].Visible = false;
					}
					break;
				case WayToShowStatistics.TIME:
					gbFStatistics.Text = gbUStatistics.Text = "Statistics, hh:mm:ss";
					gbNonsymmStatistics.Text = gbFlickStatST.Text = gbFlickStatLT.Text = "Statistics, hh:mm:ss";

					foreach (var dataGridView in dgvs)
					{
						dataGridView.Columns[(int)StaticticsColumns.NORM].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.BETWEEN].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.OUT].Visible = false;

						dataGridView.Columns[(int)StaticticsColumns.NORM_PERC].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.BETW_PERC].Visible = false;
						dataGridView.Columns[(int)StaticticsColumns.OUT_PERC].Visible = false;

						dataGridView.Columns[(int)StaticticsColumns.NORM_TIME].Visible = true;
						dataGridView.Columns[(int)StaticticsColumns.BETW_TIME].Visible = true;
						dataGridView.Columns[(int)StaticticsColumns.OUT_TIME].Visible = true;
					}
					break;
			}

			FillNonsinusData(false, true);

			#region old code

			//switch (curWayToShowSamples_)
			//{
			//    case WayToShowStatistics.NUMBER:
			//        gbFStatistics.Text = gbUStatistics.Text = "Statistics";
			//        colhFStatNorm.Width = colhUStatNorm.Width = 75;
			//        colhFStatBetween.Width = colhUStatBetween.Width = 85;
			//        colhFStatOut.Width = colhUStatOut.Width = 80;
			//        colhFStatNormPerc.Width = colhUStatNormPerc.Width = 0;
			//        colhFStatBetweenPerc.Width = colhUStatBetweenPerc.Width = 0;
			//        colhFStatOutPerc.Width = colhUStatOutPerc.Width = 0;
			//        colhFStatNormTime.Width = colhUStatNormTime.Width = 0;
			//        colhFStatBetweenTime.Width = colhUStatBetweenTime.Width = 0;
			//        colhFStatOutTime.Width = colhUStatOutTime.Width = 0;

			//        gbNonsymmStatistics.Text = gbFlickStatST.Text = gbFlickStatLT.Text = "Statistics";
			//        colhNonsymmStatNorm.Width = colhFlickStatNormST.Width = colhFlickStatNormLT.Width = 75;
			//        colhNonsymmStatBetween.Width = colhFlickStatBetweenST.Width = colhFlickStatBetweenLT.Width = 85;
			//        colhNonsymmStatOut.Width = colhFlickStatOutST.Width = colhFlickStatOutLT.Width = 80;
			//        colhNonsymmStatNormPerc.Width = colhFlickStatNormPercST.Width = colhFlickStatNormPercLT.Width = 0;
			//        colhNonsymmStatBetweenPerc.Width =
			//            colhFlickStatBetweenPercST.Width = colhFlickStatBetweenPercLT.Width = 0;
			//        colhNonsymmStatOutPerc.Width = colhFlickStatOutPercST.Width = colhFlickStatOutPercLT.Width = 0;
			//        colhNonsymmStatNormTime.Width = colhFlickStatNormTimeST.Width = colhFlickStatNormTimeLT.Width = 0;
			//        colhNonsymmStatBetweenTime.Width =
			//            colhFlickStatBetweenTimeST.Width = colhFlickStatBetweenTimeLT.Width = 0;
			//        colhNonsymmStatOutTime.Width = colhFlickStatOutTimeST.Width = colhFlickStatOutTimeLT.Width = 0;
			//        break;
			//    case WayToShowStatistics.PERCENTAGE:
			//        gbFStatistics.Text = gbUStatistics.Text = "Statistics, %";
			//        colhFStatNorm.Width = colhUStatNorm.Width = 0;
			//        colhFStatBetween.Width = colhUStatBetween.Width = 0;
			//        colhFStatOut.Width = colhUStatOut.Width = 0;
			//        colhFStatNormPerc.Width = colhUStatNormPerc.Width = 75;
			//        colhFStatBetweenPerc.Width = colhUStatBetweenPerc.Width = 85;
			//        colhFStatOutPerc.Width = colhUStatOutPerc.Width = 80;
			//        colhFStatNormTime.Width = colhUStatNormTime.Width = 0;
			//        colhFStatBetweenTime.Width = colhUStatBetweenTime.Width = 0;
			//        colhFStatOutTime.Width = colhUStatOutTime.Width = 0;

			//        gbNonsymmStatistics.Text = gbFlickStatST.Text = gbFlickStatLT.Text = "Statistics";
			//        colhNonsymmStatNorm.Width = colhFlickStatNormST.Width = colhFlickStatNormLT.Width = 0;
			//        colhNonsymmStatBetween.Width = colhFlickStatBetweenST.Width = colhFlickStatBetweenLT.Width = 0;
			//        colhNonsymmStatOut.Width = colhFlickStatOutST.Width = colhFlickStatOutLT.Width = 0;
			//        colhNonsymmStatNormPerc.Width = colhFlickStatNormPercST.Width = colhFlickStatNormPercLT.Width = 75;
			//        colhNonsymmStatBetweenPerc.Width =
			//            colhFlickStatBetweenPercST.Width = colhFlickStatBetweenPercLT.Width = 85;
			//        colhNonsymmStatOutPerc.Width = colhFlickStatOutPercST.Width = colhFlickStatOutPercLT.Width = 80;
			//        colhNonsymmStatNormTime.Width = colhFlickStatNormTimeST.Width = colhFlickStatNormTimeLT.Width = 0;
			//        colhNonsymmStatBetweenTime.Width =
			//            colhFlickStatBetweenTimeST.Width = colhFlickStatBetweenTimeLT.Width = 0;
			//        colhNonsymmStatOutTime.Width = colhFlickStatOutTimeST.Width = colhFlickStatOutTimeLT.Width = 0;
			//        break;
			//    case WayToShowStatistics.TIME:
			//        gbFStatistics.Text = gbUStatistics.Text = "Statistics, hh:mm:ss";
			//        colhFStatNorm.Width = colhUStatNorm.Width = 0;
			//        colhFStatBetween.Width = colhUStatBetween.Width = 0;
			//        colhFStatOut.Width = colhUStatOut.Width = 0;
			//        colhFStatNormPerc.Width = colhUStatNormPerc.Width = 0;
			//        colhFStatBetweenPerc.Width = colhUStatBetweenPerc.Width = 0;
			//        colhFStatOutPerc.Width = colhUStatOutPerc.Width = 0;
			//        colhFStatNormTime.Width = colhUStatNormTime.Width = 75;
			//        colhFStatBetweenTime.Width = colhUStatBetweenTime.Width = 85;
			//        colhFStatOutTime.Width = colhUStatOutTime.Width = 80;

			//        gbNonsymmStatistics.Text = gbFlickStatST.Text = gbFlickStatLT.Text = "Statistics";
			//        colhNonsymmStatNorm.Width = colhFlickStatNormST.Width = colhFlickStatNormLT.Width = 0;
			//        colhNonsymmStatBetween.Width = colhFlickStatBetweenST.Width = colhFlickStatBetweenLT.Width = 0;
			//        colhNonsymmStatOut.Width = colhFlickStatOutST.Width = colhFlickStatOutLT.Width = 0;
			//        colhNonsymmStatNormPerc.Width = colhFlickStatNormPercST.Width = colhFlickStatNormPercLT.Width = 0;
			//        colhNonsymmStatBetweenPerc.Width =
			//            colhFlickStatBetweenPercST.Width = colhFlickStatBetweenPercLT.Width = 0;
			//        colhNonsymmStatOutPerc.Width = colhFlickStatOutPercST.Width = colhFlickStatOutPercLT.Width = 0;
			//        colhNonsymmStatNormTime.Width = colhFlickStatNormTimeST.Width = colhFlickStatNormTimeLT.Width = 75;
			//        colhNonsymmStatBetweenTime.Width =
			//            colhFlickStatBetweenTimeST.Width = colhFlickStatBetweenTimeLT.Width = 85;
			//        colhNonsymmStatOutTime.Width = colhFlickStatOutTimeST.Width = colhFlickStatOutTimeLT.Width = 80;
			//        break;
			//}

			#endregion
		}

		private void btnCapNonsinusLeft_Click(object sender, EventArgs e)
		{
			ChangeNonsinusOrder(Keys.Left);
		}

		private void btnCapNonsinusRight_Click(object sender, EventArgs e)
		{
			ChangeNonsinusOrder(Keys.Right);
		}

		private void UserControlPqpPage_KeyDown(object sender, KeyEventArgs e)
		{
			if (tabControlMain.SelectedTab == tpNonsinus)
			{
				if (e.KeyData == Keys.Left || e.KeyData == Keys.Right)
				{
					ChangeNonsinusOrder(e.KeyData);
					e.Handled = true;
				}
			}
		}

		private void pbtnPqpReport_Click(object sender, EventArgs e)
		{
			try
			{
				formMain_.Cursor = Cursors.WaitCursor;

				CreatePQPReport(PQPProtocolType.VERSION_3);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in pbtnPqpReport_Click()");
				//???????????? message
				//throw;
			}
			finally
			{
				Environment.CurrentDirectory = EmService.AppDirectory;
				formMain_.Cursor = Cursors.Default;
			}
		}

		#endregion
	}
}
