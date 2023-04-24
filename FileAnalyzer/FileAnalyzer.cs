using System;
using System.Collections.Generic;
using System.IO;

using EmServiceLib;

namespace FileAnalyzerLib
{
	/// <summary>
	/// The class is responsible for reading and analyzing archive files
	/// </summary>
	public class FileAnalyzer
	{
		#region PQP

		public static bool GetPqpArchiveFields(string path, out PqpArchiveFields pqpArchive,
												ref RegistrationInfo regInfo)
		{
			BinaryReader binReader = null;
			try
			{
				pqpArchive = new PqpArchiveFields();
				//RegistrationInfo regInfo = null;
				binReader = new BinaryReader(File.Open(path, FileMode.Open));
				byte[] buffer = null;

				FileInfo fileInfo = new FileInfo(path);
				char curChar = Char.MinValue;
				string sHeaderLen = string.Empty;
				while (curChar != '|')
				{
					curChar = (char)binReader.ReadByte();
					if (Char.IsDigit(curChar)) sHeaderLen += curChar;
				}
				int headerLen = Int32.Parse(sHeaderLen);

				if (fileInfo.Length < (headerLen + EmService.PQP_SEGMENT_LENGTH))
				{
					EmService.WriteToLogFailed("GetPqpArchiveFields: fileInfo.Length <");
					return false;
				}

				//buffer = binReader.ReadBytes(headerLen);
				//int serialNumber = GetNumberFromBuffer(ref buffer, ref shift, '|');
				//int regId = GetNumberFromBuffer(ref buffer, ref shift, '|');

				//if (serialNumber == -1 || regId == -1)
				//{
				//    EmService.WriteToLogFailed("GetPqpArchiveFields: -1");
				//    return false;
				//}

				//// registration data
				//int regDataLen = GetNumberFromBuffer(ref buffer, ref shift, '|');
				//byte[] regDataBytes = new byte[regDataLen];
				//Array.Copy(buffer, shift, regDataBytes, 0, regDataLen);

				//shift += (regDataLen + 1 /*separator*/);

				int shift = headerLen + DigitCount(headerLen) + 1 /*separator*/;

				binReader.BaseStream.Seek(shift, SeekOrigin.Begin);
				buffer = binReader.ReadBytes((int)fileInfo.Length - shift);

				if (buffer.Length < EmService.PQP_SEGMENT_LENGTH)
				{
					EmService.WriteToLogFailed("GetPqpArchiveFields: fileInfo.Length <<");
					return false;
				}

				pqpArchive.DtPqpStart =
					Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 6 * 2,
															  "PQP Start date");
				pqpArchive.DtPqpEnd =
					Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 42 * 2,
															  "PQP End date");

				pqpArchive.BoolTenMinuteStatistics = Conversions.bytes_2_ushort(ref buffer, 484002) != 0;
				if (!pqpArchive.BoolTenMinuteStatistics) 
					EmService.WriteToLogGeneral("GetPqpArchiveFields: statisticValid = 0");

				if (!GetPqpVoltageDeviation(ref buffer, ref pqpArchive))
					return false;
				if (!GetPqpFrequency(ref buffer, ref pqpArchive))
					return false;
				if (!GetPqpFreqVoltageValues(ref buffer, regInfo.ConnectionScheme, ref pqpArchive))
					return false;

				if (!GetPqpNonsymmetry(ref buffer, ref pqpArchive))
					return false;

				if (!GetPqpNonsinus(ref buffer, ref pqpArchive))
					return false;

				if (!GetPqpFlicker(ref buffer, ref pqpArchive))
					return false;
				if (!GetPqpFlickerValues(ref buffer, regInfo.ConnectionScheme, ref pqpArchive))
					return false;

				if (!GetPqpDipSwell(ref buffer, ref pqpArchive, 
									Constants.AnalyseDeviceVersion(regInfo.DeviceVersion)))
					return false;

				if (!GetPqpVoltageInterharm(ref buffer, ref pqpArchive, 
									regInfo.UtransformerEnable, regInfo.UtransformerType))
					return false;

				//int pqpArchiveId = GetNumberFromBuffer(ref buffer, ref shift, '|');
				//DateTime dtArchiveStart =
				//    Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 6 * 2 + shift,
				//                                              "PQP Start date");
				//DateTime dtArchiveEnd =
				//    Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 42 * 2 + shift,
				//                                              "PQP End date");

