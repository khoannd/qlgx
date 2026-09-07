import Image from "next/image";
import Link from "next/link";

import { ArticleCard } from "@/components/ArticleCard";
import { CheckIcon, ChevronIcon, Icon } from "@/components/Icon";
import { HomeJsonLd } from "@/components/JsonLd";
import { Reveal } from "@/components/Reveal";
import { Screenshots } from "@/components/Screenshots";
import { SiteFooter } from "@/components/SiteFooter";
import { SiteHeader } from "@/components/SiteHeader";
import { Eyebrow, Sheen, ShotFrame, toneClasses } from "@/components/glass";
import { content } from "@/lib/content";
import {
  FORUM_URL,
  formatDate,
  heroScreenshot,
  heroSecondaryShot,
  screenshots,
  site,
} from "@/lib/site";


/** Cho phép nội dung sửa từ /admin (D1) lên trang chủ trong tối đa 5 phút mà
 *  không cần build lại. Trên `next dev`/`next start` (không có D1) giá trị
 *  này không có tác dụng gì thêm — trang vẫn render như server component thường. */
export const revalidate = 300;

export default async function HomePage() {
  const [release, articles, landing] = await Promise.all([
    content.getRelease(),
    content.listArticles({ limit: 3 }),
    content.getLandingContent(),
  ]);
  const { trustStats, painPoints, features, installSteps, helpLinks, faqs } = landing;

  const primaryDownload = release.downloads.find((d) => d.primary) ?? release.downloads[0];

  return (
    <>
      <HomeJsonLd release={release} faqs={faqs} />

      <a
        href="#main"
        className="absolute left-4 top-[-100px] z-[999] rounded-full bg-ink px-5 py-3 font-semibold text-white no-underline focus:top-4"
      >
        Bỏ qua, đến nội dung chính
      </a>

      <SiteHeader />

      <main id="main">
        {/* ==================== HERO ==================== */}
        <section className="wrap relative pb-20 pt-14 md:pb-28 md:pt-20">
          <div className="grid items-center gap-14 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.08fr)] lg:gap-16">
            <div>
              <span className="glass inline-flex items-center gap-2.5 rounded-full py-1.5 pl-1.5 pr-4 text-[0.85rem]">
                <span className="rounded-full bg-gradient-to-br from-brand to-brand-deep px-2.5 py-1 text-[0.68rem] font-bold uppercase tracking-[0.1em] text-white">
                  Bản mới
                </span>
                <span className="tabular-nums text-ink-soft">
                  {release.version} · phát hành{" "}
                  <time dateTime={release.publishedAt}>{formatDate(release.publishedAt)}</time>
                </span>
              </span>

              {/* Khoảng dòng để rộng 1.28 là có chủ đích: tiếng Việt có dấu nặng nằm
                  DƯỚI chữ (ọ, ộ, ụ). Thu lại là dấu chạm vào dòng kế tiếp. */}
              {/* Lora có bề ngang chữ rộng hơn Fraunces (font trước đó): ở cỡ tối
                  đa cũ (4.15rem) dòng "Sổ bộ của giáo xứ," tự gãy thành 2 dòng
                  tại độ rộng cột hero, phá vỡ bố cục 2-dòng dự tính. Giảm mức
                  trần cỡ chữ để dòng đầu luôn vừa một dòng ở màn hình desktop. */}
              <h1 className="mt-7 font-display text-[clamp(2.3rem,4.6vw,3.6rem)] font-semibold leading-[1.28] tracking-[-0.015em] text-ink">
                Sổ bộ của giáo xứ,
                <br />
                <span className="bg-gradient-to-r from-brand via-brand-deep to-violet bg-clip-text text-transparent">
                  {/* Khoảng trắng không ngắt giữ "máy tính." dính liền nhau khi
                      dòng này tự gãy — không để một mình chữ "tính." mồ côi
                      xuống dòng riêng. */}
                  gọn trong một máy&nbsp;tính.
                </span>
              </h1>

              <p className="mt-6 max-w-[54ch] text-[1.07rem] leading-relaxed text-ink-soft">
                Giữ trọn hồ sơ giáo dân, gia đình, sổ bí tích, lớp giáo lý và hội đoàn. Tra cứu
                trong vài giây, in phiếu và chứng nhận ngay trên Word hoặc Excel.
              </p>

              <div className="mt-9 flex flex-wrap gap-4">
                <Link href="#tai-ve" className="btn btn-primary w-full xs:w-auto">
                  <Icon name="download" strokeWidth={1.9} />
                  Tải miễn phí cho Windows
                </Link>
                <a
                  href={FORUM_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn-glass w-full xs:w-auto"
                >
                  Xem hướng dẫn
                  <Icon name="external" strokeWidth={1.9} />
                </a>
              </div>

              <ul className="mt-8 flex flex-wrap gap-x-7 gap-y-2.5 p-0 text-[0.93rem] text-ink-soft">
                {["Hoàn toàn miễn phí", "Dữ liệu lưu tại giáo xứ", "Không cần Internet"].map(
                  (item) => (
                    <li key={item} className="flex list-none items-center gap-2">
                      <CheckIcon className="h-4 w-4 shrink-0 text-mint" />
                      {item}
                    </li>
                  ),
                )}
              </ul>
            </div>

            {/* Hai tấm kính xếp chồng lệch nhau, tấm sau nhỏ hơn và mờ hơn */}
            <div className="relative">
              <ShotFrame label={heroScreenshot.label} className="relative z-10">
                <Image
                  src={heroScreenshot.src}
                  alt={heroScreenshot.alt}
                  width={heroScreenshot.width}
                  height={heroScreenshot.height}
                  sizes="(min-width: 1024px) 660px, 100vw"
                  priority
                  quality={82}
                  className="w-full"
                />
              </ShotFrame>

              {/* Tấm thứ hai là một mẩu CẮT SÁT ở tỉ lệ gần 1:1, không phải cả màn
                  hình thu nhỏ. Thu nguyên màn hình 852px xuống 360px thì chữ bên
                  trong thành vệt nhiễu, chẳng ai đọc được gì. Cắt sát thì đọc được
                  thật, và khoe đúng phần Sổ bí tích. */}
              <div className="absolute -bottom-16 -right-2 z-20 hidden w-[370px] xl:block">
                <ShotFrame label={heroSecondaryShot.label} floating={false}>
                  <div className="h-[184px] overflow-hidden">
                    <Image
                      src={heroSecondaryShot.src}
                      alt={heroSecondaryShot.alt}
                      width={heroSecondaryShot.width}
                      height={heroSecondaryShot.height}
                      quality={86}
                      className="max-w-none"
                      style={{ width: heroSecondaryShot.width }}
                    />
                  </div>
                </ShotFrame>
              </div>
            </div>
          </div>
        </section>

        {/* ==================== THANH TIN CẬY ==================== */}
        <section aria-label="Thông tin tổng quan về phần mềm" className="wrap">
          <ul className="glass grid list-none grid-cols-2 gap-y-8 p-0 px-6 py-9 text-center md:grid-cols-4 md:divide-x md:divide-white/70">
            {trustStats.map((stat) => (
              <li key={stat.label} className="px-3">
                <strong className="block font-display text-[clamp(1.45rem,2.5vw,1.9rem)] font-semibold leading-none text-ink">
                  {stat.value}
                </strong>
                <span className="mt-2.5 block text-[0.87rem] text-ink-soft">{stat.label}</span>
              </li>
            ))}
          </ul>
        </section>

        {/* ==================== VÌ SAO ==================== */}
        <section id="vi-sao" aria-labelledby="vi-sao-title" className="wrap py-16 md:py-20">
          <div className="grid items-start gap-14 lg:grid-cols-2 lg:gap-16">
            <Reveal>
              <Eyebrow>Vì sao cần phần mềm</Eyebrow>
              <h2
                id="vi-sao-title"
                className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.7rem)] font-semibold leading-[1.16] text-ink"
              >
                Sổ bộ giấy đang giữ ký ức của cả một cộng đoàn
              </h2>
              <p className="mt-5 max-w-[58ch] text-[1.04rem] leading-relaxed text-ink-soft">
                Mỗi cuốn sổ Rửa tội, Thêm sức, Hôn phối là chứng tích đời sống đức tin của giáo xứ.
                Nhưng giấy thì cũ đi, mực thì phai, và mỗi lần cần trích lục lại là một buổi chiều
                lật từng trang.
              </p>

              <ol className="mt-9 grid list-none gap-3 p-0">
                {painPoints.map((point, index) => (
                  <li key={point.title} className="glass flex gap-5 p-5">
                    <span
                      className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-brand/12 font-display text-[0.95rem] font-semibold tabular-nums text-brand-ink"
                      aria-hidden="true"
                    >
                      {index + 1}
                    </span>
                    <span>
                      <strong className="block text-[1.01rem] font-semibold text-ink">
                        {point.title}
                      </strong>
                      <span className="mt-1 block text-[0.95rem] leading-relaxed text-ink-soft">
                        {point.body}
                      </span>
                    </span>
                  </li>
                ))}
              </ol>
            </Reveal>

            <Reveal delay={80} className="lg:sticky lg:top-32">
              <ShotFrame label="Danh sách giáo dân — lọc và in chứng nhận">
                <Image
                  src="/images/luoi_danh_sach.jpg"
                  alt="Danh sách giáo dân dạng bảng, có thể lọc và sắp xếp theo từng cột, kèm menu in chứng nhận Rửa tội và Thêm sức."
                  width={816}
                  height={585}
                  sizes="(min-width: 1024px) 560px, 100vw"
                  loading="lazy"
                  quality={82}
                  className="w-full"
                />
              </ShotFrame>
              <p className="glass mt-5 border-l-4 border-l-mint p-5 text-[0.96rem] leading-relaxed text-ink">
                Số hoá một lần, tra cứu cả đời. Sổ giấy vẫn giữ nguyên giá trị pháp lý — phần mềm là
                bản sao sống động giúp việc quản lý ở giáo xứ nhanh hơn.
              </p>
            </Reveal>
          </div>
        </section>

        {/* ==================== TÍNH NĂNG ==================== */}
        <section id="tinh-nang" aria-labelledby="tinh-nang-title" className="wrap py-16 md:py-20">
          <Reveal className="mx-auto max-w-[46rem] text-center">
            <Eyebrow center>Tính năng chính</Eyebrow>
            <h2
              id="tinh-nang-title"
              className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.7rem)] font-semibold leading-[1.16] text-ink"
            >
              Đủ cho công việc thường ngày của một giáo xứ
            </h2>
            <p className="mx-auto mt-5 max-w-[58ch] text-[1.04rem] leading-relaxed text-ink-soft">
              Được xây dựng theo đúng cách các giáo xứ Việt Nam vẫn làm việc: từ giáo họ, gia đình,
              sổ bí tích cho tới lớp giáo lý và hội đoàn.
            </p>
          </Reveal>

          <ul className="mt-14 grid list-none grid-cols-1 gap-5 p-0 sm:grid-cols-2 lg:grid-cols-3">
            {features.map((feature, index) => (
              <Reveal as="li" key={feature.id} delay={(index % 3) * 70} className="h-full">
                <article className="group glass relative h-full overflow-hidden p-7 transition-transform duration-300 hover:-translate-y-1.5">
                  <Sheen />
                  <div className="relative">
                    <span
                      className={`inline-flex h-12 w-12 items-center justify-center rounded-2xl ${toneClasses[feature.tone].softBg} ${toneClasses[feature.tone].glow}`}
                    >
                      <Icon
                        name={feature.icon}
                        className={`h-6 w-6 ${toneClasses[feature.tone].text}`}
                      />
                    </span>
                    <h3 className="mt-5 font-display text-[1.28rem] font-semibold leading-snug text-ink">
                      {feature.title}
                    </h3>
                    <p className="mt-2.5 text-[0.95rem] leading-relaxed text-ink-soft">
                      {feature.body}
                    </p>
                  </div>
                </article>
              </Reveal>
            ))}
          </ul>
        </section>

        {/* ==================== HÌNH ẢNH ==================== */}
        <section id="hinh-anh" aria-labelledby="hinh-anh-title" className="wrap py-16 md:py-20">
          <Reveal className="mx-auto max-w-[46rem] text-center">
            <Eyebrow center>Hình ảnh phần mềm</Eyebrow>
            <h2
              id="hinh-anh-title"
              className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.7rem)] font-semibold leading-[1.16] text-ink"
            >
              Giao diện quen thuộc, không cần học lâu
            </h2>
            <p className="mx-auto mt-5 max-w-[56ch] text-[1.04rem] leading-relaxed text-ink-soft">
              Ai đã dùng qua Word hay Excel đều có thể bắt đầu ngay. Chọn từng mục bên dưới để xem{" "}
              {screenshots.length} màn hình thực tế của chương trình.
            </p>
          </Reveal>

          <Reveal className="mt-14">
            <Screenshots />
          </Reveal>
        </section>

        {/* ==================== PHIÊN BẢN MỚI ==================== */}
        <section id="phien-ban-moi" aria-labelledby="phien-ban-moi-title" className="wrap py-6">
          <div className="on-dark relative overflow-hidden rounded-[28px] bg-ink px-6 py-16 text-white md:px-14 md:py-20">
            <div
              aria-hidden="true"
              className="pointer-events-none absolute inset-0 [background:radial-gradient(760px_420px_at_10%_-10%,rgba(59,130,246,0.42),transparent_62%),radial-gradient(680px_400px_at_90%_8%,rgba(124,107,240,0.32),transparent_60%),radial-gradient(600px_380px_at_70%_100%,rgba(20,184,166,0.2),transparent_62%)]"
            />

            <div className="relative">
              <Reveal className="max-w-[52rem]">
                <Eyebrow onDark>Phiên bản mới nhất</Eyebrow>
                <h2
                  id="phien-ban-moi-title"
                  className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.8rem)] font-semibold leading-[1.16] text-white"
                >
                  Bản {release.version} — {release.headline}
                </h2>
                <p className="mt-5 max-w-[60ch] text-[1.04rem] leading-relaxed text-white/78">
                  Phát hành ngày{" "}
                  <time dateTime={release.publishedAt} className="tabular-nums">
                    {formatDate(release.publishedAt)}
                  </time>
                  . {release.summary}
                </p>
              </Reveal>

              <ul className="mt-12 grid list-none gap-5 p-0 md:grid-cols-2">
                {release.groups.map((group, index) => (
                  <Reveal as="li" key={group.id} delay={(index % 2) * 70} className="h-full">
                    <article className="group glass-dark relative h-full overflow-hidden p-7">
                      <Sheen />
                      <div className="relative">
                        <h3 className="flex items-center gap-3 font-sans text-[1.04rem] font-bold text-white">
                          <Icon name={group.icon} className="h-5 w-5 shrink-0 text-sky" />
                          {group.title}
                        </h3>
                        <ul className="mt-4 grid list-none gap-3 p-0">
                          {group.items.map((item) => (
                            <li
                              key={item}
                              className="flex gap-3 text-[0.95rem] leading-relaxed text-white/82"
                            >
                              <CheckIcon className="mt-1 h-4 w-4 shrink-0 text-sky" />
                              {item}
                            </li>
                          ))}
                        </ul>
                      </div>
                    </article>
                  </Reveal>
                ))}
              </ul>

              <div className="mt-10 flex flex-wrap gap-4">
                <Link href="#tai-ve" className="btn btn-primary">
                  <Icon name="download" strokeWidth={1.9} />
                  Tải bản {release.version}
                </Link>
                {release.articleSlug ? (
                  <Link href={`/tin-tuc/${release.articleSlug}`} className="btn btn-on-dark">
                    Đọc bài viết đầy đủ
                    <ChevronIcon className="h-4 w-4" />
                  </Link>
                ) : null}
                <Link href="/phien-ban" className="btn btn-on-dark">
                  Lịch sử phiên bản &amp; bản cũ
                  <ChevronIcon className="h-4 w-4" />
                </Link>
              </div>
            </div>
          </div>
        </section>

        {/* ==================== TẢI VỀ ==================== */}
        <section id="tai-ve" aria-labelledby="tai-ve-title" className="wrap py-16 md:py-20">
          <Reveal className="mx-auto max-w-[46rem] text-center">
            <Eyebrow center>Tải về</Eyebrow>
            <h2
              id="tai-ve-title"
              className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.7rem)] font-semibold leading-[1.16] text-ink"
            >
              Tải phần mềm {site.name}
            </h2>
            <p className="mx-auto mt-5 max-w-[56ch] text-[1.04rem] leading-relaxed text-ink-soft">
              Miễn phí cho mọi giáo xứ. Một bộ cài duy nhất dùng được cho cả máy cài lần đầu lẫn
              máy đang chạy bản cũ muốn cập nhật thủ công.
            </p>
          </Reveal>

          {/* Khung lưới co giãn theo số tệp: hiện tại chỉ có một bộ cài công khai
              (xem ghi chú tại `downloads` trong static-provider.ts) nên căn giữa
              thành một thẻ duy nhất thay vì để trống nửa lưới 2 cột. */}
          <ul
            className={`mt-14 grid list-none gap-6 p-0 ${
              release.downloads.length === 1 ? "mx-auto max-w-xl" : "md:grid-cols-2"
            }`}
          >
            {release.downloads.map((file, index) => (
              <Reveal as="li" key={file.id} delay={index * 70} className="h-full">
                <article
                  className={`group glass relative flex h-full flex-col overflow-hidden p-7 ${
                    file.primary
                      ? "ring-2 ring-brand/35 ring-offset-2 ring-offset-canvas"
                      : ""
                  }`}
                >
                  <Sheen />
                  <div className="relative flex flex-1 flex-col">
                    <span
                      className={`self-start rounded-full px-3 py-1 text-[0.68rem] font-bold uppercase tracking-[0.11em] ${
                        file.primary
                          ? "bg-gradient-to-br from-brand to-brand-deep text-white"
                          : "bg-ink/8 text-ink-soft"
                      }`}
                    >
                      {file.tag}
                    </span>

                    <h3 className="mt-5 font-display text-[1.42rem] font-semibold text-ink">
                      {file.title}
                    </h3>
                    <p className="mt-2.5 text-[0.97rem] leading-relaxed text-ink-soft">
                      {file.summary}
                    </p>

                    <ul className="my-6 grid list-none gap-2.5 border-y border-white/70 p-0 py-4 text-[0.88rem] text-ink-soft">
                      <li className="flex justify-between gap-4">
                        <span>Tệp</span>
                        <span className="font-mono text-ink">{file.fileName}</span>
                      </li>
                      <li className="flex justify-between gap-4">
                        <span>Dung lượng</span>
                        <span className="tabular-nums text-ink">{file.size}</span>
                      </li>
                      <li className="flex justify-between gap-4">
                        <span>Ngày phát hành</span>
                        <time dateTime={release.publishedAt} className="tabular-nums text-ink">
                          {formatDate(release.publishedAt)}
                        </time>
                      </li>
                    </ul>

                    <a
                      href={file.href}
                      className={`btn mt-auto w-full ${file.primary ? "btn-primary" : "btn-glass"}`}
                    >
                      <Icon name="download" strokeWidth={1.9} />
                      {file.cta}
                    </a>
                  </div>
                </article>
              </Reveal>
            ))}
          </ul>

          <Reveal className="glass mt-6 grid gap-9 p-7 md:grid-cols-[1.1fr_1fr] md:p-10">
            <div>
              <h3 className="font-sans text-[1.04rem] font-bold text-ink">
                Cài đặt trong {installSteps.length} bước
              </h3>
              <ol className="mt-6 grid list-none gap-4 p-0 [counter-reset:step]">
                {installSteps.map((step) => (
                  <li
                    key={step}
                    className="flex gap-4 text-[0.96rem] leading-relaxed [counter-increment:step]"
                  >
                    <span
                      aria-hidden="true"
                      className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-brand to-brand-deep text-[0.78rem] font-bold tabular-nums text-white shadow-[0_6px_14px_-6px_rgba(20,63,152,0.8)] before:content-[counter(step)]"
                    />
                    {step}
                  </li>
                ))}
              </ol>
            </div>

            <div className="rounded-2xl border-l-4 border-l-amber bg-amber/8 p-6">
              <h3 className="font-sans text-[0.98rem] font-bold text-amber-ink">
                Trước khi cập nhật, hãy sao lưu dữ liệu
              </h3>
              <p className="mt-3 text-[0.95rem] leading-relaxed text-ink">
                Vào menu <em>Hệ thống › Sao lưu dữ liệu</em> và cất tệp sao lưu ra USB hoặc ổ đĩa
                ngoài. Cài đè lên bản cũ không đụng tới dữ liệu (dữ liệu nằm trong một tệp cơ sở
                dữ liệu riêng), nhưng sao lưu vẫn luôn là thói quen nên có.
              </p>
              <Link
                href="/tin-tuc/sao-luu-du-lieu-dung-cach"
                className="link-underline mt-4 inline-flex items-center gap-2 py-1 text-[0.92rem] font-semibold text-brand-ink"
              >
                Đọc hướng dẫn sao lưu
                <ChevronIcon className="h-4 w-4" />
              </Link>
            </div>
          </Reveal>
        </section>

        {/* ==================== TIN TỨC ==================== */}
        <section id="tin-tuc" aria-labelledby="tin-tuc-title" className="wrap py-16 md:py-20">
          <Reveal className="flex flex-wrap items-end justify-between gap-6">
            <div className="max-w-[40rem]">
              <Eyebrow>Tin tức &amp; hướng dẫn</Eyebrow>
              <h2
                id="tin-tuc-title"
                className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.7rem)] font-semibold leading-[1.16] text-ink"
              >
                Đọc thêm từ ban phát triển
              </h2>
            </div>
            <Link href="/tin-tuc" className="btn btn-glass h-12 min-h-0">
              Xem tất cả bài viết
              <ChevronIcon className="h-4 w-4" />
            </Link>
          </Reveal>

          <ul className="mt-14 grid list-none gap-6 p-0 md:grid-cols-2 lg:grid-cols-3">
            {articles.map((article, index) => (
              <Reveal as="li" key={article.slug} delay={index * 70} className="h-full">
                <ArticleCard article={article} />
              </Reveal>
            ))}
          </ul>
        </section>

        {/* ==================== HƯỚNG DẪN ==================== */}
        {/* id="ho-tro" chứ không phải "huong-dan": /huong-dan giờ là một route
            thật (21 trang hướng dẫn nhập từ BIN/help/), đặt trùng tên neo trên
            trang chủ dễ gây nhầm khi đọc code — nội dung phần này thật ra là
            diễn đàn cộng đồng và các liên kết hỗ trợ, không phải tài liệu
            hướng dẫn. */}
        <section id="ho-tro" aria-labelledby="ho-tro-title" className="wrap py-16 md:py-20">
          <div className="grid gap-6 lg:grid-cols-[1.15fr_1fr]">
            <Reveal className="on-dark relative overflow-hidden rounded-[28px] bg-brand-deep p-8 text-white md:p-12">
              <div
                aria-hidden="true"
                className="pointer-events-none absolute inset-0 [background:radial-gradient(560px_320px_at_85%_10%,rgba(56,189,248,0.42),transparent_62%),radial-gradient(480px_300px_at_10%_100%,rgba(124,107,240,0.4),transparent_60%)]"
              />
              <div className="relative">
                <Eyebrow onDark>Diễn đàn cộng đồng</Eyebrow>
                <h2
                  id="ho-tro-title"
                  className="mt-4 font-display text-[clamp(1.7rem,3.2vw,2.4rem)] font-semibold leading-[1.18] text-white"
                >
                  Không ai phải mò mẫm một mình
                </h2>
                <p className="mt-4 max-w-[46ch] text-[1.02rem] leading-relaxed text-white/82">
                  Hướng dẫn từng bước có kèm hình ảnh, giải đáp thắc mắc và kinh nghiệm thực tế từ
                  các giáo xứ đã sử dụng phần mềm. Đặt câu hỏi và nhận trả lời trực tiếp từ tác giả.
                </p>
                <a
                  href={FORUM_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn-glass mt-8"
                >
                  <Icon name="external" strokeWidth={1.9} />
                  Vào diễn đàn quanlygiaoxu.net
                </a>
              </div>
            </Reveal>

            <Reveal delay={70}>
              <ul className="grid list-none gap-3 p-0">
                {helpLinks.map((link) => (
                  <li key={link.id}>
                    <a
                      href={link.href}
                      target={link.external ? "_blank" : undefined}
                      rel={link.external ? "noopener noreferrer" : undefined}
                      className="group glass relative flex min-h-[88px] items-center gap-5 overflow-hidden px-6 py-5 no-underline transition-transform duration-300 hover:translate-x-1"
                    >
                      <Sheen />
                      <span className="relative flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-brand/12">
                        <Icon name={link.icon} className="h-5 w-5 text-brand-ink" />
                      </span>
                      <span className="relative">
                        <strong className="block text-[1rem] font-semibold text-ink">
                          {link.title}
                        </strong>
                        <span className="text-[0.88rem] text-ink-soft">{link.subtitle}</span>
                      </span>
                      <ChevronIcon className="relative ml-auto h-5 w-5 shrink-0 text-ink-faint transition-transform group-hover:translate-x-1" />
                    </a>
                  </li>
                ))}
              </ul>
            </Reveal>
          </div>
        </section>

        {/* ==================== HỎI ĐÁP ==================== */}
        <section id="cau-hoi" aria-labelledby="cau-hoi-title" className="wrap py-16 md:py-20">
          <Reveal className="mx-auto max-w-[46rem] text-center">
            <Eyebrow center>Hỏi đáp</Eyebrow>
            <h2
              id="cau-hoi-title"
              className="mt-4 font-display text-[clamp(1.9rem,3.6vw,2.7rem)] font-semibold leading-[1.16] text-ink"
            >
              Những câu hỏi thường gặp
            </h2>
          </Reveal>

          <Reveal className="mx-auto mt-14 grid max-w-[54rem] list-none gap-3">
            {faqs.map((item, index) => (
              <details key={item.id} open={index === 0} className="group glass overflow-hidden">
                <summary className="flex min-h-[64px] cursor-pointer list-none items-center gap-5 px-6 py-4 text-[1.01rem] font-semibold text-ink [&::-webkit-details-marker]:hidden">
                  {item.question}
                  <ChevronIcon
                    direction="down"
                    className="ml-auto h-5 w-5 shrink-0 text-ink-faint transition-transform duration-300 group-open:rotate-180"
                  />
                </summary>
                <div className="px-6 pb-6 text-[0.97rem] leading-relaxed text-ink-soft">
                  {item.answer.map((paragraph) => (
                    <p key={paragraph} className="mt-0 [&+p]:mt-3">
                      {paragraph}
                    </p>
                  ))}
                </div>
              </details>
            ))}
          </Reveal>
        </section>

        {/* ==================== CTA CUỐI ==================== */}
        <section aria-labelledby="cta-title" className="wrap pb-8 pt-6">
          <div className="glass relative overflow-hidden px-6 py-16 text-center md:px-14 md:py-20">
            <div
              aria-hidden="true"
              className="pointer-events-none absolute inset-0 [background:radial-gradient(600px_320px_at_50%_-10%,rgba(59,130,246,0.22),transparent_65%)]"
            />
            <div className="relative">
              <Eyebrow center>Bắt đầu hôm nay</Eyebrow>
              <h2
                id="cta-title"
                className="mx-auto mt-4 max-w-[20ch] font-display text-[clamp(2rem,4.2vw,3.05rem)] font-semibold leading-[1.14] text-ink"
              >
                Giữ gìn sổ bộ của giáo xứ cho những thế hệ mai sau
              </h2>
              <p className="mx-auto mt-5 max-w-[58ch] text-[1.04rem] leading-relaxed text-ink-soft">
                Tải về, cài đặt trong vài phút và bắt đầu số hoá hồ sơ giáo xứ. Hoàn toàn
                miễn phí, có cộng đồng hỗ trợ.
              </p>

              <div className="mt-9 flex flex-wrap justify-center gap-4">
                <a href={primaryDownload.href} className="btn btn-primary">
                  <Icon name="download" strokeWidth={1.9} />
                  Tải phiên bản {release.version}
                </a>
                <a
                  href={FORUM_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn-glass"
                >
                  Xem hướng dẫn trên diễn đàn
                </a>
              </div>

              <blockquote className="mx-auto mt-14 max-w-[46rem] border-t border-white/70 pt-10 font-display text-[clamp(1.1rem,2.1vw,1.45rem)] italic leading-relaxed text-ink">
                “Anh em hãy chăn dắt đoàn chiên mà Thiên Chúa đã giao phó cho anh em.”
                <cite className="mt-3 block font-sans text-[0.78rem] font-bold uppercase not-italic tracking-[0.18em] text-brand">
                  1 Pr 5, 2
                </cite>
              </blockquote>
            </div>
          </div>
        </section>
      </main>

      <SiteFooter version={release.version} releaseDate={formatDate(release.publishedAt)} />
    </>
  );
}
