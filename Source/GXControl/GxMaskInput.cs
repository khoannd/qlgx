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
    public partial class GxMaskInput : UserControl
    {
        #region EventHandler
        public delegate void ErrorCancelEvent(object sender,CancelEventArgs ce);
        public delegate void ErrorCancelEventDate(object sender, CancelEventArgs ce,bool notice);
        public event ErrorCancelEvent ErrorDay;
        public event ErrorCancelEvent ErrorMonth;
        public event ErrorCancelEventDate ErrorDate;
        #endregion
        #region property
        private bool nullable = true;

        public bool Nullable
        {
            get { return nullable; }
            set { nullable = value; }
        }

        private bool isDayError = false;

        public bool IsDayError
        {
            get { return isDayError; }
            set { isDayError = value; }
        }
        private bool isMonthError = false;

        public bool IsMonthError
        {
            get { return isMonthError; }
            set { isMonthError = value; }
        }
        
        public bool ReadOnly
        {
            get { return txtDay.ReadOnly; }
            set
            {
                textBox1.ReadOnly = value;
                txtDay.ReadOnly = value;
                txtMonth.ReadOnly = value;
            }
        }
        public bool IsNullMask
        {
            get
            {
                return (txtDay.Text.Trim() == "" && txtMonth.Text.Trim() == "" );
            }
            set
            {
                if (value == true)
                {
                    txtDay.Clear();
                    txtMonth.Clear();
                }
            }
        }
        
        public object Value
        {
            get
            {
                string day = txtDay.Text;
                string month = txtMonth.Text;
                return Memory.GetMaskString(day, month);
            }
            set
            {
                if (value!=null && !string.IsNullOrEmpty(value.ToString().Trim()))
                {
                    string day = "", month = "";
                    Memory.SplitMaskPart(value.ToString(), out day, out month);
                    txtDay.Text = day;
                    txtMonth.Text = month;
                }
            }
        }
        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                foreach (Control clt in this.Controls)
                {
                    clt.Enabled = value;
                }
                if (value == false)
                {
                    label1.BackColor = System.Drawing.SystemColors.Control;
                }
                else
                {
                    label1.BackColor = Color.White;
                }
            }
        }

        public string Day
        {
            get { return txtDay.Text; }
            set { txtDay.Text = value; }
        }

        public string Month
        {
            get { return txtMonth.Text; }
            set { txtMonth.Text = value; }
        }
        
        #endregion
        
        public GxMaskInput()
        {
            InitializeComponent();
        }

        private void txtDay_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n') return;
            if (txtDay.Text.Length == 1 && Validator.IsNumber(e.KeyChar.ToString()) && txtDay.SelectionLength != 2)
            {
                txtDay.Text += e.KeyChar.ToString();
                txtMonth.Focus();
            }
        }

        private void txtMonth_KeyPress(object sender, KeyPressEventArgs e)
        {
            isDayError = false;
            IsMonthError = false;
            if (e.KeyChar == '\n') return;
            if (e.KeyChar == (char)Keys.Tab || (txtMonth.Text.Length == 1 && Validator.IsNumber(e.KeyChar.ToString())))
            {
                if (Validator.IsNumber(e.KeyChar.ToString()))
                {
                    txtMonth.Text += e.KeyChar.ToString();
                }
                CancelEventArgs ce = new CancelEventArgs();
                if(ErrorMonth!=null)
                {
                    ErrorMonth(sender, ce);
                }
                if (ce.Cancel)
                    return;
                if (ErrorDate != null)
                {
                    ErrorDate(sender, ce, true);
                }
                if (ce.Cancel)
                {
                    FocusText();
                    return;
                }
                if (this.FindForm() != null)
                {
                    Control n = this.FindForm().GetNextControl(this, true);
                    n.Focus();
                }
            }
            if (e.KeyChar == (char)Keys.Tab)
            {
                if (this.FindForm() != null)
                {
                    Control n = this.FindForm().GetNextControl(this, true);
                    n.Focus();
                }
            }
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            txtDay.Focus();
        }

        private void txtDay_Leave(object sender, EventArgs e)
        {
            isDayError = false;
            if(txtDay.Text.Trim()=="")
            {
                isDayError = false;
                return;
            }
            CancelEventArgs ce = new CancelEventArgs();
            if (ErrorDay != null)
            {
                ErrorDay(sender, ce);
            }
            if(!ce.Cancel)
            {
                isDayError = false;
                return;
            }
            isDayError = true;
        }

        private void txtDay_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = isDayError;
        }

        private void txtDay_Validated(object sender, EventArgs e)
        {
            FocusText();
        }
        private void FocusText()
        {
            if (isMonthError)
                txtMonth.Focus();
            if (isDayError)
                txtDay.Focus();
        }

        private void txtMonth_Validated(object sender, EventArgs e)
        {
            FocusText();
        }

        private void txtMonth_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = isMonthError;
        }

        private void txtMonth_Leave(object sender, EventArgs e)
        {
            isMonthError = false;
            CancelEventArgs ce = new CancelEventArgs();
            if (ErrorMonth != null)
            {
                ErrorMonth(sender, ce);
            }
            if (ce.Cancel)
            {
                isMonthError = true;
                return;
            }
        }

        private void GxMaskInput_Leave(object sender, EventArgs e)
        {
            if (isMonthError) return;
            CancelEventArgs ce = new CancelEventArgs();
            if (ErrorMonth != null)
            {
                ErrorMonth(sender, ce);
            }
            if (ce.Cancel)
            {
                FocusText();
                return;
            }
            if(ErrorDate!=null)
            {
                ErrorDate(sender, ce,true);
            }
            if(ce.Cancel)
            {
                FocusText();
            }
        }
    }
}
