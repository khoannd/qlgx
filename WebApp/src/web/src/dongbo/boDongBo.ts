/**
 * Bộ đồng bộ — Task 6, ghép Task 1-5 thành vòng chạy THẬT nói chuyện với ba đầu vào của kế hoạch 4
 * (`/api/dong-bo/{thay-doi,toan-bo,gui-len}` — xem `task-6-brief.md` để có bảng dịch TypeScript đầy
 * đủ ba DTO, và `WebApp/src/Qlgx.Api/Endpoints/DongBoEndpoints.cs` /
 * `WebApp/src/Qlgx.Api/Dtos/DongBoDtos.cs` phía máy chủ để đối chiếu khi cần).
 *
 * ## Vì sao KHÔNG dùng `src/api/client.ts`'s `goi()`
 *
 * `goi()` coi MỌI 409 là `LoiXungDot` ("bản ghi vừa được người khác cập nhật") — sai cho 409 kèm
 * `type === "may-chu-di-lui"` của `/thay-doi`: đó là LỚP 2 phát hiện máy chủ vừa bị khôi phục bằng
 * tay mà quên xoay epoch (spec 4.8.4), phải DỪNG HẲN đồng bộ và báo 🔴 — hoàn toàn khác "thử lưu lại
 * bình thường". `goiDongBo` dưới đây tự phân biệt 410 (epoch không khớp, tải lại `/toan-bo`) / 409
 * "may-chu-di-lui" (dừng đồng bộ) / lỗi khác, tham khảo phong cách đính `Authorization` của `goi()`
 * nhưng KHÔNG gọi lại `goi()`.
 *
 * ## Lịch chạy bốn nguồn kích hoạt (spec 7.3) — xem `batDauBoDongBo`
 *
 * 1. Máy chủ trả kèm dòng mới MỖI LẦN gửi lô lên (`GuiLenKetQua.dongMoi`) — vì `DongBoService.GuiLen`
 *    phía máy chủ luôn gọi `NhanVe` nội bộ (kể cả lô `thaoTac` RỖNG), một lần gọi `guiHangChoVaApDung`
 *    dưới đây LUÔN đồng thời là một lần "hỏi dữ liệu mới" — không cần gọi `/thay-doi` riêng cho vòng
 *    lặp định kỳ, chỉ cần MỘT loại lệnh gọi cho cả gửi lẫn nhận.
 * 2/3. `visibilitychange` (khi tab hiện lại)/`focus`/`online` — đánh thức vòng lặp ngay lập tức
 *    (`NguonSuKienDongBo.dangKy`), không đợi hết chu kỳ định kỳ.
 * 4. Định kỳ co giãn: `baoDangGo()` rút ngắn chu kỳ chờ tiếp theo xuống `CHU_KY_DANG_GO_MS`, các chu
 *    kỳ sau đó tự giãn về `CHU_KY_YEN_TINH_MS` nếu không có `baoDangGo()` nào thêm. Tab ẩn
 *    (`document.hidden`) làm `motLanDongBo` bỏ qua việc gửi lô NẾU hàng chờ rỗng (tức bỏ việc "hỏi dữ
 *    liệu mới" tốn một lượt gọi không cần thiết khi không ai nhìn màn hình) — nhưng hàng chờ CÓ gì
 *    thì luôn gửi, không bao giờ bị chặn bởi tab ẩn (quý sơ hay mở tab rồi để đó cả ngày).
 *
 * ## Ràng buộc 3 — MỘT giao dịch IndexedDB
 *
 * `apDungGuiLenKetQua` mở ĐÚNG MỘT `kho.transaction([...], 'readwrite')` bao cả bốn việc: ghi sổ đã
 * nhận (`ghiSoDaNhanTrongGiaoDich`), tiến con trỏ (`ghiConTroTrongGiaoDich`), ghi mục cần xem lại
 * cho kết quả `tu_choi` (`ghiCanXemLaiTrongGiaoDich`), và xoá các thao tác đã xử lý xong khỏi hàng
 * chờ (`xoaKhoiHangChoTrongGiaoDich`). Tách bốn việc này ra nhiều giao dịch sẽ tạo đúng lỗ hổng ràng
 * buộc 3 cấm: hỏng giữa chừng có thể để con trỏ đã tiến nhưng sổ đã nhận trống (Task 8 bù thiếu mà
 * không ai biết), hoặc một thao tác `tu_choi` đã bị xoá khỏi hàng chờ nhưng CHƯA kịp vào hộp cần xem
 * lại (dữ liệu người dùng nhập biến mất không dấu vết).
 *
 * ## `thietBiId`
 *
 * CHƯA có bảng `thiết_bị` ở kế hoạch này — `layThietBiId()` tự sinh một `crypto.randomUUID()` ổn
 * định, lưu `localStorage` (giá trị vô hướng đơn giản, không cần giao dịch IndexedDB), dùng ĐÚNG MỘT
 * giá trị này cho cả tham số khởi tạo `DongHoLogicMayCon` VÀ trường `thietBiId` của `GuiLenYeuCau`
 * (xem `khoiTaoPhuThuoc`) — hai chỗ dùng ID khác nhau sẽ khiến máy này tự phá hoà giữa các dấu do
 * chính nó phát ra.
 */
