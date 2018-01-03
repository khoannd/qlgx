namespace GxControl
{
    partial class GxComboField
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
            //this.label1 = new System.Windows.Forms.Label();
            this.combo = new GxComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            //this.label1.AutoSize = true;
            //this.label1.Location = new System.Drawing.Point(3, 6);
            //this.label1.Name = "label1";
            //this.label1.Size = new System.Drawing.Size(35, 13);
            //this.label1.TabIndex = 0;
            //this.label1.Text = "label1";
            //this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //this.label1.TextChanged += new System.EventHandler(this.label1_TextChanged);
            // 
            // Combo
            // 
            this.combo.FormattingEnabled = true;
            this.combo.Location = new System.Drawing.Point(45, 4);
            this.combo.Name = "Combo";
            this.combo.Size = new System.Drawing.Size(102, 21);
            this.combo.TabIndex = 1;
            // 
            // ComboField
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            //this.Controls.Add(this.Combo);
            //this.Controls.Add(this.label1);
            this.Name = "ComboField";
            this.Size = new System.Drawing.Size(150, 26);
            //this.Resize += new System.EventHandler(this.TextField_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //protected System.Windows.Forms.Label label1;
        public GxComboBox combo;



    }
}
