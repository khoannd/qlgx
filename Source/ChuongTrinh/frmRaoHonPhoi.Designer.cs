namespace GiaoXu
{
    partial class frmRaoHonPhoi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReportRaoHP));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.dtNgayRao3 = new GxControl.GxDateField();
            this.dtNgayRao2 = new GxControl.GxDateField();
            this.dtNgayRao1 = new GxControl.GxDateField();
            this.txtNguoi2 = new GxControl.GxGiaoDan();
            this.txtNguoi1 = new GxControl.GxGiaoDan();
            this.cbChaGui = new GxControl.GxComboField();
            this.txtGiaoPhanTruoc2 = new GxControl.GxTextField();
            this.txtGiaoPhanTruoc1 = new GxControl.GxTextField();
            this.txtGhiChu = new GxControl.GxTextField();
            this.txtGiaoXuTruoc2 = new GxControl.GxTextField();
            this.txtGiaoPhan2 = new GxControl.GxTextField();
            this.txtGiaoXuTruoc1 = new GxControl.GxTextField();
            this.txtGiaoXu2 = new GxControl.GxTextField();
            this.txtGiaoPhan1 = new GxControl.GxTextField();
            this.txtGiaoXu1 = new GxControl.GxTextField();
            this.txtGiaoXuNhan = new GxControl.GxTextField();
            this.txtChaNhan = new GxControl.GxLinhMuc();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.dtNgayRao3);
            this.uiGroupBox1.Controls.Add(this.dtNgayRao2);
            this.uiGroupBox1.Controls.Add(this.dtNgayRao1);
            this.uiGroupBox1.Controls.Add(this.txtNguoi2);
            this.uiGroupBox1.Controls.Add(this.txtNguoi1);
            this.uiGroupBox1.Controls.Add(this.cbChaGui);
            this.uiGroupBox1.Controls.Add(this.txtGiaoPhanTruoc2);
            this.uiGroupBox1.Controls.Add(this.txtGiaoPhanTruoc1);
            this.uiGroupBox1.Controls.Add(this.txtGhiChu);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXuTruoc2);
            this.uiGroupBox1.Controls.Add(this.txtGiaoPhan2);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXuTruoc1);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXu2);
            this.uiGroupBox1.Controls.Add(this.txtGiaoPhan1);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXu1);
            this.uiGroupBox1.Controls.Add(this.txtGiaoXuNhan);
            this.uiGroupBox1.Controls.Add(this.txtChaNhan);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(547, 432);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin báo cáo";
            // 
            // dtNgayRao3
            // 
            this.dtNgayRao3.AutoUpperFirstChar = false;
            this.dtNgayRao3.BoxLeft = 55;
            this.dtNgayRao3.EditEnabled = true;
            this.dtNgayRao3.FullInputRequired = true;
            this.dtNgayRao3.IsNullDate = true;
            this.dtNgayRao3.Label = "rao lần 3:";
            this.dtNgayRao3.Location = new System.Drawing.Point(369, 299);
            this.dtNgayRao3.Name = "dtNgayRao3";
            this.dtNgayRao3.Size = new System.Drawing.Size(154, 26);
            this.dtNgayRao3.TabIndex = 15;
            this.dtNgayRao3.Value = ((object)(resources.GetObject("dtNgayRao3.Value")));
            // 
            // dtNgayRao2
            // 
            this.dtNgayRao2.AutoUpperFirstChar = false;
            this.dtNgayRao2.BoxLeft = 55;
            this.dtNgayRao2.EditEnabled = true;
            this.dtNgayRao2.FullInputRequired = true;
            this.dtNgayRao2.IsNullDate = true;
            this.dtNgayRao2.Label = "rao lần 2:";
            this.dtNgayRao2.Location = new System.Drawing.Point(206, 299);
            this.dtNgayRao2.Name = "dtNgayRao2";
            this.dtNgayRao2.Size = new System.Drawing.Size(154, 26);
            this.dtNgayRao2.TabIndex = 14;
            this.dtNgayRao2.Value = ((object)(resources.GetObject("dtNgayRao2.Value")));
            // 
            // dtNgayRao1
            // 
            this.dtNgayRao1.AutoUpperFirstChar = false;
            this.dtNgayRao1.BoxLeft = 90;
            this.dtNgayRao1.EditEnabled = true;
            this.dtNgayRao1.FullInputRequired = true;
            this.dtNgayRao1.IsNullDate = true;
            this.dtNgayRao1.Label = "Ngày rao lần 1:";
            this.dtNgayRao1.Location = new System.Drawing.Point(6, 299);
            this.dtNgayRao1.Name = "dtNgayRao1";
            this.dtNgayRao1.Size = new System.Drawing.Size(189, 26);
            this.dtNgayRao1.TabIndex = 13;
            this.dtNgayRao1.Value = ((object)(resources.GetObject("dtNgayRao1.Value")));
            // 
            // txtNguoi2
            // 
            this.txtNguoi2.AutoCompleteEnabled = true;
            this.txtNguoi2.AutoUpperFirstChar = true;
            this.txtNguoi2.BoxLeft = 90;
            this.txtNguoi2.CurrentRow = null;
            this.txtNguoi2.DisplayMode = GxGlobal.DisplayMode.Full;
            this.txtNguoi2.EditEnabled = true;
            this.txtNguoi2.Label = "Người thứ 2:";
            this.txtNguoi2.Location = new System.Drawing.Point(6, 175);
            this.txtNguoi2.MaGiaoDan = -1;
            this.txtNguoi2.MaGiaoHo = -1;
            this.txtNguoi2.MaxLength = 32767;
            this.txtNguoi2.MultiLine = false;
            this.txtNguoi2.Name = "txtNguoi2";
            this.txtNguoi2.NumberInputRequired = true;
            this.txtNguoi2.NumberMode = false;
            this.txtNguoi2.ReadOnly = true;
            this.txtNguoi2.Size = new System.Drawing.Size(517, 26);
            this.txtNguoi2.TabIndex = 7;
            this.txtNguoi2.WhereSQL = "";
            // 
            // txtNguoi1
            // 
            this.txtNguoi1.AutoCompleteEnabled = true;
            this.txtNguoi1.AutoUpperFirstChar = true;
            this.txtNguoi1.BoxLeft = 90;
            this.txtNguoi1.CurrentRow = null;
            this.txtNguoi1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.txtNguoi1.EditEnabled = true;
            this.txtNguoi1.Label = "Người thứ nhất:";
            this.txtNguoi1.Location = new System.Drawing.Point(6, 79);
            this.txtNguoi1.MaGiaoDan = -1;
            this.txtNguoi1.MaGiaoHo = -1;
            this.txtNguoi1.MaxLength = 32767;
            this.txtNguoi1.MultiLine = false;
            this.txtNguoi1.Name = "txtNguoi1";
            this.txtNguoi1.NumberInputRequired = true;
            this.txtNguoi1.NumberMode = false;
            this.txtNguoi1.ReadOnly = true;
            this.txtNguoi1.Size = new System.Drawing.Size(517, 27);
            this.txtNguoi1.TabIndex = 2;
            this.txtNguoi1.WhereSQL = "";
            // 
            // cbChaGui
            // 
            this.cbChaGui.AutoUpperFirstChar = true;
            this.cbChaGui.BoxLeft = 90;
            this.cbChaGui.EditEnabled = true;
            this.cbChaGui.Label = "Linh mục chứng:";
            this.cbChaGui.Location = new System.Drawing.Point(6, 267);
            this.cbChaGui.Name = "cbChaGui";
            this.cbChaGui.SelectedIndex = -1;
            this.cbChaGui.SelectedText = "";
            this.cbChaGui.SelectedValue = null;
            this.cbChaGui.Size = new System.Drawing.Size(517, 26);
            this.cbChaGui.TabIndex = 12;
            // 
            // txtGiaoPhanTruoc2
            // 
            this.txtGiaoPhanTruoc2.AutoCompleteEnabled = true;
            this.txtGiaoPhanTruoc2.AutoUpperFirstChar = true;
            this.txtGiaoPhanTruoc2.BoxLeft = 60;
            this.txtGiaoPhanTruoc2.EditEnabled = true;
            this.txtGiaoPhanTruoc2.Label = "Giáo phận:";
            this.txtGiaoPhanTruoc2.Location = new System.Drawing.Point(276, 236);
            this.txtGiaoPhanTruoc2.MaxLength = 32767;
            this.txtGiaoPhanTruoc2.MultiLine = false;
            this.txtGiaoPhanTruoc2.Name = "txtGiaoPhanTruoc2";
            this.txtGiaoPhanTruoc2.NumberInputRequired = true;
            this.txtGiaoPhanTruoc2.NumberMode = false;
            this.txtGiaoPhanTruoc2.ReadOnly = false;
            this.txtGiaoPhanTruoc2.Size = new System.Drawing.Size(247, 25);
            this.txtGiaoPhanTruoc2.TabIndex = 11;
            // 
            // txtGiaoPhanTruoc1
            // 
            this.txtGiaoPhanTruoc1.AutoCompleteEnabled = true;
            this.txtGiaoPhanTruoc1.AutoUpperFirstChar = true;
            this.txtGiaoPhanTruoc1.BoxLeft = 60;
            this.txtGiaoPhanTruoc1.EditEnabled = true;
            this.txtGiaoPhanTruoc1.Label = "Giáo phận:";
            this.txtGiaoPhanTruoc1.Location = new System.Drawing.Point(276, 140);
            this.txtGiaoPhanTruoc1.MaxLength = 32767;
            this.txtGiaoPhanTruoc1.MultiLine = false;
            this.txtGiaoPhanTruoc1.Name = "txtGiaoPhanTruoc1";
            this.txtGiaoPhanTruoc1.NumberInputRequired = true;
            this.txtGiaoPhanTruoc1.NumberMode = false;
            this.txtGiaoPhanTruoc1.ReadOnly = false;
            this.txtGiaoPhanTruoc1.Size = new System.Drawing.Size(247, 25);
            this.txtGiaoPhanTruoc1.TabIndex = 6;
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.AutoCompleteEnabled = true;
            this.txtGhiChu.AutoUpperFirstChar = true;
            this.txtGhiChu.BoxLeft = 90;
            this.txtGhiChu.EditEnabled = true;
            this.txtGhiChu.Label = "Ghi chú:";
            this.txtGhiChu.Location = new System.Drawing.Point(6, 337);
            this.txtGhiChu.MaxLength = 32767;
            this.txtGhiChu.MultiLine = true;
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.NumberInputRequired = true;
            this.txtGhiChu.NumberMode = false;
            this.txtGhiChu.ReadOnly = false;
            this.txtGhiChu.Size = new System.Drawing.Size(517, 44);
            this.txtGhiChu.TabIndex = 16;
            // 
            // txtGiaoXuTruoc2
            // 
            this.txtGiaoXuTruoc2.AutoCompleteEnabled = true;
            this.txtGiaoXuTruoc2.AutoUpperFirstChar = true;
            this.txtGiaoXuTruoc2.BoxLeft = 90;
            this.txtGiaoXuTruoc2.EditEnabled = true;
            this.txtGiaoXuTruoc2.Label = "Trước đã ở xứ:";
            this.txtGiaoXuTruoc2.Location = new System.Drawing.Point(6, 236);
            this.txtGiaoXuTruoc2.MaxLength = 32767;
            this.txtGiaoXuTruoc2.MultiLine = false;
            this.txtGiaoXuTruoc2.Name = "txtGiaoXuTruoc2";
            this.txtGiaoXuTruoc2.NumberInputRequired = true;
            this.txtGiaoXuTruoc2.NumberMode = false;
            this.txtGiaoXuTruoc2.ReadOnly = false;
            this.txtGiaoXuTruoc2.Size = new System.Drawing.Size(264, 25);
            this.txtGiaoXuTruoc2.TabIndex = 10;
            // 
            // txtGiaoPhan2
            // 
            this.txtGiaoPhan2.AutoCompleteEnabled = true;
            this.txtGiaoPhan2.AutoUpperFirstChar = true;
            this.txtGiaoPhan2.BoxLeft = 60;
            this.txtGiaoPhan2.EditEnabled = true;
            this.txtGiaoPhan2.Label = "Giáo phận:";
            this.txtGiaoPhan2.Location = new System.Drawing.Point(276, 205);
            this.txtGiaoPhan2.MaxLength = 32767;
            this.txtGiaoPhan2.MultiLine = false;
            this.txtGiaoPhan2.Name = "txtGiaoPhan2";
            this.txtGiaoPhan2.NumberInputRequired = true;
            this.txtGiaoPhan2.NumberMode = false;
            this.txtGiaoPhan2.ReadOnly = false;
            this.txtGiaoPhan2.Size = new System.Drawing.Size(247, 25);
            this.txtGiaoPhan2.TabIndex = 9;
            // 
            // txtGiaoXuTruoc1
            // 
            this.txtGiaoXuTruoc1.AutoCompleteEnabled = true;
            this.txtGiaoXuTruoc1.AutoUpperFirstChar = true;
            this.txtGiaoXuTruoc1.BoxLeft = 90;
            this.txtGiaoXuTruoc1.EditEnabled = true;
            this.txtGiaoXuTruoc1.Label = "Trước đã ở xứ:";
            this.txtGiaoXuTruoc1.Location = new System.Drawing.Point(6, 140);
            this.txtGiaoXuTruoc1.MaxLength = 32767;
            this.txtGiaoXuTruoc1.MultiLine = false;
            this.txtGiaoXuTruoc1.Name = "txtGiaoXuTruoc1";
            this.txtGiaoXuTruoc1.NumberInputRequired = true;
            this.txtGiaoXuTruoc1.NumberMode = false;
            this.txtGiaoXuTruoc1.ReadOnly = false;
            this.txtGiaoXuTruoc1.Size = new System.Drawing.Size(264, 25);
            this.txtGiaoXuTruoc1.TabIndex = 5;
            // 
            // txtGiaoXu2
            // 
            this.txtGiaoXu2.AutoCompleteEnabled = true;
            this.txtGiaoXu2.AutoUpperFirstChar = true;
            this.txtGiaoXu2.BoxLeft = 90;
            this.txtGiaoXu2.EditEnabled = true;
            this.txtGiaoXu2.Label = "Hiện ở giáo xứ:";
            this.txtGiaoXu2.Location = new System.Drawing.Point(6, 205);
            this.txtGiaoXu2.MaxLength = 32767;
            this.txtGiaoXu2.MultiLine = false;
            this.txtGiaoXu2.Name = "txtGiaoXu2";
            this.txtGiaoXu2.NumberInputRequired = true;
            this.txtGiaoXu2.NumberMode = false;
            this.txtGiaoXu2.ReadOnly = false;
            this.txtGiaoXu2.Size = new System.Drawing.Size(264, 25);
            this.txtGiaoXu2.TabIndex = 8;
            // 
            // txtGiaoPhan1
            // 
            this.txtGiaoPhan1.AutoCompleteEnabled = true;
            this.txtGiaoPhan1.AutoUpperFirstChar = true;
            this.txtGiaoPhan1.BoxLeft = 60;
            this.txtGiaoPhan1.EditEnabled = true;
            this.txtGiaoPhan1.Label = "Giáo phận:";
            this.txtGiaoPhan1.Location = new System.Drawing.Point(276, 109);
            this.txtGiaoPhan1.MaxLength = 32767;
            this.txtGiaoPhan1.MultiLine = false;
            this.txtGiaoPhan1.Name = "txtGiaoPhan1";
            this.txtGiaoPhan1.NumberInputRequired = true;
            this.txtGiaoPhan1.NumberMode = false;
            this.txtGiaoPhan1.ReadOnly = false;
            this.txtGiaoPhan1.Size = new System.Drawing.Size(247, 25);
            this.txtGiaoPhan1.TabIndex = 4;
            // 
            // txtGiaoXu1
            // 
            this.txtGiaoXu1.AutoCompleteEnabled = true;
            this.txtGiaoXu1.AutoUpperFirstChar = true;
            this.txtGiaoXu1.BoxLeft = 90;
            this.txtGiaoXu1.EditEnabled = true;
            this.txtGiaoXu1.Label = "Hiện ở giáo xứ:";
            this.txtGiaoXu1.Location = new System.Drawing.Point(6, 109);
            this.txtGiaoXu1.MaxLength = 32767;
            this.txtGiaoXu1.MultiLine = false;
            this.txtGiaoXu1.Name = "txtGiaoXu1";
            this.txtGiaoXu1.NumberInputRequired = true;
            this.txtGiaoXu1.NumberMode = false;
            this.txtGiaoXu1.ReadOnly = false;
            this.txtGiaoXu1.Size = new System.Drawing.Size(264, 25);
            this.txtGiaoXu1.TabIndex = 3;
            // 
            // txtGiaoXuNhan
            // 
            this.txtGiaoXuNhan.AutoCompleteEnabled = true;
            this.txtGiaoXuNhan.AutoUpperFirstChar = true;
            this.txtGiaoXuNhan.BoxLeft = 90;
            this.txtGiaoXuNhan.EditEnabled = true;
            this.txtGiaoXuNhan.Label = "Giáo phận:";
            this.txtGiaoXuNhan.Location = new System.Drawing.Point(6, 49);
            this.txtGiaoXuNhan.MaxLength = 32767;
            this.txtGiaoXuNhan.MultiLine = false;
            this.txtGiaoXuNhan.Name = "txtGiaoXuNhan";
            this.txtGiaoXuNhan.NumberInputRequired = true;
            this.txtGiaoXuNhan.NumberMode = false;
            this.txtGiaoXuNhan.ReadOnly = false;
            this.txtGiaoXuNhan.Size = new System.Drawing.Size(517, 25);
            this.txtGiaoXuNhan.TabIndex = 1;
            // 
            // txtChaNhan
            // 
            this.txtChaNhan.AutoCompleteEnabled = true;
            this.txtChaNhan.AutoUpperFirstChar = true;
            this.txtChaNhan.BoxLeft = 90;
            this.txtChaNhan.EditEnabled = true;
            this.txtChaNhan.Label = "Kính gửi cha xứ:";
            this.txtChaNhan.Location = new System.Drawing.Point(6, 18);
            this.txtChaNhan.MaxLength = 32767;
            this.txtChaNhan.MultiLine = false;
            this.txtChaNhan.Name = "txtChaNhan";
            this.txtChaNhan.NumberInputRequired = true;
            this.txtChaNhan.NumberMode = false;
            this.txtChaNhan.ReadOnly = false;
            this.txtChaNhan.Size = new System.Drawing.Size(517, 25);
            this.txtChaNhan.TabIndex = 0;
            // 
            // gxCommand1
            // 
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 387);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(547, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // frmReportRaoHP
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(547, 432);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmRaoHonPhoi";
            this.Text = "Rao Hon Phoi";
            this.Load += new System.EventHandler(this.frmReportRaoHP_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxTextField txtGiaoXuNhan;
        private GxLinhMuc txtChaNhan;
        private GxCommand gxCommand1;
        private GxComboField cbChaGui;
        private GxGiaoDan txtNguoi1;
        private GxGiaoDan txtNguoi2;
        private GxTextField txtGiaoPhanTruoc1;
        private GxTextField txtGiaoXuTruoc1;
        private GxTextField txtGiaoPhan1;
        private GxTextField txtGiaoXu1;
        private GxTextField txtGiaoPhanTruoc2;
        private GxTextField txtGiaoXuTruoc2;
        private GxTextField txtGiaoPhan2;
        private GxTextField txtGiaoXu2;
        private GxDateField dtNgayRao1;
        private GxDateField dtNgayRao3;
        private GxDateField dtNgayRao2;
        private GxTextField txtGhiChu;
    }
}