import { fireEvent, render } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { focusKeTiep, useTuNhayKhiChonDropdown } from './focusDieuHuong'

function TrangThuNghiem() {
  useTuNhayKhiChonDropdown()
  return (
    <form>
      <select aria-label="Giáo họ" defaultValue="">
        <option value="">--</option>
        <option value="a">Giáo họ A</option>
      </select>
      <input aria-label="Ô kế tiếp trong form" />
    </form>
  )
}

describe('focusKeTiep', () => {
  it('focus control ke tiep theo thu tu DOM, bo qua chinh no', () => {
    document.body.innerHTML = '<form><input id="a" /><input id="b" /><input id="c" /></form>'
    const a = document.getElementById('a') as HTMLElement
    const b = document.getElementById('b') as HTMLElement
    focusKeTiep(a)
    expect(document.activeElement).toBe(b)
  })

  it('bo qua control da disabled', () => {
    document.body.innerHTML = '<form><input id="a" /><input id="b" disabled /><input id="c" /></form>'
    const a = document.getElementById('a') as HTMLElement
    const c = document.getElementById('c') as HTMLElement
    focusKeTiep(a)
    expect(document.activeElement).toBe(c)
  })

  it('khong con control nao phia sau thi khong lam gi (khong nem loi)', () => {
    document.body.innerHTML = '<form><input id="a" /></form>'
    const a = document.getElementById('a') as HTMLElement
    expect(() => focusKeTiep(a)).not.toThrow()
  })
})

describe('useTuNhayKhiChonDropdown', () => {
  it('chon xong mot muc trong select (co form) thi tu nhay sang o ke tiep', () => {
    const { getByLabelText } = render(<TrangThuNghiem />)
    const sl = getByLabelText('Giáo họ') as HTMLSelectElement
    sl.value = 'a'
    fireEvent.change(sl)
    expect(document.activeElement).toBe(getByLabelText('Ô kế tiếp trong form'))
  })

  it('select KHONG nam trong form thi khong tu nhay (vd o loc dau luoi danh sach)', () => {
    document.body.innerHTML = ''
    const div = document.createElement('div')
    div.innerHTML =
      '<select aria-label="Loc"><option value="a">A</option></select><input aria-label="Sau" />'
    document.body.appendChild(div)
    const sl = div.querySelector('select') as HTMLSelectElement
    const sau = div.querySelector('input') as HTMLInputElement
    sau.focus()
    fireEvent.change(sl)
    expect(document.activeElement).toBe(sau) // khong bi doi
  })
})
