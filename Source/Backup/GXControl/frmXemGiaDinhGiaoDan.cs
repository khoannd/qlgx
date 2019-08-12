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
    public partial class frmXemGiaDinhGiaoDan : frmBase
    {
        public frmXemGiaDinhGiaoDan()
        {
            InitializeComponent();
            gxCommand1.OKButton.Visible = false;
        }

        private List<int> maGiaDinhList = new List<int>();

        public List<int> MaGiaDinhList
        {
            get { return maGiaDinhList; }
            set { maGiaDinhList = value; }
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmXemGiaDinhGiaoDan_Load(object sender, EventArgs e)
        {
            gxGiaDinhList1.FormatGrid();
            if (MaGiaDinhList.Count > 0)
            {
                string sql = SqlConstants.SELECT_GIADINH_LIST + " AND ({0})";
                string where = "";
                for (int i = 0; i < MaGiaDinhList.Count; i++)
                {
                    if (i == 0)
                    {
                        where = string.Concat("MaGiaDinh=", MaGiaDinhList[i]);
                    }
                    else
                    {
                        where = string.Concat(where, " OR MaGiaDinh=", MaGiaDinhList[i]);
                    }
                }
                sql = string.Format(sql, where);
                gxGiaDinhList1.LoadData(sql, null);
            }
        }
    }
}
