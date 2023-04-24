using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmServiceLib;

namespace DeviceIO
{
	/// <summary>
	/// Stricture of common device information
	/// </summary>
	public class DeviceCommonInfo
	{
		public long SerialNumber = -1;
		public string DevVersion;
		public DateTime DevVersionDate;

		public ContentsLineStorage Content;

		public DeviceCommonInfo()
		{
			Content = new ContentsLineStorage();
		}
	}

	/// <summary>
	/// Class to store Main Records
	/// </summary>
	public class ContentsLineStorage
	{
		#region Fields

		List<ContentsLine> listRecords_ = new List<ContentsLine>();

		#endregion

		#region Public Methods

		public bool AddRecord(ContentsLine rec)
		{
			try
			{
				for (int iRec = 0; iRec < listRecords_.Count; ++iRec)
				{
					if (listRecords_[iRec].RegistrationId == rec.RegistrationId)
						return false;
				}
				listRecords_.Add(rec);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ContentsLineEtPQPStorage.AddRecord()");
				return false;
			}
		}

		public void DeleteAll()
		{
			listRecords_.Clear();
		}

		public ContentsLine FindRecord(int id)
		{
			try
			{
				for (int iRec = 0; iRec < listRecords_.Count; ++iRec)
				{
					if (listRecords_[iRec].RegistrationId == id)
						return listRecords_[iRec];
				}
				return null;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ContentsLineEtPQPStorage.FindRecord()" + ex.Message);
				return null;
			}
		}

		#endregion

		#region Properties

		public int Count
		{
			get { return listRecords_.Count; }
		}

		public ContentsLine this[int index]
		{
			get
			{
				if (index >= 0 && index < listRecords_.Count)
					return listRecords_[index];
				else
				{
					EmService.WriteToLogFailed("Invalid Main Record index!");
					return null;
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Structure of any one Main Record with real existing data
	/// </summary>
	[Serializable]
	public class ContentsLine
	{
		#region Fields

		public long SerialNumber;
		//public string DevVersion;
		//public DateTime DevVersionDate;
		public string RegistrationName;
		//public ConnectScheme ConnectionScheme;
		public DateTime CommonBegin;
		public DateTime CommonEnd;

		//public UInt32 DeviceSerialNumber;
		public UInt32 RegistrationId;
		//public short ConstraintType;
		//public float[] Constraints = new float[EtPQPAConstraints.CntConstraints];

		//public SystemInfoEtPQP_A SysInfo;

		// PQP out info
		public List<PqpSet> PqpSet = new List<PqpSet>();
		public int PqpCnt;//?????????????????? need?
		public short PqpLength;//?????????????????? need?

		//public ushort TimeZone;

		// Average values out info
		// в этих массивах хранятся данные по трем типам архивов усредненных (3 сек, 10 мин, 2 часа)
		// в 1-ом 3 стартовых, во 2-ом 3 конечных
		public AVGData[] AvgDataStart;
		public AVGData[] AvgDataEnd;
		public bool AvgExists = false;

		#endregion

		#region Constructor

		public ContentsLine()
		{
			AvgDataStart = new AVGData[3];
			AvgDataEnd = new AVGData[3];
			for (int i = 0; i < 3; ++i)
			{
				AvgDataStart[i] = new AVGData(0, DateTime.MinValue, DateTime.MinValue);
				AvgDataEnd[i] = new AVGData(0, DateTime.MinValue, DateTime.MinValue);
			}

			//SysInfo = new SystemInfoEtPQP_A();
		}

		#endregion

		#region Methods

		//public uint GetAVGStartIndexByType(AvgTypes type)
		//{
		//    switch (type)
		//    {
		//        case AvgTypes.ThreeSec: //return AvgIndexStart3sec;
		//            return AvgDataStart[(int)AvgTypes.ThreeSec - 1].Index;
		//        case AvgTypes.TenMin: //return AvgIndexStart10min;
		//            return AvgDataStart[(int)AvgTypes.TenMin - 1].Index;
		//        case AvgTypes.TwoHours: //return AvgIndexStart2hour;
		//            return AvgDataStart[(int)AvgTypes.TwoHours - 1].Index;
		//    }
		//    return 0;
		//}

		//public uint GetAVGEndIndexByType(AvgTypes type)
		//{
		//    switch (type)
		//    {
		//        case AvgTypes.ThreeSec: return AvgDataEnd[(int)AvgTypes.ThreeSec - 1].Index;
		//        case AvgTypes.TenMin: return AvgDataEnd[(int)AvgTypes.TenMin - 1].Index;
		//        case AvgTypes.TwoHours: return AvgDataEnd[(int)AvgTypes.TwoHours - 1].Index;
		//    }
		//    return 0;
		//}

		#endregion
	}

	public class AVGData
	{
		public uint Index;
		public DateTime dtStart = DateTime.MinValue;
		public DateTime dtEnd = DateTime.MinValue;

		public AVGData(uint index, DateTime start, DateTime end)
		{
			Index = index; dtStart = start; dtEnd = end;
		}
	}

	/// <summary>
	/// Structure of one PQP record
	/// </summary>
	public struct PqpSet
	{
		public uint PqpIndex;
		public DateTime PqpStart;
		public DateTime PqpEnd;
		public uint RegistrationId;
		//public ConnectScheme ConnectionScheme;
		//public float F_Nominal;
		//public float U_NominalLinear;
		//public float U_NominalPhase;
		//public float I_NominalPhase;
		//public short ConstraintType;

		public PqpSet(uint index, uint regId)
		{
			PqpIndex = index;
			RegistrationId = regId;

			PqpStart = DateTime.MinValue;
			PqpEnd = DateTime.MinValue;
			//ConnectionScheme = ConnectScheme.Unknown;
			//F_Nominal = 0;
			//U_NominalLinear = 0;
			//U_NominalPhase = 0;
			//I_NominalPhase = 0;
			//ConstraintType = 0;
		}
	}
}
