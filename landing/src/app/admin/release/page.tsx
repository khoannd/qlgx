import Link from "next/link";
import { redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";
import { content } from "@/lib/content";

export const metadata = { title: "Sửa bản phát hành", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

export default async function AdminReleasePage({
  searchParams,
}: {
  searchParams: Promise<{ saved?: string; error?: string }>;
}) {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const { saved, error } = await searchParams;
  const release = await content.getRelease();

  return (
    <main className="wrap py-16">
      <Link href="/admin" className="link-underline text-[0.9rem] text-ink-soft">
        ← Quay lại
      </Link>

      <h1 className="mt-4 font-display text-[1.8rem] font-semibold text-ink">Bản phát hành</h1>

      {saved ? (
        <p className="mt-4 rounded-lg border-l-4 border-l-mint bg-mint/8 px-4 py-3 text-[0.9rem] text-ink">
          Đã lưu. Trang chủ sẽ hiện nội dung mới trong tối đa vài phút (bộ nhớ đệm ISR).
        </p>
      ) : null}
      {error ? (
        <p className="mt-4 rounded-lg border-l-4 border-l-amber bg-amber/8 px-4 py-3 text-[0.9rem] text-ink">
          {decodeURIComponent(error)}
        </p>
      ) : null}

      <form method="post" action="/admin/api/release" className="glass mt-8 grid gap-6 p-7">
        <div className="grid gap-5 sm:grid-cols-2">
          <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
            Số phiên bản
            <input
              name="version"
              defaultValue={release.version}
              required
              placeholder="4.0.0"
              className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
            />
          </label>
          <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
            Ngày phát hành (YYYY-MM-DD)
            <input
              name="publishedAt"
              type="date"
              defaultValue={release.publishedAt}
              required
              className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
            />
          </label>
        </div>

        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Tiêu đề ngắn
          <input
            name="headline"
            defaultValue={release.headline}
            required
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
          />
        </label>

        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Tóm tắt
          <textarea
            name="summary"
            defaultValue={release.summary}
            required
            rows={3}
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
          />
        </label>

        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Slug bài viết mô tả đầy đủ bản này (tuỳ chọn)
          <input
            name="articleSlug"
            defaultValue={release.articleSlug}
            placeholder="phat-hanh-phien-ban-4-0-1"
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 font-mono text-[0.9rem] text-ink outline-none focus:border-brand"
          />
          <span className="text-[0.8rem] font-normal text-ink-faint">
            Nút &quot;Đọc bài viết đầy đủ&quot; trên trang chủ dùng slug này. Để trống nếu bản này
            chưa có bài viết riêng trên /tin-tuc — nút sẽ tự ẩn.
          </span>
        </label>

        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Danh sách thay đổi (JSON — mảng ReleaseGroup)
          <textarea
            name="groupsJson"
            defaultValue={JSON.stringify(release.groups, null, 2)}
            required
            rows={14}
            spellCheck={false}
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 font-mono text-[0.82rem] text-ink outline-none focus:border-brand"
          />
        </label>

        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Danh sách tệp tải (JSON — mảng DownloadFile)
          <textarea
            name="downloadsJson"
            defaultValue={JSON.stringify(release.downloads, null, 2)}
            required
            rows={10}
            spellCheck={false}
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 font-mono text-[0.82rem] text-ink outline-none focus:border-brand"
          />
        </label>

        <p className="text-[0.82rem] text-ink-faint">
          Hai ô JSON ở trên là danh sách có cấu trúc — sai cú pháp sẽ bị chặn lại khi lưu và không
          làm hỏng dữ liệu hiện có. Xem kiểu <code>ReleaseGroup</code>/<code>DownloadFile</code>{" "}
          trong <code>src/lib/content/types.ts</code> nếu cần đối chiếu trường.
        </p>

        <button type="submit" className="btn btn-primary w-full sm:w-auto sm:justify-self-start">
          Lưu
        </button>
      </form>
    </main>
  );
}
