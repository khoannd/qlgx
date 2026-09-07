import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { GiaDinhDetailPage } from './GiaDinhDetailPage'
import { api, LoiXungDot } from '../api/client'
import { banNhapKhoa, docBanNhap, luuBanNhap } from '../lib/banNhap'
import type { GiaDinhDetail as ChiTiet, GiaoDanTimKiem } from '../api/types'

vi.mock('../api/client', async () => {
  const actual = await vi.importActual<typeof import('../api/client')>('../api/client')
  return {
    LoiXungDot: actual.LoiXungDot,
    api: {
      giaDinh: {
        chiTiet: vi.fn(), capNhat: vi.fn(), tao: vi.fn(),
        ganVoChong: vi.fn(), themThanhVien: vi.fn(), xoaThanhVien: vi.fn(),
      },
      giaoHo: { danhMuc: vi.fn().mockResolvedValue([]) },
      timKiem: { giaoDan: vi.fn() },
    },
  }
})

const nguoiTim = (p: Partial<GiaoDanTimKiem> = {}): GiaoDanTimKiem => ({
  id: 'moi1', maGiaoDanCu: 999, tenThanh: 'Phêrô', hoTen: 'Lê Văn Mới', phai: 'Nam',
  ngaySinh: '1985-01-01', ...p,
})

const chiTiet = (p: Partial<ChiTiet> = {}): ChiTiet => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Nguyễn Văn A',
  giaoHoId: null, dienThoai: null, diaChi: null, soHoKhau: null, dienGiaDinh: null,
  ghiChu: null, daChuyenXu: false, ngayChuyen: null, noiChuyen: null,
  khongThongKe: false, rowVersion: 1, thanhVien: [], ...p,
} as ChiTiet)

