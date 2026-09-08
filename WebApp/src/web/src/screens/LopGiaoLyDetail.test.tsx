import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { LopGiaoLyDetail } from './LopGiaoLyDetail'
import { api } from '../api/client'
import type {
  ChuyenLopXemTruoc, GiaoLyVienLop, HocVienLopGiaoLy, KhoiGiaoLy, LopGiaoLy, NhapHocVienXemTruoc,
} from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaoLy: {
      lop: vi.fn(), hocVien: vi.fn(), giaoLyVien: vi.fn(), khoi: vi.fn(),
      xemTruocChuyenLop: vi.fn(), chuyenLop: vi.fn(),
      mauExcelNhapHocVien: vi.fn(),
      xemTruocNhapHocVien: vi.fn(), nhapHocVien: vi.fn(),
    },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))

const lop: LopGiaoLy = {
  id: 'lop1', maLopCu: 1, tenLop: 'Lớp Rước lễ A', khoiGiaoLyId: 'khoi1', nam: 2026,
  phongHoc: null, ghiChu: null, soHocVien: 2, tenGiaoLyVien: null, rowVersion: 1,
}

const hv = (p: Partial<HocVienLopGiaoLy> = {}): HocVienLopGiaoLy => ({
  chiTietId: 'ct1', giaoDanId: 'gd1', soThuTu: 1, hoTen: 'Trần Văn An', tenThanh: 'Giuse',
  phai: 'Nam', ngaySinh: '2015-01-01', hoanThanh: false, ghiChuGLy: null, rowVersion: 1, ...p,
})

const khoiDich: KhoiGiaoLy = {
  id: 'khoi2', maKhoiCu: 2, tenKhoi: 'Khối Thêm sức', nguoiQuanLyId: null,
  tenNguoiQuanLy: null, ghiChu: null, soLop: 1, rowVersion: 1,
}

const lopDich: LopGiaoLy = {
  id: 'lopDich1', maLopCu: 2, tenLop: 'Lớp Thêm sức B', khoiGiaoLyId: 'khoi2', nam: 2026,
  phongHoc: null, ghiChu: null, soHocVien: 0, tenGiaoLyVien: null, rowVersion: 1,
}

function taiCoBan() {
  vi.mocked(api.giaoLy.lop).mockResolvedValue([lop])
  vi.mocked(api.giaoLy.hocVien).mockResolvedValue([hv({ chiTietId: 'ct1', hoTen: 'Trần Văn An' }), hv({ chiTietId: 'ct2', giaoDanId: 'gd2', hoTen: 'Nguyễn Thị Bình' })])
  vi.mocked(api.giaoLy.giaoLyVien).mockResolvedValue([] as GiaoLyVienLop[])
}

describe('LopGiaoLyDetail — Chuyển lớp hàng loạt', () => {
  it('phai xem truoc va xac nhan dung con so truoc khi goi API ghi that', async () => {
    taiCoBan()
    vi.mocked(api.giaoLy.khoi).mockResolvedValue([khoiDich])
    vi.mocked(api.giaoLy.lop).mockImplementation((khoiId) =>
      Promise.resolve(khoiId === 'khoi2' ? [lopDich] : [lop]))
    const xemTruoc: ChuyenLopXemTruoc = {
      soLuongDaChon: 1, soLuongSeChuyen: 1, soLuongDaCoODichRoi: 0,
      tenLopNguon: 'Lớp Rước lễ A', tenLopDich: 'Lớp Thêm sức B', tenKhoiDich: 'Khối Thêm sức', namDich: 2026,
    }
    vi.mocked(api.giaoLy.xemTruocChuyenLop).mockResolvedValue(xemTruoc)
    vi.mocked(api.giaoLy.chuyenLop).mockResolvedValue({ soLuongDaChuyen: 1 })

    render(<LopGiaoLyDetail id="lop1" khoiId="khoi1" />)

    await screen.findByText('Trần Văn An')
    fireEvent.click(screen.getByRole('button', { name: 'Chuyển lớp' }))

    fireEvent.click(screen.getByRole('checkbox', { name: /Chọn Trần Văn An/ }))
    await screen.findByText('Khối Thêm sức')
    fireEvent.change(screen.getByLabelText('Khối đích'), { target: { value: 'khoi2' } })
    await screen.findByText('Lớp Thêm sức B')
    fireEvent.change(screen.getByLabelText('Lớp đích'), { target: { value: 'lopDich1' } })

    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển lớp' }))

    await waitFor(() => expect(api.giaoLy.xemTruocChuyenLop).toHaveBeenCalledWith(['ct1'], 'lopDich1'))
    expect(api.giaoLy.chuyenLop).not.toHaveBeenCalled()
    await screen.findByText(/Sẽ chuyển/)

    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận chuyển' }))

    await waitFor(() => expect(api.giaoLy.chuyenLop).toHaveBeenCalledWith(['ct1'], 'lopDich1'))
    expect(await screen.findByText(/Đã chuyển 1 học viên/)).toBeDefined()
  })

  it('chua chon hoc vien nao thi bao loi, khong goi xem truoc', async () => {
    taiCoBan()
    vi.mocked(api.giaoLy.khoi).mockResolvedValue([khoiDich])

    render(<LopGiaoLyDetail id="lop1" khoiId="khoi1" />)
    await screen.findByText('Trần Văn An')
    fireEvent.click(screen.getByRole('button', { name: 'Chuyển lớp' }))

    fireEvent.click(screen.getByRole('button', { name: 'Xem trước & chuyển lớp' }))

    expect(screen.getByRole('alert').textContent).toContain('chọn ít nhất 1 học viên')
    expect(api.giaoLy.xemTruocChuyenLop).not.toHaveBeenCalled()
  })
})

