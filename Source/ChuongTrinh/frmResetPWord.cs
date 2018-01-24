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
    public partial class frmResetPWord : frmBase
    {
        DataTable tbl;
        public frmResetPWord()
        {
            InitializeComponent();
            btnCofirm.Enabled = false;
            txtPassword.TextBox.PasswordChar = '*';
            txtCofirmPassword.TextBox.PasswordChar = '*';
            pnQuestion.Visible = false;
            pnPassword.Visible = false;
            txtQuestion.TextBox.ReadOnly = true;
        }

        private void gxTextField1_Load(object sender, EventArgs e)
        {

        }

        private void frmResetPWord_Load(object sender, EventArgs e)
        {
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        }

       
        public bool CheckInput()
        {         
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }
            if (!txtCofirmPassword.Text.Equals(txtPassword.Text))
            {
                MessageBox.Show("Mật khẩu và nhập lại mật khẩu không khớp ", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCofirmPassword.Focus();
                return false;

            }
            return true;
        }
        private void btnTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtUserName.Focus();
                return;
            }
            tbl = Memory.GetData(SqlConstants.SELECT_QUESTION, txtUserName.Text);
            if (tbl == null || tbl.Rows.Count <= 0 || tbl.Rows[0][AccountConst.CauHoiGoiY].Equals(""))
            {
                toolTip1.Show("Tài khoản không hợp lệ.", txtUserName,500);
                return;
            }
            pnQuestion.Visible = true;
            txtQuestion.Text = tbl.Rows[0][AccountConst.CauHoiGoiY].ToString();
            txtAnswer.Focus();
            btnCofirm.Enabled = true;
            this.AcceptButton =btnTestAnswer;
        }

        private void btnTestAnswer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAnswer.Text))
            {
                MessageBox.Show("Vui lòng nhập câu trả lời của bạn,câu trả lời chính là câu trả lời bạn nhập khi tạo tài khoản.", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAnswer.Focus();
                return;
            }
            if (tbl.Rows[0][AccountConst.CauTraLoiGoiY].ToString() != txtAnswer.Text)
            {
                MessageBox.Show("Câu trả lời không chính xác,vui lòng thử lại.Nếu bạn quên câu trả lời,vui lòng liên hệ người chịu trách nhiệm phần mềm để biết thêm chi tiết", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            pnPassword.Visible = true;
            this.AcceptButton = btnCofirm;
            txtPassword.Focus();

        }

        private void btnCofirm_Click(object sender, EventArgs e)
        {
            if (!CheckInput()) return;

            // đổi pass
            Memory.ExecuteSqlCommand(SqlConstants.UPDATE_PASSWORD, Memory.EnCodePassword(txtPassword.Text), txtUserName.Text);
            if (!Memory.ShowError())
            {
                MessageBox.Show("Cập nhật mật khẩu thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                return;
            }

            MessageBox.Show("Cập nhật mật khẩu không thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
