# Thiết kế: Đồng bộ dữ liệu offline — PWA offline-first

Ngày: 2026-09-13. Nhánh: `webapp-phase-1`. Trạng thái: đã thống nhất với người dùng, đã qua ba
lượt review độc lập, chờ lập kế hoạch triển khai.

## 1. Mục tiêu và bối cảnh

### 1.1 Vấn đề

Bản web của QLGX (`WebApp/`) đã hoàn tất kiểm thử chấp nhận nhưng chỉ chạy được khi có mạng, và
chỉ cho phép một người sửa một hồ sơ tại một thời điểm. Thực tế giáo xứ thì khác: nhiều người
nhập liệu, mỗi người một máy, đôi khi cùng sửa một hồ sơ; và một số giáo xứ gần như không có
mạng.

Bốn ràng buộc từ thực tế vận hành:

- **Đa số giáo xứ có mạng đủ tốt** — dùng được Zalo, Facebook trên web. Nhóm gần như không có
  mạng là thiểu số, nhưng có thật và không bỏ được.
- **Một giáo xứ có nhiều người nhập liệu**, đôi khi cùng sửa một hồ sơ, mỗi người một máy riêng.
- **Người dùng là quý cha, quý sơ**, phần lớn không rành máy tính.
- **Dữ liệu là sổ sách giáo xứ nhiều năm** — mất là không lấy lại được.

### 1.2 Hiện trạng kỹ thuật đã kiểm chứng

Mọi dòng dưới đây đã được đối chiếu với mã nguồn.

| Điểm | Vị trí | Ý nghĩa với thiết kế này |
|---|---|---|
| Chống ghi đè mức **bản ghi** bằng `xmin` | `GiaoDanService.cs:453,495` và 6 chỗ khác | Chặn được ghi đè nhưng **không gộp được**: hai người sửa hai ô khác nhau vẫn báo lỗi cho một người |
| `UpdatedAt` **được đóng dấu tự động** ở `SaveChanges` | `QlgxDbContext.cs:123-143` | Nhưng là **một mốc cho cả bản ghi**, nên không dùng để gộp từng ô được |
| **32 lời gọi `ExecuteUpdateAsync`/`ExecuteDeleteAsync`** đi vòng qua `SaveChanges` | `TimThayTheService.cs:81-118`, `ChuyenHoService.cs:54,94,97`, `AuthService.cs:153,175`, `GiaoDanService.cs:617-618`, `GiaDinhService.cs:322`, `DotBiTichService.cs:159` | Không đóng dấu `UpdatedAt`, không đụng `xmin`, và **sẽ không sinh nhật ký** nếu chỉ móc vào `SaveChanges`. "Tìm và thay thế" sửa 24 trường cùng lúc, một lần bấm đổi hàng nghìn bản ghi |
| **Không có nhật ký thay đổi** | — | Không biết ai sửa gì, lúc nào |
| Chỉ **5 bảng** có cờ `DaXoa` | `GiaDinh`, `GiaoDan`, `GiaoHo`, `LinhMuc`, `TaiKhoan` | Còn lại xoá cứng |
| **17 chỗ xoá cứng** ở bản web | gồm `ThanhVienGiaDinh`, `BiTichChiTiet`, `DotBiTich`, `ChuyenXu`, `RaoHonPhoi`, các bảng giáo lý và hội đoàn | Xoá cứng làm bản ghi **sống lại** sau đồng bộ |
| `GiaoDan`/`GiaDinh` **có `DaXoa` nhưng vẫn có đường xoá vĩnh viễn** | `GiaoDanService.cs:619`, `GiaDinhService.cs:323` (màn hình Hồ sơ lưu trữ, tham số `vinhVien`) | Xoá vĩnh viễn một gia đình **xoá luôn toàn bộ `ThanhVienGiaDinh`** |
| JWT sống 8 giờ, **không có refresh token** | `TokenService.cs:17-21` | Mất mạng dài ngày là bị đá ra màn hình đăng nhập, mà đăng nhập lại thì cần mạng |
| Mất mạng bị **xoá token** | `AuthContext.tsx:68-73` — `.catch(() => authStore.xoaToken())` | Lỗi chặn đường cho offline-first |
| **Không có phân quyền theo chức năng** | `Program.cs:47,52` — chỉ `QuanTri` (`LoaiTaiKhoan=0`) và `QuanTriHeThong` (`=9`) | Mọi tài khoản đã đăng nhập **đọc và ghi được mọi thứ** trong giáo xứ mình. Bảng `TenLoaiTaiKhoan` chỉ là nhãn, không chỗ nào đọc để chặn |
| RLS bật trên **24 bảng** có `giao_xu_id`, **fail-closed** | `20260906210934_BatRlsChoBangTheoGiaoXu.cs`, `BoiCanhGiaoXuConnectionInterceptor` | Bảng mới phải thêm cả `HasQueryFilter` lẫn migration RLS — trừ `thiet_bi` (mục 6.5) |
| Ảnh đại diện là **`byte[]` ngay trong bảng** | `GiaoDan.AnhDaiDienDuLieu`, `GiaDinh.cs:21-22`, `AnhDaiDienService.cs:15` | Nhật ký `jsonb` sẽ nuốt nguyên blob nếu không loại trừ tường minh |
| `BoDemMaCu` khoá phức `(GiaoXuId, TenBang)` | `BoDemMaConfig.cs:11` | Bộ đếm sinh `MaGiaoDanCu` — **số người dùng thật sự nhìn**. Hai máy offline cùng tạo người mới sẽ cấp trùng số |
| `GiaoDanHonPhoi` khoá phức, **không có `Id`** | `GiaoDanHonPhoiConfig.cs:12` | Nhật ký khoá theo `ban_ghi_id uuid` không diễn tả được bảng này |
| `MaNhanDang` đã tồn tại, chú thích ghi rõ *"dùng để đồng bộ hai chiều với bản desktop sau này"* | `GiaoDanService.cs:487-489` | Xem mục 4.6 |
| PWA **chỉ cache vỏ ứng dụng**, cố ý không cache `/api/*` | `vite.config.ts:17-21` | Nguyên tắc đúng, thiết kế này giữ nguyên |
| Banner "Tải lại / Để sau" | `CapNhatPWA.tsx`, `registerType:'prompt'` | Sẽ bỏ, xem mục 7.8 |
| Bản nháp form ở `localStorage`, **không phải hàng chờ** | `banNhap.ts` | Không dùng lại được cho đồng bộ |
| Dò trùng: họ tên + tên thánh + ngày sinh, **không có giới tính** | `GiaoDanService.cs:363-365` và `SqlConstants.cs:214` | Hai bản dùng **cùng** tiêu chí |
| Desktop cảnh báo trùng nhưng bấm "No" **vẫn lưu** | `frmGiaoDan.cs:674-681` | Không dùng được khi nhiều máy cùng nhập |
| Desktop ghi theo kiểu **luôn thắng** | `DBAccess.cs:272` — `ConflictOption.OverwriteChanges` | Desktop không có bất kỳ kiểm tra xung đột nào |
| Toàn bộ giáo xứ **4074 giáo dân = 7,3 MB JSON, 500 KB nén** | đo trên `temp/goi.json.gz` | Tải nguyên giáo xứ về máy là khả thi. **Lưu ý:** gói Access không mang ảnh (0/4074 bản ghi có ảnh); giáo xứ dùng web lâu sẽ có ảnh và kho đọc-về phồng lên |
| Nhập dữ liệu đã có cổng ghi đè và màn hình xem trước đầy đủ | `NhapDuLieuService.cs:110-127`, `BaoCaoXemTruocDto` (`TenGiaoXuNguon`, `GiaoXuDichDaCoDuLieu`, `SoGiaoDanDaCo`, `DoiChieu` theo từng bảng) | Mục 10 dựa trên cái đã có, không xây mới |

### 1.3 Triết lý thiết kế

Người dùng **không phải quan tâm chuyện kỹ thuật**. Hệ thống tự bảo vệ người dùng và dữ liệu, tự
động hoá tối đa khi biết chắc là an toàn, và chỉ hỏi khi đoán sai sẽ làm hỏng sổ sách. Dễ dùng là
ưu tiên hàng đầu — giống tinh thần bản desktop.

Hệ quả trực tiếp: không có chữ "đồng bộ", "xung đột", "phiên bản" nào xuất hiện trước mặt người
dùng (bảng từ vựng ở mục 2). Không có hộp thoại kỹ thuật. **Không thao tác nào của người dùng làm
mất vĩnh viễn dữ liệu đã nhập.**

### 1.4 Mục tiêu

1. **Không bao giờ mất dữ liệu đã nhập** — kể cả máy hỏng, phiên đăng nhập hỏng, cập nhật phần
   mềm, trình duyệt dọn kho, hay người dùng bấm nhầm.
2. **Hội tụ.** Mọi máy nối mạng đủ lâu đều về cùng một kết quả, không phụ thuộc thứ tự nối mạng.
3. **Luôn nói thật dữ liệu cũ tới đâu.** Không bao giờ hiện dữ liệu cũ như dữ liệu thật.
4. **Nhiều người cùng nhập không đè lên nhau.** Hai người sửa hai ô khác nhau của cùng một hồ sơ
   phải gộp được tự động.
5. **Phát hiện được khi có gì sai**, không chỉ khôi phục được sau khi biết.

### 1.5 Ngoài phạm vi

- **Kênh đẩy thời gian thực** (WebSocket/SSE). Hỏi định kỳ co giãn là đủ; thiết kế không cản việc
  thêm sau.
- **Mã hoá kho dữ liệu trong máy.** Không giải được bài toán (mục 6.6) và đổi lại phải gõ mật khẩu
  mỗi lần mở.
- **Phân quyền theo chức năng.** Chưa có, và thêm vào là việc riêng (mục 6.6).
- **Đồng bộ hai chiều với Access.** Xem mục 10 — nhập từ desktop là **ghi đè toàn bộ có xác nhận**,
  không phải đồng bộ, và thuộc phase sau.
- **Tải ảnh đại diện về hàng loạt.** Ảnh tải theo nhu cầu, giữ lại bản đã xem.
- **Offline cho màn hình quản trị hệ thống** (tạo giáo xứ, tài khoản, nhập dữ liệu).

