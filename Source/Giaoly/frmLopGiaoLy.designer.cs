namespace GiaoLy
{
    partial class frmLopGiaoLyList
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
            Janus.Windows.GridEX.GridEXLayout gxGiaoLyVien1_DesignTimeLayout = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLopGiaoLyList));
            this.txtGhiChu = new GxControl.GxTextField();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gxAddEdit2 = new GxControl.GxAddEdit();
            this.lblTotal = new System.Windows.Forms.Label();
            this.lblT = new System.Windows.Forms.Label();
            this.lblNam = new System.Windows.Forms.Label();
            this.cbNam = new Janus.Windows.EditControls.UIComboBox();
            this.txtPhongHoc = new GxControl.GxTextField();
            this.txtTenLop = new GxControl.GxTextField();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.btn1 = new GxControl.GxButton();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.gxCommand1 = new GxControl.GxCommand();
            this.gxHocSinhList1 = new GxControl.GxHocSinh();
            this.gxGiaoLyVien1 = new GxControl.GxHocSinh();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxHocSinhList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoLyVien1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.AutoCompleteEnabled = true;
            this.txtGhiChu.AutoUpperFirstChar = false;
            this.txtGhiChu.BoxLeft = 80;
            this.txtGhiChu.EditEnabled = true;
            this.txtGhiChu.Label = "Ghi chú";
            this.txtGhiChu.Location = new System.Drawing.Point(6, 109);
            this.txtGhiChu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGhiChu.MaxLength = 32767;
            this.txtGhiChu.MultiLine = true;
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.NumberInputRequired = true;
            this.txtGhiChu.NumberMode = false;
            this.txtGhiChu.ReadOnly = false;
            this.txtGhiChu.Size = new System.Drawing.Size(375, 63);
            this.txtGhiChu.TabIndex = 3;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.gxGiaoLyVien1);
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.gxAddEdit2);
            this.uiGroupBox1.Controls.Add(this.lblTotal);
            this.uiGroupBox1.Controls.Add(this.lblT);
            this.uiGroupBox1.Controls.Add(this.lblNam);
            this.uiGroupBox1.Controls.Add(this.cbNam);
            this.uiGroupBox1.Controls.Add(this.txtPhongHoc);
            this.uiGroupBox1.Controls.Add(this.txtTenLop);
            this.uiGroupBox1.Controls.Add(this.txtGhiChu);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(1053, 178);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin lớp giáo lý";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(448, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Danh sách giáo lý viên của lớp";
            // 
            // gxAddEdit2
            // 
            this.gxAddEdit2.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit2.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit2.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit2.GridData = null;
            this.gxAddEdit2.Location = new System.Drawing.Point(451, 39);
            this.gxAddEdit2.Name = "gxAddEdit2";
            this.gxAddEdit2.Size = new System.Drawing.Size(391, 29);
            this.gxAddEdit2.TabIndex = 4;
            this.gxAddEdit2.ToolTipAdd = "Thêm một học sinh vào lớp";
            this.gxAddEdit2.ToolTipButton1 = "";
            this.gxAddEdit2.ToolTipButton2 = "";
            this.gxAddEdit2.ToolTipDelete = "Xóa học sinh được chọn";
            this.gxAddEdit2.ToolTipEdit = "Sửa học sinh được chọn";
            this.gxAddEdit2.ToolTipFind = "";
            this.gxAddEdit2.ToolTipPrint = "";
            this.gxAddEdit2.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
    "ếm";
            this.gxAddEdit2.ToolTipSelect = "Thêm một người con được chọn từ danh sách giáo dân có sẵn";
            this.gxAddEdit2.SelectClick += new System.EventHandler(this.gxAddEdit2_SelectClick);
            this.gxAddEdit2.AddClick += new System.EventHandler(this.gxAddEdit2_AddClick);
            this.gxAddEdit2.EditClick += new System.EventHandler(this.gxAddEdit2_EditClick);
            this.gxAddEdit2.DeleteClick += new System.EventHandler(this.gxAddEdit2_DeleteClick);
            this.gxAddEdit2.PrintClick += new System.EventHandler(this.gxAddEdit2_PrintClick);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.Location = new System.Drawing.Point(356, 51);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(21, 13);
            this.lblTotal.TabIndex = 19;
            this.lblTotal.Text = "12";
            this.lblTotal.Visible = false;
            // 
            // lblT
            // 
            this.lblT.AutoSize = true;
            this.lblT.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblT.Location = new System.Drawing.Point(286, 51);
            this.lblT.Name = "lblT";
            this.lblT.Size = new System.Drawing.Size(61, 13);
            this.lblT.TabIndex = 18;
            this.lblT.Text = "Tổng số :";
            this.lblT.Visible = false;
            // 
            // lblNam
            // 
            this.lblNam.AutoSize = true;
            this.lblNam.Location = new System.Drawing.Point(35, 23);
            this.lblNam.Name = "lblNam";
            this.lblNam.Size = new System.Drawing.Size(50, 13);
            this.lblNam.TabIndex = 16;
            this.lblNam.Text = "Năm học";
            // 
            // cbNam
            // 
            this.cbNam.Location = new System.Drawing.Point(88, 19);
            this.cbNam.Name = "cbNam";
            this.cbNam.Size = new System.Drawing.Size(127, 20);
            this.cbNam.TabIndex = 0;
            // 
            // txtPhongHoc
            // 
            this.txtPhongHoc.AutoCompleteEnabled = true;
            this.txtPhongHoc.AutoUpperFirstChar = false;
            this.txtPhongHoc.BoxLeft = 80;
            this.txtPhongHoc.EditEnabled = true;
            this.txtPhongHoc.Label = "Phòng học";
            this.txtPhongHoc.Location = new System.Drawing.Point(6, 76);
            this.txtPhongHoc.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPhongHoc.MaxLength = 255;
            this.txtPhongHoc.MultiLine = false;
            this.txtPhongHoc.Name = "txtPhongHoc";
            this.txtPhongHoc.NumberInputRequired = false;
            this.txtPhongHoc.NumberMode = false;
            this.txtPhongHoc.ReadOnly = false;
            this.txtPhongHoc.Size = new System.Drawing.Size(267, 25);
            this.txtPhongHoc.TabIndex = 2;
            // 
            // txtTenLop
            // 
            this.txtTenLop.AutoCompleteEnabled = true;
            this.txtTenLop.AutoUpperFirstChar = false;
            this.txtTenLop.BoxLeft = 80;
            this.txtTenLop.EditEnabled = true;
            this.txtTenLop.Label = "Tên lớp";
            this.txtTenLop.Location = new System.Drawing.Point(6, 45);
            this.txtTenLop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenLop.MaxLength = 255;
            this.txtTenLop.MultiLine = false;
            this.txtTenLop.Name = "txtTenLop";
            this.txtTenLop.NumberInputRequired = false;
            this.txtTenLop.NumberMode = false;
            this.txtTenLop.ReadOnly = false;
            this.txtTenLop.Size = new System.Drawing.Size(267, 25);
            this.txtTenLop.TabIndex = 1;
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxHocSinhList1);
            this.uiGroupBox2.Controls.Add(this.gxGroupBox1);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 178);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(1053, 463);
            this.uiGroupBox2.TabIndex = 1;
            this.uiGroupBox2.Text = "Danh sách thiếu nhi trong lớp";
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.gxGroupBox1.Controls.Add(this.btn1);
            this.gxGroupBox1.Controls.Add(this.gxAddEdit1);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox1.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.gxGroupBox1.Location = new System.Drawing.Point(3, 16);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(1047, 29);
            this.gxGroupBox1.TabIndex = 2;
            // 
            // btn1
            // 
            this.btn1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn1.AutoSize = true;
            this.btn1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn1.BackgroundImage")));
            this.btn1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn1.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btn1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btn1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btn1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btn1.ImageKey = "ExportExcel";
            this.btn1.Location = new System.Drawing.Point(952, 3);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(86, 24);
            this.btn1.TabIndex = 1;
            this.btn1.Text = "Xét lên lớp";
            this.btn1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = null;
            this.gxAddEdit1.Location = new System.Drawing.Point(9, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(391, 29);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm một học sinh vào lớp";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Xóa học sinh được chọn";
            this.gxAddEdit1.ToolTipEdit = "Sửa học sinh được chọn";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "";
            this.gxAddEdit1.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
    "ếm";
            this.gxAddEdit1.ToolTipSelect = "Thêm một người con được chọn từ danh sách giáo dân có sẵn";
            this.gxAddEdit1.SelectClick += new System.EventHandler(this.gxAddEdit1_SelectClick);
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.PrintClick += new System.EventHandler(this.btnInDanhSach_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(163)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(9, 659);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(393, 15);
            this.label2.TabIndex = 23;
            this.label2.Text = "* Cho phép sửa cột hoàn thành khóa học và ghi chú trên lưới danh sách";
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 641);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(1053, 42);
            this.gxCommand1.TabIndex = 2;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "";
            this.gxCommand1.ToolTipOK = "";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // gxHocSinhList1
            // 
            this.gxHocSinhList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxHocSinhList1.Arguments = null;
            this.gxHocSinhList1.AutoLoadGridFormat = true;
            this.gxHocSinhList1.ColumnAutoResize = true;
            this.gxHocSinhList1.DisableParentOnLoadData = true;
            this.gxHocSinhList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxHocSinhList1.DynamicFiltering = true;
            this.gxHocSinhList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxHocSinhList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.gxHocSinhList1.GhiChu = "";
            this.gxHocSinhList1.GroupByBoxVisible = false;
            this.gxHocSinhList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxHocSinhList1.HoanThanh = false;
            this.gxHocSinhList1.HoTen = "";
            this.gxHocSinhList1.Location = new System.Drawing.Point(3, 45);
            this.gxHocSinhList1.MaLop = 0;
            this.gxHocSinhList1.Name = "gxHocSinhList1";
            this.gxHocSinhList1.NgaySinh = "";
            this.gxHocSinhList1.Phai = "";
            this.gxHocSinhList1.QueryString = "";
            this.gxHocSinhList1.RecordNavigator = true;
            this.gxHocSinhList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxHocSinhList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxHocSinhList1.Size = new System.Drawing.Size(1047, 415);
            this.gxHocSinhList1.SoThuTu = 0;
            this.gxHocSinhList1.TabIndex = 3;
            this.gxHocSinhList1.TenThanh = "";
            this.gxHocSinhList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxHocSinhList1.WhereSQL = "";
            // 
            // gxGiaoLyVien1
            // 
            this.gxGiaoLyVien1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoLyVien1.Arguments = null;
            this.gxGiaoLyVien1.AutoLoadGridFormat = true;
            this.gxGiaoLyVien1.ColumnAutoResize = true;
            gxGiaoLyVien1_DesignTimeLayout.LayoutString = resources.GetString("gxGiaoLyVien1_DesignTimeLayout.LayoutString");
            this.gxGiaoLyVien1.DesignTimeLayout = gxGiaoLyVien1_DesignTimeLayout;
            this.gxGiaoLyVien1.DisableParentOnLoadData = true;
            this.gxGiaoLyVien1.DynamicFiltering = true;
            this.gxGiaoLyVien1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaoLyVien1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaoLyVien1.GhiChu = "";
            this.gxGiaoLyVien1.GroupByBoxVisible = false;
            this.gxGiaoLyVien1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaoLyVien1.HoanThanh = false;
            this.gxGiaoLyVien1.HoTen = "";
            this.gxGiaoLyVien1.Location = new System.Drawing.Point(451, 68);
            this.gxGiaoLyVien1.MaLop = 0;
            this.gxGiaoLyVien1.Name = "gxGiaoLyVien1";
            this.gxGiaoLyVien1.NgaySinh = "";
            this.gxGiaoLyVien1.Phai = "";
            this.gxGiaoLyVien1.QueryString = "";
            this.gxGiaoLyVien1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoLyVien1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.Default;
            this.gxGiaoLyVien1.Size = new System.Drawing.Size(415, 100);
            this.gxGiaoLyVien1.SoThuTu = 0;
            this.gxGiaoLyVien1.TabIndex = 5;
            this.gxGiaoLyVien1.TenThanh = "";
            this.gxGiaoLyVien1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoLyVien1.WhereSQL = "";
            // 
            // frmLopGiaoLyList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1053, 683);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "frmLopGiaoLyList";
            this.Text = "Lớp giáo lý";
            this.Load += new System.EventHandler(this.frmLopGiaoLy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.gxGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxHocSinhList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoLyVien1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private GxControl.GxTextField txtGhiChu;
        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxTextField txtTenLop;
        private System.Windows.Forms.Label lblNam;
        private Janus.Windows.EditControls.UIComboBox cbNam;
        private GxControl.GxHocSinh gxHocSinhList1;
        private System.Windows.Forms.Label lblT;
        private System.Windows.Forms.Label lblTotal;
        public GxControl.GxButton btn1;
        private System.Windows.Forms.Label label1;
        private GxControl.GxAddEdit gxAddEdit2;
        private GxControl.GxHocSinh gxGiaoLyVien1;
        private System.Windows.Forms.Label label2;
        private GxControl.GxTextField txtPhongHoc;

    }
}

