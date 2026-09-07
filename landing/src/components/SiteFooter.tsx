import Link from "next/link";

import { BrandMark, Icon } from "@/components/Icon";
import { FACEBOOK_URL, FORUM_URL, SUPPORT_EMAIL, navLinks, site } from "@/lib/site";

export function SiteFooter({ version, releaseDate }: { version: string; releaseDate: string }) {
  return (
    <footer className="on-dark relative mt-20 overflow-hidden bg-ink text-white/72">
      {/* Đốm sáng mờ để nền tối không bị phẳng lì */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 [background:radial-gradient(700px_360px_at_12%_0%,rgba(59,130,246,0.32),transparent_62%),radial-gradient(600px_320px_at_88%_10%,rgba(124,107,240,0.24),transparent_60%)]"
      />

      <div className="wrap relative py-16">
        <div className="grid gap-10 md:grid-cols-2 lg:grid-cols-4">
          <div>
            <Link href="/" className="flex items-center gap-3 no-underline">
              <BrandMark className="h-10 w-10 shrink-0" id="ftr" />
              <span className="flex flex-col leading-[1.12]">
                <span className="font-display text-[1.16rem] font-semibold text-white">
                  {site.name}
                </span>
                <span className="text-[0.66rem] font-bold uppercase tracking-[0.15em] text-white/55">
                  {site.tagline}
                </span>
              </span>
            </Link>
            <p className="mt-5 max-w-[34ch] text-[0.94rem] leading-relaxed">
              Phần mềm miễn phí giúp các giáo xứ Công giáo Việt Nam quản lý giáo dân, gia đình, sổ
              bí tích và giáo lý.
            </p>
          </div>

          <nav aria-labelledby="footer-nav-trang">
            <h2
              id="footer-nav-trang"
              className="mb-4 font-sans text-[0.7rem] font-bold uppercase tracking-[0.18em] text-sky"
            >
              Trang
            </h2>
            <ul className="m-0 grid list-none gap-1 p-0 text-[0.94rem]">
              {navLinks.map((link) => (
                <li key={link.href}>
                  <Link
                    href={link.href}
                    className="inline-block py-1 no-underline hover:text-white hover:underline"
                  >
                    {link.label}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>

          <nav aria-labelledby="footer-nav-hotro">
            <h2
              id="footer-nav-hotro"
              className="mb-4 font-sans text-[0.7rem] font-bold uppercase tracking-[0.18em] text-sky"
            >
              Hỗ trợ
            </h2>
            <ul className="m-0 grid list-none gap-1 p-0 text-[0.94rem]">
              <li>
                <a
                  href={FORUM_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-block py-1 no-underline hover:text-white hover:underline"
                >
                  Diễn đàn hướng dẫn
                </a>
              </li>
              <li>
                <Link
                  href="/#cau-hoi"
                  className="inline-block py-1 no-underline hover:text-white hover:underline"
                >
                  Câu hỏi thường gặp
                </Link>
              </li>
              <li>
                <Link
                  href="/#ho-tro"
                  className="inline-block py-1 no-underline hover:text-white hover:underline"
                >
                  Liên hệ &amp; hỗ trợ
                </Link>
              </li>
              <li>
                <a
                  href={`mailto:${SUPPORT_EMAIL}`}
                  className="inline-flex items-center gap-2 py-1 no-underline hover:text-white hover:underline"
                >
                  <Icon name="mail" className="h-4 w-4 shrink-0" />
                  {SUPPORT_EMAIL}
                </a>
              </li>
              <li>
                <a
                  href={FACEBOOK_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex items-center gap-2 py-1 no-underline hover:text-white hover:underline"
                >
                  <Icon name="facebook" className="h-4 w-4 shrink-0" />
                  Facebook QLGX
                </a>
              </li>
            </ul>
          </nav>

          <div>
            <h2 className="mb-4 font-sans text-[0.7rem] font-bold uppercase tracking-[0.18em] text-sky">
              Bản phát hành
            </h2>
            <ul className="m-0 grid list-none gap-1 p-0 text-[0.94rem] tabular-nums">
              <li>Phiên bản {version}</li>
              <li>Ngày {releaseDate}</li>
              <li>Windows 7 → 11</li>
              <li className="pt-1">
                <Link
                  href="/phien-ban"
                  className="inline-block py-1 no-underline hover:text-white hover:underline"
                >
                  Lịch sử phiên bản &amp; bản cũ
                </Link>
              </li>
            </ul>
          </div>
        </div>

        <div className="mt-12 flex flex-wrap items-center justify-between gap-x-6 gap-y-3 border-t border-white/15 pt-6 text-[0.86rem] text-white/55">
          <span>
            © {new Date().getFullYear()} quanlygiaoxu.net — Phần mềm miễn phí phục vụ các giáo xứ.
          </span>
          <span className="font-display italic">Ad Maiorem Dei Gloriam</span>
        </div>
      </div>
    </footer>
  );
}