describe('LopGiaoLyDetail — Nhập học viên hàng loạt', () => {
  it('phai xem truoc va xac nhan truoc khi goi API nhap that', async () => {
    taiCoBan()
    const xemTruoc: NhapHocVienXemTruoc = {
      tepHopLe: true, loiTep: null, tenLop: 'Lớp Rước lễ A',
      dong: [
        { soDong: 1, maGD: null, tenThanh: 'Maria', hoTen: 'Vũ Thị Chi', phai: 'Nữ',
          ngaySinhHienThi: '01/01/2016', giaoHo: null, ghiChu: null, daHocXong: false,
          laGiaoDanMoi: true, loi: null },
      ],
      soSeNhap: 1, soBiBoQua: 0,
    }
    vi.mocked(api.giaoLy.xemTruocNhapHocVien).mockResolvedValue(xemTruoc)
    vi.mocked(api.giaoLy.nhapHocVien).mockResolvedValue({ soDaNhap: 1, soBiBoQua: 0 })

    render(<LopGiaoLyDetail id="lop1" khoiId="khoi1" />)
    await screen.findByText('Trần Văn An')
    fireEvent.click(screen.getByRole('button', { name: 'Nhập học viên' }))

    const tep = new File(['du lieu'], 'mau.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    fireEvent.change(screen.getByTestId('chon-tep-hoc-vien'), { target: { files: [tep] } })

    fireEvent.click(screen.getByRole('button', { name: 'Xem trước' }))

    await waitFor(() => expect(api.giaoLy.xemTruocNhapHocVien).toHaveBeenCalledWith('lop1', tep))
    expect(api.giaoLy.nhapHocVien).not.toHaveBeenCalled()
    await screen.findByText('Maria Vũ Thị Chi')

    fireEvent.click(screen.getByRole('button', { name: 'Xác nhận nhập 1 học viên' }))

    await waitFor(() => expect(api.giaoLy.nhapHocVien).toHaveBeenCalledWith('lop1', tep))
    expect(await screen.findByText(/Đã nhập 1 học viên/)).toBeDefined()
  })

  it('tep loi dinh dang thi bao loi tu may chu, khong cho xac nhan', async () => {
    taiCoBan()
    vi.mocked(api.giaoLy.xemTruocNhapHocVien).mockResolvedValue({
      tepHopLe: false, loiTep: 'Tệp không đúng định dạng Excel (.xlsx), hoặc tệp bị hỏng.',
      tenLop: 'Lớp Rước lễ A', dong: [], soSeNhap: 0, soBiBoQua: 0,
    })

    render(<LopGiaoLyDetail id="lop1" khoiId="khoi1" />)
    await screen.findByText('Trần Văn An')
    fireEvent.click(screen.getByRole('button', { name: 'Nhập học viên' }))

    const tep = new File(['khong phai excel'], 'gia.txt', { type: 'text/plain' })
    fireEvent.change(screen.getByTestId('chon-tep-hoc-vien'), { target: { files: [tep] } })
    fireEvent.click(screen.getByRole('button', { name: 'Xem trước' }))

    await screen.findByText(/Tệp không đúng định dạng Excel/)
    expect(api.giaoLy.nhapHocVien).not.toHaveBeenCalled()
  })
})
