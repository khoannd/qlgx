import { Fragment } from "react";

import type { LandingContent } from "@/lib/content";

import { ICON_OPTIONS } from "./icon-options";

/**
 * Form sửa các danh sách nội dung của trang chủ (con số tóm tắt, vấn đề của sổ
 * giấy, tính năng, bước cài đặt, liên kết hỗ trợ, câu hỏi thường gặp).
 *
 * KHÔNG dùng JavaScript phía trình duyệt để thêm/bớt dòng (khác hẳn một trình
 * soạn CMS thông thường) — toàn bộ /admin cố ý chỉ dùng form HTML thuần, POST
 * thẳng tới route handler, để không phụ thuộc hydrate/React trên trình duyệt.
 * Thay vào đó, mỗi danh sách luôn render thêm vài DÒNG TRỐNG phía sau các mục
 * đã có sẵn — muốn thêm mục mới thì gõ vào một dòng trống, muốn xoá thì xoá
 * trắng ô đầu tiên của dòng đó rồi lưu. Route handler tự bỏ qua dòng trống.
 * Đơn giản hơn cho một trang có nhiều danh sách khác kích thước, và giữ đúng
 * triết lý "công cụ nội bộ dùng bởi một người" đã áp dụng cho toàn bộ /admin.
 */

const EXTRA_BLANK_ROWS = 3;

function withBlankRows<T>(items: readonly T[], extra = EXTRA_BLANK_ROWS): (T | undefined)[] {
  return [...items, ...Array.from({ length: extra }, () => undefined)];
}

const inputClass =
  "rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.92rem] text-ink outline-none focus:border-brand";
const labelClass = "grid gap-1.5 text-[0.82rem] font-semibold text-ink-soft";

function Section({
  title,
  hint,
  children,
}: {
  title: string;
  hint?: string;
  children: React.ReactNode;
}) {
  return (
    <fieldset className="glass border-0 p-7">
      {/* `float-left w-full` để legend nằm trong dòng chảy bình thường của thẻ:
          mặc định trình duyệt đặt legend chồng lên đường viền fieldset, ở đây
          thẻ không có viền nên nó trồi hẳn ra ngoài nền kính. */}
      <legend className="float-left w-full px-0 font-display text-[1.2rem] font-semibold text-ink">
        {title}
      </legend>
      {hint ? <p className="clear-both pt-2 text-[0.86rem] text-ink-soft">{hint}</p> : null}
      <div className="clear-both pt-5">{children}</div>
    </fieldset>
  );
}

/**
 * Ranh giới giữa các mục đang hiển thị trên trang và các dòng trống để thêm
 * mới — nếu không có vạch này, dòng trống trông y hệt dòng đã có nội dung.
 */
function BlankDivider() {
  return (
    <p className="flex items-center gap-3 pt-2 text-[0.78rem] font-semibold uppercase tracking-[0.1em] text-ink-faint">
      <span className="h-px flex-1 bg-ink-faint/25" />
      Dòng trống — điền vào để thêm mục mới
      <span className="h-px flex-1 bg-ink-faint/25" />
    </p>
  );
}

