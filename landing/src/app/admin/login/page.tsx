export const metadata = { title: "Đăng nhập quản trị", robots: { index: false, follow: false } };

export default async function AdminLoginPage({
  searchParams,
}: {
  searchParams: Promise<{ error?: string }>;
}) {
  const { error } = await searchParams;

  return (
    <main className="wrap flex min-h-[70vh] items-center justify-center py-20">
      <div className="glass w-full max-w-sm p-8">
        <h1 className="font-display text-[1.6rem] font-semibold text-ink">Quản trị QLGX</h1>
        <p className="mt-2 text-[0.92rem] text-ink-soft">
          Đăng nhập để sửa bản phát hành và bài viết.
        </p>

        {error ? (
          <p className="mt-4 rounded-lg border-l-4 border-l-amber bg-amber/8 px-4 py-3 text-[0.88rem] text-ink">
            {error === "sai-mat-khau" ? "Sai mật khẩu." : decodeURIComponent(error)}
          </p>
        ) : null}

        <form method="post" action="/admin/api/login" className="mt-6 grid gap-4">
          <label className="grid gap-1.5 text-[0.88rem] font-semibold text-ink">
            Mật khẩu
            <input
              type="password"
              name="password"
              required
              autoFocus
              className="rounded-xl border border-white/70 bg-white/70 px-4 py-2.5 text-[0.96rem] text-ink outline-none focus:border-brand"
            />
          </label>
          <button type="submit" className="btn btn-primary w-full">
            Đăng nhập
          </button>
        </form>
      </div>
    </main>
  );
}
