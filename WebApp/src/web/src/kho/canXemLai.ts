/**
 * Hộp "cần xem lại" — kho `canXemLai` (xem `moKho.ts`, tên kho đã được Task 1 dựng sẵn). Chứa các
 * thao tác máy chủ đã TỪ CHỐI (`KetQuaThaoTacDto.ketQua === 'tu_choi'`, ví dụ vi phạm một rào chắn
 * tất định) — Task 6 (bộ đồng bộ) tự tạo mục ở đây khi gặp kết quả này, vì máy chủ KHÔNG tự làm
 * việc đó, và không ghi lại thì dữ liệu người dùng đã gõ sẽ biến mất khỏi hàng chờ mà không một lời
 * giải thích (xem task-6-brief.md mục "Xử lý kết quả tu_choi").
 *
 * Cùng khuôn mẫu với `soDaNhan.ts`/`hangCho.ts`: một hàm …TrongGiaoDich trên `IDBObjectStore` đã mở
 * sẵn (để Task 6 ghép được vào ĐÚNG giao dịch xoá dòng đó khỏi hàng chờ — mất nguyên tử ở đây nghĩa
 * là có thể ghi được vào hộp cần xem lại NHƯNG dòng gốc vẫn còn kẹt trong hàng chờ, bị gửi lại vô
 * hạn lần sau), cộng các hàm đọc/đếm ĐỘC LẬP cho Task 10 (thanh trạng thái) dùng sau này.
 *
 * Khoá lưu trong kho: `maThaoTac` — đây là khoá nghiệp vụ thật của MỘT thao tác (spec mục 5 ràng
 * buộc 9), duy nhất theo thiết kế, nên dùng thẳng làm khoá IndexedDB thay vì tự quản một số tăng
 * dần như `hangCho.ts` phải làm (ở đó khoá tăng dần chỉ để giữ thứ tự chèn, không mang ý nghĩa
 * nghiệp vụ) — không có `keyPath` (Task 1), nên vẫn phải truyền khoá tường minh khi `put()`.
 */
import { KHO_CAN_XEM_LAI } from './moKho'
import type { DongHangCho } from './hangCho'

/**
 * Một mục cần xem lại. Giữ NGUYÊN `dongHangChoGoc` (toàn bộ dòng hàng chờ bị từ chối) — đó là giá
 * trị người dùng đã nhập, cần có lại đầy đủ để họ nhập lại hoặc để Task 10 hiển thị "thao tác này
 * định làm gì" — không phải chỉ hiển thị mã lỗi suông.
 */
export type DongCanXemLai = {
  /** Trùng khoá lưu trong kho — lặp lại tường minh trong giá trị để nơi đọc không phải tự suy ra
   * từ khoá IndexedDB (đối xứng với `DongDaNhan` không lặp lại khoá — ở đó khoá là khoá GHÉP từ
   * hai trường đã có sẵn trong giá trị; ở đây khoá CHÍNH LÀ MỘT trường, lặp lại vẫn rõ ràng hơn). */
  maThaoTac: string
  /** Lời máy chủ giải thích vì sao từ chối (`KetQuaThaoTacDto.thongBao`) — `null` nếu máy chủ
   * không kèm lời giải thích nào. */
  thongBao: string | null
  /** Dòng hàng chờ gốc bị từ chối — xem chú thích đầu file vì sao phải giữ nguyên. */
  dongHangChoGoc: DongHangCho
  /** Mốc máy CON tự ghi lúc lưu mục này (ISO 8601) — dùng cho Task 10 sắp xếp/hiển thị, không
   * phải mốc đồng hồ lai (không cần độ chính xác micro giây ở đây). */
  taoLuc: string
}

/** Lõi dùng chung, TRÊN MỘT `IDBObjectStore` (`KHO_CAN_XEM_LAI`) đã mở sẵn ở chế độ `readwrite` —
 * không tự mở/đóng giao dịch, để Task 6 ghép được vào MỘT giao dịch DUY NHẤT cùng với việc xoá dòng
 * gốc khỏi hàng chờ (spec mục 5, ràng buộc 3 áp dụng ở đây với cùng lý do: ghi cần-xem-lại và xoá
 * hàng chờ tách rời nhau là chỗ có thể mất dữ liệu giữa chừng). Không tự `reject` khi ghi lỗi
 * (`baoLoi` để trống nếu nơi gọi đã có `onerror`/`onabort` riêng của giao dịch bao ngoài) — cùng
 * khuôn mẫu với `ghiSoDaNhanTrongGiaoDich`. */
