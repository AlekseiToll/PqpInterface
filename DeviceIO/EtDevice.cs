using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

using EmServiceLib;

namespace DeviceIO
{
	public abstract class EmDevice
	{
		#region Enums

		//protected enum QueryAvgType
		//{
		//	AAQ_TYPE_ReadStatus = 0,
		//	AAQ_TYPE_ResetAll = 1,
		//	AAQ_TYPE_Query = 2
		//}

		//protected enum QueryAvgCurStatus
		//{
		//	AAQ_STATE_Idle = 0,
		//	AAQ_STATE_Busy = 1
		//}

		#endregion

		#region Fields

		protected BackgroundWorker bw_ = null;
		protected DoWorkEventArgs e_;

		protected PortManager portManager_;

		protected EmPortType portType_;
		protected object[] portParams_;
		protected long serialNumber_;

		protected IntPtr hMainWnd_;

		// use only for Em32 and EtPQP
		protected byte[] pswdForWriting_ = null;
		protected byte[] time_for_writing_ = new byte[20];
		protected ushort[] hash_for_writing_ = new ushort[10];

		protected bool bCancelReading_ = false;

		#endregion

		#region Events

		/// <summary>Delegate of event OnStepReading</summary>
		//public delegate void StepReadingHandler();
		/// <summary>
		/// Событие OnStepReading происходит, когда считано заданное число станиц (число
		/// определено константой). Это информация для ProgressBar главного окна
		/// </summary>
		//public StepReadingHandler OnStepReading;

		#endregion

		#region Constructors

		protected EmDevice(EmPortType portType, object[] portParams, IntPtr hMainWnd)
		{
			portType_ = portType;
			portParams_ = portParams;
			hMainWnd_ = hMainWnd;
		}

		#endregion

		#region Public Methods

		/// <summary>Open device and get serial number</summary>
		/// <returns>serial number if successful; -1 if there was some error</returns>
		public abstract int OpenDevice();

		public bool Close()
		{
			return portManager_.ClosePort(true);
		}

		public abstract ExchangeResult ReadDeviceInfo();

		public abstract bool IsSomeArchiveExist();

		//public static ushort CalcCrc16(ref byte[] buffer, int start, int len)
		//{
		//    ushort temp, crc = CRC16_SEED;
		//    try
		//    {
		//        // последние два байта не рассматриваем, т.к. они зарезервированы для crc,
		//        // первый байт тоже пропускаем
		//        for (int i = start; i < (len + start - 2) && buffer.Length > len; ++i)
		//        {
		//            temp = (ushort)((buffer[i] ^ crc) & 0xFF);
		//            crc >>= 8;
		//            crc ^= CRC16Table[temp];
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.WriteToLogFailed("Error in CalcCrc16(): " + ex.Message);
		//        throw;
		//    }
		//    return crc;
		//}

		#endregion

		#region Protected Methods

		protected bool MakeIntList(ref object[] buf, out List<UInt32> list)
		{
			list = new List<UInt32>();
			if (buf == null) return true;

			for (int i = 0; i < buf.Length; ++i)
			{
				try
				{
					string s = buf[i].ToString();
					UInt32 num = UInt32.Parse(s);
					list.Add(num);
				}
				catch (InvalidCastException)
				{
					EmService.WriteToLogFailed("Error in MakeIntList: invalid cast!");
					return false;
				}
			}
			return true;
		}

