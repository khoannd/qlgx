namespace GiaoXu
{
    partial class frmCreateGiaoXu
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCreateGiaoXu));
            this.panel2 = new System.Windows.Forms.Panel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnSelectGiaoXu = new GxControl.GxButton();
            this.btnKTimThay = new GxControl.GxButton();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkSearchAll = new GxControl.GxCheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbGiaoHat = new GxControl.GxComboBox();
            this.cbGiaoPhan = new GxControl.GxComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblInfo);
            this.panel2.Controls.Add(this.listView1);
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 97);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1244, 514);
            this.panel2.TabIndex = 1;
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.AliceBlue;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(51, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(1193, 465);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Tile;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "church.jpg");
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.AliceBlue;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(51, 465);
            this.panel4.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.AliceBlue;
            this.panel3.Controls.Add(this.btnSelectGiaoXu);
            this.panel3.Controls.Add(this.btnKTimThay);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 465);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1244, 49);
            this.panel3.TabIndex = 2;
            // 
            // btnSelectGiaoXu
            // 
            this.btnSelectGiaoXu.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectGiaoXu.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSelectGiaoXu.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSelectGiaoXu.BackgroundImage")));
            this.btnSelectGiaoXu.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSelectGiaoXu.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnSelectGiaoXu.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnSelectGiaoXu.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnSelectGiaoXu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSelectGiaoXu.Location = new System.Drawing.Point(915, 6);
            this.btnSelectGiaoXu.Name = "btnSelectGiaoXu";
            this.btnSelectGiaoXu.Size = new System.Drawing.Size(62, 37);
            this.btnSelectGiaoXu.TabIndex = 3;
            this.btnSelectGiaoXu.Text = "Chọn";
            this.btnSelectGiaoXu.UseVisualStyleBackColor = false;
            this.btnSelectGiaoXu.Visible = false;
            this.btnSelectGiaoXu.Click += new System.EventHandler(this.btnSelectGiaoXu_Click);
            // 
            // btnKTimThay
            // 
            this.btnKTimThay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKTimThay.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnKTimThay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnKTimThay.BackgroundImage")));
            this.btnKTimThay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnKTimThay.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnKTimThay.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnKTimThay.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnKTimThay.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnKTimThay.Location = new System.Drawing.Point(983, 6);
            this.btnKTimThay.Name = "btnKTimThay";
            this.btnKTimThay.Size = new System.Drawing.Size(244, 37);
            this.btnKTimThay.TabIndex = 3;
            this.btnKTimThay.Text = "Tôi không tìm thấy giáo xứ của mình";
            this.btnKTimThay.UseVisualStyleBackColor = false;
            this.btnKTimThay.Click += new System.EventHandler(this.btnKTimThay_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(244, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "DANH SÁCH GIÁO XỨ";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.AliceBlue;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.chkSearchAll);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.cbGiaoHat);
            this.panel1.Controls.Add(this.cbGiaoPhan);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtSearch);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1244, 97);
            this.panel1.TabIndex = 0;
            // 
            // chkSearchAll
            // 
            this.chkSearchAll.AutoSize = true;
            this.chkSearchAll.Location = new System.Drawing.Point(982, 55);
            this.chkSearchAll.Name = "chkSearchAll";
            this.chkSearchAll.Size = new System.Drawing.Size(159, 17);
            this.chkSearchAll.TabIndex = 6;
            this.chkSearchAll.Text = "Tìm kiếm tất cả các giáo xứ ";
            this.chkSearchAll.UseVisualStyleBackColor = true;
            this.chkSearchAll.Visible = false;
            this.chkSearchAll.CheckedChanged += new System.EventHandler(this.chkSearchAll_CheckedChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(255, 58);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Giáo hạt";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Giáo phận";
            // 
            // cbGiaoHat
            // 
            this.cbGiaoHat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cbGiaoHat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGiaoHat.FormattingEnabled = true;
            this.cbGiaoHat.Location = new System.Drawing.Point(308, 55);
            this.cbGiaoHat.Name = "cbGiaoHat";
            this.cbGiaoHat.Size = new System.Drawing.Size(158, 21);
            this.cbGiaoHat.TabIndex = 3;
            // 
            // cbGiaoPhan
            // 
            this.cbGiaoPhan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.cbGiaoPhan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGiaoPhan.FormattingEnabled = true;
            this.cbGiaoPhan.Location = new System.Drawing.Point(77, 55);
            this.cbGiaoPhan.Name = "cbGiaoPhan";
            this.cbGiaoPhan.Size = new System.Drawing.Size(158, 21);
            this.cbGiaoPhan.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(885, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Tìm kiếm theo tên";
            this.label2.Visible = false;
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(982, 24);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(248, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.Visible = false;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(323, 40);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(592, 144);
            this.lblInfo.TabIndex = 5;
            this.lblInfo.Text = resources.GetString("lblInfo.Text");
            // 
            // frmCreateGiaoXu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1244, 611);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "frmCreateGiaoXu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chọn giáo xứ";
            this.Shown += new System.EventHandler(this.frmCreateGiaoXu_Shown);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        private GxControl.GxButton btnKTimThay;
        private GxControl.GxComboBox cbGiaoHat;
        private GxControl.GxComboBox cbGiaoPhan;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private GxControl.GxCheckBox chkSearchAll;
        private GxControl.GxButton btnSelectGiaoXu;
        private System.Windows.Forms.Label lblInfo;
    }
}