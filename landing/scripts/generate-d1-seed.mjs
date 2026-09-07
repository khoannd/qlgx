/**
 * Sinh migrations/0004_seed.sql trực tiếp từ src/lib/content/static-provider.ts.
 *
 * VÌ SAO: nội dung mẫu (bản phát hành + bài viết) chỉ nên tồn tại ở MỘT nơi.
 * Viết tay một bản sao trong SQL sẽ sớm lệch khỏi bản TypeScript. Script này
 * đọc thẳng module TS đang chạy thật trong ứng dụng — nếu sau này sửa nội
 * dung mẫu, chỉ cần chạy lại `npm run d1:seed:generate` là seed cập nhật theo.
 *
 * Chạy bằng Node 22+ (dùng --experimental-strip-types để nạp thẳng .ts mà
 * không cần bước build riêng).
 */

import { writeFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const outPath = path.join(here, "..", "migrations", "0004_seed.sql");

const { staticContentProvider } = await import("../src/lib/content/static-provider.ts");

/** Escape để chèn an toàn vào chuỗi SQL trong dấu nháy đơn. */
function sqlString(value) {
  return `'${String(value).replaceAll("'", "''")}'`;
}

function sqlOrNull(value) {
  return value === undefined || value === null ? "NULL" : sqlString(value);
}

const release = await staticContentProvider.getRelease();
const articles = await staticContentProvider.listArticles();
const landingContent = await staticContentProvider.getLandingContent();

const lines = [
  "-- Sinh tự động bởi scripts/generate-d1-seed.mjs — KHÔNG sửa tay tệp này.",
  "-- Muốn đổi nội dung mẫu, sửa src/lib/content/static-provider.ts rồi chạy:",
  "--   npm run d1:seed:generate",
  "",
  "DELETE FROM release WHERE id = 1;",
  `INSERT INTO release (id, version, published_at, headline, summary, groups_json, downloads_json, article_slug)`,
  `VALUES (1, ${sqlString(release.version)}, ${sqlString(release.publishedAt)}, ${sqlString(release.headline)}, ${sqlString(release.summary)}, ${sqlString(JSON.stringify(release.groups))}, ${sqlString(JSON.stringify(release.downloads))}, ${sqlOrNull(release.articleSlug)});`,
  "",
  // Xoá TRẮNG bảng trước khi chèn lại — không chỉ xoá-rồi-chèn từng slug hiện
  // có. Một bài viết bị bỏ khỏi static-provider.ts (ví dụ "phat-hanh-phien-ban-
  // 4-0-1" khi các bản vá kỹ thuật được gộp lại thành một bài duy nhất) sẽ vẫn
  // còn nằm mồ côi trong D1 nếu chỉ xoá theo slug đang có — đã xảy ra thật, phải
  // tự tay xoá bằng tay trên D1 thật một lần. Bảng nhỏ (vài chục bài là cùng),
  // xoá sạch rồi chèn lại toàn bộ không tốn kém gì.
  "DELETE FROM articles;",
  "",
];

for (const summary of articles) {
  const full = await staticContentProvider.getArticle(summary.slug);
  lines.push(
    `INSERT INTO articles (slug, title, excerpt, category_slug, category_name, category_tone, published_at, updated_at, reading_minutes, author, body_json, sort_order)`,
  );
  lines.push(
    `VALUES (${sqlString(full.slug)}, ${sqlString(full.title)}, ${sqlString(full.excerpt)}, ${sqlString(full.category.slug)}, ${sqlString(full.category.name)}, ${sqlString(full.category.tone)}, ${sqlString(full.publishedAt)}, ${sqlOrNull(full.updatedAt)}, ${full.readingMinutes}, ${sqlOrNull(full.author)}, ${sqlString(JSON.stringify(full.body))}, 0);`,
  );
  lines.push("");
}

lines.push(
  "DELETE FROM landing_content WHERE id = 1;",
  "INSERT INTO landing_content (id, data_json)",
  `VALUES (1, ${sqlString(JSON.stringify(landingContent))});`,
  "",
);

await writeFile(outPath, lines.join("\n"), "utf8");
console.log(`Đã ghi ${outPath} (${articles.length} bài viết).`);
