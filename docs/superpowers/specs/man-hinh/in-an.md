# Hạ tầng in ấn và chứng nhận (thay `Source/ExcelReport/`)

| | |
|---|---|
| Tệp nguồn tham khảo | `Source/DBAccess/WordEngine.cs` (~140 dòng, UTF-8 BOM), `Source/ExcelReport/Report*.cs` (14 mô-đun) |
| Mẫu in gốc | `BIN/Template/Chung/*.doc`/`.xls` (14 mẫu dùng chung), `BIN/Template/BMT/` (mẫu riêng giáo phận Ban Mê Thuột, ghi đè `Chung`) |
| Bảng dữ liệu đụng tới | `GiaoDan`, `GiaoXu`, `GiaoHat`, `GiaoPhan`, `GiaoHo`, `HonPhoi`, `GiaoDanHonPhoi` (tuỳ mẫu) |
| Trạng thái migrate | một phần — hạ tầng dùng chung xong, mẫu "Lý lịch cá nhân" xong; 4 mẫu còn lại của phạm vi ưu tiên (BiTich, PhieuGiaDinh, ChungNhanHonPhoi) **chưa làm** — xem mục 8 |

## 1. Vì sao cần (bối cảnh)

`VIEC-TIEP-THEO.md` mục 1.1 xếp việc này ở mức ưu tiên cao nhất: in ấn là nghiệp vụ **hằng
ngày** của văn phòng giáo xứ (giấy chứng nhận rửa tội/rước lễ/thêm sức/hôn phối, giấy giới
thiệu chuyển xứ, sổ gia đình, lý lịch cá nhân), trong khi 10/12 mục menu chuột phải của màn
hình giáo dân trước lượt này chỉ có nhãn — bấm không làm gì
(`WebApp/src/web/src/components/GxGiaoDanList.tsx`, hàm `menuGiaoDanMacDinh`, các mục không có
`chay`). Chừng nào chưa có, giáo xứ vẫn phải mở bản desktop mỗi ngày để in.

## 2. Cơ chế mẫu in của bản desktop (đọc từ mã nguồn)

`Source/DBAccess/WordEngine.cs` bọc `Microsoft.Office.Interop.Word`: mở một mẫu `.doc` làm nền
(`CreateObject(outputPath, templatePath)`), rồi gọi lặp lại `Replace(tenTruong, giaTri)` —
mỗi lời gọi là một `Find & Replace` thô trên toàn văn bản Word, tìm đúng chuỗi `tenTruong` (ví
dụ `TenGiaoXu`, `NgayRuaToi`) và thay bằng giá trị thật. Ví dụ nguyên văn
(`Source/ExcelReport/ReportLyLichCaNhan.cs` dòng 49-59):

```csharp
word.Replace(GiaoPhanConst.TenGiaoPhan, rowGiaoXu[GiaoPhanConst.TenGiaoPhan]);
word.Replace(GiaoHatConst.TenGiaoHat, rowGiaoXu[GiaoHatConst.TenGiaoHat]);
word.Replace(GiaoXuConst.TenGiaoXu, rowGiaoXu[GiaoXuConst.TenGiaoXu]);
word.Replace(GiaoXuConst.DienThoai, rowGiaoXu[GiaoXuConst.DienThoai]);
word.Replace(GiaoXuConst.Email, rowGiaoXu[GiaoXuConst.Email]);
word.Replace(GiaoXuConst.DiaChi, rowGiaoXu[GiaoXuConst.DiaChi]);
word.Replace(GiaoXuConst.Website, rowGiaoXu[GiaoXuConst.Website]);
```

Mẫu chọn theo giáo phận: `BIN/Template/BMT/` chứa các tệp cùng tên với `BIN/Template/Chung/`,
ghi đè khi giáo xứ thuộc giáo phận Ban Mê Thuột (khảo sát thư mục trực tiếp — không dò được
chính xác hàm chọn đường dẫn phía desktop trong lượt khảo sát này vì file định vị hàm đó không
grep được qua `iconv`, xem mục 9). Bài học rút ra, áp dụng cho bản web: **một giáo phận có thể
cần mẫu riêng**, cơ chế chọn mẫu phải theo giáo phận và mặc định về `Chung`.

## 3. Vì sao KHÔNG dùng lại cơ chế đó (ràng buộc kiến trúc)

`WordEngine.cs` dòng 5 `using Microsoft.Office.Interop.Word;` — chạy được vì bản desktop cài
Word ngay trên máy client, chỉ phục vụ một người dùng một lúc. Máy chủ tập trung của bản web
phục vụ nhiều giáo xứ, nhiều bản API chạy song song sau bộ cân bằng tải (ràng buộc HA — xem
`Program.cs`), nên:

