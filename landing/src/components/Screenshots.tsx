"use client";

import Image from "next/image";
import { useRef, useState } from "react";

import { Icon } from "@/components/Icon";
import { ShotFrame } from "@/components/glass";
import { screenshots } from "@/lib/site";

/**
 * Thư viện ảnh chụp màn hình dạng tab, theo đúng mẫu Tabs của WAI-ARIA:
 * chỉ tab đang chọn nằm trong thứ tự Tab, các phím mũi tên / Home / End để chuyển tab.
 *
 * Mọi panel đều được render sẵn trong HTML (panel không hoạt động chỉ mang thuộc
 * tính `hidden`), nhờ vậy công cụ tìm kiếm đọc được toàn bộ chú thích và alt của ảnh.
 * Ảnh trong panel ẩn không tải cho tới khi được mở, nhờ loading="lazy".
 */
export function Screenshots() {
  const [active, setActive] = useState(0);
  const tabRefs = useRef<(HTMLButtonElement | null)[]>([]);

  const selectTab = (index: number, moveFocus: boolean) => {
    setActive(index);
    if (moveFocus) tabRefs.current[index]?.focus();
  };

  const onKeyDown = (event: React.KeyboardEvent, index: number) => {
    const last = screenshots.length - 1;
    let next: number | null = null;

    if (event.key === "ArrowDown" || event.key === "ArrowRight") next = index === last ? 0 : index + 1;
    else if (event.key === "ArrowUp" || event.key === "ArrowLeft") next = index === 0 ? last : index - 1;
    else if (event.key === "Home") next = 0;
    else if (event.key === "End") next = last;

    if (next !== null) {
      event.preventDefault();
      selectTab(next, true);
    }
  };

  return (
    <div className="grid gap-6 lg:grid-cols-[276px_minmax(0,1fr)] lg:gap-10">
      <div
        role="tablist"
        aria-label="Các màn hình của phần mềm"
        aria-orientation="vertical"
        className="flex gap-2 overflow-x-auto pb-2 lg:flex-col lg:overflow-visible lg:pb-0"
      >
        {screenshots.map((shot, index) => {
          const selected = active === index;
          return (
            <button
              key={shot.id}
              type="button"
              role="tab"
              id={`tab-${shot.id}`}
              aria-controls={`panel-${shot.id}`}
              aria-selected={selected}
              tabIndex={selected ? 0 : -1}
              ref={(el) => {
                tabRefs.current[index] = el;
              }}
              onClick={() => selectTab(index, false)}
              onKeyDown={(event) => onKeyDown(event, index)}
              className={`flex min-h-[54px] shrink-0 items-center gap-3 whitespace-nowrap rounded-2xl px-4 py-3 text-left text-[0.95rem] transition-all duration-200 lg:whitespace-normal ${
                selected
                  ? "bg-gradient-to-br from-brand to-brand-deep font-semibold text-white shadow-[inset_0_1px_0_rgba(255,255,255,0.4),0_10px_24px_-10px_rgba(20,63,152,0.7)]"
                  : "glass font-medium text-ink-soft hover:text-brand-ink"
              }`}
            >
              <Icon
                name={shot.icon}
                className={`h-5 w-5 shrink-0 ${selected ? "text-white/85" : "text-brand"}`}
              />
              {shot.tab}
            </button>
          );
        })}
      </div>

      <div>
        {screenshots.map((shot, index) => (
          <div
            key={shot.id}
            role="tabpanel"
            id={`panel-${shot.id}`}
            aria-labelledby={`tab-${shot.id}`}
            tabIndex={0}
            hidden={active !== index}
          >
            <ShotFrame label={shot.label}>
              <Image
                src={shot.src}
                alt={shot.alt}
                width={shot.width}
                height={shot.height}
                sizes="(min-width: 1024px) 820px, 100vw"
                loading="lazy"
                quality={82}
                className="w-full"
              />
            </ShotFrame>
            <p className="mt-5 max-w-[70ch] text-[0.98rem] leading-relaxed text-ink-soft">
              {shot.caption}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}
