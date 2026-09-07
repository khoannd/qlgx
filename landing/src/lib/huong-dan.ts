import raw from "./content/huong-dan-data.json";

/**
 * Trang hướng dẫn sử dụng — chuyển từ BIN/help/*.htm (25 trang HTML tĩnh vốn
 * đi kèm bộ cài phần mềm) sang /huong-dan bằng scripts/import-huong-dan.mjs.
 *
 * Dữ liệu ở đây là TĨNH tại thời điểm build (đọc từ tệp JSON đã sinh sẵn),
 * không phải nội dung có thể sửa qua /admin — khác với `src/lib/content`.
 * Muốn cập nhật nội dung: sửa trực tiếp các tệp .htm gốc trong BIN/help/ rồi
 * chạy lại `npm run huong-dan:import`.
 */

export type GuidePage = {
  slug: string;
  title: string;
  group: string;
  /** HTML đã khử trùng ở bước build — xem scripts/import-huong-dan.mjs */
  html: string;
};

export const guidePages: GuidePage[] = raw as GuidePage[];

export function getGuidePage(slug: string): GuidePage | undefined {
  return guidePages.find((p) => p.slug === slug);
}

export function listGuideGroups(): { group: string; pages: GuidePage[] }[] {
  const order: string[] = [];
  const byGroup = new Map<string, GuidePage[]>();
  for (const page of guidePages) {
    if (!byGroup.has(page.group)) {
      byGroup.set(page.group, []);
      order.push(page.group);
    }
    byGroup.get(page.group)!.push(page);
  }
  return order.map((group) => ({ group, pages: byGroup.get(group)! }));
}

export function guideNeighbors(slug: string): { prev?: GuidePage; next?: GuidePage } {
  const index = guidePages.findIndex((p) => p.slug === slug);
  if (index === -1) return {};
  return { prev: guidePages[index - 1], next: guidePages[index + 1] };
}
