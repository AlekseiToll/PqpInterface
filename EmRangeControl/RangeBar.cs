using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;


namespace RangeControl
{
	/// <summary>
	/// The EmRangeBar class describes a slide control with two buttons.
	/// A number range is assigned to the control and with the two slide
	/// buttons you can select an interval inside the range. This control can
	/// p.e. used for threshold setting in an image processing tool.
	/// If you push with left mouse button on a slide button it will marked and
	/// while mouse button is pressed you can move the slider left and right.
	/// Otherwise you can use the keys + and - to manipulate the slider position.
	/// The control will throw two events. While left mouse button is pressed and the 
	/// position of one slider has changed the event OnRangeChanging will generate and
	/// if you release mouse button, the event OnRangeChanged signals program that
	/// a new range was selected.
	/// </summary>
	public class EmRangeBar : System.Windows.Forms.UserControl
	{
		private Label labelDtTotalMin_ = new Label();
		private Label labelDtRangeMin_ = new Label();
		private Label labelDtTotalMax_ = new Label();
		private Label labelDtRangeMax_ = new Label();

		// delegate to handle range changed
		public delegate void RangeChangedEventHandler(object sender, EventArgs e);

		// delegate to handle range is changing
		public delegate void RangeChangingEventHandler(object sender, EventArgs e);


		/// <summary> 
		/// designer variable
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EmRangeBar(long totalMin, long totalMax)
		{
			InitializeComponent();

			totalMin_ = rangeMin_ = totalMin;
			totalMax_ = rangeMax_ = totalMax;
		}

		public EmRangeBar(DateTime totalMin, DateTime totalMax)
			: this(totalMin.Ticks, totalMax.Ticks)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// EmRangeBar
			// 
			this.Name = "EmRangeBar";
			this.Size = new System.Drawing.Size(344, 64);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
			this.Resize += new System.EventHandler(this.OnResize);
			this.Load += new System.EventHandler(this.OnLoad);
			this.SizeChanged += new System.EventHandler(this.OnSizeChanged);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
			this.Leave += new System.EventHandler(this.OnLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
		}

		#endregion

		public enum ActiveMarkType
		{
			none,
			left,
			right
		};

		public enum RangeBarOrientation
		{
			horizontal,
			vertical
		};

		public enum TopBottomOrientation
		{
			top,
			bottom,
			both
		};

		private Color colorInner = Color.LightGreen;
		private Color colorRange = Color.FromKnownColor(KnownColor.Control);
		private Color colorShadowLight = Color.FromKnownColor(KnownColor.ControlLightLight);
		private Color colorShadowDark = Color.FromKnownColor(KnownColor.ControlDarkDark);
		private int sizeShadow = 1;
		private long totalMin_ = DateTime.MinValue.Ticks;
		private long totalMax_ = DateTime.MaxValue.Ticks;
		private long rangeMin_ = DateTime.MinValue.Ticks;
		private long rangeMax_ = DateTime.MaxValue.Ticks;
		private ActiveMarkType ActiveMark = ActiveMarkType.none;


		private RangeBarOrientation orientationBar = RangeBarOrientation.horizontal; // orientation of range bar
		private TopBottomOrientation orientationScale = TopBottomOrientation.bottom;
		private int BarHeight = 8; // Height of Bar
		private int MarkWidth = 8; // Width of mark knobs
		private int MarkHeight = 24; // total height of mark knobs
		private int TickHeight = 6; // Height of axis tick
		private int numAxisDivision = 10;

		private int PosL = 0, PosR = 0; // Pixel-Position of mark buttons
		private int XPosMin, XPosMax;

		private Point[] LMarkPnt = new Point[5];
		private Point[] RMarkPnt = new Point[5];

		private bool MoveLMark = false;	// for tooltip
		private bool MoveRMark = false;

		#region Properties

		/// <summary>
		/// set or get tick height
		/// </summary>
		public int HeightOfTick
		{
			set
			{
				TickHeight = Math.Min(Math.Max(1, value), BarHeight);
				Invalidate();
				Update();
			}
			get { return TickHeight; }
		}

		/// <summary>
		/// set or get mark knob height
		/// </summary>
		public int HeightOfMark
		{
			set
			{
				MarkHeight = Math.Max(BarHeight + 2, value);
				Invalidate();
				Update();
			}
			get { return MarkHeight; }
		}

