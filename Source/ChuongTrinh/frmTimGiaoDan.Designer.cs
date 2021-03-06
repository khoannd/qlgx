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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTimGiaoDan));
            this.uiGroupBox1 = new GxControl.GxGroupBox();
            this.txtNgaySinh = new GxControl.GxDateField();
            this.txtDiaChi = new GxControl.GxSearchBox();
            this.txtCmnd = new GxControl.GxSearchBox();
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
            this.uiGroupBox1.Controls.Add(this.txtNgaySinh);
            this.uiGroupBox1.Controls.Add(this.txtDiaChi);
            this.uiGroupBox1.Controls.Add(this.txtCmnd);
            this.uiGroupBox1.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox1.Controls.Add(this.txtTenMe);
            this.uiGroupBox1.Controls.Add(this.txtTenCha);
            this.uiGroupBox1.Controls.Add(this.txtSoRuaToi);
            this.uiGroupBox1.Controls.Add(this.txtTuKhoa);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(421, 290);
            this.uiGroupBox1.TabIndex = 0;
            this.uiGroupBox1.Text = "Thông tin tìm kiếm";
            // 
            // txtNgaySinh
            // 
            this.txtNgaySinh.AutoUpperFirstChar = false;
            this.txtNgaySinh.BoxLeft = 0;
            this.txtNgaySinh.EditEnabled = true;
            this.txtNgaySinh.FullInputRequired = false;
            this.txtNgaySinh.IsNullDate = true;
            this.txtNgaySinh.Label = "Ngày sinh";
            this.txtNgaySinh.Location = new System.Drawing.Point(35, 91);
            this.txtNgaySinh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNgaySinh.Name = "txtNgaySinh";
            this.txtNgaySinh.Size = new System.Drawing.Size(233, 27);
            this.txtNgaySinh.TabIndex = 2;
            this.txtNgaySinh.Value = ((object)(resources.GetObject("txtNgaySinh.Value")));
            // 
            // txtDiaChi
            // 
            this.txtDiaChi.AutoCompleteEnabled = true;
            this.txtDiaChi.AutoUpperFirstChar = false;
            this.txtDiaChi.BoxLeft = 80;
            this.txtDiaChi.EditEnabled = true;
            this.txtDiaChi.Label = "Địa chỉ";
            this.txtDiaChi.Location = new System.Drawing.Point(12, 253);
            this.txtDiaChi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtDiaChi.MaxLength = 255;
            this.txtDiaChi.MultiLine = false;
            this.txtDiaChi.Name = "txtDiaChi";
            this.txtDiaChi.NumberInputRequired = true;
            this.txtDiaChi.NumberMode = false;
            this.txtDiaChi.ReadOnly = false;
            this.txtDiaChi.Size = new System.Drawing.Size(356, 25);
            this.txtDiaChi.TabIndex = 7;
            // 
            // txtCmnd
            // 
            this.txtCmnd.AutoCompleteEnabled = true;
            this.txtCmnd.AutoUpperFirstChar = false;
            this.txtCmnd.BoxLeft = 80;
            this.txtCmnd.EditEnabled = true;
            this.txtCmnd.Label = "CMND";
            this.txtCmnd.Location = new System.Drawing.Point(12, 126);
            this.txtCmnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtCmnd.MaxLength = 255;
            this.txtCmnd.MultiLine = false;
            this.txtCmnd.Name = "txtCmnd";
            this.txtCmnd.NumberInputRequired = true;
            this.txtCmnd.NumberMode = false;
            this.txtCmnd.ReadOnly = false;
            this.txtCmnd.Size = new System.Drawing.Size(356, 25);
            this.txtCmnd.TabIndex = 3;
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
            this.cbGiaoHo.IsHasLuuTru = false;
            this.cbGiaoHo.IsLuuTru = false;
            this.cbGiaoHo.Label = "Giáo họ";
            this.cbGiaoHo.Location = new System.Drawing.Point(12, 23);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = true;
            this.cbGiaoHo.Size = new System.Drawing.Size(356, 27);
            this.cbGiaoHo.TabIndex = 0;
            this.cbGiaoHo.WhereSQL = "";
            // 
            // txtTenMe
            // 
            this.txtTenMe.AutoCompleteEnabled = true;
            this.txtTenMe.AutoUpperFirstChar = false;
            this.txtTenMe.BoxLeft = 80;
            this.txtTenMe.EditEnabled = true;
            this.txtTenMe.Label = "Tên Mẹ";
            this.txtTenMe.Location = new System.Drawing.Point(12, 220);
            this.txtTenMe.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenMe.MaxLength = 255;
            this.txtTenMe.MultiLine = false;
            this.txtTenMe.Name = "txtTenMe";
            this.txtTenMe.NumberInputRequired = true;
            this.txtTenMe.NumberMode = false;
            this.txtTenMe.ReadOnly = false;
            this.txtTenMe.Size = new System.Drawing.Size(356, 25);
            this.txtTenMe.TabIndex = 6;
            // 
            // txtTenCha
            // 
            this.txtTenCha.AutoCompleteEnabled = true;
            this.txtTenCha.AutoUpperFirstChar = false;
            this.txtTenCha.BoxLeft = 80;
            this.txtTenCha.EditEnabled = true;
            this.txtTenCha.Label = "Tên Cha";
            this.txtTenCha.Location = new System.Drawing.Point(12, 190);
            this.txtTenCha.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTenCha.MaxLength = 255;
            this.txtTenCha.MultiLine = false;
            this.txtTenCha.Name = "txtTenCha";
            this.txtTenCha.NumberInputRequired = true;
            this.txtTenCha.NumberMode = false;
            this.txtTenCha.ReadOnly = false;
            this.txtTenCha.Size = new System.Drawing.Size(356, 25);
            this.txtTenCha.TabIndex = 5;
            // 
            // txtSoRuaToi
            // 
            this.txtSoRuaToi.AutoCompleteEnabled = true;
            this.txtSoRuaToi.AutoUpperFirstChar = false;
            this.txtSoRuaToi.BoxLeft = 80;
            this.txtSoRuaToi.EditEnabled = true;
            this.txtSoRuaToi.Label = "Số rửa tội";
            this.txtSoRuaToi.Location = new System.Drawing.Point(12, 159);
            this.txtSoRuaToi.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSoRuaToi.MaxLength = 255;
            this.txtSoRuaToi.MultiLine = false;
            this.txtSoRuaToi.Name = "txtSoRuaToi";
            this.txtSoRuaToi.NumberInputRequired = true;
            this.txtSoRuaToi.NumberMode = false;
            this.txtSoRuaToi.ReadOnly = false;
            this.txtSoRuaToi.Size = new System.Drawing.Size(356, 25);
            this.txtSoRuaToi.TabIndex = 4;
            // 
            // txtTuKhoa
            // 
            this.txtTuKhoa.AutoCompleteEnabled = true;
            this.txtTuKhoa.AutoUpperFirstChar = false;
            this.txtTuKhoa.BoxLeft = 80;
            this.txtTuKhoa.EditEnabled = true;
            this.txtTuKhoa.Label = "Tên giáo dân";
            this.txtTuKhoa.Location = new System.Drawing.Point(12, 58);
            this.txtTuKhoa.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTuKhoa.MaxLength = 255;
            this.txtTuKhoa.MultiLine = false;
            this.txtTuKhoa.Name = "txtTuKhoa";
            this.txtTuKhoa.NumberInputRequired = true;
            this.txtTuKhoa.NumberMode = false;
            this.txtTuKhoa.ReadOnly = false;
            this.txtTuKhoa.Size = new System.Drawing.Size(356, 25);
            this.txtTuKhoa.TabIndex = 1;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 290);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(421, 45);
            this.gxCommand1.TabIndex = 8;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // frmTimGiaoDan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 335);
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
        private GxControl.GxSearchBox txtCmnd;
        private GxControl.GxDateField txtNgaySinh;
        private GxControl.GxSearchBox txtDiaChi;
    }
}