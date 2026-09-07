import { fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { GxGoiY } from './GxGoiY'
import { ghiNhanDaDung, layGoiY } from '../lib/goiYNhapLieu'

describe('GxGoiY', () => {
  afterEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  it('go gia tri MOI hoan toan (chua co trong lich su lan danh muc) van hoat dong binh thuong', async () => {
    render(<GxGoiY ariaLabel="Nơi sinh" truong="noiSinh" giaoXuId="gx1" />)
    const o = screen.getByLabelText('Nơi sinh') as HTMLInputElement
    await userEvent.type(o, 'Một địa danh chưa từng có')
    expect(o.value).toBe('Một địa danh chưa từng có')
  })

  it('hien gia tri tu danh muc khi focus vao o trong', async () => {
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna', 'Maria']} />)
    const o = screen.getByLabelText('Tên thánh')
    await userEvent.click(o)
    expect(await screen.findByRole('option', { name: 'Anna' })).toBeDefined()
    expect(screen.getByRole('option', { name: 'Maria' })).toBeDefined()
  })

  it('go loc dung gia tri khop, an gia tri khong khop', async () => {
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna', 'Maria']} />)
    const o = screen.getByLabelText('Tên thánh')
    await userEvent.type(o, 'An')
    expect(screen.getByRole('option', { name: 'Anna' })).toBeDefined()
    expect(screen.queryByRole('option', { name: 'Maria' })).toBeNull()
  })

  it('chon mot goi y bang chuot dien dung gia tri va dong danh sach', async () => {
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna', 'Maria']} />)
    const o = screen.getByLabelText('Tên thánh') as HTMLInputElement
    await userEvent.click(o)
    await userEvent.click(screen.getByRole('option', { name: 'Anna' }))
    expect(o.value).toBe('Anna')
    expect(screen.queryByRole('listbox')).toBeNull()
  })

  it('chon mot goi y ghi nhan vao localStorage (tang bo dem lan dung)', async () => {
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna']} />)
    await userEvent.click(screen.getByLabelText('Tên thánh'))
    await userEvent.click(screen.getByRole('option', { name: 'Anna' }))
    expect(layGoiY('gx1', 'tenThanh')).toEqual(['Anna'])
  })

  it('roi o sau khi tu go (khong qua danh sach) cung duoc ghi nhan la da dung', async () => {
    render(<GxGoiY ariaLabel="Nơi sinh" truong="noiSinh" giaoXuId="gx1" />)
    const o = screen.getByLabelText('Nơi sinh')
    await userEvent.type(o, 'Địa danh mới gõ tay')
    fireEvent.blur(o)
    expect(layGoiY('gx1', 'noiSinh')).toEqual(['Địa danh mới gõ tay'])
  })

  it('mui ten xuong/len duyet goi y, Enter xac nhan muc dang chon', async () => {
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna', 'Maria']} />)
    const o = screen.getByLabelText('Tên thánh') as HTMLInputElement
    await userEvent.click(o)
    await userEvent.keyboard('{ArrowDown}{ArrowDown}{Enter}')
    expect(o.value).toBe('Maria')
  })

  it('Esc dong danh sach goi y ma KHONG xoa noi dung dang go', async () => {
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna']} />)
    const o = screen.getByLabelText('Tên thánh') as HTMLInputElement
    await userEvent.type(o, 'An')
    expect(screen.getByRole('listbox')).toBeDefined()
    await userEvent.keyboard('{Escape}')
    expect(screen.queryByRole('listbox')).toBeNull()
    expect(o.value).toBe('An')
  })

  it('chon xong mot goi y tu nhay sang control ke tiep trong form', async () => {
    render(
      <form>
        <GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna']} />
        <input aria-label="Ô kế tiếp" />
      </form>,
    )
    await userEvent.click(screen.getByLabelText('Tên thánh'))
    await userEvent.click(screen.getByRole('option', { name: 'Anna' }))
    expect(await screen.findByLabelText('Ô kế tiếp')).toBe(document.activeElement)
  })

  it('giaoXuId null tat han phan lich su nhung van hien danh muc', async () => {
    ghiNhanDaDung('gx1', 'tenThanh', 'Từ lịch sử')
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId={null} danhMuc={['Anna']} />)
    await userEvent.click(screen.getByLabelText('Tên thánh'))
    expect(screen.getByRole('option', { name: 'Anna' })).toBeDefined()
    expect(screen.queryByRole('option', { name: 'Từ lịch sử' })).toBeNull()
  })

  it('gia tri tu lich su hien TRUOC gia tri chi co trong danh muc', async () => {
    ghiNhanDaDung('gx1', 'tenThanh', 'Vinh Sơn')
    render(<GxGoiY ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" danhMuc={['Anna', 'Vinh Sơn']} />)
    await userEvent.click(screen.getByLabelText('Tên thánh'))
    const tuyChon = screen.getAllByRole('option').map((el) => el.textContent)
    expect(tuyChon[0]).toBe('Vinh Sơn')
  })

  it('disabled thi khong go duoc', () => {
    render(<GxGoiY ariaLabel="Nơi hôn phối" truong="noiHonPhoi" giaoXuId="gx1" disabled />)
    expect((screen.getByLabelText('Nơi hôn phối') as HTMLInputElement).disabled).toBe(true)
  })

  it('name duoc gan dung tren input de FormData doc duoc', () => {
    const { container } = render(
      <GxGoiY name="tenThanh" ariaLabel="Tên thánh" truong="tenThanh" giaoXuId="gx1" defaultValue="Maria" />,
    )
    const o = container.querySelector('input[name="tenThanh"]') as HTMLInputElement
    expect(o.value).toBe('Maria')
  })
})