import { authStore } from '../api/authStore'
import {
  KHO_CAN_XEM_LAI,
  KHO_CON_TRO,
  KHO_HANG_CHO,
  KHO_SO_DA_NHAN,
} from '../kho/moKho'
import { docHangCho, xoaKhoiHangChoTrongGiaoDich, type DongHangCho } from '../kho/hangCho'
import { docConTro, ghiConTroTrongGiaoDich } from '../kho/conTro'
import { ghiSoDaNhanTrongGiaoDich, type DongDaNhan } from '../kho/soDaNhan'
import { ghiCanXemLaiTrongGiaoDich, type DongCanXemLai } from '../kho/canXemLai'
import { GUID_RONG, chuanHoaMocMayChu, soSanh, tuMocMs, type DauDongHo } from './dauDongHo'
import { DoanDongHo, DongHoDonDieu, DongHoLogicMayCon } from './dongHoMayCon'
import { TEN_KHOA_BAU_CHU, troThanhChuKhiCoTheChoDenKhiHuy, type NguonKhoa } from './bauChu'

// ============================================================================================
// Ba đầu vào của kế hoạch 4 — hình dạng THẬT, dịch từ WebApp/src/Qlgx.Api/Dtos/DongBoDtos.cs.
// Xem task-6-brief.md — camelCase đúng field mặc định của ASP.NET Core minimal API.
// ============================================================================================

export type DongHieuLucDto = {
  soThuTu: number
  bang: string
  banGhiId: string
  truong: string
  giaTri: string | null
  dongHoVatLy: string
  dongHoLogic: number
  thietBiId: string | null
  giaoDichId: string
}

export type NhanVeKetQua = { epoch: string; conTroMoi: number; conNua: boolean; dong: DongHieuLucDto[] }

export type ToanBoKetQua = { epoch: string; conTro: number; chupLuc: string; duLieuNen: string }

export type ThaoTacDto = {
  maThaoTac: string
  giaoDichId: string
  loai: string
  bang: string
  banGhiId: string
  truong: string
  giaTri: string | null
  dongHoVatLy: string
  dongHoLogic: number
  nguonGocEpoch: string | null
  nguonGocSoThuTu: number | null
}

export type GuiLenYeuCau = {
  thietBiId: string
  gioMayCon: string
  epoch: string | null
  conTro: number
  thaoTac: ThaoTacDto[]
}

export type KetQuaThaoTacDto = { maThaoTac: string; ketQua: 'ap' | 'thua' | 'tu_choi' | 'trung'; thongBao: string | null }

export type GuiLenKetQua = {
  epoch: string
  conTroMoi: number
  conNua: boolean
  ketQua: KetQuaThaoTacDto[]
  dongMoi: DongHieuLucDto[]
}

/** Ném khi 410 Gone — epoch client gửi lên không khớp epoch hiện tại của giáo xứ (`/thay-doi` VÀ
 * `/gui-len` dùng chung ngữ nghĩa này). Máy con PHẢI tải lại toàn bộ qua `/toan-bo`, KHÔNG được
 * retry mù (gửi lại y hệt sẽ nhận lại đúng lỗi này mãi mãi). Task 6 chỉ ném lỗi có tên riêng để tầng
 * gọi phân biệt được — việc TỰ ĐỘNG tải lại `/toan-bo` khi gặp lỗi này để dành cho Task 8/10 (xem
 * quyết định giới hạn phạm vi trong báo cáo Task 6: điểm nối `taiToanBoVaGiaiNen` đã có sẵn). */
export class LoiEpochKhongKhop extends Error {
  constructor() {
    super('Con trỏ đồng bộ không còn hợp lệ (epoch không khớp) — cần tải lại toàn bộ qua /toan-bo.')
    this.name = 'LoiEpochKhongKhop'
  }
}

/** Ném khi 409 kèm `type === "may-chu-di-lui"` của `/thay-doi` — LỚP 2 (spec 4.8.4) phát hiện máy
 * chủ có dấu hiệu vừa bị đưa về bản cũ. Tầng gọi (`batDauBoDongBo`) PHẢI dừng hẳn vòng đồng bộ khi
 * thấy lỗi này (không tự thử lại) — Task 10 sẽ đọc trạng thái này để chuyển 🔴, Task 6 chỉ cần expose
 * được (xem `DieuKhienBoDongBo.trangThai`). */
export class LoiMayChuDiLui extends Error {
  constructor(thongBao?: string) {
    super(thongBao ?? 'Máy chủ có dấu hiệu vừa bị đưa về bản cũ — đã dừng đồng bộ.')
    this.name = 'LoiMayChuDiLui'
  }
}

/**
 * Gọi mạng RIÊNG cho ba endpoint đồng bộ — KHÔNG gọi `goi()` của `client.ts` (xem chú thích đầu
 * file vì sao). Tham khảo phong cách đính `Authorization` của `goi()` nhưng tự phân biệt 410/409
 * "may-chu-di-lui"/lỗi khác thay vì coi mọi 409 là xung đột ghi thường.
 */
