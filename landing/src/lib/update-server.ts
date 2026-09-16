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

/**
 * Chi tiết lỗi (đường dẫn raw.githubusercontent, tên biến môi trường) chỉ vào
 * log của Worker; người gọi ẩn danh chỉ nhận một câu chung — finding T-4.
 *
 * Mã trạng thái GIỮ NGUYÊN 502 và thân vẫn là văn bản thuần: phần mềm desktop
 * cũ chỉ phân biệt "tải được / không tải được", không đọc nội dung thân lỗi.
 */
function errorResponse(err: unknown): Response {
  console.error("Máy chủ cập nhật lỗi:", err instanceof Error ? err.stack ?? err.message : err);
  return new Response("May chu cap nhat tam thoi khong san sang.", {
    status: 502,
    headers: { "content-type": "text/plain; charset=utf-8" },
  });
}

/**
 * Địa chỉ gốc theo từng nhóm đường dẫn — PHẢI khớp với "Các địa chỉ cũ" trong
 * HOP_DONG_MAY_CHU_CAP_NHAT.md. Đây là giá trị sẽ được ghi đè vĩnh viễn vào
 * máy người dùng qua `<downloadpath>` (xem chú thích ở đầu file), nên mỗi địa
 * chỉ ở đây phải là địa chỉ mà chính nhóm máy đó ĐANG dùng để gọi tới route
 * này — không được trỏ sang nhóm khác.
 *
 * "goc" và "4.0" cố tình giữ `http://`: máy chạy bản 3.3.7 trở về trước và
 * 4.0.0–4.0.1 dùng .NET Framework 2.0/4.0 trên Windows XP–7, không thương
 * lượng được TLS 1.2 — xác nhận bằng cách đọc thật code cũ (`AutoUpdate.cs`,
 * `CMemory.cs` tại tag release-3.3.7-net20/release-3.7.7-net20): nội dung bên
 * trong thẻ `<downloadpath>` được gán thẳng vào `Memory.ServerUrl`/
 * `information.ServerUrl`, dùng để tải mọi thứ sau đó (version.txt lần sau,
 * chính VersionConfig.xml, và ghép với thuộc tính `value="download-update"`
 * để tải gói .zip) — ép https ở đây chính là nguyên nhân của lỗi "báo có bản
 * mới nhưng cập nhật luôn thất bại" đã sửa trước đó.
 */
const DOWNLOAD_BASE_URL_FOR = {
  goc: "http://quanlygiaoxu.net/",
  "4.0": "http://quanlygiaoxu.net/4.0/",
  capnhat: "https://quanlygiaoxu.net/capnhat/",
} as const;

export type NhomDuongDan = keyof typeof DOWNLOAD_BASE_URL_FOR;

/**
 * TẠM DỪNG BÁO CÓ BẢN MỚI CHO TỪNG NHÓM — bật lại 2026-09-13 riêng cho "goc"
 * (máy 3.3.7 trở về trước).
 *
 * LÝ DO: bản 4.0.2 chạy trên .NET Framework 4.8 (nâng cấp từ .NET 2.0 ở bản
 * 4.0.0 — xem CLAUDE.md). Máy cài từ 3.3.7 trở về trước có thể vẫn là Windows 7
 * KHÔNG có sẵn .NET 4.8 (chỉ có .NET 2.0/4.0 đủ để chạy bản cũ) — nếu máy đó tự
 * cập nhật lên 4.0.2, gói cài xong có thể không chạy được, biến một phần mềm
 * đang dùng tốt thành hỏng hẳn. Đây là hợp đồng một chiều (xem đầu file) nên
 * KHÔNG được để xảy ra rồi mới sửa — phải chặn từ trước khi có giải pháp cho
 * nhóm máy này (ví dụ: gói cài kèm sẵn bộ cài .NET 4.8, hoặc kiểm tra phiên
 * bản .NET trước khi cho phép cập nhật).
 *
 * Đã xác nhận bằng cách đọc code máy khách (`GxCheckVersion.cs`,
 * `Validator.IsNumber()`): version.txt rỗng khiến `int.Parse("")` ném lỗi,
 * `CheckNewVersion()` return sớm TRƯỚC cả đoạn hiện hộp thoại "không có bản
 * mới" — nghĩa là chặn được CẢ kiểm tra tự động lúc mở chương trình LẪN bấm
 * nút "Kiểm tra cập nhật" thủ công, không có ngoại lệ nào lọt qua.
 *
 * "4.0" và "capnhat" KHÔNG tạm dừng: máy 4.0.0 trở lên đã tự chạy trên .NET 4.8
 * rồi (đang chạy được nghĩa là máy đã có sẵn), nên cập nhật lên 4.0.2 không có
 * rủi ro tương thích mới nào so với rủi ro vốn đã chấp nhận từ bản 4.0.0.
 */
const TAM_DUNG_CAP_NHAT_CHO_NHOM: Partial<Record<NhomDuongDan, true>> = {
  goc: true,
};

export async function versionTextResponse(kenh: NhomDuongDan): Promise<Response> {
  if (TAM_DUNG_CAP_NHAT_CHO_NHOM[kenh]) {
    return new Response("", { headers: { "Content-Type": "text/plain; charset=utf-8" } });
  }

  try {
    const version = await getVersionText();
    return new Response(version, {
      headers: { "Content-Type": "text/plain; charset=utf-8" },
    });
  } catch (err) {
    return errorResponse(err);
  }
}

const DOWNLOADPATH_TAG_RE = /(<downloadpath\b[^>]*>)([^<]*)(<\/downloadpath>)/;

/**
 * Thay ĐÚNG phần địa chỉ (nội dung bên trong thẻ) của `<downloadpath>`, giữ
 * nguyên thuộc tính `value="download-update"` — thuộc tính đó không đổi theo
 * nhóm đường dẫn, chỉ có địa chỉ gốc mới cần khác nhau (xem `AutoUpdate.cs`:
 * `node.InnerText + node.Attributes["value"].Value`).
 */
function withDownloadBaseUrl(xml: string, baseUrl: string): string {
  if (!DOWNLOADPATH_TAG_RE.test(xml)) {
    throw new Error("Không tìm thấy thẻ <downloadpath> trong VersionConfig.xml");
  }
  return xml.replace(DOWNLOADPATH_TAG_RE, (_match, open, _oldUrl, close) => `${open}${baseUrl}${close}`);
}

export async function versionConfigXmlResponse(kenh: NhomDuongDan): Promise<Response> {
  try {
    const xml = await getVersionConfigXml();
    const xmlChoNhom = withDownloadBaseUrl(xml, DOWNLOAD_BASE_URL_FOR[kenh]);
    return new Response(xmlChoNhom, {
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
