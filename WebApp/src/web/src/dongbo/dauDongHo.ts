/**
 * Port TypeScript của `WebApp/src/Qlgx.Data/DongBo/DongHoLai.cs` — "Hợp đồng đồng hồ lai" (Global
 * Constraints). ĐÂY LÀ HỢP ĐỒNG PHẢI KHỚP ĐÚNG HÀNH VI với bản C#, không phải "viết lại cho giống
 * cú pháp" — mọi máy (máy chủ .NET, máy con TypeScript) phải xếp thứ tự các thao tác GIỐNG HỆT
 * nhau, nếu không hai bản sao phân kỳ vĩnh viễn mà không một lỗi nào hiện ra (xem chú thích trong
 * bản C# gốc).
 *
 * ## Quyết định biểu diễn mốc thời gian: CHUỖI ISO-8601 UTC, ĐỘ DÀI CỐ ĐỊNH — không ép về `number`
 *
 * `Date.getTime()` của JavaScript chỉ có độ phân giải MILI GIÂY. Máy chủ (.NET, `timestamptz` của
 * Postgres) giữ tới MICRO GIÂY. Khi máy chủ gửi một mốc xuống qua JSON, nó gửi một chuỗi ISO có đủ
 * 6 chữ số phần thập phân (micro giây) — nếu máy con ép ngay về `number` mili giây (`Date.parse`),
 * PHẦN MICRO GIÂY BỊ CẮT BỎ VĨNH VIỄN. Hai mốc chỉ khác nhau ở phần micro giây (ví dụ hai lần ghi
 * liên tiếp của chính máy chủ, cách nhau vài trăm micro giây) sẽ bị máy con nhìn thấy là BẰNG NHAU
 * ở tầng vật lý — trong khi máy chủ (so `DateTimeOffset` giữ đủ tick) và các máy con khác (nếu giữ
 * nguyên chuỗi) vẫn thấy chúng KHÁC nhau. Kết quả: máy con này phá hoà bằng logic/thietBiId/maThaoTac
 * trong khi mọi máy khác đã phân được thắng thua ở tầng vật lý — thứ tự cuối cùng KHÁC NHAU giữa
 * các máy, tức đúng thứ lỗi "hai bản sao phân kỳ" mà toàn bộ thiết kế DongHoLai muốn tránh.
 *
 * Vì vậy file này giữ NGUYÊN chuỗi ISO gốc cho `vatLy`, và so sánh nó bằng SO CHUỖI TỪ ĐIỂN
 * (`<`/`>`), không parse ra `number`. So chuỗi ISO-8601 CÙNG múi giờ (luôn UTC, hậu tố `Z`) CÙNG
 * ĐỘ DÀI (luôn đúng 6 chữ số phần thập phân, luôn đủ 4 chữ số năm) cho kết quả thứ tự thời gian
 * ĐÚNG bằng so từ điển — mỗi trường số (năm, tháng, ngày, giờ...) đều được đệm số 0 ở đầu nên ký tự
 * đứng trước luôn có "trọng số" lớn hơn, y hệt so số. Giả định này được kiểm chứng bằng test
 * `so sánh chuỗi ISO cùng độ dài cho đúng thứ tự thời gian` trong `dauDongHo.test.ts` — brief yêu
 * cầu kiểm tra trước khi dùng, không giả định suông.
 *
 * Đổi lại, mọi hàm dựng mốc trong file này (`tuMocMs`, `catMicroGiay`) PHẢI luôn trả về đúng định
 * dạng cố định `YYYY-MM-DDTHH:mm:ss.ffffffZ` (6 chữ số) — không được để lọt một mốc thiếu/thừa chữ
 * số, vì việc đó sẽ phá đúng giả định so từ điển nói trên.
 */

/** Định dạng CỐ ĐỊNH mà mọi `vatLy` trong hệ thống phải tuân theo: UTC, đúng 6 chữ số micro giây,
 * hậu tố `Z`. Ví dụ hợp lệ: `"2026-09-13T10:00:00.000000Z"`. */
const RE_MOC_HOP_LE = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.(\d+)Z$/

/** Guid rỗng (`Guid.Empty` bên C#) — dùng làm giá trị mặc định cho `maThaoTac` khi chưa có thao
 * tác thật nào (ví dụ: chưa từng phát dấu nào, xem `dongHoMayCon.ts`). */
