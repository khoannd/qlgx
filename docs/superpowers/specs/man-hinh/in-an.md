# Hạ tầng in ấn và chứng nhận (thay `Source/ExcelReport/`)

| | |
|---|---|
| Tệp nguồn tham khảo | `Source/DBAccess/WordEngine.cs` (~140 dòng, UTF-8 BOM), `Source/ExcelReport/Report*.cs` (14 mô-đun) |
| Mẫu in gốc | `BIN/Template/Chung/*.doc`/`.xls` (14 mẫu dùng chung), `BIN/Template/BMT/` (mẫu riêng giáo phận Ban Mê Thuột, ghi đè `Chung`) |
| Bảng dữ liệu đụng tới | `GiaoDan`, `GiaoXu`, `GiaoHat`, `GiaoPhan`, `GiaoHo`, `HonPhoi`, `GiaoDanHonPhoi` (tuỳ mẫu) |
| Trạng thái migrate | **XONG** — hạ tầng dùng chung + 10 mẫu (Lý lịch cá nhân, Chứng nhận bí tích, Phiếu gia đình [A4+A3], Chứng nhận hôn phối, 4 mẫu Giấy giới thiệu, Rao hôn phối, Kết quả rao hôn phối) + "In danh sách"/"In lý lịch cá nhân (gia đình)"/"In sổ gia đình" (mục 5f) + Xuất Excel ClosedXML cho Giáo dân/Gia đình/Sổ bí tích/Rao hôn phối (mục 5h) + khổ giấy chọn được (mục 5c). Còn lại 5 mô-đun biểu đồ — xem mục 8 |

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

**Bản khổ lớn `PhieuGiaDinh-A3.doc` (yêu cầu gốc mục 2) — nay đã làm (lượt "in-excel-a3",
2026-09-08)**. `BoTrinhDuyet.XuatPdfAsync` nhận thêm tham số `khoGiay` (mặc định `"A4"`, GIỮ
NGUYÊN hành vi cũ mọi nơi gọi khác — chỉ truyền qua thẳng cho `PagePdfOptions.Format`).
`InAnService.XuatPhieuGiaDinh(giaDinhId, khoGiay, ct)` dùng lại NGUYÊN VẸN mẫu HTML
`PhieuGiaDinh.html` (bảng đã co giãn theo số người thật ngay từ đầu, không phải 8 dòng cố định
như bản gốc) — khác biệt A3 so với A4 CHỈ nằm ở khổ giấy Playwright xuất ra (nhiều chỗ trống
hơn), không phải bố cục khác, đúng tinh thần "chỉ khác khổ giấy và số cột, đừng viết mẫu mới"
mà nhiệm vụ chỉ đạo. Endpoint `GET /api/gia-dinh/{id}/in/phieu-gia-dinh?khoGiay=A3` chỉ chấp
nhận `"A4"`/`"A3"` (400 nếu khác — không chuyển thẳng chuỗi tuỳ ý xuống Playwright).

Nối vào giao diện: `GiaDinhDetail.tsx` có thêm ô chọn "Khổ giấy" (A4/A3) cạnh nút "In phiếu gia
đình"; menu chuột phải `GxGiaDinhList.tsx` có thêm mục riêng "In phiếu gia đình (khổ A3)"
(nhất quán với cách menu này đã tách "In chứng nhận rửa tội"/"…thêm sức" thành mục riêng thay vì
hộp thoại chọn loại).

**Phát hiện quan trọng khi khảo sát cho việc bổ sung này**: đối chiếu
`Source/GXControl/GxGiaDinhList.cs` (`InPhieuGiaDinh`/`XuatSoGiaDinhChungFile`) và
`Source/ExcelReport/ReportSoGiaDinh.cs` xác nhận **CẢ HAI đường in phiếu gia đình trên desktop
đều gán cứng tên tệp mẫu `"PhieuGiaDinh"`, không có nhánh nào chọn `PhieuGiaDinh-A3.doc`** — mẫu
A3 nằm sẵn trong `BIN/Template/Chung/` nhưng là một mẫu CHẾT trên desktop, không menu/nút nào
từng dùng tới. Việc bổ sung khổ A3 ở web vì vậy là một **khả năng MỚI theo yêu cầu nhiệm vụ**
("người dùng nên chọn được khổ khi in phiếu gia đình"), KHÔNG PHẢI tái hiện hành vi desktop có
sẵn — xem can-review-sau.md mục 69.

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

