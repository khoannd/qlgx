namespace GiaoXu
{
    partial class frmTimGiaoDan
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
            this.cbGiaoHo = new GxControl.GxGiaoHo();
            this.txtTenMe = new GxControl.GxSearchBox();
            this.txtTenCha = new GxControl.GxSearchBox();
            this.txtSoRuaToi = new GxControl.GxSearchBox();
            this.txtTuKhoa = new GxControl.GxSearchBox();
            this.gxCommand1 = new GxControl.GxCommand();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox1.Controls.Add(this.txtTenMe);
            this.uiGroupBox1.Controls.Add(this.txtTenCha);
            this.uiGroupBox1.Controls.Add(this.txtSoRuaToi);
            this.uiGroupBox1.Controls.Add(this.txtTuKhoa);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(421, 222);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin tìm kiếm";
            // 
            // cbGiaoHo
            // 
            this.cbGiaoHo.AutoLoadGrid = true;
            this.cbGiaoHo.AutoUpperFirstChar = false;
            this.cbGiaoHo.BoxLeft = 80;
            this.cbGiaoHo.EditEnabled = true;
            this.cbGiaoHo.GridGiaDinh = null;
            this.cbGiaoHo.GridGiaoDan = null;
            this.cbGiaoHo.HasShowAll = false;
            this.cbGiaoHo.IsAo = Janus.Windows.GridEX.TriState.Empty;
            this.cbGiaoHo.IsLuuTru = false;
            this.cbGiaoHo.Label = "Giáo họ";
            this.cbGiaoHo.Location = new System.Drawing.Point(12, 141);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = true;
            this.cbGiaoHo.Size = new System.Drawing.Size(356, 27);
            this.cbGiaoHo.TabIndex = 4;
            this.cbGiaoHo.WhereSQL = "";
            // 
            // txtTenMe
            // 
            this.txtTenMe.AutoCompleteEnabled = true;
            this.txtTenMe.AutoUpperFirstChar = false;
            this.txtTenMe.BoxLeft = 80;
            this.txtTenMe.EditEnabled = true;
            this.txtTenMe.Label = "Tên Mẹ";
            this.txtTenMe.Location = new System.Drawing.Point(12, 111);
            this.txtTenMe.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenMe.MaxLength = 255;
            this.txtTenMe.MultiLine = false;
            this.txtTenMe.Name = "txtTenMe";
            this.txtTenMe.NumberInputRequired = true;
            this.txtTenMe.NumberMode = false;
            this.txtTenMe.ReadOnly = false;
            this.txtTenMe.Size = new System.Drawing.Size(356, 25);
            this.txtTenMe.TabIndex = 3;
            // 
            // txtTenCha
            // 
            this.txtTenCha.AutoCompleteEnabled = true;
            this.txtTenCha.AutoUpperFirstChar = false;
            this.txtTenCha.BoxLeft = 80;
            this.txtTenCha.EditEnabled = true;
            this.txtTenCha.Label = "Tên Cha";
            this.txtTenCha.Location = new System.Drawing.Point(12, 81);
            this.txtTenCha.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenCha.MaxLength = 255;
            this.txtTenCha.MultiLine = false;
            this.txtTenCha.Name = "txtTenCha";
            this.txtTenCha.NumberInputRequired = true;
            this.txtTenCha.NumberMode = false;
            this.txtTenCha.ReadOnly = false;
            this.txtTenCha.Size = new System.Drawing.Size(356, 25);
            this.txtTenCha.TabIndex = 2;
            // 
            // txtSoRuaToi
            // 
            this.txtSoRuaToi.AutoCompleteEnabled = true;
            this.txtSoRuaToi.AutoUpperFirstChar = false;
            this.txtSoRuaToi.BoxLeft = 80;
            this.txtSoRuaToi.EditEnabled = true;
            this.txtSoRuaToi.Label = "Số rửa tội";
            this.txtSoRuaToi.Location = new System.Drawing.Point(12, 50);
            this.txtSoRuaToi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSoRuaToi.MaxLength = 255;
            this.txtSoRuaToi.MultiLine = false;
            this.txtSoRuaToi.Name = "txtSoRuaToi";
            this.txtSoRuaToi.NumberInputRequired = true;
            this.txtSoRuaToi.NumberMode = false;
            this.txtSoRuaToi.ReadOnly = false;
            this.txtSoRuaToi.Size = new System.Drawing.Size(356, 25);
            this.txtSoRuaToi.TabIndex = 1;
            // 
            // txtTuKhoa
            // 
            this.txtTuKhoa.AutoCompleteEnabled = true;
            this.txtTuKhoa.AutoUpperFirstChar = false;
            this.txtTuKhoa.BoxLeft = 80;
            this.txtTuKhoa.EditEnabled = true;
            this.txtTuKhoa.Label = "Tên giáo dân";
            this.txtTuKhoa.Location = new System.Drawing.Point(12, 19);
            this.txtTuKhoa.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTuKhoa.MaxLength = 255;
            this.txtTuKhoa.MultiLine = false;
            this.txtTuKhoa.Name = "txtTuKhoa";
            this.txtTuKhoa.NumberInputRequired = true;
            this.txtTuKhoa.NumberMode = false;
            this.txtTuKhoa.ReadOnly = false;
            this.txtTuKhoa.Size = new System.Drawing.Size(356, 25);
            this.txtTuKhoa.TabIndex = 0;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 222);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(421, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // frmTimGiaoDan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 267);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmTimGiaoDan";
            this.Text = "Tìm giáo dân";
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxSearchBox txtTuKhoa;
        private GxControl.GxGiaoHo cbGiaoHo;
        private GxControl.GxSearchBox txtSoRuaToi;
        private GxControl.GxSearchBox txtTenMe;
        private GxControl.GxSearchBox txtTenCha;
    }
}