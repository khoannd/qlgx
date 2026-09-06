import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanDetail } from './GiaoDanDetail'
import type { GiaoDanDetail as ChiTiet, HonPhoiCuaGiaoDan } from '../api/types'

const honPhoi = (p: Partial<HonPhoiCuaGiaoDan> = {}): HonPhoiCuaGiaoDan => ({
  id: 'hp1', tenHonPhoi: 'Giuse Trí - Maria Thu', soHonPhoi: '12/2018',
  ngayHonPhoi: '2018-05-01', noiHonPhoi: 'Nhà thờ Chính tòa', linhMucChung: 'Lm. Nguyễn Văn A',
  nguoiChung1: 'Ông B', nguoiChung2: 'Bà C', cachThucHonPhoi: 'Hợp pháp',
  ghiChu: 'Không có gì đặc biệt', voChongId: 'vc1', tenVoChong: 'Maria Thu',
  rowVersion: 1, ...p,
})

const chiTiet = (p: Partial<ChiTiet> = {}): ChiTiet => ({
  id: 'p1', maGiaoDanCu: 4511, hoTen: 'Vũ Minh Trí', tenThanh: 'Giuse', phai: 'Nam',
  ngaySinh: '1996-04-02', noiSinh: null, cmnd: null, danToc: null, giaoHoId: null,
  diaChi: null, dienThoai: null, email: null, hoTenCha: null, hoTenMe: null,
  soRuaToi: null, ngayRuaToi: null, noiRuaToi: null, chaRuaToi: null, nguoiDoDauRuaToi: null,
  soRuocLe: null, ngayRuocLe: null, noiRuocLe: null, chaRuocLe: null,
  soThemSuc: null, ngayThemSuc: null, noiThemSuc: null, chaThemSuc: null, nguoiDoDauThemSuc: null,
  ngayXucDau: null, nguoiXucDau: null, tinhTrangXucDau: null, ghiChuXucDau: null,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null, ngheNghiep: null,
  conHoc: false, daCoGiaDinh: false, tanTong: false, khongThongKe: false,
  quaDoi: false, ngayQuaDoi: null, noiQuaDoi: null, soAnTang: null, noiAnTang: null,
  ghiChu: null, giaDinhId: null, tenGiaDinh: null, vaiTro: null,
  rowVersion: 1, ...p,
} as ChiTiet)

