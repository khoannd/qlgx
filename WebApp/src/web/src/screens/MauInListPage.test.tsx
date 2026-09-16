import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { MauInListPage } from './MauInListPage'
import { api, LoiXungDot } from '../api/client'
import { useAuth } from '../api/AuthContext'
import type {
  MauInDanhSachItem, MauInChiTiet, ThongTinNguoiDung, CachHienThiDungSaiItem,
} from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    mauIn: {
      danhSach: vi.fn(),
      xemThu: vi.fn(),
      rieng: { layChiTiet: vi.fn(), luu: vi.fn(), khoiPhuc: vi.fn() },
      heThong: { layChiTiet: vi.fn(), luu: vi.fn(), khoiPhuc: vi.fn() },
    },
    // Khu vực "Cách hiển thị dữ liệu đúng/sai" nằm CÙNG màn hình này nên mọi bài test dưới đây
    // đều làm nó tải theo — phải mock, nếu không api.cachHienThi là undefined và cả màn hình đỏ.
    cachHienThi: {
      danhSach: vi.fn(),
      rieng: { luu: vi.fn(), khoiPhuc: vi.fn() },
      heThong: { luu: vi.fn(), khoiPhuc: vi.fn() },
    },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))
vi.mock('../api/AuthContext', () => ({ useAuth: vi.fn() }))

const nguoiDung = (loaiTaiKhoan: number): ThongTinNguoiDung => ({
  id: 'u1', tenTaiKhoan: 'test', hoTen: 'Test', loaiTaiKhoan, giaoXuId: 'gx1', tenGiaoXu: 'GX Test',
})

const dong = (p: Partial<MauInDanhSachItem> = {}): MauInDanhSachItem => ({
  tenMau: 'LyLichCaNhan', tenHienThi: 'Lý lịch cá nhân', capDangDung: 'MacDinh',
  choTrong: [{ key: 'HoTen', nhan: 'Họ và tên' }, { key: 'NgaySinh', nhan: 'Ngày sinh' }],
  // bienKhaDung là SIÊU TẬP của choTrong (xem quan-ly-mau-in.md) — combobox "Chèn chỗ trống"
  // dùng tập này, gom theo `nhom` thành <optgroup>.
  bienKhaDung: [
    { key: 'HoTen', nhan: 'Họ và tên', nhom: 'Giáo dân' },
    { key: 'NgaySinh', nhan: 'Ngày sinh', nhom: 'Giáo dân' },
    { key: 'SoRuaToi', nhan: 'Rửa tội — số sổ', nhom: 'Bí tích' },
  ],
  ...p,
})

const chiTiet = (p: Partial<MauInChiTiet> = {}): MauInChiTiet => ({
  tenMau: 'LyLichCaNhan', tenHienThi: 'Lý lịch cá nhân', daTuyChinh: false,
  noiDungHtml: '<html><body><h1>{{HoTen}}</h1></body></html>', rowVersion: 0,
  choTrong: [{ key: 'HoTen', nhan: 'Họ và tên' }, { key: 'NgaySinh', nhan: 'Ngày sinh' }],
  // bienKhaDung là SIÊU TẬP của choTrong (xem quan-ly-mau-in.md) — combobox "Chèn chỗ trống"
  // dùng tập này, gom theo `nhom` thành <optgroup>.
  bienKhaDung: [
    { key: 'HoTen', nhan: 'Họ và tên', nhom: 'Giáo dân' },
    { key: 'NgaySinh', nhan: 'Ngày sinh', nhom: 'Giáo dân' },
    { key: 'SoRuaToi', nhan: 'Rửa tội — số sổ', nhom: 'Bí tích' },
  ],
  ...p,
})

