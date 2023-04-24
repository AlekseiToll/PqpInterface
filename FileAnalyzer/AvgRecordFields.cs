using System;

namespace FileAnalyzerLib
{
	public class AvgRecordFields
	{
		public DateTime DtStart;
		public DateTime DtEnd;

		public bool BRecordIsMarked;

		// частота
		public float Fa;
		public float Fb;
		public float Fc;
		public float Fab;
		public float Fbc;
		public float Fca;

		// напряжение - действующие значения
		public float Ua;
		public float Ub;
		public float Uc;
		public float Uab;
		public float Ubc;
		public float Uca;

		// ток - действующие значения
		public float Ia;
		public float Ib;
		public float Ic;
		public float In;

		// напряжение - постоянная составляющая
		public float UaConst;
		public float UbConst;
		public float UcConst;
		public float UabConst;
		public float UbcConst;
		public float UcaConst;

		// ток - постоянная составляющая
		public float IaConst;
		public float IbConst;
		public float IcConst;
		public float InConst;

		// напряжение - средневыпрямленное значение
		public float UaAvdirect;
		public float UbAvdirect;
		public float UcAvdirect;
		public float UabAvdirect;
		public float UbcAvdirect;
		public float UcaAvdirect;

		// ток - средневыпрямленное значение
		public float IaAvdirect;
		public float IbAvdirect;
		public float IcAvdirect;
		public float InAvdirect;

		// напряжение - 1-ая гармоника
		public float Ua1harm;
		public float Ub1harm;
		public float Uc1harm;
		public float Uab1harm;
		public float Ubc1harm;
		public float Uca1harm;

		// ток - 1-ая гармоника
		public float Ia1harm;
		public float Ib1harm;
		public float Ic1harm;
		public float In1harm;

		// Мощность активная
		public float Pa;
		public float Pb;
		public float Pc;
		public float Psum;
		public float P1;
		public float P2;
		public float P12sum;

		// Мощность полная
		public float Sa;
		public float Sb;
		public float Sc;
		public float Ssum;
		public float S1;
		public float S2;
		public float S12sum;

		// Мощность реактивная (по первой гармонике)
		public float Qa;
		public float Qb;
		public float Qc;
		public float Qsum;
		public float Q1;
		public float Q2;
		public float Q12sum;

		// тангенс фи
		public float TanP;

		// Коэффициент мощности Kp
		public float Kpa;
		public float Kpb;
		public float Kpc;
		public float Kpabc;
		public float Kp12;

		////////////////// PQP /////////////////////////////////
		
		public float U1;					// Напряжение прямой последовательности 
		public float U2;					// Напряжение обратной последовательности
		public float U0;					// Напряжение нулевой последовательности
		public float K2u;					// Коэффициент обратной последовательности
		public float K0u;					// Коэффициент нулевой последовательности
		public float I1;					// Ток прямой последовательности
		public float I2;					// Ток обратной последовательности
		public float I0;					// Ток нулевой последовательности
		public float P1pqp;					// Мощность прямой последовательности
		public float P2pqp;					// Мощность обратной последовательности
		public float P0pqp;					// Мощность нулевой последовательности
		public float AngleP1;				// Угол мощности прямой последовательности
		public float AngleP2;				// Угол мощности обратной последовательности
		public float AngleP0;				// Угол мощности нулевой последовательности

		// Отклонение установившегося напряжения [относительное]
		public float RdUY;
		// Отклонение 1 гармоники от номинала [относительное]
		public float RdU1harmA;
		public float RdU1harmB;
		public float RdU1harmC;
		public float RdU1harmAB;
		public float RdU1harmBC;
		public float RdU1harmCA;

		// Положительное отклонение напряжения [абсолютное]
		public float DUposA;
		public float DUposB;
		public float DUposC;
		public float DUposAB;
		public float DUposBC;
		public float DUposCA;

		// Отрицательное отклонение напряжения [абсолютное]
		public float DUnegA;
		public float DUnegB;
		public float DUnegC;
		public float DUnegAB;
		public float DUnegBC;
		public float DUnegCA;

		// Положительное отклонение напряжения [относительное]
		public float RdUposA;
		public float RdUposB;
		public float RdUposC;
		public float RdUposAB;
		public float RdUposBC;
		public float RdUposCA;

		// Отрицательное отклонение напряжения [относительное]
		public float RdUnegA;
		public float RdUnegB;
		public float RdUnegC;
		public float RdUnegAB;
		public float RdUnegBC;
		public float RdUnegCA;

		//////////// End of PQP ////////////////////////////////

		// Углы между напряжениями и токами 1-ой гармоники
		// Между фазными напряжениями UA UB
		public float AngleUaUb;
		public float AngleUbUc;
		public float AngleUcUa;

		// Между фазным напряжением UA и током IA
		public float AngleUaIa;
		public float AngleUbIb;
		public float AngleUcIc;

		// Между междуфазными напряжениями AB и CB
		public float AngleUabUcb;
		// Между междуфазным напряжением AB и током IA
		public float AngleUabIa;
		public float AngleUcbIc;

		// Углы между напряжениями и токами 1-ой гармоники (ВСПОМОГАТЕЛЬНЫЕ)
		// Между междуфазными напряжениями AB и BC
		//public float AngleUabUbc;
		//public float AngleUbcUca;
		//public float AngleUcaUab;

		// Между междуфазным напряжением BC и током IA (вспомогательный угол для графики)
		//public float AngleUbcIa;
		//public float AngleUbcIb;
		//public float AngleUbcIc;
		//public float AngleUbcIn;

