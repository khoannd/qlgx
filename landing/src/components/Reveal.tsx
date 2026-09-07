"use client";

import { useEffect, useRef, type ElementType, type ReactNode } from "react";

type RevealProps = {
  children: ReactNode;
  as?: ElementType;
  className?: string;
  /** Độ trễ theo mili giây, dùng để tạo hiệu ứng so le cho lưới thẻ */
  delay?: number;
};

/**
 * Bọc một khối để nó mờ dần hiện lên khi cuộn tới.
 *
 * Ba điều quan trọng:
 * 1. Nội dung vẫn được server render đầy đủ trong HTML — công cụ tìm kiếm đọc được
 *    kể cả khi chưa chạy JavaScript.
 * 2. CSS chỉ ẩn khối này khi <html> có class `js`, nên JS lỗi thì trang vẫn hiện.
 * 3. Tôn trọng prefers-reduced-motion: hiện ngay, không hoạt hình.
 */
export function Reveal({ children, as: Tag = "div", className = "", delay = 0 }: RevealProps) {
  const ref = useRef<HTMLElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const prefersReduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (prefersReduced || !("IntersectionObserver" in window)) {
      el.classList.add("reveal-in");
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add("reveal-in");
            observer.unobserve(entry.target);
          }
        }
      },
      { rootMargin: "0px 0px -10% 0px", threshold: 0.05 },
    );

    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return (
    <Tag
      ref={ref}
      className={`reveal ${className}`}
      style={delay ? ({ "--reveal-delay": `${delay}ms` } as React.CSSProperties) : undefined}
    >
      {children}
    </Tag>
  );
}
