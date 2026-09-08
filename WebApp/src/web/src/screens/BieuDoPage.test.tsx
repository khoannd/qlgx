import { render, screen, fireEvent } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { BieuDoPage } from './BieuDoPage'

/**
 * Trung bình #2 (review toàn nhánh 2026-09-08): thông báo "Từ ngày không thể lớn hơn đến ngày"
 * chép nguyên từ `ThongKeChungPage.tsx` (nơi đó đúng vì là ô ngày thật) nhưng quên đổi tên
 * trường — hai ô trên màn hình "Biểu đồ" tên là "Từ năm"/"Đến năm" (số nguyên, không phải ô
 * ngày). Validation này chạy TRƯỚC khi đụng tới `<canvas>`/Chart.js nên không cần mock canvas.
 */
describe('BieuDoPage — thong bao dung ten truong (Trung binh #2)', () => {
  it('Tu nam lon hon Den nam thi bao dung "Tu nam"/"Den nam", khong noi "ngay"', () => {
    render(<BieuDoPage />)

    fireEvent.change(screen.getByLabelText('Từ năm'), { target: { value: '2030' } })
    fireEvent.change(screen.getByLabelText('Đến năm'), { target: { value: '2020' } })
    fireEvent.click(screen.getByRole('button', { name: 'Xem' }))

    // TRƯỚC KHI SỬA: `setLoi('Từ ngày không thể lớn hơn đến ngày')` — sai tên trường (màn hình
    // này không có ô ngày nào, chỉ có "Từ năm"/"Đến năm"). Assertion dưới RED nếu quay lại câu
    // cũ.
    expect(screen.getByRole('alert').textContent).toBe('Từ năm không thể lớn hơn đến năm')
  })
})
