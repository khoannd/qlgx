import { test, expect } from '@playwright/test'
import { dangNhap, tenNgauNhien, themGiaoDanMoi } from './dangNhap.ts'

test.describe('Giáo dân — luồng đầu-cuối qua API thật', () => {
  test('Đăng nhập, thêm giáo dân, sửa rồi lưu, tải lại thấy giá trị mới', async ({ page }) => {
    await dangNhap(page)

    const hoTen = tenNgauNhien('E2E Giao dan')
    await themGiaoDanMoi(page, hoTen)

    // Sửa "Ghi chú chung" rồi lưu — trường tự do, không vướng quy tắc nghiệp vụ nào.
    const ghiChuMoi = `Ghi chu sua boi e2e ${Date.now()}`
    await page.getByLabel('Ghi chú chung', { exact: true }).fill(ghiChuMoi)
    await page.getByRole('button', { name: 'Cập nhật' }).click()
    await expect(page.getByText(/Đã lưu|Cập nhật thành công|thành công/i)).toBeVisible({ timeout: 15_000 })

    // Tải lại toàn bộ trang (không chỉ điều hướng trong SPA) — đúng yêu cầu "tải lại thấy giá
    // trị mới", chứng minh giá trị đã thật sự nằm trong CSDL chứ không chỉ còn trong state React.
    await page.reload()
    await expect(page.getByRole('button', { name: 'Danh sách giáo dân' })).toBeVisible({ timeout: 15_000 })
    await page.getByRole('button', { name: 'Danh sách giáo dân' }).click()
    // `getByRole('gridcell', …)` — KHÔNG dùng `getByText` ở đây: tiêu đề <h1> của thẻ "chi
    // tiết" vừa tạo (còn mở phía sau) cũng chứa đúng chuỗi này, gây khớp nhầm phần tử ẨN.
    await page.getByRole('gridcell', { name: hoTen }).first().dblclick()
    await expect(page.getByLabel('Họ tên', { exact: true })).toHaveValue(hoTen, { timeout: 15_000 })
    await expect(page.getByLabel('Ghi chú chung', { exact: true })).toHaveValue(ghiChuMoi)
  })

  test('Tạo giáo dân mới, thấy trong danh sách, rồi xoá', async ({ page }) => {
    await dangNhap(page)

    const hoTen = tenNgauNhien('E2E Xoa')
    await themGiaoDanMoi(page, hoTen)

    // "Danh sách giáo dân" đã mở TRƯỚC KHI tạo giáo dân này (bước đầu của `themGiaoDanMoi`) và
    // lưới chỉ tải dữ liệu MỘT LẦN lúc mount — quay lại thẻ list không tự tải lại. Bấm "Tải
    // lại" để chắc chắn lưới phản ánh đúng dữ liệu mới nhất từ máy chủ, không đoán lưới đã cũ.
    await page.getByRole('button', { name: 'Danh sách giáo dân' }).click()
    await page.getByRole('button', { name: 'Tải lại' }).click()
    const dong = page.getByRole('gridcell', { name: hoTen }).first()
    await expect(dong).toBeVisible({ timeout: 15_000 })

    await dong.click() // chon dong — mo nut "Xóa giáo dân" (needSel)
    await page.getByRole('button', { name: 'Xóa giáo dân' }).click()
    await expect(page.getByRole('alertdialog')).toBeVisible()
    await page.getByRole('button', { name: 'Xóa vĩnh viễn' }).click()

    await expect(page.getByRole('gridcell', { name: hoTen })).toHaveCount(0, { timeout: 15_000 })
  })
})