async function goiDongBo<T>(duong: string, tuyChon?: RequestInit): Promise<T> {
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
    console.error(`Dong bo: loi mang khi goi ${duong}`, loiMang)
    throw new Error('Mất kết nối mạng khi đồng bộ. Sẽ tự thử lại.')
  }

  if (res.status === 410) throw new LoiEpochKhongKhop()

  if (res.status === 409) {
    // Chỉ /thay-doi phát 409 kiểu Problem Details "may-chu-di-lui" (xem DongBoEndpoints.cs) —
    // đọc `type` từ thân JSON để phân biệt với một 409 khác (không nên xảy ra ở ba endpoint này,
    // nhưng không giả định suông — kiểm tra tường minh thay vì coi MỌI 409 là may-chu-di-lui).
    const than = await res.json().catch(() => null as { type?: string; detail?: string; title?: string } | null)
    if (than?.type === 'may-chu-di-lui') throw new LoiMayChuDiLui(than.detail ?? than.title)
    throw new Error(than?.detail ?? than?.title ?? 'Máy chủ trả lỗi 409 không xác định khi đồng bộ.')
  }

  if (!res.ok) {
    console.error(`Dong bo: loi ${res.status} khi goi ${duong}`)
    throw new Error(`Máy chủ trả lỗi ${res.status} khi đồng bộ.`)
  }

  const than = await res.text()
  return than ? (JSON.parse(than) as T) : (undefined as T)
}

function layThayDoi(epoch: string | null, tu: number, toiDa?: number): Promise<NhanVeKetQua> {
  const p = new URLSearchParams({ tu: String(tu) })
  if (epoch) p.set('epoch', epoch)
  if (toiDa) p.set('toiDa', String(toiDa))
  return goiDongBo<NhanVeKetQua>(`/api/dong-bo/thay-doi?${p.toString()}`)
}

function layToanBo(): Promise<ToanBoKetQua> {
  return goiDongBo<ToanBoKetQua>('/api/dong-bo/toan-bo')
}

function guiLen(yc: GuiLenYeuCau): Promise<GuiLenKetQua> {
  return goiDongBo<GuiLenKetQua>('/api/dong-bo/gui-len', { method: 'POST', body: JSON.stringify(yc) })
}

// ============================================================================================
// thietBiId — một ID ổn định cho trình duyệt này, sinh và lưu tại localStorage (KHÔNG phải kho
// IndexedDB — xem chú thích đầu file).
// ============================================================================================

const KHOA_THIET_BI_ID = 'qlgx.thietBiId'

/** Cache trong tiến trình — tránh sinh UUID mới mỗi lần gọi khi `localStorage` bị chặn (chế độ
 * riêng tư nghiêm ngặt): ít nhất trong CÙNG một phiên tải trang, mọi lời gọi vẫn nhận lại đúng MỘT
 * giá trị (xem chú thích đầu file vì sao dùng khác nhau ở hai chỗ là nguy hiểm). */
let thietBiIdDaCache: string | null = null

export function layThietBiId(): string {
  if (thietBiIdDaCache) return thietBiIdDaCache
  try {
    const daCo = localStorage.getItem(KHOA_THIET_BI_ID)
    if (daCo) {
      thietBiIdDaCache = daCo
      return daCo
    }
    const moi = crypto.randomUUID()
    localStorage.setItem(KHOA_THIET_BI_ID, moi)
    thietBiIdDaCache = moi
    return moi
  } catch {
    // localStorage bị chặn — vẫn phải trả về MỘT giá trị hợp lệ để đồng bộ chạy được, chỉ không
    // sống sót qua lần tải trang sau (xem chú thích `authStore.datToken` — cùng tình huống).
    thietBiIdDaCache = crypto.randomUUID()
    return thietBiIdDaCache
  }
}

// ============================================================================================
// Ghép Task 4 (đồng hồ máy con) vào một bộ phụ thuộc duy nhất, dùng ĐÚNG MỘT `thietBiId` cho cả
// `DongHoLogicMayCon` lẫn `GuiLenYeuCau.thietBiId` (xem chú thích đầu file).
// ============================================================================================

export type PhuThuocBoDongBo = {
  kho: IDBDatabase
  thietBiId: string
  donDieu: DongHoDonDieu
  doan: DoanDongHo
  dongHoLogic: DongHoLogicMayCon
}

export async function khoiTaoPhuThuoc(kho: IDBDatabase): Promise<PhuThuocBoDongBo> {
  const thietBiId = layThietBiId()
  const donDieu = await DongHoDonDieu.khoiTao(kho)
  const doan = await DoanDongHo.khoiTao(kho, donDieu)
  const dongHoLogic = await DongHoLogicMayCon.khoiTao(kho, thietBiId)
  return { kho, thietBiId, donDieu, doan, dongHoLogic }
}

// ============================================================================================
// Hình dạng một dòng hàng chờ MÀ BỘ ĐỒNG BỘ GỬI ĐƯỢC — `hangCho.ts` (Task 2) cố ý để các trường
// nghiệp vụ mở cho tầng gọi quyết định (xem chú thích đầu `DongHangCho`); Task 6 là tầng đó, chốt
// hình dạng khớp 1-1 với `ThaoTacDto` phía máy chủ (trừ `maThaoTac`/`doan` đã có sẵn).
// ============================================================================================

