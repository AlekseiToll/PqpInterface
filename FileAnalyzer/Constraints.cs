using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EmServiceLib;

namespace FileAnalyzerLib
{
	public abstract class EMConstraintsBase
	{
		public enum ConstraintsType
		{
			GOST13109_0_38 = 0,			// ГОСТ 13109 0-38кВ
			GOST13109_6_20 = 1,			// ГОСТ 13109 6-20кВ
			GOST13109_35 = 2,			// ГОСТ 13109 35кВ
			GOST13109_110_330 = 3,			// ГОСТ 13109 110-330кВ
			USER1 = 4,			// Пользовательский 1
			USER2 = 5			// Пользовательский 2
		}

		//public enum ConstraintsSubType
		//{
		//    TYPE_3PH4W = 0,
		//    TYPE_3PH3W = 1
		//}

		// кол-во наборов уставок
		public const int CntConstraintsSets = 6;
		//// кол-во поднаборов в одном наборе
		//public const int CntSubsets = 2;
	}

	public class EtPQPAConstraints : EMConstraintsBase
	{
		#region Fields

		// кол-во уставок в поднаборе
		public const int CntConstraints = 100;

		// массив уставок: 6 наборов, в каждом из них 2 поднабора,
		// в каждом поднаборе 100 уставок (100 слов)
		private float[,] constraints_ = new float[CntConstraintsSets, CntConstraints];

		#endregion

		#region Properties

		public float[,] Constraints
		{
			get { return constraints_; }
		}

		/// <summary>Gets size of this memory region in bytes</summary>
		public ushort Size
		{
			get
			{
				return (ushort)(CntConstraintsSets * CntConstraints * 4);
			}
		}

		public float[, ,] EmptyConstraintsForTable
		{
			get
			{
				return new float[CntConstraintsSets, CntConstraints / 2, 2 /*2 columns*/];
			}
		}

