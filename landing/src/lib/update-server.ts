import { GITHUB_RAW_BASE, QLGX_BIN_RAW_BASE } from "@/lib/github";

/**
 * Logic dùng chung cho MÁY CHỦ CẬP NHẬT của phần mềm desktop QLGX — khác hẳn
 * mục đích với `/api/tai-ve` (đó là nút tải trên trang web cho NGƯỜI dùng bấm
 * tay). Đây là API mà chính CHƯƠNG TRÌNH gọi tự động để tự kiểm tra bản mới.
 *
 * Hợp đồng đầy đủ nằm ở `HOP_DONG_MAY_CHU_CAP_NHAT.md` tại gốc kho `qlgx` —
 * đọc tài liệu đó trước khi sửa bất cứ gì ở đây. Vài điểm mấu chốt nhắc lại:
 *
 * - Phần mềm đã phát hành, KHÔNG sửa được nữa. Đây là hợp đồng một chiều.
 * - `<downloadpath>` trong VersionConfig.xml sẽ THAY THẾ vĩnh viễn địa chỉ gốc
 *   trên máy người dùng sau lần cập nhật kế tiếp — sai là hỏng vĩnh viễn.
 * - Phải giữ các đường dẫn CŨ (/version.txt, /VersionConfig.xml,
 *   /download.asp, /help/thong_tin_cap_nhat.htm ở gốc và dưới /4.0/) sống mãi
 *   cho những máy đời trước — xem các route dùng lại hàm ở đây.
 * - Các đường dẫn CŨ phải trả lời qua http:// thuần, không được ép https.
 */

const VERSION_CONFIG_URL = `${GITHUB_RAW_BASE}/BIN/VersionConfig.xml`;
const CHANGELOG_HTML_URL = `${GITHUB_RAW_BASE}/BIN/help/thong_tin_cap_nhat.htm`;
const CACHE_TTL_SECONDS = 300;

/**
 * Cache API gốc của Cloudflare Worker — KHÁC với cache của Next.js
 * (`fetch(..., { next: { revalidate } })`), thứ không có tác dụng gì trên
 * Workers vì dự án này không cấu hình `incrementalCache`. Đã từng làm nút tải
 * trên trang chủ chậm hẳn vì gọi GitHub ở mọi lượt bấm — xem README, mục
 * "nút tải chậm". Ở đây quan trọng hơn nhiều: version.txt bị GỌI Ở MỖI LẦN MỞ
 * CHƯƠNG TRÌNH bởi mọi máy đã cài QLGX, không cache là dội GitHub liên tục.
 */
function getEdgeCache(): Cache | null {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return "caches" in globalThis ? (globalThis.caches as any).default : null;
}

async function fetchGithubTextCached(url: string): Promise<string> {
  const cache = getEdgeCache();
  const cacheKey = new Request(url);

  if (cache) {
    const hit = await cache.match(cacheKey);
    if (hit) return hit.text();
  }

  const res = await fetch(url, {
    headers: { "User-Agent": "quanlygiaoxu-capnhat" },
    signal: AbortSignal.timeout(5000),
  });
  if (!res.ok) {
    throw new Error(`GitHub trả về ${res.status} khi tải ${url}`);
  }
  const text = await res.text();

  if (cache) {
    await cache.put(
      cacheKey,
      new Response(text, {
        headers: { "Cache-Control": `public, max-age=${CACHE_TTL_SECONDS}` },
      }),
    );
  }
  return text;
}

/** Nội dung VersionConfig.xml mới nhất, y hệt tệp cài kèm phần mềm — chép nguyên xi. */
export async function getVersionConfigXml(): Promise<string> {
  return fetchGithubTextCached(VERSION_CONFIG_URL);
}

/** Trang HTML ghi chú phát hành, mở khi người dùng bấm "Xem chi tiết". */
export async function getChangelogHtml(): Promise<string> {
  return fetchGithubTextCached(CHANGELOG_HTML_URL);
}

const VERSION_VALUE_RE = /<version-info\b[^>]*\bvalue="([^"]+)"/;
const VERSION_DISPLAY_RE = /<version-info\b[^>]*\bdisplay="([^"]+)"/;

/**
 * Số phiên bản 4 phần (ví dụ "4.0.2.0") cho `version.txt` — lấy từ thuộc tính
 * `value`, KHÔNG phải `display` (3 phần, "4.0.2", chỉ để hiển thị).
 */
export async function getVersionText(): Promise<string> {
  const xml = await getVersionConfigXml();
  const match = xml.match(VERSION_VALUE_RE);
  if (!match) {
    throw new Error("Không tìm thấy version-info/@value trong VersionConfig.xml");
  }
  // .trim(): phòng xa nếu XML nguồn có khoảng trắng thừa quanh giá trị —
  // version.txt tuyệt đối không được có gì ngoài đúng bốn phần số.
  return match[1].trim();
}

/** Tên tệp gói cập nhật trong kho qlgx_bin, ví dụ "qlgx_4_0_2_update.zip". */
export async function getUpdateZipUrl(): Promise<string> {
  const xml = await getVersionConfigXml();
  const display = xml.match(VERSION_DISPLAY_RE)?.[1];
  if (!display) {
    throw new Error("Không tìm thấy version-info/@display trong VersionConfig.xml");
  }
  const fileName = `qlgx_${display.replaceAll(".", "_")}_update.zip`;
  return `${QLGX_BIN_RAW_BASE}/Release/update/${fileName}`;
}

/* ------------------------------------------------------------------ */
/* Handler dựng sẵn — mỗi route.ts (đường dẫn mới lẫn đường dẫn cũ) chỉ cần
 * re-export đúng một trong các hàm này làm GET. Nội dung phải Y HỆT nhau giữa
 * /capnhat/* và các đường dẫn cũ (/version.txt, /4.0/version.txt, ...), viết
 * một chỗ để không bao giờ lệch nhau khi sửa sau này. */
/* ------------------------------------------------------------------ */

function errorResponse(err: unknown): Response {
  const message = err instanceof Error ? err.message : "Lỗi không xác định";
  console.error("Máy chủ cập nhật lỗi:", message);
  return new Response(message, { status: 502 });
}

export async function versionTextResponse(): Promise<Response> {
  try {
    const version = await getVersionText();
    return new Response(version, {
      headers: { "Content-Type": "text/plain; charset=utf-8" },
    });
  } catch (err) {
    return errorResponse(err);
  }
}

export async function versionConfigXmlResponse(): Promise<Response> {
  try {
    const xml = await getVersionConfigXml();
    return new Response(xml, {
      headers: { "Content-Type": "application/xml; charset=utf-8" },
    });
  } catch (err) {
    return errorResponse(err);
  }
}

export async function changelogHtmlResponse(): Promise<Response> {
  try {
    const html = await getChangelogHtml();
    return new Response(html, {
      headers: { "Content-Type": "text/html; charset=utf-8" },
    });
  } catch (err) {
    return errorResponse(err);
  }
}

/**
 * 302 sang tệp .zip thật trong kho qlgx_bin. Không dùng 301: phiên bản đổi
 * theo thời gian, máy phải hỏi lại đường dẫn ở lần sau chứ không được nhớ
 * vĩnh viễn (khác với `/api/tai-ve/phien-ban-cu` — đường dẫn đó GHIM một
 * phiên bản cụ thể nên 301 mới đúng).
 */
export async function downloadUpdateResponse(): Promise<Response> {
  try {
    const zipUrl = await getUpdateZipUrl();
    return Response.redirect(zipUrl, 302);
  } catch (err) {
    return errorResponse(err);
  }
}
