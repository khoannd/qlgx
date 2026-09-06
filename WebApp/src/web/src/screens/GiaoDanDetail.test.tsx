import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GiaoDanDetail } from './GiaoDanDetail'
import type {
  GiaoDanDetail as ChiTiet, HoiDoanCuaGiaoDan, HoiDoanDanhMuc, HonPhoiCuaGiaoDan,
  TanHienCuaGiaoDan,
} from '../api/types'

const honPhoi = (p: Partial<HonPhoiCuaGiaoDan> = {}): HonPhoiCuaGiaoDan => ({
  id: 'hp1', tenHonPhoi: 'Giuse Trí - Maria Thu', soHonPhoi: '12/2018',
  ngayHonPhoi: '2018-05-01', noiHonPhoi: 'Nhà thờ Chính tòa', linhMucChung: 'Lm. Nguyễn Văn A',
  nguoiChung1: 'Ông B', nguoiChung2: 'Bà C', cachThucHonPhoi: 'Hợp pháp',
  ghiChu: 'Không có gì đặc biệt', voChongId: 'vc1', tenVoChong: 'Maria Thu',
  rowVersion: 1, ...p,
})

const tanHien = (p: Partial<TanHienCuaGiaoDan> = {}): TanHienCuaGiaoDan => ({
  id: 'th1', ngayBatDau: '2009-09-01', chucVu: 'Chủng sinh', noiTu: 'Dòng Tên',
  dongTu: 'Dòng Tên Việt Nam', noiPhucVu: 'Giáo xứ Thánh Tâm', diaChiPhucVu: '123 Nguyễn Trãi',
  dienThoaiPhucVu: '0900000000', emailPhucVu: 'thay@dongten.vn', ghiChu: 'Đang học triết',
  daHoiTuc: false, ngayVaoDCV: '2015-09-01', ngayVaoNhaThu: '2010-09-01',
  ngayVaoNhaTap: '2011-09-01', ngayVaoKhanLanDau: '2012-09-01', ngayVaoKhanTronDoi: null,
  ngayPhoTe: null, ngayThuPhongLM: null, ngayBonMang: '2023-03-19', rowVersion: 1, ...p,
})

const hoiDoan = (p: Partial<HoiDoanCuaGiaoDan> = {}): HoiDoanCuaGiaoDan => ({
  id: 'hd1', hoiDoanId: 'hoidoan1', tenHoiDoan: 'Legio Mariae',
  ngayVaoHoiDoan: '2015-01-01', ngayRaHoiDoan: null, vaiTro: 'Hội viên', rowVersion: 1, ...p,
})

