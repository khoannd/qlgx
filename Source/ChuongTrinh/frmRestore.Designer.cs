namespace GiaoXu
{
    partial class frmRestore
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRestore));
            this.gxCommand1 = new GxControl.GxCommand();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.btnRestoreFile = new GxControl.GxButton();
            this.btnRestore = new GxControl.GxButton();
            this.btnDelete = new GxControl.GxButton();
            this.btnBackup = new GxControl.GxButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.colSTT = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gxGroupBox2 = new GxControl.GxGroupBox();
            this.btnYeuCauFile = new GxControl.GxButton();
            this.btnRestoreServer = new GxControl.GxButton();
            this.listViewServer = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).BeginInit();
            this.gxGroupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 385);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(562, 48);
            this.gxCommand1.TabIndex = 4;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "ZIP file|*.zip|MDB File|*.mdb|All file|*.*";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(562, 385);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.gxGroupBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(554, 359);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Máy tính của bạn";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.btnRestoreFile);
            this.gxGroupBox1.Controls.Add(this.btnRestore);
            this.gxGroupBox1.Controls.Add(this.btnDelete);
            this.gxGroupBox1.Controls.Add(this.btnBackup);
            this.gxGroupBox1.Controls.Add(this.listView1);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox1.Location = new System.Drawing.Point(3, 3);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(548, 353);
            this.gxGroupBox1.TabIndex = 1;
            this.gxGroupBox1.Text = "Danh sách các sao lưu";
            // 
            // btnRestoreFile
            // 
            this.btnRestoreFile.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnRestoreFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRestoreFile.BackgroundImage")));
            this.btnRestoreFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRestoreFile.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnRestoreFile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnRestoreFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnRestoreFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRestoreFile.Location = new System.Drawing.Point(396, 21);
            this.btnRestoreFile.Name = "btnRestoreFile";
            this.btnRestoreFile.Size = new System.Drawing.Size(158, 23);
            this.btnRestoreFile.TabIndex = 2;
            this.btnRestoreFile.Text = "Phục hồi từ tập tin tự chọn";
            this.btnRestoreFile.UseVisualStyleBackColor = true;
            this.btnRestoreFile.Click += new System.EventHandler(this.btnRestoreFile_Click_1);
            // 
            // btnRestore
            // 
            this.btnRestore.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnRestore.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRestore.BackgroundImage")));
            this.btnRestore.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRestore.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnRestore.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnRestore.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRestore.Location = new System.Drawing.Point(232, 21);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(158, 23);
            this.btnRestore.TabIndex = 2;
            this.btnRestore.Text = "Phục hồi từ mục được chọn";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click_1);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnDelete.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnDelete.BackgroundImage")));
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnDelete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDelete.Location = new System.Drawing.Point(108, 21);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(118, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Xóa mục được chọn";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click_1);
            // 
            // btnBackup
            // 
            this.btnBackup.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnBackup.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnBackup.BackgroundImage")));
            this.btnBackup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBackup.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnBackup.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnBackup.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnBackup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnBackup.Location = new System.Drawing.Point(7, 21);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(95, 23);
            this.btnBackup.TabIndex = 0;
            this.btnBackup.Text = "Tạo sao lưu mới";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click_1);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSTT,
            this.colFileName,
            this.colDate});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(3, 50);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(556, 329);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // colSTT
            // 
            this.colSTT.Text = "STT";
            // 
            // colFileName
            // 
            this.colFileName.Text = "Tập tin sao lưu";
            this.colFileName.Width = 300;
            // 
            // colDate
            // 
            this.colDate.Text = "Ngày sao lưu";
            this.colDate.Width = 130;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.gxGroupBox2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(554, 359);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Máy chủ của hệ thống qlgx.net";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // gxGroupBox2
            // 
            this.gxGroupBox2.Controls.Add(this.btnYeuCauFile);
            this.gxGroupBox2.Controls.Add(this.btnRestoreServer);
            this.gxGroupBox2.Controls.Add(this.listViewServer);
            this.gxGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox2.Location = new System.Drawing.Point(3, 3);
            this.gxGroupBox2.Name = "gxGroupBox2";
            this.gxGroupBox2.Size = new System.Drawing.Size(548, 353);
            this.gxGroupBox2.TabIndex = 2;
            this.gxGroupBox2.Text = "Danh sách các sao lưu";
            // 
            // btnYeuCauFile
            // 
            this.btnYeuCauFile.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnYeuCauFile.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnYeuCauFile.BackgroundImage")));
            this.btnYeuCauFile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnYeuCauFile.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnYeuCauFile.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnYeuCauFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnYeuCauFile.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnYeuCauFile.Location = new System.Drawing.Point(170, 21);
            this.btnYeuCauFile.Name = "btnYeuCauFile";
            this.btnYeuCauFile.Size = new System.Drawing.Size(205, 23);
            this.btnYeuCauFile.TabIndex = 4;
            this.btnYeuCauFile.Text = "Yêu cầu nhận tập tin từ máy nhập khác";
            this.btnYeuCauFile.UseVisualStyleBackColor = true;
            this.btnYeuCauFile.Click += new System.EventHandler(this.btnYeuCauFile_Click);
            // 
            // btnRestoreServer
            // 
            this.btnRestoreServer.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnRestoreServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRestoreServer.BackgroundImage")));
            this.btnRestoreServer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRestoreServer.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnRestoreServer.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnRestoreServer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnRestoreServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRestoreServer.Location = new System.Drawing.Point(6, 21);
            this.btnRestoreServer.Name = "btnRestoreServer";
            this.btnRestoreServer.Size = new System.Drawing.Size(158, 23);
            this.btnRestoreServer.TabIndex = 2;
            this.btnRestoreServer.Text = "Phục hồi từ mục được chọn";
            this.btnRestoreServer.UseVisualStyleBackColor = true;
            this.btnRestoreServer.Click += new System.EventHandler(this.btnRestoreServer_Click);
            // 
            // listViewServer
            // 
            this.listViewServer.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewServer.FullRowSelect = true;
            this.listViewServer.GridLines = true;
            this.listViewServer.HideSelection = false;
            this.listViewServer.Location = new System.Drawing.Point(3, 50);
            this.listViewServer.Name = "listViewServer";
            this.listViewServer.ShowGroups = false;
            this.listViewServer.ShowItemToolTips = true;
            this.listViewServer.Size = new System.Drawing.Size(556, 329);
            this.listViewServer.TabIndex = 3;
            this.listViewServer.UseCompatibleStateImageBehavior = false;
            this.listViewServer.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "STT";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Tập tin sao lưu";
            this.columnHeader2.Width = 300;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Ngày sao lưu";
            this.columnHeader3.Width = 130;
            // 
            // frmRestore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 433);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpKey = "sao_luu_khoi_phuc";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRestore";
            this.Text = "Khôi phục dữ liệu";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox2)).EndInit();
            this.gxGroupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private GxControl.GxCommand gxCommand1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxButton btnRestoreFile;
        private GxControl.GxButton btnRestore;
        private GxControl.GxButton btnDelete;
        private GxControl.GxButton btnBackup;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader colSTT;
        private System.Windows.Forms.ColumnHeader colFileName;
        private System.Windows.Forms.ColumnHeader colDate;
        private System.Windows.Forms.TabPage tabPage2;
        private GxControl.GxGroupBox gxGroupBox2;
        private GxControl.GxButton btnRestoreServer;
        private System.Windows.Forms.ListView listViewServer;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private GxControl.GxButton btnYeuCauFile;
    }
}