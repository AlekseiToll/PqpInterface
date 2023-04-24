using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using EmServiceLib;

namespace Morell.GroupPanel
{
	/// <summary>
	/// GroupPanel user control
	/// </summary>
	[ToolboxBitmap(typeof(GroupPanel),"GroupPanel.bmp")] 
	public class GroupPanel : System.Windows.Forms.UserControl
	{
		// Events
		public event LabelEditEventHandler AfterLabelEdit;
		public event LabelEditEventHandler BeforeLabelEdit;
		public event MouseEventHandler TabPageMouseUp;
		public event EventHandler SelectedIndexChanged;
		
		// tabPages collection
		private TabPageCollection _tabPages;

		// Control colors
		private Color _backIDE;
		private Color _backColor;
		private Color _backDark;
		private Color _backDarkDark;
		private Color _backLight;
		private Color _backLightLight;
		
		// Mouse control
		private int _pressedIndex = -1;					// Index of the presses panel (but not shown)
		private int _pressedIndexRightButton = -1;		// Index of the presses panel (but not shown) for the right button
		private bool _mouseOnButton = false;			// Indicates if the mouse is over the pressed panel
		private int _lastPanelShown = -1;				// Keeps tracks of the shown panel, so it is not reshown unnecessarily
		private bool _mouseButtonJustReleased = false;	// Prevents fire on the MouseMove just after the MouseUp
		private int _hotTrackIndex = -1;				// Keeps track of the panel that the mouse is over
		private MouseButtons _pressedButton;			// Button pressed on MouseDown
		private bool _selectionJustChanged = false;		// Used to set the focus to the control just after the panel is changed with the mouse

		// Bitmaps
		private Bitmap _downBitmap = null;
		private Bitmap _upBitmap = null;
		private bool _bitmapHotSpot = false;
		private int _bitmapX0;
		private int _bitmapX1;
		private int _bitmapY0;

