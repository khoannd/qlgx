import type { Metadata, Viewport } from "next";
import { Inter, Lora } from "next/font/google";

import { SiteJsonLd } from "@/components/JsonLd";
import { SITE_URL, site } from "@/lib/site";
import "./globals.css";

/**
 * Cả hai font đều nạp kèm subset `vietnamese`.
 *
 * Lora: chữ có chân ấm áp, độ tương phản nét vừa phải nên dấu tiếng Việt (ế, ữ,
 * ậ) hiển thị dày dặn, rõ ràng ở cỡ tiêu đề lớn — khác với Cormorant Garamond
 * (quá mảnh, dấu bị yếu) hay Fraunces bật trục SOFT/WONK (nét cong phá cách,
 * dễ đọc nhầm dấu ở một số tổ hợp). Chọn sau khi dựng bảng so sánh trực tiếp
 * nhiều font ở đúng cỡ chữ và đúng câu tiêu đề dùng thật trên trang.
 * Inter: trung tính, dấu rõ ràng ở cỡ nhỏ, không tranh chấp với phần tiêu đề.
 *
 * next/font tự host font ngay trên domain của trang nên không có request sang
 * fonts.googleapis.com — nhanh hơn, không nhảy chữ và không lộ thông tin người truy cập.
 */
const headingFont = Lora({
  subsets: ["latin", "vietnamese"],
  weight: ["500", "600", "700"],
  // Biến vẫn tên "--font-fraunces" để không phải sửa lại globals.css — chỉ đổi
  // font đứng sau, không đổi tên biến.
  variable: "--font-fraunces",
  display: "swap",
});

const inter = Inter({
  subsets: ["latin", "vietnamese"],
  variable: "--font-inter",
  display: "swap",
});

const title = `${site.name} — Phần mềm quản lý giáo dân, gia đình và sổ bí tích`;

export const metadata: Metadata = {
  metadataBase: new URL(SITE_URL),
  title: {
    default: title,
    template: `%s | ${site.name}`,
  },
  description: site.description,
  applicationName: site.name,
  keywords: [
    "quản lý giáo xứ",
    "phần mềm quản lý giáo xứ",
    "phần mềm giáo xứ",
    "quản lý giáo dân",
    "sổ bí tích",
    "sổ rửa tội",
    "rao hôn phối",
    "quản lý giáo lý",
    "phần mềm công giáo",
    "QLGX",
  ],
  authors: [{ name: site.name, url: SITE_URL }],
  creator: site.name,
  publisher: site.name,
  alternates: { canonical: "/" },
  openGraph: {
    type: "website",
    locale: "vi_VN",
    url: SITE_URL,
    siteName: site.name,
    title,
    description: site.description,
    images: [
      {
        url: "/images/gia_dinh.jpg",
        width: 1077,
        height: 703,
        alt: `Danh sách gia đình trong phần mềm ${site.name}`,
      },
    ],
  },
  twitter: {
    card: "summary_large_image",
    title,
    description: site.description,
    images: ["/images/gia_dinh.jpg"],
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      "max-image-preview": "large",
      "max-snippet": -1,
      "max-video-preview": -1,
    },
  },
  category: "software",
};

export const viewport: Viewport = {
  themeColor: "#fbf9f4",
  width: "device-width",
  initialScale: 1,
  // Không đặt maximumScale/userScalable: người dùng phải luôn phóng to được trang.
};

/**
 * Gắn class `js` lên <html> trước lượt vẽ đầu tiên. CSS chỉ ẩn các khối `.reveal`
 * khi class này có mặt, nhờ đó trang không bao giờ trắng nội dung nếu JS lỗi.
 */
const enableJsClass = `document.documentElement.classList.add('js')`;

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    // suppressHydrationWarning: script bên dưới gắn thêm class `js` vào thẻ
    // này trước khi React thuỷ hoá — React đúng khi thấy HTML client và server
    // lệch nhau ở đúng một chỗ đó, nhưng đây là chủ đích, không phải lỗi.
    <html
      lang="vi"
      className={`${headingFont.variable} ${inter.variable}`}
      suppressHydrationWarning
    >
      <head>
        <script dangerouslySetInnerHTML={{ __html: enableJsClass }} />
        <SiteJsonLd />
      </head>
      <body>{children}</body>
    </html>
  );
}
