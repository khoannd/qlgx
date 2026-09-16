import { cookies } from "next/headers";

import { isRunningInsideCloudflareWorker } from "@/lib/cloudflare-runtime";

/**
 * Xác thực cho trang quản trị /admin.
 *
 * Cố ý đơn giản cho một công cụ nội bộ dùng bởi một người: một mật khẩu duy
 * nhất (biến bí mật `ADMIN_PASSWORD`, đặt bằng `wrangler secret put`), phiên
 * đăng nhập lưu token ngẫu nhiên trong D1 và cookie httpOnly.
 *
 * Chống dò mật khẩu gồm hai phần, PHẢI có đủ cả hai:
 *   1. Trong mã (đã làm): mỗi lần sai bị giữ lại một khoảng cố định và được ghi
 *      `console.warn` kèm IP — xem `src/app/admin/api/login/route.ts`.
 *   2. Trên dashboard Cloudflare (việc của chủ dự án, KHÔNG làm được bằng mã ở
 *      đây): Rate Limiting Rule cho `POST /admin/api/login` (gợi ý 5 lần/10
 *      phút/IP) và `ADMIN_PASSWORD` dài ≥ 24 ký tự ngẫu nhiên. Riêng phần (1)
 *      KHÔNG đủ: Worker chạy song song nhiều instance nên độ trễ chỉ làm chậm
 *      một luồng, không chặn được kẻ bắn song song.
 *
 * Giới hạn đã biết, chấp nhận được ở quy mô này: không CSRF token riêng (giảm
 * nhẹ một phần nhờ cookie SameSite=Lax — trình duyệt không gửi cookie này khi
 * có POST bắt nguồn từ trang khác).
 *
 * `/admin` chỉ hoạt động khi thực sự chạy trong Cloudflare Worker (đã deploy
 * thật, hoặc `wrangler dev`) — không hoạt động trên `next dev`/`next start`
 * thường vì không có D1 ở đó. `getDb()`/`verifyPassword()` kiểm tra điều này
 * TRƯỚC khi đụng tới `getCloudflareContext`, xem lý do đầy đủ tại
 * `src/lib/cloudflare-runtime.ts` — gọi thẳng hàm đó từ Node thường từng làm
 * sập cứng cả tiến trình trên Windows, không phải giả thuyết.
 */

export const SESSION_COOKIE = "qlgx_admin_session";
const SESSION_TTL_MS = 12 * 60 * 60 * 1000; // 12 giờ

/**
 * So sánh hai chuỗi với thời gian không đổi.
 *
 * Băm SHA-256 cả hai rồi so đúng 32 byte: số vòng lặp luôn như nhau, không phụ
 * thuộc độ dài hay nội dung của mật khẩu thật. Bản trước so trực tiếp từng
 * byte, nên nhánh "khác độ dài" lặp theo độ dài mật khẩu THẬT — rò rỉ độ dài
 * qua thời gian (finding T-6).
 */
async function timingSafeEqual(a: string, b: string): Promise<boolean> {
  const enc = new TextEncoder();
  const [bamA, bamB] = await Promise.all([
    crypto.subtle.digest("SHA-256", enc.encode(a)),
    crypto.subtle.digest("SHA-256", enc.encode(b)),
  ]);
  const x = new Uint8Array(bamA);
  const y = new Uint8Array(bamB);
  let diff = 0;
  for (let i = 0; i < x.length; i++) diff |= x[i]! ^ y[i]!;
  return diff === 0;
}

/** Lấy D1 binding từ ngữ cảnh Cloudflare hiện tại. Ném lỗi rõ ràng nếu chưa cấu hình. */
export async function getDb(): Promise<D1Database> {
  if (!isRunningInsideCloudflareWorker()) {
    throw new Error(
      "/admin chỉ chạy được khi triển khai thật trên Cloudflare Workers hoặc qua `wrangler dev` " +
        "— không chạy trên `next dev`/`next start` thường vì không có D1 ở đó. Dùng `npm run cf:preview`.",
    );
  }
  const { getCloudflareContext } = await import("@opennextjs/cloudflare");
  const { env } = await getCloudflareContext({ async: true });
  if (!env.DB) {
    throw new Error(
      "Chưa gắn D1 (binding `DB`). Xem README, mục 'Trang quản trị' để cấu hình wrangler.jsonc và chạy migrations.",
    );
  }
  return env.DB;
}

export async function verifyPassword(password: string): Promise<boolean> {
  if (!isRunningInsideCloudflareWorker()) {
    throw new Error(
      "/admin chỉ chạy được khi triển khai thật trên Cloudflare Workers hoặc qua `wrangler dev` " +
        "— không chạy trên `next dev`/`next start` thường vì không có D1 ở đó. Dùng `npm run cf:preview`.",
    );
  }
  const { getCloudflareContext } = await import("@opennextjs/cloudflare");
  const { env } = await getCloudflareContext({ async: true });
  const expected = env.ADMIN_PASSWORD;
  if (!expected) {
    throw new Error(
      "Chưa đặt biến bí mật ADMIN_PASSWORD. Chạy: npx wrangler secret put ADMIN_PASSWORD",
    );
  }
  return timingSafeEqual(password, expected);
}

/**
 * Xoá mọi phiên đã quá hạn. Gọi mỗi lần đăng nhập thành công — bảng
 * `admin_sessions` trước đây chỉ bị xoá khi đúng token đó được dùng lại, nên
 * token của phiên bỏ quên nằm lại vô thời hạn (finding T-5).
 *
 * Thu hồi khẩn mọi phiên đang mở (ví dụ nghi bị lộ mật khẩu):
 *   npx wrangler d1 execute qlgx-content --remote --command "DELETE FROM admin_sessions"
 * Đổi `ADMIN_PASSWORD` KHÔNG tự huỷ phiên đang mở — phải chạy lệnh trên.
 */
export async function donPhienHetHan(db: D1Database): Promise<void> {
  await db
    .prepare("DELETE FROM admin_sessions WHERE created_at < ?")
    .bind(Date.now() - SESSION_TTL_MS)
    .run();
}

export async function createSession(db: D1Database): Promise<string> {
  const token = crypto.randomUUID();
  await db
    .prepare("INSERT INTO admin_sessions (token, created_at) VALUES (?, ?)")
    .bind(token, Date.now())
    .run();
  return token;
}

export async function destroySession(db: D1Database, token: string): Promise<void> {
  await db.prepare("DELETE FROM admin_sessions WHERE token = ?").bind(token).run();
}

async function isSessionValid(db: D1Database, token: string): Promise<boolean> {
  const row = await db
    .prepare("SELECT created_at FROM admin_sessions WHERE token = ?")
    .bind(token)
    .first<{ created_at: number }>();
  if (!row) return false;
  if (Date.now() - row.created_at > SESSION_TTL_MS) {
    await destroySession(db, token);
    return false;
  }
  return true;
}

/**
 * Kiểm tra phiên đăng nhập hiện tại. Trả về D1Database nếu hợp lệ, hoặc `null`
 * nếu chưa đăng nhập / phiên đã hết hạn — gọi nơi dùng tự quyết định chuyển
 * hướng tới /admin/login.
 */
export async function requireSession(): Promise<D1Database | null> {
  const jar = await cookies();
  const token = jar.get(SESSION_COOKIE)?.value;
  if (!token) return null;

  const db = await getDb();
  return (await isSessionValid(db, token)) ? db : null;
}
