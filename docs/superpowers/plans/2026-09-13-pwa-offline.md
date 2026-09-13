# PWA offline-first (Kế hoạch 5) — Kế hoạch thi công

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bản web dùng được khi mất mạng — mọi thao tác lưu vào máy trước rồi tự gửi lên sau, và khi máy chủ bị lùi về bản sao lưu thì chính máy con là thứ bù lại dữ liệu đã mất.

**Architecture:** Kế hoạch 4 đã có giao thức ở máy chủ (nhận về / gửi lên / hộp cần xem lại / xoay `epoch`). Kế hoạch này viết phía trình duyệt: một kho IndexedDB giữ bản sao đầy đủ của giáo xứ, một **sổ đã nhận** (bản sao các dòng `hieu_luc` đã áp, để bù lại được sau khôi phục), một **hàng chờ** các thao tác chưa gửi, và một bộ đồng bộ chạy ở đúng một tab. Đường ghi của toàn ứng dụng đổi thành **luôn lưu vào máy trước** — một đường duy nhất, không rẽ nhánh theo tình trạng mạng.

**Tech Stack:** React 19 + TypeScript + Vite, IndexedDB (không dùng thư viện ORM — API thô, xem lý do ở Task 1), vitest + Testing Library, Playwright cho kiểm thử trình duyệt thật.

**Spec:** `docs/superpowers/specs/2026-09-13-dong-bo-offline-design.md` — mục **4.8 (khôi phục sau restore)**, 5 (ràng buộc bắt buộc), 6.6 (cờ offline), 7 (toàn bộ), 9 (giao diện).

## Global Constraints

- **Nhánh:** `webapp-phase-1`. Phiên Claude khác có thể làm song song — `git add` từng file cụ thể, **không `git add -A`**, sau commit `git show --stat` tự kiểm chứng.
- **Thư mục:** `WebApp/src/web/`. **Không đụng `Source/`.**
- **Máy chủ dev có thể đang chạy ở cổng 5096 và Vite ở 5173 — ĐỪNG dừng chúng.** Cần chạy thử thì dựng cổng riêng (ví dụ API 5299, Vite 5288) như đợt kiểm thử kế hoạch 1.
- **Đặt tên tiếng Việt không dấu**; **chú thích tiếng Việt có dấu** giải thích **vì sao**. Khớp lối viết của `lib/banNhap.ts` và `lib/trangThaiMang.ts`.
- **Câu chữ hiện ra màn hình phải theo bảng từ vựng ở spec mục 2.** Không có chữ "đồng bộ", "xung đột", "phiên bản", "hàng chờ" trước mặt người dùng. Người trực văn phòng giáo xứ không biết `IndexedDB` hay `API` là gì.
- **Chạy test:** `cd WebApp/src/web && npm test`. Kiểm thử trình duyệt thật ở Task 11.
- **Điều 4 của `CLAUDE.md`:** mỗi kiểm thử bảo vệ ràng buộc sống còn phải được **chứng minh biết báo lỗi**.

## Mười ràng buộc bắt buộc (spec mục 5) — mỗi cái phải có test riêng

Đây là danh sách quyết định chất lượng của cả kế hoạch. Không có cái nào là tuỳ chọn.

1. Máy con **không bao giờ bỏ sót** dòng `hieu_luc` đã commit.
2. **Ghi kho hiển thị và thêm dòng hàng chờ nằm trong MỘT giao dịch IndexedDB.** Không bao giờ có dữ liệu trên màn hình mà không có dòng tương ứng trong hàng chờ.
3. **Áp dòng nhận về, tiến con trỏ, và xoá thao tác đã gửi khỏi hàng chờ nằm trong MỘT giao dịch.**
4. **Không bao giờ xoá kho khi hàng chờ chưa rỗng** — không khi đăng xuất, không khi thu hồi, không khi nâng cấp.
5. **Không bao giờ xoá-và-tạo-lại kho khi mở lỗi.** `VersionError` phải hiện màn hình hướng dẫn kèm nút xuất file dự phòng.
6. **Tải lại toàn bộ chỉ khi hàng chờ rỗng.**
7. **Bộ gửi/nhận chạy ở đúng một tab.**
8. **Luồng nhận về chỉ lọc theo giáo xứ, không theo quyền người dùng.**
9. **Thao tác đã nhận không xử lý lại** (phía máy chủ đã có; máy con không được sinh `MaThaoTac` mới khi gửi lại).
10. **`navigator.storage.persist()` phải được gọi ngay sau đăng nhập** trên máy bật offline; bị từ chối thì **không** bật chế độ offline cho máy đó.

