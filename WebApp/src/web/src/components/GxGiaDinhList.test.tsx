import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GxGiaDinhList, menuGiaDinhMacDinh } from './GxGiaDinhList'
import { api } from '../api/client'
import type { GiaDinhListItem } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    giaDinh: {
      inChungNhanHonPhoi: vi.fn(() => Promise.resolve()),
      inPhieuGiaDinh: vi.fn(() => Promise.resolve()),
      inLyLichCaNhanGiaDinh: vi.fn(() => Promise.resolve()),
    },
  },
}))

const giaDinh = (p: Partial<GiaDinhListItem> = {}): GiaDinhListItem => ({
  id: 'g1', maGiaDinhCu: 12, maGiaDinhRieng: null, tenGiaDinh: 'Nguyễn Văn A',
  tenChong: 'Giuse Nguyễn Văn A', tenVo: 'Maria Trần Thị B', soLuong: 4,
  dienThoai: null, dtChong: null, dtVo: null, diaChi: null, tenGiaoHo: null,
  dienGiaDinh: 'Nghèo', ghiChu: null, gach: -1, khongThongKe: false, ...p,
})

// Ô "Người nam"/"Người nữ" được gạch qua cellClass (class `struck`, đã có sẵn trong
// qlgx.css) chứ không tô cả dòng như GxGiaoDanList — kiểm bằng cách tìm ô chứa tên rồi
// đọc class của phần tử `.ag-cell` chứa nó.
const layClassOCell = (ten: string) => screen.getByText(ten).closest('.ag-cell')?.className ?? ''

