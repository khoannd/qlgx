-- Cơ sở dữ liệu D1 cho nội dung có thể sửa từ trang quản trị (/admin).
--
-- Chỉ đúng một hàng trong `release` (id luôn bằng 1) — bản phát hành hiện hành
-- của phần mềm. `articles` là danh sách bài viết trên /tin-tuc.
--
-- Các mảng lồng nhau (danh sách thay đổi, danh sách tệp tải, thân bài viết)
-- lưu dưới dạng chuỗi JSON trong một cột TEXT thay vì tách thành bảng riêng —
-- D1/SQLite không có kiểu JSON gốc, và với quy mô một trang giới thiệu phần
-- mềm thì tách bảng chỉ thêm độ phức tạp mà không thêm lợi ích.

CREATE TABLE IF NOT EXISTS release (
  id             INTEGER PRIMARY KEY CHECK (id = 1),
  version        TEXT NOT NULL,
  published_at   TEXT NOT NULL,   -- ISO 8601, dạng YYYY-MM-DD
  headline       TEXT NOT NULL,
  summary        TEXT NOT NULL,
  groups_json    TEXT NOT NULL,   -- JSON: ReleaseGroup[]
  downloads_json TEXT NOT NULL    -- JSON: DownloadFile[]
);

CREATE TABLE IF NOT EXISTS articles (
  slug             TEXT PRIMARY KEY,
  title            TEXT NOT NULL,
  excerpt          TEXT NOT NULL,
  category_slug    TEXT NOT NULL,
  category_name    TEXT NOT NULL,
  category_tone    TEXT NOT NULL,  -- cobalt | ruby | amber | emerald
  published_at     TEXT NOT NULL,  -- ISO 8601, dạng YYYY-MM-DD
  updated_at       TEXT,
  reading_minutes  INTEGER NOT NULL,
  author           TEXT,
  body_json        TEXT NOT NULL,  -- JSON: ArticleBlock[]
  sort_order       INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_articles_published_at ON articles (published_at DESC);
CREATE INDEX IF NOT EXISTS idx_articles_category ON articles (category_slug);

-- Phiên đăng nhập quản trị. Token là chuỗi ngẫu nhiên đặt trong cookie
-- httpOnly; không lưu mật khẩu ở đây — mật khẩu là một secret riêng
-- (ADMIN_PASSWORD), so sánh trực tiếp lúc đăng nhập.
CREATE TABLE IF NOT EXISTS admin_sessions (
  token      TEXT PRIMARY KEY,
  created_at INTEGER NOT NULL  -- epoch mili giây
);
