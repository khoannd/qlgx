namespace GxControl
{
    partial class GxExtract
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GxExtract));
            this.gxGroupResult = new GxControl.GxGroupBox();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.gxGroupCondition = new GxControl.GxGroupBox();
            this.cbAgeTo = new GxControl.GxComboField();
            this.cbAgeFrom = new GxControl.GxComboField();
            this.gxButton3 = new GxControl.GxButton();
            this.gxBtnPrint = new GxControl.GxButton();
            this.gxBtnTrichXuat = new GxControl.GxButton();
            this.cbCondition = new GxControl.GxComboField();
            this.gxGiaoHo1 = new GxControl.GxGiaoHo();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupResult)).BeginInit();
            this.gxGroupResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupCondition)).BeginInit();
            this.gxGroupCondition.SuspendLayout();
            this.SuspendLayout();
            // 
            // gxGroupResult
            // 
            this.gxGroupResult.Controls.Add(this.gxGiaoDanList1);
            this.gxGroupResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupResult.Location = new System.Drawing.Point(0, 127);
            this.gxGroupResult.Name = "gxGroupResult";
            this.gxGroupResult.Size = new System.Drawing.Size(770, 397);
            this.gxGroupResult.TabIndex = 0;
            this.gxGroupResult.Text = "Kết quả thống kê";
            // 
            // gxGiaoDanList1
            // 
            this.gxGiaoDanList1.AllowColumnDrag = false;
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
            this.gxGiaoDanList1.Location = new System.Drawing.Point(3, 16);
            this.gxGiaoDanList1.Name = "gxGiaoDanList1";
            this.gxGiaoDanList1.QueryString = "";
            this.gxGiaoDanList1.RecordNavigator = true;
            this.gxGiaoDanList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoDanList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.SettingsKey = "gxGiaoDanList1";
            this.gxGiaoDanList1.Size = new System.Drawing.Size(764, 378);
            this.gxGiaoDanList1.TabIndex = 4;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            // 
            // gxGroupCondition
            // 
            this.gxGroupCondition.Controls.Add(this.cbAgeTo);
            this.gxGroupCondition.Controls.Add(this.cbAgeFrom);
            this.gxGroupCondition.Controls.Add(this.gxButton3);
            this.gxGroupCondition.Controls.Add(this.gxBtnPrint);
            this.gxGroupCondition.Controls.Add(this.gxBtnTrichXuat);
            this.gxGroupCondition.Controls.Add(this.cbCondition);
            this.gxGroupCondition.Controls.Add(this.gxGiaoHo1);
            this.gxGroupCondition.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupCondition.Location = new System.Drawing.Point(0, 0);
            this.gxGroupCondition.Name = "gxGroupCondition";
            this.gxGroupCondition.Size = new System.Drawing.Size(770, 127);
            this.gxGroupCondition.TabIndex = 0;
            this.gxGroupCondition.Text = "Điều kiện trích xuất";
            // 
            // cbAgeTo
            // 
            this.cbAgeTo.AutoUpperFirstChar = false;
            this.cbAgeTo.BoxLeft = 0;
            this.cbAgeTo.EditEnabled = true;
            this.cbAgeTo.Label = "Đến tuổi";
            this.cbAgeTo.Location = new System.Drawing.Point(498, 55);
            this.cbAgeTo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbAgeTo.MaxLength = 0;
            this.cbAgeTo.Name = "cbAgeTo";
            this.cbAgeTo.SelectedIndex = -1;
            this.cbAgeTo.SelectedText = "";
            this.cbAgeTo.SelectedValue = null;
            this.cbAgeTo.Size = new System.Drawing.Size(111, 26);
            this.cbAgeTo.TabIndex = 6;
            this.cbAgeTo.Visible = false;
            // 
            // cbAgeFrom
            // 
            this.cbAgeFrom.AutoUpperFirstChar = false;
            this.cbAgeFrom.BoxLeft = 0;
            this.cbAgeFrom.EditEnabled = true;
            this.cbAgeFrom.Label = "Từ tuổi";
            this.cbAgeFrom.Location = new System.Drawing.Point(363, 55);
            this.cbAgeFrom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbAgeFrom.MaxLength = 0;
            this.cbAgeFrom.Name = "cbAgeFrom";
            this.cbAgeFrom.SelectedIndex = -1;
            this.cbAgeFrom.SelectedText = "";
            this.cbAgeFrom.SelectedValue = null;
            this.cbAgeFrom.Size = new System.Drawing.Size(115, 26);
            this.cbAgeFrom.TabIndex = 5;
            this.cbAgeFrom.Visible = false;
            this.cbAgeFrom.Load += new System.EventHandler(this.gxComboField2_Load);
            // 
            // gxButton3
            // 
            this.gxButton3.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gxButton3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gxButton3.BackgroundImage")));
            this.gxButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gxButton3.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.gxButton3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.gxButton3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.gxButton3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gxButton3.Location = new System.Drawing.Point(243, 88);
            this.gxButton3.Name = "gxButton3";
            this.gxButton3.Size = new System.Drawing.Size(75, 23);
            this.gxButton3.TabIndex = 4;
            this.gxButton3.Text = "gxButton3";
            this.gxButton3.UseVisualStyleBackColor = false;
            // 
            // gxBtnPrint
            // 
            this.gxBtnPrint.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gxBtnPrint.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gxBtnPrint.BackgroundImage")));
            this.gxBtnPrint.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gxBtnPrint.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.gxBtnPrint.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.gxBtnPrint.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.gxBtnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gxBtnPrint.Location = new System.Drawing.Point(135, 88);
            this.gxBtnPrint.Name = "gxBtnPrint";
            this.gxBtnPrint.Size = new System.Drawing.Size(102, 23);
            this.gxBtnPrint.TabIndex = 3;
            this.gxBtnPrint.Text = "In danh sách";
            this.gxBtnPrint.UseVisualStyleBackColor = false;
            this.gxBtnPrint.Click += new System.EventHandler(this.gxButton2_Click);
            // 
            // gxBtnTrichXuat
            // 
            this.gxBtnTrichXuat.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gxBtnTrichXuat.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gxBtnTrichXuat.BackgroundImage")));
            this.gxBtnTrichXuat.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gxBtnTrichXuat.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.gxBtnTrichXuat.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.gxBtnTrichXuat.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.gxBtnTrichXuat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gxBtnTrichXuat.Location = new System.Drawing.Point(54, 88);
            this.gxBtnTrichXuat.Name = "gxBtnTrichXuat";
            this.gxBtnTrichXuat.Size = new System.Drawing.Size(75, 23);
            this.gxBtnTrichXuat.TabIndex = 2;
            this.gxBtnTrichXuat.Text = "Trích xuất";
            this.gxBtnTrichXuat.UseVisualStyleBackColor = false;
            this.gxBtnTrichXuat.Click += new System.EventHandler(this.gxBtnTrichXuat_Click);
            // 
            // cbCondition
            // 
            this.cbCondition.AutoUpperFirstChar = false;
            this.cbCondition.BoxLeft = 0;
            this.cbCondition.EditEnabled = true;
            this.cbCondition.Label = "Điều kiện trích xuất";
            this.cbCondition.Location = new System.Drawing.Point(55, 55);
            this.cbCondition.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbCondition.MaxLength = 0;
            this.cbCondition.Name = "cbCondition";
            this.cbCondition.SelectedIndex = -1;
            this.cbCondition.SelectedText = "";
            this.cbCondition.SelectedValue = null;
            this.cbCondition.Size = new System.Drawing.Size(249, 26);
            this.cbCondition.TabIndex = 1;
            // 
            // gxGiaoHo1
            // 
            this.gxGiaoHo1.AutoLoadGrid = false;
            this.gxGiaoHo1.AutoUpperFirstChar = false;
            this.gxGiaoHo1.BoxLeft = 0;
            this.gxGiaoHo1.EditEnabled = true;
            this.gxGiaoHo1.GridGiaDinh = null;
            this.gxGiaoHo1.GridGiaoDan = null;
            this.gxGiaoHo1.HasShowAll = true;
            this.gxGiaoHo1.IsAo = Janus.Windows.GridEX.TriState.Empty;
            this.gxGiaoHo1.IsLuuTru = false;
            this.gxGiaoHo1.Label = "Giáo họ";
            this.gxGiaoHo1.Location = new System.Drawing.Point(110, 20);
            this.gxGiaoHo1.MaGiaoHo = -1;
            this.gxGiaoHo1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gxGiaoHo1.Name = "gxGiaoHo1";
        
            this.gxGiaoHo1.SelectedValue = null;
            this.gxGiaoHo1.ShowNgoaiXu = true;
            this.gxGiaoHo1.Size = new System.Drawing.Size(194, 27);
            this.gxGiaoHo1.TabIndex = 0;
            this.gxGiaoHo1.WhereSQL = "";
            // 
            // GxExtract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gxGroupResult);
            this.Controls.Add(this.gxGroupCondition);
            this.Name = "GxExtract";
            this.Size = new System.Drawing.Size(770, 524);
            this.Load += new System.EventHandler(this.GxExtract_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupResult)).EndInit();
            this.gxGroupResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupCondition)).EndInit();
            this.gxGroupCondition.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxGroupBox gxGroupCondition;
        private GxGroupBox gxGroupResult;
        private GxGiaoHo gxGiaoHo1;
        private GxComboField cbCondition;
        private GxButton gxButton3;
        private GxButton gxBtnPrint;
        private GxButton gxBtnTrichXuat;
        private GxComboField cbAgeFrom;
        private GxComboField cbAgeTo;
        protected GxGiaoDanList gxGiaoDanList1;
    }
}
