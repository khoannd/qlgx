import type { MetadataRoute } from "next";

import { SITE_URL } from "@/lib/site";

export default function robots(): MetadataRoute.Robots {
  return {
    rules: [
      {
        userAgent: "*",
        allow: "/",
        // /capnhat, /version.txt, /VersionConfig.xml, /download.asp, /help,
        // /4.0 — máy chủ cập nhật cho phần mềm desktop, không phải nội dung
        // cho người đọc hay công cụ tìm kiếm. Xem HOP_DONG_MAY_CHU_CAP_NHAT.md.
        disallow: [
          "/api/",
          "/admin",
          "/capnhat/",
          "/version.txt",
          "/VersionConfig.xml",
          "/download.asp",
          "/help/",
          "/4.0/",
        ],
      },
    ],
    sitemap: `${SITE_URL}/sitemap.xml`,
    host: SITE_URL,
  };
}
