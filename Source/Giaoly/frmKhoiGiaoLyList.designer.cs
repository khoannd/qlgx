namespace GiaoLy
{
    partial class frmKhoiGiaoLyList
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
            Janus.Windows.GridEX.GridEXLayout gxKhoiGiaoLyList1_DesignTimeLayout = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmKhoiGiaoLyList));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.gxKhoiGiaoLyList1 = new GxControl.GxKhoiGiaoLyList();
            this.uiGroupBox3 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cbNam = new Janus.Windows.EditControls.UIComboBox();
            this.lblNam = new System.Windows.Forms.Label();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxKhoiGiaoLyList1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).BeginInit();
            this.uiGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.gxKhoiGiaoLyList1);
            this.uiGroupBox1.Controls.Add(this.uiGroupBox3);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 43);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(890, 555);
            this.uiGroupBox1.TabIndex = 1;
            this.uiGroupBox1.Text = "Danh sách khối giáo lý";
            // 
            // gxKhoiGiaoLyList1
            // 
            this.gxKhoiGiaoLyList1.AcceptsEscape = false;
            this.gxKhoiGiaoLyList1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxKhoiGiaoLyList1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxKhoiGiaoLyList1.Arguments = null;
            this.gxKhoiGiaoLyList1.AutoLoadGridFormat = true;
            this.gxKhoiGiaoLyList1.ColumnAutoResize = true;
            gxKhoiGiaoLyList1_DesignTimeLayout.LayoutString = resources.GetString("gxKhoiGiaoLyList1_DesignTimeLayout.LayoutString");
            this.gxKhoiGiaoLyList1.DesignTimeLayout = gxKhoiGiaoLyList1_DesignTimeLayout;
            this.gxKhoiGiaoLyList1.DisableParentOnLoadData = true;
            this.gxKhoiGiaoLyList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxKhoiGiaoLyList1.DynamicFiltering = true;
            this.gxKhoiGiaoLyList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxKhoiGiaoLyList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxKhoiGiaoLyList1.GroupByBoxVisible = false;
            this.gxKhoiGiaoLyList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxKhoiGiaoLyList1.Location = new System.Drawing.Point(3, 49);
            this.gxKhoiGiaoLyList1.Name = "gxKhoiGiaoLyList1";
            this.gxKhoiGiaoLyList1.QueryString = "SELECT * FROM KHOIGIAOLY WHERE 1 ";
            this.gxKhoiGiaoLyList1.RecordNavigator = true;
            this.gxKhoiGiaoLyList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxKhoiGiaoLyList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxKhoiGiaoLyList1.SelectionMode = Janus.Windows.GridEX.SelectionMode.MultipleSelection;
            this.gxKhoiGiaoLyList1.Size = new System.Drawing.Size(884, 503);
            this.gxKhoiGiaoLyList1.TabIndex = 0;
            this.gxKhoiGiaoLyList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxKhoiGiaoLyList1.WhereSQL = "";
            this.gxKhoiGiaoLyList1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gxGiaDinhList1_RowDoubleClick);
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
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode3;
            this.gxAddEdit1.GridData = this.gxKhoiGiaoLyList1;
            this.gxAddEdit1.Location = new System.Drawing.Point(5, 3);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(213, 29);
            this.gxAddEdit1.TabIndex = 0;
            this.gxAddEdit1.ToolTipAdd = "Thêm khối giáo lý";
            this.gxAddEdit1.ToolTipButton1 = "";
            this.gxAddEdit1.ToolTipButton2 = "";
            this.gxAddEdit1.ToolTipDelete = "Xóa khối giáo lý được chọn";
            this.gxAddEdit1.ToolTipEdit = "Sửa khối giáo lý được chọn";
            this.gxAddEdit1.ToolTipFind = "";
            this.gxAddEdit1.ToolTipPrint = "";
            this.gxAddEdit1.ToolTipReload = "Lấy lại dữ liệu trong chương trình và hiện lên lưới như khi chưa thực hiện tìm ki" +
                "ếm";
            this.gxAddEdit1.ToolTipSelect = "In danh sách khối giáo lý trong lưới hiện tại";
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.PrintClick += new System.EventHandler(this.btnInDanhSach_Click);
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            // 
            // cbNam
            // 
            this.cbNam.ComboStyle = Janus.Windows.EditControls.ComboStyle.DropDownList;
            this.cbNam.Location = new System.Drawing.Point(65, 12);
            this.cbNam.Name = "cbNam";
            this.cbNam.Size = new System.Drawing.Size(84, 20);
            this.cbNam.TabIndex = 0;
            this.cbNam.SelectedIndexChanged += new System.EventHandler(this.cbNam_SelectedIndexChanged);
            // 
            // lblNam
            // 
            this.lblNam.AutoSize = true;
            this.lblNam.Location = new System.Drawing.Point(9, 16);
            this.lblNam.Name = "lblNam";
            this.lblNam.Size = new System.Drawing.Size(50, 13);
            this.lblNam.TabIndex = 1;
            this.lblNam.Text = "Năm học";
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.lblNam);
            this.uiGroupBox2.Controls.Add(this.cbNam);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox2.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(890, 43);
            this.uiGroupBox2.TabIndex = 0;
            // 
            // frmKhoiGiaoLyList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(890, 598);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.uiGroupBox2);
            this.Name = "frmKhoiGiaoLyList";
            this.Text = "Quản lý giáo lý";
            this.Load += new System.EventHandler(this.frmKhoiGiaoLyList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxKhoiGiaoLyList1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).EndInit();
            this.uiGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            this.uiGroupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        protected GxControl.GxGroupBox uiGroupBox1;
        protected System.Windows.Forms.ToolTip toolTip1;
        protected GxControl.GxKhoiGiaoLyList gxKhoiGiaoLyList1;
        protected GxControl.GxGroupBox uiGroupBox3;
        protected GxControl.GxAddEdit gxAddEdit1;
        private Janus.Windows.EditControls.UIComboBox cbNam;
        private System.Windows.Forms.Label lblNam;
        protected GxControl.GxGroupBox uiGroupBox2;
    }
}