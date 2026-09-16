/**
 * Kiểm tra địa chỉ (`href`) do quản trị viên nhập ở /admin trước khi ghi vào D1.
 *
 * VÌ SAO CẦN: quanlygiaoxu.net là nơi quý cha, quý sơ tải bộ cài về. Nếu một
 * `href` bất kỳ ghi được vào cột `downloads_json`, thì chỉ cần một tài khoản
 * admin bị chiếm (hoặc một lần dán nhầm địa chỉ) là nút "Tải phần mềm" trên
 * trang chính thức trỏ thẳng tới bộ cài của kẻ tấn công — trong vòng 5 phút
 * (ISR `revalidate = 300`). Đây là hậu quả nặng nhất mà trang công khai này có
 * thể gây ra, nên chặn ngay ở đường GHI, không phụ thuộc vào việc lúc render
 * có nhớ lọc hay không.
 *
 * Hai mức chặt khác nhau:
 *   - `kiemTraLienKetTaiVe`  — cho nút tải phần mềm: RẤT chặt, chỉ hai nơi.
 *   - `kiemTraLienKetHoTro`  — cho các liên kết hỗ trợ/chân trang: chặn
 *     `javascript:`/`data:` và mọi tên miền lạ, nhưng vẫn cho các nơi chính
 *     thức của dự án.
 *
 * Tất cả hàm ở đây là hàm thuần, không phụ thuộc Next/Cloudflare, để chạy
 * được test bằng `node --experimental-strip-types`.
 */

/** Kết quả kiểm: hợp lệ, hoặc không hợp lệ kèm câu tiếng Việt để hiện cho admin. */
export type KetQuaKiem = { hopLe: true } | { hopLe: false; lyDo: string };

const HOP_LE: KetQuaKiem = { hopLe: true };
const khong = (lyDo: string): KetQuaKiem => ({ hopLe: false, lyDo });

/**
 * Tên miền được phép cho các liên kết hỗ trợ / chân trang. Đây là danh sách
 * ĐÓNG: thêm nơi mới phải sửa ở đây, cố ý để một lần nhập nhầm không âm thầm
 * đưa người dùng ra ngoài.
 */
export const HOST_HO_TRO_CHO_PHEP: readonly string[] = [
  "quanlygiaoxu.net",
  "www.quanlygiaoxu.net",
  "forum.quanlygiaoxu.net",
  "github.com",
  "www.github.com",
  "raw.githubusercontent.com",
  "facebook.com",
  "www.facebook.com",
  "m.facebook.com",
  "youtube.com",
  "www.youtube.com",
  "youtu.be",
];

/** Tên miền duy nhất được phép cho nút tải phần mềm khi trỏ ra ngoài. */
export const HOST_TAI_VE_CHO_PHEP = "raw.githubusercontent.com";

/** Chủ sở hữu kho trên GitHub — đường dẫn tải ngoài phải nằm dưới tài khoản này. */
export const CHU_KHO_GITHUB = "khoannd";

/** Tiền tố đường dẫn nội bộ hợp lệ cho nút tải phần mềm. */
export const DUONG_TAI_VE_NOI_BO = "/api/tai-ve/";

/**
 * Chuỗi có ký tự điều khiển / khoảng trắng lạ thì từ chối thẳng: đó là mẹo
 * kinh điển để lách bộ lọc scheme (`java\nscript:`, `%00javascript:`...).
 */
function coKyTuLa(gia_tri: string): boolean {
  // eslint-disable-next-line no-control-regex
  return /[\u0000-\u0020\u007f-\u009f\u2028\u2029]/.test(gia_tri);
}

/** Đường dẫn nội bộ thật sự: bắt đầu bằng "/" nhưng KHÔNG phải "//" hay "/\". */
function laDuongDanNoiBo(href: string): boolean {
  return href.startsWith("/") && !href.startsWith("//") && !href.startsWith("/\\");
}

/** Phân tích URL tuyệt đối, trả `null` nếu không phân tích được. */
function phanTich(href: string): URL | null {
  try {
    return new URL(href);
  } catch {
    return null;
  }
}

