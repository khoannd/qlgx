import { cookies } from "next/headers";
import { NextResponse } from "next/server";

import { SESSION_COOKIE, destroySession, getDb } from "@/lib/admin/auth";

export async function POST(request: Request) {
  const jar = await cookies();
  const token = jar.get(SESSION_COOKIE)?.value;

  if (token) {
    try {
      const db = await getDb();
      await destroySession(db, token);
    } catch {
      // D1 có thể đã mất kết nối — vẫn cứ xoá cookie phía trình duyệt.
    }
  }

  jar.delete({ name: SESSION_COOKIE, path: "/admin" });
  return NextResponse.redirect(new URL("/admin/login", request.url), 303);
}
