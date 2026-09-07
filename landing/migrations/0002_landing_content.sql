-- Thêm sau khi 0001_init.sql đã được áp dụng ở nơi khác (local hoặc thật) —
-- KHÔNG sửa lại 0001_init.sql cho việc này: `wrangler d1 migrations apply`
-- ghi nhớ từng tệp migration theo tên đã chạy, sửa nội dung một tệp cũ không
-- làm nó chạy lại. Mọi thay đổi lược đồ sau lần đầu đều phải là một tệp mới.
--
-- Các danh sách nội dung của trang chủ sửa được qua /admin/trang-chu: con số
-- tóm tắt, vấn đề của sổ giấy, tính năng, bước cài đặt, liên kết hỗ trợ và câu
-- hỏi thường gặp. Gộp cả vào MỘT cột JSON thay vì một bảng cho mỗi danh sách —
-- cùng lý do với `groups_json`/`downloads_json` trong bảng `release` — các
-- danh sách này luôn được đọc/ghi trọn vẹn cùng lúc từ trang chủ, không bao
-- giờ cần lọc theo một phần tử riêng lẻ. Xem kiểu `LandingContent` trong
-- src/lib/content/types.ts.
CREATE TABLE IF NOT EXISTS landing_content (
  id        INTEGER PRIMARY KEY CHECK (id = 1),
  data_json TEXT NOT NULL
);
