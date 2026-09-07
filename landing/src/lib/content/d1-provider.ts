import type {
  Article,
  ArticleBlock,
  ArticleCategory,
  ArticleSummary,
  ContentProvider,
  GlassTone,
  LandingContent,
  Release,
} from "./types";

/**
 * Nguồn nội dung đọc/ghi từ Cloudflare D1 — cho phép sửa bản phát hành và bài
 * viết ngay từ trang quản trị `/admin` mà không cần build lại trang.
 *
 * Lược đồ bảng nằm ở `migrations/0001_init.sql`. Mảng lồng nhau (danh sách thay
 * đổi, danh sách tệp tải, thân bài viết) lưu dưới dạng chuỗi JSON trong một cột
 * TEXT — D1/SQLite không có kiểu JSON gốc.
 */

type ReleaseRow = {
  id: number;
  version: string;
  published_at: string;
  headline: string;
  summary: string;
  groups_json: string;
  downloads_json: string;
  article_slug: string | null;
};

type ArticleRow = {
  slug: string;
  title: string;
  excerpt: string;
  category_slug: string;
  category_name: string;
  category_tone: GlassTone;
  published_at: string;
  updated_at: string | null;
  reading_minutes: number;
  author: string | null;
  body_json: string;
  sort_order: number;
};

type LandingContentRow = {
  id: number;
  data_json: string;
};

function releaseFromRow(row: ReleaseRow): Release {
  return {
    version: row.version,
    publishedAt: row.published_at,
    headline: row.headline,
    summary: row.summary,
    groups: JSON.parse(row.groups_json),
    downloads: JSON.parse(row.downloads_json),
    articleSlug: row.article_slug ?? undefined,
  };
}

function summaryFromRow(row: ArticleRow): ArticleSummary {
  return {
    slug: row.slug,
    title: row.title,
    excerpt: row.excerpt,
    category: { slug: row.category_slug, name: row.category_name, tone: row.category_tone },
    publishedAt: row.published_at,
    updatedAt: row.updated_at ?? undefined,
    readingMinutes: row.reading_minutes,
    author: row.author ?? undefined,
  };
}

function articleFromRow(row: ArticleRow): Article {
  return { ...summaryFromRow(row), body: JSON.parse(row.body_json) };
}

export function createD1ContentProvider(db: D1Database): ContentProvider {
  return {
    async getRelease() {
      const row = await db
        .prepare("SELECT * FROM release WHERE id = 1")
        .first<ReleaseRow>();
      if (!row) {
        throw new Error(
          "Bảng `release` trong D1 chưa có dữ liệu. Chạy migrations/0004_seed.sql trước.",
        );
      }
      return releaseFromRow(row);
    },

    async listArticles(options) {
      let query = "SELECT * FROM articles";
      const bindings: string[] = [];
      if (options?.category) {
        query += " WHERE category_slug = ?";
        bindings.push(options.category);
      }
      query += " ORDER BY published_at DESC, sort_order DESC";
      if (options?.limit) {
        query += ` LIMIT ${Number(options.limit)}`;
      }
      const { results } = await db
        .prepare(query)
        .bind(...bindings)
        .all<ArticleRow>();
      return results.map(summaryFromRow);
    },

    async getArticle(slug) {
      const row = await db
        .prepare("SELECT * FROM articles WHERE slug = ?")
        .bind(slug)
        .first<ArticleRow>();
      return row ? articleFromRow(row) : null;
    },

    async listArticleSlugs() {
      const { results } = await db.prepare("SELECT slug FROM articles").all<{ slug: string }>();
      return results.map((r) => r.slug);
    },

    async listCategories() {
      const { results } = await db
        .prepare(
          "SELECT DISTINCT category_slug AS slug, category_name AS name, category_tone AS tone FROM articles",
        )
        .all<ArticleCategory>();
      return results;
    },

    async getLandingContent() {
      const row = await db
        .prepare("SELECT * FROM landing_content WHERE id = 1")
        .first<LandingContentRow>();
      if (!row) {
        throw new Error(
          "Bảng `landing_content` trong D1 chưa có dữ liệu. Chạy migrations/0004_seed.sql trước.",
        );
      }
      return JSON.parse(row.data_json) as LandingContent;
    },
  };
}

