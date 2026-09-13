# Thiết kế: Đồng bộ dữ liệu offline — PWA offline-first và cầu nối từ bản desktop

Ngày: 2026-09-13. Nhánh: `webapp-phase-1`. Trạng thái: đã thống nhất với người dùng, chờ lập kế
hoạch triển khai.

## 1. Mục tiêu và bối cảnh

### 1.1 Vấn đề

QLGX đang tồn tại ở hai bản: bản desktop WinForms dùng Access (`Source/`) đang chạy thật ở nhiều
giáo xứ, và bản web (`WebApp/`) vừa hoàn tất kiểm thử chấp nhận. Hai bản không nói chuyện được
với nhau. Câu hỏi đặt ra là làm thế nào để dữ liệu chảy được giữa hai bên, và làm thế nào để bản
web dùng được ở những nơi mạng không ổn định.

Bốn ràng buộc từ thực tế vận hành:

- **Đa số giáo xứ có mạng đủ tốt** — dùng được Zalo, Facebook trên web. Nhóm gần như không có
  mạng là thiểu số, nhưng có thật và không bỏ được.
- **Một giáo xứ có nhiều người nhập liệu**, đôi khi cùng sửa một hồ sơ, mỗi người một máy riêng.
- **Người dùng là quý cha, quý sơ**, phần lớn không rành máy tính.
- **Dữ liệu là sổ sách giáo xứ nhiều năm** — mất là không lấy lại được.

Hiện trạng kỹ thuật liên quan (đã khảo sát):

| Điểm | Vị trí | Ý nghĩa với thiết kế này |
|---|---|---|
| Chống ghi đè mức **bản ghi** bằng `xmin` | `ThucTheCoSo.cs`, `GiaoDanService.cs:453,495` | Chặn được ghi đè nhưng không gộp được; hai người sửa hai ô khác nhau vẫn báo lỗi |
| `UpdatedAt` **không tự cập nhật** khi sửa | `ThucTheCoSo.cs:10-25` | Không thể dùng mốc thời gian để suy ra thay đổi |
| **Không có nhật ký thay đổi** | — | Không biết ai sửa gì, lúc nào |
| JWT sống 8 giờ, **không có refresh token** | `TokenService.cs:18-21` | Mất mạng dài ngày là bị đá ra màn hình đăng nhập, mà đăng nhập lại thì cần mạng |
| Mất mạng bị **xoá token** | `AuthContext.tsx:68-73` — `.catch(() => authStore.xoaToken())` | Lỗi chặn đường cho offline-first |
| PWA **chỉ cache vỏ ứng dụng**, cố ý không cache `/api/*` | `vite.config.ts:17-21` | Nguyên tắc đúng, thiết kế này giữ nguyên |
| Bản nháp form ở `localStorage`, **không phải hàng chờ** | `banNhap.ts` | Không dùng lại được cho đồng bộ |
| Dò trùng giáo dân: họ tên + tên thánh + ngày sinh, **không có giới tính** | `GiaoDanService.cs:364-370` và `SqlConstants.cs:214` | Hai bản dùng **cùng** tiêu chí — khác với giả định ban đầu |
| Desktop cảnh báo trùng nhưng bấm "No" **vẫn lưu** | `frmGiaoDan.cs:674-681` | Không dùng được khi nhiều máy cùng nhập |
| `DataProvider.Execute` là nút thắt ghi duy nhất, nhưng **326 chỗ gọi** truyền SQL thô | `DBAccess.cs:231-278` | Chèn nhật ký vào đây đòi phân tích cú pháp SQL — loại bỏ |
| `BangAnhXaId` sinh UUID tất định từ (bảng, ID cũ) | `Qlgx.Migration.Core` | Nhập lại nhiều lần là **idempotent** — nền tảng cho đồng bộ desktop→web |
| Toàn bộ giáo xứ 2050 giáo dân = **7,3 MB JSON, 500 KB nén** | đo trên `temp/goi.json.gz` | Tải nguyên giáo xứ về máy là khả thi |
| `Qlgx.Migration.Core` là `net10.0`, desktop là .NET Framework 4.8 | `.csproj` | Không tham chiếu trực tiếp được; desktop gọi `Qlgx.Migration.exe` như công cụ rời |

### 1.2 Triết lý thiết kế

Người dùng **không phải quan tâm chuyện kỹ thuật**. Hệ thống tự bảo vệ người dùng và dữ liệu,
tự động hoá tối đa khi biết chắc là an toàn, và chỉ hỏi khi đoán sai sẽ làm hỏng sổ sách. Dễ
dùng là ưu tiên hàng đầu — giống tinh thần bản desktop.

Hệ quả trực tiếp: không có chữ "đồng bộ", "xung đột", "phiên bản" nào xuất hiện trước mặt người
dùng. Không có hộp thoại kỹ thuật. Không có thao tác nào của người dùng làm mất vĩnh viễn dữ
liệu đã nhập.

### 1.3 Mục tiêu

1. **Không bao giờ mất dữ liệu đã nhập** — kể cả máy hỏng, phiên đăng nhập hỏng, cập nhật phần
   mềm, hay người dùng bấm nhầm.
2. **Hội tụ.** Mọi máy nối mạng đủ lâu đều về cùng một kết quả, không phụ thuộc thứ tự nối mạng.
3. **Luôn nói thật dữ liệu cũ tới đâu.** Không bao giờ hiện dữ liệu cũ như dữ liệu thật.
4. **Nhiều người cùng nhập không đè lên nhau.** Hai người sửa hai ô khác nhau của cùng một hồ sơ
   phải gộp được tự động.
