/**
 * "Hàng chờ" — kho `hangCho` (xem `moKho.ts`): danh sách các việc CHƯA GỬI được lên máy chủ. Đây
 * là kho hiển thị trên màn hình ra "việc chưa gửi" (mục 2, bảng từ vựng) và là thứ MẤT LÀ MẤT
 * VĨNH VIỄN nếu bị xoá nhầm hay ghi tách rời khỏi bản ghi hiển thị (xem `khoDuLieu.ts`).
 *
 * Kho `hangCho` KHÔNG có `keyPath` (Task 1) — file này chọn khoá là một SỐ TĂNG DẦN tự quản lý
 * (không phải `autoIncrement` của IndexedDB, vì kho đã được tạo không bật tuỳ chọn đó ở Task 1 và
 * không được đổi `keyPath`/tuỳ chọn của kho đã có dữ liệu — xem chú thích đầu `moKho.ts`). Số này
 * CHỈ dùng để giữ ĐÚNG THỨ TỰ CHÈN khi đọc hàng chờ (`docHangCho`) bằng con trỏ mặc định (khoá tăng
 * dần = thứ tự chèn) — không mang ý nghĩa nghiệp vụ. Khoá nghiệp vụ thật của một việc là `maThaoTac`
 * (do máy con sinh — xem spec mục 5 ràng buộc 9), dùng để xoá đúng dòng sau khi gửi thành công
 * (`xoaKhoiHangCho`).
 */
import { KHO_HANG_CHO } from './moKho'

/** Một dòng "việc chưa gửi". `doan` (số hiệu đoạn đồng hồ — spec mục 4.3) được Task 4 dùng để biết
 * thao tác này cần hiệu chỉnh theo độ lệch đồng hồ nào; mặc định `0` cho tới khi Task 4 gán số đoạn
 * thật. Các trường nghiệp vụ khác (bảng nào, bản ghi nào, giá trị gì...) không cố định ở Task 2 —
 * để tầng gọi (Task 6, 7) quyết định hình dạng, tránh Task 2 đoán sai rồi phải đổi lại. */
export type DongHangCho = {
  /** Khoá nghiệp vụ, do máy con sinh, chống gửi trùng khi gửi lại (spec mục 5, ràng buộc 9). */
  maThaoTac: string
  /** Số hiệu đoạn đồng hồ — mặc định 0 (xem chú thích đầu file). */
  doan: number
} & Record<string, unknown>

/**
 * Lõi dùng chung: mở con trỏ LÙI trên `khoHangCho` (khoá lớn nhất hiện có) rồi gọi `xuLy(khoaKeTiep)`
 * NGAY TRONG callback `onsuccess` của chính con trỏ đó — ĐỒNG BỘ, không qua `Promise`/`await` — để
 * nơi gọi (`khoDuLieu.ts`) có thể `put()` dòng hàng chờ mới trong CÙNG một giao dịch với việc ghi
 * bản ghi hiển thị, không rủi ro giao dịch tự động commit trước khi lệnh `put()` tiếp theo kịp gửi
 * đi (IndexedDB đóng giao dịch khi không còn yêu cầu nào treo và luồng JS quay lại vòng lặp sự
 * kiện).
 *
 * Dùng con trỏ LÙI (khoá lớn nhất thực sự đang có) thay vì `count()` để không bị lệch khi đã xoá
 * bớt dòng cũ: `count()` giảm sau khi xoá, nếu dùng nó làm khoá kế tiếp thì dòng MỚI có thể nhận
 * một khoá NHỎ HƠN dòng CŨ còn lại trong kho — làm hỏng thứ tự đọc hàng chờ (`docHangCho`).
 */
export function voiKhoaKeTiep(
  khoHangCho: IDBObjectStore,
  xuLy: (khoaKeTiep: number) => void,
  baoLoi: (loi: unknown) => void,
): void {
  const yc = khoHangCho.openCursor(null, 'prev')
  yc.onsuccess = () => {
    const con = yc.result
    xuLy(con ? (con.key as number) + 1 : 0)
  }
  yc.onerror = () => baoLoi(yc.error)
}

/** Tìm khoá (số thứ tự chèn) kế tiếp — bản bọc `Promise` của `voiKhoaKeTiep`, dùng khi gọi ĐỘC LẬP
 * (không cần ghép chung giao dịch với thao tác nào khác). */
export function laySoThuTuKeTiep(khoHangCho: IDBObjectStore): Promise<number> {
  return new Promise((resolve, reject) => voiKhoaKeTiep(khoHangCho, resolve, reject))
}

/** Đọc tối đa `gioiHan` dòng đầu hàng chờ, theo ĐÚNG thứ tự chèn (xem chú thích đầu file). Không
 * truyền `gioiHan` thì đọc hết. */
export function docHangCho(kho: IDBDatabase, gioiHan?: number): Promise<DongHangCho[]> {
  return new Promise((resolve, reject) => {
    const store = kho.transaction(KHO_HANG_CHO, 'readonly').objectStore(KHO_HANG_CHO)
    const ketQua: DongHangCho[] = []
    const yc = store.openCursor()
    yc.onsuccess = () => {
      const con = yc.result
      if (con && (gioiHan === undefined || ketQua.length < gioiHan)) {
        ketQua.push(con.value as DongHangCho)
        con.continue()
      } else {
        resolve(ketQua)
      }
    }
    yc.onerror = () => reject(yc.error)
  })
}

/** Xoá khỏi hàng chờ các việc đã gửi thành công, theo `maThaoTac` (không phải khoá lưu trong kho —
 * xem chú thích đầu file), nên phải quét toàn bộ để tìm đúng dòng. Hàng chờ vốn chỉ giữ việc CHƯA
 * gửi nên số dòng nhỏ, quét toàn bộ ở đây không đáng ngại. */
export function xoaKhoiHangCho(kho: IDBDatabase, maThaoTac: string[]): Promise<void> {
  return new Promise((resolve, reject) => {
    if (maThaoTac.length === 0) {
      resolve()
      return
    }
    const canXoa = new Set(maThaoTac)
    const gd = kho.transaction(KHO_HANG_CHO, 'readwrite')
    const store = gd.objectStore(KHO_HANG_CHO)
    const yc = store.openCursor()
    yc.onsuccess = () => {
      const con = yc.result
      if (con) {
        const dong = con.value as DongHangCho
        if (canXoa.has(dong.maThaoTac)) con.delete()
        con.continue()
      }
    }
    yc.onerror = () => reject(yc.error)
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error)
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch xoá hàng chờ bị huỷ giữa chừng'))
  })
}

/** Đếm số việc đang chờ gửi — dùng cho thanh trạng thái và các màn hình chặn ("Gỡ máy này ra",
 * "Bàn giao máy này"... xem spec mục 6.4) khi hàng chờ chưa rỗng. */
export function demHangCho(kho: IDBDatabase): Promise<number> {
  return new Promise((resolve, reject) => {
    const yc = kho.transaction(KHO_HANG_CHO, 'readonly').objectStore(KHO_HANG_CHO).count()
    yc.onsuccess = () => resolve(yc.result)
    yc.onerror = () => reject(yc.error)
  })
}
