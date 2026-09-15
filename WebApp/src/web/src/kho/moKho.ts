/**
 * Mở kho IndexedDB của máy này — nơi giữ bản sao dữ liệu giáo xứ để dùng được khi mất mạng
 * (xem docs/superpowers/specs/2026-09-13-dong-bo-offline-design.md mục 5, ràng buộc 4 và 5).
 *
 * ĐÂY LÀ CHỖ DỄ MẤT DỮ LIỆU NHẤT trong toàn kế hoạch nếu xử lý lỗi mở kho bằng cách xoá-và-tạo-lại:
 * một lỗi phiên bản (`VersionError` — kho trên máy đã MỚI HƠN mã đang chạy, ví dụ người dùng mở lại
 * bản cũ sau khi service worker đã âm thầm tải bản mới, hoặc hai tab lệch phiên bản) không phải là
 * kho hỏng — kho đó có thể đang giữ hàng chờ (việc chưa gửi lên máy chủ) của cả tuần làm việc. Xoá
 * nó đi để "sửa lỗi" là mất trắng, vĩnh viễn, không báo trước.
 *
 * Vì vậy hàm này TUYỆT ĐỐI KHÔNG BAO GIỜ gọi `indexedDB.deleteDatabase()`. Gặp `VersionError` thì
 * ném lại một lỗi có TÊN RIÊNG (`LoiKhoMoiHonMa`) để tầng trên (màn hình) nhận ra và hướng dẫn người
 * dùng (ví dụ: tải lại trang, đóng bớt tab), thay vì tự ý xử lý bằng cách phá dữ liệu.
 */

/** Tăng số này khi cần THÊM kho con mới hoặc thêm chỉ mục — KHÔNG BAO GIỜ dùng để "dọn" kho cũ, vì
 * `onupgradeneeded` ở dưới chỉ được phép thêm, không được xoá gì của người dùng (xem ràng buộc 5). */
export const PHIEN_BAN_KHO = 1

/** Tên mặc định của kho — cố định vì cả ứng dụng chỉ có một kho theo trình duyệt (khoá theo giáo
 * xứ, không theo tài khoản — xem spec mục 6.4 "Đổi người dùng"). Có thể truyền tên khác khi gọi
 * `moKho` (dùng cho kiểm thử, để mỗi ca kiểm thử có một kho riêng, không lẫn dữ liệu vào nhau). */
const TEN_KHO_MAC_DINH = 'qlgx'

/** Năm kho con CỐ ĐỊNH — xem bảng "Cấu trúc file" của kế hoạch, mỗi kho con có một file riêng phụ
 * trách (Task 2, 3 sau này). Task 1 chỉ tạo tên, KHÔNG áp `keyPath` cho bất kỳ kho nào (dùng khoá
 * ngoài dòng — out-of-line key) — quyết định hình dạng bản ghi (khoá là gì, có tự tăng hay không)
 * để dành cho task phụ trách từng kho, tránh Task 1 đoán sai rồi sau này phải đổi `keyPath` (đổi
 * `keyPath` của một kho đã có dữ liệu bắt buộc phải xoá-tạo-lại kho đó — đúng thứ ràng buộc 5 cấm). */
export const KHO_BAN_GHI = 'banGhi'
export const KHO_HANG_CHO = 'hangCho'
export const KHO_SO_DA_NHAN = 'soDaNhan'
export const KHO_CON_TRO = 'conTro'
export const KHO_CAN_XEM_LAI = 'canXemLai'

const TAT_CA_KHO_CON = [KHO_BAN_GHI, KHO_HANG_CHO, KHO_SO_DA_NHAN, KHO_CON_TRO, KHO_CAN_XEM_LAI] as const

/**
 * Lỗi ném ra khi kho trên máy đã ở phiên bản MỚI HƠN mã nguồn đang chạy (`VersionError` của
 * IndexedDB). Tầng trên (màn hình) bắt lỗi này bằng `instanceof LoiKhoMoiHonMa` để hiện hướng dẫn
 * đúng, thay vì hiện lỗi kỹ thuật chung chung hoặc — nguy hiểm hơn — tự ý xoá kho để "thử lại".
 *
 * Tình huống thật gây ra lỗi này: service worker đã tải xong bản JS mới (tăng `PHIEN_BAN_KHO`)
 * nhưng người dùng chưa tải lại trang nên tab đang mở vẫn chạy mã CŨ; hoặc người dùng mở hai tab
 * lệch phiên bản. Cách sửa đúng là tải lại trang để chạy mã mới khớp với kho, KHÔNG PHẢI xoá kho.
 */
