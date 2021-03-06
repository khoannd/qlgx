namespace GxControl
{
    partial class frmReplace
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
            this.gxCommand1 = new GxControl.GxCommand();
            this.cbForm = new GxControl.GxComboField();
            this.cbField = new GxControl.GxComboField();
            this.txtFind = new GxControl.GxTextField();
            this.txtReplace = new GxControl.GxTextField();
            this.SuspendLayout();
            // 
            // gxCommand1
            // 
            this.gxCommand1.AllowHotkey = false;
            this.gxCommand1.DisplayMode = GxGlobal.DisplayMode.Full;
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 141);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.OKIsAccept = false;
            this.gxCommand1.Size = new System.Drawing.Size(390, 49);
            this.gxCommand1.TabIndex = 4;
            this.gxCommand1.ToolTipButton1 = "";
            this.gxCommand1.ToolTipCancel = "Đóng màn hình và bỏ qua những thay đổi";
            this.gxCommand1.ToolTipOK = "Đóng màn hình và cập nhật thông tin";
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // cbForm
            // 
            this.cbForm.AutoUpperFirstChar = false;
            this.cbForm.BoxLeft = 100;
            this.cbForm.EditEnabled = true;
            this.cbForm.Label = "Tên màn hình";
            this.cbForm.Location = new System.Drawing.Point(12, 12);
            this.cbForm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbForm.MaxLength = 0;
            this.cbForm.Name = "cbForm";
            this.cbForm.SelectedIndex = -1;
            this.cbForm.SelectedText = "";
            this.cbForm.SelectedValue = null;
            this.cbForm.Size = new System.Drawing.Size(343, 26);
            this.cbForm.TabIndex = 0;
            this.cbForm.SelectedIndexChanged += new System.EventHandler(this.cbForm_SelectedIndexChanged);
            // 
            // cbField
            // 
            this.cbField.AutoUpperFirstChar = false;
            this.cbField.BoxLeft = 100;
            this.cbField.EditEnabled = true;
            this.cbField.Label = "Tên cột";
            this.cbField.Location = new System.Drawing.Point(12, 44);
            this.cbField.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cbField.MaxLength = 0;
            this.cbField.Name = "cbField";
            this.cbField.SelectedIndex = -1;
            this.cbField.SelectedText = "";
            this.cbField.SelectedValue = null;
            this.cbField.Size = new System.Drawing.Size(343, 26);
            this.cbField.TabIndex = 1;
            // 
            // txtFind
            // 
            this.txtFind.AutoCompleteEnabled = true;
            this.txtFind.AutoUpperFirstChar = false;
            this.txtFind.BoxLeft = 100;
            this.txtFind.EditEnabled = true;
            this.txtFind.Label = "Giá trị tìm kiếm";
            this.txtFind.Location = new System.Drawing.Point(12, 76);
            this.txtFind.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtFind.MaxLength = 255;
            this.txtFind.MultiLine = false;
            this.txtFind.Name = "txtFind";
            this.txtFind.NumberInputRequired = true;
            this.txtFind.NumberMode = false;
            this.txtFind.ReadOnly = false;
            this.txtFind.Size = new System.Drawing.Size(343, 26);
            this.txtFind.TabIndex = 2;
            // 
            // txtReplace
            // 
            this.txtReplace.AutoCompleteEnabled = true;
            this.txtReplace.AutoUpperFirstChar = false;
            this.txtReplace.BoxLeft = 100;
            this.txtReplace.EditEnabled = true;
            this.txtReplace.Label = "Giá trị thay thế";
            this.txtReplace.Location = new System.Drawing.Point(12, 108);
            this.txtReplace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtReplace.MaxLength = 255;
            this.txtReplace.MultiLine = false;
            this.txtReplace.Name = "txtReplace";
            this.txtReplace.NumberInputRequired = true;
            this.txtReplace.NumberMode = false;
            this.txtReplace.ReadOnly = false;
            this.txtReplace.Size = new System.Drawing.Size(343, 26);
            this.txtReplace.TabIndex = 3;
            // 
            // frmReplace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 190);
            this.Controls.Add(this.txtReplace);
            this.Controls.Add(this.txtFind);
            this.Controls.Add(this.cbField);
            this.Controls.Add(this.cbForm);
            this.Controls.Add(this.gxCommand1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmReplace";
            this.Text = "Tìm và thay thế";
            this.ResumeLayout(false);

        }

        #endregion

        private GxCommand gxCommand1;
        private GxComboField cbForm;
        private GxComboField cbField;
        private GxTextField txtFind;
        private GxTextField txtReplace;
    }
}