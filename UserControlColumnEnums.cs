using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainInterface
{
	internal enum DgvUIColumns
	{
		TIME = 0,
		MARKED = 1,
		// напряжение - действующие значения
		UA = 2,
		UB = 3,
		UC = 4,
		UAB = 5,
		UBC = 6,
		UCA = 7,
		// напряжение - 1-ая гармоника
		U1A = 8,
		U1B = 9,
		U1C = 10,
		U1AB = 11,
		U1BC = 12,
		U1CA = 13,
		// напряжение - постоянная составляющая (U const)
		U0A = 14,
		U0B = 15,
		U0C = 16,
		U0AB = 17,
		U0BC = 18,
		U0CA = 19,
		// напряжение - средневыпрямленное значение
		UavrA = 20,
		UavrB = 21,
		UavrC = 22,
		UavrAB = 23,
		UavrBC = 24,
		UavrCA = 25,

		IA = 26,
		IB = 27,
		IC = 28,
		IN = 29,
		I0A = 30,
		I0B = 31,
		I0C = 32,
		I0N = 33,
		IavrA = 34,
		IavrB = 35,
		IavrC = 36,
		IavrN = 37,
		I1A = 38,
		I1B = 39,
		I1C = 40,
		I1N = 41
	}

	internal enum DgvPowerColumns
	{
		TIME = 0,
		MARKED = 1,
		PA = 2,
		PB = 3,
		PC = 4,
		P1 = 5,
		P2 = 6,
		PSUM = 7,
		SA = 8,
		SB = 9,
		SC = 10,
		SSUM = 11,
		QA = 12,
		QB = 13,
		QC = 14,
		QSUM = 15,
		TANP = 16,
		KPA = 17,
		KPB = 18,
		KPC = 19,
		KPSUM = 20
	}

	internal enum DgvPqpColumns
	{
		TIME = 0,
		MARKED = 1,
		U1 = 2,
		U2 = 3,
		U0 = 4,
		K2U = 5,
		K0U = 6,
		I1 = 7,
		I2 = 8,
		I0 = 9,
		P1 = 10,
		P2 = 11,
		P0 = 12,
		AngleP1 = 13,
		AngleP2 = 14,
		AngleP0 = 15,
		DUrelS = 16,
		DUrel1A = 17,
		DUrel1B = 18,
		DUrel1C = 19,
		DUrel1AB = 20,
		DUrel1BC = 21,
		DUrel1CA = 22,
		DUApos = 23,
		DUBpos = 24,
		DUCpos = 25,
		DUABpos = 26,
		DUBCpos = 27,
		DUCApos = 28,
		DUAneg = 29,
		DUBneg = 30,
		DUCneg = 31,
		DUABneg = 32,
		DUBCneg = 33,
		DUCAneg = 34,
		DUrelApos = 35,
		DUrelBpos = 36,
		DUrelCpos = 37,
		DUrelABpos = 38,
		DUrelBCpos = 39,
		DUrelCApos = 40,
		DUrelAneg = 41,
		DUrelBneg = 42,
		DUrelCneg = 43,
		DUrelABneg = 44,
		DUrelBCneg = 45,
		DUrelCAneg = 46
	}

	internal enum DgvAnglesColumns
	{
		TIME = 0,
		MARKED = 1,
		U1AU1B = 2,
		U1BU1C = 3,
		U1CU1A = 4,
		U1ABU1CB = 5,

		U1AI1A = 6,
		U1BI1B = 7,
		U1CI1C = 8,
		U1ABI1A = 9,
		U1CBI1C = 10
	}

	// general enum for harmonics where there are 2 columns + 3 blocks (which len = 101)
	internal class DgvHarmColumns
	{
		public static int TIME = 0;
		public static int MARKED = 1;
		public static int BLOCKA = 2;
		public static int BLOCKB = 2 + 101;
		public static int BLOCKC = 2 + 101 * 2;
		public static int BLOCKN = 2 + 101 * 3;
	}

	// separate block of harmonics
	internal class DgvHarmBlockColumns
	{
		public static int SUM = 0;
		public static int ORDER_VALUE1 = 1;	// from 1 to 50
		public static int THDS = 51;
		public static int ORDER_COEF1 = 52; // from 2 to 50
	}

	internal class DgvInterHarmColumns
	{
		public static int TIME = 0;
		public static int MARKED = 1;
		public static int BLOCKA = 2;
		public static int BLOCKB = 2 + 51;
		public static int BLOCKC = 2 + 51 * 2;
		public static int BLOCKN = 2 + 51 * 3;
	}
}