export type DongHangChoDongBo = DongHangCho & {
  loai: string
  bang: string
  banGhiId: string
  truong: string
  giaTri: string | null
  /** Dấu vật lý ĐÃ CẮT micro giây (`dungDauDongHo`/`DongHoLogicMayCon.phatDau` tự cắt) — ghi lúc
   * `ghiVaXepHang` được gọi (Task 7), KHÔNG phải giờ hệ thống thô. */
  vatLy: string
  logic: number
  giaoDichId: string
  /** Chỉ có giá trị với thao tác BÙ LẠI sau khi máy chủ bị khôi phục (Task 8, spec 4.8.5) —
   * `undefined`/`null` ở thao tác bình thường. */
  nguonGocEpoch?: string | null
  nguonGocSoThuTu?: number | null
}

/** RÀNG BUỘC 9: dùng lại ĐÚNG `maThaoTac` đã lưu trong hàng chờ khi dịch sang `ThaoTacDto` — TUYỆT
 * ĐỐI không sinh `maThaoTac` mới ở đây. Hàng chờ chỉ xoá dòng khi máy chủ đã xác nhận xử lý xong
 * (`apDungGuiLenKetQua`); một lần gửi thất bại (mất mạng giữa chừng) để dòng CÒN NGUYÊN trong hàng
 * chờ, và lần gửi lại kế tiếp phải mang lại đúng `maThaoTac` cũ để sổ chống trùng phía máy chủ
 * (`thao_tac_da_nhan`) nhận ra "đã thấy rồi" và trả `ketQua: 'trung'` thay vì tạo một dòng hiệu lực
 * thứ hai cho cùng một hành động của người dùng. */
function thanhThaoTacDto(dong: DongHangChoDongBo): ThaoTacDto {
  return {
    maThaoTac: dong.maThaoTac,
    giaoDichId: dong.giaoDichId,
    loai: dong.loai,
    bang: dong.bang,
    banGhiId: dong.banGhiId,
    truong: dong.truong,
    giaTri: dong.giaTri,
    dongHoVatLy: dong.vatLy,
    dongHoLogic: dong.logic,
    nguonGocEpoch: dong.nguonGocEpoch ?? null,
    nguonGocSoThuTu: dong.nguonGocSoThuTu ?? null,
  }
}

/** Dựng `DauDongHo` từ một `DongHieuLucDto` nhận về từ máy chủ — BẮT BUỘC dùng `chuanHoaMocMayChu`
 * (xem chú thích chi tiết trong `dauDongHo.ts`), KHÔNG gán thẳng `dongHoVatLy` gốc: `DateTimeOffset`
 * phía .NET serialize theo định dạng round-trip "O" có thể cắt chữ số 0 cuối hoặc mang hậu tố
 * `+00:00` thay vì `Z` — gán thẳng sẽ khiến hai chuỗi biểu diễn CÙNG một khoảnh khắc so ra KHÁC
 * NHAU, hỏng ÂM THẦM đúng thứ toàn bộ thiết kế đồng hồ lai sinh ra để chặn.
 *
 * `maThaoTac` của `DauDongHo` dùng tạm `giaoDichId` — `DongHieuLucDto` không mang một trường
 * "maThaoTac" lộ ra ngoài (đó là chi tiết chống trùng nội bộ phía máy chủ), và ở đây `maThaoTac` chỉ
 * cần là MỘT định danh duy nhất để phá hoà tầng cuối cùng khi vật lý+logic+thietBiId trùng hệt nhau
 * — `giaoDichId` (một Guid, đã duy nhất theo giao dịch) đáp ứng đủ vai trò đó. */
export function dauTuDongHieuLuc(d: DongHieuLucDto): DauDongHo {
  return { vatLy: chuanHoaMocMayChu(d.dongHoVatLy), logic: d.dongHoLogic, thietBiId: d.thietBiId, maThaoTac: d.giaoDichId }
}

function dongHieuLucThanhDaNhan(d: DongHieuLucDto, epoch: string, ngayNhan: string): DongDaNhan {
  return {
    epoch,
    soThuTu: d.soThuTu,
    bang: d.bang,
    banGhiId: d.banGhiId,
    truong: d.truong,
    giaTri: d.giaTri,
    dongHoVatLy: d.dongHoVatLy,
    dongHoLogic: d.dongHoLogic,
    thietBiId: d.thietBiId,
    giaoDichId: d.giaoDichId,
    ngayNhan,
  }
}

/**
 * Sau khi áp một lô dòng hiệu lực nhận về, "nâng" `DongHoLogicMayCon` của máy này theo dấu LỚN NHẤT
 * trong lô — để dấu KẾ TIẾP mà chính máy này phát ra (thao tác thật của người dùng) luôn xếp SAU mọi
 * thứ vừa nhận (Global Constraints: "đã thấy rồi mới sửa" phải xếp sau — xem `dongHoMayCon.ts`,
 * `DongHoLogicMayCon.phatDau`). Dùng `GUID_RONG` làm `maThaoTacMoi`: đây là một lần "quan sát" nội
 * bộ để cập nhật sổ sách đồng hồ, KHÔNG phải một thao tác thật của máy này sẽ được gửi lên máy chủ
 * (không có dòng hàng chờ nào tương ứng) — xem quyết định thiết kế này trong báo cáo Task 6.
 */
async function capNhatDongHoTheoDongNhanVe(phuThuoc: PhuThuocBoDongBo, dong: DongHieuLucDto[]): Promise<void> {
  if (dong.length === 0) return
  const dauLonNhat = dong.map(dauTuDongHieuLuc).reduce((a, b) => (soSanh(a, b) >= 0 ? a : b))
  await phuThuoc.dongHoLogic.phatDau(phuThuoc.donDieu.mocHienTaiMs(), dauLonNhat, GUID_RONG)
}

