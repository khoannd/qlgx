import Link from "next/link";

import { ChevronIcon } from "@/components/Icon";
import { Sheen, toneClasses } from "@/components/glass";
import type { ArticleSummary } from "@/lib/content";
import { formatDate } from "@/lib/site";

/**
 * Thẻ giới thiệu một bài viết.
 *
 * Chấm màu theo chuyên mục giúp nhận ra loại bài từ xa, nhưng màu không phải
 * thông tin duy nhất — tên chuyên mục luôn hiển thị bằng chữ ngay bên cạnh.
 */
export function ArticleCard({ article }: { article: ArticleSummary }) {
  const tone = toneClasses[article.category.tone];

  return (
    <article className="group glass relative flex h-full flex-col overflow-hidden p-6 transition-transform duration-300 hover:-translate-y-1.5">
      <Sheen />

      <div className="relative flex flex-1 flex-col">
        <div className="flex flex-wrap items-center gap-x-2.5 gap-y-1 text-[0.72rem] font-bold uppercase tracking-[0.13em]">
          <span className={`h-2 w-2 rounded-full ${tone.dot}`} aria-hidden="true" />
          <span className={tone.text}>{article.category.name}</span>
          <span className="text-ink-faint/50" aria-hidden="true">
            ·
          </span>
          <time dateTime={article.publishedAt} className="tabular-nums text-ink-faint">
            {formatDate(article.publishedAt)}
          </time>
        </div>

        <h3 className="mt-3.5 font-display text-[1.32rem] font-semibold leading-snug text-ink">
          <Link
            href={`/tin-tuc/${article.slug}`}
            className="text-ink no-underline after:absolute after:inset-0 after:content-[''] group-hover:underline group-hover:decoration-2 group-hover:underline-offset-4"
          >
            {article.title}
          </Link>
        </h3>

        <p className="mt-3 flex-1 text-[0.95rem] leading-relaxed text-ink-soft">{article.excerpt}</p>

        <p className="mt-6 flex items-center gap-2 text-[0.86rem] font-semibold text-brand-ink">
          Đọc tiếp
          <ChevronIcon className="h-4 w-4 transition-transform duration-300 group-hover:translate-x-1" />
          <span className="ml-auto font-normal tabular-nums text-ink-faint">
            {article.readingMinutes} phút đọc
          </span>
        </p>
      </div>
    </article>
  );
}
