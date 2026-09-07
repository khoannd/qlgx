import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";

import { AlertIcon, ChevronIcon } from "@/components/Icon";
import { BreadcrumbJsonLd } from "@/components/JsonLd";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { content } from "@/lib/content";
import { getGuidePage, guideNeighbors, guidePages } from "@/lib/huong-dan";
import { FORUM_URL, SITE_URL, formatDate } from "@/lib/site";

type Params = { slug: string };

export function generateStaticParams(): Params[] {
  return guidePages.map((p) => ({ slug: p.slug }));
}

export async function generateMetadata({
  params,
}: {
  params: Promise<Params>;
}): Promise<Metadata> {
  const { slug } = await params;
  const page = getGuidePage(slug);
  if (!page) return { title: "Không tìm thấy trang hướng dẫn" };

  return {
    title: page.title,
    alternates: { canonical: `/huong-dan/${page.slug}` },
    openGraph: {
      type: "article",
      locale: "vi_VN",
      url: `${SITE_URL}/huong-dan/${page.slug}`,
      title: page.title,
    },
  };
}

export default async function GuidePageDetail({ params }: { params: Promise<Params> }) {
  const { slug } = await params;
  const page = getGuidePage(slug);
  if (!page) notFound();

  const [release, { prev, next }] = await Promise.all([
    content.getRelease(),
    Promise.resolve(guideNeighbors(slug)),
  ]);

  return (
    <>
      <BreadcrumbJsonLd
        items={[
          { name: "Trang chủ", url: `${SITE_URL}/` },
          { name: "Hướng dẫn sử dụng", url: `${SITE_URL}/huong-dan` },
          { name: page.title, url: `${SITE_URL}/huong-dan/${page.slug}` },
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
          <header className="wrap pb-8 pt-14 md:pt-20">
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
                    href="/huong-dan"
                    className="link-underline inline-block py-1.5 text-ink-soft"
                  >
                    Hướng dẫn sử dụng
                  </Link>
                </li>
              </ol>
            </nav>

            <p className="text-[0.72rem] font-bold uppercase tracking-[0.13em] text-brand">
              {page.group}
            </p>
            <h1 className="mt-3 max-w-[36ch] font-display text-[clamp(1.9rem,3.8vw,2.7rem)] font-semibold leading-[1.16] text-ink">
              {page.title}
            </h1>
          </header>

          <div className="wrap pb-16">
            <div>
              <div className="glass glass-solid px-6 py-9 md:px-11 md:py-12">
                <p className="mb-7 flex gap-3 rounded-xl border-l-4 border-l-amber bg-amber/8 px-4 py-3 text-[0.86rem] leading-relaxed text-ink">
                  <AlertIcon className="mt-0.5 h-4 w-4 shrink-0 text-amber-ink" />
                  <span>
                    Trang này chuyển thể từ tài liệu hướng dẫn gốc đi kèm phần mềm, nên một vài chỗ
                    có thể còn nhắc tới cấu hình máy tính cũ. Từ bản {release.version}, chương
                    trình chạy trên .NET Framework 4.8 và hỗ trợ Windows 7 đến 11 — xem{" "}
                    <Link href="/#phien-ban-moi" className="link-underline font-semibold">
                      thông tin phiên bản mới nhất
                    </Link>
                    .
                  </span>
                </p>

                {/*
                  Nội dung đã khử trùng bằng sanitize-html tại thời điểm build,
                  từ tệp .htm nằm sẵn trong kho mã (BIN/help/) — không phải dữ
                  liệu người dùng nhập lúc chạy. Xem scripts/import-huong-dan.mjs.
                */}
                <div className="prose-legacy" dangerouslySetInnerHTML={{ __html: page.html }} />
              </div>

              <div className="mt-8 grid gap-3 sm:grid-cols-2">
                {prev ? (
                  <Link
                    href={`/huong-dan/${prev.slug}`}
                    className="glass flex items-center gap-3 px-5 py-4 no-underline"
                  >
                    <ChevronIcon className="h-4 w-4 rotate-180 text-ink-faint" />
                    <span>
                      <span className="block text-[0.76rem] text-ink-faint">Trước đó</span>
                      <span className="font-semibold text-ink">{prev.title}</span>
                    </span>
                  </Link>
                ) : (
                  <span />
                )}
                {next ? (
                  <Link
                    href={`/huong-dan/${next.slug}`}
                    className="glass flex items-center justify-end gap-3 px-5 py-4 text-right no-underline"
                  >
                    <span>
                      <span className="block text-[0.76rem] text-ink-faint">Tiếp theo</span>
                      <span className="font-semibold text-ink">{next.title}</span>
                    </span>
                    <ChevronIcon className="h-4 w-4 text-ink-faint" />
                  </Link>
                ) : (
                  <span />
                )}
              </div>

              <p className="mt-8 text-[0.92rem] text-ink-soft">
                Còn thắc mắc? Xin đăng câu hỏi trên{" "}
                <a href={FORUM_URL} target="_blank" rel="noopener noreferrer" className="link-underline font-semibold text-brand-ink">
                  diễn đàn quanlygiaoxu.net
                </a>
                .
              </p>
            </div>
          </div>
        </article>
      </main>

      <SiteFooter version={release.version} releaseDate={formatDate(release.publishedAt)} />
    </>
  );
}
