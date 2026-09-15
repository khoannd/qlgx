/**
 * `conTro` — kho chỉ có ĐÚNG MỘT dòng, giữ trạng thái đồng bộ của máy này (vị trí đã kéo tới đâu
 * trong chuỗi `hieu_luc` của máy chủ — spec mục 4.5). Không phải một danh sách, không có nhiều dòng
 * theo bất cứ khoá nào — khoá cố định `KHOA_CON_TRO`.
 *
 * Kiểu `ConTro` CỐ Ý là một object MỞ RỘNG ĐƯỢC (`Record<string, unknown>` cộng thêm), KHÔNG khai
 * thành một `interface`/`type` đóng kín chỉ có `epoch`/`soThuTu`: Task 4 (đồng hồ máy con) sẽ thêm
 * các trường của nó (`mocNeo`, `msTaiNeo`, `doanHienTai`, `doLechDoanHienTai`...) vào ĐÚNG kiểu này
 * mà KHÔNG được sửa lại chữ ký `docConTro`/`ghiConTro` ở đây. Nếu khai kiểu đóng kín, Task 4 buộc
 * phải sửa file này (đổi kiểu) — điều brief yêu cầu tránh.
 *
 * `ghiConTro` GHI ĐÈ MỘT PHẦN (merge patch): chỉ cập nhật các trường có mặt trong `sua`, giữ nguyên
 * mọi trường khác. Lý do bắt buộc: Task 4 ghi các trường đồng hồ của nó, Task 6 ghi `epoch`/
 * `soThuTu` — nếu `ghiConTro` ghi đè toàn bộ dòng, hai task sẽ XOÁ MẤT trường của nhau mỗi lần gọi
 * xen kẽ, dù không hề có ý đó.
 */
import { KHO_CON_TRO } from './moKho'

/** Khoá cố định của dòng duy nhất trong kho `conTro` — không phải một ID nghiệp vụ, chỉ là hằng số
 * nội bộ để `put`/`get` luôn trúng đúng một chỗ. */
const KHOA_CON_TRO = 'chinh'

/** Trạng thái đồng bộ của máy này — MỞ RỘNG ĐƯỢC, xem chú thích đầu file. `epoch`/`soThuTu` là hai
 * trường lõi Task 2 đã biết trước (spec mục 4.5 — danh tính chuỗi số và vị trí trong chuỗi đó); các
 * trường khác do các task sau thêm vào qua `ghiConTro(kho, { truongMoi: ... })`. */
export type ConTro = {
  /** Danh tính của chuỗi `so_thu_tu` hiện tại — đổi khi máy chủ khôi phục hoặc dọn nhật ký (spec
   * mục 4.5). `null` nghĩa là máy này CHƯA từng nhận dữ liệu lần nào. */
  epoch: string | null
  /** Vị trí đã kéo tới trong chuỗi `hieu_luc` ứng với `epoch` hiện tại. */
  soThuTu: number
} & Record<string, unknown>

/** Giá trị mặc định khi kho `conTro` CHƯA có dòng nào (máy chưa từng đồng bộ lần nào). Chỉ gồm hai
 * trường lõi — các trường Task 4/6 thêm sau này không có mặt cho tới khi chính task đó ghi vào. */
const CON_TRO_MAC_DINH: ConTro = { epoch: null, soThuTu: 0 }

/** Đọc trạng thái đồng bộ hiện tại; trả về giá trị mặc định nếu kho chưa từng được ghi. */
export function docConTro(kho: IDBDatabase): Promise<ConTro> {
  return new Promise((resolve, reject) => {
    const yc = kho.transaction(KHO_CON_TRO, 'readonly').objectStore(KHO_CON_TRO).get(KHOA_CON_TRO)
    yc.onsuccess = () => resolve((yc.result as ConTro | undefined) ?? { ...CON_TRO_MAC_DINH })
    yc.onerror = () => reject(yc.error)
  })
}

/** Lõi dùng chung, TRÊN MỘT `IDBObjectStore` (`KHO_CON_TRO`) đã mở sẵn ở chế độ `readwrite` — không
 * tự mở/đóng giao dịch, để nơi gọi (Task 6: áp thao tác nhận về + tiến con trỏ + xoá khỏi hàng chờ
 * trong MỘT giao dịch — spec mục 5, ràng buộc 3) ghép được lệnh đọc-sửa-ghi này vào giao dịch của
 * chính nó, cùng khuôn mẫu với `xoaKhoiHangChoTrongGiaoDich`. Không tự `reject` khi đọc lỗi
 * (`baoLoi` để trống nếu nơi gọi đã có `onerror`/`onabort` riêng của giao dịch bao ngoài). */
export function ghiConTroTrongGiaoDich(
  storeConTro: IDBObjectStore,
  sua: Partial<ConTro>,
  baoLoi: (loi: unknown) => void,
): void {
  const ycDoc = storeConTro.get(KHOA_CON_TRO)
  ycDoc.onsuccess = () => {
    const hienTai = (ycDoc.result as ConTro | undefined) ?? { ...CON_TRO_MAC_DINH }
    storeConTro.put({ ...hienTai, ...sua }, KHOA_CON_TRO)
  }
  ycDoc.onerror = () => baoLoi(ycDoc.error)
}

/**
 * Ghi đè MỘT PHẦN dòng `conTro`: chỉ các trường có mặt trong `sua` bị thay, các trường khác (kể cả
 * trường do task khác thêm mà `sua` không biết tới) giữ nguyên. Đọc-sửa-ghi trong CÙNG một giao
 * dịch readwrite để không mất cập nhật nếu có ai gọi `ghiConTro` gần như đồng thời.
 *
 * Bản ĐỘC LẬP (tự mở giao dịch riêng) — dùng khi KHÔNG cần ghép chung giao dịch với thao tác nào
 * khác. Khi cần ghép chung (Task 6), dùng `ghiConTroTrongGiaoDich` trên store đã mở sẵn.
 */
export function ghiConTro(kho: IDBDatabase, sua: Partial<ConTro>): Promise<void> {
  return new Promise((resolve, reject) => {
    const gd = kho.transaction(KHO_CON_TRO, 'readwrite')
    ghiConTroTrongGiaoDich(gd.objectStore(KHO_CON_TRO), sua, () => {})

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không ghi được con trỏ'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch ghi con trỏ bị huỷ giữa chừng'))
  })
}
