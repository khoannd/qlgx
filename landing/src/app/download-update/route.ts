import { downloadUpdateResponse } from "@/lib/update-server";

/**
 * Địa chỉ CŨ, dùng bởi máy cài bản 3.3.7 trở về trước (nhóm "goc") — nội dung
 * y hệt `/capnhat/download-update`.
 *
 * ĐÃ THIẾU ROUTE NÀY TRƯỚC ĐÂY — xác nhận thật bằng cách đọc code máy khách cũ
 * (`Source/AutoUpdate/AutoUpdate.cs`, tag release-3.3.7-net20/release-3.7.7-net20):
 * máy khách ghép `<downloadpath>http://quanlygiaoxu.net/</downloadpath>` với
 * thuộc tính `value="download-update"` thành `http://quanlygiaoxu.net/download-update`
 * (KHÔNG phải `/download.asp`) để tải gói .zip cập nhật. Thiếu route này khiến
 * máy đời cũ báo "có bản mới", bắt đầu tải, nhưng tải 404 rồi treo mãi không
 * xong — không có route nào khác xử lý đúng path này. `/download.asp` vẫn giữ
 * lại song song (không xoá đường dẫn cũ) dù không route nào của phần mềm thật
 * sự gọi tới nó theo bằng chứng đã đọc được.
 */
export function GET(request: Request) {
  return downloadUpdateResponse(request, "goc");
}
