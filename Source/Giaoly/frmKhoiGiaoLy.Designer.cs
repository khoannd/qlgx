namespace GiaoLy
{
    partial class frmKhoiGiaoLy
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
            Janus.Windows.GridEX.GridEXLayout gxLopGiaoLyList1_DesignTimeLayout = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmKhoiGiaoLy));
            this.txtNguoiQuanLy = new GxControl.GxGiaoDan();
            this.txtGhiChu = new GxControl.GxTextField();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.lblNam = new System.Windows.Forms.Label();
            this.cbNam = new Janus.Windows.EditControls.UIComboBox();
            this.txtTenKhoi = new GxControl.GxTextField();
            this.gxCommand1 = new GxControl.GxCommand();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.gxLopGiaoLyList1 = new GxControl.GxLopGiaoLyList();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxLopGiaoLyList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtNguoiQuanLy
            // 
            this.txtNguoiQuanLy.AutoCompleteEnabled = true;
            this.txtNguoiQuanLy.AutoUpperFirstChar = true;
            this.txtNguoiQuanLy.BoxLeft = 80;
            this.txtNguoiQuanLy.CheckQDGXGD = false;
            this.txtNguoiQuanLy.CurrentRow = null;
            this.txtNguoiQuanLy.DisplayMode = GxGlobal.DisplayMode.Full;
            this.txtNguoiQuanLy.EditEnabled = true;
            this.txtNguoiQuanLy.Label = "Người quản lý";
            this.txtNguoiQuanLy.Location = new System.Drawing.Point(6, 74);
            this.txtNguoiQuanLy.MaGiaoDan = -1;
            this.txtNguoiQuanLy.MaGiaoHo = -1;
            this.txtNguoiQuanLy.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNguoiQuanLy.MaxLength = 255;
            this.txtNguoiQuanLy.MultiLine = false;
            this.txtNguoiQuanLy.Name = "txtNguoiQuanLy";
            this.txtNguoiQuanLy.NumberInputRequired = true;
            this.txtNguoiQuanLy.NumberMode = false;
            this.txtNguoiQuanLy.ReadOnly = true;
            this.txtNguoiQuanLy.Size = new System.Drawing.Size(392, 26);
            this.txtNguoiQuanLy.TabIndex = 0;
            this.txtNguoiQuanLy.WhereSQL = "";
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.AutoCompleteEnabled = true;
            this.txtGhiChu.AutoUpperFirstChar = false;
            this.txtGhiChu.BoxLeft = 80;
            this.txtGhiChu.EditEnabled = true;
            this.txtGhiChu.Label = "Ghi chú";
            this.txtGhiChu.Location = new System.Drawing.Point(6, 104);
            this.txtGhiChu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGhiChu.MaxLength = 32767;
            this.txtGhiChu.MultiLine = true;
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.NumberInputRequired = true;
            this.txtGhiChu.NumberMode = false;
            this.txtGhiChu.ReadOnly = false;
            this.txtGhiChu.Size = new System.Drawing.Size(851, 58);
            this.txtGhiChu.TabIndex = 14;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.lblNam);
            this.uiGroupBox1.Controls.Add(this.cbNam);
            this.uiGroupBox1.Controls.Add(this.txtTenKhoi);
            this.uiGroupBox1.Controls.Add(this.txtNguoiQuanLy);
            this.uiGroupBox1.Controls.Add(this.txtGhiChu);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(990, 174);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin khối giáo lý";
            // 
            // lblNam
            // 
            this.lblNam.AutoSize = true;
            this.lblNam.Location = new System.Drawing.Point(36, 23);
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
            this.cbNam.TabIndex = 15;
            // 
            // txtTenKhoi
            // 
            this.txtTenKhoi.AutoCompleteEnabled = true;
            this.txtTenKhoi.AutoUpperFirstChar = false;
            this.txtTenKhoi.BoxLeft = 80;
            this.txtTenKhoi.EditEnabled = true;
            this.txtTenKhoi.Label = "Tên khối";
            this.txtTenKhoi.Location = new System.Drawing.Point(6, 45);
            this.txtTenKhoi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenKhoi.MaxLength = 255;
            this.txtTenKhoi.MultiLine = false;
            this.txtTenKhoi.Name = "txtTenKhoi";
            this.txtTenKhoi.NumberInputRequired = false;
            this.txtTenKhoi.NumberMode = false;
            this.txtTenKhoi.ReadOnly = false;
            this.txtTenKhoi.Size = new System.Drawing.Size(267, 25);
            this.txtTenKhoi.TabIndex = 0;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 590);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(990, 42);
            this.gxCommand1.TabIndex = 2;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "";
            this.gxCommand1.ToolTipOK = "";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.gxGroupBox1.Controls.Add(this.gxAddEdit1);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox1.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.gxGroupBox1.Location = new System.Drawing.Point(3, 16);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(984, 29);
            this.gxGroupBox1.TabIndex = 2;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode3;
            this.gxAddEdit1.GridData = this.gxLopGiaoLyList1;
            this.gxAddEdit1.Location = new System.Drawing.Point(9, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(213, 29);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm một lớp mới";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Xóa lớp được chọn";
            this.gxAddEdit1.ToolTipEdit = "Xem sửa lớp được chọn";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "";
            this.gxAddEdit1.ToolTipReload = "";
            this.gxAddEdit1.ToolTipSelect = "";
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.PrintClick += new System.EventHandler(this.btnInDanhSach_Click);
            // 
            // gxLopGiaoLyList1
            // 
            this.gxLopGiaoLyList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxLopGiaoLyList1.AllowEditGiaDinh = true;
            this.gxLopGiaoLyList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxLopGiaoLyList1.Arguments = null;
            this.gxLopGiaoLyList1.AutoLoadGridFormat = true;
            this.gxLopGiaoLyList1.ColumnAutoResize = true;
            gxLopGiaoLyList1_DesignTimeLayout.LayoutString = resources.GetString("gxLopGiaoLyList1_DesignTimeLayout.LayoutString");
            this.gxLopGiaoLyList1.DesignTimeLayout = gxLopGiaoLyList1_DesignTimeLayout;
            this.gxLopGiaoLyList1.DisableParentOnLoadData = true;
            this.gxLopGiaoLyList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxLopGiaoLyList1.DynamicFiltering = true;
            this.gxLopGiaoLyList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxLopGiaoLyList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxLopGiaoLyList1.GroupByBoxVisible = false;
            this.gxLopGiaoLyList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxLopGiaoLyList1.Location = new System.Drawing.Point(3, 45);
            this.gxLopGiaoLyList1.Name = "gxLopGiaoLyList1";
            this.gxLopGiaoLyList1.QueryString = "";
            this.gxLopGiaoLyList1.RecordNavigator = true;
            this.gxLopGiaoLyList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxLopGiaoLyList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxLopGiaoLyList1.Size = new System.Drawing.Size(984, 368);
            this.gxLopGiaoLyList1.TabIndex = 1;
            this.gxLopGiaoLyList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxLopGiaoLyList1.WhereSQL = "";
            this.gxLopGiaoLyList1.DoubleClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxLopGiaoLyList1);
            this.uiGroupBox2.Controls.Add(this.gxGroupBox1);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 174);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(990, 416);
            this.uiGroupBox2.TabIndex = 1;
            this.uiGroupBox2.Text = "Danh sách lớp giáo lý";
            // 
            // frmKhoiGiaoLy
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(990, 632);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "frmKhoiGiaoLy";
            this.Text = "Chi tiết khối giáo lý";
            this.Load += new System.EventHandler(this.frmKhoiGiaoLy_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxLopGiaoLyList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private GxControl.GxGiaoDan txtNguoiQuanLy;
        private GxControl.GxTextField txtGhiChu;
        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxTextField txtTenKhoi;
        private System.Windows.Forms.Label lblNam;
        private Janus.Windows.EditControls.UIComboBox cbNam;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxLopGiaoLyList gxLopGiaoLyList1;
        private GxControl.GxGroupBox uiGroupBox2;

    }
}

