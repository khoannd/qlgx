namespace GiaoXu
{
    partial class frmYeuCauFileBackup
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
            this.txtConfirmEmail = new GxControl.GxTextField();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGhiChu = new System.Windows.Forms.RichTextBox();
            this.cbTenMayNhap = new GxControl.GxComboField();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtConfirmEmail
            // 
            this.txtConfirmEmail.AutoCompleteEnabled = true;
            this.txtConfirmEmail.AutoUpperFirstChar = false;
            this.txtConfirmEmail.BoxLeft = 80;
            this.txtConfirmEmail.EditEnabled = true;
            this.txtConfirmEmail.Label = "Email xác nhận";
            this.txtConfirmEmail.Location = new System.Drawing.Point(6, 20);
            this.txtConfirmEmail.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtConfirmEmail.MaxLength = 255;
            this.txtConfirmEmail.MultiLine = false;
            this.txtConfirmEmail.Name = "txtConfirmEmail";
            this.txtConfirmEmail.NumberInputRequired = true;
            this.txtConfirmEmail.NumberMode = false;
            this.txtConfirmEmail.ReadOnly = false;
            this.txtConfirmEmail.Size = new System.Drawing.Size(376, 25);
            this.txtConfirmEmail.TabIndex = 2;
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.cbTenMayNhap);
            this.gxGroupBox1.Controls.Add(this.label1);
            this.gxGroupBox1.Controls.Add(this.txtGhiChu);
            this.gxGroupBox1.Controls.Add(this.txtConfirmEmail);
            this.gxGroupBox1.Location = new System.Drawing.Point(12, 12);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(387, 251);
            this.gxGroupBox1.TabIndex = 1;
            this.gxGroupBox1.Text = "Thông tin yêu cầu";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Ghi chú";
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhiChu.HideSelection = false;
            this.txtGhiChu.Location = new System.Drawing.Point(88, 92);
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.Size = new System.Drawing.Size(293, 147);
            this.txtGhiChu.TabIndex = 4;
            this.txtGhiChu.Text = "";
            // 
            // cbTenMayNhap
            // 
            this.cbTenMayNhap.AutoUpperFirstChar = false;
            this.cbTenMayNhap.BoxLeft = 0;
            this.cbTenMayNhap.EditEnabled = true;
            this.cbTenMayNhap.Label = "Tên máy nhập ";
            this.cbTenMayNhap.Location = new System.Drawing.Point(6, 54);
            this.cbTenMayNhap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbTenMayNhap.MaxLength = 0;
            this.cbTenMayNhap.Name = "cbTenMayNhap";
            this.cbTenMayNhap.SelectedIndex = -1;
            this.cbTenMayNhap.SelectedText = "";
            this.cbTenMayNhap.SelectedValue = null;
            this.cbTenMayNhap.Size = new System.Drawing.Size(375, 26);
            this.cbTenMayNhap.TabIndex = 3;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 263);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(401, 48);
            this.gxCommand1.TabIndex = 5;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Quay về";
            this.gxCommand1.ToolTipOK = "Gửi yêu cầu";
            // 
            // frmYeuCauFileBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 311);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.gxGroupBox1);
            this.Name = "frmYeuCauFileBackup";
            this.Text = "Yêu cầu tập tin";
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.gxGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxTextField txtConfirmEmail;
        private GxControl.GxGroupBox gxGroupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox txtGhiChu;
        private GxControl.GxComboField cbTenMayNhap;
        private GxControl.GxCommand gxCommand1;
    }
}