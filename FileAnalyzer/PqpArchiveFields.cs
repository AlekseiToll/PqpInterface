using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAnalyzerLib
{
	public class PqpArchiveFields
	{
		public static readonly ushort CntTenMinutes = 1008;		// my field
		public static readonly ushort CntFrequencies = 60480;	// my field

		public uint PqpArchiveIndex;
		public uint Signature;
		public uint RegistrationIndex;
		public DateTime DtPqpStart;
		public DateTime DtPqpEnd;
		public ushort ArchiveNumber;
		public ushort ArchiveVersion;
		//public uint PqpStartDateTimeSeconds;
		//public uint PqpStopDateTimeSecondsPlanned;
		//public uint PqpStopDateTimeSecondsDefacto;
		//public DateTime PqpStopDateTimeDefacto;

		// Количество измерений частоты – полное (не более 60480)
		public ushort FreqDeviationCounterTotal;
		// Количество измерений частоты – синхронизированное с сетью (не более 60480)
		public ushort FreqDeviationCounterLocked;
		// Количество измерений частоты в НДП (calculated by me)
		public ushort FreqDeviationCounterLockedNormal;
		// Количество измерений частоты – между НДП и ПДП (не более 60480)
		public ushort FreqDeviationCounterLockedT1;
		// Количество измерений частоты – за ПДП (не более 60480)
		// !!! а в отчете Т2 это "между НДП и ПДП" + "за ПДП" !!!
		public ushort FreqDeviationCounterLockedT2;
		// Количество измерений частоты – НEсинхронизированное с сетью (не более 60480)
		public ushort FreqDeviationCounterNonlocked;
		// 0 – статистика по частоте НЕ вычислялась (недостаточно синхронизированных с 
		// сетью измерений отклонения частоты)
		// 1 - статистика по частоте вычислялась
		public bool BoolFreqDeviationStatistics;
		public bool[] FreqValuesValid = new bool[CntFrequencies];	// doesn't exist in the device

		public float FreqDeviationDown95;
		public float FreqDeviationDown100;
		public float FreqDeviationUp95;
		// МАКСИМАЛЬНОЕ значение отклонения частоты (если wFrequencyDeviationCounterLocked>0)
		public float FreqDeviationUp100;

		// Массив временных меток измерений отклонения частоты
		public int[] FreqDeviationSeconds = new int[CntFrequencies];// FrequencyDeviationStartDateTimeSeconds
		// Массив отклонений частоты (длиной wFrequencyDeviationCounterTotal)
		public float[] FreqDeviation = new float[CntFrequencies];

		// Количество 10-минутных измерений – полное (не более 1008)
		public ushort TenMinuteCounterTotal;
		public ushort TenMinuteCounterNonflagged;
		public ushort TenMinuteCounterFlagged;
		public bool BoolTenMinuteStatistics;
		// Массив временных меток 10-минутных измерений длиной [w10MinuteCounterTotal]
		public int[] TenMinuteSeconds = new int[CntTenMinutes];	// TenMinuteStartDateTimeSeconds
		// Массив признаков маркированности 10-минутных измерений длиной [w10MinuteCounterTotal]
		/// <summary>false = marked = NOT valid, true = not marked = valid</summary>
		public bool[] TenMinuteNotMarked = new bool[CntTenMinutes];		// TenMinuteFlaggedOrNonflagged

		// voltage
		// Количество измерений в НДП (calculated by me)
		public ushort UAABDeviationPosNonflaggedNormal;
		// Количество измерений dUAAB(+) между НДП и ПДП (не более 1008)
		public ushort UAABDeviationPosNonflaggedT1;
		// Количество измерений dUAAB(+) за ПДП (не более 1008)
		public ushort UAABDeviationPosNonflaggedT2;
		// ВЕРХНЕЕ значение dUAAB(+) (если bool10MinuteStatistics=1)
		public float UAABDeviationPos95;
		// МАКСИМАЛЬНОЕ значение dUAAB(+) (если w10MinuteCounterNonflagged>0)
		public float UAABDeviationPos100;

		public ushort UBBCDeviationPosNonflaggedNormal;
		public ushort UBBCDeviationPosNonflaggedT1;
		public ushort UBBCDeviationPosNonflaggedT2;
		public float UBBCDeviationPos95;
		public float UBBCDeviationPos100;

		public ushort UCCADeviationPosNonflaggedNormal;
		public ushort UCCADeviationPosNonflaggedT1;
		public ushort UCCADeviationPosNonflaggedT2;
		public float UCCADeviationPos95;
		public float UCCADeviationPos100;

		public ushort UAABDeviationNegNonflaggedNormal;
		public ushort UAABDeviationNegNonflaggedT1;
		public ushort UAABDeviationNegNonflaggedT2;
		public float UAABDeviationNeg95;
		public float UAABDeviationNeg100;

		public ushort UBBCDeviationNegNonflaggedNormal;
		public ushort UBBCDeviationNegNonflaggedT1;
		public ushort UBBCDeviationNegNonflaggedT2;
		public float UBBCDeviationNeg95;
		public float UBBCDeviationNeg100;

		public ushort UCCADeviationNegNonflaggedNormal;
		public ushort UCCADeviationNegNonflaggedT1;
		public ushort UCCADeviationNegNonflaggedT2;
		public float UCCADeviationNeg95;
		public float UCCADeviationNeg100;

		public float[] UAABDeviationPos = new float[CntTenMinutes];
		public float[] UBBCDeviationPos = new float[CntTenMinutes];
		public float[] UCCADeviationPos = new float[CntTenMinutes];
		public float[] UAABDeviationNeg = new float[CntTenMinutes];
		public float[] UBBCDeviationNeg = new float[CntTenMinutes];
		public float[] UCCADeviationNeg = new float[CntTenMinutes];

		// voltage min max mode. this data doesn't exist in the device. it's determined by user while
		// the program is running
		public bool MaxModeExists;
		public ushort MaxModeTenMinuteCounterNonflagged;

		public ushort MaxModeUAABDeviationPosNonflaggedT2;
		public ushort MaxModeUBBCDeviationPosNonflaggedT2;
		public ushort MaxModeUCCADeviationPosNonflaggedT2;
		public ushort MaxModeUAABDeviationNegNonflaggedT2;
		public ushort MaxModeUBBCDeviationNegNonflaggedT2;
		public ushort MaxModeUCCADeviationNegNonflaggedT2;

		public float MaxModeUAABDeviationPos100;
		public float MaxModeUBBCDeviationPos100;
		public float MaxModeUCCADeviationPos100;
		public float MaxModeUAABDeviationNeg100;
		public float MaxModeUBBCDeviationNeg100;
		public float MaxModeUCCADeviationNeg100;

		public bool MinModeExists;
		public ushort MinModeTenMinuteCounterNonflagged;

		public ushort MinModeUAABDeviationPosNonflaggedT2;
		public ushort MinModeUBBCDeviationPosNonflaggedT2;
		public ushort MinModeUCCADeviationPosNonflaggedT2;
		public ushort MinModeUAABDeviationNegNonflaggedT2;
		public ushort MinModeUBBCDeviationNegNonflaggedT2;
		public ushort MinModeUCCADeviationNegNonflaggedT2;

		public float MinModeUAABDeviationPos100;
		public float MinModeUBBCDeviationPos100;
		public float MinModeUCCADeviationPos100;
		public float MinModeUAABDeviationNeg100;
		public float MinModeUBBCDeviationNeg100;
		public float MinModeUCCADeviationNeg100;

		// Nonsynus
		public ushort[] UAABKHarmCounterNonflaggedNormal = new ushort[40];
		public ushort[] UAABKHarmCounterNonflaggedT1 = new ushort[40];
		public ushort[] UAABKHarmCounterNonflaggedT2 = new ushort[40];
		public float[] UAABKHarm95 = new float[40];
		public float[] UAABKHarm100 = new float[40];
		public ushort[] UBBCKHarmCounterNonflaggedNormal = new ushort[40];
		public ushort[] UBBCKHarmCounterNonflaggedT1 = new ushort[40];
		public ushort[] UBBCKHarmCounterNonflaggedT2 = new ushort[40];
		public float[] UBBCKHarm95 = new float[40];
		public float[] UBBCKHarm100 = new float[40];
		public ushort[] UCCAKHarmCounterNonflaggedNormal = new ushort[40];
		public ushort[] UCCAKHarmCounterNonflaggedT1 = new ushort[40];
		public ushort[] UCCAKHarmCounterNonflaggedT2 = new ushort[40];
		public float[] UCCAKHarm95 = new float[40];
		public float[] UCCAKHarm100 = new float[40];

		// Nonsymmetry
		public ushort K2UcounterNonflaggedNormal;
		public ushort K2UcounterNonflaggedT1;
		public ushort K2UcounterNonflaggedT2;
		public float K2U95;
		public float K2U100;
		public ushort K0UcounterNonflaggedNormal;
		public ushort K0UcounterNonflaggedT1;
		public ushort K0UcounterNonflaggedT2;
		public float K0U95;
		public float K0U100;


		// flicker
		public static readonly int CntFlickerLong = 84;		// my field
		public ushort UAABFlickerPstCounterNonflaggedNormal;
		public ushort UAABFlickerPstCounterNonflaggedT1;
		public ushort UAABFlickerPstCounterNonflaggedT2;
		public ushort UAABFlickerPltCounterNonflaggedNormal;
		public ushort UAABFlickerPltCounterNonflaggedT1;
		public ushort UAABFlickerPltCounterNonflaggedT2;
		public float UAABFlickerPst95;
		public float UAABFlickerPst100;
		public float UAABFlickerPlt95;
		public float UAABFlickerPlt100;
		public ushort UBBCFlickerPstCounterNonflaggedNormal;
		public ushort UBBCFlickerPstCounterNonflaggedT1;
		public ushort UBBCFlickerPstCounterNonflaggedT2;
		public ushort UBBCFlickerPltCounterNonflaggedNormal;
		public ushort UBBCFlickerPltCounterNonflaggedT1;
		public ushort UBBCFlickerPltCounterNonflaggedT2;
		public float UBBCFlickerPst95;
		public float UBBCFlickerPst100;
		public float UBBCFlickerPlt95;
		public float UBBCFlickerPlt100;
		public ushort UCCAFlickerPstCounterNonflaggedNormal;
		public ushort UCCAFlickerPstCounterNonflaggedT1;
		public ushort UCCAFlickerPstCounterNonflaggedT2;
		public ushort UCCAFlickerPltCounterNonflaggedNormal;
		public ushort UCCAFlickerPltCounterNonflaggedT1;
		public ushort UCCAFlickerPltCounterNonflaggedT2;
		public float UCCAFlickerPst95;
		public float UCCAFlickerPst100;
		public float UCCAFlickerPlt95;
		public float UCCAFlickerPlt100;
		public float[] UAABFlickerPst = new float[CntTenMinutes];
		public float[] UBBCFlickerPst = new float[CntTenMinutes];
		public float[] UCCAFlickerPst = new float[CntTenMinutes];
		public float[] UAABFlickerPlt = new float[CntFlickerLong];
		public float[] UBBCFlickerPlt = new float[CntFlickerLong];
		public float[] UCCAFlickerPlt = new float[CntFlickerLong];
		public int[] FlickerPltSeconds = new int[CntFlickerLong];
		public byte[] FlickerPltValid = new byte[CntFlickerLong];
		public ushort FlickerPltCounterTotal;
		public ushort FlickerPltCounterNonflagged;
		public ushort FlickerPltCounterFlagged;		// calculated by me
		public bool BoolFlickerPltStatistics;


		//public ushort[] Swells_110_112_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Swells_112_115_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Swells_115_120_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Swells_120_150_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Dips_90_85_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Dips_85_70_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Dips_70_40_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Dips_40_10_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Dips_10_5_Counter = new ushort[PqpDipSwellTimeCountOld];
		//public ushort[] Interruptions_5_0_Counter = new ushort[PqpDipSwellTimeCountOld];

		public static readonly int DipSwellCountOld = 10;
		public static readonly int DipSwellTimeCountOld = 9; // 8 main and 1 separately
		// the 1st index - value of dip or swell, the 2nd index - duration of the event
		public ushort[,] DipSwellOld = new ushort[DipSwellCountOld, DipSwellTimeCountOld];


		public static readonly int VoltageInterhartCount = 41;
		public float[] UAABInterharm = new float[VoltageInterhartCount];	// UAABIsg100
		public float[] UBBCInterharm = new float[VoltageInterhartCount];
		public float[] UCCAInterharm = new float[VoltageInterhartCount];


		//public ushort Swells_110_112_Long_Counter;
		//public ushort Swells_112_115_Long_Counter;
		//public ushort Swells_115_120_Long_Counter;
		//public ushort Swells_120_150_Long_Counter;
		//public ushort Dips_90_85_Long_Counter;
		//public ushort Dips_85_70_Long_Counter;
		//public ushort Dips_70_40_Long_Counter;
		//public ushort Dips_40_10_Long_Counter;
		//public ushort Dips_10_5_Long_Counter;
		//public ushort Interruptions_5_0_Long_Counter;

		public static readonly int DipSwellCount = 9;
		public static readonly int DipSwellTimeCount = 6;
		public static readonly int InterruptTimeCount = 7;
		// the 1st index - value of dip or swell, the 2nd index - duration of the event
		public ushort[,] DipSwell = new ushort[DipSwellCount, DipSwellTimeCount];
		public ushort[] Interrupt = new ushort[InterruptTimeCount];

		//public ushort[] Swell33073Counters_110_120 = new ushort[6];
		//public ushort[] Swell33073Counters_120_140 = new ushort[6];
		//public ushort[] Swell33073Counters_140_160 = new ushort[6];
		//public ushort[] Swell33073Counters_160_180 = new ushort[6];
		//public ushort[] Dip33073Counters_90_85 = new ushort[6];
		//public ushort[] Dip33073Counters_85_70 = new ushort[6];
		//public ushort[] Dip33073Counters_70_40 = new ushort[6];
		//public ushort[] Dip33073Counters_40_10 = new ushort[6];
		//public ushort[] Dip33073Counters_10_0 = new ushort[6];
		//public ushort[] Interruption33073Counters_5_0 = new ushort[7];
		public uint InterruptionMaxDuration;
	}
}
