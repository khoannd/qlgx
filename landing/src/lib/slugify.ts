/**
 * Sinh id nội bộ ngắn gọn từ một chuỗi tiếng Việt có dấu — dùng cho các mục
 * trong `LandingContent` (tính năng, liên kết hỗ trợ, câu hỏi) mà form quản
 * trị không bắt admin phải tự nghĩ ra id.
 *
 * Không dùng cho slug bài viết: những slug đó là một phần của URL công khai
 * (/tin-tuc/{slug}), phải do admin gõ tay và kiểm soát trực tiếp.
 */
export function slugify(text: string): string {
  const base = text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .replace(/đ/gi, "d")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "");
  return base || "muc";
}

/** Đảm bảo id không trùng trong cùng một danh sách bằng cách thêm hậu tố -2, -3... */
export function uniqueSlug(text: string, taken: Set<string>): string {
  const base = slugify(text);
  let candidate = base;
  let n = 2;
  while (taken.has(candidate)) {
    candidate = `${base}-${n}`;
    n++;
  }
  taken.add(candidate);
  return candidate;
}