## Hợp đồng đồng hồ lai — phải khớp TỪNG BIT với bản C# (R6/R7 sổ thi công kế hoạch 4)

Bản TypeScript của `DongHoLai` phải cho **cùng một kết quả** với `WebApp/src/Qlgx.Data/DongBo/DongHoLai.cs`
trên cùng đầu vào. Lệch một chỗ là hai bản sao phân kỳ vĩnh viễn **mà không có bất kỳ lỗi nào hiện ra** —
loại hỏng tệ nhất trong hệ thống này, vì nó chỉ lộ ra sau nhiều tháng, khi đã không còn cách nào biết
bên nào đúng.

- **Thứ tự Guid là dạng chuỗi `"D"` chữ thường, so ordinal** (`a < b` theo mã điểm). **Tuyệt đối không**
  so theo mảng byte (`Uint8Array`), theo `BigInt`, hay theo bất kỳ dạng nào khác.

  Lý do, đã đo thật trên 300.000 cặp Guid ngẫu nhiên (kết quả này khác với bản ghi đầu tiên của ràng
  buộc — bản đó đoán sai và đã được sửa): `Guid.CompareTo` của .NET, `memcmp` của Postgres trên byte
  canonical, và so chuỗi của JavaScript **đều khớp nhau, 0 cặp lệch**. Cách duy nhất lệch là **so mảng
  byte thô**: `Guid.ToByteArray()` của .NET đảo little-endian ba trường đầu, nên lệch **149.958/300.000
  cặp (~50%)**.

  Nghĩa là bẫy không nằm ở chỗ bạn tưởng. Nó nằm đúng ở chỗ một người viết TypeScript rất dễ sa vào:
  dựng `Uint8Array` từ UUID rồi so từng byte "cho nhanh". Làm vậy là hai bản sao phá hoà khác nhau và
  phân kỳ vĩnh viễn **mà không một lỗi nào hiện ra**. Cứ so chuỗi — nó đã là dạng máy con lưu trong
  IndexedDB, không cần chuyển đổi gì.
- **`ThietBiId` null xếp TRƯỚC mọi giá trị có danh tính** (null = ghi từ máy chủ). Không quy null về
  `Guid.Empty` / chuỗi rỗng.
- **Mốc vật lý cắt về micro giây trong chính hàm dựng `DauDongHo`**, không phải ở nơi dùng và không phải
  lúc lưu. Bản C# đã vấp đúng chỗ này: lúc đầu cắt bên trong `NangDau`, và vì cắt **rồi mới** lấy max nên
  kết quả nhỏ hơn chính đầu vào chưa cắt tới 9 tick — đo thật **111.563/200.000 ca vi phạm**. `VatLy` là
  khoá so hàng đầu, nên dấu mới xếp **trước** dấu nó vừa thấy, và lần đồng bộ sau bản cũ đè ngược lên bản
  đã hợp nhất. Chỉ hàm dựng mới cưỡng chế được; để nơi dùng tự cắt thì sớm muộn có đường quên cắt và lỗi
  đó không lộ ra. `Date.getTime()` của JS chỉ có mili giây, nên khi dựng dấu phải nâng lên micro giây rõ
  ràng và giữ nguyên độ phân giải đó suốt vòng đời.