5. **Giáo xứ chưa chuyển sang web vẫn đẩy được dữ liệu lên**, không phải xây đồng bộ hai chiều
   với Access.

### 1.4 Ngoài phạm vi

- **Kênh đẩy thời gian thực** (WebSocket/SSE). Hỏi định kỳ co giãn là đủ; thiết kế không cản việc
  thêm sau.
- **Mã hoá kho dữ liệu trong máy.** Đổi lại phải gõ mật khẩu mỗi lần mở offline — chưa đáng cho
  bản đầu. Hiện trạng desktop còn để `giaoxu.mdb` không mã hoá trên ổ đĩa.
- **Đồng bộ hai chiều thật sự với Access.** Desktop chỉ đẩy một chiều lên (mục 6).
- **Tải ảnh đại diện về hàng loạt.** Ảnh tải theo nhu cầu, giữ lại bản đã xem.
- **Offline cho màn hình quản trị hệ thống** (tạo giáo xứ, tài khoản, nhập dữ liệu Access).

## 2. Hướng đã chọn và các hướng đã loại

**Đã chọn — PWA offline-first, đồng bộ mức trường.** Bản web giữ một bản sao đầy đủ của giáo xứ
trong máy, ghi vào máy trước rồi đẩy lên sau, gộp thay đổi ở mức từng ô dữ liệu. Desktop chỉ đẩy
một chiều lên rồi dần nghỉ hưu.

**Đã loại — hộp máy chủ đặt tại giáo xứ** (LAN nội bộ, dùng lại y nguyên bản web và cơ chế
`xmin`). Rẻ về mặt code nhưng đẩy gánh nặng vận hành về giáo xứ — trái với hướng đi tập trung
hoá đã chọn. Không loại trừ về sau: cùng một giao thức đồng bộ, hộp nội bộ chỉ là một máy con
đặc biệt.

**Đã loại — đồng bộ hai chiều đầy đủ với desktop.** Đắt nhất (326 chỗ ghi, mỗi form một kiểu
`UpdateDate`, có chỗ xoá cứng) mà vẫn để lại hai bộ mã nguồn phải nuôi lâu dài.

**Chi phí không dồn vào nhóm thiểu số.** Đường ghi "lưu vào máy trước" là một đường code duy nhất
cho mọi người dùng: giáo xứ mạng tốt được hưởng lưu tức thì, không mất việc khi wifi chập, tìm
kiếm hiện theo từng phím gõ. Phần dành riêng cho nhóm mất mạng dài ngày chỉ là mục 5.6.

## 3. Nền tảng dữ liệu

### 3.1 Nhật ký thay đổi mức trường

Bảng mới `thay_doi`. Mỗi dòng là **một ô dữ liệu bị sửa**, không phải cả bản ghi:

| Cột | Kiểu | Ý nghĩa |
|---|---|---|
| `so_thu_tu` | bigint | Số tăng dần **riêng theo từng giáo xứ**; máy con dùng làm con trỏ đã-nhận-tới-đâu |
| `giao_xu_id` | uuid | Khoá tenant, chịu RLS như các bảng khác |
| `bang` | text | Tên bảng bị sửa |
| `ban_ghi_id` | uuid | Bản ghi bị sửa |
| `truong` | text | Tên trường bị sửa; **rỗng** với `tao` và `xoa` |
| `gia_tri` | jsonb | Giá trị mới |
| `loai` | text | `tao` / `sua` / `xoa` |
| `sua_luc` | timestamptz | Thời điểm theo đồng hồ máy con |
| `sua_luc_uoc_luong` | timestamptz | Thời điểm đã hiệu chỉnh về giờ máy chủ — **cái dùng để so ai mới hơn** |
| `thiet_bi_id` | uuid | Máy nào |
| `tai_khoan_id` | uuid | Ai |
| `ma_thao_tac` | uuid | Mã thao tác do máy con sinh, dùng chống gửi trùng |

Ghi mức trường là điều làm nên khác biệt: hai người sửa hai ô khác nhau của cùng một giáo dân
được gộp tự động, không ai bị chặn. Cơ chế `xmin` hiện tại chặn ở mức bản ghi nên một trong hai
chắc chắn phải làm lại.

Lợi ích kèm theo: có sổ ai-sửa-gì-lúc-nào mà cả hai bản hiện đều thiếu.

**Ba loại dòng nhật ký:**

- `tao` — một dòng duy nhất, `truong` rỗng, `gia_tri` chứa **toàn bộ bản ghi** dạng jsonb.
- `sua` — một dòng cho **mỗi ô** bị sửa.
- `xoa` — một dòng duy nhất, `truong` rỗng, `gia_tri` rỗng.

**Cách cấp `so_thu_tu`.** Số phải liên tục, không được có lỗ hổng nhìn thấy từ phía máy con: nếu
một giao dịch cấp số 4822 rồi bị huỷ trong khi 4823 đã commit, máy con nhận 4823 và **vĩnh viễn
bỏ sót 4822**. Dùng `sequence` của PostgreSQL sẽ dính đúng lỗi này.

Cách chốt: mỗi giáo xứ có một dòng đếm; giao dịch nào ghi nhật ký thì khoá dòng đó
(`SELECT ... FOR UPDATE`), lấy số kế tiếp, ghi nhật ký, rồi commit cùng một lượt. Số chỉ tăng khi
giao dịch commit thành công, nên không bao giờ có lỗ hổng. Cái giá là các lần ghi **trong cùng
một giáo xứ** bị xếp hàng — chấp nhận được vì một giáo xứ chỉ có vài người nhập liệu, và các giáo
xứ khác nhau không ảnh hưởng nhau.