export const GUID_RONG = '00000000-0000-0000-0000-000000000000'

/**
 * Dấu thời gian của một thao tác — bản TypeScript của `DauDongHo` (record struct C#), đủ để mọi
 * máy xếp thứ tự GIỐNG NHAU. Bốn thành phần theo ĐÚNG thứ tự ưu tiên khi so (xem `soSanh`).
 *
 * `thietBiId`/`maThaoTac` là chuỗi Guid dạng "D" chữ thường (ví dụ
 * `"00000000-0000-0000-0000-000000000001"`) — xem chú thích `soSanhGuid` về vì sao BẮT BUỘC đúng
 * dạng này, không phải mảng byte hay Guid.CompareTo của bất kỳ nền tảng nào.
 */
export type DauDongHo = {
  /** Mốc vật lý — chuỗi ISO-8601 UTC cố định (xem chú thích đầu file). ĐÃ được cắt về micro giây
   * (xem `catMicroGiay`) — dùng `dungDauDongHo` để dựng dấu mới thay vì gán trực tiếp field này,
   * để không quên bước cắt (giữ đúng tinh thần "constructor tự cắt" của bản C# gốc). */
  vatLy: string
  /** Phần logic của đồng hồ lai — số nguyên không âm, tăng dần. */
  logic: number
  /** `null` nghĩa là "ghi từ chính máy chủ" (xem `soSanhGuid`). */
  thietBiId: string | null
  /** Mã thao tác — khoá duy nhất của chính dấu này, tầng phá hoà cuối cùng. */
  maThaoTac: string
}

/**
 * Cắt một mốc ISO-8601 về ĐÚNG 6 chữ số phần thập phân (micro giây) — TRUNCATE (bỏ chữ số thừa),
 * KHÔNG làm tròn, và ĐỆM THÊM số 0 nếu thiếu. Đây là bản port của `DongHoLai.CatMicroGiay`: bản C#
 * cắt `DateTimeOffset.Ticks` (đơn vị 100ns) về bội số của 10 (= 1 micro giây); ở đây ta thao tác
 * trực tiếp trên CHUỖI vì đó là biểu diễn ta chọn (xem chú thích đầu file), nhưng ngữ nghĩa giống
 * hệt: bỏ phần lẻ dưới micro giây, không phải làm tròn lên/xuống.
 *
 * Vì sao bắt buộc, y hệt lý do trong bản C#: nếu để một mốc lẻ (ví dụ 9 chữ số nano giây từ một
 * nguồn khác) lọt vào mà không cắt, hai mốc mà con người coi là "cùng một micro giây" có thể so ra
 * KHÁC nhau ở tầng vật lý, đảo ngược một hợp nhất đã đúng (xem ví dụ sổ rửa tội trong bản C# gốc).
 *
 * Mốc do CHÍNH máy con TypeScript sinh ra (qua `tuMocMs`, dựa trên `Date.getTime()`) không cần cắt
 * gì thêm — JS chỉ có độ phân giải mili giây, vốn đã THÔ HƠN micro giây, `tuMocMs` luôn trả về đúng
 * dạng 6 chữ số (3 chữ số cuối luôn `000`). Hàm này chỉ thật sự cắt bớt khi mốc ĐẾN TỪ NƠI KHÁC có
 * độ phân giải mịn hơn micro giây (mốc máy chủ gửi xuống, dữ liệu kiểm thử dựng tay...).
 */
export function catMicroGiay(moc: string): string {
  const khop = RE_MOC_HOP_LE.exec(moc)
  if (!khop) {
    throw new Error(`Mốc thời gian không đúng định dạng UTC cố định "${moc}" (cần dạng YYYY-MM-DDTHH:mm:ss.ffffffZ)`)
  }
  const goc = moc.slice(0, moc.indexOf('.') + 1)
  const phanThapPhan = khop[1]
  // Đệm thêm số 0 nếu ngắn hơn 6 chữ số, rồi CẮT (không làm tròn) về đúng 6 chữ số đầu.
  const micro = (phanThapPhan + '000000').slice(0, 6)
  return `${goc}${micro}Z`
}

