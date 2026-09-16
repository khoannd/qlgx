/**
 * Bản "RAM" của lớp lưu trữ — spec 6.6 (chế độ TẮT offline), Task 7. Dùng lại NGUYÊN VẸN `moKho()`
 * (Task 1) — chỉ tiêm một `IDBFactory` khác, KHÔNG viết lại schema/logic mở kho: đúng ý spec 6.6
 * "cùng một đường ghi, chỉ khác cái hộp đựng" — mọi phần khó (`ghiVaXepHang`, hàng chờ, sổ đã nhận,
 * bộ đồng bộ...) chạy Y HỆT trên cả hai lớp lưu trữ, không rẽ nhánh logic nào ngoài chỗ NÀY.
 *
 * Factory dùng ở đây là `IDBFactory` của gói `fake-indexeddb` — một cài đặt IndexedDB THẬT (đúng
 * đặc tả W3C, không phải một object giả tối giản), chỉ khác chỗ nó giữ dữ liệu HOÀN TOÀN TRONG BỘ
 * NHỚ tiến trình, không đụng gì tới đĩa/`indexedDB` thật của trình duyệt. Vì vậy:
 *
 * - Dữ liệu chỉ sống trong RAM của tab hiện tại — mất sạch khi tab đóng/tải lại. Đây CHÍNH XÁC là ý
 *   nghĩa "tắt offline": không có gì được lưu bền, người dùng không thể vô tình "tưởng đã lưu" dữ
 *   liệu mà thật ra chỉ nằm trong bộ nhớ tạm của phiên làm việc hiện tại.
 * - `fake-indexeddb` trước đây CHỈ là devDependency (dùng để giả lập IndexedDB cho jsdom trong test,
 *   xem các file `*.test.ts` cùng thư mục) — Task 7 chuyển nó sang dependency PRODUCTION thật (xem
 *   `package.json`, GHIM đúng phiên bản — không dùng dải `^` — vì đây giờ là một cam kết runtime,
 *   không chỉ một công cụ test có thể tự do lên phiên bản mới).
 * - **IMPORT ĐỘNG** (`await import('fake-indexeddb')`), KHÔNG import tĩnh ở đầu file: nếu import
 *   tĩnh, MỌI người dùng (kể cả người chưa từng bật chế độ tắt offline) đều tải cả gói này trong
 *   chunk chính của ứng dụng — chỉ người THỰC SỰ gọi `moKhoRam()` (bật chế độ tắt offline) mới cần
 *   tải nó. Xem chú thích trong `moKho()` (Task 1) — tham số `factory` của nó nhận kiểu `IDBFactory`
 *   CHUẨN CỦA DOM (không phải kiểu riêng của `fake-indexeddb`) nên không có gì buộc phải import kiểu
 *   đó ra ngoài — `fake-indexeddb`'s `IDBFactory` cài đặt ĐÚNG giao diện DOM này (đó là mục đích tồn
 *   tại của gói), nên gán trực tiếp được mà không cần khai kiểu riêng ở đây.
 *
 * Mỗi lời gọi `moKhoRam()` KHÔNG truyền `factory` sẽ tự tạo một factory MỚI (bộ nhớ hoàn toàn riêng,
 * không chia sẻ với factory nào khác, kể cả một lần gọi `moKhoRam()` khác trong cùng tab) — đúng ý
 * "một hộp đựng mới mỗi khi cần", và cho phép kiểm thử tự tiêm một factory dùng chung để xác nhận
 * hai lần mở với CÙNG factory thấy CÙNG dữ liệu (giống cách `moKho()` thật hai lần mở cùng tên kho
 * vẫn thấy dữ liệu cũ).
 */
import { moKho, PHIEN_BAN_KHO } from './moKho'

/** Tên kho mặc định cho bản RAM — khác tên mặc định của `moKho()` thật (`'qlgx'`) chỉ để dễ phân
 * biệt khi gỡ lỗi (hai factory đã tách biệt hoàn toàn nên trùng tên không gây lẫn dữ liệu thật). */
const TEN_KHO_RAM_MAC_DINH = 'qlgx-ram'

export async function moKhoRam(
  tenKho: string = TEN_KHO_RAM_MAC_DINH,
  phienBan: number = PHIEN_BAN_KHO,
  factory?: IDBFactory,
): Promise<IDBDatabase> {
  const factoryThucTe = factory ?? new (await import('fake-indexeddb')).IDBFactory()
  return moKho(tenKho, phienBan, factoryThucTe)
}
