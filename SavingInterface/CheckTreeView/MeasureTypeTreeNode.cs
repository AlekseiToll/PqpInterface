using System;

namespace MainInterface.SavingInterface.CheckTreeView
{
	/// <summary>
	/// MeasureTypeTreeNode class.
	/// Contains global mesasure types such as PQP, AVG etc..
	/// </summary>
	public class MeasureTypeTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		/// Type of measure
		/// </summary>
		public MeasureType NodeMeasureType;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="MeasureType">Type of creating measure</param>
		public MeasureTypeTreeNode(MeasureType measureType) : base(true)
		{
			this.Tag = "MeasureType";
			this.NodeMeasureType = measureType;
		}

		#endregion
	}
}