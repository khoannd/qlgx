namespace GiaoXu
{
    partial class frmGiaoDanLuuTruList
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
            Janus.Windows.GridEX.GridEXLayout gridEXLayout1 = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGiaoDanLuuTruList));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.gxGiaoDanList1 = new GxControl.GxGiaoDanList();
            this.uiGroupBox3 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.gxCommand1 = new GxControl.GxCommand();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.chkGiaoDanAo = new GxControl.GxCheckBox();
            this.lblTotal = new System.Windows.Forms.Label();
            this.cbGiaoHo = new GxControl.GxGiaoHo();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).BeginInit();
            this.uiGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.gxGiaoDanList1);
            this.uiGroupBox1.Controls.Add(this.uiGroupBox3);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 75);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(890, 523);
            this.uiGroupBox1.TabIndex = 1;
            this.uiGroupBox1.Text = "Danh sách giáo dân";
            // 
            // gxGiaoDanList1
            // 
            this.gxGiaoDanList1.AllowContextMenu = true;
            this.gxGiaoDanList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxGiaoDanList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.AllowShowForm = true;
            this.gxGiaoDanList1.Arguments = null;
            this.gxGiaoDanList1.AutoLoadGridFormat = true;
            this.gxGiaoDanList1.ColumnAutoResize = true;
            gridEXLayout1.LayoutString = resources.GetString("gridEXLayout1.LayoutString");
            this.gxGiaoDanList1.DesignTimeLayout = gridEXLayout1;
            this.gxGiaoDanList1.DisableParentOnLoadData = true;
            this.gxGiaoDanList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGiaoDanList1.DynamicFiltering = true;
            this.gxGiaoDanList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaoDanList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaoDanList1.GroupByBoxVisible = false;
            this.gxGiaoDanList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaoDanList1.Location = new System.Drawing.Point(3, 49);
            this.gxGiaoDanList1.Name = "gxGiaoDanList1";
            this.gxGiaoDanList1.QueryString = "";
            this.gxGiaoDanList1.RecordNavigator = true;
            this.gxGiaoDanList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaoDanList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxGiaoDanList1.Size = new System.Drawing.Size(884, 471);
            this.gxGiaoDanList1.TabIndex = 3;
            this.gxGiaoDanList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaoDanList1.WhereSQL = "";
            this.gxGiaoDanList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gxGiaoDanList1_RowDoubleClick);
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.BackgroundStyle = Janus.Windows.EditControls.BackgroundStyle.ExplorerBarBackground;
            this.uiGroupBox3.Controls.Add(this.gxAddEdit1);
            this.uiGroupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox3.FrameStyle = Janus.Windows.EditControls.FrameStyle.None;
            this.uiGroupBox3.Location = new System.Drawing.Point(3, 16);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Size = new System.Drawing.Size(884, 33);
            this.uiGroupBox3.TabIndex = 2;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = this.gxGiaoDanList1;
            this.gxAddEdit1.Location = new System.Drawing.Point(5, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(391, 29);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm giáo dân";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Xóa giáo dân được chọn";
            this.gxAddEdit1.ToolTipEdit = "Sửa giáo dân được chọn";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "";
            this.gxAddEdit1.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
                "ếm";
            this.gxAddEdit1.ToolTipSelect = "Chọn giáo dân";
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            // 
            // gxCommand1
            // 
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Mode1;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 553);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(890, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "";
            this.gxCommand1.ToolTipOK = "";
            this.gxCommand1.Visible = false;
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.chkGiaoDanAo);
            this.uiGroupBox2.Controls.Add(this.lblTotal);
            this.uiGroupBox2.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(890, 75);
            this.uiGroupBox2.TabIndex = 0;
            this.uiGroupBox2.Text = "Giáo họ";
            // 
            // chkGiaoDanAo
            // 
            this.chkGiaoDanAo.AutoSize = true;
            this.chkGiaoDanAo.Location = new System.Drawing.Point(54, 50);
            this.chkGiaoDanAo.Name = "chkGiaoDanAo";
            this.chkGiaoDanAo.Size = new System.Drawing.Size(213, 17);
            this.chkGiaoDanAo.TabIndex = 11;
            this.chkGiaoDanAo.Text = "Chỉ xem giáo dân không được thống kê";
            this.toolTip1.SetToolTip(this.chkGiaoDanAo, "Giáo dân không được thống kê là giáo dân ngoài xứ, không được tính trong các mục thống kê");
            this.chkGiaoDanAo.UseVisualStyleBackColor = true;
            this.chkGiaoDanAo.CheckedChanged += new System.EventHandler(this.chkGiaoDanAo_CheckedChanged);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTotal.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblTotal.Location = new System.Drawing.Point(318, 24);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(72, 13);
            this.lblTotal.TabIndex = 10;
            this.lblTotal.Text = "Tổng cộng:";
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
            this.cbGiaoHo.IsLuuTru = false;
            this.cbGiaoHo.Label = "Giáo họ";
            this.cbGiaoHo.Location = new System.Drawing.Point(7, 17);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = true;
            this.cbGiaoHo.Size = new System.Drawing.Size(305, 25);
            this.cbGiaoHo.TabIndex = 0;
            this.cbGiaoHo.WhereSQL = "";
            // 
            // frmGiaoDanLuuTruList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(890, 598);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.uiGroupBox2);
            this.Name = "frmGiaoDanLuuTruList";
            this.Text = "Danh sách hồ sơ lưu trữ giáo dân";
            this.Load += new System.EventHandler(this.frmGiaoDanList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaoDanList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).EndInit();
            this.uiGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxGroupBox uiGroupBox3;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxGiaoHo cbGiaoHo;
        private GxControl.GxGiaoDanList gxGiaoDanList1;
        private System.Windows.Forms.Label lblTotal;
        private GxControl.GxCheckBox chkGiaoDanAo;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}