const danhMucHD: HoiDoanDanhMuc[] = [
  { id: 'hoidoan1', tenHoiDoan: 'Legio Mariae' },
  { id: 'hoidoan2', tenHoiDoan: 'Gia trưởng' },
]

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

  // --- tab "Ơn gọi tận hiến" nối API thật (trước đây chỉ là khung tĩnh) ---

  it('chua co tan hien nao thi van hien khoi Them giai doan moi', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachTanHien={[]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    expect(screen.getByRole('button', { name: 'Thêm giai đoạn mới' })).toBeDefined()
  })

  it('hien du cac truong cua mot ban ghi tan hien da co', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachTanHien={[tanHien()]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    expect(screen.getByDisplayValue('Dòng Tên')).toBeDefined()
    expect(screen.getByDisplayValue('Dòng Tên Việt Nam')).toBeDefined()
    expect(screen.getByDisplayValue('Giáo xứ Thánh Tâm')).toBeDefined()
    expect(screen.getByDisplayValue('123 Nguyễn Trãi')).toBeDefined()
    expect(screen.getByDisplayValue('0900000000')).toBeDefined()
    expect(screen.getByDisplayValue('thay@dongten.vn')).toBeDefined()
    expect(screen.getByDisplayValue('Đang học triết')).toBeDefined()
    expect(screen.getByDisplayValue('2009-09-01')).toBeDefined()
    expect(screen.getByDisplayValue('2015-09-01')).toBeDefined()
    expect(screen.getByDisplayValue('2023-03-19')).toBeDefined()
  })

  it('co nhieu giai doan tan hien thi hien het', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachTanHien={[
      tanHien({ id: 'th-cu', chucVu: 'Tu sĩ', dongTu: 'Dòng A' }),
      tanHien({ id: 'th-moi', chucVu: 'Linh mục', dongTu: 'Dòng B' }),
    ]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    expect(screen.getByDisplayValue('Dòng A')).toBeDefined()
    expect(screen.getByDisplayValue('Dòng B')).toBeDefined()
  })

  it('sua va bam Cap nhat on goi tan hien thi goi onLuuTanHien dung id va rowVersion', async () => {
    const onLuuTanHien = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachTanHien={[tanHien()]} onLuuTanHien={onLuuTanHien} />)
    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    const oDongTu = screen.getByDisplayValue('Dòng Tên Việt Nam')
    await userEvent.clear(oDongTu)
    await userEvent.type(oDongTu, 'Dòng mới')
    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật ơn gọi tận hiến' }))

    expect(onLuuTanHien).toHaveBeenCalledWith('th1', expect.objectContaining({
      dongTu: 'Dòng mới', rowVersion: 1,
    }))
  })

  it('nhap khoi Them giai doan moi roi bam Them thi goi onThemTanHien', async () => {
    const onThemTanHien = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachTanHien={[]} onThemTanHien={onThemTanHien} />)
    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    await userEvent.type(screen.getByLabelText('Dòng tu/chủng viện'), 'Dòng Chúa Cứu Thế')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm giai đoạn mới' }))

    expect(onThemTanHien).toHaveBeenCalledWith(expect.objectContaining({
      dongTu: 'Dòng Chúa Cứu Thế',
    }))
  })

  // --- tab "Hội đoàn" nối API thật (trước đây chỉ là khung tĩnh) ---

  it('chua tham gia hoi doan nao thi bao chua co, van hien khoi Them hoi doan', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHoiDoan={[]} danhMucHoiDoan={danhMucHD} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))

    expect(screen.getByText(/chưa tham gia hội đoàn nào/i)).toBeDefined()
    expect(screen.getByText('Thêm vào hội đoàn')).toBeDefined()
  })

  it('hien du cac truong cua mot luot tham gia hoi doan da co', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHoiDoan={[hoiDoan()]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))

    expect(screen.getByText('Legio Mariae')).toBeDefined()
    expect(screen.getByDisplayValue('2015-01-01')).toBeDefined()
    expect(screen.getByDisplayValue('Hội viên')).toBeDefined()
  })

  it('tham gia nhieu hoi doan thi hien het', async () => {
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHoiDoan={[
      hoiDoan({ id: 'hd1', tenHoiDoan: 'Legio Mariae' }),
      hoiDoan({ id: 'hd2', tenHoiDoan: 'Gia trưởng' }),
    ]} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))

    expect(screen.getByText('Legio Mariae')).toBeDefined()
    expect(screen.getByText('Gia trưởng')).toBeDefined()
  })

  it('sua va bam Cap nhat hoi doan thi goi onLuuHoiDoan dung id va rowVersion', async () => {
    const onLuuHoiDoan = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHoiDoan={[hoiDoan()]} onLuuHoiDoan={onLuuHoiDoan} />)
    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))

    const oVaiTro = screen.getByDisplayValue('Hội viên')
    await userEvent.clear(oVaiTro)
    await userEvent.type(oVaiTro, 'Trưởng hội đoàn')
    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hội đoàn' }))

    expect(onLuuHoiDoan).toHaveBeenCalledWith('hd1', expect.objectContaining({
      vaiTro: 'Trưởng hội đoàn', rowVersion: 1,
    }))
  })

  it('chon hoi doan tu danh muc roi bam Them thi goi onThemHoiDoan', async () => {
    const onThemHoiDoan = vi.fn().mockResolvedValue(undefined)
    render(<GiaoDanDetail duLieu={chiTiet()} danhSachHoiDoan={[]}
      danhMucHoiDoan={danhMucHD} onThemHoiDoan={onThemHoiDoan} />)
    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))

    await userEvent.selectOptions(screen.getByLabelText('Tên hội đoàn'), 'hoidoan2')
    await userEvent.type(screen.getByLabelText('Ngày vào hội đoàn'), '01/01/2020')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm hội đoàn' }))

    expect(onThemHoiDoan).toHaveBeenCalledWith(expect.objectContaining({
      hoiDoanId: 'hoidoan2', ngayVaoHoiDoan: '2020-01-01',
    }))
  })

  // --- Tab Giao ly (review-frontend "chan cung #4"): truoc day cac o nay hoan toan tinh,
  // khong name/khong defaultValue nen go vao roi mat. -------------------------------------

  it('tab Giao ly hien dung du lieu da luu (khong con tinh)', async () => {
    render(<GiaoDanDetail duLieu={chiTiet({
      ngayBD1: '2010-06-01', noiBD1: 'GX Vo Nhiem',
      ngayBD2: '2011-06-01', noiBD2: 'GX Vo Nhiem 2',
      ngayTHVaoDoi: '2012-06-01', noiTHVaoDoi: 'GX Vo Nhiem 3',
      ngayGLHN1: '2020-01-10', ngayGLHN2: '2020-02-10',
      noiGLHN: 'GX Vo Nhiem 4', nguoiChungNhanGLHN: 'Cha Giuse', xepLoaiGLHN: 'Khá',
    })} />)

    await userEvent.click(screen.getByRole('tab', { name: 'Giáo lý' }))

    expect(screen.getByDisplayValue('GX Vo Nhiem')).toBeDefined()
    expect(screen.getByDisplayValue('GX Vo Nhiem 2')).toBeDefined()
    expect(screen.getByDisplayValue('GX Vo Nhiem 3')).toBeDefined()
    expect(screen.getByDisplayValue('GX Vo Nhiem 4')).toBeDefined()
    expect(screen.getByDisplayValue('Cha Giuse')).toBeDefined()
    expect((screen.getByLabelText('Xếp loại') as HTMLSelectElement).value).toBe('Khá')
  })

  it('nhap tab Giao ly roi bam Cap nhat thi onLuu nhan dung cac truong da go', async () => {
    const onLuu = vi.fn()
    const nguoiDung = userEvent.setup()
    const { container } = render(<GiaoDanDetail duLieu={chiTiet()} onLuu={onLuu} />)

    await nguoiDung.click(screen.getByRole('tab', { name: 'Giáo lý' }))
    await nguoiDung.type(container.querySelector('#gd-gl-bd1-gx')!, 'GX Thanh Tam')
    await nguoiDung.type(screen.getByLabelText('Người cấp chứng nhận'), 'Cha Phêrô')
    await nguoiDung.selectOptions(screen.getByLabelText('Xếp loại'), 'Giỏi')
    await nguoiDung.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu).toHaveBeenCalledWith(expect.objectContaining({
      noiBD1: 'GX Thanh Tam', nguoiChungNhanGLHN: 'Cha Phêrô', xepLoaiGLHN: 'Giỏi',
    }))
  })

  // --- Thong tin chuyen xu (review-frontend "chan cung #4" phan con lai): truoc day chi co
  // mot select khong ten, khong luu duoc gi. -----------------------------------------------

  it('chua co ban ghi ChuyenXu thi mac dinh Cho tai xu, an cac o Ngay/Noi/Ghi chu', () => {
    render(<GiaoDanDetail duLieu={chiTiet({ giaoHoId: 'gh1' })} />)

    expect(screen.queryByLabelText('Ngày chuyển')).toBeNull()
    expect(screen.queryByLabelText('Nơi chuyển')).toBeNull()
  })

  it('da co ban ghi Chuyen den thi hien dung du lieu o ba o con lai', () => {
    render(<GiaoDanDetail duLieu={chiTiet({
      giaoHoId: 'gh1',
      chuyenXu: { id: 'cx1', loaiChuyen: 1, ngayChuyen: '2021-03-01', noiChuyen: 'GX Thanh Tam', ghiChuChuyen: 'Ghi chu X', rowVersion: 3 },
    })} />)

    expect(screen.getByLabelText('Nơi chuyển')).toHaveProperty('value', 'GX Thanh Tam')
    expect(screen.getByLabelText('Ghi chú', { selector: '#gd-ghichuchuyenxu' })).toHaveProperty('value', 'Ghi chu X')
  })

  it('doi Thong tin hien tai sang Chuyen den thi hien them 3 o, bam Cap nhat gui dung chuyenXu', async () => {
    const onLuu = vi.fn()
    const nguoiDung = userEvent.setup()
    render(<GiaoDanDetail duLieu={chiTiet({ giaoHoId: 'gh1' })} onLuu={onLuu} />)

    await nguoiDung.selectOptions(screen.getByLabelText('Thông tin hiện tại'), '1')
    await nguoiDung.type(screen.getByLabelText('Nơi chuyển'), 'GX Vo Nhiem')
    await nguoiDung.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu).toHaveBeenCalledWith(expect.objectContaining({
      chuyenXu: expect.objectContaining({ loaiChuyen: 1, noiChuyen: 'GX Vo Nhiem' }),
    }))
  })

  it('Ngoai xu thi khong hien khoi Thong tin chuyen xu, onLuu nhan chuyenXu:null (khong dong toi)', async () => {
    const onLuu = vi.fn()
    render(<GiaoDanDetail duLieu={chiTiet({ giaoHoId: null })} onLuu={onLuu} />)

    expect(screen.queryByText('Thông tin chuyển xứ')).toBeNull()

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(onLuu).toHaveBeenCalledWith(expect.objectContaining({ chuyenXu: null }))
  })

  // --- Task "ghi giao dan": tao moi ------------------------------------------------------

  it('ban ghi moi (khong co duLieu) van cho phep bam nut Them giao dan khi co onLuu', () => {
    render(<GiaoDanDetail onLuu={vi.fn()} />)

    expect(screen.getByRole('button', { name: 'Thêm giáo dân' })).toHaveProperty('disabled', false)
  })

  it('nhap thong tin ban ghi moi roi bam Them giao dan thi goi onLuu voi payload dung', async () => {
    const onLuu = vi.fn()
    render(<GiaoDanDetail onLuu={onLuu} />)

    await userEvent.type(screen.getByLabelText('Họ tên'), 'Nguyễn Văn Mới')
    await userEvent.selectOptions(screen.getByLabelText('Giới tính'), 'Nam')
    await userEvent.type(screen.getByLabelText('Ngày sinh'), '01/01/2000')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm giáo dân' }))

    expect(onLuu).toHaveBeenCalledWith(expect.objectContaining({
      hoTen: 'Nguyễn Văn Mới', phai: 'Nam', ngaySinh: '2000-01-01', boQuaCanhBao: false,
    }))
  })

  it('khong co onLuu thi nut Them giao dan bi vo hieu hoa', () => {
    render(<GiaoDanDetail />)

    expect(screen.getByRole('button', { name: 'Thêm giáo dân' })).toHaveProperty('disabled', true)
  })
})
