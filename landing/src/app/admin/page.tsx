import Link from "next/link";
import { redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";

export const metadata = { title: "Quản trị", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

export default async function AdminHomePage() {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const articleCount = await db
    .prepare("SELECT COUNT(*) AS n FROM articles")
    .first<{ n: number }>();

  return (
    <main className="wrap py-16">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <h1 className="font-display text-[1.8rem] font-semibold text-ink">Quản trị nội dung</h1>
        <div className="flex flex-wrap items-center gap-3">
          <Link href="/" className="btn btn-glass h-11 min-h-0">
            Xem trang chủ
          </Link>
          <form method="post" action="/admin/api/logout">
            <button type="submit" className="btn btn-glass h-11 min-h-0">
              Đăng xuất
            </button>
          </form>
        </div>
      </div>

      <div className="mt-10 grid gap-6 sm:grid-cols-2">
        <Link href="/admin/release" className="glass block p-7 no-underline">
          <h2 className="font-display text-[1.3rem] font-semibold text-ink">Bản phát hành</h2>
          <p className="mt-2 text-[0.92rem] text-ink-soft">
            Sửa số phiên bản, ngày phát hành, danh sách thay đổi và tệp tải về. Có hiệu lực ngay
            trên trang chủ, không cần build lại.
          </p>
        </Link>

        <Link href="/admin/articles" className="glass block p-7 no-underline">
          <h2 className="font-display text-[1.3rem] font-semibold text-ink">
            Bài viết ({articleCount?.n ?? 0})
          </h2>
          <p className="mt-2 text-[0.92rem] text-ink-soft">
            Thêm, sửa, xoá bài viết trên /tin-tuc.
          </p>
        </Link>

        <Link href="/admin/trang-chu" className="glass block p-7 no-underline sm:col-span-2">
          <h2 className="font-display text-[1.3rem] font-semibold text-ink">Nội dung trang chủ</h2>
          <p className="mt-2 text-[0.92rem] text-ink-soft">
            Con số tóm tắt, vấn đề của sổ giấy, tính năng, bước cài đặt, liên kết hỗ trợ và câu hỏi
            thường gặp — mọi danh sách hiện trên trang chủ, trừ ảnh chụp màn hình và tiêu đề lớn
            đầu trang.
          </p>
        </Link>

        <Link href="/admin/thong-ke" className="glass block p-7 no-underline sm:col-span-2">
          <h2 className="font-display text-[1.3rem] font-semibold text-ink">
            Thống kê tải &amp; cập nhật
          </h2>
          <p className="mt-2 text-[0.92rem] text-ink-soft">
            Ai đang tải bản nào, qua kênh nào (bấm tay trên web hay chương trình tự cập nhật), từ
            đâu — ghi lại mỗi lượt tải thật, không tính lượt chỉ mở chương trình lên.
          </p>
        </Link>
      </div>
    </main>
  );
}
