using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxControl;
using GxGlobal;

namespace GiaoXu
{
    public partial class frmThongKeChung : frmBase
    {
        public frmThongKeChung()
        {
            InitializeComponent();
            this.HelpKey = "thong_ke";

            tabThongKeChung.BackColor = this.BackColor;
            tbThongKeOnGoi.BackColor = this.BackColor;
        }

        private void frmThongKeChung_Load(object sender, EventArgs e)
        {
            
        }
    }
}