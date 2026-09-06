import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem,
} from './types'

class LoiXungDot extends Error {}
export { LoiXungDot }

async function goi<T>(duong: string, tuyChon?: RequestInit): Promise<T> {
  const res = await fetch(duong, {
    ...tuyChon,
    headers: { 'Content-Type': 'application/json', ...tuyChon?.headers },
  })
  if (res.status === 409) {
    const { thongBao } = await res.json()
    throw new LoiXungDot(thongBao)
  }
  if (!res.ok) throw new Error(`${res.status} khi gọi ${duong}`)
  return res.status === 204 ? (undefined as T) : ((await res.json()) as T)
}

const thamSo = (giaoHoId?: string, chiKhongThongKe?: boolean) => {
  const p = new URLSearchParams()
  if (giaoHoId) p.set('giaoHoId', giaoHoId)
  if (chiKhongThongKe) p.set('chiKhongThongKe', 'true')
  const s = p.toString()
  return s ? `?${s}` : ''
}

export const api = {
  giaDinh: {
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaDinhListItem[]>(`/api/gia-dinh${thamSo(giaoHoId, chiKhongThongKe)}`),
    chiTiet: (id: string) => goi<GiaDinhDetail>(`/api/gia-dinh/${id}`),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/gia-dinh/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    thanhVien: (id: string) => goi<GiaoDanListItem[]>(`/api/gia-dinh/${id}/thanh-vien`),
  },
  giaoDan: {
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaoDanListItem[]>(`/api/giao-dan${thamSo(giaoHoId, chiKhongThongKe)}`),
    chiTiet: (id: string) => goi<GiaoDanDetail>(`/api/giao-dan/${id}`),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/giao-dan/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
  },
}
