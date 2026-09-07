import type { Article, FaqItem, Release } from "@/lib/content";
import { FORUM_URL, SITE_URL, site } from "@/lib/site";

/**
 * Dữ liệu có cấu trúc (schema.org) cho công cụ tìm kiếm.
 *
 * Toàn bộ nội dung lấy từ cùng nguồn với những gì người dùng nhìn thấy trên trang,
 * nên không bao giờ lệch nhau — Google phạt nặng lỗi này.
 */

function JsonLdScript({ data }: { data: unknown }) {
  return (
    <script
      type="application/ld+json"
      // Thoát dấu "<" để một chuỗi do CMS cung cấp không thể đóng thẻ script sớm.
      dangerouslySetInnerHTML={{
        __html: JSON.stringify(data).replace(/</g, "\\u003c"),
      }}
    />
  );
}

/** Khai báo dùng chung cho mọi trang: trang web và tổ chức. */
export function SiteJsonLd() {
  return (
    <JsonLdScript
      data={{
        "@context": "https://schema.org",
        "@graph": [
          {
            "@type": "WebSite",
            "@id": `${SITE_URL}/#website`,
            url: `${SITE_URL}/`,
            name: site.name,
            description: site.description,
            inLanguage: "vi-VN",
          },
          {
            "@type": "Organization",
            "@id": `${SITE_URL}/#organization`,
            name: site.name,
            url: `${SITE_URL}/`,
            sameAs: [FORUM_URL],
          },
        ],
      }}
    />
  );
}

/** Trang chủ: mô tả phần mềm tải về được và danh sách câu hỏi thường gặp. */
export function HomeJsonLd({ release, faqs }: { release: Release; faqs: FaqItem[] }) {
  return (
    <JsonLdScript
      data={{
        "@context": "https://schema.org",
        "@graph": [
          {
            "@type": "SoftwareApplication",
            "@id": `${SITE_URL}/#software`,
            name: site.name,
            alternateName: site.shortName,
            applicationCategory: "BusinessApplication",
            applicationSubCategory: "Phần mềm quản lý giáo xứ",
            operatingSystem: "Windows 7, Windows 8, Windows 10, Windows 11",
            softwareVersion: release.version,
            datePublished: release.publishedAt,
            releaseNotes: release.summary,
            downloadUrl: release.downloads.find((d) => d.primary)?.href,
            installUrl: `${SITE_URL}/#tai-ve`,
            softwareRequirements:
              ".NET Framework 4.8; Microsoft Word hoặc Microsoft Excel để in ấn",
            inLanguage: "vi-VN",
            description: site.description,
            screenshot: `${SITE_URL}/images/gia_dinh.jpg`,
            featureList: [
              "Quản lý hồ sơ giáo dân",
              "Quản lý gia đình và giáo họ",
              "Sổ bí tích: Rửa tội, Thêm sức, Hôn phối, Qua đời",
              "Rao và điều tra hôn phối",
              "Quản lý lớp giáo lý và hội đoàn",
              "Thống kê và báo cáo cho giáo phận",
              "In ấn trên Microsoft Word và Excel",
              "Sao lưu và khôi phục dữ liệu",
              "Phân quyền người dùng",
            ],
            offers: {
              "@type": "Offer",
              price: "0",
              priceCurrency: "VND",
              availability: "https://schema.org/InStock",
            },
            isAccessibleForFree: true,
            url: `${SITE_URL}/`,
          },
          {
            "@type": "FAQPage",
            "@id": `${SITE_URL}/#faq`,
            mainEntity: faqs.map((item) => ({
              "@type": "Question",
              name: item.question,
              acceptedAnswer: { "@type": "Answer", text: item.answer.join(" ") },
            })),
          },
        ],
      }}
    />
  );
}

/** Trang chi tiết bài viết. */
export function ArticleJsonLd({ article }: { article: Article }) {
  const url = `${SITE_URL}/tin-tuc/${article.slug}`;
  return (
    <JsonLdScript
      data={{
        "@context": "https://schema.org",
        "@type": "Article",
        "@id": `${url}#article`,
        headline: article.title,
        description: article.excerpt,
        datePublished: article.publishedAt,
        dateModified: article.updatedAt ?? article.publishedAt,
        articleSection: article.category.name,
        inLanguage: "vi-VN",
        mainEntityOfPage: { "@type": "WebPage", "@id": url },
        author: { "@type": "Organization", name: article.author ?? site.name },
        publisher: { "@id": `${SITE_URL}/#organization` },
      }}
    />
  );
}

/** Đường dẫn phân cấp, giúp kết quả tìm kiếm hiển thị dạng breadcrumb. */
export function BreadcrumbJsonLd({ items }: { items: { name: string; url: string }[] }) {
  return (
    <JsonLdScript
      data={{
        "@context": "https://schema.org",
        "@type": "BreadcrumbList",
        itemListElement: items.map((item, index) => ({
          "@type": "ListItem",
          position: index + 1,
          name: item.name,
          item: item.url,
        })),
      }}
    />
  );
}
