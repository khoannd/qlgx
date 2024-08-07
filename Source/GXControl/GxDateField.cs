﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class GxDateField : GxBaseField
    {
        public GxDateField()
        {
            InitializeComponent();
            InitControl();
        }

        protected override void InitControl()
        {
            editBase = gxDateInput1;
            base.InitControl();
        }

        public new bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                gxDateInput1.Enabled = value;
            }
        }

        public GxDateInput DateInput
        {
            get { return gxDateInput1; }
        }

        public bool IsNullDate
        {
            get { return gxDateInput1.IsNullDate; }
            set { gxDateInput1.IsNullDate = value; }
        }

        public object Value
        {
            get
            {
                if (gxDateInput1.IsNullDate || !CheckInput(false))
                {
                    return DBNull.Value;
                }
                return gxDateInput1.Value;
            }
            set
            {
                if (value == DBNull.Value || value.ToString()=="")
                {
                    gxDateInput1.IsNullDate = true;
                    return;
                }
                gxDateInput1.Value = value;
            }
        }

        public string DateString
        {
            get
            {
                return Value.ToString();
            }
        }


        public DateTime Date
        {
            get {
                object d = Value;
                if (d is DBNull) return DateTime.Now;
                return Memory.GetDateFromString(Value.ToString()); 
            }
        }

        public bool FullInputRequired
        {
            get { return gxDateInput1.FullInputRequired; }
            set { gxDateInput1.FullInputRequired = value; }
        }

        public string MinDate
        {
            get;
            set;
        }

        public string MaxDate
        {
            get;
            set;
        }

        public string MinDateName
        {
            get;
            set;
        }

        public string MaxDateName
        {
            get;
            set;
        }

        public bool IsValidDate
        {
            get
            {
                return CheckInput(false);
            }
        }
        public bool CheckDateConstraint()
        {
            return CheckDateConstraint(true);
        }
        public bool CheckDateConstraint(bool required = true)
        {
            if (!CheckInput(false))
            {
                MessageBox.Show($"Hãy nhập [{this.Label}] hợp lệ", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (this.Value == null || string.IsNullOrEmpty(this.Value.ToString())) return true;

            string label = "";
            string msg = "";
            if (!string.IsNullOrEmpty(MinDate) && Memory.CompareTwoStringDate(this.Value.ToString(), MinDate) == -1)
            {
                label = !string.IsNullOrEmpty(MinDateName) ? MinDateName : "ngày " + MinDate;
                if(required)
                    msg = $"[{Label}] không thể trước {label}";
                else
                    msg = $"[{Label}] đang nhập trước {label}. Bạn có chắc muốn tiếp tục không?\r\nChọn [Yes] để tiếp tục. Chọn [No] để xem lại.";

            }

            if (!string.IsNullOrEmpty(MaxDate) && Memory.CompareTwoStringDate(this.Value.ToString(), MaxDate) == 1)
            {
                label = !string.IsNullOrEmpty(MaxDateName) ? MaxDateName : "ngày " + MaxDate;
                if (required)
                    msg = $"[{Label}] không thể sau {label}";
                else
                    msg = $"[{Label}] đang nhập sau {label}. Bạn có chắc muốn tiếp tục không?\r\nChọn [Yes] để tiếp tục. Chọn [No] để xem lại.";
            }
            if(!string.IsNullOrEmpty(msg))
            {
                if (required)
                {
                    MessageBox.Show(msg, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    if (MessageBox.Show(msg, "Chú ý", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }

        public bool CheckInput()
        {
            return CheckInput(true);
        }

        public bool CheckInput(bool isShowMsg)
        {
            return gxDateInput1.CheckInput(isShowMsg);
        }

        protected override void ChangeSize()
        {
            base.ChangeSize();
            if(this.Height < 25) this.Height = 25;
        }
    }
}