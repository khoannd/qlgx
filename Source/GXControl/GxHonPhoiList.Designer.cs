namespace GxControl
{
    partial class GxHonPhoiList
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
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GxHonPhoiList
            // 
            this.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.GxHonPhoiList_RowDoubleClick);
            this.FormattingRow += new Janus.Windows.GridEX.RowLoadEventHandler(this.GxHonPhoiList_FormattingRow);
            this.Controls.SetChildIndex(this.LblLoadData, 0);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