export class LoiKhoMoiHonMa extends Error {
  constructor(tenKho: string, phienBanYeuCau: number, loiGoc: unknown) {
    super(
      `Kho "${tenKho}" trên máy đã ở phiên bản mới hơn phiên bản ${phienBanYeuCau} mà mã đang chạy ` +
        `yêu cầu. Có thể một tab khác (hoặc service worker) đã chạy bản mới hơn. Cần tải lại trang ` +
        `để chạy đúng bản mới — KHÔNG được xoá kho, dữ liệu người dùng vẫn còn nguyên trong đó.` +
        (loiGoc instanceof Error ? ` (lỗi gốc: ${loiGoc.message})` : ''),
    )
    this.name = 'LoiKhoMoiHonMa'
  }
}

/**
 * Mở kho IndexedDB, tạo các kho con còn thiếu nếu cần.
 *
 * Hai tham số `tenKho`/`phienBan` có giá trị mặc định để lời gọi thường ngày chỉ cần `moKho()`
 * (đúng chữ ký đã công bố `moKho(): Promise<IDBDatabase>`) — cho phép truyền tham số khác chỉ để
 * kiểm thử: mỗi ca kiểm thử dùng một tên kho riêng (tránh lẫn dữ liệu giữa các ca), và ca kiểm thử
 * "nâng cấp phiên bản không làm mất hàng chờ" cần tự mô phỏng một lần nâng cấp thật bằng cách mở
 * lại với số phiên bản lớn hơn.
 */
export function moKho(tenKho: string = TEN_KHO_MAC_DINH, phienBan: number = PHIEN_BAN_KHO): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const yeuCau = indexedDB.open(tenKho, phienBan)

    // CHỈ được THÊM kho con còn thiếu — không bao giờ `deleteObjectStore` một kho đã có dữ liệu
    // người dùng (ràng buộc "onupgradeneeded chỉ được thêm kho con mới"). Nhờ kiểm tra
    // `contains` trước khi tạo, hàm này gọi lại nhiều lần (hoặc nâng `PHIEN_BAN_KHO` ở các bản sau
    // để thêm kho con mới) đều an toàn: kho con đã có dữ liệu không bị đụng tới.
    yeuCau.onupgradeneeded = () => {
      const db = yeuCau.result
      for (const ten of TAT_CA_KHO_CON) {
        if (!db.objectStoreNames.contains(ten)) {
          db.createObjectStore(ten)
        }
      }
    }

    yeuCau.onsuccess = () => resolve(yeuCau.result)

    yeuCau.onerror = () => {
      const loi = yeuCau.error
      if (loi?.name === 'VersionError') {
        // TUYỆT ĐỐI KHÔNG gọi indexedDB.deleteDatabase() ở đây — xem chú thích đầu file. Ném lại
        // một lỗi có tên riêng để tầng trên hiện màn hình hướng dẫn.
        reject(new LoiKhoMoiHonMa(tenKho, phienBan, loi))
        return
      }
      reject(loi ?? new Error(`Không mở được kho dữ liệu ngoại tuyến "${tenKho}"`))
    }

    // `onblocked` xảy ra khi một tab khác đang giữ kết nối kho ở phiên bản cũ trong lúc tab này cố
    // nâng cấp — IndexedDB sẽ treo `yeuCau` vô thời hạn cho tới khi tab kia đóng kết nối. Không xử
    // lý nhánh này thì `moKho()` không bao giờ resolve lẫn reject, promise treo mãi mãi. Ném lỗi rõ
    // ràng để tầng trên báo "đóng bớt tab khác đang mở QLGX" thay vì làm gì đó với kho.
    yeuCau.onblocked = () => {
      reject(new Error('Không mở được kho: có tab khác đang mở QLGX ở phiên bản cũ hơn. Xin đóng bớt tab đó rồi thử lại.'))
    }
  })
}
