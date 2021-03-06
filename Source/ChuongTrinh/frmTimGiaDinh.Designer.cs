namespace GiaoXu
{
    partial class frmTimGiaDinh
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
            this.txtTuKhoa = new GxControl.GxSearchBox();
            this.gxCommand1 = new GxControl.GxCommand();
            this.txtSoHoKhau = new GxControl.GxSearchBox();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).BeginInit();
            this.uiGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiGroupBox1
            // 
            this.uiGroupBox1.Controls.Add(this.txtSoHoKhau);
            this.uiGroupBox1.Controls.Add(this.cbGiaoHo);
            this.uiGroupBox1.Controls.Add(this.txtTuKhoa);
            this.uiGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox1.Name = "uiGroupBox1";
            this.uiGroupBox1.Size = new System.Drawing.Size(450, 132);
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
            this.cbGiaoHo.Location = new System.Drawing.Point(12, 88);
            this.cbGiaoHo.MaGiaoHo = -1;
            this.cbGiaoHo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbGiaoHo.Name = "cbGiaoHo";
            this.cbGiaoHo.SelectedValue = null;
            this.cbGiaoHo.ShowNgoaiXu = true;
            this.cbGiaoHo.Size = new System.Drawing.Size(258, 27);
            this.cbGiaoHo.TabIndex = 2;
            this.cbGiaoHo.WhereSQL = "";
            this.cbGiaoHo.Load += new System.EventHandler(this.cbGiaoHo_Load);
            // 
            // txtTuKhoa
            // 
            this.txtTuKhoa.AutoCompleteEnabled = true;
            this.txtTuKhoa.AutoUpperFirstChar = false;
            this.txtTuKhoa.BoxLeft = 80;
            this.txtTuKhoa.EditEnabled = true;
            this.txtTuKhoa.Label = "Tên giáo dân";
            this.txtTuKhoa.Location = new System.Drawing.Point(12, 55);
            this.txtTuKhoa.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtTuKhoa.MaxLength = 255;
            this.txtTuKhoa.MultiLine = false;
            this.txtTuKhoa.Name = "txtTuKhoa";
            this.txtTuKhoa.NumberInputRequired = true;
            this.txtTuKhoa.NumberMode = false;
            this.txtTuKhoa.ReadOnly = false;
            this.txtTuKhoa.Size = new System.Drawing.Size(407, 25);
            this.txtTuKhoa.TabIndex = 0;
            this.txtTuKhoa.Load += new System.EventHandler(this.txtTuKhoa_Load);
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 132);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(450, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            this.gxCommand1.Load += new System.EventHandler(this.gxCommand1_Load);
            // 
            // txtSoHoKhau
            // 
            this.txtSoHoKhau.AutoCompleteEnabled = true;
            this.txtSoHoKhau.AutoUpperFirstChar = false;
            this.txtSoHoKhau.BoxLeft = 80;
            this.txtSoHoKhau.EditEnabled = true;
            this.txtSoHoKhau.Label = "Số hộ khẩu";
            this.txtSoHoKhau.Location = new System.Drawing.Point(12, 22);
            this.txtSoHoKhau.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSoHoKhau.MaxLength = 255;
            this.txtSoHoKhau.MultiLine = false;
            this.txtSoHoKhau.Name = "txtSoHoKhau";
            this.txtSoHoKhau.NumberInputRequired = true;
            this.txtSoHoKhau.NumberMode = false;
            this.txtSoHoKhau.ReadOnly = false;
            this.txtSoHoKhau.Size = new System.Drawing.Size(407, 25);
            this.txtSoHoKhau.TabIndex = 3;
            // 
            // frmTimGiaDinh
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 177);
            this.Controls.Add(this.uiGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmTimGiaDinh";
            this.Text = "Tìm gia đình của giáo dân";
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox1)).EndInit();
            this.uiGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxSearchBox txtTuKhoa;
        private GxControl.GxGiaoHo cbGiaoHo;
        private GxControl.GxSearchBox txtSoHoKhau;
    }
}