export function ghiCanXemLaiTrongGiaoDich(
  storeCanXemLai: IDBObjectStore,
  dong: DongCanXemLai[],
  baoLoi: (loi: unknown) => void,
): void {
  for (const d of dong) {
    // put() (không phải add()): nếu vì lý do gì đó cùng maThaoTac bị từ chối gửi lại lần hai
    // (không nên xảy ra bình thường — hàng chờ đã xoá dòng đó — nhưng an toàn hơn là ném lỗi ở
    // đúng chỗ không quan trọng bằng việc giao dịch chính vẫn hoàn tất), ghi đè là chấp nhận được.
    const yc = storeCanXemLai.put(d, d.maThaoTac)
    yc.onerror = () => baoLoi(yc.error)
  }
}

/** Ghi nhiều mục cần xem lại trong MỘT giao dịch `readwrite` độc lập — dùng khi KHÔNG cần ghép
 * chung giao dịch với thao tác nào khác (ví dụ kiểm thử trực tiếp module này). Khi cần ghép chung
 * (Task 6, xử lý kết quả `tu_choi` cùng lúc xoá hàng chờ), dùng `ghiCanXemLaiTrongGiaoDich`. */
export function ghiCanXemLai(kho: IDBDatabase, dong: DongCanXemLai[]): Promise<void> {
  return new Promise((resolve, reject) => {
    if (dong.length === 0) {
      resolve()
      return
    }
    const gd = kho.transaction(KHO_CAN_XEM_LAI, 'readwrite')
    ghiCanXemLaiTrongGiaoDich(gd.objectStore(KHO_CAN_XEM_LAI), dong, () => {})
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không ghi được mục cần xem lại'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch ghi cần xem lại bị huỷ giữa chừng'))
  })
}

/** Đọc toàn bộ hộp cần xem lại — dùng cho Task 10 (thanh trạng thái/màn hình xử lý). Thứ tự trả về
 * theo thứ tự duyệt con trỏ mặc định (thứ tự khoá `maThaoTac`, KHÔNG mang ý nghĩa thời gian) — Task
 * 10 tự sắp lại theo `taoLuc` nếu cần hiển thị theo thời gian. */
export function docCanXemLai(kho: IDBDatabase): Promise<DongCanXemLai[]> {
  return new Promise((resolve, reject) => {
    const store = kho.transaction(KHO_CAN_XEM_LAI, 'readonly').objectStore(KHO_CAN_XEM_LAI)
    const ketQua: DongCanXemLai[] = []
    const yc = store.openCursor()
    yc.onsuccess = () => {
      const con = yc.result
      if (con) {
        ketQua.push(con.value as DongCanXemLai)
        con.continue()
      } else {
        resolve(ketQua)
      }
    }
    yc.onerror = () => reject(yc.error)
  })
}

/** Xoá một mục đã xử lý xong (người dùng đã chọn giá trị hoặc đánh dấu "đã xử lý" — Task 10). */
export function xoaKhoiCanXemLai(kho: IDBDatabase, maThaoTac: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const gd = kho.transaction(KHO_CAN_XEM_LAI, 'readwrite')
    gd.objectStore(KHO_CAN_XEM_LAI).delete(maThaoTac)
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không xoá được mục cần xem lại'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch xoá cần xem lại bị huỷ giữa chừng'))
  })
}

/** Đếm số mục đang chờ xử lý — dùng cho thanh trạng thái (Task 10) báo "có N việc cần xem lại". */
export function demCanXemLai(kho: IDBDatabase): Promise<number> {
  return new Promise((resolve, reject) => {
    const yc = kho.transaction(KHO_CAN_XEM_LAI, 'readonly').objectStore(KHO_CAN_XEM_LAI).count()
    yc.onsuccess = () => resolve(yc.result)
    yc.onerror = () => reject(yc.error)
  })
}
