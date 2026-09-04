using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace GxGlobal
{
    /// <summary>
    /// Phát hiện và sửa lỗi không xuất được file WORD/EXCEL do registry TypeLib của Office
    /// còn sót lại phiên bản cũ, gây lỗi:
    ///   "Unable to cast COM object of type 'Microsoft.Office.Interop.Excel.ApplicationClass'
    ///    to interface type 'Microsoft.Office.Interop.Excel._Application'"
    ///
    /// Nguyên nhân: gỡ Office cũ rồi cài Office mới mà không khởi động lại máy, làm khoá
    /// TypeLib còn lẫn nhiều phiên bản. Cách sửa là xoá phiên bản THỪA, giữ lại phiên bản
    /// đúng với Office đang cài.
    ///
    /// Điểm khác biệt quan trọng so với bản cũ: chỉ xoá phiên bản thừa, tuyệt đối không
    /// xoá phiên bản mà Office hiện tại đang dùng. Bản cũ xoá thẳng 1.7 và 1.8 nên trên
    /// máy dùng Office 2010 (1.7) hoặc Office 2013 (1.8) sẽ xoá mất phiên bản đang cần.
    /// </summary>
    public static class ExcelInteropFix
    {
        /// <summary>TypeLib của Microsoft Excel Object Library.</summary>
        public const string TYPELIB_EXCEL = "{00020813-0000-0000-C000-000000000046}";

        /// <summary>TypeLib của Microsoft Word Object Library.</summary>
        public const string TYPELIB_WORD = "{00020905-0000-0000-C000-000000000046}";

        private const string DUONG_DAN_TYPELIB = "TypeLib\\";

        /// <summary>
        /// Do project ChuongTrinh gán lúc khởi động. Khi phát hiện lỗi cast COM,
        /// Memory.ShowError sẽ gọi hàm này để hỏi người dùng có muốn sửa ngay không.
        /// Đặt ở đây để GXGlobal không phải tham chiếu ngược lên project giao diện.
        /// </summary>
        public static Func<bool> YeuCauSuaLoi;

        /// <summary>
        /// Ánh xạ phiên bản Office sang phiên bản TypeLib của EXCEL.
        /// Ví dụ Excel.Application.16 (Office 2016 trở lên) dùng TypeLib 1.9.
        /// </summary>
        private static readonly Dictionary<string, string> BANG_EXCEL =
            new Dictionary<string, string>
            {
                { "11", "1.5" },  // Office 2003
                { "12", "1.6" },  // Office 2007
                { "14", "1.7" },  // Office 2010
                { "15", "1.8" },  // Office 2013
                { "16", "1.9" }   // Office 2016 / 2019 / 2021 / 2024 / 365
            };

        /// <summary>
        /// Ánh xạ phiên bản Office sang phiên bản TypeLib của WORD.
        /// Word đánh số TypeLib theo hệ 8.x, hoàn toàn khác Excel (hệ 1.x):
        /// Word.Application.16 dùng TypeLib 8.7 chứ không phải 1.9.
        /// </summary>
        private static readonly Dictionary<string, string> BANG_WORD =
            new Dictionary<string, string>
            {
                { "11", "8.3" },  // Office 2003
                { "12", "8.4" },  // Office 2007
                { "14", "8.5" },  // Office 2010
                { "15", "8.6" },  // Office 2013
                { "16", "8.7" }   // Office 2016 / 2019 / 2021 / 2024 / 365
            };

        #region Phát hiện lỗi

        /// <summary>
        /// Lỗi truyền vào có đúng là lỗi cast COM của Office hay không.
        /// Kiểm tra chặt để tránh nhận nhầm các lỗi khác.
        /// </summary>
        public static bool LaLoiCastComOffice(Exception ex)
        {
            while (ex != null)
            {
                if (ex is InvalidCastException || ex is COMException)
                {
                    string thongDiep = ex.Message ?? "";
                    bool nhacToiOffice =
                        thongDiep.IndexOf("Office.Interop", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool laLoiKhongCoInterface =
                        thongDiep.IndexOf("80004002", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        thongDiep.IndexOf("E_NOINTERFACE", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (nhacToiOffice && (ex is InvalidCastException || laLoiKhongCoInterface))
                    {
                        return true;
                    }
                }
                ex = ex.InnerException;
            }
            return false;
        }

        #endregion

        #region Đọc thông tin registry

        /// <summary>Các phiên bản TypeLib hiện có trên máy, ví dụ 1.7, 1.8, 1.9.</summary>
        public static List<string> LayCacPhienBan(string typeLibGuid)
        {
            List<string> ketQua = new List<string>();
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(DUONG_DAN_TYPELIB + typeLibGuid, false))
                {
                    if (key != null)
                    {
                        ketQua.AddRange(key.GetSubKeyNames());
                    }
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
            return ketQua;
        }

        /// <summary>
        /// Phiên bản TypeLib ứng với Office đang cài, đọc từ ProgID Excel.Application\CurVer.
        /// Trả về chuỗi rỗng nếu không xác định được — khi đó KHÔNG được xoá gì cả.
        /// </summary>
        public static string LayPhienBanDangDung(string progId, Dictionary<string, string> bangPhienBan)
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(progId + "\\CurVer", false))
                {
                    if (key == null)
                    {
                        return "";
                    }
                    string curVer = Convert.ToString(key.GetValue(""));   // vd: Excel.Application.16
                    if (string.IsNullOrEmpty(curVer))
                    {
                        return "";
                    }
                    int viTri = curVer.LastIndexOf('.');
                    if (viTri < 0 || viTri >= curVer.Length - 1)
                    {
                        return "";
                    }
                    string soHieu = curVer.Substring(viTri + 1);
                    if (bangPhienBan.ContainsKey(soHieu))
                    {
                        return bangPhienBan[soHieu];
                    }
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
            return "";
        }

        #endregion

        #region Sao lưu và sửa

        /// <summary>
        /// Xuất khoá TypeLib ra file .reg để có thể khôi phục nếu cần.
        /// Trả về đường dẫn file sao lưu, hoặc chuỗi rỗng nếu không sao lưu được.
        /// </summary>
        public static string SaoLuu(string typeLibGuid)
        {
            try
            {
                string tenFile = string.Format("QLGX_TypeLib_{0}.reg", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string duongDan = Path.Combine(Path.GetTempPath(), tenFile);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "reg.exe";
                psi.Arguments = string.Format("export \"HKCR\\TypeLib\\{0}\" \"{1}\" /y", typeLibGuid, duongDan);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0 && File.Exists(duongDan))
                    {
                        return duongDan;
                    }
                }
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
            }
            return "";
        }

        /// <summary>
        /// Xoá các phiên bản TypeLib thừa, giữ lại phiên bản đúng của Office đang cài.
        /// Cần chạy với quyền Administrator vì ghi vào HKEY_CLASSES_ROOT.
        /// </summary>
        /// <param name="typeLibGuid">GUID của TypeLib.</param>
        /// <param name="progId">ProgID để tra phiên bản đang dùng, vd Excel.Application.</param>
        /// <param name="moTa">Diễn giải kết quả để hiển thị cho người dùng.</param>
        /// <returns>Số phiên bản đã xoá. Trả về -1 nếu không thực hiện được.</returns>
        public static int Sua(string typeLibGuid, string progId, Dictionary<string, string> bangPhienBan, out string moTa)
        {
            moTa = "";
            try
            {
                List<string> cacPhienBan = LayCacPhienBan(typeLibGuid);
                if (cacPhienBan.Count == 0)
                {
                    moTa = "Không tìm thấy khoá TypeLib của Office trong registry.";
                    return -1;
                }

                string phienBanDung = LayPhienBanDangDung(progId, bangPhienBan);
                if (string.IsNullOrEmpty(phienBanDung))
                {
                    moTa = "Không xác định được phiên bản Office đang cài trên máy, "
                         + "nên chương trình không dám xoá khoá nào để tránh làm hỏng Office.\r\n"
                         + "Các phiên bản TypeLib hiện có: " + string.Join(", ", cacPhienBan.ToArray());
                    return -1;
                }

                if (!cacPhienBan.Contains(phienBanDung))
                {
                    moTa = string.Format(
                        "Office đang cài cần TypeLib phiên bản {0} nhưng registry lại không có phiên bản này "
                        + "(hiện chỉ có: {1}).\r\nĐây không phải trường hợp thừa khoá cũ, nên xoá đi cũng không giải quyết được. "
                        + "Bạn nên dùng chức năng Repair của Microsoft Office.",
                        phienBanDung, string.Join(", ", cacPhienBan.ToArray()));
                    return -1;
                }

                List<string> canXoa = new List<string>();
                foreach (string phienBan in cacPhienBan)
                {
                    if (!string.Equals(phienBan, phienBanDung, StringComparison.OrdinalIgnoreCase)
                        && bangPhienBan.ContainsValue(phienBan))
                    {
                        canXoa.Add(phienBan);
                    }
                }

                if (canXoa.Count == 0)
                {
                    moTa = string.Format(
                        "Registry đang bình thường: chỉ có phiên bản {0} đúng với Office trên máy, "
                        + "không có phiên bản thừa nào cần xoá.\r\n"
                        + "Vậy nguyên nhân không xuất được file nằm ở chỗ khác.",
                        phienBanDung);
                    return 0;
                }

                string fileSaoLuu = SaoLuu(typeLibGuid);

                int daXoa = 0;
                List<string> loi = new List<string>();
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(DUONG_DAN_TYPELIB + typeLibGuid, true))
                {
                    if (key == null)
                    {
                        moTa = "Không mở được khoá registry để ghi. Bạn cần chạy chương trình bằng quyền Administrator.";
                        return -1;
                    }
                    foreach (string phienBan in canXoa)
                    {
                        try
                        {
                            key.DeleteSubKeyTree(phienBan, false);
                            daXoa++;
                        }
                        catch (Exception ex)
                        {
                            Memory.Instance.Error = ex;
                            loi.Add(phienBan + " (" + ex.Message + ")");
                        }
                    }
                }

                moTa = string.Format("Đã xoá {0} phiên bản TypeLib thừa: {1}.\r\nGiữ lại phiên bản {2} đúng với Office đang cài.",
                    daXoa, string.Join(", ", canXoa.ToArray()), phienBanDung);
                if (loi.Count > 0)
                {
                    moTa += "\r\nKhông xoá được: " + string.Join("; ", loi.ToArray());
                }
                if (!string.IsNullOrEmpty(fileSaoLuu))
                {
                    moTa += "\r\n\r\nĐã sao lưu registry trước khi sửa vào file:\r\n" + fileSaoLuu;
                }
                return daXoa;
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                moTa = "Lỗi khi sửa registry: " + ex.Message;
                return -1;
            }
        }

        /// <summary>Sửa cho cả Excel và Word. Trả về tổng số khoá đã xoá.</summary>
        public static int SuaTatCa(out string moTa)
        {
            string moTaExcel, moTaWord;
            int soExcel = Sua(TYPELIB_EXCEL, "Excel.Application", BANG_EXCEL, out moTaExcel);
            int soWord = Sua(TYPELIB_WORD, "Word.Application", BANG_WORD, out moTaWord);

            moTa = "EXCEL: " + moTaExcel + "\r\n\r\nWORD: " + moTaWord;

            int tong = 0;
            if (soExcel > 0) tong += soExcel;
            if (soWord > 0) tong += soWord;
            return tong;
        }

        #endregion
    }
}