describe('GxGiaDinhList', () => {
  it('hien du 12 cot voi dung nhan cua ban desktop', async () => {
    render(<GxGiaDinhList rows={[giaDinh()]} />)

    expect(await screen.findByText('Mã GĐ')).toBeDefined()
    expect(screen.getByText('Người nam')).toBeDefined()
    expect(screen.getByText('Người nữ')).toBeDefined()
    expect(screen.getByText('Diện gia đình')).toBeDefined()
  })

  it('gach = 0 thi gach o Nguoi nam, khong gach o Nguoi nu', async () => {
    render(<GxGiaDinhList rows={[giaDinh({ gach: 0 })]} />)
    await screen.findByText('Mã GĐ')

    await waitFor(() => {
      expect(layClassOCell('Giuse Nguyễn Văn A')).toContain('struck')
      expect(layClassOCell('Maria Trần Thị B')).not.toContain('struck')
    })
  })

  it('gach = 1 thi gach o Nguoi nu, khong gach o Nguoi nam', async () => {
    render(<GxGiaDinhList rows={[giaDinh({ gach: 1 })]} />)
    await screen.findByText('Mã GĐ')

    await waitFor(() => {
      expect(layClassOCell('Giuse Nguyễn Văn A')).not.toContain('struck')
      expect(layClassOCell('Maria Trần Thị B')).toContain('struck')
    })
  })

  it('gach = 2 thi gach ca hai o Nguoi nam va Nguoi nu', async () => {
    render(<GxGiaDinhList rows={[giaDinh({ gach: 2 })]} />)
    await screen.findByText('Mã GĐ')

    await waitFor(() => {
      expect(layClassOCell('Giuse Nguyễn Văn A')).toContain('struck')
      expect(layClassOCell('Maria Trần Thị B')).toContain('struck')
    })
  })

  it('gach = -1 thi khong gach o nao', async () => {
    render(<GxGiaDinhList rows={[giaDinh({ gach: -1 })]} />)
    await screen.findByText('Mã GĐ')

    await waitFor(() => {
      expect(layClassOCell('Giuse Nguyễn Văn A')).not.toContain('struck')
      expect(layClassOCell('Maria Trần Thị B')).not.toContain('struck')
    })
  })

  it('nhap dup mot dong thi goi onMo voi dung ban ghi', async () => {
    const onMo = vi.fn()
    render(<GxGiaDinhList rows={[giaDinh()]} onMo={onMo} />)

    await userEvent.dblClick(await screen.findByText('Nguyễn Văn A'))

    expect(onMo).toHaveBeenCalledWith(expect.objectContaining({ maGiaDinhCu: 12 }))
  })

  // Task "ghi gia đình" mục C: "In phiếu gia đình" TỪNG bị nối nhầm vào mở màn hình chi tiết
  // (moChiTiet) — xem gia-dinh-danh-sach.md mục 10 "Ưu tiên cao #2". Đã gỡ nối sai; nay đã in
  // được thật (xem docs/superpowers/specs/man-hinh/in-an.md) — không mở màn hình chi tiết.
  it('menu In phieu gia dinh KHONG mo man hinh chi tiet, goi dung api voi id gia dinh', () => {
    const moChiTiet = vi.fn()

    const menu = menuGiaDinhMacDinh(moChiTiet, vi.fn())
    const inPhieu = menu.find((m) => m.nhan === 'In phiếu gia đình')
    inPhieu?.chay?.(giaDinh({ id: 'gd-1' }))

    expect(moChiTiet).not.toHaveBeenCalled()
    expect(api.giaDinh.inPhieuGiaDinh).toHaveBeenCalledWith('gd-1')
  })

  // Mục 2 nhiệm vụ "in-excel-a3": người dùng nên chọn được khổ khi in phiếu gia đình — mục
  // riêng "In phiếu gia đình (khổ A3)" gọi cùng api nhưng truyền thêm 'A3'.
  it('menu In phieu gia dinh (kho A3) goi api voi id gia dinh va "A3"', () => {
    const menu = menuGiaDinhMacDinh(vi.fn(), vi.fn())
    const inPhieuA3 = menu.find((m) => m.nhan === 'In phiếu gia đình (khổ A3)')
    inPhieuA3?.chay?.(giaDinh({ id: 'gd-1' }))

    expect(api.giaDinh.inPhieuGiaDinh).toHaveBeenCalledWith('gd-1', 'A3')
  })

  it('menu In chung nhan hon phoi goi dung api voi id gia dinh', () => {
    const menu = menuGiaDinhMacDinh(vi.fn(), vi.fn())
    const inHonPhoi = menu.find((m) => m.nhan === 'In chứng nhận hôn phối')
    inHonPhoi?.chay?.(giaDinh({ id: 'gd-2' }))

    expect(api.giaDinh.inChungNhanHonPhoi).toHaveBeenCalledWith('gd-2')
  })

  // "In giới thiệu chuyển xứ" (mẫu thứ tư của "Giấy giới thiệu" — theo gia đình) mở
  // GioiThieuModal thay vì gọi thẳng api — xem in-an.md mục 5e.
  it('menu In gioi thieu chuyen xu goi moGioiThieuChuyenXu voi dung gia dinh', () => {
    const moGioiThieuChuyenXu = vi.fn()
    const menu = menuGiaDinhMacDinh(vi.fn(), moGioiThieuChuyenXu)
    const d = giaDinh({ id: 'gd-3' })
    const muc = menu.find((m) => m.nhan === 'In giới thiệu chuyển xứ')

    muc?.chay?.(d)

    expect(moGioiThieuChuyenXu).toHaveBeenCalledWith(d)
  })

  // "In lý lịch cá nhân" ở lưới GIA ĐÌNH nay in CẢ gia đình (một trang PDF/thành viên, đúng
  // hành vi item4_Click của bản desktop — xem in-an.md mục 5f), không còn "chưa hỗ trợ".
  it('menu In ly lich ca nhan goi dung api voi id gia dinh (in ca gia dinh)', () => {
    const menu = menuGiaDinhMacDinh(vi.fn(), vi.fn())
    const muc = menu.find((m) => m.nhan === 'In lý lịch cá nhân')

    muc?.chay?.(giaDinh({ id: 'gd-4' }))

    expect(api.giaDinh.inLyLichCaNhanGiaDinh).toHaveBeenCalledWith('gd-4')
  })

  // "Xem vị trí" mở Google Maps với địa chỉ gia đình (xem lib/xemViTri.ts) — báo lỗi bằng
  // alert() CHỈ khi gia đình không có địa chỉ (đúng hộp thoại của bản desktop), không phải vì
  // "chưa hỗ trợ" nữa.
  it('menu Xem vi tri bao loi khi gia dinh khong co dia chi', () => {
    const alertGia = vi.spyOn(window, 'alert').mockImplementation(() => {})
    const openGia = vi.spyOn(window, 'open').mockImplementation(() => null)
    const menu = menuGiaDinhMacDinh(vi.fn(), vi.fn())
    const muc = menu.find((m) => m.nhan === 'Xem vị trí')

    muc?.chay?.(giaDinh({ diaChi: null }))

    expect(alertGia).toHaveBeenCalledWith('Gia đình này không có địa chỉ để xem bản đồ.')
    expect(openGia).not.toHaveBeenCalled()
    alertGia.mockRestore()
    openGia.mockRestore()
  })

  it('menu Xem vi tri mo Google Maps voi dung dia chi khi co dia chi', () => {
    const openGia = vi.spyOn(window, 'open').mockImplementation(() => null)
    const menu = menuGiaDinhMacDinh(vi.fn(), vi.fn())
    const muc = menu.find((m) => m.nhan === 'Xem vị trí')

    muc?.chay?.(giaDinh({ diaChi: '123 Đường ABC, Phan Thiết' }))

    expect(openGia).toHaveBeenCalledWith(
      'https://www.google.com/maps/search/' + encodeURIComponent('123 Đường ABC, Phan Thiết'),
      '_blank', 'noopener,noreferrer',
    )
    openGia.mockRestore()
  })
})
