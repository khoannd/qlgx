import { versionConfigXmlResponse } from "@/lib/update-server";

/**
 * Địa chỉ CŨ, dùng bởi các máy cài từ bản 3.3.7 trở về trước — nội dung giống
 * `/capnhat/VersionConfig.xml` NGOẠI TRỪ `<downloadpath>` phải trả về địa chỉ
 * http:// của chính nhóm này (xem chú thích tại `DOWNLOAD_BASE_URL_FOR` trong
 * `src/lib/update-server.ts`). KHÔNG được xoá — xem HOP_DONG_MAY_CHU_CAP_NHAT.md,
 * mục "Các địa chỉ cũ".
 */
export function GET() {
  return versionConfigXmlResponse("goc");
}