Ràng buộc bắt buộc, phải có kiểm thử riêng: **máy con không bao giờ được bỏ sót dòng đã commit.**

### 3.2 Hiệu chỉnh đồng hồ máy con

Máy ở giáo xứ chạy nhiều năm không ai chỉnh giờ. Nếu lấy `sua_luc` của máy con làm căn cứ ai mới
hơn, một máy lệch ba ngày sẽ **luôn thắng**, âm thầm đè lên dữ liệu đúng.

Mỗi lô đồng bộ mang theo giờ hiện tại của máy con. Máy chủ tính độ lệch so với giờ của chính nó
và hiệu chỉnh mọi mốc trong lô đó thành `sua_luc_uoc_luong`. Lệch quá một giờ vẫn nhận nhưng ghi
cảnh báo cho quản trị viên.

Mọi so sánh "ai mới hơn" đều dùng `sua_luc_uoc_luong`, không bao giờ dùng `sua_luc`.

### 3.3 Xoá phải để lại dấu vết

Hiện chỉ 5 bảng có cờ `DaXoa`; vài bảng phụ như `RaoHonPhoi` **xoá cứng**. Với đồng bộ, xoá cứng
gây hậu quả rõ: máy A xoá, máy B chưa biết, đồng bộ xong bản ghi **sống lại**. Hồi sinh một bản
ghi đã xoá khó phát hiện hơn xoá nhầm.

Mọi thao tác xoá ghi một dòng `loai = 'xoa'` vào nhật ký. Nhật ký chính là bia mộ — không cần
thêm cột `da_xoa` vào từng bảng.

### 3.4 ID do máy con sinh

Khi tạo bản ghi mới lúc offline, máy con tự sinh `Guid` và dùng luôn ID đó khi đồng bộ. Không có
ID tạm rồi đổi. Hệ thống vốn dùng `Guid` làm khoá chính nên không phải đổi gì.

Hệ quả quan trọng: thao tác phụ thuộc nhau vẫn làm được khi offline — tạo gia đình xong thêm
thành viên ngay, dù cả hai chưa lên máy chủ.

### 3.5 Công tắc chế độ ghi theo giáo xứ

Mỗi giáo xứ có một công tắc: `web` hoặc `desktop`, nghĩa là bên nào đang được quyền ghi.

- **Chế độ `desktop`** — giáo xứ chưa chuyển: bản desktop đẩy dữ liệu lên, web **chỉ cho xem**.
- **Chế độ `web`** — đã chuyển: web ghi bình thường, desktop chuyển sang chỉ-đọc và chỉ nhận về.
- Đổi chế độ là thao tác có chủ ý của quản trị viên, có xác nhận và ghi nhật ký.

Công tắc này là điều kiện để **không phải xây đồng bộ hai chiều với Access** — chỗ đắt và rủi ro
nhất của bài toán — mà vẫn phục vụ được cả hai nhóm giáo xứ.

## 4. Phiên đăng nhập lâu dài

### 4.1 Hai loại vé

| | Vé ra vào (giữ nguyên) | Vé dài hạn (mới) |
|---|---|---|
| Sống bao lâu | ~1 giờ | **1 năm, tự gia hạn mỗi lần dùng** |
| Dùng làm gì | Kèm theo mọi lời gọi API | Chỉ để đổi lấy vé ra vào mới |
| Máy chủ có lưu không | Không | **Có — một dòng trong bảng `thiet_bi`** |

Mỗi lần đồng bộ, phần mềm âm thầm đổi vé dài hạn lấy vé ra vào mới. Người dùng **không bao giờ
thấy màn hình đăng nhập lại** chừng nào còn dùng ít nhất mỗi năm một lần — như Gmail, Facebook.

`TokenService.cs:9-14` ghi rằng máy chủ không giữ trạng thái phiên **trong tiến trình**, để chạy
nhiều máy chủ song song. Dòng `thiet_bi` nằm trong PostgreSQL dùng chung, **không phải trong tiến
trình**, nên ràng buộc đó vẫn nguyên. Việc tra CSDL chỉ xảy ra lúc gia hạn (khoảng mỗi giờ),
không phải mỗi lời gọi API.

### 4.2 Mất mạng không được đá người dùng ra

`AuthContext.tsx:68-73` hiện coi lỗi mạng như token hỏng và xoá token. Phải sửa: chỉ đăng xuất
khi máy chủ **thật sự trả lời 401**; lỗi mạng thì giữ nguyên phiên và chạy tiếp bằng dữ liệu
trong máy.

Nguyên tắc: **quyền vào phần mềm khi offline do bản ghi phiên trong máy quyết định, không do vé
còn hạn hay không.** Vé hết hạn lúc offline là chuyện bình thường.

### 4.3 Thiết bị và thu hồi

Mỗi máy đăng nhập lần đầu tạo một dòng `thiet_bi`: tên máy, trình duyệt, lần dùng gần nhất, ai
đăng nhập, trạng thái. Quản trị viên của giáo xứ xem được danh sách máy của giáo xứ mình.

**Đăng nhập đúng mật khẩu là đủ để máy mới dùng được ngay** — không cần ai duyệt trước. Với giáo
xứ chỉ có một người phụ trách, "chờ ai đó duyệt" là chỗ tắc không lối ra. Bảo vệ bằng thu hồi
*sau*, giống cách Gmail hiển thị các thiết bị đang đăng nhập.