- **`NangDau(dauCuoiCuaTa, nhanDuoc, gioHienTai)` phải được gọi mỗi khi máy con ÁP một dòng `hieu_luc`
  nhận về** — đây là chỗ giữ nhân quả, và là lý do hàm này tồn tại. Bỏ qua nó thì: sơ mở một hồ sơ vừa
  tải về, sửa một ô, và bản sửa **thua chính bản ghi mà nó dựa vào** vì đồng hồ máy con chạy nhanh vài
  giây. Hiệu chỉnh độ lệch vật lý không cứu được — độ lệch đo qua mạng luôn sai.
- **Chuỗi JSON của giá trị ô phải khớp TỪNG BYTE với `System.Text.Json` mặc định của .NET.** Bản C#
  escape ký tự ngoài ASCII, nên `"Nguyễn Thị Bưởi"` được lưu là `"Nguyễn Thị Bưởi"`.
  `JSON.stringify` của JavaScript **không** escape như vậy — nó trả `"Nguyễn Thị Bưởi"` nguyên chữ. Hai
  chuỗi biểu diễn cùng một giá trị nhưng khác nhau từng byte, và luật gộp so **chuỗi**, không so giá trị.
  Hậu quả nếu bỏ qua: **mọi tên người Việt có dấu sinh một xung đột giả** ở mỗi lần đồng bộ — tức gần
  như mọi giáo dân. Hộp cần xem lại ngập hàng nghìn mục vô nghĩa, và quý sơ sẽ quen tay bấm bỏ qua rồi
  bỏ qua luôn mục thật. Đây là loại lỗi làm hỏng cả tính năng chứ không chỉ một ô.
  Máy chủ cũng chuẩn hoá lại một lần nữa khi nhận (kế hoạch 4, Task 6), nhưng **máy con vẫn phải sinh
  đúng ngay từ đầu** — nếu không, chính máy con sẽ tự so sai khi quyết định có gửi lên hay không.
- **Task 6 phải có một test vector dùng chung**: một file JSON liệt kê các cặp đầu vào/đầu ra, được
  **cả** bộ test C# **và** bộ test TypeScript đọc. Không nhân bản ca kiểm thử bằng tay ở hai nơi — nhân
  bản là cách hai bản trôi khỏi nhau.

---

## Cấu trúc file

| File | Trách nhiệm |
|---|---|
| `src/kho/moKho.ts` | Mở IndexedDB, nâng cấp phiên bản, xử lý `VersionError` |
| `src/kho/khoDuLieu.ts` | Đọc/ghi bản ghi nghiệp vụ trong kho |
| `src/kho/hangCho.ts` | Hàng chờ thao tác chưa gửi |
| `src/kho/soDaNhan.ts` | Sổ đã nhận — bản sao dòng `hieu_luc`, giữ 30 ngày (spec 4.8.2) |
| `src/kho/conTro.ts` | Con trỏ `(epoch, soThuTu)` và cờ chế độ |
| `src/dongbo/boDongBo.ts` | Vòng đồng bộ: nhận về, gửi lên, lịch chạy |
| `src/dongbo/bauChu.ts` | Bầu chủ một tab qua `navigator.locks` |
| `src/dongbo/dongHoMayCon.ts` | Đồng hồ đơn điệu, đoạn đồng hồ, mốc gửi lên |
| `src/dongbo/buSauKhoiPhuc.ts` | Phát hiện khôi phục và bù lại dữ liệu (spec 4.8) |
| `src/dongbo/tepDuPhong.ts` | Tự tải file dự phòng, nạp lại từ file |
| `src/api/ghiCucBo.ts` | Đường ghi mới: luôn lưu vào máy trước |
| `src/components/ThanhTrangThai.tsx` | Thanh trạng thái ba màu |
| `src/screens/CanXemLaiPage.tsx` | Hộp cần xem lại |
| `src/screens/BanGiaoMayPage.tsx` | Trang "Bàn giao máy này" |

---

## Task 1: Kho IndexedDB và các ràng buộc mở kho

**Files:** `src/kho/moKho.ts`, `src/kho/moKho.test.ts`

