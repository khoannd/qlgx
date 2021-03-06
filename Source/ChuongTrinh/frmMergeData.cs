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
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Janus.Windows.GridEX;

namespace GiaoXu
{
    public partial class frmMergeData : frmBase
    {
        private MergeData import = null;
        private Thread thread = null;
        private bool hasError = false;
        private frmMain frmMain = Program.frmMain;
        private string filePath = "";

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
        public frmMergeData()
        {
            InitializeComponent();
            this.HelpKey = "ket_noi_du_lieu";
            gxCommand1.OKButton.Visible = false;
            gxCommand1.OKButton.Text = "Lưu kết quả";
            gxCommand1.ToolTipOK = "Xuất kết quả trên lưới giáo dân và lưới gia đình ra file excel";
            gxCommand1.OKButton.Click += new EventHandler(OKButton_Click);
            gxCommand1.CancelButton.Text = "Thoát";
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string folderPath = folderBrowserDialog1.SelectedPath;
                string filePath = folderPath + "\\KetQuaNhapGiaoDan(" + System.DateTime.Now.ToString("ddMMMyyyy hhmmss") + ").xls";
                bool exportGiaoDan = Export(gxGiaoDanList1, filePath);

                filePath = folderPath + "\\KetQuaNhapGiaDinh(" + System.DateTime.Now.ToString("ddMMMyyyy hhmmss") + ").xls";
                bool exportGiaDinh = Export(gxGiaDinhList1, filePath);

                if (exportGiaoDan && exportGiaDinh)
                {
                    MessageBox.Show("Xuất báo cáo thành công");
                }
                else
                {
                    MessageBox.Show("Xuất báo cáo không thành công");
                }
            }
        }

        public bool Export(GridEX grid, string filePath)
        {
            try
            {
                Janus.Windows.GridEX.Export.GridEXExporter ex = new Janus.Windows.GridEX.Export.GridEXExporter();
                ex.GridEX = grid;

                System.IO.FileInfo file = new System.IO.FileInfo(filePath);
                System.IO.FileStream stream = file.OpenWrite();
                ex.IncludeCollapsedRows = true;
                ex.Export(stream);
                stream.Close();
                return true;
            }
            catch (Exception ex)
            {
                Memory.ShowError(ex.Message);
                return false;
            }
        }

