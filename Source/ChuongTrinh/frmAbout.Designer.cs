namespace GiaoXu
{
    partial class frmAbout
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblTenChuongTrinh = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblEmail2 = new System.Windows.Forms.LinkLabel();
            this.lblWebsite = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.lblThongTin = new System.Windows.Forms.Label();
            this.gxCommand1 = new GxControl.GxCommand();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblVersion);
            this.panel1.Controls.Add(this.lblTenChuongTrinh);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(343, 56);
            this.panel1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Image = global::GiaoXu.Properties.Resources.church_icon;
            this.label3.Location = new System.Drawing.Point(11, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 45);
            this.label3.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.label2.Location = new System.Drawing.Point(-3, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(346, 1);
            this.label2.TabIndex = 0;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(64, 32);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(94, 13);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "Phiên bản: 1.0.0.0";
            // 
            // lblTenChuongTrinh
            // 
            this.lblTenChuongTrinh.AutoSize = true;
            this.lblTenChuongTrinh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTenChuongTrinh.Location = new System.Drawing.Point(62, 13);
            this.lblTenChuongTrinh.Name = "lblTenChuongTrinh";
            this.lblTenChuongTrinh.Size = new System.Drawing.Size(214, 13);
            this.lblTenChuongTrinh.TabIndex = 0;
            this.lblTenChuongTrinh.Text = "QLGX - Chương trình quản lý giáo xứ";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.lblEmail2);
            this.panel2.Controls.Add(this.lblWebsite);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.lblThongTin);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 56);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(343, 126);
            this.panel2.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(14, 69);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(326, 54);
            this.label8.TabIndex = 0;
            this.label8.Text = "Rất mong nhận được ý kiến đóng góp của mọi người để chương trình ngày càng hoàn t" +
                "hiện hơn. Mọi ý kiến đóng góp và thông báo lỗi, xin vui lòng liên hệ tác giả. Ch" +
                "ân thành cảm ơn!";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Website";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Email:";
            // 
            // lblEmail2
            // 
            this.lblEmail2.AutoSize = true;
            this.lblEmail2.Location = new System.Drawing.Point(64, 29);
            this.lblEmail2.Name = "lblEmail2";
            this.lblEmail2.Size = new System.Drawing.Size(131, 13);
            this.lblEmail2.TabIndex = 2;
            this.lblEmail2.TabStop = true;
            this.lblEmail2.Text = "admin@luudiachiweb.com";
            this.lblEmail2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblEmail2_LinkClicked);
            // 
            // lblWebsite
            // 
            this.lblWebsite.AutoSize = true;
            this.lblWebsite.Location = new System.Drawing.Point(64, 46);
            this.lblWebsite.Name = "lblWebsite";
            this.lblWebsite.Size = new System.Drawing.Size(150, 13);
            this.lblWebsite.TabIndex = 1;
            this.lblWebsite.TabStop = true;
            this.lblWebsite.Text = "http://www.luudiachiweb.com";
            this.lblWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblWebsite_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(64, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Nguyễn Đức Khoan";
            // 
            // lblThongTin
            // 
            this.lblThongTin.AutoSize = true;
            this.lblThongTin.Location = new System.Drawing.Point(12, 12);
            this.lblThongTin.Name = "lblThongTin";
            this.lblThongTin.Size = new System.Drawing.Size(46, 13);
            this.lblThongTin.TabIndex = 0;
            this.lblThongTin.Text = "Tác giả:";
            // 
            // gxCommand1
            // 
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 182);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = true;
            this.gxCommand1.Size = new System.Drawing.Size(343, 46);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            // 
            // frmAbout
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(343, 228);
            this.ControlBox = false;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.Text = "Thông tin phần mềm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private GxControl.GxCommand gxCommand1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblTenChuongTrinh;
        private System.Windows.Forms.LinkLabel lblEmail2;
        private System.Windows.Forms.LinkLabel lblWebsite;
        private System.Windows.Forms.Label lblThongTin;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label2;
    }
}