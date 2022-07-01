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
    public partial class frmReportCaoPho : frmBase
    {
        public string NoiAnTang
        {
            get { return txtNoiAnTang.Text.Trim(); }
        }
        public string SDTNhaHieu
        {
            get { return txtSDTNhaHieu.Text.Trim(); }
        }
        //Qua doi
        public string NgayQuaDoi
        {

            get { return dtNgayQuaDoi.DateString.Trim() == "" ? dtNgayQuaDoi.DateString.Trim() : dtNgayQuaDoi.Date.Day.ToString(); }
        }
        public string ThangQuaDoi
        {
            get
            {
                return dtNgayQuaDoi.DateString.Trim() == "" ? dtNgayQuaDoi.DateString.Trim() : dtNgayQuaDoi.Date.Month.ToString();
            }
        }
        public string NamQuaDoi
        {
            get
            {
                return dtNgayQuaDoi.DateString.Trim() == "" ? dtNgayQuaDoi.DateString.Trim() : dtNgayQuaDoi.Date.Year.ToString();
            }
        }
        public string GioQuaDoi
        {
            get { return txtGioQuaDoi.Text.Trim(); }
        }
        public string PhutQuaDoi
        {
            get { return txtPhutQuaDoi.Text.Trim(); }
        }
        //Tan liem
        public string NgayTanLiem
        {
            get { return dtNhapQuan.DateString.Trim() == "" ? dtNhapQuan.DateString.Trim() : dtNhapQuan.Date.Day.ToString(); }
        }
        public string ThangTanLiem
        {
            get { return dtNhapQuan.DateString.Trim() == "" ? dtNhapQuan.DateString.Trim() : dtNhapQuan.Date.Month.ToString(); }
        }
        public string NamTanLiem
        {
            get { return dtNhapQuan.DateString.Trim() == "" ? dtNhapQuan.DateString.Trim() : dtNhapQuan.Date.Year.ToString(); }
        }
        public string GioTanLiem
        {
            get { return txtGioTanLiem.Text.Trim(); }
        }
        public string PhutTanLiem
        {
            get { return txtPhutTanLiem.Text.Trim(); }
        }
        //TLe Tai gia
        public string NgayTLTaiGia
        {
            get { return dtTLTaiGia.DateString.Trim() == "" ? dtTLTaiGia.DateString.Trim() : dtTLTaiGia.Date.Day.ToString(); }
        }
        public string ThangTLTaiGia
        {
            get { return dtTLTaiGia.DateString.Trim() == "" ? dtTLTaiGia.DateString.Trim() : dtTLTaiGia.Date.Month.ToString(); }
        }
        public string NamTLTaiGia
        {
            get { return dtTLTaiGia.DateString.Trim() == "" ? dtTLTaiGia.DateString.Trim() : dtTLTaiGia.Date.Year.ToString(); }
        }
        public string GioTLTaiGia
        {
            get { return txtGioTLeTaiGia.Text.Trim(); }
        }
        public string PhutTLTaiGia
        {
            get { return txtPhutTLeTaiGia.Text.Trim(); }
        }
        //TLe An Tang
        public string NgayTLAnTang
        {
            get { return dtTLAnTang.DateString.Trim() == "" ? dtTLAnTang.DateString.Trim() : dtTLAnTang.Date.Day.ToString(); }
        }
        public string ThangTLAnTang
        {
            get { return dtTLAnTang.DateString.Trim() == "" ? dtTLAnTang.DateString.Trim() : dtTLAnTang.Date.Month.ToString(); }
        }
        public string NamTLAnTang
        {
            get { return dtTLAnTang.DateString.Trim() == "" ? dtTLAnTang.DateString.Trim() : dtTLAnTang.Date.Year.ToString(); }
        }
        public string GioTLAnTang
        {
            get { return txtGioTLeAnTang.Text.Trim(); }
        }
        public string PhutTLAnTang
        {
            get { return txtPhutTLeAnTang.Text.Trim(); }
        }

        public frmReportCaoPho()
        {
            InitializeComponent();
            this.AcceptButton = gxCommand1.OKButton;
            gxCommand1.OKIsAccept = true;
        }
        private void gxCommand1_OnOK(object sender, EventArgs e)
        {
            //int gioQuaDOi = int.Parse(txtGioQuaDoi.Text.Trim());
            //int phutQuadoi = int.Parse(txtPhutQuaDoi.Text.Trim());

            //if (gioQuaDOi > 24)
            //{
            //    MessageBox.Show("Giờ qua đời không được vượt qua 24", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    txtGioQuaDoi.Focus();
            //    return;
            //}
            //if (phutQuadoi > 60)
            //{
            //    MessageBox.Show("Phút qua đời không được vượt qua 60", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    txtPhutQuaDoi.Focus();
            //    return;
            //}

            //Kiểm tra các giá trị không được để trống
            if (string.IsNullOrEmpty(dtNgayQuaDoi.DateString.Trim()))
            {
                MessageBox.Show("Ngày qua đời không được để trống", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtNgayQuaDoi.Focus();
                return;
            }
            if (dtNgayQuaDoi.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Ngày qua đời không được lớn hơn ngày hiện tại!");
                dtNgayQuaDoi.Focus();
                return;
            }
            if (string.IsNullOrEmpty(dtNhapQuan.DateString.Trim()))
            {
                MessageBox.Show("Ngày Tẩn liệm không được để trống", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtNhapQuan.Focus();
                return;
            }
            if (string.IsNullOrEmpty(dtTLTaiGia.DateString.Trim()))
            {
                MessageBox.Show("Ngày T.Lể Tại Gia không được để trống", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtTLTaiGia.Focus();
                return;
            }
            if (string.IsNullOrEmpty(dtTLAnTang.DateString.Trim()))
            {
                MessageBox.Show("Ngày T.Lể An táng không được để trống", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dtTLAnTang.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtNoiAnTang.Text.Trim()))
            {
                MessageBox.Show("Nơi an táng không được để trống", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNoiAnTang.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtSDTNhaHieu.Text.Trim()))
            {
                MessageBox.Show("Số điện thoại không được để trống", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSDTNhaHieu.Focus();
                return;
            }
            //so sánh ngày qua đời không được lớn hơn các ngày còn lại
            if (dtNgayQuaDoi.Date >= dtNhapQuan.Date)
            {
                MessageBox.Show("Ngày qua đời không được lớn hơn hoặc bằng ngày Tẩn liệm!");
                dtNgayQuaDoi.Focus();
                return;
            }
            if (dtNgayQuaDoi.Date >= dtTLTaiGia.Date)
            {
                MessageBox.Show("Ngày qua đời không được lớn hơn hoặc bằng ngày T.Lễ Tại Gia!");
                dtNgayQuaDoi.Focus();
                return;
            }
            if (dtNgayQuaDoi.Date >= dtTLAnTang.Date)
            {
                MessageBox.Show("Ngày qua đời không được lớn hơn hoặc bằng ngày T.Lễ An Táng!");
                dtNgayQuaDoi.Focus();
                return;
            }

            //so sánh ngày tẩn liệm không dc lớn hơn ngày Tle tại gia, ngày Tle An táng
            if (dtNhapQuan.Date >= dtTLTaiGia.Date)
            {
                MessageBox.Show("Ngày tẩn liệm không được lớn hơn hoặc bằng ngày T.Lễ Tại Gia!");
                dtNhapQuan.Focus();
                return;
            }
            if (dtNhapQuan.Date >= dtTLAnTang.Date)
            {
                MessageBox.Show("Ngày tẩn liệm không được lớn hơn hoặc bằng ngày T.Lễ An Táng!");
                dtNhapQuan.Focus();
                return;
            }
            //so sánh ngày tl tại gia không dược lớn hơn ngày tle an táng
            if (dtTLTaiGia.Date >= dtTLAnTang.Date)
            {
                MessageBox.Show("Ngày T.Lễ Tại Gia không được lớn hơn hoặc bằng ngày T.Lễ An Táng!");
                dtTLTaiGia.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;


        }
        private void frmReportCaoPho_Load(object sender, EventArgs e)
        {

        }



        public DataTable GetTableCaoPho()
        {
            DataTable tbl = new DataTable();
            var aa = txtGioQuaDoi;
            tbl.Columns.Add(ReportCaoPhoConst.NoiAnTang);
            tbl.Columns.Add(ReportCaoPhoConst.SDTNhaHieu);
            tbl.Columns.Add(ReportCaoPhoConst.NgayQuaDoi);
            tbl.Columns.Add(ReportCaoPhoConst.ThangQuaDoi);
            tbl.Columns.Add(ReportCaoPhoConst.NamQuaDoi);
            tbl.Columns.Add(ReportCaoPhoConst.GioQuaDoi);
            tbl.Columns.Add(ReportCaoPhoConst.PhutQuaDoi);

            tbl.Columns.Add(ReportCaoPhoConst.NgayTanLiem);
            tbl.Columns.Add(ReportCaoPhoConst.ThangTanLiem);
            tbl.Columns.Add(ReportCaoPhoConst.NamTanLiem);
            tbl.Columns.Add(ReportCaoPhoConst.GioTanLiem);
            tbl.Columns.Add(ReportCaoPhoConst.PhutTanLiem);

            tbl.Columns.Add(ReportCaoPhoConst.NgayTLAnTang);
            tbl.Columns.Add(ReportCaoPhoConst.ThangTLAnTang);
            tbl.Columns.Add(ReportCaoPhoConst.NamTLAnTang);
            tbl.Columns.Add(ReportCaoPhoConst.GioTLAnTang);
            tbl.Columns.Add(ReportCaoPhoConst.PhutTLAnTang);

            tbl.Columns.Add(ReportCaoPhoConst.NgayTLTaiGia);
            tbl.Columns.Add(ReportCaoPhoConst.ThangTLTaiGia);
            tbl.Columns.Add(ReportCaoPhoConst.NamTLTaiGia);
            tbl.Columns.Add(ReportCaoPhoConst.GioTLTaiGia);
            tbl.Columns.Add(ReportCaoPhoConst.PhutTLTaiGia);

            tbl.TableName = ReportCaoPhoConst.TableName;
            Memory.InsertRow(tbl, new string[] { ReportCaoPhoConst.NoiAnTang, ReportCaoPhoConst.SDTNhaHieu,
                                                 ReportCaoPhoConst.NgayQuaDoi, ReportCaoPhoConst.ThangQuaDoi, ReportCaoPhoConst.NamQuaDoi, ReportCaoPhoConst.GioQuaDoi, ReportCaoPhoConst.PhutQuaDoi,
                                                 ReportCaoPhoConst.NgayTanLiem, ReportCaoPhoConst.ThangTanLiem, ReportCaoPhoConst.NamTanLiem, ReportCaoPhoConst.GioTanLiem, ReportCaoPhoConst.PhutTanLiem,
                                                 ReportCaoPhoConst.NgayTLAnTang, ReportCaoPhoConst.ThangTLAnTang, ReportCaoPhoConst.NamTLAnTang, ReportCaoPhoConst.GioTLAnTang, ReportCaoPhoConst.PhutTLAnTang,
                                                 ReportCaoPhoConst.NgayTLTaiGia, ReportCaoPhoConst.ThangTLTaiGia, ReportCaoPhoConst.NamTLTaiGia, ReportCaoPhoConst.GioTLTaiGia, ReportCaoPhoConst.PhutTLTaiGia,},
                new object[]{
                    NoiAnTang != "" ? NoiAnTang : "............................................................",
                    SDTNhaHieu != "" ? SDTNhaHieu : ".......................",
                    //Qua doi
                    NgayQuaDoi != ""? NgayQuaDoi : "........",
                    ThangQuaDoi != "" ? ThangQuaDoi : "........",
                    NamQuaDoi != "" ? NamQuaDoi : "........",
                    GioQuaDoi != "" ? GioQuaDoi : "........",
                    PhutQuaDoi != ""? PhutQuaDoi : "........",
                    //Tan liem
                    NgayTanLiem != ""? NgayTanLiem : "........",
                    ThangTanLiem != ""? ThangTanLiem : "........",
                    NamTanLiem != ""? NamTanLiem : "........",
                    GioTanLiem != ""? GioTanLiem : "........",
                    PhutTanLiem != ""? PhutTanLiem : "........",
                    // TLe An tang
                    NgayTLAnTang != ""? NgayTLAnTang : "........",
                    ThangTLAnTang != ""? ThangTLAnTang : "........",
                    NamTLAnTang != ""? NamTLAnTang : "........",
                    GioTLAnTang != ""? GioTLAnTang : "........",
                    PhutTLAnTang != ""? PhutTLAnTang : "........",
                    // Tle Tai Gia
                    NgayTLTaiGia != ""? NgayTLTaiGia : "........",
                    ThangTLTaiGia != ""? ThangTLTaiGia : "........",
                    NamTLTaiGia != ""? NamTLTaiGia : "........",
                    GioTLTaiGia != ""? GioTLTaiGia : "........",
                    PhutTLTaiGia != ""? PhutTLTaiGia : "........",

                });
            return tbl;
        }

        private void txtGioQuaDoi_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtPhutQuaDoi_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtGioTanLiem_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtPhutTanLiem_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtGioTLeTaiGia_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtPhutTLeTaiGia_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtGioTLeAnTang_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void txtPhutTLeAnTang_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            setNumber(sender, e);
        }

        public void setNumber(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
       (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
    }
}
