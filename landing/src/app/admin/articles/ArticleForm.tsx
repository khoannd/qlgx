import type { ArticleInput } from "@/lib/content/d1-provider";

const TONE_OPTIONS: { value: string; label: string }[] = [
  { value: "cobalt", label: "Xanh dương (cobalt)" },
  { value: "ruby", label: "Tím (ruby)" },
  { value: "amber", label: "Hổ phách (amber)" },
  { value: "emerald", label: "Ngọc lục (emerald)" },
];

const EMPTY_BODY = JSON.stringify(
  [{ type: "paragraph", text: "Nội dung đoạn văn đầu tiên." }],
  null,
  2,
);

/**
 * Form dùng chung cho thêm mới và sửa bài viết.
 *
 * Thân bài viết sửa dưới dạng một ô JSON duy nhất (mảng ArticleBlock) thay vì
 * trình soạn thảo WYSIWYG — chấp nhận đánh đổi cho một công cụ quản trị v1.
 * Sai cú pháp bị chặn ở route handler trước khi ghi, không làm hỏng dữ liệu.
 */
export function ArticleForm({
  article,
  error,
}: {
  article?: ArticleInput;
  error?: string;
}) {
  const isNew = !article;

  return (
    <form method="post" action="/admin/api/articles/save" className="glass mt-8 grid gap-6 p-7">
      <input type="hidden" name="originalSlug" value={article?.slug ?? ""} />

      {error ? (
        <p className="rounded-lg border-l-4 border-l-amber bg-amber/8 px-4 py-3 text-[0.9rem] text-ink">
          {decodeURIComponent(error)}
        </p>
      ) : null}

      <div className="grid gap-5 sm:grid-cols-2">
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Slug (phần đường dẫn — không dấu, gạch ngang)
          <input
            name="slug"
            defaultValue={article?.slug}
            required
            pattern="[a-z0-9-]+"
            placeholder="vi-du-bai-viet"
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 font-mono text-[0.9rem] text-ink outline-none focus:border-brand"
          />
        </label>
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Tiêu đề
          <input
            name="title"
            defaultValue={article?.title}
            required
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
          />
        </label>
      </div>

      <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
        Mô tả ngắn (hiện trên thẻ bài viết)
        <textarea
          name="excerpt"
          defaultValue={article?.excerpt}
          required
          rows={2}
          className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
        />
      </label>

      <div className="grid gap-5 sm:grid-cols-3">
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Chuyên mục (slug)
          <input
            name="categorySlug"
            defaultValue={article?.category.slug}
            required
            placeholder="huong-dan"
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.9rem] text-ink outline-none focus:border-brand"
          />
        </label>
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Tên chuyên mục hiển thị
          <input
            name="categoryName"
            defaultValue={article?.category.name}
            required
            placeholder="Hướng dẫn"
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.9rem] text-ink outline-none focus:border-brand"
          />
        </label>
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Sắc thái màu
          <select
            name="categoryTone"
            defaultValue={article?.category.tone ?? "cobalt"}
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.9rem] text-ink outline-none focus:border-brand"
          >
            {TONE_OPTIONS.map((t) => (
              <option key={t.value} value={t.value}>
                {t.label}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="grid gap-5 sm:grid-cols-3">
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Ngày đăng
          <input
            name="publishedAt"
            type="date"
            defaultValue={article?.publishedAt}
            required
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.9rem] text-ink outline-none focus:border-brand"
          />
        </label>
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Ngày sửa (tuỳ chọn)
          <input
            name="updatedAt"
            type="date"
            defaultValue={article?.updatedAt}
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.9rem] text-ink outline-none focus:border-brand"
          />
        </label>
        <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
          Số phút đọc
          <input
            name="readingMinutes"
            type="number"
            min={1}
            defaultValue={article?.readingMinutes ?? 3}
            required
            className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.9rem] text-ink outline-none focus:border-brand"
          />
        </label>
      </div>

      <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
        Tác giả (tuỳ chọn)
        <input
          name="author"
          defaultValue={article?.author}
          className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.95rem] text-ink outline-none focus:border-brand"
        />
      </label>

      <label className="grid gap-1.5 text-[0.86rem] font-semibold text-ink">
        Thân bài viết (JSON — mảng ArticleBlock: paragraph/heading/list/quote/note/image)
        <textarea
          name="bodyJson"
          defaultValue={article ? JSON.stringify(article.body, null, 2) : EMPTY_BODY}
          required
          rows={18}
          spellCheck={false}
          className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 font-mono text-[0.82rem] text-ink outline-none focus:border-brand"
        />
      </label>

      <button type="submit" className="btn btn-primary w-full sm:w-auto sm:justify-self-start">
        {isNew ? "Tạo bài viết" : "Lưu thay đổi"}
      </button>
    </form>
  );
}