export function LandingContentForm({
  content,
  error,
}: {
  content: LandingContent;
  error?: string;
}) {
  return (
    <form method="post" action="/admin/api/landing" className="mt-8 grid gap-6">
      {error ? (
        <p className="rounded-lg border-l-4 border-l-amber bg-amber/8 px-4 py-3 text-[0.9rem] text-ink">
          {decodeURIComponent(error)}
        </p>
      ) : null}

      <Section
        title="Con số tóm tắt"
        hint="Bốn ô hiện ngay dưới banner đầu trang (ví dụ: “Miễn phí — Không giới hạn số giáo dân”)."
      >
        <div className="grid gap-3">
          {withBlankRows(content.trustStats, 2).map((stat, i) => (
            <Fragment key={i}>
              {i === content.trustStats.length ? <BlankDivider /> : null}
              <div className="grid gap-3 sm:grid-cols-[1fr_2fr]">
                <label className={labelClass}>
                  Con số
                  <input
                    name={`ts_value_${i}`}
                    defaultValue={stat?.value}
                    placeholder="Ví dụ: Miễn phí"
                    className={inputClass}
                  />
                </label>
                <label className={labelClass}>
                  Mô tả
                  <input
                    name={`ts_label_${i}`}
                    defaultValue={stat?.label}
                    placeholder="Ví dụ: Không giới hạn số giáo dân"
                    className={inputClass}
                  />
                </label>
              </div>
            </Fragment>
          ))}
        </div>
      </Section>

      <Section
        title="Vấn đề của sổ bộ giấy"
        hint="Các khó khăn khi còn dùng sổ giấy, nêu ở phần “Vì sao cần phần mềm”."
      >
        <div className="grid gap-4">
          {withBlankRows(content.painPoints, 2).map((point, i) => (
            <Fragment key={i}>
              {i === content.painPoints.length ? <BlankDivider /> : null}
              <div className="grid gap-2 border-b border-ink-faint/20 pb-5 last:border-b-0 last:pb-0">
                <label className={labelClass}>
                  Tiêu đề
                  <input name={`pp_title_${i}`} defaultValue={point?.title} className={inputClass} />
                </label>
                <label className={labelClass}>
                  Nội dung
                  <textarea
                    name={`pp_body_${i}`}
                    defaultValue={point?.body}
                    rows={3}
                    className={inputClass}
                  />
                </label>
              </div>
            </Fragment>
          ))}
        </div>
      </Section>

      <Section
        title="Tính năng"
        hint="Các thẻ trong mục “Tính năng”. Icon chỉ chọn được từ danh sách có sẵn — chưa hỗ trợ icon tuỳ ý."
      >
        <div className="grid gap-4">
          {withBlankRows(content.features).map((feature, i) => (
            <Fragment key={i}>
              {i === content.features.length ? <BlankDivider /> : null}
              <div className="grid gap-3 border-b border-ink-faint/20 pb-5 last:border-b-0 last:pb-0">
              <div className="grid gap-3 sm:grid-cols-[1fr_1fr_2fr]">
                <label className={labelClass}>
                  Icon
                  <select
                    name={`feat_icon_${i}`}
                    defaultValue={feature?.icon ?? ""}
                    className={inputClass}
                  >
                    <option value="">— Không dùng dòng này —</option>
                    {ICON_OPTIONS.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                </label>
                <label className={labelClass}>
                  Sắc thái màu
                  <select
                    name={`feat_tone_${i}`}
                    defaultValue={feature?.tone ?? "cobalt"}
                    className={inputClass}
                  >
                    <option value="cobalt">Xanh dương (cobalt)</option>
                    <option value="ruby">Tím (ruby)</option>
                    <option value="amber">Hổ phách (amber)</option>
                    <option value="emerald">Ngọc lục (emerald)</option>
                  </select>
                </label>
                <label className={labelClass}>
                  Tiêu đề
                  <input
                    name={`feat_title_${i}`}
                    defaultValue={feature?.title}
                    className={inputClass}
                  />
                </label>
              </div>
                <label className={labelClass}>
                  Mô tả
                  <textarea
                    name={`feat_body_${i}`}
                    defaultValue={feature?.body}
                    rows={3}
                    className={inputClass}
                  />
                </label>
              </div>
            </Fragment>
          ))}
        </div>
        <p className="text-[0.8rem] text-ink-faint">
          Để trống ô Icon để bỏ qua cả dòng (dùng khi muốn xoá một tính năng, hoặc dòng đó chỉ là
          chỗ trống chưa dùng tới).
        </p>
      </Section>

      <Section
        title="Các bước cài đặt"
        hint="Danh sách đánh số ở mục “Cài đặt” — theo đúng thứ tự sẽ hiện trên trang."
      >
        <div className="grid gap-3">
          {withBlankRows(content.installSteps).map((step, i) => (
            <Fragment key={i}>
              {i === content.installSteps.length ? <BlankDivider /> : null}
              <label className={labelClass}>
                Bước {i + 1}
                <textarea
                  name={`step_${i}`}
                  defaultValue={step}
                  rows={3}
                  className={inputClass}
                />
              </label>
            </Fragment>
          ))}
        </div>
      </Section>

      <Section title="Liên kết hỗ trợ" hint="Các thẻ trong mục “Hỗ trợ” ở cuối trang chủ.">
        <div className="grid gap-4">
          {withBlankRows(content.helpLinks, 2).map((link, i) => (
            <Fragment key={i}>
              {i === content.helpLinks.length ? <BlankDivider /> : null}
              <div className="grid gap-3 border-b border-ink-faint/20 pb-5 last:border-b-0 last:pb-0">
              <div className="grid gap-3 sm:grid-cols-[1fr_1.3fr_1.3fr]">
                <label className={labelClass}>
                  Icon
                  <select
                    name={`link_icon_${i}`}
                    defaultValue={link?.icon ?? ""}
                    className={inputClass}
                  >
                    <option value="">— Không dùng dòng này —</option>
                    {ICON_OPTIONS.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                </label>
                <label className={labelClass}>
                  Tiêu đề
                  <input name={`link_title_${i}`} defaultValue={link?.title} className={inputClass} />
                </label>
                <label className={labelClass}>
                  Mô tả phụ
                  <input
                    name={`link_subtitle_${i}`}
                    defaultValue={link?.subtitle}
                    className={inputClass}
                  />
                </label>
              </div>
              <div className="grid gap-3 sm:grid-cols-[3fr_1fr] sm:items-end">
                <label className={labelClass}>
                  Đường dẫn (nội bộ như /huong-dan/cai-dat, hoặc URL đầy đủ)
                  <input
                    name={`link_href_${i}`}
                    defaultValue={link?.href}
                    className={`${inputClass} font-mono text-[0.86rem]`}
                  />
                </label>
                  <label className="flex items-center gap-2 pb-2.5 text-[0.86rem] font-semibold text-ink-soft">
                    <input
                      type="checkbox"
                      name={`link_external_${i}`}
                      defaultChecked={link?.external}
                      className="h-4 w-4"
                    />
                    Mở tab mới
                  </label>
                </div>
              </div>
            </Fragment>
          ))}
        </div>
        <p className="text-[0.8rem] text-ink-faint">Để trống ô Icon để bỏ qua cả dòng.</p>
      </Section>

      <Section
        title="Câu hỏi thường gặp"
        hint="Mỗi câu trả lời có thể gồm nhiều đoạn — cách nhau bằng một dòng trống."
      >
        <div className="grid gap-4">
          {withBlankRows(content.faqs, 2).map((faq, i) => (
            <Fragment key={i}>
              {i === content.faqs.length ? <BlankDivider /> : null}
              <div className="grid gap-2 border-b border-ink-faint/20 pb-5 last:border-b-0 last:pb-0">
                <label className={labelClass}>
                  Câu hỏi
                  <input
                    name={`faq_question_${i}`}
                    defaultValue={faq?.question}
                    className={inputClass}
                  />
                </label>
                <label className={labelClass}>
                  Câu trả lời
                  <textarea
                    name={`faq_answer_${i}`}
                    defaultValue={faq?.answer.join("\n\n")}
                    rows={6}
                    className={inputClass}
                  />
                </label>
              </div>
            </Fragment>
          ))}
        </div>
      </Section>

      <button
        type="submit"
        className="btn btn-primary w-full sm:w-auto sm:justify-self-start"
      >
        Lưu toàn bộ nội dung trang chủ
      </button>
    </form>
  );
}