**Vì sao viết API thô thay vì dùng thư viện:** ràng buộc 2 và 3 đòi hỏi **kiểm soát chính xác ranh giới giao dịch** giữa nhiều kho con (bản ghi + hàng chờ + con trỏ). Các thư viện bọc IndexedDB thường ẩn giao dịch đi hoặc tự mở giao dịch mới cho mỗi lời gọi — đúng thứ làm hỏng hai ràng buộc quan trọng nhất. Viết thẳng API thô thì ranh giới nằm trong tay mình và đọc mã là thấy.

**Interfaces:**
- Produces: `moKho(): Promise<IDBDatabase>`; `PHIEN_BAN_KHO = 1`; các tên kho con: `"banGhi"`, `"hangCho"`, `"soDaNhan"`, `"conTro"`, `"canXemLai"`.

- [ ] **Step 1: Viết test trước**

```ts
describe('moKho', () => {
  it('tao du cac kho con', async () => { /* ... */ })

  it('kho MOI HON ma nguon dang chay thi NEM VersionError, tuyet doi khong xoa kho', async () => {
    // Người dùng mở lại bản cũ sau khi service worker đã chạy bản mới, hoặc hai tab lệch phiên
    // bản. Xoá-và-tạo-lại ở đây là mất trắng hàng chờ của cả tuần làm việc.
  })

  it('nang cap phien ban KHONG lam mat hang cho', async () => { /* ... */ })
})
```

- [ ] **Step 2: Viết `moKho.ts`** — `onupgradeneeded` chỉ **thêm** kho con, không bao giờ `deleteObjectStore` dữ liệu người dùng. Bắt `VersionError` và ném lại một lỗi có tên riêng để tầng trên hiện màn hình đúng.

- [ ] **Step 3: Chứng minh test biết báo lỗi** — tạm thêm `indexedDB.deleteDatabase()` vào nhánh lỗi, xác nhận test đỏ, hoàn nguyên.

- [ ] **Step 4: Commit.**

---

## Task 2: Hàng chờ và ràng buộc "một giao dịch"

**Files:** `src/kho/hangCho.ts`, `src/kho/khoDuLieu.ts`, `src/kho/conTro.ts`, test tương ứng

**Đây là task quan trọng nhất về an toàn dữ liệu của cả kế hoạch.** Ràng buộc 2: nếu ghi bản ghi vào kho hiển thị và thêm dòng hàng chờ là **hai** giao dịch, thì mất điện xen giữa tạo ra trạng thái tệ nhất có thể — dữ liệu **hiện trên màn hình**, thanh trạng thái **xanh**, nhưng không ai gửi nó lên và nó biến mất ở lần nhận về kế tiếp.

**Interfaces:**
- Produces: `ghiVaXepHang(kho, banGhi, thaoTac): Promise<void>` — **một** giao dịch bao cả hai; `docHangCho(kho, gioiHan)`; `xoaKhoiHangCho(kho, maThaoTac[])`; `demHangCho(kho)`.

- [ ] **Step 1: Viết test — trong đó có một fact mô phỏng đứt giữa chừng**

```ts
it('ghi ban ghi va xep hang la MOT giao dich — hong giua chung thi ca hai cung khong co', async () => {
  // Ép giao dịch abort giữa hai thao tác, khẳng định kho bản ghi cũng không có gì.
  // Nếu ai đó tách thành hai giao dịch, test này đỏ.
})
```

- [ ] **Step 2-4:** Viết `hangCho.ts`/`khoDuLieu.ts` → chạy xanh → **chứng minh test biết báo lỗi** bằng cách tạm tách thành hai giao dịch → hoàn nguyên → commit.

---

## Task 3: Sổ đã nhận

**Files:** `src/kho/soDaNhan.ts` + test

Spec 4.8.2. Máy con giữ **bản sao các dòng `hieu_luc` đã áp**, kèm `epoch` và `so_thu_tu` gốc, giữ **30 ngày**.

**Vì sao không chỉ giữ kết quả:** để bù lại dữ liệu máy chủ đã mất, máy con phải trả lời được "tôi đã nhận **những dòng nào** trong khoảng máy chủ đánh rơi". Trạng thái bản ghi hiện tại không nói được điều đó.

**Interfaces:** `ghiSoDaNhan(kho, dong[])`; `docTheoKhoang(kho, epoch, tuSoThuTu)`; `donSoDaNhanCu(kho, truocNgay)`.

