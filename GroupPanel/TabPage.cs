using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace Morell.GroupPanel
{
	/// <summary>
	/// Tabpage that holds the control
	/// </summary>
	[ToolboxItem(false)]
	public class TabPage : Panel
	{
		internal Control _childControl;					// Tabpage child control reference
		
		// Coordinates for the outter rectangle
		private int _x0;
		private int _y0;
		private int _x1;
		private int _y1;

		/// <summary>
		/// Constructor
		/// </summary>
		public TabPage()
		{
			// Prevent redrawing flicker
			SetStyle(ControlStyles.ContainerControl, true);
			SetStyle(ControlStyles.FixedHeight, false);
			SetStyle(ControlStyles.FixedWidth, false);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="text">Tabpage text</param>
		public TabPage(string text) : this()
		{
			// Set a reference to the text
			_text = text;
		}
		
		/// <summary>
		/// Adds the control that the tabPage holds
		/// </summary>
		/// <param name="control">Control to add</param>
		public void AddControl(Control control)
		{
			// Set the reference to the control
			_childControl = control;

			// Add the control to the controls collection
			this.Controls.Add(_childControl);
			_childControl.Dock = DockStyle.Fill;
			
			// BUG: Detected on Treeview.
			// If the control is not Hidden and Shown, the treeview 
			// always presents and horizontal scroll bar that is not necessary.
			// If the Windows XP style is on, the scrollbar does not have the XP look.
//			_childControl.Hide();
//			_childControl.Show();
			_childControl.Width = 0;
		}

		/// <summary>
		/// Removes the control from the tabpage
		/// </summary>
		internal void ReleaseControl()
		{
			_childControl.Visible = false;
			_childControl = null;
		}

		/// <summary>
		/// Sets the focus to the control
		/// </summary>
		internal void SetControlFocus()
		{
			if (_childControl != null)
				_childControl.Focus();
		}

		/// <summary>
		/// Sets the control's coordinates
		/// </summary>
		/// <param name="x0">Left coordinate</param>
		/// <param name="y0">Top coordinate</param>
		/// <param name="x1">Right coordinate</param>
		/// <param name="y1">Bottom coordinate</param>
		internal void SetCoordinates(int x0, int y0, int x1, int y1)
		{
			_x0 = x0;
			_y0 = y0;
			_x1 = x1;
			_y1 = y1;
		}

		/// <summary>
		/// Gets the control's coordinates
		/// </summary>
		/// <param name="x0">Left coordinate</param>
		/// <param name="y0">Top coordinate</param>
		/// <param name="x1">Right coordinate</param>
		/// <param name="y1">Bottom coordinate</param>
		internal void GetCoordinates(out int x0, out int y0, out int x1, out int y1)
		{
			x0 = _x0;
			y0 = _y0;
			x1 = _x1;
			y1 = _y1;
		}

		/// <summary>
		/// Gets if the x,y coordinates are over the control
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>True if the x,y coordinates are over the control</returns>
		internal bool HitTest(int x, int y)
		{
			if (x >= _x0 && x <= _x1)
				if (y >= _y0 && y <= _y1)
					// It is over the control
					return true;

			// It is not over the control
			return false;
		}

		#region Properties

		private string _text;
		private int _imageIndex = -1;

		/// <summary>
		/// Gets or sets the text
		/// </summary>
		public override string Text
		{
			get { return _text; }
			set
			{
				if (_text != value)
				{
					string oldValue = _text;
					_text = value;
					OnPropertyChanged(Property.Text, oldValue);
				}
			}
		}

		/// <summary>
		/// Gets or sets the image index
		/// </summary>
		public int ImageIndex
		{
			get { return _imageIndex; }
			set
			{
				if (_imageIndex != value)
				{
					int oldValue = _imageIndex;
					_imageIndex = value;
					OnPropertyChanged(Property.ImageIndex, _imageIndex);
				}
			}
		}

		/// <summary>
		/// Gets the control holded by the tabpage
		/// </summary>
		public Control Control
		{
			get { return _childControl; }
		}

		#endregion

		#region Events

		public event PropChangeHandler PropertyChanged;

		/// <summary>
		/// A tabPage property has changed
		/// </summary>
		/// <param name="prop">Changed property</param>
		/// <param name="oldValue">Value before the change</param>
		public void OnPropertyChanged(Property prop, object oldValue)
		{
			// Is the event registered?
			if (PropertyChanged != null)
				// Raise the event
				PropertyChanged(this, prop, oldValue);
		}
		
		#endregion
	}
}