/**
 * RÀNG BUỘC 3: áp dòng nhận về (sổ đã nhận + con trỏ) trong MỘT giao dịch — dùng cho cả nhánh
 * `/thay-doi` (không đụng hàng chờ) lẫn nhánh `/gui-len` (đụng cả hàng chờ + cần xem lại, xem
 * `apDungGuiLenKetQua` bên dưới bọc thêm quanh hàm lõi này... không, hai nhánh có SỐ KHO KHÁC NHAU
 * trong transaction nên KHÔNG dùng chung một hàm mở transaction — viết riêng, xem bên dưới).
 */
async function apDungNhanVe(phuThuoc: PhuThuocBoDongBo, ketQua: NhanVeKetQua): Promise<void> {
  const ngayNhan = tuMocMs(phuThuoc.donDieu.mocHienTaiMs())
  const soDaNhanMoi = ketQua.dong.map((d) => dongHieuLucThanhDaNhan(d, ketQua.epoch, ngayNhan))

  await new Promise<void>((resolve, reject) => {
    const gd = phuThuoc.kho.transaction([KHO_SO_DA_NHAN, KHO_CON_TRO], 'readwrite')
    ghiSoDaNhanTrongGiaoDich(gd.objectStore(KHO_SO_DA_NHAN), soDaNhanMoi, () => {})
    ghiConTroTrongGiaoDich(gd.objectStore(KHO_CON_TRO), { epoch: ketQua.epoch, soThuTu: ketQua.conTroMoi }, () => {})
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không áp được dữ liệu thay đổi'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch áp dữ liệu thay đổi bị huỷ giữa chừng'))
  })

  await capNhatDongHoTheoDongNhanVe(phuThuoc, ketQua.dong)
}

/**
 * Gọi `/thay-doi` một lần và áp kết quả — dùng cho lần gọi "đúng chỗ cần" (spec 7.3: ngay trước khi
 * mở một hồ sơ ra sửa, và trước khi dò trùng lúc tạo người mới), TÁCH KHỎI vòng lặp định kỳ (vòng
 * lặp định kỳ dùng `guiHangChoVaApDung` — LUÔN kèm "hỏi dữ liệu mới" miễn phí qua `dongMoi`, không
 * cần gọi `/thay-doi` riêng, xem chú thích đầu file mục 1).
 */
export async function layThayDoiVaApDung(phuThuoc: PhuThuocBoDongBo, toiDa?: number): Promise<NhanVeKetQua> {
  const conTro = await docConTro(phuThuoc.kho)
  const ketQua = await layThayDoi(conTro.epoch, conTro.soThuTu, toiDa)
  await apDungNhanVe(phuThuoc, ketQua)
  return ketQua
}

/**
 * RÀNG BUỘC 3 — áp kết quả một lô gửi lên: sổ đã nhận + con trỏ + cần-xem-lại (kết quả `tu_choi`) +
 * xoá hàng chờ, TRONG MỘT `kho.transaction([...], 'readwrite')` DUY NHẤT. Xem chú thích đầu file.
 *
 * Mọi `KetQuaThaoTacDto` (`ap`/`thua`/`tu_choi`/`trung`) đều khiến dòng hàng chờ tương ứng bị xoá —
 * máy chủ đã XỬ LÝ XONG thao tác đó theo MỘT trong bốn cách, không còn gì để gửi lại: `ap` (đã ghi),
 * `thua` (luật gộp mức trường quyết định có cái mới hơn — kết quả gộp bình thường, KHÔNG phải lỗi,
 * không vào hộp cần xem lại), `trung` (đã nhận ở một lô trước — xác nhận lại việc RÀNG BUỘC 9 đã
 * làm đúng), và `tu_choi` (bị từ chối — xoá khỏi hàng chờ nhưng PHẢI ghi vào cần-xem-lại TRƯỚC/CÙNG
 * lúc xoá, không thì dữ liệu người dùng đã gõ biến mất không dấu vết, xem `task-6-brief.md`).
 */