Hai kiểu thu hồi, vì hai tình huống khác hẳn nhau:

- **Thu hồi thường** (máy cũ, đổi người phụ trách): máy được phép **gửi nốt hàng chờ** rồi mới
  xoá kho dữ liệu.
- **Thu hồi khẩn** (mất máy): xoá ngay, không nhận gì thêm.

Quản trị viên chọn, với câu chữ giải thích rõ hậu quả từng kiểu.

### 4.4 Vé dài hạn hỏng mà hàng chờ chưa rỗng

**Tuyệt đối không xoá hàng chờ.** Thanh trạng thái chuyển đỏ với dòng *"Cần đăng nhập lại để gửi
3 thay đổi đang chờ"*. Đăng nhập lại (lúc này chắc chắn có mạng, vì máy chủ vừa trả lời được),
hàng chờ gửi tiếp từ chỗ dừng.

## 5. PWA offline-first

### 5.1 Luôn lưu vào máy trước

**Không phân nhánh theo tình trạng mạng.** Mọi thao tác ghi đều vào IndexedDB trước, rồi bộ đẩy
nền gửi lên.

Ba lý do:

1. **"Có mạng" không phải câu hỏi trả lời được.** `navigator.onLine` chỉ biết máy có nối wifi
   hay không, vẫn báo "có mạng" khi đường truyền đã chết.
2. **Mất mạng giữa chừng là trạng thái tệ nhất.** Nếu máy chủ đã nhận nhưng chưa kịp trả lời,
   trình duyệt không biết đã lưu được chưa; người dùng bấm lại thì sinh bản trùng.
3. **Hai nhánh là hai đường code phải kiểm thử**, và nhánh ít chạy sẽ mục dần. Với phần mềm giữ
   sổ sách thật, đường code hiếm chạy là đường code không ai biết nó hỏng.

Khi có mạng, khác biệt chỉ là **độ trễ**: bộ đẩy nền khởi động ngay sau khi ghi vào máy, thao tác
lên tới máy chủ trong vài trăm mili-giây. Cảm giác không khác lưu thẳng.

### 5.2 Tải dữ liệu về

Máy mới đăng nhập lần đầu → tải **toàn bộ** giáo xứ về (~500 KB nén), có thanh tiến trình. Từ đó
trở đi **không tải lại toàn bộ nữa**.

Cơ chế nhận về: máy con nhớ một con số — *đã nhận tới dòng nhật ký thứ 4821* — và hỏi đúng một
câu: **"có gì sau 4821 không?"**

- Không có gì mới → trả lời rỗng, khoảng 50 byte.
- Có → trả về đúng các dòng nhật ký đó, máy con áp vào kho và ghi mốc mới.

Dùng **số thứ tự** chứ không dùng mốc thời gian: giờ máy không đáng tin (mục 3.2), và mốc thời
gian luôn mơ hồ ở ranh giới (hai thay đổi cùng giây, lấy `>` hay `>=` đều sai một kiểu). Đã nhận
tới 4821 nghĩa là chắc chắn có đủ mọi thứ tới 4821, không sót không lặp.

Chia lô để mạng yếu vẫn nuốt được; đứt giữa chừng thì lần sau xin tiếp từ mốc đã nhận xong.

**Ảnh là ngoại lệ.** Ảnh đại diện (`client.ts:197`) nặng gấp nhiều lần phần chữ nên không tải
kèm. Ảnh đã xem thì giữ lại; ảnh chưa tải mà đang offline thì hiện khung xám "Ảnh chưa tải về".

### 5.3 Tần suất: theo sự kiện, không chỉ định kỳ

Bốn nguồn kích hoạt, ba cái đầu quan trọng hơn:

1. **Mỗi lần gửi thao tác lên, máy chủ trả lời kèm luôn các dòng mới sau mốc của máy con.** Không
   tốn thêm lượt gọi. Hệ quả: **ai đang nhập liệu thì dữ liệu luôn mới**, miễn phí.
2. **Mỗi lần quay lại phần mềm** — mở tab, chuyển cửa sổ, mở lại máy sau giờ nghỉ.
3. **Ngay khi có mạng trở lại.**
4. **Định kỳ, co giãn:** đang gõ thì 1 phút/lần; ngồi yên vài phút thì giãn ra 5 phút; tab bị ẩn
   thì ngừng hẳn. Giáo xứ dùng USB 3G tính theo dung lượng không phải trả tiền cho tab không ai
   nhìn.

**Thêm một lần đúng chỗ cần:** ngay trước khi mở một hồ sơ ra sửa, và trước khi chạy dò trùng lúc
tạo người mới. Đây là hai thời điểm dữ liệu cũ gây hại thật.

Trong giáo xứ có mạng, độ trễ tối đa là 1 phút khi đang làm việc, và gần như tức thì cho người
đang nhập liệu nhờ điểm 1.

### 5.4 Tìm kiếm: chỉ tìm trong máy

**Không có dự phòng ra máy chủ.** Bản sao trong máy là **nguyên giáo xứ**, nên "không thấy trong
máy" nghĩa là **không có**, không phải "chưa tải về".

Lý do không làm dự phòng:

- **Hai nguồn thì kết quả nhảy múa** — cùng từ khoá, lúc có mạng ra 12 người, lúc mất mạng ra 11.
  Người dùng sẽ kết luận phần mềm "lúc được lúc không".
