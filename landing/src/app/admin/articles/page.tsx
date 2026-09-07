import Link from "next/link";
import { redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";
import { content } from "@/lib/content";
import { formatDate } from "@/lib/site";

export const metadata = { title: "Bài viết", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

export default async function AdminArticlesPage({
  searchParams,
}: {
  searchParams: Promise<{ saved?: string; deleted?: string }>;
}) {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const { saved, deleted } = await searchParams;
  const articles = await content.listArticles();

  return (
    <main className="wrap py-16">
      <Link href="/admin" className="link-underline text-[0.9rem] text-ink-soft">
        ← Quay lại
      </Link>

      <div className="mt-4 flex flex-wrap items-center justify-between gap-4">
        <h1 className="font-display text-[1.8rem] font-semibold text-ink">Bài viết</h1>
        <Link href="/admin/articles/new" className="btn btn-primary h-11 min-h-0">
          + Bài viết mới
        </Link>
      </div>

      {saved ? (
        <p className="mt-4 rounded-lg border-l-4 border-l-mint bg-mint/8 px-4 py-3 text-[0.9rem] text-ink">
          Đã lưu.
        </p>
      ) : null}
      {deleted ? (
        <p className="mt-4 rounded-lg border-l-4 border-l-mint bg-mint/8 px-4 py-3 text-[0.9rem] text-ink">
          Đã xoá.
        </p>
      ) : null}

      <ul className="glass mt-8 grid list-none divide-y divide-white/60 p-0">
        {articles.map((a) => (
          <li key={a.slug} className="flex flex-wrap items-center gap-4 px-6 py-4">
            <div className="min-w-0 flex-1">
              <p className="truncate font-semibold text-ink">{a.title}</p>
              <p className="text-[0.82rem] text-ink-faint">
                {a.category.name} · {formatDate(a.publishedAt)} · /{a.slug}
              </p>
            </div>
            <Link href={`/admin/articles/${a.slug}`} className="btn btn-glass h-10 min-h-0 px-4">
              Sửa
            </Link>
            <form method="post" action="/admin/api/articles/delete">
              <input type="hidden" name="slug" value={a.slug} />
              <button type="submit" className="btn btn-glass h-10 min-h-0 px-4 text-amber-ink">
                Xoá
              </button>
            </form>
          </li>
        ))}
        {articles.length === 0 ? (
          <li className="px-6 py-8 text-center text-ink-soft">Chưa có bài viết nào.</li>
        ) : null}
      </ul>
    </main>
  );
}
