import { versionTextResponse } from "@/lib/update-server";

/**
 * Địa chỉ CŨ, dùng bởi các máy cài bản 4.0.0–4.0.1. KHÔNG được xoá — xem
 * HOP_DONG_MAY_CHU_CAP_NHAT.md, mục "Các địa chỉ cũ".
 */
export function GET() {
  return versionTextResponse("4.0");
}
