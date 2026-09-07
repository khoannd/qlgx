import type { Metadata } from "next";
import Link from "next/link";

import { ChevronIcon, Icon } from "@/components/Icon";
import { BreadcrumbJsonLd } from "@/components/JsonLd";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { Eyebrow } from "@/components/glass";
import { content } from "@/lib/content";
import { listGuideGroups } from "@/lib/huong-dan";
import { FORUM_URL, SITE_URL, formatDate } from "@/lib/site";

const title = "Hướng dẫn sử dụng";
const description =
  "Toàn bộ hướng dẫn sử dụng phần mềm Quản Lý Giáo Xứ: cài đặt, nhập dữ liệu, quản lý giáo dân, gia đình, sổ bí tích, thống kê, sao lưu và các tuỳ chọn.";

export const metadata: Metadata = {
  title,
  description,
  alternates: { canonical: "/huong-dan" },
  openGraph: { type: "website", locale: "vi_VN", url: `${SITE_URL}/huong-dan`, title, description },
};

const GROUP_ICON: Record<string, "download" | "database" | "chart" | "shield" | "help"> = {
  "Bắt đầu": "download",
  "Nhập & quản lý dữ liệu": "database",
  "Tra cứu & xử lý dữ liệu": "chart",
  "Vận hành & an toàn dữ liệu": "shield",
  "Hỗ trợ": "help",
};

export default async function GuideIndexPage() {
  const release = await content.getRelease();
  const groups = listGuideGroups();

  return (
    <>
      <BreadcrumbJsonLd
        items={[
          { name: "Trang chủ", url: `${SITE_URL}/` },
          { name: title, url: `${SITE_URL}/huong-dan` },
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

          <Eyebrow>Hướng dẫn sử dụng</Eyebrow>
          <h1 className="mt-4 max-w-[24ch] font-display text-[clamp(2.2rem,4.6vw,3.4rem)] font-semibold leading-[1.16] text-ink">
            Mọi màn hình đều có hướng dẫn
          </h1>
          <p className="mt-5 max-w-[62ch] text-[1.06rem] leading-relaxed text-ink-soft">
            {description}
          </p>
        </section>

        <section aria-label="Danh mục hướng dẫn" className="wrap pb-20 md:pb-28">
          <div className="grid gap-10">
            {groups.map(({ group, pages }) => (
              <div key={group}>
                <h2 className="flex items-center gap-3 font-display text-[1.3rem] font-semibold text-ink">
                  <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-brand/12">
                    <Icon name={GROUP_ICON[group] ?? "book"} className="h-5 w-5 text-brand-ink" />
                  </span>
                  {group}
                </h2>
                <ul className="mt-4 grid list-none gap-3 p-0 sm:grid-cols-2 lg:grid-cols-3">
                  {pages.map((page) => (
                    <li key={page.slug}>
                      <Link
                        href={`/huong-dan/${page.slug}`}
                        className="glass group flex min-h-[64px] items-center gap-3 px-5 py-4 no-underline transition-transform duration-200 hover:-translate-y-0.5"
                      >
                        <span className="flex-1 text-[0.96rem] font-semibold text-ink">
                          {page.title}
                        </span>
                        <ChevronIcon className="h-4 w-4 shrink-0 text-ink-faint transition-transform group-hover:translate-x-1" />
                      </Link>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>

          <div className="glass mt-12 flex flex-wrap items-center justify-between gap-5 p-7">
            <div>
              <h2 className="font-display text-[1.15rem] font-semibold text-ink">
                Không tìm thấy điều cần tra?
              </h2>
              <p className="mt-1 text-[0.92rem] text-ink-soft">
                Đặt câu hỏi trên diễn đàn — tác giả và các giáo xứ khác sẽ cùng hỗ trợ.
              </p>
            </div>
            <a
              href={FORUM_URL}
              target="_blank"
              rel="noopener noreferrer"
              className="btn btn-glass h-11 min-h-0"
            >
              Vào diễn đàn
            </a>
          </div>
        </section>
      </main>

      <SiteFooter version={release.version} releaseDate={formatDate(release.publishedAt)} />
    </>
  );
}
