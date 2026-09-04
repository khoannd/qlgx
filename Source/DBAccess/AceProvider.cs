using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace GxGlobal
{
    /// <summary>
    /// Kiểm tra, tải về và cài đặt Microsoft Access Database Engine bản 32-bit (ACE OLEDB).
    ///
    /// Chương trình chạy ở chế độ 32-bit (PlatformTarget = x86) nên chỉ nạp được OLE DB
    /// provider 32-bit. Máy cài Office 64-bit chỉ có ACE 64-bit, khi đó chương trình
    /// KHÔNG đọc được file .accdb (nhập từ MGC) và .xlsx (nhập từ Excel).
    /// File .mdb không bị ảnh hưởng vì dùng Microsoft.Jet.OLEDB.4.0 có sẵn trong Windows.
    ///
    /// Lớp này chỉ chứa logic, không chứa giao diện. Phần hội thoại với người dùng
    /// nằm ở frmCaiDatAce trong project GiaoXu.
    /// </summary>
    public static class AceProvider
    {
        /// <summary>Tên hiển thị cho người dùng.</summary>
        public const string TEN_HIEN_THI = "Microsoft Access Database Engine 2016 (bản 32-bit)";

        /// <summary>Tên file cài đặt khi lưu vào thư mục tạm.</summary>
        public const string TEN_FILE_CAI = "AccessDatabaseEngine_x86.exe";

        /// <summary>Khoá trong app.config để đổi link tải mà không cần build lại chương trình.</summary>
        public const string KHOA_CAUHINH_URL = "ACCESS_DATABASE_ENGINE_URL";

        private const string URL_MAC_DINH =
            "https://download.microsoft.com/download/3/5/C/35C84C36-661A-44E6-9324-8786B8DBE231/AccessDatabaseEngine.exe";

        /// <summary>File cài thật khoảng 78 MB. Nhỏ hơn mức này chắc chắn là tải lỗi.</summary>
        private const long KICH_THUOC_TOI_THIEU = 40L * 1024 * 1024;

        /// <summary>Người ký hợp lệ của file cài đặt.</summary>
        private const string NHA_PHAT_HANH = "O=Microsoft Corporation";

        public const int EXIT_THANH_CONG = 0;

        /// <summary>Cài xong nhưng Windows cần khởi động lại. Vẫn coi là thành công.</summary>
        public const int EXIT_CAN_KHOI_DONG_LAI_WINDOWS = 3010;

        /// <summary>Người dùng bấm "No" ở hộp thoại UAC.</summary>
        public const int EXIT_NGUOI_DUNG_TU_CHOI_UAC = 1223;

        /// <summary>
        /// Dùng để thử nghiệm: ép IsAvailable() trả về false trên máy đã cài ACE,
        /// nhờ đó chạy thử được toàn bộ luồng hội thoại. Bật bằng tham số dòng lệnh
        /// --gia-lap-thieu-ace
        /// </summary>
        public static bool GiaLapThieu { get; set; }

        /// <summary>Link tải, đọc từ app.config, không có thì dùng link mặc định của Microsoft.</summary>
        public static string DownloadUrl
        {
            get
            {
                try
                {
                    string url = ConfigurationManager.AppSettings[KHOA_CAUHINH_URL];
                    if (!string.IsNullOrEmpty(url) && url.Trim().Length > 0)
                    {
                        return url.Trim();
                    }
                }
                catch (Exception ex)
                {
                    Memory.Instance.Error = ex;
                }
                return URL_MAC_DINH;
            }
        }

        /// <summary>Đường dẫn file cài đặt trong thư mục tạm.</summary>
        public static string DuongDanFileTam
        {
            get { return Path.Combine(Path.GetTempPath(), TEN_FILE_CAI); }
        }

        /// <summary>
        /// Máy đã có ACE OLEDB provider dùng được cho tiến trình 32-bit này chưa.
        /// Liệt kê provider ngay trong tiến trình hiện tại nên phản ánh đúng "bitness".
        /// </summary>
        public static bool IsAvailable()
        {
            if (GiaLapThieu)
            {
                return false;
            }
            try
            {
                OleDbEnumerator enumerator = new OleDbEnumerator();
                DataTable tbl = enumerator.GetElements();
                if (tbl == null)
                {
                    return false;
                }
                foreach (DataRow row in tbl.Rows)
                {
                    string ten = Convert.ToString(row["SOURCES_NAME"]);
                    if (!string.IsNullOrEmpty(ten) &&
                        ten.StartsWith("Microsoft.ACE.OLEDB", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
            return false;
        }

        /// <summary>Thử kết nối tới máy chủ tải về để biết máy có vào được Internet không.</summary>
        public static bool HasInternet()
        {
            try
            {
                // Một số máy cũ mặc định chỉ bật SSL3/TLS1.0 nên không tải được từ Microsoft.
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(DownloadUrl);
                request.Method = "HEAD";
                request.Timeout = 10000;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra file tải về có đúng là bản gốc do Microsoft ký hay không.
        /// Bắt buộc phải gọi trước khi chạy file, vì file được chạy với quyền Administrator.
        /// Kiểm tra hai lớp: chữ ký số còn nguyên vẹn (WinVerifyTrust) và người ký là Microsoft.
        /// </summary>
        public static bool VerifySignature(string duongDan, out string lyDo)
        {
            lyDo = "";
            try
            {
                if (!File.Exists(duongDan))
                {
                    lyDo = "Không tìm thấy file vừa tải về.";
                    return false;
                }

                FileInfo info = new FileInfo(duongDan);
                if (info.Length < KICH_THUOC_TOI_THIEU)
                {
                    lyDo = string.Format(
                        "File tải về chỉ có {0:N1} MB, không đủ dung lượng của bản cài đặt thật. Có thể mạng bị gián đoạn.",
                        info.Length / 1024.0 / 1024.0);
                    return false;
                }

                if (!ChuKySoHopLe(duongDan))
                {
                    lyDo = "Chữ ký số của file không hợp lệ. File có thể đã bị hỏng hoặc bị thay đổi.";
                    return false;
                }

                X509Certificate2 chungThu = new X509Certificate2(X509Certificate.CreateFromSignedFile(duongDan));
                if (chungThu.Subject.IndexOf(NHA_PHAT_HANH, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    lyDo = "File không phải do Microsoft phát hành. Người ký: " + chungThu.Subject;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                lyDo = "Không kiểm tra được chữ ký số: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Chạy bộ cài ở chế độ im lặng với quyền Administrator.
        /// Verb "runas" chỉ nâng quyền cho tiến trình cài đặt, chương trình chính không cần
        /// khởi động lại bằng quyền Administrator.
        /// Tham số /quiet còn có tác dụng bỏ qua kiểm tra xung đột với Office 64-bit.
        /// </summary>
        /// <returns>Mã thoát của bộ cài. 0 hoặc 3010 là thành công.</returns>
        public static int Install(string duongDan, out string lyDo)
        {
            lyDo = "";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = duongDan;
                psi.Arguments = "/quiet";
                psi.UseShellExecute = true;
                psi.Verb = "runas";
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    int ma = process.ExitCode;
                    if (ma != EXIT_THANH_CONG && ma != EXIT_CAN_KHOI_DONG_LAI_WINDOWS)
                    {
                        lyDo = "Bộ cài kết thúc với mã lỗi " + ma + ".";
                    }
                    return ma;
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Memory.Instance.Error = ex;
                if (ex.NativeErrorCode == EXIT_NGUOI_DUNG_TU_CHOI_UAC)
                {
                    lyDo = "Bạn đã từ chối cấp quyền Administrator nên không thể cài đặt.";
                    return EXIT_NGUOI_DUNG_TU_CHOI_UAC;
                }
                lyDo = "Không chạy được bộ cài: " + ex.Message;
                return ex.NativeErrorCode;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                lyDo = "Không chạy được bộ cài: " + ex.Message;
                return -1;
            }
        }

        /// <summary>Xoá file cài đặt trong thư mục tạm. Lỗi khi xoá không ảnh hưởng gì.</summary>
        public static void XoaFileTam()
        {
            try
            {
                if (File.Exists(DuongDanFileTam))
                {
                    File.Delete(DuongDanFileTam);
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
        }

        #region Kiểm tra chữ ký số bằng WinVerifyTrust

        private static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 =
            new Guid("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");

        private const uint WTD_UI_NONE = 2;
        private const uint WTD_REVOKE_NONE = 0;
        private const uint WTD_CHOICE_FILE = 1;
        private const uint WTD_STATEACTION_VERIFY = 1;
        private const uint WTD_STATEACTION_CLOSE = 2;
        private const uint WTD_SAFER_FLAG = 0x100;

        [StructLayout(LayoutKind.Sequential)]
        private struct WINTRUST_FILE_INFO
        {
            public uint cbStruct;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pcwszFilePath;
            public IntPtr hFile;
            public IntPtr pgKnownSubject;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINTRUST_DATA
        {
            public uint cbStruct;
            public IntPtr pPolicyCallbackData;
            public IntPtr pSIPClientData;
            public uint dwUIChoice;
            public uint fdwRevocationChecks;
            public uint dwUnionChoice;
            public IntPtr pFile;
            public uint dwStateAction;
            public IntPtr hWVTStateData;
            public IntPtr pwszURLReference;
            public uint dwProvFlags;
            public uint dwUIContext;
        }

        [DllImport("wintrust.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern uint WinVerifyTrust(
            IntPtr hwnd,
            [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
            IntPtr pWVTData);

        /// <summary>
        /// Gọi Windows xác minh chữ ký Authenticode: chữ ký còn nguyên vẹn (nội dung file
        /// không bị sửa sau khi ký) và chuỗi chứng thư đáng tin cậy.
        /// </summary>
        private static bool ChuKySoHopLe(string duongDan)
        {
            WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO();
            fileInfo.cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO));
            fileInfo.pcwszFilePath = duongDan;
            fileInfo.hFile = IntPtr.Zero;
            fileInfo.pgKnownSubject = IntPtr.Zero;

            IntPtr pFile = IntPtr.Zero;
            IntPtr pData = IntPtr.Zero;
            try
            {
                pFile = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));
                Marshal.StructureToPtr(fileInfo, pFile, false);

                WINTRUST_DATA data = new WINTRUST_DATA();
                data.cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA));
                data.dwUIChoice = WTD_UI_NONE;
                data.fdwRevocationChecks = WTD_REVOKE_NONE;
                data.dwUnionChoice = WTD_CHOICE_FILE;
                data.pFile = pFile;
                data.dwStateAction = WTD_STATEACTION_VERIFY;
                data.dwProvFlags = WTD_SAFER_FLAG;

                pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA)));
                Marshal.StructureToPtr(data, pData, false);

                uint ketQua = WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, pData);

                // Luôn gọi lại với STATEACTION_CLOSE để Windows giải phóng tài nguyên.
                data = (WINTRUST_DATA)Marshal.PtrToStructure(pData, typeof(WINTRUST_DATA));
                data.dwStateAction = WTD_STATEACTION_CLOSE;
                Marshal.StructureToPtr(data, pData, false);
                WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, pData);

                return ketQua == 0;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                return false;
            }
            finally
            {
                if (pFile != IntPtr.Zero) Marshal.FreeHGlobal(pFile);
                if (pData != IntPtr.Zero) Marshal.FreeHGlobal(pData);
            }
        }

        #endregion
    }
}
