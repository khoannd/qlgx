namespace GXControl
{
    partial class GridGiaoDanTemplate
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
            Janus.Windows.GridEX.GridEXLayout gridEXLayout1 = new Janus.Windows.GridEX.GridEXLayout();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridGiaoDanTemplate));
            this.gridEX1 = new Janus.Windows.GridEX.GridEX();
            this.gxCommand1 = new GXControl.GXCommand();
            ((System.ComponentModel.ISupportInitialize)(this.gridEX1)).BeginInit();
            this.SuspendLayout();
            // 
            // gridEX1
            // 
            this.gridEX1.AllowColumnDrag = false;
            this.gridEX1.AllowEdit = Janus.Windows.GridEX.InheritableBoolean.False;
            this.gridEX1.ColumnAutoResize = true;
            gridEXLayout1.LayoutString = resources.GetString("gridEXLayout1.LayoutString");
            this.gridEX1.DesignTimeLayout = gridEXLayout1;
            this.gridEX1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridEX1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridEX1.HideSelection = Janus.Windows.GridEX.HideSelection.HighlightInactive;
            this.gridEX1.KeepRowSettings = true;
            this.gridEX1.Location = new System.Drawing.Point(0, 0);
            this.gridEX1.Name = "gridEX1";
            this.gridEX1.RecordNavigator = true;
            this.gridEX1.RowHeaders = Janus.Windows.GridEX.InheritableBoolean.True;
            this.gridEX1.SaveSettings = false;
            this.gridEX1.SettingsKey = "gridEX1";
            this.gridEX1.Size = new System.Drawing.Size(622, 402);
            this.gridEX1.TabIndex = 0;
            this.gridEX1.SelectionChanged += new System.EventHandler(this.gridEX1_SelectionChanged);
            this.gridEX1.RowDoubleClick += new Janus.Windows.GridEX.RowActionEventHandler(this.gridEX1_RowDoubleClick);
            // 
            // gxCommand1
            // 
            this.gxCommand1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gxCommand1.Location = new System.Drawing.Point(0, 402);
            this.gxCommand1.Name = "gxCommand1";
            this.gxCommand1.Size = new System.Drawing.Size(622, 45);
            this.gxCommand1.TabIndex = 1;
            this.gxCommand1.OnOK += new System.EventHandler(this.gxCommand1_OnOK);
            // 
            // GridGiaoDanTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(622, 447);
            this.Controls.Add(this.gridEX1);
            this.Controls.Add(this.gxCommand1);
            this.Name = "GridGiaoDanTemplate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chon giao dan";
            ((System.ComponentModel.ISupportInitialize)(this.gridEX1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Janus.Windows.GridEX.GridEX gridEX1;
        private GXCommand gxCommand1;
    }
}