		// уставки в формате таблицы
		public float[, ,] ConstraintsForTable
		{
			get
			{
				float[, ,] vals = new float[CntConstraintsSets, CntConstraints / 2, 2 /*2 columns*/];

				for (int iSet = 0; iSet < CntConstraintsSets; ++iSet)
				{
					// ∆F+ synch
					vals[iSet, 0, 0] = constraints_[iSet, 2];
					vals[iSet, 0, 1] = constraints_[iSet, 3];

					// ∆F- synch
					vals[iSet, 1, 0] = constraints_[iSet, 0];
					vals[iSet, 1, 1] = constraints_[iSet, 1];

					// ∆F+ iso
					vals[iSet, 2, 0] = constraints_[iSet, 6];
					vals[iSet, 2, 1] = constraints_[iSet, 7];

					// ∆F- iso
					vals[iSet, 3, 0] = constraints_[iSet, 4];
					vals[iSet, 3, 1] = constraints_[iSet, 5];

					// δU+
					vals[iSet, 4, 0] = constraints_[iSet, 8];
					vals[iSet, 4, 1] = constraints_[iSet, 9];

					// δU-
					vals[iSet, 5, 0] = constraints_[iSet, 10];
					vals[iSet, 5, 1] = constraints_[iSet, 11];

					// flik short
					vals[iSet, 6, 0] = constraints_[iSet, 12];
					vals[iSet, 6, 1] = constraints_[iSet, 13];

					// flik long
					vals[iSet, 7, 0] = constraints_[iSet, 14];
					vals[iSet, 7, 1] = constraints_[iSet, 15];

					// K harm 2 - K harm 40
					int startNDZ = 16, startPDZ = 55;
					for (int iRow = 8; iRow < 47; ++iRow)
					{
						vals[iSet, iRow, 0] = constraints_[iSet, startNDZ++];
						vals[iSet, iRow, 1] = constraints_[iSet, startPDZ++];
					}

					// K harm total
					vals[iSet, 47, 0] = constraints_[iSet, 94];
					vals[iSet, 47, 1] = constraints_[iSet, 95];

					// K2U
					vals[iSet, 48, 0] = constraints_[iSet, 96];
					vals[iSet, 48, 1] = constraints_[iSet, 97];

					// K0U
					vals[iSet, 49, 0] = constraints_[iSet, 98];
					vals[iSet, 49, 1] = constraints_[iSet, 99];
				}

				return vals;
			}

			set
			{
				float[, ,] vals = value;
				if (vals.Length != (CntConstraintsSets * (CntConstraints / 2) * 2))
					throw new EmException("ConstraintsForTable: invalid value length!");

				for (int iSet = 0; iSet < CntConstraintsSets; ++iSet)
				{
					// ∆F+ synch
					constraints_[iSet, 2] = vals[iSet, 0, 0];
					constraints_[iSet, 3] = vals[iSet, 0, 1];

					// ∆F- synch
					constraints_[iSet, 0] = vals[iSet, 1, 0];
					constraints_[iSet, 1] = vals[iSet, 1, 1];

					// ∆F+ iso
					constraints_[iSet, 6] = vals[iSet, 2, 0];
					constraints_[iSet, 7] = vals[iSet, 2, 1];

					// ∆F- iso
					constraints_[iSet, 4] = vals[iSet, 3, 0];
					constraints_[iSet, 5] = vals[iSet, 3, 1];

					// δU-
					constraints_[iSet, 8] = vals[iSet, 4, 0];
					constraints_[iSet, 9] = vals[iSet, 4, 1];

					// δU+
					constraints_[iSet, 10] = vals[iSet, 5, 0];
					constraints_[iSet, 11] = vals[iSet, 5, 1];

					// flik short
					constraints_[iSet, 12] = vals[iSet, 6, 0];
					constraints_[iSet, 13] = vals[iSet, 6, 1];

					// flik long
					constraints_[iSet, 14] = vals[iSet, 7, 0];
					constraints_[iSet, 15] = vals[iSet, 7, 1];

					// K harm 2 - K harm 40
					int startNDZ = 16, startPDZ = 55;
					for (int iRow = 8; iRow < 47; ++iRow)
					{
						constraints_[iSet, startNDZ++] = vals[iSet, iRow, 0];
						constraints_[iSet, startPDZ++] = vals[iSet, iRow, 1];
					}

					// K harm total
					constraints_[iSet, 94] = vals[iSet, 47, 0];
					constraints_[iSet, 95] = vals[iSet, 47, 1];

					// K2U
					constraints_[iSet, 96] = vals[iSet, 48, 0];
					constraints_[iSet, 97] = vals[iSet, 48, 1];

					// K0U
					constraints_[iSet, 98] = vals[iSet, 49, 0];
					constraints_[iSet, 99] = vals[iSet, 49, 1];
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>Parses array and fills inner object list</summary>
		/// <param name="array">byte array to parse</param>
		/// <returns>True if all OK or False</returns>
		public bool Parse(ref byte[] array)
		{
			if (array == null) return false;
			if (array.Length < this.Size) return false;

			try
			{
				constraints_ = new float[CntConstraintsSets /*6*/, CntConstraints /*98*/];
				int shift = 0;

				for (int iSet = 0; iSet < CntConstraintsSets; iSet++)
				{
					for (int iConst = 0; iConst < CntConstraints; iConst++)
					{
						// первые 4 уставки - частота, для нее другой формат
						//if (iConst < 4)
						//    constraints_[iSet, iConst] =
						//        Conversions.bytes_2Signed_float65536(ref array, shift);
						//else
						constraints_[iSet, iConst] =
							Conversions.bytes_2_signed_float_Q_15_16_new(ref array, shift);

						shift += 4; // 2 words
					}
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPAConstraints::Parse(): ");
				constraints_ = null;
				return false;
			}
		}

		/// <summary>Packs all inner data into array</summary>
		public bool Pack(ref byte[] buffer, out Int32 checkSum1, out Int32 checkSum2)
		{
			try
			{
				checkSum1 = checkSum2 = 0;
				if (constraints_ == null) return false;

				buffer = new byte[this.Size];
				int shift = 0;
				Int32 tmp = 0;

				for (int iSet = 0; iSet < CntConstraintsSets; iSet++) // constraints types
				{
					for (int iConst = 0; iConst < CntConstraints; iConst++)
					{
						Conversions.signed_float2w_Q_15_16_2_bytes(constraints_[iSet, iConst],
								ref buffer, shift);

						if (iSet == 4)
						{
							tmp = Conversions.bytes_2_int(ref buffer, shift);
							//System.Diagnostics.Debug.WriteLine(tmp.ToString());
							checkSum1 += tmp;	// user1
						}
						if (iSet == 5)
						{
							tmp = Conversions.bytes_2_int(ref buffer, shift);
							checkSum2 += tmp;	// user2
						}

						shift += 4;
					}
				}
				checkSum1 += 0x23456789;
				checkSum2 += 0x23456789;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPAConstraints::Pack(): ");
				checkSum1 = checkSum2 = 0;
				return false;
			}
		}

		#endregion
	}

	public class ConstraintsDetailed
	{
		[XmlElement]
		public float FSynchroDown95;
		[XmlElement]
		public float FSynchroDown100;
		[XmlElement]
		public float FSynchroUp95;
		[XmlElement]
		public float FSynchroUp100;
		[XmlElement]
		public float FIsolateDown95;
		[XmlElement]
		public float FIsolateDown100;
		[XmlElement]
		public float FIsolateUp95;
		[XmlElement]
		public float FIsolateUp100;
		[XmlElement]
		public float UDeviationDown95;
		[XmlElement]
		public float UDeviationDown100;
		[XmlElement]
		public float UDeviationUp95;
		[XmlElement]
		public float UDeviationUp100;
		[XmlElement]
		public float FlickShortUp95;
		[XmlElement]
		public float FlickShortUp100;
		[XmlElement]
		public float FlickLongUp95;
		[XmlElement]
		public float FlickLongUp100;

		[XmlArray]
		public float[] KHarm95 = new float[39];
		[XmlArray]
		public float[] KHarm100 = new float[39];
		[XmlElement]
		public float KHarmTotal95;
		[XmlElement]
		public float KHarmTotal100;

		[XmlElement]
		public float K2u95;
		[XmlElement]
		public float K2u100;
		[XmlElement]
		public float K0u95;
		[XmlElement]
		public float K0u100;

		// tempopary fields which are actual only when the user determines peak load mode
		public float MaxModeUDeviationUp100;
		public float MaxModeUDeviationDown100;
		public float MinModeUDeviationUp100;
		public float MinModeUDeviationDown100;

		public ConstraintsDetailed()
		{ }

		public ConstraintsDetailed(float[] constr)
		{
			if (constr.Length != EtPQPAConstraints.CntConstraints)
			{
				throw new EmException("EtPQP_A_ConstraintsDetailed: Invalid length! " + constr.Length);
			}

			FSynchroDown95 = constr[0];
			FSynchroDown100 = constr[1];
			FSynchroUp95 = constr[2];
			FSynchroUp100 = constr[3];
			FIsolateDown95 = constr[4];
			FIsolateDown100 = constr[5];
			FIsolateUp95 = constr[6];
			FIsolateUp100 = constr[7];
			UDeviationDown95 = constr[8];
			UDeviationDown100 = constr[9];
			UDeviationUp95 = constr[10];
			UDeviationUp100 = constr[11];
			FlickShortUp95 = constr[12];
			FlickShortUp100 = constr[13];
			FlickLongUp95 = constr[14];
			FlickLongUp100 = constr[15];

			for (int iSet = 0; iSet < 39; ++iSet)
			{
				KHarm95[iSet] = constr[iSet + 16];
				KHarm100[iSet] = constr[iSet + 55];
			}
			KHarmTotal95 = constr[94];
			KHarmTotal100 = constr[95];

			K2u95 = constr[96];
			K2u100 = constr[97];
			K0u95 = constr[98];
			K0u100 = constr[99];
		}

		public float[] ConstraintsArray
		{
			get
			{
				float[] constr = new float[EtPQPAConstraints.CntConstraints];

				constr[0] = FSynchroDown95;
				constr[1] = FSynchroDown100;
				constr[2] = FSynchroUp95;
				constr[3] = FSynchroUp100;
				constr[4] = FIsolateDown95;
				constr[5] = FIsolateDown100;
				constr[6] = FIsolateUp95;
				constr[7] = FIsolateUp100;
				constr[8] = UDeviationDown95;
				constr[9] = UDeviationDown100;
				constr[10] = UDeviationUp95;
				constr[11] = UDeviationUp100;
				constr[12] = FlickShortUp95;
				constr[13] = FlickShortUp100;
				constr[14] = FlickLongUp95;
				constr[15] = FlickLongUp100;

				for (int iSet = 0; iSet < 39; ++iSet)
				{
					constr[iSet + 16] = KHarm95[iSet];
					constr[iSet + 55] = KHarm100[iSet];
				}
				constr[94] = KHarmTotal95;
				constr[95] = KHarmTotal100;

				constr[96] = K2u95;
				constr[97] = K2u100;
				constr[98] = K0u95;
				constr[99] = K0u100;

				return constr;
			}
		}
	}
}
