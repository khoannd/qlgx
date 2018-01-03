namespace GxControl
{
    partial class frmChonGiaoDan
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChonGiaoDan));
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.gxButton1 = new GxControl.GxButton();
            this.txtHoTen = new GxControl.GxTextField();
            this.gxGroupBox2 = new GxControl.GxGroupBox();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).BeginInit();
            this.gxGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            this.SuspendLayout();
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.gxAddEdit1);
            this.gxGroupBox1.Controls.Add(this.gxButton1);
            this.gxGroupBox1.Controls.Add(this.txtHoTen);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(693, 53);
            this.gxGroupBox1.TabIndex = 0;
            this.gxGroupBox1.Text = "Tìm giáo dân";
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = null;
            this.gxAddEdit1.Location = new System.Drawing.Point(543, 20);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(366, 23);
            this.gxAddEdit1.TabIndex = 2;
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
            // 
            // gxButton1
            // 
            this.gxButton1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gxButton1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gxButton1.BackgroundImage")));
            this.gxButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.gxButton1.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.gxButton1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.gxButton1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.gxButton1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.gxButton1.Location = new System.Drawing.Point(374, 20);
            this.gxButton1.Name = "gxButton1";
            this.gxButton1.Size = new System.Drawing.Size(37, 23);
            this.gxButton1.TabIndex = 1;
            this.gxButton1.Text = "Tìm";
            this.gxButton1.UseVisualStyleBackColor = true;
            this.gxButton1.Click += new System.EventHandler(this.gxButton1_Click);
            // 
            // txtHoTen
            // 
            this.txtHoTen.AutoCompleteEnabled = false;
            this.txtHoTen.AutoUpperFirstChar = false;
            this.txtHoTen.BoxLeft = 0;
            this.txtHoTen.EditEnabled = true;
            this.txtHoTen.Label = "Tên giáo dân";
            this.txtHoTen.Location = new System.Drawing.Point(20, 19);
            this.txtHoTen.MaxLength = 32767;
            this.txtHoTen.MultiLine = false;
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.NumberInputRequired = false;
            this.txtHoTen.NumberMode = false;
            this.txtHoTen.ReadOnly = false;
            this.txtHoTen.Size = new System.Drawing.Size(347, 26);
            this.txtHoTen.TabIndex = 0;
            // 
            // gxGroupBox2
            // 
            this.gxGroupBox2.Controls.Add(this.gxGiaoDanList1);
            this.gxGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox2.Location = new System.Drawing.Point(0, 53);
            this.gxGroupBox2.Name = "gxGroupBox2";
            this.gxGroupBox2.Size = new System.Drawing.Size(693, 378);
            this.gxGroupBox2.TabIndex = 0;
            this.gxGroupBox2.Text = "Danh sách tìm kiếm";
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
            this.gxGiaoDanList1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaoDanList1.GroupByBoxVisible = false;
            this.gxGiaoDanList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaoDanList1.Location = new System.Drawing.Point(3, 16);
            this.gxGiaoDanList1.Name = "gxGiaoDanList1";
            this.gxGiaoDanList1.QueryString = "";
            this.gxGiaoDanList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoDanList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.Default;
            this.gxGiaoDanList1.Size = new System.Drawing.Size(687, 359);
            this.gxGiaoDanList1.TabIndex = 0;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            this.gxGiaoDanList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gxGiaoDanList1_RowDoubleClick);
            this.gxGiaoDanList1.SelectionChanged += new System.EventHandler(this.gxGiaoDanList1_SelectionChanged);
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 431);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(693, 50);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // frmChonGiaoDan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 481);
            this.Controls.Add(this.gxGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.gxGroupBox1);
            this.Name = "frmChonGiaoDan";
            this.Text = "Chọn giáo dân";
            this.Load += new System.EventHandler(this.frmChonGiaoDan_Load);
            this.VisibleChanged += new System.EventHandler(this.frmChonGiaoDan_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).EndInit();
            this.gxGroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GxGroupBox gxGroupBox1;
        private GxGroupBox gxGroupBox2;
        private GxGiaoDanList gxGiaoDanList1;
        private GxCommand gxCommand1;
        private GxTextField txtHoTen;
        private GxButton gxButton1;
        private GxAddEdit gxAddEdit1;
    }
}