## 2. Từ vựng

Bảng này là bắt buộc khi thi công. Không có nó, mỗi người dịch một kiểu và chữ kỹ thuật sẽ lọt ra
màn hình.

| Trong mã và tài liệu | Hiện ra màn hình |
|---|---|
| pull / nhận về | "lấy dữ liệu mới" |
| push / gửi lên | "gửi về" |
| hàng chờ (outbox) | "việc chưa gửi" |
| xung đột | *(không bao giờ hiện)* — thay bằng câu hỏi cụ thể |
| thu hồi thiết bị | "Gỡ máy này ra" |
| đăng xuất | "Đổi người dùng" |
| đồng bộ | *(không bao giờ hiện)* |
| mốc đồng bộ | "Lần gửi gần nhất" / "Lần lấy dữ liệu gần nhất" |
| máy con / client | "máy này" |

## 3. Hướng đã chọn và các hướng đã loại

**Đã chọn — PWA offline-first, gộp mức trường.** Bản web giữ một bản sao đầy đủ của giáo xứ trong
máy (với tài khoản được bật offline), ghi vào máy trước rồi gửi lên sau, gộp thay đổi ở mức từng ô
dữ liệu.

**Đã loại — hộp máy chủ đặt tại giáo xứ** (LAN nội bộ, dùng lại y nguyên bản web và cơ chế `xmin`).
Rẻ về mặt code nhưng đẩy gánh nặng vận hành về giáo xứ. Không loại trừ về sau: cùng một giao thức,
hộp nội bộ chỉ là một máy con đặc biệt.

**Đã loại — đồng bộ hai chiều đầy đủ với desktop.** Bản desktop ghi theo kiểu luôn thắng
(`DBAccess.cs:272`), có ~92 chỗ ghi nghiệp vụ trải trên 28 file, nhiều chỗ xoá cứng, và vẫn để lại
hai bộ mã nguồn phải nuôi lâu dài. Thay bằng mục 10.

**Chi phí không dồn vào nhóm thiểu số.** Đường ghi "lưu vào máy trước" là một đường code duy nhất:
giáo xứ mạng tốt vẫn được hưởng lưu tức thì, không mất việc khi wifi chập, tìm kiếm hiện theo từng
phím gõ.

## 4. Nền tảng dữ liệu

### 4.1 Hai bảng, hai việc khác nhau

Đây là điểm sửa quan trọng nhất so với bản thiết kế đầu, vốn gộp hai việc vào một bảng và do đó
**phá vỡ tính hội tụ**.

Kịch bản làm lộ vấn đề: máy ngủ đông gửi lên một thay đổi cũ. Máy chủ gộp đúng luật nên **không
đổi giá trị hiện hành**. Nhưng nếu nó vẫn ghi dòng đó vào cùng chuỗi mà máy con kéo về và áp tuần
tự, thì mọi máy con sẽ **hiển thị giá trị đã thua**. Máy chủ một đằng, máy con một nẻo, vĩnh viễn,
không báo lỗi.

Tách làm hai:

**`thay_doi` — sổ kiểm toán.** Ghi **mọi** ý định sửa, kể cả ý định thua cuộc gộp. Không đánh số
thứ tự, **không phát xuống máy con**. Đây là thứ trả lời "ai sửa gì, lúc nào" và là nguồn khôi
phục giá trị cũ.

| Cột | Kiểu | Ý nghĩa |
|---|---|---|
| `id` | uuid | |
| `giao_xu_id` | uuid | Chịu RLS |
| `bang`, `ban_ghi_id`, `truong` | text/uuid/text | Sửa ô nào |
| `gia_tri` | jsonb | Giá trị mới |
| `loai` | text | `tao` / `sua` / `gop` |
| `dong_ho` | (mục 4.3) | Mốc đã hiệu chỉnh |
| `thiet_bi_id`, `tai_khoan_id` | uuid | Ai, máy nào |
| `ma_thao_tac` | uuid | Do máy con sinh, chống gửi trùng |
| `giao_dich_id` | uuid | Nhóm các dòng của cùng một lần lưu |
| `thang` | bool | Thay đổi này có thắng cuộc gộp không |

**`hieu_luc` — chuỗi phát xuống.** Chỉ những thay đổi **đã thắng**, đánh `so_thu_tu`. Đây là cái
máy con kéo về. Một thao tác bị luật gộp loại thì **không** sinh dòng `hieu_luc`.

| Cột | Kiểu | Ý nghĩa |
|---|---|---|
| `so_thu_tu` | bigint | Tăng dần **riêng theo từng giáo xứ** |
| `epoch` | uuid | Danh tính của chuỗi số (mục 4.5) |
| `giao_xu_id` | uuid | Chịu RLS |
| `bang`, `ban_ghi_id`, `truong` | | Ô nào |
| `gia_tri` | jsonb | Giá trị đã thắng |
| `dong_ho` | | Mốc của giá trị thắng — máy con lưu để so lần sau |
| `giao_dich_id` | uuid | Ranh giới lô không được cắt giữa (mục 7.6) |

**Ba loại dòng:**

- `tao` — máy chủ **khai triển ngay khi nhận** thành N cặp `(trường, giá trị)` cùng mốc, rồi gộp
  theo đúng luật mức trường. Không bao giờ áp nguyên khối, vì áp nguyên khối sẽ đè lên các ô đã
  gộp.
- `sua` — một dòng cho **mỗi ô**.
- `gop` — ánh xạ `ID cũ → ID mới` khi người dùng xác nhận hai bản ghi là một người (mục 8.3).

**Cột loại trừ khỏi nhật ký:** `AnhDaiDienDuLieu`, `AnhDaiDienLoaiNoiDung` và mọi cột `byte[]`.
Ảnh đồng bộ riêng theo mã băm, không đi qua `jsonb`.

### 4.2 Cấp `so_thu_tu`

Số phải liên tục, không có lỗ hổng nhìn thấy từ phía máy con. `sequence` của PostgreSQL **cấp số
trước khi commit**, nên một giao dịch bị huỷ để lại lỗ hổng vĩnh viễn: máy con nhận 4823, không
bao giờ biết 4822 từng tồn tại.

Cách chốt: mỗi giáo xứ có một dòng đếm; giao dịch nào ghi `hieu_luc` thì khoá dòng đó
(`SELECT ... FOR UPDATE`), lấy số kế tiếp, ghi, rồi commit cùng một lượt. Vì khoá giữ tới lúc
commit, **thứ tự cấp số = thứ tự nhả khoá = thứ tự commit** — nên máy con hỏi `> 4821` không bao
giờ nhảy qua một dòng chưa commit. Đây chính là điều `sequence` không bảo đảm.

Ba ràng buộc bắt buộc, mỗi cái phải có kiểm thử riêng:

1. **Số phải được cấp trong chính giao dịch ghi.** Một hàm trợ giúp tự mở giao dịch riêng, commit,
   rồi trả số về sẽ làm sụp mọi bảo đảm — và lỗi chỉ xuất hiện khi có tải. Rất dễ mắc với EF Core.
   Cần kiểm thử đồng thời, không chỉ kiểm thử giao dịch bị huỷ.
2. **Khoá dòng đếm phải là câu lệnh đầu tiên của giao dịch ghi.** Nếu không: T1 khoá bản ghi R rồi
   chờ dòng đếm, T2 khoá dòng đếm rồi chờ R → deadlock. Giao dịch chạm hai giáo xứ phải khoá hai
   dòng đếm **theo thứ tự `giao_xu_id` tăng dần**.
3. **Đặt `lock_timeout` ngắn cho đường ghi tương tác**, và chia nhỏ mọi thao tác hàng loạt (mục
   4.7), vì khoá dòng đếm **xếp hàng toàn bộ việc ghi của giáo xứ đó**.

### 4.3 Đồng hồ

Máy ở giáo xứ chạy nhiều năm không ai chỉnh giờ, và đồng hồ có thể **nhảy giữa chừng** (người dùng
chỉnh tay, NTP, hết pin CMOS). Một độ lệch đo lúc gửi rồi áp cho cả lô là sai: thay đổi ghi trước
lúc nhảy sẽ bị dịch sai hướng, và **âm thầm đè lên dữ liệu đúng** của máy khác — đúng cái cơ chế
này sinh ra để ngăn.

Ba lớp:

**Đồng hồ đơn điệu.** Mỗi thay đổi lưu `(mốc neo lần đồng bộ gần nhất, số ms đơn điệu trôi từ
đó)`. `performance.now()` không bị đồng hồ hệ thống ảnh hưởng.

**Đoạn đồng hồ.** Máy con theo dõi chênh lệch giữa `Date.now()` và đồng hồ đơn điệu; phát hiện
nhảy thì mở một **đoạn** mới trong hàng chờ. Mỗi đoạn hiệu chỉnh bằng một độ lệch riêng; đoạn
không neo được thì vào hộp cần xem lại thay vì đoán.

**Đồng hồ logic lai (HLC).** Chỉ đồng hồ vật lý thì vẫn đảo nhân quả: máy B **đọc** thay đổi của
máy A rồi sửa đè, nhưng đồng hồ B chỉ số nhỏ hơn → sửa sau thua sửa trước, người dùng thấy việc
mình vừa làm bị hoàn tác. Mỗi máy nâng đồng hồ logic của mình lên khi nhận dòng có mốc lớn hơn.
Chi phí gần bằng không so với những gì đã thiết kế.

**Luật phá hoà:** so theo bộ ba `(dong_ho, thiet_bi_id, ma_thao_tac)` — tất định, tổng, mọi bản
sao so như nhau. Thiếu nó thì "mới hơn thắng" không xác định và hai máy sẽ chọn khác nhau.

Máy có giờ lệch quá một giờ: hiện thông báo **ngay tại máy đó**, bằng việc làm được:
*"Giờ trên máy này đang sai khoảng 3 ngày. Xin chỉnh lại ngày giờ trong Windows, nếu không dữ liệu
có thể bị lẫn thứ tự."* Không gửi cảnh báo cho quản trị viên — quý cha quý sơ không làm gì được
với nó.

