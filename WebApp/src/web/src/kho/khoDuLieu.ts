/**
 * `ghiVaXepHang` — điểm ghi DUY NHẤT nối kho hiển thị (`banGhi`) với hàng chờ (`hangCho`).
 *
 * ĐÂY LÀ HÀM QUAN TRỌNG NHẤT VỀ AN TOÀN DỮ LIỆU của cả kế hoạch offline-first (spec mục 5, ràng
 * buộc 2). Nếu ghi bản ghi vào kho hiển thị và thêm dòng hàng chờ là HAI giao dịch IndexedDB riêng
 * biệt, mất điện xen giữa hai giao dịch đó tạo ra trạng thái TỆ NHẤT có thể: dữ liệu đã HIỆN trên
 * màn hình, thanh trạng thái đã XANH, nhưng không có dòng hàng chờ nào tương ứng — nghĩa là KHÔNG
 * AI gửi nó lên máy chủ, và nó sẽ BIẾN MẤT một cách im lặng ở lần nhận dữ liệu mới kế tiếp (dữ liệu
 * máy chủ chưa từng có sẽ ghi đè lên dữ liệu chỉ tồn tại trên màn hình).
 *
 * Vì vậy hai lệnh `put()` dưới đây PHẢI nằm trong CÙNG MỘT giao dịch (`kho.transaction([...], ...)`
 * gọi một lần duy nhất) — KHÔNG được tách thành hai lời gọi `kho.transaction(...)` riêng, kể cả khi
 * "trông có vẻ" chạy nối tiếp nhau. Bài kiểm thử `khoDuLieu.test.ts` ép giao dịch abort ngay sau
 * khi gửi lệnh ghi hàng chờ để chứng minh: hỏng giữa chừng thì CẢ HAI cùng không có gì (IndexedDB tự
 * cuộn ngược mọi thao tác trong một giao dịch bị abort) — không có kiểu "nửa vời" nào tồn tại được.
 */
import { KHO_BAN_GHI, KHO_HANG_CHO } from './moKho'
import { voiKhoaKeTiep, type DongHangCho } from './hangCho'

/** Một bản ghi cần ghi vào kho hiển thị (`banGhi`). Kho này không có `keyPath` (khoá ngoài dòng —
 * xem `moKho.ts`), nên phải mang khoá tường minh khi `put()`. Gợi ý đặt `khoa` dạng
 * `${bang}:${idBanGhi}` để không lẫn giữa các bảng nghiệp vụ khác nhau trong cùng một kho hiển thị
 * — quyết định định dạng cụ thể để dành cho tầng gọi (Task 6, 7), file này chỉ cần một khoá bất kỳ
 * là chuỗi hoặc số hợp lệ với IndexedDB. */
export type BanGhiKho = {
  khoa: IDBValidKey
  giaTri: unknown
}

/**
 * Ghi `banGhi` vào kho hiển thị VÀ thêm `thaoTac` vào hàng chờ trong MỘT giao dịch IndexedDB duy
 * nhất. `thaoTac.doan` mặc định `0` nếu không truyền (xem `hangCho.ts` — Task 4 sẽ gán số đoạn thật
 * theo độ lệch đồng hồ đo được lúc ghi).
 */
export function ghiVaXepHang(kho: IDBDatabase, banGhi: BanGhiKho, thaoTac: Omit<DongHangCho, 'doan'> & { doan?: number }): Promise<void> {
  return new Promise((resolve, reject) => {
    // MỘT giao dịch bao cả hai kho con — đây chính là ràng buộc sống còn của hàm này, xem chú
    // thích đầu file. Không được gọi `kho.transaction(...)` lần thứ hai ở bất cứ đâu trong hàm.
    const gd = kho.transaction([KHO_BAN_GHI, KHO_HANG_CHO], 'readwrite')
    const khoHangCho = gd.objectStore(KHO_HANG_CHO)

    gd.objectStore(KHO_BAN_GHI).put(banGhi.giaTri, banGhi.khoa)

    // `voiKhoaKeTiep` gọi `xuLy` ĐỒNG BỘ trong `onsuccess` của con trỏ — giữ lệnh `put()` hàng chờ
    // trong CÙNG giao dịch `gd` đã mở ở trên (xem chú thích trong `hangCho.ts`). Không tự `reject`
    // khi con trỏ lỗi (`baoLoi` để trống) — để `gd.onerror`/`gd.onabort` xử lý thống nhất, vì lỗi ở
    // bất kỳ yêu cầu nào trong giao dịch cũng khiến cả giao dịch abort (đúng thứ cần kiểm thử).
    // `add()` thay vì `put()`: khoá `khoaKeTiep` do `voiKhoaKeTiep` tính (khoá lớn nhất + 1) LUÔN
    // phải là khoá MỚI, chưa từng có trong kho. Nếu một lỗi tính toán nào đó (ví dụ trong lúc sửa
    // `voiKhoaKeTiep` sau này) khiến hai lần ghi trùng khoá, `add()` sẽ khiến giao dịch abort ngay
    // (bảo vệ dữ liệu — báo lỗi rõ ràng) thay vì `put()` âm thầm GHI ĐÈ mất một việc đang chờ gửi.
    voiKhoaKeTiep(
      khoHangCho,
      (khoaKeTiep) => khoHangCho.add({ ...thaoTac, doan: thaoTac.doan ?? 0 }, khoaKeTiep),
      () => {},
    )

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không ghi được bản ghi và xếp hàng chờ'))
    gd.onabort = () =>
      reject(gd.error ?? new Error('Giao dịch ghi bản ghi và xếp hàng chờ bị huỷ giữa chừng — cả hai đều không được ghi'))
  })
}
