using GxControl;
using GxGlobal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmLogin : frmBase
    {
       
        frmMain frm;
        public frmLogin()
        {
            InitializeComponent();
            txtPassword.TextBox.PasswordChar = '*';
            if (!IsCanLogin())
            {
                this.Hide();
                frm = new frmMain();
                frm.IsAdmin = true;
                frm.ShowDialog();
                
                //this.Close();
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            
        }

        private void llbForget_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var rs = MessageBox.Show("Chức năng quên mật khẩu chỉ dành cho quản trị viên.Chọn [Yes] nếu bạn là quản trị viên,nếu không,chọn [No] và liên hệ với quản trị viên để được cấp lại mật khẩu .", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(rs == DialogResult.Yes)
            {
                frmResetPWord frm = new frmResetPWord();
                this.Hide();
                frm.ShowDialog();
                this.Show();
                txtUsername.Focus();
            }
            else
            {
                return;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            DataTable tbl = Memory.GetData(SqlConstants.CHECK_LOGIN,txtUsername.Text, Memory.EnCodePassword(txtPassword.Text));
            if(tbl != null && tbl.Rows.Count > 0)
            {
                this.Hide();
                frm = new frmMain();
                frm.UserName = tbl.Rows[0][AccountConst.TenTaiKhoan].ToString();
                if (tbl.Rows[0][AccountConst.LoaiTaiKhoan].Equals((int)LoaiTaiKhoan.QUANTRI))
                {
                    frm.IsAdmin = true;
                }
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }
        }
        public bool IsCanLogin()
        {
            DataTable tbl = Memory.GetData(SqlConstants.SELECT_LIST_ACCOUNT);
            if (tbl != null && tbl.Rows.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}