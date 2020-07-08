using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Data;
using GxGlobal;
using GxControl;
using System.Configuration;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Text; 
using System.Text.RegularExpressions;

namespace GiaoXu
{
    public class GxCheckVersion
    {
        private static frmLoadDataProcess fLoad;
        public event EventHandler ShowFormCreateGiaoXuOnline = null;
        public event EventHandler OnStart = null;
        public event EventHandler OnError = null;
        public event EventHandler OnFinished = null;

        private bool alert = false;

        public bool Alert
        {
            get { return alert; }
            set { alert = value; }
        }

        private bool hasNewVersion = false;

        public bool HasNewVersion
        {
            get { return hasNewVersion; }
            set { hasNewVersion = value; }
        }

        private string updateInfo = "";

        public string UpdateInfo
        {
            get { return updateInfo; }
            set { updateInfo = value; }
        }

        public void Execute()
        {
            if (OnStart != null) OnStart(this, EventArgs.Empty);
            try
            {
                ProcessVersion();
            }
            catch (Exception ex)
            {
                if (OnError != null) OnError(ex, EventArgs.Empty);
            }
            finally
            {
                if (OnFinished != null) OnFinished(this, EventArgs.Empty);
                BackupFile();
            }
        }

        #region for check version and update
        private Assembly loadUpdateModule()
        {
            try
            {
                return Assembly.LoadFile(Memory.AppPath + "AutoUpdate.exe");
            }
            catch
            {
                return null;
            }
        }

