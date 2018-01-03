using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using GxGlobal;
using Femiani.Forms.UI.Input;

namespace AutoCompleteTextBox.Demo
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private CoolTextBox coolTextBox1;
		private System.Windows.Forms.Button Done;
        private GroupBox groupBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.coolTextBox1.Text = string.Empty;

			// Add some sample auto complete entry items...
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Phoenix, Az", "Phoenix, Az", "Az", "PHX"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Tempe, Az", "Tempe, Az", "Az", "TPE"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Chandler, Az", "Chandler, Az", "Az", "CHA"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Boxford, Ma", "Boxford, Ma", "Ma", "BXF"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Topsfield, Ma", "Topsfield, Ma", "Ma", "TPF"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 1", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 2", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 3", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 4", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 5", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 6", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 7", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 8", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 9", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 10", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 11", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 12", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 13", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 14", "Danvers, Ma", "Ma", "DNV"));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Danvers, Ma 15", "Danvers, Ma", "Ma", "DNV"));

            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Nguyễn Đức Khoan", "Nguyễn Đức Khoan".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Nguyễn Đức Trọng", "Nguyễn Đức Trọng".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Nguyễn Văn Trọng", "Nguyễn Văn Trọng".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
            //this.coolTextBox1.Items.Add(new AutoCompleteEntry("Trần Văn Lâm", "Trần Văn Lâm".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));

            DataTable tbl = Memory.GetData("SELECT TenThanh,HoTen FROM GiaoDan WHERE MaGiaoHo=1");
            if (!Memory.ShowError() && tbl != null)
            {
                foreach (DataRow row in tbl.Rows)
                {
                    this.coolTextBox1.Items.Add(new AutoCompleteEntry(row["HoTen"].ToString(), row["HoTen"].ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                }
            }

			this.Validate();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.coolTextBox1 = new Femiani.Forms.UI.Input.CoolTextBox();
            this.Done = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // coolTextBox1
            // 
            this.coolTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.coolTextBox1.BorderColor = System.Drawing.Color.LightSteelBlue;
            this.coolTextBox1.Location = new System.Drawing.Point(17, 19);
            this.coolTextBox1.Name = "coolTextBox1";
            this.coolTextBox1.Padding = new System.Windows.Forms.Padding(4);
            this.coolTextBox1.PopupWidth = 300;
            this.coolTextBox1.SelectedItemBackColor = System.Drawing.Color.LightSteelBlue;
            this.coolTextBox1.SelectedItemForeColor = System.Drawing.SystemColors.ControlText;
            this.coolTextBox1.Size = new System.Drawing.Size(316, 21);
            this.coolTextBox1.TabIndex = 3;
            this.coolTextBox1.Validating += new System.ComponentModel.CancelEventHandler(this.coolTextBox1_Validating);
            // 
            // Done
            // 
            this.Done.Location = new System.Drawing.Point(261, 46);
            this.Done.Name = "Done";
            this.Done.Size = new System.Drawing.Size(72, 23);
            this.Done.TabIndex = 4;
            this.Done.Text = "&Done";
            this.Done.Click += new System.EventHandler(this.Done_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.coolTextBox1);
            this.groupBox1.Controls.Add(this.Done);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(339, 82);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(496, 212);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "AutoComplete Demo";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private void coolTextBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.coolTextBox1.Text == string.Empty)
			{
				this.coolTextBox1.BorderColor = Color.Firebrick;
				e.Cancel = true;
			} 
			else
			{
				this.coolTextBox1.BorderColor = Color.LightSteelBlue;
			}
		}

		private void Done_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

	}
}