/** Chuyển một mốc `number` (mili giây kể từ epoch Unix, giá trị của `Date.getTime()`) sang dạng
 * chuỗi cố định của hệ thống. 3 chữ số cuối của phần micro giây LUÔN là `000` vì JS không có độ
 * phân giải mịn hơn mili giây (xem chú thích đầu file) — đây KHÔNG phải một giá trị giả, mà là biểu
 * diễn CHÍNH XÁC của một mốc chỉ có độ chính xác mili giây trong hệ quy chiếu micro giây chung. */
export function tuMocMs(ms: number): string {
  // `Date.prototype.toISOString` luôn trả về đúng dạng "YYYY-MM-DDTHH:mm:ss.sssZ" (3 chữ số mili
  // giây, múi giờ UTC) — đúng độ dài cố định ta cần, chỉ thiếu 3 chữ số để thành micro giây.
  const iso = new Date(ms).toISOString()
  return iso.slice(0, -1) + '000Z'
}

/** Chuyển một mốc chuỗi của hệ thống về `number` mili giây (dùng khi cần hiển thị hoặc làm phép
 * toán số học trên thời gian) — LOSSY: phần micro giây (nếu có) bị bỏ, vì `Date.parse` chỉ giữ tới
 * mili giây. KHÔNG dùng kết quả này để SO SÁNH thứ tự — dùng `soSanh`/so chuỗi trực tiếp cho việc
 * đó (xem chú thích đầu file về vì sao ép `number` sớm làm mất thông tin cần để so đúng). */
export function mocSangMs(moc: string): number {
  return new Date(moc).getTime()
}

/**
 * Dựng một `DauDongHo` mới, tự CẮT `vatLy` về micro giây (xem `catMicroGiay`) — tương đương việc
 * bản C# tự cắt ngay trong constructor của `record struct DauDongHo`. LUÔN dùng hàm này để tạo dấu
 * mới thay vì viết trực tiếp một object literal, để không có đường nào quên bước cắt.
 */
export function dungDauDongHo(vatLy: string, logic: number, thietBiId: string | null, maThaoTac: string): DauDongHo {
  return { vatLy: catMicroGiay(vatLy), logic, thietBiId, maThaoTac }
}

/**
 * Thứ tự Guid DUY NHẤT của hệ thống — so chuỗi dạng "D" chữ thường, ordinal (mã điểm UTF-16 của
 * JS — tương đương ordinal ASCII vì Guid chỉ gồm hex + dấu gạch, không có ký tự nào ngoài phạm vi
 * ASCII). Đây là bản port của `DongHoLai.SoSanhGuid`.
 *
 * BẮT BUỘC đúng dạng chuỗi chữ thường: đây là dạng DUY NHẤT mà cả ba nơi (Postgres `memcmp` 16
 * byte big-endian, .NET `string.CompareOrdinal` trên `ToString("D")`, và so chuỗi TypeScript ở đây)
 * biểu diễn đồng nhất — xem chú thích chi tiết trong bản C# gốc về vì sao KHÔNG được so mảng byte
 * thô hay dùng thứ tự riêng của một nền tảng nào. Chủ động hạ chữ thường ở đây (thay vì giả định
 * đầu vào đã là chữ thường) để phòng trường hợp một nơi khác lỡ truyền Guid viết hoa — vẫn phải so
 * ra kết quả giống hệt máy chủ.
 *
 * `null` nghĩa là "ghi từ chính máy chủ" — xếp TRƯỚC mọi giá trị có danh tính. Viết TƯỜNG MINH,
 * KHÔNG quy `null` về chuỗi rỗng hay `GUID_RONG` (xem chú thích trong bản C#: làm vậy sẽ khiến hai
 * dấu khác nhau so ra bằng nhau, phá bất biến `soSanh(a,b) === 0 <=> a và b thật sự là một`).
 */
export function soSanhGuid(a: string | null, b: string | null): number {
  if (a === null) return b === null ? 0 : -1
  if (b === null) return 1
  const ax = a.toLowerCase()
  const bx = b.toLowerCase()
  return ax < bx ? -1 : ax > bx ? 1 : 0
}

