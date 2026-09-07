import type { IconName } from "@/lib/content";

export type { IconName } from "@/lib/content";

/**
 * Phần nội dung cố định của trang: định danh, điều hướng và ảnh chụp màn hình.
 *
 * Khác với `src/lib/content` — nơi chứa dữ liệu thay đổi theo thời gian, sửa được
 * qua `/admin` (bài viết, bản phát hành, và từ đây các danh sách của trang chủ:
 * tính năng, vấn đề, con số, câu hỏi thường gặp, bước cài đặt, liên kết hỗ trợ) —
 * những thứ CÒN LẠI ở đây gắn liền với thiết kế trang (ảnh cần tệp thật, chữ Hero
 * ngắt dòng thủ công) nên vẫn để ngay trong mã. Xem lý do đầy đủ tại chú thích
 * của `LandingContent` trong `src/lib/content/types.ts`.
 */

export const SITE_URL = (
  process.env.NEXT_PUBLIC_SITE_URL ?? "https://quanlygiaoxu.net"
).replace(/\/$/, "");

export const FORUM_URL = "https://forum.quanlygiaoxu.net";
export const SUPPORT_EMAIL = "hotro@quanlygiaoxu.net";
export const FACEBOOK_URL = "https://www.facebook.com/qlgx2013";

export const site = {
  name: "Quản Lý Giáo Xứ",
  shortName: "QLGX",
  tagline: "Phần mềm cho giáo xứ",
  // Không nêu đích danh "cha xứ" làm chủ ngữ: người trực tiếp dùng phần mềm có
  // thể là quý Cha, cũng có thể là ban hành giáo, thư ký văn phòng hay bất kỳ
  // ai được giao việc sổ sách — dùng "giáo xứ" làm chủ ngữ để trung tính, đúng
  // với thực tế là nhiều vai trò khác nhau cùng dùng chung một phần mềm.
  description:
    "Phần mềm miễn phí giúp giáo xứ quản lý hồ sơ giáo dân, gia đình, sổ bí tích, lớp giáo lý, hội đoàn và thống kê. Chạy trên Windows, dữ liệu lưu ngay tại giáo xứ.",
} as const;

export const navLinks = [
  { href: "/#tinh-nang", label: "Tính năng" },
  { href: "/#tai-ve", label: "Tải về" },
  { href: "/huong-dan", label: "Hướng dẫn" },
  { href: "/tin-tuc", label: "Tin tức" },
  { href: "/#cau-hoi", label: "Hỏi đáp" },
] as const;

/* ------------------------------------------------------------------ */
/* Ảnh chụp màn hình                                                   */
/* ------------------------------------------------------------------ */

export type Screenshot = {
  id: string;
  tab: string;
  icon: IconName;
  label: string;
  src: string;
  width: number;
  height: number;
  alt: string;
  caption: string;
};

/**
 * Ba màn hình quan trọng nhất — Gia đình, Giáo dân, Sổ bí tích — đứng đầu danh
 * sách và cũng là thứ khách xem trước tiên.
 *
 * Mọi ảnh đã được cắt bỏ thanh tiêu đề cửa sổ Windows và thay tên giáo xứ trong
 * dữ liệu mẫu ở khâu xử lý ảnh (xem README, mục Ảnh chụp màn hình). Kích thước
 * bên dưới là kích thước SAU khi cắt — sai một pixel là trang bị nhảy layout.
 */