## 5e. Bốn mẫu "Giấy giới thiệu" (đã làm ở lượt cuối cùng) — cả hai cài đặt gốc là mã CHẾT

Dựng lại từ `Source/ExcelReport/ReportGioiThieuHP.cs` (mẫu cũ, KHÔNG dùng làm chuẩn) và
`Source/GXControl/{ReportGioiThieuRuaToi,RpGioiThieuThemSuc,RpGioiThieuChuyenXu,RpGThieuGlyHPhoi,
RpGioiThieuBase,frmReport}.cs` (mẫu chọn làm chuẩn) — bốn tệp `BIN/Template/Chung/GioiThieu*.doc`.

**Phát hiện quan trọng trước khi migrate**: đối chiếu toàn bộ `Source/` không tìm thấy bất kỳ
chỗ nào gọi `new frmReport(...)` hay `new frmReportGioiThieuHP(...)`, và không menu/nút nào
(ngoài chính hai form đó) chứa chữ "Giới thiệu" — **cả bộ mã "Giấy giới thiệu" của bản desktop
là mã CHẾT**, chưa từng chạy được từ giao diện thật. Không có "hành vi thật đang chạy" để đối
chiếu tuyệt đối; quyết định dưới đây là suy luận hợp lý nhất từ mã nguồn (xem
can-review-sau.md mục 65 để biết đầy đủ lý do).

**Vấn đề "thông tin bên thứ hai"** (lý do hạng mục này bị hoãn ở lượt trước) được bản desktop
giải quyết bằng MỘT MÀN HÌNH NHẬP TAY (`frmReport.cs`) — hai ô nhập tự do `txtGiaoPhan`/
`txtGiaoXu` (giáo phận/giáo xứ NHẬN, KHÔNG tra danh mục Giáo xứ có sẵn vì thường thuộc giáo xứ
khác) cộng một combobox `cbLinhMuc` chọn linh mục ký tên — **không** lấy dữ liệu từ bảng
`RaoHonPhoi` như suy đoán ban đầu (bảng đó phục vụ nghiệp vụ "Rao hôn phối" khác, ngoài phạm vi
4 mẫu này). Có MỘT cài đặt khác (`frmReportGioiThieuHP.cs`/`ReportGioiThieuHP.cs`, cũ hơn, cũng
chết) tự **chèn một bản ghi `GiaoDan` MỚI** vào CSDL cho người phối ngẫu từ dữ liệu gõ tay
(`AddGiaoDan()`) — **KHÔNG migrate side-effect này** (xem lý do ở can-review-sau.md mục 65).

Endpoint (theo GIÁO DÂN, ba mẫu đầu — `InAnService.XuatGioiThieuRuaToi/ThemSuc/GiaoLyHonPhoi`):

```
GET /api/giao-dan/{id}/in/gioi-thieu-rua-toi?giaoPhan2=...&giaoXu2=...&tenLinhMuc=...
GET /api/giao-dan/{id}/in/gioi-thieu-them-suc?giaoPhan2=...&giaoXu2=...&tenLinhMuc=...
GET /api/giao-dan/{id}/in/gioi-thieu-giao-ly-hon-phoi?giaoPhan2=...&giaoXu2=...&tenLinhMuc=...
```

Endpoint (theo GIA ĐÌNH, mẫu chuyển xứ — `InAnService.XuatGioiThieuChuyenXu`, bảng thành viên
co giãn theo số người thật cùng cách `XuatPhieuGiaDinh` đã dùng, khác bản desktop tính toán
AddRow/DeleteRow cố định 7 dòng trên mẫu Word):

```
GET /api/gia-dinh/{id}/in/gioi-thieu-chuyen-xu?giaoPhan2=...&giaoXu2=...&tenLinhMuc=...
```

`giaoXu2` bắt buộc (400 nếu rỗng — không có gì để giới thiệu nếu không biết giới thiệu đi đâu),
`giaoPhan2`/`tenLinhMuc` tuỳ chọn. Ba tham số này **KHÔNG lưu vào CSDL**, chỉ dùng cho một lượt
in — nhập lại từ đầu mỗi lần in, đúng cơ chế `frmReport.cs` gốc.