- Không cài Word được trên container Linux.
- Interop chạy nhiều luồng đồng thời không ổn định (Word COM không hướng đến kịch bản đa luồng
  máy chủ), một tiến trình Word treo lại ăn hết bộ nhớ.

**Hướng đã chốt**: mẫu **HTML** có chỗ trống → **Playwright headless Chromium** → **PDF**.
Excel thật (mẫu `.xls` — `DanhSachRaoHonPhoi.xls`, xuất Excel giáo dân) dùng **ClosedXML**, để
lượt sau (xem mục 8).

## 4. Hạ tầng dùng chung (đã làm xong lượt này)

| Tệp | Vai trò |
|---|---|
| `WebApp/src/Qlgx.Api/Printing/BoTrinhDuyet.cs` | Giữ **một** trình duyệt Chromium headless dùng chung cho cả tiến trình API (Singleton) — mở một `IPage` mới cho mỗi yêu cầu in, đóng ngay sau khi lấy byte PDF. KHÔNG mở tiến trình Chromium mới mỗi yêu cầu (xem lý do trong ghi chú của lớp). |
| `WebApp/src/Qlgx.Api/Printing/BoDoMauIn.cs` | Nạp mẫu HTML nhúng trong assembly (`EmbeddedResource`, xem `Qlgx.Api.csproj`), thay `{{TenCot}}` bằng giá trị đã `HtmlEncoder.Default.Encode(...)` — chặn chèn HTML/script từ dữ liệu người dùng nhập (họ tên, ghi chú tự do…). Chọn mẫu theo giáo phận (`ChuanHoaTenGiaoPhan`) — thử thư mục riêng trước, rơi về `Chung` khi không có. |
| `WebApp/src/Qlgx.Api/PrintTemplates/Chung/*.html` | Mẫu HTML dùng chung — nhúng vào assembly qua `<EmbeddedResource Include="PrintTemplates\**\*.html" />`, không phải tệp rời cạnh tệp thực thi (đi cùng bản build dù triển khai kiểu container nào). |
| `WebApp/src/Qlgx.Api/Services/InAnService.cs` | Một phương thức `Xuat*` cho mỗi loại giấy — dựng dữ liệu (đã lọc theo `IBoiCanhGiaoXu`, KHÔNG `Find()`/`FindAsync()`), gọi `BoDoMauIn` rồi `BoTrinhDuyet` để ra PDF. |

Đăng ký DI (`Program.cs`): `BoTrinhDuyet`/`BoDoMauIn` là **Singleton** (không giữ trạng thái
riêng từng yêu cầu, dùng chung trình duyệt), `InAnService` là **Scoped** (phụ thuộc
`QlgxDbContext` vốn Scoped).

Endpoint mẫu (trong nhóm `/api/giao-dan`, đã `RequireAuthorization()` và lọc `GiaoXuId` qua
claim, không nhận từ tham số):

```
GET /api/giao-dan/{id}/in/ly-lich-ca-nhan  →  application/pdf (tải về, không mở tab mới)
```

Trả `404` nếu không tìm thấy giáo dân (đã xoá mềm, id sai, **hoặc thuộc giáo xứ khác** — bộ
lọc toàn cục của `QlgxDbContext` áp dụng bình thường, đã có test xác nhận không in chéo giáo
xứ, xem `GiaoDanInAnTests.Khong_in_duoc_giao_dan_cua_giao_xu_khac`).

Ngày tháng trong mọi mẫu định dạng `dd/MM/yyyy` (hàm `Ngay()` trong `InAnService.cs`), đúng quy
ước chung của toàn hệ thống.

## 5. Mẫu "Lý lịch cá nhân" (đã làm xong lượt này)

Dựng lại từ `Source/ExcelReport/ReportLyLichCaNhan.cs` (đọc toàn bộ, xem trích dẫn mục 2) —
tương đương `BIN/Template/Chung/LyLichCaNhan.doc`. Chỗ trống trong mẫu HTML
(`PrintTemplates/Chung/LyLichCaNhan.html`), ứng với đúng các trường bản desktop thay (tên biến
giữ nguyên để đối chiếu được với mã cũ):

- Giáo xứ: `TenGiaoPhan`, `TenGiaoHat`, `TenGiaoXu`, `DiaChiGiaoXu`, `DienThoaiGiaoXu`,
  `EmailGiaoXu`, `TenGiaoHo`.
