using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using GxGlobal;
using GxControl;

namespace GiaoXu
{
    public partial class frmAbout : frmBase
    {
        public frmAbout()
        {
            InitializeComponent();
            gxCommand1.OKButton.Visible = false;
            gxCommand1.CancelButton.Text = "Đóng";
            lblVersion.Text = "Phiên bản: " + Memory.CurrentVersionDisplay;
            label8.Text = "Rất mong nhận được ý kiến đóng góp của mọi người để chương trình ngày càng hoàn thiện hơn." + Environment.NewLine +
                            "Mọi ý kiến đóng góp và thông báo lỗi, xin vui lòng liên hệ tác giả." + Environment.NewLine +
                            "Chân thành cảm ơn!";
            if (Memory.ServerUrl == "") Memory.ServerUrl = GxConstants.SERVER_URL_DEFAULT;
            lblWebsite.Text = Memory.ServerUrl;
            lblEmail2.Text = GxConstants.EMAIL;
        }

        private void lblEmail2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:" + GxConstants.EMAIL);
        }

        private void lblWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Memory.ServerUrl);
        }

    }
}