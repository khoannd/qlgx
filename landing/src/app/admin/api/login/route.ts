import { cookies } from "next/headers";
import { NextResponse } from "next/server";

import { SESSION_COOKIE, createSession, getDb, verifyPassword } from "@/lib/admin/auth";

export async function POST(request: Request) {
  const form = await request.formData();
  const password = String(form.get("password") ?? "");

  let ok: boolean;
  try {
    ok = await verifyPassword(password);
  } catch (err) {
    const message = err instanceof Error ? err.message : "Lỗi không xác định";
    return NextResponse.redirect(
      new URL(`/admin/login?error=${encodeURIComponent(message)}`, request.url),
      303,
    );
  }

  if (!ok) {
    return NextResponse.redirect(new URL("/admin/login?error=sai-mat-khau", request.url), 303);
  }

  const db = await getDb();
  const token = await createSession(db);

  const jar = await cookies();
  jar.set(SESSION_COOKIE, token, {
    httpOnly: true,
    secure: true,
    sameSite: "lax",
    path: "/admin",
    maxAge: 60 * 60 * 12,
  });

  return NextResponse.redirect(new URL("/admin", request.url), 303);
}
