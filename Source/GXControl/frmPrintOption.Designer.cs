namespace GxControl
{
    partial class frmPrint
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
            this.pnRadio = new GxControl.GxGroupBox();
            this.gxCommand1 = new GxControl.GxCommand();
            this.SING = new GxControl.GxRadioBox();
            this.MUL = new GxControl.GxRadioBox();
            ((System.ComponentModel.ISupportInitialize)(this.pnRadio)).BeginInit();
            this.pnRadio.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnRadio
            // 
            this.pnRadio.Controls.Add(this.MUL);
            this.pnRadio.Controls.Add(this.SING);
            this.pnRadio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnRadio.Location = new System.Drawing.Point(0, 0);
            this.pnRadio.Name = "pnRadio";
            this.pnRadio.Size = new System.Drawing.Size(254, 111);
            this.pnRadio.TabIndex = 0;
            this.pnRadio.Text = "Chọn kiểu in dữ liệu";
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 111);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(254, 50);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            // 
            // SING
            // 
            this.SING.AutoSize = true;
            this.SING.Checked = true;
            this.SING.Location = new System.Drawing.Point(76, 38);
            this.SING.Name = "SING";
            this.SING.Size = new System.Drawing.Size(100, 17);
            this.SING.TabIndex = 0;
            this.SING.TabStop = true;
            this.SING.Text = "In riêng từng file";
            this.SING.UseVisualStyleBackColor = true;
            // 
            // MUL
            // 
            this.MUL.AutoSize = true;
            this.MUL.Location = new System.Drawing.Point(76, 61);
            this.MUL.Name = "MUL";
            this.MUL.Size = new System.Drawing.Size(103, 17);
            this.MUL.TabIndex = 0;
            this.MUL.Text = "In chung một file";
            this.MUL.UseVisualStyleBackColor = true;
            // 
            // frmPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 161);
            this.Controls.Add(this.pnRadio);
            this.Controls.Add(this.gxCommand1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPrint";
            this.Text = "In dữ liệu";
            ((System.ComponentModel.ISupportInitialize)(this.pnRadio)).EndInit();
            this.pnRadio.ResumeLayout(false);
            this.pnRadio.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxGroupBox pnRadio;
        private GxRadioBox MUL;
        private GxRadioBox SING;
        private GxCommand gxCommand1;
    }
}