import { downloadUpdateResponse } from "@/lib/update-server";

/**
 * Địa chỉ CŨ, dùng bởi các máy cài từ bản 3.3.7 trở về trước — nội dung y hệt
 * `/capnhat/download-update`. Đời cũ gọi kèm query string `?opt=update`
 * (thuộc tính gốc là `value="download.asp?opt=update"`), nhưng chỉ có đúng
 * một giá trị từng được dùng nên không cần đọc lại query — trả thẳng gói cập
 * nhật mới nhất. KHÔNG được xoá — xem HOP_DONG_MAY_CHU_CAP_NHAT.md, mục
 * "Các địa chỉ cũ".
 */
export function GET(request: Request) {
  return downloadUpdateResponse(request, "goc");
}
