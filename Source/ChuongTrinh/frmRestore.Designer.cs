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
            this.gxGroupBox1 = new GxControl.GxGroupBox();
            this.btnRestoreFile = new GxControl.GxButton();
            this.btnRestore = new GxControl.GxButton();
            this.btnDelete = new GxControl.GxButton();
            this.btnBackup = new GxControl.GxButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.colSTT = new System.Windows.Forms.ColumnHeader();
            this.colFileName = new System.Windows.Forms.ColumnHeader();
            this.colDate = new System.Windows.Forms.ColumnHeader();
            this.gxCommand1 = new GxControl.GxCommand();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).BeginInit();
            this.gxGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gxGroupBox1
            // 
            this.gxGroupBox1.Controls.Add(this.btnRestoreFile);
            this.gxGroupBox1.Controls.Add(this.btnRestore);
            this.gxGroupBox1.Controls.Add(this.btnDelete);
            this.gxGroupBox1.Controls.Add(this.btnBackup);
            this.gxGroupBox1.Controls.Add(this.listView1);
            this.gxGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gxGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.gxGroupBox1.Name = "gxGroupBox1";
            this.gxGroupBox1.Size = new System.Drawing.Size(562, 385);
            this.gxGroupBox1.TabIndex = 0;
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
            this.btnRestoreFile.Click += new System.EventHandler(this.btnRestoreFile_Click);
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
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
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
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
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
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
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
            // frmRestore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 433);
            this.Controls.Add(this.gxGroupBox1);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpKey = "sao_luu_khoi_phuc";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmRestore";
            this.Text = "Khôi phục dữ liệu";
            ((System.ComponentModel.ISupportInitialize)(this.gxGroupBox1)).EndInit();
            this.gxGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox gxGroupBox1;
        private GxControl.GxCommand gxCommand1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader colSTT;
        private System.Windows.Forms.ColumnHeader colFileName;
        private System.Windows.Forms.ColumnHeader colDate;
        private GxControl.GxButton btnRestore;
        private GxControl.GxButton btnDelete;
        private GxControl.GxButton btnBackup;
        private GxControl.GxButton btnRestoreFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}