- **Một đường code**, chạy thật mỗi ngày, không có nhánh hiếm dùng để mục.
- **Nhanh hơn**: gõ tới đâu hiện tới đó, không độ trễ mạng.

Dữ liệu mới nhập ở máy khác không giải bằng cách tìm thêm ở máy chủ, mà bằng cách kéo dữ liệu về
cho mới (mục 5.3) — cách này có lợi cho mọi màn hình, không riêng ô tìm kiếm.

Chọn người cho hôn phối, gia đình chạy trên cùng kho trong máy nên **hoạt động y hệt khi mất
mạng**. Người vừa tạo ở chính máy này, còn trong hàng chờ, **vẫn tìm thấy và chọn được** nhờ mục
3.4.

**Cải tiến kèm theo (cắt được nếu cần):** tìm trong máy cho phép **bỏ dấu** — gõ `nguyen thi a`
ra `Nguyễn Thị A`. Máy chủ hiện chưa làm được (`GiaoDanService.cs:275-287` chỉ có
`HoTen.Contains`), nhưng trong máy chỉ là chuẩn hoá chuỗi lúc dựng chỉ mục. Bản desktop vốn có
`vnConvert.dll` làm việc này, nên đây là lấy lại thứ người dùng đã quen.

### 5.5 Giao thức gửi lên

Một lô thao tác trong hàng chờ, **theo đúng thứ tự người dùng đã làm**. Mỗi thao tác mang
`ma_thao_tac` do máy con sinh, nên gửi trùng lô — chuyện chắc chắn xảy ra khi mạng chập chờn —
máy chủ nhận ra và bỏ qua. **Không có mã này thì mạng yếu sẽ tự sinh ra dữ liệu trùng.**

**Một thao tác hỏng không được chặn cả hàng.** Thao tác bị từ chối tách riêng ra hộp cần xem lại;
những thao tác còn lại vẫn đi tiếp. Nếu làm ngược lại, một bản ghi lỗi từ tuần trước sẽ chặn toàn
bộ việc nhập của cả tháng sau mà người dùng không hiểu vì sao.

### 5.6 Máy "ngủ đông" quay lại

Rủi ro nhỏ hơn vẻ ngoài vì hai quy tắc đã có: **luôn nhận về trước khi gửi lên**, và **cùng một ô
thì mới hơn thắng** với mốc đã hiệu chỉnh — nên một lần sửa từ tuần trước **không thể** đè lên
lần sửa hôm nay.

Khoảng hở thật còn lại: người dùng tưởng máy kia hỏng nên **làm lại từ đầu ở máy khác**. Thao tác
*sửa* thì vô hại; thao tác *tạo người mới* thì thành hai bản ghi cho một người.

Ba lớp, đặt từ sớm tới muộn:

**Lớp 1 — nhắc khi chưa muộn.** Việc chưa gửi quá vài ngày thì thanh trạng thái hiện
🟡 *"Chưa gửi lên được 8 ngày (24 thay đổi)"*.

**Lớp 2 — báo chéo giữa các máy.** Máy chủ biết máy nào còn việc chưa gửi. Người dùng đăng nhập ở
máy khác thấy ngay: *"Máy 'Phòng khách' còn 24 thay đổi chưa gửi lên (từ 12/8). Nếu anh/chị đang
định nhập lại những việc đó, hãy mở máy kia và nối mạng trước."* Lớp này chặn cái gốc của vấn đề,
rẻ hơn nhiều so với dọn 24 mục nghi trùng về sau.

**Lớp 3 — hỏi một lần trước khi gửi.** Khi việc chưa gửi đã cũ quá **7 ngày**, hiện một màn hình
duy nhất cho cả lô:

> Máy này có 24 thay đổi nhập từ 12/8 đến 20/8 chưa gửi lên. Trong thời gian đó, máy khác đã nhập
> 130 thay đổi. Trong 24 thay đổi này, **6 mục có thể trùng** với dữ liệu đã có.
>
> [ **Gửi lên** ] [ **Xem 6 mục đáng chú ý trước** ] [ **Cất đi, không gửi** ]

Chỉ hỏi **một lần cho cả lô** — 24 câu hỏi liên tiếp thì ai cũng bấm bừa. Dưới ngưỡng 7 ngày thì
gửi thẳng, không hỏi.

**"Cất đi" không bao giờ là xoá.** Cả lô được gói lại, gửi lên máy chủ cất vào kho lưu, quản trị
viên lấy lại được. Phần mềm nói đúng vậy: *"Những thay đổi này được cất lại, không mất. Nếu cần
lấy lại, liên hệ người hỗ trợ."*

Bản sao đọc cũ không cần cảnh báo gì — nhận về luôn chạy trước và luôn xin từ mốc đã có.

### 5.7 Cập nhật phần mềm: tự động, người dùng không phải biết

Service worker tải bản mới **ngầm**, và đưa vào dùng khi hệ thống tự thấy an toàn:

1. **Lần mở phần mềm kế tiếp** — không có gì đang gõ, không có gì dở dang. Phần lớn trường hợp
   rơi vào đây và người dùng không nhận ra gì.
2. Hoặc **ngay trong lúc đang mở**, nếu đủ ba điều: hàng chờ rỗng, không form nào đang gõ dở,
   người dùng không thao tác vài phút.

Hàng chờ chưa rỗng thì **hoãn, không hỏi**. Bản mới nằm sẵn, đẩy xong hàng chờ thì tự vào ở lần
mở kế tiếp.

