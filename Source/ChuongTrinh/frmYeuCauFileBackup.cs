using GxControl;
using GxGlobal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GiaoXu
{
    public partial class frmYeuCauFileBackup : frmBase
    {
        private static frmLoadDataProcess fLoad;
        public frmYeuCauFileBackup()
        {
            InitializeComponent();
            gxCommand1.OKButton.Click += OKButton_Click;
        }
        public object ListMayNhap
        {
            set
            {
                if(value!=null)
                {
                    cbTenMayNhap.combo.DataSource = value;
                    cbTenMayNhap.combo.DisplayMember = "TenMay";
                    cbTenMayNhap.combo.SelectedIndex = -1;
                }
            }
            get
            {
                return cbTenMayNhap.combo.DataSource;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if(txtConfirmEmail.Text.ToString().Trim()=="")
            {
                MessageBox.Show("Vui lòng nhập Email xác nhận.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cbTenMayNhap.Text.ToString().Trim() == "")
            {
                MessageBox.Show("Vui lòng chọn hoặc nhập tên của máy nhập cần nhận tập tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Memory.TestConnectToServer())
            {
                MessageBox.Show("Hiện tại máy tính của bạn không có internet! \r\n" +
                    "Vui lòng kiểm tra lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Thread t = new Thread(()=> {
                fLoad = new frmLoadDataProcess();
                fLoad.ShowDialog();
            });
            t.IsBackground = true;
            t.Start();
            DataTable tblGiaoXu = Memory.GetData("Select * from GiaoXu");
            string tenGiaoXu = tblGiaoXu.Rows[0][GiaoXuConst.TenGiaoXu].ToString();
            string tenMay = GenerateUniqueCode.GetComputerName();
            string email = txtConfirmEmail.Text;
            string ghiChu = txtGhiChu.Text;
            string tenMayNhap = cbTenMayNhap.Text;
            try
            {
                WebClient cl = new WebClient();
                NameValueCollection infoYeuCau = new NameValueCollection();
                infoYeuCau.Add("TenGiaoXu", tenGiaoXu);
                infoYeuCau.Add("TenMay", tenMay);
                infoYeuCau.Add("Email", email);
                infoYeuCau.Add("GhiChu", ghiChu);
                infoYeuCau.Add("TenMayNhap", tenMayNhap);
                byte[] rs = cl.UploadValues(ConfigurationManager.AppSettings["SERVER"] + @"GiaoXuCL/sendMailYeuCauNhanFile", "POST", infoYeuCau);
                string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                t.Abort();
                if (Int32.Parse(temp)==1)
                {
                    MessageBox.Show("Yêu cầu nhận tập tin thành công. Vui lòng đợi hệ thống xử lý.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Yêu cầu nhận tập tin thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
                this.Close();
            }
            catch (Exception)
            {
                t.Abort();
                MessageBox.Show("Yêu cầu nhận tập tin thất bại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
}