/** Một mục đúng/sai; mặc định là "chưa ai tuỳ chỉnh" (đang dùng mặc định gốc [x]/[  ]). */
const mucDungSai = (p: Partial<CachHienThiDungSaiItem> = {}): CachHienThiDungSaiItem => ({
  tenBien: 'TanTong', nhan: 'Tân tòng', nhom: 'Giáo dân',
  khiDungDangDung: '[x]', khiSaiDangDung: '[  ]', capDangDung: 'MacDinh',
  rieng: { daTuyChinh: false, khiDung: null, khiSai: null, rowVersion: 0 },
  heThong: { daTuyChinh: false, khiDung: null, khiSai: null, rowVersion: 0 },
  ...p,
})

describe('MauInListPage', () => {
  // Khu vực "Cách hiển thị dữ liệu đúng/sai" tải cùng màn hình, nên mọi bài test đều cần nó trả
  // về một mảng hợp lệ. Bài nào quan tâm tới khu vực này thì tự mock lại giá trị của mình.
  beforeEach(() => {
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([])
  })

  it('hien danh sach mau kem huy hieu cap dang dung', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(1) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([
      dong({ tenMau: 'LyLichCaNhan', tenHienThi: 'Lý lịch cá nhân', capDangDung: 'MacDinh' }),
      dong({ tenMau: 'PhieuGiaDinh', tenHienThi: 'Phiếu gia đình', capDangDung: 'TuyChinhGiaoXu' }),
    ])

    render(<MauInListPage />)

    expect(await screen.findByText('Lý lịch cá nhân')).toBeDefined()
    expect(screen.getByText('Phiếu gia đình')).toBeDefined()
    expect(screen.getByText('Mặc định gốc')).toBeDefined()
    expect(screen.getByText('Đã tuỳ chỉnh (riêng giáo xứ)')).toBeDefined()
  })

  it('tai khoan thuong (khong phai QuanTri/QuanTriHeThong) khong thay nut Sua', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(1) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])

    render(<MauInListPage />)

    await screen.findByText('Lý lịch cá nhân')
    expect(screen.queryByText('Sửa')).toBeNull()
  })

  it('QuanTri giao xu bam Sua mo trinh soan mau RIENG', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))

    await waitFor(() => expect(api.mauIn.rieng.layChiTiet).toHaveBeenCalledWith('LyLichCaNhan'))
    expect(api.mauIn.heThong.layChiTiet).not.toHaveBeenCalled()
    expect(await screen.findByText('Sửa mẫu: Lý lịch cá nhân')).toBeDefined()
  })

  it('combobox Chen cho trong liet ke bienKhaDung va gom theo nhom bang optgroup', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')

    const chon = screen.getByLabelText('Chèn chỗ trống:') as HTMLSelectElement
    // Hai nhóm của dữ liệu thử, đúng thứ tự xuất hiện từ máy chủ (không sắp lại a-b-c).
    const nhom = [...chon.querySelectorAll('optgroup')].map((g) => g.label)
    expect(nhom).toEqual(['Giáo dân', 'Bí tích'])
    // Biến mẫu gốc CHƯA dùng ({{SoRuaToi}}) vẫn phải chèn được — đây chính là điều người dùng cần.
    expect(chon.querySelector('optgroup[label="Bí tích"] option')?.textContent)
      .toBe('Rửa tội — số sổ ({{SoRuaToi}})')
  })

  it('QuanTriHeThong bam Sua mo trinh soan mau HE THONG', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(9) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.heThong.layChiTiet).mockResolvedValue(chiTiet())

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))

    await waitFor(() => expect(api.mauIn.heThong.layChiTiet).toHaveBeenCalledWith('LyLichCaNhan'))
    expect(api.mauIn.rieng.layChiTiet).not.toHaveBeenCalled()
  })

  it('bam Xem thu goi dung API voi noi dung dang go', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.mauIn.xemThu).mockResolvedValue(undefined)

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Xem thử'))

    await waitFor(() => expect(api.mauIn.xemThu).toHaveBeenCalledWith('LyLichCaNhan', expect.stringContaining('HoTen')))
  })

  it('bam Luu goi dung API roi bao Da luu', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong()])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet())
    vi.mocked(api.mauIn.rieng.luu).mockResolvedValue(undefined)

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Lưu'))

    await waitFor(() => expect(api.mauIn.rieng.luu).toHaveBeenCalledWith('LyLichCaNhan', expect.any(String), 0))
    expect(await screen.findByText('Đã lưu.')).toBeDefined()
  })

  it('chua tuy chinh thi khong hien nut Khoi phuc; da tuy chinh thi hien va bam duoc', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([dong({ capDangDung: 'TuyChinhGiaoXu' })])
    vi.mocked(api.mauIn.rieng.layChiTiet).mockResolvedValue(chiTiet({ daTuyChinh: true }))
    vi.mocked(api.mauIn.rieng.khoiPhuc).mockResolvedValue(undefined)
    vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<MauInListPage />)
    await screen.findByText('Lý lịch cá nhân')
    fireEvent.click(screen.getByText('Sửa'))
    await screen.findByText('Sửa mẫu: Lý lịch cá nhân')

    fireEvent.click(screen.getByText('Khôi phục về mặc định'))
    await waitFor(() => expect(api.mauIn.rieng.khoiPhuc).toHaveBeenCalledWith('LyLichCaNhan'))
  })
})

