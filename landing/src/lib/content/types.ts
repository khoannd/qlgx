/**
 * Kiểu dữ liệu cho phần nội dung thay đổi theo thời gian.
 *
 * Đây là hợp đồng giữa giao diện và nguồn dữ liệu. Bất kỳ CMS nào (Strapi,
 * Payload, Sanity, WordPress, hay API tự viết) chỉ cần trả về đúng những kiểu này
 * là cắm được vào, không phải sửa một dòng nào trong các component.
 */

/** Bốn màu kính. Dùng để tô màu nhãn chuyên mục và các mảng trang trí. */
export type GlassTone = "cobalt" | "ruby" | "amber" | "emerald";

/* ------------------------------------------------------------------ */
/* Bản phát hành                                                       */
/* ------------------------------------------------------------------ */

export type DownloadFile = {
  id: string;
  tag: string;
  title: string;
  summary: string;
  fileName: string;
  size: string;
  href: string;
  primary: boolean;
  cta: string;
};

export type ReleaseGroup = {
  id: string;
  title: string;
  icon: "download" | "check-square" | "database" | "book";
  items: string[];
};

export type Release = {
  version: string;
  /** ISO 8601 (YYYY-MM-DD) — dùng cho <time dateTime> và JSON-LD */
  publishedAt: string;
  headline: string;
  summary: string;
  groups: ReleaseGroup[];
  downloads: DownloadFile[];
  /**
   * Slug bài viết /tin-tuc mô tả đầy đủ bản này — nút "Đọc bài viết đầy đủ"
   * trên trang chủ dùng trường này thay vì suy luận từ số phiên bản. Trước
   * đây trang chủ trỏ CỨNG vào "phat-hanh-phien-ban-4-0-0"; phát hành 4.0.1
   * cho thấy ngay vấn đề — link cũ vẫn trỏ về bài của bản trước, không tự cập
   * nhật. Để trống nếu bản này chưa có bài viết riêng (nút sẽ tự ẩn).
   */
  articleSlug?: string;
};

/* ------------------------------------------------------------------ */
/* Bài viết                                                            */
/* ------------------------------------------------------------------ */

export type ArticleCategory = {
  slug: string;
  name: string;
  tone: GlassTone;
};

/**
 * Thân bài được mô tả bằng các khối có kiểu rõ ràng thay vì một chuỗi HTML.
 *
 * Lý do: khi gắn CMS thật, nội dung do người khác nhập vào. Dựng lại từ khối
 * có kiểu thì React tự thoát ký tự, không có chỗ nào cần dangerouslySetInnerHTML,
 * nên một bài viết bị chèn mã độc cũng không thực thi được.
 */
export type ArticleBlock =
  | { type: "paragraph"; text: string }
  | { type: "heading"; text: string }
  | { type: "list"; items: string[] }
  | { type: "quote"; text: string; cite?: string }
  | { type: "note"; text: string }
  | { type: "image"; src: string; alt: string; width: number; height: number; caption?: string };

export type ArticleSummary = {
  slug: string;
  title: string;
  excerpt: string;
  category: ArticleCategory;
  /** ISO 8601 (YYYY-MM-DD) */
  publishedAt: string;
  updatedAt?: string;
  readingMinutes: number;
  author?: string;
};

export type Article = ArticleSummary & {
  body: ArticleBlock[];
};

/* ------------------------------------------------------------------ */
/* Hợp đồng của nguồn nội dung                                         */
/* ------------------------------------------------------------------ */

export interface ContentProvider {
  getRelease(): Promise<Release>;
  listArticles(options?: { limit?: number; category?: string }): Promise<ArticleSummary[]>;
  getArticle(slug: string): Promise<Article | null>;
  listArticleSlugs(): Promise<string[]>;
  listCategories(): Promise<ArticleCategory[]>;
  getLandingContent(): Promise<LandingContent>;
}

/* ------------------------------------------------------------------ */
/* Nội dung trang chủ — các danh sách sửa được từ /admin/trang-chu     */
/* ------------------------------------------------------------------ */

/**
 * Tên icon dùng chung cho tính năng, liên kết hỗ trợ và ảnh chụp màn hình.
 * Đây là danh sách ĐÓNG — khớp 1-1 với các khoá trong `paths` của
 * `src/components/Icon.tsx`. Thêm icon mới phải sửa cả hai chỗ.
 */
export type IconName =
  | "users"
  | "home"
  | "book"
  | "heart"
  | "graduation"
  | "chart"
  | "printer"
  | "refresh"
  | "shield"
  | "download"
  | "check-square"
  | "database"
  | "family"
  | "globe"
  | "pencil"
  | "help"
  | "external"
  | "arrow-right"
  | "mail"
  | "facebook";

export type TrustStat = { value: string; label: string };

export type PainPoint = { title: string; body: string };

export type Feature = {
  id: string;
  icon: IconName;
  tone: GlassTone;
  title: string;
  body: string;
};

export type HelpLink = {
  id: string;
  icon: IconName;
  title: string;
  subtitle: string;
  href: string;
  external: boolean;
};

/** Cũng dùng để sinh JSON-LD FAQPage — xem `HomeJsonLd` trong `components/JsonLd.tsx`. */
export type FaqItem = {
  id: string;
  question: string;
  answer: string[];
};

/**
 * Các danh sách nội dung của trang chủ có thể sửa qua `/admin/trang-chu`.
 *
 * CỐ Ý không gồm `screenshots`/`heroScreenshot`: ảnh chụp màn hình đòi hỏi tệp
 * ảnh thật đã có sẵn trong `public/images/` (đã qua khâu cắt ảnh thủ công, xem
 * README) — sửa qua form chỉ đổi được đường dẫn, không tải ảnh lên được, nên rủi
 * ro trỏ tới ảnh không tồn tại là quá cao cho một trang không có preview. Cũng
 * không gồm tiêu đề/mô tả phần Hero: chữ ở đó được ngắt dòng thủ công theo đúng
 * số đo phông chữ (xem chú thích trong `app/page.tsx`), sửa qua form dễ phá bố
 * cục mà admin không nhìn thấy ngay.
 */
export type LandingContent = {
  trustStats: TrustStat[];
  painPoints: PainPoint[];
  features: Feature[];
  installSteps: string[];
  helpLinks: HelpLink[];
  faqs: FaqItem[];
};
