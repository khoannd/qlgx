# Hạ tầng in ấn và chứng nhận (thay `Source/ExcelReport/`)

| | |
|---|---|
| Tệp nguồn tham khảo | `Source/DBAccess/WordEngine.cs` (~140 dòng, UTF-8 BOM), `Source/ExcelReport/Report*.cs` (14 mô-đun) |
| Mẫu in gốc | `BIN/Template/Chung/*.doc`/`.xls` (14 mẫu dùng chung), `BIN/Template/BMT/` (mẫu riêng giáo phận Ban Mê Thuột, ghi đè `Chung`) |
| Bảng dữ liệu đụng tới | `GiaoDan`, `GiaoXu`, `GiaoHat`, `GiaoPhan`, `GiaoHo`, `HonPhoi`, `GiaoDanHonPhoi` (tuỳ mẫu) |
| Trạng thái migrate | một phần — hạ tầng dùng chung + 4/5 mẫu ưu tiên xong (Lý lịch cá nhân, Chứng nhận bí tích, Phiếu gia đình, Chứng nhận hôn phối); Giấy giới thiệu (4 mẫu, cần thêm màn hình nhập liệu người thứ hai) **chưa làm** — xem mục 8 |

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

## 5a. Sửa lỗi trình bày dùng chung: bỏ hẳn nhãn/nối rỗng lửng (`Printing/VanBanInAn.cs`)

Tự phát hiện khi xem PDF mẫu "Lý lịch cá nhân": khi một giáo dân KHÔNG có dữ liệu bí tích, dòng
in ra thừa dấu phẩy lửng và chữ "tại" bơ vơ — ví dụ `Số — ngày tại , cha  rửa` (khi có một phần
dữ liệu: `Số 04/15/VN — ngày 25/04/2015 tại , cha LM Pet Nguyễn Huy Hồng rửa, người đỡ đầu Giuse
Trần Quốc Vũ` — thiếu `NoiRuaToi` nhưng vẫn để lại "tại ," bơ vơ). Trông cẩu thả trên giấy tờ
CHÍNH THỨC của giáo xứ.

Sửa ở **tầng dùng chung**, không vá riêng từng mẫu: `VanBanInAn.MoTaBiTich(so, ngayBiTich, noi,
chuSu, hanhDongChuSu, nguoiPhu, nhanNguoiPhu)` ghép câu kiểu gốc "Số X — ngày Y tại Z, cha A rửa,
người đỡ đầu B" nhưng **bỏ HẲN từng đoạn (kể cả liên từ/dấu câu đi kèm)** khi thiếu dữ liệu tương
ứng — không chỉ để trống giá trị mà giữ lại nhãn rỗng. Ví dụ: thiếu `noi` thì bỏ hẳn "tại ...",
không còn dấu phẩy lửng phía sau; thiếu tất cả thì trả chuỗi rỗng (nơi gọi tự quyết định ẩn hẳn
dòng đó hay hiện nhãn với ô trống, tuỳ mẫu). `VanBanInAn.GhepDong(...)` ghép nhiều dòng đã chuẩn
bị sẵn (bỏ dòng rỗng) dùng với CSS `white-space: pre-line`.

