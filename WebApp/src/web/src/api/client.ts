import type {
  GiaDinhDetail, GiaDinhListItem, GiaoDanDetail, GiaoDanListItem, GiaoDanTimKiem, GiaoHo,
  HoiDoanCuaGiaoDan, HoiDoanDanhMuc, HoiDoanQuanLy, ThanhVienHoiDoan, HonPhoiCuaGiaoDan, TanHienCuaGiaoDan,
  TaiKhoanItem, DangNhapKetQua, GiaoXuLuaChon, SucKhoe,
  GiaoPhan, GiaoHatQuanLy, GiaoXuQuanLy, BaoCaoXemTruoc, TrangThaiNhapDuLieu,
  DotBiTichListItem, DotBiTichDetail, LoaiBiTich, RaoHonPhoiListItem, RaoHonPhoiDetail,
  KhoiGiaoLy, LopGiaoLy, HocVienLopGiaoLy, GiaoLyVienLop,
} from './types'
import { authStore } from './authStore'

class LoiXungDot extends Error {}
export { LoiXungDot }

class ChuaDangNhap extends Error {}
export { ChuaDangNhap }

/** Ném khi tên đăng nhập trùng ở nhiều giáo xứ (xem AuthService.DangNhap mục M1) — client
 * PHẢI cho người dùng chọn đúng giáo xứ trong `danhSachGiaoXu` rồi gọi lại `api.auth.dangNhap`
 * kèm `giaoXuId`, không được tự đoán. */
class CanChonGiaoXu extends Error {
  danhSachGiaoXu: GiaoXuLuaChon[]
  constructor(thongBao: string, danhSachGiaoXu: GiaoXuLuaChon[]) {
    super(thongBao)
    this.danhSachGiaoXu = danhSachGiaoXu
  }
}
export { CanChonGiaoXu }

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

/** Tải ảnh đại diện (Task 1.2 VIEC-TIEP-THEO.md) về DẠNG BLOB rồi dựng object URL — KHÔNG
 * gán thẳng `duong` vào `<img src>` vì endpoint đòi RequireAuthorization()
 * (`Authorization: Bearer …`) mà thẻ `<img>` không tự đính header được, giống lý do
 * `taiTepIn` phải tự fetch() thay vì để trình duyệt tải trực tiếp. Trả `null` khi CHƯA có ảnh
 * (404 — không phải lỗi, đúng trạng thái "Chưa có hình") hoặc khi có lỗi mạng/xác thực; nơi gọi
 * coi cả hai như nhau (không hiện khung lỗi cho một chỗ chỉ là chưa có ảnh). */
async function layAnhBlobUrl(duong: string): Promise<string | null> {
  const token = authStore.layToken()
  let res: Response
  try {
    res = await fetch(duong, { headers: token ? { Authorization: `Bearer ${token}` } : {} })
  } catch (loiMang) {
    console.error(`Lỗi mạng khi tải ảnh ${duong}`, loiMang)
    return null
  }
  if (!res.ok) return null
  const blob = await res.blob()
  return URL.createObjectURL(blob)
}

/** Tải một ảnh lên qua `multipart/form-data` (trường "tep", đúng tên tham số IFormFile phía
 * `GiaoDanEndpoints.cs`/`GiaDinhEndpoints.cs`). KHÔNG dùng `goi()` — đó chỉ gửi JSON. Việc
 * kiểm định dạng/kích thước THẬT nằm ở máy chủ (`XuLyAnh.cs`); ở đây chỉ đọc thông báo lỗi
 * tiếng Việt máy chủ trả về (400) để hiện lại cho người dùng, không tự đoán trước lỗi gì. */
async function taiAnhLen(duong: string, tep: File): Promise<void> {
  const token = authStore.layToken()
  const than = new FormData()
  than.append('tep', tep)
  let res: Response
  try {
    res = await fetch(duong, {
      method: 'POST',
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      body: than,
    })
  } catch (loiMang) {
    console.error(`Lỗi mạng khi tải ảnh lên ${duong}`, loiMang)
    throw new Error('Không kết nối được máy chủ để tải ảnh lên. Kiểm tra Qlgx.Api đã chạy chưa.')
  }
  if (res.status === 401) {
    authStore.baoHet401()
    throw new ChuaDangNhap('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.')
  }
  if (!res.ok) {
    const thongBao = await docThongBaoLoi(res)
    throw new Error(thongBao ?? `Máy chủ trả lỗi ${res.status} khi tải ảnh lên.`)
  }
}

