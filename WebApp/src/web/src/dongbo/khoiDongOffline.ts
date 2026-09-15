/**
 * Task 10 — "nối dây" tầng offline (Task 1-9) vào app shell thật.
 *
 * PHÁT HIỆN của review Task 6/7/8 (ghi trong ledger, nhắc lại trong task-10-brief.md): trước Task
 * 10, KHÔNG một nơi nào trong ứng dụng thật gọi `moKho()`/`batDauBoDongBo()` — toàn bộ tầng offline
 * là MÃ CHẾT. Module này là điểm nối DUY NHẤT: mở kho IndexedDB (Task 1) rồi bắt đầu vòng đồng bộ
 * (Task 6), và nối `LoiEpochKhongKhop` (Task 6) với bước bù lại sau khôi phục (Task 8,
 * `buSauKhoiPhuc.ts`) — xem tham số `khiEpochKhongKhop` mới thêm vào `batDauBoDongBo`
 * (`boDongBo.ts`).
 *
 * GỌI ĐÚNG MỘT LẦN cho suốt vòng đời một tab (I6, cảnh báo đã có sẵn ở `khoiTaoPhuThuoc`/
 * `batDauBoDongBo` — hai lần gọi tạo ra HAI `PhuThuocBoDongBo` độc lập cùng ghi đè `conTro`/đồng hồ
 * logic lên nhau). `khoiDongOffline()` tự chống gọi lại bằng một Promise cache module-level (cùng
 * khuôn mẫu `dangChayEpochKhongKhop` của `buSauKhoiPhuc.ts`) — gọi nhiều lần (ví dụ React StrictMode
 * gọi effect hai lần ở môi trường phát triển) đều trả về ĐÚNG MỘT kết quả, không mở kho/bắt đầu vòng
 * đồng bộ lần thứ hai.
 *
 * KHÔNG import module này từ bất kỳ nơi nào ngoài đúng MỘT chỗ gọi `khoiDongOffline()` (xem
 * `App.tsx`) — nơi gọi thứ hai (dù vô tình) sẽ TRÁNH được lỗi kép nhờ cache ở trên, nhưng vẫn nên
 * giữ kỷ luật một điểm gọi duy nhất để dễ suy luận.
 */
import { moKho } from '../kho/moKho'
import { batDauBoDongBo, type DieuKhienBoDongBo } from './boDongBo'
import { xuLyEpochKhongKhopDonLuong } from './buSauKhoiPhuc'

export type TrangThaiOffline = {
  kho: IDBDatabase
  dieuKhien: DieuKhienBoDongBo
}

let dangKhoiDong: Promise<TrangThaiOffline | null> | null = null
let trangThaiHienTai: TrangThaiOffline | null = null

/**
 * Khởi động tầng offline: mở kho rồi bắt đầu vòng đồng bộ, nối sẵn `khiEpochKhongKhop` với
 * `xuLyEpochKhongKhopDonLuong` (Task 8) — đúng dòng brief mục 3 yêu cầu:
 * `khiEpochKhongKhop: (phuThuoc) => xuLyEpochKhongKhopDonLuong(phuThuoc, dieuKhien.trangThai)`.
 *
 * Trả về `null` (KHÔNG ném lỗi ra ngoài) nếu mở kho thất bại (ví dụ `LoiKhoMoiHonMa` — một tab khác
 * đang chạy phiên bản mới hơn) — tầng offline là một LỚP TĂNG CƯỜNG (đồng bộ nền + hiển thị trạng
 * thái), KHÔNG được phép làm sập hẳn ứng dụng chính nếu nó không khởi động được; màn hình vẫn phải
 * dùng được qua mạng bình thường. Lỗi vẫn được ghi `console.error` đầy đủ để gỡ lỗi.
 *
 * `ThanhTrangThai`/các màn hình đọc trạng thái qua `layTrangThaiOffline()` (đồng bộ, không async) —
 * `null` cho tới khi lượt gọi ĐẦU TIÊN này hoàn tất (hoặc mãi mãi `null` nếu thất bại).
 */
