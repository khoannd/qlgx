namespace GxControl
{
    partial class GxDateField
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
            this.gxDateInput1 = new GxControl.GxDateInput();
            this.SuspendLayout();
            // 
            // calendarCombo1
            // 
            this.gxDateInput1.BackColor = System.Drawing.Color.Transparent;
            this.gxDateInput1.IsNullDate = true;
            this.gxDateInput1.Location = new System.Drawing.Point(44, 4);
            this.gxDateInput1.Name = "calendarCombo1";
            this.gxDateInput1.Nullable = true;
            this.gxDateInput1.Size = new System.Drawing.Size(89, 20);
            this.gxDateInput1.TabIndex = 3;
            // 
            // GxDateField
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "GxDateField";
            this.Size = new System.Drawing.Size(223, 25);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //protected System.Windows.Forms.Label label1;
        private GxDateInput gxDateInput1;



    }
}
