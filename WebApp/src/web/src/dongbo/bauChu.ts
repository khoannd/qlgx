/**
 * Bầu chủ một tab — Ràng buộc 7 (spec). Quý sơ mở hai ba tab QLGX là chuyện thường (`banNhap.ts` đã
 * ghi nhận vấn đề nhiều thẻ). Nếu bộ gửi/nhận đồng bộ chạy ở CẢ HAI tab cùng lúc, chúng chạy đua ghi
 * con trỏ: tab A ghi "đã nhận tới 4830" trong khi tab B mới áp tới 4825 → tab B đè con trỏ THẤP HƠN
 * lên sau, làm mất dấu — lần sau máy này tưởng mới áp tới 4825, BỎ SÓT dòng 4826-4830 mãi mãi (vi
 * phạm Ràng buộc 1: không được mất dữ liệu).
 *
 * Cách chặn: dùng `navigator.locks.request` với một khoá đặt tên CHUNG cho mọi tab của app — tại
 * một thời điểm, trình duyệt chỉ cho ĐÚNG MỘT tab giữ khoá này; các tab khác tự động xếp hàng chờ
 * (FIFO), không cần tự cài đặt gì thêm để tránh chạy đua. Tab không giữ được khoá chỉ đọc kho và
 * nghe `BroadcastChannel` (việc đó là phạm vi Task 6, không phải file này).
 */

/**
 * Trừu tượng hoá TỐI THIỂU của Web Locks API mà file này cần dùng — CHỈ đúng phần này, để tiêm được
 * một bản giả lập trong test. Lý do bắt buộc phải tiêm (dependency injection), không gọi thẳng
 * `navigator.locks.request`: môi trường test (jsdom qua Vitest) không có polyfill chuẩn cho
 * `navigator.locks`, và bản chất phép thử ở đây cần NHIỀU "tab" giả tranh nhau MỘT khoá dùng chung
 * trong CÙNG một tiến trình Node — không có gì đảm bảo Web Locks thật (nếu có) hoạt động đúng ngữ
 * nghĩa "một tiến trình, nhiều lần gọi" đó. Cùng khuôn mẫu với `NguonThoiGian` ở `dongHoMayCon.ts`
 * (Task 4).
 */
export type NguonKhoa = {
  request<T>(tenKhoa: string, tuyChon: { signal: AbortSignal }, xuLy: () => Promise<T>): Promise<T>
}

const NGUON_KHOA_MAC_DINH: NguonKhoa = {
  request: (tenKhoa, tuyChon, xuLy) => navigator.locks.request(tenKhoa, tuyChon, xuLy),
}

/** Tên khoá dùng CHUNG cho mọi tab của app — phải giống hệt nhau ở mọi nơi gọi
 * `troThanhChuKhiCoTheChoDenKhiHuy` để cùng tranh trên MỘT khoá (khác tên là mất tác dụng). */
export const TEN_KHOA_BAU_CHU = 'qlgx-bau-chu-dong-bo'

/** `err.name === 'AbortError'` là cách chuẩn (DOM) để nhận biết một promise bị từ chối vì
 * `AbortSignal` báo huỷ, KHÔNG phải vì một lỗi thật xảy ra bên trong `khiLaChu`. */
function laLoiHuyBoThat(err: unknown): boolean {
  return err instanceof Error && err.name === 'AbortError'
}

