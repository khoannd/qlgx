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
  request: (tenKhoa, tuyChon, xuLy) => {
    // `navigator.locks` có thể KHÔNG tồn tại (secure context không đủ — http thường thay vì https
    // ngoài localhost, hoặc trình duyệt cũ). Trả về một promise BỊ TỪ CHỐI thay vì để
    // `navigator.locks.request` ném lỗi ĐỒNG BỘ ngay tại đây — hàm `troThanhChuKhiCoTheChoDenKhiHuy`
    // hứa "trả về NGAY", ném đồng bộ ở tầng này sẽ phá luôn phần khởi tạo của nơi gọi (Task 6).
    if (typeof navigator === 'undefined' || !navigator.locks) {
      return Promise.reject(new Error('navigator.locks khong kha dung (can secure context: https hoac localhost)'))
    }
    return navigator.locks.request(tenKhoa, tuyChon, xuLy)
  },
}

/** Tên khoá dùng CHUNG cho mọi tab của app — phải giống hệt nhau ở mọi nơi gọi
 * `troThanhChuKhiCoTheChoDenKhiHuy` để cùng tranh trên MỘT khoá (khác tên là mất tác dụng). */
export const TEN_KHOA_BAU_CHU = 'qlgx-bau-chu-dong-bo'

/**
 * Tranh khoá `tenKhoa` ĐÚNG MỘT LẦN. Gọi `khiLaChu()` NGAY khi giành được khoá (tab này giờ là bộ
 * gửi/nhận DUY NHẤT); gọi `khiMatChu()` khi mất khoá — DÙ VÌ LÝ DO GÌ: bị huỷ qua `tinHieuHuy`,
 * `khiLaChu` tự kết thúc bình thường (resolve), hay `khiLaChu` ném lỗi thật. Trả về NGAY (không đợi
 * giành được khoá) — trạng thái "đang chờ tới lượt/đã là chủ" đọc qua `laChu()`.
 *
 * **KHÔNG tự tranh lại sau khi mất khoá** (dù `tinHieuHuy` chưa hề báo huỷ) — nếu tầng gọi (Task 6)
 * muốn vòng đồng bộ tiếp tục vô thời hạn, CHÍNH nó phải hoặc (a) để `khiLaChu` tự là một vòng lặp
 * không bao giờ resolve cho tới khi thấy `tinHieuHuy.aborted`, hoặc (b) tự gọi lại
 * `troThanhChuKhiCoTheChoDenKhiHuy` từ trong `khiMatChu` nếu `!tinHieuHuy.aborted` (tự quyết định có
 * cần chờ/backoff trước khi tranh lại hay không, và có cần log/báo người dùng hay không nếu lý do
 * mất chủ là một lỗi thật). File này CỐ TÌNH không tự làm việc đó — quyết định "tranh lại ngay lập
 * tức, có chờ, hay bỏ cuộc và báo lỗi" phụ thuộc ngữ cảnh cụ thể của vòng đồng bộ (Task 6), không
 * phải việc của module bầu-chủ.
 *
 * `khiLaChu` có thể là một vòng lặp dài (đại diện cho toàn bộ vòng đồng bộ) — hàm `request()` của
 * Web Locks API CHỈ nhả khoá khi callback truyền vào RESOLVE/REJECT, nên khoá được giữ ĐÚNG SUỐT
 * thời gian `khiLaChu()` đang chạy, đến khi `tinHieuHuy` abort thì mới nhả (ném `AbortError`, phải
 * bắt và coi là kết thúc bình thường, không phải lỗi).
 *
 * QUAN TRỌNG (khác với suy nghĩ trực giác): một khi ĐÃ giành được khoá, bản thân `AbortSignal` KHÔNG
 * tự động buộc `khiLaChu` dừng lại hay tự nhả khoá — theo đúng thuật toán Web Locks API thật (bước
 * "process the lock request queue" gỡ handler abort khỏi signal NGAY khi khoá được cấp), `signal`
 * chỉ huỷ được request khi nó CÒN đang xếp hàng chờ (chưa được cấp khoá). Vì vậy `khiLaChu` (triển
 * khai ở Task 6) phải TỰ lắng nghe `tinHieuHuy` nó nhận được và tự kết thúc (resolve hoặc ném
 * `AbortError`) khi thấy huỷ — hàm này chỉ có nhiệm vụ BẮT `AbortError` đó để không lộ ra ngoài như
 * một lỗi thật.
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
      } finally {
        // Goi khiMatChu() TRUOC khi coLaChu doi lai false hay sau deu duoc VE MAT THU TU QUAN SAT
        // DUOC (ham nay dong bo, khong co ai doc laChu() xen giua) - nhung dat false TRUOC khi goi
        // khiMatChu de neu chinh khiMatChu (ma nguoi goi cung cap) co lo doc lai laChu(), no thay
        // dung trang thai "khong con la chu nua", khong phai trang thai cu da mat hieu luc. Chay
        // trong CA HAI nhanh (khiLaChu resolve binh thuong LAN reject) - "mat chu" la mat chu, bat
        // ke ly do gi (xem chu thich ham o tren).
        coLaChu = false
        khiMatChu()
      }
    })
    .catch((err: unknown) => {
      // Toi day co the la loi tu `khiLaChu` (nem lai tu khoi try/finally o tren - finally KHONG
      // nuot loi, chi chay xong roi loi tiep tuc lan truyen), HOAC loi tu chinh request() (con dang
      // XEP HANG thi bi huy, chua bao gio duoc goi khiLaChu, coLaChu chua tung la true - khong co gi
      // de "mat" nen KHONG goi khiMatChu, khac voi truong hop huy khi DANG la chu, da xu ly qua
      // nhanh finally o tren).
      //
      // Tieu chi PHAN BIET "huy binh thuong" voi "loi that" PHAI la `tinHieuHuy.aborted` — TUYET
      // DOI khong phai `err.name === 'AbortError'`/`err instanceof Error`: mot loi AbortError THAT
      // (vi du tu mot `fetch` khac bi mot AbortSignal KHAC huy, khong lien quan gi `tinHieuHuy` cua
      // ham nay) se bi nuot nham neu chi xet ten loi - "mat chu vi loi that" se bien mat khong dau
      // vet, dong bo ngung ma khong ai biet. Theo dung thuat toan Web Locks, duong DUY NHAT khien
      // `request()` reject vi huy la chinh `tinHieuHuy` (tham so `signal`) da abort.
      if (!tinHieuHuy.aborted) throw err
    })

  return {
    laChu: () => coLaChu,
  }
}
