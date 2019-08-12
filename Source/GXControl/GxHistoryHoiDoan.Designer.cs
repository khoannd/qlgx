namespace GxControl
{
    partial class GxHistoryHoiDoan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GxHistoryHoiDoan));
            this.panel1 = new System.Windows.Forms.Panel();
            this.dtNgayRaHoiDoan = new GxControl.GxDateField();
            this.dtNgayVaoHoiDoan = new GxControl.GxDateField();
            this.cbTenHoiDoan = new GxControl.GxComboField();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.gxListHistoryHoiDoan1 = new GxControl.GxListHistoryHoiDoan();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxListHistoryHoiDoan1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dtNgayRaHoiDoan);
            this.panel1.Controls.Add(this.dtNgayVaoHoiDoan);
            this.panel1.Controls.Add(this.cbTenHoiDoan);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(760, 40);
            this.panel1.TabIndex = 2;
            // 
            // dtNgayRaHoiDoan
            // 
            this.dtNgayRaHoiDoan.AutoUpperFirstChar = false;
            this.dtNgayRaHoiDoan.BoxLeft = 0;
            this.dtNgayRaHoiDoan.EditEnabled = true;
            this.dtNgayRaHoiDoan.FullInputRequired = false;
            this.dtNgayRaHoiDoan.IsNullDate = true;
            this.dtNgayRaHoiDoan.Label = "Ngày ra hội đoàn";
            this.dtNgayRaHoiDoan.Location = new System.Drawing.Point(539, 4);
            this.dtNgayRaHoiDoan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayRaHoiDoan.Name = "dtNgayRaHoiDoan";
            this.dtNgayRaHoiDoan.Size = new System.Drawing.Size(215, 38);
            this.dtNgayRaHoiDoan.TabIndex = 2;
            this.dtNgayRaHoiDoan.Value = ((object)(resources.GetObject("dtNgayRaHoiDoan.Value")));
            // 
            // dtNgayVaoHoiDoan
            // 
            this.dtNgayVaoHoiDoan.AutoUpperFirstChar = false;
            this.dtNgayVaoHoiDoan.BoxLeft = 0;
            this.dtNgayVaoHoiDoan.EditEnabled = true;
            this.dtNgayVaoHoiDoan.FullInputRequired = false;
            this.dtNgayVaoHoiDoan.IsNullDate = true;
            this.dtNgayVaoHoiDoan.Label = "Ngày vào hội đoàn";
            this.dtNgayVaoHoiDoan.Location = new System.Drawing.Point(267, 4);
            this.dtNgayVaoHoiDoan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtNgayVaoHoiDoan.Name = "dtNgayVaoHoiDoan";
            this.dtNgayVaoHoiDoan.Size = new System.Drawing.Size(214, 38);
            this.dtNgayVaoHoiDoan.TabIndex = 1;
            this.dtNgayVaoHoiDoan.Value = ((object)(resources.GetObject("dtNgayVaoHoiDoan.Value")));
            // 
            // cbTenHoiDoan
            // 
            this.cbTenHoiDoan.AutoUpperFirstChar = false;
            this.cbTenHoiDoan.BoxLeft = 0;
            this.cbTenHoiDoan.EditEnabled = true;
            this.cbTenHoiDoan.Label = "Tên hội đoàn";
            this.cbTenHoiDoan.Location = new System.Drawing.Point(3, 4);
            this.cbTenHoiDoan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbTenHoiDoan.MaxLength = 0;
            this.cbTenHoiDoan.Name = "cbTenHoiDoan";
            this.cbTenHoiDoan.SelectedIndex = -1;
            this.cbTenHoiDoan.SelectedText = "";
            this.cbTenHoiDoan.SelectedValue = null;
            this.cbTenHoiDoan.Size = new System.Drawing.Size(207, 26);
            this.cbTenHoiDoan.TabIndex = 0;
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.gxListHistoryHoiDoan1);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 68);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(760, 193);
            this.gxGroupBox1.TabIndex = 0;
            this.gxGroupBox1.Text = "Danh sách hội đoàn";
            // 
            // gxListHistoryHoiDoan1
            // 
            this.gxListHistoryHoiDoan1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListHistoryHoiDoan1.Arguments = null;
            this.gxListHistoryHoiDoan1.AutoLoadGridFormat = true;
            this.gxListHistoryHoiDoan1.ColumnAutoResize = true;
            this.gxListHistoryHoiDoan1.DisableParentOnLoadData = true;
            this.gxListHistoryHoiDoan1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxListHistoryHoiDoan1.DynamicFiltering = true;
            this.gxListHistoryHoiDoan1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxListHistoryHoiDoan1.GroupByBoxVisible = false;
            this.gxListHistoryHoiDoan1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxListHistoryHoiDoan1.Location = new System.Drawing.Point(3, 16);
            this.gxListHistoryHoiDoan1.Name = "gxListHistoryHoiDoan1";
            this.gxListHistoryHoiDoan1.QueryString = "";
            this.gxListHistoryHoiDoan1.RecordNavigator = true;
            this.gxListHistoryHoiDoan1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxListHistoryHoiDoan1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListHistoryHoiDoan1.Size = new System.Drawing.Size(754, 174);
            this.gxListHistoryHoiDoan1.TabIndex = 0;
            this.gxListHistoryHoiDoan1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxListHistoryHoiDoan1.WhereSQL = "";
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxAddEdit1.GridData = null;
            this.gxAddEdit1.Location = new System.Drawing.Point(0, 40);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(760, 28);
            this.gxAddEdit1.TabIndex = 1;
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
            // 
            // GxHistoryHoiDoan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gxGroupBox1);
            this.Controls.Add(this.gxAddEdit1);
            this.Controls.Add(this.panel1);
            this.Name = "GxHistoryHoiDoan";
            this.Size = new System.Drawing.Size(760, 261);
            this.Load += new System.EventHandler(this.GxHistoryHoiDoan_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxListHistoryHoiDoan1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private GxComboField cbTenHoiDoan;
        private GxDateField dtNgayVaoHoiDoan;
        private System.Windows.Forms.Panel panel1;
        private GxDateField dtNgayRaHoiDoan;
        private GxAddEdit gxAddEdit1;
        private GxGroupBox gxGroupBox1;
        private GxListHistoryHoiDoan gxListHistoryHoiDoan1;
    }
}