/**
 * Khu vực "Cách hiển thị dữ liệu đúng/sai" (xem quan-ly-mau-in.md) — cho giáo xứ đặt câu chữ in
 * ra cho các mục đúng/sai thay vì dấu [x]/[  ].
 */
describe('MauInListPage — cách hiển thị dữ liệu đúng/sai', () => {
  beforeEach(() => {
    vi.mocked(api.mauIn.danhSach).mockResolvedValue([])
  })

  it('hien tung muc kem nhan tieng Viet, hai o nhap va huy hieu cap dang dung', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([
      mucDungSai({ tenBien: 'TanTong', nhan: 'Tân tòng' }),
      mucDungSai({ tenBien: 'ConHoc', nhan: 'Còn đi học', capDangDung: 'TuyChinhHeThong' }),
    ])

    render(<MauInListPage />)

    expect(await screen.findByText('Còn đi học')).toBeDefined()
    expect(screen.getByText('Tân tòng')).toBeDefined()
    // Nhãn ô nhập phải nói rõ thuộc mục nào — người dùng không rành máy tính cần biết đang gõ cho ai.
    expect(screen.getByLabelText('Khi có — Tân tòng')).toBeDefined()
    expect(screen.getByLabelText('Khi không — Tân tòng')).toBeDefined()
    expect(screen.getByText('Mặc định gốc')).toBeDefined()
    expect(screen.getByText('Đã tuỳ chỉnh (hệ thống)')).toBeDefined()
  })

  it('huong dan noi ro de trong thi khong in ra chu gi', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([mucDungSai()])

    render(<MauInListPage />)

    expect(await screen.findByText(/Để trống ô nào thì chỗ đó không in ra chữ gì/)).toBeDefined()
  })

  it('QuanTri giao xu go cau chu roi bam Luu goi API cap RIENG kem rowVersion', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([
      mucDungSai({
        rieng: { daTuyChinh: true, khiDung: 'Cũ', khiSai: '', rowVersion: 77 },
      }),
    ])
    vi.mocked(api.cachHienThi.rieng.luu).mockResolvedValue(undefined)

    render(<MauInListPage />)
    // Chờ chính Ô NHẬP xuất hiện (không chờ một mẩu văn bản tĩnh) — dữ liệu về mới có dòng.
    const oKhiDung = await screen.findByLabelText('Khi có — Tân tòng') as HTMLInputElement
    expect(oKhiDung.value).toBe('Cũ')

    fireEvent.change(oKhiDung, { target: { value: 'Tân tòng' } })
    fireEvent.click(screen.getByText('Lưu'))

    await waitFor(() => expect(api.cachHienThi.rieng.luu)
      .toHaveBeenCalledWith('TanTong', 'Tân tòng', '', 77))
  })

  it('QuanTriHeThong bam Luu goi API cap HE THONG, khong phai cap rieng', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(9) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([
      mucDungSai({
        // Giáo xứ đã đè riêng, nhưng cấp hệ thống chưa đặt gì — ô phải TRỐNG, không được mượn
        // câu chữ của cấp khác.
        capDangDung: 'TuyChinhGiaoXu',
        rieng: { daTuyChinh: true, khiDung: 'Chữ của giáo xứ', khiSai: '', rowVersion: 5 },
        heThong: { daTuyChinh: false, khiDung: null, khiSai: null, rowVersion: 0 },
      }),
    ])
    vi.mocked(api.cachHienThi.heThong.luu).mockResolvedValue(undefined)

    render(<MauInListPage />)
    const oKhiDung = await screen.findByLabelText('Khi có — Tân tòng') as HTMLInputElement
    expect(oKhiDung.value).toBe('')

    fireEvent.change(oKhiDung, { target: { value: 'Tân tòng (chung)' } })
    fireEvent.click(screen.getByText('Lưu'))

    await waitFor(() => expect(api.cachHienThi.heThong.luu)
      .toHaveBeenCalledWith('TanTong', 'Tân tòng (chung)', '', 0))
    expect(api.cachHienThi.rieng.luu).not.toHaveBeenCalled()
  })

  it('nut Khoi phuc chi hien khi CAP DANG SUA da tuy chinh', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([mucDungSai()])

    render(<MauInListPage />)
    await screen.findByLabelText('Khi có — Tân tòng')
    expect(screen.queryByText('Khôi phục mặc định')).toBeNull()
  })

  it('da tuy chinh thi hien nut Khoi phuc va bam duoc', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([
      mucDungSai({
        capDangDung: 'TuyChinhGiaoXu',
        rieng: { daTuyChinh: true, khiDung: 'Tân tòng', khiSai: '', rowVersion: 9 },
      }),
    ])
    vi.mocked(api.cachHienThi.rieng.khoiPhuc).mockResolvedValue(undefined)
    vi.spyOn(window, 'confirm').mockReturnValue(true)

    render(<MauInListPage />)
    fireEvent.click(await screen.findByText('Khôi phục mặc định'))
    await waitFor(() => expect(api.cachHienThi.rieng.khoiPhuc).toHaveBeenCalledWith('TanTong'))
  })

  it('tai khoan thuong chi xem cau chu dang dung, khong co o nhap va nut Luu', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(1) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([
      // Câu chữ đang dùng CỐ Ý khác nhãn mục ("Tân tòng") để bài test phân biệt được hai chỗ —
      // đúng ví dụ thứ hai người dùng nêu ("Là tân tòng: đúng").
      mucDungSai({ khiDungDangDung: 'Là tân tòng: đúng', khiSaiDangDung: '' }),
    ])

    render(<MauInListPage />)
    // Tài khoản thường không có ô nhập, nên chờ theo NHÃN MỤC trong bảng.
    await screen.findByText('Tân tòng')

    // Chỉ ĐỌC được câu chữ đang áp dụng, không có ô nhập và không có nút Lưu.
    expect(screen.getByText('Là tân tòng: đúng')).toBeDefined()
    expect(screen.queryByLabelText('Khi có — Tân tòng')).toBeNull()
    expect(screen.queryByText('Lưu')).toBeNull()
  })

  it('bao loi xung dot phien ban ngay tren dong do', async () => {
    vi.mocked(useAuth).mockReturnValue({ nguoiDung: nguoiDung(0) } as unknown as ReturnType<typeof useAuth>)
    vi.mocked(api.cachHienThi.danhSach).mockResolvedValue([mucDungSai()])
    vi.mocked(api.cachHienThi.rieng.luu).mockRejectedValue(
      new LoiXungDot('Mục này vừa được người khác cập nhật.'))

    render(<MauInListPage />)
    await screen.findByLabelText('Khi có — Tân tòng')

    fireEvent.click(screen.getByText('Lưu'))

    expect(await screen.findByText('Mục này vừa được người khác cập nhật.')).toBeDefined()
  })
})
