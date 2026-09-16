import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
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
  // Neo dong ho: den trang thai nay PHU THUOC thoi gian (xem lib/denSaoLuu.ts — sao luu tre 8
  // gio thi vang, dien tap cu 14 ngay thi vang). Khong neo thi bo test se tu chuyen vang roi do
  // theo ngay thang that, va mot hom nao do do het ma khong ai doi dong nao.
  vi.setSystemTime(new Date('2026-09-13T08:00:00Z'))
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

  // I4: cau hien ra phai la tieng Viet CO DAU, khong con ten cong cu ky thuat; chuoi tho van
  // giu lai nhung gap vao trong muc "Thong tin cho nguoi ky thuat".
  it('den do hien canh bao tieng Viet, giu chuoi ky thuat trong muc rieng', async () => {
    vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(
      { ...tinhTrangXanh, den: 'do', loiGanNhat: 'restic check that bai' })
    render(<SaoLuuPage />)
    expect(await screen.findByText(/Kho sao lưu bị lỗi khi kiểm tra tính toàn vẹn/)).toBeDefined()
    expect(screen.getByText(/Thông tin cho người kỹ thuật/)).toBeDefined()
    expect(screen.getByTestId('den-do')).toBeDefined()
  })

  // I3: may chu tra ve 'xanh' vi DienTapDat=true va khong xet lan dien tap do cu tu bao gio.
  // Giao dien phai tu leo thang, neu khong nguoi dung doc chu "Dat" chu khong doc ngay.
  it('den KHONG con xanh khi dien tap phuc hoi da cu hang thang, du may chu bao xanh', async () => {
    vi.mocked(api.saoLuu.tinhTrang).mockResolvedValue(
      { ...tinhTrangXanh, den: 'xanh', dienTapGanNhat: '2026-06-01T03:00:00Z' })
    render(<SaoLuuPage />)
    expect(await screen.findByTestId('den-do')).toBeDefined()
    expect(screen.queryByText(/^🟢/)).toBeNull()
  })

  // N1: thoi gian tuong doi theo mockup spec 8.2 — de nhan ra ngay "da ba ngay chua sao luu"
  // ma khong phai tu tinh.
  it('hien thoi gian tuong doi ben canh ngay gio day du', async () => {
    render(<SaoLuuPage />)
    await screen.findByTestId('den-xanh')
    expect(screen.getByText(/2 giờ trước/)).toBeDefined()
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

  // C1 (Critical, review-frontend.md): cot "Hien tai" cua bang doi chieu TUNG lay
  // banSao[0].soGiaoDan — tuc la so cua BAN SAO MOI NHAT, khong phai so hien tai cua he thong.
  // Khi phuc hoi ve chinh ban moi nhat (thao tac pho bien nhat) hai cot bang nhau, khong dong
  // nao to do, nguoi dung duoc tran an SAI ngay luc sap mat toi 6 gio nhap lieu cua moi giao xu.
  // Test nay chay tren ma CHUA SUA phai DO (luc do man hinh hien "2050" o cot Hien tai va khong
  // he co cau canh bao nao).
  it('KHONG hien con so "hien tai" gia — noi ro chua tinh duoc va van canh bao mat du lieu', async () => {
    const { container } = render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    fireEvent.contextMenu(container.querySelector('.ag-row')!, { clientX: 10, clientY: 10 })
    await userEvent.click(await screen.findByRole('button', { name: /Phục hồi về bản sao này/ }))

    const hopThoai = await screen.findByRole('dialog')
    expect(hopThoai.textContent).toMatch(/chưa tính được/i)
    // Khong duoc lay con so cua ban sao moi nhat lam "hien tai": o ban sao ghi 2050, neu con so
    // do chi duoc xuat hien DUNG MOT lan trong hop thoai (cot "Sau khi phuc hoi"); xuat hien hai
    // lan nghia la no da bi dung lai lam cot "Hien tai" — loi cu quay lai.
    expect(within(hopThoai).getAllByText('2050').length).toBe(1)
    expect(hopThoai.textContent).toMatch(/đều sẽ mất/i)
  })

  // T1 (lo hong test, review-frontend.md): mat xich nguy hiem nhat — tu menu chuot phai toi khi
  // goi taoCongViec('phuc_hoi') — truoc day khong co test nao noi hai dau.
  it('luong phuc hoi dau-cuoi: go dung chuoi thi tao cong viec phuc_hoi dung tham so', async () => {
    vi.mocked(api.saoLuu.taoCongViec).mockResolvedValue({ id: 'job-ph' })
    vi.mocked(api.saoLuu.congViec).mockResolvedValue({
      id: 'job-ph', loai: 'phuc_hoi', trangThai: 'dang_chay', buocHienTai: 'Đang nạp dữ liệu…',
      nhatKy: null, taoLuc: '2026-09-13T07:00:00Z', batDauLuc: null, ketThucLuc: null,
    })
    const { container } = render(<SaoLuuPage />)
    await screen.findByText(/Bình thường/)

    fireEvent.contextMenu(container.querySelector('.ag-row')!, { clientX: 10, clientY: 10 })
    await userEvent.click(await screen.findByRole('button', { name: /Phục hồi về bản sao này/ }))
    await userEvent.type(screen.getByLabelText(/gõ/i), 'PHUC HOI TOAN BO')
    await userEvent.click(screen.getByRole('button', { name: /^Phục hồi về 13\/09\/2026/ }))

    await waitFor(() => expect(api.saoLuu.taoCongViec).toHaveBeenCalledWith({
      loai: 'phuc_hoi', snapshotId: 'ab12cd34', xacNhan: 'PHUC HOI TOAN BO',
    }))
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