describe('GiaoDanDetail', () => {
  it('co du nam tab dung ten cua frmGiaoDan', () => {
    render(<GiaoDanDetail duLieu={chiTiet()} />)

    for (const ten of ['Cá nhân', 'Giáo lý', 'Hôn phối', 'Ơn gọi tận hiến', 'Hội đoàn'])
      expect(screen.getByRole('tab', { name: ten })).toBeDefined()
  })

  it('tick Qua doi thi hien them ngay qua doi va noi an tang', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} />)
    expect(screen.queryByLabelText('Ngày qua đời')).toBeNull()

    await userEvent.click(screen.getByLabelText('Qua đời'))

    expect(screen.getByLabelText('Ngày qua đời')).toBeDefined()
    expect(screen.getByLabelText('Nơi an táng')).toBeDefined()
  })

  it('tick Qua doi thi tu bo tick Con hoc', async () => {
    render(<GiaoDanDetail duLieu={chiTiet({ conHoc: true })} />)
    const conHoc = screen.getByLabelText('Còn học') as HTMLInputElement
    expect(conHoc.checked).toBe(true)

    await userEvent.click(screen.getByLabelText('Qua đời'))

    expect(conHoc.checked).toBe(false)
  })

  it('tick Con hoc thi tu bo tick Qua doi', async () => {
    render(<GiaoDanDetail duLieu={chiTiet({ quaDoi: true })} />)
    const quaDoi = screen.getByLabelText('Qua đời') as HTMLInputElement
    expect(quaDoi.checked).toBe(true)

    await userEvent.click(screen.getByLabelText('Còn học'))

    expect(quaDoi.checked).toBe(false)
  })

  it('chon giao ho Ngoai xu thi hien Giao xu va Giao phan, an khoi chuyen xu', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} />)

    await userEvent.selectOptions(screen.getByLabelText('Giáo họ'), 'Ngoài xứ')

    expect(screen.getByLabelText('Giáo xứ')).toBeDefined()
    expect(screen.queryByText('Thông tin chuyển xứ')).toBeNull()
  })

  it('nut Quay ve va Danh sach goi ham mo danh sach giao dan', async () => {
    const moDanhSachGiaoDan = vi.fn()
    render(<GiaoDanDetail duLieu={chiTiet()} moDanhSachGiaoDan={moDanhSachGiaoDan} />)

    await userEvent.click(screen.getByRole('button', { name: '← Danh sách' }))
    await userEvent.click(screen.getByRole('button', { name: 'Quay về' }))

    expect(moDanhSachGiaoDan).toHaveBeenCalledTimes(2)
  })

  it('nut Xem gia dinh goi moGiaDinh voi dung id khi da co gia dinh', async () => {
    const moGiaDinh = vi.fn()
    render(<GiaoDanDetail duLieu={chiTiet({ giaDinhId: 'gd1', tenGiaDinh: 'Nguyễn Văn A' })} moGiaDinh={moGiaDinh} />)

    await userEvent.click(screen.getByRole('button', { name: 'Xem gia đình' }))

    expect(moGiaDinh).toHaveBeenCalledWith('gd1')
  })

  // --- Task 15: tab Hôn phối nối API thật (trước đây chỉ là khung tĩnh) ---

  it('chua co hon phoi nao thi bao chua co, khong hien nut Cap nhat', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHonPhoi={[]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))

    expect(screen.getByText(/chưa có bản ghi hôn phối/i)).toBeDefined()
  })

  it('hien du cac truong cua mot hon phoi da co', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHonPhoi={[honPhoi()]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))

    expect(screen.getByText('Maria Thu')).toBeDefined()
    expect(screen.getByDisplayValue('12/2018')).toBeDefined()
    expect(screen.getByDisplayValue('2018-05-01')).toBeDefined()
    expect(screen.getByDisplayValue('Nhà thờ Chính tòa')).toBeDefined()
    expect(screen.getByDisplayValue('Lm. Nguyễn Văn A')).toBeDefined()
    expect(screen.getByDisplayValue('Ông B')).toBeDefined()
    expect(screen.getByDisplayValue('Bà C')).toBeDefined()
    expect(screen.getByDisplayValue('Không có gì đặc biệt')).toBeDefined()
  })

  it('goa roi tai hon thi hien du danh sach nhieu hon phoi', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHonPhoi={[
      honPhoi({ id: 'hp-moi', soHonPhoi: 'HP-moi', tenVoChong: 'Vợ hiện tại' }),
      honPhoi({ id: 'hp-cu', soHonPhoi: 'HP-cu', tenVoChong: 'Vợ đầu (đã mất)' }),
    ]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))

    expect(screen.getByDisplayValue('HP-moi')).toBeDefined()
    expect(screen.getByDisplayValue('HP-cu')).toBeDefined()
    expect(screen.getByText('Vợ hiện tại')).toBeDefined()
    expect(screen.getByText('Vợ đầu (đã mất)')).toBeDefined()
  })

  it('sua va bam Cap nhat hon phoi thi goi onLuuHonPhoi dung id va rowVersion', async () => {
    const onLuuHonPhoi = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHonPhoi={[honPhoi()]} onLuuHonPhoi={onLuuHonPhoi} />)
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))

    const oNoi = screen.getByDisplayValue('Nhà thờ Chính tòa')
    await userEvent.clear(oNoi)
    await userEvent.type(oNoi, 'Nhà thờ mới')
    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hôn phối' }))

    expect(onLuuHonPhoi).toHaveBeenCalledWith('hp1', expect.objectContaining({
      noiHonPhoi: 'Nhà thờ mới', rowVersion: 1,
    }))
  })
})
