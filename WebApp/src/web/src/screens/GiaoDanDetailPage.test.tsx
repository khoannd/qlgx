import { act, fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { GiaoDanDetailPage } from './GiaoDanDetailPage'
import { api, LoiXungDot } from '../api/client'
import { banNhapKhoa, docBanNhap, luuBanNhap } from '../lib/banNhap'
import type { GiaoDanDetail as ChiTiet } from '../api/types'

vi.mock('../api/client', async () => {
  const actual = await vi.importActual<typeof import('../api/client')>('../api/client')
  return {
    LoiXungDot: actual.LoiXungDot,
    api: {
      giaoDan: {
        chiTiet: vi.fn(), capNhat: vi.fn(), taoMoi: vi.fn(), xoa: vi.fn(),
        honPhoi: vi.fn().mockResolvedValue([]), capNhatHonPhoi: vi.fn(),
        tanHien: vi.fn().mockResolvedValue([]), themTanHien: vi.fn(), capNhatTanHien: vi.fn(),
        hoiDoan: vi.fn().mockResolvedValue([]), themHoiDoan: vi.fn(), capNhatHoiDoan: vi.fn(),
      },
      hoiDoan: {
        danhMuc: vi.fn().mockResolvedValue([]),
      },
      giaoHo: {
        danhMuc: vi.fn().mockResolvedValue([]),
      },
    },
  }
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

describe('GiaoDanDetailPage', () => {
  afterEach(() => {
    localStorage.clear()
    vi.useRealTimers()
  })

  it('tai chi tiet that tu API va hien dung ten', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())

    render(<GiaoDanDetailPage id="p1" />)

    expect(screen.getByRole('status')).toBeDefined()
    expect(await screen.findByRole('heading', { name: /Vũ Minh Trí/ })).toBeDefined()
    expect(api.giaoDan.chiTiet).toHaveBeenCalledWith('p1')
  })

  it('luu thanh cong thi goi PUT va tai lai chi tiet', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.capNhat).mockResolvedValue({ id: 'p1', canhBao: [] })

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(api.giaoDan.capNhat).toHaveBeenCalledWith('p1', expect.objectContaining({ rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaoDan.chiTiet).toHaveBeenCalledTimes(2)
  })

  it('xung dot RowVersion (409) hien thong bao tieng Viet', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.capNhat).mockRejectedValue(
      new LoiXungDot('Giáo dân này vừa được người khác cập nhật.'),
    )

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    expect(await screen.findByText('Giáo dân này vừa được người khác cập nhật.')).toBeDefined()
  })

  // --- Task 15: tab Hôn phối nối API thật ---

  it('tai danh sach hon phoi that tu API va hien trong tab', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.honPhoi).mockResolvedValue([{
      id: 'hp1', tenHonPhoi: 'Giuse Trí - Maria Thu', soHonPhoi: '12/2018', ngayHonPhoi: '2018-05-01',
      noiHonPhoi: 'Nhà thờ Chính tòa', linhMucChung: null, nguoiChung1: null, nguoiChung2: null,
      cachThucHonPhoi: null, ghiChu: null, voChongId: 'vc1', tenVoChong: 'Maria Thu', rowVersion: 1,
    }])

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))

    expect(await screen.findByDisplayValue('12/2018')).toBeDefined()
    expect(api.giaoDan.honPhoi).toHaveBeenCalledWith('p1')
  })

  it('luu hon phoi thanh cong thi goi PUT dung id va tai lai danh sach', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.honPhoi).mockResolvedValue([{
      id: 'hp1', tenHonPhoi: null, soHonPhoi: 'So cu', ngayHonPhoi: null, noiHonPhoi: null,
      linhMucChung: null, nguoiChung1: null, nguoiChung2: null, cachThucHonPhoi: null,
      ghiChu: null, voChongId: 'vc1', tenVoChong: 'Maria Thu', rowVersion: 1,
    }])
    vi.mocked(api.giaoDan.capNhatHonPhoi).mockResolvedValue(undefined)

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))
    await screen.findByDisplayValue('So cu')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hôn phối' }))

    expect(api.giaoDan.capNhatHonPhoi).toHaveBeenCalledWith('hp1',
      expect.objectContaining({ soHonPhoi: 'So cu', rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaoDan.honPhoi).toHaveBeenCalledTimes(2)
  })

  it('xung dot RowVersion khi luu hon phoi hien thong bao tieng Viet', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.honPhoi).mockResolvedValue([{
      id: 'hp1', tenHonPhoi: null, soHonPhoi: 'So cu', ngayHonPhoi: null, noiHonPhoi: null,
      linhMucChung: null, nguoiChung1: null, nguoiChung2: null, cachThucHonPhoi: null,
      ghiChu: null, voChongId: null, tenVoChong: null, rowVersion: 1,
    }])
    vi.mocked(api.giaoDan.capNhatHonPhoi).mockRejectedValue(
      new LoiXungDot('Thông tin hôn phối này vừa được người khác cập nhật.'),
    )

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hôn phối' }))
    await screen.findByDisplayValue('So cu')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hôn phối' }))

    expect(await screen.findByText('Thông tin hôn phối này vừa được người khác cập nhật.')).toBeDefined()
  })

  // --- tab "Ơn gọi tận hiến" nối API thật ---

  it('tai danh sach tan hien that tu API va hien trong tab', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.tanHien).mockResolvedValue([{
      id: 'th1', ngayBatDau: '2009-09-01', chucVu: 'Chủng sinh', noiTu: 'Dòng Tên', dongTu: 'Dòng Tên VN',
      noiPhucVu: null, diaChiPhucVu: null, dienThoaiPhucVu: null, emailPhucVu: null, ghiChu: null,
      daHoiTuc: false, ngayVaoDCV: null, ngayVaoNhaThu: null, ngayVaoNhaTap: null,
      ngayVaoKhanLanDau: null, ngayVaoKhanTronDoi: null, ngayPhoTe: null, ngayThuPhongLM: null,
      ngayBonMang: null, rowVersion: 1,
    }])

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    expect(await screen.findByDisplayValue('Dòng Tên VN')).toBeDefined()
    expect(api.giaoDan.tanHien).toHaveBeenCalledWith('p1')
  })

  it('luu on goi tan hien thanh cong thi goi PUT dung id va tai lai danh sach', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.tanHien).mockResolvedValue([{
      id: 'th1', ngayBatDau: null, chucVu: 'Tu sĩ', noiTu: null, dongTu: 'Dòng cũ',
      noiPhucVu: null, diaChiPhucVu: null, dienThoaiPhucVu: null, emailPhucVu: null, ghiChu: null,
      daHoiTuc: false, ngayVaoDCV: null, ngayVaoNhaThu: null, ngayVaoNhaTap: null,
      ngayVaoKhanLanDau: null, ngayVaoKhanTronDoi: null, ngayPhoTe: null, ngayThuPhongLM: null,
      ngayBonMang: null, rowVersion: 1,
    }])
    vi.mocked(api.giaoDan.capNhatTanHien).mockResolvedValue(undefined)

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))
    await screen.findByDisplayValue('Dòng cũ')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật ơn gọi tận hiến' }))

    expect(api.giaoDan.capNhatTanHien).toHaveBeenCalledWith('th1',
      expect.objectContaining({ dongTu: 'Dòng cũ', rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaoDan.tanHien).toHaveBeenCalledTimes(2)
  })

  it('them giai doan tan hien moi thi goi POST roi tai lai danh sach', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.tanHien).mockResolvedValue([])
    vi.mocked(api.giaoDan.themTanHien).mockResolvedValue({ id: 'th-moi' })

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Ơn gọi tận hiến' }))

    await userEvent.type(screen.getByLabelText('Dòng tu/chủng viện'), 'Dòng mới')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm giai đoạn mới' }))

    expect(api.giaoDan.themTanHien).toHaveBeenCalledWith('p1',
      expect.objectContaining({ dongTu: 'Dòng mới' }))
    expect(await screen.findByText('Đã thêm giai đoạn mới.')).toBeDefined()
    expect(api.giaoDan.tanHien).toHaveBeenCalledTimes(2)
  })

  // --- tab "Hội đoàn" nối API thật ---

  it('tai danh muc va danh sach hoi doan that tu API va hien trong tab', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.hoiDoan).mockResolvedValue([{
      id: 'hd1', hoiDoanId: 'hoidoan1', tenHoiDoan: 'Legio Mariae',
      ngayVaoHoiDoan: '2015-01-01', ngayRaHoiDoan: null, vaiTro: 'Hội viên', rowVersion: 1,
    }])
    vi.mocked(api.hoiDoan.danhMuc).mockResolvedValue([
      { id: 'hoidoan1', tenHoiDoan: 'Legio Mariae' },
      { id: 'hoidoan2', tenHoiDoan: 'Gia trưởng' },
    ])

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))

    expect(await screen.findByRole('heading', { name: 'Legio Mariae' })).toBeDefined()
    expect(api.giaoDan.hoiDoan).toHaveBeenCalledWith('p1')
    expect(api.hoiDoan.danhMuc).toHaveBeenCalled()
  })

  it('luu hoi doan thanh cong thi goi PUT dung id va tai lai danh sach', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.hoiDoan).mockResolvedValue([{
      id: 'hd1', hoiDoanId: 'hoidoan1', tenHoiDoan: 'Legio Mariae',
      ngayVaoHoiDoan: '2015-01-01', ngayRaHoiDoan: null, vaiTro: 'Hội viên', rowVersion: 1,
    }])
    vi.mocked(api.giaoDan.capNhatHoiDoan).mockResolvedValue(undefined)

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))
    await screen.findByDisplayValue('Hội viên')

    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật hội đoàn' }))

    expect(api.giaoDan.capNhatHoiDoan).toHaveBeenCalledWith('hd1',
      expect.objectContaining({ vaiTro: 'Hội viên', rowVersion: 1 }))
    expect(await screen.findByText('Đã lưu thành công.')).toBeDefined()
    expect(api.giaoDan.hoiDoan).toHaveBeenCalledTimes(2)
  })

  it('them hoi doan moi thi goi POST roi tai lai danh sach', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.hoiDoan).mockResolvedValue([])
    vi.mocked(api.hoiDoan.danhMuc).mockResolvedValue([
      { id: 'hoidoan1', tenHoiDoan: 'Legio Mariae' },
    ])
    vi.mocked(api.giaoDan.themHoiDoan).mockResolvedValue({ id: 'hd-moi' })

    render(<GiaoDanDetailPage id="p1" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('tab', { name: 'Hội đoàn' }))
    await screen.findByLabelText('Tên hội đoàn')

    await userEvent.selectOptions(screen.getByLabelText('Tên hội đoàn'), 'hoidoan1')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm hội đoàn' }))

    expect(api.giaoDan.themHoiDoan).toHaveBeenCalledWith('p1',
      expect.objectContaining({ hoiDoanId: 'hoidoan1' }))
    expect(await screen.findByText('Đã thêm hội đoàn mới.')).toBeDefined()
    expect(api.giaoDan.hoiDoan).toHaveBeenCalledTimes(2)
  })

  // --- Task "ghi giao dan": tao moi qua POST -----------------------------------------------

  it('id=null hien form trong voi nut Them giao dan hoat dong duoc', () => {
    render(<GiaoDanDetailPage id={null} />)

    expect(screen.getByRole('button', { name: 'Thêm giáo dân' })).toHaveProperty('disabled', false)
  })

  it('tao moi thanh cong (khong canh bao) thi goi POST va mo the giao dan vua tao', async () => {
    vi.mocked(api.giaoDan.taoMoi).mockResolvedValue({ id: 'gd-moi', canhBao: [] })
    const moGiaoDan = vi.fn()

    render(<GiaoDanDetailPage id={null} moGiaoDan={moGiaoDan} />)
    await userEvent.type(screen.getByLabelText('Họ tên'), 'Nguyễn Văn Mới')
    await userEvent.selectOptions(screen.getByLabelText('Giới tính'), 'Nam')
    await userEvent.type(screen.getByLabelText('Ngày sinh'), '01/01/2000')

    await userEvent.click(screen.getByRole('button', { name: 'Thêm giáo dân' }))

    expect(api.giaoDan.taoMoi).toHaveBeenCalledWith(expect.objectContaining({ hoTen: 'Nguyễn Văn Mới' }))
    expect(await screen.findByText('Đã tạo giáo dân mới.')).toBeDefined()
    expect(moGiaoDan).toHaveBeenCalledWith('gd-moi')
  })

  it('tao moi co canh bao: xac nhan thi goi lai POST voi boQuaCanhBao=true', async () => {
    vi.mocked(api.giaoDan.taoMoi)
      .mockResolvedValueOnce({ id: null, canhBao: ['Đã có giáo dân cùng họ tên, tên thánh và ngày sinh trong hệ thống.'] })
      .mockResolvedValueOnce({ id: 'gd-moi', canhBao: [] })
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    const moGiaoDan = vi.fn()

    render(<GiaoDanDetailPage id={null} moGiaoDan={moGiaoDan} />)
    await userEvent.type(screen.getByLabelText('Họ tên'), 'Nguyễn Văn Trùng')
    await userEvent.selectOptions(screen.getByLabelText('Giới tính'), 'Nam')
    await userEvent.type(screen.getByLabelText('Ngày sinh'), '01/01/2000')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm giáo dân' }))

    await screen.findByText('Đã tạo giáo dân mới.')
    expect(window.confirm).toHaveBeenCalled()
    expect(api.giaoDan.taoMoi).toHaveBeenCalledTimes(2)
    expect(api.giaoDan.taoMoi).toHaveBeenLastCalledWith(expect.objectContaining({ boQuaCanhBao: true }))
    expect(moGiaoDan).toHaveBeenCalledWith('gd-moi')
  })

  it('tao moi co canh bao: huy thi KHONG goi lai POST va khong mo the moi', async () => {
    vi.mocked(api.giaoDan.taoMoi).mockResolvedValueOnce({
      id: null, canhBao: ['Giáo dân này hiện tại chưa đủ 18 tuổi để kết hôn.'],
    })
    vi.spyOn(window, 'confirm').mockReturnValue(false)
    const moGiaoDan = vi.fn()

    render(<GiaoDanDetailPage id={null} moGiaoDan={moGiaoDan} />)
    await userEvent.type(screen.getByLabelText('Họ tên'), 'Nguyễn Văn Huy')
    await userEvent.selectOptions(screen.getByLabelText('Giới tính'), 'Nam')
    await userEvent.type(screen.getByLabelText('Ngày sinh'), '01/01/2000')
    await userEvent.click(screen.getByRole('button', { name: 'Thêm giáo dân' }))

    expect(await screen.findByText('Đã hủy — chưa lưu giáo dân này.')).toBeDefined()
    expect(api.giaoDan.taoMoi).toHaveBeenCalledTimes(1)
    expect(moGiaoDan).not.toHaveBeenCalled()
  })

  it('tao moi that bai (400) hien thong bao loi tra ve tu may chu', async () => {
    vi.mocked(api.giaoDan.taoMoi).mockRejectedValue(new Error('Hãy nhập Họ tên'))

    render(<GiaoDanDetailPage id={null} />)
    await userEvent.click(screen.getByRole('button', { name: 'Thêm giáo dân' }))

    expect(await screen.findByText('Hãy nhập Họ tên')).toBeDefined()
  })

  // --- Task 16: PWA + bản nháp ngoại tuyến (lib/banNhap.ts) ---------------------------------

  it('tu dong luu ban nhap dinh ky khi dang go (khong luu ngay tung phim go)', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')

    // `shouldAdvanceTime` giữ đồng hồ giả chạy song song thời gian thật — findByRole (polling
    // bằng setTimeout nội bộ của testing-library) vẫn hoạt động bình thường, nhưng advance thủ
    // công bên dưới vẫn nhảy cóc được `setInterval` 5 giây của component mà không phải đợi thật.
    vi.useFakeTimers({ shouldAdvanceTime: true })
    try {
      render(<GiaoDanDetailPage id="p1" tenTaiKhoan="vanphong" />)
      expect(await screen.findByRole('heading', { name: /Vũ Minh Trí/ })).toBeDefined()
      // Để hiệu ứng "chốt mốc gốc" (xem banNhap.ts) của component vừa mount chạy xong TRƯỚC khi
      // giả lập gõ — dưới đồng hồ giả, hiệu ứng có thể bị hoãn sang một tác vụ hẹn giờ (thấy
      // được khi debug), khác hẳn trình duyệt thật (nơi nó luôn chạy xong trong vài mili giây
      // ngay sau khi dựng xong, rất lâu trước khi người dùng kịp gõ gì).
      await act(async () => { await vi.advanceTimersByTimeAsync(0) })

      fireEvent.change(screen.getByLabelText('Ghi chú chung'), { target: { value: 'Ghi chú đang gõ dở' } })

      // Chưa tới chu kỳ (5s) — chưa có gì trong localStorage.
      expect(docBanNhap(khoa, 'vanphong')).toBeNull()

      await act(async () => { await vi.advanceTimersByTimeAsync(5000) })

      const ket = docBanNhap<{ ghiChu: string | null }>(khoa, 'vanphong')
      expect(ket?.duLieu.ghiChu).toBe('Ghi chú đang gõ dở')
    } finally {
      vi.useRealTimers()
    }
  })

  it('co ban nhap cu thi hien banner hoi khoi phuc, KHONG tu dong ap dung', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Bản nháp cũ chưa lưu' })

    render(<GiaoDanDetailPage id="p1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })

    expect(await screen.findByText(/Có bản nháp chưa lưu/)).toBeDefined()
    // Dữ liệu máy chủ vẫn hiện nguyên — KHÔNG bị đè bởi nháp khi chưa xác nhận.
    expect(screen.getByLabelText('Ghi chú chung')).toHaveProperty('value', '')
  })

  it('bam Khoi phuc thi ap dung ban nhap vao form (khong dung dinh gia tri server)', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Bản nháp cũ chưa lưu' })

    render(<GiaoDanDetailPage id="p1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(await screen.findByRole('button', { name: 'Khôi phục' }))

    expect(screen.getByLabelText('Ghi chú chung')).toHaveProperty('value', 'Bản nháp cũ chưa lưu')
    expect(screen.queryByText(/Có bản nháp chưa lưu/)).toBeNull()
  })

  it('bam Bo qua thi xoa ban nhap va giu nguyen du lieu server', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Bản nháp cũ chưa lưu' })

    render(<GiaoDanDetailPage id="p1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(await screen.findByRole('button', { name: 'Bỏ qua' }))

    expect(screen.getByLabelText('Ghi chú chung')).toHaveProperty('value', '')
    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
  })

  it('luu thanh cong thi xoa ban nhap (mo lai khong hoi khoi phuc nham)', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.giaoDan.capNhat).mockResolvedValue({ id: 'p1', canhBao: [] })
    const khoa = banNhapKhoa('giaoDan', 'p1', 'vanphong')
    luuBanNhap(khoa, 'vanphong', { ghiChu: 'Nháp trước khi lưu' })

    render(<GiaoDanDetailPage id="p1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })
    await userEvent.click(screen.getByRole('button', { name: 'Cập nhật' }))

    await screen.findByText('Đã lưu thành công.')
    expect(docBanNhap(khoa, 'vanphong')).toBeNull()
  })

  it('ban nhap cua tai khoan khac khong hien banner (khong lo giua hai tai khoan chung may)', async () => {
    vi.mocked(api.giaoDan.chiTiet).mockResolvedValue(chiTiet())
    const khoa = banNhapKhoa('giaoDan', 'p1', 'nguoiKhac')
    luuBanNhap(khoa, 'nguoiKhac', { ghiChu: 'Của người khác' })

    render(<GiaoDanDetailPage id="p1" tenTaiKhoan="vanphong" />)
    await screen.findByRole('heading', { name: /Vũ Minh Trí/ })

    expect(screen.queryByText(/Có bản nháp chưa lưu/)).toBeNull()
  })
})