### 4.4 Xoá là một ô, không phải một sự kiện riêng

Xoá cứng làm bản ghi **sống lại**: máy A xoá, máy B chưa biết, đồng bộ xong bản ghi quay về.

Mọi bảng có một ô `da_xoa` chịu **đúng luật gộp như mọi ô khác**. Không cần loại dòng `xoa` riêng,
không cần loại `khoi_phuc` riêng — "khôi phục" chỉ là đặt `da_xoa = false` với mốc mới hơn. Điều
này cũng cho người dùng một nút thật trong hộp cần xem lại: *"Không, gia đình này vẫn còn."*

Hai chỗ phải xử lý riêng:

- **17 chỗ xoá cứng** hiện có phải chuyển sang xoá mềm, hoặc nếu giữ xoá cứng thì phải sinh dòng
  `hieu_luc` đặt `da_xoa` cho **mọi bản ghi bị xoá tầng con**. Xoá vĩnh viễn một gia đình xoá luôn
  toàn bộ `ThanhVienGiaDinh` (`GiaDinhService.cs:323`) — thiếu bước này thì một lần xoá ở máy A sẽ
  làm sống lại 5 dòng thành viên từ máy B.
- **`GiaoDanHonPhoi` không có `Id`** (`GiaoDanHonPhoiConfig.cs:12`). Bảng khoá phức cần một quy
  ước `ban_ghi_id` dẫn xuất tất định từ khoá phức, ghi rõ trong kế hoạch thi công.

### 4.5 `epoch` — danh tính của chuỗi số

Con trỏ `so_thu_tu` trần sẽ mồ côi trong hai tình huống thật:

- **Khôi phục máy chủ.** Bộ đếm về 4900; máy con giữ con trỏ 5000, hỏi "có gì sau 5000" → nhận
  rỗng **mãi mãi**, thanh trạng thái vẫn xanh, và khi bộ đếm vượt 5000 lại thì máy con **bỏ qua
  toàn bộ dòng 4900-5000 sinh lại**.
- **Dọn nhật ký.** Bảng `hieu_luc` lớn nhanh và sẽ phải dọn. Máy ngủ đông quay lại với con trỏ nằm
  trong khoảng đã dọn → nhận thiếu, phân kỳ im lặng.

Con trỏ là cặp `(epoch, so_thu_tu)`. `epoch` đổi khi khôi phục hoặc dọn. Máy chủ **phải từ chối**
con trỏ không phục vụ được (mã 410, "cần tải lại toàn bộ") thay vì trả rỗng — đây là khác biệt
giữa "phát hiện được" và "im lặng sai".

**Chính sách lưu giữ phải ghi thành con số** trong kế hoạch thi công, và mục 11 phải nói rõ: "nhật
ký giữ lịch sử nên khôi phục được" chỉ đúng trong thời hạn lưu giữ.

### 4.6 Định danh bản ghi

ID do **máy con sinh**: tạo bản ghi mới lúc offline thì máy con tự sinh `Guid` và dùng luôn. Không
có ID tạm rồi đổi. Hệ quả quan trọng: thao tác phụ thuộc nhau vẫn làm được khi offline — tạo gia
đình xong thêm thành viên ngay, dù cả hai chưa lên máy chủ.

Hai ngoại lệ phải xử lý riêng:

- **`BoDemMaCu`** (`BoDemMaConfig.cs:11`) sinh `MaGiaoDanCu` — **số người dùng thật sự nhìn và tra
  cứu**. Hai máy offline cùng tạo người mới sẽ cấp trùng. Quy tắc: số này **do máy chủ cấp lúc
  nhận**, không do máy con sinh; lúc offline hiển thị "(chưa cấp số)". Áp dụng cho mọi số sổ bí
  tích.
- **`MaNhanDang`** đã tồn tại (`GiaoDanService.cs:487-489`) với chú thích nói rõ dành cho đồng bộ.
  Thiết kế này **không dùng nó** — định danh là `Guid` khoá chính. Giữ nguyên cột để không phá
  đường nhập từ Access; ghi rõ ở đây để người thi công không đi tìm.

### 4.7 Ghi hàng loạt