/* ------------------------------------------------------------------ */
/* Ghi dữ liệu — dùng bởi các route quản trị, không thuộc ContentProvider */
/* ------------------------------------------------------------------ */

export type ReleaseInput = Release;

export async function saveRelease(db: D1Database, release: ReleaseInput): Promise<void> {
  await db
    .prepare(
      `INSERT INTO release (id, version, published_at, headline, summary, groups_json, downloads_json, article_slug)
       VALUES (1, ?, ?, ?, ?, ?, ?, ?)
       ON CONFLICT(id) DO UPDATE SET
         version = excluded.version,
         published_at = excluded.published_at,
         headline = excluded.headline,
         summary = excluded.summary,
         groups_json = excluded.groups_json,
         downloads_json = excluded.downloads_json,
         article_slug = excluded.article_slug`,
    )
    .bind(
      release.version,
      release.publishedAt,
      release.headline,
      release.summary,
      JSON.stringify(release.groups),
      JSON.stringify(release.downloads),
      release.articleSlug ?? null,
    )
    .run();
}

export type ArticleInput = {
  slug: string;
  title: string;
  excerpt: string;
  category: ArticleCategory;
  publishedAt: string;
  updatedAt?: string;
  readingMinutes: number;
  author?: string;
  body: ArticleBlock[];
  sortOrder?: number;
};

export async function saveArticle(db: D1Database, article: ArticleInput): Promise<void> {
  await db
    .prepare(
      `INSERT INTO articles
         (slug, title, excerpt, category_slug, category_name, category_tone,
          published_at, updated_at, reading_minutes, author, body_json, sort_order)
       VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
       ON CONFLICT(slug) DO UPDATE SET
         title = excluded.title,
         excerpt = excluded.excerpt,
         category_slug = excluded.category_slug,
         category_name = excluded.category_name,
         category_tone = excluded.category_tone,
         published_at = excluded.published_at,
         updated_at = excluded.updated_at,
         reading_minutes = excluded.reading_minutes,
         author = excluded.author,
         body_json = excluded.body_json,
         sort_order = excluded.sort_order`,
    )
    .bind(
      article.slug,
      article.title,
      article.excerpt,
      article.category.slug,
      article.category.name,
      article.category.tone,
      article.publishedAt,
      article.updatedAt ?? null,
      article.readingMinutes,
      article.author ?? null,
      JSON.stringify(article.body),
      article.sortOrder ?? 0,
    )
    .run();
}

export async function deleteArticle(db: D1Database, slug: string): Promise<void> {
  await db.prepare("DELETE FROM articles WHERE slug = ?").bind(slug).run();
}

export async function saveLandingContent(db: D1Database, data: LandingContent): Promise<void> {
  await db
    .prepare(
      `INSERT INTO landing_content (id, data_json)
       VALUES (1, ?)
       ON CONFLICT(id) DO UPDATE SET data_json = excluded.data_json`,
    )
    .bind(JSON.stringify(data))
    .run();
}

export async function getArticleRaw(db: D1Database, slug: string): Promise<ArticleInput | null> {
  const row = await db
    .prepare("SELECT * FROM articles WHERE slug = ?")
    .bind(slug)
    .first<ArticleRow>();
  if (!row) return null;
  return {
    slug: row.slug,
    title: row.title,
    excerpt: row.excerpt,
    category: { slug: row.category_slug, name: row.category_name, tone: row.category_tone },
    publishedAt: row.published_at,
    updatedAt: row.updated_at ?? undefined,
    readingMinutes: row.reading_minutes,
    author: row.author ?? undefined,
    body: JSON.parse(row.body_json),
    sortOrder: row.sort_order,
  };
}
