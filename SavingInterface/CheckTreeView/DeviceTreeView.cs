using System;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;
using System.Collections.Generic;

using DeviceIO;
using EmServiceLib;
using FileAnalyzerLib;

namespace MainInterface.SavingInterface.CheckTreeView
{
	/// <summary>
	/// DeviceTreeView class (to show contents of device data)
	/// </summary>
	public class DeviceTreeView: TreeView
	{
		//const int cntRecordsDnS_ = 16384;

		#region Fields

		internal bool EnableMouseChecks = true;

		private bool PrevCheckState = true;  //предыдущее состояние чекбокса "Осн. показатели" для 
											//управления галочками

		#endregion

		#region Events

		/// <summary>Delegate to describe functions of events NothingChecked and SomthingChecked</summary>
		public delegate void CheckedHandler();

		/// <summary>Event occures when all items were unchecked</summary>
		public static event CheckedHandler NothingChecked;

		/// <summary>Event occures when one of items wes checked</summary>
		public static event CheckedHandler SomethingChecked;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceTreeView(){ }

		#endregion

		#region Public methods

		/// <summary>
		/// Imports data from contents (for ETPQP-A)
		/// </summary>
		internal void ImportDataFromContents(ref DeviceCommonInfo devInfo, ref StoredArchivesInfo storedArchivesInfo)
		{
			try
			{
				// check - if Contents is empty
				if (devInfo.Content == null || devInfo.Content.Count == 0) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode(true);
				root.Text = String.Format("ETPQP-A #{0} (v.{1})", devInfo.SerialNumber,
					devInfo.DevVersion);
				root.Tag = "Device";
				this.Nodes.Add(root);

				EmService.WriteToLogDebug("ImportDataFromContents before creating child");
				// creating childs
				for (int iReg = 0; iReg < devInfo.Content.Count; iReg++)
				{
					// creating object node
					RegistrationTreeNode reg = new RegistrationTreeNode(iReg, 
															devInfo.Content[iReg].RegistrationName,
															devInfo.Content[iReg].RegistrationId);

					// creating PQP measures
					EmService.WriteToLogDebug("ImportDataFromContents before creating pqp");
					if (devInfo.Content[iReg].PqpCnt > 0)
					{
						for (int iPqp = 0; iPqp < devInfo.Content[iReg].PqpSet.Count; iPqp++)
						{
							bool enabled = !storedArchivesInfo.IsPqpArchiveExists(devInfo.SerialNumber,
							                        devInfo.Content[iReg].RegistrationId,
							                        devInfo.Content[iReg].PqpSet[iPqp].PqpIndex);

							reg.AddMeasurePqp(devInfo.Content[iReg].PqpSet[iPqp].PqpStart,
											devInfo.Content[iReg].PqpSet[iPqp].PqpEnd,
											iPqp,
											devInfo.Content[iReg].PqpSet[iPqp].PqpIndex,
											enabled);
						}
					}

					// creating AVG measures
					EmService.WriteToLogDebug("ImportDataFromContents before creating avg");
					if (devInfo.Content[iReg].AvgExists)
					{
						bool exists3sec =
							devInfo.Content[iReg].AvgDataStart[(int)AvgTypes.ThreeSec].dtStart != DateTime.MinValue
							&&
							devInfo.Content[iReg].AvgDataStart[(int)AvgTypes.ThreeSec].dtEnd != DateTime.MinValue;
						bool exists10min =
							devInfo.Content[iReg].AvgDataStart[(int)AvgTypes.TenMin].dtStart != DateTime.MinValue
							&&
							devInfo.Content[iReg].AvgDataStart[(int)AvgTypes.TenMin].dtEnd != DateTime.MinValue;
						bool exists2hour =
							devInfo.Content[iReg].AvgDataStart[(int)AvgTypes.TwoHours].dtStart != DateTime.MinValue
							&&
							devInfo.Content[iReg].AvgDataStart[(int)AvgTypes.TwoHours].dtEnd != DateTime.MinValue;

						if (exists3sec)
						{
							bool enabled = !storedArchivesInfo.IsAvgArchiveExists(devInfo.SerialNumber,
													devInfo.Content[iReg].RegistrationId,
													AvgTypes.ThreeSec);
							reg.AddMeasureAvg(devInfo.Content[iReg].AvgDataStart[(int) AvgTypes.ThreeSec].dtStart,
							                  devInfo.Content[iReg].AvgDataEnd[(int) AvgTypes.ThreeSec].dtEnd,
							                  0, AvgTypes.ThreeSec, enabled);
						}
						if (exists10min)
						{
							bool enabled = !storedArchivesInfo.IsAvgArchiveExists(devInfo.SerialNumber,
													devInfo.Content[iReg].RegistrationId,
													AvgTypes.TenMin);
							reg.AddMeasureAvg(devInfo.Content[iReg].AvgDataStart[(int) AvgTypes.TenMin].dtStart,
							                  devInfo.Content[iReg].AvgDataEnd[(int) AvgTypes.TenMin].dtEnd,
							                  1, AvgTypes.TenMin, enabled);
						}
						if (exists2hour)
						{
							bool enabled = !storedArchivesInfo.IsAvgArchiveExists(devInfo.SerialNumber,
													devInfo.Content[iReg].RegistrationId,
													AvgTypes.TwoHours);
							reg.AddMeasureAvg(devInfo.Content[iReg].AvgDataStart[(int) AvgTypes.TwoHours].dtStart,
							                  devInfo.Content[iReg].AvgDataEnd[(int) AvgTypes.TwoHours].dtEnd,
							                  2, AvgTypes.TwoHours, enabled);
						}
					}

					// creating DNS measures
					EmService.WriteToLogDebug("ImportDataFromContents before creating dns");
					bool enabledDns = !storedArchivesInfo.IsDnsArchiveExists(devInfo.SerialNumber,
													devInfo.Content[iReg].RegistrationId);
					reg.AddMeasureDns(devInfo.Content[iReg].CommonBegin, devInfo.Content[iReg].CommonEnd, 0, enabledDns);

					reg.Text = reg.Name + " " + devInfo.Content[iReg].CommonBegin.ToString() + " - " +
						devInfo.Content[iReg].CommonEnd.ToString();

					root.Nodes.Add(reg);
				}
				//root.Check();
				root.ExpandAll();
				root.Uncheck();		// we call it to mark parent nodes disabled if it's necessary
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromContents():");
				throw;
			}
		}

