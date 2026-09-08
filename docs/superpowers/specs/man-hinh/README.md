# Spec từng màn hình — bản desktop QLGX

Thư mục này giữ **đặc tả hành vi của từng màn hình bản WinForms hiện tại**, viết ra *trước*
khi migrate màn hình đó sang web.

## Vì sao cần

Người dùng đã yêu cầu: *"đảm bảo toàn bộ logic của từng màn hình được migrate sang hệ thống
mới để giáo xứ có thể dùng bản web hoàn toàn được"*. Chuyển đúng các trường dữ liệu là chưa
đủ — phần lớn giá trị của phần mềm nằm ở **hành vi**: quy tắc kiểm tra, giá trị mặc định,
thao tác chuột phải, phím tắt, cách bật/tắt nút, thứ tự sắp xếp, thông báo lỗi. Những thứ này
không nằm trong CSDL và sẽ mất im lặng nếu không được ghi lại.

Spec viết ra từ **mã nguồn desktop**, không phải từ trí nhớ hay suy đoán.

## Qui trình bắt buộc cho mỗi màn hình

1. Đọc kỹ mã nguồn màn hình đó trong `Source/` (kể cả file `.Designer.cs` để lấy bố cục,
   nhãn, thứ tự tab) và các `UserControl` mà nó dùng.
2. Viết spec theo khuôn mẫu dưới đây.
3. **Rồi mới** migrate. Khi migrate xong, đánh dấu đối chiếu từng mục trong spec.

Mã nguồn desktop mã hoá **UTF-16LE** — phải `iconv -f UTF-16LE -t UTF-8` thì `grep` mới đọc được.

## Nguyên tắc viết

- Ghi **hành vi thật quan sát được trong mã**, kèm trích dẫn `đường/dẫn/file.cs:dòng`.
- Chỗ nào **không chắc** thì ghi rõ là chưa chắc — đừng đoán rồi viết như sự thật.
- Ghi cả những chỗ bản desktop làm **sai hoặc kỳ quặc**, và đề xuất bản web nên xử lý thế nào.
  Không im lặng "sửa" hành vi cũ: người dùng đã quen bản cũ, mọi thay đổi phải là quyết định
  có ý thức và được ghi lại.
- Viết bằng tiếng Việt, thuật ngữ giữ nguyên như bản desktop để người dùng nhận ra.

## Khuôn mẫu

```markdown
# Màn hình: <tên tiếng Việt> (`<frmX.cs>`)

| | |
|---|---|
| Tệp nguồn | `Source/.../frmX.cs` (N dòng) |
| UserControl dùng lại | ... |
| Bảng dữ liệu đụng tới | ... |
| Trạng thái migrate | chưa / đang / xong (kèm màn hình web tương ứng) |

## 1. Mục đích
Màn hình này để làm gì, ai dùng, mở ra từ đâu.

## 2. Bố cục và các trường
Bảng: nhãn hiển thị · tên cột CSDL · kiểu · bắt buộc? · mặc định · ghi chú.

## 3. Hành vi khi tải
Dữ liệu nạp lúc mở, thứ tự sắp xếp mặc định, bộ lọc mặc định, con trỏ đặt ở đâu.

## 4. Quy tắc nghiệp vụ và kiểm tra dữ liệu
Từng quy tắc kèm trích dẫn dòng mã. Thông báo lỗi nguyên văn tiếng Việt.

## 5. Thao tác người dùng
Nút, menu chuột phải, phím tắt, double-click, kéo thả — mỗi thứ làm gì.
Điều kiện bật/tắt của từng nút.

## 6. Lưới dữ liệu (nếu có)
Danh sách cột đúng thứ tự, độ rộng, định dạng, tô màu/gạch ngang có điều kiện, cột ẩn.

## 7. Liên kết sang màn hình khác
Mở màn hình nào, truyền tham số gì, quay về ra sao.

## 8. Khác biệt cố ý ở bản web
Những chỗ bản web **cố tình** làm khác, kèm lý do (ví dụ máy chủ tập trung không ghi file cục bộ).

## 9. Chỗ chưa chắc
Những gì chưa đọc ra được từ mã nguồn, cần hỏi người dùng.
```

## Danh mục màn hình

Cập nhật bảng này mỗi khi thêm spec.

