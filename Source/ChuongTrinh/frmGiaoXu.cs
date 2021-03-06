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
    public partial class frmGiaoXu : frmBase
    {
        public frmGiaoXu()
        {
            InitializeComponent();

            this.HelpKey = "giao_xu";

            gxAddEdit1.SelectButton.Visible = false;
            this.AcceptButton = gxAddEdit1.AddButton;
        }

        private void frmGiaoXu_Load(object sender, EventArgs e)
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (Memory.ShowError()) return;
            if (tbl.Rows.Count > 0)
            {
                txtTenGiaoPhan.Text = tbl.Rows[0][GiaoPhanConst.TenGiaoPhan].ToString();
                txtTenGiaoHat.Text = tbl.Rows[0][GiaoHatConst.TenGiaoHat].ToString();
                txtTenGiaoXu.Text = tbl.Rows[0][GiaoXuConst.TenGiaoXu].ToString();
                txtDiaChi.Text = tbl.Rows[0][GiaoXuConst.DiaChi].ToString();
                txtWebsite.Text = tbl.Rows[0][GiaoXuConst.Website].ToString();
                txtDienThoai.Text = tbl.Rows[0][GiaoXuConst.DienThoai].ToString();
                txtEmail.Text = tbl.Rows[0][GiaoXuConst.Email].ToString();
                txtHinh.Text = tbl.Rows[0][GiaoXuConst.Hinh].ToString();
                txtGhiChu.Rtf = tbl.Rows[0][GiaoXuConst.GhiChu].ToString();
            }
            gxLinhMucList1.FormatGrid();
            gxLinhMucList1.LoadData();
        }

        private void gxAddEdit1_AddClick(object sender, EventArgs e)
        {
            EditLinhMucRow(GxOperation.ADD);
        }

        private void gxAddEdit1_DeleteClick(object sender, EventArgs e)
        {
            if (gxLinhMucList1.CurrentRow != null && (gxLinhMucList1.CurrentRow.DataRow as DataRowView) != null)
            {
                if (MessageBox.Show("Bạn có chắc muốn xóa thông tin Linh mục này?", "Xác nhận xóa", MessageBoxButtons.YesNo) == DialogResult.No) return;
                Memory.ExecuteSqlCommand(SqlConstants.DELETE_LINHMUC_THEO_ID,
                                                    new object[] { (gxLinhMucList1.CurrentRow.DataRow as DataRowView).Row[LinhMucConst.MaLinhMuc] });
                if (Memory.ShowError())
                {
                    return;
                }
                gxLinhMucList1.CurrentRow.Delete();

            }
        }

        private void gxAddEdit1_EditClick(object sender, EventArgs e)
        {
            EditLinhMucRow(GxOperation.EDIT);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenGiaoPhan.Text))
            {
                MessageBox.Show("Hãy nhập tên giáo phận!");
                txtTenGiaoPhan.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtTenGiaoHat.Text))
            {
                MessageBox.Show("Hãy nhập tên giáo hạt!");
                txtTenGiaoHat.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtTenGiaoXu.Text))
            {
                MessageBox.Show("Hãy nhập tên giáo xứ!");
                txtTenGiaoXu.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtDiaChi.Text))
            {
                MessageBox.Show("Hãy nhập địa chỉ giáo xứ!");
                txtDiaChi.Focus();
                return;
            }
            DataSet ds = new DataSet();

            //for giao phan
            DataTable tblGiaoPhan = Memory.GetData(SqlConstants.SELECT_GIAOPHAN);
            if (Memory.ShowError()) return;
            if (tblGiaoPhan.Rows.Count > 0)
            {
                tblGiaoPhan.Rows[0][GiaoPhanConst.TenGiaoPhan] = txtTenGiaoPhan.Text;
            }
            else
            {
                DataRow row = tblGiaoPhan.NewRow();
                row[GiaoPhanConst.MaGiaoPhan] = Memory.Instance.GetNextId(GiaoPhanConst.TableName, GiaoPhanConst.MaGiaoPhan, true);
                row[GiaoPhanConst.TenGiaoPhan] = txtTenGiaoPhan.Text;
                tblGiaoPhan.Rows.Add(row);
            }
            tblGiaoPhan.TableName = GiaoPhanConst.TableName;
            ds.Tables.Add(tblGiaoPhan);

            //for giao hat
            DataTable tblGiaoHat = Memory.GetData(SqlConstants.SELECT_GIAOHAT);
            if (Memory.ShowError()) return;
            if (tblGiaoHat.Rows.Count > 0)
            {
                tblGiaoHat.Rows[0][GiaoHatConst.TenGiaoHat] = txtTenGiaoHat.Text;
            }
            else
            {
                DataRow row = tblGiaoHat.NewRow();
                row[GiaoHatConst.MaGiaoHat] = Memory.Instance.GetNextId(GiaoHatConst.TableName, GiaoHatConst.MaGiaoHat, true);
                row[GiaoHatConst.TenGiaoHat] = txtTenGiaoHat.Text;
                row[GiaoHatConst.MaGiaoPhan] = tblGiaoPhan.Rows[0][GiaoPhanConst.MaGiaoPhan];
                tblGiaoHat.Rows.Add(row);
            }            
            tblGiaoHat.TableName = GiaoHatConst.TableName;
            ds.Tables.Add(tblGiaoHat);

            //for giao xu
            DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (Memory.ShowError()) return;
            if (tblGiaoXu.Rows.Count > 0)
            {
                tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXu] = Memory.Instance.GetNextId(GiaoXuConst.TableName, GiaoXuConst.MaGiaoXu, true);
                AssignGXData(tblGiaoXu.Rows[0]);
            }
            else
            {
                DataRow row = tblGiaoXu.NewRow();
                AssignGXData(row);
                row[GiaoXuConst.MaGiaoHat] = tblGiaoHat.Rows[0][GiaoHatConst.MaGiaoHat];
                tblGiaoXu.Rows.Add(row);                
            }
            
            tblGiaoXu.TableName = GiaoXuConst.TableName;

            ds.Tables.Add(tblGiaoXu);
            Memory.UpdateDataSet(ds);
            if (!Memory.ShowError())
            {
                this.Text += txtTenGiaoXu.Text;
                MessageBox.Show("Đã cập nhật thông tin giáo xứ!");
            }
        }

        private void AssignGXData(DataRow row)
        {
            row[GiaoXuConst.TenGiaoXu] = txtTenGiaoXu.Text;
            row[GiaoXuConst.DiaChi] = txtDiaChi.Text;
            row[GiaoXuConst.Website] = txtWebsite.Text;
            row[GiaoXuConst.DienThoai] = txtDienThoai.Text;
            row[GiaoXuConst.Email] = txtEmail.Text;
            row[GiaoXuConst.Hinh] = txtHinh.Text;
            row[GiaoXuConst.GhiChu] = txtGhiChu.Rtf;
            try
            {
                if (System.IO.File.Exists(openFileDialog1.FileName))
                {
                    System.IO.FileInfo img = new System.IO.FileInfo(openFileDialog1.FileName);
                    string dest = Memory.AppPath + txtHinh.Text;
                    System.IO.File.Copy(img.FullName, dest, true);
                    Form main = Application.OpenForms["frmMain"];
                    ((frmMain)main).pictureBox1.BackgroundImage = Image.FromFile(img.FullName);
                }
            }
            catch { }
        }

        private void gxLinhMucList1_RowDoubleClick(object sender, Janus.Windows.GridEX.RowActionEventArgs e)
        {

            EditLinhMucRow(GxOperation.EDIT);
        }

        private void EditLinhMucRow(GxOperation op)
        {
            if (op == GxOperation.EDIT && (gxLinhMucList1.CurrentRow == null || (gxLinhMucList1.CurrentRow.DataRow as DataRowView) == null)) return;
            frmLinhMuc frm = new frmLinhMuc();
            frm.Operation = op;
            DataRow row = null;
            if (op == GxOperation.EDIT)
            {
                row = (gxLinhMucList1.CurrentRow.DataRow as DataRowView).Row;
                frm.Id = (int)row[LinhMucConst.MaLinhMuc];
                frm.AssignControlData();
                showForm(row, frm);
            }
            else
            {
                DataTable tbl = gxLinhMucList1.DataSource as DataTable;
                if (tbl == null)
                {
                    tbl = Memory.GetData(SqlConstants.SELECT_LINHMUC_LIST);
                    if (Memory.ShowError())
                    {
                        return;
                    }                    
                    gxLinhMucList1.DataSource = tbl;
                }
                row = tbl.NewRow();
                DialogResult rs = showForm(row, frm);
                if (rs == DialogResult.OK)
                {
                    tbl.Rows.Add(row);
                }
            }
        }

        private DialogResult showForm(DataRow row, frmBase frm)
        {
            DialogResult rs = DialogResult.Cancel;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                if (frm.DataReturn != null)
                {
                    row[LinhMucConst.MaLinhMuc] = frm.DataReturn[LinhMucConst.MaLinhMuc];
                    row[LinhMucConst.TenThanh] = frm.DataReturn[LinhMucConst.TenThanh];
                    row[LinhMucConst.HoTen] = frm.DataReturn[LinhMucConst.HoTen];
                    row[LinhMucConst.ChucVu] = frm.DataReturn[LinhMucConst.ChucVu];
                    row[LinhMucConst.NgaySinh] = frm.DataReturn[LinhMucConst.NgaySinh];
                    row[LinhMucConst.TuNgay] = frm.DataReturn[LinhMucConst.TuNgay];
                    row[LinhMucConst.DenNgay] = frm.DataReturn[LinhMucConst.DenNgay];
                    row[LinhMucConst.GhiChu] = frm.DataReturn[LinhMucConst.GhiChu];
                    rs = DialogResult.OK;
                }
            }
            return rs;
        }

        private void gxLinhMucList1_RowCountChanged(object sender, System.EventArgs e)
        {
            if (gxLinhMucList1.DataSource != null && gxLinhMucList1.RowCount > 0)
            {
                gxAddEdit1.EditButton.Enabled = true;
                gxAddEdit1.DeleteButton.Enabled = true;
            }
            else
            {
                gxAddEdit1.EditButton.Enabled = false;
                gxAddEdit1.DeleteButton.Enabled = false;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtHinh.Text = System.IO.Path.GetFileName(openFileDialog1.FileName);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UpdateProcess.sendGiaoXuInfo();
            base.OnClosing(e);
        }

        private void gxAddEdit1_Load(object sender, EventArgs e)
        {

        }
    }
}