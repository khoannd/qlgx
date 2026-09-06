import type { Page } from '@playwright/test'
import { expect } from '@playwright/test'
import { runtimeEnv } from '../runtimeEnv.ts'

/** Đăng nhập thật qua UI (không set localStorage tắt) — đúng tài khoản quản trị được
 * `globalSetup.ts` tạo bằng lệnh vận hành thật `dotnet run -- tao-tai-khoan-quan-tri`. */
export async function dangNhap(page: Page) {
  const env = runtimeEnv()
  await page.goto('/')
  await page.getByLabel('Tên đăng nhập').fill(env.taiKhoanQuanTri)
  await page.getByLabel('Mật khẩu').fill(env.matKhauQuanTri)
  await page.getByRole('button', { name: 'Đăng nhập' }).click()
  // Sau đăng nhập, App.tsx tự mở "Tổng quan" rồi "Danh sách gia đình" — chờ sidenav xuất hiện
  // là dấu hiệu chắc chắn đã vào được ứng dụng (khác hẳn còn ở màn hình đăng nhập).
  await expect(page.getByRole('button', { name: 'Danh sách giáo dân' })).toBeVisible()
}

/**
 * Tạo một giáo dân mới TỪ màn hình danh sách và đóng lại thẻ nháp "Giáo dân mới" ngay sau khi
 * lưu xong. Bước đóng thẻ KHÔNG chỉ là dọn dẹp cho gọn — nó SỬA một cái bẫy thật của bộ khung
 * tab (`TabDocs.tsx`): tab ẩn (`hidden`) vẫn giữ nguyên trong DOM thay vì gỡ hẳn, nên ngay sau
 * khi lưu, CẢ hai thẻ "Giáo dân mới" (thẻ nháp cũ, vẫn còn mở, chỉ ẩn) và "Giáo dân" (thẻ mới
 * mở, đang hiện) cùng tồn tại — và cả hai đều render cùng một cặp `id`/`<label for>` tĩnh
 * ("gd-hoten", "gd-ghichu", …). Trình duyệt phân giải `<label for="gd-ghichu">` theo ID ĐẦU
 * TIÊN khớp trong toàn tài liệu, BẤT KỂ ẩn hay hiện — nên `getByLabel('Ghi chú chung')` có thể
 * âm thầm trúng đúng ô nằm trong thẻ ẨN (thẻ nháp cũ), khiến "Đợi hiện rồi điền" treo mãi vì ô
 * đó không bao giờ hiện được. Đóng thẻ nháp cũ loại bỏ hẳn cặp ID trùng khỏi DOM, khiến
 * `getByLabel` chỉ còn đúng MỘT lựa chọn — chính ô đang hiện thật sự.
 */
export async function themGiaoDanMoi(page: Page, hoTen: string) {
  await page.getByRole('button', { name: 'Danh sách giáo dân' }).click()
  await page.getByRole('button', { name: 'Thêm giáo dân' }).click()
  await page.getByLabel('Họ tên', { exact: true }).fill(hoTen)
  await page.getByLabel('Ngày sinh', { exact: true }).fill('01/01/1990')
  await page.getByLabel('Ngày sinh', { exact: true }).blur()
  await page.getByRole('button', { name: 'Thêm giáo dân', exact: true }).click()
  await expect(page.getByRole('button', { name: 'Cập nhật' })).toBeVisible({ timeout: 15_000 })

  const theNhap = page.getByRole('tab', { name: /^Giáo dân mới/ })
  if (await theNhap.count() > 0) {
    await theNhap.getByRole('button', { name: '×' }).click()
  }
}

/**
 * Tạo một gia đình mới. KHÔNG có bước "đóng thẻ nháp" như `themGiaoDanMoi` — khác với giáo
 * dân, `GiaDinhDetailPage.taoMoi()` cố ý cập nhật `idThat` NGAY TRÊN CÙNG một thẻ tài liệu sau
 * khi tạo (xem chú thích gốc "không cần đổi route/thẻ tài liệu" trong file đó), không mở thẻ
 * mới — nên không có thẻ trùng/`id` trùng nào phải dọn. ĐÃ THỬ đóng thẻ "Gia đình mới" ở đây
 * (rập khuôn theo `themGiaoDanMoi`) và phát hiện lỗi thật: vì đó chính là thẻ ĐANG MỞ (không
 * phải một thẻ nháp cũ còn sót), đóng nó xoá luôn nội dung gia đình vừa tạo và nhảy về thẻ mở
 * gần nhất trước đó — hỏng cả bài test.
 */
export async function taoGiaDinhMoi(page: Page, tenGiaDinh: string) {
  await page.getByRole('button', { name: 'Danh sách gia đình' }).click()
  await page.getByRole('button', { name: 'Thêm gia đình' }).click()
  await page.getByLabel('Tên gia đình', { exact: true }).fill(tenGiaDinh)
  await page.getByRole('button', { name: 'Tạo gia đình' }).click()
  await expect(page.getByRole('button', { name: 'Thêm vào gia đình' })).toBeVisible({ timeout: 15_000 })
}

/** Tên duy nhất cho mỗi lần chạy test — tránh hai lần chạy song song/liên tiếp giẫm dữ liệu
 * lên nhau, và giúp dò đúng dòng vừa tạo trên lưới bằng ô lọc theo cột (đến giữa bộ test,
 * lưới có thể có vài chục dòng do các test khác tạo ra, không được đoán "dòng đầu tiên"). */
export function tenNgauNhien(tienTo: string): string {
  return `${tienTo} ${Date.now()}_${Math.random().toString(36).slice(2, 8)}`
}
