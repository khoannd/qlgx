import { NextResponse } from "next/server";

import { requireSession } from "@/lib/admin/auth";
import { saveArticle, type ArticleInput } from "@/lib/content/d1-provider";
import type { ArticleBlock, GlassTone } from "@/lib/content/types";

const TONES: GlassTone[] = ["cobalt", "ruby", "amber", "emerald"];

function fail(request: Request, redirectSlug: string, message: string) {
  const target = redirectSlug ? `/admin/articles/${redirectSlug}` : "/admin/articles/new";
  return NextResponse.redirect(
    new URL(`${target}?error=${encodeURIComponent(message)}`, request.url),
    303,
  );
}

export async function POST(request: Request) {
  const db = await requireSession();
  if (!db) return NextResponse.redirect(new URL("/admin/login", request.url), 303);

  const form = await request.formData();
  const originalSlug = String(form.get("originalSlug") ?? "");
  const slug = String(form.get("slug") ?? "").trim();
  const title = String(form.get("title") ?? "").trim();
  const excerpt = String(form.get("excerpt") ?? "").trim();
  const categorySlug = String(form.get("categorySlug") ?? "").trim();
  const categoryName = String(form.get("categoryName") ?? "").trim();
  const categoryTone = String(form.get("categoryTone") ?? "").trim() as GlassTone;
  const publishedAt = String(form.get("publishedAt") ?? "").trim();
  const updatedAt = String(form.get("updatedAt") ?? "").trim();
  const readingMinutes = Number(form.get("readingMinutes") ?? 0);
  const author = String(form.get("author") ?? "").trim();
  const bodyRaw = String(form.get("bodyJson") ?? "");

  const redirectSlug = originalSlug || slug;

  if (!slug || !/^[a-z0-9-]+$/.test(slug)) {
    return fail(
      request,
      redirectSlug,
      "Slug chỉ được chứa chữ thường không dấu, số và dấu gạch ngang.",
    );
  }
  if (!title || !excerpt || !categorySlug || !categoryName || !publishedAt) {
    return fail(request, redirectSlug, "Thiếu thông tin bắt buộc.");
  }
  if (!TONES.includes(categoryTone)) {
    return fail(request, redirectSlug, "Sắc thái chuyên mục không hợp lệ.");
  }
  if (!Number.isFinite(readingMinutes) || readingMinutes <= 0) {
    return fail(request, redirectSlug, "Số phút đọc phải là một số dương.");
  }

  let body: ArticleBlock[];
  try {
    body = JSON.parse(bodyRaw);
    if (!Array.isArray(body)) throw new Error("not array");
  } catch {
    return fail(request, redirectSlug, "Thân bài viết không phải JSON hợp lệ.");
  }

  const article: ArticleInput = {
    slug,
    title,
    excerpt,
    category: { slug: categorySlug, name: categoryName, tone: categoryTone },
    publishedAt,
    updatedAt: updatedAt || undefined,
    readingMinutes,
    author: author || undefined,
    body,
  };

  try {
    // Đổi slug: xoá bản ghi cũ sau khi ghi bản ghi mới thành công, tránh mất
    // dữ liệu nếu bước ghi mới thất bại giữa chừng.
    await saveArticle(db, article);
    if (originalSlug && originalSlug !== slug) {
      await db.prepare("DELETE FROM articles WHERE slug = ?").bind(originalSlug).run();
    }
  } catch (err) {
    return fail(request, redirectSlug, err instanceof Error ? err.message : "Lỗi khi lưu.");
  }

  return NextResponse.redirect(new URL("/admin/articles?saved=1", request.url), 303);
}
