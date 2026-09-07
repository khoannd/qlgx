import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { LoginPage } from './LoginPage'
import { useAuth } from '../api/AuthContext'
import { CanChonGiaoXu } from '../api/client'

vi.mock('../api/AuthContext', () => ({ useAuth: vi.fn() }))

describe('LoginPage', () => {
  it('goi dangNhap voi ten tai khoan va mat khau da go', async () => {
    const dangNhap = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useAuth).mockReturnValue({ dangNhap } as unknown as ReturnType<typeof useAuth>)

    render(<LoginPage />)
    await userEvent.type(screen.getByLabelText('Tên đăng nhập'), 'vanphong')
    await userEvent.type(screen.getByLabelText('Mật khẩu'), 'matkhau123')
    await userEvent.click(screen.getByRole('button', { name: /Đăng nhập/ }))

    await waitFor(() => expect(dangNhap).toHaveBeenCalledWith('vanphong', 'matkhau123', undefined))
  })

  it('ten dang nhap trung o nhieu giao xu thi hien hop chon giao xu, khong dang nhap ngay', async () => {
    const dangNhap = vi.fn()
      .mockRejectedValueOnce(new CanChonGiaoXu('Vui lòng chọn giáo xứ', [
        { id: 'gx-a', tenGiaoXu: 'Giáo xứ A' },
        { id: 'gx-b', tenGiaoXu: 'Giáo xứ B' },
      ]))
      .mockResolvedValueOnce(undefined)
    vi.mocked(useAuth).mockReturnValue({ dangNhap } as unknown as ReturnType<typeof useAuth>)

    render(<LoginPage />)
    await userEvent.type(screen.getByLabelText('Tên đăng nhập'), 'vanphong')
    await userEvent.type(screen.getByLabelText('Mật khẩu'), 'matkhau123')
    await userEvent.click(screen.getByRole('button', { name: /Đăng nhập/ }))

    await screen.findByLabelText('Giáo xứ')
    expect((screen.getByRole('button', { name: /Đăng nhập/ }) as HTMLButtonElement).disabled).toBe(true)

    await userEvent.selectOptions(screen.getByLabelText('Giáo xứ'), 'gx-b')
    await userEvent.click(screen.getByRole('button', { name: /Đăng nhập/ }))

    await waitFor(() => expect(dangNhap).toHaveBeenLastCalledWith('vanphong', 'matkhau123', 'gx-b'))
  })

  it('dang nhap that bai hien thong bao loi, KHONG lam mat du lieu da go', async () => {
    const dangNhap = vi.fn().mockRejectedValue(new Error('Tên đăng nhập hoặc mật khẩu không chính xác'))
    vi.mocked(useAuth).mockReturnValue({ dangNhap } as unknown as ReturnType<typeof useAuth>)

    render(<LoginPage />)
    await userEvent.type(screen.getByLabelText('Tên đăng nhập'), 'vanphong')
    await userEvent.type(screen.getByLabelText('Mật khẩu'), 'sai')
    await userEvent.click(screen.getByRole('button', { name: /Đăng nhập/ }))

    const canhBao = await screen.findByRole('alert')
    expect(canhBao.textContent).toContain('không chính xác')
    expect((screen.getByLabelText('Tên đăng nhập') as HTMLInputElement).value).toBe('vanphong')
  })
})
