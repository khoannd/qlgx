-- Thêm sau khi 0001_init.sql đã áp dụng ở nơi khác — không sửa lại tệp cũ, xem
-- lý do tại migrations/0003_landing_content.sql.
--
-- Slug bài viết /tin-tuc mô tả đầy đủ bản phát hành hiện hành. Trước đây nút
-- "Đọc bài viết đầy đủ" trên trang chủ trỏ CỨNG vào một slug viết chết trong
-- page.tsx ("phat-hanh-phien-ban-4-0-0") — phát hành bản 4.0.1 cho thấy ngay
-- vấn đề: link không tự trỏ sang bài mới. Cột này để trang chủ luôn đọc đúng
-- slug hiện hành từ chính bản ghi release, sửa được qua /admin/release.
ALTER TABLE release ADD COLUMN article_slug TEXT;
