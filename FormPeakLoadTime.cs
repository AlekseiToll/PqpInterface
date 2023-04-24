using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using EmServiceLib;

namespace MainInterface
{
	public partial class FormPeakLoadTime : Form
	{
		private DateTime dtMaxModeStart1_, dtMaxModeEnd1_, dtMaxModeStart2_, dtMaxModeEnd2_;

		private float constrNPLtopMax_,
		              constrUPLtopMax_,
		              constrNPLbottomMax_,
		              constrUPLbottomMax_,
		              constrNPLtopMin_,
		              constrUPLtopMin_,
		              constrNPLbottomMin_,
		              constrUPLbottomMin_;

		public FormPeakLoadTime(DateTime dtMaxModeStart1, DateTime dtMaxModeEnd1, 
							DateTime dtMaxModeStart2, DateTime dtMaxModeEnd2,
							float constrNPLtopMax, float constrUPLtopMax, 
							float constrNPLbottomMax, float constrUPLbottomMax,
							float constrNPLtopMin, float constrUPLtopMin, 
							float constrNPLbottomMin, float constrUPLbottomMin)
		{
			InitializeComponent();

			dtMaxModeStart1_ = dtMaxModeStart1;
			dtMaxModeEnd1_ = dtMaxModeEnd1;
			dtMaxModeStart2_ = dtMaxModeStart2;
			dtMaxModeEnd2_ = dtMaxModeEnd2;

			constrNPLtopMax_ = constrNPLtopMax;
			constrUPLtopMax_ = constrUPLtopMax;
			constrNPLbottomMax_ = constrNPLbottomMax;
			constrUPLbottomMax_ = constrUPLbottomMax;
			constrNPLtopMin_ = constrNPLtopMin;
			constrUPLtopMin_ = constrUPLtopMin;
			constrNPLbottomMin_ = constrNPLbottomMin;
			constrUPLbottomMin_ = constrUPLbottomMin;
		}

		private void FormPeakLoadTime_Load(object sender, EventArgs e)
		{
			if(dtMaxModeStart1_ != DateTime.MinValue)
				dtpMaxStart1.Value = dtMaxModeStart1_;
			if (dtMaxModeEnd1_ != DateTime.MinValue)
				dtpMaxEnd1.Value = dtMaxModeEnd1_;
			if (dtMaxModeStart2_ != DateTime.MinValue)
				dtpMaxStart2.Value = dtMaxModeStart2_;
			if (dtMaxModeEnd2_ != DateTime.MinValue)
				dtpMaxEnd2.Value = dtMaxModeEnd2_;

			if (!Single.IsNaN(constrNPLtopMax_))
				tbMaxNPLtop.Text = constrNPLtopMax_.ToString();
			if (!Single.IsNaN(constrUPLtopMax_))
				tbMaxUPLtop.Text = constrUPLtopMax_.ToString();
			if (!Single.IsNaN(constrNPLbottomMax_))
				tbMaxNPLbottom.Text = constrNPLbottomMax_.ToString();
			if (!Single.IsNaN(constrUPLbottomMax_))
				tbMaxUPLbottom.Text = constrUPLbottomMax_.ToString();

			if (!Single.IsNaN(constrNPLtopMin_))
				tbMinNPLtop.Text = constrNPLtopMin_.ToString();
			if (!Single.IsNaN(constrUPLtopMin_))
				tbMinUPLtop.Text = constrUPLtopMin_.ToString();
			if (!Single.IsNaN(constrNPLbottomMin_))
				tbMinNPLbottom.Text = constrNPLbottomMin_.ToString();
			if (!Single.IsNaN(constrUPLbottomMin_))
				tbMinUPLbottom.Text = constrUPLbottomMin_.ToString();		
		}

		public DateTime TimeMaxStart1
		{
			get { return dtpMaxStart1.Value; }
		}

		public DateTime TimeMaxEnd1
		{
			get { return dtpMaxEnd1.Value; }
		}

		public DateTime TimeMaxStart2
		{
			get { return dtpMaxStart2.Value; }
		}

		public DateTime TimeMaxEnd2
		{
			get { return dtpMaxEnd2.Value; }
		}

		public bool GetConstrNPLtopMaxMode(out float value)
		{
			value = -1;
			if (tbMaxNPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxNPLtop.Text, out value);
		}

		public bool GetConstrUPLtopMaxMode(out float value)
		{
			value = -1;
			if (tbMaxUPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxUPLtop.Text, out value);
		}

		public bool GetConstrNPLbottomMaxMode(out float value)
		{
			value = -1;
			if (tbMaxNPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxNPLbottom.Text, out value);
		}

		public bool GetConstrUPLbottomMaxMode(out float value)
		{
			value = -1;
			if (tbMaxUPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMaxUPLbottom.Text, out value);
		}

		public bool GetConstrNPLtopMinMode(out float value)
		{
			value = -1;
			if (tbMinNPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinNPLtop.Text, out value);
		}

		public bool GetConstrUPLtopMinMode(out float value)
		{
			value = -1;
			if (tbMinUPLtop.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinUPLtop.Text, out value);
		}

		public bool GetConstrNPLbottomMinMode(out float value)
		{
			value = -1;
			if (tbMinNPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinNPLbottom.Text, out value);
		}

		public bool GetConstrUPLbottomMinMode(out float value)
		{
			value = -1;
			if (tbMinUPLbottom.Text.Length == 0) return false;
			return Conversions.object_2_float_en_ru(tbMinUPLbottom.Text, out value);
		}

		private void tb_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!Char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',' && e.KeyChar != '\b')
				e.Handled = true;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{		
		}

		private void frmPeakLoadTime_FormClosing(object sender, FormClosingEventArgs e)
		{
			// проверяем чтобы НДП не было больше ПДП
			// (если больше, то делаем НДП = ПДП)
			float npl, upl;
			if (tbMaxNPLtop.Text.Length > 0 && tbMaxUPLtop.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMaxNPLtop.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMaxUPLtop.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMaxNPLtop.Text = tbMaxUPLtop.Text;
				}
			}
			if (tbMaxNPLbottom.Text.Length > 0 && tbMaxUPLbottom.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMaxNPLbottom.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMaxUPLbottom.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMaxNPLbottom.Text = tbMaxUPLbottom.Text;
				}
			}

			// теперь тоже самое для наименьших нагрузок
			if (tbMinNPLtop.Text.Length > 0 && tbMinUPLtop.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMinNPLtop.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMinUPLtop.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMinNPLtop.Text = tbMinUPLtop.Text;
				}
			}
			if (tbMinNPLbottom.Text.Length > 0 && tbMinUPLbottom.Text.Length > 0)
			{
				if (!Conversions.object_2_float_en_ru(tbMinNPLbottom.Text, out npl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (!Conversions.object_2_float_en_ru(tbMinUPLbottom.Text, out upl))
				{
					MessageBoxes.InvalidData(this);
					e.Cancel = true;
					return;
				}
				if (npl > upl)
				{
					tbMinNPLbottom.Text = tbMinUPLbottom.Text;
				}
			}
		}
	}
}
