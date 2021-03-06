namespace GxControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRaoHonPhoi));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.txtGiaoXuNhan = new GxControl.GxTextField();
            this.txtChaNhan = new GxControl.GxLinhMuc();
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
            this.gxCommand1 = new GxControl.GxCommand();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDoiRao = new GxControl.GxTextField();
            this.txtGiaoXuNQ1 = new GxControl.GxTextField();
            this.txtGiaoPhanNQ1 = new GxControl.GxTextField();
            this.txtGiaoXuNQ2 = new GxControl.GxTextField();
            this.txtGiaoPhanNQ2 = new GxControl.GxTextField();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.txtGiaoXuNhan);
            this.uiGroupBox1.Controls.Add(this.txtChaNhan);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(599, 83);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin báo cáo";
            // 
            // txtGiaoXuNhan
            // 
            this.txtGiaoXuNhan.AutoCompleteEnabled = true;
            this.txtGiaoXuNhan.AutoUpperFirstChar = true;
            this.txtGiaoXuNhan.BoxLeft = 90;
            this.txtGiaoXuNhan.EditEnabled = true;
            this.txtGiaoXuNhan.Label = "Giáo phận:";
            this.txtGiaoXuNhan.Location = new System.Drawing.Point(6, 45);
            this.txtGiaoXuNhan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXuNhan.MaxLength = 255;
            this.txtGiaoXuNhan.MultiLine = false;
            this.txtGiaoXuNhan.Name = "txtGiaoXuNhan";
            this.txtGiaoXuNhan.NumberInputRequired = true;
            this.txtGiaoXuNhan.NumberMode = false;
            this.txtGiaoXuNhan.ReadOnly = false;
            this.txtGiaoXuNhan.Size = new System.Drawing.Size(579, 24);
            this.txtGiaoXuNhan.TabIndex = 1;
            // 
            // txtChaNhan
            // 
            this.txtChaNhan.AutoCompleteEnabled = true;
            this.txtChaNhan.AutoLoadData = true;
            this.txtChaNhan.AutoUpperFirstChar = true;
            this.txtChaNhan.BoxLeft = 90;
            this.txtChaNhan.EditEnabled = true;
            this.txtChaNhan.Label = "Kính gửi cha xứ:";
            this.txtChaNhan.Location = new System.Drawing.Point(6, 14);
            this.txtChaNhan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtChaNhan.MaxLength = 255;
            this.txtChaNhan.MultiLine = false;
            this.txtChaNhan.Name = "txtChaNhan";
            this.txtChaNhan.NumberInputRequired = true;
            this.txtChaNhan.NumberMode = false;
            this.txtChaNhan.ReadOnly = false;
            this.txtChaNhan.Size = new System.Drawing.Size(579, 24);
            this.txtChaNhan.TabIndex = 0;
            // 
            // dtNgayRao3
            // 
            this.dtNgayRao3.AutoUpperFirstChar = false;
            this.dtNgayRao3.BoxLeft = 55;
            this.dtNgayRao3.EditEnabled = true;
            this.dtNgayRao3.FullInputRequired = true;
            this.dtNgayRao3.IsNullDate = true;
            this.dtNgayRao3.Label = "rao lần 3:";
            this.dtNgayRao3.Location = new System.Drawing.Point(415, 329);
            this.dtNgayRao3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayRao3.Name = "dtNgayRao3";
            this.dtNgayRao3.Size = new System.Drawing.Size(172, 26);
            this.dtNgayRao3.TabIndex = 17;
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
            this.dtNgayRao2.Location = new System.Drawing.Point(225, 329);
            this.dtNgayRao2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayRao2.Name = "dtNgayRao2";
            this.dtNgayRao2.Size = new System.Drawing.Size(173, 26);
            this.dtNgayRao2.TabIndex = 17;
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
            this.dtNgayRao1.Location = new System.Drawing.Point(12, 329);
            this.dtNgayRao1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayRao1.Name = "dtNgayRao1";
            this.dtNgayRao1.Size = new System.Drawing.Size(207, 26);
            this.dtNgayRao1.TabIndex = 16;
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
            this.txtNguoi2.Location = new System.Drawing.Point(12, 177);
            this.txtNguoi2.MaGiaoDan = -1;
            this.txtNguoi2.MaGiaoHo = -1;
            this.txtNguoi2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNguoi2.MaxLength = 32767;
            this.txtNguoi2.MultiLine = false;
            this.txtNguoi2.Name = "txtNguoi2";
            this.txtNguoi2.NumberInputRequired = true;
            this.txtNguoi2.NumberMode = false;
            this.txtNguoi2.ReadOnly = true;
            this.txtNguoi2.Size = new System.Drawing.Size(569, 26);
            this.txtNguoi2.TabIndex = 8;
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
            this.txtNguoi1.Location = new System.Drawing.Point(12, 49);
            this.txtNguoi1.MaGiaoDan = -1;
            this.txtNguoi1.MaGiaoHo = -1;
            this.txtNguoi1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNguoi1.MaxLength = 32767;
            this.txtNguoi1.MultiLine = false;
            this.txtNguoi1.Name = "txtNguoi1";
            this.txtNguoi1.NumberInputRequired = true;
            this.txtNguoi1.NumberMode = false;
            this.txtNguoi1.ReadOnly = true;
            this.txtNguoi1.Size = new System.Drawing.Size(569, 27);
            this.txtNguoi1.TabIndex = 1;
            this.txtNguoi1.WhereSQL = "";
            // 
            // cbChaGui
            // 
            this.cbChaGui.AutoUpperFirstChar = true;
            this.cbChaGui.BoxLeft = 90;
            this.cbChaGui.EditEnabled = true;
            this.cbChaGui.Label = "Linh mục chứng:";
            this.cbChaGui.Location = new System.Drawing.Point(12, 291);
            this.cbChaGui.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbChaGui.MaxLength = 255;
            this.cbChaGui.Name = "cbChaGui";
            this.cbChaGui.SelectedIndex = -1;
            this.cbChaGui.SelectedText = "";
            this.cbChaGui.SelectedValue = null;
            this.cbChaGui.Size = new System.Drawing.Size(573, 26);
            this.cbChaGui.TabIndex = 15;
            // 
            // txtGiaoPhanTruoc2
            // 
            this.txtGiaoPhanTruoc2.AutoCompleteEnabled = true;
            this.txtGiaoPhanTruoc2.AutoUpperFirstChar = true;
            this.txtGiaoPhanTruoc2.BoxLeft = 60;
            this.txtGiaoPhanTruoc2.EditEnabled = true;
            this.txtGiaoPhanTruoc2.Label = "Giáo phận:";
            this.txtGiaoPhanTruoc2.Location = new System.Drawing.Point(317, 265);
            this.txtGiaoPhanTruoc2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhanTruoc2.MaxLength = 255;
            this.txtGiaoPhanTruoc2.MultiLine = false;
            this.txtGiaoPhanTruoc2.Name = "txtGiaoPhanTruoc2";
            this.txtGiaoPhanTruoc2.NumberInputRequired = true;
            this.txtGiaoPhanTruoc2.NumberMode = false;
            this.txtGiaoPhanTruoc2.ReadOnly = false;
            this.txtGiaoPhanTruoc2.Size = new System.Drawing.Size(268, 25);
            this.txtGiaoPhanTruoc2.TabIndex = 14;
            // 
            // txtGiaoPhanTruoc1
            // 
            this.txtGiaoPhanTruoc1.AutoCompleteEnabled = true;
            this.txtGiaoPhanTruoc1.AutoUpperFirstChar = true;
            this.txtGiaoPhanTruoc1.BoxLeft = 60;
            this.txtGiaoPhanTruoc1.EditEnabled = true;
            this.txtGiaoPhanTruoc1.Label = "Giáo phận:";
            this.txtGiaoPhanTruoc1.Location = new System.Drawing.Point(317, 137);
            this.txtGiaoPhanTruoc1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhanTruoc1.MaxLength = 255;
            this.txtGiaoPhanTruoc1.MultiLine = false;
            this.txtGiaoPhanTruoc1.Name = "txtGiaoPhanTruoc1";
            this.txtGiaoPhanTruoc1.NumberInputRequired = true;
            this.txtGiaoPhanTruoc1.NumberMode = false;
            this.txtGiaoPhanTruoc1.ReadOnly = false;
            this.txtGiaoPhanTruoc1.Size = new System.Drawing.Size(268, 25);
            this.txtGiaoPhanTruoc1.TabIndex = 7;
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.AutoCompleteEnabled = true;
            this.txtGhiChu.AutoUpperFirstChar = true;
            this.txtGhiChu.BoxLeft = 90;
            this.txtGhiChu.EditEnabled = true;
            this.txtGhiChu.Label = "Ghi chú:";
            this.txtGhiChu.Location = new System.Drawing.Point(12, 357);
            this.txtGhiChu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGhiChu.MaxLength = 32767;
            this.txtGhiChu.MultiLine = true;
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.NumberInputRequired = true;
            this.txtGhiChu.NumberMode = false;
            this.txtGhiChu.ReadOnly = false;
            this.txtGhiChu.Size = new System.Drawing.Size(573, 35);
            this.txtGhiChu.TabIndex = 19;
            // 
            // txtGiaoXuTruoc2
            // 
            this.txtGiaoXuTruoc2.AutoCompleteEnabled = true;
            this.txtGiaoXuTruoc2.AutoUpperFirstChar = true;
            this.txtGiaoXuTruoc2.BoxLeft = 90;
            this.txtGiaoXuTruoc2.EditEnabled = true;
            this.txtGiaoXuTruoc2.Label = "Trước đã ở xứ:";
            this.txtGiaoXuTruoc2.Location = new System.Drawing.Point(12, 263);
            this.txtGiaoXuTruoc2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXuTruoc2.MaxLength = 255;
            this.txtGiaoXuTruoc2.MultiLine = false;
            this.txtGiaoXuTruoc2.Name = "txtGiaoXuTruoc2";
            this.txtGiaoXuTruoc2.NumberInputRequired = true;
            this.txtGiaoXuTruoc2.NumberMode = false;
            this.txtGiaoXuTruoc2.ReadOnly = false;
            this.txtGiaoXuTruoc2.Size = new System.Drawing.Size(299, 25);
            this.txtGiaoXuTruoc2.TabIndex = 13;
            // 
            // txtGiaoPhan2
            // 
            this.txtGiaoPhan2.AutoCompleteEnabled = true;
            this.txtGiaoPhan2.AutoUpperFirstChar = true;
            this.txtGiaoPhan2.BoxLeft = 60;
            this.txtGiaoPhan2.EditEnabled = true;
            this.txtGiaoPhan2.Label = "Giáo phận:";
            this.txtGiaoPhan2.Location = new System.Drawing.Point(317, 237);
            this.txtGiaoPhan2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhan2.MaxLength = 255;
            this.txtGiaoPhan2.MultiLine = false;
            this.txtGiaoPhan2.Name = "txtGiaoPhan2";
            this.txtGiaoPhan2.NumberInputRequired = true;
            this.txtGiaoPhan2.NumberMode = false;
            this.txtGiaoPhan2.ReadOnly = false;
            this.txtGiaoPhan2.Size = new System.Drawing.Size(268, 25);
            this.txtGiaoPhan2.TabIndex = 12;
            // 
            // txtGiaoXuTruoc1
            // 
            this.txtGiaoXuTruoc1.AutoCompleteEnabled = true;
            this.txtGiaoXuTruoc1.AutoUpperFirstChar = true;
            this.txtGiaoXuTruoc1.BoxLeft = 90;
            this.txtGiaoXuTruoc1.EditEnabled = true;
            this.txtGiaoXuTruoc1.Label = "Trước đã ở xứ:";
            this.txtGiaoXuTruoc1.Location = new System.Drawing.Point(12, 137);
            this.txtGiaoXuTruoc1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXuTruoc1.MaxLength = 255;
            this.txtGiaoXuTruoc1.MultiLine = false;
            this.txtGiaoXuTruoc1.Name = "txtGiaoXuTruoc1";
            this.txtGiaoXuTruoc1.NumberInputRequired = true;
            this.txtGiaoXuTruoc1.NumberMode = false;
            this.txtGiaoXuTruoc1.ReadOnly = false;
            this.txtGiaoXuTruoc1.Size = new System.Drawing.Size(299, 25);
            this.txtGiaoXuTruoc1.TabIndex = 6;
            // 
            // txtGiaoXu2
            // 
            this.txtGiaoXu2.AutoCompleteEnabled = true;
            this.txtGiaoXu2.AutoUpperFirstChar = true;
            this.txtGiaoXu2.BoxLeft = 90;
            this.txtGiaoXu2.EditEnabled = true;
            this.txtGiaoXu2.Label = "Hiện ở giáo xứ:";
            this.txtGiaoXu2.Location = new System.Drawing.Point(12, 235);
            this.txtGiaoXu2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXu2.MaxLength = 255;
            this.txtGiaoXu2.MultiLine = false;
            this.txtGiaoXu2.Name = "txtGiaoXu2";
            this.txtGiaoXu2.NumberInputRequired = true;
            this.txtGiaoXu2.NumberMode = false;
            this.txtGiaoXu2.ReadOnly = false;
            this.txtGiaoXu2.Size = new System.Drawing.Size(299, 25);
            this.txtGiaoXu2.TabIndex = 11;
            // 
            // txtGiaoPhan1
            // 
            this.txtGiaoPhan1.AutoCompleteEnabled = true;
            this.txtGiaoPhan1.AutoUpperFirstChar = true;
            this.txtGiaoPhan1.BoxLeft = 60;
            this.txtGiaoPhan1.EditEnabled = true;
            this.txtGiaoPhan1.Label = "Giáo phận:";
            this.txtGiaoPhan1.Location = new System.Drawing.Point(317, 108);
            this.txtGiaoPhan1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhan1.MaxLength = 255;
            this.txtGiaoPhan1.MultiLine = false;
            this.txtGiaoPhan1.Name = "txtGiaoPhan1";
            this.txtGiaoPhan1.NumberInputRequired = true;
            this.txtGiaoPhan1.NumberMode = false;
            this.txtGiaoPhan1.ReadOnly = false;
            this.txtGiaoPhan1.Size = new System.Drawing.Size(268, 25);
            this.txtGiaoPhan1.TabIndex = 5;
            // 
            // txtGiaoXu1
            // 
            this.txtGiaoXu1.AutoCompleteEnabled = true;
            this.txtGiaoXu1.AutoUpperFirstChar = true;
            this.txtGiaoXu1.BoxLeft = 90;
            this.txtGiaoXu1.EditEnabled = true;
            this.txtGiaoXu1.Label = "Hiện ở giáo xứ:";
            this.txtGiaoXu1.Location = new System.Drawing.Point(12, 108);
            this.txtGiaoXu1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXu1.MaxLength = 255;
            this.txtGiaoXu1.MultiLine = false;
            this.txtGiaoXu1.Name = "txtGiaoXu1";
            this.txtGiaoXu1.NumberInputRequired = true;
            this.txtGiaoXu1.NumberMode = false;
            this.txtGiaoXu1.ReadOnly = false;
            this.txtGiaoXu1.Size = new System.Drawing.Size(299, 25);
            this.txtGiaoXu1.TabIndex = 4;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 482);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(599, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.label1);
            this.gxGroupBox1.Controls.Add(this.dtNgayRao3);
            this.gxGroupBox1.Controls.Add(this.txtNguoi1);
            this.gxGroupBox1.Controls.Add(this.dtNgayRao2);
            this.gxGroupBox1.Controls.Add(this.txtDoiRao);
            this.gxGroupBox1.Controls.Add(this.txtGiaoXuNQ1);
            this.gxGroupBox1.Controls.Add(this.txtGiaoXu1);
            this.gxGroupBox1.Controls.Add(this.dtNgayRao1);
            this.gxGroupBox1.Controls.Add(this.txtGiaoPhanNQ1);
            this.gxGroupBox1.Controls.Add(this.txtGiaoPhan1);
            this.gxGroupBox1.Controls.Add(this.txtNguoi2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoXuNQ2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoXu2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoXuTruoc1);
            this.gxGroupBox1.Controls.Add(this.cbChaGui);
            this.gxGroupBox1.Controls.Add(this.txtGiaoPhanNQ2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoPhan2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoPhanTruoc2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoXuTruoc2);
            this.gxGroupBox1.Controls.Add(this.txtGiaoPhanTruoc1);
            this.gxGroupBox1.Controls.Add(this.txtGhiChu);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 83);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(599, 399);
            this.gxGroupBox1.TabIndex = 17;
            this.gxGroupBox1.Text = "Thông tin điều tra";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(309, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(243, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "(Chương trình tự động điền khi nhập xong đôi rao)";
            // 
            // txtDoiRao
            // 
            this.txtDoiRao.AutoCompleteEnabled = true;
            this.txtDoiRao.AutoUpperFirstChar = true;
            this.txtDoiRao.BoxLeft = 90;
            this.txtDoiRao.EditEnabled = true;
            this.txtDoiRao.Label = "Đôi rao:";
            this.txtDoiRao.Location = new System.Drawing.Point(12, 14);
            this.txtDoiRao.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDoiRao.MaxLength = 255;
            this.txtDoiRao.MultiLine = false;
            this.txtDoiRao.Name = "txtDoiRao";
            this.txtDoiRao.NumberInputRequired = true;
            this.txtDoiRao.NumberMode = false;
            this.txtDoiRao.ReadOnly = false;
            this.txtDoiRao.Size = new System.Drawing.Size(299, 25);
            this.txtDoiRao.TabIndex = 0;
            // 
            // txtGiaoXuNQ1
            // 
            this.txtGiaoXuNQ1.AutoCompleteEnabled = true;
            this.txtGiaoXuNQ1.AutoUpperFirstChar = true;
            this.txtGiaoXuNQ1.BoxLeft = 90;
            this.txtGiaoXuNQ1.EditEnabled = true;
            this.txtGiaoXuNQ1.Label = "Nguyên quán xứ";
            this.txtGiaoXuNQ1.Location = new System.Drawing.Point(12, 79);
            this.txtGiaoXuNQ1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXuNQ1.MaxLength = 255;
            this.txtGiaoXuNQ1.MultiLine = false;
            this.txtGiaoXuNQ1.Name = "txtGiaoXuNQ1";
            this.txtGiaoXuNQ1.NumberInputRequired = true;
            this.txtGiaoXuNQ1.NumberMode = false;
            this.txtGiaoXuNQ1.ReadOnly = false;
            this.txtGiaoXuNQ1.Size = new System.Drawing.Size(299, 25);
            this.txtGiaoXuNQ1.TabIndex = 2;
            // 
            // txtGiaoPhanNQ1
            // 
            this.txtGiaoPhanNQ1.AutoCompleteEnabled = true;
            this.txtGiaoPhanNQ1.AutoUpperFirstChar = true;
            this.txtGiaoPhanNQ1.BoxLeft = 60;
            this.txtGiaoPhanNQ1.EditEnabled = true;
            this.txtGiaoPhanNQ1.Label = "Giáo phận:";
            this.txtGiaoPhanNQ1.Location = new System.Drawing.Point(317, 79);
            this.txtGiaoPhanNQ1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhanNQ1.MaxLength = 255;
            this.txtGiaoPhanNQ1.MultiLine = false;
            this.txtGiaoPhanNQ1.Name = "txtGiaoPhanNQ1";
            this.txtGiaoPhanNQ1.NumberInputRequired = true;
            this.txtGiaoPhanNQ1.NumberMode = false;
            this.txtGiaoPhanNQ1.ReadOnly = false;
            this.txtGiaoPhanNQ1.Size = new System.Drawing.Size(268, 25);
            this.txtGiaoPhanNQ1.TabIndex = 3;
            // 
            // txtGiaoXuNQ2
            // 
            this.txtGiaoXuNQ2.AutoCompleteEnabled = true;
            this.txtGiaoXuNQ2.AutoUpperFirstChar = true;
            this.txtGiaoXuNQ2.BoxLeft = 90;
            this.txtGiaoXuNQ2.EditEnabled = true;
            this.txtGiaoXuNQ2.Label = "Nguyên quán xứ";
            this.txtGiaoXuNQ2.Location = new System.Drawing.Point(12, 207);
            this.txtGiaoXuNQ2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoXuNQ2.MaxLength = 255;
            this.txtGiaoXuNQ2.MultiLine = false;
            this.txtGiaoXuNQ2.Name = "txtGiaoXuNQ2";
            this.txtGiaoXuNQ2.NumberInputRequired = true;
            this.txtGiaoXuNQ2.NumberMode = false;
            this.txtGiaoXuNQ2.ReadOnly = false;
            this.txtGiaoXuNQ2.Size = new System.Drawing.Size(299, 25);
            this.txtGiaoXuNQ2.TabIndex = 9;
            // 
            // txtGiaoPhanNQ2
            // 
            this.txtGiaoPhanNQ2.AutoCompleteEnabled = true;
            this.txtGiaoPhanNQ2.AutoUpperFirstChar = true;
            this.txtGiaoPhanNQ2.BoxLeft = 60;
            this.txtGiaoPhanNQ2.EditEnabled = true;
            this.txtGiaoPhanNQ2.Label = "Giáo phận:";
            this.txtGiaoPhanNQ2.Location = new System.Drawing.Point(317, 209);
            this.txtGiaoPhanNQ2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGiaoPhanNQ2.MaxLength = 255;
            this.txtGiaoPhanNQ2.MultiLine = false;
            this.txtGiaoPhanNQ2.Name = "txtGiaoPhanNQ2";
            this.txtGiaoPhanNQ2.NumberInputRequired = true;
            this.txtGiaoPhanNQ2.NumberMode = false;
            this.txtGiaoPhanNQ2.ReadOnly = false;
            this.txtGiaoPhanNQ2.Size = new System.Drawing.Size(268, 25);
            this.txtGiaoPhanNQ2.TabIndex = 10;
            // 
            // frmRaoHonPhoi
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(599, 527);
            this.Controls.Add(this.gxGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmRaoHonPhoi";
            this.Text = "Điều Tra Hôn Phối";
            this.Load += new System.EventHandler(this.frmReportRaoHP_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.gxGroupBox1.PerformLayout();
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
        private GxGroupBox gxGroupBox1;
        private GxTextField txtDoiRao;
        private System.Windows.Forms.Label label1;
        private GxTextField txtGiaoXuNQ1;
        private GxTextField txtGiaoPhanNQ1;
        private GxTextField txtGiaoXuNQ2;
        private GxTextField txtGiaoPhanNQ2;
    }
}