import { createD1ContentProvider } from "./d1-provider";
import { staticContentProvider } from "./static-provider";
import type { ContentProvider } from "./types";
import { isRunningInsideCloudflareWorker } from "@/lib/cloudflare-runtime";

export * from "./types";
export { createD1ContentProvider } from "./d1-provider";

/**
 * Điểm nối duy nhất giữa giao diện và nguồn nội dung.
 *
 * Chọn nguồn theo môi trường đang chạy, không phải theo cấu hình tĩnh:
 *   - Đang thực sự chạy trong Cloudflare Worker (đã deploy thật, hoặc
 *     `wrangler dev`) VÀ đã gắn binding D1 tên `DB` → đọc/ghi từ D1. Đây là
 *     nguồn có thể sửa qua trang quản trị `/admin` mà không cần build lại.
 *   - Mọi trường hợp khác (`next dev`/`next build`/`next start` trên Node,
 *     đã deploy Cloudflare nhưng chưa tạo D1, HOẶC D1 trả lỗi thoáng qua) →
 *     dữ liệu tĩnh trong `static-provider.ts`. Đây cũng là nội dung dùng để
 *     tạo `migrations/0004_seed.sql`.
 *
 * QUAN TRỌNG — `isRunningInsideCloudflareWorker()` phải được gọi TRƯỚC khi
 * đụng tới `getCloudflareContext`, xem lý do đầy đủ tại
 * `src/lib/cloudflare-runtime.ts`: gọi thẳng `getCloudflareContext` từ một
 * tiến trình Node thường (next dev/build/start) khiến nó tự khởi động một
 * Miniflare con để mô phỏng D1, và trên Windows việc đó từng làm SẬP CỨNG cả
 * tiến trình Node (crash native, không phải lỗi JS) — không phải suy đoán, đã
 * xảy ra ba lần thật khi phát triển trang này.
 *
 * Ngoài ra mỗi lời gọi đều thử D1 lại từ đầu và có lưới đỡ RIÊNG cho lỗi D1
 * trả về (khác với crash tiến trình ở trên) — không cache "đã chọn D1 hay
 * chưa" cho cả vòng đời tiến trình, để một truy vấn lỗi thoáng qua không biến
 * thành "provider chính thức" cho mọi trang còn lại.
 */

async function getD1(): Promise<D1Database | null> {
  if (!isRunningInsideCloudflareWorker()) return null;
  try {
    const { getCloudflareContext } = await import("@opennextjs/cloudflare");
    const { env } = await getCloudflareContext({ async: true });
    return env.DB ?? null;
  } catch {
    return null;
  }
}

async function withD1Fallback<T>(
  run: (provider: ContentProvider) => Promise<T>,
): Promise<T> {
  const db = await getD1();
  if (db) {
    try {
      return await run(createD1ContentProvider(db));
    } catch (err) {
      console.error("Truy vấn D1 thất bại, dùng tạm dữ liệu tĩnh:", err);
    }
  }
  return run(staticContentProvider);
}

export const content: ContentProvider = {
  getRelease: () => withD1Fallback((p) => p.getRelease()),
  listArticles: (options) => withD1Fallback((p) => p.listArticles(options)),
  getArticle: (slug) => withD1Fallback((p) => p.getArticle(slug)),
  listArticleSlugs: () => withD1Fallback((p) => p.listArticleSlugs()),
  listCategories: () => withD1Fallback((p) => p.listCategories()),
  getLandingContent: () => withD1Fallback((p) => p.getLandingContent()),
};
