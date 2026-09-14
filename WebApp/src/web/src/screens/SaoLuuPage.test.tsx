import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  SaoLuuPage, nhanNguon, dinhDangKichThuoc, nhanLoaiCongViec, nhanTrangThaiCongViec,
} from './SaoLuuPage'
import { api } from '../api/client'

vi.mock('../api/client', () => ({
  api: { saoLuu: {
    tinhTrang: vi.fn(), danhSach: vi.fn(), congViecGanDay: vi.fn(),
    taoCongViec: vi.fn(), congViec: vi.fn(),
    duongDanTaiVe: (id: string) => `/api/sao-luu/tai-ve/${id}`,
    taiBanSaoVe: vi.fn(),
  } },
}))

const tinhTrangXanh = {
  den: 'xanh' as const, saoLuuGanNhat: '2026-09-13T06:00:00Z', soBanSao: 47,
  dienTapGanNhat: '2026-09-08T03:00:00Z', dienTapDat: true, loiGanNhat: null,
}
const motBanSao = {
  id: 'ab12cd34', thoiDiem: '2026-09-13T06:00:00Z', nhan: 'tu-dong',
  kichThuocByte: 12_582_912, soGiaoDan: 2050, soGiaDinh: 40, nguon: 'tu_dong' as const,
}

beforeEach(() => {
  vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(tinhTrangXanh)
  vi.mocked(api.saoLuu.danhSach).mockResolvedValue([motBanSao])
  vi.mocked(api.saoLuu.congViecGanDay).mockResolvedValue([])
  vi.mocked(api.saoLuu.taiBanSaoVe).mockResolvedValue(undefined)
})

afterEach(() => {
  vi.useRealTimers()
})

describe('dinhDangKichThuoc', () => {
  it('doi byte sang don vi doc duoc', () => {
    expect(dinhDangKichThuoc(0)).toBe('0 B')
    expect(dinhDangKichThuoc(12_582_912)).toBe('12,0 MB')
  })
})

describe('nhanNguon', () => {
  it('dich du bon nguon sang tieng Viet', () => {
    expect(nhanNguon('tu_dong')).toBe('Tự động')
    expect(nhanNguon('thu_cong')).toBe('Thủ công')
    expect(nhanNguon('truoc_cap_nhat')).toBe('Trước cập nhật')
    expect(nhanNguon('truoc_phuc_hoi')).toBe('Trước phục hồi')
  })
})

describe('nhanLoaiCongViec', () => {
  it('dich du sau loai cong viec sang tieng Viet', () => {
    expect(nhanLoaiCongViec('sao_luu')).toBe('Sao lưu')
    expect(nhanLoaiCongViec('phuc_hoi')).toBe('Phục hồi')
    expect(nhanLoaiCongViec('kiem_tra')).toBe('Kiểm tra')
    expect(nhanLoaiCongViec('dien_tap')).toBe('Diễn tập')
    expect(nhanLoaiCongViec('tai_ve')).toBe('Tải về')
    expect(nhanLoaiCongViec('dong_bo_danh_sach')).toBe('Đồng bộ danh sách')
  })
})

describe('nhanTrangThaiCongViec', () => {
  it('dich du bon trang thai sang tieng Viet', () => {
    expect(nhanTrangThaiCongViec('cho')).toBe('Chờ')
    expect(nhanTrangThaiCongViec('dang_chay')).toBe('Đang chạy')
    expect(nhanTrangThaiCongViec('xong')).toBe('Xong')
    expect(nhanTrangThaiCongViec('loi')).toBe('Lỗi')
  })
})

