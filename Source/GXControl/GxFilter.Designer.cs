namespace GxControl
{
    partial class GxFilter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GxFilter));
            this.filterEditor1 = new Janus.Windows.FilterEditor.FilterEditor();
            this.SuspendLayout();
            // 
            // filterEditor1
            // 
            this.filterEditor1.AutoApply = true;
            this.filterEditor1.BackColor = System.Drawing.Color.Transparent;
            this.filterEditor1.BuiltInTextsData = resources.GetString("filterEditor1.BuiltInTextsData");
            this.filterEditor1.DefaultConditionOperator = Janus.Data.ConditionOperator.Equal;
            this.filterEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterEditor1.InnerAreaStyle = Janus.Windows.UI.Dock.PanelInnerAreaStyle.UseFormatStyle;
            this.filterEditor1.Location = new System.Drawing.Point(0, 0);
            this.filterEditor1.MinSize = new System.Drawing.Size(0, 0);
            this.filterEditor1.Name = "filterEditor1";
            this.filterEditor1.Office2007ColorScheme = Janus.Windows.Common.Office2007ColorScheme.Default;
            this.filterEditor1.ScrollMode = Janus.Windows.UI.Dock.ScrollMode.Both;
            this.filterEditor1.ScrollStep = 15;
            this.filterEditor1.Size = new System.Drawing.Size(408, 30);
            // 
            // GxFilter
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.filterEditor1);
            this.Name = "GxFilter";
            this.Size = new System.Drawing.Size(408, 30);
            this.Load += new System.EventHandler(this.FMSFilter_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Janus.Windows.FilterEditor.FilterEditor filterEditor1;
    }
}
