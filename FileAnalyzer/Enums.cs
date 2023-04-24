namespace FileAnalyzerLib
{
	public enum PqpDipSwellTimeOld
	{
		FROM_0_01_TILL_0_05 = 0,
		FROM_0_05_TILL_0_1 = 1,
		FROM_0_1_TILL_0_5 = 2,
		FROM_0_5_TILL_1 = 3,
		FROM_1_TILL_3 = 4,
		FROM_3_TILL_20 = 5,
		FROM_20_TILL_60 = 6,
		OVER_60 = 7,
		OVER_180 = 8
	}

	public enum PqpDipSwellTime
	{
		FROM_0_01_TILL_0_2 = 0,
		FROM_0_2_TILL_0_5 = 1,
		FROM_0_5_TILL_1 = 2,
		FROM_1_TILL_5 = 3,
		FROM_5_TILL_20 = 4,
		FROM_20_TILL_60 = 5
	}

	public enum PqpInterruptTime
	{
		FROM_0_TILL_0_5 = 0,
		FROM_0_5_TILL_1 = 1,
		FROM_1_TILL_5 = 2,
		FROM_5_TILL_20 = 3,
		FROM_20_TILL_60 = 4,
		FROM_60_TILL_180 = 5,
		OVER_180 = 6
	}

	public enum PqpDipSwellValuesOld
	{
		SWELLS_110_112_COUNTER = 0,
		SWELLS_112_115_COUNTER = 1,
		SWELLS_115_120_COUNTER = 2,
		SWELLS_120_150_COUNTER = 3,
		DIPS_90_85_COUNTER = 4,
		DIPS_85_70_COUNTER = 5,
		DIPS_70_40_COUNTER = 6,
		DIPS_40_10_COUNTER = 7,
		DIPS_10_5_COUNTER = 8,
		INTERRUPTIONS_5_0_COUNTER = 9
	}
}
