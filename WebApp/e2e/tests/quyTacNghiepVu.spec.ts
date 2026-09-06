import { test, expect } from '@playwright/test'
import { dangNhap, taoGiaDinhMoi, tenNgauNhien, themGiaoDanMoi } from './dangNhap.ts'

test.describe('Vi phạm quy tắc nghiệp vụ hiện thông báo tiếng Việt, không phải lỗi 500', () => {
  test('Xoá vĩnh viễn một giáo dân đang thuộc gia đình bị chặn, kèm lý do tiếng Việt', async ({ page }) => {
    await dangNhap(page)

    const tenGiaoDan = tenNgauNhien('E2E Chan xoa')
    await themGiaoDanMoi(page, tenGiaoDan)

    const tenGiaDinh = tenNgauNhien('E2E Gia dinh chan')
    await taoGiaDinhMoi(page, tenGiaDinh)

    await page.locator('#gdinh-them-thanhvien').getByTitle('Chọn từ danh sách giáo dân').click()
    await page.getByPlaceholder('Gõ tên hoặc mã cũ để tìm…').fill(tenGiaoDan)
    await page.getByRole('button', { name: new RegExp(tenGiaoDan) }).click()
    await page.getByRole('button', { name: 'Thêm vào gia đình' }).click()
    await expect(page.getByRole('gridcell', { name: tenGiaoDan })).toBeVisible({ timeout: 15_000 })

    // Giờ thử xoá VĨNH VIỄN giáo dân này từ màn hình Danh sách giáo dân — máy chủ phải CHẶN
    // (409 Conflict, xem GiaoDanEndpoints.cs) và trả lý do tiếng Việt, KHÔNG phải lỗi 500/
    // trang trắng.
    // "Danh sách giáo dân" đã mở từ trước (bước đầu của `themGiaoDanMoi`) và lưới chỉ tải một
    // lần lúc mount — bấm "Tải lại" để chắc chắn thấy đúng dữ liệu mới nhất.
    await page.getByRole('button', { name: 'Danh sách giáo dân' }).click()
    await page.getByRole('button', { name: 'Tải lại' }).click()
    await page.getByRole('gridcell', { name: tenGiaoDan }).first().click()
    await page.getByRole('button', { name: 'Xóa giáo dân' }).click()
    await expect(page.getByRole('alertdialog')).toBeVisible()
    await page.getByRole('button', { name: 'Xóa vĩnh viễn' }).click()

    const thongBaoLoi = page.getByRole('alertdialog').getByRole('alert')
    await expect(thongBaoLoi).toBeVisible({ timeout: 15_000 })
    await expect(thongBaoLoi).toContainText('Vui lòng xóa giáo dân ra khỏi gia đình trước khi xóa giáo dân này')
    await expect(thongBaoLoi).toContainText(tenGiaDinh)

    // Ứng dụng vẫn sống — hộp thoại còn nguyên, dòng vẫn còn trên lưới (không bị xoá một nửa).
    await expect(page.getByRole('alertdialog')).toBeVisible()
    await page.getByRole('button', { name: 'Hủy bỏ' }).click()
    await expect(page.getByRole('gridcell', { name: tenGiaoDan })).toBeVisible()
  })
})