/**
 * Liên kết hỗ trợ (`helpLinks`, chân trang): cho đường dẫn nội bộ, neo `#`,
 * `mailto:` và `https:` tới tên miền trong danh sách trắng. Mọi thứ khác —
 * kể cả `http:` và `//host` — bị từ chối.
 */
export function kiemTraLienKetHoTro(href: string): KetQuaKiem {
  const gia_tri = href.trim();
  if (!gia_tri) return khong("Địa chỉ liên kết để trống.");
  if (coKyTuLa(gia_tri)) return khong("Địa chỉ liên kết chứa ký tự không hợp lệ.");

  if (gia_tri.startsWith("#")) return HOP_LE;
  if (laDuongDanNoiBo(gia_tri)) return HOP_LE;

  const url = phanTich(gia_tri);
  if (!url) {
    return khong(
      `Địa chỉ "${gia_tri}" không hợp lệ. Dùng đường dẫn nội bộ bắt đầu bằng "/", ` +
        `hoặc địa chỉ https:// đầy đủ.`,
    );
  }

  if (url.protocol === "mailto:") {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(url.pathname)
      ? HOP_LE
      : khong(`Địa chỉ email trong "${gia_tri}" không hợp lệ.`);
  }

  if (url.protocol !== "https:") {
    return khong(
      `Địa chỉ "${gia_tri}" dùng "${url.protocol}" — chỉ chấp nhận https://, mailto: ` +
        `hoặc đường dẫn nội bộ bắt đầu bằng "/".`,
    );
  }

  if (url.username || url.password) {
    return khong(`Địa chỉ "${gia_tri}" chứa tên đăng nhập/mật khẩu — không được phép.`);
  }

  if (!HOST_HO_TRO_CHO_PHEP.includes(url.hostname.toLowerCase())) {
    return khong(
      `Tên miền "${url.hostname}" không nằm trong danh sách cho phép ` +
        `(${HOST_HO_TRO_CHO_PHEP.join(", ")}). Nếu thật sự cần, thêm nó vào ` +
        `src/lib/admin/kiem-tra-lien-ket.ts.`,
    );
  }

  return HOP_LE;
}

/**
 * Nút tải phần mềm: chỉ hai nơi — endpoint tải nội bộ `/api/tai-ve/...` hoặc
 * tệp nằm trong kho GitHub của chính dự án. Nút tải KHÔNG có lý do chính đáng
 * nào để trỏ đi nơi khác.
 */
export function kiemTraLienKetTaiVe(href: string): KetQuaKiem {
  const gia_tri = href.trim();
  if (!gia_tri) return khong("Địa chỉ tệp tải để trống.");
  if (coKyTuLa(gia_tri)) return khong("Địa chỉ tệp tải chứa ký tự không hợp lệ.");

  if (laDuongDanNoiBo(gia_tri)) {
    if (!gia_tri.startsWith(DUONG_TAI_VE_NOI_BO)) {
      return khong(
        `Đường dẫn tải nội bộ phải bắt đầu bằng "${DUONG_TAI_VE_NOI_BO}" (đang là "${gia_tri}").`,
      );
    }
    if (gia_tri.includes("..")) {
      return khong(`Đường dẫn tải "${gia_tri}" chứa ".." — không được phép.`);
    }
    return HOP_LE;
  }

  const url = phanTich(gia_tri);
  if (!url) {
    return khong(
      `Địa chỉ tệp tải "${gia_tri}" không hợp lệ. Dùng "${DUONG_TAI_VE_NOI_BO}..." ` +
        `hoặc https://${HOST_TAI_VE_CHO_PHEP}/${CHU_KHO_GITHUB}/...`,
    );
  }

  if (url.protocol !== "https:" || url.username || url.password) {
    return khong(
      `Địa chỉ tệp tải "${gia_tri}" phải là https:// tới ${HOST_TAI_VE_CHO_PHEP}, ` +
        `không kèm tên đăng nhập.`,
    );
  }

  if (url.hostname.toLowerCase() !== HOST_TAI_VE_CHO_PHEP) {
    return khong(
      `Nút tải chỉ được trỏ tới "${DUONG_TAI_VE_NOI_BO}..." hoặc ` +
        `https://${HOST_TAI_VE_CHO_PHEP}/${CHU_KHO_GITHUB}/... — ` +
        `"${url.hostname}" không được phép.`,
    );
  }

  if (!url.pathname.startsWith(`/${CHU_KHO_GITHUB}/`)) {
    return khong(
      `Tệp tải trên GitHub phải nằm dưới tài khoản "${CHU_KHO_GITHUB}" (đang là "${url.pathname}").`,
    );
  }

  return HOP_LE;
}

