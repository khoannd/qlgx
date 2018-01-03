namespace GxControl
{
    partial class frmOptionBieuDo
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
            this.cbLoaiBieuDo = new GxControl.GxComboField();
            this.SuspendLayout();
            // 
            // cbLoaiBieuDo
            // 
            this.cbLoaiBieuDo.AutoUpperFirstChar = false;
            this.cbLoaiBieuDo.BoxLeft = 0;
            this.cbLoaiBieuDo.EditEnabled = true;
            this.cbLoaiBieuDo.Label = "Loại biểu đồ";
            this.cbLoaiBieuDo.Location = new System.Drawing.Point(53, 34);
            this.cbLoaiBieuDo.MaxLength = 0;
            this.cbLoaiBieuDo.Name = "cbLoaiBieuDo";
            this.cbLoaiBieuDo.SelectedIndex = -1;
            this.cbLoaiBieuDo.SelectedText = "";
            this.cbLoaiBieuDo.SelectedValue = null;
            this.cbLoaiBieuDo.Size = new System.Drawing.Size(340, 26);
            this.cbLoaiBieuDo.TabIndex = 0;
            // 
            // frmOptionBieuDo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 11F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 413);
            this.Controls.Add(this.cbLoaiBieuDo);
            this.Name = "frmOptionBieuDo";
            this.Text = "frmOptionBieuDo";
            this.ResumeLayout(false);

        }

        #endregion

        private GxComboField cbLoaiBieuDo;
    }
}