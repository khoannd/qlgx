namespace GxControl
{
    partial class frmGiaDinh
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
            this.components = new System.ComponentModel.Container();
            Janus.Windows.GridEX.GridEXLayout gxGiaoDanList1_DesignTimeLayout = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGiaDinh));
            this.lblGhiChu = new System.Windows.Forms.Label();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.gxCommand1 = new GxControl.GxCommand();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbDienGiaDinh = new GxControl.GxDienGiaDinh();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtSoHoKhau = new GxControl.GxTextField();
            this.gxPictureField1 = new GxControl.GxPictureField();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdChuHoNu = new GxControl.GxRadioBox();
            this.rdChuHoNam = new GxControl.GxRadioBox();
            this.txtGhiChu = new GxControl.GxTextField();
            this.gxXemHonPhoi1 = new GxControl.GxHonPhoiGiaDinh();
            this.chkDelete = new GxControl.GxCheckBox();
            this.txtNoiChuyen = new GxControl.GxTextField();
            this.chkGiaDinhAo = new GxControl.GxCheckBox();
            this.chkChuyenXu = new GxControl.GxCheckBox();
            this.txtMaGiaDinhRieng = new GxControl.GxTextField();
            this.txtMaGiaDinh = new GxControl.GxTextField();
            this.cbGiaoHo = new GxControl.GxGiaoHo();
            this.txtTenGiaDinh = new GxControl.GxTextField();
            this.txtNguoiChong = new GxControl.GxGiaoDan();
            this.txtNguoiVo = new GxControl.GxGiaoDan();
            this.txtDienThoai = new GxControl.GxTextField();
            this.txtDiaChi = new GxControl.GxTextField();
            this.dtNgayChuyen = new GxControl.GxDateField();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblGhiChu
            // 
            this.lblGhiChu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblGhiChu.AutoSize = true;
            this.lblGhiChu.Location = new System.Drawing.Point(2, 632);
            this.lblGhiChu.Name = "lblGhiChu";
            this.lblGhiChu.Size = new System.Drawing.Size(421, 13);
            this.lblGhiChu.TabIndex = 3;
            this.lblGhiChu.Text = "* Ghi chú: Thành viên bị gạch ngang là người đã qua đời, chuyển xứ hoặc lập GĐ ri" +
    "êng";
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxGiaoDanList1);
            this.uiGroupBox2.Controls.Add(this.gxGroupBox1);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 395);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(897, 219);
            this.uiGroupBox2.TabIndex = 1;
            this.uiGroupBox2.Text = "Các thành viên khác trong gia đình";
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
            gxGiaoDanList1_DesignTimeLayout.LayoutString = resources.GetString("gxGiaoDanList1_DesignTimeLayout.LayoutString");
            this.gxGiaoDanList1.DesignTimeLayout = gxGiaoDanList1_DesignTimeLayout;
            this.gxGiaoDanList1.DisableParentOnLoadData = true;
            this.gxGiaoDanList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGiaoDanList1.DynamicFiltering = true;
            this.gxGiaoDanList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaoDanList1.Font = new System.Drawing.Font("Arial", 8.25F);
            this.gxGiaoDanList1.GroupByBoxVisible = false;
            this.gxGiaoDanList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaoDanList1.Location = new System.Drawing.Point(3, 45);
            this.gxGiaoDanList1.Name = "gxGiaoDanList1";
            this.gxGiaoDanList1.QueryString = "";
            this.gxGiaoDanList1.RecordNavigator = true;
            this.gxGiaoDanList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoDanList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.SelectionMode = Janus.Windows.GridEX.SelectionMode.MultipleSelection;
            this.gxGiaoDanList1.Size = new System.Drawing.Size(891, 171);
            this.gxGiaoDanList1.TabIndex = 1;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            this.gxGiaoDanList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gxGiaoDanList1_RowDoubleClick);
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.gxGroupBox1.Controls.Add(this.gxAddEdit1);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox1.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.gxGroupBox1.Location = new System.Drawing.Point(3, 16);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(891, 29);
            this.gxGroupBox1.TabIndex = 2;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = this.gxGiaoDanList1;
            this.gxAddEdit1.Location = new System.Drawing.Point(9, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(391, 29);
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
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 614);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(897, 50);
            this.gxCommand1.TabIndex = 2;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "";
            this.gxCommand1.ToolTipOK = "";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.BackColor = System.Drawing.Color.AliceBlue;
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.cbDienGiaDinh);
            this.uiGroupBox1.Controls.Add(this.pictureBox1);
            this.uiGroupBox1.Controls.Add(this.txtSoHoKhau);
            this.uiGroupBox1.Controls.Add(this.gxPictureField1);
            this.uiGroupBox1.Controls.Add(this.panel1);
            this.uiGroupBox1.Controls.Add(this.txtGhiChu);
            this.uiGroupBox1.Controls.Add(this.gxXemHonPhoi1);
            this.uiGroupBox1.Controls.Add(this.chkDelete);
            this.uiGroupBox1.Controls.Add(this.txtNoiChuyen);
            this.uiGroupBox1.Controls.Add(this.chkGiaDinhAo);
            this.uiGroupBox1.Controls.Add(this.chkChuyenXu);
            this.uiGroupBox1.Controls.Add(this.txtMaGiaDinhRieng);
            this.uiGroupBox1.Controls.Add(this.txtMaGiaDinh);
            this.uiGroupBox1.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox1.Controls.Add(this.txtTenGiaDinh);
            this.uiGroupBox1.Controls.Add(this.txtNguoiChong);
            this.uiGroupBox1.Controls.Add(this.txtNguoiVo);
            this.uiGroupBox1.Controls.Add(this.txtDienThoai);
            this.uiGroupBox1.Controls.Add(this.txtDiaChi);
            this.uiGroupBox1.Controls.Add(this.dtNgayChuyen);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(897, 395);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin gia đình";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(513, 212);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Hình gia đình";
            // 
            // cbDienGiaDinh
            // 
            this.cbDienGiaDinh.AutoUpperFirstChar = false;
            this.cbDienGiaDinh.BoxLeft = 40;
            this.cbDienGiaDinh.EditEnabled = true;
            this.cbDienGiaDinh.Label = "Diện";
            this.cbDienGiaDinh.Location = new System.Drawing.Point(185, 157);
            this.cbDienGiaDinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbDienGiaDinh.MaxLength = 0;
            this.cbDienGiaDinh.Name = "cbDienGiaDinh";
            this.cbDienGiaDinh.SelectedIndex = -1;
            this.cbDienGiaDinh.SelectedText = "";
            this.cbDienGiaDinh.SelectedValue = null;
            this.cbDienGiaDinh.Size = new System.Drawing.Size(261, 26);
            this.cbDienGiaDinh.TabIndex = 9;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GxControl.Properties.Resources.map;
            this.pictureBox1.Location = new System.Drawing.Point(446, 184);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(27, 22);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "Xem bản đồ");
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // txtSoHoKhau
            // 
            this.txtSoHoKhau.AutoCompleteEnabled = false;
            this.txtSoHoKhau.AutoUpperFirstChar = true;
            this.txtSoHoKhau.BoxLeft = 80;
            this.txtSoHoKhau.EditEnabled = true;
            this.txtSoHoKhau.Label = "Số hộ khẩu";
            this.txtSoHoKhau.Location = new System.Drawing.Point(7, 157);
            this.txtSoHoKhau.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSoHoKhau.MaxLength = 255;
            this.txtSoHoKhau.MultiLine = false;
            this.txtSoHoKhau.Name = "txtSoHoKhau";
            this.txtSoHoKhau.NumberInputRequired = true;
            this.txtSoHoKhau.NumberMode = false;
            this.txtSoHoKhau.ReadOnly = false;
            this.txtSoHoKhau.Size = new System.Drawing.Size(177, 25);
            this.txtSoHoKhau.TabIndex = 8;
            // 
            // gxPictureField1
            // 
            this.gxPictureField1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gxPictureField1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gxPictureField1.ImagePicture = global::GxControl.Properties.Resources.family_hope_cntr_icon;
            this.gxPictureField1.Location = new System.Drawing.Point(587, 212);
            this.gxPictureField1.Margin = new System.Windows.Forms.Padding(4);
            this.gxPictureField1.Name = "gxPictureField1";
            this.gxPictureField1.Size = new System.Drawing.Size(292, 169);
            this.gxPictureField1.TabIndex = 18;
            this.gxPictureField1.TenManHinh = "Gia đình";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdChuHoNu);
            this.panel1.Controls.Add(this.rdChuHoNam);
            this.panel1.Location = new System.Drawing.Point(378, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(86, 54);
            this.panel1.TabIndex = 19;
            // 
            // rdChuHoNu
            // 
            this.rdChuHoNu.AutoSize = true;
            this.rdChuHoNu.Location = new System.Drawing.Point(6, 34);
            this.rdChuHoNu.Name = "rdChuHoNu";
            this.rdChuHoNu.Size = new System.Drawing.Size(59, 17);
            this.rdChuHoNu.TabIndex = 5;
            this.rdChuHoNu.TabStop = true;
            this.rdChuHoNu.Text = "Chủ hộ";
            this.rdChuHoNu.UseVisualStyleBackColor = true;
            this.rdChuHoNu.CheckedChanged += new System.EventHandler(this.rdChuHoNu_CheckedChanged);
            // 
            // rdChuHoNam
            // 
            this.rdChuHoNam.AutoSize = true;
            this.rdChuHoNam.Location = new System.Drawing.Point(6, 7);
            this.rdChuHoNam.Name = "rdChuHoNam";
            this.rdChuHoNam.Size = new System.Drawing.Size(59, 17);
            this.rdChuHoNam.TabIndex = 3;
            this.rdChuHoNam.TabStop = true;
            this.rdChuHoNam.Text = "Chủ hộ";
            this.rdChuHoNam.UseVisualStyleBackColor = true;
            this.rdChuHoNam.CheckedChanged += new System.EventHandler(this.rdChuHoNam_CheckedChanged);
            // 
            // txtGhiChu
            // 
            this.txtGhiChu.AutoCompleteEnabled = true;
            this.txtGhiChu.AutoUpperFirstChar = false;
            this.txtGhiChu.BoxLeft = 80;
            this.txtGhiChu.EditEnabled = true;
            this.txtGhiChu.Label = "Ghi chú";
            this.txtGhiChu.Location = new System.Drawing.Point(7, 210);
            this.txtGhiChu.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtGhiChu.MaxLength = 32767;
            this.txtGhiChu.MultiLine = true;
            this.txtGhiChu.Name = "txtGhiChu";
            this.txtGhiChu.NumberInputRequired = true;
            this.txtGhiChu.NumberMode = false;
            this.txtGhiChu.ReadOnly = false;
            this.txtGhiChu.Size = new System.Drawing.Size(440, 71);
            this.txtGhiChu.TabIndex = 12;
            // 
            // gxXemHonPhoi1
            // 
            this.gxXemHonPhoi1.CurrentRow = null;
            this.gxXemHonPhoi1.Location = new System.Drawing.Point(484, 10);
            this.gxXemHonPhoi1.MaHonPhoi = -1;
            this.gxXemHonPhoi1.Name = "gxXemHonPhoi1";
            this.gxXemHonPhoi1.NguoiChong = -1;
            this.gxXemHonPhoi1.NguoiVo = -1;
            this.gxXemHonPhoi1.Operation = GxGlobal.GxOperation.NONE;
            this.gxXemHonPhoi1.Size = new System.Drawing.Size(401, 198);
            this.gxXemHonPhoi1.SoThuTu = 1;
            this.gxXemHonPhoi1.TabIndex = 17;
            this.gxXemHonPhoi1.TenHonPhoi = "";
            // 
            // chkDelete
            // 
            this.chkDelete.AutoSize = true;
            this.chkDelete.Location = new System.Drawing.Point(190, 25);
            this.chkDelete.Name = "chkDelete";
            this.chkDelete.Size = new System.Drawing.Size(60, 17);
            this.chkDelete.TabIndex = 1;
            this.chkDelete.Text = "Đã xóa";
            this.chkDelete.UseVisualStyleBackColor = true;
            this.chkDelete.Visible = false;
            // 
            // txtNoiChuyen
            // 
            this.txtNoiChuyen.AutoCompleteEnabled = true;
            this.txtNoiChuyen.AutoUpperFirstChar = true;
            this.txtNoiChuyen.BoxLeft = 80;
            this.txtNoiChuyen.EditEnabled = true;
            this.txtNoiChuyen.Label = "Nơi chuyển";
            this.txtNoiChuyen.Location = new System.Drawing.Point(7, 333);
            this.txtNoiChuyen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNoiChuyen.MaxLength = 255;
            this.txtNoiChuyen.MultiLine = false;
            this.txtNoiChuyen.Name = "txtNoiChuyen";
            this.txtNoiChuyen.NumberInputRequired = true;
            this.txtNoiChuyen.NumberMode = false;
            this.txtNoiChuyen.ReadOnly = false;
            this.txtNoiChuyen.Size = new System.Drawing.Size(436, 25);
            this.txtNoiChuyen.TabIndex = 15;
            this.txtNoiChuyen.Visible = false;
            // 
            // chkGiaDinhAo
            // 
            this.chkGiaDinhAo.AutoSize = true;
            this.chkGiaDinhAo.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.chkGiaDinhAo.Location = new System.Drawing.Point(257, 288);
            this.chkGiaDinhAo.Name = "chkGiaDinhAo";
            this.chkGiaDinhAo.Size = new System.Drawing.Size(185, 17);
            this.chkGiaDinhAo.TabIndex = 16;
            this.chkGiaDinhAo.Text = "Là gia đình không được thống kê";
            this.chkGiaDinhAo.UseVisualStyleBackColor = true;
            this.chkGiaDinhAo.CheckedChanged += new System.EventHandler(this.chkGiaDinhAo_CheckedChanged);
            // 
            // chkChuyenXu
            // 
            this.chkChuyenXu.AutoSize = true;
            this.chkChuyenXu.Location = new System.Drawing.Point(90, 288);
            this.chkChuyenXu.Name = "chkChuyenXu";
            this.chkChuyenXu.Size = new System.Drawing.Size(131, 17);
            this.chkChuyenXu.TabIndex = 13;
            this.chkChuyenXu.Text = "Đã chuyển đi xứ khác";
            this.chkChuyenXu.UseVisualStyleBackColor = true;
            this.chkChuyenXu.CheckedChanged += new System.EventHandler(this.chkChuyenXu_CheckedChanged);
            // 
            // txtMaGiaDinhRieng
            // 
            this.txtMaGiaDinhRieng.AutoCompleteEnabled = false;
            this.txtMaGiaDinhRieng.AutoUpperFirstChar = false;
            this.txtMaGiaDinhRieng.BoxLeft = 80;
            this.txtMaGiaDinhRieng.EditEnabled = true;
            this.txtMaGiaDinhRieng.Label = "Mã gia đình";
            this.txtMaGiaDinhRieng.Location = new System.Drawing.Point(6, 21);
            this.txtMaGiaDinhRieng.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMaGiaDinhRieng.MaxLength = 255;
            this.txtMaGiaDinhRieng.MultiLine = false;
            this.txtMaGiaDinhRieng.Name = "txtMaGiaDinhRieng";
            this.txtMaGiaDinhRieng.NumberInputRequired = false;
            this.txtMaGiaDinhRieng.NumberMode = false;
            this.txtMaGiaDinhRieng.ReadOnly = false;
            this.txtMaGiaDinhRieng.Size = new System.Drawing.Size(178, 25);
            this.txtMaGiaDinhRieng.TabIndex = 0;
            // 
            // txtMaGiaDinh
            // 
            this.txtMaGiaDinh.AutoCompleteEnabled = true;
            this.txtMaGiaDinh.AutoUpperFirstChar = false;
            this.txtMaGiaDinh.BoxLeft = 80;
            this.txtMaGiaDinh.EditEnabled = false;
            this.txtMaGiaDinh.Label = "Mã gia đình";
            this.txtMaGiaDinh.Location = new System.Drawing.Point(6, 21);
            this.txtMaGiaDinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMaGiaDinh.MaxLength = 9;
            this.txtMaGiaDinh.MultiLine = false;
            this.txtMaGiaDinh.Name = "txtMaGiaDinh";
            this.txtMaGiaDinh.NumberInputRequired = true;
            this.txtMaGiaDinh.NumberMode = false;
            this.txtMaGiaDinh.ReadOnly = false;
            this.txtMaGiaDinh.Size = new System.Drawing.Size(178, 25);
            this.txtMaGiaDinh.TabIndex = 0;
            // 
            // cbGiaoHo
            // 
            this.cbGiaoHo.AutoLoadGrid = true;
            this.cbGiaoHo.AutoUpperFirstChar = false;
            this.cbGiaoHo.BoxLeft = 80;
            this.cbGiaoHo.EditEnabled = true;
            this.cbGiaoHo.GridGiaDinh = null;
            this.cbGiaoHo.GridGiaoDan = null;
            this.cbGiaoHo.HasShowAll = false;
            this.cbGiaoHo.IsAo = Janus.Windows.GridEX.TriState.Empty;
            this.cbGiaoHo.IsLuuTru = false;
            this.cbGiaoHo.Label = "Giáo họ";
            this.cbGiaoHo.Location = new System.Drawing.Point(6, 131);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = true;
            this.cbGiaoHo.Size = new System.Drawing.Size(440, 25);
            this.cbGiaoHo.TabIndex = 7;
            this.cbGiaoHo.WhereSQL = "";
            // 
            // txtTenGiaDinh
            // 
            this.txtTenGiaDinh.AutoCompleteEnabled = false;
            this.txtTenGiaDinh.AutoUpperFirstChar = true;
            this.txtTenGiaDinh.BoxLeft = 80;
            this.txtTenGiaDinh.EditEnabled = true;
            this.txtTenGiaDinh.Label = "Tên gia đình";
            this.txtTenGiaDinh.Location = new System.Drawing.Point(6, 104);
            this.txtTenGiaDinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenGiaDinh.MaxLength = 255;
            this.txtTenGiaDinh.MultiLine = false;
            this.txtTenGiaDinh.Name = "txtTenGiaDinh";
            this.txtTenGiaDinh.NumberInputRequired = true;
            this.txtTenGiaDinh.NumberMode = false;
            this.txtTenGiaDinh.ReadOnly = false;
            this.txtTenGiaDinh.Size = new System.Drawing.Size(270, 25);
            this.txtTenGiaDinh.TabIndex = 6;
            // 
            // txtNguoiChong
            // 
            this.txtNguoiChong.AutoCompleteEnabled = true;
            this.txtNguoiChong.AutoUpperFirstChar = true;
            this.txtNguoiChong.BoxLeft = 80;
            this.txtNguoiChong.CheckQDGXGD = false;
            this.txtNguoiChong.CurrentRow = null;
            this.txtNguoiChong.DisplayMode = GxGlobal.DisplayMode.Full;
            this.txtNguoiChong.EditEnabled = true;
            this.txtNguoiChong.Label = "Người nam";
            this.txtNguoiChong.Location = new System.Drawing.Point(6, 47);
            this.txtNguoiChong.MaGiaoDan = -1;
            this.txtNguoiChong.MaGiaoHo = -1;
            this.txtNguoiChong.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNguoiChong.MaxLength = 255;
            this.txtNguoiChong.MultiLine = false;
            this.txtNguoiChong.Name = "txtNguoiChong";
            this.txtNguoiChong.NumberInputRequired = true;
            this.txtNguoiChong.NumberMode = false;
            this.txtNguoiChong.ReadOnly = true;
            this.txtNguoiChong.Size = new System.Drawing.Size(368, 26);
            this.txtNguoiChong.TabIndex = 2;
            this.txtNguoiChong.WhereSQL = "";
            this.txtNguoiChong.DeleteButton += new System.EventHandler(this.txtNguoiChong_DeleteButton);
            this.txtNguoiChong.OnSelecting += new GxControl.GxGiaoDan.SelectingHandler(this.txtNguoiChong_OnSelecting);
            this.txtNguoiChong.OnAdding += new GxControl.GxGiaoDan.SelectingHandler(this.txtNguoiChong_OnSelecting);
            // 
            // txtNguoiVo
            // 
            this.txtNguoiVo.AutoCompleteEnabled = true;
            this.txtNguoiVo.AutoUpperFirstChar = true;
            this.txtNguoiVo.BoxLeft = 80;
            this.txtNguoiVo.CheckQDGXGD = false;
            this.txtNguoiVo.CurrentRow = null;
            this.txtNguoiVo.DisplayMode = GxGlobal.DisplayMode.Full;
            this.txtNguoiVo.EditEnabled = true;
            this.txtNguoiVo.Label = "Người nữ";
            this.txtNguoiVo.Location = new System.Drawing.Point(7, 76);
            this.txtNguoiVo.MaGiaoDan = -1;
            this.txtNguoiVo.MaGiaoHo = -1;
            this.txtNguoiVo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNguoiVo.MaxLength = 255;
            this.txtNguoiVo.MultiLine = false;
            this.txtNguoiVo.Name = "txtNguoiVo";
            this.txtNguoiVo.NumberInputRequired = true;
            this.txtNguoiVo.NumberMode = false;
            this.txtNguoiVo.ReadOnly = true;
            this.txtNguoiVo.Size = new System.Drawing.Size(368, 26);
            this.txtNguoiVo.TabIndex = 4;
            this.txtNguoiVo.WhereSQL = "";
            this.txtNguoiVo.DeleteButton += new System.EventHandler(this.txtNguoiVo_DeleteButton);
            this.txtNguoiVo.OnSelecting += new GxControl.GxGiaoDan.SelectingHandler(this.txtNguoiVo_OnSelecting);
            this.txtNguoiVo.OnAdding += new GxControl.GxGiaoDan.SelectingHandler(this.txtNguoiVo_OnSelecting);
            // 
            // txtDienThoai
            // 
            this.txtDienThoai.AutoCompleteEnabled = false;
            this.txtDienThoai.AutoUpperFirstChar = false;
            this.txtDienThoai.BoxLeft = 80;
            this.txtDienThoai.EditEnabled = true;
            this.txtDienThoai.Label = "Điện thoại";
            this.txtDienThoai.Location = new System.Drawing.Point(7, 183);
            this.txtDienThoai.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDienThoai.MaxLength = 255;
            this.txtDienThoai.MultiLine = false;
            this.txtDienThoai.Name = "txtDienThoai";
            this.txtDienThoai.NumberInputRequired = true;
            this.txtDienThoai.NumberMode = false;
            this.txtDienThoai.ReadOnly = false;
            this.txtDienThoai.Size = new System.Drawing.Size(177, 25);
            this.txtDienThoai.TabIndex = 10;
            // 
            // txtDiaChi
            // 
            this.txtDiaChi.AutoCompleteEnabled = true;
            this.txtDiaChi.AutoUpperFirstChar = true;
            this.txtDiaChi.BoxLeft = 40;
            this.txtDiaChi.EditEnabled = true;
            this.txtDiaChi.Label = "Địa chỉ";
            this.txtDiaChi.Location = new System.Drawing.Point(185, 183);
            this.txtDiaChi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDiaChi.MaxLength = 255;
            this.txtDiaChi.MultiLine = false;
            this.txtDiaChi.Name = "txtDiaChi";
            this.txtDiaChi.NumberInputRequired = true;
            this.txtDiaChi.NumberMode = false;
            this.txtDiaChi.ReadOnly = false;
            this.txtDiaChi.Size = new System.Drawing.Size(261, 25);
            this.txtDiaChi.TabIndex = 11;
            // 
            // dtNgayChuyen
            // 
            this.dtNgayChuyen.AutoUpperFirstChar = false;
            this.dtNgayChuyen.BoxLeft = 80;
            this.dtNgayChuyen.EditEnabled = true;
            this.dtNgayChuyen.FullInputRequired = false;
            this.dtNgayChuyen.IsNullDate = true;
            this.dtNgayChuyen.Label = "Ngày chuyển";
            this.dtNgayChuyen.Location = new System.Drawing.Point(7, 306);
            this.dtNgayChuyen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayChuyen.Name = "dtNgayChuyen";
            this.dtNgayChuyen.Size = new System.Drawing.Size(244, 30);
            this.dtNgayChuyen.TabIndex = 14;
            this.dtNgayChuyen.Value = ((object)(resources.GetObject("dtNgayChuyen.Value")));
            this.dtNgayChuyen.Visible = false;
            // 
            // frmGiaDinh
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(897, 664);
            this.Controls.Add(this.lblGhiChu);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmGiaDinh";
            this.Text = "Nhập gia đình";
            this.Load += new System.EventHandler(this.frmGiaDinh_Load);
            this.Shown += new System.EventHandler(this.frmGiaDinh_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private GxControl.GxGiaoDan txtNguoiChong;
        private GxControl.GxTextField txtTenGiaDinh;
        private GxControl.GxGiaoDan txtNguoiVo;
        private GxControl.GxTextField txtGhiChu;
        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxGiaoDanList gxGiaoDanList1;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxGiaoHo cbGiaoHo;
        private GxControl.GxTextField txtMaGiaDinh;
        private GxControl.GxTextField txtDienThoai;
        private GxControl.GxTextField txtDiaChi;
        private GxControl.GxTextField txtNoiChuyen;
        private GxControl.GxDateField dtNgayChuyen;
        private GxControl.GxCheckBox chkChuyenXu;
        private GxControl.GxCheckBox chkDelete;
        private System.Windows.Forms.Label lblGhiChu;
        private GxControl.GxCheckBox chkGiaDinhAo;
        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxTextField txtMaGiaDinhRieng;
        private GxControl.GxHonPhoiGiaDinh gxXemHonPhoi1;
        private System.Windows.Forms.Panel panel1;
        private GxRadioBox rdChuHoNu;
        private GxRadioBox rdChuHoNam;
        private GxPictureField gxPictureField1;
        private GxTextField txtSoHoKhau;
        private System.Windows.Forms.PictureBox pictureBox1;
        private GxDienGiaDinh cbDienGiaDinh;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