async function apDungGuiLenKetQua(
  phuThuoc: PhuThuocBoDongBo,
  hangChoDaGui: DongHangChoDongBo[],
  ketQua: GuiLenKetQua,
): Promise<void> {
  const theoMa = new Map(hangChoDaGui.map((d) => [d.maThaoTac, d]))
  const ngayNhan = tuMocMs(phuThuoc.donDieu.mocHienTaiMs())

  const canXoaKhoiHangCho: string[] = []
  const canXemLaiMoi: DongCanXemLai[] = []

  for (const kq of ketQua.ketQua) {
    canXoaKhoiHangCho.push(kq.maThaoTac)
    if (kq.ketQua === 'tu_choi') {
      const dongGoc = theoMa.get(kq.maThaoTac)
      // `dongGoc` luôn phải có mặt trong luồng bình thường (máy chủ chỉ trả kết quả cho thao tác
      // ta vừa gửi) — bỏ qua nếu thiếu (phòng hờ dữ liệu bất thường) thay vì ném lỗi làm hỏng cả
      // giao dịch của MỌI kết quả khác trong cùng lô.
      if (dongGoc) {
        canXemLaiMoi.push({ maThaoTac: kq.maThaoTac, thongBao: kq.thongBao, dongHangChoGoc: dongGoc, taoLuc: ngayNhan })
      }
    }
  }

  const soDaNhanMoi = ketQua.dongMoi.map((d) => dongHieuLucThanhDaNhan(d, ketQua.epoch, ngayNhan))

  await new Promise<void>((resolve, reject) => {
    const gd = phuThuoc.kho.transaction([KHO_SO_DA_NHAN, KHO_CON_TRO, KHO_HANG_CHO, KHO_CAN_XEM_LAI], 'readwrite')
    ghiSoDaNhanTrongGiaoDich(gd.objectStore(KHO_SO_DA_NHAN), soDaNhanMoi, () => {})
    ghiConTroTrongGiaoDich(gd.objectStore(KHO_CON_TRO), { epoch: ketQua.epoch, soThuTu: ketQua.conTroMoi }, () => {})
    ghiCanXemLaiTrongGiaoDich(gd.objectStore(KHO_CAN_XEM_LAI), canXemLaiMoi, () => {})
    xoaKhoiHangChoTrongGiaoDich(gd.objectStore(KHO_HANG_CHO), canXoaKhoiHangCho, () => {})
    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không áp được kết quả gửi lên'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch áp kết quả gửi lên bị huỷ giữa chừng'))
  })

  await capNhatDongHoTheoDongNhanVe(phuThuoc, ketQua.dongMoi)
}

/** Gửi TOÀN BỘ hàng chờ hiện có (kể cả rỗng — xem chú thích đầu file mục 1: một lô rỗng vẫn kéo về
 * `dongMoi` "miễn phí") lên `/gui-len`, rồi áp kết quả (Ràng buộc 3). Trả lại `conNua` để tầng gọi
 * biết có cần hỏi tiếp ngay (không đợi hết chu kỳ) hay không. */
export async function guiHangChoVaApDung(
  phuThuoc: PhuThuocBoDongBo,
  hangCho: DongHangChoDongBo[],
): Promise<{ conNua: boolean }> {
  const conTro = await docConTro(phuThuoc.kho)
  const yc: GuiLenYeuCau = {
    thietBiId: phuThuoc.thietBiId,
    gioMayCon: tuMocMs(phuThuoc.donDieu.mocHienTaiMs()),
    epoch: conTro.epoch,
    conTro: conTro.soThuTu,
    thaoTac: hangCho.map(thanhThaoTacDto),
  }

  const ketQua = await guiLen(yc)
  await apDungGuiLenKetQua(phuThuoc, hangCho, ketQua)
  return { conNua: ketQua.conNua }
}

/**
 * Một chu kỳ đồng bộ đầy đủ: cập nhật phát hiện nhảy giờ (`DoanDongHo.soDoanHienTai`, xem
 * `dongHoMayCon.ts` — phải gọi trước khi gắn số đoạn cho các dòng SẼ được xếp hàng sau đó, dù ở
 * ĐÂY ta chỉ ĐỌC hàng chờ hiện có, không tự ghi thêm dòng nào), rồi gửi hàng chờ.
 *
 * `tabAn = true` VÀ hàng chờ rỗng: bỏ qua hẳn lượt gọi mạng — đây chính là "tab ẩn thì ngừng hỏi dữ
 * liệu mới" (không có gì để gửi thì gọi `/gui-len` rỗng chỉ để "hỏi dữ liệu mới" một cách trá hình,
 * không cần thiết khi không ai nhìn màn hình). `tabAn = true` NHƯNG hàng chờ CÓ gì: vẫn gửi bình
 * thường — "KHÔNG BAO GIỜ ngừng gửi hàng chờ" (spec 7.3).
 */
export async function motLanDongBo(phuThuoc: PhuThuocBoDongBo, tabAn: boolean): Promise<{ conNua: boolean }> {
  await phuThuoc.doan.soDoanHienTai()
  const hangCho = (await docHangCho(phuThuoc.kho)) as DongHangChoDongBo[]
  if (tabAn && hangCho.length === 0) return { conNua: false }
  return guiHangChoVaApDung(phuThuoc, hangCho)
}

// ============================================================================================
// Vòng lặp lịch chạy — bốn nguồn kích hoạt (xem chú thích đầu file) + bầu chủ một tab (Task 5).
// ============================================================================================

/** Chu kỳ chờ khi vừa có `baoDangGo()` (đang gõ) — 1 phút, đúng spec 7.3. */
export const CHU_KY_DANG_GO_MS = 60_000
/** Chu kỳ chờ mặc định khi ngồi yên vài phút — 5 phút, đúng spec 7.3. */
export const CHU_KY_YEN_TINH_MS = 5 * 60_000

/** Trừu tượng hoá các nguồn sự kiện trình duyệt mà lịch chạy cần — TIÊM ĐƯỢC (cùng khuôn mẫu
 * `NguonThoiGian`/`NguonKhoa`) để kiểm thử được TẤT ĐỊNH, không phụ thuộc `document`/`window` thật
 * (dù jsdom có polyfill, việc tiêm vẫn cho phép giả lập "tab ẩn"/"mất mạng" mà không phải đụng vào
 * `Object.defineProperty(document, 'hidden', ...)` trong từng ca kiểm thử). */
