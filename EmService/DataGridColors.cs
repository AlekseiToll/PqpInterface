using System.Drawing;

namespace EmServiceLib
{
	public class DataGridColors
	{
		/// <summary>Gets color for all common data independently of phase</summary>
		public static Color ColorCommon
		{
			get { return Color.AliceBlue; }
		}

		/// <summary>Gets color for datetime column</summary>
		public static Color ColorAvgTime
		{
			get { return Color.White; }
		}

		/// <summary>Gets color for columns for parameter of phase A</summary>
		public static Color ColorAvgPhaseA
		{
			get { return Color.FromArgb(0xFF, 0xFF, 0xE1); }
		}

		/// <summary>Gets color for columns for parameter of phase B</summary>
		public static Color ColorAvgPhaseB
		{
			get { return Color.FromArgb(0xF0, 0xFF, 0xF0); }
		}

		/// <summary>Gets color for columns for parameter of phase C</summary>
		public static Color ColorAvgPhaseC
		{
			get { return Color.FromArgb(0xFF, 0xF0, 0xF0); }
		}

		/// <summary>Gets color for result PQP data independently of phase</summary>
		public static Color ColorPkeResult
		{
			get { return Color.Ivory; }
		}

		/// <summary>Gets color for all standsrd PQP data independently of phase</summary>
		public static Color ColorPkeStandard
		{
			get { return Color.LavenderBlush; }
		}

		/// <summary>Gets color for datetime column</summary>
		public static Color ColorPqpParam
		{
			get { return Color.White; }
		}
	}
}
