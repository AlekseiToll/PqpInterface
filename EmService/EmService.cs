using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Resources;

// ���������� ���� �� ������������

// NRM = 95% = ��� (NPL)
// MAX = 100% = ��� (UPL)

namespace EmServiceLib
{
	public enum DurationDipSwell
	{
		T_0_01_till_0_2 = 0,	// t = time
		T_0_2_till_0_5 = 1,
		T_0_5_till_1 = 2,
		T_1_till_5 = 3,
		T_5_till_20 = 4,
		T_20_till_60 = 5
	}

	public enum DurationInterrupt
	{
		T_0_till_0_5 = 0,
		T_0_5_till_1 = 1,
		T_1_till_5 = 2,
		T_5_till_20 = 3,
		T_20_till_60 = 4,
		T_60_till_180 = 5,
		T_Over_180 = 6
	}

	public enum DeviationDipSwell
	{
		US_110_till_120 = 0,	// us = voltage swell
		US_120_till_140 = 1,
		US_140_till_160 = 2,
		US_160_till_180 = 3,
		UD_90_till_85 = 4,		// ud = voltage dip
		UD_85_till_70 = 5,
		UD_70_till_40 = 6,
		UD_40_till_10 = 7,
		UD_10_till_0 = 8
	}

	public enum ExchangeResult
	{
		OK = 0,
		CRC_ERROR = 1,
		READ_ERROR = 2,
		WRITE_ERROR = 3,
		SYNCHROBYTE_ERROR = 4,
		NORMALBYTE_ERROR = 5,
		OTHER_ERROR = 6,
		PARSE_ERROR = 7,
		DISCONNECT = 8,
		TIMEOUT = 9,
		CANCELLED = 10,
		NONE = 11,
		EXCEPTION = 12,
		DEVICE_EMPTY_EXCEPTION = 13,
		DEVICE_OLD_VERSION_EXCEPTION = 14
	}

	public enum EmCommands
	{
		// common
		COMMAND_OK = 0x1000,
		COMMAND_UNKNOWN_COMMAND = 0x1001,
		COMMAND_CRC_ERROR = 0x1002,
		COMMAND_ECHO = 0x0000,
		COMMAND_BAD_DATA = 0x1003,
		COMMAND_BAD_PASSWORD = 0x1004,
		COMMAND_ACCESS_ERROR = 0x1005,
		COMMAND_CHECK_FAILED = 0x1006,
		COMMAND_NO_DATA = 0x1007,

		// other
		COMMAND_ReadTime = 0x0001,
		COMMAND_ReadEventLogger = 0x0012,

		// sets
		COMMAND_ReadSets_PQP_A = 0x4008,
		COMMAND_WriteSets = 0x0008,
		COMMAND_ReadSets_PQP = 0x0007,

		// system
		COMMAND_ReadSystemData = 0x000B,
		COMMAND_WriteSystemData = 0x000C,
		COMMAND_RestartInterface = 0x0003,

		// Et-PQP-A
		COMMAND_ReadDSIArchivesByRegistration = 0x400E,
		COMMAND_ReadDSIArchives = 0x4007,
		COMMAND_ReadRegistrationIndices = 0x4009,
		COMMAND_ReadRegistrationByIndex = 0x400A,
		COMMAND_ReadRegistrationArchiveByIndex = 0x400D,

		// avg
		COMMAND_ReadAverageArchive3SecIndices = 0x4013,
		COMMAND_ReadAverageArchive10MinIndices = 0x4014,
		COMMAND_ReadAverageArchive2HourIndices = 0x4015,
		COMMAND_ReadAverageArchive3SecByIndex = 0x4010,
		COMMAND_ReadAverageArchive10MinByIndex = 0x4011,
		COMMAND_ReadAverageArchive2HourByIndex = 0x4012,
		COMMAND_ReadAverageArchive3SecIndexByDateTime = 0x4016,
		COMMAND_ReadAverageArchive10MinIndexByDateTime = 0x4017,
		COMMAND_ReadAverageArchive2HourIndexByDateTime = 0x4018,
		COMMAND_ReadAverageArchive3SecMinMaxIndices = 0x4019,
		COMMAND_ReadAverageArchive10MinMinMaxIndices = 0x401A,
		COMMAND_ReadAverageArchive2HourMinMaxIndices = 0x401B,
		COMMAND_Read3secValues = 0x000D,
		COMMAND_Read1minValues = 0x000E,
		COMMAND_Read30minValues = 0x000F,
		COMMAND_Read3secArchiveByTimestampObjectDemand = 0x0026,
		COMMAND_Read1minArchiveByTimestampObjectDemand = 0x0027,
		COMMAND_Read30minArchiveByTimestampObjectDemand = 0x0028,
		COMMAND_AverageArchiveQuery = 0x0006,//???????????????? delete unused commands
		COMMAND_Read3secArchiveByTimestamp = 0x0013,
		COMMAND_Read1minArchiveByTimestamp = 0x0014,
		COMMAND_Read30minArchiveByTimestamp = 0x0015,
		COMMAND_ReadEarliestAndLatestAverageTimestamp = 0x001E,
		COMMAND_ReadMeasurements3Sec = 0xA000,
		COMMAND_ReadMeasurements10Min = 0xA001,
		COMMAND_ReadMeasurements2Hour = 0xA002,

