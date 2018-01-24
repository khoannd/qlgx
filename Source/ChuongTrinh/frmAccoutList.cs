using GxControl;
using GxGlobal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmAccoutList : frmBase
    {
        /// <summary>
        /// Class thực hiện một số tác vụ :
        ///     
        ///     + Thêm/xóa/sửa một acccount người dùng với một số thông tin.
        ///         ** Có các button phục vụ cho từng thao tác => các event hanler xử lý các button đó.
        ///         
        /// </summary>
        public frmAccoutList()
        {
            InitializeComponent();
            cbPhanQuyen.combo.DropDownStyle = ComboBoxStyle.DropDownList;
            pnQuestion.Visible = false;
            gxAddEdit.AddClick += GxAddEdit_AddClick;
            gxAddEdit.EditClick += GxAddEdit_EditClick;
            gxAddEdit.DeleteClick += GxAddEdit_DeleteClick;
            gxListAccout1.SelectionChanged += GxListAccout1_SelectionChanged;
            gxListAccout1.RowCountChanged += GxListAccout1_RowCountChanged;
            gxAddEdit.SelectClick += GxAddEdit_UpdateClick;
            gxAddEdit.ReloadClick += GxAddEdit_ReloadClick;
            gxAddEdit.EditButton.Text = "&Sửa";
            Operation = GxOperation.NONE;
        }
        /// <summary>
        /// Event xử lý khi người dùng nhấn "Tải lại danh sách"
        /// ** Các bước : 1.Gọi hàm LoadData() của Gridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GxAddEdit_ReloadClick(object sender, EventArgs e)
        {
            gxListAccout1.LoadData();
        }
        /// <summary>
        /// **Tác vụ : Kiểm tra account đã tồn tại chưa khi người dùng đki tài khoản;
        /// **Logic : Sử dụng Query với tham số là "Tên tài khoản" => nếu table có số Row lớn hơn 0 => Account đã tồn tại.
        /// </summary>
        /// <returns></returns>
        public bool IsHasAccount()
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_ACCOUNT, txtUserName.Text);
            if (tbl != null && tbl.Rows.Count > 0) return true;
            return false;
        }
        /// <summary>
        /// ** Tác vụ : 
        /// + Gọi hàm Update dữ liệu xuống datable khi người dùng nhấn "Lưu";
        /// + Gọi hàm EnableEditControls(false) xử lý ẩn hiện các button cùng một số hiệu ứng đi kèm.
        ///             
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GxAddEdit_UpdateClick(object sender, EventArgs e)
        {
            if (!checkInput()) return;
            UpdateData();
            EnableEditControls(false);
        }
        /// <summary>
        /// ** Tác vụ : Xử lý khi số row của Gridview thay đổi = > Mục đích để ẩn chức năng "Sửa","Xóa" khi số row = 0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GxListAccout1_RowCountChanged(object sender, EventArgs e)
        {
            EnableWhenRowIsZezo();
        }
        public void EnableWhenRowIsZezo()
        {
            if (gxListAccout1.DataSource == null) return;
            if (gxListAccout1.RowCount == 0)
            {
                gxAddEdit.DeleteButton.Enabled = false;
                gxAddEdit.EditButton.Enabled = false;
                Clear();
                grpInfor.Enabled = false;
                return;
            }
            gxAddEdit.DeleteButton.Enabled = true;
            gxAddEdit.EditButton.Enabled = true;
        }

        /// <summary>
        /// ** Tác vụ : Load dữ liệu từ lưới lên các textbox khi người dùng chọn row khác nhau trên lưới
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GxListAccout1_SelectionChanged(object sender, EventArgs e)
        {
            changeSelection();
            EnableEditControls(false);
        }

        private void GxAddEdit_DeleteClick(object sender, EventArgs e)
        {

            //DELETE ở đây
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa ?", "Nhắc nhở", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            if (gxListAccout1.CurrentRow != null && (gxListAccout1.CurrentRow.DataRow as DataRowView) != null)
            {    
                Memory.ExecuteSqlCommand(SqlConstants.DELETE_ACCOUNT, txtUserName.Text);
                if (!Memory.ShowError())
                {
                    gxListAccout1.LoadData();
                    MessageBox.Show("Xóa tài khoản thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Xóa tài khoản không thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);            
                }
            }
        }
        bool checkInput()
        {
            Regex re;

            if (string.IsNullOrEmpty(txtFullName.Text))
            {
                MessageBox.Show("Vui lòng điền họ tên người dùng", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                MessageBox.Show("Vui lòng điền tên đăng nhập", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return false;
            }
            re = new Regex("^[a-zA-Z0-9]+$");
            if (!re.IsMatch(txtUserName.Text))
            {
                MessageBox.Show("Tên đăng nhập không hợp lệ", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return false;
            }
            if (Operation == GxOperation.ADD && IsHasAccount())
            {
                MessageBox.Show("Tên tài khoản đã tồn tại,thử một tên khác", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUserName.Focus();
                return false;
            }
            if (operation == GxOperation.ADD)
            {
                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Vui lòng điền mật khẩu", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }
            }
            if (txtPassword.Text != "" || operation == GxOperation.ADD)
            {
                if (!txtPassword.Text.Equals(txtCofirmPassword.Text))
                {
                    MessageBox.Show("Mật khẩu và nhập lại mật khẩu không khớp", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCofirmPassword.Focus();
                    return false;
                }
            }
            if (cbPhanQuyen.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn loại tài khoản", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbPhanQuyen.Focus();
                return false;
            }
            if (pnQuestion.Visible)
            {
                if (string.IsNullOrEmpty(txtQuestion.Text))
                {
                    MessageBox.Show("Vui lòng nhập câu hỏi dùng để khôi phục mật khẩu", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtQuestion.Focus();
                    return false;
                }
                if (string.IsNullOrEmpty(txtAnswer.Text))
                {
                    MessageBox.Show("Vui lòng nhập câu trả lời dùng để khôi phục mật khẩu", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtAnswer.Focus();
                    return false;
                }
            }
            return true;
        }
        private void GxAddEdit_EditClick(object sender, EventArgs e)
        {
            if (operation == GxOperation.EDIT)
            {
                cancelEdit();
                return;
            }
            this.Operation = GxOperation.EDIT;
            EnableEditControls(true);
        }
        private void AssignData(DataRow row)
        {
            row[AccountConst.HoTenNguoiDung] = txtFullName.Text;
            row[AccountConst.TenTaiKhoan] = txtUserName.Text;
            row[AccountConst.MatKhau] = Memory.EnCodePassword(txtPassword.Text);
            row[AccountConst.Email] = txtEmail.Text;
            row[AccountConst.SoDienThoai] = txtPhone.Text;
            row[AccountConst.LoaiTaiKhoan] = cbPhanQuyen.SelectedIndex;
            row[AccountConst.CauHoiGoiY] = txtQuestion.Text;
            row[AccountConst.CauTraLoiGoiY] = txtAnswer.Text;

        }
        public bool UpdateData()
        {

            DataTable tbl = Memory.GetData(string.Concat(SqlConstants.SELECT_LIST_ACCOUNT, string.Format(" AND TenTaiKhoan = ? ")), txtUserName.Text);
            if (tbl != null)
            {
                if (Operation == GxOperation.ADD)
                {
                    DataRow row = tbl.NewRow();
                    AssignData(row);
                    tbl.Rows.Add(row);
                    tbl.TableName = AccountConst.TableName;
                    DataSet ds = new DataSet();
                    ds.Tables.Add(tbl);
                    Memory.UpdateDataSet(ds);
                    if (!Memory.ShowError())
                    {
                        MessageBox.Show("Thêm tài khoản thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        row.AcceptChanges();

                        DataTable tbl1 = (DataTable)gxListAccout1.DataSource;
                        DataRow row1 = tbl1.NewRow();
                        row1[AccountConst.HoTenNguoiDung] = txtFullName.Text;
                        row1[AccountConst.TenTaiKhoan] = txtUserName.Text;
                        row1[AccountConst.Email] = txtEmail.Text;
                        row1[AccountConst.SoDienThoai] = txtPhone.Text;
                        row1[AccountConst.TenLoai] = cbPhanQuyen.combo.Items[cbPhanQuyen.SelectedIndex];
                        tbl1.Rows.Add(row1);
                        //tại đây
                        gxListAccout1.Row = gxListAccout1.RowCount - 1;
                        return true;
                    }
                    MessageBox.Show("Thêm tài khoản không thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (Operation == GxOperation.EDIT)
                {
                    if (gxListAccout1.CurrentRow != null && (gxListAccout1.CurrentRow.DataRow as DataRowView) != null)
                    {

                        DataRow row = (gxListAccout1.CurrentRow.DataRow as DataRowView).Row;

                        row[AccountConst.HoTenNguoiDung] = txtFullName.Text;
                        row[AccountConst.TenTaiKhoan] = txtUserName.Text;
                        row[AccountConst.Email] = txtEmail.Text;
                        row[AccountConst.SoDienThoai] = txtPhone.Text;
                        row[AccountConst.TenLoai] = cbPhanQuyen.combo.Items[cbPhanQuyen.SelectedIndex];
                        if (txtPassword.Text != "")
                        {
                            Memory.ExecuteSqlCommand(SqlConstants.UPDATE_TAIKHOAN, new object[] { txtFullName.Text, Memory.EnCodePassword(txtPassword.Text), txtEmail.Text, txtPhone.Text, cbPhanQuyen.SelectedIndex, txtQuestion.Text, txtAnswer.Text, txtUserName.Text });
                        }
                        else
                        {
                            Memory.ExecuteSqlCommand(SqlConstants.UPDATE_TAIKHOAN_NOTPASSWORD, new object[] { txtFullName.Text, txtEmail.Text, txtPhone.Text, cbPhanQuyen.SelectedIndex, txtQuestion.Text, txtAnswer.Text, txtUserName.Text });
                        }
                        if (!Memory.ShowError())
                        {
                            row.AcceptChanges();
                            MessageBox.Show("Tài khoản đã được cập nhật!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                        MessageBox.Show("Cập nhật tài khoản không thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        row.RejectChanges();
                    }
                }
            }
            return false;
        }
        private void GxAddEdit_AddClick(object sender, EventArgs e)
        {
            if (operation == GxOperation.ADD)
            {
                cancelEdit();
                return;
            }
            this.Operation = GxOperation.ADD;
            EnableEditControls(true);
            Clear();
        }

        private void EnableEditControls(bool enabled)
        {
            if (!enabled)
            {
                grpInfor.Enabled = false;
                gxAddEdit.SelectButton.Enabled = false;
                gxAddEdit.AddButton.Enabled = true;
                DataTable tb = gxListAccout1.DataSource as DataTable;
                if (tb != null && tb.Rows.Count > 0)
                {
                    gxAddEdit.EditButton.Enabled = true;
                    gxAddEdit.DeleteButton.Enabled = true;
                }
                operation = GxOperation.NONE;
                gxAddEdit.AddButton.Text = "&Thêm";
                gxAddEdit.EditButton.Text = "&Sửa";
                lbRcomPass.Visible = false;
                this.AcceptButton = gxAddEdit.AddButton;
                txtPassword.Text = "";
                txtCofirmPassword.Text = "";
                return;
            }
            if (operation == GxOperation.ADD || operation == GxOperation.EDIT)
            {
                grpInfor.Enabled = true;
                txtFullName.Focus();
            }
            gxAddEdit.SelectButton.Enabled = true;
            gxAddEdit.AddButton.Enabled = false;
            gxAddEdit.EditButton.Enabled = false;
            gxAddEdit.DeleteButton.Enabled = false;
            if (operation == GxOperation.ADD)
            {
                gxAddEdit.AddButton.Enabled = true;
                txtUserName.ReadOnly = false;
                gxAddEdit.AddButton.Text = "&Thôi";
            }
            else if (operation == GxOperation.EDIT)
            {
                gxAddEdit.EditButton.Enabled = true;
                txtUserName.ReadOnly = true;
                gxAddEdit.EditButton.Text = "&Thôi";
                lbRcomPass.Visible = true;
            }
            this.AcceptButton = gxAddEdit.SelectButton;

        }
        private void changeSelection()
        {
            if (gxListAccout1.CurrentRow != null && (gxListAccout1.CurrentRow.DataRow as DataRowView) != null)
            {
                var row = (gxListAccout1.CurrentRow.DataRow as DataRowView);
                txtFullName.Text = row[AccountConst.HoTenNguoiDung].ToString();
                txtUserName.Text = row[AccountConst.TenTaiKhoan].ToString();
                txtEmail.Text = row[AccountConst.Email].ToString();
                txtPhone.Text = row[AccountConst.SoDienThoai].ToString();
                txtQuestion.Text = row[AccountConst.CauHoiGoiY].ToString();
                txtAnswer.Text = row[AccountConst.CauTraLoiGoiY].ToString();
                cbPhanQuyen.SelectedText = row[AccountConst.TenLoai].ToString();
            }
        }
        private void cancelEdit()
        {
            if (operation == GxOperation.ADD || operation == GxOperation.EDIT)
            {
                EnableEditControls(false);
                gxAddEdit.AddButton.Focus();
                if (gxListAccout1.RowCount > 0)
                {
                    changeSelection();
                }

            }
        }
        public void Clear()
        {
            foreach (Control control in grpInfor.Controls)
            {
                if (control is GxTextField) control.Text = "";
                else if (control is GxComboField) (control as GxComboField).SelectedIndex = -1;
            }
            grpInfor.Enabled = true;
        }
        private void frmAccoutList_Load(object sender, EventArgs e)
        {
            
            txtPassword.TextBox.PasswordChar = '*';
            txtCofirmPassword.TextBox.PasswordChar = '*';
            gxAddEdit.SelectButton.Text = "&Lưu";
            EnableWhenRowIsZezo();
            lbRcomPass.Visible = false;
            grpInfor.Enabled = false;
            changeSelection();
        }
        private void cbPhanQuyen_Load(object sender, EventArgs e)
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_LIST_LOAI_TAI_KHOAN);
            foreach (DataRow row in tbl.Rows)
            {
                cbPhanQuyen.combo.Items.Add(row[AccountConst.TenLoai]);
            }
        }
      

        /// <summary>
        /// ** Tác vụ : ẩn hiện các trường :"Câu hỏi bí mật","Câu trả lời bí mật" phụ thuộc vào loại tài khoản người dùng chọn trên Combobox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPhanQuyen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPhanQuyen.SelectedIndex == (int)LoaiTaiKhoan.QUANTRI)
            {
                pnQuestion.Visible = true;
            }
            else
            {
                pnQuestion.Visible = false;
                txtQuestion.Text = "";
                txtAnswer.Text = "";
            }
        }

        private void pnQuestion_VisibleChanged(object sender, EventArgs e)
        {
            if (!pnQuestion.Visible)
            {
                txtQuestion.Text = "";
                txtAnswer.Text = "";
            }
        }
    }
    public enum LoaiTaiKhoan
    {
        QUANTRI,
        SUB1,
        SUB2,
    }
}
