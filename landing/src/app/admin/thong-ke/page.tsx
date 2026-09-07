import Link from "next/link";
import { redirect } from "next/navigation";

import { requireSession } from "@/lib/admin/auth";

export const metadata = { title: "Thống kê tải & cập nhật", robots: { index: false, follow: false } };
export const dynamic = "force-dynamic";

type ByGroup = { label: string; n: number };
type LogRow = {
  created_at: number;
  channel: string;
  kenh: string;
  version: string;
  ip: string | null;
  country: string | null;
};

const DAY_MS = 24 * 60 * 60 * 1000;

export default async function ThongKePage() {
  const db = await requireSession();
  if (!db) redirect("/admin/login");

  const since30d = Date.now() - 30 * DAY_MS;
  const since7d = Date.now() - 7 * DAY_MS;

  const [total, last7d, last30d, byChannel, byVersion, byCountry, recent] = await Promise.all([
    db.prepare("SELECT COUNT(*) AS n FROM download_log").first<{ n: number }>(),
    db
      .prepare("SELECT COUNT(*) AS n FROM download_log WHERE created_at >= ?")
      .bind(since7d)
      .first<{ n: number }>(),
    db
      .prepare("SELECT COUNT(*) AS n FROM download_log WHERE created_at >= ?")
      .bind(since30d)
      .first<{ n: number }>(),
    db
      .prepare(
        "SELECT channel AS label, COUNT(*) AS n FROM download_log WHERE created_at >= ? GROUP BY channel ORDER BY n DESC",
      )
      .bind(since30d)
      .all<ByGroup>(),
    db
      .prepare(
        "SELECT version AS label, COUNT(*) AS n FROM download_log WHERE created_at >= ? GROUP BY version ORDER BY n DESC LIMIT 10",
      )
      .bind(since30d)
      .all<ByGroup>(),
    db
      .prepare(
        "SELECT COALESCE(country, '(không rõ)') AS label, COUNT(*) AS n FROM download_log WHERE created_at >= ? GROUP BY country ORDER BY n DESC LIMIT 10",
      )
      .bind(since30d)
      .all<ByGroup>(),
    db
      .prepare(
        "SELECT created_at, channel, kenh, version, ip, country FROM download_log ORDER BY created_at DESC LIMIT 50",
      )
      .all<LogRow>(),
  ]);

  return (
    <main className="wrap py-16">
      <Link href="/admin" className="link-underline text-[0.9rem] text-ink-soft">
        ← Quay lại
      </Link>

      <h1 className="mt-4 font-display text-[1.8rem] font-semibold text-ink">
        Thống kê tải &amp; cập nhật
      </h1>
      <p className="mt-2 max-w-[62ch] text-[0.92rem] text-ink-soft">
        Đếm lượt tải thật (bấm nút trên trang web, hoặc chương trình tự tải gói cập nhật) — không
        đếm lượt chỉ mở chương trình lên và tự kiểm tra phiên bản.
      </p>

      <div className="mt-8 grid gap-4 sm:grid-cols-3">
        <div className="glass p-6">
          <p className="text-[0.78rem] font-bold uppercase tracking-[0.1em] text-ink-faint">
            Tổng cộng
          </p>
          <p className="mt-2 font-display text-[2rem] font-semibold text-ink">{total?.n ?? 0}</p>
        </div>
        <div className="glass p-6">
          <p className="text-[0.78rem] font-bold uppercase tracking-[0.1em] text-ink-faint">
            7 ngày qua
          </p>
          <p className="mt-2 font-display text-[2rem] font-semibold text-ink">{last7d?.n ?? 0}</p>
        </div>
        <div className="glass p-6">
          <p className="text-[0.78rem] font-bold uppercase tracking-[0.1em] text-ink-faint">
            30 ngày qua
          </p>
          <p className="mt-2 font-display text-[2rem] font-semibold text-ink">{last30d?.n ?? 0}</p>
        </div>
      </div>

      <div className="mt-6 grid gap-6 md:grid-cols-3">
        <StatTable title="Theo kênh (30 ngày)" rows={byChannel.results} />
        <StatTable title="Theo phiên bản (30 ngày)" rows={byVersion.results} />
        <StatTable title="Theo quốc gia (30 ngày)" rows={byCountry.results} />
      </div>

      <h2 className="mt-10 font-display text-[1.3rem] font-semibold text-ink">
        50 lượt gần nhất
      </h2>
      <div className="glass mt-4 overflow-x-auto p-2">
        <table className="w-full min-w-[640px] border-collapse text-[0.86rem]">
          <thead>
            <tr className="text-left text-[0.7rem] font-bold uppercase tracking-[0.08em] text-ink-faint">
              <th className="px-4 py-2.5">Thời điểm</th>
              <th className="px-4 py-2.5">Kênh</th>
              <th className="px-4 py-2.5">Loại</th>
              <th className="px-4 py-2.5">Phiên bản</th>
              <th className="px-4 py-2.5">Quốc gia</th>
              <th className="px-4 py-2.5">IP</th>
            </tr>
          </thead>
          <tbody>
            {recent.results.map((row, i) => (
              <tr key={i} className="border-t border-white/60">
                <td className="whitespace-nowrap px-4 py-2 tabular-nums text-ink-soft">
                  {new Date(row.created_at).toLocaleString("vi-VN")}
                </td>
                <td className="px-4 py-2">{row.channel === "web" ? "Web" : "Chương trình"}</td>
                <td className="px-4 py-2 font-mono text-[0.8rem]">{row.kenh}</td>
                <td className="px-4 py-2 tabular-nums">{row.version}</td>
                <td className="px-4 py-2">{row.country ?? "—"}</td>
                <td className="px-4 py-2 font-mono text-[0.8rem] text-ink-faint">{row.ip ?? "—"}</td>
              </tr>
            ))}
            {recent.results.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-ink-soft">
                  Chưa có lượt tải nào được ghi nhận.
                </td>
              </tr>
            ) : null}
          </tbody>
        </table>
      </div>
    </main>
  );
}

function StatTable({ title, rows }: { title: string; rows: ByGroup[] }) {
  return (
    <div className="glass p-5">
      <h3 className="font-display text-[1rem] font-semibold text-ink">{title}</h3>
      <ul className="mt-3 grid list-none gap-1.5 p-0 text-[0.88rem]">
        {rows.map((r) => (
          <li key={r.label} className="flex items-center justify-between gap-3">
            <span className="text-ink-soft">{r.label}</span>
            <span className="tabular-nums font-semibold text-ink">{r.n}</span>
          </li>
        ))}
        {rows.length === 0 ? <li className="text-ink-faint">Chưa có dữ liệu.</li> : null}
      </ul>
    </div>
  );
}
