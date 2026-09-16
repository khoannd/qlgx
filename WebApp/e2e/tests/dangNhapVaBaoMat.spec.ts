import { test, expect } from '@playwright/test'
import { runtimeEnv } from '../runtimeEnv.ts'
import { dangNhap } from './dangNhap.ts'

test.describe('Đăng nhập và bảo vệ phiên', () => {
  test('Chưa đăng nhập thì bị chuyển về màn hình đăng nhập', async ({ page }) => {
    // Context test của Playwright luôn TRẮNG (không localStorage/cookie từ test khác) —
    // vào thẳng "/" mà không đăng nhập đúng kịch bản "chưa đăng nhập".
    await page.goto('/')
    await expect(page.getByLabel('Tên đăng nhập', { exact: true })).toBeVisible()
    await expect(page.getByLabel('Mật khẩu', { exact: true })).toBeVisible()
    // Không thấy bất kỳ màn hình nghiệp vụ nào (sidenav) khi chưa đăng nhập.
    await expect(page.getByRole('button', { name: 'Danh sách giáo dân' })).toHaveCount(0)
  })

  test('Sai mật khẩu hiện thông báo tiếng Việt, không vào được ứng dụng', async ({ page }) => {
    const env = runtimeEnv()
    await page.goto('/')
    await page.getByLabel('Tên đăng nhập', { exact: true }).fill(env.taiKhoanQuanTri)
    await page.getByLabel('Mật khẩu', { exact: true }).fill('mat-khau-sai-chac-chan')
    await page.getByRole('button', { name: 'Đăng nhập' }).click()
    await expect(page.getByRole('alert')).toContainText(/không chính xác/i)
    await expect(page.getByLabel('Tên đăng nhập', { exact: true })).toBeVisible()
  })

  test('Token bị xoá (hết hạn/đăng xuất) thì tải lại trang quay về màn hình đăng nhập', async ({ page }) => {
    await dangNhap(page)
    // Mô phỏng đúng ca "chưa đăng nhập" xảy ra giữa phiên làm việc (token hết hạn 8 tiếng —
    // Task 14 — hoặc bị xoá) — KHÔNG có localStorage hợp lệ nào lúc tải lại trang.
    await page.evaluate(() => localStorage.clear())
    await page.reload()
    await expect(page.getByLabel('Tên đăng nhập', { exact: true })).toBeVisible({ timeout: 10_000 })
  })
})
