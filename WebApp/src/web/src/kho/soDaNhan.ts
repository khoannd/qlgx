/**
 * Sổ đã nhận — kho `soDaNhan` (xem `moKho.ts`): bản sao các dòng `hieu_luc` đã áp, kèm epoch
 * và so_thu_tu gốc, giữ 30 ngày. Dùng để bù lại dữ liệu máy chủ đã mất (spec mục 4.8.2 và
 * 4.8.5).
 *
 * Kho `soDaNhan` KHÔNG có `keyPath` (Task 1) — file này chọn khoá là `${epoch}:${soThuTu}`
 * (chuỗi), vì cặp (epoch, soThuTu) định danh duy nhất một dòng và không bao giờ đổi, tránh
 * việc cập nhật một dòng cũ (ghi đè khoá trùng). Đảm bảo một dòng từ một epoch/soThuTu nhất
 * định chỉ xuất hiện một lần trong kho (vì khoá duy nhất).
 */
import { KHO_SO_DA_NHAN } from './moKho'

/**
 * Một dòng đã nhận từ máy chủ — lưu toàn bộ các trường của DongHieuLucDto phía máy chủ,
 * kèm epoch/soThuTu gốc và ngayNhan (thời điểm máy con ghi vào sổ). Vì Task 8 (bù lại
 * dữ liệu) sẽ đọc lại chính các trường này để gửi lên máy chủ như thao tác ghi bình thường,
 * tên trường phải khớp đúng (camelCase từ ASP.NET Core minimal API).
 */
export type DongDaNhan = {
  /** Danh tính chuỗi so_thu_tu lúc dòng này được áp — dùng để lọc theo epoch cũ khi bù (4.8.5). */
  epoch: string
  /** Vị trí trong chuỗi hieu_luc của epoch trên — số nguyên, khớp SoThuTu (long) phía máy chủ. */
  soThuTu: number
  /** Các trường còn lại của DongHieuLucDto phía máy chủ, y nguyên tên trường JSON (camelCase). */
  bang: string
  banGhiId: string
  truong: string
  giaTri: string | null
  dongHoVatLy: string
  dongHoLogic: number
  thietBiId: string | null
  giaoDichId: string
  /** Mốc máy CON tự ghi lúc lưu dòng này vào sổ (KHÔNG phải mốc từ máy chủ) — ISO 8601,
   * dùng làm mốc "30 ngày" của donSoDaNhanCu. */
  ngayNhan: string
}

/**
 * Ghi nhiều dòng đã nhận trong MỘT giao dịch `readwrite` duy nhất. Khoá lưu trong kho là
 * `${epoch}:${soThuTu}` — định danh duy nhất một dòng từ một chuỗi so_thu_tu cụ thể. Nếu
 * ghi lại dòng cũ (cùng epoch/soThuTu) thì ghi đè (put, không add).
 */
export function ghiSoDaNhan(kho: IDBDatabase, dong: DongDaNhan[]): Promise<void> {
  return new Promise((resolve, reject) => {
    if (dong.length === 0) {
      resolve()
      return
    }

    const gd = kho.transaction(KHO_SO_DA_NHAN, 'readwrite')
    const store = gd.objectStore(KHO_SO_DA_NHAN)

    // Ghi tất cả dòng trong cùng một giao dịch.
    for (const d of dong) {
      const khoa = `${d.epoch}:${d.soThuTu}`
      store.put(d, khoa)
    }

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không ghi được sổ đã nhận'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch ghi sổ đã nhận bị huỷ giữa chừng'))
  })
}

/**
 * Đọc các dòng có epoch khớp và soThuTu > tuSoThuTu, sắp xếp theo soThuTu tăng dần.
 * Dùng để áp dữ liệu nhận về (Task 6) hoặc bù lại dữ liệu sau khôi phục máy chủ (Task 8,
 * spec mục 4.8.5 bước 2).
 */
export function docTheoKhoang(kho: IDBDatabase, epoch: string, tuSoThuTu: number): Promise<DongDaNhan[]> {
  return new Promise((resolve, reject) => {
    const store = kho.transaction(KHO_SO_DA_NHAN, 'readonly').objectStore(KHO_SO_DA_NHAN)
    const ketQua: DongDaNhan[] = []

    // Mở con trỏ để duyệt toàn bộ kho — cần lọc epoch và soThuTu theo điều kiện trong callback
    // (IndexedDB không hỗ trợ lọc compound condition trên một khoá chuỗi dạng "epoch:soThuTu").
    const yc = store.openCursor()
    yc.onsuccess = () => {
      const con = yc.result
      if (con) {
        const d = con.value as DongDaNhan
        // Chỉ lấy dòng có epoch khớp VÀ soThuTu > tuSoThuTu
        if (d.epoch === epoch && d.soThuTu > tuSoThuTu) {
          ketQua.push(d)
        }
        con.continue()
      } else {
        // Con trỏ hết — sắp xếp theo soThuTu tăng dần trước khi trả về
        ketQua.sort((a, b) => a.soThuTu - b.soThuTu)
        resolve(ketQua)
      }
    }
    yc.onerror = () => reject(yc.error)
  })
}

/**
 * Xoá các dòng có ngayNhan < truocNgay (so sánh chuỗi ISO 8601 hợp lệ theo thứ tự từ điển
 * = thứ tự thời gian). Dòng có ngayNhan >= truocNgay KHÔNG được đụng tới — điều này bắt
 * buộc phải kiểm thử riêng (fact từ brief, xem dòng dưới).
 *
 * Lý do giữ 30 ngày: spec mục 4.8.2 — máy con giữ bản sao các dòng đã áp để bù lại dữ liệu
 * máy chủ đã mất, nhưng giữ vô hạn thì lưu trữ phình to. Sau 30 ngày nếu vẫn chưa tính được
 * gì từ sổ đã nhận thì an toàn để xoá (máy chủ đã ghi lại dữ liệu cuối cùng của nó).
 *
 * **Fact bắt buộc (từ brief):** dòng có ngayNhan >= truocNgay KHÔNG được đụng tới — viết
 * test khẳng định rõ điều này (không chỉ test "dòng cũ bị xoá", mà còn test "dòng mới còn
 * nguyên" trong cùng một lần gọi).
 */
export function donSoDaNhanCu(kho: IDBDatabase, truocNgay: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const gd = kho.transaction(KHO_SO_DA_NHAN, 'readwrite')
    const store = gd.objectStore(KHO_SO_DA_NHAN)

    const yc = store.openCursor()
    yc.onsuccess = () => {
      const con = yc.result
      if (con) {
        const d = con.value as DongDaNhan
        // Xoá chỉ khi ngayNhan < truocNgay (so sánh chuỗi)
        if (d.ngayNhan < truocNgay) {
          con.delete()
        }
        con.continue()
      }
    }

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không dọn được sổ đã nhận cũ'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch dọn sổ đã nhận bị huỷ giữa chừng'))
  })
}
