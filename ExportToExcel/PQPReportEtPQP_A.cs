using System;
using System.Collections.Generic;
using System.IO;

using EmServiceLib;
using FileAnalyzerLib;

namespace ExportToExcel
{
    public class PQPReportEtPQPA : PQPReportBase
    {
		protected const short TemperatureMinLimit = 18;	// нижняя граница диапазона норм.температур для ЭтПКЭ-А
		protected const short TemperatureMaxLimit = 28;	// верхняя граница диапазона норм.температур для ЭтПКЭ-А

		protected PQPProtocolType protocolType_;
		protected double gpsLatitude_ = 0;
		protected double gpsLongitude_ = 0;
		protected DEVICE_VERSIONS deviceVersions_;
		protected short temperatureMin_ = Int16.MaxValue;
		protected short temperatureMax_ = Int16.MaxValue;

		public PQPReportEtPQPA(ref EmSettings settings, ref PqpArchiveFields archiveFields,
							ref RegistrationInfo regInfo, ref PqpArchiveInfo archiveInfo,
							PQPProtocolType type, string reportFileName,
							string reportNumber, string appendixNumber, bool bOpenReportAfterSaving,
							DEVICE_VERSIONS deviceVersions)
            : base(ref settings, ref archiveFields, ref regInfo, ref archiveInfo, reportFileName, reportNumber,
					appendixNumber, bOpenReportAfterSaving)
        {
			protocolType_ = type;
			gpsLatitude_ = regInfo_.GpsLatitude;
			gpsLongitude_ = regInfo_.GpsLongitude;
			deviceVersions_ = deviceVersions;
        }

		// добавлена температура
		public PQPReportEtPQPA(ref EmSettings settings, ref PqpArchiveFields archiveFields,
							ref RegistrationInfo regInfo, ref PqpArchiveInfo archiveInfo,
							PQPProtocolType type, string reportFileName,
							string reportNumber, string appendixNumber, bool bOpenReportAfterSaving,
							DEVICE_VERSIONS deviceVersions,
							short temperatureMin, short temperatureMax)
			: base(ref settings, ref archiveFields, ref regInfo, ref archiveInfo, reportFileName, reportNumber,
					appendixNumber, bOpenReportAfterSaving)
		{
			protocolType_ = type;
			gpsLatitude_ = regInfo_.GpsLatitude;
			gpsLongitude_ = regInfo_.GpsLongitude;
			deviceVersions_ = deviceVersions;
			temperatureMin_ = temperatureMin;
			temperatureMax_ = temperatureMax;
		}

        /// <summary>
        /// Exports all PQP data into Excel format
        /// </summary>
        public void ExportReport()
        {
            string out_fn = string.Empty;
            try
            {
                StyleName_ = string.Empty;

                //bool bEXIT = false;			// exit flag
                string rt = string.Empty;	// text to process

                // reading template file
                string templateFileName = string.Empty;
				switch (regInfo_.ConnectionScheme)
				{
					case ConnectScheme.Ph3W4:
						switch (protocolType_)
						{
							case PQPProtocolType.VERSION_1:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph4w"; break;
							case PQPProtocolType.VERSION_2:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph4w_v2"; break;
							case PQPProtocolType.VERSION_3:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph4w_v3"; break;
						}
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						switch (protocolType_)
						{
							case PQPProtocolType.VERSION_1:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph3w"; break;
							case PQPProtocolType.VERSION_2:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph3w_v2"; break;
							case PQPProtocolType.VERSION_3:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_3ph3w_v3"; break;
						}
						break;
					case ConnectScheme.Ph1W2:
						switch (protocolType_)
						{
							case PQPProtocolType.VERSION_1:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_1ph2w"; break;
							case PQPProtocolType.VERSION_2:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_1ph2w_v2"; break;
							case PQPProtocolType.VERSION_3:
								templateFileName = EmService.AppDirectory + @"templates\PQPReportEtPQP_A_1ph2w_v3"; break;
						}
						break;
				}
				if (deviceVersions_ != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073) templateFileName += "_old";
	            templateFileName += ".xml";

                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(templateFileName, System.Text.Encoding.UTF8);
                    rt = sr.ReadToEnd();
                }
                catch
                {
                    //MessageBoxes.PqpReportTmplateError(this, templateFileName);?????????????????
                    return;
                }
                finally { if (sr != null) sr.Close(); }

                // Header
                ExprortReportPQPHead(ref rt, appendixNumber_, reportNumber_);

				// GPS
				//if (gpsLatitude_ > 0 || gpsLongitude_ > 0)
				//{
				//    rt = rt.Replace("{gps_latitude}", gpsLatitude_.ToString());
				//    rt = rt.Replace("{gps_longitude}", gpsLongitude_.ToString());
				//}

                // parsing template file and inserting values

                // float format
                string fl = "0.00";
                if (settings_.FloatSigns > 1)
                {
                    fl = "0.";
                    for (int iSign = 0; iSign < settings_.FloatSigns; ++iSign) fl += "0";
                }

				// it's important to fill this table at first! lapses arrays are filled here
				ExportTemperatureTable(ref rt, fl);

				ExportFDeviation(ref rt, fl);
				ExportUDeviation(ref rt, fl);
				ExportNonsymmetry(ref rt, fl);
				ExportNonsinus(ref rt, fl);
				if (deviceVersions_ != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
					ExportDipAndSwell_OLD(ref rt, fl);
				else ExportDipAndSwell_GOST2014(ref rt, fl);
				ExportFlicker(ref rt, fl);
				ExportInterharm(ref rt, fl);
				ExportMarkedTable(ref rt, regInfo_.DtStart, regInfo_.DtEnd, fl);

                // напоследок убиваем все шаблоны, что не смогли заменить значениями
                System.Text.RegularExpressions.Regex regex =
                    new System.Text.RegularExpressions.Regex("{.*?}");
                rt = regex.Replace(rt, " - ");

                // saving file
                StreamWriter sw = null;
                try
                {
                    sw = new StreamWriter(out_fn, false, System.Text.Encoding.UTF8);
                    sw.Write(rt);
                }
                catch
                {
					//MessageBoxes.PqpReportWriteError(this, reportFileName_);????????????????????
                    return;
                }
                finally { if (sw != null) sw.Close(); }

                // and opening if needed
                if (bOpenReportAfterSaving_)
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "excel.exe";
                    process.StartInfo.Arguments = String.Format("\"{0}\"", out_fn);
                    process.StartInfo.WorkingDirectory = "";
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.Start();
                }
                else
                {
                    //MessageBoxes.PqpReportSavedSuccess(this, out_fn);??????????????????
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in ExportReportPQP_A():");
                //MessageBoxes.PqpReportWriteError(this, reportFileName_);????????????????????
                throw;
            }
        }

