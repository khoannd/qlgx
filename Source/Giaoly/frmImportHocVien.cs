using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using GxControl;
using GxGlobal;
using Janus.Windows.EditControls;

namespace GiaoLy
{
    public partial class frmImportHocVien : frmBase
    {
        private IGxProcess import = null;
        private Thread thread = null;
        private bool hasError = false;

        private DataTable importedData = null;

        public DataTable ImportedData
        {
            get { return importedData; }
        }

        private int maLop = 0;
        public int MaLop
        {
            get { return maLop; }
            set { maLop = value; }
        }
        public frmImportHocVien()
        {
            InitializeComponent();
            gxCommand1.CancelButton.Text = "&Thoát";
            gxCommand1.OKButton.Click += OKButton_Click;
        }

        void OKButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (thread == null || thread.ThreadState == ThreadState.Stopped)
            {
                lblStatus.Text = "Chuẩn bị nhập dữ liệu...";
                Memory.Instance.SetMemory("UserAbort", false);
                EnableControl(false);
                txtErrorOutput.Clear();
                progressBar1.Value = 0;
                import = new ImportData(txtDB.Text, "Sheet1");
                import.ProcessOptions = ProcessOptions.ImportHocVien;
                import.ProcessData = maLop;
                import.OnStart += new EventHandler(import_OnStart);
                import.OnExecuting += new EventHandler(import_OnImporting);
                import.OnError += new CancelEventHandler(import_OnError);
                import.OnFinished += new EventHandler(import_OnFinished);

                ThreadStart threadStart = new ThreadStart(import.Execute);
                thread = new Thread(threadStart);
                thread.Start();
            }
            else if (thread.ThreadState == ThreadState.Suspended)
            {
                thread.Resume();
                btnStart.Text = "Tạm &dừng";
            }
            else if (thread.ThreadState == ThreadState.Running)
            {
                thread.Suspend();
                btnStart.Text = "Tiếp &tục";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart_Click(sender, e);
            if (MessageBox.Show("Nếu ngưng việc nhập dữ liệu giữa chừng, có thể sẽ có lỗi với dữ liệu của bạn.\r\nBạn có chắc dừng việc nhập dữ liệu?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                btnStart_Click(sender, e);
                return;
            }
            btnStart_Click(sender, e);
            this.Enabled = false;
            WriteStatus("Đang hủy bỏ việc nhập dữ liệu...");
            Memory.Instance.SetMemory("UserAbort", true);            
        }

        private void EnableControl(bool enable)
        {
            txtDB.Enabled = enable;
            btnSelectDB.Enabled = enable;
            btnStop.Enabled = !enable;
            btnStart.Text = enable? "Bắt đầu" : "Tạm &dừng";
            //lblPercent.Visible = !enable;
        }

        void import_OnStart(object sender, EventArgs e)
        {
            if (this.progressBar1.InvokeRequired)
            {
                EventHandler d = new EventHandler(import_OnStart);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                int total = (int)Memory.Instance.GetMemory("TongDuLieu");
                progressBar1.Maximum = total;
                WriteStatus("Bắt đầu");
            }            
        }

        void import_OnImporting(object sender, EventArgs e)
        {
            if (this.progressBar1.InvokeRequired)
            {
                EventHandler d = new EventHandler(import_OnImporting);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                int? current = (int?)Memory.Instance.GetMemory("DuLieuHienTai");
                if (current != null && current <= progressBar1.Maximum) progressBar1.Value = (int)current;
                //if (progressBar1.Maximum > 0)
                //{
                //    lblPercent.Text = string.Format("{0}%", ((int)(progressBar1.Value / progressBar1.Maximum) * 100));
                //}
                WriteStatus(sender.ToString());
            } 
        }

        void import_OnError(object sender, CancelEventArgs e)
        {
            if (this.txtErrorOutput.InvokeRequired)
            {
                CancelEventHandler d = new CancelEventHandler(import_OnError);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                //if (sender.ToString() == GXConstants.MSG_INVALID_FILE_IMPORT)
                //{
                //    MessageBox.Show(GXConstants.MSG_INVALID_FILE_IMPORT);
                //}
                WriteError(sender.ToString());
                hasError = true;
                btnStart.Text = "Bắt đầu";
            } 
        }

        void import_OnFinished(object sender, EventArgs e)
        {
            if (this.lblStatus.InvokeRequired)
            {
                EventHandler d = new EventHandler(import_OnFinished);
                this.Invoke(d, new object[] { sender, e });
            }
            else
            {
                bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                string msg = "";
                if (!userStop)
                {
                    msg = hasError ? "Kết thúc nhập dữ liệu. Có cảnh báo trong quá trình nhập." : "Kết thúc nhập dữ liệu";
                    if (!hasError) WriteError("Không có lỗi nào xảy ra");
                }
                else
                {
                    msg = "Bạn đã chọn ngưng giữa chừng việc nhập dữ liệu!";
                    this.Enabled = true;
                }
                if (sender != null && (sender as DataTable) != null)
                {
                    importedData = (DataTable)sender;
                }
                progressBar1.Value = 0;
                Memory.ClearError();
                Memory.Instance.ClearMemory();
                MessageBox.Show(msg);
                WriteStatus("Kết thúc");
                thread = null;
                hasError = false;
                EnableControl(true);
            } 
        }

        private void WriteStatus(string s)
        {
            lblStatus.Text = s;
            txtErrorOutput.Text += s + Environment.NewLine;
        }

        private void WriteError(string s)
        {
            txtErrorOutput.Text += s + Environment.NewLine;
        }

        private void btnSelectDB_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtDB.Text = openFileDialog1.FileName;
            }
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            if (thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Suspended))
            {
                btnStop_Click(sender, e);
            }
        }

        private void frmImportHocVien_Load(object sender, EventArgs e)
        {
            btnSelectDB_Click(sender, e);
        }

        
    }
}