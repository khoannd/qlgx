namespace GxControl
{
    partial class GxCommand
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GxCommand));
            this.ImageList = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnCancel = new GxControl.GxButton();
            this.btnOK = new GxControl.GxButton();
            this.uiGroupBox6 = new GxControl.GxGroupBox();
            this.btn2 = new GxControl.GxButton();
            this.btn1 = new GxControl.GxButton();
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox6)).BeginInit();
            this.uiGroupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImageList
            // 
            this.ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList.ImageStream")));
            this.ImageList.TransparentColor = System.Drawing.Color.White;
            this.ImageList.Images.SetKeyName(0, "Save");
            this.ImageList.Images.SetKeyName(1, "Close");
            this.ImageList.Images.SetKeyName(2, "ExportExcel");
            this.ImageList.Images.SetKeyName(3, "Accept");
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnCancel.ImageKey = "Close";
            this.btnCancel.ImageList = this.ImageList;
            this.btnCancel.Location = new System.Drawing.Point(459, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 26);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "&Quay về";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btnCancel, "Đóng màn hình và bỏ qua những thay đổi");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.VisibleChanged += new System.EventHandler(this.button_VisibleChanged);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnOK.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOK.BackgroundImage")));
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btnOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btnOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btnOK.ImageKey = "Save";
            this.btnOK.ImageList = this.ImageList;
            this.btnOK.Location = new System.Drawing.Point(378, 13);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 26);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "Đồ&ng ý";
            this.btnOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btnOK, "Đóng màn hình và cập nhật thông tin");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.TabIndexChanged += new System.EventHandler(this.btnOK_TabIndexChanged);
            this.btnOK.VisibleChanged += new System.EventHandler(this.button_VisibleChanged);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // uiGroupBox6
            // 
            this.uiGroupBox6.Controls.Add(this.btnCancel);
            this.uiGroupBox6.Controls.Add(this.btn2);
            this.uiGroupBox6.Controls.Add(this.btn1);
            this.uiGroupBox6.Controls.Add(this.btnOK);
            this.uiGroupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiGroupBox6.FrameStyle = Janus.Windows.EditControls.FrameStyle.Top;
            this.uiGroupBox6.Location = new System.Drawing.Point(0, 0);
            this.uiGroupBox6.Name = "uiGroupBox6";
            this.uiGroupBox6.Size = new System.Drawing.Size(547, 45);
            this.uiGroupBox6.TabIndex = 7;
            // 
            // btn2
            // 
            this.btn2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn2.AutoSize = true;
            this.btn2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn2.BackgroundImage")));
            this.btn2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn2.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btn2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btn2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btn2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn2.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btn2.ImageKey = "ExportExcel";
            this.btn2.Location = new System.Drawing.Point(204, 13);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(81, 26);
            this.btn2.TabIndex = 1;
            this.btn2.Text = "Button2";
            this.btn2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Visible = false;
            this.btn2.VisibleChanged += new System.EventHandler(this.button_VisibleChanged);
            this.btn2.Click += new System.EventHandler(this.Button1_Click);
            // 
            // btn1
            // 
            this.btn1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn1.AutoSize = true;
            this.btn1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn1.BackgroundImage")));
            this.btn1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn1.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            this.btn1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.btn1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SkyBlue;
            this.btn1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btn1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.btn1.ImageKey = "ExportExcel";
            this.btn1.ImageList = this.ImageList;
            this.btn1.Location = new System.Drawing.Point(291, 13);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(81, 26);
            this.btn1.TabIndex = 1;
            this.btn1.Text = "Button1";
            this.btn1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Visible = false;
            this.btn1.VisibleChanged += new System.EventHandler(this.button_VisibleChanged);
            this.btn1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // GxCommand
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.uiGroupBox6);
            this.Name = "GxCommand";
            this.Size = new System.Drawing.Size(547, 45);
            this.Resize += new System.EventHandler(this.GxCommand_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.uiGroupBox6)).EndInit();
            this.uiGroupBox6.ResumeLayout(false);
            this.uiGroupBox6.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GxControl.GxGroupBox uiGroupBox6;
        public GxButton btnCancel;
        public GxButton btnOK;
        private System.Windows.Forms.ToolTip toolTip1;
        public GxButton btn1;
        public System.Windows.Forms.ImageList ImageList;
        public GxButton btn2;
    }
}
