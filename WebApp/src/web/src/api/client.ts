import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem, HoiDoanCuaGiaoDan,
  HoiDoanDanhMuc, HonPhoiCuaGiaoDan, TanHienCuaGiaoDan,
} from './types'

class LoiXungDot extends Error {}
export { LoiXungDot }

/** Đọc `{ thongBao }` từ thân lỗi JSON nếu có — endpoint trả 400/409 kèm thông báo tiếng
 * Việt sẵn (xem GiaDinhEndpoints/GiaoDanEndpoints); phần lớn lỗi khác (404, 500, lỗi mạng)
 * không có thân JSON nên phải chấp nhận rơi về thông báo chung. */
async function docThongBaoLoi(res: Response): Promise<string | null> {
  try {
    const than = await res.json()
    return typeof than?.thongBao === 'string' ? than.thongBao : null
  } catch {
    return null
  }
}

async function goi<T>(duong: string, tuyChon?: RequestInit): Promise<T> {
  let res: Response
  try {
    res = await fetch(duong, {
      ...tuyChon,
      headers: { 'Content-Type': 'application/json', ...tuyChon?.headers },
    })
  } catch (loiMang) {
    // Lỗi mạng (server chưa chạy, mất kết nối…) KHÔNG được nuốt thành mảng rỗng — người dùng
    // cần biết đây là sự cố kết nối, không phải "giáo xứ chưa có dữ liệu".
    console.error(`Lỗi mạng khi gọi ${duong}`, loiMang)
    throw new Error(`Không kết nối được máy chủ khi gọi ${duong}. Kiểm tra Qlgx.Api đã chạy chưa.`)
  }

  if (res.status === 409) {
    const thongBao = await docThongBaoLoi(res)
    throw new LoiXungDot(thongBao ?? 'Bản ghi vừa được người khác cập nhật.')
  }
  if (!res.ok) {
    const thongBao = await docThongBaoLoi(res)
    const loi = new Error(thongBao ?? `Máy chủ trả lỗi ${res.status} khi gọi ${duong}`)
    console.error(`Lỗi ${res.status} khi gọi ${duong}`, thongBao)
    throw loi
  }

  // KHÔNG chỉ dựa vào status 204: `Results.Ok()` không kèm giá trị (PUT thành công của cả
  // hai endpoint gia-dinh/giao-dan) trả về 200 nhưng thân rỗng — gọi thẳng `res.json()` trên
  // thân rỗng ném "Unexpected end of JSON input" (bắt được khi kiểm thử thật trên trình
  // duyệt, xem task-noi-frontend-report.md). Đọc text trước rồi mới parse là cách an toàn với
  // MỌI cách máy chủ báo "không có nội dung", bất kể mã trạng thái.
  const than = await res.text()
  return than ? (JSON.parse(than) as T) : (undefined as T)
}

const thamSo = (giaoHoId?: string, chiKhongThongKe?: boolean, hienCaDaMat?: boolean) => {
  const p = new URLSearchParams()
  if (giaoHoId) p.set('giaoHoId', giaoHoId)
  if (chiKhongThongKe) p.set('chiKhongThongKe', 'true')
  if (hienCaDaMat) p.set('hienCaDaMat', 'true')
  const s = p.toString()
  return s ? `?${s}` : ''
}

/** Kết quả tạo/sửa một giáo dân — đúng KetQuaLuuGiaoDanDto phía backend. `id === null` nghĩa
 * là còn cảnh báo (`canhBao`) chưa được xác nhận, CHƯA LƯU — gọi lại với `boQuaCanhBao: true`
 * trong thân yêu cầu để lưu bất chấp cảnh báo. */
export type KetQuaLuuGiaoDan = { id: string | null; canhBao: string[] }

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
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean, hienCaDaMat?: boolean) =>
      goi<GiaoDanListItem[]>(`/api/giao-dan${thamSo(giaoHoId, chiKhongThongKe, hienCaDaMat)}`),
    chiTiet: (id: string) => goi<GiaoDanDetail>(`/api/giao-dan/${id}`),
    // 201 (đã lưu) hoặc 200 (còn cảnh báo chưa xác nhận) — cả hai đọc cùng KetQuaLuuGiaoDan.
    taoMoi: (than: unknown) =>
      goi<KetQuaLuuGiaoDan>('/api/giao-dan', { method: 'POST', body: JSON.stringify(than) }),
    capNhat: (id: string, than: unknown) =>
      goi<KetQuaLuuGiaoDan>(`/api/giao-dan/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    // vinhVien=false (mặc định) = xoá mềm (đưa vào lưu trữ); vinhVien=true = xoá vĩnh viễn,
    // có thể bị máy chủ chặn (409) nếu giáo dân đang thuộc gia đình nào.
    xoa: (id: string, vinhVien: boolean) =>
      goi<void>(`/api/giao-dan/${id}?vinhVien=${vinhVien}`, { method: 'DELETE' }),
    honPhoi: (id: string) => goi<HonPhoiCuaGiaoDan[]>(`/api/giao-dan/${id}/hon-phoi`),
    capNhatHonPhoi: (honPhoiId: string, than: unknown) =>
      goi<void>(`/api/giao-dan/hon-phoi/${honPhoiId}`, { method: 'PUT', body: JSON.stringify(than) }),
    tanHien: (id: string) => goi<TanHienCuaGiaoDan[]>(`/api/giao-dan/${id}/tan-hien`),
    themTanHien: (id: string, than: unknown) =>
      goi<{ id: string }>(`/api/giao-dan/${id}/tan-hien`, { method: 'POST', body: JSON.stringify(than) }),
    capNhatTanHien: (tanHienId: string, than: unknown) =>
      goi<void>(`/api/giao-dan/tan-hien/${tanHienId}`, { method: 'PUT', body: JSON.stringify(than) }),
    hoiDoan: (id: string) => goi<HoiDoanCuaGiaoDan[]>(`/api/giao-dan/${id}/hoi-doan`),
    themHoiDoan: (id: string, than: unknown) =>
      goi<{ id: string }>(`/api/giao-dan/${id}/hoi-doan`, { method: 'POST', body: JSON.stringify(than) }),
    capNhatHoiDoan: (chiTietId: string, than: unknown) =>
      goi<void>(`/api/giao-dan/hoi-doan/${chiTietId}`, { method: 'PUT', body: JSON.stringify(than) }),
  },
  hoiDoan: {
    danhMuc: () => goi<HoiDoanDanhMuc[]>('/api/hoi-doan'),
  },
}
