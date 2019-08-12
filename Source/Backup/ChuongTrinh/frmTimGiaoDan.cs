using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using GxControl;

namespace GiaoXu
{
    public partial class frmTimGiaoDan : frmBase
    {
        public frmTimGiaoDan()
        {
            InitializeComponent();
            this.HelpKey = "tim_kiem";
            this.AcceptButton = gxCommand1.OKButton;
            cbGiaoHo.HasShowAll = true;
            cbGiaoHo.SelectedValue = -1;
            gxCommand1.OKIsAccept = true;
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            string where = "  AND HoTen LIKE ? ";
            if (cbGiaoHo.MaGiaoHo > -1)
            {
                where += " AND GiaoDan.MaGiaoHo=" + cbGiaoHo.MaGiaoHo.ToString();
            }
            frmGiaoDanList frm = new frmGiaoDanList();
            //frm.LoadGiaoDanList(string.Format(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO, where), new object[] { "%" + txtTuKhoa.Text.Replace("\"", "") + "%" });
            frm.QueryString = string.Concat(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO, where);
            frm.Arguments = new object[] { "%" + txtTuKhoa.Text.Replace("\"", "") + "%" };
            if (cbGiaoHo.MaGiaoHo > -1) 
                frm.MaGiaoHo = cbGiaoHo.MaGiaoHo;
            Memory.Instance.SetMemory(GxConstants.DangTimKiemGiaDinhGiaoDan, 1);
            Dispatcher.ShowTab(frm);
            this.DialogResult = DialogResult.OK;
        }
    }
}