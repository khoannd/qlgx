import type { IconName } from "@/lib/content";

/**
 * Nhãn tiếng Việt cho từng icon, dùng trong các ô chọn của form nội dung
 * trang chủ. Danh sách giá trị phải khớp với `IconName` trong
 * `src/lib/content/types.ts` — TypeScript sẽ báo lỗi ở đây nếu thiếu hoặc
 * thừa một icon.
 */
export const ICON_OPTIONS: { value: IconName; label: string }[] = [
  { value: "users", label: "Người — giáo dân" },
  { value: "home", label: "Nhà — gia đình" },
  { value: "book", label: "Sách — sổ bí tích" },
  { value: "heart", label: "Trái tim — hôn phối" },
  { value: "graduation", label: "Mũ tốt nghiệp — giáo lý" },
  { value: "chart", label: "Biểu đồ — thống kê" },
  { value: "printer", label: "Máy in" },
  { value: "refresh", label: "Làm mới — sao lưu" },
  { value: "shield", label: "Khiên — bảo mật/phân quyền" },
  { value: "download", label: "Tải xuống" },
  { value: "check-square", label: "Dấu tick — hoàn tất" },
  { value: "database", label: "Cơ sở dữ liệu" },
  { value: "family", label: "Gia đình (nhiều người)" },
  { value: "globe", label: "Địa cầu" },
  { value: "pencil", label: "Bút chì — nhập liệu" },
  { value: "help", label: "Dấu hỏi — trợ giúp" },
  { value: "external", label: "Mũi tên chéo — liên kết ngoài" },
  { value: "arrow-right", label: "Mũi tên phải" },
  { value: "mail", label: "Thư — email" },
  { value: "facebook", label: "Facebook" },
];

/** Danh sách giá trị hợp lệ, dùng để kiểm tra dữ liệu gửi lên từ form. */
export const ICON_VALUES: IconName[] = ICON_OPTIONS.map((o) => o.value);
