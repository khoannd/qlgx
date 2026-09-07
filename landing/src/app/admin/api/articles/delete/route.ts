import { NextResponse } from "next/server";

import { requireSession } from "@/lib/admin/auth";
import { deleteArticle } from "@/lib/content/d1-provider";

export async function POST(request: Request) {
  const db = await requireSession();
  if (!db) return NextResponse.redirect(new URL("/admin/login", request.url), 303);

  const form = await request.formData();
  const slug = String(form.get("slug") ?? "");
  if (slug) await deleteArticle(db, slug);

  return NextResponse.redirect(new URL("/admin/articles?deleted=1", request.url), 303);
}
