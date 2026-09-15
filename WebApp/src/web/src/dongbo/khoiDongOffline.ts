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
import { docHangCho } from '../kho/hangCho'
import { batDauBoDongBo, type DieuKhienBoDongBo } from './boDongBo'
import { xuLyEpochKhongKhopDonLuong } from './buSauKhoiPhuc'
import { canTuDongDuPhong, taiFileDuPhongXuong } from './tepDuPhong'

export type TrangThaiOffline = {
  kho: IDBDatabase
  dieuKhien: DieuKhienBoDongBo
}

let dangKhoiDong: Promise<TrangThaiOffline | null> | null = null
let trangThaiHienTai: TrangThaiOffline | null = null

// F4 (fix round 2): chống race "dungOffline() gọi giữa lúc khoiDongOffline() còn dở" (vd. đăng xuất
// đúng lúc đang await moKho()) — nếu không có cờ này, dungOffline() chạy khi idInterKiemTraDuPhong
// còn null (chưa có gì để clearInterval), rồi thân hàm async của khoiDongOffline() tiếp tục chạy
// xong và GÁN ĐÈ trangThaiHienTai/đặt interval mới, làm tầng offline "sống lại" sau khi tưởng đã
// dừng. Cờ module-level đơn giản là đủ cho ca hẹp này: `khoiDongOffline()` đặt cờ về `false` mỗi lần
// BẮT ĐẦU một lượt khởi động mới (không phải lượt trả về cache); `dungOffline()` luôn bật cờ lên
// `true` mỗi lần được gọi. `khoiDongOffline()` đọc lại cờ này ngay TRƯỚC khi gán
// `trangThaiHienTai`/đặt interval ở cuối — nếu thấy cờ đã bật (nghĩa là `dungOffline()` xen vào giữa
// lúc đang khởi động), tự dọn luôn (đóng kho, dừng `dieuKhien`) thay vì gán làm trạng thái hiện tại.
let dangDungGiuaChung = false

// L3 (fix round Task 11, spec 4.8.6): trạng thái "đang bù lại" sau khi phát hiện epoch không khớp
// (máy chủ vừa được khôi phục) — `ThanhTrangThai` đọc biến này (qua `layTrangThaiBuLai()`) để hiện
// câu "Máy chủ vừa được khôi phục..." trong lúc bù, và dòng tổng kết "Đã gửi lại N thay đổi." sau
// khi bù xong. KHÔNG cần tự reset về trạng thái ban đầu sau một khoảng thời gian — giữ nguyên tới
// lần bù kế tiếp/tải lại trang là đủ, `ThanhTrangThai` tự quyết định hiện dòng tổng kết trong bao
// lâu bằng state riêng của nó.
let trangThaiBuLai: { dangBu: boolean; soDongDaBu: number | null } = { dangBu: false, soDongDaBu: null }

export function layTrangThaiBuLai() {
  return trangThaiBuLai
}

// L4 (fix round Task 11, spec 7.10): `canTuDongDuPhong()` đã có đầy đủ logic từ Task 9 nhưng chưa
// có nơi nào GỌI nó — mã chết. `khoiDongOffline` là nơi hợp lý nhất để bắt đầu vòng lặp nền kiểm
// tra định kỳ này, vì đã có sẵn `kho` trong tay và đã là nơi "khởi động các vòng lặp nền" của tầng
// offline (cùng tinh thần vòng đồng bộ `batDauBoDongBo` ở trên).
//
// Ngưỡng đòi tự dự phòng (spec 7.10) là "quá 2 NGÀY hoặc quá 20 việc" — không cần kiểm tra liên
// tục, kiểm tra mỗi vài phút là đủ nhạy mà không tốn tài nguyên đọc IndexedDB (`docHangCho`) vô ích.
const CHU_KY_KIEM_TRA_DU_PHONG_MS = 5 * 60 * 1000

// Mốc lần tự-dự-phòng gần nhất — lưu ở localStorage để "hãm" chống spam (xem JSDoc
// `canTuDongDuPhong`, tepDuPhong.ts) sống sót qua lần tải lại trang, không chỉ trong bộ nhớ tab.
const KHOA_LAN_DU_PHONG_GAN_NHAT = 'qlgx.lanTuDongDuPhongGanNhat'

function docLanDuPhongGanNhat(): string | null {
  try {
    return localStorage.getItem(KHOA_LAN_DU_PHONG_GAN_NHAT)
  } catch {
    return null
  }
}

function ghiLanDuPhongGanNhat(iso: string) {
  try {
    localStorage.setItem(KHOA_LAN_DU_PHONG_GAN_NHAT, iso)
  } catch {
    // localStorage có thể bị chặn — không sao, chỉ mất tác dụng "hãm" chống spam, không hỏng gì.
  }
}

let idInterKiemTraDuPhong: ReturnType<typeof setInterval> | null = null

