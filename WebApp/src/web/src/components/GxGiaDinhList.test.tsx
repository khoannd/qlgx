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

    const menu = menuGiaDinhMacDinh(moChiTiet)
    const inPhieu = menu.find((m) => m.nhan === 'In phiếu gia đình')
    inPhieu?.chay?.(giaDinh({ id: 'gd-1' }))

    expect(moChiTiet).not.toHaveBeenCalled()
    expect(api.giaDinh.inPhieuGiaDinh).toHaveBeenCalledWith('gd-1')
  })

  it('menu In chung nhan hon phoi goi dung api voi id gia dinh', () => {
    const menu = menuGiaDinhMacDinh(vi.fn())
    const inHonPhoi = menu.find((m) => m.nhan === 'In chứng nhận hôn phối')
    inHonPhoi?.chay?.(giaDinh({ id: 'gd-2' }))

    expect(api.giaDinh.inChungNhanHonPhoi).toHaveBeenCalledWith('gd-2')
  })

  it('cac muc con lai van bao "chua ho tro" (In ly lich ca nhan, gioi thieu chuyen xu, xem vi tri)', () => {
    const alertGia = vi.spyOn(window, 'alert').mockImplementation(() => {})
    const menu = menuGiaDinhMacDinh(vi.fn())

    for (const nhan of ['In lý lịch cá nhân', 'In giới thiệu chuyển xứ', 'Xem vị trí']) {
      const muc = menu.find((m) => m.nhan === nhan)
      muc?.chay?.(giaDinh())
    }

    expect(alertGia).toHaveBeenCalledTimes(3)
    alertGia.mockRestore()
  })
})
