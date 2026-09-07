import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { GxDate } from './GxDate'

describe('GxDate', () => {
  it('hien dd/MM/yyyy tu gia tri ISO ban dau, khong phai kieu My', () => {
    render(<GxDate id="d1" ariaLabel="Ngày sinh" defaultValue="2015-04-25" />)
    expect((screen.getByLabelText('Ngày sinh') as HTMLInputElement).value).toBe('25/04/2015')
  })

  it('o trong hien san dau ngan cach __/__/____ (khong phai o trang trong)', () => {
    render(<GxDate id="d2" ariaLabel="Ngày sinh" />)
    expect((screen.getByLabelText('Ngày sinh') as HTMLInputElement).value).toBe('__/__/____')
  })

  it('gia tri rong/null hien khuon __/__/____, khong lam sap', () => {
    render(<GxDate id="d3" ariaLabel="Ngày mất" defaultValue={null} />)
    expect((screen.getByLabelText('Ngày mất') as HTMLInputElement).value).toBe('__/__/____')
  })

  it('du lieu loi (chi co nam) hien nguyen van, khong sap', () => {
    render(<GxDate id="d4" ariaLabel="Ngày lỗi" defaultValue="1958" />)
    expect((screen.getByLabelText('Ngày lỗi') as HTMLInputElement).value).toBe('1958')
  })

  it('gia tri ISO that su nam tren o lich an (co name) de FormData/querySelector doc duoc', () => {
    const { container } = render(<GxDate name="ngaySinh" ariaLabel="Ngày sinh" />)
    const oAn = container.querySelector('input[name="ngaySinh"]') as HTMLInputElement
    expect(oAn.type).toBe('date')
    expect(oAn.value).toBe('')
  })

  it('go 25/04/2015 thi cap nhat o lich an sang ISO 2015-04-25 ngay khi go xong, khong can roi o', async () => {
    const { container } = render(<GxDate name="ngaySinh" ariaLabel="Ngày sinh" />)
    await userEvent.type(screen.getByLabelText('Ngày sinh'), '25/04/2015')
    const oAn = container.querySelector('input[name="ngaySinh"]') as HTMLInputElement
    expect(oAn.value).toBe('2015-04-25')
  })

  it('go sai thu tu kieu My (13 lam thang) thi bao loi ro rang, khong am tham nuot gia tri', async () => {
    const { container } = render(<GxDate name="ngaySinh" ariaLabel="Ngày sinh" />)
    const o = screen.getByLabelText('Ngày sinh')
    await userEvent.type(o, '13/25/2015')
    fireEvent.blur(o)
    const canhBao = await screen.findByRole('alert')
    expect(canhBao.textContent).toMatch(/không hợp lệ/i)
    const oAn = container.querySelector('input[name="ngaySinh"]') as HTMLInputElement
    expect(oAn.value).toBe('')
  })

  it('chon tu lich (input date an) thi cap nhat ca o hien thi lan gia tri gui di', () => {
    const { container } = render(<GxDate name="ngayRuaToi" ariaLabel="Ngày rửa tội" />)
    const native = container.querySelector('input.gx-date-native') as HTMLInputElement
    fireEvent.change(native, { target: { value: '2015-04-25' } })
    expect((screen.getByLabelText('Ngày rửa tội') as HTMLInputElement).value).toBe('25/04/2015')
    expect(native.value).toBe('2015-04-25')
  })

  it('co nut mo lich', () => {
    render(<GxDate id="d5" ariaLabel="Ngày sinh" />)
    expect(screen.getByTitle('Chọn ngày từ lịch')).toBeDefined()
  })

  it('khong ten (name) thi o lich an cung khong co name, giong o ngay tinh chua noi API', () => {
    const { container } = render(<GxDate ariaLabel="Ngày kết thúc khóa học" />)
    const native = container.querySelector('input.gx-date-native') as HTMLInputElement
    expect(native.name).toBe('')
  })

  it('go lien tuc 01021985 KHONG can go dau / van ra dung 01/02/1985', async () => {
    const { container } = render(<GxDate name="ngaySinh" ariaLabel="Ngày sinh" />)
    const o = screen.getByLabelText('Ngày sinh')
    await userEvent.type(o, '01021985')
    expect((o as HTMLInputElement).value).toBe('01/02/1985')
    const oAn = container.querySelector('input[name="ngaySinh"]') as HTMLInputElement
    expect(oAn.value).toBe('1985-02-01')
  })

  it('nhap thang xong tu nhay sang nam (khong can chuot), roi go xong nam tu nhay ra control ke tiep', async () => {
    render(
      <>
        <GxDate name="ngaySinh" ariaLabel="Ngày sinh" />
        <input aria-label="Ô kế tiếp trên form" />
      </>,
    )
    const o = screen.getByLabelText('Ngày sinh')
    await userEvent.type(o, '01021985')
    expect(document.activeElement).toBe(screen.getByLabelText('Ô kế tiếp trên form'))
  })

  it('go xong 8 so lien tuc (tu nhay control ke tiep) thi KHONG con bao loi gia — bug rieng dinh dang cu', async () => {
    // Tái hiện đúng lỗi người dùng thật báo cáo: gõ liên tục "01/02/2003" (hợp lệ) vào ô ngày
    // rồi tự nhảy sang control kế tiếp — trước khi sửa, việc tự nhảy focus (đồng bộ) làm bắn ra
    // sự kiện blur dùng closure CŨ (state `text` chưa kịp commit), khiến ô bị đè lại thông báo
    // "Ngày không hợp lệ" SAI dù giá trị đã đúng. Xem GxDate.tsx: thuChuanHoaVaCoTheNhay.
    render(
      <>
        <GxDate name="ngayXucDau" ariaLabel="Ngày xức dầu" />
        <input aria-label="Ô kế tiếp trên form" />
      </>,
    )
    const o = screen.getByLabelText('Ngày xức dầu') as HTMLInputElement
    await userEvent.type(o, '01022003')
    expect(document.activeElement).toBe(screen.getByLabelText('Ô kế tiếp trên form'))
    expect(o.value).toBe('01/02/2003')
    expect(o.getAttribute('aria-invalid')).toBeNull()
    expect(screen.queryByRole('alert')).toBeNull()
  })

  it('bam chuot/focus thang vao o nam roi go luon, roi o thi tu dien 01/01', async () => {
    const { container } = render(<GxDate name="ngaySinh" ariaLabel="Ngày sinh" />)
    const o = screen.getByLabelText('Ngày sinh') as HTMLInputElement
    o.focus()
    o.setSelectionRange(6, 6) // click thẳng vào ô năm — không cần gõ ngày/tháng trước
    await userEvent.keyboard('1985')
    fireEvent.blur(o)
    expect(o.value).toBe('01/01/1985')
    const oAn = container.querySelector('input[name="ngaySinh"]') as HTMLInputElement
    expect(oAn.value).toBe('1985-01-01')
  })

  it('bam chuot thang vao o thang roi go, khong bat buoc phai go ngay truoc', async () => {
    const { container } = render(<GxDate name="ngaySinh" ariaLabel="Ngày sinh" />)
    const o = screen.getByLabelText('Ngày sinh') as HTMLInputElement
    o.focus()
    o.setSelectionRange(3, 3) // click thẳng vào ô tháng
    await userEvent.keyboard('051985')
    fireEvent.blur(o)
    expect(o.value).toBe('01/05/1985')
    const oAn = container.querySelector('input[name="ngaySinh"]') as HTMLInputElement
    expect(oAn.value).toBe('1985-05-01')
  })

  it('xoa lui ve hoan toan trong thi dong bo NGAY o lich an ve rong, khong doi blur (tranh luu nham ngay da xoa)', async () => {
    const { container } = render(<GxDate name="ngayRuaToi" ariaLabel="Ngày rửa tội" defaultValue="2015-04-25" />)
    const o = screen.getByLabelText('Ngày rửa tội') as HTMLInputElement
    o.focus()
    o.setSelectionRange(9, 9)
    for (let i = 0; i < 8; i++) await userEvent.keyboard('{Backspace}')
    expect(o.value).toBe('__/__/____')
    const oAn = container.querySelector('input[name="ngayRuaToi"]') as HTMLInputElement
    // KHÔNG blur — mô phỏng tình huống bấm thẳng nút "Cập nhật" ngay sau khi xoá, không rời ô
    // theo cách thông thường trước.
    expect(oAn.value).toBe('')
  })

  it('Tab van hoat dong binh thuong, khong bi chan boi control ngay thang (khong bay focus)', async () => {
    render(
      <>
        <GxDate ariaLabel="Ngày sinh" />
        <input aria-label="Ô kế tiếp" />
      </>,
    )
    const o = screen.getByLabelText('Ngày sinh')
    o.focus()
    await userEvent.tab()
    expect(document.activeElement).toBe(screen.getByLabelText('Ô kế tiếp'))
  })
})
