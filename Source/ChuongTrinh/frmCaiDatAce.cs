using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using GxGlobal;

namespace GiaoXu
{
    /// <summary>
    /// Màn hình tải Microsoft Access Database Engine bản 32-bit, kèm toàn bộ luồng hội thoại
    /// phát hiện thiếu - hỏi ý kiến - tải - kiểm tra chữ ký - cài đặt.
    ///
    /// Giao diện được tạo hoàn toàn bằng mã nguồn nên không cần file Designer và file .resx.
    /// </summary>
    public class frmCaiDatAce : Form
    {
        private Label lblTrangThai;
        private ProgressBar progressBar;
        private Button btnHuy;
        private WebClient webClient;
        private bool daHuy = false;

        /// <summary>Lỗi xảy ra trong lúc tải, rỗng nghĩa là tải thành công.</summary>
        public string LoiTaiVe { get; private set; }

        public frmCaiDatAce()
        {
            LoiTaiVe = "";
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            this.Text = "Đang tải thành phần bắt buộc";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(460, 130);
            this.Font = new Font("Microsoft Sans Serif", 9F);

            lblTrangThai = new Label();
            lblTrangThai.AutoSize = false;
            lblTrangThai.Location = new Point(15, 15);
            lblTrangThai.Size = new Size(430, 40);
            lblTrangThai.Text = "Đang chuẩn bị tải...";

            progressBar = new ProgressBar();
            progressBar.Location = new Point(15, 60);
            progressBar.Size = new Size(430, 22);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;

            btnHuy = new Button();
            btnHuy.Text = "Hủy";
            btnHuy.Size = new Size(90, 26);
            btnHuy.Location = new Point(355, 92);
            btnHuy.Click += btnHuy_Click;

            this.Controls.Add(lblTrangThai);
            this.Controls.Add(progressBar);
            this.Controls.Add(btnHuy);
            this.CancelButton = btnHuy;

            this.Shown += frmCaiDatAce_Shown;
            this.FormClosing += frmCaiDatAce_FormClosing;
        }

        private void frmCaiDatAce_Shown(object sender, EventArgs e)
        {
            BatDauTai();
        }

