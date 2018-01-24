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
    public partial class frmChangePass : frmBase
    {
        private string userName;
        public frmChangePass()
        {
            InitializeComponent();
            txtAgainNewPass.TextBox.PasswordChar = '*';
            txtNewPass.TextBox.PasswordChar = '*';
            txtOldPass.TextBox.PasswordChar = '*';
        }

        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                userName = value;
            }
        }

        private void gxTextField1_Load(object sender, EventArgs e)
        {
           
        }

        private void btnCofirm_Click(object sender, EventArgs e)
        {
            if (!CheckInput()) return;
            var dtb = Memory.GetData(SqlConstants.CHECK_LOGIN, userName, Memory.EnCodePassword(txtOldPass.Text));
            if (dtb != null && dtb.Rows.Count > 0)
            {
                Memory.ExecuteSqlCommand(SqlConstants.UPDATE_PASSWORD, Memory.EnCodePassword(txtNewPass.Text), userName);
                if (!Memory.ShowError())
                {
                    MessageBox.Show("Đổi mật khẩu thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    return;
                }
                MessageBox.Show("Đổi mật khẩu không thành công,thử lại sau", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
            else
            {
                MessageBox.Show("Mật khẩu cũ không chính xác,vui lòng thử lại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtOldPass.Focus();
                return;
            }

        }
        public bool CheckInput()
        {
            if (string.IsNullOrEmpty(txtOldPass.Text))
            {
                MessageBox.Show("Vui lòng điền mật khẩu cũ của bạn", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOldPass.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtNewPass.Text))
            {
                MessageBox.Show("Vui lòng điền mật khẩu mới của bạn", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNewPass.Focus();
                return false;
            }
            if (string.IsNullOrEmpty(txtAgainNewPass.Text))
            {
                MessageBox.Show("Vui lòng nhập lại mật khẩu mới của bạn", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAgainNewPass.Focus();
                return false;
            }
            return true;
        }

        private void frmChangePass_Load(object sender, EventArgs e)
        {
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }
    }
}
