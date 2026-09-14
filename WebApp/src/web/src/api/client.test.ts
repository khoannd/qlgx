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

  it('navigator.onLine=true (may chu chua chay) thi bao cau chu binh dan, khong lo duong dan API', async () => {
    vi.stubGlobal('navigator', { onLine: true })
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')))

    // Câu hiển thị cho người dùng KHÔNG được nhắc "Qlgx.Api" hay đường dẫn API — người trực
    // văn phòng giáo xứ không hiểu đó là gì (UX review 2026-09-08 mục 2). Chi tiết kỹ thuật đó
    // vẫn còn trong console.error, chỉ không lộ ra chuỗi lỗi hiển thị trên màn hình.
    await expect(api.giaoHo.danhMuc()).rejects.toThrow(
      'Không kết nối được máy chủ. Vui lòng thử lại sau ít phút hoặc báo cho người quản trị.',
    )
  })
})

describe('api.saoLuu', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('taoCongViec goi dung duong dan va phuong thuc POST', async () => {
    const fetchGia = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ id: 'abc' }), { status: 200 }))
    vi.stubGlobal('fetch', fetchGia)

    await api.saoLuu.taoCongViec({ loai: 'sao_luu' })

    const [duongDan, tuyChon] = fetchGia.mock.calls[0]!
    expect(String(duongDan)).toContain('/api/sao-luu/cong-viec')
    expect(tuyChon.method).toBe('POST')
  })

  it('duongDanTaiVe gan ma cong viec vao duong dan', () => {
    expect(api.saoLuu.duongDanTaiVe('ma-1')).toBe('/api/sao-luu/tai-ve/ma-1')
  })
})
