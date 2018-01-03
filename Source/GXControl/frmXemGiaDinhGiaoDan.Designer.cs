namespace GxControl
{
    partial class frmXemGiaDinhGiaoDan
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
            this.gxGiaDinhList1 = new GxControl.GxGiaDinhList();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaDinhList1)).BeginInit();
            this.SuspendLayout();
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
            this.gxGiaDinhList1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGiaDinhList1.DynamicFiltering = true;
            this.gxGiaDinhList1.FilterMode = Janus.Windows.GridEX.FilterMode.Automatic;
            this.gxGiaDinhList1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gxGiaDinhList1.GroupByBoxVisible = false;
            this.gxGiaDinhList1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gxGiaDinhList1.Location = new System.Drawing.Point(0, 0);
            this.gxGiaDinhList1.Name = "gxGiaDinhList1";
            this.gxGiaDinhList1.Operation = GxGlobal.GxOperation.EDIT;
            this.gxGiaDinhList1.QueryString = "SELECT * FROM SELECT_GIADINH_LIST WHERE MaGiaDinh<>NULL ";
            this.gxGiaDinhList1.RowHeaderContent = Janus.Windows.GridEX.RowHeaderContent.RowPosition;
            this.gxGiaDinhList1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.Default;
            this.gxGiaDinhList1.Size = new System.Drawing.Size(769, 453);
            this.gxGiaDinhList1.TabIndex = 0;
            this.gxGiaDinhList1.VisualStyle = Janus.Windows.GridEX.VisualStyle.Office2003;
            this.gxGiaDinhList1.WhereSQL = "";
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 453);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(769, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            // 
            // frmXemGiaDinhGiaoDan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(769, 498);
            this.Controls.Add(this.gxGiaDinhList1);
            this.Controls.Add(this.gxCommand1);
            this.Name = "frmXemGiaDinhGiaoDan";
            this.Text = "Xem gia dinh giao dan";
            this.Load += new System.EventHandler(this.frmXemGiaDinhGiaoDan_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gxGiaDinhList1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private GxGiaDinhList gxGiaDinhList1;
        private GxCommand gxCommand1;
    }
}