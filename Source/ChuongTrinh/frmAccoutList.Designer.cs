namespace GiaoXu
{
    partial class frmAccoutList
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
            this.grpInfor = new GxControl.GxGroupBox();
            this.lbRcomPass = new System.Windows.Forms.Label();
            this.pnQuestion = new System.Windows.Forms.Panel();
            this.txtAnswer = new GxControl.GxTextField();
            this.txtQuestion = new GxControl.GxTextField();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPhone = new GxControl.GxTextField();
            this.cbPhanQuyen = new GxControl.GxComboField();
            this.txtEmail = new GxControl.GxTextField();
            this.txtCofirmPassword = new GxControl.GxTextField();
            this.txtPassword = new GxControl.GxTextField();
            this.txtUserName = new GxControl.GxTextField();
            this.txtFullName = new GxControl.GxTextField();
            this.gxGroupBox2 = new GxControl.GxGroupBox();
            this.gxListAccout1 = new GxControl.GxListAccout();
            this.gxGroupBox3 = new GxControl.GxGroupBox();
            this.gxAddEdit = new GxControl.GxAddEdit();
            ((System.ComponentModel.ISupportInitialize)(this.grpInfor)).BeginInit();
            this.grpInfor.SuspendLayout();
            this.pnQuestion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).BeginInit();
            this.gxGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxListAccout1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox3)).BeginInit();
            this.gxGroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpInfor
            // 
            this.grpInfor.Controls.Add(this.lbRcomPass);
            this.grpInfor.Controls.Add(this.pnQuestion);
            this.grpInfor.Controls.Add(this.txtPhone);
            this.grpInfor.Controls.Add(this.cbPhanQuyen);
            this.grpInfor.Controls.Add(this.txtEmail);
            this.grpInfor.Controls.Add(this.txtCofirmPassword);
            this.grpInfor.Controls.Add(this.txtPassword);
            this.grpInfor.Controls.Add(this.txtUserName);
            this.grpInfor.Controls.Add(this.txtFullName);
            this.grpInfor.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpInfor.Location = new System.Drawing.Point(0, 0);
            this.grpInfor.Name = "grpInfor";
            this.grpInfor.Size = new System.Drawing.Size(1001, 198);
            this.grpInfor.TabIndex = 2;
            this.grpInfor.Text = "Thông tin tài khoản";
            // 
            // lbRcomPass
            // 
            this.lbRcomPass.AutoSize = true;
            this.lbRcomPass.ForeColor = System.Drawing.Color.Red;
            this.lbRcomPass.Location = new System.Drawing.Point(51, 148);
            this.lbRcomPass.Name = "lbRcomPass";
            this.lbRcomPass.Size = new System.Drawing.Size(296, 13);
            this.lbRcomPass.TabIndex = 16;
            this.lbRcomPass.Text = "*Để \"Mật khẩu\" và \"Nhập lại mật khẩu\" trống nếu không đổi";
            // 
            // pnQuestion
            // 
            this.pnQuestion.Controls.Add(this.txtAnswer);
            this.pnQuestion.Controls.Add(this.txtQuestion);
            this.pnQuestion.Controls.Add(this.label1);
            this.pnQuestion.Location = new System.Drawing.Point(583, 110);
            this.pnQuestion.Name = "pnQuestion";
            this.pnQuestion.Size = new System.Drawing.Size(406, 82);
            this.pnQuestion.TabIndex = 15;
            this.pnQuestion.VisibleChanged += new System.EventHandler(this.pnQuestion_VisibleChanged);
            // 
            // txtAnswer
            // 
            this.txtAnswer.AutoCompleteEnabled = true;
            this.txtAnswer.AutoUpperFirstChar = false;
            this.txtAnswer.BoxLeft = 0;
            this.txtAnswer.EditEnabled = true;
            this.txtAnswer.Label = "Câu trả lời";
            this.txtAnswer.Location = new System.Drawing.Point(216, 49);
            this.txtAnswer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtAnswer.MaxLength = 32767;
            this.txtAnswer.MultiLine = false;
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.NumberInputRequired = true;
            this.txtAnswer.NumberMode = false;
            this.txtAnswer.ReadOnly = false;
            this.txtAnswer.Size = new System.Drawing.Size(189, 26);
            this.txtAnswer.TabIndex = 8;
            // 
            // txtQuestion
            // 
            this.txtQuestion.AutoCompleteEnabled = true;
            this.txtQuestion.AutoUpperFirstChar = false;
            this.txtQuestion.BoxLeft = 0;
            this.txtQuestion.EditEnabled = true;
            this.txtQuestion.Label = "Câu hỏi";
            this.txtQuestion.Location = new System.Drawing.Point(26, 49);
            this.txtQuestion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtQuestion.MaxLength = 32767;
            this.txtQuestion.MultiLine = false;
            this.txtQuestion.Name = "txtQuestion";
            this.txtQuestion.NumberInputRequired = true;
            this.txtQuestion.NumberMode = false;
            this.txtQuestion.ReadOnly = false;
            this.txtQuestion.Size = new System.Drawing.Size(184, 26);
            this.txtQuestion.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(1, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(402, 37);
            this.label1.TabIndex = 17;
            this.label1.Text = "Dành riêng cho quản trị viên dùng để khôi phục mật khẩu,hãy tự đặt một câu hỏi sa" +
    "u đó trả lời,chúng tôi sẽ sử dụng câu hỏi đó để giúp bạn khôi phục mật khẩu(*) :" +
    "";
            // 
            // txtPhone
            // 
            this.txtPhone.AutoCompleteEnabled = true;
            this.txtPhone.AutoUpperFirstChar = false;
            this.txtPhone.BoxLeft = 0;
            this.txtPhone.EditEnabled = true;
            this.txtPhone.Label = "Số điện thoại";
            this.txtPhone.Location = new System.Drawing.Point(583, 49);
            this.txtPhone.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPhone.MaxLength = 32767;
            this.txtPhone.MultiLine = false;
            this.txtPhone.Name = "txtPhone";
            this.txtPhone.NumberInputRequired = true;
            this.txtPhone.NumberMode = false;
            this.txtPhone.ReadOnly = false;
            this.txtPhone.Size = new System.Drawing.Size(406, 26);
            this.txtPhone.TabIndex = 5;
            // 
            // cbPhanQuyen
            // 
            this.cbPhanQuyen.AutoUpperFirstChar = false;
            this.cbPhanQuyen.BoxLeft = 0;
            this.cbPhanQuyen.EditEnabled = true;
            this.cbPhanQuyen.Label = "Loại tài khoản";
            this.cbPhanQuyen.Location = new System.Drawing.Point(578, 83);
            this.cbPhanQuyen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbPhanQuyen.MaxLength = 0;
            this.cbPhanQuyen.Name = "cbPhanQuyen";
            this.cbPhanQuyen.SelectedIndex = -1;
            this.cbPhanQuyen.SelectedText = "";
            this.cbPhanQuyen.SelectedValue = null;
            this.cbPhanQuyen.Size = new System.Drawing.Size(411, 26);
            this.cbPhanQuyen.TabIndex = 6;
            this.cbPhanQuyen.Tag = "";
            this.cbPhanQuyen.SelectedIndexChanged += new System.EventHandler(this.cbPhanQuyen_SelectedIndexChanged);
            this.cbPhanQuyen.Load += new System.EventHandler(this.cbPhanQuyen_Load);
            // 
            // txtEmail
            // 
            this.txtEmail.AutoCompleteEnabled = true;
            this.txtEmail.AutoUpperFirstChar = false;
            this.txtEmail.BoxLeft = 0;
            this.txtEmail.EditEnabled = true;
            this.txtEmail.Label = "Email";
            this.txtEmail.Location = new System.Drawing.Point(621, 15);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEmail.MaxLength = 32767;
            this.txtEmail.MultiLine = false;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.NumberInputRequired = true;
            this.txtEmail.NumberMode = false;
            this.txtEmail.ReadOnly = false;
            this.txtEmail.Size = new System.Drawing.Size(368, 26);
            this.txtEmail.TabIndex = 4;
            // 
            // txtCofirmPassword
            // 
            this.txtCofirmPassword.AutoCompleteEnabled = false;
            this.txtCofirmPassword.AutoUpperFirstChar = false;
            this.txtCofirmPassword.BoxLeft = 0;
            this.txtCofirmPassword.EditEnabled = true;
            this.txtCofirmPassword.Label = "Nhập lại mật khẩu";
            this.txtCofirmPassword.Location = new System.Drawing.Point(54, 118);
            this.txtCofirmPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCofirmPassword.MaxLength = 32767;
            this.txtCofirmPassword.MultiLine = false;
            this.txtCofirmPassword.Name = "txtCofirmPassword";
            this.txtCofirmPassword.NumberInputRequired = true;
            this.txtCofirmPassword.NumberMode = false;
            this.txtCofirmPassword.ReadOnly = false;
            this.txtCofirmPassword.Size = new System.Drawing.Size(436, 26);
            this.txtCofirmPassword.TabIndex = 3;
            // 
            // txtPassword
            // 
            this.txtPassword.AutoCompleteEnabled = false;
            this.txtPassword.AutoUpperFirstChar = false;
            this.txtPassword.BoxLeft = 0;
            this.txtPassword.EditEnabled = true;
            this.txtPassword.Label = "Mật khẩu";
            this.txtPassword.Location = new System.Drawing.Point(95, 83);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPassword.MaxLength = 32767;
            this.txtPassword.MultiLine = false;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.NumberInputRequired = true;
            this.txtPassword.NumberMode = false;
            this.txtPassword.ReadOnly = false;
            this.txtPassword.Size = new System.Drawing.Size(395, 26);
            this.txtPassword.TabIndex = 2;
            // 
            // txtUserName
            // 
            this.txtUserName.AutoCompleteEnabled = true;
            this.txtUserName.AutoUpperFirstChar = false;
            this.txtUserName.BoxLeft = 0;
            this.txtUserName.EditEnabled = true;
            this.txtUserName.Label = "Tên tài khoản";
            this.txtUserName.Location = new System.Drawing.Point(76, 49);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtUserName.MaxLength = 32767;
            this.txtUserName.MultiLine = false;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.NumberInputRequired = true;
            this.txtUserName.NumberMode = false;
            this.txtUserName.ReadOnly = false;
            this.txtUserName.Size = new System.Drawing.Size(414, 26);
            this.txtUserName.TabIndex = 1;
            // 
            // txtFullName
            // 
            this.txtFullName.AutoCompleteEnabled = true;
            this.txtFullName.AutoUpperFirstChar = true;
            this.txtFullName.BoxLeft = 0;
            this.txtFullName.EditEnabled = true;
            this.txtFullName.Label = "Họ tên ";
            this.txtFullName.Location = new System.Drawing.Point(106, 15);
            this.txtFullName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtFullName.MaxLength = 32767;
            this.txtFullName.MultiLine = false;
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.NumberInputRequired = true;
            this.txtFullName.NumberMode = false;
            this.txtFullName.ReadOnly = false;
            this.txtFullName.Size = new System.Drawing.Size(384, 26);
            this.txtFullName.TabIndex = 0;
            // 
            // gxGroupBox2
            // 
            this.gxGroupBox2.Controls.Add(this.gxListAccout1);
            this.gxGroupBox2.Controls.Add(this.gxGroupBox3);
            this.gxGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox2.Location = new System.Drawing.Point(0, 198);
            this.gxGroupBox2.Name = "gxGroupBox2";
            this.gxGroupBox2.Size = new System.Drawing.Size(1001, 334);
            this.gxGroupBox2.TabIndex = 1;
            this.gxGroupBox2.Text = "Danh sách tài khoản người dùng";
            // 
            // gxListAccout1
            // 
            this.gxListAccout1.AllowColumnDrag = false;
            this.gxListAccout1.AllowDelete = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListAccout1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxListAccout1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListAccout1.Arguments = null;
            this.gxListAccout1.AutoLoadGridFormat = false;
            this.gxListAccout1.ColumnAutoResize = true;
            this.gxListAccout1.DisableParentOnLoadData = true;
            this.gxListAccout1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxListAccout1.DynamicFiltering = true;
            this.gxListAccout1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxListAccout1.GroupByBoxVisible = false;
            this.gxListAccout1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxListAccout1.KeepRowSettings = true;
            this.gxListAccout1.Location = new System.Drawing.Point(3, 53);
            this.gxListAccout1.Name = "gxListAccout1";
            this.gxListAccout1.QueryString = "";
            this.gxListAccout1.RecordNavigator = true;
            this.gxListAccout1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxListAccout1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListAccout1.Size = new System.Drawing.Size(995, 278);
            this.gxListAccout1.TabIndex = 0;
            this.gxListAccout1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxListAccout1.WhereSQL = "";
            // 
            // gxGroupBox3
            // 
            this.gxGroupBox3.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.gxGroupBox3.Controls.Add(this.gxAddEdit);
            this.gxGroupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox3.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.gxGroupBox3.Location = new System.Drawing.Point(3, 16);
            this.gxGroupBox3.Name = "gxGroupBox3";
            this.gxGroupBox3.Size = new System.Drawing.Size(995, 37);
            this.gxGroupBox3.TabIndex = 0;
            // 
            // gxAddEdit
            // 
            this.gxAddEdit.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit.GridData = null;
            this.gxAddEdit.Location = new System.Drawing.Point(9, 6);
            this.gxAddEdit.Name = "gxAddEdit";
            this.gxAddEdit.Size = new System.Drawing.Size(391, 28);
            this.gxAddEdit.TabIndex = 1;
            this.gxAddEdit.ToolTipAdd = "Thêm";
            this.gxAddEdit.ToolTipButton1 = "";
            this.gxAddEdit.ToolTipButton2 = "";
            this.gxAddEdit.ToolTipDelete = "Loại bỏ khỏi danh sách trên lưới";
            this.gxAddEdit.ToolTipEdit = "Sửa";
            this.gxAddEdit.ToolTipFind = "";
            this.gxAddEdit.ToolTipPrint = "In danh sách trên lưới";
            this.gxAddEdit.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
    "ếm";
            this.gxAddEdit.ToolTipSelect = "Chọn";
            // 
            // frmAccoutList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1001, 532);
            this.Controls.Add(this.gxGroupBox2);
            this.Controls.Add(this.grpInfor);
            this.Name = "frmAccoutList";
            this.Text = "Quản lý tài khoản";
            this.Load += new System.EventHandler(this.frmAccoutList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grpInfor)).EndInit();
            this.grpInfor.ResumeLayout(false);
            this.grpInfor.PerformLayout();
            this.pnQuestion.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).EndInit();
            this.gxGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxListAccout1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox3)).EndInit();
            this.gxGroupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox grpInfor;
        private GxControl.GxGroupBox gxGroupBox2;
        private GxControl.GxGroupBox gxGroupBox3;
        private GxControl.GxAddEdit gxAddEdit;
        private GxControl.GxListAccout gxListAccout1;
        private GxControl.GxTextField txtPhone;
        private GxControl.GxComboField cbPhanQuyen;
        private GxControl.GxTextField txtEmail;
        private GxControl.GxTextField txtCofirmPassword;
        private GxControl.GxTextField txtPassword;
        private GxControl.GxTextField txtUserName;
        private GxControl.GxTextField txtFullName;
        private System.Windows.Forms.Panel pnQuestion;
        private GxControl.GxTextField txtAnswer;
        private GxControl.GxTextField txtQuestion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbRcomPass;
    }
}