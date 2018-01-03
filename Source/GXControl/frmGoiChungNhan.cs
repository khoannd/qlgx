using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GxGlobal;
using GxControl;

namespace GxControl
{
    public partial class frmGoiChungNhan : frmBase
    {
        public string LinhMucNhan {
            get { return txtLinhMucNhan.Text.Trim(); }
        }
        public string LinhMucGoi
        {
            get { return cbChaGui.Text.Trim(); }
        }
        public string GiaoXu
        {
            get { return txtGiaoXuNhan.Text.Trim(); }
        }
        public string GiaoPhan
        {
            get { return txtGiaoPhanNhan.Text.Trim(); }
        }
        public string LyDo
        {
            get { return txtLyDo.Text.Trim(); }
        }

        public frmGoiChungNhan()
        {
            InitializeComponent();
            this.AcceptButton = gxCommand1.OKButton;
            //load linh muc combobox
            DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND DenNgay IS NULL ORDER BY ChucVu ASC, TuNgay DESC");
            if (!Memory.ShowError() && tblLinhMuc != null)
            {
                foreach (DataRow row in tblLinhMuc.Rows)
                {
                    cbChaGui.Combo.Items.Add(row[LinhMucConst.TenThanh].ToString() + " " + row[LinhMucConst.HoTen].ToString());
                }
                if (cbChaGui.Combo.Items.Count > 0) cbChaGui.Combo.SelectedIndex = 0;
            }
            gxCommand1.OKIsAccept = true;
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        public DataTable GetTableNguoiNhan()
        {
            DataTable tbl = new DataTable();
            tbl.Columns.Add(ReportChungNhanBTConst.TenLinhMucGui);
            tbl.Columns.Add(ReportChungNhanBTConst.TenLinhMucNhan);
            tbl.Columns.Add(ReportChungNhanBTConst.TenGiaoXuNhan);
            tbl.Columns.Add(ReportChungNhanBTConst.TenGiaoPhanNhan);
            tbl.Columns.Add(ReportChungNhanBTConst.LyDo);
            tbl.TableName = ReportChungNhanBTConst.TableName;
            Memory.InsertRow(tbl, new string[]{ ReportChungNhanBTConst.TenLinhMucGui, ReportChungNhanBTConst.TenLinhMucNhan, ReportChungNhanBTConst.TenGiaoXuNhan, ReportChungNhanBTConst.TenGiaoPhanNhan, ReportChungNhanBTConst.LyDo }, 
                new object[]{ LinhMucGoi != "" ? LinhMucGoi : "..............................",
                    LinhMucNhan!= "" ? LinhMucNhan : "..............................",
                    GiaoXu != "" ? GiaoXu : "..............................",
                    GiaoPhan != "" ? GiaoPhan : "........................",
                    LyDo != "" ? LyDo : "............................................................"});
            return tbl;
        }
    }
}