namespace GiaoXu
{
    partial class frmThongKeChung
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabThongKeChung = new System.Windows.Forms.TabPage();
            this.gxThongKeChung1 = new GxControl.GxThongKeChung();
            this.tbThongKeOnGoi = new System.Windows.Forms.TabPage();
            this.gxThongKeOnGoi1 = new GxControl.GxThongKeOnGoi();
            this.tabControl1.SuspendLayout();
            this.tabThongKeChung.SuspendLayout();
            this.tbThongKeOnGoi.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabThongKeChung);
            this.tabControl1.Controls.Add(this.tbThongKeOnGoi);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(770, 523);
            this.tabControl1.TabIndex = 0;
            // 
            // tabThongKeChung
            // 
            this.tabThongKeChung.Controls.Add(this.gxThongKeChung1);
            this.tabThongKeChung.Location = new System.Drawing.Point(4, 22);
            this.tabThongKeChung.Name = "tabThongKeChung";
            this.tabThongKeChung.Padding = new System.Windows.Forms.Padding(3);
            this.tabThongKeChung.Size = new System.Drawing.Size(762, 497);
            this.tabThongKeChung.TabIndex = 0;
            this.tabThongKeChung.Text = "Thống kê chung";
            this.tabThongKeChung.UseVisualStyleBackColor = true;
            // 
            // gxThongKeChung1
            // 
            this.gxThongKeChung1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxThongKeChung1.Location = new System.Drawing.Point(3, 3);
            this.gxThongKeChung1.Name = "gxThongKeChung1";
            this.gxThongKeChung1.Size = new System.Drawing.Size(756, 491);
            this.gxThongKeChung1.TabIndex = 0;
            // 
            // tbThongKeOnGoi
            // 
            this.tbThongKeOnGoi.Controls.Add(this.gxThongKeOnGoi1);
            this.tbThongKeOnGoi.Location = new System.Drawing.Point(4, 22);
            this.tbThongKeOnGoi.Name = "tbThongKeOnGoi";
            this.tbThongKeOnGoi.Padding = new System.Windows.Forms.Padding(3);
            this.tbThongKeOnGoi.Size = new System.Drawing.Size(762, 497);
            this.tbThongKeOnGoi.TabIndex = 1;
            this.tbThongKeOnGoi.Text = "Thống kê ơn gọi tận hiến";
            this.tbThongKeOnGoi.UseVisualStyleBackColor = true;
            // 
            // gxThongKeOnGoi1
            // 
            this.gxThongKeOnGoi1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxThongKeOnGoi1.Location = new System.Drawing.Point(3, 3);
            this.gxThongKeOnGoi1.Name = "gxThongKeOnGoi1";
            this.gxThongKeOnGoi1.Size = new System.Drawing.Size(756, 491);
            this.gxThongKeOnGoi1.TabIndex = 0;
            // 
            // frmThongKeChung
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 523);
            this.Controls.Add(this.tabControl1);
            this.Name = "frmThongKeChung";
            this.Text = "Thống kê chung";
            this.Load += new System.EventHandler(this.frmThongKeChung_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabThongKeChung.ResumeLayout(false);
            this.tbThongKeOnGoi.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabThongKeChung;
        private System.Windows.Forms.TabPage tbThongKeOnGoi;
        private GxControl.GxThongKeChung gxThongKeChung1;
        private GxControl.GxThongKeOnGoi gxThongKeOnGoi1;

    }
}