import { cookies } from "next/headers";
import { NextResponse } from "next/server";

import {
  SESSION_COOKIE,
  createSession,
  donPhienHetHan,
  getDb,
  verifyPassword,
} from "@/lib/admin/auth";

/**
 * Giữ lại mỗi lần đăng nhập sai đúng khoảng này. Hạ tốc độ dò tuần tự từ hàng
 * nghìn lần/phút xuống khoảng 40 lần/phút, và không làm phiền người nhập đúng.
 *
 * KHÔNG phải lớp phòng thủ đủ một mình: Worker chạy nhiều instance song song
 * nên kẻ tấn công bắn song song vẫn nhanh. Lớp thật là Cloudflare Rate Limiting
 * Rule cho `POST /admin/api/login` — việc phải làm trên dashboard, xem
 * `src/lib/admin/auth.ts` và README.
 */
const TRE_KHI_SAI_MS = 1500;

function ngu(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}

export async function POST(request: Request) {
  const form = await request.formData();
  const password = String(form.get("password") ?? "");

  let ok: boolean;
  try {
    ok = await verifyPassword(password);
  } catch (err) {
    /* Thông điệp lỗi nội bộ (tên biến bí mật chưa đặt, hướng dẫn cấu hình D1)
     * chỉ vào log của Worker, không trả cho người gọi ẩn danh — finding T-4. */
    console.error("Đăng nhập quản trị lỗi cấu hình:", err);
    return NextResponse.redirect(new URL("/admin/login?error=loi-he-thong", request.url), 303);
  }

  if (!ok) {
    /* Ghi lại để còn thấy được một đợt dò mật khẩu. Không ghi mật khẩu đã thử. */
    console.warn(
      "Đăng nhập quản trị SAI MẬT KHẨU",
      JSON.stringify({
        ip: request.headers.get("cf-connecting-ip") ?? "khong-ro",
        quocGia: request.headers.get("cf-ipcountry") ?? "khong-ro",
        userAgent: request.headers.get("user-agent")?.slice(0, 120) ?? "khong-ro",
        luc: new Date().toISOString(),
      }),
    );
    await ngu(TRE_KHI_SAI_MS);
    return NextResponse.redirect(new URL("/admin/login?error=sai-mat-khau", request.url), 303);
  }

  const db = await getDb();
  /* Dọn phiên quá hạn ở mỗi lần đăng nhập thành công — finding T-5. Lỗi dọn dẹp
   * không được làm hỏng việc đăng nhập. */
  try {
    await donPhienHetHan(db);
  } catch (err) {
    console.error("Không dọn được phiên quản trị hết hạn:", err);
  }
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
