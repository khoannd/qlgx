/**
 * Chặn đóng tab khi hàng chờ chưa rỗng — spec 6.6 (chế độ TẮT offline), Task 7.
 *
 * `beforeunload` là sự kiện ĐỒNG BỘ: trình duyệt không đợi một `Promise` để quyết định có chặn hay
 * không (đọc hàng chờ qua `demHangCho`/IndexedDB là BẤT ĐỒNG BỘ). Vì vậy hàm này KHÔNG tự đọc kho —
 * nó nhận một hàm `laySoHangCho` ĐỒNG BỘ (đọc một bộ đếm giữ sẵn trong bộ nhớ, do tầng gọi tự cập
 * nhật mỗi khi ghi thêm/xoá bớt hàng chờ — ví dụ qua `demHangCho()` gọi lại định kỳ, hoặc tăng/giảm
 * ngay tại chỗ gọi `ghiCucBo`/khi bộ đồng bộ xác nhận đã gửi xong) — tách trách nhiệm "giữ con số mới
 * nhất" ra khỏi file này, để file này chỉ làm đúng một việc: đăng ký/gỡ listener.
 *
 * CHỈ có Ý NGHĨA ở chế độ TẮT offline (spec 6.6: dữ liệu chỉ nằm trong RAM của tab — xem
 * `moKhoRam.ts`, mất hết khi tab đóng) — ở chế độ offline BÌNH THƯỜNG (IndexedDB thật), đóng tab
 * KHÔNG làm mất hàng chờ (đã ghi bền xuống đĩa, bộ đồng bộ sẽ gửi tiếp ở lần mở app kế tiếp), nên
 * KHÔNG cần chặn gì cả — tầng gọi (màn hình/App shell) tự quyết định có đăng ký hàm này hay không tuỳ
 * theo tab đang chạy ở chế độ nào, file này không tự biết và không rẽ nhánh theo chế độ.
 */

/**
 * Đăng ký chặn đóng tab khi `laySoHangCho() > 0`. Trả về một hàm HUỶ đăng ký (gọi khi tắt chế độ tắt
 * offline, hoặc khi unmount thành phần quản lý chế độ này).
 *
 * Trình duyệt hiện đại KHÔNG còn hiển thị chuỗi tuỳ biến trong hộp thoại xác nhận (bỏ vì lý do bảo
 * mật — trang web có thể lừa người dùng bằng thông báo giả) — gọi `preventDefault()` VÀ gán
 * `returnValue` (cách cũ, một số trình duyệt vẫn cần cả hai) là đủ để trình duyệt tự hiện hộp thoại
 * xác nhận CHUẨN của chính nó, không cần/không thể tự viết nội dung.
 *
 * `returnValue` PHẢI là một giá trị KHÁC RỖNG (đặc tả HTML: `returnValue = ''` không kích hoạt hộp
 * thoại — chỉ giá trị "truthy" mới có tác dụng) — nhiều máy phòng xứ chạy trình duyệt/Windows cũ
 * (Chrome/Edge < 119 còn cần đúng `returnValue`, không chỉ `preventDefault()`), nên KHÔNG được để
 * chuỗi rỗng dù các trình duyệt mới nhất có thể tha thứ được lỗi này.
 */
export function chanDongTabKhiConHangCho(laySoHangCho: () => number): () => void {
  if (typeof window === 'undefined') return () => {}

  const xuLy = (e: BeforeUnloadEvent): void => {
    if (laySoHangCho() > 0) {
      e.preventDefault()
      e.returnValue = true
    }
  }

  window.addEventListener('beforeunload', xuLy)
  return () => window.removeEventListener('beforeunload', xuLy)
}
