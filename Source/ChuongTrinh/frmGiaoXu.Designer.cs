namespace GiaoXu
{
    partial class frmGiaoXu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGiaoXu));
            Janus.Windows.GridEX.GridEXLayout gxLinhMucList1_DesignTimeLayout = new Janus.Windows.GridEX.GridEXLayout();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtGhiChu = new System.Windows.Forms.RichTextBox();
            this.btnBrowse = new GxControl.GxButton();
            this.txtDienThoai = new GxControl.GxTextField();
            this.txtEmail = new GxControl.GxTextField();
            this.btnUpdate = new GxControl.GxButton();
            this.txtHinh = new GxControl.GxTextField();
            this.txtWebsite = new GxControl.GxTextField();
            this.txtDiaChi = new GxControl.GxTextField();
            this.txtTenGiaoPhan = new GxControl.GxTextField();
            this.txtTenGiaoHat = new GxControl.GxTextField();
            this.txtTenGiaoXu = new GxControl.GxTextField();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.gxLinhMucList1 = new GxControl.GxLinhMucList();
            this.uiGroupBox3 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxLinhMucList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).BeginInit();
            this.uiGroupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.txtGhiChu);
            this.uiGroupBox1.Controls.Add(this.btnBrowse);
            this.uiGroupBox1.Controls.Add(this.txtDienThoai);
            this.uiGroupBox1.Controls.Add(this.txtEmail);
            this.uiGroupBox1.Controls.Add(this.btnUpdate);
            this.uiGroupBox1.Controls.Add(this.txtHinh);
            this.uiGroupBox1.Controls.Add(this.txtWebsite);
            this.uiGroupBox1.Controls.Add(this.txtDiaChi);
            this.uiGroupBox1.Controls.Add(this.txtTenGiaoPhan);
            this.uiGroupBox1.Controls.Add(this.txtTenGiaoHat);
            this.uiGroupBox1.Controls.Add(this.txtTenGiaoXu);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(873, 272);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin giáo xứ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(478, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Ghi chú";
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGhiChu.HideSelection = false;
            this.txtGhiChu.Location = new System.Drawing.Point(481, 50);
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.Size = new System.Drawing.Size(380, 181);
            this.txtGhiChu.TabIndex = 10;
            this.txtGhiChu.Text = "";
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnBrowse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnBrowse.BackgroundImage")));
            this.btnBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBrowse.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnBrowse.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnBrowse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBrowse.Location = new System.Drawing.Point(390, 208);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 8;
            this.btnBrowse.Text = "&Chọn hình";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtDienThoai
            // 
            this.txtDienThoai.AutoCompleteEnabled = true;
            this.txtDienThoai.AutoUpperFirstChar = false;
            this.txtDienThoai.BoxLeft = 80;
            this.txtDienThoai.EditEnabled = true;
            this.txtDienThoai.Label = "Điện thoại";
            this.txtDienThoai.Location = new System.Drawing.Point(8, 175);
            this.txtDienThoai.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDienThoai.MaxLength = 20;
            this.txtDienThoai.MultiLine = false;
            this.txtDienThoai.Name = "txtDienThoai";
            this.txtDienThoai.NumberInputRequired = true;
            this.txtDienThoai.NumberMode = false;
            this.txtDienThoai.ReadOnly = false;
            this.txtDienThoai.Size = new System.Drawing.Size(190, 25);
            this.txtDienThoai.TabIndex = 5;
            // 
            // txtEmail
            // 
            this.txtEmail.AutoCompleteEnabled = true;
            this.txtEmail.AutoUpperFirstChar = false;
            this.txtEmail.BoxLeft = 40;
            this.txtEmail.EditEnabled = true;
            this.txtEmail.Label = "Email";
            this.txtEmail.Location = new System.Drawing.Point(216, 175);
            this.txtEmail.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEmail.MaxLength = 255;
            this.txtEmail.MultiLine = false;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.NumberInputRequired = true;
            this.txtEmail.NumberMode = false;
            this.txtEmail.ReadOnly = false;
            this.txtEmail.Size = new System.Drawing.Size(249, 25);
            this.txtEmail.TabIndex = 6;
            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnUpdate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUpdate.BackgroundImage")));
            this.btnUpdate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnUpdate.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnUpdate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnUpdate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnUpdate.Location = new System.Drawing.Point(90, 238);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 9;
            this.btnUpdate.Text = "Cập nhật";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // txtHinh
            // 
            this.txtHinh.AutoCompleteEnabled = true;
            this.txtHinh.AutoUpperFirstChar = false;
            this.txtHinh.BoxLeft = 80;
            this.txtHinh.EditEnabled = true;
            this.txtHinh.Label = "Hình giáo xứ";
            this.txtHinh.Location = new System.Drawing.Point(8, 206);
            this.txtHinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtHinh.MaxLength = 32767;
            this.txtHinh.MultiLine = false;
            this.txtHinh.Name = "txtHinh";
            this.txtHinh.NumberInputRequired = true;
            this.txtHinh.NumberMode = false;
            this.txtHinh.ReadOnly = true;
            this.txtHinh.Size = new System.Drawing.Size(376, 25);
            this.txtHinh.TabIndex = 7;
            // 
            // txtWebsite
            // 
            this.txtWebsite.AutoCompleteEnabled = true;
            this.txtWebsite.AutoUpperFirstChar = false;
            this.txtWebsite.BoxLeft = 80;
            this.txtWebsite.EditEnabled = true;
            this.txtWebsite.Label = "Website";
            this.txtWebsite.Location = new System.Drawing.Point(8, 144);
            this.txtWebsite.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtWebsite.MaxLength = 255;
            this.txtWebsite.MultiLine = false;
            this.txtWebsite.Name = "txtWebsite";
            this.txtWebsite.NumberInputRequired = true;
            this.txtWebsite.NumberMode = false;
            this.txtWebsite.ReadOnly = false;
            this.txtWebsite.Size = new System.Drawing.Size(457, 25);
            this.txtWebsite.TabIndex = 4;
            // 
            // txtDiaChi
            // 
            this.txtDiaChi.AutoCompleteEnabled = true;
            this.txtDiaChi.AutoUpperFirstChar = false;
            this.txtDiaChi.BoxLeft = 80;
            this.txtDiaChi.EditEnabled = true;
            this.txtDiaChi.Label = "Địa chỉ";
            this.txtDiaChi.Location = new System.Drawing.Point(8, 113);
            this.txtDiaChi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDiaChi.MaxLength = 255;
            this.txtDiaChi.MultiLine = false;
            this.txtDiaChi.Name = "txtDiaChi";
            this.txtDiaChi.NumberInputRequired = true;
            this.txtDiaChi.NumberMode = false;
            this.txtDiaChi.ReadOnly = false;
            this.txtDiaChi.Size = new System.Drawing.Size(457, 25);
            this.txtDiaChi.TabIndex = 3;
            // 
            // txtTenGiaoPhan
            // 
            this.txtTenGiaoPhan.AutoCompleteEnabled = true;
            this.txtTenGiaoPhan.AutoUpperFirstChar = false;
            this.txtTenGiaoPhan.BoxLeft = 80;
            this.txtTenGiaoPhan.EditEnabled = true;
            this.txtTenGiaoPhan.Label = "Tên giáo phận";
            this.txtTenGiaoPhan.Location = new System.Drawing.Point(8, 19);
            this.txtTenGiaoPhan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenGiaoPhan.MaxLength = 255;
            this.txtTenGiaoPhan.MultiLine = false;
            this.txtTenGiaoPhan.Name = "txtTenGiaoPhan";
            this.txtTenGiaoPhan.NumberInputRequired = true;
            this.txtTenGiaoPhan.NumberMode = false;
            this.txtTenGiaoPhan.ReadOnly = false;
            this.txtTenGiaoPhan.Size = new System.Drawing.Size(458, 25);
            this.txtTenGiaoPhan.TabIndex = 0;
            // 
            // txtTenGiaoHat
            // 
            this.txtTenGiaoHat.AutoCompleteEnabled = true;
            this.txtTenGiaoHat.AutoUpperFirstChar = false;
            this.txtTenGiaoHat.BoxLeft = 80;
            this.txtTenGiaoHat.EditEnabled = true;
            this.txtTenGiaoHat.Label = "Tên giáo hạt";
            this.txtTenGiaoHat.Location = new System.Drawing.Point(8, 50);
            this.txtTenGiaoHat.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenGiaoHat.MaxLength = 255;
            this.txtTenGiaoHat.MultiLine = false;
            this.txtTenGiaoHat.Name = "txtTenGiaoHat";
            this.txtTenGiaoHat.NumberInputRequired = true;
            this.txtTenGiaoHat.NumberMode = false;
            this.txtTenGiaoHat.ReadOnly = false;
            this.txtTenGiaoHat.Size = new System.Drawing.Size(457, 25);
            this.txtTenGiaoHat.TabIndex = 1;
            // 
            // txtTenGiaoXu
            // 
            this.txtTenGiaoXu.AutoCompleteEnabled = true;
            this.txtTenGiaoXu.AutoUpperFirstChar = false;
            this.txtTenGiaoXu.BoxLeft = 80;
            this.txtTenGiaoXu.EditEnabled = true;
            this.txtTenGiaoXu.Label = "Tên giáo xứ";
            this.txtTenGiaoXu.Location = new System.Drawing.Point(8, 81);
            this.txtTenGiaoXu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenGiaoXu.MaxLength = 255;
            this.txtTenGiaoXu.MultiLine = false;
            this.txtTenGiaoXu.Name = "txtTenGiaoXu";
            this.txtTenGiaoXu.NumberInputRequired = true;
            this.txtTenGiaoXu.NumberMode = false;
            this.txtTenGiaoXu.ReadOnly = false;
            this.txtTenGiaoXu.Size = new System.Drawing.Size(457, 25);
            this.txtTenGiaoXu.TabIndex = 2;
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxLinhMucList1);
            this.uiGroupBox2.Controls.Add(this.uiGroupBox3);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 272);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(873, 260);
            this.uiGroupBox2.TabIndex = 0;
            this.uiGroupBox2.Text = "Danh sách các cha quản xứ";
            // 
            // gxLinhMucList1
            // 
            this.gxLinhMucList1.AllowColumnDrag = false;
            this.gxLinhMucList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxLinhMucList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxLinhMucList1.Arguments = null;
            this.gxLinhMucList1.AutoLoadGridFormat = false;
            this.gxLinhMucList1.ColumnAutoResize = true;
            gxLinhMucList1_DesignTimeLayout.LayoutString = resources.GetString("gxLinhMucList1_DesignTimeLayout.LayoutString");
            this.gxLinhMucList1.DesignTimeLayout = gxLinhMucList1_DesignTimeLayout;
            this.gxLinhMucList1.DisableParentOnLoadData = true;
            this.gxLinhMucList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxLinhMucList1.DynamicFiltering = true;
            this.gxLinhMucList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxLinhMucList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxLinhMucList1.GroupByBoxVisible = false;
            this.gxLinhMucList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxLinhMucList1.KeepRowSettings = true;
            this.gxLinhMucList1.Location = new System.Drawing.Point(3, 49);
            this.gxLinhMucList1.Name = "gxLinhMucList1";
            this.gxLinhMucList1.QueryString = "SELECT * FROM LinhMuc WHERE DaXoa = 0 ";
            this.gxLinhMucList1.RecordNavigator = true;
            this.gxLinhMucList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxLinhMucList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxLinhMucList1.Size = new System.Drawing.Size(867, 208);
            this.gxLinhMucList1.TabIndex = 0;
            this.gxLinhMucList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxLinhMucList1.WhereSQL = "";
            this.gxLinhMucList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gxLinhMucList1_RowDoubleClick);
            this.gxLinhMucList1.RowCountChanged += new System.EventHandler(this.gxLinhMucList1_RowCountChanged);
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.uiGroupBox3.Controls.Add(this.gxAddEdit1);
            this.uiGroupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox3.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.uiGroupBox3.Location = new System.Drawing.Point(3, 16);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Size = new System.Drawing.Size(867, 33);
            this.uiGroupBox3.TabIndex = 3;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = this.gxLinhMucList1;
            this.gxAddEdit1.Location = new System.Drawing.Point(5, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(391, 29);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Xóa";
            this.gxAddEdit1.ToolTipEdit = "Sửa";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "";
            this.gxAddEdit1.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
    "ếm";
            this.gxAddEdit1.ToolTipSelect = "Chọn";
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.Load += new System.EventHandler(this.gxAddEdit1_Load);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "JPG (*.JPG)|*.jpg|GIF (*.GIF)|*.gif|BITMAP (*.BMP)|*.bmp|All files (*.*)|*.*";
            // 
            // frmGiaoXu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 532);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "frmGiaoXu";
            this.Text = "Nhập thông tin giáo xứ";
            this.Load += new System.EventHandler(this.frmGiaoXu_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxLinhMucList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).EndInit();
            this.uiGroupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxTextField txtDiaChi;
        private GxControl.GxTextField txtTenGiaoXu;
        private GxControl.GxButton btnUpdate;
        private GxControl.GxLinhMucList gxLinhMucList1;
        private GxControl.GxGroupBox uiGroupBox3;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxTextField txtHinh;
        private GxControl.GxTextField txtWebsite;
        private GxControl.GxTextField txtDienThoai;
        private GxControl.GxTextField txtEmail;
        private GxControl.GxButton btnBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private GxControl.GxTextField txtTenGiaoPhan;
        private GxControl.GxTextField txtTenGiaoHat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox txtGhiChu;
    }
}