        private void BatDauTai()
        {
            try
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                webClient = new WebClient();
                webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(AceProvider.DownloadUrl), AceProvider.DuongDanFileTam);
            }
            catch (Exception ex)
            {
                Memory.Instance.Error = ex;
                LoiTaiVe = ex.Message;
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int phanTram = e.ProgressPercentage;
            if (phanTram < 0) phanTram = 0;
            if (phanTram > 100) phanTram = 100;
            progressBar.Value = phanTram;
            lblTrangThai.Text = string.Format(
                "Đang tải {0} ...\r\nĐã tải {1:N1} MB / {2:N1} MB  ({3}%)",
                AceProvider.TEN_HIEN_THI,
                e.BytesReceived / 1024.0 / 1024.0,
                e.TotalBytesToReceive / 1024.0 / 1024.0,
                phanTram);
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || daHuy)
            {
                LoiTaiVe = "Bạn đã hủy việc tải về.";
                this.DialogResult = DialogResult.Cancel;
            }
            else if (e.Error != null)
            {
                Memory.Instance.Error = e.Error;
                LoiTaiVe = e.Error.Message;
                this.DialogResult = DialogResult.Abort;
            }
            else
            {
                LoiTaiVe = "";
                this.DialogResult = DialogResult.OK;
            }
            this.Close();
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            daHuy = true;
            if (webClient != null)
            {
                webClient.CancelAsync();
            }
        }

        private void frmCaiDatAce_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (webClient != null)
            {
                webClient.DownloadProgressChanged -= webClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted -= webClient_DownloadFileCompleted;
                webClient.Dispose();
                webClient = null;
            }
        }

        #region Luồng xử lý chung

        /// <summary>
        /// Bảo đảm máy đã có ACE 32-bit. Nếu chưa thì báo cho người dùng biết lý do và
        /// đề nghị tải, cài tự động.
        /// </summary>
        /// <returns>true nếu dùng được ngay; false nếu màn hình gọi nó phải đóng lại.</returns>
        public static bool DamBaoDaCai(IWin32Window owner)
        {
            if (AceProvider.IsAvailable())
            {
                return true;
            }

            const string TIEU_DE = "Thiếu thành phần bắt buộc";
            string moTaLoi =
                "Máy tính chưa cài " + AceProvider.TEN_HIEN_THI + ".\r\n\r\n" +
                "Thiếu thành phần này, chương trình không đọc được file Access (.accdb) khi nhập dữ liệu " +
                "từ phần mềm MGC, và không đọc được file Excel (.xlsx) khi nhập dữ liệu từ Excel.\r\n\r\n" +
                "Nguyên nhân thường gặp: máy đã cài Microsoft Office bản 64-bit nên chỉ có " +
                "Access Database Engine bản 64-bit, trong khi chương trình này chạy ở chế độ 32-bit.";

            if (!AceProvider.HasInternet())
            {
                MessageBox.Show(owner,
                    moTaLoi + "\r\n\r\n" +
                    "Hiện máy không kết nối được Internet nên chương trình không thể tự tải về.\r\n\r\n" +
                    HuongDanCaiThuCong(),
                    TIEU_DE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            DialogResult traLoi = MessageBox.Show(owner,
                moTaLoi + "\r\n\r\n" +
                "Bạn có muốn chương trình tự động tải về và cài đặt ngay bây giờ không?\r\n" +
                "Dung lượng cần tải khoảng 78 MB.",
                TIEU_DE, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (traLoi != DialogResult.Yes)
            {
                return false;
            }

            AceProvider.XoaFileTam();

            frmCaiDatAce frm = new frmCaiDatAce();
            DialogResult ketQuaTai = frm.ShowDialog(owner);
            if (ketQuaTai != DialogResult.OK)
            {
                AceProvider.XoaFileTam();
                if (ketQuaTai == DialogResult.Abort)
                {
                    MessageBox.Show(owner,
                        "Tải về không thành công: " + frm.LoiTaiVe + "\r\n\r\n" + HuongDanCaiThuCong(),
                        TIEU_DE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }

            string lyDo;
            if (!AceProvider.VerifySignature(AceProvider.DuongDanFileTam, out lyDo))
            {
                AceProvider.XoaFileTam();
                MessageBox.Show(owner,
                    "File tải về không vượt qua bước kiểm tra an toàn nên chương trình đã xóa file đi.\r\n\r\n" +
                    "Lý do: " + lyDo + "\r\n\r\n" +
                    "Bạn hãy thử lại, hoặc " + HuongDanCaiThuCong(),
                    TIEU_DE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            MessageBox.Show(owner,
                "Đã tải xong và kiểm tra an toàn đạt yêu cầu.\r\n\r\n" +
                "Chương trình sẽ tiến hành cài đặt. Windows sẽ hiện hộp thoại xin quyền Administrator, " +
                "bạn hãy bấm \"Yes\" để cho phép cài đặt.\r\n\r\n" +
                "Quá trình cài đặt chạy ngầm, có thể mất khoảng một phút.",
                TIEU_DE, MessageBoxButtons.OK, MessageBoxIcon.Information);

            Cursor cuTro = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            int maThoat;
            try
            {
                maThoat = AceProvider.Install(AceProvider.DuongDanFileTam, out lyDo);
            }
            finally
            {
                Cursor.Current = cuTro;
            }

            if (maThoat != AceProvider.EXIT_THANH_CONG &&
                maThoat != AceProvider.EXIT_CAN_KHOI_DONG_LAI_WINDOWS)
            {
                MessageBox.Show(owner,
                    "Cài đặt không thành công.\r\n\r\n" +
                    "Lý do: " + lyDo + "\r\n\r\n" + HuongDanCaiThuCong(),
                    TIEU_DE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            AceProvider.XoaFileTam();

            DialogResult traLoiKhoiDong = MessageBox.Show(owner,
                "Đã cài đặt xong " + AceProvider.TEN_HIEN_THI + ".\r\n\r\n" +
                "Cần khởi động lại chương trình thì thành phần vừa cài mới có hiệu lực.\r\n" +
                "Bạn có muốn khởi động lại chương trình ngay bây giờ không?",
                "Cài đặt thành công", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (traLoiKhoiDong == DialogResult.Yes)
            {
                Application.Restart();
            }

            // Dù người dùng chọn gì, màn hình đang mở vẫn phải đóng vì tiến trình hiện tại
            // chưa nạp được provider vừa cài.
            return false;
        }

        private static string HuongDanCaiThuCong()
        {
            return
                "Bạn có thể cài thủ công theo các bước sau:\r\n" +
                "1. Tải file tại địa chỉ:\r\n   " + AceProvider.DownloadUrl + "\r\n" +
                "2. Mở Command Prompt bằng quyền Administrator.\r\n" +
                "3. Chạy lệnh:  \"<đường dẫn file vừa tải>\" /quiet\r\n" +
                "   (bắt buộc phải có /quiet nếu máy đang cài Office 64-bit)\r\n" +
                "4. Mở lại chương trình.";
        }

        #endregion
    }
}
