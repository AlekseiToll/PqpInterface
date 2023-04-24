using System;
using System.Windows.Forms;
using EmServiceLib;

namespace MainInterface.SavingInterface.CheckTreeView
{
	public enum EmCheckState
	{
		// The control is unchecked.
		UNCHECKED = 0,
		// The control is checked.
		CHECKED = 1,
		// The control is indeterminate. An indeterminate control generally has a shaded
		// appearance.
		INDETERMINATE = 2,
		// disable for checking
		DISABLED = 3
	}

	/// <summary>
	/// CheckTreeNode class.
	/// All other types of nodes are inherited from thus class
	/// </summary>
	public class CheckTreeNode : TreeNode
	{
		#region Fields

		/// <summary>
		/// Checked State
		/// </summary>
		private EmCheckState checkState_;

		#endregion

		#region Properties

		/// <summary>
		/// Gets checked state
		/// </summary>
		public EmCheckState CheckState
		{
			get
			{
				return checkState_;
			}
		}

		#endregion

		#region Constructors

		//public CheckTreeNode()
		//{
		//	SetState(this, EmCheckState.UNCHECKED);
		//}

		public CheckTreeNode(bool enabled)
		{
			if(enabled)
				SetState(this, EmCheckState.UNCHECKED);
			else
				SetState(this, EmCheckState.DISABLED);
		}

		#endregion

		#region Private methods

		private void SetState(CheckTreeNode node, EmCheckState checkState)
		{
			node.checkState_ = checkState;
			// Images
			if (checkState == EmCheckState.UNCHECKED)
			{
				node.ImageIndex = 0;
				node.SelectedImageIndex = 0;
			}
			else if (checkState == EmCheckState.CHECKED)
			{
				node.ImageIndex = 1;
				node.SelectedImageIndex = 1;
			}
			else if (checkState == EmCheckState.INDETERMINATE)
			{
				node.ImageIndex = 2;
				node.SelectedImageIndex = 2;
			}
			else if (checkState == EmCheckState.DISABLED)
			{
				node.ImageIndex = 3;
				node.SelectedImageIndex = 3;
			}
		}

		private EmCheckState AnalyseState(CheckTreeNode node)
		{
			if(node.CheckState == EmCheckState.DISABLED) 
				return EmCheckState.DISABLED;

			int iChecked = 0;
			int iUnchecked = 0;
			int iNumOfCheckTreeNode = node.Nodes.Count;

			for (int i = 0; i < node.Nodes.Count; i++)
			{
				// to skip if child is not CheckTreeNode
				if (!(node.Nodes[i] is CheckTreeNode))
				{
					iNumOfCheckTreeNode--;
					continue;
				}

				if ((node.Nodes[i] as CheckTreeNode).CheckState == EmCheckState.CHECKED)
				{
					iChecked++;
				}
				else if ((node.Nodes[i] as CheckTreeNode).CheckState == EmCheckState.UNCHECKED ||
						(node.Nodes[i] as CheckTreeNode).CheckState == EmCheckState.DISABLED)
				{
					iUnchecked++;
				}
			}

			if (iChecked == iNumOfCheckTreeNode) return EmCheckState.CHECKED;
			if (iUnchecked == iNumOfCheckTreeNode) return EmCheckState.UNCHECKED;
			return EmCheckState.INDETERMINATE;
		}

		private void UpdateParent(CheckTreeNode node)
		{
			try
			{
				if (node.Parent == null) return;
				//if ( !(Node.Parent is CheckTreeNode) ) return;

				SetState((node.Parent as CheckTreeNode), AnalyseState(node.Parent as CheckTreeNode));
				UpdateParent((node.Parent as CheckTreeNode));
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in UpdateParent():");
				throw;
			}
		}

		private void UpdateChilds(CheckTreeNode node, EmCheckState checkState)
		{
			try
			{
				bool enabledChildExists = false;
				for (int i = 0; i < node.Nodes.Count; i++)
				{
					if (!(node.Nodes[i] is CheckTreeNode)) continue;
					if ((node.Nodes[i] as CheckTreeNode).CheckState == EmCheckState.DISABLED) continue;

					enabledChildExists = true;
					SetState((node.Nodes[i] as CheckTreeNode), checkState);
					UpdateChilds((node.Nodes[i] as CheckTreeNode), checkState);
				}
				if(!enabledChildExists && node.Nodes.Count > 0)
					SetState(node, EmCheckState.DISABLED);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in UpdateChilds():");
				throw;
			}
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Check node (and all child nodes recursively)
		/// </summary>
		public void Check()
		{
			if (checkState_ == EmCheckState.DISABLED)
				return;

			// this
			SetState(this, EmCheckState.CHECKED);

			// child
			UpdateChilds(this, EmCheckState.CHECKED);

			// parent
			UpdateParent(this);
		}

		/// <summary>
		/// Uncheck node (and all child nodes recursively)
		/// </summary>
		public void Uncheck()
		{
			if (checkState_ == EmCheckState.DISABLED)
				return;

			// this
			SetState(this, EmCheckState.UNCHECKED);

			// child
			UpdateChilds(this, EmCheckState.UNCHECKED);

			// parent
			UpdateParent(this);
		}

		public bool IsChecked()
		{
			return checkState_ == EmCheckState.CHECKED || checkState_ == EmCheckState.INDETERMINATE;
		}

		#endregion
	}

}