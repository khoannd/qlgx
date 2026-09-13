import { versionTextResponse } from "@/lib/update-server";

/**
 * Địa chỉ CŨ, dùng bởi các máy cài từ bản 3.3.7 trở về trước. KHÔNG được xoá:
 * những máy này nằm cứng địa chỉ gốc `http://quanlygiaoxu.net/` và sẽ vĩnh
 * viễn không cập nhật được nếu thiếu. Xem HOP_DONG_MAY_CHU_CAP_NHAT.md, mục
 * "Các địa chỉ cũ".
 *
 * ĐANG TẠM DỪNG báo có bản mới cho riêng nhóm này — xem
 * `TAM_DUNG_CAP_NHAT_CHO_NHOM` trong `src/lib/update-server.ts`.
 */
export function GET() {
  return versionTextResponse("goc");
}
