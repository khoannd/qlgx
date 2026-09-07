import type { Metadata } from "next";
import Link from "next/link";

import { ArticleCard } from "@/components/ArticleCard";
import { BreadcrumbJsonLd } from "@/components/JsonLd";
import { Reveal } from "@/components/Reveal";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { Eyebrow, Sheen, toneClasses } from "@/components/glass";
import { content } from "@/lib/content";
import { SITE_URL, formatDate } from "@/lib/site";

const title = "Tin tức & hướng dẫn";
const description =
  "Thông báo phát hành, hướng dẫn sử dụng và kinh nghiệm số hoá sổ bộ từ các giáo xứ đang dùng phần mềm Quản Lý Giáo Xứ.";

export const metadata: Metadata = {
  title,
  description,
  alternates: { canonical: "/tin-tuc" },
  openGraph: {
    type: "website",
    locale: "vi_VN",
    url: `${SITE_URL}/tin-tuc`,
    title,
    description,
  },
};

/** Cùng lý do với trang chủ: cho bài viết mới/sửa từ /admin lên trong 5 phút. */
export const revalidate = 300;

export default async function NewsPage() {
  const [articles, release] = await Promise.all([content.listArticles(), content.getRelease()]);
  const [featured, ...rest] = articles;

  return (
    <>
      <BreadcrumbJsonLd
        items={[
          { name: "Trang chủ", url: `${SITE_URL}/` },
          { name: title, url: `${SITE_URL}/tin-tuc` },
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
        <section className="wrap pb-10 pt-14 md:pt-20">
          <nav aria-label="Đường dẫn" className="mb-8 text-[0.86rem] text-ink-soft">
            <ol className="m-0 flex list-none flex-wrap items-center gap-2 p-0">
              <li>
                <Link href="/" className="link-underline inline-block py-1.5 text-ink-soft">
                  Trang chủ
                </Link>
              </li>
              <li aria-hidden="true">/</li>
              <li aria-current="page" className="font-semibold text-ink">
                {title}
              </li>
            </ol>
          </nav>

          <Eyebrow>Tin tức &amp; hướng dẫn</Eyebrow>
          <h1 className="mt-4 max-w-[20ch] font-display text-[clamp(2.2rem,4.6vw,3.4rem)] font-semibold leading-[1.16] text-ink">
            Thông báo, hướng dẫn và kinh nghiệm
          </h1>
          <p className="mt-5 max-w-[60ch] text-[1.05rem] leading-relaxed text-ink-soft">
            {description}
          </p>
        </section>

        <section aria-label="Danh sách bài viết" className="wrap pb-20 md:pb-28">
          {featured ? (
            <Reveal>
              <article className="group glass relative grid overflow-hidden md:grid-cols-[1.35fr_1fr]">
                <Sheen />
                <div className="relative p-8 md:p-12">
                  <div className="flex flex-wrap items-center gap-x-2.5 gap-y-1 text-[0.72rem] font-bold uppercase tracking-[0.13em]">
                    <span className="rounded-full bg-gradient-to-br from-brand to-brand-deep px-2.5 py-1 text-white">
                      Mới nhất
                    </span>
                    <span
                      className={`h-2 w-2 rounded-full ${toneClasses[featured.category.tone].dot}`}
                      aria-hidden="true"
                    />
                    <span className={toneClasses[featured.category.tone].text}>
                      {featured.category.name}
                    </span>
                    <span className="text-ink-faint/50" aria-hidden="true">
                      ·
                    </span>
                    <time dateTime={featured.publishedAt} className="tabular-nums text-ink-faint">
                      {formatDate(featured.publishedAt)}
                    </time>
                  </div>

                  <h2 className="mt-4 max-w-[22ch] font-display text-[clamp(1.6rem,3.2vw,2.3rem)] font-semibold leading-[1.2] text-ink">
                    <Link
                      href={`/tin-tuc/${featured.slug}`}
                      className="text-ink no-underline after:absolute after:inset-0 after:content-[''] group-hover:underline group-hover:decoration-2 group-hover:underline-offset-4"
                    >
                      {featured.title}
                    </Link>
                  </h2>

                  <p className="mt-5 max-w-[58ch] text-[1.01rem] leading-relaxed text-ink-soft">
                    {featured.excerpt}
                  </p>

                  <p className="mt-6 text-[0.88rem] tabular-nums text-ink-faint">
                    {featured.readingMinutes} phút đọc
                    {featured.author ? ` · ${featured.author}` : ""}
                  </p>
                </div>

                {/* Mảng chuyển sắc thay cho ảnh bìa — bài viết chưa có ảnh riêng */}
                <div
                  aria-hidden="true"
                  className="relative hidden min-h-[260px] md:block [background:radial-gradient(420px_260px_at_70%_20%,rgba(59,130,246,0.55),transparent_65%),radial-gradient(380px_240px_at_20%_85%,rgba(124,107,240,0.5),transparent_62%),radial-gradient(320px_220px_at_95%_95%,rgba(20,184,166,0.4),transparent_60%)]"
                />
              </article>
            </Reveal>
          ) : null}

          {rest.length > 0 ? (
            <ul className="mt-6 grid list-none gap-6 p-0 md:grid-cols-2 lg:grid-cols-3">
              {rest.map((article, index) => (
                <Reveal as="li" key={article.slug} delay={(index % 3) * 70} className="h-full">
                  <ArticleCard article={article} />
                </Reveal>
              ))}
            </ul>
          ) : null}

          {articles.length === 0 ? (
            <p className="glass p-10 text-center text-ink-soft">
              Chưa có bài viết nào. Xin mời quay lại sau.
            </p>
          ) : null}
        </section>
      </main>

      <SiteFooter version={release.version} releaseDate={formatDate(release.publishedAt)} />
    </>
  );
}
