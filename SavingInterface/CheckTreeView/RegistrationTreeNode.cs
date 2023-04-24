using System;
using System.Resources;

using EmServiceLib;

namespace MainInterface.SavingInterface.CheckTreeView
{
	/// <summary>
	/// RegistrationTreeNode class. 
	/// Contains object names 
	/// (one of several main records of the device) and times of measuring
	/// </summary>
	public class RegistrationTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		/// Number of this registration in EnergomonitorIO.ContentLines array
		/// </summary>
		private int number_;

		private uint registrationId_;

		#endregion

		#region Properties

		/// <summary>
		/// Gets inner index number
		/// </summary>
		public int Number
		{
			get { return number_; }
		}

		/// <summary>
		/// Registration Id
		/// </summary>
		public uint RegistrationId
		{
			get { return registrationId_; }
		}

		/// <summary>
		/// Gets name
		/// </summary>
		public string NodeName
		{
			get { return this.Name; }
		}

		#endregion

		#region Constructors

		/// <summary>Constructor</summary>
		/// <param name="number">Inner index number</param>
		/// <param name="nodeName">Name</param>
		/// <param name="regId">Registration Id</param>
		public RegistrationTreeNode(int number, string nodeName, uint regId) : base(true)
		{
			this.Tag = "Registration";
			this.number_ = number;
			this.Name = nodeName;
			this.registrationId_ = regId;
		}

		#endregion

		#region Public methods

		/// <summary>Add measure type node</summary>
		/// <param name="measureType">Type of measure</param>
		private int AddMeasureType(MeasureType measureType)
		{
			try
			{
				int iNeededMeasureTypeIndex = -1;

				// searching node with needes MeasureType
				for (int i = 0; i < this.Nodes.Count; i++)
				{
					if ((this.Nodes[i] as MeasureTypeTreeNode).NodeMeasureType == measureType)
					{
						iNeededMeasureTypeIndex = i;
					}
				}

				// if searching node no find we had to create it
				if (iNeededMeasureTypeIndex == -1)
				{
					MeasureTypeTreeNode mtNode = new MeasureTypeTreeNode(measureType);
					ResourceManager rm = new ResourceManager("MainInterface.emstrings", this.GetType().Assembly);

					switch (measureType)
					{
						case MeasureType.PQP:
						{
							mtNode.Text = rm.GetString("name_pke_full");
							break;
						}
						case MeasureType.AVG:
						{
							mtNode.Text = rm.GetString("name_avg_full");
							break;
						}
						case MeasureType.DNS:
						{
							mtNode.Text = rm.GetString("name_dns_full");
							break;
						}
					}
					this.Nodes.Add(mtNode);
					iNeededMeasureTypeIndex = this.Nodes.Count - 1;
				}

				return iNeededMeasureTypeIndex;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in AddMeasureType():");
				throw;
			}
		}

		/// <summary>Add measure PQP</summary>
		/// <param name="startDateTime">Date and time of measure was starts</param>
		/// <param name="endDateTime">Date and time of measure was ends</param>
		/// <param name="measureIndex">Inner measure index</param>
		/// <param name="pqpIndex">Index of PQP archive in the device</param>
		public void AddMeasurePqp(DateTime startDateTime, DateTime endDateTime, int measureIndex, uint pqpIndex, bool enabled)
		{
			try
			{
				MeasureTreeNode mNode = new MeasureTreeNodePqp(startDateTime, endDateTime, measureIndex,
				                                               MeasureType.PQP, pqpIndex, enabled);
				mNode.Text = startDateTime.ToString() + " - " + endDateTime.ToString();

				int iNeededMeasureTypeIndex = AddMeasureType(MeasureType.PQP);
				this.Nodes[iNeededMeasureTypeIndex].Nodes.Add(mNode);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in AddMeasurePqp():");
				throw;
			}
		}

		/// <summary>Add measure AVG</summary>
		/// <param name="startDateTime">Date and time of measure was starts</param>
		/// <param name="endDateTime">Date and time of measure was ends</param>
		/// <param name="measureIndex">Inner measure index</param>
		/// <param name="avgType">AVG type</param>
		public void AddMeasureAvg(DateTime startDateTime, DateTime endDateTime, int measureIndex, AvgTypes avgType, bool enabled)
		{
			try
			{
				MeasureTreeNode mNode = new MeasureTreeNodeAvg(startDateTime, endDateTime, measureIndex,
				                                               MeasureType.AVG, avgType, enabled);
				mNode.Text = startDateTime.ToString() + " - " + endDateTime.ToString();
				switch (avgType)
				{
					case AvgTypes.ThreeSec: mNode.Text += "  3 sec"; break;
					case AvgTypes.TenMin: mNode.Text += "  10 min"; break;
					case AvgTypes.TwoHours: mNode.Text += "  2 hour"; break;
				}

				int iNeededMeasureTypeIndex = AddMeasureType(MeasureType.AVG);
				this.Nodes[iNeededMeasureTypeIndex].Nodes.Add(mNode);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in AddMeasureAvg():");
				throw;
			}
		}

		/// <summary>Add measure DNS</summary>
		/// <param name="startDateTime">Date and time of measure was starts</param>
		/// <param name="endDateTime">Date and time of measure was ends</param>
		/// <param name="measureIndex">Inner measure index</param>
		public void AddMeasureDns(DateTime startDateTime, DateTime endDateTime, int measureIndex, bool enabled)
		{
			try
			{
				MeasureTreeNode mNode = new MeasureTreeNode(startDateTime, endDateTime, measureIndex, MeasureType.DNS, enabled);
				mNode.Text = startDateTime.ToString() + " - " + endDateTime.ToString();

				int iNeededMeasureTypeIndex = AddMeasureType(MeasureType.DNS);
				this.Nodes[iNeededMeasureTypeIndex].Nodes.Add(mNode);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in AddMeasure():");
				throw;
			}
		}

		#endregion
	}
}