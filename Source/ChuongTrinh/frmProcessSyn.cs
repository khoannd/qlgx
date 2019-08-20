using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmProcessSyn : Form
    {
        public frmProcessSyn()
        {
            InitializeComponent();
        }

        private void frmProcessSyn_Load(object sender, EventArgs e)
        {
            progressBar1.Show();
        }

        private void frmProcessSyn_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }
    }
}