- [ ] **Step 1-4:** test trước (gồm một fact khẳng định dọn 30 ngày **không** đụng dòng mới hơn) → viết → xanh → commit.

---

## Task 4: Đồng hồ máy con

**Files:** `src/dongbo/dongHoMayCon.ts` + test

Spec 4.3. Ba lớp:

1. **Đồng hồ đơn điệu** — mỗi thao tác lưu `(mốc neo lần đồng bộ gần nhất, số ms đã trôi theo `performance.now()`)`. `performance.now()` không bị đồng hồ hệ thống ảnh hưởng.
2. **Đoạn đồng hồ** — theo dõi chênh lệch giữa `Date.now()` và đồng hồ đơn điệu; phát hiện nhảy thì mở một **đoạn** mới trong hàng chờ. Mỗi đoạn hiệu chỉnh bằng một độ lệch riêng.
3. **Đồng hồ logic** — nâng lên khi nhận dòng có mốc lớn hơn.

**Kịch bản hỏng nếu làm sai** (spec 4.3): máy phòng xứ chạy lệch +3 ngày nhiều năm; ngày 1/9 mất mạng; ngày 5/9 ai đó chỉnh lại đồng hồ về đúng; ngày 10/9 nối mạng gửi lô. Nếu dùng **một** độ lệch đo lúc gửi cho cả lô, các mốc ghi trước lúc chỉnh sẽ bị dịch sai hướng và **âm thầm đè lên dữ liệu đúng** của máy khác.

- [ ] **Step 1:** Viết test, gồm fact tái hiện đúng kịch bản trên. → Step 2-4 như thường lệ.

---

## Task 5: Bầu chủ một tab

**Files:** `src/dongbo/bauChu.ts` + test

Ràng buộc 7. Quý sơ mở hai ba tab là chuyện thường (`banNhap.ts` đã ghi nhận vấn đề nhiều thẻ). Hai tab cùng chạy bộ đồng bộ sẽ **chạy đua ghi con trỏ**: tab A ghi "đã nhận tới 4830" trong khi tab B mới áp tới 4825 → **bỏ sót dòng 4826-4830**, vi phạm ràng buộc 1.

Dùng `navigator.locks.request` với khoá đặt tên; tab không giành được khoá chỉ đọc kho và nghe `BroadcastChannel`.

- [ ] **Step 1-4:** test (gồm fact hai "tab" giả cùng chạy, chỉ một cái thực sự làm việc) → viết → xanh → commit.

---

## Task 6: Bộ đồng bộ

**Files:** `src/dongbo/boDongBo.ts` + test

Ghép Task 1-5 thành vòng chạy thật, gọi ba đầu vào của kế hoạch 4.

**Lịch chạy (spec 7.3), bốn nguồn kích hoạt:**

1. **Mỗi lần gửi lên, máy chủ trả kèm dòng mới** — không tốn thêm lượt gọi. Hệ quả: ai đang nhập liệu thì dữ liệu luôn mới, miễn phí.
2. **Mỗi lần quay lại phần mềm** — `visibilitychange`, `focus`.
3. **Ngay khi có mạng trở lại** — sự kiện `online`.
4. **Định kỳ co giãn** — đang gõ thì 1 phút; ngồi yên vài phút thì 5 phút; **tab bị ẩn thì ngừng hỏi dữ liệu mới nhưng KHÔNG BAO GIỜ ngừng gửi hàng chờ** (quý sơ hay mở tab rồi để đó cả ngày).

Thêm một lần **đúng chỗ cần**: ngay trước khi mở một hồ sơ ra sửa, và trước khi dò trùng lúc tạo người mới.

**Ràng buộc 3** nằm ở đây: áp dòng nhận về + tiến con trỏ + xoá thao tác đã gửi khỏi hàng chờ là **một** giao dịch IndexedDB.

**Ràng buộc 9**: gửi lại thì dùng lại `MaThaoTac` cũ đã lưu trong hàng chờ, **không sinh mới**.

