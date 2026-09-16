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

/** Lõi dùng chung, TRÊN MỘT `IDBObjectStore` (`KHO_SO_DA_NHAN`) đã mở sẵn ở chế độ `readwrite` —
 * không tự mở/đóng giao dịch, để nơi gọi (Task 6: áp thao tác nhận về + tiến con trỏ + ghi sổ đã
 * nhận + xoá khỏi hàng chờ trong MỘT giao dịch — spec mục 5, ràng buộc 3) ghép được lệnh ghi này
 * vào giao dịch của chính nó, cùng khuôn mẫu với `xoaKhoiHangChoTrongGiaoDich`/`ghiConTroTrongGiaoDich`.
 * Không tự `reject` khi ghi lỗi (`baoLoi` để trống nếu nơi gọi đã có `onerror`/`onabort` riêng của
 * giao dịch bao ngoài). Thiếu hàm này thì Task 6 phải commit riêng con trỏ trước rồi mới ghi sổ đã
 * nhận sau — sập máy/hết quota đúng giữa hai bước đó khiến con trỏ nói "đã nhận tới X" trong khi sổ
 * đã nhận không có các dòng đó, và Task 8 (bù lại sau khôi phục) sẽ bù thiếu mà không ai biết. */
export function ghiSoDaNhanTrongGiaoDich(
  storeSoDaNhan: IDBObjectStore,
  dong: DongDaNhan[],
  baoLoi: (loi: unknown) => void,
): void {
  for (const d of dong) {
    const yc = storeSoDaNhan.put(d, `${d.epoch}:${d.soThuTu}`)
    yc.onerror = () => baoLoi(yc.error)
  }
}

/**
 * Ghi nhiều dòng đã nhận trong MỘT giao dịch `readwrite` duy nhất. Khoá lưu trong kho là
 * `${epoch}:${soThuTu}` — định danh duy nhất một dòng từ một chuỗi so_thu_tu cụ thể (BẮT BUỘC
 * gồm cả `epoch`: khi máy chủ xoay epoch sau khôi phục, chuỗi so_thu_tu mới khởi động lại từ số
 * nhỏ, nên hai epoch khác nhau có thể trùng soThuTu — bỏ epoch khỏi khoá sẽ khiến dòng epoch CŨ,
 * đúng thứ Task 8 cần để bù lại, bị ghi đè im lặng). Nếu ghi lại dòng cũ (cùng epoch/soThuTu) thì
 * ghi đè (put, không add).
 *
 * Bản ĐỘC LẬP (tự mở giao dịch riêng) — dùng khi KHÔNG cần ghép chung giao dịch với thao tác nào
 * khác. Khi cần ghép chung (Task 6), dùng `ghiSoDaNhanTrongGiaoDich` trên store đã mở sẵn.
 */
export function ghiSoDaNhan(kho: IDBDatabase, dong: DongDaNhan[]): Promise<void> {
  return new Promise((resolve, reject) => {
    if (dong.length === 0) {
      resolve()
      return
    }

    const gd = kho.transaction(KHO_SO_DA_NHAN, 'readwrite')
    ghiSoDaNhanTrongGiaoDich(gd.objectStore(KHO_SO_DA_NHAN), dong, () => {})

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
    const gd = kho.transaction(KHO_SO_DA_NHAN, 'readonly')
    const ketQua: DongDaNhan[] = []

    // Mở con trỏ để duyệt toàn bộ kho — cần lọc epoch và soThuTu theo điều kiện trong callback
    // (IndexedDB không hỗ trợ lọc compound condition trên một khoá chuỗi dạng "epoch:soThuTu").
    const yc = gd.objectStore(KHO_SO_DA_NHAN).openCursor()
    yc.onsuccess = () => {
      const con = yc.result
      if (con) {
        const d = con.value as DongDaNhan
        // `>` KHÔNG `>=`: spec 4.8.5 bước 2 — con trỏ cũ (epoch_cũ, 5000) so với máy chủ còn tối
        // đa 4900 nghĩa là so_thu_tu <= 4900 máy chủ VẪN CÒN (không mất), chỉ phần > 4900 mới là
        // phần bị mất cần bù lại. Lấy cả == thì gửi lại một dòng máy chủ chưa từng đánh rơi.
        if (d.epoch === epoch && d.soThuTu > tuSoThuTu) {
          ketQua.push(d)
        }
        con.continue()
      }
    }
    yc.onerror = () => reject(yc.error)
    gd.oncomplete = () => {
      // Sắp xếp SỐ HỌC theo soThuTu (không phải thứ tự duyệt cursor, vốn theo khoá CHUỖI
      // "epoch:soThuTu" — "e1:100" đứng trước "e1:20" theo so chuỗi dù 100 > 20 theo số).
      ketQua.sort((a, b) => a.soThuTu - b.soThuTu)
      resolve(ketQua)
    }
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch đọc sổ đã nhận bị huỷ giữa chừng'))
  })
}

/**
 * Xoá các dòng có `ngayNhan < truocNgay` (so sánh chuỗi ISO 8601 hợp lệ theo thứ tự từ điển
 * = thứ tự thời gian). Dòng có `ngayNhan >= truocNgay` KHÔNG được đụng tới.
 *
 * Lý do giữ 30 ngày: spec mục 4.8.2 — máy con giữ bản sao các dòng đã áp để bù lại dữ liệu
 * máy chủ đã mất, nhưng giữ vô hạn thì lưu trữ phình to. Sau 30 ngày nếu vẫn chưa tính được
 * gì từ sổ đã nhận thì an toàn để xoá (máy chủ đã ghi lại dữ liệu cuối cùng của nó). Duyệt
 * TOÀN BỘ kho bằng con trỏ (không dừng sớm dù đã xoá được một dòng) vì thứ tự khoá chuỗi
 * "epoch:soThuTu" không liên quan gì tới thứ tự `ngayNhan` — dòng cần xoá có thể nằm ở bất
 * kỳ vị trí nào trong lần duyệt.
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
        if (d.ngayNhan < truocNgay) {
          con.delete()
        }
        con.continue()
      }
    }
    yc.onerror = () => reject(yc.error)

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không dọn được sổ đã nhận cũ'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch dọn sổ đã nhận bị huỷ giữa chừng'))
  })
}