        private void frmMergeData_Load(object sender, EventArgs e)
        {
            gxGiaDinhList1.FormatGrid();
            gxGiaoDanList1.FormatGrid();
            cbGiaoHoGiaDinh.SelectedValue = -1;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (thread == null || thread.ThreadState == ThreadState.Stopped)
            {
                lblStatus.Text = "Chuẩn bị nhập dữ liệu...";

                if (!ProcessVersion(filePath))
                {
                    return;
                }
                Memory.Instance.SetMemory("UserAbort", false);
                EnableControl(false);

                progressBar1.Value = 0;
                import = new MergeData(filePath, "Admin", GxConstants.DB_PASSWORD);
                //For giao dan
                import.OverrideGiaoDan = radOverrideGiaoDan.Checked;
                import.ForceOverrideGiaoDan = radForceOverrideGiaoDan.Checked;
                //For gia dinh
                import.OverrideGiaDinh = radOverrideGiaDinh.Checked;
                import.ForceOverrideGiaDinh = radForceOverrideGiaDinh.Checked;
                import.MaGiaoHoGiaDinh = cbGiaoHoGiaDinh.MaGiaoHo;
                //For hon phoi
                import.OverrideHonPhoi = radOverrideHonPhoi.Checked;
                import.ForceOverrideHonPhoi = radForceOverrideHonPhoi.Checked;
                //For giao ho
                import.OverrideGiaoHo = radOverrideGiaoHo.Checked;
                //For Dot Bi Tich
                import.OverrideBiTich = radOverrideDotBiTich.Checked;

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
            grbGiaoDan.Enabled = enable;
            grbGiaDinh.Enabled = enable;
            grbHonPhoi.Enabled = enable;
            grbGiaoHo.Enabled = enable;
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
                WriteError("Bắt đầu nhập dữ liệu từ tập tin: " + System.IO.Path.GetFileName(txtDB.Text) + " [" + Memory.GetDateString(DateTime.Now.ToString()) + "]");
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

        void import_OnError(object sender, EventArgs e)
        {
            if (this.txtErrorOutput.InvokeRequired)
            {
                EventHandler d = new EventHandler(import_OnError);
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
                if (!this.IsDisposed)
                {
                    bool userStop = (bool)Memory.Instance.GetMemory("UserAbort");
                    string msg = "";
                    if (!userStop)
                    {
                        msg = hasError ? "Kết thúc nhập dữ liệu. Có lỗi trong quá trình nhập.\r\nXin vui lòng gởi tập tin ImportLog.txt trong thư  mục chương trình hoặc thông tin trong mục thông báo kết quả cho tác giả để được hỗ trợ." : "Kết thúc nhập dữ liệu";
                        if (!hasError) WriteError("Không có lỗi nào xảy ra");
                    }
                    else
                    {
                        msg = "Bạn đã chọn ngưng giữa chừng việc nhập dữ liệu!";
                        this.Enabled = true;
                    }

                    //Export result to grid
                    gxGiaoDanList1.DataSource = import.tblGiaoDanMayCon;
                    gxGiaDinhList1.DataSource = import.tblGiaDinhCu;
                    gxCommand1.OKButton.Visible = true;

                    progressBar1.Value = 0;
                    Memory.ClearError();
                    Memory.Instance.ClearMemory();
                    MessageBox.Show(msg);
                    WriteStatus("Kết thúc");
                    WriteError("Kết thúc nhập dữ liệu từ tập tin: " + System.IO.Path.GetFileName(txtDB.Text) + " [" + Memory.GetDateString(DateTime.Now.ToString()) + "]");
                    thread = null;
                    hasError = false;
                    EnableControl(true);
                }
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
                filePath = txtDB.Text;
                if (filePath != "" && filePath.EndsWith(".zip"))
                {
                    FastZip zip = new FastZip();
                    string exPath = Memory.GetTempPath("");
                    zip.ExtractZip(filePath, exPath, "mdb");
                    filePath = exPath + "\\" + "giaoxu.mdb";
                }
                DataProvider provider = new DataProvider(filePath, "Admin", "khoanvnit");
                DataTable tblGiaoHo = provider.GetData("SELECT * FROM GiaoHo ");
                cbGiaoHoGiaDinh.LoadData(tblGiaoHo);
                cbGiaoHoGiaDinh.SelectedValue = -1;
                cbGiaoHoGiaDinh.Enabled = true;
            }
        }

        private void gxCommand1_OnCancel(object sender, EventArgs e)
        {
            if (thread != null && (thread.ThreadState == ThreadState.Running || thread.ThreadState == ThreadState.Suspended))
            {
                btnStop_Click(sender, e);
            }
        }

        #region for check version and update

        private bool ProcessVersion(string dbPath)
        {
            Memory.Instance.ChangeDatabase(dbPath, Memory.DbUser, Memory.DbPassword);
            Memory.LoadConfig();
            string dbVersion = Memory.DbVersion;
            try
            {
                if (Memory.CurrentVersion != "" && string.Compare(Memory.DbVersion = GetDbVersion(), Memory.CurrentVersion) < 0)
                {
                    frmProcess frmUpdate = new frmProcess();
                    frmUpdate.LabelStart = "Đang cập nhật dữ liệu được chọn lên phiên bản mới...";
                    frmUpdate.LableFinished = "Đã cập nhật xong!";
                    frmUpdate.Text = "Đang cập nhật dữ liệu được chọn lên phiên bản mới. Có thể mất vài phút. Xin vui lòng đợi...";
                    frmUpdate.ProcessClass = new UpdateProcess();
                    frmUpdate.StartFunction = new EventHandler(OnUpdating);
                    frmUpdate.FinishedFunction = new EventHandler(OnUpdateFinished);
                    frmUpdate.ShowDialog();
                    return true;
                }
                else if (Memory.CurrentVersion != "" && string.Compare(Memory.DbVersion, Memory.CurrentVersion) > 0)
                {
                    MessageBox.Show("Phiên bản dữ liệu được chọn mới hơn phiên bản chương trình hiện tại.\r\nXin vui lòng cập nhật chương trình lên phiên bản mới nhất trước khi tiếp tục sử dụng chức năng này.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Memory.ShowError(ex.Message);
            }
            finally
            {
                Memory.Instance.ChangeDatabase(Memory.AppPath + Memory.DbName, Memory.DbUser, Memory.DbPassword);
                Memory.DbVersion = dbVersion;
                Memory.LoadConfig();
            }
            return false;
        }

        public string GetDbVersion()
        {
            //return "2.0.0.1";
            
            if (Memory.GetConfig(GxConstants.CF_CURRENT_DB_VERSION) != "")
            {
                string version = Memory.GetConfig(GxConstants.CF_CURRENT_DB_VERSION);
                if (version == "2.0.0.0")
                {
                    if (!isDbVersion2())
                    {
                        return "1.0.0.3";
                    }
                }
                else if (version == "2.0.0.2")
                {
                    if (!isDbVersion2_0_0_2())
                    {
                        return "2.0.0.0";
                    }
                }
                else if (version == "2.0.0.3")
                {
                    if (!isDbVersion2_0_0_3())
                    {
                        return "2.0.0.2";
                    }
                }
                else if (version == "2.0.0.4")
                {
                    if (!isDbVersion2_0_0_4())
                    {
                        return "2.0.0.3";
                    }
                }
                return version;
            }
            if (System.IO.File.Exists(Memory.AppPath + GxConstants.VERSION_UPDATE_MARK_FILE))
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(Memory.AppPath + GxConstants.VERSION_UPDATE_MARK_FILE);
                string version = sr.ReadToEnd();
                if (version.Trim() != "1.0.0.3")
                {
                    return "1.0.0.3";
                }
            }
            return "1.0.0.1";
        }

        private bool isDbVersion2()
        {
            DataProvider provider = new DataProvider(Memory.Instance.Provider.ConnectionString);
            //Check GiaoDan table
            DataTable tblGiaoDan = provider.GetData("SELECT * FROM GiaoDan WHERE 1=2");//get table structure
            if (tblGiaoDan == null) return true;
            string[] columnCheck = new string[] { GiaoDanConst.SoRuocLe, GiaoDanConst.DaCoGiaDinh, GiaoDanConst.GiaoDanAo };
            foreach (string s in columnCheck)
            {
                if (!tblGiaoDan.Columns.Contains(s))
                {
                    return false;
                }
            }
            //Check GiaDinh Table
            DataTable tblGiaDinh = provider.GetData("SELECT * FROM GiaDinh WHERE 1=2");//get table structure
            columnCheck = new string[] { GiaDinhConst.GiaDinhAo };
            if (tblGiaDinh == null) return true;
            foreach (string s in columnCheck)
            {
                if (!tblGiaDinh.Columns.Contains(s))
                {
                    return false;
                }
            }
            return true;
        }

        private bool isDbVersion2_0_0_2()
        {
            DataProvider provider = new DataProvider(Memory.Instance.Provider.ConnectionString);
            //Check GiaoDan table
            DataTable tblGiaoDan = provider.GetData("SELECT * FROM GiaoDan WHERE 1=2");//get table structure
            if (tblGiaoDan == null) return true;
            string[] columnCheck = new string[] { GiaoDanConst.TanTong, GiaoDanConst.MaNhanDang };
            foreach (string s in columnCheck)
            {
                if (!tblGiaoDan.Columns.Contains(s))
                {
                    return false;
                }
            }
            //Check GiaDinh Table
            DataTable tblGiaDinh = provider.GetData("SELECT * FROM GiaDinh WHERE 1=2");//get table structure
            columnCheck = new string[] { GiaDinhConst.MaNhanDang };
            if (tblGiaDinh == null) return true;
            foreach (string s in columnCheck)
            {
                if (!tblGiaDinh.Columns.Contains(s))
                {
                    return false;
                }
            }
            return true;
        }

        private bool isDbVersion2_0_0_3()
        {
            DataProvider provider = new DataProvider(Memory.Instance.Provider.ConnectionString);
            //Check GiaoDan table
            DataTable tblGiaoDan = provider.GetData("SELECT * FROM GiaoDan WHERE 1=2");//get table structure
            if (tblGiaoDan == null) return true;
            string[] columnCheck = new string[] { GiaoDanConst.ThuocGiaoXu };
            foreach (string s in columnCheck)
            {
                if (!tblGiaoDan.Columns.Contains(s))
                {
                    return false;
                }
            }
            Memory.ClearError();
            DataTable tblRaoHP = provider.GetData("SELECT * FROM RaoHonPhoi WHERE 1=2");//get table structure
            if (Memory.HasError())
            {
                return false;
            }
            return true;
        }

        private bool isDbVersion2_0_0_4()
        {
            DataProvider provider = new DataProvider(Memory.Instance.Provider.ConnectionString);
            //Check GiaoDan table
            DataTable tblGiaDinh = provider.GetData("SELECT * FROM GiaDinh WHERE 1=2");//get table structure
            if (tblGiaDinh == null) return true;
            string[] columnCheck = new string[] { GiaDinhConst.MaGiaDinhRieng };
            foreach (string s in columnCheck)
            {
                if (!tblGiaDinh.Columns.Contains(s))
                {
                    return false;
                }
            }
            Memory.ClearError();
            return true;
        }

        public void OnUpdateFinished(object sender, EventArgs e)
        {
            try
            {
                if (frmMain.InvokeRequired)
                {
                    EventHandler d = new EventHandler(OnUpdateFinished);
                    frmMain.Invoke(d, new object[] { sender, e });
                }
                else
                {
                    frmMain.Enabled = true;
                    Memory.Instance.ChangeDatabase(Memory.AppPath + Memory.DbName, Memory.DbUser, Memory.DbPassword);
                    Memory.LoadConfig();
                }
            }
            catch
            {

            }
        }

        public void OnUpdating(object sender, EventArgs e)
        {
            if (frmMain.InvokeRequired)
            {
                EventHandler d = new EventHandler(OnUpdating);
                frmMain.Invoke(d, new object[] { sender, e });
            }
            else
            {
                frmMain.Enabled = false;
            }
        }
        #endregion
    }
}