		// for Em32 and EtPQP only!
		protected void CalcHashForWriting(ref byte[] time_buf, ref ushort[] hash_buf)
		{
			try
			{
				int i, j, k;
				ushort w0, w1;
				for (i = 0; i < 10; i++)
				{
					w0 = (ushort)((((ushort)pswdForWriting_[i]) & 0x000F) ^ 0xA5A5);

					for (j = 0; j < 20; j++)
					{
						w1 = time_buf[j];
						for (k = 0; k < 8; k++)
						{
							if (((w0 ^ w1) & 0x0001) != 0x0000)
								w0 = (ushort)((w0 >> 1) ^ ((((ushort)pswdForWriting_[i]) & 0x000F) ^ 0xA5A5));
							else
								w0 = (ushort)(w0 >> 1);
							w1 = (ushort)(w1 >> 1);
						}
					}
					hash_buf[i] = w0;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CalcHashForWriting:");
				throw;
			}
		}

		protected ushort PhaseToUshort(int phase, byte type)
		{
			ushort res = 0;
			switch (phase)
			{
				case 0: if (type == 0) res = 0x08; else res = 0x00; break;
				case 1: if (type == 0) res = 0x09; else res = 0x01; break;
				case 2: if (type == 0) res = 0x0A; else res = 0x02; break;
				case 3: if (type == 0) res = 0x0B; else res = 0x03; break;
				case 4: if (type == 0) res = 0x0C; else res = 0x04; break;
				case 5: if (type == 0) res = 0x0D; else res = 0x05; break;
				case 6: if (type == 0) res = 0x0E; else res = 0x06; break;
				case 7: if (type == 0) res = 0x0F; else res = 0x07; break;
			}
			return res;
		}

		#endregion

		#region Properties

		/// <summary>If user wants to cancel reading</summary>
		public bool BCancelReading
		{
			//get { return bCancelReading_; }
			set { bCancelReading_ = value; }
		}

		/// <summary>Port Type</summary>
		public EmPortType PortType
		{
			get { return this.portType_; }
			set { this.portType_ = value; }
		}

		/// <summary>Serial number</summary>
		public long SerialNumber
		{
			get { return this.serialNumber_; }
			set { this.serialNumber_ = value; }
		}

		#endregion
	}

	public class EtDevice : EmDevice
	{
		#region Constants and enums

		public const ushort CntPqpSegments = 256;
		public const uint PqpArchiveLength = 512 * 1024;
		public const ushort PqpSegmentLength = 2048;

		private const int RegistrationRecordLength = 2048;
		private const int CountObjNames = 11;

		#endregion

		#region Fields

		private DeviceCommonInfo devInfo_ = new DeviceCommonInfo();

		private static readonly int avgRecordLength_ = 16384;

		private string wifiProfileName_;
		private string wifiPassword_;

		#endregion

		#region Properties

		public DeviceCommonInfo DeviceInfo
		{
			get { return this.devInfo_; }
		}

		//public static int AvgRecordLength_PQP_A
		//{
		//	get { return avgRecordLength_; }
		//}

		#endregion

		#region Constructors

		public EtDevice(EmPortType portType, object[] port_params,
			string wifiProfileName, string wifiPassword,
			IntPtr hMainWnd,
			ref BackgroundWorker bw, ref DoWorkEventArgs e)
			: base(portType, port_params, hMainWnd)
		{
			byte[] tempPswd = { 0x08, 0x01, 0x02, 0x03, 0x02, 0x07, 0x02, 0x01, 0x01, 0x01 };
			pswdForWriting_ = new byte[tempPswd.Length];
			tempPswd.CopyTo(pswdForWriting_, 0);

			wifiProfileName_ = wifiProfileName;
			wifiPassword_ = wifiPassword;

			bw_ = bw;
			e_ = e;
		}

		#endregion

		#region Public Methods

		public override int OpenDevice()
		{
			try
			{
				if (portManager_ != null) portManager_.ClosePort(true);

				if (portType_ != EmPortType.WI_FI)
					portManager_ = new PortManager(hMainWnd_, portType_, ref portParams_);
				else portManager_ = new PortManager(hMainWnd_, portType_, wifiProfileName_, wifiPassword_, ref portParams_);

				if (!portManager_.CreatePort()) return -1;

				if (!portManager_.OpenPort()) return -1;

				// для работы с EtPQP сначала на всякий случай посылаем команду - сброс обработки
				// запроса усредненных. эти запросы долго обрабатываются и могут приходить пакеты
				// от прошлых запросов
				//ResetAllAvgQuery();

				long ser;
				if (!ReadDeviceSerialNumber(out ser))
				{
					portManager_.ClosePort(true);
					return -1;
				}
				//Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 2, 0, 0);
				bw_.ReportProgress(1);
				return (int)ser;
			}
			catch (EmDisconnectException)
			{
				portManager_.ClosePort(true);
				return -1;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Open EtPQP-A device:");
				throw;
			}
		}

		public override bool IsSomeArchiveExist()
		{
			return (devInfo_.Content != null && devInfo_.Content.Count > 0);
		}