		//internal RegistrationTreeNode GetParentObject(TreeNode node)
		//{
		//    TreeNode tempNode = node;
		//    while (tempNode.Parent != null)
		//    {
		//        if (tempNode.Parent is RegistrationTreeNode) 
		//                return tempNode.Parent as RegistrationTreeNode;

		//        tempNode = tempNode.Parent;
		//    }
		//    return null;
		//}

		#endregion

		#region Overriden methods

		/// <summary>Checking/unchecking</summary>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Left && EnableMouseChecks)
				{
					TreeNode node = GetNodeAt(e.X, e.Y);
					if (node != null)
					{
						if (e.X > node.Bounds.Left - 16 && e.X < node.Bounds.Left - 3)
						{
							if ((node as CheckTreeNode).CheckState == EmCheckState.DISABLED)
								return;

							//node.Bounds
							if ((node as CheckTreeNode).CheckState == EmCheckState.UNCHECKED)
							{
								// implement check
								(node as CheckTreeNode).Check();
								// raising SomthingChecked event
								if (SomethingChecked != null) SomethingChecked();
							}
							else // Checked or Indeterminate
							{
								// implement uncheck
								(node as CheckTreeNode).Uncheck();
								// raising event
								if ((this.Nodes[0] as CheckTreeNode).CheckState == EmCheckState.UNCHECKED)
									if (NothingChecked != null) NothingChecked();
							}
						}
					}
				}
				base.OnMouseUp(e);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceTreeView::OnMouseUp():");
				throw;
			}
		}

		#endregion
	}
}