export type NguonSuKienDongBo = {
  /** `true` nghĩa là tab đang ẩn (`document.hidden`) — spec 7.3 mục 4. */
  tabDangAn(): boolean
  /** Đăng ký `danhThuc` cho cả ba sự kiện (`visibilitychange` khi tab HIỆN LẠI, `focus`, `online`)
   * — trả về một hàm HUỶ đăng ký cả ba, gọi khi vòng lặp dừng hoặc mất vai trò chủ. */
  dangKy(danhThuc: () => void): () => void
}

const NGUON_SU_KIEN_MAC_DINH: NguonSuKienDongBo = {
  tabDangAn: () => typeof document !== 'undefined' && document.hidden,
  dangKy(danhThuc) {
    if (typeof document === 'undefined' || typeof window === 'undefined') return () => {}
    const khiHienLai = () => {
      if (!document.hidden) danhThuc()
    }
    document.addEventListener('visibilitychange', khiHienLai)
    window.addEventListener('focus', danhThuc)
    window.addEventListener('online', danhThuc)
    return () => {
      document.removeEventListener('visibilitychange', khiHienLai)
      window.removeEventListener('focus', danhThuc)
      window.removeEventListener('online', danhThuc)
    }
  },
}

export type TrangThaiBoDongBo = 'dang_chay' | 'dung_do_may_chu_di_lui'

export type DieuKhienBoDongBo = {
  /** Dừng hẳn vòng đồng bộ (huỷ `AbortSignal` truyền cho `troThanhChuKhiCoTheChoDenKhiHuy`) — gọi
   * khi tab đóng/unload. */
  dung(): void
  /** Báo "người dùng vừa gõ gì đó" — rút ngắn chu kỳ chờ TIẾP THEO xuống `CHU_KY_DANG_GO_MS` VÀ
   * đánh thức ngay vòng lặp nếu đang chờ (spec 7.3 mục 4). */
  baoDangGo(): void
  /**
   * Đánh thức vòng lặp ngay lập tức nếu đang chờ — dùng cho các lần gọi "đúng chỗ cần" (trước khi
   * mở hồ sơ/dò trùng). GIỚI HẠN PHẠM VI CỐ Ý (xem báo cáo Task 6): hàm này CHỈ đánh thức, KHÔNG
   * đợi chu kỳ đó chạy xong — chỉ có tác dụng khi CHÍNH TAB NÀY đang là chủ (Task 7 cần cầu nối
   * đầy đủ hơn — ví dụ đợi qua `BroadcastChannel` khi tab khác mới là chủ — nằm ngoài phạm vi việc
   * ghép vòng lặp cốt lõi của Task 6).
   */
  danhThucNgay(): void
  /** `'dung_do_may_chu_di_lui'` sau khi gặp `LoiMayChuDiLui` — Task 10 đọc để chuyển 🔴. */
  trangThai(): TrangThaiBoDongBo
}

/** Trừu tượng hoá `setTimeout`/`clearTimeout` — TIÊM ĐƯỢC (cùng khuôn mẫu `NguonThoiGian`/
 * `NguonKhoa`) để kiểm thử lịch chạy được TẤT ĐỊNH mà KHÔNG cần `vi.useFakeTimers()`: các thao tác
 * kho (`fake-indexeddb`) dùng lại cơ chế hàng đợi sự kiện thật của môi trường chạy, và bật fake
 * timers toàn cục sẽ đứng luôn cả các `await` chờ IndexedDB (đã kiểm chứng — xem báo cáo Task 6),
 * không riêng gì vòng chờ định kỳ của module này. */
export type NguonHenGio = {
  dat(ms: number, fn: () => void): unknown
  huy(id: unknown): void
}

const NGUON_HEN_GIO_MAC_DINH: NguonHenGio = {
  dat: (ms, fn) => setTimeout(fn, ms),
  huy: (id) => clearTimeout(id as ReturnType<typeof setTimeout>),
}

function moiThoiGian(ms: number, tinHieuHuy: AbortSignal, henGio: NguonHenGio): Promise<void> {
  return new Promise((resolve) => {
    const id = henGio.dat(ms, resolve)
    tinHieuHuy.addEventListener(
      'abort',
      () => {
        henGio.huy(id)
        resolve()
      },
      { once: true },
    )
  })
}

/**
 * Khởi động vòng đồng bộ vô thời hạn, CHỈ CHẠY khi tab này giành được vai trò chủ (Task 5 —
 * `troThanhChuKhiCoTheChoDenKhiHuy`). Theo đúng cách (a) mà `bauChu.ts` mô tả: `khiLaChu` TỰ là một
 * vòng lặp không resolve cho tới khi thấy `tinHieuHuy.aborted` — vì vậy KHÔNG cần tự tranh lại khoá
 * từ `khiMatChu` (cách (b)): một khi mất chủ, nghĩa là `tinHieuHuy` đã abort (do `dung()` gọi) hoặc
 * gặp `LoiMayChuDiLui` (cố ý `return` sớm để NHẢ khoá, dừng hẳn — không tự tranh lại).
 */
