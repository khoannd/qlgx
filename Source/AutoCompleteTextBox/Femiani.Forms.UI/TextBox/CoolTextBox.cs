using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Femiani.Forms.UI.Input
{
	/// <summary>
	/// Summary description for CoolTextBox.
	/// </summary>
	public class CoolTextBox : System.Windows.Forms.UserControl
	{
		private Color borderColor = Color.LightSteelBlue;
		public Color BorderColor
		{
			get
			{
				return this.borderColor;
			}
			set
			{
				if (this.borderColor != value)
				{
					this.borderColor = value;
					this.Invalidate();
				}
			}
		}

		public Color SelectedItemBackColor
		{
			get
			{
				return this.autoCompleteTextBox1.PopupSelectionBackColor;
			}
			set
			{
				this.autoCompleteTextBox1.PopupSelectionBackColor = value;
			}
		}

		public Color SelectedItemForeColor
		{
			get
			{
				return this.autoCompleteTextBox1.PopupSelectionForeColor;
			}
			set
			{
				this.autoCompleteTextBox1.PopupSelectionForeColor = value;
			}
		}

		[System.ComponentModel.Editor(typeof(AutoCompleteEntryCollection.AutoCompleteEntryCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public AutoCompleteEntryCollection Items
		{
			get
			{
				return this.autoCompleteTextBox1.Items;
			}
			set
			{
				this.autoCompleteTextBox1.Items = value;
			}
		}

		[System.ComponentModel.Editor(typeof(AutoCompleteTriggerCollection.AutoCompleteTriggerCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public AutoCompleteTriggerCollection Triggers
		{
			get
			{
				return this.autoCompleteTextBox1.Triggers;
			}
			set
			{
				this.autoCompleteTextBox1.Triggers = value;
			}
		}

		[Browsable(true)]
		public override string Text
		{
			get
			{
				return this.autoCompleteTextBox1.Text;
			}
			set
			{
				this.autoCompleteTextBox1.Text = value;
			}
		}

		public override Color ForeColor
		{
			get
			{
				return this.autoCompleteTextBox1.ForeColor;
			}
			set
			{
				this.autoCompleteTextBox1.ForeColor = value;
			}
		}

		public override ContextMenu ContextMenu
		{
			get
			{
				return this.autoCompleteTextBox1.ContextMenu;
			}
			set
			{
				this.autoCompleteTextBox1.ContextMenu = value;
			}
		}

		public int PopupWidth
		{
			get
			{
				return this.autoCompleteTextBox1.PopupWidth;
			}
			set
			{
				this.autoCompleteTextBox1.PopupWidth = value;
			}
		}

        private AutoCompleteTextBox autoCompleteTextBox1;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CoolTextBox()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);
			
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);

			Rectangle rect = this.ClientRectangle;
			rect.Width -= 1;
			rect.Height -= 1;
			Pen p = new Pen(this.BorderColor);
			e.Graphics.DrawRectangle(p, rect);
			
			p = new Pen(Color.FromArgb(100, this.BorderColor)); 
			rect.Inflate(-1, -1);
			e.Graphics.DrawRectangle(p, rect);
			
			p = new Pen(Color.FromArgb(45, this.BorderColor)); 
			rect.Inflate(-1, -1);
			e.Graphics.DrawRectangle(p, rect);

			p = new Pen(Color.FromArgb(15, this.BorderColor)); 
			rect.Inflate(-1, -1);
			e.Graphics.DrawRectangle(p, rect);

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.autoCompleteTextBox1 = new AutoCompleteTextBox();
			this.SuspendLayout();
			// 
			// autoCompleteTextBox1
			// 
			this.autoCompleteTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.autoCompleteTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoCompleteTextBox1.Location = new System.Drawing.Point(4, 4);
			this.autoCompleteTextBox1.Name = "autoCompleteTextBox1";
			this.autoCompleteTextBox1.PopupBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.autoCompleteTextBox1.PopupOffset = new System.Drawing.Point(12, 4);
			this.autoCompleteTextBox1.PopupSelectionBackColor = System.Drawing.SystemColors.Highlight;
			this.autoCompleteTextBox1.PopupSelectionForeColor = System.Drawing.SystemColors.HighlightText;
			this.autoCompleteTextBox1.PopupWidth = 120;
			this.autoCompleteTextBox1.Size = new System.Drawing.Size(120, 13);
			this.autoCompleteTextBox1.TabIndex = 0;
			this.autoCompleteTextBox1.Text = "autoCompleteTextBox1";
			this.autoCompleteTextBox1.SizeChanged += new System.EventHandler(this.TextBox_SizeChanged);
			// 
			// CoolTextBox
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.autoCompleteTextBox1);
			this.DockPadding.All = 4;
			this.Name = "CoolTextBox";
			this.Size = new System.Drawing.Size(128, 22);
			this.ResumeLayout(false);

		}
		#endregion

		private void TextBox_SizeChanged(object sender, System.EventArgs e)
		{
			AutoCompleteTextBox tb = sender as AutoCompleteTextBox;

			this.Height = tb.Height + 8;
		}

	}
}
