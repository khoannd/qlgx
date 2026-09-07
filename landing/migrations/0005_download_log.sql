-- Ghi lại mỗi lượt tải/cập nhật thật (không phải mỗi lượt xem trang) để làm
-- thống kê sau này: ai đang dùng bản nào, tải qua kênh nào, từ đâu.
--
-- CỐ Ý không log /capnhat/version.txt (app tự hỏi ở MỖI LẦN MỞ CHƯƠNG TRÌNH —
-- log cái đó sẽ ra hàng chục nghìn dòng nhiễu mỗi ngày mà không nói lên điều gì
-- mới hơn dòng log của chính lượt tải/cập nhật thật).
CREATE TABLE IF NOT EXISTS download_log (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  created_at  INTEGER NOT NULL,  -- epoch mili giây
  channel     TEXT NOT NULL,     -- 'web' (nút bấm trên quanlygiaoxu.net) | 'app' (chương trình tự tải)
  kenh        TEXT NOT NULL,     -- vd. 'full', 'update', 'phien-ban-cu:3.3.7', 'capnhat', 'goc', '4.0'
  version     TEXT NOT NULL,     -- số phiên bản của tệp được tải
  ip          TEXT,              -- CF-Connecting-IP, có thể NULL nếu thiếu header
  country     TEXT,              -- CF-IPCountry (Cloudflare tự kèm, không cần tra cứu thêm)
  user_agent  TEXT
);

CREATE INDEX IF NOT EXISTS idx_download_log_created_at ON download_log (created_at DESC);
CREATE INDEX IF NOT EXISTS idx_download_log_channel ON download_log (channel);