Bỏ banner "Có bản cập nhật mới — Tải lại / Để sau" hiện có (`CapNhatPWA.tsx`): với offline-first,
tải lại **có thể làm mất việc chưa gửi**, nên việc đúng không phải hỏi hay không hỏi, mà là chọn
thời điểm không có gì để mất — hệ thống biết chắc chắn hơn người dùng.

Cần bổ sung so với hiện tại: **chủ động kiểm tra bản mới** mỗi giờ và mỗi lần có mạng trở lại
(service worker mặc định chỉ kiểm lúc điều hướng, mà PWA mở suốt ngày thì hiếm khi điều hướng).

**Giữ nguyên nguyên tắc không cache `/api/*`.** Dữ liệu offline đến từ kho IndexedDB do ta chủ
động quản lý và biết rõ mốc, **không** từ cache HTTP âm thầm. Cache HTTP trả bản cũ mà không ai
biết cũ bao lâu; kho IndexedDB có mốc rõ ràng nên hiển thị được "dữ liệu tới 08:15". Cùng là đọc
dữ liệu không mới, nhưng một bên nói dối, một bên nói thật.

### 5.8 Tương thích ngược khi phiên bản lệch

Gói gửi lên **mang số phiên bản giao thức**, và máy chủ **chấp nhận ít nhất hai phiên bản gần
nhất**. Máy mất mạng ba tuần vẫn gửi được những gì đã nhập.

IndexedDB có số phiên bản; mỗi lần đổi cấu trúc phải viết bước nâng cấp. Bước nâng cấp **không
được xoá hàng chờ** — thà giữ dạng cũ và chuyển đổi lúc gửi.

Khi phiên bản quá cũ để máy chủ đỡ được, máy chủ **không im lặng bỏ qua** mà trả lỗi hiện nguyên
văn: *"Phần mềm trên máy này quá cũ so với máy chủ. Hãy nối mạng rồi bấm Tải lại bản mới. Dữ liệu
anh/chị đã nhập vẫn còn nguyên trong máy."* — câu cuối là câu quan trọng nhất.

## 6. Cầu nối từ bản desktop

### 6.1 Desktop **không** cần nhật ký thay đổi

Ở chế độ `desktop` (mục 3.5), web bị khoá chỉ-đọc nên **không có luồng thứ hai để gộp**. Máy chủ
không cần biết người dùng đã sửa ô nào; nó chỉ cần biết dữ liệu hiện giờ trông ra sao và tự suy
ra cái gì đã đổi.

### 6.2 Đẩy nguyên gói, máy chủ tự so

Mỗi lần đồng bộ, desktop:

1. Gọi `Qlgx.Migration.exe` xuất toàn bộ `.mdb` ra `.json.gz` (~500 KB).
2. Gửi gói lên bằng giáo xứ ID + token.
3. Máy chủ so gói mới với dữ liệu đang giữ và **tự sinh các dòng nhật ký `tao`/`sua`/`xoa`** cho
   đúng những ô thật sự khác.

Phần đắt nhất đã xong: `BangAnhXaId` sinh UUID tất định từ (bảng, ID cũ) nên nhập lại lần hai
không tạo bản trùng mà ánh xạ đúng vào bản ghi cũ. Đợt nhập thử hai giáo xứ thật (An Phú, Thạch
Bi) xác nhận cơ chế này chạy đúng.

Việc còn phải làm phía web chỉ là đổi "ghi đè nếu đã có" thành "so từng trường, ô nào khác thì
ghi nhật ký". Logic so trường **dùng chung** với phần web ghi nhật ký ở mục 3.1 — viết một lần,
dùng hai chỗ.

### 6.3 Bốn điểm đáng nói

**Xoá suy ra từ sự vắng mặt.** Bản ghi có ở máy chủ mà không có trong gói mới → đã bị xoá ở
desktop. Chỉ an toàn **nhờ công tắc mục 3.5**: vì web không được ghi ở chế độ này, "vắng mặt"
chắc chắn nghĩa là xoá. Không có công tắc thì suy luận này sẽ xoá nhầm dữ liệu người khác vừa
nhập.

**Không đụng tới 326 chỗ ghi.** Phương án chèn nhật ký vào `DataProvider.Execute` nghe gọn vì đó
là nút thắt duy nhất, nhưng các chỗ gọi truyền SQL thô — muốn biết sửa ô nào phải **phân tích cú
pháp SQL**. Đó là loại code sai âm thầm trên phần mềm đang giữ sổ sách thật.

**Vấn đề .NET Framework tự tan.** Desktop không nhúng thư viện `net10.0`, chỉ gọi
`Qlgx.Migration.exe` self-contained kèm theo như một công cụ rời. Hai thế giới runtime không chạm
nhau.

**500 KB mỗi lần là chấp nhận được** — nhỏ hơn 9 lần so với chính file `.mdb`, và giáo xứ chế độ
desktop chỉ đồng bộ theo tuần/tháng. Nếu sau này có giáo xứ quá lớn, tối ưu là gửi kèm mã băm
từng bảng để bỏ qua bảng không đổi. Chưa cần làm.

### 6.4 Công sức

| Việc | Ước lượng |
|---|---|
| Desktop: nút "Đồng bộ lên", lưu ID + token qua `Memory.GetConfig/SetConfig`, gọi exporter, gửi gói, hiện kết quả | Nhỏ — không đụng 43 form nhập liệu |
| Máy chủ: đầu vào nhận gói bằng token thay vì đăng nhập quản trị | Nhỏ — tái dùng `/api/quan-tri/nhap-du-lieu/*` |
| Máy chủ: đổi "ghi đè" thành "so trường rồi ghi nhật ký" | Vừa — **dùng chung với mục 3.1** |
| Desktop: khoá chỉ-đọc khi giáo xứ đã sang chế độ `web` | Nhỏ |

