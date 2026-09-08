import { render, screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { RaoHonPhoiDetail } from './RaoHonPhoiDetail'
import { api } from '../api/client'
import type { RaoHonPhoiDetail as RaoHonPhoiDetailType } from '../api/types'

vi.mock('../api/client', () => ({
  api: {
    raoHonPhoi: {
      chiTiet: vi.fn(),
      capNhat: vi.fn(() => Promise.resolve()),
      inKetQua: vi.fn(() => Promise.resolve()),
    },
  },
  LoiXungDot: class LoiXungDot extends Error {},
}))

function xoaTrangONgay(o: HTMLInputElement) {
  o.focus()
  o.setSelectionRange(10, 10)
  for (let i = 0; i < 10; i++) fireEvent.keyDown(o, { key: 'Backspace' })
  expect(o.value).toBe('__/__/____')
}

const chiTiet = (p: Partial<RaoHonPhoiDetailType> = {}): RaoHonPhoiDetailType => ({
  id: 'r1', maRaoHonPhoiCu: 71, tenRaoHonPhoi: 'Doi rao thu nghiem',
  giaoDan1Id: 'g1', tenGiaoDan1: 'Nguoi Mot', giaoDan2Id: 'g2', tenGiaoDan2: 'Nguoi Hai',
  ngayRaoLan1: null, ngayRaoLan2: null, ngayRaoLan3: null,
  giaoXu1: null, giaoPhan1: null, giaoXuTruoc1: null, giaoPhanTruoc1: null,
  giaoXu2: null, giaoPhan2: null, giaoXuTruoc2: null, giaoPhanTruoc2: null,
  linhMucNhan: null, giaoXuNhan: null, ghiChu: null,
  tam1: null, tam2: null, tam3: null,
  giaoXuNQ1: null, giaoPhanNQ1: null, giaoXuNQ2: null, giaoPhanNQ2: null,
  rowVersion: 1, ...p,
})

describe('RaoHonPhoiDetail — In ket qua rao hon phoi (in-an.md muc 8)', () => {
  it('nut In ket qua bi vo hieu hoa khi la doi rao moi (chua co id)', () => {
    render(<RaoHonPhoiDetail id={null} />)

    expect((screen.getByRole('button', { name: 'In kết quả rao hôn phối' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('bam In ket qua thi goi api.raoHonPhoi.inKetQua voi dung id', async () => {
    vi.mocked(api.raoHonPhoi.chiTiet).mockResolvedValue(chiTiet({ id: 'r9' }))
    render(<RaoHonPhoiDetail id="r9" />)

    await waitFor(() =>
      expect((screen.getByRole('button', { name: 'In kết quả rao hôn phối' }) as HTMLButtonElement).disabled).toBe(false))
    await userEvent.click(screen.getByRole('button', { name: 'In kết quả rao hôn phối' }))

    expect(api.raoHonPhoi.inKetQua).toHaveBeenCalledWith('r9')
  })
})

describe('RaoHonPhoiDetail — xoa ngay roi Luu (Nghiem trong #1)', () => {
  it('xoa trang ca ba o Rao lan 1/2/3 roi Cap nhat thi gui null, KHONG gui chuoi rong', async () => {
    vi.mocked(api.raoHonPhoi.chiTiet).mockResolvedValue(chiTiet({
      ngayRaoLan1: '2024-01-05', ngayRaoLan2: '2024-01-12', ngayRaoLan3: '2024-01-19',
    }))

    render(<RaoHonPhoiDetail id="r1" />)
    const rao1 = await screen.findByLabelText('Rao lần 1') as HTMLInputElement
    const rao2 = screen.getByLabelText('Rao lần 2') as HTMLInputElement
    const rao3 = screen.getByLabelText('Rao lần 3') as HTMLInputElement
    expect(rao1.value).toBe('05/01/2024')

    xoaTrangONgay(rao1)
    xoaTrangONgay(rao2)
    xoaTrangONgay(rao3)

    fireEvent.click(screen.getByText('Cập nhật'))

    await waitFor(() => expect(api.raoHonPhoi.capNhat).toHaveBeenCalled())
    const than = vi.mocked(api.raoHonPhoi.capNhat).mock.calls[0]![1] as Record<string, unknown>
    // TRƯỚC KHI SỬA: `onIsoChange={(v) => d('ngayRaoLan1', v)}` (và lần 2/3) nhận thẳng chuỗi
    // rỗng `''` từ GxDate, không qua chuẩn hoá `|| null` — payload gửi `ngayRaoLanX: ''` lên máy
    // chủ, DTO backend `DateOnly?` không bind được → 400 kỹ thuật (xem review toàn nhánh
    // 2026-09-08 "Nghiêm trọng #1"). Ba assertion dưới RED nếu quay lại `d('ngayRaoLanX', v)`
    // trần.
    expect(than.ngayRaoLan1).toBeNull()
    expect(than.ngayRaoLan2).toBeNull()
    expect(than.ngayRaoLan3).toBeNull()
  })
})

describe('RaoHonPhoiDetail — nut "Thu lai" khi tai loi (Nghiem trong #2)', () => {
  it('tai loi, bam Thu lai va lan sau thanh cong thi THOAT khoi man hinh loi', async () => {
    vi.mocked(api.raoHonPhoi.chiTiet)
      .mockRejectedValueOnce(new Error('Mất kết nối mạng'))
      .mockResolvedValueOnce(chiTiet())

    render(<RaoHonPhoiDetail id="r1" />)

    expect(await screen.findByText(/Không tải được dữ liệu/)).toBeDefined()
    fireEvent.click(screen.getByRole('button', { name: 'Thử lại' }))

    // TRƯỚC KHI SỬA: `onThuLai` gọi thẳng `api.raoHonPhoi.chiTiet(id).then(setRao)`, không hề
    // `setLoi(null)` — màn hình đứng yên ở nhánh lỗi dù lần gọi lại thành công (xem review toàn
    // nhánh 2026-09-08 "Nghiêm trọng #2"). Assertion dưới RED nếu quay lại closure cũ.
    await waitFor(() => expect(screen.queryByText(/Không tải được dữ liệu/)).toBeNull())
    expect(await screen.findByText('Doi rao thu nghiem')).toBeDefined()
  })
})
