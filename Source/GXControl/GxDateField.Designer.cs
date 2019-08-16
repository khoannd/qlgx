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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GxDateField));
            this.gxDateInput1 = new GxControl.GxDateInput();
            this.SuspendLayout();
            // 
            // gxDateInput1
            // 
            this.gxDateInput1.BackColor = System.Drawing.Color.Transparent;
            this.gxDateInput1.Day = "";
            this.gxDateInput1.FullInputRequired = false;
            this.gxDateInput1.IsDayError = false;
            this.gxDateInput1.IsMonthError = false;
            this.gxDateInput1.IsNullDate = true;
            this.gxDateInput1.IsYearError = false;
            this.gxDateInput1.Location = new System.Drawing.Point(44, 4);
            this.gxDateInput1.Month = "";
            this.gxDateInput1.Name = "gxDateInput1";
            this.gxDateInput1.Nullable = true;
            this.gxDateInput1.ReadOnly = false;
            this.gxDateInput1.Size = new System.Drawing.Size(89, 20);
            this.gxDateInput1.TabIndex = 3;
            this.gxDateInput1.Value = ((object)(resources.GetObject("gxDateInput1.Value")));
            this.gxDateInput1.Year = "";
            // 
            // GxDateField
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "GxDateField";
            this.Size = new System.Drawing.Size(223, 38);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //protected System.Windows.Forms.Label label1;
        private GxDateInput gxDateInput1;
    }
}
