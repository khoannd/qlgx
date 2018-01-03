namespace GiaoXu
{
    partial class frmBieuDo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBieuDo));
            this.dtDateTo = new GxControl.GxDateField();
            this.dtDateFrom = new GxControl.GxDateField();
            this.rdTongGiaoDan = new GxControl.GxRadioBox();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.chkLuuTru = new GxControl.GxCheckBox();
            this.rdBiTich = new GxControl.GxRadioBox();
            this.gxGroupBox2 = new GxControl.GxGroupBox();
            this.chkNullAccept = new GxControl.GxCheckBox();
            this.rdGiaoHo = new GxControl.GxRadioBox();
            this.rdDoTuoi = new GxControl.GxRadioBox();
            this.gxCommand1 = new GxControl.GxCommand();
            this.rdHonPhoi = new GxControl.GxRadioBox();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).BeginInit();
            this.gxGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dtDateTo
            // 
            this.dtDateTo.AutoUpperFirstChar = false;
            this.dtDateTo.BoxLeft = 60;
            this.dtDateTo.EditEnabled = true;
            this.dtDateTo.FullInputRequired = false;
            this.dtDateTo.IsNullDate = true;
            this.dtDateTo.Label = "Đến ngày";
            this.dtDateTo.Location = new System.Drawing.Point(199, 20);
            this.dtDateTo.Name = "dtDateTo";
            this.dtDateTo.Size = new System.Drawing.Size(182, 26);
            this.dtDateTo.TabIndex = 3;
            this.dtDateTo.Value = ((object)(resources.GetObject("dtDateTo.Value")));
            // 
            // dtDateFrom
            // 
            this.dtDateFrom.AutoUpperFirstChar = false;
            this.dtDateFrom.BoxLeft = 60;
            this.dtDateFrom.EditEnabled = true;
            this.dtDateFrom.FullInputRequired = false;
            this.dtDateFrom.IsNullDate = true;
            this.dtDateFrom.Label = "Từ ngày";
            this.dtDateFrom.Location = new System.Drawing.Point(19, 19);
            this.dtDateFrom.Name = "dtDateFrom";
            this.dtDateFrom.Size = new System.Drawing.Size(178, 26);
            this.dtDateFrom.TabIndex = 2;
            this.dtDateFrom.Value = ((object)(resources.GetObject("dtDateFrom.Value")));
            // 
            // rdTongGiaoDan
            // 
            this.rdTongGiaoDan.AutoSize = true;
            this.rdTongGiaoDan.Location = new System.Drawing.Point(19, 15);
            this.rdTongGiaoDan.Name = "rdTongGiaoDan";
            this.rdTongGiaoDan.Size = new System.Drawing.Size(84, 15);
            this.rdTongGiaoDan.TabIndex = 5;
            this.rdTongGiaoDan.TabStop = true;
            this.rdTongGiaoDan.Text = "Tổng giáo dân";
            this.rdTongGiaoDan.UseVisualStyleBackColor = true;
            this.rdTongGiaoDan.CheckedChanged += new System.EventHandler(this.rdTongGiaoDan_CheckedChanged);
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.chkLuuTru);
            this.gxGroupBox1.Controls.Add(this.dtDateFrom);
            this.gxGroupBox1.Controls.Add(this.dtDateTo);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(397, 71);
            this.gxGroupBox1.TabIndex = 6;
            this.gxGroupBox1.Text = "Thời gian thống kê";
            // 
            // chkLuuTru
            // 
            this.chkLuuTru.AutoSize = true;
            this.chkLuuTru.Location = new System.Drawing.Point(81, 49);
            this.chkLuuTru.Name = "chkLuuTru";
            this.chkLuuTru.Size = new System.Drawing.Size(144, 15);
            this.chkLuuTru.TabIndex = 12;
            this.chkLuuTru.Text = "Tính cả trong hồ sơ lưu trữ";
            this.chkLuuTru.UseVisualStyleBackColor = true;
            this.chkLuuTru.Visible = false;
            // 
            // rdBiTich
            // 
            this.rdBiTich.AutoSize = true;
            this.rdBiTich.Location = new System.Drawing.Point(19, 57);
            this.rdBiTich.Name = "rdBiTich";
            this.rdBiTich.Size = new System.Drawing.Size(101, 15);
            this.rdBiTich.TabIndex = 5;
            this.rdBiTich.TabStop = true;
            this.rdBiTich.Text = "Tình hình bí tích";
            this.rdBiTich.UseVisualStyleBackColor = true;
            // 
            // gxGroupBox2
            // 
            this.gxGroupBox2.Controls.Add(this.chkNullAccept);
            this.gxGroupBox2.Controls.Add(this.rdGiaoHo);
            this.gxGroupBox2.Controls.Add(this.rdDoTuoi);
            this.gxGroupBox2.Controls.Add(this.rdTongGiaoDan);
            this.gxGroupBox2.Controls.Add(this.rdHonPhoi);
            this.gxGroupBox2.Controls.Add(this.rdBiTich);
            this.gxGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox2.Location = new System.Drawing.Point(0, 71);
            this.gxGroupBox2.Name = "gxGroupBox2";
            this.gxGroupBox2.Size = new System.Drawing.Size(397, 150);
            this.gxGroupBox2.TabIndex = 7;
            this.gxGroupBox2.Text = "Loại biểu đồ";
            // 
            // chkNullAccept
            // 
            this.chkNullAccept.AutoSize = true;
            this.chkNullAccept.Location = new System.Drawing.Point(19, 129);
            this.chkNullAccept.Name = "chkNullAccept";
            this.chkNullAccept.Size = new System.Drawing.Size(218, 15);
            this.chkNullAccept.TabIndex = 11;
            this.chkNullAccept.Text = "Tính cả những dữ liệu không có ngày tháng";
            this.chkNullAccept.UseVisualStyleBackColor = true;
            this.chkNullAccept.Visible = false;
            // 
            // rdGiaoHo
            // 
            this.rdGiaoHo.AutoSize = true;
            this.rdGiaoHo.Location = new System.Drawing.Point(19, 78);
            this.rdGiaoHo.Name = "rdGiaoHo";
            this.rdGiaoHo.Size = new System.Drawing.Size(245, 15);
            this.rdGiaoHo.TabIndex = 5;
            this.rdGiaoHo.TabStop = true;
            this.rdGiaoHo.Text = "So sánh Giáo họ (không phụ thuộc năm thống kê)";
            this.rdGiaoHo.UseVisualStyleBackColor = true;
            // 
            // rdDoTuoi
            // 
            this.rdDoTuoi.AutoSize = true;
            this.rdDoTuoi.Location = new System.Drawing.Point(19, 99);
            this.rdDoTuoi.Name = "rdDoTuoi";
            this.rdDoTuoi.Size = new System.Drawing.Size(241, 15);
            this.rdDoTuoi.TabIndex = 5;
            this.rdDoTuoi.TabStop = true;
            this.rdDoTuoi.Text = "So sánh độ tuổi (không phụ thuộc năm thống kê)";
            this.rdDoTuoi.UseVisualStyleBackColor = true;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 221);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(397, 45);
            this.gxCommand1.TabIndex = 8;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // rdHonPhoi
            // 
            this.rdHonPhoi.AutoSize = true;
            this.rdHonPhoi.Location = new System.Drawing.Point(19, 36);
            this.rdHonPhoi.Name = "rdHonPhoi";
            this.rdHonPhoi.Size = new System.Drawing.Size(62, 15);
            this.rdHonPhoi.TabIndex = 5;
            this.rdHonPhoi.TabStop = true;
            this.rdHonPhoi.Text = "Hôn phối";
            this.rdHonPhoi.UseVisualStyleBackColor = true;
            // 
            // frmBieuDo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 266);
            this.Controls.Add(this.gxGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.gxGroupBox1);
            this.Name = "frmBieuDo";
            this.Text = "Xem biểu đồ thống kê";
            this.Load += new System.EventHandler(this.frmBieuDo_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.gxGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).EndInit();
            this.gxGroupBox2.ResumeLayout(false);
            this.gxGroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxDateField dtDateTo;
        private GxControl.GxDateField dtDateFrom;
        private GxControl.GxRadioBox rdTongGiaoDan;
        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxGroupBox gxGroupBox2;
        private GxControl.GxRadioBox rdGiaoHo;
        private GxControl.GxRadioBox rdDoTuoi;
        private GxControl.GxRadioBox rdBiTich;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxCheckBox chkLuuTru;
        private GxControl.GxCheckBox chkNullAccept;
        private GxControl.GxRadioBox rdHonPhoi;
    }
}