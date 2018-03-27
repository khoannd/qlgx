namespace GxControl
{
    partial class frmReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmReport));
            this.txtGiaoXu = new GxControl.GxTextField();
            this.txtGiaoPhan = new GxControl.GxTextField();
            this.gxCommand1 = new GxControl.GxCommand();
            this.cbLinhMuc = new GxControl.GxComboField();
            this.SuspendLayout();
            // 
            // txtGiaoXu
            // 
            this.txtGiaoXu.AutoCompleteEnabled = true;
            this.txtGiaoXu.AutoUpperFirstChar = true;
            this.txtGiaoXu.BoxLeft = 0;
            this.txtGiaoXu.EditEnabled = true;
            this.txtGiaoXu.Label = "Giáo xứ nhận : ";
            resources.ApplyResources(this.txtGiaoXu, "txtGiaoXu");
            this.txtGiaoXu.MaxLength = 32767;
            this.txtGiaoXu.MultiLine = false;
            this.txtGiaoXu.Name = "txtGiaoXu";
            this.txtGiaoXu.NumberInputRequired = true;
            this.txtGiaoXu.NumberMode = false;
            this.txtGiaoXu.ReadOnly = false;
            // 
            // txtGiaoPhan
            // 
            this.txtGiaoPhan.AutoCompleteEnabled = true;
            this.txtGiaoPhan.AutoUpperFirstChar = true;
            this.txtGiaoPhan.BoxLeft = 0;
            this.txtGiaoPhan.EditEnabled = true;
            this.txtGiaoPhan.Label = "Giáo phận : ";
            resources.ApplyResources(this.txtGiaoPhan, "txtGiaoPhan");
            this.txtGiaoPhan.MaxLength = 32767;
            this.txtGiaoPhan.MultiLine = false;
            this.txtGiaoPhan.Name = "txtGiaoPhan";
            this.txtGiaoPhan.NumberInputRequired = true;
            this.txtGiaoPhan.NumberMode = false;
            this.txtGiaoPhan.ReadOnly = false;
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            resources.ApplyResources(this.gxCommand1, "gxCommand1");
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            // 
            // cbLinhMuc
            // 
            this.cbLinhMuc.AutoUpperFirstChar = false;
            this.cbLinhMuc.BoxLeft = 0;
            this.cbLinhMuc.EditEnabled = true;
            this.cbLinhMuc.Label = "Linh mục chứng : ";
            resources.ApplyResources(this.cbLinhMuc, "cbLinhMuc");
            this.cbLinhMuc.MaxLength = 0;
            this.cbLinhMuc.Name = "cbLinhMuc";
            this.cbLinhMuc.SelectedIndex = -1;
            this.cbLinhMuc.SelectedText = "";
            this.cbLinhMuc.SelectedValue = null;
            // 
            // frmReport
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbLinhMuc);
            this.Controls.Add(this.gxCommand1);
            this.Controls.Add(this.txtGiaoPhan);
            this.Controls.Add(this.txtGiaoXu);
            this.Name = "frmReport";
            this.Load += new System.EventHandler(this.frmGioiThieuRuaToi_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private GxTextField txtGiaoXu;
        private GxTextField txtGiaoPhan;
        private GxCommand gxCommand1;
        private GxComboField cbLinhMuc;
    }
}