		public override ExchangeResult ReadDeviceInfo()
		{
			byte[] buffer = null;
			byte[] bufferMainRecord = null;

			// creating DeviceCommonInfo object
			devInfo_ = new DeviceCommonInfo();
			devInfo_.SerialNumber = serialNumber_;
			devInfo_.DevVersion = "0.0";

			try
			{
				// Чтение списка абсолютных индексов доступных Регистраций 
				if (Read(EmCommands.COMMAND_ReadRegistrationIndices, ref buffer, null) != 0)
				{
					EmService.WriteToLogFailed("COMMAND_ReadRegistrationIndices failed 1");
					return ExchangeResult.OTHER_ERROR;
				}
				if (buffer == null || buffer.Length < 4)	// 4 bytes is the length of 1 index
				{
					Thread.Sleep(1000);
					if (Read(EmCommands.COMMAND_ReadRegistrationIndices, ref buffer, null) != 0)
					{
						EmService.WriteToLogFailed("COMMAND_ReadRegistrationIndices failed 2");
						return ExchangeResult.OTHER_ERROR;
					}
					if (buffer == null || buffer.Length < 4)
						throw new EmDeviceEmptyException();
				}

				// parse buffer
				int regCount = buffer.Length / 4;
				ContentsLine[] mainRecords = new ContentsLine[regCount];

				UInt32[] regIndexes = new UInt32[regCount];
				for (int iInd = 0; iInd < regCount; ++iInd)
				{
					regIndexes[iInd] = Conversions.bytes_2_uint_new(ref buffer, iInd * 4);
				}

				// переменная указывает на архивы, сделанные со старой прошивкой
				// если есть старые и новые архивы, то старые игнорируем. если есть
				// только старые, то выдаем сообщение о необходимости перепрошить прибор
				bool oldArchiveExists = false;

				for (int iReg = 0; iReg < regCount; ++iReg)
				{
					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return ExchangeResult.CANCELLED;
					}

					mainRecords[iReg] = new ContentsLine();
					mainRecords[iReg].RegistrationId = regIndexes[iReg];

					if (Read(EmCommands.COMMAND_ReadRegistrationByIndex, ref bufferMainRecord,
								new object[] { mainRecords[iReg].RegistrationId }) != 0)
					{
						EmService.WriteToLogFailed("COMMAND_ReadRegistrationByIndex failed");
						return ExchangeResult.OTHER_ERROR;
					}
					if (bufferMainRecord.Length < RegistrationRecordLength)
					{
						EmService.WriteToLogFailed("Error: reg buffer.Length = " +
							bufferMainRecord.Length.ToString());
						EmService.WriteToLogFailed("Registration number = " + iReg.ToString());
						return ExchangeResult.OTHER_ERROR;
					}

					// проверяем не слишком ли старая прошивка у прибора
					ushort ver_num = Conversions.bytes_2_ushort(ref bufferMainRecord, 122);
					if (ver_num < 1)
					{
						oldArchiveExists = true;
						EmService.WriteToLogFailed("Device version: " + ver_num.ToString());
						EmService.WriteToLogFailed(string.Format("Device version bytes: {0}, {1}",
															bufferMainRecord[122], bufferMainRecord[123]));
						//throw new EmDeviceOldVersionException();
						continue;
					}

					// begin
					ushort zone;
					mainRecords[iReg].CommonBegin =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref bufferMainRecord, 8,
											"Start date for main record", out zone);
					//mainRecords[iReg].TimeZone = zone;
					// end
					mainRecords[iReg].CommonEnd =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref bufferMainRecord, 36,
											"End date for main record");

					// object name
					mainRecords[iReg].RegistrationName = Conversions.bytes_2_string(ref bufferMainRecord, 64, 16);
					if (mainRecords[iReg].RegistrationName == "")
						mainRecords[iReg].RegistrationName = "default object";

					// connection scheme
					//ushort conScheme = Conversions.bytes_2_ushort(ref bufferMainRecord, 80);
					//switch (conScheme)
					//{
					//    case 0: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph1W2; break;
					//    case 1: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph3W4; break;
					//    case 2: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph3W3; break;
					//    case 3: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph3W3_B_calc; break;
					//    default: mainRecords[iReg].ConnectionScheme = ConnectScheme.Unknown; break;
					//}


					// constraints /////////////////////////////////
					//for (int iConstr = 0; iConstr < EtPQPAConstraints.CntConstraints;
					//    ++iConstr)
					//{
					//    mainRecords[iReg].Constraints[iConstr] =
					//        Conversions.bytes_2_signed_float_Q_15_16_new(ref bufferMainRecord, 128 + iConstr * 4);
					//}

					// constraint_type
					//mainRecords[iReg].ConstraintType = Conversions.bytes_2_short(ref bufferMainRecord, 108);

					#region PQP

