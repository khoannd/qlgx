import { logDownload } from "@/lib/download-log";
import { GITHUB_RAW_BASE } from "@/lib/github";
import { downloadableVersions } from "@/lib/version-history";

/**
 * Tải một phiên bản CŨ cụ thể, không phải bản mới nhất — khác với
 * `/api/tai-ve/[kenh]` ở chỗ phiên bản không tự đổi theo thời gian, nên
 * chuyển hướng thẳng bằng 301 (trình duyệt nhớ lâu là đúng ý ở đây).
 *
 * Tham số `phienBan` là số phiên bản hiển thị (ví dụ "3.3.7"), không phải tên
 * tệp — đối chiếu qua danh sách trắng `downloadableVersions` (chỉ những bản
 * còn tệp cài đặt thật trong `Release/`) trước khi dựng URL, để không ai dựng
 * được đường dẫn tới một tệp bất kỳ trên repo.
 */
export async function GET(
  request: Request,
  { params }: { params: Promise<{ phienBan: string }> },
) {
  const { phienBan } = await params;
  const entry = downloadableVersions.find((v) => v.label === phienBan);

  if (!entry) {
    return Response.json(
      {
        error: "Không có bản cài đặt cho phiên bản này.",
        hop_le: downloadableVersions.map((v) => v.label),
      },
      { status: 404 },
    );
  }

  // PHẢI await: xem chú thích trong logDownload()/src/lib/update-server.ts —
  // dùng "void" từng làm mất log vì Worker có thể dừng trước khi kịp đăng ký
  // ctx.waitUntil(). Bản thân await ở đây rất nhanh (không đợi ghi D1 xong).
  await logDownload({ channel: "web", kenh: `phien-ban-cu:${phienBan}`, version: phienBan, request });

  return Response.redirect(`${GITHUB_RAW_BASE}/Release/${entry.download.fileName}`, 301);
}