		/// <summary>
		/// set/get height of mark
		/// </summary>
		public int HeightOfBar
		{
			set
			{
				BarHeight = Math.Min(value, MarkHeight - 2);
				Invalidate();
				Update();
			}
			get { return BarHeight; }
		}

		/// <summary>
		/// set or get range bar orientation
		/// </summary>
		public RangeBarOrientation Orientation
		{
			set
			{
				orientationBar = value;
				Invalidate();
				Update();
			}
			get { return orientationBar; }
		}

		/// <summary>
		/// set or get scale orientation
		/// </summary>
		public TopBottomOrientation ScaleOrientation
		{
			set
			{
				orientationScale = value;
				Invalidate();
				Update();
			}
			get { return orientationScale; }
		}

		/// <summary>
		///  set or get right side of range
		/// </summary>
		public long RangeMaximum
		{
			set
			{
				rangeMax_ = value;
				if (rangeMax_ < totalMin_)
					rangeMax_ = totalMin_;
				else if (rangeMax_ > totalMax_)
					rangeMax_ = totalMax_;
				if (rangeMax_ < rangeMin_)
					rangeMax_ = rangeMin_;
				Range2Pos();
				Invalidate(true);
			}
			get { return rangeMax_; }
		}

		/// <summary>
		/// set or get left side of range
		/// </summary>
		public long RangeMinimum
		{
			set
			{
				rangeMin_ = value;
				if (rangeMin_ < totalMin_)
					rangeMin_ = totalMin_;
				else if (rangeMin_ > totalMax_)
					rangeMin_ = totalMax_;
				if (rangeMin_ > rangeMax_)
					rangeMin_ = rangeMax_;
				Range2Pos();
				Invalidate(true);
			}
			get { return rangeMin_; }
		}

		/// <summary>
		/// set or get right side of total range
		/// </summary>
		public long TotalMaximum
		{
			//set
			//{
			//    totalMax_ = value;
			//    if (rangeMax_ > totalMax_)
			//        rangeMax_ = totalMax_;
			//    Range2Pos();
			//    Invalidate(true);
			//}
			get { return totalMax_; }
		}

		/// <summary>
		/// set or get left side of total range
		/// </summary>
		public long TotalMinimum
		{
			//set
			//{
			//    totalMin_ = value;
			//    if (rangeMin_ < totalMin_)
			//        rangeMin_ = totalMin_;
			//    Range2Pos();
			//    Invalidate(true);
			//}
			get { return totalMin_; }
		}

		/// <summary>
		/// set or get number of divisions
		/// </summary>
		public int DivisionNum
		{
			set
			{
				numAxisDivision = value;
				Refresh();
			}
			get { return numAxisDivision; }
		}

		/// <summary>
		/// set or get color of inner range
		/// </summary>
		public Color InnerColor
		{
			set
			{
				colorInner = value;
				Refresh();
			}
			get { return colorInner; }
		}

		#endregion

		/// <summary>
		/// set selected range
		/// </summary>
		/// <param name="left">left side of range</param>
		/// <param name="right">right side of range</param>
		public void SelectRange(long left, long right)
		{
			RangeMinimum = left;
			RangeMaximum = right;
			Range2Pos();
			Invalidate(true);
		}

		/// <summary>
		/// set range limits
		/// </summary>
		/// <param name="left">left side of range limit</param>
		/// <param name="right">right side of range limit</param>
		//public void SetTotalMinMax(long left, long right)
		//{
		//    totalMin_ = left;
		//    if (rangeMin_ < totalMin_)
		//        rangeMin_ = totalMin_;
		//    labelDtTotalMin_.Text = new DateTime(totalMin_).ToString("dd-MM HH:mm:ss");

		//    totalMax_ = right;
		//    if (rangeMax_ > totalMax_)
		//        rangeMax_ = totalMax_;

		//    Range2Pos();
		//    Invalidate(true);
		//}

		// paint event reaction
		private void OnPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			int h = this.Height;
			int w = this.Width;
			int baryoff, markyoff, tickyoff1, tickyoff2;
			double dtick;
			int tickpos;
			Pen penRange = new Pen(colorRange);
			Pen penShadowLight = new Pen(colorShadowLight);
			Pen penShadowDark = new Pen(colorShadowDark);
			SolidBrush brushShadowLight = new SolidBrush(colorShadowLight);
			SolidBrush brushShadowDark = new SolidBrush(colorShadowDark);
			SolidBrush brushInner;
			SolidBrush brushRange = new SolidBrush(colorRange);

