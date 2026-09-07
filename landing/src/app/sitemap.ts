import type { MetadataRoute } from "next";

import { content } from "@/lib/content";
import { guidePages } from "@/lib/huong-dan";
import { SITE_URL } from "@/lib/site";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [release, articles] = await Promise.all([content.getRelease(), content.listArticles()]);

  const newest = articles[0]?.publishedAt ?? release.publishedAt;

  return [
    {
      url: `${SITE_URL}/`,
      lastModified: new Date(release.publishedAt),
      changeFrequency: "monthly",
      priority: 1,
    },
    {
      url: `${SITE_URL}/tin-tuc`,
      lastModified: new Date(newest),
      changeFrequency: "weekly",
      priority: 0.7,
    },
    ...articles.map((article) => ({
      url: `${SITE_URL}/tin-tuc/${article.slug}`,
      lastModified: new Date(article.updatedAt ?? article.publishedAt),
      changeFrequency: "yearly" as const,
      priority: 0.6,
    })),
    {
      url: `${SITE_URL}/huong-dan`,
      lastModified: new Date(release.publishedAt),
      changeFrequency: "monthly",
      priority: 0.7,
    },
    ...guidePages.map((page) => ({
      url: `${SITE_URL}/huong-dan/${page.slug}`,
      lastModified: new Date(release.publishedAt),
      changeFrequency: "yearly" as const,
      priority: 0.5,
    })),
  ];
}