/** Tải một tệp lên qua `multipart/form-data` (trường "tep") rồi ĐỌC LẠI thân JSON trả về —
 * khác `taiAnhLen` (chỉ cần biết thành công/thất bại): màn hình "Nhập dữ liệu Access" cần đọc
 * báo cáo đối chiếu (BaoCaoXemTruoc/TrangThaiNhapDuLieu) ngay trong thân phản hồi 200. Việc
 * kiểm định dạng tệp THẬT (chữ ký gzip, kích thước) nằm ở máy chủ (NhapDuLieuService) — ở đây
 * chỉ chuyển tiếp thông báo lỗi tiếng Việt máy chủ trả về (400). */
async function taiTepLenVaDoc<T>(duong: string, tep: File): Promise<T> {
  const token = authStore.layToken()
  const than = new FormData()
  than.append('tep', tep)
  let res: Response
  try {
    res = await fetch(duong, {
      method: 'POST',
      headers: token ? { Authorization: `Bearer ${token}` } : {},
      body: than,
    })
  } catch (loiMang) {
    console.error(`Lỗi mạng khi tải tệp lên ${duong}`, loiMang)
    throw new Error('Không kết nối được máy chủ để tải tệp lên. Kiểm tra Qlgx.Api đã chạy chưa.')
  }
  if (res.status === 401) {
    authStore.baoHet401()
    throw new ChuaDangNhap('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.')
  }
  if (!res.ok) {
    const thongBao = await docThongBaoLoi(res)
    throw new Error(thongBao ?? `Máy chủ trả lỗi ${res.status} khi tải tệp lên.`)
  }
  return (await res.json()) as T
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
    /** In "Phiếu gia đình" (xem docs/superpowers/specs/man-hinh/in-an.md) — tải PDF về máy. */
    inPhieuGiaDinh: (id: string) =>
      taiTepIn(`/api/gia-dinh/${id}/in/phieu-gia-dinh`, 'PhieuGiaDinh.pdf'),
    /** In "Chứng nhận hôn phối" — 404 khi gia đình chưa có hôn phối nào để chứng nhận. */
    inChungNhanHonPhoi: (id: string) =>
      taiTepIn(`/api/gia-dinh/${id}/in/chung-nhan-hon-phoi`, 'ChungNhanHonPhoi.pdf'),
    /** "Xuất Excel" (thay CSV cũ) — tải tệp .xlsx thật về máy, tôn trọng đúng bộ lọc đang áp
     * dụng trên màn hình "Danh sách gia đình" (giáo họ đã chọn/ô "chỉ xem không thống kê"),
     * dùng lại `thamSo()` giống `danhSach()` ở trên — cùng tham số, không viết lại logic lọc. */
    xuatExcel: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      taiTepIn(`/api/gia-dinh/xuat-excel${thamSo(giaoHoId, chiKhongThongKe)}`, 'DanhSachGiaDinh.xlsx'),
    /** "Hồ sơ lưu trữ gia đình" (frmGiaDinhLuuTruList.cs) — gia đình đã xoá mềm HOẶC đã chuyển
     * xứ (OR, xem GiaDinhService.LayDanhSachLuuTru). */
    danhSachLuuTru: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaDinhListItem[]>(`/api/gia-dinh/luu-tru${thamSo(giaoHoId, chiKhongThongKe)}`),
    xuatExcelLuuTru: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      taiTepIn(`/api/gia-dinh/luu-tru/xuat-excel${thamSo(giaoHoId, chiKhongThongKe)}`, 'HoSoLuuTruGiaDinh.xlsx'),
    /** Ảnh đại diện gia đình (Task 1.2) — cùng ba thao tác với giaoDan bên dưới. */
    layAnh: (id: string) => layAnhBlobUrl(`/api/gia-dinh/${id}/anh-dai-dien`),
    taiAnhLen: (id: string, tep: File) => taiAnhLen(`/api/gia-dinh/${id}/anh-dai-dien`, tep),
    xoaAnh: (id: string) => goi<void>(`/api/gia-dinh/${id}/anh-dai-dien`, { method: 'DELETE' }),
  },
  giaoDan: {
    danhSach: (giaoHoId?: string, chiKhongThongKe?: boolean, hienCaDaMat?: boolean) =>
      goi<GiaoDanListItem[]>(`/api/giao-dan${thamSo(giaoHoId, chiKhongThongKe, hienCaDaMat)}`),
    /** "Xuất Excel" (thay CSV cũ) — cùng ba tham số lọc với `danhSach()` ở trên, tôn trọng đúng
     * bộ lọc đang áp dụng trên màn hình "Danh sách giáo dân". */
    xuatExcel: (giaoHoId?: string, chiKhongThongKe?: boolean, hienCaDaMat?: boolean) =>
      taiTepIn(`/api/giao-dan/xuat-excel${thamSo(giaoHoId, chiKhongThongKe, hienCaDaMat)}`, 'DanhSachGiaoDan.xlsx'),
    /** "Hồ sơ lưu trữ giáo dân" (frmGiaoDanLuuTruList.cs) — giáo dân đã xoá mềm, HOẶC đã qua
     * đời, HOẶC đã chuyển xứ (OR, xem GiaoDanService.LayDanhSachLuuTru). */
    danhSachLuuTru: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      goi<GiaoDanListItem[]>(`/api/giao-dan/luu-tru${thamSo(giaoHoId, chiKhongThongKe)}`),
    xuatExcelLuuTru: (giaoHoId?: string, chiKhongThongKe?: boolean) =>
      taiTepIn(`/api/giao-dan/luu-tru/xuat-excel${thamSo(giaoHoId, chiKhongThongKe)}`, 'HoSoLuuTruGiaoDan.xlsx'),
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
    /** In "Chứng nhận bí tích" — `loai` rỗng/undefined = mục chung "In chứng nhận bí tích"
     * (liệt kê cả ba); "RuaToi"/"RuocLe"/"ThemSuc" = mục riêng từng bí tích. */
    inChungNhanBiTich: (id: string, loai?: 'RuaToi' | 'RuocLe' | 'ThemSuc') =>
      taiTepIn(`/api/giao-dan/${id}/in/chung-nhan-bi-tich${loai ? `?loai=${loai}` : ''}`, 'ChungNhanBiTich.pdf'),
    /** Ảnh đại diện (Task 1.2 VIEC-TIEP-THEO.md, xem can-review-sau.md mục 36) — `layAnh` trả
     * object URL (hoặc `null` nếu chưa có ảnh/lỗi mạng, xem `layAnhBlobUrl`), `taiAnhLen` gửi
     * multipart, `xoaAnh` xoá hẳn. */
    layAnh: (id: string) => layAnhBlobUrl(`/api/giao-dan/${id}/anh-dai-dien`),
    taiAnhLen: (id: string, tep: File) => taiAnhLen(`/api/giao-dan/${id}/anh-dai-dien`, tep),
    xoaAnh: (id: string) => goi<void>(`/api/giao-dan/${id}/anh-dai-dien`, { method: 'DELETE' }),
  },
  hoiDoan: {
    danhMuc: () => goi<HoiDoanDanhMuc[]>('/api/hoi-doan'),
  },
  /** Màn hình "Danh sách hội đoàn" (frmHoiDoanList.cs + frmHoiDoan.cs, cấp quản lý danh mục) —
   * xem docs/superpowers/specs/man-hinh/hoi-doan-danh-sach.md. Khác `api.hoiDoan` ở trên (danh
   * mục rút gọn cho combo) và `api.giaoDan.hoiDoan*` (lịch sử của MỘT giáo dân). */
  hoiDoanQuanLy: {
    danhSach: () => goi<HoiDoanQuanLy[]>('/api/hoi-doan/danh-sach'),
    them: (than: unknown) => goi<{ id: string }>('/api/hoi-doan', { method: 'POST', body: JSON.stringify(than) }),
    sua: (id: string, than: unknown) =>
      goi<void>(`/api/hoi-doan/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoa: (id: string) => goi<void>(`/api/hoi-doan/${id}`, { method: 'DELETE' }),
    thanhVien: (id: string, chiXemHienTai: boolean) =>
      goi<ThanhVienHoiDoan[]>(`/api/hoi-doan/${id}/thanh-vien?chiXemHienTai=${chiXemHienTai}`),
    themThanhVien: (id: string, than: unknown) =>
      goi<void>(`/api/hoi-doan/${id}/thanh-vien`, { method: 'POST', body: JSON.stringify(than) }),
    suaThanhVien: (chiTietId: string, than: unknown) =>
      goi<void>(`/api/hoi-doan/thanh-vien/${chiTietId}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoaThanhVien: (chiTietId: string) =>
      goi<void>(`/api/hoi-doan/thanh-vien/${chiTietId}`, { method: 'DELETE' }),
  },
  /** Phân hệ Giáo lý — Khối/Lớp/Học viên/Giáo lý viên (`frmKhoiGiaoLyList.cs` +
   * `frmKhoiGiaoLy.cs` + `frmLopGiaoLy.cs`) — xem docs/superpowers/specs/man-hinh/giao-ly.md. */
  giaoLy: {
    khoi: () => goi<KhoiGiaoLy[]>('/api/giao-ly/khoi'),
    themKhoi: (than: unknown) => goi<{ id: string }>('/api/giao-ly/khoi', { method: 'POST', body: JSON.stringify(than) }),
    suaKhoi: (id: string, than: unknown) =>
      goi<void>(`/api/giao-ly/khoi/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoaKhoi: (id: string) => goi<void>(`/api/giao-ly/khoi/${id}`, { method: 'DELETE' }),
    lop: (khoiId: string, nam?: number | null) =>
      goi<LopGiaoLy[]>(`/api/giao-ly/khoi/${khoiId}/lop${nam ? `?nam=${nam}` : ''}`),
    themLop: (khoiId: string, than: unknown) =>
      goi<{ id: string }>(`/api/giao-ly/khoi/${khoiId}/lop`, { method: 'POST', body: JSON.stringify(than) }),
    suaLop: (id: string, than: unknown) =>
      goi<void>(`/api/giao-ly/lop/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoaLop: (id: string) => goi<void>(`/api/giao-ly/lop/${id}`, { method: 'DELETE' }),
    hocVien: (lopId: string) => goi<HocVienLopGiaoLy[]>(`/api/giao-ly/lop/${lopId}/hoc-vien`),
    themHocVien: (lopId: string, giaoDanId: string) =>
      goi<void>(`/api/giao-ly/lop/${lopId}/hoc-vien`, { method: 'POST', body: JSON.stringify({ giaoDanId }) }),
    suaHocVien: (chiTietId: string, than: unknown) =>
      goi<void>(`/api/giao-ly/hoc-vien/${chiTietId}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoaHocVien: (chiTietId: string) => goi<void>(`/api/giao-ly/hoc-vien/${chiTietId}`, { method: 'DELETE' }),
    giaoLyVien: (lopId: string) => goi<GiaoLyVienLop[]>(`/api/giao-ly/lop/${lopId}/giao-ly-vien`),
    themGiaoLyVien: (lopId: string, giaoDanId: string) =>
      goi<void>(`/api/giao-ly/lop/${lopId}/giao-ly-vien`, { method: 'POST', body: JSON.stringify({ giaoDanId }) }),
    xoaGiaoLyVien: (id: string) => goi<void>(`/api/giao-ly/giao-ly-vien/${id}`, { method: 'DELETE' }),
  },
  /** "Danh sách sổ bí tích" (frmDotBiTichList.cs + frmBiTichChiTiet.cs) — xem
   * docs/superpowers/specs/man-hinh/so-bi-tich.md. */
  dotBiTich: {
    danhSach: (loaiBiTich: LoaiBiTich, tuNam?: number, denNam?: number) => {
      const p = new URLSearchParams({ loaiBiTich: String(loaiBiTich) })
      if (tuNam) p.set('tuNam', String(tuNam))
      if (denNam) p.set('denNam', String(denNam))
      return goi<DotBiTichListItem[]>(`/api/dot-bi-tich?${p.toString()}`)
    },
    chiTiet: (id: string) => goi<DotBiTichDetail>(`/api/dot-bi-tich/${id}`),
    tao: (than: unknown) => goi<DotBiTichDetail>('/api/dot-bi-tich', { method: 'POST', body: JSON.stringify(than) }),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/dot-bi-tich/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoa: (id: string) => goi<void>(`/api/dot-bi-tich/${id}`, { method: 'DELETE' }),
    themNguoiNhan: (id: string, than: unknown) =>
      goi<void>(`/api/dot-bi-tich/${id}/nguoi-nhan`, { method: 'POST', body: JSON.stringify(than) }),
    suaNguoiNhan: (id: string, giaoDanId: string, than: unknown) =>
      goi<void>(`/api/dot-bi-tich/${id}/nguoi-nhan/${giaoDanId}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoaNguoiNhan: (id: string, giaoDanId: string, xoaThongTinBiTich: boolean) =>
      goi<void>(`/api/dot-bi-tich/${id}/nguoi-nhan/${giaoDanId}?xoaThongTinBiTich=${xoaThongTinBiTich}`,
        { method: 'DELETE' }),
  },
  /** "Danh sách rao hôn phối" (frmRaoHonPhoiList.cs + frmRaoHonPhoi.cs) — xem
   * docs/superpowers/specs/man-hinh/rao-hon-phoi.md. */
  raoHonPhoi: {
    danhSach: (xemTatCa?: boolean) =>
      goi<RaoHonPhoiListItem[]>(`/api/rao-hon-phoi${xemTatCa ? '?xemTatCa=true' : ''}`),
    chiTiet: (id: string) => goi<RaoHonPhoiDetail>(`/api/rao-hon-phoi/${id}`),
    tao: (than: unknown) => goi<RaoHonPhoiDetail>('/api/rao-hon-phoi', { method: 'POST', body: JSON.stringify(than) }),
    capNhat: (id: string, than: unknown) =>
      goi<void>(`/api/rao-hon-phoi/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    xoa: (id: string) => goi<void>(`/api/rao-hon-phoi/${id}`, { method: 'DELETE' }),
  },
  danhMuc: {
    // Danh sách "Tên thánh" tĩnh (bảng `du_lieu_chung`, 343 dòng đã chuyển từ Access) — một
    // trong hai nguồn gợi ý nhập liệu, nguồn còn lại là lịch sử `localStorage` (xem
    // `lib/goiYNhapLieu.ts` và docs/superpowers/specs/man-hinh/ho-tro-nhap-lieu.md mục C.1).
    tenThanh: () => goi<string[]>('/api/danh-muc/ten-thanh'),
  },
  giaoHo: {
    // Danh mục thật (Id + tên) — thay data/giaoHoTam.ts hard-code theo tên, xem
    // docs/superpowers/specs/man-hinh/can-review-sau.md mục 19.
    danhMuc: () => goi<GiaoHo[]>('/api/giao-ho'),
    // Thêm/sửa — luôn trong phạm vi giáo xứ của người gọi (giaoXuId từ claim, không nhận từ
    // đây), khác nhóm "quanTri" bên dưới vốn cố ý xuyên giáo xứ.
    them: (than: { tenGiaoHo: string; giaoHoChaId?: string | null }) =>
      goi<void>('/api/giao-ho', { method: 'POST', body: JSON.stringify(than) }),
    sua: (id: string, than: { tenGiaoHo: string; giaoHoChaId?: string | null }) =>
      goi<void>(`/api/giao-ho/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
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
    dangNhap: async (tenTaiKhoan: string, matKhau: string, giaoXuId?: string): Promise<DangNhapKetQua> => {
      const res = await fetch('/api/auth/dang-nhap', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tenTaiKhoan, matKhau, giaoXuId: giaoXuId ?? null }),
      })
      if (res.status === 400) {
        // Có thể là "cần chọn giáo xứ" (canChonGiaoXu:true, kèm danh sách) hoặc lỗi đầu vào
        // khác — phân biệt bằng cờ canChonGiaoXu, không suy đoán từ status code một mình.
        const than = await res.json().catch(() => null) as
          { thongBao?: string; canChonGiaoXu?: boolean; giaoXu?: GiaoXuLuaChon[] } | null
        if (than?.canChonGiaoXu) {
          throw new CanChonGiaoXu(than.thongBao ?? 'Vui lòng chọn giáo xứ', than.giaoXu ?? [])
        }
        throw new Error(than?.thongBao ?? 'Không đăng nhập được')
      }
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
    /** Tự đổi mật khẩu của chính mình (VIEC-TIEP-THEO.md mục 1.3) — máy chủ trả 400 kèm
     * `thongBao` rõ ràng khi mật khẩu hiện tại sai hoặc mật khẩu mới quá ngắn, `goi()` đã tự
     * ném Error với đúng thông báo đó. */
    doiMatKhau: (matKhauHienTai: string, matKhauMoi: string) =>
      goi<void>('/api/auth/mat-khau', { method: 'PUT', body: JSON.stringify({ matKhauHienTai, matKhauMoi }) }),
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
  /** Màn hình "Quản lý giáo phận/giáo hạt/giáo xứ" (policy "QuanTriHeThong", LoaiTaiKhoan=9 —
   * xem docs/superpowers/specs/man-hinh/quan-ly-giao-xu.md). CỐ Ý xuyên giáo xứ: chỉ tài
   * khoản "Quản trị hệ thống" gọi được, quản trị viên thường của một giáo xứ nhận 403. */
  quanTri: {
    giaoPhan: {
      danhSach: () => goi<GiaoPhan[]>('/api/quan-tri/giao-phan'),
      tao: (than: { tenGiaoPhan: string; ghiChu: string | null }) =>
        goi<void>('/api/quan-tri/giao-phan', { method: 'POST', body: JSON.stringify(than) }),
      sua: (id: string, than: { tenGiaoPhan: string; ghiChu: string | null }) =>
        goi<void>(`/api/quan-tri/giao-phan/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    },
    giaoHat: {
      danhSach: () => goi<GiaoHatQuanLy[]>('/api/quan-tri/giao-hat'),
      tao: (than: { giaoPhanId: string; tenGiaoHat: string; ghiChu: string | null }) =>
        goi<void>('/api/quan-tri/giao-hat', { method: 'POST', body: JSON.stringify(than) }),
      sua: (id: string, than: { giaoPhanId: string; tenGiaoHat: string; ghiChu: string | null }) =>
        goi<void>(`/api/quan-tri/giao-hat/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
    },
    giaoXu: {
      danhSach: () => goi<GiaoXuQuanLy[]>('/api/quan-tri/giao-xu'),
      tao: (than: {
        giaoHatId: string; tenGiaoXu: string; diaChi: string | null
        dienThoai: string | null; email: string | null; website: string | null; ghiChu: string | null
      }) => goi<void>('/api/quan-tri/giao-xu', { method: 'POST', body: JSON.stringify(than) }),
      sua: (id: string, than: {
        giaoHatId: string; tenGiaoXu: string; diaChi: string | null
        dienThoai: string | null; email: string | null; website: string | null; ghiChu: string | null
      }) => goi<void>(`/api/quan-tri/giao-xu/${id}`, { method: 'PUT', body: JSON.stringify(than) }),
      /** Tạo tài khoản quản trị ĐẦU TIÊN cho một giáo xứ khác — luôn LoaiTaiKhoan=0, giaoXuId
       * lấy từ đường dẫn {id} (đường dẫn thứ tư được phép xuyên giáo xứ, xem spec mục 4). */
      taoTaiKhoan: (giaoXuId: string, than: {
        tenTaiKhoan: string; matKhau: string; hoTenNguoiDung: string
        email: string | null; soDienThoai: string | null
      }) => goi<{ id: string }>(`/api/quan-tri/giao-xu/${giaoXuId}/tai-khoan`,
        { method: 'POST', body: JSON.stringify(than) }),
    },
    /** Màn hình "Nhập dữ liệu Access" (policy "QuanTriHeThong", VIEC-TIEP-THEO.md mục 2.4) —
     * xem NhapDuLieuService.cs (Qlgx.Api) cho kiến trúc hai bước: quản trị viên chạy
     * Qlgx.Migration tại máy Windows của mình để rút gói dữ liệu trung gian (.json.gz), rồi
     * tải GÓI đó lên đây (KHÔNG tải file .mdb — máy chủ Linux không đọc được). */
    nhapDuLieu: {
      /** Chạy thử — KHÔNG ghi gì vào PostgreSQL, chỉ đối chiếu số dòng để xem trước. */
      xemTruoc: (giaoXuDichId: string, tep: File) =>
        taiTepLenVaDoc<BaoCaoXemTruoc>(`/api/quan-tri/nhap-du-lieu/${giaoXuDichId}/xem-truoc`, tep),
      /** Khởi động một lượt NHẬP THẬT, chạy NỀN phía máy chủ — trả JobId ngay, không đợi nhập
       * xong (có thể mất nhiều giây). Client tự gọi `trangThai` định kỳ để theo dõi. */
      batDau: (giaoXuDichId: string, tep: File, xacNhanGhiDe: boolean) =>
        taiTepLenVaDoc<{ jobId: string }>(
          `/api/quan-tri/nhap-du-lieu/${giaoXuDichId}/bat-dau?xacNhanGhiDe=${xacNhanGhiDe}`, tep),
      trangThai: (jobId: string) =>
        goi<TrangThaiNhapDuLieu>(`/api/quan-tri/nhap-du-lieu/trang-thai/${jobId}`),
    },
  },
}
