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
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace GiaoXu
{
    public class GxCheckVersion
    {
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
            if (!Program.firstTime)
            {
                createBackupData();
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
        private void createBackupData()
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
                //
                //2018-08-13 Gia modify start
                string fileName = "data" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";
                //2018-08-13 Gia modify end
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                if (!File.Exists(backupPath + fileName))
                {
                    FastZip fzip = new FastZip();
                    //string path = Memory.GetTempPath(filePath);
                    fzip.CreateZip(backupPath + fileName, Memory.AppPath, false, "giaoxu.mdb");
                }
                //2018-08-08 Gia add start
                uploadFile(fileName, backupPath);
                //2018-08-08 Gia add end
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi Exception createBackupData()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //2018-08-14 Gia add start
        private int insertInfoGXInFirstTime()
        {
            WebClient cl = new WebClient();
            NameValueCollection infoGX = new NameValueCollection();
            infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOXU),GiaoXuConst.TableName, GiaoXuConst.MaGiaoXuRieng));
            infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOHAT),GiaoHatConst.TableName, GiaoHatConst.MaGiaoHatRieng));
            infoGX.Add(createrInfoTable(Memory.GetData(SqlConstants.SELECT_GIAOPHAN),GiaoPhanConst.TableName, GiaoPhanConst.MaGiaoPhanRieng));
            if (infoGX.Count>0)
            {
                byte[] rs = cl.UploadValues(GxConstants.SERVER + @"GiaoXuCL/insert", "post", infoGX);
                string temp= System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                Dictionary<string, int> maID = JsonConvert.DeserializeObject<Dictionary<string, int>>(temp);
                if (maID.Count>0)
                {
                    if (maID.ContainsKey("IDGP"))
                    {
                        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOPHANRIENG, new object[] { maID["IDGP"] });
                    }
                    if (maID.ContainsKey("IDGH"))
                    {
                        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOHATRIENG, new object[] { maID["IDGH"] });
                    }
                    if (maID.ContainsKey("IDGX"))
                    {
                        Memory.ExecuteSqlCommand(SqlConstants.UPDATE_MAGIAOXURIENG, new object[] { maID["IDGX"] });
                    }
                    return maID["IDGX"];
                }
            }
            return -1;

        }
        private NameValueCollection createrInfoTable(DataTable tbl,string nameTable, string nameCotRieng)
        {
            if (tbl.Rows.Count > 0)
            {
                NameValueCollection temp = new NameValueCollection();
                int maRieng;
               
                bool check = int.TryParse(tbl.Rows[0][nameCotRieng].ToString(), out maRieng);
                if (!check)
                {
                    temp.Add(nameTable, "");
                    foreach (DataColumn item in tbl.Columns)
                    {
                        temp.Add(nameTable + item.ColumnName, tbl.Rows[0][item].ToString());
                    }
                }
                return temp;

            }
            return null;
        }
        //2018-08-14 Gia add end
        private void uploadFile(string fileName, string backupPath)
        {

            //2018-08-01 Gia add start
            WebClient cl = new WebClient();
            try
            {
                //upload to server
                //get thong tin giaoxu
                DataTable tblGiaoXu = Memory.GetData(SqlConstants.SELECT_GIAOXU);
                if (tblGiaoXu.Rows.Count > 0)
                {
                    int maGiaoXuRieng;
                    bool check = int.TryParse(tblGiaoXu.Rows[0][GiaoXuConst.MaGiaoXuRieng].ToString(), out maGiaoXuRieng);
                    if (!check)//Giao xu chưa có thông tin trên server
                    {
                       maGiaoXuRieng= insertInfoGXInFirstTime();
                    }
                    //check Last Upload
                    DateTime lastUpload;
                    check = DateTime.TryParse(tblGiaoXu.Rows[0][GiaoXuConst.LastUpload].ToString(), out lastUpload);
                    if (!check || DateTime.Now.Subtract(lastUpload).TotalDays > 1.0)//last > 1 ngày
                    {
                        // upload file backup to server//lay ve time upload server
                        byte[] rs = cl.UploadFile(GxConstants.SERVER + @"BackupCL/uploadFile/" + maGiaoXuRieng, backupPath + fileName);
                        string temp = System.Text.Encoding.UTF8.GetString(rs, 0, rs.Length);
                        check = DateTime.TryParse(temp, out lastUpload);
                        if (check)
                        {
                            Memory.ExecuteSqlCommand(SqlConstants.UPDATE_LASTUPLOAD, new object[] { lastUpload });
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


        #endregion
    }
}
