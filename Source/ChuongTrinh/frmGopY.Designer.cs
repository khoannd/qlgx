namespace GiaoXu
{
    partial class frmGopY
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
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtYKien = new GxControl.GxTextField();
            this.txtHoTen = new GxControl.GxTextField();
            this.txtEmail = new GxControl.GxTextField();
            this.gxCommand1 = new GxControl.GxCommand();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.label3);
            this.gxGroupBox1.Controls.Add(this.label2);
            this.gxGroupBox1.Controls.Add(this.label1);
            this.gxGroupBox1.Controls.Add(this.txtYKien);
            this.gxGroupBox1.Controls.Add(this.txtHoTen);
            this.gxGroupBox1.Controls.Add(this.txtEmail);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(562, 384);
            this.gxGroupBox1.TabIndex = 0;
            this.gxGroupBox1.Text = "Thông tin liên hệ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 354);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(278, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Rất vui lòng khi nhận được các thông tin liên hệ từ quý vị";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Thông tin liên hệ";
            // 
            // txtYKien
            // 
            this.txtYKien.AutoCompleteEnabled = true;
            this.txtYKien.AutoUpperFirstChar = false;
            this.txtYKien.BoxLeft = 0;
            this.txtYKien.EditEnabled = true;
            this.txtYKien.Label = "";
            this.txtYKien.Location = new System.Drawing.Point(12, 110);
            this.txtYKien.MaxLength = 32767;
            this.txtYKien.MultiLine = true;
            this.txtYKien.Name = "txtYKien";
            this.txtYKien.NumberInputRequired = true;
            this.txtYKien.NumberMode = false;
            this.txtYKien.ReadOnly = false;
            this.txtYKien.Size = new System.Drawing.Size(538, 234);
            this.txtYKien.TabIndex = 2;
            // 
            // txtHoTen
            // 
            this.txtHoTen.AutoCompleteEnabled = true;
            this.txtHoTen.AutoUpperFirstChar = true;
            this.txtHoTen.BoxLeft = 60;
            this.txtHoTen.EditEnabled = true;
            this.txtHoTen.Label = "Quý danh";
            this.txtHoTen.Location = new System.Drawing.Point(12, 51);
            this.txtHoTen.MaxLength = 255;
            this.txtHoTen.MultiLine = false;
            this.txtHoTen.Name = "txtHoTen";
            this.txtHoTen.NumberInputRequired = true;
            this.txtHoTen.NumberMode = false;
            this.txtHoTen.ReadOnly = false;
            this.txtHoTen.Size = new System.Drawing.Size(248, 26);
            this.txtHoTen.TabIndex = 1;
            // 
            // txtEmail
            // 
            this.txtEmail.AutoCompleteEnabled = true;
            this.txtEmail.AutoUpperFirstChar = false;
            this.txtEmail.BoxLeft = 60;
            this.txtEmail.EditEnabled = true;
            this.txtEmail.Label = "Email";
            this.txtEmail.Location = new System.Drawing.Point(12, 19);
            this.txtEmail.MaxLength = 255;
            this.txtEmail.MultiLine = false;
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.NumberInputRequired = true;
            this.txtEmail.NumberMode = false;
            this.txtEmail.ReadOnly = false;
            this.txtEmail.Size = new System.Drawing.Size(248, 26);
            this.txtEmail.TabIndex = 0;
            // 
            // gxCommand1
            // 
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 384);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(562, 49);
            this.gxCommand1.TabIndex = 3;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(261, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(293, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "(Xin vui lòng nhập chính xác để nhận được trả lời qua email)";
            // 
            // frmGopY
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 433);
            this.ControlBox = false;
            this.Controls.Add(this.gxGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmGopY";
            this.Text = "Liên hệ";
            this.Load += new System.EventHandler(this.frmGopY_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.gxGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private GxControl.GxTextField txtYKien;
        private GxControl.GxTextField txtHoTen;
        private GxControl.GxTextField txtEmail;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}