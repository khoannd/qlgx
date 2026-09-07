import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { notFound } from "next/navigation";

import { ArticleCard } from "@/components/ArticleCard";
import { ChevronIcon } from "@/components/Icon";
import { ArticleJsonLd, BreadcrumbJsonLd } from "@/components/JsonLd";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { toneClasses } from "@/components/glass";
import { content, type ArticleBlock } from "@/lib/content";
import { FORUM_URL, SITE_URL, formatDate } from "@/lib/site";

type Params = { slug: string };

/** Dựng sẵn mọi bài lúc build; bài mới từ CMS vẫn dựng được khi có yêu cầu đầu tiên. */
export async function generateStaticParams(): Promise<Params[]> {
  const slugs = await content.listArticleSlugs();
  return slugs.map((slug) => ({ slug }));
}

/** Dựng lại trang tối đa mỗi 5 phút, để bài sửa trên CMS tự lên mà không cần build lại. */
export const revalidate = 300;

export async function generateMetadata({
  params,
}: {
  params: Promise<Params>;
}): Promise<Metadata> {
  const { slug } = await params;
  const article = await content.getArticle(slug);
  if (!article) return { title: "Không tìm thấy bài viết" };

  const url = `${SITE_URL}/tin-tuc/${article.slug}`;
  return {
    title: article.title,
    description: article.excerpt,
    alternates: { canonical: `/tin-tuc/${article.slug}` },
    openGraph: {
      type: "article",
      locale: "vi_VN",
      url,
      title: article.title,
      description: article.excerpt,
      publishedTime: article.publishedAt,
      modifiedTime: article.updatedAt ?? article.publishedAt,
      section: article.category.name,
    },
    twitter: {
      card: "summary_large_image",
      title: article.title,
      description: article.excerpt,
    },
  };
}

/**
 * Dựng lại thân bài từ các khối có kiểu.
 *
 * Không dùng dangerouslySetInnerHTML ở đâu cả: khi gắn CMS thật, nội dung do người
 * khác nhập vào, và React tự thoát ký tự nên một bài bị chèn mã độc cũng không chạy.
 */
function Block({ block }: { block: ArticleBlock }) {
  switch (block.type) {
    case "heading":
      return (
        <h2 className="mt-12 font-display text-[clamp(1.4rem,2.6vw,1.85rem)] font-semibold leading-snug text-ink">
          {block.text}
        </h2>
      );

    case "paragraph":
      return <p className="mt-5 leading-[1.78] text-ink">{block.text}</p>;

    case "list":
      return (
        <ul className="mt-5 grid list-none gap-3 p-0">
          {block.items.map((item) => (
            <li key={item} className="flex gap-4 leading-[1.72] text-ink">
              <span
                className="mt-2.5 h-2 w-2 shrink-0 rounded-full bg-brand"
                aria-hidden="true"
              />
              {item}
            </li>
          ))}
        </ul>
      );

    case "quote":
      return (
        <blockquote className="glass my-10 border-l-4 border-l-violet px-7 py-6">
          <p className="font-display text-[1.15rem] italic leading-relaxed text-ink">
            “{block.text}”
          </p>
          {block.cite ? (
            <cite className="mt-3 block font-sans text-[0.78rem] font-bold uppercase not-italic tracking-[0.14em] text-violet-ink">
              {block.cite}
            </cite>
          ) : null}
        </blockquote>
      );

    case "note":
      return (
        <aside className="glass my-9 border-l-4 border-l-amber px-7 py-5 text-[0.98rem] leading-relaxed text-ink">
          {block.text}
        </aside>
      );

    case "image":
      return (
        <figure className="my-10">
          <div className="glass overflow-hidden p-2">
            <Image
              src={block.src}
              alt={block.alt}
              width={block.width}
              height={block.height}
              sizes="(min-width: 768px) 720px, 100vw"
              loading="lazy"
              quality={82}
              className="w-full rounded-[12px]"
            />
          </div>
          {block.caption ? (
            <figcaption className="mt-3 text-[0.9rem] text-ink-soft">{block.caption}</figcaption>
          ) : null}
        </figure>
      );
  }
}