- [ ] **Step 1-5:** test (gồm fact "gửi lại lô không sinh bản ghi thứ hai", fact "tab ẩn vẫn gửi hàng chờ") → viết → xanh → **chứng minh test ràng buộc 3 biết báo lỗi** → commit.

---

## Task 7: Đường ghi luôn-lưu-vào-máy-trước

**Files:** `src/api/ghiCucBo.ts`, sửa `src/api/client.ts`, các màn hình ghi

Spec 7.1. **Không phân nhánh theo tình trạng mạng.** Ba lý do:

1. `navigator.onLine` chỉ biết máy có nối wifi hay không — vẫn báo "có mạng" khi đường truyền đã chết.
2. Mất mạng giữa chừng là trạng thái tệ nhất: máy chủ đã nhận nhưng chưa kịp trả lời, trình duyệt không biết đã lưu được chưa, người dùng bấm lại thì sinh bản trùng.
3. Hai nhánh là hai đường code phải kiểm thử, và nhánh ít chạy sẽ mục dần.

**Chế độ tắt offline (spec 6.6):** cùng một đường ghi, chỉ khác **cái hộp đựng** — lớp lưu trữ có hai bản cài đặt, một ghi xuống IndexedDB, một giữ trong RAM. Toàn bộ phần khó (gộp, nhật ký, hộp xem lại) dùng chung, không rẽ nhánh. Chế độ tắt phải **chặn đóng tab khi hàng chờ chưa rỗng** và không cho nhập tiếp khi mất mạng.

- [ ] **Step 1-5:** test → viết → chuyển từng màn hình ghi sang đường mới → chạy toàn bộ `npm test` → commit.

---

## Task 8: Bù lại sau khi máy chủ được khôi phục

**Files:** `src/dongbo/buSauKhoiPhuc.ts` + test

**Đọc spec mục 4.8 đầy đủ trước khi viết.** Đây là lý do tồn tại của cả hướng offline-first.

**Phát hiện — hai lớp:**
- Lớp 1: `epoch` từ máy chủ khác `epoch` đang giữ.
- Lớp 2 (lưới an toàn): con trỏ của máy con **lớn hơn** số lớn nhất máy chủ báo mà `epoch` **vẫn khớp** — chuyện không bao giờ xảy ra khi vận hành bình thường. Máy con **dừng đồng bộ**, chuyển 🔴, báo người hỗ trợ, **không** tự bù.

**Bù lại (chỉ khi máy chủ báo chế độ `lay_lai`):**
1. Lọc trong sổ đã nhận những dòng thuộc `epoch` cũ có `so_thu_tu >` số lớn nhất hiện tại của máy chủ.
2. Gửi lên như thao tác thường nhưng mang **danh tính gốc** (`NguonGocEpoch` + `NguonGocSoThuTu`) và **giữ nguyên mốc đồng hồ gốc** — đó là sự thật lịch sử, không phải thay đổi mới.
3. Chế độ `bo_han`: **không** gửi, nhưng cũng **không xoá trắng** — gói phần bị bỏ vào file dự phòng (Task 9) trước.

- [ ] **Step 1: Viết test — bốn fact**

```ts
it('phat hien qua epoch doi', async () => {})
it('phat hien qua so_thu_tu di lui khi epoch van khop — va KHONG tu bu', async () => {})
it('che do lay_lai gui dung nhung dong may chu da mat, mang danh tinh goc', async () => {})
it('che do bo_han khong gui gi nhung phai luu file du phong truoc khi bo', async () => {})
```

- [ ] **Step 2-5:** viết → xanh → **chứng minh fact lớp 2 biết báo lỗi** (tạm cho nó tự bù, xác nhận đỏ) → commit.

---

## Task 9: File dự phòng

**Files:** `src/dongbo/tepDuPhong.ts` + test

Spec 7.10. Hàng chờ nằm **trong trình duyệt**, và trình duyệt mất dữ liệu vì những lý do không ai lường: hết dung lượng đĩa thì trình duyệt tự dọn; "cháu biết máy tính" chạy CCleaner; cài lại Windows; đổi máy; Safari/iOS xoá dữ liệu website sau 7 ngày không dùng nếu PWA chưa cài vào màn hình chính.

