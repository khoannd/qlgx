namespace GxControl
{
    partial class frmReportGioiThieuHP
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReportGioiThieuHP));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.txtTenThanh = new GxControl.GxTextField();
            this.txtNoiHocGiaoLy = new GxControl.GxTextField();
            this.dtNgaySinh2 = new GxControl.GxDateField();
            this.cbChaGui = new GxControl.GxComboField();
            this.txtNguoi1 = new GxControl.GxTextField();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGiaoXu2 = new GxControl.GxTextField();
            this.txtGiaoPhan2 = new GxControl.GxTextField();
            this.txtTenMe2 = new GxControl.GxTextField();
            this.txtHoten = new GxControl.GxTextField();
            this.txtTenCha2 = new GxControl.GxTextField();
            this.txtGiaoXuNhan = new GxControl.GxTextField();
            this.txtChaNhan = new GxControl.GxTextField();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.txtTenThanh);
            this.uiGroupBox1.Controls.Add(this.txtNoiHocGiaoLy);
            this.uiGroupBox1.Controls.Add(this.dtNgaySinh2);
            this.uiGroupBox1.Controls.Add(this.cbChaGui);
            this.uiGroupBox1.Controls.Add(this.txtNguoi1);
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXu2);
            this.uiGroupBox1.Controls.Add(this.txtGiaoPhan2);
            this.uiGroupBox1.Controls.Add(this.txtTenMe2);
            this.uiGroupBox1.Controls.Add(this.txtHoten);
            this.uiGroupBox1.Controls.Add(this.txtTenCha2);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXuNhan);
            this.uiGroupBox1.Controls.Add(this.txtChaNhan);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(577, 441);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin giới thiệu";
            this.uiGroupBox1.Click += new System.EventHandler(this.uiGroupBox1_Click);
            // 
            // txtTenThanh
            // 
            this.txtTenThanh.AutoCompleteEnabled = true;
            this.txtTenThanh.AutoUpperFirstChar = true;
            this.txtTenThanh.BoxLeft = 90;
            this.txtTenThanh.EditEnabled = true;
            this.txtTenThanh.Label = "Tên thánh";
            this.txtTenThanh.Location = new System.Drawing.Point(0, 138);
            this.txtTenThanh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenThanh.MaxLength = 255;
            this.txtTenThanh.MultiLine = false;
            this.txtTenThanh.Name = "txtTenThanh";
            this.txtTenThanh.NumberInputRequired = true;
            this.txtTenThanh.NumberMode = false;
            this.txtTenThanh.ReadOnly = false;
            this.txtTenThanh.Size = new System.Drawing.Size(249, 25);
            this.txtTenThanh.TabIndex = 3;
            this.txtTenThanh.Load += new System.EventHandler(this.gxTextField1_Load);
            // 
            // txtNoiHocGiaoLy
            // 
            this.txtNoiHocGiaoLy.AutoCompleteEnabled = true;
            this.txtNoiHocGiaoLy.AutoUpperFirstChar = true;
            this.txtNoiHocGiaoLy.BoxLeft = 90;
            this.txtNoiHocGiaoLy.EditEnabled = true;
            this.txtNoiHocGiaoLy.Label = "Nơi học GLHN:";
            this.txtNoiHocGiaoLy.Location = new System.Drawing.Point(0, 331);
            this.txtNoiHocGiaoLy.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNoiHocGiaoLy.MaxLength = 255;
            this.txtNoiHocGiaoLy.MultiLine = false;
            this.txtNoiHocGiaoLy.Name = "txtNoiHocGiaoLy";
            this.txtNoiHocGiaoLy.NumberInputRequired = true;
            this.txtNoiHocGiaoLy.NumberMode = false;
            this.txtNoiHocGiaoLy.ReadOnly = false;
            this.txtNoiHocGiaoLy.Size = new System.Drawing.Size(495, 25);
            this.txtNoiHocGiaoLy.TabIndex = 10;
            this.txtNoiHocGiaoLy.Load += new System.EventHandler(this.txtNoiHocGiaoLy_Load);
            // 
            // dtNgaySinh2
            // 
            this.dtNgaySinh2.AutoUpperFirstChar = false;
            this.dtNgaySinh2.BoxLeft = 90;
            this.dtNgaySinh2.EditEnabled = true;
            this.dtNgaySinh2.FullInputRequired = false;
            this.dtNgaySinh2.IsNullDate = true;
            this.dtNgaySinh2.Label = "Ngày sinh";
            this.dtNgaySinh2.Location = new System.Drawing.Point(0, 171);
            this.dtNgaySinh2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgaySinh2.Name = "dtNgaySinh2";
            this.dtNgaySinh2.Size = new System.Drawing.Size(266, 26);
            this.dtNgaySinh2.TabIndex = 5;
            this.dtNgaySinh2.Value = ((object)(resources.GetObject("dtNgaySinh2.Value")));
            this.dtNgaySinh2.Load += new System.EventHandler(this.dtNgaySinh2_Load);
            // 
            // cbChaGui
            // 
            this.cbChaGui.AutoUpperFirstChar = true;
            this.cbChaGui.BoxLeft = 90;
            this.cbChaGui.EditEnabled = true;
            this.cbChaGui.Label = "Linh mục chứng:";
            this.cbChaGui.Location = new System.Drawing.Point(0, 362);
            this.cbChaGui.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbChaGui.MaxLength = 255;
            this.cbChaGui.Name = "cbChaGui";
            this.cbChaGui.SelectedIndex = -1;
            this.cbChaGui.SelectedText = "";
            this.cbChaGui.SelectedValue = null;
            this.cbChaGui.Size = new System.Drawing.Size(495, 26);
            this.cbChaGui.TabIndex = 11;
            this.cbChaGui.Load += new System.EventHandler(this.cbChaGui_Load);
            // 
            // txtNguoi1
            // 
            this.txtNguoi1.AutoCompleteEnabled = true;
            this.txtNguoi1.AutoUpperFirstChar = false;
            this.txtNguoi1.BoxLeft = 90;
            this.txtNguoi1.EditEnabled = true;
            this.txtNguoi1.Label = "Giới thiệu cho:";
            this.txtNguoi1.Location = new System.Drawing.Point(6, 81);
            this.txtNguoi1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNguoi1.MaxLength = 32767;
            this.txtNguoi1.MultiLine = false;
            this.txtNguoi1.Name = "txtNguoi1";
            this.txtNguoi1.NumberInputRequired = true;
            this.txtNguoi1.NumberMode = false;
            this.txtNguoi1.ReadOnly = true;
            this.txtNguoi1.Size = new System.Drawing.Size(495, 25);
            this.txtNguoi1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Muốn kết hôn theo phép đạo với anh(chị) :";
            // 
            // txtGiaoXu2
            // 
            this.txtGiaoXu2.AutoCompleteEnabled = true;
            this.txtGiaoXu2.AutoUpperFirstChar = true;
            this.txtGiaoXu2.BoxLeft = 90;
            this.txtGiaoXu2.EditEnabled = true;
            this.txtGiaoXu2.Label = "Giáo xứ:";
            this.txtGiaoXu2.Location = new System.Drawing.Point(0, 268);
            this.txtGiaoXu2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXu2.MaxLength = 255;
            this.txtGiaoXu2.MultiLine = false;
            this.txtGiaoXu2.Name = "txtGiaoXu2";
            this.txtGiaoXu2.NumberInputRequired = true;
            this.txtGiaoXu2.NumberMode = false;
            this.txtGiaoXu2.ReadOnly = false;
            this.txtGiaoXu2.Size = new System.Drawing.Size(495, 25);
            this.txtGiaoXu2.TabIndex = 8;
            this.txtGiaoXu2.Load += new System.EventHandler(this.txtGiaoXu2_Load);
            // 
            // txtGiaoPhan2
            // 
            this.txtGiaoPhan2.AutoCompleteEnabled = true;
            this.txtGiaoPhan2.AutoUpperFirstChar = true;
            this.txtGiaoPhan2.BoxLeft = 90;
            this.txtGiaoPhan2.EditEnabled = true;
            this.txtGiaoPhan2.Label = "Giáo phận:";
            this.txtGiaoPhan2.Location = new System.Drawing.Point(0, 299);
            this.txtGiaoPhan2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhan2.MaxLength = 255;
            this.txtGiaoPhan2.MultiLine = false;
            this.txtGiaoPhan2.Name = "txtGiaoPhan2";
            this.txtGiaoPhan2.NumberInputRequired = true;
            this.txtGiaoPhan2.NumberMode = false;
            this.txtGiaoPhan2.ReadOnly = false;
            this.txtGiaoPhan2.Size = new System.Drawing.Size(495, 25);
            this.txtGiaoPhan2.TabIndex = 9;
            this.txtGiaoPhan2.Load += new System.EventHandler(this.txtGiaoPhan2_Load);
            // 
            // txtTenMe2
            // 
            this.txtTenMe2.AutoCompleteEnabled = true;
            this.txtTenMe2.AutoUpperFirstChar = true;
            this.txtTenMe2.BoxLeft = 90;
            this.txtTenMe2.EditEnabled = true;
            this.txtTenMe2.Label = "và bà:";
            this.txtTenMe2.Location = new System.Drawing.Point(0, 236);
            this.txtTenMe2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenMe2.MaxLength = 255;
            this.txtTenMe2.MultiLine = false;
            this.txtTenMe2.Name = "txtTenMe2";
            this.txtTenMe2.NumberInputRequired = true;
            this.txtTenMe2.NumberMode = false;
            this.txtTenMe2.ReadOnly = false;
            this.txtTenMe2.Size = new System.Drawing.Size(495, 25);
            this.txtTenMe2.TabIndex = 7;
            this.txtTenMe2.Load += new System.EventHandler(this.txtTenMe2_Load);
            // 
            // txtHoten
            // 
            this.txtHoten.AutoCompleteEnabled = true;
            this.txtHoten.AutoUpperFirstChar = true;
            this.txtHoten.BoxLeft = 90;
            this.txtHoten.EditEnabled = true;
            this.txtHoten.Label = "Họ tên";
            this.txtHoten.Location = new System.Drawing.Point(243, 138);
            this.txtHoten.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtHoten.MaxLength = 255;
            this.txtHoten.MultiLine = false;
            this.txtHoten.Name = "txtHoten";
            this.txtHoten.NumberInputRequired = true;
            this.txtHoten.NumberMode = false;
            this.txtHoten.ReadOnly = false;
            this.txtHoten.Size = new System.Drawing.Size(252, 25);
            this.txtHoten.TabIndex = 4;
            this.txtHoten.Load += new System.EventHandler(this.txtNguoi2_Load);
            // 
            // txtTenCha2
            // 
            this.txtTenCha2.AutoCompleteEnabled = true;
            this.txtTenCha2.AutoUpperFirstChar = true;
            this.txtTenCha2.BoxLeft = 90;
            this.txtTenCha2.EditEnabled = true;
            this.txtTenCha2.Label = "Con ông:";
            this.txtTenCha2.Location = new System.Drawing.Point(0, 205);
            this.txtTenCha2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenCha2.MaxLength = 255;
            this.txtTenCha2.MultiLine = false;
            this.txtTenCha2.Name = "txtTenCha2";
            this.txtTenCha2.NumberInputRequired = true;
            this.txtTenCha2.NumberMode = false;
            this.txtTenCha2.ReadOnly = false;
            this.txtTenCha2.Size = new System.Drawing.Size(495, 25);
            this.txtTenCha2.TabIndex = 6;
            this.txtTenCha2.Load += new System.EventHandler(this.txtTenCha2_Load);
            // 
            // txtGiaoXuNhan
            // 
            this.txtGiaoXuNhan.AutoCompleteEnabled = true;
            this.txtGiaoXuNhan.AutoUpperFirstChar = true;
            this.txtGiaoXuNhan.BoxLeft = 90;
            this.txtGiaoXuNhan.EditEnabled = true;
            this.txtGiaoXuNhan.Label = "Giáo phận:";
            this.txtGiaoXuNhan.Location = new System.Drawing.Point(6, 48);
            this.txtGiaoXuNhan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXuNhan.MaxLength = 255;
            this.txtGiaoXuNhan.MultiLine = false;
            this.txtGiaoXuNhan.Name = "txtGiaoXuNhan";
            this.txtGiaoXuNhan.NumberInputRequired = true;
            this.txtGiaoXuNhan.NumberMode = false;
            this.txtGiaoXuNhan.ReadOnly = false;
            this.txtGiaoXuNhan.Size = new System.Drawing.Size(495, 25);
            this.txtGiaoXuNhan.TabIndex = 1;
            // 
            // txtChaNhan
            // 
            this.txtChaNhan.AutoCompleteEnabled = true;
            this.txtChaNhan.AutoUpperFirstChar = true;
            this.txtChaNhan.BoxLeft = 90;
            this.txtChaNhan.EditEnabled = true;
            this.txtChaNhan.Label = "Kính gửi cha xứ:";
            this.txtChaNhan.Location = new System.Drawing.Point(6, 17);
            this.txtChaNhan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtChaNhan.MaxLength = 255;
            this.txtChaNhan.MultiLine = false;
            this.txtChaNhan.Name = "txtChaNhan";
            this.txtChaNhan.NumberInputRequired = true;
            this.txtChaNhan.NumberMode = false;
            this.txtChaNhan.ReadOnly = false;
            this.txtChaNhan.Size = new System.Drawing.Size(495, 25);
            this.txtChaNhan.TabIndex = 0;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 396);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(577, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // frmReportGioiThieuHP
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(577, 441);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmReportGioiThieuHP";
            this.Text = "Xuat gioi thieu hon phoi";
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxTextField txtGiaoXuNhan;
        private GxTextField txtChaNhan;
        private System.Windows.Forms.Label label1;
        private GxCommand gxCommand1;
        private GxComboField cbChaGui;
        private GxTextField txtNguoi1;
        private GxDateField dtNgaySinh2;
        private GxTextField txtGiaoPhan2;
        private GxTextField txtTenMe2;
        private GxTextField txtTenCha2;
        private GxTextField txtHoten;
        private GxTextField txtGiaoXu2;
        private GxTextField txtNoiHocGiaoLy;
        private GxTextField txtTenThanh;
    }
}