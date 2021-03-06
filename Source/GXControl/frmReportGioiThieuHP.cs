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
    public partial class frmReportGioiThieuHP : frmBase
    {
        public frmReportGioiThieuHP()
        {
            InitializeComponent();
            gxCommand1.OKButton.Text = "Xuất giới thiệu";
            gxCommand1.ToolTipOK = "Xuất giới thiệu hôn phối cho giáo dân này ra excel";
            gxCommand1.OKButton.Left = gxCommand1.CancelButton.Left - GxConstants.CONTROL_DIS - gxCommand1.OKButton.Width;
            this.AcceptButton = gxCommand1.OKButton;
            //load linh muc combobox
            DataTable tblLinhMuc = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST + " AND DenNgay IS NULL ");
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

        public string TenGiaoDan
        {
            get { return txtNguoi1.Text; }
            set { txtNguoi1.Text = value; }
        }

        private int maGiaoDan = -1;
        public string GioiTinh
        {
            get;set;
        }
        public int MaGiaoDan
        {
            get { return maGiaoDan; }
            set { maGiaoDan = value; }
        }

        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            if (maGiaoDan == -1 || DataObj == null || DataObj.Tables.Count == 0 || !DataObj.Tables.Contains(GiaoDanConst.TableName))
            {
                MessageBox.Show("Rất tiếc!\r\nCó lỗi không mong muốn xảy ra. Vui lòng liên hệ với tác giả hoặc người hỗ trợ phần mềm", "Lỗi DataObj null");
                return;

            }

            if (txtHoten.Text == "")
            {
                MessageBox.Show("Hãy nhập người muốn kết hôn với!");
                txtHoten.Focus();
                return;
            }

            if (DataObj.Tables.Contains(GiaoXuConst.TableName))
            {
                DataTable tblGiaoXu = DataObj.Tables[GiaoXuConst.TableName];
                //for linh muc cua giao xu hien tai
                tblGiaoXu.Rows[0][ReportGiaoDanConst.TenLinhMucGui] = cbChaGui.Text;
            }
            //add giao dan
            AddGiaoDan();

            //thong tin giao xu nhan chung nhan
            DataTable tblGiaoXuNhan = new DataTable();
            tblGiaoXuNhan.TableName = ReportHonPhoiConst.TableName;
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.TenLinhMucNhan);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.TenGiaoXuNhan);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.Tuoi1);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.Nguoi2);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.Tuoi2);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.TenCha2);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.TenMe2);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.TenGiaoXu2);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.TenGiaoPhan2);
            tblGiaoXuNhan.Columns.Add(ReportHonPhoiConst.NoiHocGLHN);

            DataRow rowGiaoXuNhan = tblGiaoXuNhan.NewRow();
            rowGiaoXuNhan[ReportHonPhoiConst.TenLinhMucNhan] = txtChaNhan.Text;
            rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoXuNhan] = txtGiaoXuNhan.Text;
            rowGiaoXuNhan[ReportHonPhoiConst.Nguoi2] = string.Concat(txtTenThanh.Text," ",txtHoten.Text);
            rowGiaoXuNhan[ReportHonPhoiConst.Tuoi2] = Memory.GetTuoi(dtNgaySinh2.Value.ToString());
            rowGiaoXuNhan[ReportHonPhoiConst.TenCha2] = txtTenCha2.Text;
            rowGiaoXuNhan[ReportHonPhoiConst.TenMe2] = txtTenMe2.Text;
            rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoXu2] = txtGiaoXu2.Text;
            rowGiaoXuNhan[ReportHonPhoiConst.TenGiaoPhan2] = txtGiaoPhan2.Text;
            rowGiaoXuNhan[ReportHonPhoiConst.NoiHocGLHN] = txtNoiHocGiaoLy.Text;

            tblGiaoXuNhan.Rows.Add(rowGiaoXuNhan);
            if (DataObj.Tables.Contains(ReportHonPhoiConst.TableName)) DataObj.Tables.Remove(ReportHonPhoiConst.TableName);
            DataObj.Tables.Add(tblGiaoXuNhan);

            int rs = ExcelReport.ReportGioiThieuHP.Export(DataObj);
            Memory.ShowError();



        }
        //2018-01-12 SonVc add start
        public void AddGiaoDan()
        {
            DataSet ds = new DataSet();
            if (Memory.GetConfig(GxConstants.CF_TUNHAP_MAGIAODAN) != GxConstants.CF_TRUE)
            {
                id = Memory.Instance.GetNextId(GiaoDanConst.TableName, GiaoDanConst.MaGiaoDan, true);
            }
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_GIAODAN_LIST_CO_GIAOHO + " AND MaGiaoDan = " + id.ToString());
            tbl.TableName = GiaoDanConst.TableName;
            dataReturn = tbl.NewRow();
            AssignDataSource(dataReturn);
            tbl.Rows.Add(dataReturn);
            ds.Tables.Add(tbl);
            Memory.UpdateDataSet(ds);
        }
        public void AssignDataSource(DataRow row)
        {      
            row[GiaoDanConst.MaGiaoDan] = id;
            row[GiaoDanConst.MaGiaoHo] = 0;
            row[GiaoDanConst.TenThanh] = txtTenThanh.Text;
            row[GiaoDanConst.HoTen] = txtHoten.Text;
            row[GiaoDanConst.NgaySinh] = dtNgaySinh2.Value;
            row[GiaoDanConst.HoTenCha] = txtTenCha2.Text;
            row[GiaoDanConst.HoTenMe] = txtTenMe2.Text;
            row[GiaoDanConst.ThuocGiaoPhan] = txtGiaoPhan2.Text;
            row[GiaoDanConst.ThuocGiaoXu] = txtGiaoXu2.Text;
            row[GiaoDanConst.Phai] = GioiTinh.Equals("Nam") ? GxConstants.NU : GxConstants.NAM;
        }

        //2018-01-12 SonVc add end
        private void txtNguoi2_Leave(object sender, EventArgs e)
        {
            txtHoten.Text = Memory.AutoUpperFirstChar(txtHoten.Text);
        }

        private void uiGroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void cbChaGui_Load(object sender, EventArgs e)
        {

        }

        private void txtNoiHocGiaoLy_Load(object sender, EventArgs e)
        {

        }

        private void txtGiaoPhan2_Load(object sender, EventArgs e)
        {

        }

        private void txtGiaoXu2_Load(object sender, EventArgs e)
        {

        }

        private void txtTenMe2_Load(object sender, EventArgs e)
        {

        }

        private void txtTenCha2_Load(object sender, EventArgs e)
        {

        }

        private void dtNgaySinh2_Load(object sender, EventArgs e)
        {

        }

        private void gxTextField1_Load(object sender, EventArgs e)
        {

        }

        private void txtNguoi2_Load(object sender, EventArgs e)
        {

        }
    }
}