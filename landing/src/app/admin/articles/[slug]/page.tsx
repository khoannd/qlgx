import Link from "next/link";
import { notFound, redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";
import { getArticleRaw } from "@/lib/content/d1-provider";

import { ArticleForm } from "../ArticleForm";

export const metadata = { title: "Sửa bài viết", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

export default async function EditArticlePage({
  params,
  searchParams,
}: {
  params: Promise<{ slug: string }>;
  searchParams: Promise<{ error?: string }>;
}) {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const { slug } = await params;
  const { error } = await searchParams;
  const article = await getArticleRaw(db, slug);
  if (!article) notFound();

  return (
    <main className="wrap py-16">
      <Link href="/admin/articles" className="link-underline text-[0.9rem] text-ink-soft">
        ← Quay lại danh sách
      </Link>
      <h1 className="mt-4 font-display text-[1.8rem] font-semibold text-ink">
        Sửa: {article.title}
      </h1>
      <ArticleForm article={article} error={error} />
    </main>
  );
}
