import { NextResponse } from "next/server";

import { requireSession } from "@/lib/admin/auth";
import { kiemTraDanhSachTaiVe, kiemTraNhomThayDoi } from "@/lib/admin/kiem-tra-lien-ket";
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

  let groupsThuTho: unknown;
  let downloadsThuTho: unknown;
  try {
    groupsThuTho = JSON.parse(groupsRaw);
    downloadsThuTho = JSON.parse(downloadsRaw);
  } catch {
    return fail(request, "Danh sách thay đổi hoặc danh sách tệp tải không phải JSON hợp lệ.");
  }

  /* Kiểm lược đồ VÀ kiểm `href` của nút tải trước khi ghi vào D1.
   * Nút "Tải phần mềm" trên trang chủ dùng thẳng `href` này, nên một địa chỉ
   * lạ ở đây = phát bộ cài của kẻ khác cho mọi giáo xứ. Xem lý do đầy đủ ở
   * src/lib/admin/kiem-tra-lien-ket.ts. */
  const kqNhom = kiemTraNhomThayDoi(groupsThuTho);
  if (!kqNhom.hopLe) return fail(request, kqNhom.lyDo);

  const kqTaiVe = kiemTraDanhSachTaiVe(downloadsThuTho);
  if (!kqTaiVe.hopLe) return fail(request, kqTaiVe.lyDo);

  const groups = groupsThuTho as ReleaseGroup[];
  const downloads = downloadsThuTho as DownloadFile[];

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
