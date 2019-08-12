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
                if (value == DBNull.Value)
                {
                    gxDateInput1.IsNullDate = true;
                    return;
                }
                gxDateInput1.Value = value;
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
            this.Height = 26;
        }
    }
}