					// the length of pqp archives
					mainRecords[iReg].PqpLength = Conversions.bytes_2_short(ref bufferMainRecord, 110);
					// count of pqp archives
					mainRecords[iReg].PqpCnt = Conversions.bytes_2_int(ref bufferMainRecord, 118);
					// pqp archive indexes
					for (int iPqp = 0; iPqp < 64; ++iPqp)
					{
						uint index = Conversions.bytes_2_uint_new(ref bufferMainRecord, 944 + iPqp * 4);
						if (index == (UInt32)0xFFFFFFFF) continue;

						mainRecords[iReg].PqpSet.Add(new PqpSet(index,
							mainRecords[iReg].RegistrationId));
						EmService.WriteToLogGeneral("PqP Index: " + index.ToString());
					}
					if (mainRecords[iReg].PqpCnt > mainRecords[iReg].PqpSet.Count)
					{
						EmService.WriteToLogFailed("PqpCount = " +
							mainRecords[iReg].PqpCnt.ToString() +
							" but PqpSet.Count = " + mainRecords[iReg].PqpSet.Count.ToString());
						mainRecords[iReg].PqpCnt = (ushort)mainRecords[iReg].PqpSet.Count;
					}

					// read the first segment of each pqp archive to get its start date
					for (int iPqpDate = 0; iPqpDate < mainRecords[iReg].PqpSet.Count; ++iPqpDate)
					{
						if (Read(EmCommands.COMMAND_ReadRegistrationArchiveByIndex, ref buffer,
							new object[] { mainRecords[iReg].PqpSet[iPqpDate].PqpIndex, 0 }) != 0)
						{
							EmService.WriteToLogFailed(
								"COMMAND_ReadRegistrationArchiveByIndex failed  " +
								iPqpDate.ToString());
							mainRecords[iReg].PqpSet.RemoveAt(iPqpDate);
							--iPqpDate;
							continue;
							//return -1;
						}
						if (buffer == null || buffer.Length < (EtDevice.PqpSegmentLength + 6))
						{
							mainRecords[iReg].PqpSet.RemoveAt(iPqpDate);
							--iPqpDate;
							continue;
						}

						// убираем первые 6 байт, в которых archive_id и номер сегмента
						byte[] buffer_old = buffer;
						buffer = new byte[EtDevice.PqpSegmentLength];
						Array.Copy(buffer_old, 6, buffer, 0, EtDevice.PqpSegmentLength);

						PqpSet curPQP = mainRecords[iReg].PqpSet[iPqpDate];
						curPQP.PqpStart = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 12, "PQP Start date");
						curPQP.PqpEnd = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 84, "PQP End date");
						mainRecords[iReg].PqpSet[iPqpDate] = curPQP;

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return ExchangeResult.CANCELLED;
						}
					}

					#endregion

					#region AVG info

					//mainRecords[iReg].AvgIndexStart3sec =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1192*/1200);
					//mainRecords[iReg].AvgCnt3sec =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1196*/1204);

					//// еще не прошли цикл
					//// UPD: про цикл закомментировано, т.к. Боря сказал, что пока мы живы до значения 0xFFFFFFFF
					//// дело не успеет дойти
					////if ((0xFFFFFFFF - mainRecords[iReg].AvgIndexStart3sec) > mainRecords[iReg].AvgCnt3sec)
					//mainRecords[iReg].AvgIndexEnd3sec =
					//        mainRecords[iReg].AvgIndexStart3sec + mainRecords[iReg].AvgCnt3sec - 1;
					////else // прошли цикл, нумерация началась сначала
					////{
					////    EmService.WriteToLogDebug("AVG new circle!!!");
					////    mainRecords[iReg].AvgIndexEnd3sec =
					////        mainRecords[iReg].AvgCnt3sec - (0xFFFFFFFF - mainRecords[iReg].AvgIndexStart3sec);
					////}

					//mainRecords[iReg].AvgIndexStart10min =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1200*/1208);
					//mainRecords[iReg].AvgCnt10min =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1204*/1212);
					//mainRecords[iReg].AvgIndexEnd10min =
					//        mainRecords[iReg].AvgIndexStart10min + mainRecords[iReg].AvgCnt10min - 1;

					//mainRecords[iReg].AvgIndexStart2hour =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1208*/1216);
					//mainRecords[iReg].AvgCnt2hour =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1212*/1220);
					//mainRecords[iReg].AvgIndexEnd2hour =
					//        mainRecords[iReg].AvgIndexStart2hour + mainRecords[iReg].AvgCnt2hour - 1;

					// временный массив, чтобы можно было 3 типа усредненных считывать в цикле
					EmCommands[] avgCommandsDate = new EmCommands[] { EmCommands.COMMAND_ReadAverageArchive3SecByIndex,
					    EmCommands.COMMAND_ReadAverageArchive10MinByIndex,
					    EmCommands.COMMAND_ReadAverageArchive2HourByIndex };
					EmCommands[] avgCommandsRealIndex = new EmCommands[] { 
						EmCommands.COMMAND_ReadAverageArchive3SecMinMaxIndices,
					    EmCommands.COMMAND_ReadAverageArchive10MinMinMaxIndices,
					    EmCommands.COMMAND_ReadAverageArchive2HourMinMaxIndices };
					//UInt32[] indexesStart = new UInt32[] { mainRecords[iReg].AvgIndexStart3sec,
					//    mainRecords[iReg].AvgIndexStart10min, mainRecords[iReg].AvgIndexStart2hour };
					//UInt32[] indexesEnd = new UInt32[] { mainRecords[iReg].AvgIndexEnd3sec,
					//    mainRecords[iReg].AvgIndexEnd10min, mainRecords[iReg].AvgIndexEnd2hour };
					//UInt32[] cntAvg = new UInt32[] { mainRecords[iReg].AvgCnt3sec,
					//    mainRecords[iReg].AvgCnt10min, mainRecords[iReg].AvgCnt2hour };

					for (int iAvgType = 0; iAvgType < 3; ++iAvgType)
					{
						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return ExchangeResult.CANCELLED;
						}

						// сначала узнаем стартовый и конечный фактические индексы
						if (Read(avgCommandsRealIndex[iAvgType], ref buffer,
										new object[] { mainRecords[iReg].RegistrationId }) != 0)
						{
							EmService.WriteToLogFailed(
								"ReadDevInfo:COMMAND_ReadAverageArchiveXXXMinMaxIndices failed1! " +
								iAvgType.ToString());
							if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
							else EmService.WriteToLogFailed("buffer = null");
							continue;
						}

						if (buffer == null || buffer.Length < 120)
						{
							EmService.WriteToLogFailed(
								"ReadDevInfo:COMMAND_ReadAverageArchiveXXXMinMaxIndices failed2!  " +
								iAvgType.ToString());
							if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
							else EmService.WriteToLogFailed("buffer = null");
							continue;
						}

						// стартовый индекс
						UInt32 curIndex = Conversions.bytes_2_uint_new(ref buffer, 0);

						DateTime start_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 4, "AVG record start date1");
						DateTime end_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 32, "AVG record end date1");

						mainRecords[iReg].AvgDataStart[iAvgType] = new AVGData(curIndex,
							start_datetime, end_datetime);

						// конечный индекс
						curIndex = Conversions.bytes_2_uint_new(ref buffer, 60);

						start_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 64, "AVG record start date2");
						end_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 92, "AVG record end date2");

						mainRecords[iReg].AvgDataEnd[iAvgType] = new AVGData(curIndex,
							start_datetime, end_datetime);

						//if (cntAvg[iAvgType] == 0) continue;

						//// start
						//if (Read(avgCommandsDate[iAvgType], ref buffer,
						//                new object[] { indexesStart[iAvgType] }) != 0)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed1! " +
						//        iAvgType.ToString());
						//    EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    continue;
						//}
						//if (buffer == null || buffer.Length < avgRecordLength_)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed2!  " +
						//        iAvgType.ToString());
						//    if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    else EmService.WriteToLogFailed("buffer = null");
						//    continue;
						//}

						//DateTime start_datetime =
						//    Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 12, "AVG record start date");
						//DateTime end_datetime =
						//    Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 40, "AVG record end date");
						//mainRecords[iReg].AvgDataStart[iAvgType] = new AVGDataEtPQP_A(indexesStart[iAvgType],
						//    start_datetime, end_datetime);
						//UInt32 curIndex = Conversions.bytes_2_uint_new(ref buffer, 0);
						//if (curIndex != indexesStart[iAvgType])
						//{
						//    EmService.WriteToLogFailed(string.Format("ReadDevInfo error 111: {0}, {1}",
						//        indexesStart[iAvgType], curIndex));
						//}

						//// end
						//if (Read(avgCommandsDate[iAvgType], ref buffer, new object[] { indexesEnd[iAvgType] }) != 0)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed3! " +
						//        iAvgType.ToString());
						//    EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    continue;
						//}
						//if (buffer == null || buffer.Length < avgRecordLength_)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed4!  " +
						//        iAvgType.ToString());
						//    if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    else EmService.WriteToLogFailed("buffer = null");
						//    continue;
						//}

						//start_datetime = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 12, 
						//    "AVG record start date");
						//end_datetime = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 40, "AVG record end date");
						//mainRecords[iReg].AvgDataEnd[iAvgType] = new AVGDataEtPQP_A(indexesEnd[iAvgType],
						//    start_datetime, end_datetime);
						//curIndex = Conversions.bytes_2_uint_new(ref buffer, 0);
						//if (curIndex != indexesEnd[iAvgType])
						//{
						//    EmService.WriteToLogFailed(string.Format("ReadDevInfo error 222: {0}, {1}",
						//        indexesEnd[iAvgType], curIndex));
						//}
					}

					if (mainRecords[iReg].AvgDataStart.Length >= 3 && mainRecords[iReg].AvgDataEnd.Length >= 3)
					{
						if ((mainRecords[iReg].AvgDataStart[0].dtStart != DateTime.MinValue &&
							mainRecords[iReg].AvgDataEnd[0].dtStart != DateTime.MinValue) ||
							(mainRecords[iReg].AvgDataStart[1].dtStart != DateTime.MinValue &&
							mainRecords[iReg].AvgDataEnd[1].dtStart != DateTime.MinValue) ||
							(mainRecords[iReg].AvgDataStart[2].dtStart != DateTime.MinValue &&
							mainRecords[iReg].AvgDataEnd[2].dtStart != DateTime.MinValue))
							mainRecords[iReg].AvgExists = true;
					}

					#endregion

					devInfo_.Content.AddRecord(mainRecords[iReg]);

					bw_.ReportProgress(iReg * 100 / regCount);
				}

				if (oldArchiveExists && devInfo_.Content.Count == 0)
				{
					throw new EmDeviceOldVersionException();
				}

				// device version
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)364 }) != 0)
					return ExchangeResult.OTHER_ERROR;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read Device version!");
				}
				else
				{
					uint verNum = Conversions.bytes_2_uint_new(ref buffer, 0);
					string sVernum = verNum.ToString();
					devInfo_.DevVersion = sVernum;
					try
					{
						devInfo_.DevVersionDate = new DateTime(
							Int32.Parse(sVernum.Substring(sVernum.Length - 6, 2)) + 2000,
							Int32.Parse(sVernum.Substring(sVernum.Length - 4, 2)),
							Int32.Parse(sVernum.Substring(sVernum.Length - 2, 2)));
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Error getting device version date!");
						devInfo_.DevVersionDate = DateTime.MinValue;
					}

					//if (dtBuildDate < new DateTime(2011, 1, 15))
					//{
					//    throw new EmDeviceOldVersionException();
					//}
				}
				// end of device version
			}
			catch (System.Threading.ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in ReadDeviceInfo()");
				Thread.ResetAbort();
				return ExchangeResult.OTHER_ERROR;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDeviceInfo()");
				throw;
			}
			return ExchangeResult.OK;
		}

		public ExchangeResult ReadObjectNames(ref string[] names)
		{
			try
			{
				byte[] buffer = null;
				ExchangeResult errCode = ExchangeResult.OTHER_ERROR;
				names = new string[CountObjNames];

				int curParamNumber = 56;
				for (int iName = 0; iName < CountObjNames; ++iName)
				{
					errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
						new object[] { (ushort)curParamNumber++ });
					if (errCode != ExchangeResult.OK)
						return errCode;
					names[iName] = Conversions.bytes_2_string(ref buffer, 0, 16);
				}
				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadObjectNames:");
				throw;
			}
		}

		/// <summary>
		/// Reading device serial number
		/// </summary>
		public bool ReadDeviceSerialNumber(out long serialNumber)
		{
			serialNumber = -1;

			byte[] buffer = null;

			try
			{
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
							new object[] { (ushort)0 }) != 0) return false;

				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read serial number!");
					serialNumber = -1;
					return false;
				}
				serialNumber = (long)(Conversions.bytes_2_ushort(ref buffer, 0));
				EmService.WriteToLogGeneral("SERIAL NUMBER = " + serialNumber);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDeviceSerialNumber():");
				serialNumber = -1;
				return false;
			}
			finally
			{
				serialNumber_ = serialNumber;
			}
		}

		public bool ReadTime(ref byte[] buffer)
		{
			buffer = null;

			try
			{
				if (Read(EmCommands.COMMAND_ReadTime, ref buffer, null) != 0) return false;

				if (buffer == null || buffer.Length < 20)
				{
					EmService.WriteToLogFailed("Unable to read device time!");
					buffer = null;
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ReadTime(): " + ex.Message);
				buffer = null;
				throw;
			}
		}

		public ExchangeResult Read(EmCommands command, ref byte[] buffer, object[] request_params)
		{
			return Read(command, ref buffer, request_params, 3);
		}

		public ExchangeResult Read(EmCommands command, ref byte[] buffer, object[] request_params, int attempts)
		{
			try
			{
				buffer = null;
				int num = 0;
				ExchangeResult res = ExchangeResult.OTHER_ERROR;

				List<UInt32> listParams;
				MakeIntList(ref request_params, out listParams);

				//List<byte> listBuffer = null;
				while (++num <= attempts)
				{
					if (bCancelReading_) throw new EmDisconnectException();

					//listBuffer = new List<byte>();
					//res = portA_.ReadData(command, listBuffer, listParams);
					res = portManager_.ReadData(command, ref buffer, listParams);

					if (res != 0)
					{
						EmService.WriteToLogFailed("Reading error. Attempt " + num);
						Thread.Sleep(1000);
					}
					if (res == ExchangeResult.OK)  // если успешно
						break;
					if (res == ExchangeResult.DISCONNECT)  // если был дисконнект
					{
						portManager_.ClosePort(true);

						if (bCancelReading_)
							throw new EmDisconnectException();
						else
						{
							EmService.WriteToLogFailed("Error reading (EtPQP-A): OpenFast");
							if (!portManager_.OpenFast(true)) throw new EmDisconnectException();
							Thread.Sleep(1000);
						}
					}

					if (num >= 2)
					{
						if (portType_ == EmPortType.USB)
						{
							RestartUSB();
						}
						portManager_.OpenFast(false);
					}
				}
				//if (listBuffer != null && listBuffer.Count > 0)
				//{
				//    buffer = new byte[listBuffer.Count];
				//    listBuffer.CopyTo(buffer);
				//}
				return res;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDevice::Read():");
				throw;
			}
		}

		/// <summary>Write Constraints To the Device</summary>
		public ExchangeResult WriteSets(ref byte[] buffer, Int32 checkSum1, Int32 checkSum2)
		{
			try
			{
				//if (buffer == null) return ExchangeResult.Other_Error;

				////List<byte> tempList = null;
				//int shift = 1600;	//госты писать не надо! их пропускаем
				//byte[] tempBuf = new byte[6];   // param (2) + data (4)
				//int param_num = 164;

				//for (int iConstr = 0; iConstr < (EtPQPAConstraints.CntConstraints * 2); ++iConstr)
				//{
				//    // эта чехарда с номерами - оттого, что в приборе пришлось добавить 2
				//    // уставки для напряжения и теперь в приборе они идут не подряд
				//    if (iConstr == 10) { param_num = 386; }
				//    if (iConstr == 11) { param_num = 387; }
				//    if (iConstr == 12) { param_num = 174; }
				//    if (iConstr == 110) { param_num = 388; }
				//    if (iConstr == 111) { param_num = 389; }
				//    if (iConstr == 112) { param_num = 273; }

				//    tempBuf[0] = (byte)(param_num & 0xFF);
				//    tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
				//    Array.Copy(buffer, shift, tempBuf, 2, 4);
				//    //tempList = new List<byte>(tempBuf);
				//    if (portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempBuf) != 0)
				//    {
				//        EmService.WriteToLogFailed(
				//            "Error while writing constraints Et-PQP-A: i = " + iConstr.ToString());
				//        return ExchangeResult.Write_Error;
				//    }

				//    shift += 4;
				//    ++param_num;

				//    if (param_num == 262)		// check sum for user1
				//    {
				//        tempBuf[0] = (byte)(param_num & 0xFF);
				//        tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
				//        Conversions.int_2_bytes(checkSum1, ref tempBuf, 2);
				//        //tempList = new List<byte>(tempBuf);
				//        if (portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempBuf) != 0)
				//        {
				//            EmService.WriteToLogFailed(
				//                "Error while writing constraints Et-PQP-A: checkSum1");
				//            return ExchangeResult.Write_Error;
				//        }

				//        ++param_num;
				//    }

				//    if (param_num == 361)		// check sum for user2
				//    {
				//        tempBuf[0] = (byte)(param_num & 0xFF);
				//        tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
				//        Conversions.int_2_bytes(checkSum2, ref tempBuf, 2);
				//        //tempList = new List<byte>(tempBuf);
				//        if (portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempBuf) != 0)
				//        {
				//            EmService.WriteToLogFailed(
				//                "Error while writing constraints Et-PQP-A: checkSum2");
				//            return ExchangeResult.Write_Error;
				//        }

				//        ++param_num;
				//    }
				//}

				return ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDevice::WriteSets():");
				throw;
			}
		}

		public ExchangeResult WriteObjectNames(ref byte[] buffer)
		{
			try
			{
				//byte[] tempBuf;
				//int oneNameLen = 16;
				//int curParamNumber = 56;
				//int curShift = 0;
				//for (int iName = 0; iName < COUNT_OBJ_NAMES; ++iName)
				//{
				//    tempBuf = new byte[oneNameLen];
				//    Array.Copy(buffer, curShift, tempBuf, 0, tempBuf.Length);
				//    // вместо нулей вставляем пробелы, а то прибор выдает BAD_DATA
				//    for (int iByte = 0; iByte < tempBuf.Length; ++iByte)
				//        if (tempBuf[iByte] == 0) tempBuf[iByte] = 32;

				//    // write to device
				//    ExchangeResult res = WriteSystemData(curParamNumber++, ref tempBuf);
				//    if (res != ExchangeResult.OK) return res;

				//    curShift += oneNameLen;
				//}

				return ExchangeResult.OK;
			}
			catch (ArgumentOutOfRangeException aex)
			{
				EmService.DumpException(aex, "ArgumentOutOfRangeException in WriteObjectNames():");
				return ExchangeResult.OTHER_ERROR;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDevice::WriteObjectNames():");
				throw;
			}
		}

		/// <summary>Write System Data To the Device</summary>
		/// <param name="param">Number of Sytem Parameter To Write</param>
		/// <param name="buffer">Value of this Parameter in Bytes</param>
		public ExchangeResult WriteSystemData(int param, ref byte[] buffer)
		{
			try
			{
				byte[] time_buf = new byte[20];
				if (!ReadTime(ref time_buf))
					return ExchangeResult.OTHER_ERROR;
				ushort[] hash_buf = new ushort[10];
				CalcHashForWriting(ref time_buf, ref hash_buf);
				byte[] buffer_to_write = new byte[66 + buffer.Length];
				for (int iBuf = 0; iBuf < 66 + buffer.Length; ++iBuf) buffer_to_write[iBuf] = 0;
				buffer_to_write[0] = (byte)(param & 0xFF);
				buffer_to_write[1] = (byte)((param >> 8) & 0xFF);
				Array.Copy(buffer, 0, buffer_to_write, 2, buffer.Length);
				EmService.CopyUShortArrayToByteArray(ref hash_buf, ref buffer_to_write,
					2 + buffer.Length);

				//List<byte> tempList = new List<byte>(buffer_to_write);
				//return ((EtPqpAUSB)portA_).WriteData(COMMAND_WriteSystemData, tempList);
				return portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref buffer_to_write);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDevice::Write():");
				throw;
			}
		}

		public void RestartUSB()
		{
			try
			{
				EmService.WriteToLogGeneral("!!!!!!!!!!! Restart USB !!!!!!!!!!!!!");

				//byte[] time_buf = new byte[20];
				//if (!ReadTime(ref time_buf))
				//{
				//    EmService.WriteToLogFailed("Error in RestartUSB 1");
				//    return;
				//}
				//ushort[] hash_buf = new ushort[10];
				//CalcHashForWriting(ref time_buf, ref hash_buf);
				//byte[] buffer_to_write = new byte[66];
				//for (int iBuf = 0; iBuf < 66; ++iBuf) buffer_to_write[iBuf] = 0;
				//EmService.CopyUShortArrayToByteArray(ref hash_buf, ref buffer_to_write, 0);

				//List<byte> tempList = new List<byte>(buffer_to_write);
				//return ((EtPqpAUSB)portA_).WriteData(COMMAND_WriteSystemData, tempList);

				byte[] buffer = null;
				portManager_.WriteData(EmCommands.COMMAND_RestartInterface, ref buffer);
				Thread.Sleep(3000);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtDevice::RestartUSB():");
				throw;
			}
		}

		#endregion
	}
}
