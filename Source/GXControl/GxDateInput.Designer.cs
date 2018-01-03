namespace GxControl
{
    partial class GxDateInput
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
            this.txtYear = new System.Windows.Forms.TextBox();
            this.txtMonth = new System.Windows.Forms.TextBox();
            this.txtDay = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // txtYear
            // 
            this.txtYear.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtYear.Location = new System.Drawing.Point(56, 4);
            this.txtYear.MaxLength = 4;
            this.txtYear.Name = "txtYear";
            this.txtYear.Size = new System.Drawing.Size(27, 13);
            this.txtYear.TabIndex = 4;
            this.txtYear.Text = "2009";
            this.txtYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtYear.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtYear_MouseMove);
            this.txtYear.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtYear_KeyDown);
            this.txtYear.Leave += new System.EventHandler(this.txtYear_Leave);
            this.txtYear.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtYear_KeyUp);
            this.txtYear.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtYear_KeyPress);
            this.txtYear.Enter += new System.EventHandler(this.txtYear_Enter);
            this.txtYear.Validating += new System.ComponentModel.CancelEventHandler(this.txtYear_Validating);
            // 
            // txtMonth
            // 
            this.txtMonth.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMonth.Location = new System.Drawing.Point(30, 4);
            this.txtMonth.MaxLength = 2;
            this.txtMonth.Name = "txtMonth";
            this.txtMonth.Size = new System.Drawing.Size(16, 13);
            this.txtMonth.TabIndex = 3;
            this.txtMonth.Text = "05";
            this.txtMonth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtMonth.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtMonth_MouseMove);
            this.txtMonth.Leave += new System.EventHandler(this.txtMonth_Leave);
            this.txtMonth.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtMonth_KeyUp);
            this.txtMonth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMonth_KeyPress);
            this.txtMonth.Enter += new System.EventHandler(this.txtMonth_Enter);
            this.txtMonth.Validating += new System.ComponentModel.CancelEventHandler(this.txtMonth_Validating);
            // 
            // txtDay
            // 
            this.txtDay.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDay.Location = new System.Drawing.Point(2, 4);
            this.txtDay.MaxLength = 2;
            this.txtDay.Name = "txtDay";
            this.txtDay.Size = new System.Drawing.Size(16, 13);
            this.txtDay.TabIndex = 2;
            this.txtDay.Text = "05";
            this.txtDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDay.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtDay_MouseMove);
            this.txtDay.Leave += new System.EventHandler(this.txtDay_Leave);
            this.txtDay.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtDay_KeyUp);
            this.txtDay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDay_KeyPress);
            this.txtDay.Enter += new System.EventHandler(this.txtDay_Enter);
            this.txtDay.Validating += new System.ComponentModel.CancelEventHandler(this.txtDay_Validating);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(48, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "/";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(21, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(12, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "/";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(0, 1);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(92, 20);
            this.textBox1.TabIndex = 7;
            this.textBox1.Enter += new System.EventHandler(this.textBox1_Enter);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(90, 1);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(19, 20);
            this.dateTimePicker1.TabIndex = 8;
            this.dateTimePicker1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dateTimePicker1_MouseMove);
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            this.dateTimePicker1.DropDown += new System.EventHandler(this.dateTimePicker1_DropDown);
            this.dateTimePicker1.Enter += new System.EventHandler(this.dateTimePicker1_Enter);
            // 
            // GxDateInput
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.txtYear);
            this.Controls.Add(this.txtMonth);
            this.Controls.Add(this.txtDay);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.dateTimePicker1);
            this.Name = "GxDateInput";
            this.Size = new System.Drawing.Size(117, 23);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GxDateInput_MouseMove);
            this.Leave += new System.EventHandler(this.GxDateInput_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtYear;
        private System.Windows.Forms.TextBox txtMonth;
        private System.Windows.Forms.TextBox txtDay;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
    }
}
