import { defineCloudflareConfig } from "@opennextjs/cloudflare";

/**
 * Cấu hình cho bộ chuyển đổi OpenNext, dùng khi triển khai lên Cloudflare Workers.
 *
 * Để mặc định là đủ cho trang này: không có bộ nhớ đệm phân tán, không hàng đợi.
 * Khi nào gắn CMS và muốn bài viết tự dựng lại (ISR) trên toàn cầu thì mới cần
 * khai báo thêm incrementalCache bằng R2 hoặc KV ở đây.
 */
export default defineCloudflareConfig();
