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
      /**
       * CSP hẹp, CHỈ cho các đường trang web (finding TB-5, làm một phần).
       *
       * CỐ Ý không liệt kê `script-src`/`style-src`/`img-src`: trang có một
       * `<script>` nội tuyến ở layout và Next.js còn chèn script bootstrap của
       * riêng nó, nên muốn siết `script-src` cho có ích thì phải phát nonce qua
       * middleware — mà middleware ở đây sẽ chặn ngang CẢ các đường máy chủ cập
       * nhật (`/version.txt`, `/VersionConfig.xml`, `/download.asp`, `/4.0/*`)
       * mà máy giáo xứ bản cũ phụ thuộc. Việc đó cần bàn và kiểm riêng, không
       * làm kèm ở đây.
       *
       * Bốn chỉ thị dưới đây thì áp được ngay, không thể làm hỏng trang: không
       * chỗ nào trong `src/` dùng <iframe>/<object>/<embed>, không có <base>,
       * và mọi <form> đều post về cùng gốc (đã kiểm bằng grep).
       *
       * `source` liệt kê TỪNG nhóm đường trang — TUYỆT ĐỐI không dùng
       * "/:path*" ở đây: các endpoint máy chủ cập nhật không nên nhận header lạ.
       */
      ...["/", "/tin-tuc/:path*", "/huong-dan/:path*", "/phien-ban", "/admin/:path*"].map(
        (source) => ({
          source,
          headers: [
            {
              key: "Content-Security-Policy",
              value: "frame-ancestors 'self'; object-src 'none'; base-uri 'self'; form-action 'self'",
            },
          ],
        }),
      ),
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
