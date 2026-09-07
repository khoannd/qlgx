import type { Metadata } from "next";
import Link from "next/link";

import { AlertIcon, Icon } from "@/components/Icon";
import { BreadcrumbJsonLd } from "@/components/JsonLd";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { content } from "@/lib/content";
import { FORUM_URL, SITE_URL, formatDate } from "@/lib/site";
import { versionHistory } from "@/lib/version-history";

const title = "Lịch sử phiên bản";
const description =
  "Toàn bộ các phiên bản đã phát hành của phần mềm Quản Lý Giáo Xứ, kèm nội dung thay đổi và bản cài đặt cũ cho những máy chưa cần lên bản mới nhất.";

export const metadata: Metadata = {
  title,
  description,
  alternates: { canonical: "/phien-ban" },
  openGraph: { type: "website", locale: "vi_VN", url: `${SITE_URL}/phien-ban`, title, description },
};

export default async function VersionHistoryPage() {
  const release = await content.getRelease();
  const detailed = versionHistory.filter((v) => v.download);
  const olderOnly = versionHistory.filter((v) => !v.download);

  return (
    <>
      <BreadcrumbJsonLd
        items={[
          { name: "Trang chủ", url: `${SITE_URL}/` },
          { name: title, url: `${SITE_URL}/phien-ban` },
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
        <header className="wrap pb-8 pt-14 md:pt-20">
          <nav aria-label="Đường dẫn" className="mb-8 text-[0.86rem] text-ink-soft">
            <ol className="m-0 flex list-none flex-wrap items-center gap-2 p-0">
              <li>
                <Link href="/" className="link-underline inline-block py-1.5 text-ink-soft">
                  Trang chủ
                </Link>
              </li>
              <li aria-hidden="true">/</li>
              <li className="text-ink">{title}</li>
            </ol>
          </nav>

          <h1 className="max-w-[24ch] font-display text-[clamp(2.2rem,4.6vw,3.4rem)] font-semibold leading-[1.16] text-ink">
            {title}
          </h1>
          <p className="mt-5 max-w-[68ch] text-[1.06rem] leading-relaxed text-ink-soft">
            Nội dung thay đổi của từng bản, tính từ bản {release.version} trở về trước. Muốn tải bản{" "}
            mới nhất thì xem{" "}
            <Link href="/#tai-ve" className="link-underline font-semibold">
              trang chủ
            </Link>
            .
          </p>

          <p className="glass mt-7 flex max-w-[68ch] gap-3 border-l-4 border-l-amber bg-amber/8 px-5 py-4 text-[0.9rem] leading-relaxed text-ink">
            <AlertIcon className="mt-0.5 h-4 w-4 shrink-0 text-amber-ink" />
            <span>
              Thông tin này trước đây nằm trên diễn đàn forum.quanlygiaoxu.net. Diễn đàn dự kiến sẽ
              ngừng hoạt động trong tương lai, nên từ nay trang này là nơi lưu giữ lịch sử phiên bản
              lâu dài.
            </span>
          </p>
        </header>

        <section aria-labelledby="ban-co-the-tai" className="wrap pb-16">
          <h2 id="ban-co-the-tai" className="sr-only">
            Các phiên bản còn tải được
          </h2>
          <ul className="m-0 grid list-none gap-6 p-0">
            {detailed.map((v) => (
              <li key={v.label} className="glass p-7 md:p-9">
                <div className="flex flex-wrap items-start justify-between gap-4">
                  <div>
                    <p className="font-display text-[1.4rem] font-semibold text-ink">
                      Phiên bản {v.label}
                      {v.label === release.version ? (
                        <span className="ml-3 rounded-full bg-gradient-to-br from-brand to-brand-deep px-2.5 py-1 align-middle text-[0.66rem] font-bold uppercase tracking-[0.1em] text-white">
                          Mới nhất
                        </span>
                      ) : null}
                    </p>
                    {v.date ? (
                      <p className="mt-1 text-[0.86rem] tabular-nums text-ink-faint">
                        Phát hành ngày <time dateTime={v.date}>{formatDate(v.date)}</time>
                      </p>
                    ) : (
                      <p className="mt-1 text-[0.86rem] text-ink-faint">
                        Chưa xác định được ngày phát hành chính xác
                      </p>
                    )}
                  </div>
                  <a
                    href={
                      v.label === release.version
                        ? "/api/tai-ve/full"
                        : `/api/tai-ve/phien-ban-cu/${encodeURIComponent(v.label)}`
                    }
                    className="btn btn-glass h-11 min-h-0 shrink-0"
                  >
                    <Icon name="download" strokeWidth={1.9} />
                    Tải {v.download?.size ? `(${v.download.size})` : null}
                  </a>
                </div>

                {v.highlights ? (
                  <ul className="mt-6 grid list-none gap-2.5 border-t border-white/60 p-0 pt-6">
                    {v.highlights.map((item) => (
                      <li key={item} className="flex gap-3 text-[0.95rem] leading-relaxed text-ink">
                        <span
                          className="mt-2.5 h-1.5 w-1.5 shrink-0 rounded-full bg-brand"
                          aria-hidden="true"
                        />
                        {item}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-6 border-t border-white/60 pt-6 text-[0.9rem] text-ink-soft">
                    Không có thông báo riêng cho bản này trên diễn đàn — không rõ nội dung thay đổi
                    cụ thể.
                  </p>
                )}

                {v.sourceUrl ? (
                  <a
                    href={v.sourceUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="link-underline mt-4 inline-block text-[0.84rem] text-ink-faint"
                  >
                    Xem bài đăng gốc trên diễn đàn
                  </a>
                ) : null}
              </li>
            ))}
          </ul>
        </section>

        <section aria-labelledby="ban-cu-hon" className="wrap pb-24">
          <h2
            id="ban-cu-hon"
            className="font-display text-[1.4rem] font-semibold text-ink"
          >
            Các phiên bản cũ hơn
          </h2>
          <p className="mt-2 max-w-[68ch] text-[0.94rem] leading-relaxed text-ink-soft">
            Không còn tệp cài đặt để cung cấp tải về — chỉ còn mốc thời gian, giữ lại cho đủ dòng
            lịch sử.
          </p>

          <div className="glass mt-6 overflow-x-auto p-2">
            <table className="w-full min-w-[420px] border-collapse text-[0.92rem]">
              <thead>
                <tr className="text-left text-[0.72rem] font-bold uppercase tracking-[0.1em] text-ink-faint">
                  <th className="px-5 py-3">Phiên bản</th>
                  <th className="px-5 py-3">Ngày phát hành</th>
                  <th className="px-5 py-3">Nguồn</th>
                </tr>
              </thead>
              <tbody>
                {olderOnly.map((v) => (
                  <tr key={v.label} className="border-t border-white/60">
                    <td className="px-5 py-3 font-semibold text-ink">{v.label}</td>
                    <td className="px-5 py-3 tabular-nums text-ink-soft">
                      {v.date ? <time dateTime={v.date}>{formatDate(v.date)}</time> : "—"}
                    </td>
                    <td className="px-5 py-3">
                      {v.sourceUrl ? (
                        <a
                          href={v.sourceUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="link-underline text-brand-ink"
                        >
                          Bài đăng gốc
                        </a>
                      ) : (
                        <span className="text-ink-faint">—</span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <p className="mt-8 text-[0.92rem] text-ink-soft">
            Còn thắc mắc về một bản cụ thể? Xin đăng câu hỏi trên{" "}
            <a
              href={FORUM_URL}
              target="_blank"
              rel="noopener noreferrer"
              className="link-underline font-semibold text-brand-ink"
            >
              diễn đàn quanlygiaoxu.net
            </a>
            .
          </p>
        </section>
      </main>

      <SiteFooter version={release.version} releaseDate={formatDate(release.publishedAt)} />
    </>
  );
}