export function khoiDongOffline(): Promise<TrangThaiOffline | null> {
  if (dangKhoiDong) return dangKhoiDong

  dangKhoiDong = (async () => {
    try {
      const kho = await moKho()
      // I6/`khoiTaoPhuThuoc`: `dieuKhien` được tham chiếu bên trong chính closure truyền cho
      // `batDauBoDongBo` — hợp lệ vì closure chỉ THỰC SỰ đọc `dieuKhien.trangThai` khi callback được
      // GỌI (lúc gặp `LoiEpochKhongKhop`), tại thời điểm đó `const dieuKhien` bên dưới chắc chắn đã
      // gán xong (JS đóng biến theo binding, không theo giá trị tại thời điểm tạo closure).
      const dieuKhien = batDauBoDongBo(kho, undefined, undefined, undefined, async (phuThuoc) => {
        // `xuLyEpochKhongKhopDonLuong` trả `{ soDongDaBu }` (Task 8) — `batDauBoDongBo` chỉ cần
        // `Promise<void>` (chỉ quan tâm "đã xong hay chưa"/"có ném lỗi hay không"), bỏ qua giá trị
        // trả về ở đây là cố ý, không phải quên đọc kết quả.
        await xuLyEpochKhongKhopDonLuong(phuThuoc, dieuKhien.trangThai)
      })
      trangThaiHienTai = { kho, dieuKhien }
      return trangThaiHienTai
    } catch (loi) {
      console.error(
        'Khong khoi dong duoc tang offline (mo kho that bai) — ung dung van chay binh thuong qua mang, ' +
          'chi khong co ban sao ngoai tuyen/dong bo nen', loi,
      )
      return null
    }
  })()

  return dangKhoiDong
}

/** Đọc trạng thái offline hiện có — `null` nếu chưa khởi động xong hoặc khởi động thất bại. Dùng
 * cho các component UI (Task 10: `ThanhTrangThai`, `CanXemLaiPage`, `BanGiaoMayPage`) cần `kho` để
 * gọi `demHangCho(kho)`, hoặc `dieuKhien.trangThai()` để biết có đang `dung_do_may_chu_di_lui`. */
export function layTrangThaiOffline(): TrangThaiOffline | null {
  return trangThaiHienTai
}

/**
 * I4 (fix round 1): dừng tầng offline khi đăng xuất/đổi tài khoản trên cùng tab — không có bước
 * này, vòng đồng bộ (`dieuKhien`) và kho IndexedDB đã mở vẫn tiếp tục hoạt động với dữ liệu của
 * tài khoản/giáo xứ CŨ trong khi token đã đổi sang tài khoản mới, rủi ro trộn dữ liệu hai giáo xứ.
 *
 * QUAN TRỌNG — spec mục 5.4: chỉ ĐÓNG kho (`kho.close()`), TUYỆT ĐỐI KHÔNG xoá
 * (`indexedDB.deleteDatabase`) — hàng chờ có thể chưa rỗng và dữ liệu chưa gửi lên máy chủ. Đóng
 * kho không xoá dữ liệu đã lưu; lần đăng nhập sau (`moKho()`) sẽ mở lại đúng kho đó.
 *
 * Sau khi gọi, cache module-level (`dangKhoiDong`/`trangThaiHienTai`) bị xoá — lượt gọi
 * `khoiDongOffline()` tiếp theo (ví dụ sau khi đăng nhập lại) sẽ mở một phiên MỚI, không dùng lại
 * Promise cache cũ.
 */
export function dungOffline(): void {
  if (trangThaiHienTai) {
    trangThaiHienTai.dieuKhien.dung()
    trangThaiHienTai.kho.close()
  }
  trangThaiHienTai = null
  dangKhoiDong = null
}

/** CHỈ dùng cho kiểm thử — reset cache module-level giữa các ca kiểm thử (mỗi ca cần gọi lại
 * `khoiDongOffline()` từ đầu, không dính kết quả cache của ca trước). KHÔNG gọi trong mã sản phẩm. */
export function _resetChoKiemThu(): void {
  dangKhoiDong = null
  trangThaiHienTai = null
}
