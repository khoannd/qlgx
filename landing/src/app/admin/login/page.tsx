export const metadata = { title: "Đăng nhập quản trị", robots: { index: false, follow: false } };

/**
 * Danh sách ĐÓNG các thông báo lỗi. Trước đây trang này in nguyên văn nội dung
 * tham số `?error=` — tức là in cả thông điệp lỗi nội bộ (tên biến bí mật chưa
 * đặt, hướng dẫn cấu hình D1) cho người gọi ẩn danh, và cho phép người ngoài
 * đặt chữ tuỳ ý lên trang đăng nhập bằng một đường link (finding T-4).
 * Chi tiết lỗi thật nằm ở log của Worker.
 */
const THONG_BAO_LOI: Record<string, string> = {
  "sai-mat-khau": "Sai mật khẩu.",
  "loi-he-thong": "Hệ thống đang gặp sự cố. Xem nhật ký Worker để biết chi tiết.",
};

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
            {THONG_BAO_LOI[error] ?? THONG_BAO_LOI["loi-he-thong"]}
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