| Màn hình | Tệp nguồn | Dòng | Spec | Migrate |
|---|---|---|---|---|
| Chi tiết gia đình | `GXControl/frmGiaDinh.cs` | 2101 | xong (`gia-dinh-chi-tiet.md`) | xong |
| Chi tiết giáo dân | `GXControl/frmGiaoDan.cs` | 1547 | xong (`giao-dan-chi-tiet.md`) | xong |
| Danh sách gia đình | `ChuongTrinh/frmGiaDinhList.cs` | 386 | xong (`gia-dinh-danh-sach.md`) | xong |
| Danh sách giáo dân | `ChuongTrinh/frmGiaoDanList.cs` | 357 | xong (`giao-dan-danh-sach.md`) | xong |
| Màn hình chính | `ChuongTrinh/frmMain.cs` | 993 | chưa | một phần |
| Hôn phối | `GXControl/frmHonPhoi.cs` + `GXControl/GxHonPhoiGiaDinh.cs` | 422 + 300 | xong (`hon-phoi.md`) | một phần (xem mục 10) |
| Rao hôn phối | `ChuongTrinh/frmRaoHonPhoiList.cs` + `GXControl/frmRaoHonPhoi.cs` | 177 + 637 | xong (`rao-hon-phoi.md`) | một phần (xem mục 10) |
| Ơn gọi tận hiến | `GXControl/GxTanHien.cs` | 291 | xong (`tan-hien.md`) | một phần (xem mục 10) |
| Hội đoàn | `GXControl/frmHoiDoan.cs` + `GXControl/GxHistoryHoiDoan.cs` | 663 + 311 | xong (`hoi-doan.md`) | một phần (xem mục 10) |
| Quản lý giáo phận/giáo hạt/giáo xứ | `ChuongTrinh/frmGiaoXu.cs` | 280 | xong (`quan-ly-giao-xu.md`) | xong |
| Giáo họ | `ChuongTrinh/frmGiaoHo.cs` | 598 | xong (`giao-ho.md`, thay cho phần cũ ở `quan-ly-giao-xu.md` mục 9) | xong (danh sách + thêm/sửa + chọn giáo họ cha; không có nút xoá, xem `can-review-sau.md` mục 37) |
| Danh sách hội đoàn | `ChuongTrinh/frmHoiDoanList.cs` + `GXControl/frmHoiDoan.cs` | 89 + 663 | xong (`hoi-doan-danh-sach.md`) | xong (danh mục + thêm/sửa/xoá hội đoàn + quản lý hội viên) |
| Danh sách sổ bí tích | `ChuongTrinh/frmDotBiTichList.cs` + `ChuongTrinh/frmBiTichChiTiet.cs` | 176 + 574 | xong (`so-bi-tich.md`) | một phần (xem mục 10) |
| Giáo lý (Khối/Lớp/Học viên/Giáo lý viên) | `Giaoly/frmKhoiGiaoLyList.cs` + `frmKhoiGiaoLy.cs` + `frmLopGiaoLy.cs` + `frmHocSinh.cs` | 215+313+724+177 | xong (`giao-ly.md`) | một phần (quản lý khối/lớp/học viên/giáo lý viên xong; Chuyển lớp và Nhập học viên hàng loạt từ Excel chưa migrate, xem `giao-ly.md` mục 8) |
| Quản lý tài khoản | `ChuongTrinh/frmAccoutList.cs` | 440 | xong (`quan-ly-tai-khoan.md`) | xong |
| Đăng nhập | `ChuongTrinh/frmLogin.cs` | 89 | (gộp trong `quan-ly-tai-khoan.md` mục 8) | xong |
| In ấn và chứng nhận | `DBAccess/WordEngine.cs` + `ExcelReport/Report*.cs` (14 mô-đun) | ~140 + nhiều | xong (`in-an.md`) | một phần (hạ tầng + Lý lịch cá nhân xong, 4 mẫu ưu tiên còn lại + phần còn lại chưa) |
| Hỗ trợ nhập liệu (control ngày tháng, tự nhảy ô, gợi ý theo tần suất — xuyên màn hình) | `GXControl/GxDateInput.cs` + `GxTextField.cs` + `DBAccess/CMemory.cs` | 633 + nhiều | xong (`ho-tro-nhap-lieu.md`) | chưa |
| Hồ sơ lưu trữ giáo dân / gia đình | `ChuongTrinh/frmGiaoDanLuuTruList.cs` + `ChuongTrinh/frmGiaDinhLuuTruList.cs` | 263 + 261 | xong (`ho-so-luu-tru.md`) | xong (danh sách + lọc + sửa + xóa vĩnh viễn + xuất Excel; không có "khôi phục" — bản gốc cũng không có, xem mục 4) |
| Công cụ dữ liệu (Kiểm tra/Chuẩn hoá/Chuyển hộ/Tạo ds bí tích tự động) | `ChuongTrinh/frmKiemTraGiaoDanList.cs` + `frmKiemTraGiaDinhList.cs` + `ReviewGiaoDanProcess.cs` + `ReviewGiaDinhProcess.cs` + `frmChuyenHoGiaoDan.cs` + `frmChuyenHoGiaDinh.cs` + `UpdateProcess.cs` + `frmTaoDotBiTich.cs` + `GXControl/GenerateDotBiTichProcess.cs` | 316+346+257+266+170+245+233 (Tạo ds bí tích: đã xác định nguồn, chưa migrate) | đầy đủ (`cong-cu-du-lieu.md`, trừ 1 mục chưa migrate) | một phần (Kiểm tra dữ liệu + Chuyển họ hàng loạt + Chuẩn hoá dữ liệu xong cả 2 nửa giáo dân/gia đình; Tạo danh sách bí tích tự động chưa) |

| Giáo xứ (tự sửa thông tin xứ mình) | `ChuongTrinh/frmGiaoXu.cs` | 280 | xong (`giao-xu.md`) | một phần (tên/địa chỉ/điện thoại/email/website/ghi chú xong; cố ý bỏ sửa giáo phận/giáo hạt, danh sách Linh mục, ảnh đại diện — xem `giao-xu.md` mục 3) |
| Thống kê chung & Biểu đồ | `ChuongTrinh/frmThongKeChung.cs`+`GXControl/GxThongKeChung.cs`+`GxThongKeOnGoi.cs`+`GXControl/Extract.cs` / `ChuongTrinh/frmBieuDo.cs`+5 mô-đun `ExcelReport/Chart*.cs` | 28+848+195+168 / 400+5×~100 | xong (`thong-ke-bieu-do.md`) | một phần (xem mục 10) |

Còn khoảng 60 màn hình nhỏ hơn — bổ sung dần theo thứ tự migrate.