Sai khác có chủ đích với mã gốc (không phải "sửa cho đúng" âm thầm): dòng "Đã Rửa tội" của mẫu
Thêm sức/Giáo lý hôn phối dùng `VanBanInAn.MoTaBiTich` (bỏ hẳn đoạn thiếu dữ liệu) thay vì
literal `"..................."` mà `RpGioiThieuThemSuc.cs` in khi thiếu `NgayRuaToi` — nhiệm vụ
này chỉ đạo rõ dùng lại cơ chế `VanBanInAn` cho MỌI mẫu mới để không tái diễn kiểu trình bày
cẩu thả (xem mục 5a), và literal dấu chấm khi có `NoiRuaToi` nhưng thiếu `NgayRuaToi` sẽ tạo ra
một dòng còn xấu hơn lỗi dấu phẩy lửng đã sửa.

"Linh mục giới thiệu" ở bản web là Ô NHẬP TỰ DO (không phải danh mục `LinhMuc` như `cbLinhMuc`
gốc) — bản web chưa có màn hình quản lý danh mục Linh mục (bảng `LinhMuc` có trong CSDL, chưa có
API/UI), nhất quán với các trường "tên cha …" khác của `GiaoDan` (`ChaRuaToi`, `ChaThemSuc`…)
vốn cũng là chuỗi tự do trong toàn hệ thống.

Nối vào giao diện: `GioiThieuModal.tsx` (modal dùng CHUNG cho cả 4 mẫu — 3 ô nhập giáo phận/
giáo xứ nhận + linh mục ký tên) mở từ menu chuột phải "In giấy giới thiệu chứng nhận rửa tội/
giáo lý hôn phối/chứng nhận thêm sức" (`GxGiaoDanList.tsx`, dùng ở màn hình Danh sách giáo dân,
Hồ sơ lưu trữ giáo dân, và lưới thành viên trong Chi tiết gia đình) và "In giới thiệu chuyển xứ"
(`GxGiaDinhList.tsx`, dùng ở Danh sách gia đình và Hồ sơ lưu trữ gia đình) — state dùng chung
qua hai hook `lib/useGioiThieuGiaoDan.ts`/`lib/useGioiThieuChuyenXu.ts` để tránh lặp lại ở 5 màn
hình nhúng. Mục "In giới thiệu hôn phối" (`GxGiaoDanList.tsx`, toolbar Hồ sơ lưu trữ giáo dân)
**KHÔNG thuộc 4 mẫu này** — đó là giấy RAO hôn phối (`ReportRaoHP.cs`) — nay đã in được thật, xem
mục 5g (khảo sát lại mã desktop cho lượt "in-excel-a3" phát hiện nút này thật ra wire vào một
mẫu KHÁC trên desktop, xem can-review-sau.md mục 69 mục 1-2).

## 5f. "In danh sách", "In lý lịch cá nhân (gia đình)", "In sổ gia đình" (lượt "hoàn tất mọi thao
tác còn báo chưa hỗ trợ") — xem can-review-sau.md mục 66 để biết đầy đủ lý do nghiên cứu mã
desktop và các quyết định phạm vi.