Hướng desktop→web gần như **không tốn thêm gì ngoài phần đã phải làm cho PWA**.

## 7. Gộp và xung đột

### 7.1 Bảng quyết định

Từ "tự xử" xuống "phải hỏi":

| Tình huống | Xử lý |
|---|---|
| Hai người sửa **hai ô khác nhau** của cùng một hồ sơ | **Gộp, im lặng.** Phần lớn trường hợp. |
| Hai người sửa cùng một ô, **cùng một giá trị** | Không có xung đột. Bỏ qua. |
| Một bên **để trống**, bên kia **điền vào** | **Lấy giá trị có.** Điền chỗ trống gần như luôn đúng. |
| Cùng một ô, **hai giá trị khác nhau**, ô thuộc nhóm "mới hơn thì đúng hơn" | **Lấy giá trị mới hơn, không hỏi.** |
| Cùng một ô, hai giá trị khác nhau, ô thuộc nhóm nhạy cảm | Lấy **mới hơn** làm hiện hành, **đồng thời** đưa vào hộp cần xem lại |
| Máy này sửa hồ sơ mà máy kia đã xoá | Giữ nguyên đã xoá, đưa vào hộp cần xem lại |
| Tạo người **nghi trùng** (mục 7.2) | Không tạo, đưa vào hộp cần xem lại |

**Hệ thống không bao giờ đứng chờ người dùng trả lời.** Nó chọn luôn giá trị mới hơn để phần mềm
chạy tiếp bình thường; hộp cần xem lại chỉ là lời mời kiểm lại sau. Nếu bắt giải quyết xung đột
mới dùng tiếp được, người dùng sẽ bấm bừa cho xong — kết quả tệ hơn.

**Phân nhóm trường** là một bảng cấu hình, sửa được mà không phải sửa code:

- *Mới hơn thì đúng hơn* (tự quyết): số điện thoại, địa chỉ, ghi chú, nghề nghiệp.
- *Nhạy cảm* (vào hộp): ngày sinh, ngày các bí tích, tên thánh, họ tên, giới tính, quan hệ gia
  đình.

### 7.2 Dò trùng ba lớp

**Lớp 1 — bản sao đầy đủ.** Vì tải nguyên giáo xứ, cảnh báo trùng lúc offline chạy **đúng như khi
online**, dùng cùng tiêu chí hiện có (họ tên + tên thánh + ngày sinh). Khoảng hở còn lại chỉ là
người được thêm ở máy khác sau lần đồng bộ gần nhất.

Ghi chú về tiêu chí: cả hai bản **đều không dùng giới tính** để dò trùng — bản web ở
`GiaoDanService.cs:364-370`, bản desktop ở `SqlConstants.cs:214`. Thiết kế này giữ nguyên tiêu
chí đang có; nếu muốn thêm giới tính thì đó là thay đổi riêng, không thuộc phạm vi đồng bộ.

**Lớp 2 — máy chủ kiểm tra lại lúc nhận.** Thao tác "tạo giáo dân" bị dò trùng **một lần nữa**
trên dữ liệu mới nhất. Trùng thì **không tạo, cũng không âm thầm gộp** — vào hộp cần xem lại:
*"Anh A sơ nhập hôm 12/9 có thể trùng với anh A đã có sẵn. Là cùng một người hay hai người khác
nhau?"* với hai nút **Là cùng một người** (gộp, giữ các trường vừa nhập) / **Hai người khác
nhau** (tạo mới thật).

Đây là thay đổi có chủ ý so với desktop, nơi bấm "No" vẫn lưu bản trùng
(`frmGiaoDan.cs:674-681`). Với nhiều máy cùng nhập, để máy tự đoán là sai; hỏi người biết việc
mới đúng.

**Lớp 3 — sửa thì gộp, không chặn.** Nhờ nhật ký mức trường, hai người sửa hai ô khác nhau không
sinh ra phiên bản thứ hai.

### 7.3 Toàn vẹn tham chiếu khi offline lâu

Offline lâu sinh ra tình huống máy chủ **phải** lường trước, không được đổ vỡ: thêm thành viên
vào gia đình mà máy khác vừa xoá; chọn giáo dân vào hôn phối mà người đó đã bị xoá; tham chiếu
giáo họ không còn.

Loạt lỗi này giống hệt những gì gặp lúc nhập dữ liệu Access, và đã có cách xử lý đúng: **không đổ
cả lô, tách riêng dòng có vấn đề, ghi lại lý do**. Ở đây "ghi lại lý do" thành một mục trong hộp
cần xem lại bằng tiếng Việt thay vì một dòng cảnh báo trong log.

## 8. Giao diện người dùng

### 8.1 Thanh trạng thái

Một dòng, một chấm màu, luôn nhìn thấy:

| Màu | Dòng chữ | Nghĩa |
|---|---|---|
| 🟢 Xanh | **Đã lưu an toàn** | Mọi thứ đã lên máy chủ |
| 🟡 Vàng | **Đang chờ lưu (3)** | Có việc chưa gửi được, hệ thống đang tự thử lại |
| 🔴 Đỏ | **Cần xem lại (1)** | Có việc hệ thống không tự quyết được |