describe('GiaDinhDetailPage', () => {
  afterEach(() => { localStorage.clear() })

  it('tai chi tiet that tu API va hien dung ten', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())

    render(<GiaDinhDetailPage id="g1" />)

    expect(screen.getByRole('status')).toBeDefined()
    expect(await screen.findByRole('heading', { name: /Nguyễn Văn A/ })).toBeDefined()
    expect(api.giaDinh.chiTiet).toHaveBeenCalledWith('g1')
  })

  // Loi so 2 (kiem thu nguoi dung 2026-09-07): tieu de the tai lieu phai hien TEN gia dinh,
  // khong phai chu "Gia dinh" chung chung — App.tsx doi tieu de qua callback nay khi tai xong.
  it('tai xong thi goi onTieuDe voi dung ten gia dinh', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet({ tenGiaDinh: 'Paul Trần Văn Thái' }))
    const onTieuDe = vi.fn()

    render(<GiaDinhDetailPage id="g1" onTieuDe={onTieuDe} />)

    await screen.findByRole('heading', { name: /Paul Trần Văn Thái/ })
    expect(onTieuDe).toHaveBeenCalledWith('Paul Trần Văn Thái')
  })

  it('gia dinh chua co ten thi onTieuDe nhan chuoi du phong theo ma gia dinh', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet({ tenGiaDinh: null, maGiaDinhCu: 42 }))
    const onTieuDe = vi.fn()

    render(<GiaDinhDetailPage id="g1" onTieuDe={onTieuDe} />)

    await screen.findByLabelText('Tên gia đình')
    expect(onTieuDe).toHaveBeenCalledWith('Gia đình #42')
  })

  it('luu thanh cong thi goi PUT va tai lai chi tiet', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaDinh.capNhat).mockResolvedValue(undefined)

    render(<GiaDinhDetailPage id="g1" />)
    await screen.findByLabelText('Tên gia đình')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(api.giaDinh.capNhat).toHaveBeenCalledWith('g1', expect.objectContaining({ rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaDinh.chiTiet).toHaveBeenCalledTimes(2)
  })

  it('xung dot RowVersion (409) hien thong bao tieng Viet, khong mat du lieu vua go', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaDinh.capNhat).mockRejectedValue(
      new LoiXungDot('Gia đình này vừa được người khác cập nhật.'),
    )

    render(<GiaDinhDetailPage id="g1" />)
    const oTen = await screen.findByLabelText('Tên gia đình') as HTMLInputElement
    await userEvent.clear(oTen)
    await userEvent.type(oTen, 'Tên vừa sửa')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(await screen.findByText('Gia đình này vừa được người khác cập nhật.')).toBeDefined()
    // Không bị điều hướng mất nội dung đang gõ dở
    expect(oTen.value).toBe('Tên vừa sửa')
  })

  it('tao gia dinh moi: bam Tao gia dinh thi goi POST roi tu chuyen sang xem chi tiet vua tao', async () => {
    vi.mocked(api.giaDinh.tao).mockResolvedValue({ id: 'g9', maGiaDinhCu: 9 })
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet({ id: 'g9', tenGiaDinh: 'Gia đình Mới' }))

    render(<GiaDinhDetailPage id={null} />)
    await userEvent.type(screen.getByLabelText('Tên gia đình'), 'Gia đình Mới')
    await userEvent.click(screen.getByRole('button', { name: 'Tạo gia đình' }))

    expect(api.giaDinh.tao).toHaveBeenCalledWith({ tenGiaDinh: 'Gia đình Mới', giaoHoId: null })
    // Sau khi tạo xong, tự tải chi tiết THẬT bằng id vừa tạo — không cần đổi thẻ tài liệu.
    expect(await screen.findByRole('heading', { name: /Gia đình Mới/ })).toBeDefined()
    expect(api.giaDinh.chiTiet).toHaveBeenCalledWith('g9')
  })

  // --- Gan/doi Nguoi nam qua cay quyet dinh NguoiCu (xem lib/nguoiCu.ts) --------------------
  it('doi Nguoi nam khi vai tro dang co nguoi: hoi Yes (xoa han) roi gui lai voi xuLyNguoiCu', async () => {
    const coNguoiNam = chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
      ],
    })
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(coNguoiNam)
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([nguoiTim()])
    vi.mocked(api.giaDinh.ganVoChong)
      .mockResolvedValueOnce({
        giaoDanId: null,
        canhBao: ['Gia đình này đã có người ở vai trò này. Cần chọn xoá hẳn người cũ hoặc hạ xuống thành viên (kèm vai trò mới) trước khi gán người mới.'],
      })
      .mockResolvedValueOnce({ giaoDanId: 'moi1', canhBao: [] })

    render(<GiaDinhDetailPage id="g1" />)
    await screen.findByLabelText('Tên gia đình')

    await userEvent.click(screen.getAllByTitle('Chọn từ danh sách giáo dân')[0])
    await userEvent.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Mới')
    await userEvent.click(await screen.findByText(/Lê Văn Mới/))

    // Câu hỏi đầu tiên của cây quyết định NguoiCu — nguyên văn dòng 687-689 bản gốc.
    expect(await screen.findByText(/Bạn có muốn xóa giáo dân Giuse Nguyễn Văn A ra khỏi gia đình/)).toBeDefined()
    await userEvent.click(screen.getByRole('button', { name: 'Yes' }))

    expect(api.giaDinh.ganVoChong).toHaveBeenCalledTimes(2)
    expect(api.giaDinh.ganVoChong).toHaveBeenLastCalledWith('g1', 0, {
      giaoDanId: 'moi1', rowVersion: 1, boQuaCanhBao: true, xuLyNguoiCu: { xoa: true, vaiTroMoi: null },
    })
    // Tải lại chi tiết sau khi thành công.
    expect(api.giaDinh.chiTiet).toHaveBeenCalledTimes(2)
  })

  it('canh bao nghiep vu (vd tuoi/da tung ket hon) duoc gop MOT hop xac nhan roi gui lai boQuaCanhBao', async () => {
    const trong = chiTiet()
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(trong)
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([nguoiTim()])
    vi.mocked(api.giaDinh.ganVoChong)
      .mockResolvedValueOnce({ giaoDanId: null, canhBao: ['Giáo dân này đã từng kết hôn với [X].'] })
      .mockResolvedValueOnce({ giaoDanId: 'moi1', canhBao: [] })

    render(<GiaDinhDetailPage id="g1" />)
    await screen.findByLabelText('Tên gia đình')

    await userEvent.click(screen.getAllByTitle('Chọn từ danh sách giáo dân')[0])
    await userEvent.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Mới')
    await userEvent.click(await screen.findByText(/Lê Văn Mới/))

    expect(await screen.findByText(/đã từng kết hôn với \[X\]/)).toBeDefined()
    await userEvent.click(screen.getByRole('button', { name: 'Yes' }))

    expect(api.giaDinh.ganVoChong).toHaveBeenLastCalledWith('g1', 0, {
      giaoDanId: 'moi1', rowVersion: 1, boQuaCanhBao: true, xuLyNguoiCu: null,
    })
  })

  it('xoa mot thanh vien: hoi xac nhan nguyen van roi goi DELETE va tai lai', async () => {
    const coThanhVien = chiTiet({
      thanhVien: [
        { giaoDanId: 'p3', maGiaoDanCu: 1003, vaiTro: 4, chuHo: false, tenThanh: 'Anna', hoTen: 'Nguyễn Thị Cha Mẹ', phai: 'Nữ', ngaySinh: '1950-01-01', quaDoi: false, daXoa: false },
      ],
    })
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(coThanhVien)
    vi.mocked(api.giaDinh.xoaThanhVien).mockResolvedValue(undefined)

    render(<GiaDinhDetailPage id="g1" />)
    const dong = (await screen.findByText('Nguyễn Thị Cha Mẹ')).closest('.ag-row')!
    const { fireEvent } = await import('@testing-library/react')
    fireEvent.contextMenu(dong)
    await userEvent.click(await screen.findByText('Xoá khỏi gia đình'))

    expect(await screen.findByText(/Bạn có thực sự muốn xóa vĩnh viễn giáo dân này/)).toBeDefined()
    await userEvent.click(screen.getByRole('button', { name: 'Yes' }))

    expect(api.giaDinh.xoaThanhVien).toHaveBeenCalledWith('g1', 'p3', 4)
    expect(api.giaDinh.chiTiet).toHaveBeenCalledTimes(2)
  })

  // --- Task 16: bản nháp ngoại tuyến (lib/banNhap.ts) ---------------------------------------

  it('co ban nhap cu thi hien banner hoi khoi phuc, bam Khoi phuc thi ap dung vao form', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaDinh', 'g1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Ghi chú nháp gia đình' })

    render(<GiaDinhDetailPage id="g1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Nguyễn Văn A/ })

    expect(await screen.findByText(/Có bản nháp chưa lưu/)).toBeDefined()
    expect(screen.getByLabelText('Ghi chú')).toHaveProperty('value', '')

    await userEvent.click(screen.getByRole('button', { name: 'Khôi phục' }))

    expect(screen.getByLabelText('Ghi chú')).toHaveProperty('value', 'Ghi chú nháp gia đình')
    expect(screen.queryByText(/Có bản nháp chưa lưu/)).toBeNull()
  })

  it('luu thanh cong thi xoa ban nhap gia dinh', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaDinh.capNhat).mockResolvedValue(undefined)
    const khoa = banNhapKhoa('giaDinh', 'g1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Nháp trước khi lưu' })

    render(<GiaDinhDetailPage id="g1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Nguyễn Văn A/ })
    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    await screen.findByText('Đã lưu thành công.')
    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
  })

  it('khong truyen tenTaiKhoan thi tat tinh nang ban nhap, khong hien banner du co du lieu cu', async () => {
    vi.mocked(api.giaDinh.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaDinh', 'g1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Nháp' })

    render(<GiaDinhDetailPage id="g1" />)
    await screen.findByRole('heading', { name: /Nguyễn Văn A/ })

    expect(screen.queryByText(/Có bản nháp chưa lưu/)).toBeNull()
  })
})
