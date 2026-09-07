/// <reference types="@cloudflare/workers-types" />

/**
 * Mở rộng `CloudflareEnv` (khai báo bởi @opennextjs/cloudflare) để thêm các
 * biến ràng buộc riêng của dự án này: cơ sở dữ liệu D1 chứa nội dung có thể
 * sửa qua trang quản trị, và mật khẩu quản trị.
 */
declare global {
  interface CloudflareEnv {
    /** Cơ sở dữ liệu D1 chứa bản phát hành và bài viết — xem migrations/0001_init.sql */
    DB?: D1Database;
    /** Mật khẩu đăng nhập /admin. Đặt bằng `wrangler secret put ADMIN_PASSWORD`. */
    ADMIN_PASSWORD?: string;
  }
}

export {};