async function kiemTraTuDongDuPhong(kho: IDBDatabase): Promise<void> {
  try {
    const hangCho = await docHangCho(kho)
    if (!canTuDongDuPhong(hangCho, docLanDuPhongGanNhat())) return
    taiFileDuPhongXuong(hangCho)
    ghiLanDuPhongGanNhat(new Date().toISOString())
  } catch (loi) {
    console.error('Khong kiem tra/tai duoc file du phong tu dong', loi)
  }
}

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

  // F4: bắt đầu một lượt khởi động MỚI — hạ cờ "đã bị dừng giữa chừng" của lượt trước (nếu có) về
  // false; dungOffline() sẽ bật lại cờ này nếu xen vào TRONG lúc lượt khởi động dưới đây đang chạy.
  dangDungGiuaChung = false

  dangKhoiDong = (async () => {
    try {
      const kho = await moKho()
      // I6/`khoiTaoPhuThuoc`: `dieuKhien` được tham chiếu bên trong chính closure truyền cho
      // `batDauBoDongBo` — hợp lệ vì closure chỉ THỰC SỰ đọc `dieuKhien.trangThai` khi callback được
      // GỌI (lúc gặp `LoiEpochKhongKhop`), tại thời điểm đó `const dieuKhien` bên dưới chắc chắn đã
      // gán xong (JS đóng biến theo binding, không theo giá trị tại thời điểm tạo closure).
      const dieuKhien = batDauBoDongBo(kho, undefined, undefined, undefined, async (phuThuoc) => {
        // L3 (spec 4.8.6): đặt `dangBu = true` TRƯỚC khi gọi `xuLyEpochKhongKhopDonLuong` để
        // `ThanhTrangThai` hiện được câu "Máy chủ vừa được khôi phục..." NGAY khi bắt đầu bù, rồi
        // đặt lại `soDongDaBu` (bỏ qua kết quả trả về trước đây là cố ý — nay đọc lại để hiện dòng
        // tổng kết) sau khi bù xong.
        trangThaiBuLai = { dangBu: true, soDongDaBu: null }
        try {
          const { soDongDaBu } = await xuLyEpochKhongKhopDonLuong(phuThuoc, dieuKhien.trangThai)
          trangThaiBuLai = { dangBu: false, soDongDaBu }
        } catch (loi) {
          // F1 (fix round 2): nếu bước bù ném lỗi (vd. LoiThieuMocXoayEpoch — boDongBo.ts:894-899 bắt
          // lỗi này rồi chỉ log, vòng lặp đồng bộ chạy tiếp), PHẢI đặt lại cờ về false TRƯỚC KHI ném
          // lại lỗi — không thì `dangBu: true` kẹt vĩnh viễn, che mất nhánh 🔴 "Cần xem lại" trong
          // `tinhTrangThai` (đứng sau nhánh dangBuLai). Ném lại nguyên lỗi để boDongBo.ts vẫn log
          // đúng như cũ, không đổi hành vi log.
          trangThaiBuLai = { dangBu: false, soDongDaBu: null }
          throw loi
        }
      })
      // F4: kiểm tra ngay trước khi gán trạng thái cuối cùng — nếu dungOffline() đã xen vào TRONG
      // lúc đang await moKho()/batDauBoDongBo() ở trên, tự dọn (đóng kho, dừng dieuKhien) thay vì
      // "sống lại" bằng cách gán làm trạng thái hiện tại/đặt interval mới.
      if (dangDungGiuaChung) {
        dieuKhien.dung()
        kho.close()
        return null
      }
      trangThaiHienTai = { kho, dieuKhien }
      // L4: bắt đầu vòng lặp nền kiểm tra định kỳ "có cần tự dự phòng không" — chạy một lượt ngay
      // (không đợi hết chu kỳ đầu) rồi lặp lại mỗi CHU_KY_KIEM_TRA_DU_PHONG_MS.
      void kiemTraTuDongDuPhong(kho)
      idInterKiemTraDuPhong = setInterval(() => { void kiemTraTuDongDuPhong(kho) }, CHU_KY_KIEM_TRA_DU_PHONG_MS)
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
  // F4: báo cho một lượt khoiDongOffline() có thể đang dở (await moKho()/batDauBoDongBo()) biết là
  // đã bị dừng — xem chú thích ở khai báo dangDungGiuaChung và ở khoiDongOffline().
  dangDungGiuaChung = true
  if (trangThaiHienTai) {
    trangThaiHienTai.dieuKhien.dung()
    trangThaiHienTai.kho.close()
  }
  // L4: dừng vòng lặp nền kiểm tra tự dự phòng cùng lúc — không có bước này, interval của phiên CŨ
  // vẫn tiếp tục chạy trên `kho` đã đóng sau khi đăng xuất/đổi tài khoản.
  if (idInterKiemTraDuPhong !== null) {
    clearInterval(idInterKiemTraDuPhong)
    idInterKiemTraDuPhong = null
  }
  trangThaiHienTai = null
  dangKhoiDong = null
  // F3 (fix round 2): reset luôn trạng thái "đang bù lại" — không có bước này, máy dùng chung tài
  // khoản (A đăng xuất, B đăng nhập) có thể thấy nhầm dòng tổng kết "Đã gửi lại N thay đổi." của
  // tài khoản trước còn sót lại trong `ThanhTrangThai`.
  trangThaiBuLai = { dangBu: false, soDongDaBu: null }
}

/** CHỈ dùng cho kiểm thử — reset cache module-level giữa các ca kiểm thử (mỗi ca cần gọi lại
 * `khoiDongOffline()` từ đầu, không dính kết quả cache của ca trước). KHÔNG gọi trong mã sản phẩm. */
export function _resetChoKiemThu(): void {
  dangKhoiDong = null
  trangThaiHienTai = null
  if (idInterKiemTraDuPhong !== null) {
    clearInterval(idInterKiemTraDuPhong)
    idInterKiemTraDuPhong = null
  }
  // F3 (fix round 2): reset luôn giữa các ca kiểm thử — xem chú thích ở dungOffline().
  trangThaiBuLai = { dangBu: false, soDongDaBu: null }
}
