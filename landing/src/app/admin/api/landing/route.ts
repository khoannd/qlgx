import { NextResponse } from "next/server";

import { requireSession } from "@/lib/admin/auth";
import { saveLandingContent } from "@/lib/content/d1-provider";
import type {
  Feature,
  FaqItem,
  GlassTone,
  HelpLink,
  IconName,
  LandingContent,
  PainPoint,
  TrustStat,
} from "@/lib/content/types";
import { uniqueSlug } from "@/lib/slugify";

import { ICON_VALUES } from "../../trang-chu/icon-options";

const TONES: GlassTone[] = ["cobalt", "ruby", "amber", "emerald"];

function fail(request: Request, message: string) {
  return NextResponse.redirect(
    new URL(`/admin/trang-chu?error=${encodeURIComponent(message)}`, request.url),
    303,
  );
}

/**
 * Duyệt các dòng được đánh số `{prefix}_{i}` do `LandingContentForm` render —
 * xem chú thích ở đầu file đó về vì sao form không dùng JS để thêm/bớt dòng.
 * Dừng lại ở dòng đầu tiên không tồn tại field nào cả (vượt quá số dòng đã
 * render), nên không cần biết trước có bao nhiêu dòng.
 */
function collectRows<T>(
  form: FormData,
  prefixes: string[],
  build: (get: (p: string) => string, has: (p: string) => boolean, index: number) => T | null,
): T[] {
  const result: T[] = [];
  for (let i = 0; ; i++) {
    if (!prefixes.some((p) => form.has(`${p}_${i}`))) break;
    const get = (p: string) => String(form.get(`${p}_${i}`) ?? "").trim();
    const has = (p: string) => form.has(`${p}_${i}`);
    const item = build(get, has, i);
    if (item) result.push(item);
  }
  return result;
}

export async function POST(request: Request) {
  const db = await requireSession();
  if (!db) return NextResponse.redirect(new URL("/admin/login", request.url), 303);

  const form = await request.formData();

  const trustStats = collectRows<TrustStat>(form, ["ts_value", "ts_label"], (get) => {
    const value = get("ts_value");
    const label = get("ts_label");
    return value && label ? { value, label } : null;
  });

  const painPoints = collectRows<PainPoint>(form, ["pp_title", "pp_body"], (get) => {
    const title = get("pp_title");
    const body = get("pp_body");
    return title && body ? { title, body } : null;
  });

  const usedFeatureIds = new Set<string>();
  const features = collectRows<Feature>(
    form,
    ["feat_icon", "feat_tone", "feat_title", "feat_body"],
    (get) => {
      const icon = get("feat_icon") as IconName | "";
      if (!icon) return null;
      const tone = (get("feat_tone") || "cobalt") as GlassTone;
      const title = get("feat_title");
      const body = get("feat_body");
      if (!title || !body) return null;
      return { id: uniqueSlug(title, usedFeatureIds), icon, tone, title, body };
    },
  );
  for (const f of features) {
    if (!ICON_VALUES.includes(f.icon)) return fail(request, `Icon không hợp lệ cho tính năng "${f.title}".`);
    if (!TONES.includes(f.tone)) return fail(request, `Sắc thái màu không hợp lệ cho tính năng "${f.title}".`);
  }

  const installSteps = collectRows<string>(form, ["step"], (get) => get("step") || null);

  const usedLinkIds = new Set<string>();
  const helpLinks = collectRows<HelpLink>(
    form,
    ["link_icon", "link_title", "link_subtitle", "link_href", "link_external"],
    (get, has) => {
      const icon = get("link_icon") as IconName | "";
      if (!icon) return null;
      const title = get("link_title");
      const href = get("link_href");
      if (!title || !href) return null;
      return {
        id: uniqueSlug(title, usedLinkIds),
        icon,
        title,
        subtitle: get("link_subtitle"),
        href,
        external: has("link_external"),
      };
    },
  );
  for (const l of helpLinks) {
    if (!ICON_VALUES.includes(l.icon)) return fail(request, `Icon không hợp lệ cho liên kết "${l.title}".`);
  }

  const usedFaqIds = new Set<string>();
  const faqs = collectRows<FaqItem>(form, ["faq_question", "faq_answer"], (get) => {
    const question = get("faq_question");
    const answerRaw = get("faq_answer");
    if (!question || !answerRaw) return null;
    const answer = answerRaw
      .split(/\n\s*\n/)
      .map((p) => p.trim())
      .filter(Boolean);
    if (answer.length === 0) return null;
    return { id: uniqueSlug(question, usedFaqIds), question, answer };
  });

  if (trustStats.length === 0 || features.length === 0 || faqs.length === 0) {
    return fail(request, "Cần ít nhất một con số tóm tắt, một tính năng và một câu hỏi thường gặp.");
  }

  const data: LandingContent = { trustStats, painPoints, features, installSteps, helpLinks, faqs };

  try {
    await saveLandingContent(db, data);
  } catch (err) {
    return fail(request, err instanceof Error ? err.message : "Lỗi không xác định khi lưu.");
  }

  return NextResponse.redirect(new URL("/admin/trang-chu?saved=1", request.url), 303);
}
