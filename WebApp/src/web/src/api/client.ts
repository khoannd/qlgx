import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem, HonPhoiCuaGiaoDan,
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
    honPhoi: (id: string) => goi<HonPhoiCuaGiaoDan[]>(`/api/giao-dan/${id}/hon-phoi`),
    capNhatHonPhoi: (honPhoiId: string, than: unknown) =>
      goi<void>(`/api/giao-dan/hon-phoi/${honPhoiId}`, { method: 'PUT', body: JSON.stringify(than) }),
  },
}
