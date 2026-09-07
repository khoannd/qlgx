import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem, GiaoDanTimKiem, GiaoHo,
  HoiDoanCuaGiaoDan, HoiDoanDanhMuc, HonPhoiCuaGiaoDan, TanHienCuaGiaoDan,
  TaiKhoanItem, DangNhapKetQua, SucKhoe,
} from './types'
import { authStore } from './authStore'

class LoiXungDot extends Error {}
export { LoiXungDot }

class ChuaDangNhap extends Error {}
export { ChuaDangNhap }

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
  // GiaoXuId của phiên KHÔNG BAO GIỜ đi qua tham số ở đây — nó nằm trong claim của token,
  // đọc phía máy chủ (xem BoiCanhGiaoXuTuNguoiDung.cs). Ở đây chỉ đính token, không có chỗ
  // nào trong toàn bộ client này được phép thêm giaoXuId vào query/body.
  const token = authStore.layToken()
  let res: Response
  try {
    res = await fetch(duong, {
      ...tuyChon,
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...tuyChon?.headers,
      },
    })
  } catch (loiMang) {
    // Lỗi mạng (server chưa chạy, mất kết nối…) KHÔNG được nuốt thành mảng rỗng — người dùng
    // cần biết đây là sự cố kết nối, không phải "giáo xứ chưa có dữ liệu". Phân biệt hai
    // trường hợp bằng `navigator.onLine` (Task 16): mất mạng THẬT (giáo xứ vùng xa, đường
    // truyền chập chờn) cần một câu trấn an — dữ liệu đang gõ không mất (xem `lib/banNhap.ts`)
    // và thử lại được — khác với máy chủ chưa chạy lúc phát triển.
    console.error(`Lỗi mạng khi gọi ${duong}`, loiMang)
    const dangMatMang = typeof navigator !== 'undefined' && navigator.onLine === false
    throw new Error(
      dangMatMang
        ? 'Mất kết nối mạng. Dữ liệu bạn đã nhập vẫn được giữ nguyên trên máy — hãy thử lại khi có mạng.'
        : `Không kết nối được máy chủ khi gọi ${duong}. Kiểm tra Qlgx.Api đã chạy chưa.`,
    )
  }

  if (res.status === 401) {
    // Token thiếu/hết hạn/sai — báo cho AuthProvider tự đăng xuất và hiện lại màn hình đăng
    // nhập. KHÔNG xoá bản nháp form đang gõ (việc đó do cơ chế nháp localStorage của từng
    // form chi tiết đảm nhiệm, độc lập với đăng nhập).
    authStore.baoHet401()
    throw new ChuaDangNhap('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.')
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

/** Tải một tệp nhị phân (PDF in ấn — xem `Qlgx.Api/Endpoints/GiaoDanEndpoints.cs`
 * `/in/ly-lich-ca-nhan`) và kích trình duyệt lưu về máy, thay vì cố `res.json()` như `goi()`.
 * Đọc tên tệp thật từ header `Content-Disposition` mà `Results.File(...)` phía máy chủ đã đặt
 * sẵn (ví dụ `LyLichCaNhan_1234.pdf`), rơi về một tên chung nếu vì lý do gì đó thiếu header. */
async function taiTepIn(duong: string, tenTepMacDinh: string): Promise<void> {
  const token = authStore.layToken()
  let res: Response
  try {
    res = await fetch(duong, { headers: token ? { Authorization: `Bearer ${token}` } : {} })
  } catch (loiMang) {
    console.error(`Lỗi mạng khi in ${duong}`, loiMang)
    throw new Error('Không kết nối được máy chủ để in. Kiểm tra Qlgx.Api đã chạy chưa.')
  }
  if (res.status === 401) {
    authStore.baoHet401()
    throw new ChuaDangNhap('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.')
  }
  if (res.status === 404) {
    throw new Error('Không tìm thấy dữ liệu để in — có thể bản ghi đã bị xoá.')
  }
  if (!res.ok) {
    const thongBao = await docThongBaoLoi(res)
    throw new Error(thongBao ?? `Máy chủ trả lỗi ${res.status} khi in.`)
  }
  const blob = await res.blob()
  const dispo = res.headers.get('Content-Disposition') ?? ''
  const ten = /filename="?([^";]+)"?/.exec(dispo)?.[1] ?? tenTepMacDinh
  const url = URL.createObjectURL(blob)
  try {
    const a = document.createElement('a')
    a.href = url
    a.download = ten
    document.body.appendChild(a)
    a.click()
    a.remove()
  } finally {
    URL.revokeObjectURL(url)
  }
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

/** Ánh xạ 1-1 với `KetQuaThemThanhVienDto` — dùng chung cho cả "Thêm thành viên"
 * (`POST /api/gia-dinh/{id}/thanh-vien`) lẫn "Gán vợ chồng" (`PUT .../vo-chong/{vaiTro}`).
 * `giaoDanId === null` nghĩa là CHƯA lưu — `canhBao` chứa hoặc (a) cảnh báo nghiệp vụ thật cần
 * xác nhận (gửi lại `boQuaCanhBao: true`), hoặc (b) một câu cảnh báo CỐ ĐỊNH (sentinel) yêu cầu
 * client quyết định thêm — xem `lib/canhBaoGiaDinh.ts` để phân biệt hai loại này. */
export type KetQuaGhiGiaDinh = { giaoDanId: string | null; canhBao: string[] }