/* ------------------------------------------------------------------ */
/* Kiểm lược đồ cho JSON dán tay ở /admin/release                      */
/* ------------------------------------------------------------------ */

const ICON_NHOM_CHO_PHEP = ["download", "check-square", "database", "book"];

function laChuoiCoNoiDung(gia_tri: unknown): gia_tri is string {
  return typeof gia_tri === "string" && gia_tri.trim() !== "";
}

/**
 * Kiểm `downloads_json`: đúng hình dạng `DownloadFile[]` VÀ mọi `href` qua được
 * `kiemTraLienKetTaiVe`. Trả câu tiếng Việt đầu tiên gặp lỗi.
 */
export function kiemTraDanhSachTaiVe(downloads: unknown): KetQuaKiem {
  if (!Array.isArray(downloads)) return khong("Danh sách tệp tải không phải một mảng.");
  if (downloads.length === 0) return khong("Cần ít nhất một tệp tải.");

  for (let i = 0; i < downloads.length; i++) {
    const muc = downloads[i] as Record<string, unknown> | null;
    const o = i + 1;
    if (!muc || typeof muc !== "object" || Array.isArray(muc)) {
      return khong(`Tệp tải thứ ${o} không phải một đối tượng JSON.`);
    }
    for (const truong of ["id", "tag", "title", "summary", "fileName", "size", "cta"]) {
      if (!laChuoiCoNoiDung(muc[truong])) {
        return khong(`Tệp tải thứ ${o}: thiếu hoặc sai trường "${truong}".`);
      }
    }
    if (typeof muc.primary !== "boolean") {
      return khong(`Tệp tải thứ ${o}: trường "primary" phải là true hoặc false.`);
    }
    if (!laChuoiCoNoiDung(muc.href)) {
      return khong(`Tệp tải thứ ${o}: thiếu trường "href".`);
    }
    const kq = kiemTraLienKetTaiVe(muc.href);
    if (!kq.hopLe) return khong(`Tệp tải thứ ${o}: ${kq.lyDo}`);
  }

  return HOP_LE;
}

/** Kiểm `groups_json`: đúng hình dạng `ReleaseGroup[]`, icon nằm trong danh sách đóng. */
export function kiemTraNhomThayDoi(groups: unknown): KetQuaKiem {
  if (!Array.isArray(groups)) return khong("Danh sách thay đổi không phải một mảng.");

  for (let i = 0; i < groups.length; i++) {
    const muc = groups[i] as Record<string, unknown> | null;
    const o = i + 1;
    if (!muc || typeof muc !== "object" || Array.isArray(muc)) {
      return khong(`Nhóm thay đổi thứ ${o} không phải một đối tượng JSON.`);
    }
    for (const truong of ["id", "title"]) {
      if (!laChuoiCoNoiDung(muc[truong])) {
        return khong(`Nhóm thay đổi thứ ${o}: thiếu hoặc sai trường "${truong}".`);
      }
    }
    if (typeof muc.icon !== "string" || !ICON_NHOM_CHO_PHEP.includes(muc.icon)) {
      return khong(
        `Nhóm thay đổi thứ ${o}: icon phải là một trong ${ICON_NHOM_CHO_PHEP.join(", ")}.`,
      );
    }
    if (!Array.isArray(muc.items) || !muc.items.every((x) => laChuoiCoNoiDung(x))) {
      return khong(`Nhóm thay đổi thứ ${o}: "items" phải là mảng các chuỗi.`);
    }
  }

  return HOP_LE;
}
