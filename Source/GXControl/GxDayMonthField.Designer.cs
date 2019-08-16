namespace GxControl
{
    partial class GxDayMonthField
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
            this.gxMaskInput1 = new GxControl.GxMaskInput();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 9);
            // 
            // gxMaskInput1
            // 
            this.gxMaskInput1.BackColor = System.Drawing.Color.Transparent;
            this.gxMaskInput1.Day = "";
            this.gxMaskInput1.IsDayError = false;
            this.gxMaskInput1.IsMonthError = false;
            this.gxMaskInput1.IsNullMask = true;
            this.gxMaskInput1.Location = new System.Drawing.Point(44, 4);
            this.gxMaskInput1.Month = "";
            this.gxMaskInput1.Name = "gxMaskInput1";
            this.gxMaskInput1.Nullable = true;
            this.gxMaskInput1.ReadOnly = false;
            this.gxMaskInput1.Size = new System.Drawing.Size(89, 20);
            this.gxMaskInput1.TabIndex = 3;
            this.gxMaskInput1.Value = "";
            // 
            // GxDayMonthField
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "GxDayMonthField";
            this.Size = new System.Drawing.Size(223, 38);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        //protected System.Windows.Forms.Label label1;
        private GxMaskInput gxMaskInput1;
    }
}
