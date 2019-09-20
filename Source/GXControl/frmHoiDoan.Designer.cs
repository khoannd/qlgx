namespace GxControl
{
    partial class frmHoiDoan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHoiDoan));
            this.gxGroupBox4 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gxCommand1 = new GxControl.GxCommand();
            this.gxGroupBox2 = new GxControl.GxGroupBox();
            this.gxGroupBox5 = new GxControl.GxGroupBox();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.gxGroupBox3 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.dmNgayBonMang = new GxControl.GxDayMonthField();
            this.cbThongKe = new GxControl.GxCheckBox();
            this.dtNgayThanhLap = new GxControl.GxDateField();
            this.txtThanhBonMang = new GxControl.GxTextField();
            this.txtTenHoiDoan = new GxControl.GxTextField();
            this.txtGhiChu = new GxControl.GxTextField();
            this.txtMaHoiDoan = new GxControl.GxTextField();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox4)).BeginInit();
            this.gxGroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).BeginInit();
            this.gxGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox5)).BeginInit();
            this.gxGroupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox3)).BeginInit();
            this.gxGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gxGroupBox4
            // 
            this.gxGroupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gxGroupBox4.BackColor = System.Drawing.Color.AliceBlue;
            this.gxGroupBox4.Controls.Add(this.label1);
            this.gxGroupBox4.Controls.Add(this.gxCommand1);
            this.gxGroupBox4.Location = new System.Drawing.Point(0, 516);
            this.gxGroupBox4.Name = "gxGroupBox4";
            this.gxGroupBox4.Size = new System.Drawing.Size(830, 51);
            this.gxGroupBox4.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(14, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(360, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "*Ghi chú: Những hội viên bị gạch đỏ là những hội viên đã ra khỏi hội đoàn";
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Location = new System.Drawing.Point(648, 0);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(174, 45);
            this.gxCommand1.TabIndex = 0;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // gxGroupBox2
            // 
            this.gxGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gxGroupBox2.BackColor = System.Drawing.Color.AliceBlue;
            this.gxGroupBox2.Controls.Add(this.gxGroupBox5);
            this.gxGroupBox2.Controls.Add(this.gxGroupBox3);
            this.gxGroupBox2.Location = new System.Drawing.Point(0, 128);
            this.gxGroupBox2.Name = "gxGroupBox2";
            this.gxGroupBox2.Size = new System.Drawing.Size(830, 393);
            this.gxGroupBox2.TabIndex = 1;
            this.gxGroupBox2.Text = "Các thành viên trong hội đoàn";
            // 
            // gxGroupBox5
            // 
            this.gxGroupBox5.Controls.Add(this.gxGiaoDanList1);
            this.gxGroupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox5.Location = new System.Drawing.Point(3, 63);
            this.gxGroupBox5.Name = "gxGroupBox5";
            this.gxGroupBox5.Size = new System.Drawing.Size(824, 327);
            this.gxGroupBox5.TabIndex = 2;
            // 
            // gxGiaoDanList1
            // 
            this.gxGiaoDanList1.AllowContextMenu = true;
            this.gxGiaoDanList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxGiaoDanList1.AllowEditGiaoDan = true;
            this.gxGiaoDanList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.AllowShowForm = true;
            this.gxGiaoDanList1.Arguments = null;
            this.gxGiaoDanList1.AutoLoadGridFormat = true;
            this.gxGiaoDanList1.ColumnAutoResize = true;
            this.gxGiaoDanList1.DisableParentOnLoadData = true;
            this.gxGiaoDanList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGiaoDanList1.DynamicFiltering = true;
            this.gxGiaoDanList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaoDanList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaoDanList1.GroupByBoxVisible = false;
            this.gxGiaoDanList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaoDanList1.Location = new System.Drawing.Point(3, 8);
            this.gxGiaoDanList1.Name = "gxGiaoDanList1";
            this.gxGiaoDanList1.QueryString = "";
            this.gxGiaoDanList1.RecordNavigator = true;
            this.gxGiaoDanList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoDanList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.SelectionMode = Janus.Windows.GridEX.SelectionMode.MultipleSelection;
            this.gxGiaoDanList1.Size = new System.Drawing.Size(818, 316);
            this.gxGiaoDanList1.TabIndex = 4;
            this.gxGiaoDanList1.TabStop = false;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            this.gxGiaoDanList1.SelectionChanged += new System.EventHandler(this.gxGiaoDanList1_SelectionChanged);
            // 
            // gxGroupBox3
            // 
            this.gxGroupBox3.Controls.Add(this.gxAddEdit1);
            this.gxGroupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox3.Location = new System.Drawing.Point(3, 16);
            this.gxGroupBox3.Name = "gxGroupBox3";
            this.gxGroupBox3.Size = new System.Drawing.Size(824, 47);
            this.gxGroupBox3.TabIndex = 0;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = null;
            this.gxAddEdit1.Location = new System.Drawing.Point(6, 15);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(391, 28);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Loại bỏ khỏi danh sách trên lưới";
            this.gxAddEdit1.ToolTipEdit = "Sửa";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "In danh sách trên lưới";
            this.gxAddEdit1.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
    "ếm";
            this.gxAddEdit1.ToolTipSelect = "Chọn";
            this.gxAddEdit1.SelectClick += new System.EventHandler(this.gxAddEdit1_SelectClick);
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.ReloadClick += new System.EventHandler(this.gxAddEdit1_ReloadClick);
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.BackColor = System.Drawing.Color.AliceBlue;
            this.gxGroupBox1.Controls.Add(this.dmNgayBonMang);
            this.gxGroupBox1.Controls.Add(this.cbThongKe);
            this.gxGroupBox1.Controls.Add(this.dtNgayThanhLap);
            this.gxGroupBox1.Controls.Add(this.txtThanhBonMang);
            this.gxGroupBox1.Controls.Add(this.txtTenHoiDoan);
            this.gxGroupBox1.Controls.Add(this.txtGhiChu);
            this.gxGroupBox1.Controls.Add(this.txtMaHoiDoan);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(830, 122);
            this.gxGroupBox1.TabIndex = 0;
            this.gxGroupBox1.Text = "Thông tin hội đoàn";
            // 
            // dmNgayBonMang
            // 
            this.dmNgayBonMang.AutoUpperFirstChar = false;
            this.dmNgayBonMang.BoxLeft = 0;
            this.dmNgayBonMang.EditEnabled = true;
            this.dmNgayBonMang.IsNullMask = true;
            this.dmNgayBonMang.Label = "Ngày bổn mạng";
            this.dmNgayBonMang.Location = new System.Drawing.Point(188, 25);
            this.dmNgayBonMang.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dmNgayBonMang.Name = "dmNgayBonMang";
            this.dmNgayBonMang.Size = new System.Drawing.Size(145, 29);
            this.dmNgayBonMang.TabIndex = 1;
            this.dmNgayBonMang.Value = ((object)(resources.GetObject("dmNgayBonMang.Value")));
            // 
            // cbThongKe
            // 
            this.cbThongKe.AutoSize = true;
            this.cbThongKe.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.cbThongKe.Location = new System.Drawing.Point(83, 95);
            this.cbThongKe.Name = "cbThongKe";
            this.cbThongKe.Size = new System.Drawing.Size(169, 17);
            this.cbThongKe.TabIndex = 11;
            this.cbThongKe.TabStop = false;
            this.cbThongKe.Text = "Danh sách tất cả các hội viên";
            this.cbThongKe.UseVisualStyleBackColor = true;
            this.cbThongKe.CheckedChanged += new System.EventHandler(this.cbThongKe_CheckedChanged);
            // 
            // dtNgayThanhLap
            // 
            this.dtNgayThanhLap.AutoUpperFirstChar = false;
            this.dtNgayThanhLap.BoxLeft = 0;
            this.dtNgayThanhLap.EditEnabled = true;
            this.dtNgayThanhLap.FullInputRequired = false;
            this.dtNgayThanhLap.IsNullDate = false;
            this.dtNgayThanhLap.Label = "Ngày thành lập";
            this.dtNgayThanhLap.Location = new System.Drawing.Point(360, 28);
            this.dtNgayThanhLap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayThanhLap.Name = "dtNgayThanhLap";
            this.dtNgayThanhLap.Size = new System.Drawing.Size(210, 26);
            this.dtNgayThanhLap.TabIndex = 2;
            this.dtNgayThanhLap.Value = "05/04/2009";
            // 
            // txtThanhBonMang
            // 
            this.txtThanhBonMang.AutoCompleteEnabled = true;
            this.txtThanhBonMang.AutoUpperFirstChar = false;
            this.txtThanhBonMang.BoxLeft = 0;
            this.txtThanhBonMang.EditEnabled = true;
            this.txtThanhBonMang.Label = "Thánh bổn mạng";
            this.txtThanhBonMang.Location = new System.Drawing.Point(353, 62);
            this.txtThanhBonMang.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtThanhBonMang.MaxLength = 32767;
            this.txtThanhBonMang.MultiLine = false;
            this.txtThanhBonMang.Name = "txtThanhBonMang";
            this.txtThanhBonMang.NumberInputRequired = true;
            this.txtThanhBonMang.NumberMode = false;
            this.txtThanhBonMang.ReadOnly = false;
            this.txtThanhBonMang.Size = new System.Drawing.Size(258, 26);
            this.txtThanhBonMang.TabIndex = 3;
            // 
            // txtTenHoiDoan
            // 
            this.txtTenHoiDoan.AutoCompleteEnabled = true;
            this.txtTenHoiDoan.AutoUpperFirstChar = false;
            this.txtTenHoiDoan.BoxLeft = 0;
            this.txtTenHoiDoan.EditEnabled = true;
            this.txtTenHoiDoan.Label = "Tên hội đoàn";
            this.txtTenHoiDoan.Location = new System.Drawing.Point(11, 62);
            this.txtTenHoiDoan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenHoiDoan.MaxLength = 32767;
            this.txtTenHoiDoan.MultiLine = false;
            this.txtTenHoiDoan.Name = "txtTenHoiDoan";
            this.txtTenHoiDoan.NumberInputRequired = true;
            this.txtTenHoiDoan.NumberMode = false;
            this.txtTenHoiDoan.ReadOnly = false;
            this.txtTenHoiDoan.Size = new System.Drawing.Size(320, 26);
            this.txtTenHoiDoan.TabIndex = 0;
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhiChu.AutoCompleteEnabled = true;
            this.txtGhiChu.AutoUpperFirstChar = false;
            this.txtGhiChu.BoxLeft = 0;
            this.txtGhiChu.EditEnabled = true;
            this.txtGhiChu.Label = "Ghi chú";
            this.txtGhiChu.Location = new System.Drawing.Point(617, 28);
            this.txtGhiChu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGhiChu.MaxLength = 32767;
            this.txtGhiChu.MultiLine = true;
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.NumberInputRequired = true;
            this.txtGhiChu.NumberMode = false;
            this.txtGhiChu.ReadOnly = false;
            this.txtGhiChu.Size = new System.Drawing.Size(201, 60);
            this.txtGhiChu.TabIndex = 4;
            // 
            // txtMaHoiDoan
            // 
            this.txtMaHoiDoan.AutoCompleteEnabled = true;
            this.txtMaHoiDoan.AutoUpperFirstChar = false;
            this.txtMaHoiDoan.BoxLeft = 0;
            this.txtMaHoiDoan.EditEnabled = true;
            this.txtMaHoiDoan.Label = "Mã hội đoàn";
            this.txtMaHoiDoan.Location = new System.Drawing.Point(15, 28);
            this.txtMaHoiDoan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMaHoiDoan.MaxLength = 32767;
            this.txtMaHoiDoan.MultiLine = false;
            this.txtMaHoiDoan.Name = "txtMaHoiDoan";
            this.txtMaHoiDoan.NumberInputRequired = true;
            this.txtMaHoiDoan.NumberMode = false;
            this.txtMaHoiDoan.ReadOnly = true;
            this.txtMaHoiDoan.Size = new System.Drawing.Size(143, 26);
            this.txtMaHoiDoan.TabIndex = 10;
            this.txtMaHoiDoan.TabStop = false;
            // 
            // frmHoiDoan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(830, 564);
            this.Controls.Add(this.gxGroupBox4);
            this.Controls.Add(this.gxGroupBox2);
            this.Controls.Add(this.gxGroupBox1);
            this.Name = "frmHoiDoan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hội đoàn";
            this.Load += new System.EventHandler(this.frmHoiDoan_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox4)).EndInit();
            this.gxGroupBox4.ResumeLayout(false);
            this.gxGroupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).EndInit();
            this.gxGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox5)).EndInit();
            this.gxGroupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox3)).EndInit();
            this.gxGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.gxGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxGroupBox gxGroupBox1;
        private GxGroupBox gxGroupBox2;
        private GxGroupBox gxGroupBox4;
        private GxGroupBox gxGroupBox3;
        private GxAddEdit gxAddEdit1;
        private GxCommand gxCommand1;
        private GxTextField txtTenHoiDoan;
        private GxTextField txtMaHoiDoan;
        private GxTextField txtGhiChu;
        private GxGroupBox gxGroupBox5;
        private GxTextField txtThanhBonMang;
        private GxDateField dtNgayThanhLap;
        private GxGiaoDanList gxGiaoDanList1;
        private GxCheckBox cbThongKe;
        private System.Windows.Forms.Label label1;
        private GxDayMonthField dmNgayBonMang;
    }
}