Áp dụng lại cho `LyLichCaNhan` (3 dòng bí tích đổi từ 5-6 placeholder rời ghép trực tiếp trong
HTML sang 1 khoá `MoTaRuaToi`/`MoTaRuocLe`/`MoTaThemSuc` đã ghép sẵn trong C#) và dùng làm quy
tắc mặc định cho MỌI mẫu mới (`ChungNhanBiTich`, `PhieuGiaDinh`, `ChungNhanHonPhoi` — xem 5b-5d).
Đã kiểm chứng bằng dữ liệu thật thiếu-đủ khác nhau trong `qlgx_thu` (xem mục 7) — không còn
trường hợp nào để lại giới từ/dấu câu bơ vơ.

## 5b. Mẫu "Chứng nhận bí tích" (đã làm ở lượt này)

Dựng lại từ `Source/ExcelReport/ReportChungNhanBT.cs` — bốn mục menu chuột phải "In chứng nhận
bí tích / rửa tội / xưng tội-rước lễ / thêm sức" (`GxGiaoDanList.tsx`) dùng CHUNG một endpoint
`GET /api/giao-dan/{id}/in/chung-nhan-bi-tich?loai=RuaToi|RuocLe|ThemSuc` (bỏ trống `loai` = mục
chung, liệt kê cả ba) và CHUNG một mẫu `PrintTemplates/Chung/ChungNhanBiTich.html` — chỉ đổi
tiêu đề và dòng bí tích được liệt kê theo `loai` (`InAnService.XuatChungNhanBiTich`). Bản desktop
cũng chỉ đổi TÊN TỆP mẫu theo `LoaiBiTich`, mọi phép `Replace` chạy giống nhau cho cả 4 loại —
bản web tái hiện đúng tinh thần đó với cấu trúc gọn hơn (một mẫu, danh sách nhiều dòng qua
`VanBanInAn.GhepDong`) thay vì 4 tệp `.doc` gần trùng lặp.

CỐ Ý KHÔNG dựng phần "gửi giáo xứ nhận" (`TenLinhMucNhan`/`TenGiaoXuNhan`/`TenGiaoPhanNhan`/
`LyDo` của bản gốc) — đó là dữ liệu của giấy CHUYỂN giáo xứ, cần nhập tay lúc in, chưa có màn
hình nhập ở Phase 1 (xem can-review-sau.md mục 34c). Mẫu web hiện tại là "chứng nhận nội bộ".

## 5c. Mẫu "Phiếu gia đình" (đã làm ở lượt này — KHÔNG có bản A3)

Dựng lại từ `Source/ExcelReport/ReportSoGiaDinh.cs` — nút "In phiếu gia đình" ở `GiaDinhDetail.tsx`
(nối thẳng, không mơ hồ vì luôn là CẢ gia đình đang mở) và mục cùng tên trên menu chuột phải của
`GxGiaDinhList.tsx`. Endpoint `GET /api/gia-dinh/{id}/in/phieu-gia-dinh`
(`InAnService.XuatPhieuGiaDinh`) dựng bảng thành viên co giãn theo đúng số người thật (khác bản
desktop: một bảng Word cố định 8 dòng, chèn thêm dòng khi vượt) — mỗi dòng gồm STT, Tên thánh,
Họ tên, Ngày sinh, Nơi sinh, Phái, Vai trò (Chủ hộ/Chồng/Vợ/Con), ba cột bí tích (mỗi ô một câu
`MoTaBiTich` đã ghép, xem 5a) và cột Hôn phối (chỉ điền ở dòng Chồng/Vợ). Người đã qua đời được
gạch ngang (cùng tinh thần `word.StrikeThroughRow` của bản gốc).

Vì bảng có số dòng KHÔNG cố định, mẫu HTML không thể dùng cơ chế `{{Key}}` thông thường (một chỗ
trống ứng với một giá trị) — `BoDoMauIn.Dung()` thêm tham số `khoiHtmlAnToan` (xem
can-review-sau.md mục 34f) để `InAnService` tự dựng khối `<tr>` lặp lại, tự
`HtmlEncoder.Default.Encode(...)` từng mẩu dữ liệu người dùng trước khi ghép.

**Bản khổ lớn `PhieuGiaDinh-A3.doc` (yêu cầu gốc mục 2) CHƯA làm** — `BoTrinhDuyet.XuatPdfAsync`
hiện cố định `Format = "A4"`; hỗ trợ A3 cần thêm tham số khổ giấy xuyên suốt (`InAnService` →
`BoDoMauIn`/mẫu HTML có bố cục ngang phù hợp hơn cho nhiều cột → `BoTrinhDuyet`). Cân nhắc dừng ở
đây để ưu tiên đủ 3 mẫu tài liệu (bí tích/gia đình/hôn phối) hơn một biến thể khổ giấy của một
mẫu đã có — A4 vẫn in đọc được đầy đủ, chỉ chữ nhỏ hơn khi gia đình đông thành viên.

## 5d. Mẫu "Chứng nhận hôn phối" (đã làm ở lượt này)

Dựng lại từ `Source/ExcelReport/ReportChungNhanHP.cs` — mục "In chứng nhận hôn phối" trên menu
chuột phải của `GxGiaDinhList.tsx`. Endpoint `GET /api/gia-dinh/{id}/in/chung-nhan-hon-phoi`
(`InAnService.XuatChungNhanHonPhoi`) suy ra hôn phối "hiện tại" của gia đình qua
`GiaDinhService.LayVoChongVaHonPhoi` (tái dùng logic đã có, xem can-review-sau.md mục 34g), trả
`404` nếu gia đình chưa có hôn phối nào để chứng nhận (không chỉ khi không tìm thấy gia đình).

Nam/Nữ trên giấy chứng nhận xác định theo `GiaoDan.Phai` THẬT của từng người tham gia
`GiaoDanHonPhoi`, KHÔNG giả định "Chồng luôn là Nam" (xem can-review-sau.md mục 34h) — hiển thị
song song hai cột thông tin (họ tên, ngày/nơi sinh, cha/mẹ, giáo họ, rửa tội, thêm sức) rồi khối
"Nghi thức hôn phối" chung (số/ngày/nơi/chủ sự/cách thức/người chứng). Cũng CỐ Ý bỏ phần "gửi
giáo xứ nhận" như mục 5b.

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

- Backend: `WebApp/tests/Qlgx.Api.Tests/GiaoDanInAnTests.cs` (Lý lịch cá nhân) và
  `InAnMauMoiTests.cs` (3 mẫu mới — Chứng nhận bí tích cả 4 biến thể `loai`, Phiếu gia đình,
  Chứng nhận hôn phối, kể cả trường hợp không đủ dữ liệu bí tích vẫn phải in được và không nhầm
  thành 404) — xuất PDF thành công (kiểm chữ ký tệp `%PDF-`), 404 khi không tìm thấy hoặc (riêng
  Chứng nhận hôn phối) khi gia đình chưa có hôn phối nào, và KHÔNG in được bản ghi của giáo xứ
  khác (cách ly dữ liệu — đúng ràng buộc "giao_xu_id luôn từ claim").
- Frontend: `GiaoDanDetail.test.tsx` (nút In lý lịch cá nhân), `GxGiaoDanList.test.tsx` (menu 12
  mục, mọi mục đều có `chay`, 4 mục bí tích gọi đúng `api.giaoDan.inChungNhanBiTich` với đúng
  `loai`), `GxGiaDinhList.test.tsx` (2 mục in mới gọi đúng `api.giaDinh.inChungNhanHonPhoi`/
  `inPhieuGiaDinh`, 3 mục còn lại vẫn báo "chưa hỗ trợ"), `GiaDinhDetail.test.tsx` (nút "In phiếu
  gia đình" gọi đúng api, vô hiệu hoá khi gia đình còn là bản nháp chưa lưu).
- Chạy thật qua API thật (tài khoản tạm, JWT thật, gọi `curl`/`fetch` vào `Qlgx.Api` chạy trên
  `qlgx_thu`), in cho các bản ghi thật nhiều dữ liệu nhất tìm được, mở PDF kiểm tra — xem ảnh
  chụp/PDF mẫu `WebApp/anh-chup-kiem-thu/` (đánh số theo báo cáo nhiệm vụ) và
  can-review-sau.md mục 34e.

## 8. Phạm vi CHƯA làm ở lượt này (khác biệt cố ý với yêu cầu ban đầu)

Yêu cầu ban đầu xếp thứ tự 5 mẫu: hạ tầng, Lý lịch cá nhân, Chứng nhận bí tích, Phiếu gia đình,
Chứng nhận hôn phối. Hai lượt đã làm xong cả 5/5 phần đó (lượt 1: hạ tầng + Lý lịch cá nhân;
lượt 2 — lượt này: Chứng nhận bí tích, Phiếu gia đình, Chứng nhận hôn phối, xem mục 5a-5d). Còn
lại "Giấy giới thiệu" (4 mẫu) KHÔNG kịp làm — khác các mẫu trên ở chỗ cần dữ liệu của "người thứ
hai" thường KHÔNG có bản ghi `GiaoDan` nào trong CSDL (ở giáo xứ khác), nghĩa là cần thêm MỘT
MÀN HÌNH NHẬP LIỆU mới trước khi in được, không chỉ "thêm một `.html` + một `Xuat*`" như ba mẫu
vừa làm — xem can-review-sau.md mục 34d.

Còn lại, CHƯA làm (mọi mục menu tương ứng vẫn báo "chức năng này chưa được hỗ trợ trên web ở
giai đoạn này" qua `chuaHoTro()`, không có mục nào bấm không phản hồi):

- Giấy giới thiệu (chuyển xứ/rửa tội/thêm sức/giáo lý hôn phối — 4 mẫu, `ReportGioiThieuHP.cs`
  và các `GxConstants.REPORT_GIOITHIEU_*`) — cần màn hình nhập liệu mới, xem trên.
- Phiếu gia đình khổ lớn (`PhieuGiaDinh-A3.doc`) — xem mục 5c, cần thêm tham số khổ giấy xuyên
  suốt hạ tầng in (hiện `BoTrinhDuyet` cố định A4).
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
