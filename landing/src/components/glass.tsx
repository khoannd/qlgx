import type { GlassTone } from "@/lib/content";

/**
 * Những mảnh dựng nên phong cách kính mờ, dùng chung cho mọi trang.
 */

/**
 * Bảng tra lớp Tailwind theo từng sắc thái.
 *
 * Xanh dương là màu chủ đạo; ba màu còn lại chỉ dùng để phân biệt chuyên mục và
 * tạo chút chiều sâu. Cột `text` luôn là bản đủ đậm để đọc trên nền sáng —
 * không bao giờ dùng bản nhạt làm màu chữ.
 */
export const toneClasses: Record<
  GlassTone,
  { text: string; softBg: string; dot: string; glow: string }
> = {
  cobalt: {
    text: "text-brand-ink",
    softBg: "bg-brand/12",
    dot: "bg-brand",
    glow: "shadow-[0_10px_28px_-12px_rgba(29,93,219,0.55)]",
  },
  ruby: {
    text: "text-violet-ink",
    softBg: "bg-violet/12",
    dot: "bg-violet",
    glow: "shadow-[0_10px_28px_-12px_rgba(124,107,240,0.55)]",
  },
  amber: {
    text: "text-amber-ink",
    softBg: "bg-amber/12",
    dot: "bg-amber",
    glow: "shadow-[0_10px_28px_-12px_rgba(217,119,6,0.5)]",
  },
  emerald: {
    text: "text-mint-ink",
    softBg: "bg-mint/12",
    dot: "bg-mint",
    glow: "shadow-[0_10px_28px_-12px_rgba(20,184,166,0.5)]",
  },
};

/**
 * Vệt sáng chéo quét ngang khi rê chuột — thứ tạo cảm giác mặt gương.
 * Đặt bên trong một phần tử có `group relative overflow-hidden`.
 */
export function Sheen() {
  return <span className="sheen" aria-hidden="true" />;
}

/**
 * Nhãn nhỏ in hoa đứng trước tiêu đề mỗi phần.
 */
export function Eyebrow({
  children,
  center = false,
  onDark = false,
  className = "",
}: {
  children: React.ReactNode;
  center?: boolean;
  onDark?: boolean;
  className?: string;
}) {
  return (
    <p
      className={`eyebrow ${center ? "eyebrow-center justify-center" : ""} ${
        onDark ? "text-sky-soft" : "text-brand"
      } ${className}`}
    >
      {children}
    </p>
  );
}

/**
 * Khung cho ảnh chụp màn hình.
 *
 * Ảnh đã được cắt bỏ thanh tiêu đề Windows từ khâu xử lý ảnh, nên ở đây không
 * vẽ lại bất kỳ khung cửa sổ giả nào — chỉ một tấm kính bo góc, ảnh nằm gọn bên
 * trong, kèm một nhãn nổi ở góc.
 */
export function ShotFrame({
  label,
  children,
  className = "",
  floating = true,
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
  floating?: boolean;
}) {
  return (
    <figure className={`group glass relative m-0 overflow-hidden p-2 sm:p-2.5 ${className}`}>
      <Sheen />
      {/* Khung cuộn riêng: trên điện thoại ảnh giữ bề rộng đọc được và trượt
          ngang bên trong đây, thay vì làm cả trang bị cuộn ngang. */}
      <div className="shot-scroll overflow-hidden rounded-[12px] bg-white/70 shadow-[0_1px_3px_rgba(14,32,76,0.12)]">
        {children}
      </div>
      {floating ? (
        /* Nhãn đặt ở mép TRÊN chứ không phải mép dưới: ở đầu trang có một tấm
           ảnh thứ hai chồng lên góc dưới bên phải, đặt nhãn ở dưới là bị che. */
        <figcaption className="pointer-events-none absolute left-4 right-4 top-4 sm:left-5 sm:right-auto sm:max-w-[75%]">
          <span className="inline-block truncate rounded-full bg-ink/78 px-3.5 py-1.5 text-[0.76rem] font-semibold text-white shadow-[0_6px_18px_-6px_rgba(14,23,41,0.7)] backdrop-blur-md">
            {label}
          </span>
        </figcaption>
      ) : (
        <figcaption className="px-2 pb-1 pt-3 text-[0.8rem] font-semibold text-ink-soft">
          {label}
        </figcaption>
      )}
    </figure>
  );
}
