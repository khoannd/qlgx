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
    public partial class GxDateInput : UserControl
    {
        #region DECLARE
        Point currentPoint = Point.Empty;
        Control currentControl = null;
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
        private bool isYearError = false;

        public bool IsYearError
        {
            get { return isYearError; }
            set { isYearError = value; }
        }

        private bool fullInputRequired = false;

        public bool FullInputRequired
        {
            get { return fullInputRequired; }
            set { fullInputRequired = value; }
        }

        public event EventHandler OnOK;

        //public new bool Enabled
        //{
        //    get { return base.Enabled; }
        //    set
        //    {
        //        txtDay.Enabled = value;
        //        txtMonth.Enabled = value;
        //        txtYear.Enabled = value;
        //        label1.Enabled = value;
        //        label2.Enabled = value;
        //        base.Enabled = value;
        //    }
        //}

        public bool ReadOnly
        {
            get { return txtDay.ReadOnly; }
            set
            {
                txtDay.ReadOnly = value;
                txtMonth.ReadOnly = value;
                txtYear.ReadOnly = value;
            }
        }

        public bool IsNullDate
        {
            get {
                return (txtDay.Text.Trim() == "" && txtMonth.Text.Trim() == "" && txtYear.Text.Trim() == "");
            }
            set {
                if (value == true)
                {
                    txtDay.Clear();
                    txtMonth.Clear();
                    txtYear.Clear();
                }
            }
        }

        public object Value
        {
            get
            {
                string day = txtDay.Text;
                string month = txtMonth.Text;
                string year = txtYear.Text;
                if (!Validator.IsYear(year))
                {
                    return DBNull.Value;
                }
                return Memory.GetDateString(day, month, year);
            }
            set
            {
                DateTime d = DateTime.Now;
                if (value is DateTime)
                {
                    d = (DateTime)value;
                    txtDay.Text = d.Day.ToString("00");
                    txtMonth.Text = d.Month.ToString("00");
                    txtYear.Text = d.Year.ToString("0000");
                }
                else if (value is string)
                {
                    if (!string.IsNullOrEmpty(value.ToString().Trim()))
                    {
                        string day = "", month = "", year = "";
                        Memory.SplitDatePart(value.ToString(), out day, out month, out year);

                        txtDay.Text = day;
                        txtMonth.Text = month;
                        txtYear.Text = year;
                    }
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
                    label2.BackColor = System.Drawing.SystemColors.Control;
                }
                else
                {
                    label1.BackColor = Color.White;
                    label2.BackColor = Color.White;
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

        public string Year
        {
            get { return txtYear.Text; }
            set { txtYear.Text = value; }
        }
        #endregion

        #region constructor
        public GxDateInput()
        {
            InitializeComponent();
        }
        #endregion

        #region public method
        public bool CheckInput()
        {
            return CheckInput(true);
        }

        public bool CheckInput(bool isShowMsg)
        {
            if (txtDay.Focused || txtMonth.Focused || txtYear.Focused) return true;

            string day = txtDay.Text.Trim();
            string month = txtMonth.Text.Trim();
            string year = txtYear.Text.Trim();

            if (day != "")
            {
                if (month == "")
                {
                    if (isShowMsg)
                    {
                        MessageBox.Show("Hãy nhập tháng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    isMonthError = true;
                    return false;
                }
                if (year == "")
                {
                    if (isShowMsg)
                    {
                        MessageBox.Show("Hãy nhập năm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    isYearError = true;
                    return false;
                }
            }
            if (month != "" && year == "")
            {
                if (isShowMsg)
                {
                    MessageBox.Show("Hãy nhập năm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            if ((day != "" && month != "" && year != "") && !Validator.IsDate(year, month, day))
            {
                if (isShowMsg)
                {
                    MessageBox.Show("Hãy nhập ngày/tháng/năm hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                isDayError = true;
                return false;
            }
            else if (fullInputRequired && (day == "" || month == "" || year == ""))
            {
                MessageBox.Show("Hãy nhập đầy đủ ngày/tháng/năm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Focus();
                txtDay.Focus();
                return false;
            }
            return true;
        }
        #endregion

        #region Event Handler
        private void txtDay_Leave(object sender, EventArgs e)
        {
            if (IsNullDate)
            {
                return;
            }
            
            if (txtDay.Text.Trim() != "" && (!Validator.IsNumber(txtDay.Text) || int.Parse(txtDay.Text) > 31 || int.Parse(txtDay.Text) < 1))
            {
                isDayError = true;
                MessageBox.Show("Hãy nhập ngày hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDay.SelectAll();
                return;
            }
            isDayError = false;
            //hiepdv begin edit
          //  CheckInput();
            //hiepdv begin edit
            //if (txtMonth.Text.Trim() != "" && txtYear.Text.Trim() != "")
            //{
            //    CheckInput();
            //}
            if (fullInputRequired && !txtMonth.Focused && !txtYear.Focused
                && Validator.IsDate(Year, Month, Day))
            {
                if (OnOK != null) OnOK(this, EventArgs.Empty);
            }
            currentControl = null;
        }
        private void txtMonth_Leave(object sender, EventArgs e)
        {
            if (IsNullDate)
            {
                return;
            }
            if (txtMonth.Text.Trim() != "" && (!Validator.IsNumber(txtMonth.Text) || !Validator.IsMonth(txtMonth.Text)))
            {
                isMonthError = true;
                MessageBox.Show("Hãy nhập tháng hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMonth.SelectAll();
                return;
            }
            isMonthError = false;
            bool isOK = CheckInput(false);
            if (!isOK)
            {
                isMonthError = true;
                return;
            }
            //if (txtYear.Text.Trim() != "")
            //{
            //    CheckInput();
            //}
            if (fullInputRequired && !txtDay.Focused && !txtYear.Focused
                && Validator.IsDate(Year, Month, Day))
            {
                if (OnOK != null) OnOK(this, EventArgs.Empty);
            }
            currentControl = null;
        }

        private void txtYear_Leave(object sender, EventArgs e)
        {
            isYearError = false;
            //bool isOK = false;
            //if (IsNullDate)
            //{
            //    isYearError = false;
            //    return;
            //}


            if (!Validator.IsNumber(txtYear.Text) || !Validator.IsYear(txtYear.Text))
            {
                return;
            }
            if (currentControl != null && (currentControl.Equals(txtDay) || currentControl.Equals(txtMonth) || currentControl.Equals(this)))
            {
                return;
            }


            //if (this.FindForm() != null)
            //{
            //    Control n = this.FindForm().GetNextControl(this, true);
            //    n.Focus();
            //}

            //if (!Validator.IsNumber(txtYear.Text) || !Validator.IsYear(txtYear.Text))
            //{
            //    isYearError = true;
            //    MessageBox.Show("Hãy nhập năm hợp lệ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    //txtYear.SelectAll();
            //    //txtYear.Focus();
            //    return;
            //}
            //txtYear.Text = Memory.GetYear(txtYear.Text).ToString();
            //isYearError = false;
            //isOK = CheckInput();
            //if (isOK)
            //{
            //    if (OnOK != null) OnOK(sender, e);
            //}
            //currentControl = null;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            currentControl = null;
            txtDay.Focus();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            txtDay.Focus();
        }

        private void txtDay_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = isDayError;
        }

        private void txtMonth_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = isMonthError;
        }

        private void txtYear_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = isYearError;
        }
        // forcus 
        private void FocusText()
        {
            if (isYearError)
                txtYear.Focus();
            if (isMonthError)
                txtMonth.Focus();
            if (isDayError)
                txtDay.Focus();
        }
        private void txtDay_Enter(object sender, EventArgs e)
        {
            txtDay.ImeMode = ImeMode.Off;
            currentControl = txtDay;
            
            txtDay.SelectAll();
        }

        private void txtMonth_Enter(object sender, EventArgs e)
        {
            txtMonth.ImeMode = ImeMode.Off;
            currentControl = txtMonth;
            
            txtMonth.SelectAll();
        }

        private void txtYear_Enter(object sender, EventArgs e)
        {
            txtYear.ImeMode = ImeMode.Off;
            currentControl = txtYear;

            txtYear.SelectAll();
        }

        private void txtDay_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void txtMonth_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void txtYear_KeyUp(object sender, KeyEventArgs e)
        {

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
            if (e.KeyChar== '\n') return;
            if (txtMonth.Text.Length == 1 && Validator.IsNumber(e.KeyChar.ToString()) && txtMonth.SelectionLength != 2)
            {
                txtMonth.Text += e.KeyChar.ToString();
                txtYear.Focus();
            }
        }

        private void txtYear_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n') return;
            if (e.KeyChar == (char)Keys.Tab || (txtYear.Text.Length == 3 && Validator.IsNumber(e.KeyChar.ToString()) && !isDayError && !isMonthError && !isYearError))
            {
                if (Validator.IsNumber(e.KeyChar.ToString()))
                {
                    txtYear.Text += e.KeyChar.ToString();
                }
                if (txtDay.Text.Trim() != "" && txtMonth.Text.Trim() == "")
                {
                    txtMonth.Focus();
                    MessageBox.Show("Hãy nhập tháng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //hiepdv begin add
                isDayError = false;
                if ((txtDay.Text.Trim() != "" && txtMonth.Text.Trim() != "" && txtYear.Text.Trim() != "") && !Validator.IsDate(txtYear.Text.Trim(), txtMonth.Text.Trim(), txtDay.Text.Trim()))
                {
                    txtDay.Focus();
                    MessageBox.Show("Hãy nhập ngày/tháng/năm hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isDayError = true;
                }
                //hiepdv end add
                if(!IsDayError)
                if (this.FindForm() != null)
                {
                    Control n = this.FindForm().GetNextControl(this, true);
                    n.Focus();
                }
            }
            if(e.KeyChar == (char)Keys.Tab)
            {
                if (this.FindForm() != null)
                {
                    Control n = this.FindForm().GetNextControl(this, true);
                    n.Focus();
                }
            }
        }
        bool isValueChanging = false;
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if(!isValueChanging) this.Value = dateTimePicker1.Value;
            if (OnOK != null) OnOK(sender, e);
        }

        private void dateTimePicker1_DropDown(object sender, EventArgs e)
        {
            if (!IsNullDate)
            {
                isValueChanging = true;
                try
                {
                    dateTimePicker1.Value = Memory.GetDateFromString(Value.ToString(), true);
                }
                catch { }
                isValueChanging = false;
            }
        }

        private void GxDateInput_MouseMove(object sender, MouseEventArgs e)
        {
            currentControl = this;
        }

        private void txtMonth_MouseMove(object sender, MouseEventArgs e)
        {
            currentControl = txtMonth;
        }

        private void txtDay_MouseMove(object sender, MouseEventArgs e)
        {
            currentControl = txtDay;
        }

        private void txtYear_MouseMove(object sender, MouseEventArgs e)
        {
            currentControl = txtYear;
        }

        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
            currentControl = this;
        }

        private void dateTimePicker1_MouseMove(object sender, MouseEventArgs e)
        {
                currentControl = this;
        }
        
        private void GxDateInput_Leave(object sender, EventArgs e)
        {
            currentControl = null;
            bool isOK = false;
            if (IsNullDate)
            {
                isYearError = false;
                return;
            }

            if (!Validator.IsNumber(txtYear.Text) || !Validator.IsYear(txtYear.Text))
            {
                if (txtDay.Text.Trim() != "" && txtMonth.Text.Trim() == "")
                {
                    isMonthError = true;
                    MessageBox.Show("Hãy nhập tháng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //txtYear.SelectAll();

                isYearError = true;
                MessageBox.Show("Hãy nhập năm hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            txtYear.Text = Memory.GetYear(txtYear.Text).ToString();
            isYearError = false;
            isOK = CheckInput();
            if (isOK)
            {
                if(txtDay.Text.Trim()!=""&&txtMonth.Text.Trim()=="")
                {
                    isMonthError = true;
                    MessageBox.Show("Hãy nhập tháng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
              
                if (this.FindForm() != null)
                {
                    Control n = this.FindForm().GetNextControl(this, true);
                    n.Focus();
                    if(!CheckInput())
                    {
                        isDayError = false;
                        txtDay.Focus();
                        return;
                    }
                }
                if (OnOK != null) OnOK(sender, e);
            }
            else
            {
                //hiepdv begin edit
                return;
             //   MessageBox.Show("Ngày tháng năm nhập không hợp lệ", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //hiepdv end edit
            }
            currentControl = null;
        }

        private void txtYear_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void t(object sender, EventArgs e)
        {

        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            currentControl = null;
            txtDay.Focus();
        }

        private void txtYear_Validated(object sender, EventArgs e)
        {
            FocusText();
        }

        private void txtMonth_Validated(object sender, EventArgs e)
        {
            FocusText();
        }

        private void txtDay_Validated(object sender, EventArgs e)
        {
            FocusText();
        }
        #endregion
    }
}