			if (this.Enabled == true)
				brushInner = new SolidBrush(colorInner);
			else
				brushInner = new SolidBrush(Color.FromKnownColor(KnownColor.InactiveCaption));

			// range
			XPosMin = MarkWidth + 1;
			if (this.orientationBar == RangeBarOrientation.horizontal)
				XPosMax = w - MarkWidth - 1;
			else
				XPosMax = h - MarkWidth - 1;

			// range check
			if (PosL < XPosMin) PosL = XPosMin;
			if (PosL > XPosMax) PosL = XPosMax;
			if (PosR > XPosMax) PosR = XPosMax;
			if (PosR < XPosMin) PosR = XPosMin;

			Range2Pos();

			if (this.orientationBar == RangeBarOrientation.horizontal)
			{
				baryoff = (h - BarHeight) / 2;
				markyoff = baryoff + (BarHeight - MarkHeight) / 2 - 1;

				// total range bar frame			
				e.Graphics.FillRectangle(brushShadowDark, 0, baryoff, w - 1, sizeShadow); // top
				e.Graphics.FillRectangle(brushShadowDark, 0, baryoff, sizeShadow, BarHeight - 1); // left
				e.Graphics.FillRectangle(brushShadowLight, 0, baryoff + BarHeight - 1 - sizeShadow, w - 1, sizeShadow); // bottom
				e.Graphics.FillRectangle(brushShadowLight, w - 1 - sizeShadow, baryoff, sizeShadow, BarHeight - 1); // right

				// inner region
				e.Graphics.FillRectangle(brushInner, PosL, baryoff + sizeShadow, PosR - PosL, BarHeight - 1 - 2 * sizeShadow);

				// Skala
				if (orientationScale == TopBottomOrientation.bottom)
				{
					tickyoff1 = tickyoff2 = baryoff + BarHeight + 2;
				}
				else if (orientationScale == TopBottomOrientation.top)
				{
					tickyoff1 = tickyoff2 = baryoff - TickHeight - 4;
				}
				else
				{
					tickyoff1 = baryoff + BarHeight + 2;
					tickyoff2 = baryoff - TickHeight - 4;
				}

				if (numAxisDivision > 1)
				{
					dtick = (double) (XPosMax - XPosMin) / (double) numAxisDivision;
					for (int i = 0; i < numAxisDivision + 1; i++)
					{
						tickpos = (int) Math.Round((double) i * dtick);
						if (orientationScale == TopBottomOrientation.bottom
						    || orientationScale == TopBottomOrientation.both)
						{
							e.Graphics.DrawLine(penShadowDark, MarkWidth + 1 + tickpos,
							                    tickyoff1,
							                    MarkWidth + 1 + tickpos,
							                    tickyoff1 + TickHeight);
						}
						if (orientationScale == TopBottomOrientation.top
						    || orientationScale == TopBottomOrientation.both)
						{
							e.Graphics.DrawLine(penShadowDark, MarkWidth + 1 + tickpos,
							                    tickyoff2,
							                    MarkWidth + 1 + tickpos,
							                    tickyoff2 + TickHeight);
						}
					}
				}

				// left mark knob				
				LMarkPnt[0].X = PosL - MarkWidth;
				LMarkPnt[0].Y = markyoff + MarkHeight / 3;
				LMarkPnt[1].X = PosL;
				LMarkPnt[1].Y = markyoff;
				LMarkPnt[2].X = PosL;
				LMarkPnt[2].Y = markyoff + MarkHeight;
				LMarkPnt[3].X = PosL - MarkWidth;
				LMarkPnt[3].Y = markyoff + 2 * MarkHeight / 3;
				LMarkPnt[4].X = PosL - MarkWidth;
				LMarkPnt[4].Y = markyoff;
				e.Graphics.FillPolygon(brushRange, LMarkPnt);
				e.Graphics.DrawLine(penShadowDark, LMarkPnt[3].X - 1, LMarkPnt[3].Y, LMarkPnt[1].X - 1, LMarkPnt[2].Y);
					// lower left shadow
				e.Graphics.DrawLine(penShadowLight, LMarkPnt[0].X - 1, LMarkPnt[0].Y, LMarkPnt[0].X - 1, LMarkPnt[3].Y);
					// left shadow				
				e.Graphics.DrawLine(penShadowLight, LMarkPnt[0].X - 1, LMarkPnt[0].Y, LMarkPnt[1].X - 1, LMarkPnt[1].Y);
					// upper shadow
				if (PosL < PosR)
					e.Graphics.DrawLine(penShadowDark, LMarkPnt[1].X, LMarkPnt[1].Y + 1, LMarkPnt[1].X, LMarkPnt[2].Y); // right shadow
				if (ActiveMark == ActiveMarkType.left)
				{
					e.Graphics.DrawLine(penShadowLight, PosL - MarkWidth / 2 - 1, markyoff + MarkHeight / 3, PosL - MarkWidth / 2 - 1,
					                    markyoff + 2 * MarkHeight / 3); // active mark
					e.Graphics.DrawLine(penShadowDark, PosL - MarkWidth / 2, markyoff + MarkHeight / 3, PosL - MarkWidth / 2,
					                    markyoff + 2 * MarkHeight / 3); // active mark			
				}

				// right mark knob
				RMarkPnt[0].X = PosR + MarkWidth;
				RMarkPnt[0].Y = markyoff + MarkHeight / 3;
				RMarkPnt[1].X = PosR;
				RMarkPnt[1].Y = markyoff;
				RMarkPnt[2].X = PosR;
				RMarkPnt[2].Y = markyoff + MarkHeight;
				RMarkPnt[3].X = PosR + MarkWidth;
				RMarkPnt[3].Y = markyoff + 2 * MarkHeight / 3;
				RMarkPnt[4].X = PosR + MarkWidth;
				RMarkPnt[4].Y = markyoff;
				e.Graphics.FillPolygon(brushRange, RMarkPnt);
				if (PosL < PosR)
					e.Graphics.DrawLine(penShadowLight, RMarkPnt[1].X - 1, RMarkPnt[1].Y + 1, RMarkPnt[2].X - 1, RMarkPnt[2].Y);
					// left shadow
				e.Graphics.DrawLine(penShadowDark, RMarkPnt[2].X, RMarkPnt[2].Y, RMarkPnt[3].X, RMarkPnt[3].Y);
					// lower right shadow
				e.Graphics.DrawLine(penShadowDark, RMarkPnt[0].X, RMarkPnt[0].Y, RMarkPnt[1].X, RMarkPnt[1].Y); // upper shadow
				e.Graphics.DrawLine(penShadowDark, RMarkPnt[0].X, RMarkPnt[0].Y + 1, RMarkPnt[3].X, RMarkPnt[3].Y); // right shadow
				if (ActiveMark == ActiveMarkType.right)
				{
					e.Graphics.DrawLine(penShadowLight, PosR + MarkWidth / 2 - 1, markyoff + MarkHeight / 3, PosR + MarkWidth / 2 - 1,
					                    markyoff + 2 * MarkHeight / 3); // active mark
					e.Graphics.DrawLine(penShadowDark, PosR + MarkWidth / 2, markyoff + MarkHeight / 3, PosR + MarkWidth / 2,
					                    markyoff + 2 * MarkHeight / 3); // active mark
				}

				//if (MoveLMark)  // tooltip
				//{
				//    Font fontMark = new Font("Arial", MarkWidth);
				//    SolidBrush brushMark = new SolidBrush(colorShadowDark);
				//    StringFormat strformat = new StringFormat();
				//    strformat.Alignment = StringAlignment.Center;
				//    strformat.LineAlignment = StringAlignment.Near;
				//    e.Graphics.DrawString(rangeMin_.ToString(), fontMark, brushMark, PosL, tickyoff1 + TickHeight, strformat);
				//}

				//if (MoveRMark)
				//{
				//    Font fontMark = new Font("Arial", MarkWidth);
				//    SolidBrush brushMark = new SolidBrush(colorShadowDark);
				//    StringFormat strformat = new StringFormat();
				//    strformat.Alignment = StringAlignment.Center;
				//    strformat.LineAlignment = StringAlignment.Near;
				//    e.Graphics.DrawString(rangeMax_.ToString(), fontMark, brushMark, PosR, tickyoff1 + TickHeight, strformat);
				//}

			}
			else // vertical bar
			{
				baryoff = (w + BarHeight) / 2;
				markyoff = baryoff - BarHeight / 2 - MarkHeight / 2;
				if (orientationScale == TopBottomOrientation.bottom)
				{
					tickyoff1 = tickyoff2 = baryoff + 2;
				}
				else if (orientationScale == TopBottomOrientation.top)
				{
					tickyoff1 = tickyoff2 = baryoff - BarHeight - 2 - TickHeight;
				}
				else
				{
					tickyoff1 = baryoff + 2;
					tickyoff2 = baryoff - BarHeight - 2 - TickHeight;
				}

				// total range bar frame			
				e.Graphics.FillRectangle(brushShadowDark, baryoff - BarHeight, 0, BarHeight, sizeShadow); // top
				e.Graphics.FillRectangle(brushShadowDark, baryoff - BarHeight, 0, sizeShadow, h - 1); // left				
				e.Graphics.FillRectangle(brushShadowLight, baryoff, 0, sizeShadow, h - 1); // right
				e.Graphics.FillRectangle(brushShadowLight, baryoff - BarHeight, h - sizeShadow, BarHeight, sizeShadow); // bottom

				// inner region
				e.Graphics.FillRectangle(brushInner, baryoff - BarHeight + sizeShadow, PosL, BarHeight - 2 * sizeShadow, PosR - PosL);

				// Skala
				if (numAxisDivision > 1)
				{
					dtick = (double) (XPosMax - XPosMin) / (double) numAxisDivision;
					for (int i = 0; i < numAxisDivision + 1; i++)
					{
						tickpos = (int) Math.Round((double) i * dtick);
						if (orientationScale == TopBottomOrientation.bottom
						    || orientationScale == TopBottomOrientation.both)
							e.Graphics.DrawLine(penShadowDark,
							                    tickyoff1,
							                    MarkWidth + 1 + tickpos,
							                    tickyoff1 + TickHeight,
							                    MarkWidth + 1 + tickpos
								);
						if (orientationScale == TopBottomOrientation.top
						    || orientationScale == TopBottomOrientation.both)
							e.Graphics.DrawLine(penShadowDark,
							                    tickyoff2,
							                    MarkWidth + 1 + tickpos,
							                    tickyoff2 + TickHeight,
							                    MarkWidth + 1 + tickpos
								);
					}
				}

				// left(upper) mark knob				
				LMarkPnt[0].Y = PosL - MarkWidth;
				LMarkPnt[0].X = markyoff + MarkHeight / 3;
				LMarkPnt[1].Y = PosL;
				LMarkPnt[1].X = markyoff;
				LMarkPnt[2].Y = PosL;
				LMarkPnt[2].X = markyoff + MarkHeight;
				LMarkPnt[3].Y = PosL - MarkWidth;
				LMarkPnt[3].X = markyoff + 2 * MarkHeight / 3;
				LMarkPnt[4].Y = PosL - MarkWidth;
				LMarkPnt[4].X = markyoff;
				e.Graphics.FillPolygon(brushRange, LMarkPnt);
				e.Graphics.DrawLine(penShadowDark, LMarkPnt[3].X, LMarkPnt[3].Y, LMarkPnt[2].X, LMarkPnt[2].Y); // right shadow
				e.Graphics.DrawLine(penShadowLight, LMarkPnt[0].X - 1, LMarkPnt[0].Y, LMarkPnt[3].X - 1, LMarkPnt[3].Y);
					// top shadow				
				e.Graphics.DrawLine(penShadowLight, LMarkPnt[0].X - 1, LMarkPnt[0].Y, LMarkPnt[1].X - 1, LMarkPnt[1].Y);
					// left shadow
				if (PosL < PosR)
					e.Graphics.DrawLine(penShadowDark, LMarkPnt[1].X, LMarkPnt[1].Y, LMarkPnt[2].X, LMarkPnt[2].Y); // lower shadow
				if (ActiveMark == ActiveMarkType.left)
				{
					e.Graphics.DrawLine(penShadowLight, markyoff + MarkHeight / 3, PosL - MarkWidth / 2, markyoff + 2 * MarkHeight / 3,
					                    PosL - MarkWidth / 2); // active mark
					e.Graphics.DrawLine(penShadowDark, markyoff + MarkHeight / 3, PosL - MarkWidth / 2 + 1,
					                    markyoff + 2 * MarkHeight / 3, PosL - MarkWidth / 2 + 1); // active mark			
				}

				// right mark knob
				RMarkPnt[0].Y = PosR + MarkWidth;
				RMarkPnt[0].X = markyoff + MarkHeight / 3;
				RMarkPnt[1].Y = PosR;
				RMarkPnt[1].X = markyoff;
				RMarkPnt[2].Y = PosR;
				RMarkPnt[2].X = markyoff + MarkHeight;
				RMarkPnt[3].Y = PosR + MarkWidth;
				RMarkPnt[3].X = markyoff + 2 * MarkHeight / 3;
				RMarkPnt[4].Y = PosR + MarkWidth;
				RMarkPnt[4].X = markyoff;
				e.Graphics.FillPolygon(brushRange, RMarkPnt);
				e.Graphics.DrawLine(penShadowDark, RMarkPnt[2].X, RMarkPnt[2].Y, RMarkPnt[3].X, RMarkPnt[3].Y);
					// lower right shadow
				e.Graphics.DrawLine(penShadowDark, RMarkPnt[0].X, RMarkPnt[0].Y, RMarkPnt[1].X, RMarkPnt[1].Y); // upper shadow
				e.Graphics.DrawLine(penShadowDark, RMarkPnt[0].X, RMarkPnt[0].Y, RMarkPnt[3].X, RMarkPnt[3].Y); // right shadow
				if (PosL < PosR)
					e.Graphics.DrawLine(penShadowLight, RMarkPnt[1].X, RMarkPnt[1].Y, RMarkPnt[2].X, RMarkPnt[2].Y); // left shadow
				if (ActiveMark == ActiveMarkType.right)
				{
					e.Graphics.DrawLine(penShadowLight, markyoff + MarkHeight / 3, PosR + MarkWidth / 2 - 1,
					                    markyoff + 2 * MarkHeight / 3, PosR + MarkWidth / 2 - 1); // active mark
					e.Graphics.DrawLine(penShadowDark, markyoff + MarkHeight / 3, PosR + MarkWidth / 2, markyoff + 2 * MarkHeight / 3,
					                    PosR + MarkWidth / 2); // active mark				
				}

				if (MoveLMark)
				{
					Font fontMark = new Font("Arial", MarkWidth);
					SolidBrush brushMark = new SolidBrush(colorShadowDark);
					StringFormat strformat = new StringFormat();
					strformat.Alignment = StringAlignment.Near;
					strformat.LineAlignment = StringAlignment.Center;
					e.Graphics.DrawString(rangeMin_.ToString(), fontMark, brushMark, tickyoff1 + TickHeight + 2, PosL, strformat);
				}

				if (MoveRMark)
				{
					Font fontMark = new Font("Arial", MarkWidth);
					SolidBrush brushMark = new SolidBrush(colorShadowDark);
					StringFormat strformat = new StringFormat();
					strformat.Alignment = StringAlignment.Near;
					strformat.LineAlignment = StringAlignment.Center;
					e.Graphics.DrawString(rangeMax_.ToString(), fontMark, brushMark, tickyoff1 + TickHeight, PosR, strformat);
				}
			}

		}