export function batDauBoDongBo(
  kho: IDBDatabase,
  nguonSuKien: NguonSuKienDongBo = NGUON_SU_KIEN_MAC_DINH,
  nguonHenGio: NguonHenGio = NGUON_HEN_GIO_MAC_DINH,
  // Chỉ dùng để kiểm thử: `navigator.locks` không có sẵn trong jsdom (cần secure context —
  // https/localhost). Không truyền gì thì dùng đúng mặc định của `bauChu.ts` (Web Locks API thật).
  nguonKhoa?: NguonKhoa,
): DieuKhienBoDongBo {
  const dieuKhienHuy = new AbortController()
  let trangThaiHienTai: TrangThaiBoDongBo = 'dang_chay'
  let danhThucVongHienTai: (() => void) | null = null
  let thoiHanKeTiepMs = CHU_KY_YEN_TINH_MS

  async function khiLaChu(tinHieuHuy: AbortSignal): Promise<void> {
    const phuThuoc = await khoiTaoPhuThuoc(kho)
    const huyDangKy = nguonSuKien.dangKy(() => danhThucVongHienTai?.())

    try {
      while (!tinHieuHuy.aborted) {
        const tabAn = nguonSuKien.tabDangAn()
        try {
          await motLanDongBo(phuThuoc, tabAn)
        } catch (loi) {
          if (loi instanceof LoiMayChuDiLui) {
            // DỪNG HẲN — không tự thử lại (spec 4.8.4: đây là dấu hiệu sự cố nghiêm trọng phía máy
            // chủ, không phải lỗi mạng vặt). `return` để thoát vòng lặp, khiến `khiLaChu` resolve
            // bình thường và nhả khoá chủ cho tab khác (tab đó sẽ gặp lại đúng lỗi này và cũng tự
            // dừng — hành vi đúng cho tới khi Task 10/quản trị viên can thiệp).
            trangThaiHienTai = 'dung_do_may_chu_di_lui'
            console.error('Dong bo: may chu co dau hieu vua bi dua ve ban cu — DA DUNG dong bo', loi)
            return
          }
          // LoiEpochKhongKhop hoặc lỗi mạng thường: ghi log rồi thử lại ở chu kỳ kế tiếp — Task 6
          // KHÔNG tự động tải lại /toan-bo khi gặp LoiEpochKhongKhop (xem giới hạn phạm vi trong
          // báo cáo: điểm nối `taiToanBoVaGiaiNen` đã sẵn cho Task 8 dùng).
          console.error('Dong bo: loi trong mot chu ky, se thu lai o chu ky sau', loi)
        }

        if (tinHieuHuy.aborted) break

        const doiMs = thoiHanKeTiepMs
        thoiHanKeTiepMs = CHU_KY_YEN_TINH_MS // tự giãn về yên tĩnh nếu không có baoDangGo() nào thêm
        await Promise.race([
          new Promise<void>((res) => {
            danhThucVongHienTai = res
          }),
          moiThoiGian(doiMs, tinHieuHuy, nguonHenGio),
        ])
        danhThucVongHienTai = null
      }
    } finally {
      huyDangKy()
    }
  }

  troThanhChuKhiCoTheChoDenKhiHuy(khiLaChu, () => {}, dieuKhienHuy.signal, TEN_KHOA_BAU_CHU, nguonKhoa)

  return {
    dung: () => dieuKhienHuy.abort(),
    baoDangGo: () => {
      thoiHanKeTiepMs = CHU_KY_DANG_GO_MS
      danhThucVongHienTai?.()
    },
    danhThucNgay: () => danhThucVongHienTai?.(),
    trangThai: () => trangThaiHienTai,
  }
}

// ============================================================================================
// /toan-bo — GIỚI HẠN PHẠM VI CỐ Ý (được brief cho phép tường minh, xem báo cáo Task 6): hàm này
// CHỈ gọi endpoint, giải nén gzip+base64 bằng DecompressionStream('gzip') gốc trình duyệt, và trả
// về dữ liệu ĐÃ PARSE — việc ghi CỤ THỂ từng bảng vào kho `banGhi` (Task 1) CHƯA làm ở đây, để dành
// thiết kế riêng (khối lượng ghi lớn, cần quyết định khoá/định dạng cho từng bảng nghiệp vụ, vượt
// quá phạm vi "ghép vòng chạy đồng bộ" của Task 6). Điểm nối rõ ràng: một hàm `apDungToanBo(kho,
// ketQua)` (CHƯA VIẾT) sẽ nhận đúng object `{ epoch, conTro, chupLuc, duLieu }` mà hàm này trả về.
// ============================================================================================

export type ToanBoDaGiaiNen = { epoch: string; conTro: number; chupLuc: string; duLieu: Record<string, unknown[]> }

export async function taiToanBoVaGiaiNen(): Promise<ToanBoDaGiaiNen> {
  const ketQua = await layToanBo()
  const nhiPhan = Uint8Array.from(atob(ketQua.duLieuNen), (c) => c.charCodeAt(0))
  const luongDaGiaiNen = new Blob([nhiPhan]).stream().pipeThrough(new DecompressionStream('gzip'))
  const van = await new Response(luongDaGiaiNen).text()
  const duLieu = JSON.parse(van) as Record<string, unknown[]>
  return { epoch: ketQua.epoch, conTro: ketQua.conTro, chupLuc: ketQua.chupLuc, duLieu }
}

// Xuất lại type NguonKhoa để tránh tầng gọi (Task 7/10) phải import trực tiếp từ bauChu.ts khi chỉ
// cần khai báo kiểu tham số cho một bản giả lập dùng trong kiểm thử tích hợp cao hơn.
export type { NguonKhoa }
