import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect, it, vi } from 'vitest'
import { SaoLuuPhucHoiModal, CHUOI_XAC_NHAN_PHUC_HOI } from './SaoLuuPhucHoiModal'

const banSao = {
  id: 'ab12cd34', thoiDiem: '2026-09-13T06:00:00Z', nhan: 'tu-dong',
  kichThuocByte: 1024, soGiaoDan: 2050, soGiaDinh: 40, nguon: 'tu_dong' as const,
}

function dung(props: Partial<Parameters<typeof SaoLuuPhucHoiModal>[0]> = {}) {
  const onXacNhan = vi.fn()
  render(<SaoLuuPhucHoiModal banSao={banSao} soGiaoDanHienTai={2053} soGiaDinhHienTai={40}
    onDong={vi.fn()} onXacNhan={onXacNhan} {...props} />)
  return { onXacNhan }
}

it('chuoi xac nhan khop nguyen van voi phia may chu', () => {
  expect(CHUOI_XAC_NHAN_PHUC_HOI).toBe('PHUC HOI TOAN BO')
})

it('hien bang doi chieu truoc-sau va TO DO dong se giam', () => {
  dung()
  expect(screen.getByText('2053')).toBeDefined()
  expect(screen.getByText('2050')).toBeDefined()
  expect(screen.getByTestId('canh-bao-giam-giao-dan')).toBeDefined()
})

it('KHONG to do khi so lieu khong giam', () => {
  dung({ soGiaoDanHienTai: 2050 })
  expect(screen.queryByTestId('canh-bao-giam-giao-dan')).toBeNull()
})

// C1: khi chua biet so lieu hien tai, THA KHONG HIEN CON SO con hon hien so sai — va van phai
// canh bao mat du lieu (bang cu im lang dung luc can noi nhat).
it('chua biet so hien tai thi ghi ro "chua tinh duoc" va VAN canh bao mat du lieu', () => {
  dung({ soGiaoDanHienTai: null, soGiaDinhHienTai: null })
  expect(screen.getAllByText(/chưa tính được/i).length).toBeGreaterThan(0)
  expect(screen.getByTestId('chua-tinh-duoc-so-hien-tai')).toBeDefined()
  // Khong duoc to do theo mot phep so sanh vo nghia khi chua co so that.
  expect(screen.queryByTestId('canh-bao-giam-giao-dan')).toBeNull()
})

it('KHONG hien cau "chua tinh duoc" khi da co so hien tai that', () => {
  dung()
  expect(screen.queryByTestId('chua-tinh-duoc-so-hien-tai')).toBeNull()
})

// N3: chuoi xac nhan hien ngay tren o nhap, boi den + Ctrl+V la qua duoc lop rao "go tay".
it('chan DAN vao o xac nhan, va noi ro vi sao', async () => {
  dung()
  const o = screen.getByLabelText(/gõ/i) as HTMLInputElement
  o.focus()
  await userEvent.paste(CHUOI_XAC_NHAN_PHUC_HOI)
  expect(o.value).toBe('')
  expect((screen.getByRole('button', { name: /^Phục hồi về/ }) as HTMLButtonElement).disabled)
    .toBe(true)
  expect(screen.getByText(/Hãy gõ tay câu xác nhận/i)).toBeDefined()
})

// I1: chua co man hinh chan toan trang — toi thieu phai bao nguoi sap bam rang ho lam gian
// doan cong viec cua cac giao xu khac.
it('canh bao ro rang rang moi nguoi phai ngung nhap lieu', () => {
  dung()
  expect(screen.getByText(/phải ngừng nhập liệu/i)).toBeDefined()
})

it('nut xac nhan bi KHOA khi chua go dung chuoi', async () => {
  dung()
  const nut = screen.getByRole('button', { name: /Phục hồi/ }) as HTMLButtonElement
  expect(nut.disabled).toBe(true)

  await userEvent.type(screen.getByLabelText(/gõ/i), 'phuc hoi toan bo')
  expect(nut.disabled).toBe(true)
})

it('nut mo khoa khi go DUNG chuoi, va goi onXacNhan voi dung tham so', async () => {
  const { onXacNhan } = dung()
  await userEvent.type(screen.getByLabelText(/gõ/i), CHUOI_XAC_NHAN_PHUC_HOI)
  const nut = screen.getByRole('button', { name: /Phục hồi/ }) as HTMLButtonElement
  expect(nut.disabled).toBe(false)

  await userEvent.click(nut)
  expect(onXacNhan).toHaveBeenCalledWith('ab12cd34', CHUOI_XAC_NHAN_PHUC_HOI)
})

it('noi ro pham vi la TOAN MAY CHU, khong rieng mot giao xu', () => {
  dung()
  expect(screen.getByText(/toàn bộ máy chủ/i)).toBeDefined()
})

it('hien thoi diem ban sao theo dd\/MM\/yyyy HH:mm', () => {
  dung()
  // Chuoi thoi diem xuat hien ca trong doan mo ta lan nhan nut ("Phuc hoi ve ...") — dung
  // getAllByText thay vi getByText de khong bi loi "tim thay nhieu phan tu".
  expect(screen.getAllByText(/13\/09\/2026 \d{2}:\d{2}/).length).toBeGreaterThan(0)
})