Vàng là trạng thái **bình thường** ở giáo xứ mất mạng — không phải lỗi, nên không dùng đỏ. Đỏ chỉ
dành cho việc thật sự cần người ra tay: nghi trùng, xung đột ô nhạy cảm, máy quá cũ, máy bị thu
hồi quyền, cần đăng nhập lại.

Bấm vào → bảng giải thích bằng lời thường: *"Những thay đổi này đã lưu trong máy và sẽ tự gửi lên
khi có mạng. Anh/chị không cần làm gì, cũng không mất dữ liệu."* Bên dưới là danh sách việc đang
chờ, và mục **"Thông tin kỹ thuật"** gập lại — mã máy, mốc đồng bộ gần nhất, phiên bản phần mềm,
lỗi gần nhất — kèm nút **"Sao chép để gửi người hỗ trợ"**.

Quy tắc cứng: **không bao giờ hiện "Đã lưu" trơn** khi thay đổi còn nằm trong máy.

### 8.2 Hộp cần xem lại

Không phải danh sách lỗi kỹ thuật. Mỗi mục là một câu hỏi đời thường kèm hai nút:

> **Ngày sinh của Maria Nguyễn Thị A**
> Sơ Hoa ghi **12/03/1985** (hôm qua). Cha Bình ghi **12/03/1986** (sáng nay).
> Hiện đang dùng: **12/03/1986**
> [ Giữ 12/03/1986 ] [ Đổi lại thành 12/03/1985 ]

Không có chữ "xung đột", "phiên bản", "đồng bộ". Chọn xong thì mục biến mất, thanh trạng thái về
🟢.

**Hộp nằm ở máy chủ, không nằm ở máy con** — chung cho cả giáo xứ. Nghĩa là ai xử lý cũng được,
không phải đúng người ở đúng máy đã gây ra nó; và người xử lý xong thì mọi máy khác thấy ngay.
Ngoại lệ duy nhất là những mục phát sinh khi máy đang offline: chúng nằm tạm ở máy con và chuyển
lên máy chủ ở lần đồng bộ kế tiếp.

### 8.3 Ranh giới offline

- **Đăng nhập** cần mạng; đã đăng nhập rồi thì mở lại offline vẫn vào được (mục 4.2).
- **Thống kê, báo cáo, in ấn** tính từ dữ liệu đã tải về nên vẫn chạy, nhưng ghi rõ *"tính trên
  dữ liệu tới 08:15"*.
- **Quản trị hệ thống** (tạo giáo xứ, tài khoản, nhập dữ liệu) chỉ khi có mạng.

## 9. Rủi ro và cách giảm

| Rủi ro | Mức | Cách giảm |
|---|---|---|
| Máy con bỏ sót dòng nhật ký do lỗ hổng trong `so_thu_tu` | Cao | Ràng buộc thiết kế ở mục 3.1; kiểm thử riêng cho trường hợp giao dịch bị huỷ xen kẽ |
| Hàng chờ trong IndexedDB bị mất (người dùng xoá dữ liệu duyệt web, chế độ riêng tư) | Cao | Cảnh báo khi phát hiện chạy ở chế độ riêng tư; đẩy hàng chờ càng sớm càng tốt; mục 5.6 lớp 1 nhắc khi tồn đọng lâu |
| Gộp sai làm hỏng dữ liệu thật mà không ai biết | Cao | Nhật ký giữ toàn bộ lịch sử nên khôi phục được giá trị cũ; phân nhóm trường thận trọng (7.1) |
| Người dùng bấm "Cất đi, không gửi" rồi hối hận | Trung bình | Không xoá, chỉ cất vào kho lưu ở máy chủ (5.6) |
| Kho dữ liệu trong máy bị người khác trên cùng máy Windows đọc | Trung bình | Không tệ hơn hiện trạng `giaoxu.mdb`; xoá kho khi đăng xuất; thu hồi thiết bị (4.3). Mã hoá để bản sau |
| Nhập trùng ở chế độ desktop do suy luận xoá-từ-vắng-mặt | Trung bình | Công tắc chế độ ghi (3.5) là điều kiện tiên quyết, phải kiểm tra trước mỗi lần nhận gói |
| Dung lượng IndexedDB vượt hạn mức trình duyệt ở giáo xứ rất lớn | Thấp | 7,3 MB cho 2050 giáo dân; theo dõi và cảnh báo khi vượt ngưỡng |

## 10. Thứ tự triển khai đề xuất

Chia làm ba đợt để có giá trị sớm và giảm rủi ro:

**Đợt 1 — nền tảng máy chủ.** Bảng `thay_doi`, hiệu chỉnh đồng hồ, ghi nhật ký ở mọi đường ghi
hiện có, hai đầu vào nhận-về/gửi-lên, bảng `thiet_bi` và vé dài hạn. Bản web vẫn chạy online như
cũ nhưng đã sinh nhật ký đầy đủ. Sửa `AuthContext.tsx`.

**Đợt 2 — cầu nối desktop.** Đầu vào nhận gói bằng token, so trường sinh nhật ký, công tắc chế độ
ghi, nút đồng bộ ở desktop. Giáo xứ chưa chuyển bắt đầu đẩy được dữ liệu lên. Đợt này độc lập với
đợt 3.

**Đợt 3 — PWA offline-first.** Kho IndexedDB, hàng chờ, bộ đẩy nền, tìm kiếm trong máy, thanh
trạng thái, hộp cần xem lại, cập nhật tự động, xử lý máy ngủ đông.

Kế hoạch thi công chi tiết sẽ do bước lập kế hoạch tạo ra.
