namespace GiaoXu
{
    partial class frmBiTichChiTiet
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBiTichChiTiet));
            this.dtNgayBiTich = new GxControl.GxDateField();
            this.txtMoTa = new GxControl.GxTextField();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNoiBiTich = new GxControl.GxTextField();
            this.txtMaDotBiTich = new GxControl.GxTextField();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.gxBiTichChiTiet1 = new GxControl.GxBiTichChiTiet();
            this.uiGroupBox3 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.gxCommand1 = new GxControl.GxCommand();
            this.txtLinhMuc = new GxControl.GxTextField();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxBiTichChiTiet1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).BeginInit();
            this.uiGroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // dtNgayBiTich
            // 
            this.dtNgayBiTich.AutoUpperFirstChar = false;
            this.dtNgayBiTich.BoxLeft = 100;
            this.dtNgayBiTich.EditEnabled = true;
            this.dtNgayBiTich.FullInputRequired = false;
            this.dtNgayBiTich.IsNullDate = false;
            this.dtNgayBiTich.Label = "Ngày bí tích";
            this.dtNgayBiTich.Location = new System.Drawing.Point(6, 21);
            this.dtNgayBiTich.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayBiTich.Name = "dtNgayBiTich";
            this.dtNgayBiTich.Size = new System.Drawing.Size(218, 26);
            this.dtNgayBiTich.TabIndex = 1;
            this.dtNgayBiTich.Value = "06/04/2009";
            // 
            // txtMoTa
            // 
            this.txtMoTa.AutoCompleteEnabled = true;
            this.txtMoTa.AutoUpperFirstChar = false;
            this.txtMoTa.BoxLeft = 100;
            this.txtMoTa.EditEnabled = true;
            this.txtMoTa.Label = "Mô tả";
            this.txtMoTa.Location = new System.Drawing.Point(6, 52);
            this.txtMoTa.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMoTa.MaxLength = 255;
            this.txtMoTa.MultiLine = false;
            this.txtMoTa.Name = "txtMoTa";
            this.txtMoTa.NumberInputRequired = true;
            this.txtMoTa.NumberMode = false;
            this.txtMoTa.ReadOnly = false;
            this.txtMoTa.Size = new System.Drawing.Size(557, 25);
            this.txtMoTa.TabIndex = 2;
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.txtLinhMuc);
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.dtNgayBiTich);
            this.uiGroupBox1.Controls.Add(this.txtNoiBiTich);
            this.uiGroupBox1.Controls.Add(this.txtMoTa);
            this.uiGroupBox1.Controls.Add(this.txtMaDotBiTich);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(875, 155);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin đợt bí tích";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(652, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 118);
            this.label1.TabIndex = 6;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // txtNoiBiTich
            // 
            this.txtNoiBiTich.AutoCompleteEnabled = true;
            this.txtNoiBiTich.AutoUpperFirstChar = false;
            this.txtNoiBiTich.BoxLeft = 100;
            this.txtNoiBiTich.EditEnabled = true;
            this.txtNoiBiTich.Label = "Nơi nhận bí tích";
            this.txtNoiBiTich.Location = new System.Drawing.Point(5, 119);
            this.txtNoiBiTich.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNoiBiTich.MaxLength = 255;
            this.txtNoiBiTich.MultiLine = false;
            this.txtNoiBiTich.Name = "txtNoiBiTich";
            this.txtNoiBiTich.NumberInputRequired = true;
            this.txtNoiBiTich.NumberMode = false;
            this.txtNoiBiTich.ReadOnly = false;
            this.txtNoiBiTich.Size = new System.Drawing.Size(558, 25);
            this.txtNoiBiTich.TabIndex = 4;
            // 
            // txtMaDotBiTich
            // 
            this.txtMaDotBiTich.AutoCompleteEnabled = true;
            this.txtMaDotBiTich.AutoUpperFirstChar = false;
            this.txtMaDotBiTich.BoxLeft = 100;
            this.txtMaDotBiTich.EditEnabled = false;
            this.txtMaDotBiTich.Label = "Mã đợt bí tích";
            this.txtMaDotBiTich.Location = new System.Drawing.Point(8, 22);
            this.txtMaDotBiTich.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMaDotBiTich.MaxLength = 32767;
            this.txtMaDotBiTich.MultiLine = false;
            this.txtMaDotBiTich.Name = "txtMaDotBiTich";
            this.txtMaDotBiTich.NumberInputRequired = true;
            this.txtMaDotBiTich.NumberMode = false;
            this.txtMaDotBiTich.ReadOnly = false;
            this.txtMaDotBiTich.Size = new System.Drawing.Size(193, 25);
            this.txtMaDotBiTich.TabIndex = 0;
            this.txtMaDotBiTich.Visible = false;
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxBiTichChiTiet1);
            this.uiGroupBox2.Controls.Add(this.uiGroupBox3);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 155);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(875, 446);
            this.uiGroupBox2.TabIndex = 1;
            this.uiGroupBox2.Text = "Danh sách người chịu bí tích";
            // 
            // gxBiTichChiTiet1
            // 
            this.gxBiTichChiTiet1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxBiTichChiTiet1.AlternatingColors = true;
            this.gxBiTichChiTiet1.Arguments = null;
            this.gxBiTichChiTiet1.AutoLoadGridFormat = true;
            this.gxBiTichChiTiet1.ColumnAutoResize = true;
            this.gxBiTichChiTiet1.DisableParentOnLoadData = true;
            this.gxBiTichChiTiet1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxBiTichChiTiet1.DynamicFiltering = true;
            this.gxBiTichChiTiet1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxBiTichChiTiet1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.gxBiTichChiTiet1.GroupByBoxVisible = false;
            this.gxBiTichChiTiet1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxBiTichChiTiet1.LinhMucColumnName = "";
            this.gxBiTichChiTiet1.LoaiBiTich = GxGlobal.LoaiBiTich.RuaToi;
            this.gxBiTichChiTiet1.Location = new System.Drawing.Point(3, 49);
            this.gxBiTichChiTiet1.MaDotBiTich = -1;
            this.gxBiTichChiTiet1.Name = "gxBiTichChiTiet1";
            this.gxBiTichChiTiet1.NgayBiTichColumnName = "";
            this.gxBiTichChiTiet1.NguoiDoDauColumnName = "";
            this.gxBiTichChiTiet1.NoiBiTichColumnName = "";
            this.gxBiTichChiTiet1.QueryString = "";
            this.gxBiTichChiTiet1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxBiTichChiTiet1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxBiTichChiTiet1.Size = new System.Drawing.Size(869, 394);
            this.gxBiTichChiTiet1.SoBiTichColumnName = "";
            this.gxBiTichChiTiet1.SoBiTichColumnText = "";
            this.gxBiTichChiTiet1.TabIndex = 2;
            this.gxBiTichChiTiet1.TenBiTich = "";
            this.gxBiTichChiTiet1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxBiTichChiTiet1.WhereSQL = "";
            this.gxBiTichChiTiet1.SelectionChanged += new System.EventHandler(this.gxDotBiTichChiTiet1_SelectionChanged);
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.uiGroupBox3.Controls.Add(this.gxAddEdit1);
            this.uiGroupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox3.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.uiGroupBox3.Location = new System.Drawing.Point(3, 16);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Size = new System.Drawing.Size(869, 33);
            this.uiGroupBox3.TabIndex = 1;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode1;
            this.gxAddEdit1.GridData = this.gxBiTichChiTiet1;
            this.gxAddEdit1.Location = new System.Drawing.Point(5, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(392, 29);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm một người con chưa có trong danh sách giáo dân";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Xóa người con được chọn";
            this.gxAddEdit1.ToolTipEdit = "Sửa người con được chọn";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "";
            this.gxAddEdit1.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
    "ếm";
            this.gxAddEdit1.ToolTipSelect = "Thêm một người con được chọn từ danh sách giáo dân có sẵn";
            this.gxAddEdit1.SelectClick += new System.EventHandler(this.gxAddEdit1_SelectClick);
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.Button1Click += new System.EventHandler(this.gxAddEdit1_Button1Click);
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 601);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(875, 45);
            this.gxCommand1.TabIndex = 2;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "";
            this.gxCommand1.ToolTipOK = "";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            // 
            // txtLinhMuc
            // 
            this.txtLinhMuc.AutoCompleteEnabled = true;
            this.txtLinhMuc.AutoUpperFirstChar = false;
            this.txtLinhMuc.BoxLeft = 0;
            this.txtLinhMuc.EditEnabled = true;
            this.txtLinhMuc.Label = "Người ban bí tích";
            this.txtLinhMuc.Location = new System.Drawing.Point(17, 85);
            this.txtLinhMuc.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtLinhMuc.MaxLength = 32767;
            this.txtLinhMuc.MultiLine = false;
            this.txtLinhMuc.Name = "txtLinhMuc";
            this.txtLinhMuc.NumberInputRequired = true;
            this.txtLinhMuc.NumberMode = false;
            this.txtLinhMuc.ReadOnly = false;
            this.txtLinhMuc.Size = new System.Drawing.Size(546, 26);
            this.txtLinhMuc.TabIndex = 3;
            // 
            // frmBiTichChiTiet
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(875, 646);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "frmBiTichChiTiet";
            this.Text = "Sổ bí tích chi tiết";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxBiTichChiTiet1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).EndInit();
            this.uiGroupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private GxControl.GxDateField dtNgayBiTich;
        private GxControl.GxTextField txtMoTa;
        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxGroupBox uiGroupBox3;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxTextField txtMaDotBiTich;
        private GxControl.GxBiTichChiTiet gxBiTichChiTiet1;
        private GxControl.GxTextField txtNoiBiTich;
        private System.Windows.Forms.Label label1;
        private GxControl.GxTextField txtLinhMuc;
    }
}

