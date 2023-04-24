using System;
using System.Collections.Generic;

using EmServiceLib;

namespace FileAnalyzerLib
{
	public class RegistrationInfo
	{
		public long SerialNumber;

		public long RegId;
		public string RegistrationName;
		public DateTime DtStart;
		public DateTime DtEnd;
		public string DeviceVersion;
		public DateTime DeviceVersionDate;

		public ConnectScheme ConnectionScheme;
		public ConstraintsType ConstraintType;
		public float[] ConstraintsArray = new float[EtPQPAConstraints.CntConstraints];
		public ConstraintsDetailed Constraints;
		public bool MarkedOnOff;

		public float Fnominal;
		public float UnominalLinear;
		public float UnominalPhase;
		public bool UtransformerEnable;
		public short UtransformerType;
		public short ItransformerUsage;			// Трансформатор тока – использование
		public short ItransformerPrimary;		// Трансформатор тока – первичный ток
		public short ItransformerSecondary;		// Трансформатор тока – вторичный ток
		public short Flimit;
		public short Ulimit;
		public short Ilimit;
		public double GpsLatitude;
		public double GpsLongitude;
		public bool AutocorrectTimeGpsEnable;

		public List<PqpArchiveInfo> PqpArchives = new List<PqpArchiveInfo>();
		public AvgArchiveInfo AvgArchive3Sec = null;
		public AvgArchiveInfo AvgArchive10Min = null;
		public AvgArchiveInfo AvgArchive2Hour = null;
		public DnsArchiveInfo DnsArchive;

		public bool IfPqpArchiveExists(int id)
		{
			foreach (var item in PqpArchives)
			{
				if (item.Id == id) return true;
			}
			return false;
		}

		public void SetAvgInfo(AvgArchiveInfo info)
		{
			switch (info.AvgType)
			{
				case AvgTypes.ThreeSec:
					AvgArchive3Sec = info; break;
				case AvgTypes.TenMin:
					AvgArchive10Min = info; break;
				case AvgTypes.TwoHours:
					AvgArchive2Hour = info; break;
			}
		}
	}

	public class GeneralArchiveInfo
	{
		public ArchiveType Type;
		public string Path = string.Empty;
		public DateTime DtStart;
		public DateTime DtEnd;
		public bool Selected = false;	// if user selected this archive
	}

	public class DnsArchiveInfo : GeneralArchiveInfo
	{
		public DnsArchiveInfo(string path, DateTime start, DateTime end)
		{
			Path = path;
			DtStart = start;
			DtEnd = end;
			Type = ArchiveType.DNS;
		}
	}

	public class AvgArchiveInfo : GeneralArchiveInfo
	{
		public AvgTypes AvgType;

		public DateTime DtStartSelected = DateTime.MinValue;
		public DateTime DtEndSelected = DateTime.MinValue;

		public AvgArchiveInfo(string path, DateTime start, DateTime end, AvgTypes type)
		{
			Path = path;
			DtStart = start;
			DtEnd = end;
			AvgType = type;
			Type = ArchiveType.AVG;
		}
	}

	public class PqpArchiveInfo : GeneralArchiveInfo
	{
		public int Id;

		public PqpArchiveInfo(int id, string path, DateTime start, DateTime end)
		{
			Id = id;
			Path = path;
			DtStart = start;
			DtEnd = end;
			Type = ArchiveType.PQP;
		}
	}
}
