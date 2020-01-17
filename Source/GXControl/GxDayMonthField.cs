using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class GxDayMonthField : GxBaseField
    {
        public GxDayMonthField()
        {
            InitializeComponent();
            InitControl();
            gxMaskInput1.ErrorDay += GxMaskInput1_ErrorDay;
            gxMaskInput1.ErrorMonth += GxMaskInput1_ErrorMonth;
            gxMaskInput1.ErrorDate += GxMaskInput1_ErrorDate;
        }

        private void GxMaskInput1_ErrorDate(object sender, CancelEventArgs ce, bool notice)
        {
            gxMaskInput1.IsMonthError = false;
            gxMaskInput1.IsDayError = false;
            if (gxMaskInput1.Day.Trim() == "" && gxMaskInput1.Month.Trim() == "") return;
            if (gxMaskInput1.Day.Trim()!=""&&gxMaskInput1.Month.Trim()=="")
            {
                gxMaskInput1.IsMonthError = true;
                ce.Cancel = true;
                if (notice)
                {
                    MessageBox.Show("Hãy nhập tháng", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            if (gxMaskInput1.Day.Trim() == "" && gxMaskInput1.Month.Trim() != "")
            {
                gxMaskInput1.IsDayError = true;
                ce.Cancel = true;
                if (notice)
                {
                    MessageBox.Show("Hãy nhập ngày", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            int day = Convert.ToInt32(gxMaskInput1.Day.Trim());
            int month = Convert.ToInt32(gxMaskInput1.Month.Trim());
            switch (month)
            {
                case 4:
                case 6:
                case 9:
                case 11:
                {
                    if(day>30)
                    {
                        gxMaskInput1.IsMonthError = true;
                        ce.Cancel = true;
                        if (notice)
                        {
                            MessageBox.Show("Hãy nhập tháng hợp lệ", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        return;
                    }
                }
                break;
                case 2:
                if (day > 29)
                {
                    ce.Cancel = true;
                    gxMaskInput1.IsMonthError = true;
                    if (notice)
                    {
                        MessageBox.Show("Hãy nhập tháng hợp lệ", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }
                break;
            }
        }

        private void GxMaskInput1_ErrorMonth(object sender, CancelEventArgs ce)
        {
            if (gxMaskInput1.Month.Trim() == "") return;
            if (Validator.IsNumber(gxMaskInput1.Month.ToString()) &&
                ((Int32.Parse(gxMaskInput1.Month.ToString()) > 0 && (Int32.Parse(gxMaskInput1.Month.ToString()) < 13))))
            {
                ce.Cancel = false;
                return;
            }
            ce.Cancel = true;
            MessageBox.Show("Hãy nhập tháng hợp lệ", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        private void GxMaskInput1_ErrorDay(object sender, CancelEventArgs ce)
        {
            if (gxMaskInput1.Day.Trim() == "") return;
            if (Validator.IsNumber(gxMaskInput1.Day.ToString()) && 
                ((Int32.Parse(gxMaskInput1.Day.ToString())>0 && (Int32.Parse(gxMaskInput1.Day.ToString()) < 32))))
            {
                ce.Cancel = false;
                return;
            }
            ce.Cancel = true;
            MessageBox.Show("Hãy nhập ngày hợp lệ", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        protected override void InitControl()
        {
            editBase = gxMaskInput1;
            base.InitControl();
        }
        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                gxMaskInput1.Enabled = value;
            }
        }

        public GxMaskInput MaskInput
        {
            get { return gxMaskInput1; }
        }

        public bool IsNullMask
        { 
            get { return gxMaskInput1.IsNullMask; }
            set { gxMaskInput1.IsNullMask = value; }
        }

        public object Value
        {
            get
            {
                if (gxMaskInput1.IsNullMask)
                {
                    return DBNull.Value;
                }
                return gxMaskInput1.Value;
            }
            set
            {
                if (value == DBNull.Value)
                {
                    gxMaskInput1.IsNullMask = true;
                    return;
                }
                gxMaskInput1.Value = value;
            }
        }
        protected override void ChangeSize()
        {
            base.ChangeSize();
            if (this.Height < 25) this.Height = 25;
        }
    }
}