export const screenshots: Screenshot[] = [
  {
    id: "gia-dinh",
    tab: "Gia đình",
    icon: "family",
    label: "Danh sách gia đình theo giáo họ",
    src: "/images/gia_dinh.jpg",
    width: 1077,
    height: 703,
    alt: "Danh sách gia đình trong phần mềm Quản Lý Giáo Xứ, hiển thị mã gia đình, tên gia đình, tên người chồng và người vợ, số người và số điện thoại.",
    caption:
      "Lập phiếu gia đình với đầy đủ vai trò từng thành viên; chương trình tự cảnh báo khi một người bị đưa vào hai gia đình khác nhau.",
  },
  {
    id: "giao-dan",
    tab: "Hồ sơ giáo dân",
    icon: "users",
    label: "Nhập hồ sơ một giáo dân",
    src: "/images/them_giao_dan.jpg",
    width: 825,
    height: 700,
    alt: "Màn hình nhập hồ sơ một giáo dân gồm thông tin cá nhân, Rửa tội, Rước lễ lần đầu, Thêm sức và thông tin chuyển xứ.",
    caption:
      "Toàn bộ lý lịch và các bí tích của một giáo dân nằm gọn trong một màn hình, chia theo từng nhóm để dễ nhập và dễ tra.",
  },
  {
    id: "bi-tich",
    tab: "Sổ bí tích",
    icon: "book",
    label: "Sổ bí tích — chi tiết một đợt",
    src: "/images/dot_bi_tich_chi_tiet.jpg",
    width: 852,
    height: 606,
    alt: "Màn hình chi tiết một đợt bí tích Rửa tội, liệt kê danh sách người lãnh nhận kèm số rửa tội, tên thánh, họ tên, ngày sinh và người đỡ đầu.",
    caption:
      "Nhập theo đợt cho cả lớp Rửa tội hay Thêm sức, số bí tích được đánh tự động theo từng sổ.",
  },
  {
    id: "danh-sach-giao-dan",
    tab: "Danh sách giáo dân",
    icon: "chart",
    label: "Danh sách giáo dân — lọc và in chứng nhận",
    src: "/images/luoi_danh_sach.jpg",
    width: 816,
    height: 585,
    alt: "Danh sách giáo dân dạng bảng, có thể lọc và sắp xếp theo từng cột, kèm menu in chứng nhận Rửa tội, Thêm sức và giới thiệu hôn phối.",
    caption:
      "Bấm chuột phải lên một giáo dân là in được ngay chứng nhận Rửa tội, Thêm sức hay giấy giới thiệu hôn phối.",
  },
  {
    id: "rao-hon-phoi",
    tab: "Rao hôn phối",
    icon: "heart",
    label: "Danh sách rao hôn phối",
    src: "/images/rao_hon_phoi1.jpg",
    width: 825,
    height: 301,
    alt: "Danh sách rao hôn phối theo từng đợt, hiển thị tên đôi hôn phối và ba ngày rao.",
    caption:
      "Lập danh sách rao theo ba Chúa nhật, in tờ rao và tờ điều tra hôn phối trực tiếp từ chương trình.",
  },
  {
    id: "thong-ke",
    tab: "Thống kê",
    icon: "chart",
    label: "Thống kê chung theo năm và giáo họ",
    src: "/images/thong_ke.jpg",
    width: 1017,
    height: 710,
    alt: "Màn hình thống kê chung với bộ lọc theo khoảng năm và giáo họ, kết quả hiển thị dạng bảng danh sách giáo dân.",
    caption:
      "Chọn khoảng năm và giáo họ, chương trình đếm ngay số người sinh ra, rửa tội, thêm sức, hôn phối, qua đời hay tân tòng — kèm danh sách chi tiết để in.",
  },
  {
    id: "mau-in",
    tab: "Mẫu in",
    icon: "printer",
    label: "Tờ điều tra hôn phối xuất ra Word",
    src: "/images/report_dieu_tra_hon_phoi.jpg",
    width: 780,
    height: 1075,
    alt: "Tờ điều tra hôn phối được chương trình xuất ra file Microsoft Word, đã điền sẵn thông tin đôi hôn phối.",
    caption:
      "Các mẫu in là file Word và Excel thông thường — giáo xứ có thể tự sửa lại cho hợp với mẫu của giáo phận mình.",
  },
  {
    id: "sao-luu",
    tab: "Sao lưu dữ liệu",
    icon: "refresh",
    label: "Sao lưu và khôi phục dữ liệu",
    src: "/images/sao_luu_khoi_phuc.jpg",
    width: 568,
    height: 465,
    alt: "Hộp thoại sao lưu và khôi phục dữ liệu, cho phép chọn nơi lưu tệp sao lưu.",
    caption:
      "Nên sao lưu hằng tuần ra USB hoặc ổ đĩa ngoài. Tệp sao lưu có thể mang sang máy khác để khôi phục nguyên vẹn.",
  },
];

/** Ảnh lớn ở đầu trang. Dùng màn hình Gia đình vì nó cho thấy dữ liệu thật nhất. */
export const heroScreenshot = {
  src: "/images/gia_dinh.jpg",
  width: 1077,
  height: 703,
  label: "Danh sách gia đình theo giáo họ",
  alt: `Danh sách gia đình trong phần mềm ${site.name}, hiển thị mã gia đình, tên gia đình, tên người chồng và người vợ, số người trong gia đình và số điện thoại.`,
} as const;

/** Ảnh phụ nhỏ hơn, xếp chồng lệch phía sau ảnh chính ở đầu trang. */
export const heroSecondaryShot = {
  src: "/images/dot_bi_tich_chi_tiet.jpg",
  width: 852,
  height: 606,
  label: "Sổ bí tích",
  alt: "Màn hình chi tiết một đợt bí tích Rửa tội với danh sách người lãnh nhận.",
} as const;

/* ------------------------------------------------------------------ */
/* Tiện ích                                                            */
/* ------------------------------------------------------------------ */

/** Định dạng ngày ISO thành dạng quen thuộc với người Việt: 04/09/2026 */
export function formatDate(iso: string): string {
  const [y, m, d] = iso.split("-");
  return `${d}/${m}/${y}`;
}
