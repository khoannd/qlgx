import { NextResponse } from "next/server";

import { requireSession } from "@/lib/admin/auth";
import { saveRelease, type ReleaseInput } from "@/lib/content/d1-provider";
import type { DownloadFile, ReleaseGroup } from "@/lib/content/types";

function fail(request: Request, message: string) {
  return NextResponse.redirect(
    new URL(`/admin/release?error=${encodeURIComponent(message)}`, request.url),
    303,
  );
}

export async function POST(request: Request) {
  const db = await requireSession();
  if (!db) return NextResponse.redirect(new URL("/admin/login", request.url), 303);

  const form = await request.formData();
  const version = String(form.get("version") ?? "").trim();
  const publishedAt = String(form.get("publishedAt") ?? "").trim();
  const headline = String(form.get("headline") ?? "").trim();
  const summary = String(form.get("summary") ?? "").trim();
  const articleSlug = String(form.get("articleSlug") ?? "").trim();
  const groupsRaw = String(form.get("groupsJson") ?? "");
  const downloadsRaw = String(form.get("downloadsJson") ?? "");

  if (!version || !publishedAt || !headline || !summary) {
    return fail(request, "Thiếu thông tin bắt buộc.");
  }

  let groups: ReleaseGroup[];
  let downloads: DownloadFile[];
  try {
    groups = JSON.parse(groupsRaw);
    downloads = JSON.parse(downloadsRaw);
    if (!Array.isArray(groups) || !Array.isArray(downloads)) throw new Error("not array");
  } catch {
    return fail(request, "Danh sách thay đổi hoặc danh sách tệp tải không phải JSON hợp lệ.");
  }

  const release: ReleaseInput = {
    version,
    publishedAt,
    headline,
    summary,
    groups,
    downloads,
    articleSlug: articleSlug || undefined,
  };

  try {
    await saveRelease(db, release);
  } catch (err) {
    return fail(request, err instanceof Error ? err.message : "Lỗi không xác định khi lưu.");
  }

  return NextResponse.redirect(new URL("/admin/release?saved=1", request.url), 303);
}
