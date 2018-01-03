using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmShowUpdateInfo : Form
    {
        string info = "";

        public string Info
        {
            get { return info; }
            set { info = value; }
        }
        public frmShowUpdateInfo()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnChiTiet_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GxGlobal.Memory.ServerUrl + "help/thong_tin_cap_nhat.htm");
        }

        private void frmShowUpdateInfo_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("about:blank");
            if (webBrowser1.Document != null)
            {
                webBrowser1.Document.Write(string.Empty);
            }
            webBrowser1.DocumentText = info;
            //webBrowser1.Url = new System.Uri(GxGlobal.Memory.ServerUrl + "help/thong_tin_cap_nhat.htm", System.UriKind.Absolute);
        }
    }
}
