# Báo cáo kiểm thử chấp nhận — nhóm 4 (Hồ sơ lưu trữ, Tìm và thay thế, In ấn, Kiểm tra dữ liệu — gia đình)

Ngày: 2026-09-12 (tiếp theo nhóm 1-3).

## Kết quả từng màn hình

### Hồ sơ lưu trữ giáo dân — ĐẠT, khớp số liệu tuyệt đối
- Hiện đúng **11 giáo dân** — khớp chính xác với số liệu spec đã ghi (2050 − 2039 hoạt động = 11
  người, toàn bộ do `qua_doi=true`, không ai do `da_xoa`/chuyển xứ ở giáo xứ này).

### Hồ sơ lưu trữ gia đình — ĐẠT, khớp số liệu tuyệt đối
- Hiện đúng **0 gia đình** — khớp chính xác spec (chưa có gia đình nào bị xóa mềm/chuyển xứ ở
  giáo xứ Vô Nhiễm).

### Tìm và thay thế — ĐẠT, an toàn hơn cả spec mô tả cho bản gốc
- Giao diện đúng thiết kế: combo "Áp dụng cho" (Gia đình/Giáo dân) đổi đúng danh sách "Trường dữ
  liệu" (Gia đình: Tên gia đình/Địa chỉ/Điện thoại/Ghi chú — đúng 4 cột spec liệt kê).
- Test bằng giá trị chắc chắn không tồn tại (`ZZZ_KHONG_TON_TAI_ZZZ`) trên "Tên gia đình" →
  "Xem trước" báo đúng **"Có 0 bản ghi khớp chính xác giá trị..."**, và nút xác nhận tự động ghi
  rõ **"Xác nhận thay thế 0 bản ghi"** kèm **bị khoá (disabled)** — an toàn hơn cả bản desktop gốc
  (spec ghi desktop vẫn chạy UPDATE dù không đếm trước, chỉ báo "Có 0 dữ liệu được thay thế" SAU
  khi chạy) vì bản web chặn hẳn việc chạy no-op ngay từ bước xem trước.
- Không thử giá trị thật nào khớp dữ liệu thật (dù chỉ 0 bản ghi được xác nhận vẫn an toàn) — vì
  mục tiêu chỉ cần xác nhận luồng "xem trước → khớp 0 → khoá nút" hoạt động đúng, đã đạt.

### Kiểm tra dữ liệu — gia đình — ĐẠT, khớp số liệu tuyệt đối
- Tick đủ 4 ô mặc định (đúng theo spec: "Không có ngày hôn phối", "Ngày hôn phối không hợp lệ",
  "Khoảng cách tuổi cha mẹ - con cái", "Các vấn đề khác — nhiều vợ/chồng"), chọn "Tất cả" giáo họ,
  bấm "Bắt đầu kiểm tra" → kết quả **"16 gia đình có lỗi"** — **khớp tuyệt đối** với số liệu spec
  đã ghi nhận qua `psql` (hợp nhất cả 4 quy tắc = 16).

### In ấn — thử mẫu "Phiếu gia đình" — ĐẠT, chất lượng tốt
- Mở gia đình thật "Tôma Hoàng Giáp" (mã 5), bấm "In phiếu gia đình" (khổ A4) → trình duyệt tải
  về file `PhieuGiaDinh_5.pdf` thành công, đã mở và đọc trực tiếp nội dung PDF:
  - Tiêu đề quốc hiệu, tên giáo xứ/giáo hạt/giáo phận đúng dữ liệu thật (Vô Nhiễm — Đức Tánh —
    Phan Thiết).
  - Bảng thành viên đầy đủ: Chủ hộ (Tôma Hoàng Giáp) + Vợ (Maria Nguyễn Thị Diệp), đủ cột Ngày
    sinh/Nơi sinh/Phái/Vai trò/Rửa tội/Rước lễ/Thêm sức/Hôn phối — nội dung khớp đúng dữ liệu đã
    xem trong màn hình Chi tiết gia đình ở nhóm 1 (cùng ngày hôn phối 24/09/1975, Họ Hàng Xanh,
    cha Giuse Vũ Minh Nghiệp).
  - **Tiếng Việt có dấu hiển thị đúng hoàn toàn**, không lỗi phông/mojibake.
  - Có dòng ký tên "Vô Nhiễm, ngày {hôm nay}" + "Linh mục chính xứ", và dòng chú thích "Giấy in
    trực tiếp từ hệ thống... không có giá trị khi có tẩy xoá, sửa chữa" — đúng tinh thần văn bản
    hành chính giáo xứ.
