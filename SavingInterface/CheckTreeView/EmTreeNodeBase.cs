using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using EmServiceLib;

namespace MainInterface.SavingInterface.EmArchiveTree
{
	public abstract class EmTreeNodeBase : TreeNode
	{
		#region Fields

		protected EmTreeNodeType nodeType_;

		#endregion

		#region Constructor

		/// <summary>Public constructor</summary>
		/// <param name="nodeType">Node Type</param>
		public EmTreeNodeBase(EmTreeNodeType nodeType)
		{
			this.nodeType_ = nodeType;
		}

		public EmTreeNodeBase()
		{
			this.nodeType_ = EmTreeNodeType.Folder;
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets node type</summary>
		public EmTreeNodeType NodeType
		{
			get { return nodeType_; }
			set { nodeType_ = value; }
		}

		/// <summary>Gets PostgreSQL Node index</summary>
		public int PgServerIndex
		{
			get
			{
				TreeNode tn = this;
				while (tn.Parent != null)
				{
					tn = tn.Parent;
				}
				return tn.Index;
			}
		}

		/// <summary>Gets PostgreSQL Node item</summary>
		public TreeNode PgServerNode
		{
			get
			{
				TreeNode tn = this;
				while (tn.Parent != null)
				{
					tn = tn.Parent;
				}
				return tn;
			}
		}

		#endregion

		#region Overriden methods

		/// <summary>Overriden method Clone()</summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeBase)node).NodeType = this.NodeType;
			return node;
		}

		#endregion
	}

	/// <summary>Base class for all archive nodes</summary>
	public abstract class EmArchNodeBase : EmTreeNodeBase
	{
		#region Fields

		protected ConnectScheme connectionScheme_ = ConnectScheme.Unknown;

		protected EmArchNodeBase parentDevice_;
		protected EmArchNodeBase parentObject_;

		/// <summary>State showing is the measure active (opened) now</summary>
		protected bool active_;
		/// <summary>Background color of inactive (closed) node</summary>
		protected System.Drawing.Color DefaultColor = System.Drawing.Color.White;
		/// <summary>Background color of active (opened) node</summary>
		protected System.Drawing.Color ActiveColor = System.Drawing.Color.NavajoWhite;

		#endregion

		#region Properties

		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme_; }
			set { connectionScheme_ = value; }
		}

		public EmArchNodeBase ParentDevice
		{
			get { return parentDevice_; }
			set { parentDevice_ = value; }
		}

		public EmArchNodeBase ParentObject
		{
			get { return parentObject_; }
			set { parentObject_ = value; }
		}

		#endregion

		#region Constructors

		/// <summary>Constructor</summary>
		public EmArchNodeBase(EmTreeNodeType nodeType,
							EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(nodeType)
		{
			this.parentDevice_ = parentDev;
			this.parentObject_ = parentObj;
		}

		public EmArchNodeBase() : base(EmTreeNodeType.Folder)
		{
		}

		#endregion

		#region Overriden methods

		/// <summary>Overriden method Clone()</summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmArchNodeBase)node).ParentDevice = this.ParentDevice;
			((EmArchNodeBase)node).ParentObject = this.ParentObject;
			return node;
		}

		#endregion
	}
}