		// pqp
		COMMAND_ReadQualityDatesByObject = 0x001F,
		COMMAND_ReadQualityDates = 0x0009,
		COMMAND_ReadQualityContents = 0x0032,
		COMMAND_ReadQualityEntryByTimestampByObject = 0x0033,
		COMMAND_ReadQualityVFArraysByTimestampByObject = 0x0036,
		COMMAND_ReadQualityEntry = 0x000A,

		// dip
		COMMAND_ReadDipSwellArchiveByObject = 0x0021,
		COMMAND_ReadDipSwellStatus = 0x0010,
		COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject = 0x0035,
		COMMAND_ReadDipSwellIndexByStartTimestamp = 0x001A,
		COMMAND_ReadDipSwellIndexByEndTimestamp = 0x001B,
		COMMAND_ReadDipSwellArchive = 0x0019,
		COMMAND_ReadEarliestAndLatestDipSwellTimestamp = 0x001C,
	}

	public enum Phase
	{
		A = 0,
		B = 1,
		C = 2,
		AB = 3,
		BC = 4,
		CA = 5,
		ABCN = 6,
		ABC = 7
	}

	public class PhasesInfo
	{
		public enum Phase6
		{
			A = 0,
			B = 1,
			C = 2,
			AB = 3,
			BC = 4,
			CA = 5
		}

		public enum Phase3
		{
			A = 0,
			B = 1,
			C = 2
		}

		public static string GetPhase6Name(int index) { return ((Phase6)index).ToString(); }

		//public static Color GetPhase6Color(int index)
		//{
		//    switch (index)
		//    {
		//        case 0:
		//        case 3: return DataGridColors.ColorAvgPhaseA;
		//        case 1:
		//        case 4: return DataGridColors.ColorAvgPhaseB;
		//        case 2:
		//        case 5: return DataGridColors.ColorAvgPhaseC;
		//    }
		//    return DataGridColors.ColorCommon;
		//}
	}

	public enum ConnectScheme
	{
		Unknown = 0,
		Ph3W4 = 1,
		Ph3W3 = 2,
		Ph1W2 = 3,
		Ph3W3_B_calc = 4
		//Ph3W4_B_calc = 5
	}

	public enum PQPProtocolType
	{
		VERSION_1 = 0,
		VERSION_2 = 1,
		VERSION_3 = 2
	}

	public enum AvgPages
	{
		F_U_I = 0,
		POWER = 1,
		ANGLES = 2,
		PQP = 3,
		I_HARMONICS = 4,
		U_PH_HARMONICS = 5,
		U_LIN_HARMONICS = 6,
		HARMONIC_POWERS = 7,
		HARMONIC_ANGLES = 8,
		U_PH_INTERHARM = 9,
		U_LIN_INTERHARM = 10,
		I_INTERHARM = 11
		//COMPLEX = 12
	}

	public enum ArchiveType { PQP = 0, AVG = 1, DNS = 2, ALL = 3 }

	public enum AvgTypes { ThreeSec = 0, TenMin = 1, TwoHours = 2, Bad = 3 }

	public enum ConstraintsType
	{
		GOST_1_0_38 = 0,
		GOST_2_6_20 = 1,
		GOST_3_35 = 2,
		GOST_4_110_330 = 3,
		USER1 = 4,
		USER2 = 5
	}

	public class EmService
	{
		public static readonly string FileNameForReader = "ForReaderProc.cfg";

		public static readonly int AVG_RECORD_LENGTH = 16384;
		public static readonly int PQP_SEGMENT_LENGTH = 2048;
		public static readonly int DNS_RECORD_LENGTH = 128;

