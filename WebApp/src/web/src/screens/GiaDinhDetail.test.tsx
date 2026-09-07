import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhDetail } from './GiaDinhDetail'
import { api } from '../api/client'
import type { GiaDinhDetail as ChiTiet, GiaoDanTimKiem } from '../api/types'

vi.mock('../api/client', () => ({
  api: { timKiem: { giaoDan: vi.fn() } },
}))

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

describe('GiaDinhDetail', () => {
  it('hien dung tieu de va cac truong chinh cua ho so gia dinh', () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)

    expect(screen.getByRole('heading', { name: /Nguyễn Văn A/ })).toBeDefined()
    expect(screen.getByLabelText('Tên gia đình')).toBeDefined()
    expect(screen.getByLabelText('Giáo họ')).toBeDefined()
    expect(screen.getByLabelText('Địa chỉ')).toBeDefined()
  })

  it('vua mo mot ho so gia dinh da co, chua sua gi thi KHONG bao Thay doi chua duoc luu', () => {
    // Cung loi voi GiaoDanDetail (kiem thu kham pha 2026-09-07): thongBaoLuu mac dinh null nen
    // luc moi mo, chua dong den o nao, van hien nham "Thay doi chua duoc luu".
    render(<GiaDinhDetail duLieu={chiTiet()} />)

    expect(screen.queryByText('Thay đổi chưa được lưu')).toBeNull()
  })

  it('tick Da chuyen di xu khac thi hien Ngay chuyen va Noi chuyen', async () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)
    expect(screen.queryByLabelText('Ngày chuyển')).toBeNull()

    await userEvent.click(screen.getByLabelText('Đã chuyển đi xứ khác'))

    expect(screen.getByLabelText('Ngày chuyển')).toBeDefined()
    expect(screen.getByLabelText('Nơi chuyển')).toBeDefined()
  })

  it('nut Quay ve va Danh sach goi ham mo danh sach gia dinh', async () => {
    const moDanhSachGiaDinh = vi.fn()
    render(<GiaDinhDetail duLieu={chiTiet()} moDanhSachGiaDinh={moDanhSachGiaDinh} />)

    await userEvent.click(screen.getByRole('button', { name: '← Danh sách' }))
    await userEvent.click(screen.getByRole('button', { name: 'Quay về' }))

    expect(moDanhSachGiaDinh).toHaveBeenCalledTimes(2)
  })

  it('hien luoi thanh vien voi dung so nguoi', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 2, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1998-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    expect(await screen.findByText('Trần Thị B')).toBeDefined()
  })

  it('luoi Thanh vien khac loai tru vo chong (chuHo), da hien rieng o Nguoi nam/Nguoi nu', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: true, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p3', vaiTro: 2, chuHo: false, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn C', phai: 'Nam', ngaySinh: '2000-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    expect(await screen.findByText('Nguyễn Văn C')).toBeDefined()
    expect(screen.queryByText('Nguyễn Văn A')).toBeNull()
    expect(screen.queryByText('Trần Thị B')).toBeNull()
  })

  it('Nguoi nam/Nguoi nu xac dinh theo vaiTro (0/1), khong theo chuHo', async () => {
    // chuHo chỉ đúng MỘT người (thường không phải người vợ ở đây) — nếu component còn lấy
    // theo chuHo thay vì vaiTro thì "Người nữ" sẽ hiện rỗng ("—") thay vì đúng tên vợ.
    const { container } = render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    // GxPicker dựng bằng <span id="..."> (không phải ô nhập) nên getByLabelText không tìm ra
    // được — tra trực tiếp qua id đã nối với nhãn bằng htmlFor.
    expect(container.querySelector('#gdinh-nguoinam')?.textContent).toContain('Nguyễn Văn A')
    expect(container.querySelector('#gdinh-nguoinu')?.textContent).toContain('Trần Thị B')
  })

  it('chon nguoi moi cho Nguoi nam qua GxPicker thi goi onGanVoChong(0, ...)', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([nguoiTim()])
    const onGanVoChong = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet()} onGanVoChong={onGanVoChong} />)

    // Người nam là ô picker ĐẦU TIÊN của form.
    await nguoiDung.click(screen.getAllByTitle('Chọn từ danh sách giáo dân')[0])
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Mới')
    await nguoiDung.click(await screen.findByText(/Lê Văn Mới/))

    expect(onGanVoChong).toHaveBeenCalledWith(0, nguoiTim())
  })

  it('bam Bo chon o Nguoi nu (dang co nguoi) thi goi onBoChonVoChong(1)', async () => {
    const onBoChonVoChong = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
      ],
    })} onBoChonVoChong={onBoChonVoChong} />)

    // Thứ tự các nút "Bỏ chọn": Người nam, Người nữ, (rồi ô thêm thành viên).
    await nguoiDung.click(screen.getAllByTitle('Bỏ chọn')[1])

    expect(onBoChonVoChong).toHaveBeenCalledWith(1)
  })

  it('them mot thanh vien: chon nguoi + vai tro roi bam Them vao gia dinh', async () => {
    vi.mocked(api.timKiem.giaoDan).mockResolvedValue([nguoiTim()])
    const onThemThanhVien = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet()} onThemThanhVien={onThemThanhVien} />)

    const nutChon = screen.getAllByTitle('Chọn từ danh sách giáo dân')
    // Không có Người nam/nữ nên chỉ có MỘT picker: ô thêm thành viên.
    await nguoiDung.click(nutChon[nutChon.length - 1])
    await nguoiDung.type(screen.getByPlaceholderText('Gõ tên hoặc mã cũ để tìm…'), 'Mới')
    await nguoiDung.click(await screen.findByText(/Lê Văn Mới/))

    await nguoiDung.selectOptions(screen.getByLabelText('Vai trò thành viên mới'), '4')
    await nguoiDung.click(screen.getByRole('button', { name: 'Thêm vào gia đình' }))

    expect(onThemThanhVien).toHaveBeenCalledWith(nguoiTim(), 4)
  })

  it('muc menu Xoa khoi gia dinh goi onXoaThanhVien voi dung vaiTro', async () => {
    const onXoaThanhVien = vi.fn()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p3', vaiTro: 4, chuHo: false, tenThanh: 'Anna', hoTen: 'Nguyễn Thị Cha Me', phai: 'Nữ', ngaySinh: '1950-01-01', quaDoi: false, daXoa: false },
      ],
    })} onXoaThanhVien={onXoaThanhVien} />)

    const dong = (await screen.findByText('Nguyễn Thị Cha Me')).closest('.ag-row')!
    fireEvent.contextMenu(dong)
    await userEvent.click(await screen.findByText('Xoá khỏi gia đình'))

    expect(onXoaThanhVien).toHaveBeenCalledWith('p3', 4)
  })

  // --- Chu ho (review-frontend "chan cung #1"): dungPayloadTuForm truoc day KHONG doc radio
  // "chuho" — nguoi dung tuong da dat chu ho nhung bam Cap nhat thi khong luu gi ca. ------------

  it('dang co Nguoi nam la chu ho thi radio Chu ho canh Nguoi nam duoc check san', () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    const radios = screen.getAllByRole('radio', { name: 'Chủ hộ' }) as HTMLInputElement[]
    expect(radios[0].checked).toBe(true)
    expect(radios[1].checked).toBe(false)
  })

  it('chua co Nguoi nam/nu thi radio Chu ho bi vo hieu hoa (khong ai de lam chu ho)', () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)

    const radios = screen.getAllByRole('radio', { name: 'Chủ hộ' }) as HTMLInputElement[]
    expect(radios[0].disabled).toBe(true)
    expect(radios[1].disabled).toBe(true)
  })

  it('bam Cap nhat sau khi chon radio Chu ho o Nguoi nu thi onLuu nhan dung chuHoVaiTro:1', async () => {
    const onLuu = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
      ],
    })} onLuu={onLuu} />)

    const radios = screen.getAllByRole('radio', { name: 'Chủ hộ' })
    await nguoiDung.click(radios[1])
    await nguoiDung.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu).toHaveBeenCalledTimes(1)
    expect(onLuu.mock.calls[0][0]).toMatchObject({ chuHoVaiTro: 1 })
  })

  it('khong dong nao duoc check thi onLuu nhan chuHoVaiTro:null', async () => {
    const onLuu = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', vaiTro: 0, chuHo: false, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
      ],
    })} onLuu={onLuu} />)

    await nguoiDung.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu.mock.calls[0][0]).toMatchObject({ chuHoVaiTro: null })
  })

  it('gia dinh moi: bam Tao gia dinh khi chua nhap ten thi bao loi, khong goi onTaoMoi', async () => {
    const onTaoMoi = vi.fn()
    const baoLoi = vi.spyOn(window, 'alert').mockImplementation(() => {})
    render(<GiaDinhDetail onTaoMoi={onTaoMoi} />)

    await userEvent.click(screen.getByRole('button', { name: 'Tạo gia đình' }))

    expect(baoLoi).toHaveBeenCalledWith('Hãy nhập tên gia đình!')
    expect(onTaoMoi).not.toHaveBeenCalled()
    baoLoi.mockRestore()
  })

  it('gia dinh moi: nhap ten roi bam Tao gia dinh thi goi onTaoMoi dung payload', async () => {
    const onTaoMoi = vi.fn()
    render(<GiaDinhDetail onTaoMoi={onTaoMoi} />)

    await userEvent.type(screen.getByLabelText('Tên gia đình'), 'Gia đình Thử Nghiệm')
    await userEvent.click(screen.getByRole('button', { name: 'Tạo gia đình' }))

    expect(onTaoMoi).toHaveBeenCalledWith({ tenGiaDinh: 'Gia đình Thử Nghiệm', giaoHoId: null })
  })
})
