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

namespace GiaoXu
{
    public partial class frmImport : frmBase
    {
        public enum ImportType
        {
            MGC,
            Excel
        }
        private IGxProcess import = null;
        private Thread thread = null;
        private bool hasError = false;
        private ImportType importDataType = ImportType.MGC;

        public ImportType ImportDataType
        {
            get { return importDataType; }
            set { importDataType = value; }
        }
        public frmImport()
        {
            InitializeComponent();
            gxCommand1.OKButton.Visible = false;
            gxCommand1.CancelButton.Text = "Thoát";
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
                if (txtDB.Text.Trim().EndsWith("xls") || txtDB.Text.Trim().EndsWith("xlsx"))
                {
                    import = new ImportData(txtDB.Text, "Table1");
                }
                else
                {
                    import = new ImportDataMGC(txtDB.Text, "", "");
                }
                import.ProcessData = cmbGiaoXu.Combo.SelectedValue;
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
                    msg = hasError ? "Kết thúc nhập dữ liệu. Có lỗi trong quá trình nhập.\r\nXem file ImportLog.txt trong thư mục chương trình để biết thêm chi tiết" : "Kết thúc nhập dữ liệu";
                    if (!hasError) WriteError("Không có lỗi nào xảy ra");
                }
                else
                {
                    msg = "Bạn đã chọn ngưng giữa chừng việc nhập dữ liệu!";
                    this.Enabled = true;
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
                if (openFileDialog1.FileName.ToLower().EndsWith(".mdb") || openFileDialog1.FileName.ToLower().EndsWith(".zip")
                     || openFileDialog1.FileName.ToLower().EndsWith(".accdb"))
                {
                    cmbGiaoXu.Combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    DataProvider provider = new DataProvider(openFileDialog1.FileName, "Admin", "");
                    DataTable tblGiaoXuCu = provider.GetData(@"
SELECT gx.MaGiaoPhan, gx.MaGiaoXu, gp.TenGiaoPhan, gh.TenHat, gxl.TenGiaoXu, gx.DiaChi, gx.XaHuyenQuan, gx.TinhTP, gx.DienThoai, gx.EMail
FROM ((GiaoXu AS gx INNER JOIN GXListAct AS gxl ON gx.MaGiaoXu = gxl.MaGiaoXu) INNER JOIN GPList AS gp ON gx.MaGiaoPhan = gp.MaGiaoPhan)  INNER JOIN HatList AS gh ON gx.MaHat = gh.MaHat AND gh.MaGiaoPhan=gx.MaGiaoPhan
GROUP BY gx.MaGiaoPhan, gx.MaGiaoXu, gp.TenGiaoPhan, gh.TenHat, gxl.TenGiaoXu, gx.DiaChi, gx.XaHuyenQuan, gx.TinhTP, gx.DienThoai, gx.EMail
");
                    if (tblGiaoXuCu == null || tblGiaoXuCu.Rows.Count == 0)
                    {
                        MessageBox.Show("Không tìm thấy thông tin giáo xứ. Chương trình không thể nhập dữ liệu từ tập tin bạn chọn", "Tập tin không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnStart.Enabled = false;
                        btnStop.Enabled = false;
                        cmbGiaoXu.Combo.Items.Clear();
                        return;
                    }
                    cmbGiaoXu.Visible = true;
                    btnStart.Enabled = true;
                    btnStop.Enabled = true;
                    cmbGiaoXu.Combo.Items.Clear();
                    Dictionary<int, string> items = new Dictionary<int, string>();
                    foreach (DataRow row in tblGiaoXuCu.Rows)
                    {
                        items.Add(Memory.GetInt(row["MaGiaoXu"]), Memory.ConvertVNI2UNI(row["TenGiaoXu"].ToString()));
                    }
                    cmbGiaoXu.Combo.DataSource = new BindingSource(items, null);
                    cmbGiaoXu.Combo.DisplayMember = "Value";
                    cmbGiaoXu.Combo.ValueMember = "Key";
                    cmbGiaoXu.Combo.SelectedIndex = 0;
                }
                else
                {
                    btnStart.Enabled = true;
                    btnStop.Enabled = true;
                    cmbGiaoXu.Visible = false;
                    cmbGiaoXu.Combo.Items.Clear();
                }
            }
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            if (thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Suspended))
            {
                btnStop_Click(sender, e);
            }
        }

        private void frmImport_Load(object sender, EventArgs e)
        {
            if (importDataType == ImportType.MGC)
            {
                this.Text = "Nhap du lieu tu chuong trinh MGC";
                this.openFileDialog1.Filter = "MS Access File (*.mdb, *.accdb)|*.mdb;*.accdb|All files (*.*)|*.*";
            }
            else if (importDataType == ImportType.Excel)
            {
                this.Text = "Nhap du lieu tu chuong trinh MS Excel";
                this.openFileDialog1.Filter = "MS Excel (*.xls, *.xlsx)|*.xls;*.xlsx";
                this.openFileDialog1.FileName = "mau-excel-giao-dan.xls";
            }
        }

        
    }
}