		//////////// Voltage Harmonics ////////////////////////
		public const int CountHarmonisc = 50;
		// Ua
		// Суммарное значение для гармонических подгрупп порядка > 1
		public float UHarmSummForOrderMore1A;
		// Значение для порядка = 1, Значения для порядков 2…50
		public float[] UHarmOrderValueA = new float[CountHarmonisc];
		// Суммарный коэффициент, Коэффициенты для порядков 2…50
		public float[] UHarmOrderCoefA = new float[CountHarmonisc];
		// Ub
		public float UHarmSummForOrderMore1B;
		public float[] UHarmOrderValueB = new float[CountHarmonisc];
		public float[] UHarmOrderCoefB = new float[CountHarmonisc];
		// Uc
		public float UHarmSummForOrderMore1C;
		public float[] UHarmOrderValueC = new float[CountHarmonisc];
		public float[] UHarmOrderCoefC = new float[CountHarmonisc];
		// Uab
		public float UHarmSummForOrderMore1AB;
		public float[] UHarmOrderValueAB = new float[CountHarmonisc];
		public float[] UHarmOrderCoefAB = new float[CountHarmonisc];
		// Ubc
		public float UHarmSummForOrderMore1BC;
		public float[] UHarmOrderValueBC = new float[CountHarmonisc];
		public float[] UHarmOrderCoefBC= new float[CountHarmonisc];
		// Uab
		public float UHarmSummForOrderMore1CA;
		public float[] UHarmOrderValueCA = new float[CountHarmonisc];
		public float[] UHarmOrderCoefCA = new float[CountHarmonisc];

		//////////// Current Harmonics ////////////////////////
		// Ia
		// Суммарное значение для гармонических подгрупп порядка > 1
		public float IHarmSummForOrderMore1A;
		// Значение для порядка = 1, Значения для порядков 2…50
		public float[] IHarmOrderValueA = new float[CountHarmonisc];
		// Суммарный коэффициент, Коэффициенты для порядков 2…50
		public float[] IHarmOrderCoefA = new float[CountHarmonisc];
		// Ib
		public float IHarmSummForOrderMore1B;
		public float[] IHarmOrderValueB = new float[CountHarmonisc];
		public float[] IHarmOrderCoefB = new float[CountHarmonisc];
		// Ic
		public float IHarmSummForOrderMore1C;
		public float[] IHarmOrderValueC = new float[CountHarmonisc];
		public float[] IHarmOrderCoefC = new float[CountHarmonisc];
		// In
		public float IHarmSummForOrderMore1N;
		public float[] IHarmOrderValueN = new float[CountHarmonisc];
		public float[] IHarmOrderCoefN = new float[CountHarmonisc];

		//////////// Voltage Interharmonics ////////////////////////
		// Ua
		// Среднеквадратическое значение субгармонической группы
		public float UInterHarmAvgSquareA;
		// Среднеквадратическое значение интергармонических групп порядков 1…50
		public float[] UInterHarmAvgSquareOrderA = new float[CountHarmonisc];
		// Ub
		public float UInterHarmAvgSquareB;
		public float[] UInterHarmAvgSquareOrderB = new float[CountHarmonisc];
		// Uc
		public float UInterHarmAvgSquareC;
		public float[] UInterHarmAvgSquareOrderC = new float[CountHarmonisc];
		// Uab
		public float UInterHarmAvgSquareAB;
		public float[] UInterHarmAvgSquareOrderAB = new float[CountHarmonisc];
		// Ubc
		public float UInterHarmAvgSquareBC;
		public float[] UInterHarmAvgSquareOrderBC = new float[CountHarmonisc];
		// Uca
		public float UInterHarmAvgSquareCA;
		public float[] UInterHarmAvgSquareOrderCA = new float[CountHarmonisc];

		//////////// Current Interharmonics ////////////////////////
		// Ia
		// Среднеквадратическое значение субгармонической группы
		public float IInterHarmAvgSquareA;
		// Среднеквадратическое значение интергармонических групп порядков 1…50
		public float[] IInterHarmAvgSquareOrderA = new float[CountHarmonisc];
		// Ib
		public float IInterHarmAvgSquareB;
		public float[] IInterHarmAvgSquareOrderB = new float[CountHarmonisc];
		// Ic
		public float IInterHarmAvgSquareC;
		public float[] IInterHarmAvgSquareOrderC = new float[CountHarmonisc];
		// Iab
		public float IInterHarmAvgSquareN;
		public float[] IInterHarmAvgSquareOrderN = new float[CountHarmonisc];

		//////////// Harmonic Powers /////////////////////////////
		// There are 3 types og harmonic powers:
		// 1. Активная составляющая (порядок 1..50)
		// 2. Реактивная составляющая
		// 3. Угол
		public const int CountHarmonicPowersType = 3;

		public float[,] HarmPowerA = new float[CountHarmonicPowersType, CountHarmonisc];
		public float[,] HarmPowerB = new float[CountHarmonicPowersType, CountHarmonisc];
		public float[,] HarmPowerC = new float[CountHarmonicPowersType, CountHarmonisc];
		public float[,] HarmPower1 = new float[CountHarmonicPowersType, CountHarmonisc];
		public float[,] HarmPower2 = new float[CountHarmonicPowersType, CountHarmonisc];
		public float[,] HarmPowerSUM = new float[CountHarmonicPowersType, CountHarmonisc];
	}

	public enum AvgHarmonicPowersType
	{
		/// <summary>Активная составляющая (порядок 1..50)</summary>
		ACTIVE = 0,
		/// <summary>Реактивная составляющая</summary>
		REACTIVE = 1,
		/// <summary>Угол</summary>
		ANGLE = 2
	}
}
