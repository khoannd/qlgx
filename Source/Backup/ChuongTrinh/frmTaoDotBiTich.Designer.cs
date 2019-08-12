namespace GiaoXu
{
    partial class frmTaoDotBiTich
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTaoDotBiTich));
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.txtKetQua = new GxControl.GxTextField();
            this.dtDateTo = new GxControl.GxDateField();
            this.dtDateFrom = new GxControl.GxDateField();
            this.cbLoaiBiTich = new GxControl.GxLoaiBiTich();
            this.gxCommand1 = new GxControl.GxCommand();
            this.txtNoiBiTich = new GxControl.GxTextField();
            this.txtLinhMuc = new GxControl.GxLinhMuc();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.txtLinhMuc);
            this.gxGroupBox1.Controls.Add(this.txtNoiBiTich);
            this.gxGroupBox1.Controls.Add(this.txtKetQua);
            this.gxGroupBox1.Controls.Add(this.dtDateTo);
            this.gxGroupBox1.Controls.Add(this.dtDateFrom);
            this.gxGroupBox1.Controls.Add(this.cbLoaiBiTich);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(529, 291);
            this.gxGroupBox1.TabIndex = 0;
            this.gxGroupBox1.Text = "Điều kiện tạo";
            // 
            // txtKetQua
            // 
            this.txtKetQua.AutoCompleteEnabled = true;
            this.txtKetQua.AutoUpperFirstChar = false;
            this.txtKetQua.BoxLeft = 100;
            this.txtKetQua.EditEnabled = true;
            this.txtKetQua.Label = "Kết quả";
            this.txtKetQua.Location = new System.Drawing.Point(11, 187);
            this.txtKetQua.MaxLength = 32767;
            this.txtKetQua.MultiLine = true;
            this.txtKetQua.Name = "txtKetQua";
            this.txtKetQua.NumberInputRequired = true;
            this.txtKetQua.NumberMode = false;
            this.txtKetQua.ReadOnly = false;
            this.txtKetQua.Size = new System.Drawing.Size(512, 90);
            this.txtKetQua.TabIndex = 5;
            // 
            // dtDateTo
            // 
            this.dtDateTo.AutoUpperFirstChar = false;
            this.dtDateTo.BoxLeft = 100;
            this.dtDateTo.EditEnabled = true;
            this.dtDateTo.FullInputRequired = false;
            this.dtDateTo.IsNullDate = true;
            this.dtDateTo.Label = "Đến ngày";
            this.dtDateTo.Location = new System.Drawing.Point(10, 153);
            this.dtDateTo.Name = "dtDateTo";
            this.dtDateTo.Size = new System.Drawing.Size(215, 26);
            this.dtDateTo.TabIndex = 4;
            this.dtDateTo.Value = ((object)(resources.GetObject("dtDateTo.Value")));
            // 
            // dtDateFrom
            // 
            this.dtDateFrom.AutoUpperFirstChar = false;
            this.dtDateFrom.BoxLeft = 100;
            this.dtDateFrom.EditEnabled = true;
            this.dtDateFrom.FullInputRequired = false;
            this.dtDateFrom.IsNullDate = true;
            this.dtDateFrom.Label = "Từ ngày";
            this.dtDateFrom.Location = new System.Drawing.Point(10, 121);
            this.dtDateFrom.Name = "dtDateFrom";
            this.dtDateFrom.Size = new System.Drawing.Size(218, 26);
            this.dtDateFrom.TabIndex = 3;
            this.dtDateFrom.Value = ((object)(resources.GetObject("dtDateFrom.Value")));
            // 
            // cbLoaiBiTich
            // 
            this.cbLoaiBiTich.AutoUpperFirstChar = false;
            this.cbLoaiBiTich.BoxLeft = 100;
            this.cbLoaiBiTich.EditEnabled = true;
            this.cbLoaiBiTich.GridBiTichList = null;
            this.cbLoaiBiTich.Label = "Loai bí tích";
            this.cbLoaiBiTich.Location = new System.Drawing.Point(10, 23);
            this.cbLoaiBiTich.Name = "cbLoaiBiTich";
            this.cbLoaiBiTich.SelectedValue = null;
            this.cbLoaiBiTich.Size = new System.Drawing.Size(301, 27);
            this.cbLoaiBiTich.TabIndex = 0;
            this.cbLoaiBiTich.Value = -1;
            // 
            // gxCommand1
            // 
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 291);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(529, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnCancel += new System.EventHandler(this.gxCommand1_OnCancel);
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // txtNoiBiTich
            // 
            this.txtNoiBiTich.AutoCompleteEnabled = true;
            this.txtNoiBiTich.AutoUpperFirstChar = false;
            this.txtNoiBiTich.BoxLeft = 100;
            this.txtNoiBiTich.EditEnabled = true;
            this.txtNoiBiTich.Label = "Nơi nhận bí tích";
            this.txtNoiBiTich.Location = new System.Drawing.Point(10, 89);
            this.txtNoiBiTich.MaxLength = 32767;
            this.txtNoiBiTich.MultiLine = false;
            this.txtNoiBiTich.Name = "txtNoiBiTich";
            this.txtNoiBiTich.NumberInputRequired = true;
            this.txtNoiBiTich.NumberMode = false;
            this.txtNoiBiTich.ReadOnly = false;
            this.txtNoiBiTich.Size = new System.Drawing.Size(301, 26);
            this.txtNoiBiTich.TabIndex = 2;
            // 
            // txtLinhMuc
            // 
            this.txtLinhMuc.AutoCompleteEnabled = true;
            this.txtLinhMuc.AutoLoadData = true;
            this.txtLinhMuc.AutoUpperFirstChar = false;
            this.txtLinhMuc.BoxLeft = 100;
            this.txtLinhMuc.EditEnabled = true;
            this.txtLinhMuc.Label = "Người ban bí tích";
            this.txtLinhMuc.Location = new System.Drawing.Point(10, 57);
            this.txtLinhMuc.MaxLength = 32767;
            this.txtLinhMuc.MultiLine = false;
            this.txtLinhMuc.Name = "txtLinhMuc";
            this.txtLinhMuc.NumberInputRequired = true;
            this.txtLinhMuc.NumberMode = false;
            this.txtLinhMuc.ReadOnly = false;
            this.txtLinhMuc.Size = new System.Drawing.Size(301, 27);
            this.txtLinhMuc.TabIndex = 6;
            // 
            // frmTaoDotBiTich
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 336);
            this.Controls.Add(this.gxGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmTaoDotBiTich";
            this.Text = "Tạo đợt bí tích tự động";
            this.Load += new System.EventHandler(this.frmTaoDotBiTich_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxLoaiBiTich cbLoaiBiTich;
        private GxControl.GxDateField dtDateTo;
        private GxControl.GxDateField dtDateFrom;
        private GxControl.GxTextField txtKetQua;
        private GxControl.GxTextField txtNoiBiTich;
        private GxControl.GxLinhMuc txtLinhMuc;
    }
}