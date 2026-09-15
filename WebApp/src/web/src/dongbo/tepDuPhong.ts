/**
 * File dự phòng hàng chờ — Task 9, spec 7.10. Hàng chờ ("việc chưa gửi", kho `hangCho` — xem
 * `kho/hangCho.ts`) nằm HOÀN TOÀN trong trình duyệt, và trình duyệt mất dữ liệu vì những lý do
 * không ai lường trước: hết dung lượng đĩa thì trình duyệt tự dọn; "cháu biết máy tính" chạy
 * CCleaner; cài lại Windows; đổi máy; Safari/iOS xoá dữ liệu website sau 7 ngày không dùng nếu
 * PWA chưa cài vào màn hình chính. Khi hàng chờ tồn đọng quá lâu/quá nhiều, tự tải một file JSON
 * nhỏ xuống thư mục Tải về của máy — mất máy thì gửi file đó cho người hỗ trợ là khôi phục lại
 * được các việc chưa gửi (nút "Nạp lại file dự phòng" thuộc Task 10, dùng `napLaiFileDuPhong` ở
 * đây).
 *
 * ## Quyết định "taoLuc cũ nhất" — CHỌN HƯỚNG (a) (xem task-9-report.md để có đầy đủ lý do)
 *
 * `DongHangCho` (Task 2) không có trường "thời điểm dòng này được thêm vào hàng chờ" tường minh
 * — chỉ có `vatLy` (mốc đồng hồ lai của `DongHangChoDongBo`, Task 6/7 — KHÔNG phải giờ hệ thống
 * thô). Hướng (a): đọc thẳng `vatLy` nếu có (trường này nằm trong phần `Record<string, unknown>`
 * mở của `DongHangCho`, không phải MỌI dòng đều có — ví dụ dữ liệu test dựng tay không gắn, hoặc
 * một hàng chờ giả lập không qua Task 6/7) để suy ra "taoLuc cũ nhất", so trực tiếp với
 * `Date.now()`. Chấp nhận sai số nhỏ khi máy đang lệch giờ/nhảy giờ (biên hiếm, hậu quả NHẸ: tự
 * lưu dự phòng sớm/muộn hơn 2 ngày một chút — không phải mất dữ liệu) — đổi lại KHÔNG phải sửa
 * hình dạng `DongHangCho` đã đóng ở Task 2, và không phải thêm một trường mốc riêng vào `conTro`
 * do tầng gọi khác quản (hướng (b) trong brief) chỉ để phục vụ MỘT ngưỡng cảnh báo phụ (không
 * phải logic đồng bộ cốt lõi) — tránh ripple effect không cần thiết sang Task 4/6/7/10.
 *
 * Dòng KHÔNG có `vatLy` hợp lệ bị bỏ qua khi tìm "cũ nhất" (không làm điều kiện "> 2 ngày" tự
 * kích hoạt sai vì thiếu dữ liệu) — điều kiện "> 20 việc" vẫn hoạt động độc lập, không phụ thuộc
 * `vatLy`.
 */
import { KHO_HANG_CHO } from '../kho/moKho'
import { voiKhoaKeTiep, type DongHangCho } from '../kho/hangCho'

/** Nội dung file dự phòng — bọc thêm `phienBanFile`/`taoLuc` ngoài mảng hàng chờ thô, để sau này
 * còn đường mở rộng định dạng (ví dụ thêm trường) mà không phá các file dự phòng CŨ đã tải xuống
 * máy người dùng từ trước — rẻ nên thêm ngay từ đầu, dù Task 9 chưa dùng `phienBanFile` để làm gì.
 */
type NoiDungFileDuPhong = {
  phienBanFile: 1
  taoLuc: string
  hangCho: DongHangCho[]
}

const PHIEN_BAN_FILE_HIEN_TAI = 1

function dungNoiDungFileDuPhong(hangCho: DongHangCho[]): NoiDungFileDuPhong {
  return { phienBanFile: PHIEN_BAN_FILE_HIEN_TAI, taoLuc: new Date().toISOString(), hangCho }
}

