import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { GxDate } from './GxDate'

describe('GxDate', () => {
  it('hien dd/MM/yyyy tu gia tri ISO ban dau, khong phai kieu My', () => {
    render(<GxDate id="d1" ariaLabel="Ngày sinh" defaultValue="2015-04-25" />)
    expect((screen.getByLabelText('Ngày sinh') as HTMLInputElement).value).toBe('25/04/2015')
  })

  it('placeholder la dd/mm/yyyy, khong phai mm/dd/yyyy', () => {
    render(<GxDate id="d2" ariaLabel="Ngày sinh" />)
    expect((screen.getByLabelText('Ngày sinh') as HTMLInputElement).placeholder).toBe('dd/mm/yyyy')
  })

  it('gia tri rong/null khong lam sap, hien o trong', () => {
    render(<GxDate id="d3" ariaLabel="Ngày mất" defaultValue={null} />)
    expect((screen.getByLabelText('Ngày mất') as HTMLInputElement).value).toBe('')
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
})
