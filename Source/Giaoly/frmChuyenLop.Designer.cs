namespace GiaoLy
{
    partial class frmChuyenLop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChuyenLop));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbLop = new Janus.Windows.EditControls.UIComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbKhoi = new Janus.Windows.EditControls.UIComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUncheckAll = new GxControl.GxButton();
            this.btnCheckAll = new GxControl.GxButton();
            this.lblNam = new System.Windows.Forms.Label();
            this.cbNam = new Janus.Windows.EditControls.UIComboBox();
            this.gxCommand1 = new GxControl.GxCommand();
            this.gxHocSinhList1 = new GxControl.GxHocSinh();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxHocSinhList1)).BeginInit();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.label3);
            this.uiGroupBox1.Controls.Add(this.cbLop);
            this.uiGroupBox1.Controls.Add(this.label2);
            this.uiGroupBox1.Controls.Add(this.cbKhoi);
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.btnUncheckAll);
            this.uiGroupBox1.Controls.Add(this.btnCheckAll);
            this.uiGroupBox1.Controls.Add(this.lblNam);
            this.uiGroupBox1.Controls.Add(this.cbNam);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(781, 101);
            this.uiGroupBox1.TabIndex = 1;
            this.uiGroupBox1.Text = "Thông tin lớp giáo lý";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(519, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Lớp";
            // 
            // cbLop
            // 
            this.cbLop.ComboStyle = Janus.Windows.EditControls.ComboStyle.DropDownList;
            this.cbLop.Location = new System.Drawing.Point(550, 19);
            this.cbLop.Name = "cbLop";
            this.cbLop.Size = new System.Drawing.Size(199, 20);
            this.cbLop.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(250, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Khối";
            // 
            // cbKhoi
            // 
            this.cbKhoi.ComboStyle = Janus.Windows.EditControls.ComboStyle.DropDownList;
            this.cbKhoi.Location = new System.Drawing.Point(284, 19);
            this.cbKhoi.Name = "cbKhoi";
            this.cbKhoi.Size = new System.Drawing.Size(204, 20);
            this.cbKhoi.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(3, 82);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(349, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Danh sách các học sinh được chọn sẽ chuyển lên lớp cho năm học sau";
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUncheckAll.AutoSize = true;
            this.btnUncheckAll.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnUncheckAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUncheckAll.BackgroundImage")));
            this.btnUncheckAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnUncheckAll.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnUncheckAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnUncheckAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnUncheckAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnUncheckAll.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnUncheckAll.ImageKey = "ExportExcel";
            this.btnUncheckAll.Location = new System.Drawing.Point(683, 71);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(86, 24);
            this.btnUncheckAll.TabIndex = 20;
            this.btnUncheckAll.Text = "Bỏ chọn";
            this.btnUncheckAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUncheckAll.UseVisualStyleBackColor = true;
            this.btnUncheckAll.Visible = false;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckAll.AutoSize = true;
            this.btnCheckAll.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnCheckAll.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCheckAll.BackgroundImage")));
            this.btnCheckAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCheckAll.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnCheckAll.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnCheckAll.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnCheckAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCheckAll.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnCheckAll.ImageKey = "ExportExcel";
            this.btnCheckAll.Location = new System.Drawing.Point(591, 71);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(86, 24);
            this.btnCheckAll.TabIndex = 19;
            this.btnCheckAll.Text = "Chọn tất cả";
            this.btnCheckAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Visible = false;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
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
            this.cbNam.ComboStyle = Janus.Windows.EditControls.ComboStyle.DropDownList;
            this.cbNam.Location = new System.Drawing.Point(88, 19);
            this.cbNam.Name = "cbNam";
            this.cbNam.Size = new System.Drawing.Size(127, 20);
            this.cbNam.TabIndex = 15;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 513);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(781, 42);
            this.gxCommand1.TabIndex = 5;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "";
            this.gxCommand1.ToolTipOK = "";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // gxHocSinhList1
            // 
            this.gxHocSinhList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
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
            this.gxHocSinhList1.Location = new System.Drawing.Point(0, 101);
            this.gxHocSinhList1.MaLop = 0;
            this.gxHocSinhList1.Name = "gxHocSinhList1";
            this.gxHocSinhList1.NgaySinh = "";
            this.gxHocSinhList1.Phai = "";
            this.gxHocSinhList1.QueryString = "";
            this.gxHocSinhList1.RecordNavigator = true;
            this.gxHocSinhList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxHocSinhList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxHocSinhList1.Size = new System.Drawing.Size(781, 454);
            this.gxHocSinhList1.SoThuTu = 0;
            this.gxHocSinhList1.TabIndex = 4;
            this.gxHocSinhList1.TenThanh = "";
            this.gxHocSinhList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxHocSinhList1.WhereSQL = "";
            // 
            // frmChuyenLop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(781, 555);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.gxHocSinhList1);
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "frmChuyenLop";
            this.Text = "Chuyen Lop Giao Ly";
            this.Load += new System.EventHandler(this.frmChuyenLop_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxHocSinhList1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private System.Windows.Forms.Label lblNam;
        private Janus.Windows.EditControls.UIComboBox cbNam;
        private GxControl.GxHocSinh gxHocSinhList1;
        public GxControl.GxButton btnUncheckAll;
        public GxControl.GxButton btnCheckAll;
        private GxControl.GxCommand gxCommand1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private Janus.Windows.EditControls.UIComboBox cbLop;
        private System.Windows.Forms.Label label2;
        private Janus.Windows.EditControls.UIComboBox cbKhoi;

    }
}