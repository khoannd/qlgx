namespace GxControl
{
    partial class GxGiaoHo
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
            this.uiComboBox1 = new Janus.Windows.EditControls.UIComboBox();
            this.SuspendLayout();
            // 
            // uiComboBox1
            // 
            this.uiComboBox1.Location = new System.Drawing.Point(45, 4);
            this.uiComboBox1.Name = "uiComboBox1";
            this.uiComboBox1.Size = new System.Drawing.Size(110, 20);
            this.uiComboBox1.TabIndex = 2;
            // 
            // GxGiaoHo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Name = "GxGiaoHo";
            this.Size = new System.Drawing.Size(180, 27);
            this.Load += new System.EventHandler(this.GxGiaoHo_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //protected System.Windows.Forms.Label label1;
        private Janus.Windows.EditControls.UIComboBox uiComboBox1;
    }
}