export const api = {
  giaDinh: {
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaDinhListItem[]>(`/api/gia-dinh${thamSo(giaoHoId, chiKhongThongKe)}`),
    chiTiet: (id: string) => goi<GiaDinhDetail>(`/api/gia-dinh/${id}`),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/gia-dinh/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    thanhVien: (id: string) => goi<GiaoDanListItem[]>(`/api/gia-dinh/${id}/thanh-vien`),
    /** Xoá gia đình — mềm (`vinhVien=false`, mặc định) hoặc vĩnh viễn, đúng 2 lựa chọn
     * [No]/[Yes] của hộp thoại 3 nút gốc (xem `GiaDinhService.Xoa`, KHÔNG có điều kiện chặn
     * nào khác giáo dân). */
    xoa: (id: string, vinhVien: boolean) =>
      goi<void>(`/api/gia-dinh/${id}?vinhVien=${vinhVien}`, { method: 'DELETE' }),
    /** Tạo một gia đình mới trống (chỉ Tên gia đình + Giáo họ) — xem `GiaDinhService.Tao`. */
    tao: (than: unknown) =>
      goi<{ id: string; maGiaDinhCu: number }>('/api/gia-dinh', { method: 'POST', body: JSON.stringify(than) }),
    /** Gán/đổi Người nam (vaiTro=0) hay Người nữ (vaiTro=1) — xem `GiaDinhService.GanVoChong`. */
    ganVoChong: (id: string, vaiTro: 0 | 1, than: unknown) =>
      goi<KetQuaGhiGiaDinh>(`/api/gia-dinh/${id}/vo-chong/${vaiTro}`, { method: 'PUT', body: JSON.stringify(than) }),
    /** Thêm một người vào lưới "Thành viên khác" — xem `GiaDinhService.ThemThanhVien`. */
    themThanhVien: (id: string, than: unknown) =>
      goi<KetQuaGhiGiaDinh>(`/api/gia-dinh/${id}/thanh-vien`, { method: 'POST', body: JSON.stringify(than) }),
    /** Xoá VĨNH VIỄN một thành viên (kể cả Người nam/nữ nếu vaiTro=0/1) — xem
     * can-review-sau.md mục 5. */
    xoaThanhVien: (id: string, giaoDanId: string, vaiTro: number) =>
      goi<void>(`/api/gia-dinh/${id}/thanh-vien/${giaoDanId}/${vaiTro}`, { method: 'DELETE' }),
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
    /** In "Lý lịch cá nhân" (mẫu đầu tiên của hạ tầng in ấn — xem
     * docs/superpowers/specs/man-hinh/in-an.md) — tải PDF về máy, không mở tab mới. */
    inLyLichCaNhan: (id: string) =>
      taiTepIn(`/api/giao-dan/${id}/in/ly-lich-ca-nhan`, 'LyLichCaNhan.pdf'),
  },
  hoiDoan: {
    danhMuc: () => goi<HoiDoanDanhMuc[]>('/api/hoi-doan'),
  },
  giaoHo: {
    // Danh mục thật (Id + tên) — thay data/giaoHoTam.ts hard-code theo tên, xem
    // docs/superpowers/specs/man-hinh/can-review-sau.md mục 19.
    danhMuc: () => goi<GiaoHo[]>('/api/giao-ho'),
  },
  timKiem: {
    // Dùng cho GxPicker thật (gõ để tìm Tên Cha/Mẹ, Người nam/nữ…) — giới hạn kết quả, KHÔNG
    // tải hết danh sách giáo dân về trình duyệt.
    giaoDan: (tuKhoa: string, limit = 20) =>
      goi<GiaoDanTimKiem[]>(`/api/giao-dan/tim?tuKhoa=${encodeURIComponent(tuKhoa)}&limit=${limit}`),
  },
  auth: {
    /** KHÔNG dùng `goi()` — endpoint này chủ ý ẩn danh và một mật khẩu sai cũng trả về 401
     * (đúng ngữ nghĩa HTTP), nhưng đó KHÔNG phải "token hết hạn" nên không được kích hoạt
     * luồng tự đăng xuất của `goi()`. Xử lý status ngay tại đây. */
    dangNhap: async (tenTaiKhoan: string, matKhau: string): Promise<DangNhapKetQua> => {
      const res = await fetch('/api/auth/dang-nhap', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tenTaiKhoan, matKhau }),
      })
      if (res.status === 401) {
        throw new Error('Tên đăng nhập hoặc mật khẩu không chính xác')
      }
      if (!res.ok) {
        throw new Error(`Máy chủ trả lỗi ${res.status} khi đăng nhập`)
      }
      return (await res.json()) as DangNhapKetQua
    },
    toi: () => goi<{
      tenTaiKhoan: string; hoTen: string | null; loaiTaiKhoan: number | null
      giaoXuId: string | null; tenGiaoXu: string | null
    }>('/api/auth/toi'),
  },
  /** Anonymous — chỉ dùng để hiện phiên bản bản web thật ở chân SideNav (xem
   * can-review-sau.md mục 32), không mang dữ liệu giáo xứ nào nên không cần token. */
  he: {
    sucKhoe: () => goi<SucKhoe>('/api/suc-khoe'),
  },
  taiKhoan: {
    danhSach: () => goi<TaiKhoanItem[]>('/api/tai-khoan'),
    tao: (than: unknown) => goi<{ id: string }>('/api/tai-khoan', { method: 'POST', body: JSON.stringify(than) }),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/tai-khoan/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoa: (id: string) => goi<void>(`/api/tai-khoan/${id}`, { method: 'DELETE' }),
  },
}
