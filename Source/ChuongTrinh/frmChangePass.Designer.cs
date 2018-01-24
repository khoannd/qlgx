namespace GiaoXu
{
    partial class frmChangePass
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmChangePass));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnCofirm = new GxControl.GxButton();
            this.txtOldPass = new GxControl.GxTextField();
            this.txtNewPass = new GxControl.GxTextField();
            this.txtAgainNewPass = new GxControl.GxTextField();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(453, 55);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(142, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(175, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "ĐỔI MẬT KHẨU";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnCofirm);
            this.panel2.Controls.Add(this.txtOldPass);
            this.panel2.Controls.Add(this.txtNewPass);
            this.panel2.Controls.Add(this.txtAgainNewPass);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 55);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(453, 191);
            this.panel2.TabIndex = 1;
            // 
            // btnCofirm
            // 
            this.btnCofirm.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnCofirm.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCofirm.BackgroundImage")));
            this.btnCofirm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCofirm.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnCofirm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnCofirm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnCofirm.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCofirm.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCofirm.Location = new System.Drawing.Point(121, 132);
            this.btnCofirm.Name = "btnCofirm";
            this.btnCofirm.Size = new System.Drawing.Size(222, 47);
            this.btnCofirm.TabIndex = 3;
            this.btnCofirm.Text = "Xác nhận";
            this.btnCofirm.UseVisualStyleBackColor = false;
            this.btnCofirm.Click += new System.EventHandler(this.btnCofirm_Click);
            // 
            // txtOldPass
            // 
            this.txtOldPass.AutoCompleteEnabled = false;
            this.txtOldPass.AutoUpperFirstChar = false;
            this.txtOldPass.BoxLeft = 0;
            this.txtOldPass.EditEnabled = true;
            this.txtOldPass.Label = "Mật khẩu cũ";
            this.txtOldPass.Location = new System.Drawing.Point(59, 13);
            this.txtOldPass.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtOldPass.MaxLength = 32767;
            this.txtOldPass.MultiLine = false;
            this.txtOldPass.Name = "txtOldPass";
            this.txtOldPass.NumberInputRequired = true;
            this.txtOldPass.NumberMode = false;
            this.txtOldPass.ReadOnly = false;
            this.txtOldPass.Size = new System.Drawing.Size(382, 26);
            this.txtOldPass.TabIndex = 0;
            this.txtOldPass.Load += new System.EventHandler(this.gxTextField1_Load);
            // 
            // txtNewPass
            // 
            this.txtNewPass.AutoCompleteEnabled = false;
            this.txtNewPass.AutoUpperFirstChar = false;
            this.txtNewPass.BoxLeft = 0;
            this.txtNewPass.EditEnabled = true;
            this.txtNewPass.Label = "Mật khẩu mới";
            this.txtNewPass.Location = new System.Drawing.Point(53, 47);
            this.txtNewPass.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtNewPass.MaxLength = 32767;
            this.txtNewPass.MultiLine = false;
            this.txtNewPass.Name = "txtNewPass";
            this.txtNewPass.NumberInputRequired = true;
            this.txtNewPass.NumberMode = false;
            this.txtNewPass.ReadOnly = false;
            this.txtNewPass.Size = new System.Drawing.Size(388, 26);
            this.txtNewPass.TabIndex = 1;
            // 
            // txtAgainNewPass
            // 
            this.txtAgainNewPass.AutoCompleteEnabled = false;
            this.txtAgainNewPass.AutoUpperFirstChar = false;
            this.txtAgainNewPass.BoxLeft = 0;
            this.txtAgainNewPass.EditEnabled = true;
            this.txtAgainNewPass.Label = "Nhập lại mật khẩu mới";
            this.txtAgainNewPass.Location = new System.Drawing.Point(12, 81);
            this.txtAgainNewPass.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtAgainNewPass.MaxLength = 32767;
            this.txtAgainNewPass.MultiLine = false;
            this.txtAgainNewPass.Name = "txtAgainNewPass";
            this.txtAgainNewPass.NumberInputRequired = true;
            this.txtAgainNewPass.NumberMode = false;
            this.txtAgainNewPass.ReadOnly = false;
            this.txtAgainNewPass.Size = new System.Drawing.Size(429, 26);
            this.txtAgainNewPass.TabIndex = 2;
            // 
            // frmChangePass
            // 
            this.AcceptButton = this.btnCofirm;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 246);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "frmChangePass";
            this.Text = "Đổi mật khẩu";
            this.Load += new System.EventHandler(this.frmChangePass_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private GxControl.GxTextField txtOldPass;
        private GxControl.GxTextField txtNewPass;
        private GxControl.GxTextField txtAgainNewPass;
        private System.Windows.Forms.Label label1;
        private GxControl.GxButton btnCofirm;
    }
}