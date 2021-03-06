namespace GxControl
{
    partial class frmChonDuLieu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChonDuLieu));
            this.gxCommand1 = new GxControl.GxCommand();
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.txtFind = new GxControl.GxTextField();
            this.btnFind = new GxControl.GxButton();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.cbGiaoHo = new GxControl.GxGiaoHo();
            this.gxGiaDinhList1 = new GxControl.GxGiaDinhList();
            this.uiGroupBox3 = new GxControl.GxGroupBox();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaDinhList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).BeginInit();
            this.uiGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            this.SuspendLayout();
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 452);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(691, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.AutoScroll = true;
            this.uiGroupBox1.Controls.Add(this.txtFind);
            this.uiGroupBox1.Controls.Add(this.btnFind);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 50);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(691, 51);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Tìm kiếm";
            this.uiGroupBox1.Visible = false;
            // 
            // txtFind
            // 
            this.txtFind.AutoCompleteEnabled = true;
            this.txtFind.AutoUpperFirstChar = false;
            this.txtFind.BoxLeft = 0;
            this.txtFind.EditEnabled = true;
            this.txtFind.Label = "Tên giáo dân";
            this.txtFind.Location = new System.Drawing.Point(20, 18);
            this.txtFind.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtFind.MaxLength = 32767;
            this.txtFind.MultiLine = false;
            this.txtFind.Name = "txtFind";
            this.txtFind.NumberInputRequired = true;
            this.txtFind.NumberMode = false;
            this.txtFind.ReadOnly = false;
            this.txtFind.Size = new System.Drawing.Size(340, 26);
            this.txtFind.TabIndex = 0;
            // 
            // btnFind
            // 
            this.btnFind.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnFind.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFind.BackgroundImage")));
            this.btnFind.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnFind.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnFind.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnFind.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnFind.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnFind.Location = new System.Drawing.Point(385, 18);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(75, 23);
            this.btnFind.TabIndex = 1;
            this.btnFind.Text = "Tìm";
            this.btnFind.UseVisualStyleBackColor = false;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.label1);
            this.uiGroupBox2.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(691, 50);
            this.uiGroupBox2.TabIndex = 3;
            this.uiGroupBox2.Text = "Giáo họ";
            // 
            // cbGiaoHo
            // 
            this.cbGiaoHo.AutoLoadGrid = true;
            this.cbGiaoHo.AutoUpperFirstChar = false;
            this.cbGiaoHo.BoxLeft = 0;
            this.cbGiaoHo.EditEnabled = true;
            this.cbGiaoHo.GridGiaDinh = null;
            this.cbGiaoHo.GridGiaoDan = null;
            this.cbGiaoHo.HasShowAll = false;
            this.cbGiaoHo.IsAo = Janus.Windows.GridEX.TriState.Empty;
            this.cbGiaoHo.IsHasLuuTru = false;
            this.cbGiaoHo.IsLuuTru = false;
            this.cbGiaoHo.Label = "Giáo họ";
            this.cbGiaoHo.Location = new System.Drawing.Point(20, 15);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = true;
            this.cbGiaoHo.Size = new System.Drawing.Size(340, 25);
            this.cbGiaoHo.TabIndex = 0;
            this.cbGiaoHo.WhereSQL = "";
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
            this.gxGiaDinhList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaDinhList1.GroupByBoxVisible = false;
            this.gxGiaDinhList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaDinhList1.Location = new System.Drawing.Point(204, 62);
            this.gxGiaDinhList1.Name = "gxGiaDinhList1";
            this.gxGiaDinhList1.Operation = GxGlobal.GxOperation.EDIT;
            this.gxGiaDinhList1.QueryString = "SELECT * FROM SELECT_GIADINH_LIST WHERE 1 ";
            this.gxGiaDinhList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaDinhList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.Default;
            this.gxGiaDinhList1.Size = new System.Drawing.Size(400, 129);
            this.gxGiaDinhList1.TabIndex = 4;
            this.gxGiaDinhList1.Visible = false;
            this.gxGiaDinhList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaDinhList1.WhereSQL = "";
            this.gxGiaDinhList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gridEX1_RowDoubleClick);
            this.gxGiaDinhList1.SelectionChanged += new System.EventHandler(this.gridEX1_SelectionChanged);
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.Controls.Add(this.gxGiaDinhList1);
            this.uiGroupBox3.Controls.Add(this.gxGiaoDanList1);
            this.uiGroupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox3.Location = new System.Drawing.Point(0, 101);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Size = new System.Drawing.Size(691, 351);
            this.uiGroupBox3.TabIndex = 5;
            this.uiGroupBox3.Text = "Dữ liệu có thể chọn";
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
            this.gxGiaoDanList1.SelectionMode = Janus.Windows.GridEX.SelectionMode.MultipleSelection;
            this.gxGiaoDanList1.Size = new System.Drawing.Size(685, 332);
            this.gxGiaoDanList1.TabIndex = 5;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            this.gxGiaoDanList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gridEX1_RowDoubleClick);
            this.gxGiaoDanList1.SelectionChanged += new System.EventHandler(this.gridEX1_SelectionChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(383, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Xin vui lòng chọn giáo họ để tải dữ liệu";
            // 
            // frmChonDuLieu
            // 
            this.AcceptButton = this.btnFind;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(691, 497);
            this.Controls.Add(this.uiGroupBox3);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.uiGroupBox2);
            this.Controls.Add(this.gxCommand1);
            this.Name = "frmChonDuLieu";
            this.Text = "Chọn giáo dân";
            this.Load += new System.EventHandler(this.frmChonGiaoDan_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaDinhList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).EndInit();
            this.uiGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GxCommand gxCommand1;
        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxGroupBox uiGroupBox2;
        private GxGiaoHo cbGiaoHo;
        private GxGiaDinhList gxGiaDinhList1;
        private GxControl.GxGroupBox uiGroupBox3;
        protected GxGiaoDanList gxGiaoDanList1;
        private GxButton btnFind;
        private GxTextField txtFind;
        private System.Windows.Forms.Label label1;
    }
}