				return true;
			}
			catch (IOException ex)
			{
				EmService.DumpException(ex, "IOException in GetPqpArchiveFields");
				pqpArchive = null;
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetPqpArchiveFields");
				throw;
			}
			finally
			{
				if (binReader != null)
				{
					binReader.Close();
					binReader.Dispose();
				}
			}
		}

		private static bool GetPqpFrequency(ref byte[] buffer, ref PqpArchiveFields pqpArchive)
		{
			try
			{
				pqpArchive.BoolFreqDeviationStatistics = Conversions.bytes_2_ushort(ref buffer, 138) != 0;
				if (!pqpArchive.BoolFreqDeviationStatistics)
				{
					EmService.WriteToLogGeneral("GetPqpFrequency: statisticValid = 0");
					return true;  // there's no errors but statistics wasn't calculated
				}

				// ГОСТы из уставок
				//float f95_u_syn = constraints[2];	// Верхнее откл-е частоты для синхр. электросети (95%)
				//float f95_d_syn = constraints[0];	// Нижнее откл-е частоты для синхр. электросети (95%)
				//float f100_u_syn = constraints[3];	// Верхнее откл-е частоты для синхр. электросети (100%)
				//float f100_d_syn = constraints[1];	// Нижнее откл-е частоты для синхр. электросети (100%)
				//float f95_u_iso = constraints[6];	// Верхнее откл-е частоты для изолир. электросети (95%)
				//float f95_d_iso = constraints[4];	// Нижнее откл-е частоты для изолир. электросети (95%)
				//float f100_u_iso = constraints[7];	// Верхнее откл-е частоты для изолир. электросети (100%)
				//float f100_d_iso = constraints[5];	// Нижнее откл-е частоты для изолир. электросети (100%)

				// общее кол-во отсчетов
				pqpArchive.FreqDeviationCounterTotal = Conversions.bytes_2_ushort(ref buffer, 128);
				// кол-во отсчетов, синхронизированное с сетью
				pqpArchive.FreqDeviationCounterLocked = Conversions.bytes_2_ushort(ref buffer, 130);
				// кол-во отсчетов, не синхронизированное с сетью
				pqpArchive.FreqDeviationCounterNonlocked = Conversions.bytes_2_ushort(ref buffer, 136);
				// отсчетов между ПДП и НДП
				pqpArchive.FreqDeviationCounterLockedT1 = Conversions.bytes_2_ushort(ref buffer, 132);
				//отсчетов за ПДП
				pqpArchive.FreqDeviationCounterLockedT2 = Conversions.bytes_2_ushort(ref buffer, 134);
				// отсчетов в НДП
				pqpArchive.FreqDeviationCounterLockedNormal =
					(ushort)(pqpArchive.FreqDeviationCounterTotal - pqpArchive.FreqDeviationCounterLockedT1 - pqpArchive.FreqDeviationCounterLockedT2);

				// наибольшее и наименьшее значения
				pqpArchive.FreqDeviationUp100 = 
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 152), 3);
				pqpArchive.FreqDeviationDown100 = 
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 144), 3);

				// 95 %
				pqpArchive.FreqDeviationDown95 = 
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 140), 3);
				pqpArchive.FreqDeviationUp95 = 
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 148), 3);

				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in GetPqpFrequency():");
				return false;
			}
		}

		private static bool GetPqpFreqVoltageValues(ref byte[] buffer, ConnectScheme connectScheme, 
													ref PqpArchiveFields pqpArchive)
		{
			try
			{
				EmService.WriteToLogGeneral("GetPqpFreqVoltageValues: num_f = " + 
					pqpArchive.FreqDeviationCounterTotal);
				int shiftF = 242076;
				int shiftFSeconds = 156;

				//float[] df = new float[num_f];
				//int[] f_seconds = new int[num_f];	
				for (int iArr = 0; iArr < pqpArchive.FreqDeviationCounterTotal; ++iArr) 
					pqpArchive.FreqDeviation[iArr] = -1;

				// считываем значение из буфера
				for (int iF = 0; iF < pqpArchive.FreqDeviationCounterTotal; ++iF)
				{
					pqpArchive.FreqDeviationSeconds[iF] = 
						Conversions.bytes_2_int(ref buffer, shiftFSeconds);

					pqpArchive.FreqDeviation[iF] = 
						Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftF);
					if (Conversions.bytes_2_uint(ref buffer, shiftF) == 0x7FFFFFFF)	// not valid
					{
						pqpArchive.FreqDeviation[iF] = -1;
						pqpArchive.FreqValuesValid[iF] = false;
					}
					else     // valid
					{
						pqpArchive.FreqDeviation[iF] = (float) Math.Round(pqpArchive.FreqDeviation[iF], 6);
						pqpArchive.FreqValuesValid[iF] = true;
					}

					if (Single.IsInfinity(pqpArchive.FreqDeviation[iF])) 
						pqpArchive.FreqDeviation[iF] = -1;

					shiftF += 4;	// 4 is length of one record
					shiftFSeconds += 4;
				}

				/////////////////////////////
				// voltage
				/////////////////////////////

				int shiftMarked = 488036;
				int shiftU = 489116;
				int shiftRecSeconds = 484004;

				for (int iArr = 0; iArr < pqpArchive.TenMinuteCounterTotal; ++iArr)
				{
					pqpArchive.UAABDeviationPos[iArr] = -1;
					pqpArchive.UAABDeviationNeg[iArr] = -1;
					pqpArchive.UBBCDeviationPos[iArr] = -1;
					pqpArchive.UBBCDeviationNeg[iArr] = -1;
					pqpArchive.UCCADeviationPos[iArr] = -1;
					pqpArchive.UCCADeviationNeg[iArr] = -1;
					pqpArchive.TenMinuteNotMarked[iArr] = false;
					pqpArchive.TenMinuteSeconds[iArr] = 0;
				}

				// считываем значение из буфера
				for (int iU = 0; iU < pqpArchive.TenMinuteCounterTotal; ++iU)
				{
					pqpArchive.TenMinuteNotMarked[iU] = buffer[shiftMarked] == 0;
					pqpArchive.TenMinuteSeconds[iU] = Conversions.bytes_2_int(ref buffer, shiftRecSeconds);

					if (pqpArchive.TenMinuteNotMarked[iU])
					{
						pqpArchive.UAABDeviationPos[iU] = 
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU);
						pqpArchive.UAABDeviationPos[iU] = (float)Math.Round(pqpArchive.UAABDeviationPos[iU], 4);
						pqpArchive.UAABDeviationNeg[iU] = 
							Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 12096);
						pqpArchive.UAABDeviationNeg[iU] = (float)Math.Round(pqpArchive.UAABDeviationNeg[iU], 4);
					}

					if (connectScheme != ConnectScheme.Ph1W2)
					{
						if (pqpArchive.TenMinuteNotMarked[iU])
						{
							pqpArchive.UBBCDeviationPos[iU] = 
								Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 4032);
							pqpArchive.UBBCDeviationPos[iU] = (float)Math.Round(pqpArchive.UBBCDeviationPos[iU], 4);
							pqpArchive.UCCADeviationPos[iU] = 
								Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 8064);
							pqpArchive.UCCADeviationPos[iU] = (float)Math.Round(pqpArchive.UCCADeviationPos[iU], 4);
							pqpArchive.UBBCDeviationNeg[iU] = 
								Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 16128);
							pqpArchive.UBBCDeviationNeg[iU] = (float)Math.Round(pqpArchive.UBBCDeviationNeg[iU], 4);
							pqpArchive.UCCADeviationNeg[iU] = 
								Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shiftU + 20160);
							pqpArchive.UCCADeviationNeg[iU] = (float)Math.Round(pqpArchive.UCCADeviationNeg[iU], 4);
						}
					}

					if (Single.IsInfinity(pqpArchive.UAABDeviationPos[iU]))
						pqpArchive.UAABDeviationPos[iU] = -1;
					if (Single.IsInfinity(pqpArchive.UAABDeviationNeg[iU]))
						pqpArchive.UAABDeviationNeg[iU] = -1;
					if (Single.IsInfinity(pqpArchive.UBBCDeviationPos[iU]))
						pqpArchive.UBBCDeviationPos[iU] = -1;
					if (Single.IsInfinity(pqpArchive.UBBCDeviationNeg[iU]))
						pqpArchive.UBBCDeviationNeg[iU] = -1;
					if (Single.IsInfinity(pqpArchive.UCCADeviationPos[iU]))
						pqpArchive.UCCADeviationPos[iU] = -1;
					if (Single.IsInfinity(pqpArchive.UCCADeviationNeg[iU]))
						pqpArchive.UCCADeviationNeg[iU] = -1;

					shiftU += 4;	// 4 is length of one record
					shiftMarked += 1;
					shiftRecSeconds += 4;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpFreqVoltageValues():");
				return false;
			}
		}

		private static bool GetPqpVoltageDeviation(ref byte[] buffer, ref PqpArchiveFields pqpArchive)
		{
			try
			{
				// ГОСТы из уставок
				//float f95_u = constraints[8];	// положительное отклонение напряжения (95%)
				//float f100_u = constraints[9];	// положительное отклонение напряжения (100%)
				//float f95_d = constraints[10];	// отрицательное отклонение напряжения (95%)
				//float f100_d = constraints[11];	// отрицательное отклонение напряжения (100%)

				// общее кол-во отсчетов
				pqpArchive.TenMinuteCounterTotal = Conversions.bytes_2_ushort(ref buffer, 483996);
				// кол-во не маркированных отсчетов
				pqpArchive.TenMinuteCounterNonflagged = Conversions.bytes_2_ushort(ref buffer, 483998);
				// кол-во маркированных отсчетов
				pqpArchive.TenMinuteCounterFlagged = Conversions.bytes_2_ushort(ref buffer, 484000);

				// отсчетов между ПДП и НДП
				pqpArchive.UAABDeviationPosNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 489044);
				// отсчетов за ПДП
				pqpArchive.UAABDeviationPosNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 489046);
				// отсчетов в НДП
				pqpArchive.UAABDeviationPosNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
				     pqpArchive.UAABDeviationPosNonflaggedT1 - pqpArchive.UAABDeviationPosNonflaggedT2);
				// наибольшее отклонение
				pqpArchive.UAABDeviationPos100 = 
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489052), 4);
				// верхнее отклонение
				pqpArchive.UAABDeviationPos95 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489048), 4);


				pqpArchive.UBBCDeviationPosNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 489056);
				pqpArchive.UBBCDeviationPosNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 489058);
				pqpArchive.UBBCDeviationPosNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					pqpArchive.UBBCDeviationPosNonflaggedT1 - pqpArchive.UBBCDeviationPosNonflaggedT2);
				pqpArchive.UBBCDeviationPos100 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489064), 4);
				pqpArchive.UBBCDeviationPos95 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489060), 4);


				pqpArchive.UCCADeviationPosNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 489068);
				pqpArchive.UCCADeviationPosNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 489070);
				pqpArchive.UCCADeviationPosNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					pqpArchive.UCCADeviationPosNonflaggedT1 - pqpArchive.UCCADeviationPosNonflaggedT2);
				pqpArchive.UCCADeviationPos100 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489076), 4);
				pqpArchive.UCCADeviationPos95 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489072), 4);


				pqpArchive.UAABDeviationNegNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 489080);
				pqpArchive.UAABDeviationNegNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 489082);
				pqpArchive.UAABDeviationNegNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					 pqpArchive.UAABDeviationNegNonflaggedT1 - pqpArchive.UAABDeviationNegNonflaggedT2);
				pqpArchive.UAABDeviationNeg100 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489088), 4);
				pqpArchive.UAABDeviationNeg95 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489084), 4);


				pqpArchive.UBBCDeviationNegNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 489092);
				pqpArchive.UBBCDeviationNegNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 489094);
				pqpArchive.UBBCDeviationNegNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					 pqpArchive.UBBCDeviationNegNonflaggedT1 - pqpArchive.UBBCDeviationNegNonflaggedT2);
				pqpArchive.UBBCDeviationNeg100 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489100), 4);
				pqpArchive.UBBCDeviationNeg95 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489096), 4);


				pqpArchive.UCCADeviationNegNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 489104);
				pqpArchive.UCCADeviationNegNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 489106);
				pqpArchive.UCCADeviationNegNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					 pqpArchive.UCCADeviationNegNonflaggedT1 - pqpArchive.UCCADeviationNegNonflaggedT2);
				pqpArchive.UCCADeviationNeg100 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489112), 4);
				pqpArchive.UCCADeviationNeg95 =
					(float)Math.Round(Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 489108), 4);
				
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in GetVoltageDeviation():");
				return false;
			}
		}

		private static bool GetPqpNonsymmetry(ref byte[] buffer, ref PqpArchiveFields pqpArchive)
		{
			try
			{
				// ГОСТы из уставок
				//float f95_u_k2u = constraints[96];
				//float f100_u_k2u = constraints[97];
				//float f95_u_k0u = constraints[98];
				//float f100_u_k0u = constraints[99];

				// отсчетов между ПДП и НДП
				pqpArchive.K2UcounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514748);
				// отсчетов за ПДП
				pqpArchive.K2UcounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514750);
				// отсчетов в НДП
				pqpArchive.K2UcounterNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged
					- pqpArchive.K2UcounterNonflaggedT1 - pqpArchive.K2UcounterNonflaggedT2);
				
				pqpArchive.K0UcounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514760);
				pqpArchive.K0UcounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514762);
				pqpArchive.K0UcounterNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged
					- pqpArchive.K0UcounterNonflaggedT1 - pqpArchive.K0UcounterNonflaggedT2);

				pqpArchive.K2U95 = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514752);
				pqpArchive.K2U100 = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514756);
				pqpArchive.K0U95 = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514764);
				pqpArchive.K0U100 = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, 514768);

				// округляем, а то не вставится в БД
				//f_up_k2u = (float)Math.Round(f_up_k2u, 15);
				//f_up_k0u = (float)Math.Round(f_up_k0u, 15);
				//f_max_k2u = (float)Math.Round(f_max_k2u, 15);
				//f_max_k0u = (float)Math.Round(f_max_k0u, 15);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpNonsymmetry():");
				return false;
			}
		}

		private static bool GetPqpNonsinus(ref byte[] buffer, ref PqpArchiveFields pqpArchive)
		{
			try
			{
				int shift_int = 0;
				int shift_float = 0;
				//int shift_constraints = 16;

				float f_95_a_total = 0.0F, f_max_a_total = 0.0F;
				float f_95_b_total = 0.0F, f_max_b_total = 0.0F;
				float f_95_c_total = 0.0F, f_max_c_total = 0.0F;

				for (int iKu = 0; iKu < 40; ++iKu)
				{
					// ГОСТы из уставок
					//float fNDP = 0;
					//float fPDP = 0;
					//if (iKu == 0)
					//{
					//    fNDP = constraints[94];
					//    fPDP = constraints[95];
					//}
					//else
					//{
					//    fNDP = constraints[shift_constraints];
					//    fPDP = constraints[shift_constraints + 39];
					//    ++shift_constraints;
					//}

					// отсчеты прибора
					// отсчетов между ПДП и НДП
					pqpArchive.UAABKHarmCounterNonflaggedT1[iKu] = Conversions.bytes_2_ushort(ref buffer, shift_int + 513308);
					// за ПДП
					pqpArchive.UAABKHarmCounterNonflaggedT2[iKu] = Conversions.bytes_2_ushort(ref buffer, shift_int + 513388);
					// в НДП
					pqpArchive.UAABKHarmCounterNonflaggedNormal[iKu] = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
						pqpArchive.UAABKHarmCounterNonflaggedT1[iKu] - pqpArchive.UAABKHarmCounterNonflaggedT2[iKu]);

					pqpArchive.UBBCKHarmCounterNonflaggedT1[iKu] = Conversions.bytes_2_ushort(ref buffer, shift_int + 513788);
					pqpArchive.UBBCKHarmCounterNonflaggedT2[iKu] = Conversions.bytes_2_ushort(ref buffer, shift_int + 513868);
					pqpArchive.UBBCKHarmCounterNonflaggedNormal[iKu] = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
						pqpArchive.UBBCKHarmCounterNonflaggedT1[iKu] - pqpArchive.UBBCKHarmCounterNonflaggedT2[iKu]);

					pqpArchive.UCCAKHarmCounterNonflaggedT1[iKu] = Conversions.bytes_2_ushort(ref buffer, shift_int + 514268);
					pqpArchive.UCCAKHarmCounterNonflaggedT2[iKu] = Conversions.bytes_2_ushort(ref buffer, shift_int + 514348);
					pqpArchive.UCCAKHarmCounterNonflaggedNormal[iKu] = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
						pqpArchive.UCCAKHarmCounterNonflaggedT1[iKu] - pqpArchive.UCCAKHarmCounterNonflaggedT2[iKu]);

					// верхнее значение (95%) и наибольшее значение (max)
					pqpArchive.UAABKHarm95[iKu] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 513468);
					pqpArchive.UAABKHarm100[iKu] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 513628);

					pqpArchive.UBBCKHarm95[iKu] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 513948);
					pqpArchive.UBBCKHarm100[iKu] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 514108);

					pqpArchive.UCCAKHarm95[iKu] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 514428);
					pqpArchive.UCCAKHarm100[iKu] = Conversions.bytes_2_signed_float_Q_15_16_new(ref buffer, shift_float + 514588);

					if (iKu == 0)       // сохраняем суммарные значения
					{
						f_95_a_total = pqpArchive.UAABKHarm95[iKu];
						f_max_a_total = pqpArchive.UAABKHarm100[iKu];
						f_95_b_total = pqpArchive.UBBCKHarm95[iKu];
						f_max_b_total = pqpArchive.UBBCKHarm100[iKu];
						f_95_c_total = pqpArchive.UCCAKHarm95[iKu];
						f_max_c_total = pqpArchive.UCCAKHarm100[iKu];
					}
					else   // исправляем ошибку в прошивке прибора - если значение зашкаливает, ставим 0
					{
						if (pqpArchive.UAABKHarm95[iKu] > f_95_a_total) pqpArchive.UAABKHarm95[iKu] = 0.0F;
						if (pqpArchive.UAABKHarm100[iKu] > f_max_a_total) pqpArchive.UAABKHarm100[iKu] = 0.0F;
						if (pqpArchive.UBBCKHarm95[iKu] > f_95_b_total) pqpArchive.UBBCKHarm95[iKu] = 0.0F;
						if (pqpArchive.UBBCKHarm100[iKu] > f_max_b_total) pqpArchive.UBBCKHarm100[iKu] = 0.0F;
						if (pqpArchive.UCCAKHarm95[iKu] > f_95_c_total) pqpArchive.UCCAKHarm95[iKu] = 0.0F;
						if (pqpArchive.UCCAKHarm100[iKu] > f_max_c_total) pqpArchive.UCCAKHarm100[iKu] = 0.0F;
					}

					shift_int += 2;
					shift_float += 4;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpNonsinus():");
				return false;
			}
		}

		private static bool GetPqpFlicker(ref byte[] buffer, ref PqpArchiveFields pqpArchive)
		{
			try
			{
				pqpArchive.BoolFlickerPltStatistics = Conversions.bytes_2_ushort(ref buffer, 521796) != 0;
				if (!pqpArchive.BoolFlickerPltStatistics)
					EmService.WriteToLogGeneral("GetPqpFlicker: BoolFlickerPltStatistics = 0");

				pqpArchive.FlickerPltCounterTotal = Conversions.bytes_2_ushort(ref buffer, 521792);
				pqpArchive.FlickerPltCounterNonflagged = Conversions.bytes_2_ushort(ref buffer, 521794);
				pqpArchive.FlickerPltCounterFlagged = (ushort)(pqpArchive.FlickerPltCounterTotal - pqpArchive.FlickerPltCounterNonflagged);

				// ГОСТы из уставок
				//float fNDP_short = constraints[12];
				//float fPDP_short = constraints[13];
				//float fNDP_long = constraints[14];
				//float fPDP_long = constraints[15];

				// отсчеты прибора
				// отсчетов между ПДП и НДП
				pqpArchive.UAABFlickerPstCounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514772);
				// за ПДП
				pqpArchive.UAABFlickerPstCounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514774);
				// в НДП
				pqpArchive.UAABFlickerPstCounterNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					pqpArchive.UAABFlickerPstCounterNonflaggedT1 - pqpArchive.UAABFlickerPstCounterNonflaggedT2);

				pqpArchive.UBBCFlickerPstCounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514788);
				pqpArchive.UBBCFlickerPstCounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514790);
				pqpArchive.UBBCFlickerPstCounterNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged - 
					pqpArchive.UBBCFlickerPstCounterNonflaggedT1 - pqpArchive.UBBCFlickerPstCounterNonflaggedT2);

				pqpArchive.UCCAFlickerPstCounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514804);
				pqpArchive.UCCAFlickerPstCounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514806);
				pqpArchive.UCCAFlickerPstCounterNonflaggedNormal = (ushort)(pqpArchive.TenMinuteCounterNonflagged -
					pqpArchive.UCCAFlickerPstCounterNonflaggedT1 - pqpArchive.UCCAFlickerPstCounterNonflaggedT2);

				pqpArchive.UAABFlickerPltCounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514776);
				pqpArchive.UAABFlickerPltCounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514778);
				pqpArchive.UAABFlickerPltCounterNonflaggedNormal = (ushort)(pqpArchive.FlickerPltCounterTotal -
					pqpArchive.UAABFlickerPltCounterNonflaggedT1 - pqpArchive.UAABFlickerPltCounterNonflaggedT2);

				pqpArchive.UBBCFlickerPltCounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514792);
				pqpArchive.UBBCFlickerPltCounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514794);
				pqpArchive.UBBCFlickerPltCounterNonflaggedNormal = (ushort)(pqpArchive.FlickerPltCounterTotal -
					pqpArchive.UBBCFlickerPltCounterNonflaggedT1 - pqpArchive.UBBCFlickerPltCounterNonflaggedT2);

				pqpArchive.UCCAFlickerPltCounterNonflaggedT1 = Conversions.bytes_2_ushort(ref buffer, 514808);
				pqpArchive.UCCAFlickerPltCounterNonflaggedT2 = Conversions.bytes_2_ushort(ref buffer, 514810);
				pqpArchive.UCCAFlickerPltCounterNonflaggedNormal = (ushort)(pqpArchive.FlickerPltCounterTotal -
					pqpArchive.UCCAFlickerPltCounterNonflaggedT1 - pqpArchive.UCCAFlickerPltCounterNonflaggedT2);

				// верхнее значение (95%) и наибольшее значение (max)
				pqpArchive.UAABFlickerPst95 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514780);
				pqpArchive.UAABFlickerPst100 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514782);

				pqpArchive.UBBCFlickerPst95 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514796);
				pqpArchive.UBBCFlickerPst100 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514798);

				pqpArchive.UCCAFlickerPst95 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514812);
				pqpArchive.UCCAFlickerPst100 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514814);

				pqpArchive.UAABFlickerPlt95 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514784);
				pqpArchive.UAABFlickerPlt100 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514786);

				pqpArchive.UBBCFlickerPlt95 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514800);
				pqpArchive.UBBCFlickerPlt100 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514802);

				pqpArchive.UCCAFlickerPlt95 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514816);
				pqpArchive.UCCAFlickerPlt100 = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, 514818);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpFlicker():");
				return false;
			}
		}

		private static bool GetPqpFlickerValues(ref byte[] buffer,
						ConnectScheme connectScheme, ref PqpArchiveFields pqpArchive)
		{
			try
			{
				// общее кол-во отсчетов
				//ushort num_all_short = Conversions.bytes_2_ushort(ref buffer, 483996);
				//ushort num_all_long = Conversions.bytes_2_ushort(ref buffer, 521792);
				//int shiftMarkedShort = 488036;
				int shiftMarkedLong = 521708;
				int shiftFlickerShort = 514820;
				int shiftFlickerLong = 520868;
				//int shiftSecondsShort = 484004;
				int shiftSecondsLong = 521372;

				//float[] flik_A = new float[pqpArchive.TenMinuteCounterTotal];
				//float[] flik_B = new float[pqpArchive.TenMinuteCounterTotal];
				//float[] flik_C = new float[pqpArchive.TenMinuteCounterTotal];
				//float[] flik_A_long = new float[pqpArchive.TenMinuteCounterTotal];
				//float[] flik_B_long = new float[pqpArchive.TenMinuteCounterTotal];
				//float[] flik_C_long = new float[pqpArchive.TenMinuteCounterTotal];
				//short[] flik_marked_short = new short[pqpArchive.TenMinuteCounterTotal];
				//short[] flik_marked_long = new short[pqpArchive.TenMinuteCounterTotal];
				//int[] seconds_short = new int[pqpArchive.TenMinuteCounterTotal];// Массив временных меток измерений
				//int[] seconds_long = new int[pqpArchive.TenMinuteCounterTotal];	// Массив временных меток измерений
				for (int iArr = 0; iArr < PqpArchiveFields.CntTenMinutes; ++iArr)
				{
					pqpArchive.UAABFlickerPst[iArr] = -1;
					pqpArchive.UBBCFlickerPst[iArr] = -1;
					pqpArchive.UCCAFlickerPst[iArr] = -1;
					//flik_marked_short[iArr] = 1;
					//seconds_short[iArr] = 0;
				}
				for (int iArr = 0; iArr < PqpArchiveFields.CntFlickerLong; ++iArr)
				{
					pqpArchive.UAABFlickerPlt[iArr] = -1;
					pqpArchive.UBBCFlickerPlt[iArr] = -1;
					pqpArchive.UCCAFlickerPlt[iArr] = -1;
					//flik_marked_long[iArr] = 1;
					pqpArchive.FlickerPltSeconds[iArr] = 0;
				}

				// считываем значение из буфера
				for (int iShort = 0; iShort < PqpArchiveFields.CntTenMinutes; ++iShort)
				{
					//seconds_short[iShort] = Conversions.bytes_2_int(ref buffer, shiftSecondsShort);

					if (pqpArchive.TenMinuteNotMarked[iShort])
						pqpArchive.UAABFlickerPst[iShort] =
							Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerShort);

					if (connectScheme != ConnectScheme.Ph1W2)
					{
						if (pqpArchive.TenMinuteNotMarked[iShort])
							pqpArchive.UBBCFlickerPst[iShort] = 
								Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerShort + 2016);
						if (pqpArchive.TenMinuteNotMarked[iShort])
							pqpArchive.UCCAFlickerPst[iShort] = 
								Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerShort + 4032);
					}

					shiftFlickerShort += 2;	// 2 is length of one record
					//shiftSecondsShort += 4;
				}

				//lt
				if (pqpArchive.FlickerPltCounterTotal > 0)
				{
					for (int iLong = 0; iLong < PqpArchiveFields.CntFlickerLong; ++iLong)
					{
						pqpArchive.FlickerPltValid[iLong] = buffer[shiftMarkedLong];
						pqpArchive.FlickerPltSeconds[iLong] = Conversions.bytes_2_int(ref buffer, shiftSecondsLong);

						if (pqpArchive.FlickerPltValid[iLong] == 0)
						{
							pqpArchive.UAABFlickerPlt[iLong] = Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerLong);
							if (connectScheme != ConnectScheme.Ph1W2)
							{
								pqpArchive.UBBCFlickerPlt[iLong] =
									Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerLong + 168);
								pqpArchive.UCCAFlickerPlt[iLong] =
									Conversions.bytes_2_signed_float_Q_7_8(ref buffer, shiftFlickerLong + 336);
							}
						}

						shiftFlickerLong += 2;
						shiftMarkedLong += 1;
						shiftSecondsLong += 4;
					}
				}

				for (int iRecord = 0; iRecord < PqpArchiveFields.CntTenMinutes; ++iRecord)
				{
					if (Single.IsInfinity(pqpArchive.UAABFlickerPst[iRecord]))
						pqpArchive.UAABFlickerPst[iRecord] = -1;
					if (Single.IsInfinity(pqpArchive.UBBCFlickerPst[iRecord]))
						pqpArchive.UBBCFlickerPst[iRecord] = -1;
					if (Single.IsInfinity(pqpArchive.UCCAFlickerPst[iRecord]))
						pqpArchive.UCCAFlickerPst[iRecord] = -1;
				}
				for (int iRecord = 0; iRecord < PqpArchiveFields.CntFlickerLong; ++iRecord)
				{
					if (Single.IsInfinity(pqpArchive.UAABFlickerPlt[iRecord]))
						pqpArchive.UAABFlickerPlt[iRecord] = -1;
					if (Single.IsInfinity(pqpArchive.UBBCFlickerPlt[iRecord]))
						pqpArchive.UBBCFlickerPlt[iRecord] = -1;
					if (Single.IsInfinity(pqpArchive.UCCAFlickerPlt[iRecord]))
						pqpArchive.UCCAFlickerPlt[iRecord] = -1;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpFlickerValues():");
				return false;
			}
		}

		private static bool GetPqpDipSwell(ref byte[] buffer, ref PqpArchiveFields pqpArchive, DEVICE_VERSIONS devVersion)
		{
			try
			{
				if (devVersion != DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073)
				{
					int shift = 260900 * 2;
					int shift_over_180 = 261226 * 2; // для событий длительностью >180 (они в архиве отдельно)
					for (int iEvent = 0; iEvent < PqpArchiveFields.DipSwellCountOld; ++iEvent)
					{
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_0_01_TILL_0_05]
													= Conversions.bytes_2_ushort(ref buffer, shift);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_0_05_TILL_0_1]
													= Conversions.bytes_2_ushort(ref buffer, shift + 2);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_0_1_TILL_0_5]
													= Conversions.bytes_2_ushort(ref buffer, shift + 4);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_0_5_TILL_1]
													= Conversions.bytes_2_ushort(ref buffer, shift + 6);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_1_TILL_3]
													= Conversions.bytes_2_ushort(ref buffer, shift + 8);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_3_TILL_20]
													= Conversions.bytes_2_ushort(ref buffer, shift + 10);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.FROM_20_TILL_60]
													= Conversions.bytes_2_ushort(ref buffer, shift + 12);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.OVER_60]
													= Conversions.bytes_2_ushort(ref buffer, shift + 14);
						pqpArchive.DipSwellOld[iEvent, (int)PqpDipSwellTimeOld.OVER_180]
													= Conversions.bytes_2_ushort(ref buffer, shift_over_180);

						shift += 8 * 2;
						shift_over_180 += 2;
					}
				}
				else
				{
					int shift = 261236 * 2;
					for (int iEvent = 0; iEvent < PqpArchiveFields.DipSwellCount; ++iEvent)
					{
						pqpArchive.DipSwell[iEvent, (int)PqpDipSwellTime.FROM_0_01_TILL_0_2]
													= Conversions.bytes_2_ushort(ref buffer, shift);
						pqpArchive.DipSwell[iEvent, (int)PqpDipSwellTime.FROM_0_2_TILL_0_5]
													= Conversions.bytes_2_ushort(ref buffer, shift + 2);
						pqpArchive.DipSwell[iEvent, (int)PqpDipSwellTime.FROM_0_5_TILL_1]
													= Conversions.bytes_2_ushort(ref buffer, shift + 4);
						pqpArchive.DipSwell[iEvent, (int)PqpDipSwellTime.FROM_1_TILL_5]
													= Conversions.bytes_2_ushort(ref buffer, shift + 6);
						pqpArchive.DipSwell[iEvent, (int)PqpDipSwellTime.FROM_5_TILL_20]
													= Conversions.bytes_2_ushort(ref buffer, shift + 8);
						pqpArchive.DipSwell[iEvent, (int)PqpDipSwellTime.FROM_20_TILL_60]
													= Conversions.bytes_2_ushort(ref buffer, shift + 10);

						shift += 6 * 2;
					}

					// прерывания теперь отдельно
					shift = 261290 * 2;
					pqpArchive.Interrupt[(int)PqpInterruptTime.FROM_0_TILL_0_5] = 
													Conversions.bytes_2_ushort(ref buffer, shift);
					pqpArchive.Interrupt[(int)PqpInterruptTime.FROM_0_5_TILL_1] =
													Conversions.bytes_2_ushort(ref buffer, shift + 2);
					pqpArchive.Interrupt[(int)PqpInterruptTime.FROM_1_TILL_5] =
													Conversions.bytes_2_ushort(ref buffer, shift + 4);
					pqpArchive.Interrupt[(int)PqpInterruptTime.FROM_5_TILL_20] =
													Conversions.bytes_2_ushort(ref buffer, shift + 6);
					pqpArchive.Interrupt[(int)PqpInterruptTime.FROM_20_TILL_60] =
													Conversions.bytes_2_ushort(ref buffer, shift + 8);
					pqpArchive.Interrupt[(int)PqpInterruptTime.FROM_60_TILL_180] =
													Conversions.bytes_2_ushort(ref buffer, shift + 10);
					pqpArchive.Interrupt[(int)PqpInterruptTime.OVER_180] =
													Conversions.bytes_2_ushort(ref buffer, shift + 12);

					pqpArchive.InterruptionMaxDuration = Conversions.bytes_2_uint(ref buffer, 261298 * 2);
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpDipSwell():");
				return false;
			}
		}

		private static bool GetPqpVoltageInterharm(ref byte[] buffer, ref PqpArchiveFields pqpArchive,
											bool uTransformerEnable, short uTransformerType)
		{
			try
			{
				int shift = 521960;

				float uMultiplier = 1;
				if (uTransformerEnable)
					uMultiplier = GetUTransformerMultiplier(uTransformerType);
				uMultiplier /= 1000000f;

				for (int iInter = 0; iInter < PqpArchiveFields.VoltageInterhartCount; ++iInter)
				{
					pqpArchive.UAABInterharm[iInter] = Conversions.bytes_2_int(ref buffer, shift) * uMultiplier;
					pqpArchive.UBBCInterharm[iInter] = Conversions.bytes_2_int(ref buffer, shift + 164) * uMultiplier;
					pqpArchive.UCCAInterharm[iInter] = 
						Conversions.bytes_2_int(ref buffer, shift + 164 + 164) * uMultiplier;
					pqpArchive.UAABInterharm[iInter] = (float)Math.Round(pqpArchive.UAABInterharm[iInter], 4);
					pqpArchive.UBBCInterharm[iInter] = (float)Math.Round(pqpArchive.UBBCInterharm[iInter], 4);
					pqpArchive.UCCAInterharm[iInter] = (float)Math.Round(pqpArchive.UCCAInterharm[iInter], 4);

					shift += 4;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetPqpVoltageInterharm():");
				return false;
			}
		}

		#endregion

		#region General info

		private static bool GetRegistrationInfo(string path, int length, ref RegistrationInfo regInfo,
												out int shift, out int headerLen, ref BinaryReader binReader,
												ref byte[] buffer)
		{
			try
			{
				shift = 0;
				//headerLen = 0;

				FileInfo fileInfo = new FileInfo(path);
				char curChar = Char.MinValue;
				string sHeaderLen = string.Empty;
				while (curChar != '|')
				{
					curChar = (char) binReader.ReadByte();
					if (Char.IsDigit(curChar)) sHeaderLen += curChar;
				}
				headerLen = Int32.Parse(sHeaderLen);

				if (fileInfo.Length < (headerLen + length))
				{
					EmService.WriteToLogFailed("GetMainArchiveInfo: fileInfo.Length <");
					return false;
				}

				buffer = binReader.ReadBytes(headerLen + length);
				int fileVersion = GetNumberFromBuffer(ref buffer, ref shift, '|');	// my field, not used so far
				int serialNumber = GetNumberFromBuffer(ref buffer, ref shift, '|');
				int regId = GetNumberFromBuffer(ref buffer, ref shift, '|');

				if (serialNumber == -1 || regId == -1)
				{
					EmService.WriteToLogFailed("GetMainArchiveInfo: -1");
					return false;
				}

				// registration data
				int regDataLen = GetNumberFromBuffer(ref buffer, ref shift, '|');
				byte[] regDataBytes = new byte[regDataLen];
				Array.Copy(buffer, shift, regDataBytes, 0, regDataLen);

				// проверяем не слишком ли старая прошивка у прибора
				ushort ver_num = Conversions.bytes_2_ushort(ref regDataBytes, 122);
				if (ver_num < 1)
				{
					EmService.WriteToLogFailed("Device version: " + ver_num.ToString());
					EmService.WriteToLogFailed(string.Format("Device version bytes: {0}, {1}",
					                                         regDataBytes[122], regDataBytes[123]));
					return false;
				}

				#region Fill registration fields

				regInfo = new RegistrationInfo();
				regInfo.SerialNumber = serialNumber;
				regInfo.RegId = regId;

				regInfo.DtStart = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref regDataBytes, 8,
				                                                            "reg Start date");
				regInfo.DtEnd = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref regDataBytes, 36,
				                                                          "reg Start date");
				// object name
				regInfo.RegistrationName = Conversions.bytes_2_string(ref regDataBytes, 64, 16);
				if (regInfo.RegistrationName == "") regInfo.RegistrationName = "default object";
				// connection scheme
				ushort usConScheme = Conversions.bytes_2_ushort(ref regDataBytes, 80);
				switch (usConScheme)
				{
					case 0:
						regInfo.ConnectionScheme = ConnectScheme.Ph1W2;
						break;
					case 1:
						regInfo.ConnectionScheme = ConnectScheme.Ph3W4;
						break;
					case 2:
						regInfo.ConnectionScheme = ConnectScheme.Ph3W3;
						break;
					case 3:
						regInfo.ConnectionScheme = ConnectScheme.Ph3W3_B_calc;
						break;
					default:
						regInfo.ConnectionScheme = ConnectScheme.Unknown;
						break;
				}

				regInfo.Ulimit = Conversions.bytes_2_short(ref regDataBytes, 82);
				regInfo.Ilimit = Conversions.bytes_2_short(ref regDataBytes, 84);
				regInfo.Flimit = Conversions.bytes_2_short(ref regDataBytes, 86);

				regInfo.AutocorrectTimeGpsEnable = Conversions.bytes_2_ushort(ref regDataBytes, 104) != 0;
				regInfo.GpsLatitude = Conversions.bytes_2_double(ref regDataBytes, 1224);
				regInfo.GpsLongitude = Conversions.bytes_2_double(ref regDataBytes, 1232);

				// nominal f
				regInfo.Fnominal = Conversions.bytes_2_ushort(ref regDataBytes, 86);

				// transformers
				regInfo.UtransformerEnable = (Conversions.bytes_2_ushort(ref regDataBytes, 88) == 0);
				regInfo.UtransformerType = Conversions.bytes_2_short(ref regDataBytes, 90);
				regInfo.ItransformerUsage = Conversions.bytes_2_short(ref regDataBytes, 98);
				regInfo.ItransformerPrimary = Conversions.bytes_2_short(ref regDataBytes, 100);
				if (regInfo.ItransformerUsage == 1) regInfo.ItransformerSecondary = 1;
				else if (regInfo.ItransformerUsage == 2) regInfo.ItransformerSecondary = 5;

				switch (regInfo.ConnectionScheme)
				{
					case ConnectScheme.Ph1W2:
						regInfo.UnominalPhase = 220;
						regInfo.UnominalLinear = 381;
						break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						if (regInfo.UtransformerEnable)
						{
							regInfo.UnominalPhase = 57.7F;
							regInfo.UnominalLinear = 100;
						}
						else
						{
							regInfo.UnominalPhase = 219.4F;
							regInfo.UnominalLinear = 380;
						}
						break;
					case ConnectScheme.Ph3W4:
					//case ConnectScheme.Ph3W4_B_calc:
						if (regInfo.UtransformerEnable)
						{
							regInfo.UnominalPhase = 57.7F;
							regInfo.UnominalLinear = 100;
						}
						else
						{
							regInfo.UnominalPhase = 220;
							regInfo.UnominalLinear = 381;
						}
						break;
				}

				// constraint_type
				regInfo.ConstraintType =
					(ConstraintsType) Conversions.bytes_2_short(ref regDataBytes, 108);
				// constraints
				for (int iConstr = 0; iConstr < EtPQPAConstraints.CntConstraints;
					++iConstr)
				{
					regInfo.ConstraintsArray[iConstr] =
						Conversions.bytes_2_signed_float_Q_15_16_new(ref regDataBytes, 128 + iConstr * 4);
				}
				regInfo.Constraints = new ConstraintsDetailed(regInfo.ConstraintsArray);

				// marked on off
				regInfo.MarkedOnOff = (Conversions.bytes_2_ushort(ref regDataBytes, 1258) == 0);

				// device version
				uint verNum = Conversions.bytes_2_uint_new(ref regDataBytes, 124);
				string sVernum = verNum.ToString();
				regInfo.DeviceVersion = sVernum;
				try
				{
					regInfo.DeviceVersionDate = new DateTime(
						Int32.Parse(sVernum.Substring(sVernum.Length - 6, 2)) + 2000,
						Int32.Parse(sVernum.Substring(sVernum.Length - 4, 2)),
						Int32.Parse(sVernum.Substring(sVernum.Length - 2, 2)));
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Error getting device version date!");
					regInfo.DeviceVersionDate = DateTime.MinValue;
				}

				#endregion

				shift += (regDataLen + 1 /*separator*/);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetRegistrationInfo:");
				throw;
			}
		}

		public static bool GetInfoAboutAvgArchive(string path, out RegistrationInfo regInfo,
		                                          out AvgArchiveInfo avgArchive)
		{
			regInfo = null;
			avgArchive = null;
			try
			{
				BinaryReader binReader = new BinaryReader(File.Open(path, FileMode.Open));
				int shift, headerLen;
				byte[] buffer = null;
				if (!GetRegistrationInfo(path, EmService.AVG_RECORD_LENGTH, ref regInfo, out shift,
				                         out headerLen, ref binReader, ref buffer))
					return false;

				AvgTypes avgType = (AvgTypes)GetNumberFromBuffer(ref buffer, ref shift, '|');

				DateTime dtArchiveStart =
					Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 6 * 2 + shift,
					                                          "AVG Start date");

				FileInfo fileInfo = new FileInfo(path);
				long archiveLen = fileInfo.Length - shift;
				int avgRecordsCount = (int) (archiveLen / EmService.AVG_RECORD_LENGTH);
				shift = headerLen + EmService.AVG_RECORD_LENGTH * (avgRecordsCount - 1);
				shift += DigitCount(headerLen) + 1 /*separator*/;

				binReader.BaseStream.Seek(shift, SeekOrigin.Begin);
				buffer = binReader.ReadBytes(EmService.AVG_RECORD_LENGTH);
				DateTime dtArchiveEnd =
					Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 20 * 2,
					                                          "AVG End date");

				avgArchive = new AvgArchiveInfo(path, dtArchiveStart, dtArchiveEnd, avgType);
				regInfo.SetAvgInfo(avgArchive);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetInfoAboutAvgArchive:");
				throw;
			}
		}

		public static bool GetInfoAboutPqpArchive(string path, out RegistrationInfo regInfo, 
													out PqpArchiveInfo pqpArchive)
		{
			regInfo = null;
			pqpArchive = null;
			try
			{
				BinaryReader binReader = new BinaryReader(File.Open(path, FileMode.Open));
				int shift, headerLen;
				byte[] buffer = null;
				if (!GetRegistrationInfo(path, EmService.PQP_SEGMENT_LENGTH, ref regInfo, out shift,
										 out headerLen, ref binReader, ref buffer))
					return false;

				int pqpArchiveId = GetNumberFromBuffer(ref buffer, ref shift, '|');

				DateTime dtArchiveStart =
					Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 6 * 2 + shift,
					                                          "PQP Start date");
				DateTime dtArchiveEnd =
					Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 42 * 2 + shift,
					                                          "PQP End date");

				pqpArchive = new PqpArchiveInfo(pqpArchiveId, path, dtArchiveStart, dtArchiveEnd);
				regInfo.PqpArchives.Add(pqpArchive);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetInfoAboutPqpArchive");
				throw;
			}
		}

		public static bool GetInfoAboutDnsArchive(string path, out RegistrationInfo regInfo,
													out DnsArchiveInfo dnsArchive)
		{
			regInfo = null;
			dnsArchive = null;
			try
			{
				BinaryReader binReader = new BinaryReader(File.Open(path, FileMode.Open));
				int shift, headerLen;
				byte[] buffer = null;
				if (!GetRegistrationInfo(path, EmService.DNS_RECORD_LENGTH, ref regInfo, out shift,
										 out headerLen, ref binReader, ref buffer))
					return false;

				dnsArchive = new DnsArchiveInfo(path, regInfo.DtStart, regInfo.DtEnd);
				regInfo.DnsArchive = dnsArchive;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetInfoAboutDnsArchive:");
				throw;
			}
		}

		private static int GetNumberFromBuffer(ref byte[] buf, ref int shift, char separator)
		{
			string sNum = string.Empty;
			try
			{
				for (int i = shift; i < buf.Length; ++i)
				{
					char current = (char)buf[i];
					if (Char.IsDigit(current)) sNum += current;
					++shift;
					if (current == separator) break;
				}
				return Int32.Parse(sNum);
			}
			catch (Exception)
			{
				EmService.WriteToLogFailed("GetNumberFromBuffer error: " + sNum);
				return -1;
			}
		}

		private static int DigitCount(long number)
		{
			int result = 1;
			while ((number /= 10) != 0)
				result++;
			return result;
		}

		#endregion

		#region AVG

		// for AVG archive the user can change start and end dates so we must pass them through parameters
		// (dtStart and dtEnd)
		public static bool GetAvgRecordFields(string path, out List<AvgRecordFields> listAvgRecords,
												ref RegistrationInfo regInfo,
												DateTime dtStartSelected, DateTime dtEndSelected)
		{
			BinaryReader binReader = null;
			try
			{
				listAvgRecords = new List<AvgRecordFields>();

				binReader = new BinaryReader(File.Open(path, FileMode.Open));
				byte[] buffer = null;

				FileInfo fileInfo = new FileInfo(path);
				char curChar = Char.MinValue;
				string sHeaderLen = string.Empty;
				while (curChar != '|')
				{
					curChar = (char) binReader.ReadByte();
					if (Char.IsDigit(curChar)) sHeaderLen += curChar;
				}
				int headerLen = Int32.Parse(sHeaderLen);

				if (fileInfo.Length < (headerLen + EmService.AVG_RECORD_LENGTH))
				{
					EmService.WriteToLogFailed("GetAvgRecordFields: fileInfo.Length <");
					return false;
				}

				//buffer = binReader.ReadBytes(headerLen);
				//int serialNumber = GetNumberFromBuffer(ref buffer, ref shift, '|');
				//int regId = GetNumberFromBuffer(ref buffer, ref shift, '|');

				//if (serialNumber == -1 || regId == -1)
				//{
				//    EmService.WriteToLogFailed("GetPqpArchiveFields: -1");
				//    return false;
				//}

				//// registration data
				//int regDataLen = GetNumberFromBuffer(ref buffer, ref shift, '|');
				//byte[] regDataBytes = new byte[regDataLen];
				//Array.Copy(buffer, shift, regDataBytes, 0, regDataLen);

				//shift += (regDataLen + 1 /*separator*/);

				int shift = headerLen + DigitCount(headerLen) + 1 /*separator*/;
				// read all data which is after header
				binReader.BaseStream.Seek(shift, SeekOrigin.Begin);
				buffer = binReader.ReadBytes((int) fileInfo.Length - shift);

				if (buffer.Length < EmService.AVG_RECORD_LENGTH)
				{
					EmService.WriteToLogFailed("GetAvgRecordFields: fileInfo.Length <<");
					return false;
				}

				int recordsCount = buffer.Length / EmService.AVG_RECORD_LENGTH;
				List<UInt32> listRecordId = new List<UInt32>(recordsCount);

				shift = 0;
				for (int iRecord = 0; iRecord < recordsCount; ++iRecord)
				{
					// check for duplication
					uint recordId = Conversions.bytes_2_uint_new(ref buffer, shift + 0);
					if (listRecordId.Contains(recordId))
					{
						EmService.WriteToLogFailed("GetAvgRecordFields: Duplicate recordId!  " + recordId.ToString());
						shift += EmService.AVG_RECORD_LENGTH;
						continue;
					}
					listRecordId.Add(recordId);
					// end of check for duplication

					AvgRecordFields record = new AvgRecordFields();
					record.DtStart =
						Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, shift + 12, "AVG record start date");
					record.DtEnd =
						Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, shift + 40, "AVG record end date");

					if (record.DtStart == DateTime.MinValue || record.DtEnd == DateTime.MinValue)
					{
						EmService.WriteToLogFailed("Error in GetAvgRecordFields() dates:");
						shift += EmService.AVG_RECORD_LENGTH;
						continue;
					}

					if (record.DtStart < dtStartSelected || record.DtEnd > dtEndSelected)
					{
						shift += EmService.AVG_RECORD_LENGTH;
						continue;
					}

					// 0-record is NOT marked, 1-record is marked
					record.BRecordIsMarked = Conversions.bytes_2_ushort(ref buffer, shift + 92) != 0;

					if (!GetAvgUIF(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgPower(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgPqp(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgAngles(ref buffer, shift, ref record)) continue;
					if (!GetAvgVoltageHarmonics(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgVoltageInterHarmonics(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgCurrentHarmonics(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgCurrentInterHarmonics(ref buffer, shift, ref record, ref regInfo)) continue;
					if (!GetAvgHarmonicPower(ref buffer, shift, ref record, ref regInfo)) continue;

					listAvgRecords.Add(record);
					shift += EmService.AVG_RECORD_LENGTH;
				}

				return true;
			}
			catch (IOException ex)
			{
				EmService.DumpException(ex, "IOException in GetAvgRecordFields");
				listAvgRecords = null;
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GetAvgRecordFields");
				throw;
			}
			finally
			{
				if (binReader != null)
				{
					binReader.Close();
					binReader.Dispose();
				}
			}
		}

		/// <summary>Частота, напряжение, ток</summary>
		private static bool GetAvgUIF(ref byte[] buffer, int shift, ref AvgRecordFields record, ref RegistrationInfo regInfo)
		{
			try
			{
				float uMultiplier = 1;
				if (regInfo.UtransformerEnable)
					uMultiplier = EmService.GetUTransformerMultiplier(regInfo.UtransformerType);
				uMultiplier /= 1000000f;

				float iMultiplier = regInfo.Ilimit;
				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					iMultiplier *= regInfo.ItransformerPrimary;
					iMultiplier /= regInfo.ItransformerSecondary;
				}
				iMultiplier /= 1000000f;

				// частота
				record.Fa = Conversions.bytes_2_uint_new(ref buffer, shift + 256) / 1000f;
				record.Fb = Conversions.bytes_2_uint_new(ref buffer, shift + 260) / 1000f;
				record.Fc = Conversions.bytes_2_uint_new(ref buffer, shift + 264) / 1000f;
				record.Fab = Conversions.bytes_2_uint_new(ref buffer, shift + 268) / 1000f;
				record.Fbc = Conversions.bytes_2_uint_new(ref buffer, shift + 272) / 1000f;
				record.Fca = Conversions.bytes_2_uint_new(ref buffer, shift + 276) / 1000f;

				// напряжение - действующие значения
				record.Ua = Conversions.bytes_2_uint_new(ref buffer, shift + 280) * uMultiplier;
				record.Ub = Conversions.bytes_2_uint_new(ref buffer, shift + 284) * uMultiplier;
				record.Uc = Conversions.bytes_2_uint_new(ref buffer, shift + 288) * uMultiplier;
				record.Uab = Conversions.bytes_2_uint_new(ref buffer, shift + 292) * uMultiplier;
				record.Ubc = Conversions.bytes_2_uint_new(ref buffer, shift + 296) * uMultiplier;
				record.Uca = Conversions.bytes_2_uint_new(ref buffer, shift + 300) * uMultiplier;

				// ток - действующие значения
				record.Ia = Conversions.bytes_2_uint_new(ref buffer, shift + 304) * iMultiplier;
				record.Ib = Conversions.bytes_2_uint_new(ref buffer, shift + 308) * iMultiplier;
				record.Ic = Conversions.bytes_2_uint_new(ref buffer, shift + 312) * iMultiplier;
				record.In = Conversions.bytes_2_uint_new(ref buffer, shift + 316) * iMultiplier;

				// напряжение - постоянная составляющая
				record.UaConst = Conversions.bytes_2_int(ref buffer, shift + 320) * uMultiplier;
				record.UbConst = Conversions.bytes_2_int(ref buffer, shift + 324) * uMultiplier;
				record.UcConst = Conversions.bytes_2_int(ref buffer, shift + 328) * uMultiplier;
				record.UabConst = Conversions.bytes_2_int(ref buffer, shift + 332) * uMultiplier;
				record.UbcConst = Conversions.bytes_2_int(ref buffer, shift + 336) * uMultiplier;
				record.UcaConst = Conversions.bytes_2_int(ref buffer, shift + 340) * uMultiplier;

				// ток - постоянная составляющая
				record.IaConst = Conversions.bytes_2_int(ref buffer, shift + 344) * iMultiplier;
				record.IbConst = Conversions.bytes_2_int(ref buffer, shift + 348) * iMultiplier;
				record.IcConst = Conversions.bytes_2_int(ref buffer, shift + 352) * iMultiplier;
				record.InConst = Conversions.bytes_2_int(ref buffer, shift + 356) * iMultiplier;

				// напряжение - средневыпрямленное значение
				record.UaAvdirect = Conversions.bytes_2_int(ref buffer, shift + 360) * uMultiplier;
				record.UbAvdirect = Conversions.bytes_2_int(ref buffer, shift + 364) * uMultiplier;
				record.UcAvdirect = Conversions.bytes_2_int(ref buffer, shift + 368) * uMultiplier;
				record.UabAvdirect = Conversions.bytes_2_int(ref buffer, shift + 372) * uMultiplier;
				record.UbcAvdirect = Conversions.bytes_2_int(ref buffer, shift + 376) * uMultiplier;
				record.UcaAvdirect = Conversions.bytes_2_int(ref buffer, shift + 380) * uMultiplier;

				// ток - средневыпрямленное значение
				record.IaAvdirect = Conversions.bytes_2_int(ref buffer, shift + 384) * iMultiplier;
				record.IbAvdirect = Conversions.bytes_2_int(ref buffer, shift + 388) * iMultiplier;
				record.IcAvdirect = Conversions.bytes_2_int(ref buffer, shift + 392) * iMultiplier;
				record.InAvdirect = Conversions.bytes_2_int(ref buffer, shift + 396) * iMultiplier;

				// напряжение - 1-ая гармоника
				record.Ua1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 400) * uMultiplier;
				record.Ub1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 404) * uMultiplier;
				record.Uc1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 408) * uMultiplier;
				record.Uab1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 412) * uMultiplier;
				record.Ubc1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 416) * uMultiplier;
				record.Uca1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 420) * uMultiplier;

				// ток - 1-ая гармоника
				record.Ia1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 424) * iMultiplier;
				record.Ib1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 428) * iMultiplier;
				record.Ic1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 432) * iMultiplier;
				record.In1harm = Conversions.bytes_2_uint_new(ref buffer, shift + 436) * iMultiplier;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgUIF():");
				return false;
			}
		}

		private static bool GetAvgPower(ref byte[] buffer, int shift, ref AvgRecordFields record, ref RegistrationInfo regInfo)
		{
			try
			{
				float pMultiplier = regInfo.Ilimit;
				if (regInfo.UtransformerEnable)
					pMultiplier *= EmService.GetUTransformerMultiplier(regInfo.UtransformerType);

				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					pMultiplier *= regInfo.ItransformerPrimary;
					pMultiplier /= regInfo.ItransformerSecondary;
				}
				pMultiplier /= 1000000f;

				// Мощность активная
				record.Pa = Conversions.bytes_2_int(ref buffer, shift + 440) * pMultiplier;
				record.Pb = Conversions.bytes_2_int(ref buffer, shift + 444) * pMultiplier;
				record.Pc = Conversions.bytes_2_int(ref buffer, shift + 448) * pMultiplier;
				record.Psum = Conversions.bytes_2_int(ref buffer, shift + 452) * pMultiplier;
				record.P1 = Conversions.bytes_2_int(ref buffer, shift + 456) * pMultiplier;
				record.P2 = Conversions.bytes_2_int(ref buffer, shift + 460) * pMultiplier;
				record.P12sum = Conversions.bytes_2_int(ref buffer, shift + 464) * pMultiplier;

				// Мощность полная
				record.Sa = Conversions.bytes_2_int(ref buffer, shift + 468) * pMultiplier;
				record.Sb = Conversions.bytes_2_int(ref buffer, shift + 472) * pMultiplier;
				record.Sc = Conversions.bytes_2_int(ref buffer, shift + 476) * pMultiplier;
				record.Ssum = Conversions.bytes_2_int(ref buffer, shift + 480) * pMultiplier;
				record.S1 = Conversions.bytes_2_int(ref buffer, shift + 484) * pMultiplier;
				record.S2 = Conversions.bytes_2_int(ref buffer, shift + 488) * pMultiplier;
				record.S12sum = Conversions.bytes_2_int(ref buffer, shift + 492) * pMultiplier;

				// Мощность реактивная (по первой гармонике)
				record.Qa = Conversions.bytes_2_int(ref buffer, shift + 496) * pMultiplier;
				record.Qb = Conversions.bytes_2_int(ref buffer, shift + 500) * pMultiplier;
				record.Qc = Conversions.bytes_2_int(ref buffer, shift + 504) * pMultiplier;
				record.Qsum = Conversions.bytes_2_int(ref buffer, shift + 508) * pMultiplier;
				record.Q1 = Conversions.bytes_2_int(ref buffer, shift + 512) * pMultiplier;
				record.Q2 = Conversions.bytes_2_int(ref buffer, shift + 516) * pMultiplier;
				record.Q12sum = Conversions.bytes_2_int(ref buffer, shift + 520) * pMultiplier;

				if (regInfo.ConnectionScheme == ConnectScheme.Ph3W3 ||
					regInfo.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					if (Math.Abs(record.P12sum) >= 0.05 && Math.Abs(record.Q12sum) >= 0.05)
					{
						record.TanP = (record.P12sum == 0 ? 0 : record.Q12sum / record.P12sum);
						record.TanP = (float)Math.Round((double)record.TanP, 8);
					}
				}
				else if (regInfo.ConnectionScheme == ConnectScheme.Ph1W2)
				{
					if (Math.Abs(record.Pa) >= 0.05 && Math.Abs(record.Qa) >= 0.05)
					{
						record.TanP = (record.Pa == 0 ? 0 : record.Qa / record.Pa);
						record.TanP = (float)Math.Round((double)record.TanP, 8);
					}
				}
				else  // 3ph 4w
				{
					if (Math.Abs(record.Psum) >= 0.05 && Math.Abs(record.Qsum) >= 0.05)
					{
						record.TanP = (record.Psum == 0 ? 0 : record.Qsum / record.Psum);
						record.TanP = (float)Math.Round((double)record.TanP, 8);
					}
				}

				// Коэффициент мощности Kp
				record.Kpa = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 524);
				record.Kpb = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 528);
				record.Kpc = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 532);
				record.Kpabc = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 536);
				record.Kp12 = Conversions.bytes_2_signed_float_Q_0_31_new(ref buffer, shift + 540);

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgPower():");
				return false;
			}
		}

		private static bool GetAvgPqp(ref byte[] buffer, int shift, ref AvgRecordFields record, ref RegistrationInfo regInfo)
		{
			try
			{
				float uMultiplier = 1;
				if (regInfo.UtransformerEnable)
					uMultiplier = EmService.GetUTransformerMultiplier(regInfo.UtransformerType);
				uMultiplier /= 1000000f;

				float iMultiplier = regInfo.Ilimit;
				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					iMultiplier *= regInfo.ItransformerPrimary;
					iMultiplier /= regInfo.ItransformerSecondary;
				}
				iMultiplier /= 1000000f;

				float pMultiplier = regInfo.Ilimit;
				if (regInfo.UtransformerEnable)
					pMultiplier *= EmService.GetUTransformerMultiplier(regInfo.UtransformerType);

				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					pMultiplier *= regInfo.ItransformerPrimary;
					pMultiplier /= regInfo.ItransformerSecondary;
				}
				pMultiplier /= 1000000f;

				// Напряжение прямой последовательности 
				record.U1 = Conversions.bytes_2_int(ref buffer, shift + 544) * uMultiplier;
				// Напряжение обратной последовательности
				record.U2 = Conversions.bytes_2_int(ref buffer, shift + 548) * uMultiplier;
				// Напряжение нулевой последовательности
				record.U0 = Conversions.bytes_2_int(ref buffer, shift + 552) * uMultiplier;
				// Коэффициент обратной последовательности
				record.K2u = Conversions.bytes_2_int(ref buffer, shift + 556) / 1342177.28f;
				// Коэффициент нулевой последовательности
				record.K0u = Conversions.bytes_2_int(ref buffer, shift + 560) / 1342177.28f;
				// Ток прямой последовательности
				record.I1 = Conversions.bytes_2_int(ref buffer, shift + 564) * iMultiplier;
				// Ток обратной последовательности
				record.I2 = Conversions.bytes_2_int(ref buffer, shift + 568) * iMultiplier;
				// Ток нулевой последовательности
				record.I0 = Conversions.bytes_2_int(ref buffer, shift + 572) * iMultiplier;
				// Мощность прямой последовательности
				record.P1pqp = Conversions.bytes_2_int(ref buffer, shift + 576) * pMultiplier;
				// Мощность обратной последовательности
				record.P2pqp = Conversions.bytes_2_int(ref buffer, shift + 580) * pMultiplier;
				// Мощность нулевой последовательности
				record.P0pqp = Conversions.bytes_2_int(ref buffer, shift + 584) * pMultiplier;
				// Угол мощности прямой последовательности
				record.AngleP1 = Conversions.bytes_2_int(ref buffer, shift + 588) / 1000f;
				// Угол мощности обратной последовательности
				record.AngleP2 = Conversions.bytes_2_int(ref buffer, shift + 592) / 1000f;
				// Угол мощности нулевой последовательности
				record.AngleP0 = Conversions.bytes_2_int(ref buffer, shift + 596) / 1000f;

				// Отклонение установившегося напряжения [относительное]
				record.RdUY = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 600) * 100;
				// Отклонение 1 гармоники от номинала – фаза A [относительное]
				record.RdU1harmA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 604) * 100;
				record.RdU1harmB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 608) * 100;
				record.RdU1harmC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 612) * 100;
				record.RdU1harmAB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 616) * 100;
				record.RdU1harmBC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 620) * 100;
				record.RdU1harmCA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 624) * 100;

				// Положительное отклонение напряжения - фаза A [абсолютное]
				record.DUposA = Conversions.bytes_2_int(ref buffer, shift + 628) * uMultiplier;
				record.DUposB = Conversions.bytes_2_int(ref buffer, shift + 632) * uMultiplier;
				record.DUposC = Conversions.bytes_2_int(ref buffer, shift + 636) * uMultiplier;
				record.DUposAB = Conversions.bytes_2_int(ref buffer, shift + 640) * uMultiplier;
				record.DUposBC = Conversions.bytes_2_int(ref buffer, shift + 644) * uMultiplier;
				record.DUposCA = Conversions.bytes_2_int(ref buffer, shift + 648) * uMultiplier;

				// Отрицательное отклонение напряжения - фаза A [абсолютное]
				record.DUnegA = Conversions.bytes_2_int(ref buffer, shift + 652) * uMultiplier;
				record.DUnegB = Conversions.bytes_2_int(ref buffer, shift + 656) * uMultiplier;
				record.DUnegC = Conversions.bytes_2_int(ref buffer, shift + 660) * uMultiplier;
				record.DUnegAB = Conversions.bytes_2_int(ref buffer, shift + 664) * uMultiplier;
				record.DUnegBC = Conversions.bytes_2_int(ref buffer, shift + 668) * uMultiplier;
				record.DUnegCA = Conversions.bytes_2_int(ref buffer, shift + 672) * uMultiplier;

				// Положительное отклонение напряжения - фаза A [относительное]
				record.RdUposA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 676) * 100;
				record.RdUposB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 680) * 100;
				record.RdUposC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 684) * 100;
				record.RdUposAB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 688) * 100;
				record.RdUposBC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 692) * 100;
				record.RdUposCA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 696) * 100;

				// Отрицательное отклонение напряжения - фаза A [относительное]
				record.RdUnegA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 700) * 100;
				record.RdUnegB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 704) * 100;
				record.RdUnegC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 708) * 100;
				record.RdUnegAB = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 712) * 100;
				record.RdUnegBC = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 716) * 100;
				record.RdUnegCA = Conversions.bytes_2_signed_float_Q_4_27_new(ref buffer, shift + 720) * 100;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgPqp():");
				return false;
			}
		}

		private static bool GetAvgAngles(ref byte[] buffer, int shift, ref AvgRecordFields record)
		{
			try
			{
				const float aMultiplier = 1f / 1000f;

				// Углы между напряжениями и токами 1-ой гармоники
				// Между фазными напряжениями UA UB
				record.AngleUaUb = Conversions.bytes_2_int(ref buffer, shift + 724) * aMultiplier;
				record.AngleUbUc = Conversions.bytes_2_int(ref buffer, shift + 728) * aMultiplier;
				record.AngleUcUa = Conversions.bytes_2_int(ref buffer, shift + 732) * aMultiplier;

				// Между фазным напряжением UA и током IA
				record.AngleUaIa = Conversions.bytes_2_int(ref buffer, shift + 736) * aMultiplier;
				record.AngleUbIb = Conversions.bytes_2_int(ref buffer, shift + 740) * aMultiplier;
				record.AngleUcIc = Conversions.bytes_2_int(ref buffer, shift + 744) * aMultiplier;

				// Между междуфазными напряжениями AB и CB
				record.AngleUabUcb = Conversions.bytes_2_int(ref buffer, shift + 748) * aMultiplier;
				// Между междуфазным напряжением AB и током IA
				record.AngleUabIa = Conversions.bytes_2_int(ref buffer, shift + 752) * aMultiplier;
				record.AngleUcbIc = Conversions.bytes_2_int(ref buffer, shift + 756) * aMultiplier;

				// Углы между напряжениями и токами 1-ой гармоники (ВСПОМОГАТЕЛЬНЫЕ)
				// Между междуфазными напряжениями AB и BC
				//record.AngleUabUbc = Conversions.bytes_2_int(ref buffer, shift + 10600) * aMultiplier;
				//record.AngleUbcUca = Conversions.bytes_2_int(ref buffer, shift + 10604) * aMultiplier;
				//record.AngleUcaUab = Conversions.bytes_2_int(ref buffer, shift + 10608) * aMultiplier;

				// Между междуфазным напряжением BC и током IA (вспомогательный угол для графики)
				//record.AngleUbcIa = Conversions.bytes_2_int(ref buffer, shift + 10612) * aMultiplier;
				//record.AngleUbcIb = Conversions.bytes_2_int(ref buffer, shift + 10616) * aMultiplier;
				//record.AngleUbcIc = Conversions.bytes_2_int(ref buffer, shift + 10620) * aMultiplier;
				//record.AngleUbcIn = Conversions.bytes_2_int(ref buffer, shift + 10624) * aMultiplier;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgAngles():");
				return false;
			}
		}

		private static bool GetAvgVoltageHarmonics(ref byte[] buffer, int shift, ref AvgRecordFields record, 
													ref RegistrationInfo regInfo)
		{
			try
			{
				float uMultiplier = 1;
				if (regInfo.UtransformerEnable) 
					uMultiplier = EmService.GetUTransformerMultiplier(regInfo.UtransformerType);
				uMultiplier /= 1000000f;

				// Ua
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.UHarmSummForOrderMore1A = Conversions.bytes_2_int(ref buffer, shift + 880) * uMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderValueA[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 884 + iOrder * 4) * uMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderCoefA[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
												ref buffer, shift + 1084 + iOrder * 4) * 100;
				}
				// Ub
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.UHarmSummForOrderMore1B = Conversions.bytes_2_int(ref buffer, shift + 1284) * uMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderValueB[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 1288 + iOrder * 4) * uMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderCoefB[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
													ref buffer, shift + 1488 + iOrder * 4) * 100;
				}
				// Uc
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.UHarmSummForOrderMore1C = Conversions.bytes_2_int(ref buffer, shift + 1688) * uMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderValueC[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 1692 + iOrder * 4) * uMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderCoefC[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
													ref buffer, shift + 1892 + iOrder * 4) * 100;
				}

				// Uab
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.UHarmSummForOrderMore1AB = Conversions.bytes_2_int(ref buffer, shift + 2092) * uMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderValueAB[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 2096 + iOrder * 4) * uMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderCoefAB[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
														ref buffer, shift + 2296 + iOrder * 4) * 100;
				}
				// Ubc
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.UHarmSummForOrderMore1BC = Conversions.bytes_2_int(ref buffer, shift + 2496) * uMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderValueBC[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 2500 + iOrder * 4) * uMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderCoefBC[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
														ref buffer, shift + 2700 + iOrder * 4) * 100;
				}
				// Uca
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.UHarmSummForOrderMore1CA = Conversions.bytes_2_int(ref buffer, shift + 2900) * uMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderValueCA[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 2904 + iOrder * 4) * uMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UHarmOrderCoefCA[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
														ref buffer, shift + 3104 + iOrder * 4) * 100;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgVoltageHarmonics():");
				return false;
			}
		}

		private static bool GetAvgVoltageInterHarmonics(ref byte[] buffer, int shift, ref AvgRecordFields record,
													ref RegistrationInfo regInfo)
		{
			try
			{
				float uMultiplier = 1;
				if (regInfo.UtransformerEnable) 
					uMultiplier = EmService.GetUTransformerMultiplier(regInfo.UtransformerType);
				uMultiplier /= 1000000f;

				// Ua
				// Среднеквадратическое значение субгармонической группы
				record.UInterHarmAvgSquareA = Conversions.bytes_2_int(ref buffer, shift + 4920) * uMultiplier;
				// Среднеквадратическое значение интергармонических групп порядков 1…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UInterHarmAvgSquareOrderA[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 4924 + iOrder * 4) * uMultiplier;
				}
				// Ub
				record.UInterHarmAvgSquareB = Conversions.bytes_2_int(ref buffer, shift + 5124) * uMultiplier;
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UInterHarmAvgSquareOrderB[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 5128 + iOrder * 4) * uMultiplier;
				}
				// Uc
				record.UInterHarmAvgSquareC = Conversions.bytes_2_int(ref buffer, shift + 5328) * uMultiplier;
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UInterHarmAvgSquareOrderC[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 5332 + iOrder * 4) * uMultiplier;
				}

				// Uab
				// Среднеквадратическое значение субгармонической группы
				record.UInterHarmAvgSquareAB = Conversions.bytes_2_int(ref buffer, shift + 5532) * uMultiplier;
				// Среднеквадратическое значение интергармонических групп порядков 1…50
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UInterHarmAvgSquareOrderAB[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 5536 + iOrder * 4) * uMultiplier;
				}
				// Ubc
				record.UInterHarmAvgSquareBC = Conversions.bytes_2_int(ref buffer, shift + 5736) * uMultiplier;
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UInterHarmAvgSquareOrderBC[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 5740 + iOrder * 4) * uMultiplier;
				}
				// Uca
				record.UInterHarmAvgSquareCA = Conversions.bytes_2_int(ref buffer, shift + 5940) * uMultiplier;
				for (int iOrder = 0; iOrder < AvgRecordFields.CountHarmonisc; ++iOrder)
				{
					record.UInterHarmAvgSquareOrderCA[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 5944 + iOrder * 4) * uMultiplier;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgVoltageInterHarmonics():");
				return false;
			}
		}

		private static bool GetAvgCurrentHarmonics(ref byte[] buffer, int shift, ref AvgRecordFields record,
													ref RegistrationInfo regInfo)
		{
			try
			{
				float iMultiplier = regInfo.Ilimit;
				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					iMultiplier *= regInfo.ItransformerPrimary;
					iMultiplier /= regInfo.ItransformerSecondary;
				}
				iMultiplier /= 1000000f;

				// Ia
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.IHarmSummForOrderMore1A = Conversions.bytes_2_int(ref buffer, shift + 3304) * iMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderValueA[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 3308 + iOrder * 4) * iMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderCoefA[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
															ref buffer, shift + 3508 + iOrder * 4) * 100;
				}
				// Ib
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.IHarmSummForOrderMore1B = Conversions.bytes_2_int(ref buffer, shift + 3708) * iMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderValueB[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 3712 + iOrder * 4) * iMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderCoefB[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
														ref buffer, shift + 3912 + iOrder * 4) * 100;
				}
				// Ic
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.IHarmSummForOrderMore1C = Conversions.bytes_2_int(ref buffer, shift + 4112) * iMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderValueC[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 4116 + iOrder * 4) * iMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderCoefC[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
															ref buffer, shift + 4316 + iOrder * 4) * 100;
				}
				// In
				// Суммарное значение для гармонических подгрупп порядка > 1
				record.IHarmSummForOrderMore1N = Conversions.bytes_2_int(ref buffer, shift + 4516) * iMultiplier;
				// Значение для порядка = 1, Значения для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderValueN[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 4520 + iOrder * 4) * iMultiplier;
				}
				// Суммарный коэффициент, Коэффициенты для порядков 2…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IHarmOrderCoefN[iOrder] = Conversions.bytes_2_signed_float_Q_4_27_new(
															ref buffer, shift + 4720 + iOrder * 4) * 100;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgCurrentHarmonics():");
				return false;
			}
		}

		private static bool GetAvgCurrentInterHarmonics(ref byte[] buffer, int shift, ref AvgRecordFields record,
													ref RegistrationInfo regInfo)
		{
			try
			{
				float iMultiplier = regInfo.Ilimit;
				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					iMultiplier *= regInfo.ItransformerPrimary;
					iMultiplier /= regInfo.ItransformerSecondary;
				}
				iMultiplier /= 1000000f;

				// Ia
				// Среднеквадратическое значение субгармонической группы
				record.IInterHarmAvgSquareA = Conversions.bytes_2_int(ref buffer, shift + 6144) * iMultiplier;
				// Среднеквадратическое значение интергармонических групп порядков 1…50
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IInterHarmAvgSquareOrderA[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6148 + iOrder * 4) * iMultiplier;
				}
				// Ib
				record.IInterHarmAvgSquareB = Conversions.bytes_2_int(ref buffer, shift + 6348) * iMultiplier;
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IInterHarmAvgSquareOrderB[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6352 + iOrder * 4) * iMultiplier;
				}
				// Ic
				record.IInterHarmAvgSquareC = Conversions.bytes_2_int(ref buffer, shift + 6552) * iMultiplier;
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IInterHarmAvgSquareOrderC[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6556 + iOrder * 4) * iMultiplier;
				}
				// In
				record.IInterHarmAvgSquareN = Conversions.bytes_2_int(ref buffer, shift + 6756) * iMultiplier;
				for (int iOrder = 0; iOrder < 50; ++iOrder)
				{
					record.IInterHarmAvgSquareOrderN[iOrder] =
						Conversions.bytes_2_int(ref buffer, shift + 6760 + iOrder * 4) * iMultiplier;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgCurrentInterHarmonics():");
				return false;
			}
		}

		private static bool GetAvgHarmonicPower(ref byte[] buffer, int shift, ref AvgRecordFields record,
													ref RegistrationInfo regInfo)
		{
			try
			{
				float pMultiplier = regInfo.Ilimit;
				if (regInfo.UtransformerEnable)
					pMultiplier *= EmService.GetUTransformerMultiplier(regInfo.UtransformerType);

				if (regInfo.ItransformerUsage == 1 || regInfo.ItransformerUsage == 2)
				{
					pMultiplier *= regInfo.ItransformerPrimary;
					pMultiplier /= regInfo.ItransformerSecondary;
				}
				pMultiplier /= 1000000f;

				int harm_shift;

				if (regInfo.ConnectionScheme != ConnectScheme.Ph3W3 && 
					regInfo.ConnectionScheme != ConnectScheme.Ph3W3_B_calc)
				{
					harm_shift = shift + 6960;
					// 3 типа - это активная, реактивная и угол
					for (int iType = 0; iType < AvgRecordFields.CountHarmonicPowersType; ++iType)
					{
						for (int iHarm = 0; iHarm < AvgRecordFields.CountHarmonisc; ++iHarm)
						{
							record.HarmPowerA[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) record.HarmPowerA[iType, iHarm] *= pMultiplier;
							harm_shift += 4;
						}
					}

					if (regInfo.ConnectionScheme != ConnectScheme.Ph1W2)
					{
						harm_shift = shift + 7560;
						for (int iType = 0; iType < AvgRecordFields.CountHarmonicPowersType; ++iType)
						{
							for (int iHarm = 0; iHarm < AvgRecordFields.CountHarmonisc; ++iHarm)
							{
								record.HarmPowerB[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
								if (iType < 2) record.HarmPowerB[iType, iHarm] *= pMultiplier;
								harm_shift += 4;
							}
						}

						harm_shift = shift + 8160;
						for (int iType = 0; iType < AvgRecordFields.CountHarmonicPowersType; ++iType)
						{
							for (int iHarm = 0; iHarm < AvgRecordFields.CountHarmonisc; ++iHarm)
							{
								record.HarmPowerC[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
								if (iType < 2) record.HarmPowerC[iType, iHarm] *= pMultiplier;
								harm_shift += 4;
							}
						}
					}
				}
				else
				{
					harm_shift = shift + 8760;
					for (int iType = 0; iType < AvgRecordFields.CountHarmonicPowersType; ++iType)
					{
						for (int iHarm = 0; iHarm < AvgRecordFields.CountHarmonisc; ++iHarm)
						{
							record.HarmPower1[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) record.HarmPower1[iType, iHarm] *= pMultiplier;
							harm_shift += 4;
						}
					}

					harm_shift = shift + 9360;
					for (int iType = 0; iType < AvgRecordFields.CountHarmonicPowersType; ++iType)
					{
						for (int iHarm = 0; iHarm < AvgRecordFields.CountHarmonisc; ++iHarm)
						{
							record.HarmPower2[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) record.HarmPower2[iType, iHarm] *= pMultiplier;
							harm_shift += 4;
						}
					}
				}

				if (regInfo.ConnectionScheme != ConnectScheme.Ph1W2)
				{
					harm_shift = shift + 9960;
					for (int iType = 0; iType < AvgRecordFields.CountHarmonicPowersType; ++iType)
					{
						for (int iHarm = 0; iHarm < AvgRecordFields.CountHarmonisc; ++iHarm)
						{
							record.HarmPowerSUM[iType, iHarm] = Conversions.bytes_2_int(ref buffer, harm_shift);
							if (iType < 2) record.HarmPowerSUM[iType, iHarm] *= pMultiplier;
							harm_shift += 4;
						}
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAvgHarmonicPower():");
				return false;
			}
		}

		#endregion

		#region Service Methods

		public static float GetUTransformerMultiplier(short type)
		{
			switch (type)
			{
				case 0: return 1;
				case 1: return 30;
				case 2: return 31.5F;
				case 3: return 33;
				case 4: return 60;
				case 5: return 66;
				case 6: return 100;
				case 7: return 105;
				case 8: return 110;
				case 9: return 138;
				case 10: return 150;
				case 11: return 157.5F;
				case 12: return 180;
				case 13: return 200;
				case 14: return 240;
				case 15: return 270;
				case 16: return 350;
				case 17: return 1100;
				case 18: return 1500;
				case 19: return 2200;
				case 20: return 3300;
				case 21: return 5000;
				case 22: return 7500;
				case 23: return 8;
				case 24: return 330;
			}

			return 1;
		}

		#endregion
	}
}