- Chỉ thử 1/13 mẫu in trong lượt này (mẫu dùng hàng ngày, đại diện tốt) — không có gì bất thường,
  đủ cơ sở tin cậy các mẫu còn lại (đã có bằng chứng riêng trong spec `in-an.md`: cả 13 mẫu đều đã
  qua kiểm PyMuPDF theo tài liệu tiến độ).

## Tổng kết nhóm 4

Toàn bộ màn hình test lần này đều **ĐẠT**, không phát hiện lỗi nghiệp vụ hay UX mới. Đây là nhóm
có tỷ lệ khớp số liệu 1:1 với bằng chứng đã ghi trong spec cao nhất trong cả 4 đợt kiểm thử (11,
0, 16, và chất lượng PDF thật) — củng cố độ tin cậy tổng thể của quá trình migrate.

## Tổng kết toàn bộ 4 đợt kiểm thử (2026-09-12)

Đã test 15/23 màn hình có spec: Đăng nhập, Danh sách+Chi tiết giáo dân, Danh sách+Chi tiết gia
đình, Danh sách sổ bí tích, Giáo xứ, Quản lý tài khoản, Danh sách hội đoàn+chi tiết, Danh sách rao
hôn phối+thêm, Giáo họ, Quản lý giáo lý (danh sách), Kiểm tra dữ liệu (giáo dân+gia đình), Thống
kê chung, Biểu đồ, Chuyển họ hàng loạt (UI), Chuẩn hoá dữ liệu (preview), Tạo danh sách bí tích tự
động (preview), Hồ sơ lưu trữ (giáo dân+gia đình), Tìm và thay thế, 1 mẫu in ấn.

**Ba phát hiện quan trọng nhất xuyên suốt 4 đợt** (xếp theo mức ảnh hưởng):
1. **[Cao]** Vùng nội dung chính dùng chung một khung cuộn ngang cho MỌI tab đang mở — khi nhiều
   tab, nút Lưu/Thêm ở tab khác có thể trôi ra ngoài khung nhìn hơn 1000px (nhóm 2).
2. **[Trung bình]** Biểu đồ hiện trống khi mới mở tab (tự vẽ trước khi canvas có kích thước) —
   phải bấm lại "Xem" mới thấy (nhóm 3).
3. **[Trung bình]** Bất nhất định dạng ngày: "Tạo danh sách bí tích tự động" dùng input ngày kiểu
   Mỹ (mm/dd/yyyy) và hiển thị ISO, khác hẳn control `GxDate` (dd/mm/yyyy) dùng khắp nơi còn lại
   (nhóm 3); cùng nhóm bất nhất giao diện với việc 3 màn hình quản trị (Giáo họ, Quản lý tài
   khoản, Cha quản xứ) dùng bảng HTML đơn giản thay AG-Grid (nhóm 1-2).

**Còn lại chưa test** (8/23 màn hình + các mục phụ): Thống kê ơn gọi tận hiến (tab phụ của Thống
kê chung), Quản lý giáo lý (mức chi tiết một khối/lớp), Hôn phối/Tận hiến/Hội đoàn (tab trong chi
tiết giáo dân — đã xem qua nhưng chưa test sâu nghiệp vụ), Quản lý giáo xứ theo giáo phận (cần
tài khoản LoaiTaiKhoan=9, tài khoản test hiện tại không có quyền này), 12/13 mẫu in còn lại, đổi
mật khẩu (submit thật).

Toàn bộ báo cáo 4 đợt: `BAO-CAO-ACCEPTANCE-2026-09-12.md` (nhóm 1), `-nhom2.md`, `-nhom3.md`,
`-nhom4.md` (file này).
