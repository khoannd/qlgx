import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createRef } from 'react'
import { describe, expect, it, vi } from 'vitest'
import { GxGiaoDanList } from './GxGiaoDanList'
import type { GxGridHandle } from './GxGrid'
import type { GiaoDanListItem } from '../api/types'

const nguoi = (p: Partial<GiaoDanListItem> = {}): GiaoDanListItem => ({
  id: 'a1', maGiaoDanCu: 4412, tenThanh: 'Maria', hoTen: 'Trần Thị Khánh Ngọc',
  phai: 'Nữ', ngaySinh: '2005-03-14', namSinh: '2005', ngayRuaToi: null,
  ngayRuocLe: null, ngayThemSuc: null, lapGd: false, hoTenCha: null, hoTenMe: null,
  tanTong: false, conHoc: true, ngheNghiep: null, ghiChu: null, dienThoai: null,
  diaChi: null, tenGiaoHo: 'Giáo họ Thánh Tâm', daChuyenDi: false,
  trinhDoVanHoa: null, trinhDoChuyenMon: null, bietNgoaiNgu: null,
  quaDoi: false, ngayQuaDoi: null, noiAnTang: null, noiSinh: null,
  noiRuaToi: null, noiRuocLe: null, noiThemSuc: null, quanHe: null,
  giaDinhId: null, khongThongKe: false, ...p,
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

  it('man hinh danh sach giao dan KHONG gach ngang dong nao (dung ban desktop: FormattingRow rong)', async () => {
    const { container } = render(
      <GxGiaoDanList rows={[nguoi({ quaDoi: true }), nguoi({ id: 'a2', maGiaoDanCu: 1, lapGd: true })]} />,
    )

    await screen.findByText('Mã GD')
    await waitFor(() => expect(container.querySelectorAll('.ag-row')).toHaveLength(2))
    expect(container.querySelectorAll('.dong-gach-do')).toHaveLength(0)
    expect(screen.queryByText(/Gạch ngang đỏ/)).toBeNull()
  })

  it('luoi thanh vien gia dinh (quanHeGiaDinh) gach ngang nguoi qua doi/chuyen xu, KHONG gach nguoi da lap GD', async () => {
    const { container } = render(
      <GxGiaoDanList
        quanHeGiaDinh
        rows={[
          nguoi({ id: 'a1', quaDoi: true }),
          nguoi({ id: 'a2', daChuyenDi: true }),
          nguoi({ id: 'a3', lapGd: true }),
        ]}
      />,
    )

    await screen.findByText('Mã GD')
    await waitFor(() => expect(container.querySelectorAll('.dong-gach-do')).toHaveLength(2))
    expect(await screen.findByText(/Gạch ngang đỏ: đã qua đời hoặc đã chuyển xứ/)).toBeDefined()
  })

  it('mac dinh (hangLoc) thi hien hang loc duoi moi tieu de cot', async () => {
    const { container } = render(<GxGiaoDanList rows={[nguoi()]} />)

    await screen.findByText('Mã GD')
    // Ag-grid (bản legacy theme) đánh dấu ô lọc nổi bằng class `ag-floating-filter` —
    // đủ 29 cột thì phải có đủ 29 ô lọc.
    await waitFor(() => expect(container.querySelectorAll('.ag-floating-filter')).toHaveLength(29))
  })

  it('hangLoc=false thi khong hien hang loc', async () => {
    const { container } = render(<GxGiaoDanList rows={[nguoi()]} hangLoc={false} />)

    await screen.findByText('Mã GD')
    await waitFor(() => expect(container.querySelectorAll('.ag-floating-filter')).toHaveLength(0))
  })

  // Cột ngày hiện dd/MM/yyyy nhưng PHẢI sắp xếp theo giá trị ngày thật (giữ nguyên field ISO,
  // chỉ đổi valueFormatter) — nếu lỡ đổi field/valueGetter thành chuỗi đã định dạng, sắp xếp
  // sẽ so sánh chuỗi và cho ra thứ tự sai (vd "25/04/2015" đứng trước "26/03/1990" theo ASCII
  // dù 1990 có trước 2015 rất xa). Xem docs/superpowers/specs/man-hinh/can-review-sau.md mục W1.
  it('cot ngay sinh hien dd/MM/yyyy nhung sap xep dung theo thoi gian, khong theo chuoi', async () => {
    const ref = createRef<GxGridHandle>()
    render(
      <GxGiaoDanList
        ref={ref}
        rows={[
          nguoi({ id: 'a', hoTen: 'Người sinh 2015', ngaySinh: '2015-04-25' }),
          nguoi({ id: 'b', hoTen: 'Người sinh 1990', ngaySinh: '1990-03-26' }),
          nguoi({ id: 'c', hoTen: 'Người sinh 2005', ngaySinh: '2005-01-01' }),
        ]}
        hangLoc={false}
      />,
    )

    const tieuDeChu = await screen.findByText('Ngày sinh')
    // Hiển thị phải là dd/MM/yyyy, không phải ISO — kiểm tra trước khi sắp xếp.
    await waitFor(() => expect(screen.getByText('25/04/2015')).toBeDefined())
    expect(screen.getByText('26/03/1990')).toBeDefined()
    expect(screen.getByText('01/01/2005')).toBeDefined()

    // Bấm đúng vào `.ag-header-cell-label` để kích hoạt sắp xếp của ag-grid — trình lắng nghe
    // click sắp xếp của ag-grid gắn trên phần tử này (`data-ref="eLabel"`), không phải trên
    // toàn bộ `.ag-header-cell` hay riêng đoạn chữ `.ag-header-cell-text`. Dùng `fireEvent`
    // (một sự kiện `click` thô) thay vì `userEvent.click` — chuỗi pointerdown/pointerup của
    // `userEvent` bị ag-grid hiểu nhầm thành thao tác kéo cột thay vì bấm sắp xếp dưới jsdom.
    const oTieuDe = tieuDeChu.closest('.ag-header-cell')!.querySelector('.ag-header-cell-label') as HTMLElement
    fireEvent.click(oTieuDe)

    // Đọc lại DỮ LIỆU THẬT của lưới sau khi sắp xếp qua `layCsv()` (xuất đúng những gì lưới
    // đang hiển thị theo bộ lọc/sắp xếp hiện tại) thay vì lục lại DOM `.ag-cell` — dưới jsdom,
    // virtualization của ag-grid không tái dựng lại vị trí các dòng trong `container` dù mô
    // hình dữ liệu bên trong ĐÃ sắp đúng (đã kiểm tra trực tiếp bằng `getDataAsCsv()`), nên
    // assert theo CSV mới phản ánh đúng "dữ liệu được sắp xếp" mà cột ngày cam kết.
    await waitFor(() => {
      const csv = ref.current!.layCsv()
      expect(csv).toBeTruthy()
      const dong = (csv ?? '').trim().split('\n').slice(1)
      // Cột "Ngày sinh" là cột thứ 5 (chỉ số 4) trong 29 cột CSV.
      const ngaySinhTheoThuTu = dong.map((d) => d.split(',')[4].replaceAll('"', ''))
      // Sắp tăng dần THẬT theo thời gian: 1990 < 2005 < 2015 — nếu (do lỗi) sắp theo CHUỖI
      // dd/MM/yyyy đã định dạng thay vì ISO gốc thì "01/01/2005" (bắt đầu bằng "0") vẫn tình
      // cờ đứng giữa, nhưng phép thử thật sự nằm ở việc dữ liệu bên dưới KHÔNG BAO GIỜ được
      // đổi field/valueGetter sang chuỗi hiển thị — xem chú thích `ngay()` ở cotGiaoDan.ts.
      expect(ngaySinhTheoThuTu).toEqual(['26/03/1990', '01/01/2005', '25/04/2015'])
    })
  })
})
