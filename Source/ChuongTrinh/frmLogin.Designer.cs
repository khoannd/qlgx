namespace GiaoXu
{
    partial class frmLogin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtUsername = new GxControl.GxTextField();
            this.txtPassword = new GxControl.GxTextField();
            this.btnLogin = new GxControl.GxButton();
            this.llbForget = new System.Windows.Forms.LinkLabel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(432, 63);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(132, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(185, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "ĐĂNG NHẬP";
            // 
            // txtUsername
            // 
            this.txtUsername.AutoCompleteEnabled = true;
            this.txtUsername.AutoUpperFirstChar = false;
            this.txtUsername.BoxLeft = 0;
            this.txtUsername.EditEnabled = true;
            this.txtUsername.Label = "Tên đăng nhập";
            this.txtUsername.Location = new System.Drawing.Point(28, 83);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtUsername.MaxLength = 32767;
            this.txtUsername.MultiLine = false;
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.NumberInputRequired = true;
            this.txtUsername.NumberMode = false;
            this.txtUsername.ReadOnly = false;
            this.txtUsername.Size = new System.Drawing.Size(366, 26);
            this.txtUsername.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.AutoCompleteEnabled = false;
            this.txtPassword.AutoUpperFirstChar = false;
            this.txtPassword.BoxLeft = 0;
            this.txtPassword.EditEnabled = true;
            this.txtPassword.Label = "Mật khẩu";
            this.txtPassword.Location = new System.Drawing.Point(58, 126);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtPassword.MaxLength = 32767;
            this.txtPassword.MultiLine = false;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.NumberInputRequired = true;
            this.txtPassword.NumberMode = false;
            this.txtPassword.ReadOnly = false;
            this.txtPassword.Size = new System.Drawing.Size(336, 26);
            this.txtPassword.TabIndex = 2;
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLogin.BackgroundImage")));
            this.btnLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLogin.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLogin.Location = new System.Drawing.Point(138, 191);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(160, 38);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Đăng nhập";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // llbForget
            // 
            this.llbForget.AutoSize = true;
            this.llbForget.Location = new System.Drawing.Point(314, 156);
            this.llbForget.Name = "llbForget";
            this.llbForget.Size = new System.Drawing.Size(80, 13);
            this.llbForget.TabIndex = 4;
            this.llbForget.TabStop = true;
            this.llbForget.Text = "Quên mật khẩu";
            this.llbForget.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbForget_LinkClicked);
            // 
            // frmLogin
            // 
            this.AcceptButton = this.btnLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 241);
            this.Controls.Add(this.llbForget);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "frmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập";
            this.Load += new System.EventHandler(this.frmLogin_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private GxControl.GxTextField txtUsername;
        private GxControl.GxTextField txtPassword;
        private GxControl.GxButton btnLogin;
        private System.Windows.Forms.LinkLabel llbForget;
    }
}