        public void CheckNewVersion()
        {
            try
            {
                if (!Memory.IsConnectionAvailable()) return;
                System.Net.WebClient web = new System.Net.WebClient();
                //check new version
                if (!Memory.ServerUrl.EndsWith("/")) Memory.ServerUrl += "/";
                string serverVersion = web.DownloadString(Memory.ServerUrl + "version.txt").Replace("ï»¿", "");
                serverVersion = serverVersion.Substring(serverVersion.Length - 7);
                string t = serverVersion.Replace(".", "");
                if (!Validator.IsNumber(t)) return;

                int check = string.Compare(serverVersion, Memory.CurrentVersion);
                //if no new version
                if (check <= 0)
                {
                    if (alert) MessageBox.Show("Không tìm thấy bản mới hơn. Phiên bản bạn đang dùng là phiên bản mới nhất",
                         "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //if has new version, get new version info
                Assembly updateAsm = loadUpdateModule();
                if (updateAsm == null)
                {
                    MessageBox.Show("Không tìm thấy thư viện cập nhật chương trình (Tập tin [AutoUpdate.exe] đã bị xóa hoặc bị hư).\r\nXin vui lòng cài đặt lại chương trình hoặc liên hệ tác giả");
                    return;
                }

                Type objectType = updateAsm.GetType("AutoUpdate.frmProcess");
                object update = Activator.CreateInstance(objectType);

                check = (int)update.GetType().InvokeMember("CheckVersion", BindingFlags.InvokeMethod, null, update, null);
                object info = update.GetType().InvokeMember("Information", BindingFlags.GetProperty, null, update, null);
                Memory.ServerUrl = (string)info.GetType().InvokeMember("ServerUrl", BindingFlags.GetProperty, null, info, null);
                //Memory.CurrentVersion = (string)info.GetType().InvokeMember("CurrentVersion", BindingFlags.GetProperty, null, info, null);

                if (check == -1)
                {
                    if (alert) MessageBox.Show(info.GetType().InvokeMember("ErrorInfo", BindingFlags.GetProperty, null, info, null).ToString(),
                         "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (check == 0)
                {
                    if (alert) MessageBox.Show("Không tìm thấy bản mới hơn. Phiên bản bạn đang dùng là phiên bản mới nhất",
                                             "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                hasNewVersion = true;
                updateInfo = (string)info.GetType().InvokeMember("UpdateInfo", BindingFlags.GetProperty, null, info, null);
                //if (MessageBox.Show("Đã có phiên bản mới hơn của chương trình. Bạn có muốn tải bản mới nhất không?" + Environment.NewLine +
                //                     "Thông tin cập nhật trong bản mới:" + Environment.NewLine + updateInfo +
                //                     "Nếu cập nhật thất bại, bạn vui lòng vào website " + Memory.ServerUrl + " để tại bản cập nhật mới nhất về hoặc liên hệ tác giả",
                //                        "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                //    return;

                //frmShowUpdateInfo updateDialog = new frmShowUpdateInfo();
                //updateInfo = updateInfo.Trim().Replace(Environment.NewLine, "<br />");
                //updateInfo = string.Concat("<thml><body style='font-family:Microsoft Sans Serif;font-size:12px;margin:0 10 0 10'>", updateInfo, "</body></html>");
                //updateDialog.Info = updateInfo;
                //DialogResult rs = updateDialog.ShowDialog(Program.frmMain);
                //if (rs != DialogResult.OK)
                //{
                //    return;
                //}

                //try
                //{
                //    System.Diagnostics.Process.Start("AutoUpdate.exe");
                //    Application.Exit();
                //}
                //catch (Exception)
                //{
                //    MessageBox.Show("Không tìm thấy file cập nhật chương trình. Xin vui lòng cài lại chương trình hoặc liên hệ tác giả", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
            }
            catch (Exception ex)
            {
                if (alert) MessageBox.Show(ex.Message, "Lỗi Exception (frmMain, CheckNewVersion)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ProcessVersion()
        {
            if (Memory.Instance.GetVersionInfo())
            {
                Memory.DbVersion = GetDbVersion();
                Console.WriteLine(Memory.CurrentVersion);
                Console.WriteLine(Memory.DbVersion);
                if (Memory.CurrentVersion != "" && Memory.DbVersion != "" && string.Compare(Memory.DbVersion, Memory.CurrentVersion) < 0)
                {
                    frmProcess frmUpdate = new frmProcess();
                    frmUpdate.LabelStart = "Đang cập nhật chương trình lên phiên bản mới. Xin vui lòng đợi...";
                    frmUpdate.LableFinished = "Đã cập nhật xong!";
                    frmUpdate.Text = "Đang cập nhật chương trình lên phiên bản mới. Có thể mất vài phút. Xin vui lòng đợi...";
                    frmUpdate.ProcessClass = new UpdateProcess();
                    frmUpdate.StartFunction = new EventHandler(OnUpdating);
                    frmUpdate.FinishedFunction = new EventHandler(OnUpdateFinished);
                    frmUpdate.ShowDialog();
                }
                if (Memory.GetConfig(GxConstants.CF_AUTO_UPDATE) == GxConstants.CF_TRUE)
                {
                    CheckNewVersion();
                }
            }

            //auto backup data 

            //if (!Program.firstTime)
            //{
            //    createBackupData();
            //}

            //hiepdv begin add

           
        }
        public void BackupFile()
        {
            WebClient wcl = new WebClient();
            if (!Memory.ServerUrl.EndsWith("/")) Memory.ServerUrl += "/";
         string UrlBackup = "https://957d11ecec10.ngrok.io/Parish-data-synchronization/QuanLyGiaoXu/";
            //  string UrlBackup = "http://ql.deploy-app.xyz/QuanLyGiaoXu/";
            //string UrlBackup =  wcl.DownloadString(Memory.ServerUrl + "urlbackup.txt").Replace("ï»¿", "");
            Memory.ChangeValueAppConfig("SERVER", UrlBackup);
            string UrlBackupFile = "https://957d11ecec10.ngrok.io/Parish-data-synchronization/data/CsvToClient/";
            //  string UrlBackupFile = "http://ql.deploy-app.xyz/data/CsvToClient/";
            //string UrlBackup =  wcl.DownloadString(Memory.ServerUrl + "urlbackupfile.txt").Replace("ï»¿", "");
            Memory.ChangeValueAppConfig("SERVER_File", UrlBackupFile);
            CheckThongTin();
        }
        public void CheckThongTin()
        {
             //create file backup and save to local
            string pathFileName = createBackupData();

            //check info giáo xứ
            DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (tblGiaoXu != null && tblGiaoXu.Rows.Count > 0)
            {
                //Kiểm tra giáo xứ đã gửi thông tin lên server chưa
                if (tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString() == "" && tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuDoi].ToString() != "")
                {
                    Thread tWait = new Thread(() =>
                    {
                        fLoad = new frmLoadDataProcess();
                        fLoad.ShowDialog();
                    });
                    tWait.IsBackground = true;
                    tWait.Start();
                    if (CheckThongTinDaRequest(tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuDoi].ToString()))
                    {
                        UploadFileAvatar();
                    }
                    fLoad.Invoke((MethodInvoker)delegate
                    {
                        fLoad.Close();
                    });
                }
            }
            //Đối với backup
            CheckThongTinTenServer(pathFileName);
            //Đối với Sync

            //hiepdv end add
        }
        //upload file to server 
        public bool UploadFileToServer(string linkToServer, string maGiaoXuRieng, string linkToFolderInLocal, string fileName, out DateTime UploadDate)
        {
            WebClient cl = new WebClient();
            UploadDate = new DateTime();
            try
            {
                // upload file backup to server
                //lay ve time upload server
                if (File.Exists(String.Concat(linkToFolderInLocal, "/", fileName)))
                {
                    byte[] rs = cl.UploadFile(String.Concat(linkToServer, maGiaoXuRieng), String.Concat(linkToFolderInLocal, fileName));
                    string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                    bool check = DateTime.TryParse(temp, out UploadDate);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi UploadFileToServer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        //Upload file avatar
        public void UploadFileAvatar()
        {
            DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (tblGiaoXu != null && tblGiaoXu.Rows.Count > 0 && tblGiaoXu.Rows[0][GiaoXuConst.Hinh].ToString() != "church.jpg" && tblGiaoXu.Rows[0][GiaoXuConst.Hinh].ToString() != "")
            {
                DateTime a;
                UploadFileToServer(ConfigurationManager.AppSettings["SERVER"] + @"GiaoXuCL/uploadAvatar/",
                    tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString(),
                    Memory.AppPath, tblGiaoXu.Rows[0][GiaoXuConst.Hinh].ToString(), out a);
            }
        }

        //kiểm tra thông tin đã request
        public bool CheckThongTinDaRequest(string MaGiaoXuDoi)
        {
            try
            {
                if (!Memory.TestConnectToServer())
                    return false;

                //request lên server kiểm tra xem giáo xứ đã được phê duyệt chưa
                WebClient cl = new WebClient();
                byte[] rs = cl.DownloadData(ConfigurationManager.AppSettings["SERVER"] + @"GiaoXuCL/getGiaoXuDaPheDuyet/" + MaGiaoXuDoi);
                string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                if (temp == "-1")
                    return false;
                List<GiaoXu> GiaoXuDoi = JsonConvert.DeserializeObject<List<GiaoXu>>(temp);
                string MaDinhDanh = GenerateUniqueCode.GetUniqueCode();
                string TenMay = GenerateUniqueCode.GetComputerName();
                if (Memory.SendMaDinhDanhTenMayLenServer(MaDinhDanh, TenMay, GiaoXuDoi[0].MaGiaoXuRieng))
                {
                    //Update Giáo Phận
                    Memory.ExecuteSqlCommand(SqlConstants.UPDATE_GIAOPHAN, GiaoXuDoi[0].TenGiaoPhan, GiaoXuDoi[0].MaGiaoPhanRieng,Memory.Instance.GetServerDateTime().ToString());
                    //Update Giáo Hạt
                    Memory.ExecuteSqlCommand(SqlConstants.UPDATE_GIAOHAT, GiaoXuDoi[0].TenGiaoHat, GiaoXuDoi[0].MaGiaoHatRieng, Memory.Instance.GetServerDateTime().ToString());
                    //Update Giáo Xứ
                    Memory.ExecuteSqlCommand(SqlConstants.UPDATE_GIAOXU, GiaoXuDoi[0].TenGiaoXu, GiaoXuDoi[0].DiaChi, GiaoXuDoi[0].DienThoai,
                                                GiaoXuDoi[0].Email, GiaoXuDoi[0].Website, GiaoXuDoi[0].Hinh, GiaoXuDoi[0].GhiChu, GiaoXuDoi[0].MaGiaoXuRieng, Memory.Instance.GetServerDateTime().ToString());
                }
                //Memory.SetMaGiaoXuRiengAllTable(GiaoXuDoi[0].MaGiaoXuRieng);
                return true;
                  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi CheckThongTinDaRequest " + ex.Message, "GxCheckVersion", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        //kiểm tra có thông tin trên server
        public void CheckThongTinTenServer(string pathFileName)
        {
            if (Memory.GetConfig(GxConstants.BACKUP_DATA_TO_SERVER) == "0" && Memory.GetConfig(GxConstants.SYNC_DATA_TO_SERVER) == "0")
            {
                return;
            }
            //chuỗi thông báo 
            string noti = "";
            if (Memory.GetConfig(GxConstants.BACKUP_DATA_TO_SERVER) == "1" && Memory.GetConfig(GxConstants.SYNC_DATA_TO_SERVER) == "1")
            {
                noti = " SAO LƯU DỮ LIỆU và ĐỒNG BỘ DỮ LIỆU ";
            }
            else
            {
                if (Memory.GetConfig(GxConstants.BACKUP_DATA_TO_SERVER) == "1")
                    noti = " SAO LƯU DỮ LIỆU ";
                else
                    noti = " ĐỒNG BỘ DỮ LIỆU ";
            }
            //Sao lưu dữ liệu lên server
            DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
            if (Memory.ShowError())
            {
                return;
            }
            if (tblGiaoXu == null || tblGiaoXu.Rows.Count <= 0)
            {
                Notify(true, noti);
            }
            else
            {
                if (tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString().Trim() == "" && tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuDoi].ToString().Trim() == "")
                {
                    Notify(false, noti);
                }
                else
                {
                    if (Memory.TestConnectToServer() && Memory.GetConfig(GxConstants.BACKUP_DATA_TO_SERVER) == "1")
                    {
                        Thread tWait = new Thread(() =>
                        {
                            fLoad = new frmLoadDataProcess();
                            fLoad.ShowDialog();
                        });
                        tWait.IsBackground = true;
                        tWait.Start();
                        //Backupfile
                        if(pathFileName!="")
                        uploadFile(pathFileName);
                        fLoad.Invoke((MethodInvoker)delegate
                        {
                            fLoad.Close();
                        });
                    }
                }
            }
        }
        public void Notify(bool theFirst, string noti)
        {
            string info = "";
            if (theFirst)
            {
                info = "Chương trình hiện đang có tính năng" + noti + "để phòng trường hợp giáo xứ bị mất dữ liệu, " +
                      "hoàn toàn không có mục đích nào khác và thông tin của giáo xứ là bảo mật.\r\n" +
                      "Chọn [YES] để chọn giáo xứ của bạn. \r\n" +
                      "Chọn [NO] để bỏ tính năng" + noti + "và nhập thông tin giáo xứ.\r\n" +
                      "Chọn [CANCEL] để nhập thông tin giáo xứ.\r\n" +
                      "(Để bật (tắt) tính năng" + noti + "vui lòng vào Công cụ -> Tùy chọn -> Sao lưu dữ).";
            }
            else
            {
                info = "Chương trình mới cập nhật tính năng" + noti + "để phòng trường hợp giáo xứ bị mất dữ liệu, " +
                        "hoàn toàn không có mục đích nào khác và thông tin của giáo xứ là bảo mật.\r\n" +
                        "Chọn [YES] để chọn giáo xứ của bạn. \r\n" +
                        "Chọn [NO] để bỏ tính năng" + noti + "và tiếp tục.\r\n" +
                        "Chọn [CANCEL] để nhập thông tin giáo xứ.\r\n" +
                        "(Để bật (tắt) tính năng" + noti + "vui lòng vào Công cụ -> Tùy chọn -> Sao lưu dữ).";
            }
            DialogResult tl = MessageBox.Show(info, "Thông báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            if (tl == DialogResult.Yes)
            {
                if (!Memory.TestConnectToServer())
                {
                    DialogResult tl1 = MessageBox.Show("Hiện tại máy tính của bạn không có kết nối Internet. Bạn vui lòng kiểm tra và khởi động lại chương trình!\r\n" +
                        "Chọn [YES] để thoát chương trình và kiểm tra lại Internet.\r\n" +
                        "Chọn [NO] để tiếp tục.",
                        "Thông báo lỗi", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                    if (tl1 == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    return;
                }
                else
                {
                    if (ShowFormCreateGiaoXuOnline != null)
                    {
                        ShowFormCreateGiaoXuOnline(this,EventArgs.Empty);
                        return;
                    }
                }
            }
            if (tl == DialogResult.No)
            {
                Memory.SetConfig(GxConstants.BACKUP_DATA_TO_SERVER, 0);
                Memory.SetConfig(GxConstants.SYNC_DATA_TO_SERVER, 0);
                MessageBox.Show("Hiện tại tính năng" + noti + " đã tắt.\r\n" +
                    "(Để bật (tắt) tính năng" + noti + "vui lòng vào Công cụ -> Tùy chọn -> Sao lưu dữ).", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public string GetDbVersion()
        {
            //return "2.0.0.1";
            string version = Memory.GetConfig(GxConstants.CF_CURRENT_DB_VERSION);
            if (version != "")
            {

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
            return version;
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
                if (Program.frmMain.InvokeRequired)
                {
                    EventHandler d = new EventHandler(OnUpdateFinished);
                    Program.frmMain.Invoke(d, new object[] { sender, e });
                }
                else
                {
                    Program.frmMain.Enabled = true;
                    frmHelp frm = new frmHelp();
                    frm.SetHelp("thong_tin_cap_nhat");
                    frm.WindowState = FormWindowState.Normal;
                    frm.Show();
                    Program.frmMain.ShowHelp("thong_tin_cap_nhat");
                }
            }
            catch
            {

            }
        }

        public void OnUpdating(object sender, EventArgs e)
        {
            if (Program.frmMain.InvokeRequired)
            {
                EventHandler d = new EventHandler(OnUpdating);
                Program.frmMain.Invoke(d, new object[] { sender, e });
            }
            else
            {
                try
                {
                    Program.frmMain.Enabled = false;
                }
                catch { }
            }
        }
        #endregion

        #region backup data
        private string createBackupData()
        {
            try
            {
                int max = 40;
                string maxBackup = Memory.GetConfig(GxConstants.CF_MAX_BACKUP);
                if (Validator.IsNumber(maxBackup))
                {
                    max = int.Parse(maxBackup);
                }
                string backupPath = Memory.AppPath + "backup\\";
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                //check and delete old file, only keep last 40 files in backup
                DirectoryInfo dir = new DirectoryInfo(backupPath);
                FileInfo[] files = dir.GetFiles("*.zip");
                if (files.Length > max)
                {
                    for (int i = files.Length - 1; i >= 0; i--)
                    {
                        if (i > max) files[i].Delete();
                    }
                    //foreach (FileInfo file in files)
                    //{
                    //    TimeSpan ts = System.DateTime.Now.Subtract(file.CreationTime.Date);
                    //    if (ts.TotalDays > 30)
                    //    {
                    //        file.Delete();
                    //    }
                    //}
                }
                //2018/09/30 gnguyen start delete
                //string fileName = "data" + System.DateTime.Now.ToString("yyyyMMddHH") + ".zip";
                //2018/09/30 gnguyen end delete 
                //2018/09/30 gnguyen start add
                string fileName = "data" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";
                //2018/09/30 gnguyen end add 


                if (!File.Exists(backupPath + fileName))
                {
                    FastZip fzip = new FastZip();
                    //string path = Memory.GetTempPath(filePath);
                    fzip.CreateZip(backupPath + fileName, Memory.AppPath, false, "giaoxu.mdb");
                }
                //2018-08-08 Gia add start
                //uploadFile(fileName, backupPath);
                return backupPath + fileName;
                //2018-08-08 Gia add end
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception createBackupData()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }
        #region 2018-08-14 Gia add start
        //private int insertInfoGXInFirstTime()
        //{
        //    try
        //    {
        //        WebClient cl = new WebClient();
        //        NameValueCollection infoGX = new NameValueCollection();
        //        infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOXU), GiaoXuConst.TableName, GiaoXuConst.MaGiaoXuRieng));
        //        infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOHAT), GiaoHatConst.TableName, GiaoHatConst.MaGiaoHatRieng));
        //        infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOPHAN), GiaoPhanConst.TableName, GiaoPhanConst.MaGiaoPhanRieng));
        //        if (infoGX.Count > 0)
        //        {
        //            byte[] rs = cl.UploadValues(ConfigurationManager.AppSettings["SERVER"] + @"GiaoXuCL/insert", "POST", infoGX);
        //            string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
        //            Dictionary<string, int> maID = JsonConvert.DeserializeObject<Dictionary<string, int>>(temp);
        //            if (maID.Count > 0)
        //            {
        //                if (maID.ContainsKey("IDGP"))
        //                {
        //                    if(int.Parse(maID["IDGP"].ToString())>=0)
        //                        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOPHANRIENG, new object[] { maID["IDGP"] });
        //                }
        //                if (maID.ContainsKey("IDGH"))
        //                {
        //                    if (int.Parse(maID["IDGH"].ToString()) >= 0)
        //                        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOHATRIENG, new object[] { maID["IDGH"] });
        //                }
        //                if (maID.ContainsKey("IDGX"))
        //                {
        //                    Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOXURIENG, new object[] { maID["IDGX"] });
        //                }
        //                return maID["IDGX"];
        //            }
        //        }
        //        return -1;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message,"GxCheckVersion");
        //        throw;
        //    }

        //}

        //private NameValueCollection createrInfoTable(DataTable tbl, string nameTable, string nameCotRieng)
        //{
        //    if (tbl != null && tbl.Rows.Count > 0)
        //    {
        //        NameValueCollection temp = new NameValueCollection();
        //        int maRieng;

        //        bool check = int.TryParse(tbl.Rows[0][nameCotRieng].ToString(), out maRieng);
        //        if (!check)
        //        {
        //            temp.Add(nameTable, "");
        //            foreach (DataColumn item in tbl.Columns)
        //            {
        //                temp.Add(nameTable + item.ColumnName, tbl.Rows[0][item].ToString());
        //            }
        //        }
        //        return temp;

        //    }
        //    return null;
        //}
        #endregion


        //2018-08-14 Gia add end
        private void uploadFile(string pathFileName)
        {
            //2018-08-01 Gia add start
            try
            {
                //upload to server
                //get thong tin giaoxu
                DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                if (tblGiaoXu != null && tblGiaoXu.Rows.Count > 0)
                {
                    string maGiaoXuRieng = tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString().Trim();
                    string maDinhDanh = GenerateUniqueCode.GetUniqueCode().ToString();
                    string tenMay = GenerateUniqueCode.GetComputerName().ToString();
                    if (maGiaoXuRieng != "" && maDinhDanh != "")
                    {
                        // bool check = int.TryParse(tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString(), out maGiaoXuRieng);
                        //if (!check)//Giao xu chưa có thông tin trên server
                        //{
                        //    maGiaoXuRieng = insertInfoGXInFirstTime();
                        //}
                        //check Last Upload
                        DateTime lastUpload;
                        bool check = DateTime.TryParse(tblGiaoXu.Rows[0][GiaoXuConst.LastUpload].ToString(), out lastUpload);
                        if (!check || DateTime.Now.Subtract(lastUpload).TotalDays > 1.0)//last > 1 ngày
                        {
                            WebClient client = new WebClient();
                            // upload file backup to server//lay ve time upload server
                            byte[] rs = client.UploadFile(ConfigurationManager.AppSettings["SERVER"] + String.Format("BackupCL/uploadFile/{0}/{1}/{2}",maGiaoXuRieng, maDinhDanh, tenMay), pathFileName);

                            string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                            check = DateTime.TryParse(temp, out lastUpload);
                            if (check)
                            {
                                Memory.ExecuteSqlCommand(SqlConstants.UPDATE_LASTUPLOAD, new object[] { lastUpload,Memory.Instance.GetServerDateTime().ToString() });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception uploadfileServer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //2018-08-01 Gia add end
        }

        //2018/09/30 gnguyen start add


        private string createrFileSyn()
        {
            string giaoxusynPath = Memory.AppPath + "sync\\";
            if (!Directory.Exists(giaoxusynPath))
            {
                Directory.CreateDirectory(giaoxusynPath);
            }
            DataSet ds = new DataSet();
            ds.Tables.AddRange(new DataTable[] {
                Memory.GetData(SqlConstants.SELECT_ALLBiTichChiTiet,BiTichChiTietConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLCauHinh,CauHinhConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLChiTietLopGiaoLy,ChiTietLopGiaoLyConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLChuyenXu,ChuyenXuConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLDotBiTich,DotBiTichConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLDuLieuChung,DuLieuChungConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaDinh,GiaDinhConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoDan,GiaoDanConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoDanHonPhoi,GiaoDanHonPhoiConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoHat,GiaoHatConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoHo,GiaoHoConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoLyVien,GiaoLyVienConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoPhan,GiaoPhanConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLGiaoXu,GiaoXuConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLHonPhoi,HonPhoiConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLKhoiGiaoLy,KhoiGiaoLyConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLLinhMuc,LinhMucConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLLopGiaoLy,LopGiaoLyConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLRaoHonPhoi,RaoHonPhoiConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLTanHien,TanHienConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLThanhVienGiaDinh,ThanhVienGiaDinhConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLVaiTro,VaiTroConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLTaiKhoan,TaiKhoanConst.TableName),
                Memory.GetData(SqlConstants.SELECT_ALLTenLoaiTaiKhoan,TenLoaiTaiKhoanConst.TableName),
            });
            if (ds.Tables.Count > 0)
            {
                foreach (DataTable item in ds.Tables)
                {
                    string temp = this.ToCSV(item);
                    using (StreamWriter sw = new StreamWriter(giaoxusynPath + item.TableName + ".csv"))
                    {
                        sw.Write(temp);
                    }

                }
                string fileName = "gxsyn" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";
                FastZip fzip = new FastZip();
                fzip.CreateZip(giaoxusynPath + fileName, giaoxusynPath, false, @"\.csv$");
                foreach (string sFile in System.IO.Directory.GetFiles(giaoxusynPath, "*.csv"))
                {
                    System.IO.File.Delete(sFile);
                }
                return giaoxusynPath + fileName;
            }
            return null;
        }
        private string ToCSV(DataTable table)
        {
            var result = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                result.Append(table.Columns[i].ColumnName);
                result.Append(i == table.Columns.Count - 1 ? "\n" : ";");
            }

            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {

                    var value = row[i];
                    if (value.GetType().Name == "String")
                    {
                        value = Regex.Replace(value.ToString(), @"(\s{2,})|\n", " ");

                    }
                    if (value.GetType().Name == "DateTime")
                    {
                        value = string.Format("{0:yyyy/MM/dd hh:mm:ss}", value);
                    }
                    result.Append(value);
                    result.Append(i == table.Columns.Count - 1 ? "\n" : ";");
                }
            }

            return result.ToString();
        }

        //2018/09/30 gnguyen end add
        #endregion
    }
}
