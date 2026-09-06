import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { GxGiaoDanList } from './GxGiaoDanList'
import type { GiaoDanListItem } from '../api/types'

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'a1', maGiaoDanCu: 4412, tenThanh: 'Maria', hoTen: 'Trần Thị Khánh Ngọc',
  phai: 'Nữ', ngaySinh: '2005-03-14', namSinh: '2005', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: false, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: true, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: false, ngayQuaDoi: null, noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null, ...p,
})

// ag-grid dựng header/hàng qua setTimeout(0) nội bộ (dồn sự kiện để tối ưu hiệu năng) nên
// dưới jsdom nội dung lưới chưa có ngay sau render() — phải chờ bằng findByText/waitFor
// thay vì getByText/queryByText đồng bộ.
describe('GxGiaoDanList', () => {
  it('hien du 29 cot cua ban desktop khi dung o man hinh danh sach', async () => {
    render(<GxGiaoDanList rows={[nguoi()]} />)

    expect(await screen.findByText('Mã GD')).toBeDefined()
    expect(screen.getByText('Tên thánh')).toBeDefined()
    expect(screen.getByText('Nơi thêm sức')).toBeDefined()
    expect(screen.queryByText('Quan hệ GĐ')).toBeNull()
  })

  it('them cot Quan he GD va bo cot Dien thoai khi nhung trong form gia dinh', async () => {
    render(<GxGiaoDanList rows={[nguoi()]} quanHeGiaDinh />)

    expect(await screen.findByText('Quan hệ GĐ')).toBeDefined()
    expect(screen.queryByText('Điện thoại')).toBeNull()
  })

  it('nhap dup mot dong thi goi onMo voi dung ban ghi', async () => {
    const onMo = vi.fn()
    render(<GxGiaoDanList rows={[nguoi()]} onMo={onMo} />)

    await userEvent.dblClick(await screen.findByText('Trần Thị Khánh Ngọc'))

    expect(onMo).toHaveBeenCalledWith(expect.objectContaining({ maGiaoDanCu: 4412 }))
  })

  it('to do dong cua nguoi da qua doi, chuyen xu hoac lap gia dinh rieng', async () => {
    const { container } = render(
      <GxGiaoDanList rows={[nguoi({ quaDoi: true }), nguoi({ id: 'a2', maGiaoDanCu: 1, quaDoi: false })]} />,
    )

    await screen.findByText('Mã GD')
    await waitFor(() => expect(container.querySelectorAll('.dong-gach-do')).toHaveLength(1))
  })
})