- Nhân thân: `MaGiaoDan`, `HoTen` (ghép `TenThanh` + `HoTen`), `NgaySinh`, `NoiSinh`, `Phai`,
  `VaiTro` (Chồng/Vợ/Con, suy từ `ThanhVienGiaDinh.VaiTro` — cùng công thức
  `VaiTro > Vợ ⇒ Con` mà `GiaoDanService.LayChiTiet` đã dùng), `TenCha`, `TenMe`.
- Liên lạc/học vấn: `DiaChiGiaoDan`, `DienThoaiGiaoDan`, `EmailGiaoDan`, `DanToc`, `NgheNghiep`,
  `TrinhDoVanHoa`, `TrinhDoChuyenMon`, `BietNgoaiNgu`.
- Bí tích: `SoRuaToi`/`NgayRuaToi`/`NoiRuaToi`/`ChaRuaToi`/`NguoiDoDauRuaToi`,
  `SoRuocLe`/`NgayRuocLe`/`NoiRuocLe`/`ChaRuocLe`,
  `SoThemSuc`/`NgayThemSuc`/`NoiThemSuc`/`ChaThemSuc`/`NguoiDoDauThemSuc`.
- Hôn phối (nếu có, lấy bản mới nhất theo `NgayHonPhoi`, cùng cách tra không N+1 mà
  `GiaoDanService.LayHonPhoi` đã dùng): `SoHonPhoi`, `VoChong` (tên người kia), `NgayHonPhoi`,
  `NoiHonPhoi`, `ChaHonPhoi` (`LinhMucChung`), `CachThucHonPhoi`, `NguoiChung1`, `NguoiChung2`.
- Tình trạng khác: `ConHoc`/`TanTong`/`DaCoGiaDinh` (dấu `[x]`/`[  ]`, đúng ký hiệu bản desktop
  dùng — xem `ReportLyLichCaNhan.cs`: `(bool)... ? "[x]" : "[  ]"`), `QuaDoi`/`NgayQuaDoi`/
  `NoiAnTang`/`SoAnTang` (chỉ hiện khi `QuaDoi = true`, đúng nhánh `if/else` gốc).
- `NgayThangNamIn`: ngày lập giấy = hôm nay, `dd/MM/yyyy` (bản desktop có thêm bản tiếng Anh
  khi cấu hình ngôn ngữ — bản web CHƯA có màn hình đổi ngôn ngữ nào, bỏ nhánh đó có chủ đích).

Bố cục: quốc hiệu → tên giáo phận/giáo hạt/giáo xứ → tiêu đề "LÝ LỊCH CÁ NHÂN" → bảng thông
tin → 3 khối bí tích/hôn phối/tình trạng khác → chỗ ký tên "Linh mục chính xứ" — theo đúng tinh
thần văn bản hành chính Việt Nam, khổ A4 (`BoTrinhDuyet.XuatPdfAsync`: `Format = "A4"`).

Nối vào giao diện: nút "In lý lịch cá nhân" ở `GiaoDanDetail.tsx` (gọi `api.giaoDan
.inLyLichCaNhan` qua `GiaoDanDetailPage.tsx`, vô hiệu hoá khi bản ghi chưa lưu) và mục "In lý
lịch cá nhân" trên menu chuột phải của `GxGiaoDanList.tsx` (đều tải PDF về máy qua
`taiTepIn()` trong `api/client.ts`, đọc tên tệp thật từ header `Content-Disposition`).

## 6. Chọn mẫu theo giáo phận

`BoDoMauIn.ChuanHoaTenGiaoPhan(tenGiaoPhan)` chuẩn hoá tên giáo phận (bỏ dấu, bỏ khoảng trắng —
ví dụ "Ban Mê Thuột" → "BanMeThuot") làm tên thư mục mẫu ưu tiên; `Dung()` thử
`PrintTemplates/{giaoPhanDaChuanHoa}/{tenMau}.html` trước, rơi về
`PrintTemplates/Chung/{tenMau}.html` khi không có. Dữ liệu thật hiện tại chỉ có giáo phận
**Phan Thiết** (không có mẫu riêng) nên luôn dùng `Chung` — cơ chế thư mục riêng dựng sẵn để
lượt sau chỉ cần thêm thư mục là có mẫu riêng ngay, không cần sửa mã. Đây là một lựa chọn có
chủ đích, khác cách bản desktop định vị theo tên thư mục cố định `BMT` (không có ràng buộc rõ
với một cột giáo phận cụ thể trong dữ liệu — xem mục 9): bản web buộc mẫu riêng phải khớp với
`GiaoPhan.TenGiaoPhan` thật trong CSDL, không có bảng ánh xạ tên thư mục tuỳ ý nào khác.

## 7. Kiểm thử

