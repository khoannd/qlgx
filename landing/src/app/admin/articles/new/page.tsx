import Link from "next/link";
import { redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";

import { ArticleForm } from "../ArticleForm";

export const metadata = { title: "Bài viết mới", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

export default async function NewArticlePage({
  searchParams,
}: {
  searchParams: Promise<{ error?: string }>;
}) {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const { error } = await searchParams;

  return (
    <main className="wrap py-16">
      <Link href="/admin/articles" className="link-underline text-[0.9rem] text-ink-soft">
        ← Quay lại danh sách
      </Link>
      <h1 className="mt-4 font-display text-[1.8rem] font-semibold text-ink">Bài viết mới</h1>
      <ArticleForm error={error} />
    </main>
  );
}