describe('SaoLuuPage', () => {
  it('hien den xanh va so ban sao', async () => {
    render(<SaoLuuPage />)
    expect(await screen.findByText(/Bình thường/)).toBeDefined()
    expect(screen.getByText(/47 bản sao/)).toBeDefined()
  })

  it('hien thoi diem theo dd/MM/yyyy HH:mm, KHONG phai ISO', async () => {
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)
    expect(screen.queryByText(/2026-09-13T/)).toBeNull()
    expect(screen.getAllByText(/13\/09\/2026 \d{2}:\d{2}/).length).toBeGreaterThan(0)
  })

  it('den do hien canh bao noi bat', async () => {
    vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(
      { ...tinhTrangXanh, den: 'do', loiGanNhat: 'restic check that bai' })
    render(<SaoLuuPage />)
    expect(await screen.findByText(/restic check that bai/)).toBeDefined()
  })

  it('bam "Sao luu ngay" thi tao cong viec loai sao_luu', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-1' })
    vi.mocked(api.saoLuu.congViec).mockResolvedValue({
      id: 'job-1', loai: 'sao_luu', trangThai: 'xong', buocHienTai: null,
      nhatKy: null, taoLuc: '2026-09-13T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    })
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))

    await waitFor(() => expect(api.saoLuu.taoCongViec)
      .toHaveBeenCalledWith(expect.objectContaining({ loai: 'sao_luu' })))
  })

  it('bao loi ro rang khi tao cong viec that bai, khong im lang', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockRejectedValue(new Error('Đang có một công việc chạy dở'))
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))

    expect(await screen.findByText(/Đang có một công việc chạy dở/)).toBeDefined()
  })

  it('canh bao ro rang ngay tai nut tai ve rang tep chua ma hoa', async () => {
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)
    expect(screen.getByText(/chưa mã hoá/i)).toBeDefined()
  })

  it('nhat ky cong viec gan day hien nhan tieng Viet, khong phai ma tho', async () => {
    vi.mocked(api.saoLuu.congViecGanDay).mockResolvedValue([{
      id: 'job-cu', loai: 'sao_luu', trangThai: 'xong', buocHienTai: null,
      nhatKy: null, taoLuc: '2026-09-12T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    }])
    render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    await userEvent.click(screen.getByText(/Nhật ký công việc gần đây/))

    const dong = screen.getByText(
      (_, el) => el?.tagName === 'LI' && /Sao lưu/.test(el.textContent ?? '') && /Xong/.test(el.textContent ?? ''),
    )
    expect(dong).toBeDefined()
    expect(screen.queryByText(/sao_luu/)).toBeNull()
  })

  it('dung polling va bao loi ro rang sau qua nhieu lan hoi lai that bai lien tiep', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    try {
      vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-1' })
      vi.mocked(api.saoLuu.congViec).mockRejectedValue(new Error('mang loi tam thoi'))
      render(<SaoLuuPage />)
      await screen.findByText(/Bình thường/)

      fireEvent.click(screen.getByRole('button', { name: /Sao lưu ngay/ }))
      // Cho promise cua taoCongViec/theoDoi chay xong truoc khi bat dau dem gio hoi lai.
      await act(async () => { await vi.advanceTimersByTimeAsync(0) })

      // 60 lan that bai lien tiep, moi lan cach 2s (MS_HOI_LAI) = dung 2 phut.
      await act(async () => { await vi.advanceTimersByTimeAsync(2000 * 60) })

      expect(await screen.findByText(/Không nhận được phản hồi từ máy chủ/)).toBeDefined()
      const soLanGoiSauKhiDung = vi.mocked(api.saoLuu.congViec).mock.calls.length
      expect(soLanGoiSauKhiDung).toBe(60)

      // Polling da dung han — khong goi them nua du cho troi qua tiep.
      await act(async () => { await vi.advanceTimersByTimeAsync(2000 * 10) })
      expect(vi.mocked(api.saoLuu.congViec).mock.calls.length).toBe(soLanGoiSauKhiDung)
    } finally {
      vi.useRealTimers()
    }
  })

  // Finding 1 cua vong review cuoi cung: lien ket tai ve tung la <a href> tran, dieu huong
  // trinh duyet KHONG dinh header Authorization nen luon nhan 401 — xem client.test.ts cho
  // phan kiem chung `taiBanSaoVe` tu dinh header dung cach. O day chi kiem chung man hinh goi
  // dung ham moi (khong con dung <a href>) va xu ly loi/trang thai dang tai.
  it('bam nut "Tai te da chuan bi xong" thi goi taiBanSaoVe dung ma cong viec (khong con la <a href>)', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-tv' })
    vi.mocked(api.saoLuu.congViec).mockResolvedValue({
      id: 'job-tv', loai: 'tai_ve', trangThai: 'xong', buocHienTai: null,
      nhatKy: null, taoLuc: '2026-09-13T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    })
    const { container } = render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    const dong = container.querySelector('.ag-row')
    expect(dong).not.toBeNull()
    fireEvent.contextMenu(dong!, { clientX: 10, clientY: 10 })

    await userEvent.click(await screen.findByRole('button', { name: /Tải bản sao này về máy/ }))

    await waitFor(() => expect(api.saoLuu.taoCongViec)
      .toHaveBeenCalledWith(expect.objectContaining({ loai: 'tai_ve', snapshotId: motBanSao.id })))

    // Polling hoi lai moi MS_HOI_LAI (2s, timer that) truoc khi cong viec chuyen 'xong' — cho
    // du hon khoang do de nut xuat hien on dinh, tranh flaky vi timer that cua may CI cham.
    const nutTai = await screen.findByRole(
      'button', { name: /Tải tệp đã chuẩn bị xong/ }, { timeout: 4000 })
    expect(nutTai.tagName).toBe('BUTTON')
    await userEvent.click(nutTai)

    await waitFor(() => expect(api.saoLuu.taiBanSaoVe).toHaveBeenCalledWith('job-tv'))
  })

  it('bao loi ro rang khi tai tep that bai, khong im lang', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-tv' })
    vi.mocked(api.saoLuu.congViec).mockResolvedValue({
      id: 'job-tv', loai: 'tai_ve', trangThai: 'xong', buocHienTai: null,
      nhatKy: null, taoLuc: '2026-09-13T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    })
    vi.mocked(api.saoLuu.taiBanSaoVe).mockRejectedValue(
      new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.'))
    const { container } = render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    const dong = container.querySelector('.ag-row')
    fireEvent.contextMenu(dong!, { clientX: 10, clientY: 10 })
    await userEvent.click(await screen.findByRole('button', { name: /Tải bản sao này về máy/ }))
    await waitFor(() => expect(api.saoLuu.taoCongViec).toHaveBeenCalled())

    // Polling hoi lai moi MS_HOI_LAI (2s, timer that) truoc khi cong viec chuyen 'xong' — cho
    // du hon khoang do de nut xuat hien on dinh, tranh flaky vi timer that cua may CI cham.
    const nutTai = await screen.findByRole(
      'button', { name: /Tải tệp đã chuẩn bị xong/ }, { timeout: 4000 })
    await userEvent.click(nutTai)

    expect(await screen.findByText(/Phiên đăng nhập đã hết hạn/)).toBeDefined()
  })
})
