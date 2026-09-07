/**
 * Bộ nạp ảnh dùng khi triển khai lên Cloudflare Workers.
 *
 * VÌ SAO CẦN: bộ tối ưu ảnh có sẵn của Next.js dựa vào thư viện `sharp`, vốn là
 * mã máy nên không chạy được trên Workers. Nếu để nguyên, đường dẫn
 * /_next/image vẫn trả về 200 nhưng là ẢNH GỐC, không thu nhỏ và không đổi sang
 * AVIF/WebP — tốn băng thông mà không ai biết.
 *
 * CÁCH XỬ LÝ: đẩy việc thu nhỏ và đổi định dạng sang chính hạ tầng của
 * Cloudflare qua đường dẫn /cdn-cgi/image/.
 *
 * Bật bằng biến môi trường NEXT_PUBLIC_CF_IMAGES=1 khi build, vì tính năng này
 * cần được kích hoạt cho tên miền trong bảng điều khiển Cloudflare
 * (Images → Transformations). Chưa bật thì bộ nạp trả về đúng đường dẫn gốc:
 * ảnh trong thư mục public đã được thu nhỏ và nén sẵn từ khâu chuẩn bị nên vẫn
 * dùng tốt, chỉ là không có thêm lớp tối ưu theo từng kích thước màn hình.
 */

type LoaderArgs = {
  src: string;
  width: number;
  quality?: number;
};

const ENABLED = process.env.NEXT_PUBLIC_CF_IMAGES === "1";

export default function cloudflareImageLoader({ src, width, quality }: LoaderArgs): string {
  // Ảnh ở máy chủ khác thì để nguyên, Cloudflare chỉ biến đổi ảnh cùng tên miền.
  if (!ENABLED || /^https?:\/\//i.test(src)) return src;

  const options = [`width=${width}`, `quality=${quality ?? 82}`, "format=auto", "fit=scale-down"];
  return `/cdn-cgi/image/${options.join(",")}${src}`;
}