		// Edit
		//private TextBox _textBox = null;				// Textbox control used to edit the tabPage text
		//private bool _editting = false;					// True if a tabPage's text is been edited
		//private bool _temporalDelete = false;			// Used to move the panels without deleting the controls
		//private int _editIndex;							// Index of the tabPage been edited
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public GroupPanel()
		{
			// Prevent flicker with double buffering and all painting inside WM_PAINT
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			
			// Initialize the tabPages collection
			_tabPages = new TabPageCollection();
			_tabPages.Added += new CollectionChange(this.Tab_Added);
			//_tabPages.Deleted += new CollectionChange(this.Tab_Deleted);
			_tabPages.Clearing += new CollectionClear(this.Tab_Clearing);

			// Set the default color
			DefineBackColor(this.BackColor);
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		#region Tabpages collection modified

		/// <summary>
		/// A tabPage was added
		/// </summary>
		/// <param name="index">Tabpage index</param>
		/// <param name="tabPage">Added tabPage</param>
		private void Tab_Added(int index, TabPage tabPage)
		{
			try
			{
				// Catch Events
				tabPage.Visible = false;
				tabPage.PropertyChanged += new PropChangeHandler(this.Tab_Changed);
				//tabPage.StartEdit += new EventHandler(this.StartEdit);

				// Add the tabPage to the controls collection
				Controls.Add(tabPage);
				_lastPanelShown = -1;

				// Mantaint the previous selected tab
				if (index == _selectedIndex)
					_selectedIndex = _selectedIndex + 1;

				// Refresh the control
				Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::Tab_Added:");
				throw;
			}
		}
		
		/// <summary>
		/// The tabPages is been cleared
		/// </summary>
		private void Tab_Clearing()
		{
			try
			{
				// Unhook the events and remove from the controls collection each tabPage
				foreach (TabPage tabPage in _tabPages)
				{
					tabPage.Visible = false;
					tabPage.PropertyChanged -= new PropChangeHandler(this.Tab_Changed);
					//tabPage.StartEdit -= new EventHandler(this.StartEdit);
					tabPage.ReleaseControl();

					// Remove the page
					Controls.Remove(tabPage);
				}

				// Refresh the control
				_lastPanelShown = -1;
				_selectedIndex = -1;
				Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::Tab_Clear:");
				throw;
			}
		}

		#endregion

		#region Paint

		/// <summary>
		/// A tabPage property was changed
		/// </summary>
		/// <param name="tabPage">Tabpage</param>
		/// <param name="prop">Property changed</param>
		/// <param name="oldValue">Original value before the change</param>
		private void Tab_Changed(TabPage tabPage, Property prop, object oldValue)
		{
			// Refresh the control
			Invalidate();
		}
		
		/// <summary>
		/// Defines the control's colors base on the parameter color
		/// </summary>
		/// <param name="newColor">Base color</param>
		protected void DefineBackColor(Color newColor)
		{
			try
			{
				base.BackColor = newColor;

				// Calculate the modified colors from this base
				_backColor = newColor;
				_backLight = ControlPaint.Light(newColor);
				_backLightLight = ControlPaint.LightLight(newColor);
				_backDark = ControlPaint.Dark(newColor);
				_backDarkDark = ControlPaint.DarkDark(newColor);
				// Check for the 'Classic' control color
				if ((this.BackColor.R == 212) &&
				    (this.BackColor.G == 208) &&
				    (this.BackColor.B == 200))
				{
					// Use the exact background for this color
					_backIDE = Color.FromArgb(247, 243, 233);
				}
				else
				{
					// Check for the 'XP' control color
					if ((this.BackColor.R == 236) &&
					    (this.BackColor.G == 233) &&
					    (this.BackColor.B == 216))
					{
						// Use the exact background for this color
						_backIDE = Color.FromArgb(255, 251, 233);
					}
					else
					{
						// Calculate the IDE background color as only half as dark as the control color
						int red = 255 - ((255 - newColor.R) / 2);
						int green = 255 - ((255 - newColor.G) / 2);
						int blue = 255 - ((255 - newColor.B) / 2);
						_backIDE = Color.FromArgb(red, green, blue);
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::DefineBackColor:");
				throw;
			}
		}
		
		/// <summary>
		/// The control needs to be redrawn
		/// </summary>
		/// <param name="e">PaintEventArgs</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				// Get a reference to the graphics
				Graphics g = e.Graphics;
				
				// Create the brushes and pens
				Brush brushText = null;
				Brush brushLight = new SolidBrush(_backLight);
				Brush brushDark = new SolidBrush(_backDark);
				Pen penLight = new Pen(brushLight, 1);
				Pen penDark = new Pen(brushDark, 1);
				
				// Text measurements
				int textHeight = (int) g.MeasureString("bg", this.Font).Height;
				int textOffset = (_tabHeight - textHeight)/2;
				
				// Draw the border
				if (_borderStyle == BorderStyle.FixedSingle)
					g.DrawRectangle(Pens.Black, 0, 0, this.Width-1, this.Height-1);
				else if (_borderStyle == BorderStyle.Fixed3D)
				{
					Brush brushLightLight = new SolidBrush(_backLightLight);
					Brush brushDarkDark = new SolidBrush(_backDarkDark);
					Pen penLightLight = new Pen(brushLightLight, 1);
					Pen penDarkDark = new Pen(brushDarkDark, 1);
					
					g.DrawLine(penDark, 0, 0, this.Width-2, 0);
					g.DrawLine(penDark, 0, 0, 0, this.Height-2);
					g.DrawLine(penDarkDark, 1, 1, this.Width-3, 1);
					g.DrawLine(penDarkDark, 1, 1, 1, this.Height-3);
					
					g.DrawLine(penLightLight, 0, this.Height-1, this.Width-1, this.Height-1);
					g.DrawLine(penLightLight, this.Width-1, this.Height-1, this.Width-1, 0);
					g.DrawLine(penLight, 1, this.Height-2, this.Width-2, this.Height-2);
					g.DrawLine(penLight, this.Width-2, this.Height-2, this.Width-2, 1);
					
					penLightLight.Dispose();
					penDarkDark.Dispose();
					brushDarkDark.Dispose();
					brushLightLight.Dispose();

				}	// if (_borderStyle == BorderStyle.FixedSingle)
				
				// Get the offsets based on the border type
				int hOffset;
				int vOffset;
				if (_borderStyle == BorderStyle.None)
				{
					hOffset = -1;
					vOffset = -1;
				}
				else if (_borderStyle == BorderStyle.FixedSingle)
				{
					hOffset = 0;
					vOffset = 0;
				}
				else
				{
					hOffset = 1;
					vOffset = 1;
				}
				
				int index = -1;
				int top = vOffset + 1;
				int oldTop;
				int panelTop = 0;
				int panelBottom = 0;
				int fitWidth;
				int textX2;
				
				// Calculate the text position based on the bitmap
				if (_bitmapHotSpot)
					textX2 = this.Width - hOffset - 2 - _downBitmap.Width * 2;
				else
					textX2 = this.Width - hOffset - 2;
				
				// Draw each tabPage
				foreach (TabPage tabPage in _tabPages)
				{
					index++;
				
					// Draw the rectangle
					if (this.ColorLeft != this.BackColor || this.ColorRight != this.BackColor )
					{
						Rectangle rect = new Rectangle(hOffset+1, top, this.Width - hOffset*2 - 1, _tabHeight);
						LinearGradientBrush brush = new LinearGradientBrush(rect, _colorLeft, _colorRight, 0, false);
						g.FillRectangle(brush, rect);
						brush.Dispose();
					}
					
					if (index == _pressedIndexRightButton && _mouseOnButton)
					{
						g.DrawLine(penDark, hOffset+1, top, this.Width - hOffset - 2, top);
						g.DrawLine(penDark, hOffset+1, top, hOffset+1, top + _tabHeight -1);
						g.DrawLine(penLight, this.Width - hOffset-2, top+1, this.Width - hOffset-2, top + _tabHeight - 1);
						g.DrawLine(penLight, hOffset + 2, top + _tabHeight-1, this.Width - hOffset-2, top + _tabHeight - 1);
					}
					else
					{
						g.DrawLine(penLight, hOffset+1, top, this.Width - hOffset - 2, top);
						g.DrawLine(penLight, hOffset+1, top, hOffset+1, top + _tabHeight -1);
						g.DrawLine(penDark, this.Width - hOffset-2, top+1, this.Width - hOffset-2, top + _tabHeight - 1);
						g.DrawLine(penDark, hOffset + 2, top + _tabHeight-1, this.Width - hOffset-2, top + _tabHeight - 1);
					}

					if (_bitmapHotSpot)
					{
						_bitmapX0 = this.Width-_downBitmap.Width-8;
						_bitmapX1 = _bitmapX0 + _downBitmap.Width;
						_bitmapY0 = (_tabHeight - _downBitmap.Height)/2 + top;
					}
					tabPage.SetCoordinates(hOffset+1, top, this.Width - hOffset - 2, top + _tabHeight -1);
				
					// Draw the image
					int textX;
					if (_imageList != null)
					{
						if (tabPage.ImageIndex > -1)
						{
							Bitmap bitmap = (Bitmap) _imageList.Images[tabPage.ImageIndex];
							bitmap.MakeTransparent(Color.Magenta);
							g.DrawImageUnscaled( bitmap, 4, top + (_tabHeight - _imageList.ImageSize.Height) / 2 );
						}
						textX = hOffset + 8 + _imageList.ImageSize.Width;
					}
					else
					{
						textX = hOffset + 4;
					}

					// Draw the text
					// Verify label lenght
					if (_hotTrack)
					{
						if (_hotTrackIndex == index)
							brushText = new SolidBrush(_hotTrackColor);
						else
							brushText = new SolidBrush(this.ForeColor);
					}
					else
						brushText = new SolidBrush(this.ForeColor);
					
					fitWidth = (int) g.MeasureString(tabPage.Text, this.Font).Width;
					if (fitWidth > textX2 - textX + 8)
						g.DrawString(ShrinkText(tabPage.Text, textX2 - textX - 5, g, this.Font) , this.Font, brushText, textX, top + textOffset);
					else
						g.DrawString(tabPage.Text, this.Font, brushText, textX, top + textOffset);
					
					oldTop = top;
					if (index == _selectedIndex)
					{
						panelTop = top + _tabHeight;
						top = (top + _tabHeight - 1) + (this.Height - vOffset * 2) - (index+1)*_tabHeight - (_tabPages.Count-1-index)*_tabHeight - 1;
						panelBottom = top;
						
						// Verify vertical space
						if (top < oldTop+_tabHeight*2)
						{
							panelBottom = this.Height - vOffset;
							break;
						}
					}
					else
					{
						// Images
						if (_bitmapHotSpot)
						{
							if (index < _selectedIndex)
								g.DrawImageUnscaled(_downBitmap, _bitmapX0, _bitmapY0);
							else
								g.DrawImageUnscaled(_upBitmap, this.Width-_upBitmap.Width-8, _bitmapY0);
						}

						top += _tabHeight;

					}	// if (index == _selectedIndex)

				}	// foreach (TabPage tabPage in _tabPages)

				// Dispose the brushes
				if (brushText != null) brushText.Dispose();
				if (brushLight != null) brushLight.Dispose();
				if (brushDark != null) brushDark.Dispose();
				
				try	// Prevents errors on design mode
				{
					// Make the panel visible
					if (_lastPanelShown != _selectedIndex)
					{
						_lastPanelShown = _selectedIndex;
						foreach (TabPage tabPage in _tabPages)
							tabPage.Visible = false;
						_tabPages[_selectedIndex].Left = hOffset+1;
						_tabPages[_selectedIndex].Width = this.Width - hOffset - 2;
						_tabPages[_selectedIndex].Top = panelTop;
						_tabPages[_selectedIndex].Height = panelBottom - panelTop;
						_tabPages[_selectedIndex].Visible = true;
						//_tabPages[_selectedIndex].SetControlFocus();
					}
					if (_selectionJustChanged)
					{
						_selectionJustChanged = false;
						_tabPages[_selectedIndex].SetControlFocus();
					}
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Exception in GroupPanel::OnPaint1:");
					//throw;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::OnPaint2:");
				//throw;
			}
		}

		/// <summary>
		/// The control was resized
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResize(System.EventArgs e)
		{
			// Force panel resize
			_lastPanelShown = -1;

			// Refresh the control
			Invalidate();
		}

		/// <summary>
		/// Sets the up and down bitmaps
		/// </summary>
		private void SetBitmaps()
		{
			try
			{
				_bitmapHotSpot = (_downImage != null) && (_upImage != null);

				if (_bitmapHotSpot)
				{
					_downBitmap = (Bitmap) _downImage;
					_downBitmap.MakeTransparent(_transparentColor);

					_upBitmap = (Bitmap) _upImage;
					_upBitmap.MakeTransparent(_transparentColor);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::SetBitmaps:");
				throw;
			}
		}
		
		/// <summary>
		/// Shrinks the text (adds dots) if it does not fit in the control widht
		/// </summary>
		/// <param name="text">Text to shrink</param>
		/// <param name="width">Available text widht, excluding the bitmap</param>
		/// <param name="g">Graphics</param>
		/// <param name="font">Tabpage font</param>
		/// <returns>Shrinked text</returns>
		private string ShrinkText(string text, int width, Graphics g, Font font)
		{
			int lenght = text.Length;
			try
			{
				while ( (int) g.MeasureString(text, font).Width > width)
				{
					lenght--;
					text = text.Substring(0, lenght);
				}
				return text + "...";
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::ShrinkText:");
				return "...";
			}
		}
		
		#endregion

		#region Events

		/// <summary>
		/// The selected tabPage changed
		/// </summary>
		private void OnSelectedIndexChanged()
		{
			// Is the event registered?
			if (SelectedIndexChanged != null)
				// Raise the event
				SelectedIndexChanged(_tabPages[_selectedIndex], new EventArgs());
		}

		#endregion
		
		#region Properties

		private int _selectedIndex = 0;
		private int _tabHeight = 16;
		private BorderStyle _borderStyle = BorderStyle.None;
		private Color _colorLeft;
		private Color _colorRight;
		private Image _downImage = null;
		private Image _upImage = null;
		private Color _transparentColor;
		private int _labelMaxLenght = 32767;
		private ImageList _imageList;
		private bool _hotTrack = true;
		private Color _hotTrackColor = SystemColors.HotTrack;

		/// <summary>
		/// Gets or sets the tabPages collection
		/// </summary>
		public TabPageCollection TabPages
		{
			get { return _tabPages; }
			set
			{
				_tabPages.Clear();
				_tabPages = value;
			}
		}

		/// <summary>
		/// Gets or sets the selected index tabPage
		/// </summary>
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				_selectedIndex = value;
				OnSelectedIndexChanged();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the tab height
		/// </summary>
		public int TabHeight
		{
			get { return _tabHeight; }
			set 
			{
				if (_tabHeight != value)
				{
					_tabHeight = value;
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the border style
		/// </summary>
		public BorderStyle BorderStyle
		{
			get { return _borderStyle; }
			set
			{
				if (_borderStyle != value)
				{
					_borderStyle = value;
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the color at the left side
		/// </summary>
		/// <remarks>
		/// With this color a gradient can be drawn
		/// </remarks>
		public Color ColorLeft
		{
			get { return _colorLeft; }
			set
			{
				if (_colorLeft != value)
				{
					_colorLeft = value;
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the color at the right side
		/// </summary>
		/// <remarks>
		/// With this color a gradient can be drawn
		/// </remarks>
		public Color ColorRight
		{
			get { return _colorRight; }
			set
			{
				if (_colorRight != value)
				{
					_colorRight = value;
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the down image for each tabPage
		/// </summary>
		public Image DownImage
		{
			get { return _downImage; }
			set
			{
				if (_downImage != value)
				{
					_downImage = value;
					SetBitmaps();
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the up image for each tabPage
		/// </summary>
		public Image UpImage
		{
			get { return _upImage; }
			set
			{
				if (_upImage != value)
				{
					_upImage = value;
					SetBitmaps();
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the transparent color
		/// </summary>
		/// <remarks>
		/// This color is used to mask the portion that has to be hidden for the up and down images
		/// </remarks>
		public Color TransparentColor
		{
			get { return _transparentColor; }
			set
			{
				if (_transparentColor != value)
				{
					_transparentColor = value;
					SetBitmaps();
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the back color
		/// </summary>
		public override Color BackColor
		{
			get { return base.BackColor; }
			
			set 
			{
				if (this.BackColor != value)
				{
					DefineBackColor(value);
					Invalidate();
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the image list that has the up and down images
		/// </summary>
		public ImageList ImageList
		{
			get { return _imageList; }
			set
			{
				_imageList = value;
			}
		}
		
		/// <summary>
		/// Gets or sets if the control should hot track the mouse movements
		/// </summary>
		public bool HotTrack
		{
			get { return _hotTrack; }
			set { _hotTrack = value; }
		}
		
		/// <summary>
		/// Gets or sets the hot track color
		/// </summary>
		public Color HotTrackColor
		{
			get { return _hotTrackColor; }
			set { _hotTrackColor = value; }
		}

		#endregion
	
		#region Mouse events

		/// <summary>
		/// The mouse was pressed
		/// </summary>
		/// <param name="e">MouseEventArgs</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			try
			{
				// Set a reference to the pressed mouse button
				_pressedButton = e.Button;

				// Get the tabPage bar
				bool hitTest;
				foreach (TabPage tabPage in _tabPages)
				{
					if (e.Button == MouseButtons.Right)
						hitTest = tabPage.HitTest(e.X, e.Y);
					else
						hitTest = tabPage.HitTest(e.X, e.Y);
					if (hitTest)
					{
						_pressedIndexRightButton = _tabPages.IndexOf(tabPage);

						if (e.Button == MouseButtons.Right)
						{
							_mouseOnButton = true;
							Invalidate();
						}
						else
						{
							if (_selectedIndex != _tabPages.IndexOf(tabPage))
							{
								_pressedIndex = _pressedIndexRightButton;
								_mouseOnButton = true;
								//if (!_bitmapHotSpot)
								Invalidate();
							}
						}
						break;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::OnMouseDown:");
				throw;
			}
		}
		
		/// <summary>
		/// The mouse is moving over the control
		/// </summary>
		/// <param name="e">MouseEventArgs</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			try
			{
				// The OnMouseMove is fired just after OnMouseUp ???
				// This check avoids painting a raised border just after realesing the mouse button
				if (_mouseButtonJustReleased)
				{
					_mouseButtonJustReleased = false;
					return;
				}

				bool found = false;
				bool invalidate = false;

				// Detect if the mouse is over a tabPage
				_hotTrackIndex = -1;
				foreach (TabPage tabPage in _tabPages)
				{
					if (tabPage.HitTest(e.X, e.Y))
					{
						_hotTrackIndex = _tabPages.IndexOf(tabPage);
						invalidate = true;
						if (_hotTrackIndex != _selectedIndex)
							found = true;
						break;
					}
				}
				if (found)
					this.Cursor = Cursors.Hand;
				else
					this.Cursor = Cursors.Default;

				if (_pressedIndex != -1)
				{
					bool onButton = _tabPages[_pressedIndex].HitTest(e.X, e.Y);
					if (_mouseOnButton != onButton)
					{
						_mouseOnButton = onButton;
						invalidate = true;
					}
				}

				// Refresh the control
				if (invalidate)
					Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::OnMouseMove:");
				throw;
			}
		}
		
		/// <summary>
		/// The mouse button was released
		/// </summary>
		/// <param name="e">MouseEventArgs</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			try
			{
				bool hitTest;

				TabPage tabPageMouseUp = null;

				// Get the tabPage at the mouse coordinates
				_mouseButtonJustReleased = true;
				foreach (TabPage tabPage in _tabPages)
				{
					if (e.Button == MouseButtons.Right)
						hitTest = tabPage.HitTest(e.X, e.Y);
					else
						hitTest = tabPage.HitTest(e.X, e.Y);
					if (hitTest)
					{
						if (_pressedIndexRightButton == _tabPages.IndexOf(tabPage))
							tabPageMouseUp = tabPage;

						if (_pressedIndex == _tabPages.IndexOf(tabPage))
						{
							if (e.Button == MouseButtons.Left)
							{
								_selectionJustChanged = true;
								_selectedIndex = _pressedIndex;
							}

							break;
						}
					}
				}

				// Fire MoueUp event
				if (tabPageMouseUp != null)
					if (TabPageMouseUp != null)
						TabPageMouseUp(tabPageMouseUp, e);

				if (_selectedIndex == _pressedIndex && _selectedIndex != -1)
				{
					OnSelectedIndexChanged();
					_tabPages[_selectedIndex].SetControlFocus();
				}

				// Refresh the control
				_pressedIndex = -1;
				_pressedIndexRightButton = -1;
				_mouseOnButton = false;
				Invalidate();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in GroupPanel::OnMouseUp:");
				throw;
			}
		}

		/// <summary>
		/// The mouse left the control
		/// </summary>
		protected override void OnMouseLeave(System.EventArgs e)
		{
			// Reset the cursor
			this.Cursor = Cursors.Default;

			// Reset the hot track index
			_hotTrackIndex = -1;

			// Refresh the control
			Invalidate();
		}

		#endregion

	}
}
