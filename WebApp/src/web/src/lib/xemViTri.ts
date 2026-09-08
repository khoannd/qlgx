/**
 * "Xem vị trí" (menu chuột phải `GxGiaoDanList.tsx`/`GxGiaDinhList.tsx`) — tương đương
 * `Memory.ViewMap(string address)` của bản desktop (`Source/DBAccess/CMemory.cs` dòng
 * 1732-1745):
 *
 * ```csharp
 * public static void ViewMap(string address) {
 *     var root = "https://www.google.com/maps/search/";
 *     if (!string.IsNullOrEmpty(address.Trim()))
 *         Process.Start(string.Concat(root, HttpUtility.UrlEncode(address.Trim())));
 * }
 * ```
 *
 * `GxGiaDinhList.ViewMapFamily()`/`GxGiaoDanList.ViewMapGiaoDan()` gọi hàm này với đúng
 * chuỗi Địa chỉ THÔ của dòng đang chọn (không tách/chuẩn hoá gì thêm) — bản web migrate Y HỆT:
 * mở `https://www.google.com/maps/search/<địa chỉ đã mã hoá URL>` ở tab mới.
 *
 * CẢNH BÁO đã ghi vào can-review-sau.md: đây là gửi dữ liệu địa chỉ giáo dân/gia đình (thông
 * tin cá nhân) ra một dịch vụ bên thứ ba (Google) qua trình duyệt của người dùng — bản desktop
 * làm vậy (`Process.Start` mở trình duyệt máy khách) nên bản web tái hiện nguyên vẹn (nhiệm vụ
 * này không được "âm thầm sửa cho đúng"), nhưng người dùng cần biết trước khi bấm.
 *
 * Khác `window.open` thẳng: dùng `window.alert` khi địa chỉ rỗng, đúng hộp thoại
 * "...không có địa chỉ để xem bản đồ" của bản gốc (`GxGiaDinhList.cs:1093`,
 * `GxGiaoDanList.cs:911`) thay vì mở tab trống.
 */
export function moBanDo(diaChi: string | null | undefined, thongBaoRong: string): void {
  const daCat = (diaChi ?? '').trim()
  if (!daCat) {
    window.alert(thongBaoRong)
    return
  }
  window.open('https://www.google.com/maps/search/' + encodeURIComponent(daCat), '_blank', 'noopener,noreferrer')
}