Khi hàng chờ tồn đọng quá **2 ngày** hoặc **20 việc**, tự tải một file nhỏ xuống thư mục Tải về:

> Để cho chắc, phần mềm vừa lưu một bản dự phòng những việc chưa gửi vào thư mục Tải về của máy. Nếu máy có chuyện gì, gửi file đó cho người hỗ trợ là lấy lại được.

Kèm nút **"Nạp lại file dự phòng"** trong bảng trạng thái.

- [ ] **Step 1-4:** test (gồm fact "nạp lại file không tạo bản trùng" — nhờ `MaThaoTac` giữ nguyên) → viết → xanh → commit.

---

## Task 10: Thanh trạng thái, hộp cần xem lại, trang bàn giao

**Files:** `src/components/ThanhTrangThai.tsx`, `src/screens/CanXemLaiPage.tsx`, `src/screens/BanGiaoMayPage.tsx` + test

**Thanh trạng thái (spec 9.1)** — một dòng, một chấm màu, luôn nhìn thấy:

| Màu | Dòng chữ | Nghĩa |
|---|---|---|
| 🟢 | **Đã lưu an toàn** | Đã lên máy chủ và **máy chủ đã xác nhận** |
| 🟡 | **Đã lưu ở máy này (3) — đang gửi về** | Có việc chưa gửi được, hệ thống đang tự thử lại |
| 🔴 | **Cần xem lại (1)** | Có việc hệ thống không tự quyết được |

Vàng là trạng thái **bình thường** ở giáo xứ mất mạng — không phải lỗi. Câu chữ đặt vế trấn an lên trước: "Đã lưu ở máy này" chứ không phải "Đang chờ lưu", vì với quý cha quý sơ "chờ lưu" nghĩa là **chưa lưu**, tức là có thể mất — ngược hẳn điều thiết kế muốn nói.

**Quy tắc cứng:** không bao giờ hiện "Đã lưu an toàn" khi thay đổi còn nằm trong máy.

Bấm vào → bảng giải thích bằng lời thường: *"Những thay đổi này đã lưu trong máy và sẽ tự gửi lên khi có mạng. Anh/chị không cần làm gì, cũng không mất dữ liệu."* Bên dưới: danh sách việc đang chờ, nút "Nạp lại file dự phòng", trang "Bàn giao máy này", và mục **"Thông tin kỹ thuật"** gập lại kèm nút **"Sao chép để gửi người hỗ trợ"**.

**Hộp cần xem lại (spec 9.2)** — mỗi mục là một câu hỏi đời thường:

> **Ngày sinh của Maria Nguyễn Thị A**
> Máy phòng khách ghi **12/03/1985** (hôm qua). Máy văn phòng ghi **12/03/1986** (sáng nay).
> Hiện đang dùng: **12/03/1986**
> [ Giữ 12/03/1986 ] [ Đổi lại thành 12/03/1985 ]

**Trang "Bàn giao máy này"**: liệt kê số việc chưa gửi và số mục cần xem lại; chỉ khi **cả hai bằng 0** mới hiện nút xanh *"Đã gửi về hết. Có thể gỡ máy này ra an toàn."*

- [ ] **Step 1-4:** test (gồm fact "không bao giờ hiện xanh khi hàng chờ chưa rỗng") → viết → xanh → commit.

---

## Task 11: Kiểm thử trên trình duyệt thật

**Files:** không sửa mã. Chạy, quan sát, ghi lại.

Dựng môi trường riêng (API cổng 5299, Vite 5288 — **không** đụng 5096/5173 của phiên khác), đăng nhập một giáo xứ thử, chạy đủ các ca sau bằng trình duyệt thật. Dùng công cụ điều khiển trình duyệt để **ngắt mạng thật** (`Network.emulateNetworkConditions` hoặc tắt máy chủ), không giả lập bằng cách gọi hàm nội bộ.

