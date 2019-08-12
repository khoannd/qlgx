using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;

namespace GxControl
{
    public partial class frmChuyenXu : frmBase
    {
        private int maGiaoDan = -1;

        public int MaGiaoDan
        {
            get { return maGiaoDan; }
            set
            {
                maGiaoDan = value;
                if (maGiaoDan > -1)
                {
                    gxChuyenXuList1.MaGiaoDan = maGiaoDan;
                }
            }
        }
        private string tenGiaoDan = "";

        public string TenGiaoDan
        {
            get { return tenGiaoDan; }
            set
            {
                tenGiaoDan = value;
                if (value.Trim() != "")
                {
                    this.Text += " - " + value;
                }
            }
        }
        public frmChuyenXu()
        {
            InitializeComponent();
            gxChuyenXuList1.FilterMode = Janus.Windows.GridEX.FilterMode.None;
            gxCommand1.OKButton.Visible = false;
        }
    }
}