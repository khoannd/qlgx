import { afterEach, describe, expect, it, vi } from 'vitest'
import { api } from './client'
import { authStore } from './authStore'

describe('client.goi — lỗi mạng (Task 16, phân biệt mất mạng thật vs máy chủ chưa chạy)', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
    authStore.xoaToken()
  })

  it('navigator.onLine=false thi bao "Mat ket noi mang", tran an du lieu khong mat', async () => {
    vi.stubGlobal('navigator', { onLine: false })
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))

    await expect(api.giaoHo.danhMuc()).rejects.toThrow(
      'Mất kết nối mạng. Dữ liệu bạn đã nhập vẫn được giữ nguyên trên máy — hãy thử lại khi có mạng.',
    )
  })

  it('navigator.onLine=true (may chu chua chay) thi bao cau chu danh cho dev', async () => {
    vi.stubGlobal('navigator', { onLine: true })
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))

    await expect(api.giaoHo.danhMuc()).rejects.toThrow(/Kiểm tra Qlgx\.Api đã chạy chưa/)
  })
})