**"In danh sách"** (`GiaoDanList.tsx`/`GiaDinhList.tsx`, toolbar) — `GET /api/giao-dan/in/danh-
sach`/`GET /api/gia-dinh/in/danh-sach` (`InAnService.XuatDanhSachGiaoDan`/`XuatDanhSachGiaDinh`),
CÙNG bộ tham số lọc (`giaoHoId`/`chiKhongThongKe`/`hienCaDaMat`) với GET danh sách thường và
"Xuất Excel" — gọi lại đúng `GiaoDanService.LayDanhSach`/`GiaDinhService.LayDanhSach`, KHÔNG viết
lại điều kiện lọc. Mẫu `PrintTemplates/Chung/DanhSachGiaoDan.html`/`DanhSachGiaDinh.html`, khổ
NGANG (`BoTrinhDuyet.XuatPdfAsync` nhận thêm tham số `landscape`, mặc định `false` — không đổi
hành vi các mẫu dọc cũ), 29/12 cột đúng thứ tự `cotGiaoDan.ts`/`cotGiaDinh.ts` (khớp
`XuatExcelService`), dựng bằng cơ chế `khoiHtmlAnToan` (số dòng không cố định, như "Phiếu gia
đình"). Chân trang có tổng số dòng và giờ in. Bản gốc thật ra xuất Janus GridEX control ra
`.xls` tạm (`btnInDanhSach_Click`), không phải in giấy trực tiếp — bản web chọn PDF để nhất quán
với các mẫu in khác của hạ tầng này (Excel đã có nút riêng "Xuất Excel").

**"In lý lịch cá nhân" từ lưới GIA ĐÌNH** (`GxGiaDinhList.tsx`, nút ở `GiaDinhDetail.tsx`) —
`GET /api/gia-dinh/{id}/in/ly-lich-ca-nhan` (`InAnService.XuatLyLichCaNhanGiaDinh`) — in lý lịch
cá nhân của TẤT CẢ thành viên gia đình (không riêng chủ hộ), mỗi người một trang PDF, gộp một
tệp, thứ tự `VaiTro` tăng dần — đúng `item4_Click`/`GxGiaoDanList.XuatLyLichCaNhan(Dictionary)`
của bản desktop. Cài đặt: `InAnService.DungHtmlLyLichCaNhan` (dựng HTML một người, tách khỏi
`XuatLyLichCaNhan` cũ để tái dùng — PDF một người KHÔNG đổi so với trước) gọi lặp lại cho từng
thành viên, `TachKieuVaThan` tách `<style>`/`<body>` của mỗi trang đã dựng rồi nối bằng
`page-break-after: always`. 404 khi gia đình không tồn tại hoặc không còn thành viên nào.

**"In sổ gia đình"** (`GiaDinhLuuTruList.tsx`) — reuse THẲNG `GET /api/gia-dinh/{id}/in/phieu-
gia-dinh` (không có endpoint/mẫu riêng) — nghiên cứu mã nguồn xác nhận hai nhãn gọi cùng report
khi chỉ chọn một gia đình (trường hợp duy nhất màn hình lưu trữ hỗ trợ).

Test: `InAnMauMoiTests.cs` (8 test mới — PDF thành công/404/cách ly giáo xứ cho cả ba nhóm),
`GxGiaDinhList.test.tsx`/`GxGiaoDanList.test.tsx`/`GiaoDanList.test.tsx`/`GiaDinhList.test.tsx`/
`GiaDinhLuuTruList.test.tsx`/`GiaDinhDetail.test.tsx`. Chạy thật trên `qlgx_thu`, PDF kiểm bằng
PyMuPDF — ảnh/PDF `185`-`191` ở `WebApp/anh-chup-kiem-thu/`.

## 5g. "In giới thiệu hôn phối" / "In kết quả rao hôn phối" (lượt "in-excel-a3", 2026-09-08) —
Rao hôn phối, mẫu cuối cùng còn báo "chưa hỗ trợ" trên toàn ứng dụng

Dựng lại từ `Source/ExcelReport/ReportRaoHP.cs` (`Export`/`Export(printRS: true)`) —
`BIN/Template/Chung/RaoHonPhoi.doc` ("GIẤY XIN ĐIỀU TRA VÀ RAO HÔN PHỐI") và
`KQRaoHonPhoi.doc` ("GIẤY KẾT QUẢ RAO HÔN PHỐI"). Đọc trực tiếp text Unicode trong hai tệp `.doc`
gốc (không qua `antiword` — bảng mã 8-bit của các file này không khớp mã trang chuẩn nào, phải tự
giải mã CLX/PlcPcd của định dạng Word 97 bằng `olefile` để lấy đúng text UTF-16LE embedded) để có
đúng nguyên văn tiếng Việt.

**Phát hiện quan trọng — chú thích cũ ở mục 5e sai một phần**: xem can-review-sau.md mục 69 mục
1 — nút "In giới thiệu hôn phối" (`GxGiaoDanList.tsx`) thật ra desktop wire vào một mẫu KHÁC
(`frmReportGioiThieuHP`, mẫu giáo lý hôn phối cũ ở mục 65), còn đường dẫn tới `ReportRaoHP.cs`
(`item7`) bị COMMENT LẠI trên UI thật, không bao giờ chạy được. Nhiệm vụ này chỉ đạo rõ coi nút
web tương ứng `ReportRaoHP.cs` — đã làm THEO chỉ đạo đó, ghi lại phát hiện để người dùng quyết
định sau.

**Endpoint** (theo GIÁO DÂN — bấm từ một dòng trên lưới giáo dân, không phải từ một đôi rao cụ
thể):
```
GET /api/giao-dan/{id}/in/gioi-thieu-hon-phoi
```
`InAnService.XuatGioiThieuHonPhoi` tìm đôi rao (`RaoHonPhoi`) MỚI NHẤT (theo `CreatedAt`) mà
giáo dân này là `GiaoDan1` HOẶC `GiaoDan2` — luôn in đủ cả hai người của đôi rao đó (không chỉ
người vừa bấm), đúng cách bản gốc luôn dựng từ cả `row1`/`row2`. 404 nếu giáo dân chưa có đôi
rao nào (chưa tạo ở "Danh sách rao hôn phối") — không có gì để in.

Các trường tính toán chép nguyên công thức gốc (`Source/GXControl/frmRaoHonPhoi.cs:270-277`):
`AnhChi1`/`AnhChi2` = `Phai == "Nam" ? "Anh" : "Chị"`; `Tuoi1`/`Tuoi2` = năm hiện tại − năm sinh
(`Memory.GetTuoi`, để TRỐNG khi cùng năm hiện tại hoặc thiếu ngày sinh, KHÔNG phải "0"). Sai
khác migrate Y HỆT có chủ đích: `TenGiaoXuNhan` lấy từ cột `LinhMucNhan`, `TenGiaoPhanNhan` lấy
từ cột `GiaoXuNhan` — tên cột không khớp nhãn in ra (bảng `RaoHonPhoi` không có cột
`GiaoPhanNhan` riêng) — xem can-review-sau.md mục 69 mục 3. Nửa dưới trang là phiếu "KẾT QUẢ RAO
HÔN PHỐI" TRỐNG (dấu chấm để điền tay) y hệt cấu trúc `RaoHonPhoi.doc` gốc — KHÔNG có dữ liệu từ
CSDL, giữ nguyên như văn bản tĩnh.

**Endpoint thứ hai** (theo ĐÔI RAO cụ thể, nối vào `RaoHonPhoiDetail.tsx` — nút "In kết quả rao
hôn phối", chỉ bật khi đã lưu):
```
GET /api/rao-hon-phoi/{id}/in/ket-qua
```
`InAnService.XuatKetQuaRaoHonPhoi` dựng đủ thông tin bí tích (Rửa tội/Thêm sức, dùng lại
`VanBanInAn.MoTaBiTich`)/địa chỉ/điện thoại của cả hai người — cùng tinh thần với "Chứng nhận
hôn phối" (mục 5d). Khối "Đã điều tra và rao ba lần vào các ngày" ghép 1-3 dòng từ
`NgayRaoLan1/2/3` (bỏ dòng nào chưa có ngày, dùng `VanBanInAn.GhepDong`). `TenLinhMucGui` để
TRỐNG (bản gốc tra từ danh mục `LinhMuc`, web chưa có màn hình quản lý danh mục này — xem mục
5e và can-review-sau.md mục 69 mục 5). 404 khi không tìm thấy đôi rao (id sai hoặc thuộc giáo xứ
khác).

## 5h. Xuất Excel ClosedXML cho "Danh sách sổ bí tích"/"Danh sách rao hôn phối" (lượt
"in-excel-a3", 2026-09-08)

Hai màn hình migrate ở commit `aa4a2af`, phần xuất Excel bị hoãn tới lượt này. Dùng lại
`XuatExcelService` + ClosedXML đã có (cùng khuôn với `XuatGiaoDan`/`XuatGiaDinh`): tiêu đề in
đậm, đóng băng hàng đầu, tự giãn cột, `AutoFilter`. CÙNG tham số lọc với GET danh sách hiện có
(`loaiBiTich`/`tuNam`/`denNam` cho sổ bí tích, `xemTatCa` cho rao hôn phối) — gọi thẳng
`DotBiTichService.LayDanhSach`/`RaoHonPhoiService.LayDanhSach`, KHÔNG viết lại điều kiện lọc.

```
GET /api/dot-bi-tich/xuat-excel?loaiBiTich=...&tuNam=...&denNam=...
GET /api/rao-hon-phoi/xuat-excel?xemTatCa=...
```

**"Sổ bí tích"**: 5 cột đúng `cotDotBiTich.ts`/`GxDotBiTichList.FormatGrid` — Ngày, Mô tả (kèm
tên loại bí tích ghép trước, ví dụ "Rửa tội — Đợt Giáng Sinh 2024"), Người ban bí tích, Nơi nhận
bí tích, Số lượng GD. Ở MỨC ĐỢT — KHÔNG xuất chi tiết 6150 bản ghi từng người nhận trong đợt
(`BiTichChiTiet` không có màn hình danh sách/bộ lọc riêng để tái dùng, xem can-review-sau.md
mục 69 mục 6).

**"Rao hôn phối"**: 8 cột đúng `cotRaoHonPhoi.ts`/`GxRaoHonPhoiList.FormatGrid` — Mã rao, Đôi
rao, Người thứ nhất, Người thứ hai, Rao lần 1/2/3, Ghi chú. Mức TÓM TẮT trên lưới danh sách,
KHÔNG PHẢI 26 cột chi tiết của `frmRaoHonPhoi` (mẫu gốc `DanhSachRaoHonPhoi.xls`/
`ReportRaoHP.ExportList` cũng chỉ xuất đúng lưới 8 cột đang hiển thị qua cơ chế GridEX — không
phải một nguồn dữ liệu khác, xem can-review-sau.md mục 69 mục 7).

Nối vào giao diện: nút "Xuất Excel" trên thanh công cụ `DotBiTichList.tsx` (chỉ bật được sau khi
đã "Tìm kiếm" — trước đó không có `loaiBiTich` để lọc) và `RaoHonPhoiList.tsx`, cùng khuôn với
"Xuất Excel" đã có ở `GiaoDanList.tsx`/`GiaDinhList.tsx`.

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

- Backend: `WebApp/tests/Qlgx.Api.Tests/GiaoDanInAnTests.cs` (Lý lịch cá nhân),
  `InAnMauMoiTests.cs` (Chứng nhận bí tích cả 4 biến thể `loai`, Phiếu gia đình, Chứng nhận hôn
  phối, kể cả trường hợp không đủ dữ liệu bí tích vẫn phải in được và không nhầm thành 404), và
  `GioiThieuInAnTests.cs` (4 mẫu Giấy giới thiệu — xuất PDF thành công, 400 khi thiếu `giaoXu2`,
  404 khi không tìm thấy, mẫu Thêm sức vẫn in được khi thiếu dữ liệu Rửa tội) — xuất PDF thành
  công (kiểm chữ ký tệp `%PDF-`), 404 khi không tìm thấy hoặc (riêng Chứng nhận hôn phối) khi
  gia đình chưa có hôn phối nào, và KHÔNG in được bản ghi của giáo xứ khác (cách ly dữ liệu —
  đúng ràng buộc "giao_xu_id luôn từ claim").
- Frontend: `GiaoDanDetail.test.tsx` (nút In lý lịch cá nhân), `GxGiaoDanList.test.tsx` (menu 12
  mục, MỌI mục đều đã in được thật — không còn mục nào báo "chưa hỗ trợ" — 4 mục bí tích gọi
  đúng `api.giaoDan.inChungNhanBiTich` với đúng `loai`, 3 mục Giấy giới thiệu mở `GioiThieuModal`
  với đúng `loai`, "In giới thiệu hôn phối" gọi đúng `api.giaoDan.inGioiThieuHonPhoi`),
  `GxGiaDinhList.test.tsx` (mọi mục in gọi đúng api, kể cả "In phiếu gia đình (khổ A3)" truyền
  thêm `'A3'`), `GiaDinhDetail.test.tsx` (nút "In phiếu gia đình" gọi đúng api kèm khổ giấy đang
  chọn, vô hiệu hoá khi gia đình còn là bản nháp chưa lưu), `RaoHonPhoiDetail.test.tsx` (nút "In
  kết quả rao hôn phối" gọi đúng api, vô hiệu hoá khi chưa lưu), `RaoHonPhoiInAnTests.cs`/
  `SoBiTichRaoHonPhoiExcelTests.cs` (backend, PDF/Excel thành công + 404 + cách ly giáo xứ),
  `DotBiTichList.test.tsx`/`RaoHonPhoiList.test.tsx` (nút "Xuất Excel").
- Chạy thật qua API thật (tài khoản tạm, JWT thật, gọi `curl`/`fetch` vào `Qlgx.Api` chạy trên
  `qlgx_thu`), in cho các bản ghi thật nhiều dữ liệu nhất tìm được, mở PDF/Excel kiểm tra bằng
  PyMuPDF/openpyxl — xem ảnh chụp/PDF/Excel mẫu `WebApp/anh-chup-kiem-thu/` (đánh số theo báo
  cáo nhiệm vụ, 4 mẫu Giấy giới thiệu là `181`-`184`, lượt "in-excel-a3" là `200` trở lên) và
  can-review-sau.md mục 34e/65/69.

## 8. Phạm vi CHƯA làm (khác biệt cố ý với yêu cầu ban đầu)

Yêu cầu ban đầu xếp thứ tự 5 mẫu: hạ tầng, Lý lịch cá nhân, Chứng nhận bí tích, Phiếu gia đình,
Chứng nhận hôn phối — xong đủ 5/5 phần đó qua hai lượt đầu (xem mục 5-5d). "Giấy giới thiệu" (4
mẫu) bị hoãn ở lượt đó vì tưởng cần thêm màn hình nhập liệu người thứ hai phức tạp (dữ liệu của
người thứ hai thường KHÔNG có bản ghi `GiaoDan` nào trong CSDL vì thuộc giáo xứ khác); lượt sau
đã làm xong cả 4 mẫu — xem mục 5e (cơ chế thật đơn giản hơn tưởng: chỉ hai ô nhập tự do
giáo phận/giáo xứ nhận + tên linh mục, không cần tra cứu người thứ hai trong CSDL nào cả).

Lượt "in-excel-a3" (2026-09-08) làm nốt ba việc còn lại của kế hoạch ban đầu: mẫu Rao hôn phối +
Kết quả rao hôn phối (mục 5g), Xuất Excel ClosedXML cho Giáo dân/Gia đình (đã có từ trước)/Sổ bí
tích/Rao hôn phối (mục 5h), và khổ giấy chọn được cho Phiếu gia đình (A3, mục 5c) — không còn
mục menu in ấn nào trên toàn ứng dụng báo "chưa hỗ trợ" (`chuaHoTro()`).

Còn lại, CHƯA làm:

- 5 mô-đun biểu đồ (`Chart*.cs`) — theo đúng chỉ đạo "để sau, không thuộc lượt này".
- "In danh sách theo ngày rao cụ thể" của màn hình Rao hôn phối
  (`gxAddEdit1.PrintButton`/`GxRaoHonPhoiList.Print()` — hộp thoại chọn "ngày rao" rồi xuất Excel
  từ bảng tạm `RaoHonPhoiTMP`, khác hẳn "Xuất Excel" đơn giản đã làm ở mục 5h) — xem
  `rao-hon-phoi.md` mục 10.
- Xuất Excel chi tiết 6150 bản ghi `BiTichChiTiet` (theo từng người nhận trong một đợt) — mục
  5h chỉ xuất được 5 cột MỨC ĐỢT, chưa có màn hình danh sách/bộ lọc riêng cho bảng chi tiết để
  tái dùng logic lọc có sẵn (nguyên tắc xuyên suốt `XuatExcelService`).
- "In danh sách bí tích"/"In chứng nhận" nối vào màn hình "Danh sách sổ bí tích" — hạ tầng
  `/api/giao-dan/{id}/in/chung-nhan-bi-tich` đã có (mục 5b) nhưng chưa có nút gọi từ màn hình
  này, xem `so-bi-tich.md` mục 10.

## 9. Chỗ chưa chắc

- Không xác định được chính xác đoạn mã desktop nào chọn thư mục `BMT` so với `Chung` (file
  chứa hàm `Memory.GetReportTemplatePath` không tìm thấy dạng `.cs` nguồn qua tìm kiếm trực
  tiếp trong `Source/` ở lượt khảo sát này, chỉ thấy trong các `.dll`/`.pdb` đã biên dịch) — cơ
  chế chọn theo `TenGiaoPhan` chuẩn hoá ở mục 6 là suy luận hợp lý từ tên thư mục quan sát
  được, KHÔNG phải đối chiếu trực tiếp từ mã nguồn. Cần xác nhận lại nếu sau này giáo xứ Ban Mê
  Thuột thật sự dùng bản web và cần mẫu riêng.
- Chưa rõ bản desktop có cho phép nhiều mẫu riêng khác ngoài `Chung`/`BMT` hay không (ví dụ mỗi
  giáo phận một thư mục) — dữ liệu khảo sát chỉ thấy 2 thư mục.
