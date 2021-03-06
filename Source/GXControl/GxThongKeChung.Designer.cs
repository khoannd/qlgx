namespace GxControl
{
    partial class GxThongKeChung
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GxThongKeChung));
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.gxHonPhoiList1 = new GxControl.GxHonPhoiList();
            this.gxGiaDinhList1 = new GxControl.GxGiaDinhList();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.gxCbToAge = new GxControl.GxComboField();
            this.gxCbAgeFrom = new GxControl.GxComboField();
            this.gxCbMarried = new GxControl.GxComboField();
            this.gxCbCondition = new GxControl.GxComboField();
            this.cbGiaoHo = new GxControl.GxGiaoHo();
            this.chkLuuTru = new GxControl.GxCheckBox();
            this.chkNullAccept = new GxControl.GxCheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnFilter = new GxControl.GxButton();
            this.btnPrint = new GxControl.GxButton();
            this.btnSearch = new GxControl.GxButton();
            this.dtDateTo = new GxControl.GxDateField();
            this.dtDateFrom = new GxControl.GxDateField();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxHonPhoiList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaDinhList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxHonPhoiList1);
            this.uiGroupBox2.Controls.Add(this.gxGiaDinhList1);
            this.uiGroupBox2.Controls.Add(this.gxGiaoDanList1);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 214);
            this.uiGroupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(770, 310);
            this.uiGroupBox2.TabIndex = 1;
            this.uiGroupBox2.Text = "Kết quả thống kê";
            // 
            // gxHonPhoiList1
            // 
            this.gxHonPhoiList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxHonPhoiList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxHonPhoiList1.Arguments = null;
            this.gxHonPhoiList1.AutoLoadGridFormat = true;
            this.gxHonPhoiList1.ColumnAutoResize = true;
            this.gxHonPhoiList1.DisableParentOnLoadData = true;
            this.gxHonPhoiList1.DynamicFiltering = true;
            this.gxHonPhoiList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxHonPhoiList1.GroupByBoxVisible = false;
            this.gxHonPhoiList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxHonPhoiList1.ListMode = 1;
            this.gxHonPhoiList1.Location = new System.Drawing.Point(27, 37);
            this.gxHonPhoiList1.MaGiaoDan = -1;
            this.gxHonPhoiList1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxHonPhoiList1.Name = "gxHonPhoiList1";
            this.gxHonPhoiList1.Phai = "";
            this.gxHonPhoiList1.QueryString = "";
            this.gxHonPhoiList1.RecordNavigator = true;
            this.gxHonPhoiList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxHonPhoiList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.Default;
            this.gxHonPhoiList1.Size = new System.Drawing.Size(585, 79);
            this.gxHonPhoiList1.TabIndex = 2;
            this.gxHonPhoiList1.Visible = false;
            this.gxHonPhoiList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxHonPhoiList1.WhereSQL = "";
            this.gxHonPhoiList1.LoadDataFinished += new System.EventHandler(this.gxHonPhoiList1_LoadDataFinished);
            // 
            // gxGiaDinhList1
            // 
            this.gxGiaDinhList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxGiaDinhList1.AllowEditGiaDinh = true;
            this.gxGiaDinhList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaDinhList1.Arguments = null;
            this.gxGiaDinhList1.AutoLoadGridFormat = true;
            this.gxGiaDinhList1.ColumnAutoResize = true;
            this.gxGiaDinhList1.DisableParentOnLoadData = true;
            this.gxGiaDinhList1.DynamicFiltering = true;
            this.gxGiaDinhList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaDinhList1.Font = new System.Drawing.Font("Arial", 8.25F);
            this.gxGiaDinhList1.GroupByBoxVisible = false;
            this.gxGiaDinhList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaDinhList1.Location = new System.Drawing.Point(6, 150);
            this.gxGiaDinhList1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxGiaDinhList1.Name = "gxGiaDinhList1";
            this.gxGiaDinhList1.Operation = GxGlobal.GxOperation.EDIT;
            this.gxGiaDinhList1.QueryString = "SELECT * FROM SELECT_GIADINH_LIST WHERE 1 ";
            this.gxGiaDinhList1.RecordNavigator = true;
            this.gxGiaDinhList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaDinhList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaDinhList1.Size = new System.Drawing.Size(719, 152);
            this.gxGiaDinhList1.TabIndex = 1;
            this.gxGiaDinhList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaDinhList1.WhereSQL = "";
            this.gxGiaDinhList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gxGiaDinhList1_RowDoubleClick);
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
            this.gxGiaoDanList1.DynamicFiltering = true;
            this.gxGiaoDanList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaoDanList1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaoDanList1.GroupByBoxVisible = false;
            this.gxGiaoDanList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaoDanList1.Location = new System.Drawing.Point(3, 17);
            this.gxGiaoDanList1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxGiaoDanList1.Name = "gxGiaoDanList1";
            this.gxGiaoDanList1.QueryString = "";
            this.gxGiaoDanList1.RecordNavigator = true;
            this.gxGiaoDanList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoDanList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.SelectionMode = Janus.Windows.GridEX.SelectionMode.MultipleSelection;
            this.gxGiaoDanList1.Size = new System.Drawing.Size(705, 121);
            this.gxGiaoDanList1.TabIndex = 0;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.gxCbToAge);
            this.uiGroupBox1.Controls.Add(this.gxCbAgeFrom);
            this.uiGroupBox1.Controls.Add(this.gxCbMarried);
            this.uiGroupBox1.Controls.Add(this.gxCbCondition);
            this.uiGroupBox1.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox1.Controls.Add(this.chkLuuTru);
            this.uiGroupBox1.Controls.Add(this.chkNullAccept);
            this.uiGroupBox1.Controls.Add(this.label1);
            this.uiGroupBox1.Controls.Add(this.lblTotal);
            this.uiGroupBox1.Controls.Add(this.btnFilter);
            this.uiGroupBox1.Controls.Add(this.btnPrint);
            this.uiGroupBox1.Controls.Add(this.btnSearch);
            this.uiGroupBox1.Controls.Add(this.dtDateTo);
            this.uiGroupBox1.Controls.Add(this.dtDateFrom);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(770, 214);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Điều kiện thống kê";
            // 
            // gxCbToAge
            // 
            this.gxCbToAge.AutoUpperFirstChar = false;
            this.gxCbToAge.BoxLeft = 0;
            this.gxCbToAge.EditEnabled = true;
            this.gxCbToAge.Label = "Đến tuổi";
            this.gxCbToAge.Location = new System.Drawing.Point(210, 54);
            this.gxCbToAge.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxCbToAge.MaxLength = 0;
            this.gxCbToAge.Name = "gxCbToAge";
            this.gxCbToAge.SelectedIndex = -1;
            this.gxCbToAge.SelectedText = "";
            this.gxCbToAge.SelectedValue = null;
            this.gxCbToAge.Size = new System.Drawing.Size(167, 26);
            this.gxCbToAge.TabIndex = 17;
            this.gxCbToAge.Visible = false;
            // 
            // gxCbAgeFrom
            // 
            this.gxCbAgeFrom.AutoUpperFirstChar = false;
            this.gxCbAgeFrom.BoxLeft = 0;
            this.gxCbAgeFrom.EditEnabled = true;
            this.gxCbAgeFrom.Label = "Từ tuổi";
            this.gxCbAgeFrom.Location = new System.Drawing.Point(46, 54);
            this.gxCbAgeFrom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxCbAgeFrom.MaxLength = 0;
            this.gxCbAgeFrom.Name = "gxCbAgeFrom";
            this.gxCbAgeFrom.SelectedIndex = -1;
            this.gxCbAgeFrom.SelectedText = "";
            this.gxCbAgeFrom.SelectedValue = null;
            this.gxCbAgeFrom.Size = new System.Drawing.Size(162, 26);
            this.gxCbAgeFrom.TabIndex = 16;
            this.gxCbAgeFrom.Visible = false;
            // 
            // gxCbMarried
            // 
            this.gxCbMarried.AutoUpperFirstChar = false;
            this.gxCbMarried.BoxLeft = 0;
            this.gxCbMarried.EditEnabled = true;
            this.gxCbMarried.Label = "Tình trạng";
            this.gxCbMarried.Location = new System.Drawing.Point(382, 19);
            this.gxCbMarried.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxCbMarried.MaxLength = 0;
            this.gxCbMarried.Name = "gxCbMarried";
            this.gxCbMarried.SelectedIndex = -1;
            this.gxCbMarried.SelectedText = "";
            this.gxCbMarried.SelectedValue = null;
            this.gxCbMarried.Size = new System.Drawing.Size(277, 26);
            this.gxCbMarried.TabIndex = 15;
            this.gxCbMarried.Visible = false;
            // 
            // gxCbCondition
            // 
            this.gxCbCondition.AutoUpperFirstChar = false;
            this.gxCbCondition.BoxLeft = 0;
            this.gxCbCondition.EditEnabled = true;
            this.gxCbCondition.Label = "Điều kiện ";
            this.gxCbCondition.Location = new System.Drawing.Point(32, 20);
            this.gxCbCondition.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxCbCondition.MaxLength = 0;
            this.gxCbCondition.Name = "gxCbCondition";
            this.gxCbCondition.SelectedIndex = -1;
            this.gxCbCondition.SelectedText = "";
            this.gxCbCondition.SelectedValue = null;
            this.gxCbCondition.Size = new System.Drawing.Size(346, 26);
            this.gxCbCondition.TabIndex = 14;
            this.gxCbCondition.SelectedIndexChanged += new System.EventHandler(this.gxComboField1_SelectedIndexChanged);
            // 
            // cbGiaoHo
            // 
            this.cbGiaoHo.AutoLoadGrid = false;
            this.cbGiaoHo.AutoUpperFirstChar = false;
            this.cbGiaoHo.BoxLeft = 60;
            this.cbGiaoHo.EditEnabled = true;
            this.cbGiaoHo.GridGiaDinh = null;
            this.cbGiaoHo.GridGiaoDan = this.gxGiaoDanList1;
            this.cbGiaoHo.HasShowAll = true;
            this.cbGiaoHo.IsAo = Janus.Windows.GridEX.TriState.Empty;
            this.cbGiaoHo.IsHasLuuTru = false;
            this.cbGiaoHo.IsLuuTru = false;
            this.cbGiaoHo.Label = "Giáo họ";
            this.cbGiaoHo.Location = new System.Drawing.Point(27, 89);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = false;
            this.cbGiaoHo.Size = new System.Drawing.Size(352, 25);
            this.cbGiaoHo.TabIndex = 11;
            this.cbGiaoHo.WhereSQL = "";
            // 
            // chkLuuTru
            // 
            this.chkLuuTru.AutoSize = true;
            this.chkLuuTru.Location = new System.Drawing.Point(49, 137);
            this.chkLuuTru.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkLuuTru.Name = "chkLuuTru";
            this.chkLuuTru.Size = new System.Drawing.Size(152, 17);
            this.chkLuuTru.TabIndex = 10;
            this.chkLuuTru.Text = "Tính cả trong hồ sơ lưu trữ";
            this.chkLuuTru.UseVisualStyleBackColor = true;
            // 
            // chkNullAccept
            // 
            this.chkNullAccept.AutoSize = true;
            this.chkNullAccept.Location = new System.Drawing.Point(245, 137);
            this.chkNullAccept.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkNullAccept.Name = "chkNullAccept";
            this.chkNullAccept.Size = new System.Drawing.Size(235, 17);
            this.chkNullAccept.TabIndex = 10;
            this.chkNullAccept.Text = "Tính cả những dữ liệu không có ngày tháng";
            this.chkNullAccept.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(384, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(338, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Nếu nhập [Từ năm] = [Đến năm] thì xem như chỉ thống kê trong 1 năm";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.ForeColor = System.Drawing.Color.Crimson;
            this.lblTotal.Location = new System.Drawing.Point(389, 177);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(86, 16);
            this.lblTotal.TabIndex = 8;
            this.lblTotal.Text = "Tổng cộng:";
            // 
            // btnFilter
            // 
            this.btnFilter.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnFilter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFilter.BackgroundImage")));
            this.btnFilter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFilter.Enabled = false;
            this.btnFilter.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnFilter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnFilter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFilter.Location = new System.Drawing.Point(217, 173);
            this.btnFilter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(101, 22);
            this.btnFilter.TabIndex = 13;
            this.btnFilter.Text = "&Lọc lại trên lưới";
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnPrint.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPrint.BackgroundImage")));
            this.btnPrint.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPrint.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnPrint.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnPrint.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnPrint.Location = new System.Drawing.Point(130, 173);
            this.btnPrint.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(81, 22);
            this.btnPrint.TabIndex = 13;
            this.btnPrint.Text = "&In danh sách";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSearch.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSearch.BackgroundImage")));
            this.btnSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnSearch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSearch.Location = new System.Drawing.Point(49, 173);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 22);
            this.btnSearch.TabIndex = 12;
            this.btnSearch.Text = "Thống kê";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // dtDateTo
            // 
            this.dtDateTo.AutoUpperFirstChar = false;
            this.dtDateTo.BoxLeft = 60;
            this.dtDateTo.EditEnabled = true;
            this.dtDateTo.FullInputRequired = false;
            this.dtDateTo.IsNullDate = true;
            this.dtDateTo.Label = "Đến ngày";
            this.dtDateTo.Location = new System.Drawing.Point(205, 54);
            this.dtDateTo.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.dtDateTo.Name = "dtDateTo";
            this.dtDateTo.Size = new System.Drawing.Size(178, 26);
            this.dtDateTo.TabIndex = 1;
            this.dtDateTo.Value = ((object)(resources.GetObject("dtDateTo.Value")));
            this.dtDateTo.Leave += new System.EventHandler(this.txtYear_Leave);
            // 
            // dtDateFrom
            // 
            this.dtDateFrom.AutoUpperFirstChar = false;
            this.dtDateFrom.BoxLeft = 60;
            this.dtDateFrom.EditEnabled = true;
            this.dtDateFrom.FullInputRequired = false;
            this.dtDateFrom.IsNullDate = true;
            this.dtDateFrom.Label = "Từ ngày";
            this.dtDateFrom.Location = new System.Drawing.Point(27, 53);
            this.dtDateFrom.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.dtDateFrom.Name = "dtDateFrom";
            this.dtDateFrom.Size = new System.Drawing.Size(178, 26);
            this.dtDateFrom.TabIndex = 0;
            this.dtDateFrom.Value = ((object)(resources.GetObject("dtDateFrom.Value")));
            this.dtDateFrom.Leave += new System.EventHandler(this.txtYear_Leave);
            // 
            // GxThongKeChung
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.uiGroupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "GxThongKeChung";
            this.Size = new System.Drawing.Size(770, 524);
            this.Load += new System.EventHandler(this.frmThongKeChung_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxHonPhoiList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaDinhList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.uiGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxGiaoDanList gxGiaoDanList1;
        private GxControl.GxGiaDinhList gxGiaDinhList1;
        private GxControl.GxHonPhoiList gxHonPhoiList1;
        private GxDateField dtDateFrom;
        private GxDateField dtDateTo;
        private GxButton btnSearch;
        private GxButton btnPrint;
        private GxButton btnFilter;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label1;
        private GxCheckBox chkNullAccept;
        private GxCheckBox chkLuuTru;
        protected GxGiaoHo cbGiaoHo;
        private GxComboField gxCbCondition;
        private GxComboField gxCbMarried;
        private GxComboField gxCbAgeFrom;
        private GxComboField gxCbToAge;
        private GxGroupBox uiGroupBox1;
    }
}