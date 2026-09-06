import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { LoginPage } from './LoginPage'
import { useAuth } from '../api/AuthContext'

vi.mock('../api/AuthContext', () => ({ useAuth: vi.fn() }))

describe('LoginPage', () => {
  it('goi dangNhap voi ten tai khoan va mat khau da go', async () => {
    const dangNhap = vi.fn().mockResolvedValue(undefined)
    vi.mocked(useAuth).mockReturnValue({ dangNhap } as unknown as ReturnType<typeof useAuth>)

    render(<LoginPage />)
    await userEvent.type(screen.getByLabelText('Tên đăng nhập'), 'vanphong')
    await userEvent.type(screen.getByLabelText('Mật khẩu'), 'matkhau123')
    await userEvent.click(screen.getByRole('button', { name: /Đăng nhập/ }))

    await waitFor(() => expect(dangNhap).toHaveBeenCalledWith('vanphong', 'matkhau123'))
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
