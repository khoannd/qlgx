"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useRef, useState } from "react";

import { BrandMark, Icon } from "@/components/Icon";
import { FACEBOOK_URL, SUPPORT_EMAIL, navLinks, site } from "@/lib/site";

/**
 * Thanh điều hướng dính đầu trang.
 *
 * Là client component vì cần ba hành vi chỉ có ở trình duyệt: tấm kính đậm dần
 * khi cuộn, đóng/mở menu trên màn hình nhỏ, và đánh dấu mục đang xem trên trang chủ.
 * Toàn bộ phần còn lại của trang vẫn là server component.
 */
export function SiteHeader() {
  const pathname = usePathname();
  const [scrolled, setScrolled] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const [activeHash, setActiveHash] = useState<string | null>(null);
  const toggleRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    let ticking = false;
    const onScroll = () => {
      if (ticking) return;
      ticking = true;
      window.requestAnimationFrame(() => {
        setScrolled(window.scrollY > 8);
        ticking = false;
      });
    };
    window.addEventListener("scroll", onScroll, { passive: true });
    onScroll();
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  /* Phím Esc đóng menu và trả tiêu điểm về nút mở */
  useEffect(() => {
    if (!menuOpen) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setMenuOpen(false);
        toggleRef.current?.focus();
      }
    };
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [menuOpen]);

  /* Đánh dấu mục đang xem — chỉ có ý nghĩa trên trang chủ */
  useEffect(() => {
    if (pathname !== "/" || !("IntersectionObserver" in window)) return;
    const ids = navLinks.filter((l) => l.href.startsWith("/#")).map((l) => l.href.slice(2));
    const sections = ids
      .map((id) => document.getElementById(id))
      .filter((el): el is HTMLElement => el !== null);
    if (sections.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) setActiveHash(`/#${entry.target.id}`);
        }
      },
      { rootMargin: "-45% 0px -50% 0px" },
    );
    sections.forEach((s) => observer.observe(s));
    return () => observer.disconnect();
  }, [pathname]);

  const isActive = (href: string) =>
    href.startsWith("/#") ? pathname === "/" && activeHash === href : pathname.startsWith(href);

  return (
    <header className="sticky top-0 z-50">
      {/* Viên kính điều hướng bên dưới chỉ rộng tối đa 1220px và trôi nổi giữa
          trang — khung <header> bao ngoài lại rộng hết màn hình nhưng trong
          suốt, nên khi cuộn, nội dung phía sau lộ ra ở hai bên và phía trên
          viên kính (đặc biệt rõ ở màn hình rộng hơn 1220px). Lớp nền này phủ
          kín toàn bộ chiều rộng, chỉ hiện khi đã cuộn — lúc chưa cuộn, để
          trong suốt cho ảnh Hero lộ ra phía sau như thiết kế ban đầu. */}
      <div
        aria-hidden="true"
        className={`absolute inset-0 border-b backdrop-blur-xl transition-opacity duration-300 ${
          scrolled
            ? "border-white/60 bg-white/85 opacity-100 shadow-[0_8px_24px_-18px_rgba(14,32,76,0.35)]"
            : "border-transparent bg-transparent opacity-0"
        }`}
      />
      <div className="relative px-3 pt-3 sm:px-5 sm:pt-4">
        <div
          className={`glass mx-auto flex w-full max-w-[1220px] items-center gap-5 rounded-full px-4 py-2.5 transition-all duration-300 sm:px-5 ${
            scrolled ? "glass-solid shadow-[0_16px_40px_-20px_rgba(14,32,76,0.4)]" : ""
          }`}
        >
        <Link href="/" className="flex shrink-0 items-center gap-3 no-underline">
          <BrandMark className="h-10 w-10 shrink-0" id="hdr" />
          <span className="flex flex-col leading-[1.12]">
            <span className="font-display text-[1.16rem] font-semibold tracking-tight text-ink">
              {site.name}
            </span>
            <span className="text-[0.66rem] font-bold uppercase tracking-[0.15em] text-ink-faint">
              {site.tagline}
            </span>
          </span>
        </Link>

        <button
          ref={toggleRef}
          type="button"
          className="btn-glass ml-auto flex h-11 w-11 items-center justify-center rounded-full border lg:hidden"
          aria-expanded={menuOpen}
          aria-controls="site-nav"
          aria-label={menuOpen ? "Đóng menu điều hướng" : "Mở menu điều hướng"}
          onClick={() => setMenuOpen((open) => !open)}
        >
          <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth={1.8}
            strokeLinecap="round"
            className="h-5 w-5"
            aria-hidden="true"
          >
            {menuOpen ? <path d="m6 6 12 12M18 6 6 18" /> : <path d="M4 7h16M4 12h16M4 17h16" />}
          </svg>
        </button>

        {/* Mặc định ẩn; mở ra khi data-open="true" (điện thoại) hoặc từ 1024px trở lên.
            Viết theo hướng "hidden rồi bật lên" vì trong Tailwind v4, biến thể
            data-[...] xếp sau biến thể lg: trong CSS sinh ra — nếu dùng
            `data-[open=false]:hidden lg:block` thì lớp hidden sẽ thắng ở desktop
            và toàn bộ menu biến mất. */}
        <nav
          id="site-nav"
          data-open={menuOpen}
          aria-label="Điều hướng chính"
          className="glass glass-solid absolute inset-x-3 top-full mt-2 hidden rounded-3xl p-3 data-[open=true]:block sm:inset-x-5 lg:static lg:ml-auto lg:mt-0 lg:block lg:border-0 lg:bg-none lg:p-0 lg:shadow-none lg:backdrop-blur-none"
        >
          <ul className="m-0 flex list-none flex-col gap-1 p-0 lg:flex-row lg:items-center lg:gap-0.5">
            {navLinks.map((link) => (
              <li key={link.href}>
                <Link
                  href={link.href}
                  onClick={() => setMenuOpen(false)}
                  aria-current={isActive(link.href) ? "page" : undefined}
                  className={`block rounded-full px-4 py-3 text-[0.96rem] font-medium no-underline transition-colors lg:py-2 ${
                    isActive(link.href)
                      ? "bg-brand/12 text-brand-ink"
                      : "text-ink-soft hover:bg-white/60 hover:text-brand-ink"
                  }`}
                >
                  {link.label}
                </Link>
              </li>
            ))}
            <li className="lg:hidden">
              <Link
                href="/#tai-ve"
                onClick={() => setMenuOpen(false)}
                className="btn btn-primary mt-2 w-full"
              >
                Tải phần mềm
              </Link>
            </li>
          </ul>
        </nav>

        {/* Chỉ hiện từ lg trở lên: header di động đã chật với logo + nút menu,
            hai liên kết này vẫn có mặt đầy đủ ở footer trên mọi kích thước màn hình. */}
        <a
          href={`mailto:${SUPPORT_EMAIL}`}
          className="hidden h-11 w-11 shrink-0 items-center justify-center rounded-full text-ink-faint transition-colors hover:text-brand-ink lg:flex"
          aria-label={`Gửi email hỗ trợ tới ${SUPPORT_EMAIL}`}
          title={`Email hỗ trợ: ${SUPPORT_EMAIL}`}
        >
          <Icon name="mail" className="h-[1.15rem] w-[1.15rem]" />
        </a>
        <a
          href={FACEBOOK_URL}
          target="_blank"
          rel="noopener noreferrer"
          className="hidden h-11 w-11 shrink-0 items-center justify-center rounded-full text-ink-faint transition-colors hover:text-brand-ink lg:flex"
          aria-label="Trang Facebook của Quản Lý Giáo Xứ"
          title="Facebook QLGX"
        >
          <Icon name="facebook" className="h-[1.15rem] w-[1.15rem]" />
        </a>

        <Link
          href="/#tai-ve"
          className="btn btn-primary hidden h-11 min-h-0 shrink-0 px-5 text-[0.94rem] lg:inline-flex"
        >
          <Icon name="download" strokeWidth={1.9} />
          Tải phần mềm
        </Link>
        </div>
      </div>
    </header>
  );
}