/** Nội dung file dự phòng — mảng thô các dòng hàng chờ, TỰ THÂN đủ để gửi lại (không cần bản ghi
 * hiển thị đi kèm — `DongHangCho` không tham chiếu gì tới `banGhi`, xem `hangCho.ts`). */
export function taoNoiDungFileDuPhong(hangCho: DongHangCho[]): string {
  return JSON.stringify(dungNoiDungFileDuPhong(hangCho))
}

/** Windows (NTFS) không cho `:` trong tên file, và Explorer/trình duyệt hay tải trùng tên rồi tự
 * thêm hậu tố "(1)" — thay `:`/`.` của ISO timestamp bằng `-` cho tên file gọn, không dấu lạ, vẫn
 * đủ để sắp xếp theo thời gian trong thư mục Tải về. */
function tenFileDuPhong(taoLuc: string): string {
  return `qlgx-du-phong-${taoLuc.replace(/[:.]/g, '-')}.json`
}

/** Kích trình duyệt tải file dự phòng xuống thư mục Tải về — Blob + `URL.createObjectURL` + click
 * một thẻ `<a>` ẩn, cùng khuôn mẫu `taiXuongCsv` (`lib/csv.ts`) đã dùng cho "Xuất dữ liệu (CSV)".
 * KHÔNG kéo thư viện ngoài (`file-saver`...) — trình duyệt đã có sẵn đủ, cùng tinh thần
 * "không thêm dependency khi trình duyệt đã có sẵn" như `DecompressionStream` ở Task 6
 * (`boDongBo.ts`). */
