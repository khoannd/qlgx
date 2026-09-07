import type { NextConfig } from "next";

/**
 * Đích triển khai. Các script cf:* đặt biến này thành "cloudflare".
 * Chạy thường (next dev / next build / next start) thì không đặt.
 */
const isCloudflare = process.env.DEPLOY_TARGET === "cloudflare";

const nextConfig: NextConfig = {
  reactStrictMode: true,

  images: isCloudflare
    ? {
        // Trên Cloudflare Workers không có `sharp`, nên bộ tối ưu có sẵn của
        // Next.js chỉ trả về ảnh gốc. Chuyển việc đó cho hạ tầng Cloudflare.
        // Xem src/lib/cloudflare-image-loader.ts.
        loader: "custom",
        loaderFile: "./src/lib/cloudflare-image-loader.ts",
      }
    : {
        formats: ["image/avif", "image/webp"],
        // Next 16 chỉ chấp nhận các mức chất lượng được liệt kê ở đây; giá trị
        // khác bị âm thầm bỏ qua và rơi về 75.
        qualities: [80, 82, 86],
      },

  // Header bảo mật cơ bản cho một trang công khai.
  async headers() {
    return [
      {
        source: "/:path*",
        headers: [
          { key: "X-Content-Type-Options", value: "nosniff" },
          { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
          { key: "X-Frame-Options", value: "SAMEORIGIN" },
        ],
      },
      {
        // Ảnh và font trong /public không bao giờ đổi nội dung mà không đổi tên,
        // nên cho trình duyệt giữ lại lâu.
        source: "/images/:path*",
        headers: [{ key: "Cache-Control", value: "public, max-age=31536000, immutable" }],
      },
    ];
  },
};

export default nextConfig;
