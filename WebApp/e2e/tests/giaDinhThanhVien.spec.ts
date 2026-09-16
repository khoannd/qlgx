import { test, expect } from '@playwright/test'
import { dangNhap, taoGiaDinhMoi, tenNgauNhien, themGiaoDanMoi } from './dangNhap.ts'

test.describe('Gia đình — thêm/xoá thành viên qua API thật', () => {
  test('Mở gia đình mới tạo, thêm một thành viên rồi xoá khỏi gia đình', async ({ page }) => {
    await dangNhap(page)

    // 1) Tạo một giáo dân riêng để làm "thành viên" thêm vào gia đình — picker tìm theo tên
    // qua GET /api/giao-dan/tim, cần một cái tên đủ lạ để không lẫn với ai khác.
    const tenThanhVien = tenNgauNhien('E2E Thanh vien')
    await themGiaoDanMoi(page, tenThanhVien)

    // 2) Tạo một gia đình mới.
    const tenGiaDinh = tenNgauNhien('E2E Gia dinh')
    await taoGiaDinhMoi(page, tenGiaDinh)

    // 3) Thêm thành viên: mở picker, gõ tên, chọn đúng người, chọn vai trò rồi bấm thêm.
    await page.locator('#gdinh-them-thanhvien').getByTitle('Chọn từ danh sách giáo dân').click()
    await page.getByPlaceholder('Gõ tên hoặc mã cũ để tìm…').fill(tenThanhVien)
    await page.getByRole('button', { name: new RegExp(tenThanhVien) }).click()
    await page.getByRole('button', { name: 'Thêm vào gia đình' }).click()

    // Thành viên xuất hiện trên lưới "Thành viên khác trong gia đình" — dùng `gridcell`, KHÔNG
    // `getByText`: tiêu đề <h1> của thẻ "chi tiết giáo dân" tạo ở bước 1 (còn mở phía sau, chỉ
    // ẩn chứ không unmount) chứa đúng chuỗi này và bị Playwright khớp nhầm nếu dùng getByText.
    const dongThanhVien = page.getByRole('gridcell', { name: tenThanhVien })
    await expect(dongThanhVien).toBeVisible({ timeout: 15_000 })

    // 4) Xoá khỏi gia đình qua menu chuột phải của lưới thành viên. DÙNG `dispatchEvent`, KHÔNG
    // phải click chuột phải thật (`page.mouse`/`locator.click({button:'right'})`) — đã xác
    // nhận thanh "Thêm thành viên" (picker + nút) đứng NGAY TRÊN lưới này trong bố cục và Chrome
    // luôn định tuyến sự kiện chuột THẬT theo toạ độ màn hình cho bất cứ thứ gì vẽ đè lên trên,
    // dù `force: true` cũng không giúp gì (nó chỉ bỏ qua bước Playwright TỰ kiểm tra, không đổi
    // cách Chrome bắt sự kiện chuột thật). Bắn thẳng sự kiện DOM "contextmenu" lên đúng phần tử
    // né hoàn toàn việc dò toạ độ — khớp đúng cách `GxGrid.tsx` đọc sự kiện (bắt ở ancestor
    // `.grid-wrap`, dò `e.target.closest('.ag-row')`, không cần toạ độ chuột thật).
    await dongThanhVien.dispatchEvent('contextmenu', { bubbles: true, cancelable: true })
    await page.getByRole('button', { name: 'Xoá khỏi gia đình' }).click()
    // "Xoá khỏi gia đình" chỉ MỞ một hộp thoại xác nhận (GxHoiDap, kiểu Yes/No) — đúng
    // `gxAddEdit1_DeleteClick` gốc, chưa xoá thật cho tới khi bấm "Yes".
    await page.getByRole('button', { name: 'Yes' }).click()
    await expect(page.getByRole('gridcell', { name: tenThanhVien })).toHaveCount(0, { timeout: 15_000 })

    // Tải lại để chắc chắn việc xoá đã ghi xuống CSDL, không chỉ còn trên state React.
    await page.reload()
    await expect(page.getByRole('button', { name: 'Danh sách gia đình' })).toBeVisible({ timeout: 15_000 })
    await page.getByRole('gridcell', { name: tenGiaDinh }).first().dblclick()
    await expect(page.getByRole('gridcell', { name: tenThanhVien })).toHaveCount(0)
  })
})
