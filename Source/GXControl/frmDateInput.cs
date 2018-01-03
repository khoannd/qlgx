using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GxControl
{
    public partial class frmDateInput : frmBase
    {
        public frmDateInput()
        {
            InitializeComponent();
            this.AcceptButton = btnOK;
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd/MM/yyyy";
        }

        public string Label
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        public DateTime Value
        {
            get { return dateTimePicker1.Value; }
            set { dateTimePicker1.Value = value; }
        }

        public DateTimePicker DateTimePicker
        {
            get { return dateTimePicker1; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}