export function taiFileDuPhongXuong(hangCho: DongHangCho[]): void {
  const noiDung = dungNoiDungFileDuPhong(hangCho)
  const blob = new Blob([JSON.stringify(noiDung)], { type: 'application/json;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  try {
    const a = document.createElement('a')
    a.href = url
    a.download = tenFileDuPhong(noiDung.taoLuc)
    document.body.appendChild(a)
    a.click()
    a.remove()
  } finally {
    URL.revokeObjectURL(url)
  }
}

/** Đọc lại nội dung một file dự phòng (chuỗi JSON) — ném lỗi rõ ràng nếu không đúng định dạng
 * (fail-loud, đừng âm thầm trả mảng rỗng khi người dùng lỡ chọn nhầm file khác — im lặng trả rỗng
 * ở đây trông giống hệt "nạp thành công, không có gì để nạp", khiến người dùng tưởng đã khôi phục
 * xong trong khi thực ra chưa nạp được gì). Chỉ kiểm tra hai trường LÕI bắt buộc của `DongHangCho`
 * (`maThaoTac`: string, `doan`: number) — không kiểm tra các trường nghiệp vụ khác (`loai`, `bang`,
 * `giaTri`...) vì `DongHangCho` cố ý để chúng mở cho tầng gọi (xem `hangCho.ts`), file cũ hơn có
 * thể thiếu trường mà một bản sau này mới thêm. */
export function docNoiDungFileDuPhong(vanBan: string): DongHangCho[] {
  let obj: unknown
  try {
    obj = JSON.parse(vanBan)
  } catch {
    throw new Error('File dự phòng không phải là JSON hợp lệ — có thể đã chọn nhầm file khác.')
  }

  if (typeof obj !== 'object' || obj === null || !('hangCho' in obj)) {
    throw new Error('File dự phòng sai định dạng: không tìm thấy trường "hangCho".')
  }

  const hangCho: unknown = (obj as { hangCho: unknown }).hangCho
  if (!Array.isArray(hangCho)) {
    throw new Error('File dự phòng sai định dạng: trường "hangCho" phải là một mảng.')
  }

  hangCho.forEach((dong: unknown, i) => {
    if (typeof dong !== 'object' || dong === null) {
      throw new Error(`File dự phòng sai định dạng: dòng thứ ${i + 1} không phải một object.`)
    }
    const d = dong as Record<string, unknown>
    if (typeof d.maThaoTac !== 'string' || d.maThaoTac === '') {
      throw new Error(`File dự phòng sai định dạng: dòng thứ ${i + 1} thiếu "maThaoTac" hợp lệ.`)
    }
    if (typeof d.doan !== 'number') {
      throw new Error(`File dự phòng sai định dạng: dòng thứ ${i + 1} thiếu "doan" hợp lệ.`)
    }
  })

  return hangCho as DongHangCho[]
}

/** Nạp các dòng đọc được từ file vào hàng chờ hiện có. KHÔNG cần tự chống trùng cục bộ ở đây —
 * "nạp lại không tạo bản trùng" đến từ việc `maThaoTac` được GIỮ NGUYÊN (đọc thẳng từ file, không
 * sinh mới ở `docNoiDungFileDuPhong`) và máy chủ tự nhận ra qua `thao_tac_da_nhan` (trả `"trung"`,
 * Task 6 tự xoá khỏi hàng chờ) NẾU dòng đó đã từng được gửi thành công trước khi mất dữ liệu — kể
 * cả khi lỡ nạp file vào một hàng chờ ĐANG CÒN giữ đúng những dòng đó (không mất dữ liệu, cùng lắm
 * gửi trùng một lần vô hại).
 *
 * Xin khoá kế tiếp ĐÚNG MỘT LẦN (`voiKhoaKeTiep`) rồi `put` từng dòng với khoá tăng dần thủ công
 * (`khoaBatDau + i`) — KHÔNG gọi `voiKhoaKeTiep` một lần cho MỖI dòng trong vòng lặp: mỗi lần gọi
 * mở một con trỏ `openCursor` mới, và vì các yêu cầu trong CÙNG một giao dịch được xếp hàng theo
 * đúng thứ tự tạo ra (không đợi yêu cầu trước xong mới tạo yêu cầu sau), gọi lặp lại sẽ khiến MỌI
 * lần đều đọc thấy cùng một "khoá lớn nhất hiện có" (con trỏ thứ hai mở trước khi `put` của dòng
 * đầu kịp chạy) — mọi dòng nhận trùng MỘT khoá, dòng sau đè dòng trước, mất dữ liệu ngay trong lúc
 * đang cố khôi phục dữ liệu. */
export function napLaiFileDuPhong(kho: IDBDatabase, hangChoTuFile: DongHangCho[]): Promise<void> {
  return new Promise((resolve, reject) => {
    if (hangChoTuFile.length === 0) {
      resolve()
      return
    }

    const gd = kho.transaction(KHO_HANG_CHO, 'readwrite')
    const store = gd.objectStore(KHO_HANG_CHO)

    voiKhoaKeTiep(
      store,
      (khoaBatDau) => {
        hangChoTuFile.forEach((dong, i) => store.put(dong, khoaBatDau + i))
      },
      reject,
    )

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không nạp được file dự phòng vào hàng chờ'))
    gd.onabort = () => reject(gd.error ?? new Error('Giao dịch nạp file dự phòng bị huỷ giữa chừng'))
  })
}

/** Ngưỡng "tồn đọng" — spec 7.10: quá 2 ngày HOẶC quá 20 việc. */
const HAI_NGAY_MS = 2 * 24 * 60 * 60 * 1000
const NGUONG_SO_LUONG_TON_DONG = 20

function vatLyCuaDong(dong: DongHangCho): string | null {
  const v = (dong as Record<string, unknown>).vatLy
  return typeof v === 'string' ? v : null
}

/** `vatLy` cũ nhất (nhỏ nhất) trong số các dòng CÓ `vatLy` hợp lệ — `null` nếu không dòng nào có
 * (xem chú thích đầu file: khi đó điều kiện "> 2 ngày" không tự kích hoạt, chỉ "> 20 việc" còn
 * hoạt động). */
function vatLyCuNhatMs(hangCho: DongHangCho[]): number | null {
  let cuNhat: number | null = null
  for (const dong of hangCho) {
    const vatLy = vatLyCuaDong(dong)
    if (vatLy === null) continue
    const ms = Date.parse(vatLy)
    if (Number.isNaN(ms)) continue
    if (cuNhat === null || ms < cuNhat) cuNhat = ms
  }
  return cuNhat
}

/** Quyết định có cần tự tải file dự phòng lúc này không: hàng chờ tồn đọng > 2 NGÀY (so `vatLy`
 * cũ nhất trong hàng chờ hiện có với giờ hiện tại — xem quyết định hướng (a) đầu file) HOẶC > 20
 * việc.
 *
 * `lanDuPhongGanNhatIso`: mốc lần tự-dự-phòng GẦN NHẤT (`null` nếu chưa từng tự dự phòng lần nào)
 * — dùng làm "hãm" chống spam: một khi đã tải một file dự phòng, hàm này sẽ không đòi tải NGAY một
 * file khác trong ít nhất 2 ngày kế tiếp, dù điều kiện tồn đọng vẫn còn đúng. Không có hãm này, một
 * máy offline kéo dài (hàng chờ tồn đọng SUỐT, tầng gọi — Task 10 — hỏi hàm này định kỳ mỗi vài
 * phút) sẽ khiến người dùng nhận HÀNG CHỤC file dự phòng giống hệt nhau chỉ trong một buổi, làm rối
 * thư mục Tải về thay vì giúp ích. Mốc "hãm" so với GIỜ HIỆN TẠI (không so với `vatLy` cũ nhất) vì
 * mục đích của nó là hạn chế TẦN SUẤT tải file, không phải xác định "đã tồn đọng hay chưa" (việc đó
 * đã do `quaSoLuong`/`quaThoiGian` đảm nhiệm) — một chuỗi ISO không hợp lệ (`Date.parse` ra `NaN`)
 * bị coi như "chưa từng dự phòng" (an toàn hơn — thà tải thêm một file thừa còn hơn im lặng bỏ qua
 * một lần cần dự phòng thật vì dữ liệu mốc hỏng).
 *
 * **`msTuLanTruoc >= 0` là điều kiện BẮT BUỘC, không phải phòng thủ thừa** (phát hiện qua review):
 * nếu đồng hồ hệ thống bị LÙI sau lần tự dự phòng gần nhất (đặt sai ngày, RTC hỏng, cài lại
 * Windows...), `Date.now() - Date.parse(lanDuPhongGanNhatIso)` ra một số ÂM — và số âm LUÔN nhỏ hơn
 * `HAI_NGAY_MS`, khiến nhánh hãm coi như "còn trong 2 ngày kể từ lần dự phòng trước" MÃI MÃI, chặn
 * vĩnh viễn CẢ điều kiện `quaSoLuong` (vốn không hề phụ thuộc đồng hồ) — đúng lớp an toàn sinh ra
 * cho tình huống "máy có chuyện gì" lại bị chính một chuyện-gì-đó-với-đồng-hồ vô hiệu hoá âm thầm.
 * Coi một delta ÂM là "hãm đã hết hạn từ lâu" (không phải "còn hãm") — nhất quán với cách hàm đã xử
 * lý chuỗi ISO không hợp lệ: thà tải thêm một file thừa còn hơn im lặng bỏ sót. */
export function canTuDongDuPhong(hangCho: DongHangCho[], lanDuPhongGanNhatIso: string | null): boolean {
  if (hangCho.length === 0) return false

  const quaSoLuong = hangCho.length > NGUONG_SO_LUONG_TON_DONG
  const cuNhatMs = vatLyCuNhatMs(hangCho)
  const quaThoiGian = cuNhatMs !== null && Date.now() - cuNhatMs > HAI_NGAY_MS

  if (!quaSoLuong && !quaThoiGian) return false

  if (lanDuPhongGanNhatIso !== null) {
    const msTuLanTruoc = Date.now() - Date.parse(lanDuPhongGanNhatIso)
    if (!Number.isNaN(msTuLanTruoc) && msTuLanTruoc >= 0 && msTuLanTruoc < HAI_NGAY_MS) return false
  }

  return true
}