		// структура, описывающая одни сутки
		struct DayForMarkedTable 
		{
			public DateTime dtStart;
			public DateTime dtEnd;
			public int cntMarked;
			public int cntAll;
		}

		protected void ExportMarkedTable(ref string rt, DateTime dtStart, DateTime dtEnd, string fl)
		{
			try
			{
				// если архив не больше суток, то просто берем кол-во маркированных и подставляем в таблицу
				TimeSpan dtDiff = dtEnd - dtStart;
				if ((dtDiff.Days == 0) || 
					(dtDiff.Days == 1 && dtDiff.Hours == 0 && dtDiff.Minutes == 0 && dtDiff.Seconds == 0))
				{
					rt = rt.Replace("{start1}", dtStart.ToString());
					rt = rt.Replace("{end1}", dtEnd.ToString());

					float perc_marked = (float)archiveFields_.TenMinuteCounterFlagged * 100.0F / 
							((float)archiveFields_.TenMinuteCounterFlagged + (float)archiveFields_.TenMinuteCounterNonflagged);
					rt = rt.Replace("{mark_a_1}", perc_marked.ToString(fl));
					rt = rt.Replace("{mark_b_1}", perc_marked.ToString(fl));
					rt = rt.Replace("{mark_c_1}", perc_marked.ToString(fl));
				}
				else  // если в архиве больше суток, надо рассчитать все по суткам
				{
					DateTime curDateTime = dtStart;
					List<Pair<DateTime, bool>> listMarked = new List<Pair<DateTime, bool>>();
					for (int iSample = 0; iSample < archiveFields_.TenMinuteNotMarked.Length; iSample++)
					{
						listMarked.Add(new Pair<DateTime, bool>(curDateTime, archiveFields_.TenMinuteNotMarked[iSample]));
						curDateTime = curDateTime.AddMinutes(10);
					}

					bool notFinished = true;
					int dayNumber = 1;
					curDateTime = dtStart;
					while (notFinished)
					{
						DayForMarkedTable curData = new DayForMarkedTable();
						curData.dtStart = curDateTime;
						curData.dtEnd = curDateTime.AddDays(1);
						if (curData.dtEnd >= dtEnd)
						{
							if (curData.dtEnd > dtEnd) curData.dtEnd = dtEnd;
							notFinished = false;
						}

						// выбираем строки относящиеся к текущим суткам
						for (int iItem = 0; iItem < listMarked.Count; ++iItem)
						{
							if (listMarked[iItem].First >= curData.dtStart && listMarked[iItem].First <= curData.dtEnd)
							{
								curData.cntAll++;
								if (!listMarked[iItem].Second) curData.cntMarked++;
							}
						}

						rt = rt.Replace("{start" + dayNumber.ToString() + "}", curData.dtStart.ToString());
						rt = rt.Replace("{end" + dayNumber.ToString() + "}", curData.dtEnd.ToString());

						float perc_marked = 0;
						if (curData.cntAll != 0) perc_marked = curData.cntMarked * 100 / curData.cntAll;
						string strCurValue = (curData.cntAll != 0) ? perc_marked.ToString(fl) : "-";
						rt = rt.Replace("{mark_a_" + dayNumber.ToString() + "}", strCurValue);
						rt = rt.Replace("{mark_b_" + dayNumber.ToString() + "}", strCurValue);
						rt = rt.Replace("{mark_c_" + dayNumber.ToString() + "}", strCurValue);

						dayNumber++;
						curDateTime = curDateTime.AddDays(1);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportMarkedTable():");
				throw;
			}
		}

		private Pair<string, float> GetLapse(bool main, string name)
		{
			if (main)
			{
				foreach (var item in lapsesMain_)
				{
					if (item.First == name) return item;
				}
			}
			else
			{
				foreach (var item in lapsesAdditional_)
				{
					if (item.First == name) return item;
				}
			}
			return null;
		}

	    private List<Pair<string, float>> lapsesMain_ = new List<Pair<string, float>>();
		private List<Pair<string, float>> lapsesAdditional_ = new List<Pair<string, float>>(); 

		protected void ExportTemperatureTable(ref string rt, string fl)//, int Unom, 
			//float U0, float U1, float Ua, float Ub, float Uc)
		{
			fl = "0.000";  //we decided to always use 3 signs 

			// if user didn't introduce temperature values, exit the function
			if(temperatureMin_ == Int16.MaxValue || temperatureMax_ == Int16.MaxValue)
				return;

			#region Lapses

			lapsesMain_.Clear();
			lapsesAdditional_.Clear();

			lapsesMain_.Add(new Pair<string, float>("_df", 0.01f));
			lapsesMain_.Add(new Pair<string, float>("_f", 0.01f));
			//lapsesMain_.Add(new Pair<string, float>("_u_av", 0.1f));
			//lapsesMain_.Add(new Pair<string, float>("_u1", 0.1f));
			//lapsesMain_.Add(new Pair<string, float>("_ud", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_dU", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_angleU", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_k2u", 0.15f));
			lapsesMain_.Add(new Pair<string, float>("_ku", 5.0f));			// was 0.05
			lapsesMain_.Add(new Pair<string, float>("_ku_harm", 5.0f));		// was 0.05
			lapsesMain_.Add(new Pair<string, float>("_u_harm", 5.0f));
			lapsesMain_.Add(new Pair<string, float>("_u_interharm", 5.0f));
			//lapsesMain_.Add(new Pair<string, float>("_dip_u", 0.1f));
			//lapsesMain_.Add(new Pair<string, float>("_interr_u", 0.1f));
			lapsesMain_.Add(new Pair<string, float>("_dip_val", 0.2f));
			lapsesMain_.Add(new Pair<string, float>("_interr_t", 0.2f));
			lapsesMain_.Add(new Pair<string, float>("_dip_t", 0.02f));
			//lapsesMain_.Add(new Pair<string, float>("_u_over", 0.2f));
			lapsesMain_.Add(new Pair<string, float>("_over_t", 0.02f));
			lapsesMain_.Add(new Pair<string, float>("_flik_sh", 5.0f));
			lapsesMain_.Add(new Pair<string, float>("_flik_long", 5.0f));
			lapsesMain_.Add(new Pair<string, float>("_time_utc", 0.005f));
			lapsesMain_.Add(new Pair<string, float>("_time_not_utc", 0.5f));

			#endregion

			try
			{
				// calculate temperature difference
				// min
				int dMin, dMax, dAll;
				if (temperatureMin_ < TemperatureMinLimit)
					dMin = TemperatureMinLimit - temperatureMin_;
				else if (temperatureMin_ > TemperatureMaxLimit)
					dMin = temperatureMin_ - TemperatureMaxLimit;
				else dMin = 0;

				if (temperatureMax_ < TemperatureMinLimit)
					dMax = TemperatureMinLimit - temperatureMax_;
				else if (temperatureMax_ > TemperatureMaxLimit)
					dMax = temperatureMax_ - TemperatureMaxLimit;
				else dMax = 0;

				dAll = dMax > dMin ? dMax : dMin;
				rt = rt.Replace("{dTemper}", dAll.ToString());

				// calculate additional lapse
				//float normLapse_f = lapses_.Find(x => x.First.Equals("_df")).Second;
				//float additionalLapse_f = 0.02f /*const for EtPQP-A*/ * normLapse_f * dAll;
				//rt = rt.Replace("{tl_f}", additionalLapse_f.ToString(fl));
				foreach (var curPair in lapsesMain_)
				{
					float normLapse = curPair.Second;
					float additionalLapse = 0.02f /*const for EtPQP-A*/ * normLapse * dAll;
					rt = rt.Replace("{tl" + curPair.First + "}", additionalLapse.ToString(fl));
					lapsesAdditional_.Add(new Pair<string, float>(curPair.First, additionalLapse));
				}

				//////////////////////////////////////////////////////
				// fill inaccuracy in all other tables
				float lapseMain;
				float lapseAdd;

				if (!regInfo_.AutocorrectTimeGpsEnable)
				{
					EmService.WriteToLogGeneral(
						"ExportTemperatureTable: autocorrectTimeGpsEnable_ = false");
				}

				if ((gpsLatitude_ > 0 || gpsLongitude_ > 0) && regInfo_.AutocorrectTimeGpsEnable)
				{
					lapseMain = GetLapse(true, "_time_utc").Second;
					lapseAdd = GetLapse(false, "_time_utc").Second;
					rt = rt.Replace("{markir_data_lapse}", (lapseMain + lapseAdd).ToString(fl));
				}
				else
				{
					lapseMain = GetLapse(true, "_time_not_utc").Second;
					lapseAdd = GetLapse(false, "_time_not_utc").Second;
					rt = rt.Replace("{markir_data_lapse}", (lapseMain + lapseAdd).ToString(fl));
				}

				// dU
				lapseMain = GetLapse(true, "_dU").Second;
				lapseAdd = GetLapse(false, "_dU").Second;
				rt = rt.Replace("{dU_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// df
				lapseMain = GetLapse(true, "_df").Second;
				lapseAdd = GetLapse(false, "_df").Second;
				rt = rt.Replace("{df_lapse}", (lapseMain + lapseAdd).ToString(fl));
				
				// K2u, K0u
				lapseMain = GetLapse(true, "_k2u").Second;
				lapseAdd = GetLapse(false, "_k2u").Second;
				rt = rt.Replace("{dK2U_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// Ku
				lapseMain = GetLapse(true, "_ku").Second;
				lapseAdd = GetLapse(false, "_ku").Second;
				rt = rt.Replace("{dKu_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// Ku n
				lapseMain = GetLapse(true, "_ku_harm").Second;
				lapseAdd = GetLapse(false, "_ku_harm").Second;
				rt = rt.Replace("{dKun_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// dip an swell
				lapseMain = GetLapse(true, "_dip_val").Second;
				lapseAdd = GetLapse(false, "_dip_val").Second;
				rt = rt.Replace("{dipswell_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// fliker short
				lapseMain = GetLapse(true, "_flik_sh").Second;
				lapseAdd = GetLapse(false, "_flik_sh").Second;
				rt = rt.Replace("{dFst_lapse}", (lapseMain + lapseAdd).ToString(fl));

				// fliker long
				lapseMain = GetLapse(true, "_flik_long").Second;
				lapseAdd = GetLapse(false, "_flik_long").Second;
				rt = rt.Replace("{dFlt_lapse}", (lapseMain + lapseAdd).ToString(fl));
				///////////////////////////////////////////////////////
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportTemperatureTable():");
				throw;
			}
		}

		protected void ExportFDeviation(ref string rt, string fl)
		{
			try
			{
				if (archiveFields_.BoolFreqDeviationStatistics)
				{
					// lower value
					rt = rt.Replace("{dfнр}", archiveFields_.FreqDeviationDown95.ToString(fl));
					// min value
					rt = rt.Replace("{dfнмр}", archiveFields_.FreqDeviationDown100.ToString(fl));
					// upper value
					rt = rt.Replace("{dfвр}", archiveFields_.FreqDeviationUp95.ToString(fl));
					// max value
					rt = rt.Replace("{dfнбр}", archiveFields_.FreqDeviationUp100.ToString(fl));
				}

				// nominals
				rt = rt.Replace("{dfнн}", regInfo_.Constraints.FSynchroDown95.ToString(fl));
				rt = rt.Replace("{dfнмн}", regInfo_.Constraints.FSynchroDown100.ToString(fl));
				rt = rt.Replace("{dfвн}", regInfo_.Constraints.FSynchroUp95.ToString(fl));
				rt = rt.Replace("{dfнбн}", regInfo_.Constraints.FSynchroUp100.ToString(fl));

				ReplaceExcelName(ref rt, "dfT1", archiveFields_.FreqDeviationCounterLockedT1,
				                        archiveFields_.FreqDeviationCounterLockedT2,
				                        archiveFields_.FreqDeviationCounterLocked, fl);

				ReplaceExcelName(ref rt, "dfT2", archiveFields_.FreqDeviationCounterLockedT2,
										archiveFields_.FreqDeviationCounterLocked, fl);

				// Погрешности df
				rt = rt.Replace("{ddfр}", "±0,01 Гц. (абс.)");
				rt = rt.Replace("{ddfн}", "±0,01 Гц.");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportFDeviation():");
				throw;
			}
		}

		protected void ExportUDeviation(ref string rt, string fl)
		{
			try
			{
				if (protocolType_ == PQPProtocolType.VERSION_1)
				{
					rt = rt.Replace("{dUA-r}", archiveFields_.UAABDeviationNeg100.ToString(fl));
					rt = rt.Replace("{dUA+r}", archiveFields_.UAABDeviationPos100.ToString(fl));
					rt = rt.Replace("{dUB-r}", archiveFields_.UBBCDeviationNeg100.ToString(fl));
					rt = rt.Replace("{dUB+r}", archiveFields_.UBBCDeviationPos100.ToString(fl));
					rt = rt.Replace("{dUC-r}", archiveFields_.UCCADeviationNeg100.ToString(fl));
					rt = rt.Replace("{dUC+r}", archiveFields_.UCCADeviationPos100.ToString(fl));

					// nominals
					rt = rt.Replace("{dUA-n}", regInfo_.Constraints.UDeviationDown100.ToString(fl));
					rt = rt.Replace("{dUA+n}", regInfo_.Constraints.UDeviationUp100.ToString(fl));
					rt = rt.Replace("{dUB-n}", regInfo_.Constraints.UDeviationDown100.ToString(fl));
					rt = rt.Replace("{dUB+n}", regInfo_.Constraints.UDeviationUp100.ToString(fl));
					rt = rt.Replace("{dUC-n}", regInfo_.Constraints.UDeviationDown100.ToString(fl));
					rt = rt.Replace("{dUC+n}", regInfo_.Constraints.UDeviationUp100.ToString(fl));

					float t2a = archiveFields_.UAABDeviationPosNonflaggedT2 + archiveFields_.UAABDeviationNegNonflaggedT2;
					float t2b = archiveFields_.UBBCDeviationPosNonflaggedT2 + archiveFields_.UBBCDeviationNegNonflaggedT2;
					float t2c = archiveFields_.UCCADeviationPosNonflaggedT2 + archiveFields_.UCCADeviationNegNonflaggedT2;

					// num_not_marked не разбивается по фазам, поэтому можно использовать число из последней строки
					if (archiveFields_.TenMinuteCounterNonflagged != 0)
					{
						t2a /= archiveFields_.TenMinuteCounterNonflagged;
						t2b /= archiveFields_.TenMinuteCounterNonflagged;
						t2c /= archiveFields_.TenMinuteCounterNonflagged;

						t2a = (float)Math.Round(t2a * 100, settings_.FloatSigns);
						t2b = (float)Math.Round(t2b * 100, settings_.FloatSigns);
						t2c = (float)Math.Round(t2c * 100, settings_.FloatSigns);

						rt = rt.Replace("{dUAT2}", t2a.ToString(fl));
						rt = rt.Replace("{dUBT2}", t2b.ToString(fl));
						rt = rt.Replace("{dUCT2}", t2c.ToString(fl));
					}
				}

				#region Peak Load Mode

				// переделать через ReplaceExcelName ???????????????????????????????
				// режимы наибольших и наименьших нагрузок для В.2
				if (protocolType_ == PQPProtocolType.VERSION_2 || protocolType_ == PQPProtocolType.VERSION_3)
				{
					// nominals
					rt = rt.Replace("{dUA-n_max}", regInfo_.Constraints.MaxModeUDeviationDown100.ToString(fl));
					rt = rt.Replace("{dUA+n_max}", regInfo_.Constraints.MaxModeUDeviationUp100.ToString(fl));
					rt = rt.Replace("{dUB-n_max}", regInfo_.Constraints.MaxModeUDeviationDown100.ToString(fl));
					rt = rt.Replace("{dUB+n_max}", regInfo_.Constraints.MaxModeUDeviationUp100.ToString(fl));
					rt = rt.Replace("{dUC-n_max}", regInfo_.Constraints.MaxModeUDeviationDown100.ToString(fl));
					rt = rt.Replace("{dUC+n_max}", regInfo_.Constraints.MaxModeUDeviationUp100.ToString(fl));

					if (archiveFields_.MaxModeExists)
					{
						rt = rt.Replace("{dUA-r_max}", archiveFields_.MaxModeUAABDeviationNeg100.ToString(fl));
						rt = rt.Replace("{dUA+r_max}", archiveFields_.MaxModeUAABDeviationPos100.ToString(fl));
						rt = rt.Replace("{dUB-r_max}", archiveFields_.MaxModeUBBCDeviationNeg100.ToString(fl));
						rt = rt.Replace("{dUB+r_max}", archiveFields_.MaxModeUBBCDeviationPos100.ToString(fl));
						rt = rt.Replace("{dUC-r_max}", archiveFields_.MaxModeUCCADeviationNeg100.ToString(fl));
						rt = rt.Replace("{dUC+r_max}", archiveFields_.MaxModeUCCADeviationPos100.ToString(fl));

						if (protocolType_ == PQPProtocolType.VERSION_2)
						{
							float t2a = archiveFields_.MaxModeUAABDeviationPos100 + archiveFields_.MaxModeUAABDeviationNeg100;
							float t2b = archiveFields_.MaxModeUBBCDeviationPos100 + archiveFields_.MaxModeUBBCDeviationNeg100;
							float t2c = archiveFields_.MaxModeUCCADeviationPos100 + archiveFields_.MaxModeUCCADeviationNeg100;

							if (archiveFields_.MaxModeTenMinuteCounterNonflagged != 0)
							{
								t2a /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2b /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2c /= archiveFields_.MaxModeTenMinuteCounterNonflagged;

								t2a = (float)Math.Round(t2a * 100, settings_.FloatSigns);
								t2b = (float)Math.Round(t2b * 100, settings_.FloatSigns);
								t2c = (float)Math.Round(t2c * 100, settings_.FloatSigns);

								rt = rt.Replace("{dUAT2_max}", t2a.ToString(fl));
								rt = rt.Replace("{dUBT2_max}", t2b.ToString(fl));
								rt = rt.Replace("{dUCT2_max}", t2c.ToString(fl));
							}
						}
						else if (protocolType_ == PQPProtocolType.VERSION_3)
						{
							float t2aPos = archiveFields_.MaxModeUAABDeviationPos100;
							float t2bPos = archiveFields_.MaxModeUBBCDeviationPos100;
							float t2cPos = archiveFields_.MaxModeUCCADeviationPos100;
							float t2aNeg = archiveFields_.MaxModeUAABDeviationNeg100;
							float t2bNeg = archiveFields_.MaxModeUBBCDeviationNeg100;
							float t2cNeg = archiveFields_.MaxModeUCCADeviationNeg100;

							if (archiveFields_.MaxModeTenMinuteCounterNonflagged != 0)
							{
								t2aPos /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2bPos /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2cPos /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2aNeg /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2bNeg /= archiveFields_.MaxModeTenMinuteCounterNonflagged;
								t2cNeg /= archiveFields_.MaxModeTenMinuteCounterNonflagged;

								t2aPos = (float)Math.Round(t2aPos * 100, settings_.FloatSigns);
								t2bPos = (float)Math.Round(t2bPos * 100, settings_.FloatSigns);
								t2cPos = (float)Math.Round(t2cPos * 100, settings_.FloatSigns);
								t2aNeg = (float)Math.Round(t2aNeg * 100, settings_.FloatSigns);
								t2bNeg = (float)Math.Round(t2bNeg * 100, settings_.FloatSigns);
								t2cNeg = (float)Math.Round(t2cNeg * 100, settings_.FloatSigns);

								rt = rt.Replace("{dUA+T2_max}", t2aPos.ToString(fl));
								rt = rt.Replace("{dUB+T2_max}", t2bPos.ToString(fl));
								rt = rt.Replace("{dUC+T2_max}", t2cPos.ToString(fl));
								rt = rt.Replace("{dUA-T2_max}", t2aNeg.ToString(fl));
								rt = rt.Replace("{dUB-T2_max}", t2bNeg.ToString(fl));
								rt = rt.Replace("{dUC-T2_max}", t2cNeg.ToString(fl));
							}
						}
					}

					if (archiveFields_.MinModeExists)
					{
						rt = rt.Replace("{dUA-r_min}", archiveFields_.MinModeUAABDeviationNeg100.ToString(fl));
						rt = rt.Replace("{dUA+r_min}", archiveFields_.MinModeUAABDeviationPos100.ToString(fl));
						rt = rt.Replace("{dUB-r_min}", archiveFields_.MinModeUBBCDeviationNeg100.ToString(fl));
						rt = rt.Replace("{dUB+r_min}", archiveFields_.MinModeUBBCDeviationPos100.ToString(fl));
						rt = rt.Replace("{dUC-r_min}", archiveFields_.MinModeUCCADeviationNeg100.ToString(fl));
						rt = rt.Replace("{dUC+r_min}", archiveFields_.MinModeUCCADeviationPos100.ToString(fl));

						if (protocolType_ == PQPProtocolType.VERSION_2)
						{
							float t2a = archiveFields_.MinModeUAABDeviationPos100 + archiveFields_.MinModeUAABDeviationNeg100;
							float t2b = archiveFields_.MinModeUBBCDeviationPos100 + archiveFields_.MinModeUBBCDeviationNeg100;
							float t2c = archiveFields_.MinModeUCCADeviationPos100 + archiveFields_.MinModeUCCADeviationNeg100;

							if (archiveFields_.MinModeTenMinuteCounterNonflagged != 0)
							{
								t2a /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2b /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2c /= archiveFields_.MinModeTenMinuteCounterNonflagged;

								t2a = (float)Math.Round(t2a * 100, settings_.FloatSigns);
								t2b = (float)Math.Round(t2b * 100, settings_.FloatSigns);
								t2c = (float)Math.Round(t2c * 100, settings_.FloatSigns);

								rt = rt.Replace("{dUAT2_min}", t2a.ToString(fl));
								rt = rt.Replace("{dUBT2_min}", t2b.ToString(fl));
								rt = rt.Replace("{dUCT2_min}", t2c.ToString(fl));
							}
						}
						else if (protocolType_ == PQPProtocolType.VERSION_3)
						{
							float t2aPos = archiveFields_.MinModeUAABDeviationPos100;
							float t2bPos = archiveFields_.MinModeUBBCDeviationPos100;
							float t2cPos = archiveFields_.MinModeUCCADeviationPos100;
							float t2aNeg = archiveFields_.MinModeUAABDeviationNeg100;
							float t2bNeg = archiveFields_.MinModeUBBCDeviationNeg100;
							float t2cNeg = archiveFields_.MinModeUCCADeviationNeg100;

							if (archiveFields_.MinModeTenMinuteCounterNonflagged != 0)
							{
								t2aPos /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2bPos /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2cPos /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2aNeg /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2bNeg /= archiveFields_.MinModeTenMinuteCounterNonflagged;
								t2cNeg /= archiveFields_.MinModeTenMinuteCounterNonflagged;

								t2aPos = (float)Math.Round(t2aPos * 100, settings_.FloatSigns);
								t2bPos = (float)Math.Round(t2bPos * 100, settings_.FloatSigns);
								t2cPos = (float)Math.Round(t2cPos * 100, settings_.FloatSigns);
								t2aNeg = (float)Math.Round(t2aNeg * 100, settings_.FloatSigns);
								t2bNeg = (float)Math.Round(t2bNeg * 100, settings_.FloatSigns);
								t2cNeg = (float)Math.Round(t2cNeg * 100, settings_.FloatSigns);

								rt = rt.Replace("{dUA+T2_min}", t2aPos.ToString(fl));
								rt = rt.Replace("{dUB+T2_min}", t2bPos.ToString(fl));
								rt = rt.Replace("{dUC+T2_min}", t2cPos.ToString(fl));
								rt = rt.Replace("{dUA-T2_min}", t2aNeg.ToString(fl));
								rt = rt.Replace("{dUB-T2_min}", t2bNeg.ToString(fl));
								rt = rt.Replace("{dUC-T2_min}", t2cNeg.ToString(fl));
							}
						}
					}
				}

				#endregion

				// Погрешности dUy
				rt = rt.Replace("{ddUр}", "±0,1% (абс.)");
				rt = rt.Replace("{ddUн}", "±0,1%");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportUDeviation():");
				throw;
			}
		}

		protected void ExportNonsymmetry(ref string rt, string fl)
		{
			try
			{
				#region Коэффициент нессиметрии прямой последовательности

				// k2u
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					if (archiveFields_.BoolTenMinuteStatistics)
					{
						// upper
						rt = rt.Replace("{K2Uвр}", archiveFields_.K2U95.ToString(fl));
						// max
						rt = rt.Replace("{K2Uнбр}", archiveFields_.K2U100.ToString(fl));

						// nominals
						rt = rt.Replace("{K2Uвн}", regInfo_.Constraints.K2u95.ToString(fl));
						rt = rt.Replace("{K2Uнбн}", regInfo_.Constraints.K2u100.ToString(fl));
					}

					ReplaceExcelName(ref rt, "K2UвT", archiveFields_.K2UcounterNonflaggedT1,
										archiveFields_.K2UcounterNonflaggedT2,
										archiveFields_.TenMinuteCounterNonflagged, fl);

					ReplaceExcelName(ref rt, "K2UнбT", archiveFields_.K2UcounterNonflaggedT2,
											archiveFields_.TenMinuteCounterNonflagged, fl);

					// Погрешности k2u
					rt = rt.Replace("{dK2Uр}", "±0,15% (абс.)");
					rt = rt.Replace("{dK2Uн}", "±0,15%");
				}

				#endregion

				#region Коэффициент нессиметрии обратной последовательности

				// k0u
				if (regInfo_.ConnectionScheme == ConnectScheme.Ph3W4)
				{
					if (archiveFields_.BoolTenMinuteStatistics)
					{
						// upper
						rt = rt.Replace("{K0Uвр}", archiveFields_.K0U95.ToString(fl));
						// max
						rt = rt.Replace("{K0Uнбр}", archiveFields_.K0U100.ToString(fl));

						// nominals
						rt = rt.Replace("{K0Uвн}", regInfo_.Constraints.K0u95.ToString(fl));
						rt = rt.Replace("{K0Uнбн}", regInfo_.Constraints.K0u100.ToString(fl));
					}

					ReplaceExcelName(ref rt, "K0UвT", archiveFields_.K0UcounterNonflaggedT1,
										archiveFields_.K0UcounterNonflaggedT2,
										archiveFields_.TenMinuteCounterNonflagged, fl);

					ReplaceExcelName(ref rt, "K0UнбT", archiveFields_.K0UcounterNonflaggedT2,
											archiveFields_.TenMinuteCounterNonflagged, fl);

					// Погрешности k0u
					rt = rt.Replace("{dK0Uр}", "±0,15% (абс.)");
					rt = rt.Replace("{dK0Uн}", "±0,15%");
				}

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportNonsymmetry():");
				throw;
			}
		}

		protected void ExportNonsinus(ref string rt, string fl)
		{
			try
			{
				if (!archiveFields_.BoolTenMinuteStatistics)
				{
					EmService.WriteToLogGeneral("ExportNonsinus: statistics is not valid");
					return;
				}

				rt = rt.Replace("{KUAвр}", archiveFields_.UAABKHarm95[0].ToString(fl));
				rt = rt.Replace("{KUAнбр}", archiveFields_.UAABKHarm100[0].ToString(fl));

				ReplaceExcelName(ref rt, "KUAвT", archiveFields_.UAABKHarmCounterNonflaggedT1[0],
				                 archiveFields_.UAABKHarmCounterNonflaggedT2[0],
				                 archiveFields_.TenMinuteCounterNonflagged, fl);

				ReplaceExcelName(ref rt, "KUAнбT", archiveFields_.UAABKHarmCounterNonflaggedT2[0],
				                 archiveFields_.TenMinuteCounterNonflagged, fl);

				// nominals
				rt = rt.Replace("{KUвн}", regInfo_.Constraints.KHarmTotal95.ToString(fl));
				rt = rt.Replace("{KUнбн}", regInfo_.Constraints.KHarmTotal100.ToString(fl));

				if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					rt = rt.Replace("{KUBвр}", archiveFields_.UBBCKHarm95[0].ToString(fl));
					rt = rt.Replace("{KUBнбр}", archiveFields_.UBBCKHarm100[0].ToString(fl));

					ReplaceExcelName(ref rt, "KUBвT", archiveFields_.UBBCKHarmCounterNonflaggedT1[0],
					                 archiveFields_.UBBCKHarmCounterNonflaggedT2[0],
					                 archiveFields_.TenMinuteCounterNonflagged, fl);

					ReplaceExcelName(ref rt, "KUBнбT", archiveFields_.UBBCKHarmCounterNonflaggedT2[0],
					                 archiveFields_.TenMinuteCounterNonflagged, fl);

					rt = rt.Replace("{KUCвр}", archiveFields_.UCCAKHarm95[0].ToString(fl));
					rt = rt.Replace("{KUCнбр}", archiveFields_.UCCAKHarm100[0].ToString(fl));

					ReplaceExcelName(ref rt, "KUCвT", archiveFields_.UCCAKHarmCounterNonflaggedT1[0],
					                 archiveFields_.UCCAKHarmCounterNonflaggedT2[0],
					                 archiveFields_.TenMinuteCounterNonflagged, fl);

					ReplaceExcelName(ref rt, "KUCнбT", archiveFields_.UCCAKHarmCounterNonflaggedT2[0],
					                 archiveFields_.TenMinuteCounterNonflagged, fl);
				}

				// Погрешности kU (A, B, C)
				//rt = rt.Replace("{dKUр}", "±0,05% (абс.) при <= 1%,\n ±5% (отн.) при > 1%");
				rt = rt.Replace("{dKUн}", "±0,05% или ±5%");

				#region Коэффициэнты гармонических составляющих

				// KuN ( N = 2..40 )
				for (int iHarm = 2; iHarm < 41; iHarm++)
				{
					ReplaceTextForReport(ref rt, "{kua" + iHarm.ToString() + "в}", archiveFields_.UAABKHarm95[iHarm - 1], fl);
					ReplaceTextForReport(ref rt, "{kua" + iHarm.ToString() + "нб}", archiveFields_.UAABKHarm100[iHarm - 1], fl);

					ReplaceExcelNameHarm(ref rt, "kua", iHarm.ToString(), "T1",
										 archiveFields_.UAABKHarmCounterNonflaggedT1[iHarm - 1],
										 archiveFields_.UAABKHarmCounterNonflaggedT2[iHarm - 1], 
										 archiveFields_.TenMinuteCounterNonflagged, fl);
					ReplaceExcelNameHarm(ref rt, "kua", iHarm.ToString(), "T2",
										 archiveFields_.UAABKHarmCounterNonflaggedT2[iHarm - 1],
										 archiveFields_.TenMinuteCounterNonflagged, fl);

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						rt = rt.Replace("{kub" + iHarm.ToString() + "в}", archiveFields_.UBBCKHarm95[iHarm - 1].ToString(fl));
						rt = rt.Replace("{kub" + iHarm.ToString() + "нб}", archiveFields_.UBBCKHarm100[iHarm - 1].ToString(fl));

						ReplaceExcelNameHarm(ref rt, "kub", iHarm.ToString(), "T1",
											archiveFields_.UBBCKHarmCounterNonflaggedT1[iHarm - 1],
											archiveFields_.UBBCKHarmCounterNonflaggedT2[iHarm - 1],
											archiveFields_.TenMinuteCounterNonflagged, fl);
						ReplaceExcelNameHarm(ref rt, "kub", iHarm.ToString(), "T2",
						                    archiveFields_.UBBCKHarmCounterNonflaggedT2[iHarm - 1],
						                    archiveFields_.TenMinuteCounterNonflagged, fl);

						rt = rt.Replace("{kuc" + iHarm.ToString() + "в}", archiveFields_.UCCAKHarm95[iHarm - 1].ToString(fl));
						rt = rt.Replace("{kuc" + iHarm.ToString() + "нб}", archiveFields_.UCCAKHarm100[iHarm - 1].ToString(fl));

						ReplaceExcelNameHarm(ref rt, "kuc", iHarm.ToString(), "T1",
											archiveFields_.UCCAKHarmCounterNonflaggedT1[iHarm - 1],
											archiveFields_.UCCAKHarmCounterNonflaggedT2[iHarm - 1],
											archiveFields_.TenMinuteCounterNonflagged, fl);
						ReplaceExcelNameHarm(ref rt, "kuc", iHarm.ToString(), "T2",
											archiveFields_.UCCAKHarmCounterNonflaggedT2[iHarm - 1],
											archiveFields_.TenMinuteCounterNonflagged, fl);
					}

					rt = rt.Replace("{ku" + iHarm.ToString() + "вн}",
					                regInfo_.Constraints.KHarm95[iHarm - 1].ToString(fl));
					rt = rt.Replace("{ku" + iHarm.ToString() + "нбн}",
					                regInfo_.Constraints.KHarm100[iHarm - 1].ToString(fl));
				}
				// Погрешности KuN ( N = 2..40 )
				//rt = rt.Replace("{dKUNр}", "±0,05% (абс.) при <= 1%,\n ±5% (отн.) при > 1%");
				rt = rt.Replace("{dKUNн}", "±0,05% или ±5%");

				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportNonsinus():");
				throw;
			}
		}

		protected void ExportDipAndSwell_OLD(ref string rt, string fl)
		{
			try
			{
				//GridColumnStylesCollection stDip = dgDips_.TableStyles[0].GridColumnStyles;
				//GridColumnStylesCollection stSwell = dgOvers_.TableStyles[0].GridColumnStyles;

				//// провалы
				//// number of rows in the dip table
				//int rows = (dgDips_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

				//// перебираем все номера
				//int ival = 0;
				//for (int i = 0; i < rows; i++)
				//{
				//	string row_name = (dgDips_[i, 0] as string).TrimEnd();

				//	if (row_name.Contains("90") && row_name.Contains("85")) row_name = "{dip90_85_";
				//	else if (row_name.Contains("85") && row_name.Contains("70")) row_name = "{dip85_70_";
				//	else if (row_name.Contains("70") && row_name.Contains("40")) row_name = "{dip70_40_";
				//	else if (row_name.Contains("40") && row_name.Contains("10")) row_name = "{dip40_10_";
				//	else if (row_name.Contains("10") && row_name.Contains("5")) row_name = "{dip10_5_";
				//	else if (row_name.Contains("5") && row_name.Contains("0")) row_name = "{dip5_0_";

				//	int temp = stDip.IndexOf(stDip["num_0_01_till_0_05"]);
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_01_till_0_05"])]);
				//	rt = rt.Replace(row_name + "1}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_05_till_0_1"])]);
				//	rt = rt.Replace(row_name + "2}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_1_till_0_5"])]);
				//	rt = rt.Replace(row_name + "3}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_0_5_till_1"])]);
				//	rt = rt.Replace(row_name + "4}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_1_till_3"])]);
				//	rt = rt.Replace(row_name + "5}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_3_till_20"])]);
				//	rt = rt.Replace(row_name + "6}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_20_till_60"])]);
				//	rt = rt.Replace(row_name + "7}", ival.ToString());
				//	ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_over_60"])]);
				//	rt = rt.Replace(row_name + "8}", ival.ToString());

				//	try
				//	{
				//		ival = Convert.ToInt32(dgDips_[i, stDip.IndexOf(stDip["num_over_180"])]);
				//		rt = rt.Replace(row_name + "9}", ival.ToString());
				//	}
				//	catch { rt = rt.Replace(row_name + "9}", "-"); }
				//}

				//// swells
				//// number of rows in the swell table
				//rows = (dgOvers_.DataSource as System.Data.DataSet).Tables[0].Rows.Count;

				//// перебираем все номера
				//for (int i = 0; i < rows; i++)
				//{
				//	string row_name = (dgOvers_[i, 0] as string).TrimEnd();

				//	if (row_name.Contains("110") && row_name.Contains("112")) row_name = "{over110_112_";
				//	else if (row_name.Contains("112") && row_name.Contains("115")) row_name = "{over112_115_";
				//	else if (row_name.Contains("115") && row_name.Contains("120")) row_name = "{over115_120_";
				//	else if (row_name.Contains("120") && row_name.Contains("150")) row_name = "{over120_150_";

				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_01_till_0_05"])]);
				//	rt = rt.Replace(row_name + "1}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_05_till_0_1"])]);
				//	rt = rt.Replace(row_name + "2}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_1_till_0_5"])]);
				//	rt = rt.Replace(row_name + "3}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_0_5_till_1"])]);
				//	rt = rt.Replace(row_name + "4}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_1_till_3"])]);
				//	rt = rt.Replace(row_name + "5}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_3_till_20"])]);
				//	rt = rt.Replace(row_name + "6}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_20_till_60"])]);
				//	rt = rt.Replace(row_name + "7}", ival.ToString());
				//	ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_over_60"])]);
				//	rt = rt.Replace(row_name + "8}", ival.ToString());

				//	try
				//	{
				//		ival = Convert.ToInt32(dgOvers_[i, stSwell.IndexOf(stSwell["num_over_180"])]);
				//		rt = rt.Replace(row_name + "9}", ival.ToString());
				//	}
				//	catch { rt = rt.Replace(row_name + "9}", "-"); }
				//}
			}
			catch (Exception ex)
            {
				EmService.DumpException(ex, "Error in ExportDipAndSwell():");
                throw;
            }
		}

		protected void ExportDipAndSwell_GOST2014(ref string rt, string fl)
		{
			try
			{
				string[] valueNames = new string[] {"{dip90_85_", "{dip85_70_", "{dip70_40_", "{dip40_10_", "{dip10_5_"};
				// dips
				for (int iVal = 0; iVal < valueNames.Length; iVal++)
				{
					rt = rt.Replace(valueNames[iVal] + "1}", archiveFields_.DipSwell[iVal + 4, 0].ToString());
					rt = rt.Replace(valueNames[iVal] + "2}", archiveFields_.DipSwell[iVal + 4, 1].ToString());
					rt = rt.Replace(valueNames[iVal] + "3}", archiveFields_.DipSwell[iVal + 4, 2].ToString());
					rt = rt.Replace(valueNames[iVal] + "4}", archiveFields_.DipSwell[iVal + 4, 3].ToString());
					rt = rt.Replace(valueNames[iVal] + "5}", archiveFields_.DipSwell[iVal + 4, 4].ToString());
					rt = rt.Replace(valueNames[iVal] + "6}", archiveFields_.DipSwell[iVal + 4, 5].ToString());
				}

				// interrupts
				string valueName = "{dip5_0_";
				rt = rt.Replace(valueName + "1}", archiveFields_.Interrupt[0].ToString());
				rt = rt.Replace(valueName + "2}", archiveFields_.Interrupt[1].ToString());
				rt = rt.Replace(valueName + "3}", archiveFields_.Interrupt[2].ToString());
				rt = rt.Replace(valueName + "4}", archiveFields_.Interrupt[3].ToString());
				rt = rt.Replace(valueName + "5}", archiveFields_.Interrupt[4].ToString());
				rt = rt.Replace(valueName + "6}", archiveFields_.Interrupt[5].ToString());
				rt = rt.Replace(valueName + "7}", archiveFields_.Interrupt[6].ToString());
				rt = rt.Replace("{dip_max_len}", archiveFields_.InterruptionMaxDuration.ToString());

				// swells
				valueNames = new string[] { "{over110_120_", "{over120_140_", "{over140_160_", "{over160_180_" };
				for (int iVal = 0; iVal < valueNames.Length; iVal++)
				{
					rt = rt.Replace(valueNames[iVal] + "1}", archiveFields_.DipSwell[iVal, 0].ToString());
					rt = rt.Replace(valueNames[iVal] + "2}", archiveFields_.DipSwell[iVal, 1].ToString());
					rt = rt.Replace(valueNames[iVal] + "3}", archiveFields_.DipSwell[iVal, 2].ToString());
					rt = rt.Replace(valueNames[iVal] + "4}", archiveFields_.DipSwell[iVal, 3].ToString());
					rt = rt.Replace(valueNames[iVal] + "5}", archiveFields_.DipSwell[iVal, 4].ToString());
					rt = rt.Replace(valueNames[iVal] + "6}", archiveFields_.DipSwell[iVal, 5].ToString());
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportDipAndSwell():");
				throw;
			}
		}

		protected void ExportFlicker(ref string rt, string fl)
		{
			try
			{
				if (regInfo_.ConnectionScheme != ConnectScheme.Ph3W3 && 
					regInfo_.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					// short
					// phase A
					rt = rt.Replace("{f_st_a}", archiveFields_.UAABFlickerPst100.ToString(fl));

					// nominal
					rt = rt.Replace("{f_st_n}", regInfo_.Constraints.FlickShortUp100.ToString(fl));

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						rt = rt.Replace("{f_st_b}", archiveFields_.UBBCFlickerPst100.ToString(fl));
						rt = rt.Replace("{f_st_c}", archiveFields_.UCCAFlickerPst100.ToString(fl));
					}

					// long
					// phase A
					rt = rt.Replace("{f_lt_a}", archiveFields_.UAABFlickerPlt100.ToString(fl));

					// nominal
					rt = rt.Replace("{f_lt_n}", regInfo_.Constraints.FlickLongUp100.ToString(fl));

					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						rt = rt.Replace("{f_lt_b}", archiveFields_.UBBCFlickerPlt100.ToString(fl));
						rt = rt.Replace("{f_lt_c}", archiveFields_.UCCAFlickerPlt100.ToString(fl));
					}

					// Погрешности
					//rt = rt.Replace("{dFst_r}", "±5,0 (отн.)");
					rt = rt.Replace("{dFst_n}", "±5,0");
					//rt = rt.Replace("{dFlt_r}", "±5,0 (отн.)");
					rt = rt.Replace("{dFlt_n}", "±5,0");
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportFlicker():");
				throw;
			}
		}

		protected void ExportInterharm(ref string rt, string fl)
		{
			try
			{
				for (int iInter = 0; iInter <= 40; iInter++)
				{
					ReplaceTextForReport(ref rt, "{IsgA" + iInter.ToString() + "}", 
										archiveFields_.UAABInterharm[iInter], fl);
					if (regInfo_.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						ReplaceTextForReport(ref rt, "{IsgB" + iInter.ToString() + "}",
						                     archiveFields_.UBBCInterharm[iInter], fl);
						ReplaceTextForReport(ref rt, "{IsgC" + iInter.ToString() + "}",
						                     archiveFields_.UCCAInterharm[iInter], fl);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExportInterharm():");
				throw;
			}
		}

		// T1
        protected void ReplaceExcelNameHarm(ref string rt, 
							string excelNameStart, string excelNameNumber, string excelNameEnd,
							float value1, float value2, float cntAll, string fl)
        {
			try
			{
				float res = 0;
				if (cntAll != 0)
					res = (value1 + value2) / cntAll * 100.0F;

				CheckForPaintExcelValueHarm(ref rt, excelNameStart, excelNameNumber, excelNameEnd, value1);

				ReplaceTextForReport(ref rt, "{" + excelNameStart + excelNameNumber + excelNameEnd + "}", res, fl);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReplaceExcelNameHarm():");
				throw;
			}
        }

		//T2
		protected void ReplaceExcelNameHarm(ref string rt,
							string excelNameStart, string excelNameNumber, string excelNameEnd,
							float value, float cntAll, string fl)
		{
			try
			{
				float res = 0;
				if (cntAll != 0) res = value / cntAll * 100.0F;

				CheckForPaintExcelValueHarm(ref rt,
					excelNameStart, excelNameNumber, excelNameEnd,
					value);

				ReplaceTextForReport(ref rt, "{" + excelNameStart + excelNameNumber + excelNameEnd + "}", res, fl);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReplaceExcelNameHarm():");
				throw;
			}
		}

		// T1
        protected void ReplaceExcelName(ref string rt, string excelName,
										float value1, float value2, ushort valueCount,
										string fl)
        {
            try
            {
				float res = 0;
				if (valueCount != 0) res = (value1 + value2) / valueCount * 100.0F;

                CheckForPaintExcelValue(ref rt, excelName, res);

                ReplaceTextForReport(ref rt, "{" + excelName + "}", res, fl);
            }
            catch (Exception ex)
            {
				EmService.DumpException(ex, "Exception in ReplaceExcelName():");
                throw;
            }
        }

		// T2
		protected void ReplaceExcelName(ref string rt, string excelName,
										float value, ushort valueCount,
										string fl)
		{
			try
			{
				float res = 0;
				if (valueCount != 0) res = value / valueCount * 100.0F;

				CheckForPaintExcelValue(ref rt, excelName, res);

				ReplaceTextForReport(ref rt, "{" + excelName + "}", res, fl);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in ReplaceExcelName():");
				throw;
			}
		}

		//protected void ReplaceExcelNameNonSymm(ref string rt, DataGrid datagrid, string ExcelName,
		//    string GridName1, string GridName2,
		//    int i, string fl, GridColumnStylesCollection st)
		//{
		//    try
		//    {
		//        float value;
		//        Conversions.object_2_float_en_ru(datagrid[i, st.IndexOf(st[GridName1])], out value);
		//        int value2 = (int)datagrid[i, st.IndexOf(st[GridName2])];
		//        float res = 0;
		//        if (value2 != 0) res = value / value2 * 100;

		//        CheckForPaintExcelValue(ref rt, ExcelName, res);

		//        //rt = rt.Replace("{" + ExcelName + "}", res.ToString(fl));
		//        ReplaceTextForReport(ref rt, "{" + ExcelName + "}", res, fl);
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Exception in ReplaceExcelNameNonSymm():");
		//        throw;
		//    }
		//}

        protected override void CheckForPaintExcelValue(ref string rt, string ExcelName, float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelName)
                {
                    case "dU{0}T1": limit = 5; break;
                    case "dU{0}T2": limit = 0; break;
                }

                if ((limit != -1) && (value > limit))
                {
                    if (StyleName_.Length == 0)
                    {
                        InsertPaintExcelStyle(ref rt);
                    }
                    PaintExcelValue(ref rt, "{" + ExcelName + "}");
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in CheckForPaintExcelValueEtPqpA():");
                throw;
            }
        }

        protected void CheckForPaintExcelValue(ref string rt, string ExcelName,
             string phase, float value)
        {
            try
            {
                float limit = -1;
                switch (ExcelName)
                {
                    case "dU{0}T1": limit = 5; break;
                    case "dU{0}T2": limit = 0; break;
                }

                if ((limit != -1) && (value > limit))
                {
                    if (StyleName_.Length == 0)
                    {
                        InsertPaintExcelStyle(ref rt);
                    }

                    PaintExcelValue(ref rt, "{" + string.Format(ExcelName, phase) + "}");
                }
            }
            catch (Exception ex)
            {
                EmService.DumpException(ex, "Error in CheckForPaintExcelValueEtPqpA():");
                throw;
            }
        }
    }
}
