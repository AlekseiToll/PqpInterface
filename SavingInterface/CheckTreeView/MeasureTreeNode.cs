using System;
using System.Collections.Generic;

using EmServiceLib;

namespace MainInterface.SavingInterface.CheckTreeView
{
	/// <summary>
	/// MeasureTreeNode class.
	/// Contains date end time of the start and end of data measuring
	/// </summary>
	public class MeasureTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		///  Type of measure
		/// </summary>
		public MeasureType NodeMeasureType;

		/// <summary>
		/// Inner measure index in tree
		/// </summary>
		public int MeasureTreeIndex;

		/// <summary>
		/// Date and time of measure was starts
		/// </summary>
		public DateTime StartDateTime;

		/// <summary>
		/// Date and time of measure was ends
		/// </summary>
		public DateTime EndDateTime;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="startDateTime">Date and time of measure was starts</param>
		/// <param name="endDateTime">Date and time of measure was ends</param>
		/// <param name="measureIndex">Inner measure index</param>
		/// <param name="measureType">Type (PQP, AVG or DNS)</param>
		public MeasureTreeNode(DateTime startDateTime, DateTime endDateTime, int measureTreeIndex,
								MeasureType measureType, bool enabled)
			: base(enabled)
		{
			this.StartDateTime = startDateTime;
			this.EndDateTime = endDateTime;
			this.MeasureTreeIndex = measureTreeIndex;
			this.Tag = "Measure";
			this.NodeMeasureType = measureType;
		}

		#endregion
	}

	public class MeasureTreeNodePqp : MeasureTreeNode
	{
		#region Fields

		/// <summary>
		///  Index of PQP archive in the device
		/// </summary>
		public uint PqpIndex;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="StartDateTime">Date and time of measure was starts</param>
		/// <param name="EndDateTime">Date and time of measure was ends</param>
		/// <param name="MeasureIndex">Inner measure index</param>
		public MeasureTreeNodePqp(DateTime startDateTime, DateTime endDateTime, int measureTreeIndex,
								MeasureType curMeasureType, uint pqpIndex, bool enabled) :
			base(startDateTime, endDateTime, measureTreeIndex, curMeasureType, enabled)
		{
			this.PqpIndex = pqpIndex;
		}

		#endregion
	}

	public class MeasureTreeNodeAvg : MeasureTreeNode
	{
		#region Fields

		/// <summary>
		///  Index of PQP archive in the device
		/// </summary>
		public AvgTypes AvgType;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="StartDateTime">Date and time of measure was starts</param>
		/// <param name="EndDateTime">Date and time of measure was ends</param>
		/// <param name="MeasureIndex">Inner measure index</param>
		public MeasureTreeNodeAvg(DateTime startDateTime, DateTime endDateTime, int measureTreeIndex,
								MeasureType curMeasureType, AvgTypes avgType, bool enabled) :
			base(startDateTime, endDateTime, measureTreeIndex, curMeasureType, enabled)
		{
			this.AvgType = avgType;
		}

		#endregion
	}
}