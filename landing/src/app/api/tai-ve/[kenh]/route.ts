import { content } from "@/lib/content";
import { GITHUB_RAW_BASE } from "@/lib/github";

/**
 * Điểm tải về: /api/tai-ve/full và /api/tai-ve/update
 *
 * Trang web không bao giờ nhúng thẳng đường dẫn tệp. Nó luôn trỏ vào đây, còn
 * đây mới quyết định phiên bản mới nhất là bản nào rồi chuyển hướng sang đúng
 * tệp trên GitHub.
 *
 * QUAN TRỌNG — đã xác minh thực tế, không suy đoán: repo `khoannd/qlgx` là repo
 * thật của phần mềm này, nhưng KHÔNG có GitHub Releases nào được publish
 * (`/releases/latest` trả 404). Bản cài đặt thật nằm ngay trong thư mục
 * `Release/` của repo, dạng file thường (raw.githubusercontent.com), và
 * `Release/VersionConfig.xml` là nguồn xác định phiên bản mới nhất — chính tệp
 * này đã được chương trình desktop dùng để tự kiểm tra cập nhật từ trước tới nay.
 *
 * Thứ tự xác định phiên bản:
 *   1. QLGX_LATEST_VERSION — chốt cứng, dùng khi muốn ghim một bản cụ thể.
 *   2. VersionConfig.xml đọc trực tiếp từ nhánh chính trên GitHub — tự động
 *      khớp với repo, không cần sửa gì khi phát hành bản mới.
 *   3. Phiên bản đang hiển thị trên trang, để không bao giờ trả về lỗi trắng.
 *
 * Nếu sau này repo chuyển sang publish GitHub Releases thật (khuyên dùng về lâu
 * dài — xem README), sửa lại hàm `resolveDownloadUrl` để ưu tiên gọi
 * `/repos/{repo}/releases/latest` trước bước đọc VersionConfig.xml.
 */

const RAW_BASE = GITHUB_RAW_BASE;

/** Chuyển "4.0.0" thành "4_0_0" để ghép vào tên tệp phát hành. */
const toFileVersion = (version: string) => version.replaceAll(".", "_");

const ASSET_FOR: Record<string, (v: string) => string> = {
  full: (v) => `qlgx_${toFileVersion(v)}.exe`,
  update: (v) => `qlgx_${toFileVersion(v)}_update.zip`,
};

const VERSION_CHECK_URL = `${RAW_BASE}/Release/VersionConfig.xml`;
const VERSION_CACHE_TTL_SECONDS = 300;

/**
 * Đọc thuộc tính display="x.y.z" từ Release/VersionConfig.xml trên GitHub —
 * có nhớ tạm bằng Cache API gốc của Worker (`caches.default`).
 *
 * BẪY ĐÃ GẶP: tuỳ chọn `next: { revalidate: 300 }` (bộ nhớ đệm fetch của
 * Next.js) KHÔNG có tác dụng gì trên Cloudflare Workers — dự án này không cấu
 * hình `incrementalCache` trong open-next.config.ts (xem chú thích ở đó), nên
 * mỗi lượt bấm "Tải phần mềm" đều gọi thật sang GitHub trước khi chuyển hướng,
 * cộng thêm độ trễ thấy rõ trước khi hộp thoại lưu tệp hiện ra. Cache API của
 * chính Worker (khác hẳn cache của Next) luôn hoạt động bất kể có cấu hình gì
 * hay không, nên dùng thẳng nó ở đây.
 */
async function fetchVersionFromRepo(): Promise<string | null> {
  // `as any`: kiểu `CacheStorage` của lib DOM (nạp sẵn cho code React) không có
  // `.default` — đó là phần mở rộng riêng của Cloudflare Workers.
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const cache: Cache | null = "caches" in globalThis ? (globalThis.caches as any).default : null;
  const cacheKey = new Request(VERSION_CHECK_URL);

  if (cache) {
    const hit = await cache.match(cacheKey);
    if (hit) return (await hit.text()) || null;
  }

  try {
    const res = await fetch(VERSION_CHECK_URL, {
      headers: { "User-Agent": "quanlygiaoxu-landing" },
      // Bắt buộc phải có: nếu GitHub chậm hoặc mạng đang có vấn đề, người bấm
      // nút tải không được để quay vô hạn — 3 giây là đủ cho một yêu cầu HTTP
      // bình thường, hết giờ thì rơi xuống lớp dự phòng ngay bên dưới.
      signal: AbortSignal.timeout(3000),
    });
    if (!res.ok) return null;
    const xml = await res.text();
    const match = xml.match(/<version-info\b[^>]*\bdisplay="([^"]+)"/);
    const version = match?.[1] ?? null;

    if (cache && version) {
      // Lưu chuỗi phiên bản đã trích, không lưu nguyên XML — lần đọc sau khỏi
      // phải parse lại. `waitUntil` không cần thiết: .put() ở Cache API trả về
      // promise nhưng không chặn response, và ta đã await nó trước khi trả lời.
      await cache.put(
        cacheKey,
        new Response(version, {
          headers: { "Cache-Control": `public, max-age=${VERSION_CACHE_TTL_SECONDS}` },
        }),
      );
    }
    return version;
  } catch {
    // Mạng lỗi, hết giờ, hoặc GitHub chặn — vẫn còn lớp dự phòng bên dưới.
    return null;
  }
}

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ kenh: string }> },
) {
  const { kenh } = await params;
  const buildAssetName = ASSET_FOR[kenh];

  if (!buildAssetName) {
    return Response.json(
      { error: "Kênh tải không hợp lệ.", hop_le: Object.keys(ASSET_FOR) },
      { status: 404 },
    );
  }

  const version =
    process.env.QLGX_LATEST_VERSION?.trim() ||
    (await fetchVersionFromRepo()) ||
    (await content.getRelease()).version;

  const downloadUrl = `${RAW_BASE}/Release/${buildAssetName(version)}`;

  // 302: người dùng luôn đi qua endpoint này nên lần sau vẫn nhận được bản mới
  // nhất. Dùng 301 thì trình duyệt nhớ vĩnh viễn và sẽ tải mãi bản cũ.
  return Response.redirect(downloadUrl, 302);
}
