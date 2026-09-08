import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { ThongKeChungPage } from './ThongKeChungPage'
import { api } from '../api/client'

vi.mock('../api/client', () => ({
  api: {
    giaoHo: { danhMuc: vi.fn() },
    thongKe: { chung: vi.fn(), onGoi: vi.fn() },
  },
}))

describe('ThongKeChungPage', () => {
  it('mac dinh dieu kien Sinh ra, tim kiem tra ve luoi giao dan voi tong cong dung', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([])
    vi.mocked(api.thongKe.chung).mockResolvedValue({
      tongCong: 2, nhan: ' người được sinh ra',
      giaoDan: [
        { id: 'g1', maGiaoDanCu: 1, tenThanh: null, hoTen: 'A', phai: 'Nam', ngaySinh: '2015-01-01',
          namSinh: '2015', ngayRuaToi: null, ngayRuocLe: null, ngayThemSuc: null, lapGd: false,
          hoTenCha: null, hoTenMe: null, tanTong: false, conHoc: false, ngheNghiep: null,
          ghiChu: null, dienThoai: null, diaChi: null, tenGiaoHo: null, daChuyenDi: false,
          trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null, quaDoi: false,
          ngayQuaDoi: null, noiAnTang: null, noiSinh: null, noiRuaToi: null, noiRuocLe: null,
          noiThemSuc: null, quanHe: null, giaDinhId: null, khongThongKe: false },
      ],
      giaDinh: null, honPhoi: null,
    })

    render(<ThongKeChungPage />)
    fireEvent.click((await screen.findAllByText('Tìm kiếm'))[0])

    expect(await screen.findByText('2')).toBeDefined()
    expect(screen.getByText(/người được sinh ra/)).toBeDefined()
    expect(api.thongKe.chung).toHaveBeenCalledWith(expect.objectContaining({ dieuKien: 'SinhRa' }))
  })

  it('doi sang Gia truong bao thieu tu tuoi/den tuoi khi chua chon', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([])

    render(<ThongKeChungPage />)
    fireEvent.change(await screen.findByLabelText('Điều kiện'), { target: { value: 'GiaTruong' } })
    fireEvent.click(screen.getAllByText('Tìm kiếm')[0])

    expect(await screen.findByText('Vui lòng chọn Từ tuổi/Đến tuổi')).toBeDefined()
    expect(api.thongKe.chung).not.toHaveBeenCalled()
  })

  it('doi sang Cao nien dien san tu tuoi 60 va khong doi hoi den tuoi', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([])
    vi.mocked(api.thongKe.chung).mockResolvedValue({
      tongCong: 5, nhan: ' cao niên', giaoDan: [], giaDinh: null, honPhoi: null,
    })

    render(<ThongKeChungPage />)
    fireEvent.change(await screen.findByLabelText('Điều kiện'), { target: { value: 'CaoNien' } })
    fireEvent.click(screen.getAllByText('Tìm kiếm')[0])

    await vi.waitFor(() => expect(api.thongKe.chung).toHaveBeenCalledWith(
      expect.objectContaining({ dieuKien: 'CaoNien', tuTuoi: 60, denTuoi: undefined })))
  })

  it('dieu kien Tong so gia dinh khong hien o ngay/tuoi va tra ve luoi gia dinh', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([])
    vi.mocked(api.thongKe.chung).mockResolvedValue({
      tongCong: 1, nhan: ' gia đình', giaoDan: null, honPhoi: null,
      giaDinh: [{ id: 'gd1', maGiaDinhCu: 1, maGiaDinhRieng: null, tenGiaDinh: 'GD A', tenChong: null,
        tenVo: null, soLuong: 3, dienThoai: null, dtChong: null, dtVo: null, diaChi: null,
        tenGiaoHo: null, dienGiaDinh: null, ghiChu: null, gach: -1, khongThongKe: false }],
    })

    render(<ThongKeChungPage />)
    fireEvent.change(await screen.findByLabelText('Điều kiện'), { target: { value: 'TongSoGiaDinh' } })
    expect(screen.queryByLabelText('Từ tuổi')).toBeNull()
    fireEvent.click(screen.getAllByText('Tìm kiếm')[0])

    expect(await screen.findByText('GD A')).toBeDefined()
    expect(api.thongKe.chung).toHaveBeenCalledWith(
      expect.objectContaining({ dieuKien: 'TongSoGiaDinh', tuNgay: undefined, denNgay: undefined }))
  })

  it('xoa trang o "Den ngay" (con "Tu ngay" van co) thi bao dung ten o thieu (Trung binh #2)', async () => {
    vi.mocked(api.giaoHo.danhMuc).mockResolvedValue([])

    const { container } = render(<ThongKeChungPage />)
    await screen.findByLabelText('Điều kiện')
    // `<label>Đến ngày</label>` ở tab "Thống kê chung" KHÔNG gắn `htmlFor`/bọc trực tiếp ô nhập
    // (`GxDate` dựng ra một cấu trúc `<span>` lồng nhau, không đơn giản là một `<input>` ngay
    // dưới `<label>`) nên `getByLabelText` không khớp được — lấy trực tiếp ô ngày THỨ HAI
    // ("Đến ngày" đứng sau "Từ ngày") trong panel tab đang active (không bị `hidden`).
    const panelDangChon = container.querySelector('.form-body-panel:not([hidden])')!
    const oNgay = panelDangChon.querySelectorAll<HTMLInputElement>('.gx-date input[type="text"]')
    const denNgay = oNgay[1]!
    // Điều kiện mặc định "Sinh ra" đã điền sẵn Từ ngày/Đến ngày của năm hiện tại — xoá trắng
    // đúng ô "Đến ngày" bằng Backspace liên tục, giữ nguyên "Từ ngày".
    denNgay.focus()
    denNgay.setSelectionRange(10, 10)
    for (let i = 0; i < 10; i++) fireEvent.keyDown(denNgay, { key: 'Backspace' })
    expect(denNgay.value).toBe('__/__/____')

    fireEvent.click(screen.getAllByText('Tìm kiếm')[0])

    // TRƯỚC KHI SỬA: thông báo luôn nói "Hãy nhập từ ngày" bất kể ô nào thực sự bị thiếu — ở
    // đây "Từ ngày" vẫn còn giá trị, chỉ "Đến ngày" bị xoá, thông báo cũ nói sai tên ô cần sửa
    // (xem review toàn nhánh 2026-09-08 "Trung bình #2"). Assertion dưới RED nếu quay lại thông
    // báo cứng "Hãy nhập từ ngày".
    await waitFor(() => expect(screen.queryByText('Hãy nhập đến ngày')).not.toBeNull())
    expect(screen.queryByText('Hãy nhập từ ngày')).toBeNull()
    expect(api.thongKe.chung).not.toHaveBeenCalled()
  })
})
