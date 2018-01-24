namespace GiaoXu
{
    partial class frmResetPWord
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmResetPWord));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUserName = new GxControl.GxTextField();
            this.btnTestAccount = new GxControl.GxButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pnQuestion = new System.Windows.Forms.Panel();
            this.btnTestAnswer = new GxControl.GxButton();
            this.txtAnswer = new GxControl.GxTextField();
            this.txtQuestion = new GxControl.GxTextField();
            this.pnPassword = new System.Windows.Forms.Panel();
            this.btnCofirm = new GxControl.GxButton();
            this.txtPassword = new GxControl.GxTextField();
            this.txtCofirmPassword = new GxControl.GxTextField();
            this.panel1.SuspendLayout();
            this.pnQuestion.SuspendLayout();
            this.pnPassword.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(408, 62);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(109, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "TÌM LẠI MẬT KHẨU";
            // 
            // txtUserName
            // 
            this.txtUserName.AutoCompleteEnabled = true;
            this.txtUserName.AutoUpperFirstChar = false;
            this.txtUserName.BoxLeft = 0;
            this.txtUserName.EditEnabled = true;
            this.txtUserName.Label = "Tên tài khoản";
            this.txtUserName.Location = new System.Drawing.Point(21, 74);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtUserName.MaxLength = 32767;
            this.txtUserName.MultiLine = false;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.NumberInputRequired = true;
            this.txtUserName.NumberMode = false;
            this.txtUserName.ReadOnly = false;
            this.txtUserName.Size = new System.Drawing.Size(294, 26);
            this.txtUserName.TabIndex = 0;
            // 
            // btnTestAccount
            // 
            this.btnTestAccount.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnTestAccount.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnTestAccount.BackgroundImage")));
            this.btnTestAccount.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTestAccount.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnTestAccount.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnTestAccount.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnTestAccount.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTestAccount.Location = new System.Drawing.Point(321, 77);
            this.btnTestAccount.Name = "btnTestAccount";
            this.btnTestAccount.Size = new System.Drawing.Size(80, 23);
            this.btnTestAccount.TabIndex = 1;
            this.btnTestAccount.Text = "Kiểm tra";
            this.btnTestAccount.UseVisualStyleBackColor = false;
            this.btnTestAccount.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // pnQuestion
            // 
            this.pnQuestion.Controls.Add(this.btnTestAnswer);
            this.pnQuestion.Controls.Add(this.txtAnswer);
            this.pnQuestion.Controls.Add(this.txtQuestion);
            this.pnQuestion.Location = new System.Drawing.Point(0, 107);
            this.pnQuestion.Name = "pnQuestion";
            this.pnQuestion.Size = new System.Drawing.Size(408, 62);
            this.pnQuestion.TabIndex = 7;
            // 
            // btnTestAnswer
            // 
            this.btnTestAnswer.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnTestAnswer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnTestAnswer.BackgroundImage")));
            this.btnTestAnswer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTestAnswer.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnTestAnswer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnTestAnswer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnTestAnswer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTestAnswer.Location = new System.Drawing.Point(321, 34);
            this.btnTestAnswer.Name = "btnTestAnswer";
            this.btnTestAnswer.Size = new System.Drawing.Size(80, 23);
            this.btnTestAnswer.TabIndex = 10;
            this.btnTestAnswer.Text = "Kiểm tra";
            this.btnTestAnswer.UseVisualStyleBackColor = false;
            this.btnTestAnswer.Click += new System.EventHandler(this.btnTestAnswer_Click);
            // 
            // txtAnswer
            // 
            this.txtAnswer.AutoCompleteEnabled = false;
            this.txtAnswer.AutoUpperFirstChar = false;
            this.txtAnswer.BoxLeft = 0;
            this.txtAnswer.EditEnabled = true;
            this.txtAnswer.Label = "Câu trả lời";
            this.txtAnswer.Location = new System.Drawing.Point(35, 35);
            this.txtAnswer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtAnswer.MaxLength = 32767;
            this.txtAnswer.MultiLine = false;
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.NumberInputRequired = true;
            this.txtAnswer.NumberMode = false;
            this.txtAnswer.ReadOnly = false;
            this.txtAnswer.Size = new System.Drawing.Size(280, 26);
            this.txtAnswer.TabIndex = 9;
            // 
            // txtQuestion
            // 
            this.txtQuestion.AutoCompleteEnabled = true;
            this.txtQuestion.AutoUpperFirstChar = false;
            this.txtQuestion.BoxLeft = 0;
            this.txtQuestion.EditEnabled = true;
            this.txtQuestion.Label = "Câu hỏi";
            this.txtQuestion.Location = new System.Drawing.Point(49, 1);
            this.txtQuestion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtQuestion.MaxLength = 32767;
            this.txtQuestion.MultiLine = false;
            this.txtQuestion.Name = "txtQuestion";
            this.txtQuestion.NumberInputRequired = true;
            this.txtQuestion.NumberMode = false;
            this.txtQuestion.ReadOnly = false;
            this.txtQuestion.Size = new System.Drawing.Size(352, 26);
            this.txtQuestion.TabIndex = 8;
            // 
            // pnPassword
            // 
            this.pnPassword.Controls.Add(this.btnCofirm);
            this.pnPassword.Controls.Add(this.txtPassword);
            this.pnPassword.Controls.Add(this.txtCofirmPassword);
            this.pnPassword.Location = new System.Drawing.Point(0, 167);
            this.pnPassword.Name = "pnPassword";
            this.pnPassword.Size = new System.Drawing.Size(408, 147);
            this.pnPassword.TabIndex = 11;
            // 
            // btnCofirm
            // 
            this.btnCofirm.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnCofirm.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCofirm.BackgroundImage")));
            this.btnCofirm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCofirm.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnCofirm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnCofirm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnCofirm.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCofirm.Location = new System.Drawing.Point(102, 92);
            this.btnCofirm.Name = "btnCofirm";
            this.btnCofirm.Size = new System.Drawing.Size(217, 44);
            this.btnCofirm.TabIndex = 9;
            this.btnCofirm.Text = "Xác nhận";
            this.btnCofirm.UseVisualStyleBackColor = false;
            this.btnCofirm.Click += new System.EventHandler(this.btnCofirm_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.AutoCompleteEnabled = false;
            this.txtPassword.AutoUpperFirstChar = false;
            this.txtPassword.BoxLeft = 0;
            this.txtPassword.EditEnabled = true;
            this.txtPassword.Label = "Mật khẩu mới";
            this.txtPassword.Location = new System.Drawing.Point(21, 10);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPassword.MaxLength = 32767;
            this.txtPassword.MultiLine = false;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.NumberInputRequired = true;
            this.txtPassword.NumberMode = false;
            this.txtPassword.ReadOnly = false;
            this.txtPassword.Size = new System.Drawing.Size(384, 26);
            this.txtPassword.TabIndex = 7;
            // 
            // txtCofirmPassword
            // 
            this.txtCofirmPassword.AutoCompleteEnabled = false;
            this.txtCofirmPassword.AutoUpperFirstChar = false;
            this.txtCofirmPassword.BoxLeft = 0;
            this.txtCofirmPassword.EditEnabled = true;
            this.txtCofirmPassword.Label = "Nhập lại mật khẩu";
            this.txtCofirmPassword.Location = new System.Drawing.Point(0, 44);
            this.txtCofirmPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCofirmPassword.MaxLength = 32767;
            this.txtCofirmPassword.MultiLine = false;
            this.txtCofirmPassword.Name = "txtCofirmPassword";
            this.txtCofirmPassword.NumberInputRequired = true;
            this.txtCofirmPassword.NumberMode = false;
            this.txtCofirmPassword.ReadOnly = false;
            this.txtCofirmPassword.Size = new System.Drawing.Size(405, 26);
            this.txtCofirmPassword.TabIndex = 8;
            // 
            // frmResetPWord
            // 
            this.AcceptButton = this.btnTestAccount;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 314);
            this.Controls.Add(this.pnPassword);
            this.Controls.Add(this.pnQuestion);
            this.Controls.Add(this.btnTestAccount);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.panel1);
            this.Name = "frmResetPWord";
            this.Text = "Tìm lại mật khẩu";
            this.Load += new System.EventHandler(this.frmResetPWord_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnQuestion.ResumeLayout(false);
            this.pnPassword.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private GxControl.GxTextField txtUserName;
        private GxControl.GxButton btnTestAccount;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel pnQuestion;
        private GxControl.GxButton btnTestAnswer;
        private GxControl.GxTextField txtAnswer;
        private GxControl.GxTextField txtQuestion;
        private System.Windows.Forms.Panel pnPassword;
        private GxControl.GxButton btnCofirm;
        private GxControl.GxTextField txtPassword;
        private GxControl.GxTextField txtCofirmPassword;
    }
}