| # | Ca kiểm thử | Kỳ vọng |
|---|---|---|
| 1 | Sửa một giáo dân **khi đang online** | Lưu ngay, thanh trạng thái 🟢 sau một nhịp |
| 2 | **Ngắt mạng**, sửa ba giáo dân | Cả ba lưu được, màn hình hiện đúng, thanh 🟡 "Đã lưu ở máy này (3)" |
| 3 | **Tải lại trang khi vẫn đang mất mạng** | Không bị đá ra đăng nhập; ba thay đổi vẫn còn; vẫn 🟡 (3) |
| 4 | **Nối mạng lại** | Tự gửi, không cần bấm gì, thanh về 🟢, CSDL có đủ ba thay đổi |
| 5 | **Tắt hẳn máy chủ**, sửa hai bản ghi, bật lại máy chủ | Như ca 4 — phân biệt "mất mạng" với "máy chủ chết" |
| 6 | Tìm kiếm giáo dân **khi mất mạng** | Vẫn tìm được, gõ tới đâu hiện tới đó |
| 7 | Tạo giáo dân mới khi mất mạng rồi chọn người đó vào một gia đình | Chọn được ngay dù cả hai chưa lên máy chủ |
| 8 | Hai tab cùng mở, sửa ở tab A | Tab B thấy thay đổi; **chỉ một tab** thực sự gọi máy chủ |
| 9 | **Hai máy sửa hai ô khác nhau** của cùng một giáo dân | Gộp tự động, cả hai thay đổi cùng còn |
| 10 | **Hai máy sửa cùng ô nhạy cảm** (ngày sinh) | Giá trị mới hơn được dùng, sinh **đúng một** mục cần xem lại |
| 11 | Chọn giá trị cũ hơn trong hộp cần xem lại | Giá trị đó thắng và **không bị tự đổi lại** ở lần đồng bộ sau |
| 12 | **Khôi phục máy chủ về bản cũ** (chế độ `lay_lai`), máy con còn giữ dữ liệu mới hơn | Máy con tự bù, dữ liệu quay lại đầy đủ, thanh trạng thái nói rõ đang làm gì |
| 13 | **Khôi phục ở chế độ `bo_han`** | Máy con **không** đẩy ngược lên; file dự phòng được tạo trước khi bỏ |
| 14 | Khôi phục CSDL bằng tay **quên xoay `epoch`** | Máy con phát hiện `so_thu_tu` đi lùi, **dừng đồng bộ**, chuyển 🔴 |
| 15 | Hàng chờ tồn đọng quá ngưỡng | File dự phòng tự tải xuống; nạp lại được và không sinh bản trùng |
| 16 | Đóng tab khi hàng chờ chưa rỗng (chế độ **tắt** offline) | Bị chặn kèm cảnh báo rõ ràng |
| 17 | Các màn hình chính (danh sách, chi tiết, thống kê, in ấn) | Không hồi quy |

Ca **12, 13, 14** là lý do tồn tại của cả thiết kế — làm kỹ nhất, ghi lại từng bước và từng câu SQL kiểm chứng.

- [ ] **Step 1:** Dựng môi trường, chạy đủ 17 ca, ghi kết quả từng ca.
- [ ] **Step 2:** Dọn sạch dữ liệu test, hoàn nguyên mọi thứ đã sửa, dừng các tiến trình đã khởi động.
- [ ] **Step 3:** Ghi kết quả vào cuối tài liệu thiết kế (mục "Đã kiểm chứng") và commit.

---

## Ngoài phạm vi kế hoạch này

| Việc | Kế hoạch |
|---|---|
| Xoá mềm toàn diện — tới khi có, đồng bộ vẫn chưa mang được thao tác xoá | 2 |
| Bảng `thiet_bi`, vé dài hạn, cờ offline phía máy chủ (ở đây chỉ có phần trình duyệt đọc cờ) | 3 |
| Mã hoá kho dữ liệu trong máy | Không làm — xem spec 6.6, không giải được bài toán |
| Kênh đẩy thời gian thực (WebSocket/SSE) | Không làm — hỏi định kỳ co giãn là đủ |
| Nhập từ desktop | 6 |
