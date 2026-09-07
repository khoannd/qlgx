import Link from "next/link";
import { redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";
import { content } from "@/lib/content";

import { LandingContentForm } from "./LandingContentForm";

export const metadata = { title: "Nội dung trang chủ", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

export default async function AdminLandingPage({
  searchParams,
}: {
  searchParams: Promise<{ saved?: string; error?: string }>;
}) {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const { saved, error } = await searchParams;
  const landing = await content.getLandingContent();

  return (
    <main className="wrap py-16">
      <Link href="/admin" className="link-underline text-[0.9rem] text-ink-soft">
        ← Quay lại
      </Link>

      <h1 className="mt-4 font-display text-[1.8rem] font-semibold text-ink">Nội dung trang chủ</h1>
      <p className="mt-2 max-w-[62ch] text-[0.92rem] text-ink-soft">
        Sửa các danh sách hiện trên trang chủ: con số tóm tắt, vấn đề của sổ giấy, tính năng, bước
        cài đặt, liên kết hỗ trợ và câu hỏi thường gặp. Không gồm ảnh chụp màn hình hay tiêu đề lớn
        đầu trang — những phần đó gắn chặt với bố cục và ảnh có sẵn, cần sửa trong mã nguồn.
      </p>

      {saved ? (
        <p className="mt-4 rounded-lg border-l-4 border-l-mint bg-mint/8 px-4 py-3 text-[0.9rem] text-ink">
          Đã lưu.{" "}
          <Link href="/" className="link-underline font-semibold">
            Xem trang chủ
          </Link>{" "}
          — nội dung mới hiện ra trong tối đa vài phút (bộ nhớ đệm ISR).
        </p>
      ) : null}

      <LandingContentForm content={landing} error={error} />
    </main>
  );
}
