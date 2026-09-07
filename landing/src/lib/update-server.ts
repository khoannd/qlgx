import { logDownload } from "@/lib/download-log";
import { QLGX_BIN_RAW_BASE } from "@/lib/github";

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
 *
 * NGUỒN — đọc từ `qlgx_bin`, KHÔNG phải `BIN/` của kho `qlgx`: `BIN/` là thư
 * mục build, đổi liên tục khi phát triển (mỗi lần build local đều ghi đè).
 * Nếu API đọc thẳng từ đó, một commit `BIN/VersionConfig.xml` bình thường lúc
 * đang phát triển (chưa hề có ý định phát hành) sẽ lập tức khiến MỌI máy đã
 * cài QLGX nhận thông báo "có bản mới" — dù file `.zip` tương ứng còn chưa có.
 * `qlgx_bin` chỉ nhận commit đúng lúc phát hành thật (xem lịch sử commit của
 * repo đó: toàn "Phat hanh x.y.z"), nên ổn định hơn hẳn làm nguồn cho API
 * công khai. Từ nay, phát hành bản mới phải CHÉP `Release/VersionConfig.xml`
 * và `Release/thong_tin_cap_nhat.htm` từ kho `qlgx` sang `qlgx_bin` rồi mới
 * commit — xem `QUY_TRINH_PHAT_HANH.md` mục 5.1.
 */

const VERSION_CONFIG_URL = `${QLGX_BIN_RAW_BASE}/Release/VersionConfig.xml`;
const CHANGELOG_HTML_URL = `${QLGX_BIN_RAW_BASE}/Release/thong_tin_cap_nhat.htm`;
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

/**
 * GitHub raw phục vụ file theo xuống dòng Unix (`\n`) — do `core.autocrlf` của
 * git chuẩn hoá CRLF thành LF lúc commit, nên blob lưu trên GitHub luôn là LF,
 * bất kể file gốc trên máy Windows là CRLF. Đã kiểm chứng thật: cài bản 4.0.2,
 * bấm "Kiểm tra phiên bản mới", hộp thoại hiện đúng nội dung nhưng MẤT HẾT
 * xuống dòng — dồn thành một khối chữ. Nguyên nhân: control hiển thị (kiểu
 * RichTextBox của WinForms) chỉ nhận `\r\n` làm dấu xuống dòng, `\n` đơn thuần
 * bị bỏ qua. Chuẩn hoá lại thành CRLF trước khi trả về — làm ở ĐÂY (một chỗ
 * duy nhất, ngay khi đọc xong) để version.txt/URL gói .zip suy ra từ cùng một
 * chuỗi đã chuẩn hoá, không lệch nhau.
 */
function toCrlf(text: string): string {
  return text.replace(/\r\n/g, "\n").replace(/\n/g, "\r\n");
}

/**
 * Bỏ dấu BOM (U+FEFF) ở đầu chuỗi nếu có. `Response.text()` trên Node (dùng
 * lúc chạy `next dev`/`next start` để thử ở máy) tự cắt BOM UTF-8, nhưng đã
 * kiểm chứng thật: trên Cloudflare Workers thì KHÔNG — cùng một đoạn mã, cùng
 * một file nguồn, response thật ở production có `EF BB BF` ở đầu còn bản thử
 * ở máy thì không. Không cắt tay ở đây thì tái diễn đúng lỗi BOM đã từng làm
 * hỏng phần mềm (`ï»¿`, xem HOP_DONG_MAY_CHU_CAP_NHAT.md) — lần này ở phía
 * server thay vì phía client.
 */
function stripBom(text: string): string {
  return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

/** Nội dung VersionConfig.xml mới nhất, y hệt tệp cài kèm phần mềm — chép nguyên xi. */
export async function getVersionConfigXml(): Promise<string> {
  const xml = await fetchGithubTextCached(VERSION_CONFIG_URL);
  return toCrlf(stripBom(xml));
}

/** Trang HTML ghi chú phát hành, mở khi người dùng bấm "Xem chi tiết". */
export async function getChangelogHtml(): Promise<string> {
  return stripBom(await fetchGithubTextCached(CHANGELOG_HTML_URL));
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
export async function downloadUpdateResponse(request: Request, kenh: string): Promise<Response> {
  try {
    const [zipUrl, version] = await Promise.all([getUpdateZipUrl(), getVersionText()]);
    // PHẢI await: logDownload() chỉ chờ tới lúc đăng ký xong ctx.waitUntil() rồi
    // trả về ngay — việc GHI D1 thật vẫn chạy nền, không chặn response. Awaited
    // hờ hững (`void logDownload(...)`) từng làm mất log thật: Worker trả lời
    // xong rồi có thể bị dừng trước khi hàm kịp gọi tới ctx.waitUntil().
    await logDownload({ channel: "app", kenh, version, request });
    return Response.redirect(zipUrl, 302);
  } catch (err) {
    return errorResponse(err);
  }
}