		// mouse down event
		private void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.Enabled)
			{
				Rectangle LMarkRect = new Rectangle(
					Math.Min(LMarkPnt[0].X, LMarkPnt[1].X), // X
					Math.Min(LMarkPnt[0].Y, LMarkPnt[3].Y), // Y
					Math.Abs(LMarkPnt[2].X - LMarkPnt[0].X), // width
					Math.Max(Math.Abs(LMarkPnt[0].Y - LMarkPnt[3].Y), Math.Abs(LMarkPnt[0].Y - LMarkPnt[1].Y))); // height
				Rectangle RMarkRect = new Rectangle(
					Math.Min(RMarkPnt[0].X, RMarkPnt[2].X), // X
					Math.Min(RMarkPnt[0].Y, RMarkPnt[1].Y), // Y
					Math.Abs(RMarkPnt[0].X - RMarkPnt[2].X), // width
					Math.Max(Math.Abs(RMarkPnt[2].Y - RMarkPnt[0].Y), Math.Abs(RMarkPnt[1].Y - RMarkPnt[0].Y))); // height

				if (LMarkRect.Contains(e.X, e.Y))
				{
					this.Capture = true;
					MoveLMark = true;
					ActiveMark = ActiveMarkType.left;
					Invalidate(true);
				}

				if (RMarkRect.Contains(e.X, e.Y))
				{
					this.Capture = true;
					MoveRMark = true;
					ActiveMark = ActiveMarkType.right;
					Invalidate(true);
				}
			}
		}

		// mouse up event
		private void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.Enabled)
			{
				this.Capture = false;

				MoveLMark = false;
				MoveRMark = false;

				Invalidate();

				OnRangeChanged(EventArgs.Empty);
			}
		}

		// mouse move event
		private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (this.Enabled)
			{
				int h = this.Height;
				int w = this.Width;
				double r1 = (double) rangeMin_ * (double) w / (double) (totalMax_ - totalMin_);
				double r2 = (double) rangeMax_ * (double) w / (double) (totalMax_ - totalMin_);
				Rectangle LMarkRect = new Rectangle(
					Math.Min(LMarkPnt[0].X, LMarkPnt[1].X), // X
					Math.Min(LMarkPnt[0].Y, LMarkPnt[3].Y), // Y
					Math.Abs(LMarkPnt[2].X - LMarkPnt[0].X), // width
					Math.Max(Math.Abs(LMarkPnt[0].Y - LMarkPnt[3].Y), Math.Abs(LMarkPnt[0].Y - LMarkPnt[1].Y))); // height
				Rectangle RMarkRect = new Rectangle(
					Math.Min(RMarkPnt[0].X, RMarkPnt[2].X), // X
					Math.Min(RMarkPnt[0].Y, RMarkPnt[1].Y), // Y
					Math.Abs(RMarkPnt[0].X - RMarkPnt[2].X), // width
					Math.Max(Math.Abs(RMarkPnt[2].Y - RMarkPnt[0].Y), Math.Abs(RMarkPnt[1].Y - RMarkPnt[0].Y))); // height

				if (LMarkRect.Contains(e.X, e.Y) || RMarkRect.Contains(e.X, e.Y))
				{
					if (this.orientationBar == RangeBarOrientation.horizontal)
						this.Cursor = Cursors.SizeWE;
					else
						this.Cursor = Cursors.SizeNS;
				}
				else this.Cursor = Cursors.Arrow;

				if (MoveLMark)
				{
					if (this.orientationBar == RangeBarOrientation.horizontal)
						this.Cursor = Cursors.SizeWE;
					else
						this.Cursor = Cursors.SizeNS;
					if (this.orientationBar == RangeBarOrientation.horizontal)
						PosL = e.X;
					else
						PosL = e.Y;
					if (PosL < XPosMin)
						PosL = XPosMin;
					if (PosL > XPosMax)
						PosL = XPosMax;
					if (PosR < PosL)
						PosR = PosL;
					Pos2Range();
					ActiveMark = ActiveMarkType.left;
					Invalidate(true);

					OnRangeChanging(EventArgs.Empty);
				}
				else if (MoveRMark)
				{
					if (this.orientationBar == RangeBarOrientation.horizontal)
						this.Cursor = Cursors.SizeWE;
					else
						this.Cursor = Cursors.SizeNS;
					if (this.orientationBar == RangeBarOrientation.horizontal)
						PosR = e.X;
					else
						PosR = e.Y;
					if (PosR > XPosMax)
						PosR = XPosMax;
					if (PosR < XPosMin)
						PosR = XPosMin;
					if (PosL > PosR)
						PosL = PosR;
					Pos2Range();
					ActiveMark = ActiveMarkType.right;
					Invalidate(true);

					OnRangeChanging(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		///  transform pixel position to range position
		/// </summary>
		private void Pos2Range()
		{
			int w;
			int posw;

			if (this.orientationBar == RangeBarOrientation.horizontal)
				w = this.Width;
			else
				w = this.Height;
			posw = w - 2 * MarkWidth - 2;

			rangeMin_ = totalMin_ + (long) Math.Round((double) (totalMax_ - totalMin_) * (double) (PosL - XPosMin) / (double) posw);
			rangeMax_ = totalMin_ + (long) Math.Round((double) (totalMax_ - totalMin_) * (double) (PosR - XPosMin) / (double) posw);

			if (rangeMin_ > totalMin_)
			{
				labelDtRangeMin_.Text = new DateTime(rangeMin_).ToString("dd-MM HH:mm:ss");
				labelDtRangeMin_.BackColor = Color.LightPink;
			}
			else
			{
				labelDtRangeMin_.Text = "";
				labelDtRangeMin_.BackColor = Parent.BackColor;
			}

			if (rangeMax_ < totalMax_)
			{
				labelDtRangeMax_.Text = new DateTime(rangeMax_).ToString("dd-MM HH:mm:ss");
				labelDtRangeMax_.BackColor = Color.LightPink;
			}
			else
			{
				labelDtRangeMax_.Text = "";
				labelDtRangeMax_.BackColor = Parent.BackColor;
			}
		}

		/// <summary>
		///  transform range position to pixel position
		/// </summary>
		private void Range2Pos()
		{
			int w;
			int posw;

			if (this.orientationBar == RangeBarOrientation.horizontal)
				w = this.Width;
			else
				w = this.Height;
			posw = w - 2 * MarkWidth - 2;

			PosL = XPosMin + (int) Math.Round((double) posw * (double) (rangeMin_ - totalMin_) / (double) (totalMax_ - totalMin_));
			PosR = XPosMin + (int) Math.Round((double) posw * (double) (rangeMax_ - totalMin_) / (double) (totalMax_ - totalMin_));
		}

		/// <summary>
		/// method to handle resize event
		/// </summary>
		/// <param name="sender">object that sends event to resize</param>
		/// <param name="e">event parameter</param>
		private void OnResize(object sender, System.EventArgs e)
		{
			//Range2Pos();
			Invalidate(true);
		}

		/// <summary>
		/// method to handle key pressed event
		/// </summary>
		/// <param name="sender">object that sends key pressed event</param>
		/// <param name="e">event parameter</param>
		private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (this.Enabled)
			{
				if (ActiveMark == ActiveMarkType.left)
				{
					if (e.KeyChar == '+')
					{
						rangeMin_++;
						if (rangeMin_ > totalMax_)
							rangeMin_ = totalMax_;
						if (rangeMax_ < rangeMin_)
							rangeMax_ = rangeMin_;
						OnRangeChanged(EventArgs.Empty);
					}
					else if (e.KeyChar == '-')
					{
						rangeMin_--;
						if (rangeMin_ < totalMin_)
							rangeMin_ = totalMin_;
						OnRangeChanged(EventArgs.Empty);
					}
				}
				else if (ActiveMark == ActiveMarkType.right)
				{
					if (e.KeyChar == '+')
					{
						rangeMax_++;
						if (rangeMax_ > totalMax_)
							rangeMax_ = totalMax_;
						OnRangeChanged(EventArgs.Empty);
					}
					else if (e.KeyChar == '-')
					{
						rangeMax_--;
						if (rangeMax_ < totalMin_)
							rangeMax_ = totalMin_;
						if (rangeMax_ < rangeMin_)
							rangeMin_ = rangeMax_;
						OnRangeChanged(EventArgs.Empty);
					}
				}
				Invalidate(true);
			}
		}

		private void OnLoad(object sender, System.EventArgs e)
		{
			// use double buffering
			SetStyle(ControlStyles.DoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.UserPaint, true);

			labelDtTotalMin_.Location = new Point(2,2);
			labelDtTotalMin_.Parent = this;
			labelDtTotalMin_.Text = new DateTime(totalMin_).ToString("dd-MM HH:mm:ss");
			labelDtTotalMin_.AutoSize = true;
			labelDtRangeMin_.Location = new Point(2, this.Height - labelDtTotalMin_.Height - 2);
			labelDtRangeMin_.Parent = this;
			labelDtRangeMin_.AutoSize = true;
			labelDtTotalMin_.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			labelDtRangeMin_.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

			labelDtTotalMax_.Parent = this;
			labelDtTotalMax_.Text = new DateTime(totalMax_).ToString("dd-MM HH:mm:ss");
			labelDtTotalMax_.AutoSize = true;
			labelDtTotalMax_.Location = new Point(this.Width - labelDtTotalMax_.Width - 2, 2);
			// all labels have the same height so we can take the height of labelDtTotalMax_
			// because it already has text.
			labelDtRangeMax_.Location = new Point(
				labelDtTotalMax_.Left, this.Height - labelDtTotalMax_.Height - 2);
			labelDtRangeMax_.Parent = this;
			labelDtRangeMax_.AutoSize = true;
			labelDtTotalMax_.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			labelDtRangeMax_.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		}

		private void OnSizeChanged(object sender, System.EventArgs e)
		{
			Invalidate(true);
			Update();
		}

		private void OnLeave(object sender, System.EventArgs e)
		{
			ActiveMark = ActiveMarkType.none;
		}

		public event RangeChangedEventHandler RangeChanged; // event handler for range changed
		public event RangeChangedEventHandler RangeChanging; // event handler for range is changing

		public virtual void OnRangeChanged(EventArgs e)
		{
			if (RangeChanged != null)
				RangeChanged(this, e);
		}

		public virtual void OnRangeChanging(EventArgs e)
		{
			if (RangeChanging != null)
				RangeChanging(this, e);
		}
	}
}