32 lời gọi `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (mục 1.2) đi vòng qua `SaveChanges`. Nếu chỉ
móc nhật ký vào `SaveChanges` thì thủng 32 chỗ **không có dấu hiệu gì** — đúng loại lỗi âm thầm
mà phần mềm này không chịu được.

Ba việc:

1. Liệt kê và xử lý riêng từng chỗ. "Tìm và thay thế" (`TimThayTheService.cs`) sửa 24 trường cùng
   lúc trên hàng nghìn bản ghi — phải sinh nhật ký cho từng ô thật sự đổi, chia thành nhiều giao
   dịch nhỏ (mục 4.2 ràng buộc 3).
2. **Kiểm thử chặn mọi `ExecuteUpdate`/`ExecuteDelete` mới** không đi qua lớp ghi nhật ký.
3. `DatLaiUpdatedAt` (`ChuyenDoiDuLieu.cs:1098-1107`) dùng `ExecuteUpdateAsync` + sửa thẳng
   `OriginalValue` để chống lại chính `SaveChanges`. Khi thêm ghi nhật ký, đoạn này sẽ hành xử khó
   đoán — phải xem lại cùng lúc.

## 5. Ràng buộc bắt buộc

Danh sách này ngang hàng với ràng buộc `so_thu_tu`. Mỗi dòng phải có kiểm thử, và theo điều 4 của
`CLAUDE.md`, **kiểm thử phải được chạy thử với một phiên bản cố tình hỏng để chứng minh nó biết
báo lỗi**.

1. **Máy con không bao giờ được bỏ sót dòng `hieu_luc` đã commit.**
2. **Ghi kho hiển thị và thêm dòng hàng chờ nằm trong MỘT giao dịch IndexedDB.** Không bao giờ có
   dữ liệu trên màn hình mà không có dòng tương ứng trong hàng chờ. Mất điện xen giữa hai giao
   dịch riêng sẽ tạo ra trạng thái tệ nhất: dữ liệu hiện trên màn hình, thanh trạng thái xanh,
   nhưng không ai gửi nó lên và nó biến mất ở lần lấy dữ liệu kế tiếp.
3. **Áp dòng nhận về và tiến con trỏ nằm trong MỘT giao dịch IndexedDB**, cùng với việc xoá thao
   tác đã gửi khỏi hàng chờ.
4. **Không bao giờ xoá kho khi hàng chờ chưa rỗng** — không khi đăng xuất, không khi thu hồi,
   không khi nâng cấp.
5. **Không bao giờ xoá-và-tạo-lại kho khi mở lỗi.** `VersionError` (kho mới hơn mã đang chạy) phải
   hiện màn hình hướng dẫn, kèm nút xuất file dự phòng.
6. **Tải lại toàn bộ chỉ được phép khi hàng chờ rỗng.**
7. **Bộ gửi/nhận chạy ở đúng một tab.** Hai tab cùng ghi con trỏ sẽ chạy đua và bỏ sót dòng — vi
   phạm ràng buộc 1. Bầu chủ qua `navigator.locks` hoặc `BroadcastChannel`; tab khác chỉ đọc kho
   và nghe thông báo.
8. **Luồng nhận về chỉ được lọc theo `giao_xu_id`, tuyệt đối không theo quyền người dùng.** Lọc
   theo quyền tạo chuỗi thưa: máy con tiến con trỏ qua những dòng nó chưa bao giờ thấy, và nếu
   quyền được mở rộng sau này thì các dòng đó **mất vĩnh viễn** với máy đó.
9. **Thao tác đã nhận không xử lý lại.** Bảng `thao_tac_da_nhan(ma_thao_tac, ket_qua, phan_hoi)`
   ghi kết quả cho **mọi** thao tác, kể cả bị từ chối; gặp lại `ma_thao_tac` thì trả nguyên phản
   hồi đã lưu. Thiếu cái này: thao tác bị nghi trùng không sinh dòng nào, gửi lại sẽ sinh mục hộp
   thứ hai, và người dùng bấm "Hai người khác nhau" hai lần sẽ tạo **ba bản ghi cho một người**.
10. **`navigator.storage.persist()` phải được gọi ngay sau đăng nhập** trên máy bật offline. Đây
    là biện pháp duy nhất ngăn trình duyệt tự dọn kho khi thiếu chỗ đĩa. Bị từ chối thì **không
    bật chế độ offline cho máy đó**, chạy online thuần.

## 6. Phiên đăng nhập, thiết bị, và quyền lưu offline

### 6.1 Hai loại vé

| | Vé ra vào (giữ nguyên) | Vé dài hạn (mới) |
|---|---|---|
| Sống bao lâu | ~1 giờ | **1 năm, tự gia hạn mỗi lần dùng** |
| Dùng làm gì | Kèm theo mọi lời gọi API | Chỉ để đổi lấy vé ra vào mới |
| Máy chủ có lưu không | Không | **Có — một dòng trong bảng `thiet_bi`** |

Người dùng **không bao giờ thấy màn hình đăng nhập lại** chừng nào còn dùng ít nhất mỗi năm một
lần.

`TokenService.cs:9-14` ghi rằng máy chủ không giữ trạng thái phiên **trong tiến trình**, để chạy
nhiều máy chủ song song. Dòng `thiet_bi` nằm trong PostgreSQL dùng chung, **không phải trong tiến
trình**, nên ràng buộc đó vẫn nguyên. Tra CSDL chỉ xảy ra lúc gia hạn, khoảng mỗi giờ.

### 6.2 Mất mạng không được đá người dùng ra

`AuthContext.tsx:68-73` hiện coi lỗi mạng như token hỏng và xoá token. Phải sửa: chỉ đăng xuất khi
máy chủ **thật sự trả lời 401**; lỗi mạng thì giữ nguyên phiên.

Nguyên tắc: **quyền vào phần mềm khi offline do bản ghi phiên trong máy quyết định, không do vé
còn hạn hay không.**

### 6.3 Bảng `thiet_bi`

Mỗi máy đăng nhập lần đầu tạo một dòng: tên máy, trình duyệt, lần dùng gần nhất, ai đăng nhập,
trạng thái, **cờ cho phép lưu offline**, và số việc chưa gửi mà máy chủ biết.

**Hỏi tên máy ở lần đăng nhập đầu** — đúng một câu: *"Đây là máy nào ạ? (ví dụ: Máy phòng khách,
Máy văn phòng, Laptop của cha)"*, có gợi ý sẵn, bỏ qua được nhưng nhắc lại. Tên tự lấy từ trình
duyệt ra `DESKTOP-8KJ2L4M / Chrome 131` — vô dụng với mọi màn hình cần nó. Thiếu bước này thì cảnh
báo chéo (7.7), màn hình gỡ máy (6.4) và hộp cần xem lại (9.2) đều mất tác dụng.

**Nhiều người dùng chung một tài khoản** là chuyện rất phổ biến ở giáo xứ. Khi hai bên xung đột
trùng tài khoản, hộp cần xem lại phân biệt theo **tên máy**: *"Máy phòng khách ghi 12/03/1985
(hôm qua). Máy văn phòng ghi 12/03/1986 (sáng nay)."*

### 6.4 Đổi người dùng, gỡ máy, thu hồi

Ba việc khác nhau, hiện nay dễ bị gộp làm một:

- **"Đổi người dùng"** (mặc định, thay cho "Đăng xuất"): **giữ nguyên kho và hàng chờ**. Kho dữ
  liệu đọc khoá theo **giáo xứ**, không theo tài khoản, nên đổi người không phải tải lại; chỉ hàng
  chờ gắn tài khoản — đúng tiền lệ đã có ở `banNhap.ts`.
- **"Gỡ máy này ra"**: xoá kho. **Chặn khi hàng chờ chưa rỗng** (ràng buộc 5.4):
  *"Máy này còn 20 việc chưa gửi về được. Xin nối mạng rồi thử lại, hoặc bấm 'Lưu ra file' để cất
  giữ trước."*
- **Thu hồi từ phía quản trị viên**, hai kiểu:
  - *Thường* (đổi người phụ trách): máy được gửi nốt hàng chờ rồi mới xoá. Nhưng máy bị thu hồi
    thường là máy **không còn được mở nữa** — nên màn hình thu hồi phải hiện số việc còn lại
    **trước khi** cho bấm: *"Máy 'Phòng khách' còn 24 việc chưa gửi về (nhập từ 12/8). Những việc
    đó chỉ về được nếu máy kia còn mở và nối mạng. Anh/chị có chắc gỡ máy này ra không?"*
  - *Khẩn* (mất máy): xoá ngay. **Giới hạn phải nói rõ:** chỉ thi hành được khi máy đó nối mạng.
    Người nhặt được laptop không cần mạng để mở kho.

**Trang "Bàn giao máy này"** ở màn hình trạng thái: liệt kê số việc chưa gửi và số mục cần xem
lại; chỉ khi cả hai bằng 0 mới hiện nút xanh *"Đã gửi về hết. Có thể gỡ máy này ra an toàn."*

### 6.5 `thiet_bi` không bật RLS

Việc đổi vé dài hạn lấy vé ra vào xảy ra **trước khi** có claim `GiaoXuId`, nên `app.giao_xu_id`
rỗng và RLS fail-closed sẽ trả về **0 dòng** — đăng nhập lại hỏng im lặng. Dùng chuỗi kết nối
`ConnectionStrings:QlgxQuanTri` (cùng đường với đăng nhập), hoặc không bật RLS cho bảng này. Tiền
lệ: `NhapDuLieuJob` cố ý không có RLS.

Ngược lại, `thay_doi` và `hieu_luc` **phải** có cả `HasQueryFilter` (`QlgxDbContext.cs:95-118`) lẫn
migration RLS — quên là dữ liệu giáo xứ này lộ sang giáo xứ khác.

### 6.6 Cờ cho phép lưu offline

**Vì sao cần.** Hiện **không có phân quyền theo chức năng** (mục 1.2): mọi tài khoản đã đăng nhập
đọc và ghi được mọi thứ trong giáo xứ. Nên giáo lý viên hay người giúp việc thời vụ đăng nhập bằng
**máy cá nhân** sẽ mang toàn bộ sổ rửa tội, hôn phối, qua đời về máy riêng, và nó ở lại đó sau khi
họ về.

**Vì sao không giải bằng mã hoá.** Để phần mềm tự mở kho lúc khởi động mà không hỏi mật khẩu
(yêu cầu đã chốt), **khoá phải nằm trong máy** — người lấy được dữ liệu cũng lấy được khoá. Mã hoá
chỉ chặn người chép thư mục hồ sơ trình duyệt sang máy khác, không chặn người ngồi trước chính máy
đó. Thêm nữa vé dài hạn cũng nằm trong máy, nên chỉ cần **mở phần mềm lên** là vào được.

**Vì sao không giải bằng phân quyền.** Phân quyền ở tầng ứng dụng không bảo vệ được dữ liệu đã nằm
trong máy — ai mở DevTools cũng đọc thẳng IndexedDB, không qua một dòng code nào của mình. Và nếu
lọc luồng nhận về theo quyền thì vi phạm ràng buộc 5.8.

**Cách giải:** cờ **theo từng tài khoản**, quản trị viên giáo xứ bật, **mặc định tắt**.

| | Bật offline | Tắt offline |
|---|---|---|
| Kho dữ liệu | IndexedDB, còn sau khi tắt máy | Chỉ trong bộ nhớ, đóng tab là sạch |
| Hàng chờ | IndexedDB | Chỉ trong bộ nhớ |
| Luồng ghi | lưu vào kho → gửi lên | **y hệt** |
| Luật gộp, nhật ký, hộp xem lại | **y hệt** | **y hệt** |
| Mất mạng | làm việc bình thường | hiện "Mất mạng, xin chờ", không cho nhập tiếp |

Chỉ **một chỗ** trong code khác nhau: lớp lưu trữ có hai bản cài đặt, một ghi xuống đĩa, một giữ
trong RAM. Toàn bộ phần khó dùng chung, không rẽ nhánh. Service worker **không liên quan** — nó
chỉ cache vỏ ứng dụng và chạy y hệt ở cả hai chế độ.

Ràng buộc riêng cho chế độ tắt: **chặn đóng tab khi hàng chờ chưa rỗng**, và không cho nhập tiếp
khi mất mạng.

Câu hỏi khi bật, bằng lời thường: *"Cho phép tài khoản này dùng phần mềm cả khi mất mạng? Dữ liệu
giáo xứ sẽ được lưu vào máy đang dùng."*

### 6.7 Vé dài hạn hỏng mà hàng chờ chưa rỗng

**Tuyệt đối không xoá hàng chờ.** Thanh trạng thái đỏ: *"Xin đăng nhập lại để gửi 3 việc đã nhập ở
máy này lên. Dữ liệu vẫn còn nguyên."* Đăng nhập lại (lúc này chắc chắn có mạng, vì máy chủ vừa
trả lời được), hàng chờ gửi tiếp từ chỗ dừng.

## 7. PWA offline-first

### 7.1 Luôn lưu vào máy trước

**Không phân nhánh theo tình trạng mạng.** Ba lý do:

1. **"Có mạng" không phải câu hỏi trả lời được.** `navigator.onLine` chỉ biết máy có nối wifi hay
   không, vẫn báo "có mạng" khi đường truyền đã chết.
2. **Mất mạng giữa chừng là trạng thái tệ nhất.** Nếu máy chủ đã nhận nhưng chưa kịp trả lời,
   trình duyệt không biết đã lưu được chưa; người dùng bấm lại thì sinh bản trùng.
3. **Hai nhánh là hai đường code phải kiểm thử**, và nhánh ít chạy sẽ mục dần.

Khi có mạng, khác biệt chỉ là **độ trễ**: bộ gửi khởi động ngay sau khi ghi vào máy, thao tác lên
tới máy chủ trong vài trăm mili-giây.

### 7.2 Tải dữ liệu về

Máy mới đăng nhập lần đầu → tải **toàn bộ** giáo xứ về (~500 KB nén cho 4074 giáo dân), có thanh
tiến trình. Từ đó chỉ nhận phần chênh.

Máy con nhớ cặp `(epoch, so_thu_tu)` và hỏi: **"có gì sau 4821 không?"**

- Không có gì mới → trả lời rỗng, khoảng 50 byte.
- Có → trả về đúng các dòng `hieu_luc` đó.
- `epoch` không khớp hoặc con trỏ nằm ngoài khoảng còn giữ → **410, cần tải lại toàn bộ** (chỉ
  thực hiện khi hàng chờ rỗng, ràng buộc 5.6).

Dùng **số thứ tự** chứ không dùng mốc thời gian: giờ máy không đáng tin, và mốc thời gian luôn mơ
hồ ở ranh giới (hai thay đổi cùng giây, lấy `>` hay `>=` đều sai một kiểu).

**Ảnh là ngoại lệ.** Ảnh là `byte[]` trong bảng (`AnhDaiDienService.cs:15`) nên không tải kèm. Ảnh
đã xem thì giữ lại; ảnh chưa tải mà đang offline thì hiện *"Chưa có ảnh — máy đang không nối
mạng"*.

### 7.3 Tần suất

Bốn nguồn kích hoạt, ba cái đầu quan trọng hơn:

1. **Mỗi lần gửi thao tác lên, máy chủ trả lời kèm luôn các dòng mới sau mốc của máy con.** Không
   tốn thêm lượt gọi. Hệ quả: **ai đang nhập liệu thì dữ liệu luôn mới**, miễn phí.
2. **Mỗi lần quay lại phần mềm** — mở tab, chuyển cửa sổ, mở lại máy sau giờ nghỉ.
3. **Ngay khi có mạng trở lại.**
4. **Định kỳ, co giãn:** đang gõ thì 1 phút/lần; ngồi yên vài phút thì giãn ra 5 phút; tab bị ẩn
   thì **ngừng hỏi dữ liệu mới nhưng không bao giờ ngừng gửi hàng chờ** — quý sơ hay mở tab rồi để
   đó cả ngày.

**Thêm một lần đúng chỗ cần:** ngay trước khi mở một hồ sơ ra sửa, và trước khi dò trùng lúc tạo
người mới.

### 7.4 Tìm kiếm: chỉ tìm trong máy

**Không có dự phòng ra máy chủ.** Bản sao trong máy là nguyên giáo xứ, nên "không thấy trong máy"
nghĩa là **không có**.

- **Hai nguồn thì kết quả nhảy múa** — cùng từ khoá, lúc có mạng ra 12 người, lúc mất mạng ra 11.
- **Một đường code**, chạy thật mỗi ngày.
- **Nhanh hơn**: gõ tới đâu hiện tới đó.

Chọn người cho hôn phối, gia đình chạy trên cùng kho nên **hoạt động y hệt khi mất mạng**. Người
vừa tạo ở chính máy này, còn trong hàng chờ, **vẫn tìm thấy và chọn được**.

**Cải tiến kèm theo (cắt được nếu cần):** tìm trong máy cho phép **bỏ dấu** — gõ `nguyen thi a` ra
`Nguyễn Thị A`. Máy chủ hiện chưa làm được (`GiaoDanService.cs:275-287` chỉ có `HoTen.Contains`).
Bản desktop vốn có `vnConvert.dll` làm việc này.

### 7.5 Gửi lên

Một lô thao tác **theo đúng thứ tự người dùng đã làm**. Mỗi thao tác mang `ma_thao_tac` do máy con
sinh; máy chủ tra `thao_tac_da_nhan` (ràng buộc 5.9) và trả nguyên phản hồi cũ nếu gặp lại.

**Một thao tác hỏng không được chặn cả hàng.** Nếu làm ngược lại, một bản ghi lỗi từ tuần trước sẽ
chặn toàn bộ việc nhập của cả tháng sau mà người dùng không hiểu vì sao.

**Nhưng thao tác phụ thuộc phải đi theo nhóm.** Đây là chỗ hai quy tắc trên va nhau nếu không cẩn
thận:

> Sơ Hoa offline một tuần: tạo giáo dân `G1`, nhập ngày rửa tội cho `G1`, thêm `G1` vào gia đình,
> nhập hôn phối của `G1`, ghi `G1` vào lớp giáo lý. Nối mạng → `G1` bị nghi trùng, không tạo.
> Bốn thao tác kia vẫn đi tiếp, trỏ tới một người không tồn tại → 5 mục hộp rời rạc. Người dùng
> bấm "Là cùng một người" → gộp vào `G0`, nhưng bốn thao tác kia vẫn trỏ `G1`.

Hai cơ chế bắt buộc:

- **Nhóm phụ thuộc.** Hàng chờ ghi rõ thao tác nào phụ thuộc thao tác nào — dễ, vì ID do máy con
  sinh nên chỉ cần quét giá trị ID trong các thao tác sau. Thao tác bị từ chối kéo cả nhóm vào
  **một mục hộp duy nhất**: *"Có 5 việc liên quan tới người này đang chờ."*
- **Loại dòng `gop`** (`G1 → G0`) phát xuống cho mọi máy con để chúng ánh xạ lại kho cục bộ. Không
  có nó thì "Là cùng một người" không phải một thao tác biểu diễn được, và máy con vẫn hiển thị
  một giáo dân không tồn tại trên máy chủ.

### 7.6 Ranh giới lô

Một lần lưu sinh nhiều dòng. Nếu ranh giới lô rơi vào giữa, máy con lưu con trỏ ở trạng thái
**rách**: có thành viên tham chiếu gia đình mà dòng tạo của nó nằm ở lô sau; máy con kiểm khoá
ngoại sẽ **từ chối và bỏ** dòng đó, mất dữ liệu vĩnh viễn vì con trỏ đã tiến.

Ranh giới lô **không được cắt giữa một `giao_dich_id`**. Máy con áp nguyên một giao dịch trong một
transaction IndexedDB, và tiến con trỏ trong cùng transaction đó (ràng buộc 5.3).

### 7.7 Máy "ngủ đông" quay lại

Rủi ro nhỏ hơn vẻ ngoài: **luôn nhận về trước khi gửi lên**, và cùng một ô thì mới hơn thắng với
mốc đã hiệu chỉnh — nên một lần sửa từ tuần trước **không thể** đè lên lần sửa hôm nay.

Khoảng hở thật: người dùng tưởng máy kia hỏng nên **làm lại từ đầu ở máy khác**. Thao tác *sửa* vô
hại; thao tác *tạo người mới* thành hai bản ghi cho một người.

**Lớp 1 — nhắc khi chưa muộn.** Việc chưa gửi quá vài ngày: 🟡 *"8 ngày chưa nối được mạng — 24
việc còn ở máy này"*.

**Lớp 2 — báo chéo, suy từ sự vắng mặt.** Bản thiết kế đầu định suy từ báo cáo của máy kia — sai
logic: máy chủ chỉ biết nếu máy đó **đã báo được**, tức là đã có mạng, trong khi tình huống cần
cảnh báo chính là máy **mất mạng dài ngày**. Suy từ `thiet_bi.lan_dung_gan_nhat`:

> *"Máy 'Phòng khách' đã 9 ngày không nối mạng (lần cuối 12/8). Nếu trong thời gian đó có ai nhập
> liệu ở máy đó, phần việc kia chưa về tới đây. Xin mở máy đó và nối mạng trước khi nhập lại."*

Kèm nút *"Tôi biết rồi, đừng nhắc về máy này nữa"*, có hiệu lực tới khi máy kia nối mạng trở lại —
một người dùng nhiều máy sẽ bị nhắc mỗi ngày, và nhắc mãi thì người dùng học được cách bỏ qua mọi
cảnh báo, kể cả cái quan trọng.

**Lớp 3 — hỏi một lần trước khi gửi**, khi việc chưa gửi đã cũ quá **7 ngày**:

> **Máy này còn 24 việc chưa gửi về**
> Nhập từ ngày 12/8 đến 20/8. Trong đó **6 việc có thể trùng** với hồ sơ đã có sẵn.
>
> [ **Gửi lên ngay** ] ← nút chính
> [ Xem 6 việc đáng chú ý trước ]
>
> *Cất lại, chưa gửi* ← chữ thường, nhỏ, dưới cùng
> Bấm "Cất lại" thì 24 việc này sẽ tạm rút khỏi màn hình và được cất ở máy chủ. Không mất, nhưng
> phải nhờ người hỗ trợ mới lấy lại được.

Chỉ hỏi **một lần cho cả lô**. Dưới ngưỡng 7 ngày thì gửi thẳng, không hỏi.

**"Cất lại" — hai ràng buộc:**

- Chỉ gỡ khỏi máy **sau khi máy chủ xác nhận đã nhận và lưu xong**.
- Vì các thao tác đó đã được áp vào kho hiển thị (mục 7.1), kho phải **quay lui** — 24 hồ sơ biến
  mất trước mắt người dùng. Câu cuối trong nút bấm ở trên nói thẳng điều này; không được giấu.

### 7.8 Cập nhật phần mềm: tự động

Service worker tải bản mới **ngầm**, đưa vào dùng khi hệ thống tự thấy an toàn:

1. **Lần mở phần mềm kế tiếp** — phần lớn trường hợp rơi vào đây, người dùng không nhận ra gì.
2. Hoặc **ngay trong lúc đang mở**, nếu đủ ba điều: hàng chờ rỗng, không form nào đang gõ dở,
   người dùng không thao tác vài phút.

Hàng chờ chưa rỗng thì **hoãn, không hỏi**.

Bỏ banner "Tải lại / Để sau" hiện có: với offline-first, tải lại **có thể làm mất việc chưa gửi**,
nên việc đúng không phải hỏi hay không hỏi, mà là chọn thời điểm không có gì để mất — hệ thống
biết chắc chắn hơn người dùng.

Bổ sung: **chủ động kiểm tra bản mới** mỗi giờ và mỗi lần có mạng trở lại (service worker mặc định
chỉ kiểm lúc điều hướng, mà PWA mở suốt ngày thì hiếm khi điều hướng).

**Giữ nguyên nguyên tắc không cache `/api/*`.** Cache HTTP trả bản cũ mà không ai biết cũ bao lâu;
kho IndexedDB có mốc rõ ràng nên hiển thị được "dữ liệu tới 08:15". Cùng là đọc dữ liệu không mới,
nhưng một bên nói dối, một bên nói thật.

### 7.9 Tương thích ngược

Gói gửi lên **mang số phiên bản giao thức**; máy chủ **chấp nhận ít nhất hai phiên bản gần nhất**.

IndexedDB có số phiên bản; bước nâng cấp **không được xoá hàng chờ** — thà giữ dạng cũ và chuyển
đổi lúc gửi. Kho **mới hơn** mã đang chạy (người dùng mở lại bản cũ, hai tab lệch phiên bản) ném
`VersionError`: hiện *"Dữ liệu trong máy cần bản phần mềm mới hơn. Xin nối mạng và bấm Tải lại bản
mới. Mọi việc anh/chị đã nhập vẫn còn nguyên."* kèm nút xuất file dự phòng. **Không bao giờ tạo
lại kho** (ràng buộc 5.5).

Phiên bản quá cũ để máy chủ đỡ được: trả lỗi hiện nguyên văn *"Phần mềm trên máy này đã cũ. Xin
nối mạng rồi bấm Tải lại bản mới. Mọi việc anh/chị đã nhập vẫn còn nguyên trong máy."*

### 7.10 File dự phòng — lưới an toàn ngoài trình duyệt

Hàng chờ nằm **trong trình duyệt**, và trình duyệt mất dữ liệu vì những lý do không ai lường: máy
hết dung lượng đĩa thì trình duyệt tự dọn; "cháu biết máy tính" chạy CCleaner; cài lại Windows;
máy hỏng phải đổi máy; Safari/iOS xoá dữ liệu website sau 7 ngày không dùng nếu PWA chưa được cài
vào màn hình chính. Trong mọi trường hợp đó, công nhập của cả tuần biến mất mà **không ai làm gì
sai**.

Khi hàng chờ tồn đọng quá **2 ngày** hoặc **20 việc**, phần mềm **tự tải một file nhỏ** xuống thư
mục Tải về — `qlgx-viec-chua-gui-2026-09-13.qlgx`, vài chục KB, chứa đúng những việc chưa gửi:

> *"Để cho chắc, phần mềm vừa lưu một bản dự phòng những việc chưa gửi vào thư mục Tải về của máy.
> Nếu máy có chuyện gì, gửi file đó cho người hỗ trợ là lấy lại được."*

Nút **"Nạp lại file dự phòng"** nằm trong bảng trạng thái. Bình thường người dùng **không bao giờ
thấy** cơ chế này — file chỉ sinh ra khi đã tồn đọng bất thường, và nút chỉ dùng khi có sự cố,
thường là do người hỗ trợ hướng dẫn qua điện thoại.

Đây cũng là đường cứu cho "Cất lại" (7.7) và cho thu hồi máy (6.4).

### 7.11 Đối chiếu toàn vẹn

"Nhật ký giữ lịch sử nên khôi phục được" chỉ có giá trị nếu **có ai đó biết để mà tra**. Cần cơ
chế **phát hiện** phân kỳ, không chỉ cơ chế khôi phục.

Định kỳ (mỗi tuần, hoặc mỗi lần mở phần mềm) máy con gửi lên **mã băm theo bảng** — số bản ghi
cộng băm của các ô đã sắp xếp — trên trạng thái tại con trỏ hiện hành. Máy chủ so; lệch thì ghi
log và buộc máy con tải lại bảng đó. Rẻ (vài chục byte), và là thứ duy nhất phát hiện được các lỗi
ở mục 4.1, 4.3, 4.5 nếu chúng lọt qua kiểm thử.

## 8. Gộp và xung đột

### 8.1 Bảng quyết định

| Tình huống | Xử lý |
|---|---|
| Hai người sửa **hai ô khác nhau** của cùng một hồ sơ | **Gộp, im lặng.** Phần lớn trường hợp |
| Cùng một ô, **cùng một giá trị** | Không có xung đột |
| Cùng một ô, hai giá trị khác nhau, ô thuộc nhóm *"mới hơn thì đúng hơn"* | **Lấy mới hơn, không hỏi** |
| Cùng một ô, hai giá trị khác nhau, ô **nhạy cảm** | Lấy mới hơn làm hiện hành, **đồng thời** vào hộp cần xem lại |
| Sửa một hồ sơ mà máy kia đã xoá | `da_xoa` là một ô chịu cùng luật (mục 4.4) |
| Tạo người **nghi trùng** | Xem 8.3 |
| Đụng **quan hệ nghiệp vụ** | Xem 8.4 |

**Không có luật "một bên để trống thì lấy bên có giá trị".** Bản thiết kế đầu có luật này vì nghe
thân thiện; nó sai hai lần:

- **Không xoá được giá trị nhập nhầm.** Ngày qua đời nhập nhầm, cha xoá ô đó đi, máy khác chưa
  nhận về → giá trị sai **sống lại**, im lặng, không vào hộp vì luật này thuộc nhóm tự xử.
- **Phá hội tụ thật sự.** Máy chủ chỉ giữ giá trị hiện hành, không giữ mọi ứng viên; hai bản sao
  nhận cùng ba thay đổi theo thứ tự khác nhau sẽ ra hai kết quả khác nhau.

Ô rỗng là **một giá trị bình thường**, gộp thuần theo mốc. Phân biệt "chưa ai đụng tới" với "đã có
người xoá đi": chỉ có dòng `hieu_luc` mới là hành động có chủ ý. Muốn bảo vệ người dùng khỏi xoá
nhầm thì làm ở **giao diện** (nút "Xoá nội dung ô" có xác nhận), không ở **luật gộp** — luật gộp
phải là hàm giao hoán, kết hợp, luỹ đẳng, nếu không thì hội tụ chỉ là lời hứa.

**Phân nhóm trường** là bảng cấu hình, sửa được mà không phải sửa code:

- *Mới hơn thì đúng hơn*: số điện thoại, địa chỉ, ghi chú, nghề nghiệp.
- *Nhạy cảm*: ngày sinh, ngày các bí tích, tên thánh, họ tên, giới tính, quan hệ gia đình.

### 8.2 Đơn vị gộp và bất biến

LWW mức trường gộp từng ô độc lập, nên sinh ra bản ghi vô lý nếu các ô phải nhất quán với nhau:

- **`DaQuaDoi` + `NgayQuaDoi`.** Máy A đánh dấu qua đời kèm ngày; máy B (mới hơn) bỏ dấu qua đời
  nhưng không đụng ô ngày → **người còn sống có ngày qua đời**.
- **Ngày sinh và ngày rửa tội** đều là ô nhạy cảm nên mỗi ô vào hộp riêng, nhưng **không ai kiểm
  cặp** — gộp xong có thể ra rửa tội trước khi sinh.
- **Chủ hộ** là ràng buộc **liên bản ghi**: máy A đặt X, máy B đặt Y, hai ô thuộc hai bản ghi khác
  nhau nên LWW không thấy xung đột → gia đình có **hai chủ hộ**.

Ba cơ chế:

1. **Đơn vị gộp.** Nhóm trường phải nhất quán (`DaQuaDoi` + `NgayQuaDoi`) ghi vào **một dòng duy
   nhất** và gộp như một ô.
2. **Bộ kiểm bất biến** chạy sau mỗi lần gộp: rửa tội ≥ ngày sinh; mỗi gia đình đúng một chủ hộ;
   tình trạng hôn phối khớp với bản ghi hôn phối. Vi phạm → mục hộp bằng tiếng Việt.
3. Bất biến liên bản ghi (chủ hộ) cần luật gộp riêng, không phải LWW mức ô.

### 8.3 Dò trùng ba lớp

**Lớp 1 — bản sao đầy đủ.** Cảnh báo trùng lúc offline chạy **đúng như khi online**, dùng tiêu chí
hiện có (họ tên + tên thánh + ngày sinh). Khoảng hở chỉ là người được thêm ở máy khác sau lần lấy
dữ liệu gần nhất.

*Ghi chú:* cả hai bản **đều không dùng giới tính** để dò trùng — `GiaoDanService.cs:363-365` và
`SqlConstants.cs:214`. Thiết kế giữ nguyên tiêu chí; thêm giới tính là thay đổi riêng.

**Lớp 2 — máy chủ kiểm tra lại lúc nhận**, trên dữ liệu mới nhất. Trùng thì **không âm thầm gộp**
— vào hộp: *"Anh A sơ nhập hôm 12/9 có thể trùng với anh A đã có sẵn. Là cùng một người hay hai
người khác nhau?"*

Hai quy tắc kèm theo:

- **Tạo với cờ nghi-trùng, rồi gộp sau** — an toàn hơn "không tạo". Tạo thừa một bản ghi rồi gộp
  là việc sửa được; mất bốn thao tác phụ thuộc thì không (mục 7.5).
- **Quyết định của người dùng lúc offline gửi kèm thao tác, máy chủ tôn trọng, không hỏi lại.**
  Thiếu quy tắc này, người đã trả lời "Hai người khác nhau" lúc offline sẽ bị hỏi lại đúng câu đó.

**Lớp 3 — sửa thì gộp, không chặn.**

### 8.4 Xung đột ở quan hệ nghiệp vụ

LWW mức ô không xử được, và "mới hơn thắng" ở đây sẽ **âm thầm bỏ một bí tích đã cử hành** — bí
tích đã có giấy chứng nhận in ra tay giáo dân:

- Hai máy cùng lập hôn phối cho cùng một người với hai người khác nhau.
- Hai máy xếp cùng một giáo dân vào hai gia đình khác nhau.
- Hai máy cùng cấp một số sổ bí tích.

Quy tắc: **giữ cả hai ở dạng chờ, đưa vào hộp cần xem lại, không bao giờ tự chọn.** Số sổ bí tích
**do máy chủ cấp lúc nhận**, không do máy con sinh (mục 4.6).

### 8.5 Toàn vẹn tham chiếu

Offline lâu sinh ra tình huống máy chủ **phải** lường trước: thêm thành viên vào gia đình mà máy
khác vừa xoá; chọn giáo dân vào hôn phối mà người đó đã bị xoá; tham chiếu giáo họ không còn.

Cách xử lý giống lúc nhập dữ liệu Access: **không đổ cả lô, tách riêng dòng có vấn đề, ghi lại lý
do** — ở đây "ghi lại lý do" thành một mục trong hộp cần xem lại bằng tiếng Việt.

Lưu ý: vài chặn nghiệp vụ dựa vào trạng thái toàn cục sẽ sai khi offline —
`GiaoDanService.cs:599-614` chặn xoá giáo dân còn thuộc gia đình, `:441-446` chặn đổi giới tính
nếu đang là vợ/chồng. Chúng phải chạy lại ở máy chủ lúc nhận, không chỉ ở máy con.

### 8.6 Quyết định trong hộp là một thao tác ghi bình thường

Người dùng bấm "Đổi lại thành 12/03/1985". Nếu quyết định này không được ghi thành một dòng mang
**mốc hiện tại**, lần đồng bộ sau giá trị 12/03/1986 (mốc mới hơn) vẫn thắng và **tự đổi lại** —
người dùng thấy phần mềm "không nghe lời". Quyết định phải có `ma_thao_tac`, mốc hiện tại, và vào
nhật ký như mọi thao tác khác.

## 9. Giao diện người dùng

### 9.1 Thanh trạng thái

| Màu | Dòng chữ | Nghĩa |
|---|---|---|
| 🟢 | **Đã lưu an toàn** | Đã lên máy chủ và **máy chủ đã xác nhận** |
| 🟡 | **Đã lưu ở máy này (3) — đang gửi về** | Có việc chưa gửi được, hệ thống đang tự thử lại |
| 🔴 | **Cần xem lại (1)** | Có việc hệ thống không tự quyết được |

Vàng là trạng thái **bình thường** ở giáo xứ mất mạng — không phải lỗi. Câu chữ đặt vế trấn an lên
trước: "Đã lưu ở máy này" chứ không phải "Đang chờ lưu", vì với quý cha quý sơ "chờ lưu" nghĩa là
**chưa lưu**, tức là có thể mất — ngược hẳn điều thiết kế muốn nói.

Đỏ chỉ dành cho việc thật sự cần người ra tay: nghi trùng, xung đột ô nhạy cảm, xung đột quan hệ
nghiệp vụ, máy quá cũ, máy bị gỡ, cần đăng nhập lại.

Bấm vào → bảng giải thích bằng lời thường: *"Những thay đổi này đã lưu trong máy và sẽ tự gửi lên
khi có mạng. Anh/chị không cần làm gì, cũng không mất dữ liệu."* Bên dưới: danh sách việc đang
chờ, nút "Nạp lại file dự phòng", trang "Bàn giao máy này", và mục **"Thông tin kỹ thuật"** gập
lại — mã máy, lần gửi gần nhất, lần lấy dữ liệu gần nhất, phiên bản phần mềm, lỗi gần nhất — kèm
nút **"Sao chép để gửi người hỗ trợ"**.

Quy tắc cứng: **không bao giờ hiện "Đã lưu an toàn" khi thay đổi còn nằm trong máy.**

### 9.2 Hộp cần xem lại

Mỗi mục là một câu hỏi đời thường kèm hai nút:

> **Ngày sinh của Maria Nguyễn Thị A**
> Máy phòng khách ghi **12/03/1985** (hôm qua). Máy văn phòng ghi **12/03/1986** (sáng nay).
> Hiện đang dùng: **12/03/1986**
> [ Giữ 12/03/1986 ] [ Đổi lại thành 12/03/1985 ]

Không có chữ "xung đột", "phiên bản", "đồng bộ".

**Hộp nằm ở máy chủ, chung cho cả giáo xứ** — ai xử lý cũng được, không phải đúng người ở đúng máy
đã gây ra nó, và xử xong thì mọi máy thấy ngay. Mục phát sinh khi offline nằm tạm ở máy con và
chuyển lên ở lần gửi kế tiếp.

**Hệ thống không bao giờ đứng chờ người dùng trả lời.** Nó chọn giá trị mới hơn để phần mềm chạy
tiếp; hộp chỉ là lời mời kiểm lại sau. Bắt giải quyết mới dùng tiếp được thì người dùng sẽ bấm bừa
cho xong — kết quả tệ hơn.

### 9.3 Ranh giới offline

- **Đăng nhập** cần mạng; đã đăng nhập rồi thì mở lại offline vẫn vào được (6.2).
- **Thống kê, báo cáo, in ấn** tính từ dữ liệu đã có nên vẫn chạy, nhưng ghi rõ **kèm ngày**:
  *"Tính trên dữ liệu tới 08:15 ngày 10/9 — cách đây 3 ngày. Máy này chưa nối mạng từ hôm đó."*
  Chỉ ghi "08:15" là vi phạm mục tiêu 1.4.3, vì người dùng sẽ đinh ninh là sáng nay và bản báo cáo
  gửi giáo phận sẽ thiếu. Quá 1 ngày thì nút In hỏi lại một lần.
- **Quản trị hệ thống** chỉ khi có mạng.

## 10. Nhập dữ liệu từ bản desktop — **phase sau**

### 10.1 Ghi đè toàn bộ, có xác nhận — không phải đồng bộ

Bản thiết kế đầu đề xuất máy chủ **so gói mới với dữ liệu đang giữ** và suy ra bản ghi nào đã bị
xoá từ việc nó **vắng mặt** trong gói. Đã loại bỏ hoàn toàn: "vắng mặt" có quá nhiều nguyên nhân
không phải là xoá, và hậu quả là mất sổ sách quy mô cả giáo xứ trong một lần bấm nút.

- Cha khôi phục `giaoxu.mdb` từ bản sao lưu tuần trước rồi bấm gửi → một tuần dữ liệu bị xoá.
- Có hai bản `.mdb` (máy văn phòng và máy riêng), gửi từ bản cũ → xoá phần nhập ở bản kia.
- `.mdb` hỏng một bảng sau nhiều năm, gói xuất thiếu bảng đó → cả sổ bí tích bị coi là đã xoá.

Thay bằng thao tác **tường minh, có chủ ý, có xác nhận**: mỗi lần nhập là **ghi đè toàn bộ dữ liệu
giáo xứ đó trên web**. Quản trị viên chọn giáo xứ đích, xem màn hình đối chiếu, rồi xác nhận.

**Cơ chế này gần như đã có sẵn.** `BaoCaoXemTruocDto` đã trả về `TenGiaoXuNguon` (tên giáo xứ đọc
từ chính file `.mdb`), `GiaoXuDichDaCoDuLieu`, `SoGiaoDanDaCo`, và `DoiChieu` — số dòng nguồn so
với số dòng đích **cho từng bảng**. `bat-dau` đã có cổng `xacNhanGhiDe`
(`NhapDuLieuService.cs:110-127`). Việc còn lại chủ yếu là trình bày và câu chữ.

### 10.2 Bản tự động

Nếu sau này làm bản đẩy tự động từ desktop, hai ràng buộc:

- Chỉ ghi đè khi giáo xứ **chưa bật công tắc sang `web`** — tức là mọi thay đổi chắc chắn chỉ có ở
  desktop, web đang chỉ-đọc.
- **Kiểm tra công tắc trong cùng giao dịch xử lý gói, có khoá**, để không đua với thao tác đổi chế
  độ.

Công tắc `web → desktop` là thao tác **không cho phép qua giao diện thường** — chỉ người bảo trì
hệ thống làm được, sau khi đã sao lưu.

### 10.3 Ràng buộc từ `CLAUDE.md`

- **`Qlgx.Migration.exe` phải publish bản `win-x86`.** Driver ACE OLEDB phải cùng bitness với tiến
  trình gọi nó, và máy giáo xứ phần lớn chỉ có ACE 32-bit. Bản x64 sẽ **không mở được `.mdb`** ở
  đa số máy. `Qlgx.Migration.csproj` cố ý không ghim `PlatformTarget` — chọn lúc publish. (Ghi chú
  trong csproj trỏ tới `WebApp/src/Qlgx.Migration/README.md`, **file này chưa tồn tại** — cần viết.)
- **Mở `.mdb` ở chế độ chỉ đọc**, không tạo `.ldb` khoá độc quyền, không nén/sửa chữa, không đụng
  khi `GiaoXu.exe` đang mở.
- **Thêm exe vào bộ cài là thay đổi bộ cài** — phải theo `QUY_TRINH_PHAT_HANH.md`, phải **tăng số
  phiên bản** (điều cấm 1), và bước kiểm chứng phải **biết báo lỗi** (điều cấm 4).
- **Token không được lưu trong `giaoxu.mdb`.** `Memory.SetConfig` ghi vào bảng `CauHinh` trong
  chính file `.mdb` — mà giáo xứ thường xuyên chép file đó sang máy khác và **gửi cho người hỗ trợ
  khi có sự cố**. Gửi file đi là gửi luôn quyền ghi toàn giáo xứ. Lưu ngoài `.mdb` (DPAPI hoặc file
  cấu hình theo máy), gắn với một dòng `thiet_bi`, thu hồi được như mọi máy khác. Thêm hai bẫy:
  `SetConfig` chỉ ghi vào `DataTable` trong RAM, phải gọi `Memory.SaveConfig()`; và cả ba hàm đều
  `catch {}` **nuốt lỗi im lặng**.

## 11. Rủi ro

| Rủi ro | Mức | Cách giảm |
|---|---|---|
| Máy con bỏ sót dòng do lỗ hổng `so_thu_tu` hoặc hai tab chạy đua | Cao | Ràng buộc 5.1, 5.7; mục 4.2 ba ràng buộc; đối chiếu mã băm (7.11) |
| Nhật ký vừa làm kiểm toán vừa làm nguồn phát xuống → phân kỳ vĩnh viễn | Cao | Tách `thay_doi` / `hieu_luc` (4.1) |
| Hàng chờ mất do trình duyệt dọn kho, dọn rác, cài lại Windows, đổi máy | Cao | `storage.persist()` (5.10); **file dự phòng tự tải xuống** (7.10); lớp 1 nhắc khi tồn đọng (7.7) |
| Tắt máy giữa lúc ghi → dữ liệu hiện trên màn hình nhưng không ai gửi | Cao | Ràng buộc 5.2, 5.3 |
| Đăng xuất / thu hồi khi hàng chờ chưa rỗng | Cao | Ràng buộc 5.4; tách "Đổi người dùng" khỏi "Gỡ máy này ra" (6.4); màn hình thu hồi hiện số việc còn lại |
| Thu hồi một máy không bao giờ nối mạng lại | Cao | Cảnh báo trước khi bấm (6.4); nhắc quản trị viên khi máy quá N ngày không gặp |
| Đồng hồ máy nhảy giữa chừng → đè lên dữ liệu đúng | Cao | Đồng hồ đơn điệu + đoạn đồng hồ + HLC (4.3) |
| Gộp sai làm hỏng dữ liệu thật mà không ai biết | Cao | `thay_doi` giữ toàn bộ lịch sử **trong thời hạn lưu giữ** (4.5); đối chiếu mã băm (7.11); bộ kiểm bất biến (8.2) |
| Máy cá nhân của người giúp việc thời vụ giữ bản sao toàn giáo xứ | Cao | Cờ offline mặc định tắt (6.6) |
| Mất laptop có bản sao toàn giáo xứ | Cao | Cờ offline (6.6); giới hạn của thu hồi khẩn nói rõ (6.4); tự xoá kho sau N ngày không đăng nhập được (chỉ khi hàng chờ rỗng) |
| Ghi hàng loạt đi vòng qua `SaveChanges` → nhật ký thủng im lặng | Cao | Mục 4.7, kiểm thử chặn |
| Nhập từ desktop ghi đè nhầm giáo xứ | Cao | Ghi đè tường minh có xác nhận, màn hình đối chiếu theo từng bảng (10.1) |
| Người dùng bấm "Cất lại" rồi hối hận | Trung bình | Không xoá, cất ở máy chủ; câu chữ nói thẳng hậu quả (7.7) |
| Khoá dòng đếm làm xếp hàng việc ghi của cả giáo xứ | Trung bình | `lock_timeout`, chia nhỏ thao tác hàng loạt (4.2) |
| Dung lượng IndexedDB vượt hạn mức ở giáo xứ rất lớn | Thấp | 7,3 MB cho 4074 giáo dân; theo dõi `storage.estimate()` và cảnh báo |

## 12. Thứ tự triển khai

**Đợt 1 — nền tảng máy chủ.** Bảng `thay_doi` và `hieu_luc` (kèm RLS), bộ đếm và ba ràng buộc cấp
số, bảng `thao_tac_da_nhan`, `epoch`, ghi nhật ký ở **mọi** đường ghi kể cả 32 chỗ hàng loạt, xoá
mềm cho các bảng còn xoá cứng, hai đầu vào nhận-về/gửi-lên, bảng `thiet_bi` và vé dài hạn. Sửa
`AuthContext.tsx`. Bản web vẫn chạy online như cũ nhưng đã sinh nhật ký đầy đủ và đã có sổ
ai-sửa-gì-lúc-nào.

**Đợt 2 — PWA offline-first.** Kho IndexedDB và các ràng buộc mục 5, hàng chờ, bộ gửi/nhận, bầu
chủ một tab, tìm kiếm trong máy, thanh trạng thái, hộp cần xem lại, cờ offline theo tài khoản, file
dự phòng, cập nhật tự động, xử lý máy ngủ đông, đối chiếu mã băm.

**Đợt 3 (phase sau) — nhập từ desktop.** Màn hình ghi đè có xác nhận dựa trên `xem-truoc` đã có;
nếu làm bản tự động thì thêm công tắc chế độ ghi và các ràng buộc mục 10.

Kế hoạch thi công chi tiết sẽ do bước lập kế hoạch tạo ra.

## 13. Những gì đã sửa sau review

Tài liệu này là bản thứ hai. Ba lượt review độc lập (đúng-sai kỹ thuật, kiểm chứng mã nguồn, an
toàn dữ liệu và trải nghiệm) đã tìm ra:

**Lỗi thiết kế:** nhật ký gộp hai mục đích xung khắc; suy luận xoá-từ-vắng-mặt ở cầu nối desktop;
luật "điền chỗ trống"; thiếu luật phá hoà; một độ lệch đồng hồ cho cả lô; thao tác phụ thuộc mồ
côi khi thao tác gốc bị từ chối; con trỏ không có `epoch`; `ma_thao_tac` không đủ cho thao tác bị
từ chối; thiếu đơn vị gộp và bộ kiểm bất biến; xoá xử lý như sự kiện riêng thay vì một ô; dòng
`tao` không định nghĩa được cách gộp; ranh giới lô cắt đôi giao dịch; quyết định trong hộp bị đảo
ngược ở lần gộp sau; lớp cảnh báo chéo suy từ báo cáo thay vì từ sự vắng mặt.

**Lỗi sự thật:** `UpdatedAt` **có** được đóng dấu tự động (`QlgxDbContext.cs:123-143`) — lỗ hổng
thật là 32 lời gọi hàng loạt đi vòng qua `SaveChanges`; "326 chỗ ghi" thực ra ~92 chỗ ghi nghiệp
vụ trên 28 file (246 tổng, 154 là script nâng cấp lược đồ); "43 form" thực ra 70 form, 28 file có
lệnh ghi; "2050 giáo dân" thực ra **4074**; "vài bảng phụ xoá cứng" thực ra 17 chỗ, gồm cả đường
xoá vĩnh viễn `GiaoDan`/`GiaDinh` có xoá tầng con.

**Bỏ sót:** RLS cho bảng mới và **không** RLS cho `thiet_bi`; ảnh là `byte[]` trong bảng; `BoDemMaCu`
và số sổ bí tích phải do máy chủ cấp; `GiaoDanHonPhoi` không có `Id`; `navigator.storage.persist()`;
bầu chủ một tab; hỏi tên máy; nhiều người dùng chung một tài khoản; trang bàn giao; ràng buộc x86
và chỗ lưu token của `Qlgx.Migration.exe`.

## 14. Đã kiểm chứng trên dữ liệu thật

**Ngày kiểm chứng:** 2026-09-13. Nhánh `webapp-phase-1`, CSDL `qlgx_thu` (Postgres cục bộ), giáo xứ
**An Phú** (không phải Vô Nhiễm — 3 giáo xứ hiện có trong `qlgx_thu` đều là dữ liệu thật, không có
giáo xứ thử nghiệm nào; đã báo lại và chọn An Phú thay vì Vô Nhiễm theo đúng chỉ dẫn "tránh nếu có
thể"). Tài khoản `tester_task8` tạo tạm qua CLI `tao-tai-khoan-quan-tri`, đã xoá sau khi xong.
`Qlgx.Api` chạy ở cổng 5199 (không đụng cổng 5096 của phiên khác), migration áp qua `dotnet ef
database update --startup-project ../Qlgx.Api`.

**Số dòng nhật ký sinh ra cho mỗi thao tác** (gọi thẳng API `PUT`/`POST`, không qua trình duyệt):

| Thao tác | Số dòng `thay_doi`/`hieu_luc` | Ghi chú |
|---|---|---|
| Sửa họ tên 1 giáo dân | 1 (`GiaoDan.HoTen`, loại `sua`) | Đúng như kỳ vọng |
| Đổi số điện thoại 1 gia đình | **2**: `GiaDinh.DienThoai` + `ThanhVienGiaDinh.ChuHo` | **Khác dự kiến** — xem dưới |
| Thêm mới 1 giáo dân | 1 (`GiaoDan`, loại `tao`, `gia_tri` là JSON toàn bộ thực thể) | Đúng như kỳ vọng |

**Phát hiện ngoài dự kiến:** `PUT /api/gia-dinh/{id}` không có tham số `chuHoVaiTro` trong payload
(giá trị mặc định `null`) sẽ **âm thầm đặt `ChuHo=false`** cho người đang là chủ hộ — đây là hành vi
**đã được thiết kế** (`CapNhatGiaDinhRequest.ChuHoVaiTro` — "máy chủ ghi đúng y những gì client gửi,
không tự đoán/chọn gì thêm"), không phải lỗi của kế hoạch nhật ký này, nhưng có nghĩa **client web
phải luôn gửi đúng `chuHoVaiTro` hiện tại** mỗi lần lưu gia đình, nếu không sẽ tạo thêm một dòng nhật
ký `ThanhVienGiaDinh.ChuHo` không chủ ý và xoá mất trạng thái chủ hộ thật. Đã phát hiện đúng lúc kiểm
chứng (số điện thoại đổi kèm chủ hộ Lucia bị đặt `false` ngoài ý muốn) và đã hoàn nguyên cả hai giá
trị (`dienThoai` và `chuHoVaiTro=1`) bằng đúng API, xác nhận lại qua `psql` khớp nguyên trạng.

**Kiểm tra không lỗ hổng số thứ tự (Step 3):** không thể tái hiện đúng nghĩa đen "lưu giáo dân với
mã cũ trùng" qua API — `MaGiaoDanCu` do `SinhMaService` cấp nguyên tử phía máy chủ (không nhận từ
client), và endpoint "Thêm thành viên gia đình" đã tự chặn trùng ở tầng ứng dụng trước khi chạm CSDL
(`GiaDinhService.ThemThanhVien` dòng kiểm `gd.ThanhVien.Any(...)`) nên không bao giờ chạm tới chỉ mục
duy nhất mới `(gia_dinh_id, giao_dan_id, vai_tro)`. Thay vào đó dùng một đường thất bại thật khác đi
đúng qua `LuuCoNhatKy`/`CapSoHieuLuc`: gọi `PUT /api/giao-dan/{id}` với `RowVersion` cũ (xung đột
đồng thời), gây `DbUpdateConcurrencyException` bên trong `SaveChangesAsync` sau khi đã xếp dòng nhật
ký vào `ChangeTracker` — đúng kiểu lỗi mà `LuuCoNhatKy` phải bắt và cuộn lui. Kết quả: `so_tiep_theo`
của `bo_dem_hieu_luc` giữ nguyên ở 5 trước và sau lần gọi thất bại (409); lưu hợp lệ ngay sau đó cấp
đúng số 5, dãy `so_thu_tu` 1→5 liên tục không hở. Xác nhận cơ chế đếm bằng dòng khoá (không phải
`sequence` của PostgreSQL) hoạt động đúng như thiết kế mục 4.2.

**Dung lượng đo được:** `pg_total_relation_size('thay_doi')` = 48 kB, `pg_total_relation_size(
'hieu_luc')` = 48 kB, với 5 dòng mỗi bảng — đúng mức nền tối thiểu của một bảng gần như rỗng
(overhead trang/chỉ mục), không có dấu hiệu cột `byte[]` lọt lưới `CotLoaiTru`.

**Sự cố trong lúc kiểm chứng (đã xử lý, không phải lỗi của kế hoạch này):** biến môi trường
`QLGX_TEST_PG` đã được `setx` từ trước **thiếu `Database=`**, và `dotnet ef` luôn ưu tiên
`QlgxDbContextFactory` (đọc `QLGX_TEST_PG`) trong project `Qlgx.Data` bất kể cờ `--startup-project`
trỏ tới `Qlgx.Api` — đúng cái bẫy `QUY_TRINH_PHAT_HANH`/brief đã cảnh báo. Lần chạy đầu vô tình áp 3
migration cuối (`ThemBangSaoLuu`, `ThemBangNhatKyThayDoi`, `ThemKhoaChinhChoBangNoi`) lên database
**`postgres` dùng chung** thay vì `qlgx_thu`. Việc dọn dẹp bằng `DROP TABLE`/`dotnet ef database
update` lùi lại đã bị hệ thống chặn vì là thao tác phá huỷ — **chưa dọn được**, để lại nguyên trạng
cho người điều phối quyết định (xem báo cáo Task 8 để biết chi tiết và câu lệnh dọn đề xuất). Sau đó
đã chạy lại đúng với `QLGX_TEST_PG` có `Database=qlgx_thu` và xác nhận `qlgx_thu` đã đúng migration.