- Backend: `WebApp/tests/Qlgx.Api.Tests/GiaoDanInAnTests.cs` — xuất PDF thành công (kiểm chữ ký
  tệp `%PDF-`), 404 khi không tìm thấy, và KHÔNG in được giáo dân của giáo xứ khác (cách ly dữ
  liệu — đúng ràng buộc "giao_xu_id luôn từ claim").
- Frontend: `GiaoDanDetail.test.tsx` (nút In lý lịch cá nhân: vô hiệu hoá khi chưa có `onIn`,
  gọi đúng khi bấm, đổi nhãn khi `dangIn`), `GxGiaoDanList.test.tsx` (menu 12 mục, mọi mục đều
  có `chay` — không còn mục nào im lặng không phản hồi, mục in gọi đúng
  `api.giaoDan.inLyLichCaNhan`).
- Chạy thật qua trình duyệt (Playwright MCP), in cho một giáo dân thật trong `qlgx_thu`, mở PDF
  kiểm tra — xem ảnh chụp `WebApp/anh-chup-kiem-thu/` (đánh số theo báo cáo nhiệm vụ) và PDF
  mẫu lưu kèm.

## 8. Phạm vi CHƯA làm ở lượt này (khác biệt cố ý với yêu cầu ban đầu)

Yêu cầu ban đầu xếp thứ tự 5 mẫu: hạ tầng, Lý lịch cá nhân, Chứng nhận bí tích (`BiTich.doc`),
Phiếu gia đình (`PhieuGiaDinh.doc`), Chứng nhận hôn phối (`ChungNhanHonPhoi.doc`). Lượt này chỉ
kịp làm hạ tầng + Lý lịch cá nhân cho THẬT TỐT (đã kiểm thử tự động lẫn chạy tay), quyết định
có chủ đích dừng ở đây thay vì làm dở cả 5 mẫu — xem
`docs/superpowers/specs/man-hinh/can-review-sau.md` mục ghi quyết định của nhiệm vụ này.

Còn lại, CHƯA làm (mọi mục menu tương ứng đã đổi từ "im lặng" sang báo "chức năng này chưa được
hỗ trợ trên web ở giai đoạn này" qua `chuaHoTro()`, không còn mục nào bấm không phản hồi):

- Chứng nhận bí tích (`BiTich.doc` — `Source/ExcelReport/ReportChungNhanBT.cs`).
- Phiếu gia đình (`PhieuGiaDinh.doc`/`PhieuGiaDinh-A3.doc` — `ReportSoGiaDinh.cs`).
- Chứng nhận hôn phối (`ChungNhanHonPhoi.doc` — `ReportChungNhanHP.cs`).
- Giấy giới thiệu (chuyển xứ/rửa tội/thêm sức/giáo lý hôn phối — 4 mẫu, `ReportGioiThieuHP.cs`
  và các `GxConstants.REPORT_GIOITHIEU_*`).
- Rao hôn phối + danh sách rao (`RaoHonPhoi.doc`, `KQRaoHonPhoi.doc`,
  `DanhSachRaoHonPhoi.xls` — `ReportRaoHP.cs`).
- Xuất Excel thật bằng ClosedXML (`ExportGrid.cs`, `mau-excel-giaodan*.xls`) — nút "Xuất dữ
  liệu (CSV)" hiện tại chỉ xuất CSV thô phía trình duyệt, không dùng mẫu Excel định dạng sẵn.
- 5 mô-đun biểu đồ (`Chart*.cs`) — theo đúng chỉ đạo "để sau, không thuộc lượt này".

## 9. Chỗ chưa chắc

- Không xác định được chính xác đoạn mã desktop nào chọn thư mục `BMT` so với `Chung` (file
  chứa hàm `Memory.GetReportTemplatePath` không tìm thấy dạng `.cs` nguồn qua tìm kiếm trực
  tiếp trong `Source/` ở lượt khảo sát này, chỉ thấy trong các `.dll`/`.pdb` đã biên dịch) — cơ
  chế chọn theo `TenGiaoPhan` chuẩn hoá ở mục 6 là suy luận hợp lý từ tên thư mục quan sát
  được, KHÔNG phải đối chiếu trực tiếp từ mã nguồn. Cần xác nhận lại nếu sau này giáo xứ Ban Mê
  Thuột thật sự dùng bản web và cần mẫu riêng.
- Chưa rõ bản desktop có cho phép nhiều mẫu riêng khác ngoài `Chung`/`BMT` hay không (ví dụ mỗi
  giáo phận một thư mục) — dữ liệu khảo sát chỉ thấy 2 thư mục.
