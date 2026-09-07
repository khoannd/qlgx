import type { IconName } from "@/lib/site";

/**
 * Bộ icon SVG nội tuyến, nét 1.6px, đầu nét vuông-bo — hợp với phong cách viền chì.
 *
 * Dùng SVG thay cho emoji: emoji phụ thuộc font hệ thống, hiển thị khác nhau giữa
 * Windows, macOS và Android, và không thể điều khiển bằng design token.
 *
 * Mặc định icon là trang trí (`aria-hidden`) vì luôn đi kèm nhãn chữ nhìn thấy được.
 * Chỉ truyền `title` khi icon đứng một mình và mang thông tin.
 */

const paths: Record<IconName, React.ReactNode> = {
  users: (
    <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8" />
  ),
  home: <path d="M3 21V9l9-6 9 6v12M9 21v-6h6v6" />,
  family: <path d="M4 21v-2a4 4 0 0 1 4-4h8a4 4 0 0 1 4 4v2M12 3l4 4-4 4-4-4z" />,
  book: (
    <>
      <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z" />
      <path d="M12 6v6m-2.2-3h4.4" />
    </>
  ),
  heart: (
    <path d="M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1-1.1a5.5 5.5 0 1 0-7.8 7.8l8.8 8.8 8.8-8.8a5.5 5.5 0 0 0 0-7.8z" />
  ),
  graduation: (
    <>
      <path d="M22 10 12 5 2 10l10 5 10-5z" />
      <path d="M6 12v5c0 1.7 2.7 3 6 3s6-1.3 6-3v-5" />
    </>
  ),
  chart: (
    <>
      <path d="M3 3v18h18" />
      <path d="m7 15 3.5-4 3 3L20 7" />
    </>
  ),
  printer: (
    <>
      <path d="M6 9V2h12v7M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" />
      <path d="M6 14h12v8H6z" />
    </>
  ),
  refresh: (
    <>
      <path d="M21 12a9 9 0 1 1-3-6.7L21 8" />
      <path d="M21 3v5h-5" />
    </>
  ),
  shield: (
    <>
      <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
      <path d="m9.5 12 1.8 1.8 3.4-3.6" />
    </>
  ),
  download: <path d="M12 3v12m0 0-4-4m4 4 4-4M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2" />,
  "check-square": (
    <path d="m9 11 3 3L22 4M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
  ),
  database: <path d="M21 8v13H3V8M1 3h22v5H1zM10 12h4" />,
  globe: (
    <path d="M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20zM2 12h20M12 2a15 15 0 0 1 0 20 15 15 0 0 1 0-20z" />
  ),
  pencil: <path d="M12 20h9M4 20h4l11-11a2.8 2.8 0 1 0-4-4L4 16z" />,
  help: (
    <path d="M9.1 9a3 3 0 0 1 5.8 1c0 2-3 3-3 3M12 17h.01M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z" />
  ),
  external: (
    <>
      <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
      <path d="M15 3h6v6M10 14 21 3" />
    </>
  ),
  "arrow-right": <path d="M4 12h16m0 0-6-6m6 6-6 6" />,
  mail: (
    <>
      <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z" />
      <path d="m22 6-10 7L2 6" />
    </>
  ),
  facebook: (
    <path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z" />
  ),
};

type IconProps = {
  name: IconName;
  className?: string;
  title?: string;
  strokeWidth?: number;
};

export function Icon({ name, className, title, strokeWidth = 1.6 }: IconProps) {
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={strokeWidth}
      strokeLinecap="round"
      strokeLinejoin="round"
      role={title ? "img" : undefined}
      aria-hidden={title ? undefined : true}
      aria-label={title}
      focusable="false"
    >
      {paths[name]}
    </svg>
  );
}

export function CheckIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2.6}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M20 6 9 17l-5-5" />
    </svg>
  );
}

export function AlertIcon({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M12 9v4m0 4h.01M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0z" />
    </svg>
  );
}

export function ChevronIcon({
  className,
  direction = "right",
}: {
  className?: string;
  direction?: "right" | "down";
}) {
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={2}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {direction === "right" ? <path d="m9 18 6-6-6-6" /> : <path d="m6 9 6 6 6-6" />}
    </svg>
  );
}

/**
 * Dấu hiệu nhận diện: một viên kính bo góc màu xanh, có ánh phản quang ở mép
 * trên và một thánh giá mảnh ở giữa.
 *
 * `id` phải khác nhau giữa các lần dùng: hai `<svg>` cùng trang mà trùng id của
 * gradient thì trình duyệt lấy định nghĩa đầu tiên, logo thứ hai sẽ sai màu.
 */
export function BrandMark({ className, id = "bm" }: { className?: string; id?: string }) {
  return (
    <svg className={className} viewBox="0 0 44 44" aria-hidden="true" focusable="false">
      <defs>
        <linearGradient id={`${id}-fill`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#3B82F6" />
          <stop offset="55%" stopColor="#1D5DDB" />
          <stop offset="100%" stopColor="#143F98" />
        </linearGradient>
        <linearGradient id={`${id}-gloss`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="#fff" stopOpacity="0.55" />
          <stop offset="100%" stopColor="#fff" stopOpacity="0" />
        </linearGradient>
      </defs>
      <rect width="44" height="44" rx="13" fill={`url(#${id}-fill)`} />
      <rect x="1" y="1" width="42" height="20" rx="12" fill={`url(#${id}-gloss)`} />
      <path
        d="M22 11v22M13.5 19.5h17"
        stroke="#fff"
        strokeWidth="2.6"
        strokeLinecap="round"
        opacity="0.96"
      />
      <rect
        x="0.75"
        y="0.75"
        width="42.5"
        height="42.5"
        rx="12.5"
        fill="none"
        stroke="#fff"
        strokeOpacity="0.5"
        strokeWidth="1.5"
      />
    </svg>
  );
}
