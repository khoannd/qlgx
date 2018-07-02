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
    public partial class frmTimGiaDinh : frmBase
    {
        public frmTimGiaDinh()
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
            string where = " WHERE GiaoDan.HoTen LIKE ? ";
            var args = new List<object>() { "%" + txtTuKhoa.Text.Replace("\"", "") + "%" };
            if (cbGiaoHo.MaGiaoHo > -1)
            {
                where += " AND GiaDinh.MaGiaoHo=" + cbGiaoHo.MaGiaoHo.ToString();
            }
            if(txtSoHoKhau.Text != "")
            {
                where += "  AND SoHoKhau LIKE ? ";
                args.Add("%" + txtSoHoKhau.Text.Replace("\"", "") + "%");
            }
            frmGiaDinhList frm = new frmGiaDinhList();
            //frm.LoadGiaDinhList(string.Format(SqlConstants.SELECT_GIADINH_GIAODAN, where), new object[] { "%" + txtTuKhoa.Text.Replace("\"", "") + "%" });
            frm.QueryString = string.Format(SqlConstants.SELECT_GIADINH_GIAODAN, where);
            frm.Arguments = args.ToArray();
            if (cbGiaoHo.MaGiaoHo > -1) frm.MaGiaoHo = cbGiaoHo.MaGiaoHo;
            Memory.Instance.SetMemory(GxConstants.DangTimKiemGiaDinhGiaoDan, 1);
            Dispatcher.ShowTab(frm);
            this.DialogResult = DialogResult.OK;
        }

        private void gxCommand1_Load(object sender, EventArgs e)
        {

        }

        private void txtTuKhoa_Load(object sender, EventArgs e)
        {

        }

        private void cbGiaoHo_Load(object sender, EventArgs e)
        {

        }
    }
}