export default async function ArticlePage({ params }: { params: Promise<Params> }) {
  const { slug } = await params;
  const [article, release] = await Promise.all([content.getArticle(slug), content.getRelease()]);

  if (!article) notFound();

  const related = (await content.listArticles({ limit: 4 }))
    .filter((a) => a.slug !== slug)
    .slice(0, 3);
  const tone = toneClasses[article.category.tone];

  return (
    <>
      <ArticleJsonLd article={article} />
      <BreadcrumbJsonLd
        items={[
          { name: "Trang chủ", url: `${SITE_URL}/` },
          { name: "Tin tức", url: `${SITE_URL}/tin-tuc` },
          { name: article.title, url: `${SITE_URL}/tin-tuc/${article.slug}` },
        ]}
      />

      <a
        href="#main"
        className="absolute left-4 top-[-100px] z-[999] rounded-full bg-ink px-5 py-3 font-semibold text-white no-underline focus:top-4"
      >
        Bỏ qua, đến nội dung chính
      </a>

      <SiteHeader />

      <main id="main">
        <article>
          <header className="wrap pb-10 pt-14 md:pt-20">
            <nav aria-label="Đường dẫn" className="mb-8 text-[0.86rem] text-ink-soft">
              <ol className="m-0 flex list-none flex-wrap items-center gap-2 p-0">
                <li>
                  <Link href="/" className="link-underline inline-block py-1.5 text-ink-soft">
                    Trang chủ
                  </Link>
                </li>
                <li aria-hidden="true">/</li>
                <li>
                  <Link
                    href="/tin-tuc"
                    className="link-underline inline-block py-1.5 text-ink-soft"
                  >
                    Tin tức
                  </Link>
                </li>
              </ol>
            </nav>

            <div className="flex flex-wrap items-center gap-x-2.5 gap-y-2 text-[0.72rem] font-bold uppercase tracking-[0.13em]">
              <span className={`h-2 w-2 rounded-full ${tone.dot}`} aria-hidden="true" />
              <span className={tone.text}>{article.category.name}</span>
              <span className="text-ink-faint/50" aria-hidden="true">
                ·
              </span>
              <time dateTime={article.publishedAt} className="tabular-nums text-ink-faint">
                {formatDate(article.publishedAt)}
              </time>
              <span className="text-ink-faint/50" aria-hidden="true">
                ·
              </span>
              <span className="tabular-nums text-ink-faint">
                {article.readingMinutes} phút đọc
              </span>
            </div>

            <h1 className="mt-5 max-w-[32ch] font-display text-[clamp(2rem,4.4vw,3.2rem)] font-semibold leading-[1.16] text-ink">
              {article.title}
            </h1>

            <p className="mt-5 max-w-[62ch] text-[1.09rem] leading-relaxed text-ink-soft">
              {article.excerpt}
            </p>

            {article.author ? (
              <p className="mt-7 max-w-[62ch] border-t border-white/70 pt-5 text-[0.9rem] text-ink-soft">
                {article.author}
              </p>
            ) : null}
          </header>

          <div className="wrap pb-16">
            <div className="glass glass-solid px-6 py-10 text-[1.04rem] md:px-12 md:py-14">
              {article.body.map((block, index) => (
                <Block key={index} block={block} />
              ))}

              <div className="mt-14 border-t border-white/70 pt-8">
                <p className="text-[0.97rem] leading-relaxed text-ink-soft">
                  Còn thắc mắc? Xin đăng câu hỏi trên diễn đàn — tác giả và các giáo xứ khác sẽ cùng
                  hỗ trợ.
                </p>
                <a
                  href={FORUM_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn-glass mt-5"
                >
                  Vào diễn đàn quanlygiaoxu.net
                </a>
              </div>
            </div>
          </div>
        </article>

        {related.length > 0 ? (
          <section aria-labelledby="bai-lien-quan" className="wrap pb-20 md:pb-28">
            <div className="flex flex-wrap items-end justify-between gap-6">
              <h2
                id="bai-lien-quan"
                className="font-display text-[clamp(1.5rem,2.8vw,2.1rem)] font-semibold text-ink"
              >
                Bài viết khác
              </h2>
              <Link href="/tin-tuc" className="btn btn-glass h-12 min-h-0">
                Xem tất cả
                <ChevronIcon className="h-4 w-4" />
              </Link>
            </div>

            <ul className="mt-10 grid list-none gap-6 p-0 md:grid-cols-2 lg:grid-cols-3">
              {related.map((item) => (
                <li key={item.slug} className="h-full">
                  <ArticleCard article={item} />
                </li>
              ))}
            </ul>
          </section>
        ) : null}
      </main>

      <SiteFooter version={release.version} releaseDate={formatDate(release.publishedAt)} />
    </>
  );
}
