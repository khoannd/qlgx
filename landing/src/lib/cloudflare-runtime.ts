/**
 * Kiểm tra xem mã đang chạy có thực sự nằm TRONG một Cloudflare Worker hay không
 * (Worker thật đã deploy, hoặc `wrangler dev` — cả hai đều chạy đúng file
 * `worker.js` do OpenNext build ra), trước khi cho phép gọi tới D1.
 *
 * VÌ SAO CẦN HÀM NÀY — đã xảy ra thật, không phải phòng ngừa lý thuyết:
 *
 * `@opennextjs/cloudflare`'s `getCloudflareContext({ async: true })` có hai
 * nhánh hoàn toàn khác nhau:
 *   1. Nếu ngữ cảnh đã được worker.js gán sẵn lên global scope (trường hợp
 *      Worker thật / `wrangler dev`) → trả về ngay, đồng bộ, không tốn gì.
 *   2. Nếu KHÔNG (mọi trường hợp chạy Node thường: `next dev`, `next build`,
 *      `next start`) và `NEXT_RUNTIME === "nodejs"` (luôn đúng cho runtime mặc
 *      định) → tự động `import("wrangler")` rồi gọi `getPlatformProxy()`, thứ
 *      này KHỞI ĐỘNG MỘT TIẾN TRÌNH MINIFLARE/WORKERD CON để mô phỏng D1 bằng
 *      một tệp SQLite cục bộ trong `.wrangler/state`.
 *
 * Nhánh (2) là tính năng tiện lợi của `next dev` (chỉ nên bật khi chủ động gọi
 * `initOpenNextCloudflareForDev()` trong `next.config.ts` — dự án này KHÔNG
 * gọi hàm đó), nhưng vì không có gì chặn nên mọi lệnh gọi `getCloudflareContext`
 * từ code của ta đều rơi vào đó khi chạy `next build`/`next start` thường.
 *
 * Hậu quả trên Windows: nhiều tiến trình Node (dev, build, start, các lệnh
 * `wrangler d1 execute`) cùng lúc mở chung một tệp SQLite gây crash CỨNG ở
 * tầng native — `*** Fatal uncaught kj::Exception ... SQLITE_BUSY` — làm sập
 * toàn bộ tiến trình Node, KHÔNG bắt được bằng try/catch ở tầng JavaScript vì
 * đây không phải một exception JS. Đã gặp ba lần trong thực tế, không phải suy
 * đoán. `try/catch` quanh truy vấn D1 trong `content/index.ts` chỉ đỡ được lỗi
 * D1 trả về, không đỡ được kiểu crash tiến trình này.
 *
 * Cách chặn: tự kiểm tra symbol toàn cục mà worker.js dùng để gán ngữ cảnh
 * TRƯỚC khi gọi `getCloudflareContext` — nếu chưa có, coi như không có D1 và
 * dừng lại ngay, không bao giờ chạm tới nhánh (2) nguy hiểm ở trên.
 */
const CLOUDFLARE_CONTEXT_SYMBOL = Symbol.for("__cloudflare-context__");

export function isRunningInsideCloudflareWorker(): boolean {
  return CLOUDFLARE_CONTEXT_SYMBOL in globalThis;
}