		// ��� ���������� ����� ��� �������������� ���������� ������ ������� ���� �������� �����
		//public static bool ShowWndFeedback = true;

		//public const int WM_USER = 0x0400;

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

		public static string GetConnSchemeAsStringFull(ConnectScheme conScheme)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmService.emstrings",
									System.Reflection.Assembly.GetExecutingAssembly());
				switch (conScheme)
				{
					case ConnectScheme.Ph3W4:
					//case ConnectScheme.Ph3W4_B_calc:
						return rm.GetString("name_con_scheme_Ph3W4_full");
					case ConnectScheme.Ph3W3:
						return rm.GetString("name_con_scheme_Ph3W3_full");
					case ConnectScheme.Ph3W3_B_calc:
						return rm.GetString("name_con_scheme_Ph3W3_B_calc_full");
					case ConnectScheme.Ph1W2:
						return rm.GetString("name_con_scheme_Ph1W2_full");
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				DumpException(ex, "Error in GetConnSchemeAsStringFull():");
				return string.Empty;
			}
		}

		public static string GetConnSchemeAsStringShort(ConnectScheme conScheme)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmService.emstrings",
									System.Reflection.Assembly.GetExecutingAssembly());
				switch (conScheme)
				{
					case ConnectScheme.Ph3W4:
					//case ConnectScheme.Ph3W4_B_calc:
						return rm.GetString("name_con_scheme_Ph3W4_short");
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						return rm.GetString("name_con_scheme_Ph3W3_short");
					case ConnectScheme.Ph1W2:
						return rm.GetString("name_con_scheme_Ph1W2_short");
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				DumpException(ex, "Error in GetConnSchemeAsStringShort():");
				return string.Empty;
			}
		}

		public static string GetConstraintsAsStringFull(ConstraintsType type)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmService.emstrings",
									System.Reflection.Assembly.GetExecutingAssembly());
				switch (type)
				{
					case ConstraintsType.GOST_1_0_38:
						return rm.GetString("name_constraint_type_pqpa_1_full");
					case ConstraintsType.GOST_2_6_20:
						return rm.GetString("name_constraint_type_pqpa_2_full");
					case ConstraintsType.GOST_3_35:
						return rm.GetString("name_constraint_type_pqpa_3_full");
					case ConstraintsType.GOST_4_110_330:
						return rm.GetString("name_constraint_type_pqpa_4_full");
					case ConstraintsType.USER1:
						return rm.GetString("name_constraint_type_pqpa_5_full");
					case ConstraintsType.USER2:
						return rm.GetString("name_constraint_type_pqpa_6_full");
				}
				return string.Empty;
			}
			catch (Exception ex)
			{
				DumpException(ex, "Error in GetConstraintsAsStringFull():");
				return string.Empty;
			}
		}

		public static void SearchAdditionalDirs(string path, string[] extensions,
			ref List<string> listAdditionalDirs, ref DoWorkEventArgs e,
			ref BackgroundWorker bwSearchFiles, string[] ExistingPaths)
		{
			List<string> dirs = new List<string>();

			try
			{
				string[] files = null;
				foreach (var extension in extensions)
				{
					files = Directory.GetFiles(path, extension, SearchOption.TopDirectoryOnly);
					if (files.Length > 0) break;		// it's enough to find one of types of archives
				}
				if (files != null && files.Length > 0)
				{
					string dirMain = EmService.GetDirFirstLevel(path);
					if (Array.Find(ExistingPaths, p => p == dirMain) == null)
					{
						if (!listAdditionalDirs.Contains(dirMain))
							listAdditionalDirs.Add(dirMain);
					}
				}
				else    // we go deeper only if we didn't find anything at top level
				// because we store only root directory
				{
					dirs.AddRange(Directory.GetDirectories(path));
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				WriteToLogFailed("SearchDirs UnauthorizedAccessException" + ex.Message);
			}
			catch (IOException ioe)
			{
				WriteToLogFailed("SearchDirs IOException" + ioe.Message);
			}

			foreach (string dir in dirs)
			{
				if (bwSearchFiles.CancellationPending)
				{
					e.Cancel = true;
					e.Result = false;
					return;
				}

				DirectoryInfo dirInfo = new DirectoryInfo(dir);
				if ((dirInfo.Attributes & FileAttributes.System) == 0 &&
					(dirInfo.Attributes & FileAttributes.Hidden) == 0 &&
					(dirInfo.Attributes & FileAttributes.Archive) == 0)
					SearchAdditionalDirs(dir, extensions, ref listAdditionalDirs, ref e,
						ref bwSearchFiles, ExistingPaths);
			}
		}

		public static string GetDirFirstLevel(string dir)
		{
			try
			{
				int pos = dir.IndexOf('\\');
				if (pos != -1)
				{
					pos = dir.IndexOf('\\', pos + 1);
					if (pos != -1)
					{
						dir = dir.Substring(0, pos);
					}
				}
				return dir;
			}
			catch (Exception ex)
			{
				DumpException(ex, "Error in GetDirFirstLevel():");
				return string.Empty;
			}
		}

		public static string GetCurrentDhcpIpAddress()
		{
			string res = string.Empty;
			try
			{
				System.Management.ManagementClass objMC =
					new System.Management.ManagementClass("Win32_NetworkAdapterConfiguration");
				System.Management.ManagementObjectCollection objMOC = objMC.GetInstances();

				foreach (System.Management.ManagementObject objMO in objMOC)
				{
					if (!(bool) objMO["ipEnabled"])
						continue;
					res = (string) objMO["DHCPServer"];
				}
				return res;
			}
			catch (Exception ex)
			{
				DumpException(ex, "Error in GetCurrentDhcpIpAddress():");
				return string.Empty;
			}
		}

		/// <summary>
		/// ������ � ��������� ���� ����������� ��� ������ ResourceManager
		/// </summary>
		public static string GetAvgTypeAsString(AvgTypes type)
		{
			switch (type)
			{
				case AvgTypes.ThreeSec: return "name_avg_time_3sec_full";
				case AvgTypes.TenMin: return "name_avg_time_10min_full";
				case AvgTypes.TwoHours: return "name_avg_time_2hour_full";
				default: return "";
			}
		}

		public const int CountPhases = 8;

		public static string GetPhaseName(int phase)
		{
			switch (phase)
			{
				case 0: return "A";
				case 1: return "B";
				case 2: return "C";
				case 3: return "AB";
				case 4: return "BC";
				case 5: return "CA";
				case 6: return "ABCN";
				case 7: return "ABC";
			}
			return "";
		}

		public static int GetPhaseAsNumber(string phase)
		{
			switch (phase)
			{
				case "A": return 0;
				case "B": return 1;
				case "C": return 2;
				case "AB": return 3;
				case "BC": return 4;
				case "CA": return 5;
				case "ABCN": return 6;
				case "ABC": return 7;
			}
			return -1;
		}

		public static string GetEm32CommandText(ushort command)
		{
			switch (command)
			{
				case 0x1000: return "COMMAND_OK";
				case 0x1001: return "COMMAND_UNKNOWN_COMMAND";
				case 0x1002: return "COMMAND_CRC_ERROR";
				case 0x0000: return "COMMAND_ECHO";
				case 0x0001: return "COMMAND_ReadTime";
				case 0x0002: return "COMMAND_ReadCalibration";
				case 0x0003: return "COMMAND_WriteCalibration";
				case 0x0009: return "COMMAND_ReadQualityDates";
				case 0x000A: return "COMMAND_ReadQualityEntry";
				case 0x0007: return "COMMAND_ReadSets";
				case 0x0008: return "COMMAND_WriteSets";
				case 0x000B: return "COMMAND_ReadSystemData";
				case 0x000C: return "COMMAND_WriteSystemData";
				case 0x000D: return "COMMAND_Read3secValues";
				case 0x000E: return "COMMAND_Read1minValues";
				case 0x000F: return "COMMAND_Read30minValues";
				case 0x0012: return "COMMAND_ReadEventLogger";
				case 0x0019: return "COMMAND_ReadDipSwellArchive";
				case 0x0010: return "COMMAND_ReadDipSwellStatus";
				case 0x001A: return "COMMAND_ReadDipSwellIndexByStartTimestamp";
				case 0x001B: return "COMMAND_ReadDipSwellIndexByEndTimestamp";
				case 0x001C: return "COMMAND_ReadEarliestAndLatestDipSwellTimestamp";
				case 0x0013: return "COMMAND_Read3secArchiveByTimestamp";
				case 0x0014: return "COMMAND_Read1minArchiveByTimestamp";
				case 0x0015: return "COMMAND_Read30minArchiveByTimestamp";
				case 0x001E: return "COMMAND_ReadEarliestAndLatestAverageTimestamp";
				case 0x001F: return "COMMAND_ReadQualityDatesByObject";
				case 0x0006: return "COMMAND_AverageArchiveQuery";
				case 0x0026: return "COMMAND_Read3secArchiveByTimestampObjectDemand";
				case 0x0027: return "COMMAND_Read1minArchiveByTimestampObjectDemand";
				case 0x0028: return "COMMAND_Read30minArchiveByTimestampObjectDemand";
				case 0x0024: return "COMMAND_ReadObjectsEntrys";
				case 0x0025: return "COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand";
				case 0x0021: return "COMMAND_ReadDipSwellArchiveByObject";
				case 0x0022: return "COMMAND_ReadDipSwellIndexByStartTimestampByObject";
				case 0x0023: return "COMMAND_ReadDipSwellIndexByEndTimestampByObject";
				//case 0x0020: return "COMMAND_ReadQualityEntryObjectDemand";
				case 0x0032: return "COMMAND_ReadQualityContents";
				case 0x0033: return "COMMAND_ReadQualityEntryByTimestampByObject";
				case 0x4009: return "COMMAND_ReadRegistrationIndices";
				case 0x400A: return "COMMAND_ReadRegistrationByIndex";
				case 0x400D: return "COMMAND_ReadRegistrationArchiveByIndex";
				case 0x4013: return "COMMAND_ReadAverageArchive3SecIndices";
				case 0x4014: return "COMMAND_ReadAverageArchive10MinIndices";
				case 0x4015: return "COMMAND_ReadAverageArchive2HourIndices";
				case 0x4010: return "COMMAND_ReadAverageArchive3SecByIndex";
				case 0x4011: return "COMMAND_ReadAverageArchive10MinByIndex";
				case 0x4012: return "COMMAND_ReadAverageArchive2HourByIndex";
				case 0x4016: return "COMMAND_ReadAverageArchive3SecIndexByDateTime";
				case 0x4017: return "COMMAND_ReadAverageArchive10MinIndexByDateTime";
				case 0x4018: return "COMMAND_ReadAverageArchive2HourIndexByDateTime";
				case 0x4019: return "COMMAND_ReadAverageArchive3SecMinMaxIndices";
				case 0x401A: return "COMMAND_ReadAverageArchive10MinMinMaxIndices";
				case 0x401B: return "COMMAND_ReadAverageArchive2HourMinMaxIndices";
				default: return command.ToString();
			}
		}

		// ����, ��������� �������� ����������, ��� ����� ���������� ������� 
		// ���������� �������� �����/������
		//public static bool bStopIo = false;

		public static void Init()
		{
			currentDirectory_ = System.AppDomain.CurrentDomain.BaseDirectory;

			LogGeneralName = currentDirectory_ + "LogGeneral.txt";
			LogDebugName = currentDirectory_ + "LogDebug.txt";
			LogFailedName = currentDirectory_ + "LogFailed.txt";
		}

		#region Logs

		public static string LogGeneralName;
		public static string LogDebugName;
		public static string LogFailedName;

		private static object logDebugLock_ = new object();
		private static object logFailedLock_ = new object();
		private static object logGeneralLock_ = new object();

		public static void WriteToLogGeneral(string s)
		{
			StreamWriter sw = null;
			try
			{
				System.Diagnostics.Debug.WriteLine(s);
				lock (logGeneralLock_)
				{
					try
					{
						sw = new StreamWriter(LogGeneralName, true);
						sw.WriteLine(s);
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error in WriteToLogGeneral() " + ex.Message);
				throw;
			}
		}

		public static void WriteToLogDebug(string s)
		{
			StreamWriter sw = null;
			try
			{
				System.Diagnostics.Debug.WriteLine(s);
				lock (logDebugLock_)
				{
					try
					{
						sw = new StreamWriter(LogDebugName, true);
						sw.WriteLine(s);
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error in WriteToLogDebug() " + ex.Message);
				throw;
			}
		}

		public static void WriteToLogDebugToSameString(string s)
		{
			StreamWriter sw = null;
			try
			{
				s += " | ";
				System.Diagnostics.Debug.Write(s);
				lock (logDebugLock_)
				{
					try
					{
						sw = new StreamWriter(LogDebugName, true);
						sw.Write(s);
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error in WriteToLogDebug() " + ex.Message);
				throw;
			}
		}

		public static void WriteBytesToLogDebug(List<byte> bytes)
		{
			StreamWriter sw = null;
			try
			{
				string s = "\n";
				for (int i = 0; i < bytes.Count; ++i)
				{
					//s += bytes[i].ToString() + " | ";
					s += bytes[i].ToString() + "\n";
				}
				System.Diagnostics.Debug.Write(s);
				s += "\n";
				lock (logDebugLock_)
				{
					try
					{
						sw = new StreamWriter(LogDebugName, true);
						sw.Write(s);
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error in WriteToLogDebug() " + ex.Message);
				throw;
			}
		}

		// ��������� ���
		public static void WriteToLogFailed(string s)
		{
			StreamWriter sw = null;

			try
			{
				System.Diagnostics.Debug.WriteLine(s);
				lock (logFailedLock_)
				{
					try
					{
						sw = new StreamWriter(LogFailedName, true);

						sw.WriteLine("=======================================");
						sw.WriteLine(DateTime.Now);
						sw.WriteLine(s);
						sw.WriteLine("=======================================");
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Error in WriteToLogFailed() " + ex.Message);
				throw;
			}
		}

		public static void DumpException(Exception ex, string info)
		{
			StreamWriter sw = null;

			try
			{
				lock (logFailedLock_)
				{
					try
					{
						sw = new StreamWriter(LogFailedName, true);
						sw.WriteLine("Exception at " + DateTime.Now.ToString());

						if (ex is EmException)
						{
							sw.WriteLine(info);
							sw.WriteLine((ex as EmException).Message);
							return;
						}

						sw.WriteLine(info);
						System.Diagnostics.Debug.WriteLine(info);

						System.Diagnostics.Debug.WriteLine("--------- Outer Exception Data ---------");
						sw.WriteLine("========= Exception Dump ===============");
						sw.WriteLine("--------- Outer Exception Data ---------");
						WriteExceptionInfo(ex, ref sw);

						ex = ex.InnerException;
						while (ex != null)
						{
							System.Diagnostics.Debug.WriteLine("--------- Inner Exception Data ---------");
							sw.WriteLine("--------- Inner Exception Data ---------");
							WriteExceptionInfo(ex, ref sw);
							ex = ex.InnerException;
						}
						sw.WriteLine("========= end of Exception Dump ========");
					}
					finally
					{
						if (sw != null) sw.Close();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Error in DumpException() " + e.Message);
				throw e;
			}
		}

		public static void WriteExceptionInfo(Exception ex, ref StreamWriter sw)
		{
			System.Diagnostics.Debug.WriteLine("\nMessage: {0}", ex.Message);
			sw.WriteLine("\nMessage: {0}", ex.Message);
			System.Diagnostics.Debug.WriteLine("\nException Type: {0}", ex.GetType().FullName);
			sw.WriteLine("\nException Type: {0}", ex.GetType().FullName);
			System.Diagnostics.Debug.WriteLine("\nSource: {0}", ex.Source);
			sw.WriteLine("\nSource: {0}", ex.Source);
			System.Diagnostics.Debug.WriteLine("\nStrackTrace: {0}", ex.StackTrace);
			sw.WriteLine("\nStrackTrace: {0}", ex.StackTrace);
			System.Diagnostics.Debug.WriteLine("\nTargetSite: {0}", ex.TargetSite.ToString());
			sw.WriteLine("\nTargetSite: {0}", ex.TargetSite);
		}

		#endregion

		private static string currentDirectory_;

		//private static string xmlImageFileExtention_ = "debug.xml";
		//private static string xmlInfoFileExtention_ = "devinfo.xml";
		//private static string tempImageDir_ = "TempImagesFiles";

		//public static string TEMP_IMAGE_DIR
		//{
		//    get
		//    {
		//        return String.Format("{0}{1}\\", currentDirectory_, tempImageDir_);
		//    }
		//}
		//public static string SUPPORT_PAGES_FILE_NAME = "PagesDump_{0}";

		//public static string GetXmlImageFilePathAndName()
		//{
		//    try
		//    {
		//        string fileName = TEMP_IMAGE_DIR;
		//        if (!Directory.Exists(fileName))
		//        {
		//            Directory.CreateDirectory(fileName);
		//        }
		//        fileName += SUPPORT_PAGES_FILE_NAME;
		//        fileName = string.Format(fileName,
		//            DateTime.Now.ToString("yyyyMMddHHmmss")) + "." +
		//            xmlImageFileExtention_;

		//        return fileName;
		//    }
		//    catch (Exception ex)
		//    {
		//        DumpException(ex, "Error in GetXmlImageFilePathAndName():");
		//        throw;
		//    }
		//}

		//public static string GetXmlInfoFileName()
		//{
		//    try
		//    {
		//        string fileName = TEMP_IMAGE_DIR;
		//        if (!Directory.Exists(fileName))
		//        {
		//            Directory.CreateDirectory(fileName);
		//        }
		//        fileName += SUPPORT_PAGES_FILE_NAME;
		//        fileName = string.Format(fileName,
		//            DateTime.Now.ToString("yyyyMMddHHmmss")) + "." +
		//            xmlInfoFileExtention_;

		//        return fileName;
		//    }
		//    catch (Exception ex)
		//    {
		//        DumpException(ex, "Error in GetXmlInfoFileName():");
		//        throw;
		//    }
		//}

		//public static string GetAvgTmpFileName(string avgType, DateTime start)
		//{
		//    try
		//    {
		//        return avgType + start.ToString("MM_dd_yyyy_HH_mm_ss");
		//    }
		//    catch (Exception ex)
		//    {
		//        DumpException(ex, "Error in GetAvgTmpFileName():");
		//        throw;
		//    }
		//}

		public static string AppDirectory
		{
			get
			{
				return currentDirectory_;
			}
		}

		public static void CopyUShortArrayToByteArray(ref ushort[] src, ref byte[] dest, long destIndex)
		{
			try
			{
				byte[] destTemp = new byte[src.Length * 2];
				for (int iShort = 0; iShort < src.Length; ++iShort)
				{
					Array.Copy(BitConverter.GetBytes(src[iShort]), 0, destTemp, iShort * 2, 2);
				}
				//for (int iByte = 0; (iByte + 1) < destTemp.Length; iByte += 2)
				//{
				//    byte temp = destTemp[iByte];
				//    destTemp[iByte] = destTemp[iByte + 1];
				//    destTemp[iByte + 1] = temp;
				//}
				Array.Copy(destTemp, 0, dest, destIndex, destTemp.Length);
			}
			catch (Exception ex)
			{
				DumpException(ex, "Error in CopyUShortArrayToByteArray():");
				throw;
			}
		}

		public static string GetValidFileName(string fn)
		{
			fn = fn.Replace('<', '_');
			fn = fn.Replace('>', '_');
			fn = fn.Replace(':', '_');
			fn = fn.Replace('"', '_');
			fn = fn.Replace('/', '_');
			fn = fn.Replace('\\', '_');
			fn = fn.Replace('|', '_');
			fn = fn.Replace('*', '_');
			fn = fn.Replace('?', '_');
			fn = fn.Replace('.', '_');
			return fn;
		}
	}

	public enum EmPortType
	{
		/// <summary>USB port</summary>
		USB = 0,
		/// <summary>Internet</summary>
		Internet = 1,
		/// <summary>Wi-Fi</summary>
		WI_FI = 2
	}

	public class Pair<T1, T2>
	{
		T1 first_;
		T2 second_;

		public Pair(T1 f, T2 s)
		{
			first_ = f;
			second_ = s;
		}

		public T1 First
		{
			get { return first_; }
			set { first_ = value; }
		}

		public T2 Second
		{
			get { return second_; }
			set { second_ = value; }
		}

		public bool Equals(Pair<T1, T2> other)
		{
			if (other == null) return false;
			return (other.first_.Equals(this.first_) && other.second_.Equals(this.second_));
		}

		public static bool operator ==(Pair<T1, T2> obj1, Pair<T1, T2> obj2)
		{
			try
			{
				if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
					return ReferenceEquals(obj1, obj2);

				return (obj1.first_.Equals(obj2.first_) && obj1.second_.Equals(obj2.second_));
			}
			catch { return false; }
		}

		public static bool operator !=(Pair<T1, T2> obj1, Pair<T1, T2> obj2)
		{
			try
			{
				//return !(obj1 == obj2);

				if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
					return !ReferenceEquals(obj1, obj2);

				return (!obj1.first_.Equals(obj2.first_) || !obj1.second_.Equals(obj2.second_));
			}
			catch { return true; }
		}
	}

	public class Trio<T1, T2, T3>
	{
		T1 first_;
		T2 second_;
		T3 third_;

		public Trio(T1 f, T2 s, T3 t)
		{
			first_ = f;
			second_ = s;
			third_ = t;
		}

		public T1 First
		{
			get { return first_; }
			set { first_ = value; }
		}

		public T2 Second
		{
			get { return second_; }
			set { second_ = value; }
		}

		public T3 Third
		{
			get { return third_; }
			set { third_ = value; }
		}
	}

	public class Constants
	{
		public const byte StartMaxModeHour = 8;
		public const byte EndMaxModeHour = 23;

		/// <summary>
		/// Flicker's period (in minutes)
		/// </summary>
		public const short TFlickerST = 10;
		public const short TFlickerLT = 120;

		/// <summary>
		/// ����� ��������� ����� ����� ��������� ������� ������� ��������� � ������ ������������ ��������
		/// </summary>
		/// <param name="begin">������ ��������� ������� �������</param>
		/// <param name="end">����� ��������� ������� �������</param>
		/// <param name="max">�������� ���������� ������ ����. ��������</param>
		/// <param name="min">�������� ���������� ������ ���. ��������</param>
		public static void DiffMaxMinMode(DateTime begin, DateTime end, out TimeSpan max, out TimeSpan min)
		{
			DateTime tmp;
			max = TimeSpan.Zero;
			min = TimeSpan.Zero;
			while (begin < end)
			{
				if (begin.Hour < StartMaxModeHour && begin < end)	// �� ������ ������ ����.��������
				{
					tmp = new DateTime(begin.Year, begin.Month, begin.Day, StartMaxModeHour, 0, 0, 0);
					if (end <= tmp)
					{
						min += end - begin;
						begin = end;
						break;
					}
					if (end > tmp)
					{
						min += tmp - begin;
						begin = tmp;
					}
				}
				if (begin.Hour >= EndMaxModeHour && begin < end)	// ����� ��������� ������ ����.��������
				{
					tmp = new DateTime(begin.Year, begin.Month, begin.Day, StartMaxModeHour, 0, 0, 0);
					tmp = tmp.AddDays(1);
					if (end <= tmp)
					{
						min += end - begin;
						begin = end;
						break;
					}
					if (end > tmp)
					{
						min += tmp - begin;
						begin = tmp;
					}
				}
				if (begin.Hour >= StartMaxModeHour
					&& begin.Hour < EndMaxModeHour && begin < end)	// ����� ����.��������
				{
					tmp = new DateTime(begin.Year, begin.Month, begin.Day, EndMaxModeHour, 0, 0, 0);
					if (end <= tmp)
					{
						max += end - begin;
						begin = end;
						break;
					}
					if (end > tmp)
					{
						max += tmp - begin;
						begin = tmp;
					}
				}
			}
		}

		/// <summary>
		/// ������� ���������� ���� �� ������� > 180 ��� � �����-�
		/// </summary>
		public static DEVICE_VERSIONS AnalyseDeviceVersion(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				EmService.WriteToLogFailed("AnalyseDeviceVersion: IsNullOrEmpty");
				return DEVICE_VERSIONS.ETPQP_A_OLD;
			}

			try
			{
				DateTime dtVersion = new DateTime(
					Int32.Parse(version.Substring(version.Length - 6, 2)) + 2000,
					Int32.Parse(version.Substring(version.Length - 4, 2)),
					Int32.Parse(version.Substring(version.Length - 2, 2)));

				if (dtVersion < new DateTime(2014, 11, 10)) return DEVICE_VERSIONS.ETPQP_A_OLD;

				if(dtVersion <  new DateTime(2015, 4, 10)) return DEVICE_VERSIONS.ETPQP_A_ADDED_DIP_OVER180;

				return DEVICE_VERSIONS.ETPQP_A_DIP_GOST33073;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "AnalyseDeviceVersion error! " + version);
				return DEVICE_VERSIONS.ETPQP_A_OLD;
			}
		}
	}

	public enum DEVICE_VERSIONS
	{
		ETPQP_A_OLD = 0,
		ETPQP_A_ADDED_DIP_OVER180 = 1,
		ETPQP_A_DIP_GOST33073 = 2
	}

	public class EmException : Exception
	{
		private string message_;

		public EmException(string mess)
		{
			message_ = mess;
		}

		public override string Message
		{
			get { return message_; }
		}
	}

	public class EmDeviceEmptyException : Exception
	{
	}

	public class EmDisconnectException : Exception
	{
	}

	public class EmDeviceOldVersionException : Exception
	{
	}
}
