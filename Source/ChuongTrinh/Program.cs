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

namespace GiaoXu
{
    static class Program
    {
        public static frmMain frmMain;
        public static bool firstTime = false;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
          
            //try
            //{
            Application.EnableVisualStyles();       //style Kiểu winXP

                Application.SetCompatibleTextRenderingDefault(false);
            //Load config
        //        Memory.LoadConfig();
                //Check database existed
                if (!File.Exists(Memory.AppPath + GxConstants.DB_FILENAME))
                {
                    firstTime = true;
                    if (!CreateDatabase())
                    {
                        return;
                    }
                }
                //frmMain = new frmMain();
                Memory.LoadConfig();
                ////update new version
                //ThreadStart threadStart = new ThreadStart(ProcessVersion);
                //Thread thread = new Thread(threadStart);
                //thread.Start();
                Application.Run(new frmLogin());
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }

        #region CreateDatabase
        public static bool CreateDatabase()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                
                using (Stream stream = assembly.GetManifestResourceStream("GiaoXu.Resources.giaoxu.mdb"))
                {
                    FileStream file = new FileStream(Memory.AppPath + GxConstants.DB_FILENAME, FileMode.CreateNew, FileAccess.Write);
                    int length = 256;
                    Byte[] buffer = new Byte[length];
                    int bytesRead = 0;
                    // write the required bytes
                    do
                    {
                        bytesRead = stream.Read(buffer, 0, length);
                        file.Write(buffer, 0, bytesRead);
                    }
                    while (bytesRead == length);
                    file.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Không thể tạo cơ sở dữ liệu. Xin vui lòng liên hệ tác giả\r\n" + ex.Message, "Exception CreateDatabase", System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        #endregion
    }
}