
namespace GxControl
{
    partial class frmReportCaoPho
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReportCaoPho));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtSDTNhaHieu = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dtTLAnTang = new GxControl.GxDateField();
            this.txtGioTLeAnTang = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPhutTLeAnTang = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtGioTLeTaiGia = new System.Windows.Forms.TextBox();
            this.dtTLTaiGia = new GxControl.GxDateField();
            this.label7 = new System.Windows.Forms.Label();
            this.txtPhutTLeTaiGia = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtPhutTanLiem = new System.Windows.Forms.TextBox();
            this.dtNhapQuan = new GxControl.GxDateField();
            this.txtGioTanLiem = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtGioQuaDoi = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPhutQuaDoi = new System.Windows.Forms.TextBox();
            this.dtNgayQuaDoi = new GxControl.GxDateField();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtNoiAnTang = new System.Windows.Forms.TextBox();
            this.lblNotify = new System.Windows.Forms.Label();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.label10);
            this.uiGroupBox1.Controls.Add(this.txtSDTNhaHieu);
            this.uiGroupBox1.Controls.Add(this.groupBox4);
            this.uiGroupBox1.Controls.Add(this.groupBox3);
            this.uiGroupBox1.Controls.Add(this.groupBox2);
            this.uiGroupBox1.Controls.Add(this.groupBox1);
            this.uiGroupBox1.Controls.Add(this.label2);
            this.uiGroupBox1.Controls.Add(this.txtNoiAnTang);
            this.uiGroupBox1.Controls.Add(this.lblNotify);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(886, 428);
            this.uiGroupBox1.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(587, 145);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(99, 20);
            this.label10.TabIndex = 31;
            this.label10.Text = "Sđt nhà hiếu";
            // 
            // txtSDTNhaHieu
            // 
            this.txtSDTNhaHieu.Location = new System.Drawing.Point(655, 141);
            this.txtSDTNhaHieu.Multiline = true;
            this.txtSDTNhaHieu.Name = "txtSDTNhaHieu";
            this.txtSDTNhaHieu.Size = new System.Drawing.Size(189, 26);
            this.txtSDTNhaHieu.TabIndex = 30;
            this.txtSDTNhaHieu.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dtTLAnTang);
            this.groupBox4.Controls.Add(this.txtGioTLeAnTang);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Controls.Add(this.txtPhutTLeAnTang);
            this.groupBox4.Controls.Add(this.label8);
            this.groupBox4.Location = new System.Drawing.Point(53, 287);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(501, 82);
            this.groupBox4.TabIndex = 29;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Thời gian thánh lễ an táng";
            // 
            // dtTLAnTang
            // 
            this.dtTLAnTang.AutoUpperFirstChar = false;
            this.dtTLAnTang.BoxLeft = 90;
            this.dtTLAnTang.EditEnabled = true;
            this.dtTLAnTang.FullInputRequired = false;
            this.dtTLAnTang.IsNullDate = true;
            this.dtTLAnTang.Label = "Thánh lễ an táng";
            this.dtTLAnTang.Location = new System.Drawing.Point(9, 25);
            this.dtTLAnTang.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtTLAnTang.MaxDate = null;
            this.dtTLAnTang.MaxDateName = null;
            this.dtTLAnTang.MinDate = null;
            this.dtTLAnTang.MinDateName = null;
            this.dtTLAnTang.Name = "dtTLAnTang";
            this.dtTLAnTang.Size = new System.Drawing.Size(226, 26);
            this.dtTLAnTang.TabIndex = 10;
            this.dtTLAnTang.Value = ((object)(resources.GetObject("dtTLAnTang.Value")));
            // 
            // txtGioTLeAnTang
            // 
            this.txtGioTLeAnTang.Location = new System.Drawing.Point(281, 25);
            this.txtGioTLeAnTang.Name = "txtGioTLeAnTang";
            this.txtGioTLeAnTang.Size = new System.Drawing.Size(51, 26);
            this.txtGioTLeAnTang.TabIndex = 23;
            this.txtGioTLeAnTang.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtGioTLeAnTang.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtGioTLeAnTang_KeyPress);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(241, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(34, 20);
            this.label9.TabIndex = 24;
            this.label9.Text = "Giờ";
            // 
            // txtPhutTLeAnTang
            // 
            this.txtPhutTLeAnTang.Location = new System.Drawing.Point(433, 25);
            this.txtPhutTLeAnTang.Name = "txtPhutTLeAnTang";
            this.txtPhutTLeAnTang.Size = new System.Drawing.Size(42, 26);
            this.txtPhutTLeAnTang.TabIndex = 25;
            this.txtPhutTLeAnTang.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPhutTLeAnTang.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhutTLeAnTang_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(385, 31);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 20);
            this.label8.TabIndex = 26;
            this.label8.Text = "Phút";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtGioTLeTaiGia);
            this.groupBox3.Controls.Add(this.dtTLTaiGia);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.txtPhutTLeTaiGia);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Location = new System.Drawing.Point(53, 199);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(501, 82);
            this.groupBox3.TabIndex = 28;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Thời gian thánh lễ tại gia";
            // 
            // txtGioTLeTaiGia
            // 
            this.txtGioTLeTaiGia.Location = new System.Drawing.Point(281, 25);
            this.txtGioTLeTaiGia.Name = "txtGioTLeTaiGia";
            this.txtGioTLeTaiGia.Size = new System.Drawing.Size(51, 26);
            this.txtGioTLeTaiGia.TabIndex = 19;
            this.txtGioTLeTaiGia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtGioTLeTaiGia.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtGioTLeTaiGia_KeyPress);
            // 
            // dtTLTaiGia
            // 
            this.dtTLTaiGia.AutoUpperFirstChar = false;
            this.dtTLTaiGia.BoxLeft = 90;
            this.dtTLTaiGia.EditEnabled = true;
            this.dtTLTaiGia.FullInputRequired = false;
            this.dtTLTaiGia.IsNullDate = true;
            this.dtTLTaiGia.Label = "Thánh lễ tại gia";
            this.dtTLTaiGia.Location = new System.Drawing.Point(9, 25);
            this.dtTLTaiGia.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtTLTaiGia.MaxDate = null;
            this.dtTLTaiGia.MaxDateName = null;
            this.dtTLTaiGia.MinDate = null;
            this.dtTLTaiGia.MinDateName = null;
            this.dtTLTaiGia.Name = "dtTLTaiGia";
            this.dtTLTaiGia.Size = new System.Drawing.Size(226, 26);
            this.dtTLTaiGia.TabIndex = 8;
            this.dtTLTaiGia.Value = ((object)(resources.GetObject("dtTLTaiGia.Value")));
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(237, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(34, 20);
            this.label7.TabIndex = 20;
            this.label7.Text = "Giờ";
            // 
            // txtPhutTLeTaiGia
            // 
            this.txtPhutTLeTaiGia.Location = new System.Drawing.Point(429, 25);
            this.txtPhutTLeTaiGia.Name = "txtPhutTLeTaiGia";
            this.txtPhutTLeTaiGia.Size = new System.Drawing.Size(42, 26);
            this.txtPhutTLeTaiGia.TabIndex = 21;
            this.txtPhutTLeTaiGia.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPhutTLeTaiGia.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhutTLeTaiGia_KeyPress);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(381, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 20);
            this.label6.TabIndex = 22;
            this.label6.Text = "Phút";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtPhutTanLiem);
            this.groupBox2.Controls.Add(this.dtNhapQuan);
            this.groupBox2.Controls.Add(this.txtGioTanLiem);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(53, 116);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(501, 77);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Thời gian tẩn liệm";
            // 
            // txtPhutTanLiem
            // 
            this.txtPhutTanLiem.Location = new System.Drawing.Point(433, 25);
            this.txtPhutTanLiem.Name = "txtPhutTanLiem";
            this.txtPhutTanLiem.Size = new System.Drawing.Size(42, 26);
            this.txtPhutTanLiem.TabIndex = 17;
            this.txtPhutTanLiem.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPhutTanLiem.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhutTanLiem_KeyPress);
            // 
            // dtNhapQuan
            // 
            this.dtNhapQuan.AutoUpperFirstChar = false;
            this.dtNhapQuan.BoxLeft = 90;
            this.dtNhapQuan.EditEnabled = true;
            this.dtNhapQuan.FullInputRequired = false;
            this.dtNhapQuan.IsNullDate = true;
            this.dtNhapQuan.Label = "Ngày Tẩn liệm";
            this.dtNhapQuan.Location = new System.Drawing.Point(9, 23);
            this.dtNhapQuan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNhapQuan.MaxDate = null;
            this.dtNhapQuan.MaxDateName = null;
            this.dtNhapQuan.MinDate = null;
            this.dtNhapQuan.MinDateName = null;
            this.dtNhapQuan.Name = "dtNhapQuan";
            this.dtNhapQuan.Size = new System.Drawing.Size(226, 26);
            this.dtNhapQuan.TabIndex = 7;
            this.dtNhapQuan.Value = ((object)(resources.GetObject("dtNhapQuan.Value")));
            // 
            // txtGioTanLiem
            // 
            this.txtGioTanLiem.Location = new System.Drawing.Point(281, 25);
            this.txtGioTanLiem.Name = "txtGioTanLiem";
            this.txtGioTanLiem.Size = new System.Drawing.Size(51, 26);
            this.txtGioTanLiem.TabIndex = 15;
            this.txtGioTanLiem.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtGioTanLiem.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtGioTanLiem_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(241, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 20);
            this.label5.TabIndex = 16;
            this.label5.Text = "Giờ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(385, 31);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 20);
            this.label4.TabIndex = 18;
            this.label4.Text = "Phút";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtGioQuaDoi);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtPhutQuaDoi);
            this.groupBox1.Controls.Add(this.dtNgayQuaDoi);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(53, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(501, 82);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thời gian qua đời";
            // 
            // txtGioQuaDoi
            // 
            this.txtGioQuaDoi.Location = new System.Drawing.Point(281, 36);
            this.txtGioQuaDoi.Name = "txtGioQuaDoi";
            this.txtGioQuaDoi.Size = new System.Drawing.Size(51, 26);
            this.txtGioQuaDoi.TabIndex = 11;
            this.txtGioQuaDoi.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtGioQuaDoi.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtGioQuaDoi_KeyPress);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(385, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 20);
            this.label3.TabIndex = 14;
            this.label3.Text = "Phút";
            // 
            // txtPhutQuaDoi
            // 
            this.txtPhutQuaDoi.Location = new System.Drawing.Point(433, 36);
            this.txtPhutQuaDoi.Name = "txtPhutQuaDoi";
            this.txtPhutQuaDoi.Size = new System.Drawing.Size(42, 26);
            this.txtPhutQuaDoi.TabIndex = 13;
            this.txtPhutQuaDoi.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtPhutQuaDoi.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPhutQuaDoi_KeyPress);
            // 
            // dtNgayQuaDoi
            // 
            this.dtNgayQuaDoi.AutoUpperFirstChar = false;
            this.dtNgayQuaDoi.BoxLeft = 90;
            this.dtNgayQuaDoi.EditEnabled = true;
            this.dtNgayQuaDoi.FullInputRequired = false;
            this.dtNgayQuaDoi.IsNullDate = true;
            this.dtNgayQuaDoi.Label = "Ngày qua đời";
            this.dtNgayQuaDoi.Location = new System.Drawing.Point(9, 32);
            this.dtNgayQuaDoi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayQuaDoi.MaxDate = null;
            this.dtNgayQuaDoi.MaxDateName = null;
            this.dtNgayQuaDoi.MinDate = null;
            this.dtNgayQuaDoi.MinDateName = null;
            this.dtNgayQuaDoi.Name = "dtNgayQuaDoi";
            this.dtNgayQuaDoi.Size = new System.Drawing.Size(226, 26);
            this.dtNgayQuaDoi.TabIndex = 9;
            this.dtNgayQuaDoi.Value = ((object)(resources.GetObject("dtNgayQuaDoi.Value")));
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(241, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "Giờ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(587, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Nơi an táng";
            // 
            // txtNoiAnTang
            // 
            this.txtNoiAnTang.Location = new System.Drawing.Point(655, 70);
            this.txtNoiAnTang.Multiline = true;
            this.txtNoiAnTang.Name = "txtNoiAnTang";
            this.txtNoiAnTang.Size = new System.Drawing.Size(189, 41);
            this.txtNoiAnTang.TabIndex = 3;
            // 
            // lblNotify
            // 
            this.lblNotify.AutoSize = true;
            this.lblNotify.ForeColor = System.Drawing.Color.Red;
            this.lblNotify.Location = new System.Drawing.Point(58, 392);
            this.lblNotify.Name = "lblNotify";
            this.lblNotify.Size = new System.Drawing.Size(219, 20);
            this.lblNotify.TabIndex = 3;
            this.lblNotify.Text = "* Các thông tin bắt buộc nhập";
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 428);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(886, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // frmReportCaoPho
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 473);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmReportCaoPho";
            this.Text = "Xuất Cáo Phó";
            this.Load += new System.EventHandler(this.frmReportCaoPho_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private System.Windows.Forms.Label lblNotify;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNoiAnTang;
        private GxDateField dtNgayQuaDoi;
        private GxDateField dtTLTaiGia;
        private GxDateField dtNhapQuan;
        private GxDateField dtTLAnTang;
        private System.Windows.Forms.TextBox txtGioQuaDoi;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtPhutTLeAnTang;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtGioTLeAnTang;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPhutTLeTaiGia;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtGioTLeTaiGia;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPhutTanLiem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtGioTanLiem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPhutQuaDoi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSDTNhaHieu;
    }
}