/**
 * Tranh khoá `tenKhoa` mãi mãi cho tới khi `tinHieuHuy` báo huỷ (đóng tab/dừng ứng dụng chủ động).
 * Gọi `khiLaChu()` NGAY khi giành được khoá (tab này giờ là bộ gửi/nhận DUY NHẤT); gọi `khiMatChu()`
 * khi mất khoá (bị huỷ, hoặc — không xảy ra với `navigator.locks` thật trừ khi tự huỷ — lỗi). Trả
 * về NGAY (không đợi giành được khoá) — trạng thái "đang chờ tới lượt/đã là chủ" đọc qua `laChu()`.
 *
 * `khiLaChu` có thể là một vòng lặp dài (đại diện cho toàn bộ vòng đồng bộ) — hàm `request()` của
 * Web Locks API CHỈ nhả khoá khi callback truyền vào RESOLVE/REJECT, nên khoá được giữ ĐÚNG SUỐT
 * thời gian `khiLaChu()` đang chạy, đến khi `tinHieuHuy` abort thì mới nhả (ném `AbortError`, phải
 * bắt và coi là kết thúc bình thường, không phải lỗi).
 *
 * QUAN TRỌNG (khác với suy nghĩ trực giác): một khi ĐÃ giành được khoá, bản thân `AbortSignal` KHÔNG
 * tự động buộc `khiLaChu` dừng lại hay tự nhả khoá — theo đúng Web Locks API thật, `signal` chỉ huỷ
 * được request khi nó CÒN đang xếp hàng chờ (chưa được cấp khoá). Vì vậy `khiLaChu` (triển khai ở
 * Task 6) phải TỰ lắng nghe `tinHieuHuy` nó nhận được và tự kết thúc (resolve hoặc ném `AbortError`)
 * khi thấy huỷ — hàm này chỉ có nhiệm vụ BẮT `AbortError` đó để không lộ ra ngoài như một lỗi thật.
 */
export function troThanhChuKhiCoTheChoDenKhiHuy(
  khiLaChu: (tinHieuHuy: AbortSignal) => Promise<void>,
  khiMatChu: () => void,
  tinHieuHuy: AbortSignal,
  tenKhoa: string = TEN_KHOA_BAU_CHU,
  nguon: NguonKhoa = NGUON_KHOA_MAC_DINH,
): { laChu(): boolean } {
  // Cờ đọc qua `laChu()` — chỉ đúng MỘT nơi set no: bên trong callback truyền cho `nguon.request`,
  // set `true` ngay khi callback bắt đầu chạy (tức đã giành được khoá) và `false` ngay trong khối
  // `finally` (tức đã/sắp mất khoá) — nhờ vậy `laChu()` không bao giờ "lạc hậu một nhịp" so với thực
  // tế đang giữ hay không giữ khoá.
  let coLaChu = false

  nguon
    .request(tenKhoa, { signal: tinHieuHuy }, async () => {
      coLaChu = true
      try {
        await khiLaChu(tinHieuHuy)
      } catch (err) {
        // AbortError la ket thuc BINH THUONG (tab chu tu dong roi vi tinHieuHuy huy) - nuot o day,
        // KHONG duoc coi la loi that. Loi nao khac phai duoc nem lai, khong duoc am tham bo qua -
        // "mat chu vi loi" van la mat chu that (xem chu thich khiMatChu o tren), nhung ban than
        // loi do van phai lo ra ngoai de khong bi giau di.
        if (!laLoiHuyBoThat(err)) throw err
      } finally {
        // Goi khiMatChu() TRUOC khi coLaChu doi lai false hay sau deu duoc VE MAT THU TU QUAN SAT
        // DUOC (ham nay dong bo, khong co ai doc laChu() xen giua) - nhung dat false TRUOC khi goi
        // khiMatChu de neu chinh khiMatChu (ma nguoi goi cung cap) co lo doc lai laChu(), no thay
        // dung trang thai "khong con la chu nua", khong phai trang thai cu da mat hieu luc.
        coLaChu = false
        khiMatChu()
      }
    })
    .catch((err: unknown) => {
      // Toi day la loi TU CHINH request() (khong phai tu khiLaChu, vi loi tu khiLaChu da duoc xu ly
      // va nem lai/nuot o tren). Hai truong hop:
      // 1. AbortError vi tinHieuHuy huy trong luc CON XEP HANG (chua bao gio duoc goi khiLaChu,
      //    coLaChu chua tung la true) - day la huy binh thuong cua mot tab CHUA TUNG la chu, khong
      //    co gi de "mat" nen KHONG goi khiMatChu (khac voi truong hop huy khi DANG la chu, da duoc
      //    xu ly qua nhanh finally o tren).
      // 2. Loi that (khong phai AbortError) duoc nem lai tu khoi catch ben trong - ham nay tra ve
      //    NGAY tu dau (khong phai promise) nen khong con noi nao de bao loi nay len tren; danh phai
      //    de no thanh unhandled rejection de khong bi am tham mat di hoan toan.
      if (!laLoiHuyBoThat(err)) throw err
    })

  return {
    laChu: () => coLaChu,
  }
}