/**
 * So hai dấu — bản port của `DongHoLai.SoSanh`. Dương nghĩa là `a` MỚI HƠN.
 *
 * Bốn tầng phá hoà, ĐÚNG THỨ TỰ (không được đổi): vật lý → logic → `thietBiId` → `maThaoTac`. Ba
 * tầng sau tầng vật lý là BẮT BUỘC — thiếu chúng thì hai thao tác cùng mốc vật lý sẽ được các máy
 * con kết luận khác nhau, và các bản sao phân kỳ vĩnh viễn mà không ai báo lỗi (xem bản C# gốc).
 */
export function soSanh(a: DauDongHo, b: DauDongHo): number {
  const theoVatLy = a.vatLy < b.vatLy ? -1 : a.vatLy > b.vatLy ? 1 : 0
  if (theoVatLy !== 0) return theoVatLy

  const theoLogic = a.logic < b.logic ? -1 : a.logic > b.logic ? 1 : 0
  if (theoLogic !== 0) return theoLogic

  const theoThietBi = soSanhGuid(a.thietBiId, b.thietBiId)
  if (theoThietBi !== 0) return theoThietBi

  return soSanhGuid(a.maThaoTac, b.maThaoTac)
}

/** Mốc nhỏ hơn MỌI mốc hợp lệ khác trong hệ thống — bản port của `DateTimeOffset.MinValue`, dùng
 * làm giá trị "chưa từng nhận gì" trong `nangDau`. Vẫn giữ ĐÚNG định dạng cố định 6 chữ số để so
 * chuỗi từ điển không bị lệch độ dài với các mốc thật (năm "0001" so từ điển vẫn nhỏ hơn mọi năm
 * bắt đầu bằng "1" hoặc "2" vì đều đệm đủ 4 chữ số). */
const MOC_TOI_THIEU = '0001-01-01T00:00:00.000000Z'

/**
 * Sinh mốc kế tiếp mà máy này sẽ PHÁT RA, sau khi (tuỳ chọn) nhận một dấu từ nơi khác — bản port
 * của `DongHoLai.NangDau`. Đọc kỹ chú thích trong bản C# gốc về vì sao cần đủ ba tham số:
 *
 * - `dauCuoiCuaTa`: mốc gần nhất CHÍNH máy này đã phát. Thiếu nó thì đồng hồ máy bị chỉnh LÙI
 *   (NTP kéo về, admin sửa tay) sẽ làm hai lần ghi liên tiếp của cùng một máy đảo thứ tự.
 * - `nhanDuoc`: dấu vừa nhận, hoặc `null` nếu đây là lần phát nội bộ (không liên quan tới việc vừa
 *   đọc dữ liệu của ai). Nâng theo nó là cách duy nhất giữ nhân quả: "đã thấy rồi mới sửa" phải
 *   xếp SAU. Hiệu chỉnh độ lệch vật lý KHÔNG thay được việc này.
 * - `gioHienTai`: để phần logic có đường VỀ 0 khi thời gian thật đã vượt qua mọi mốc, nếu không nó
 *   chỉ tăng mãi.
 *
 * Trả về `{ vatLy, logic }` — chưa gắn `thietBiId`/`maThaoTac`, vì đó là việc của nơi gọi (xem
 * `dongHoMayCon.ts`, lớp `DongHoLogicMayCon`).
 */
export function nangDau(
  dauCuoiCuaTa: DauDongHo,
  nhanDuoc: DauDongHo | null,
  gioHienTai: string,
): { vatLy: string; logic: number } {
  const bayGio = catMicroGiay(gioHienTai)
  const cuaTa = catMicroGiay(dauCuoiCuaTa.vatLy)
  const cuaHo = nhanDuoc !== null ? catMicroGiay(nhanDuoc.vatLy) : MOC_TOI_THIEU

  let vatLy = bayGio
  if (cuaTa > vatLy) vatLy = cuaTa
  if (cuaHo > vatLy) vatLy = cuaHo

  // Đồng hồ vật lý đã đi tới TRƯỚC cả hai mốc: đếm logic hết việc, về 0.
  if (vatLy === bayGio && bayGio > cuaTa && bayGio > cuaHo) return { vatLy, logic: 0 }

  let logic = 0
  if (cuaTa === vatLy) logic = dauCuoiCuaTa.logic
  if (nhanDuoc !== null && cuaHo === vatLy) logic = Math.max(logic, nhanDuoc.logic)
  return { vatLy, logic: logic + 1 }
}
