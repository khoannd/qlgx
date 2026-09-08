import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaDinhDetail } from './GiaDinhDetail'
import { api } from '../api/client'
import type { GiaDinhDetail as ChiTiet, GiaoDanTimKiem } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    timKiem: { giaoDan: vi.fn() },
    giaDinh: {
      inPhieuGiaDinh: vi.fn(() => Promise.resolve()),
      inLyLichCaNhanGiaDinh: vi.fn(() => Promise.resolve()),
    },
    // Giấy giới thiệu (3 mẫu theo giáo dân) — menuThanhVien của GiaDinhDetail nhúng
    // menuGiaoDanMacDinh nên cần các hàm này tồn tại dù không dùng trong các test hiện có
    // (xem lib/useGioiThieuGiaoDan.ts, tra cứu ngay lúc import module).
    giaoDan: {
      inGioiThieuRuaToi: vi.fn(() => Promise.resolve()),
      inGioiThieuThemSuc: vi.fn(() => Promise.resolve()),
      inGioiThieuGiaoLyHonPhoi: vi.fn(() => Promise.resolve()),
    },
  },
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
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 2, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1998-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    expect(await screen.findByText('Trần Thị B')).toBeDefined()
    // Cot "Ma GD" phai hien dung ma giao dan he cu (1002) — tung bi bo sot o ThanhVienDto
    // (backend) nen frontend luon dien 0 lam gia tri tam, xem can-review-sau.md muc 40.
    expect(await screen.findByText('1002')).toBeDefined()
  })

  it('luoi Thanh vien khac loai tru vo chong (chuHo), da hien rieng o Nguoi nam/Nguoi nu', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: true, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p3', maGiaoDanCu: 1003, vaiTro: 2, chuHo: false, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn C', phai: 'Nam', ngaySinh: '2000-01-01', quaDoi: false, daXoa: false },
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
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
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

  // Nút "+" của GxPicker (Người nam) — trước đây vô hiệu hoá hẳn, nay mở một thẻ "Giáo dân
  // mới" tách biệt qua `moGiaoDanMoiChoPicker` rồi điền ngược kết quả vào `onGanVoChong(0, …)`
  // — xem GxPicker.tsx, App.moChiTietGiaDinh, can-review-sau.md.
  it('bam nut + o Nguoi nam thi goi moGiaoDanMoiChoPicker, tao xong dien nguoc qua onGanVoChong(0, ...)', async () => {
    const onGanVoChong = vi.fn()
    const moGiaoDanMoiChoPicker = vi.fn((onTaoXong: (gd: GiaoDanTimKiem) => void) => onTaoXong(nguoiTim()))
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet()} onGanVoChong={onGanVoChong}
      moGiaoDanMoiChoPicker={moGiaoDanMoiChoPicker} />)

    await nguoiDung.click(screen.getAllByTitle('Thêm giáo dân mới')[0])

    expect(moGiaoDanMoiChoPicker).toHaveBeenCalledOnce()
    expect(onGanVoChong).toHaveBeenCalledWith(0, nguoiTim())
  })

  it('khong truyen moGiaoDanMoiChoPicker thi nut + van vo hieu hoa nhu cu', () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)

    expect(screen.getAllByTitle(/Thêm giáo dân mới/)[0]).toHaveProperty('disabled', true)
  })

  it('bam Bo chon o Nguoi nu (dang co nguoi) thi goi onBoChonVoChong(1)', async () => {
    const onBoChonVoChong = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
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
        { giaoDanId: 'p3', maGiaoDanCu: 1003, vaiTro: 4, chuHo: false, tenThanh: 'Anna', hoTen: 'Nguyễn Thị Cha Me', phai: 'Nữ', ngaySinh: '1950-01-01', quaDoi: false, daXoa: false },
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
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
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
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
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
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: false, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
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

  // "In phiếu gia đình" (xem docs/superpowers/specs/man-hinh/in-an.md) — không mơ hồ như "In
  // lý lịch cá nhân" (luôn là cả gia đình đang mở), nên nối thẳng vào api.giaDinh.inPhieuGiaDinh
  // thay vì báo "chưa hỗ trợ".
  it('bam In phieu gia dinh thi goi dung api voi id gia dinh dang mo', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({ id: 'g9' })} />)

    await userEvent.click(screen.getByRole('button', { name: 'In phiếu gia đình' }))

    expect(api.giaDinh.inPhieuGiaDinh).toHaveBeenCalledWith('g9', 'A4')
  })

  // Bổ sung theo mục 2 nhiệm vụ "người dùng nên chọn được khổ khi in phiếu gia đình"
  // (PhieuGiaDinh-A3.doc bản desktop — gia đình đông người, xem in-an.md mục 5c/8).
  it('chon kho A3 roi bam In phieu gia dinh thi goi api voi khoGiay A3', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({ id: 'g9' })} />)

    await userEvent.selectOptions(screen.getByLabelText('Khổ giấy'), 'A3')
    await userEvent.click(screen.getByRole('button', { name: 'In phiếu gia đình' }))

    expect(api.giaDinh.inPhieuGiaDinh).toHaveBeenCalledWith('g9', 'A3')
  })

  it('gia dinh moi (chua luu): nut In phieu gia dinh bi vo hieu hoa', () => {
    render(<GiaDinhDetail onTaoMoi={vi.fn()} />)

    expect(screen.getByRole('button', { name: 'In phiếu gia đình' })).toHaveProperty('disabled', true)
  })

  // "In lý lịch cá nhân" — hết mơ hồ (in CẢ gia đình đang mở, một trang/thành viên — đúng hành
  // vi item4_Click của bản desktop, xem in-an.md mục 5f), nối thẳng vào
  // api.giaDinh.inLyLichCaNhanGiaDinh thay vì báo "chưa hỗ trợ".
  it('bam In ly lich ca nhan thi goi dung api voi id gia dinh dang mo', async () => {
    render(<GiaDinhDetail duLieu={chiTiet({ id: 'g9' })} />)

    await userEvent.click(screen.getByRole('button', { name: 'In lý lịch cá nhân' }))

    expect(api.giaDinh.inLyLichCaNhanGiaDinh).toHaveBeenCalledWith('g9')
  })

  it('gia dinh moi (chua luu): nut In ly lich ca nhan bi vo hieu hoa', () => {
    render(<GiaDinhDetail onTaoMoi={vi.fn()} />)

    expect(screen.getByRole('button', { name: 'In lý lịch cá nhân' })).toHaveProperty('disabled', true)
  })

  // --- Loi so 1 (kiem thu nguoi dung 2026-09-07): man hinh gia dinh vo bo cuc — khong thay khoi
  // hon phoi, khong thay luoi thanh vien. Nguyen nhan goc: `.detail-page` khai dung 4 hang luoi
  // nhung co 6 phan tu con truc tiep, day GxGiaoDanList vao hang an cao ~0px. Cac bai duoi day
  // xac nhan (o muc lam duoc trong jsdom — KHONG thay the anh chup trinh duyet that) ca khoi hon
  // phoi lan luoi thanh vien deu co mat trong DOM va dung so dong. -----------------------------

  it('chua co Nguoi nam/nu: khoi Hon phoi VAN hien tren DOM nhung cac o bi vo hieu hoa', () => {
    render(<GiaDinhDetail duLieu={chiTiet()} />)

    expect(screen.getByRole('heading', { name: 'Hôn phối' })).toBeDefined()
    const oSo = screen.getByLabelText('Số hôn phối') as HTMLInputElement
    expect(oSo.disabled).toBe(true)
    expect(screen.getByText(/Chọn Người nam hoặc Người nữ ở trên trước khi nhập hôn phối/)).toBeDefined()
  })

  it('co Nguoi nam: khoi Hon phoi duoc mo va hien dung du lieu da co', () => {
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
      ],
      honPhoi: {
        id: 'hp1', soHonPhoi: 'HP-01', ngayHonPhoi: '1995-05-20', noiHonPhoi: 'Nhà thờ Chính toà',
        linhMucChung: 'Cha Phêrô', nguoiChung1: 'Ông Ba', nguoiChung2: 'Bà Tư',
        cachThucHonPhoi: 'Hợp pháp', ghiChu: 'Không có gì đặc biệt', rowVersion: 3,
      },
    })} />)

    const oSo = screen.getByLabelText('Số hôn phối') as HTMLInputElement
    expect(oSo.disabled).toBe(false)
    expect(oSo.value).toBe('HP-01')
    expect((screen.getByLabelText('Nơi hôn phối') as HTMLInputElement).value).toBe('Nhà thờ Chính toà')
    expect(screen.queryByText(/Chọn Người nam hoặc Người nữ ở trên trước khi nhập hôn phối/)).toBeNull()
  })

  it('bam Cap nhat voi Nguoi nam da chon thi onLuu nhan dung honPhoi tu form', async () => {
    const onLuu = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
      ],
      honPhoi: {
        id: 'hp1', soHonPhoi: null, ngayHonPhoi: null, noiHonPhoi: null, linhMucChung: null,
        nguoiChung1: null, nguoiChung2: null, cachThucHonPhoi: null, ghiChu: null, rowVersion: 7,
      },
    })} onLuu={onLuu} />)

    await nguoiDung.type(screen.getByLabelText('Nơi hôn phối'), 'Nhà thờ Vô Nhiễm')
    await nguoiDung.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu).toHaveBeenCalledTimes(1)
    expect(onLuu.mock.calls[0][0].honPhoi).toMatchObject({ noiHonPhoi: 'Nhà thờ Vô Nhiễm', rowVersion: 7 })
  })

  it('chua co Nguoi nam/nu thi onLuu nhan honPhoi: null (khong dung tro gi ca)', async () => {
    const onLuu = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet()} onLuu={onLuu} />)

    await nguoiDung.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu.mock.calls[0][0].honPhoi).toBeNull()
  })

  it('co Nguoi nam: nut "Mo ho so trong the moi" canh Nguoi nam goi dung moGiaoDan', async () => {
    const moGiaoDan = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
      ],
    })} moGiaoDan={moGiaoDan} />)

    await nguoiDung.click(screen.getAllByTitle('Mở hồ sơ trong thẻ mới')[0])

    expect(moGiaoDan).toHaveBeenCalledWith('p1')
  })

  it('chua co Nguoi nam/nu thi KHONG hien nut "Mo ho so trong the moi"', () => {
    render(<GiaDinhDetail duLieu={chiTiet()} moGiaoDan={vi.fn()} />)

    expect(screen.queryByTitle('Mở hồ sơ trong thẻ mới')).toBeNull()
  })

  it('luoi thanh vien hien DUNG so dong (khong con co 0px nhu loi bo cuc da bao)', async () => {
    const { container } = render(<GiaDinhDetail duLieu={chiTiet({
      thanhVien: [
        { giaoDanId: 'p1', maGiaoDanCu: 1001, vaiTro: 0, chuHo: true, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn A', phai: 'Nam', ngaySinh: '1970-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p2', maGiaoDanCu: 1002, vaiTro: 1, chuHo: false, tenThanh: 'Maria', hoTen: 'Trần Thị B', phai: 'Nữ', ngaySinh: '1975-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p3', maGiaoDanCu: 1003, vaiTro: 2, chuHo: false, tenThanh: 'Giuse', hoTen: 'Nguyễn Văn C', phai: 'Nam', ngaySinh: '2000-01-01', quaDoi: false, daXoa: false },
        { giaoDanId: 'p4', maGiaoDanCu: 1004, vaiTro: 2, chuHo: false, tenThanh: 'Maria', hoTen: 'Nguyễn Thị D', phai: 'Nữ', ngaySinh: '2002-01-01', quaDoi: false, daXoa: false },
      ],
    })} />)

    await screen.findByText('Nguyễn Văn C')
    expect(container.querySelectorAll('.ag-row').length).toBe(2) // loai vo chong (p1/p2), con p3+p4
    expect(container.querySelector('.count-pill')?.textContent).toContain('2')
  })
})
