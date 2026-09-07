import { isRunningInsideCloudflareWorker } from "@/lib/cloudflare-runtime";

/**
 * Ghi lại một lượt tải/cập nhật THẬT (không phải lượt xem trang) để làm thống
 * kê sau này — xem migrations/0005_download_log.sql.
 *
 * Không chặn tốc độ tải: dùng `ctx.waitUntil()` để việc ghi D1 chạy song song,
 * response chuyển hướng (302) vẫn trả về ngay cho người/chương trình đang tải,
 * không phải đợi ghi log xong. Ghi log lỗi tuyệt đối không được làm hỏng lượt
 * tải thật — mọi lỗi đều nuốt lặng lẽ, chỉ log ra console để xem sau.
 *
 * An toàn khi chạy ngoài Cloudflare Worker (next dev/build/start): tự bỏ qua
 * ngay, không đụng tới `getCloudflareContext` — xem lý do đầy đủ tại
 * `src/lib/cloudflare-runtime.ts`.
 */
export async function logDownload(entry: {
  channel: "web" | "app";
  kenh: string;
  version: string;
  request: Request;
}): Promise<void> {
  if (!isRunningInsideCloudflareWorker()) return;

  try {
    const { getCloudflareContext } = await import("@opennextjs/cloudflare");
    const { env, ctx } = await getCloudflareContext({ async: true });
    if (!env.DB) return;

    const { channel, kenh, version, request } = entry;
    const write = env.DB.prepare(
      `INSERT INTO download_log (created_at, channel, kenh, version, ip, country, user_agent)
       VALUES (?, ?, ?, ?, ?, ?, ?)`,
    )
      .bind(
        Date.now(),
        channel,
        kenh,
        version,
        request.headers.get("CF-Connecting-IP"),
        request.headers.get("CF-IPCountry"),
        request.headers.get("User-Agent"),
      )
      .run()
      .catch((err) => {
        console.error("Ghi download_log thất bại:", err);
      });

    ctx.waitUntil(write);
  } catch (err) {
    console.error("Không ghi được download_log:", err);
  }
}
