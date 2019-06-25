namespace GiaoXu
{
    partial class frmHoiDoanList
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
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.uiGroupBox3 = new GxControl.GxGroupBox();
            this.gxListHoiDoan1 = new GxControl.GxListHoiDoan();
            this.uiGroupBox2 = new GxControl.GxGroupBox();
            this.gxAddEdit1 = new GxControl.GxAddEdit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).BeginInit();
            this.uiGroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxListHoiDoan1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).BeginInit();
            this.uiGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.uiGroupBox3);
            this.uiGroupBox1.Controls.Add(this.uiGroupBox2);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(800, 450);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Danh sách hội đoàn";
            // 
            // uiGroupBox3
            // 
            this.uiGroupBox3.Controls.Add(this.gxListHoiDoan1);
            this.uiGroupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox3.Location = new System.Drawing.Point(3, 61);
            this.uiGroupBox3.Name = "uiGroupBox3";
            this.uiGroupBox3.Size = new System.Drawing.Size(794, 386);
            this.uiGroupBox3.TabIndex = 1;
            // 
            // gxListHoiDoan1
            // 
            this.gxListHoiDoan1.AllowColumnDrag = false;
            this.gxListHoiDoan1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gxListHoiDoan1.AllowRemoveColumns = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListHoiDoan1.Arguments = null;
            this.gxListHoiDoan1.AutoLoadGridFormat = true;
            this.gxListHoiDoan1.ColumnAutoResize = true;
            this.gxListHoiDoan1.DisableParentOnLoadData = true;
            this.gxListHoiDoan1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxListHoiDoan1.DynamicFiltering = true;
            this.gxListHoiDoan1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxListHoiDoan1.GroupByBoxVisible = false;
            this.gxListHoiDoan1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxListHoiDoan1.Location = new System.Drawing.Point(3, 8);
            this.gxListHoiDoan1.Name = "gxListHoiDoan1";
            this.gxListHoiDoan1.QueryString = "";
            this.gxListHoiDoan1.RecordNavigator = true;
            this.gxListHoiDoan1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxListHoiDoan1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gxListHoiDoan1.SelectionMode = Janus.Windows.GridEX.SelectionMode.MultipleSelection;
            this.gxListHoiDoan1.Size = new System.Drawing.Size(788, 375);
            this.gxListHoiDoan1.TabIndex = 0;
            this.gxListHoiDoan1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxListHoiDoan1.WhereSQL = "";
            this.gxListHoiDoan1.SelectionChanged += new System.EventHandler(this.gxListHoiDoan1_SelectionChanged);
            // 
            // uiGroupBox2
            // 
            this.uiGroupBox2.Controls.Add(this.gxAddEdit1);
            this.uiGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.uiGroupBox2.Location = new System.Drawing.Point(3, 16);
            this.uiGroupBox2.Name = "uiGroupBox2";
            this.uiGroupBox2.Size = new System.Drawing.Size(794, 45);
            this.uiGroupBox2.TabIndex = 0;
            // 
            // gxAddEdit1
            // 
            this.gxAddEdit1.BackColor = System.Drawing.Color.Transparent;
            this.gxAddEdit1.CaptionDisplayMode = GxGlobal.CaptionDisplayMode.Full;
            this.gxAddEdit1.DisplayMode = GxGlobal.DisplayMode.Mode2;
            this.gxAddEdit1.GridData = null;
            this.gxAddEdit1.Location = new System.Drawing.Point(6, 11);
            this.gxAddEdit1.Name = "gxAddEdit1";
            this.gxAddEdit1.Size = new System.Drawing.Size(391, 28);
            this.gxAddEdit1.TabIndex = 0;
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
            this.gxAddEdit1.AddClick += new System.EventHandler(this.gxAddEdit1_AddClick);
            this.gxAddEdit1.EditClick += new System.EventHandler(this.gxAddEdit1_EditClick);
            this.gxAddEdit1.DeleteClick += new System.EventHandler(this.gxAddEdit1_DeleteClick);
            this.gxAddEdit1.ReloadClick += new System.EventHandler(this.gxAddEdit1_ReloadClick);
            // 
            // frmHoiDoanList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.uiGroupBox1);
            this.Name = "frmHoiDoanList";
            this.Text = "Danh sách hội đoàn";
            this.Load += new System.EventHandler(this.frmHoiDoanList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox3)).EndInit();
            this.uiGroupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxListHoiDoan1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox2)).EndInit();
            this.uiGroupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxGroupBox uiGroupBox3;
        protected GxControl.GxGroupBox uiGroupBox2;
        private GxControl.GxAddEdit gxAddEdit1;
        private GxControl.GxListHoiDoan gxListHoiDoan1;
    }
}