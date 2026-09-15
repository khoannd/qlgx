/**
 * `ghiCucBo` — đường ghi luôn-lưu-vào-máy-trước (spec 7.1, Task 7). ĐIỂM GHI mà mọi màn hình sửa dữ
 * liệu PHẢI đi qua thay vì gọi thẳng `api.xxx.yyy()` (POST/PUT/PATCH) của `client.ts`.
 *
 * ## KHÔNG PHÂN NHÁNH theo tình trạng mạng (spec 7.1)
 *
 * `ghiCucBo` LUÔN ghi xuống kho cục bộ trước (`ghiVaXepHang`/`ghiVaXepHangNhieuTruong`, Task 2) rồi
 * trả về ngay — KHÔNG chờ mạng, KHÔNG kiểm tra `navigator.onLine`. Bộ đồng bộ (Task 6, `boDongBo.ts`)
 * tự gửi hàng chờ lên máy chủ khi có dịp (vòng lặp định kỳ + bốn nguồn kích hoạt spec 7.3). Ba lý do
 * (xem brief task-7-brief.md mục Spec 7.1, đã trích trong `task-7-report.md`): `navigator.onLine`
 * không đáng tin, mất mạng giữa chừng là trạng thái tệ nhất, và hai nhánh mạng có/không là hai đường
 * code phải bảo trì trong khi nhánh ít chạy sẽ mục dần.
 *
 * ## Vì sao KHÔNG dùng `client.ts`'s `goi()`
 *
 * `goi()` (và mọi hàm `api.xxx.yyy()` xây trên nó) gọi THẲNG máy chủ qua `fetch`, đợi phản hồi rồi
 * mới coi là "đã lưu" — đúng ý muốn CHẶN của kế hoạch offline-first. `ghiCucBo` không gọi `goi()`
 * (cũng không tự `fetch` gì cả): việc gửi lên máy chủ hoàn toàn là việc CỦA BỘ ĐỒNG BỘ, tách khỏi
 * hành động của người dùng.
 *
 * ## Một hay nhiều dòng hàng chờ cho một lần lưu?
 *
 * `DongHangChoDongBo` (Task 6) mang `truong`/`giaTri` SỐ ÍT — một dòng hàng chờ ứng với ĐÚNG MỘT ô
 * (bang, banGhiId, truong), khớp với cách máy chủ áp thao tác động theo tên thuộc tính entity EF
 * Core (`ApThaoTac.ApMotO`, xem `DongBoService.cs`) — máy chủ hợp nhất (LWW) TỪNG Ô ĐỘC LẬP, không
 * theo cả bản ghi. Một lần lưu màn hình đổi NHIỀU trường cùng lúc do đó cần NHIỀU dòng hàng chờ.
 *
 * Ghi N dòng qua N lần gọi `ghiVaXepHang` RIÊNG (N giao dịch IndexedDB tách biệt) sẽ mở lại đúng cửa
 * sổ nguy hiểm mà chú thích đầu `khoDuLieu.ts` cảnh báo: mất điện giữa hai lần gọi để lại bản ghi
 * hiển thị đã có ĐỦ N thay đổi trong khi hàng chờ mới có MỘT phần — phần còn lại (đã hiện trên màn
 * hình, thanh trạng thái đã xanh) biến mất im lặng, không ai gửi lên máy chủ. Vì vậy khi có từ hai
 * trường trở lên, `ghiCucBo` mở MỘT giao dịch IndexedDB DUY NHẤT bao cả `banGhi.put()` lẫn TẤT CẢ N
 * lần `hangCho.add()` — dùng lại đúng khuôn mẫu (`voiKhoaKeTiep`, `add()` thay vì `put()`) mà
 * `ghiVaXepHang`/`hangCho.ts` đã dùng và đã kiểm thử kỹ (xem `khoDuLieu.test.ts`). Trường hợp CHỈ MỘT
 * trường (phổ biến nhất) gọi THẲNG `ghiVaXepHang` (Task 2) — đúng nghĩa đen ràng buộc 1 của brief.
 */
import { KHO_BAN_GHI, KHO_HANG_CHO } from '../kho/moKho'
import { voiKhoaKeTiep } from '../kho/hangCho'
import { ghiVaXepHang, type BanGhiKho } from '../kho/khoDuLieu'
import type { DongHangChoDongBo, PhuThuocBoDongBo } from '../dongbo/boDongBo'

/** Một trường đã đổi trong một lần ghi — `giaTri` LUÔN là chuỗi đã serialize (đúng hợp đồng
 * `ThaoTacDto.giaTri`/`ApThaoTac` phía máy chủ: mọi kiểu dữ liệu, kể cả số/ngày/JSON lồng, đi qua
 * dây dưới dạng chuỗi) hoặc `null` (xoá giá trị ô đó) — KHÔNG truyền số/boolean/object trần. Việc
 * serialize đúng định dạng cho từng kiểu cột (ví dụ ngày tháng ISO, số dùng `Intl`-bất biến...) là
 * trách nhiệm của MÀN HÌNH gọi `ghiCucBo`, vì chỉ nó biết kiểu thật của cột đang sửa. */
export type TruongDaDoi = { truong: string; giaTri: string | null }

export type ThamSoGhiCucBo = {
  /** Loại thao tác — đúng chuỗi `ThaoTacDto.loai` phía máy chủ mong đợi (ví dụ "tao"/"sua"/"xoa"),
   * do màn hình gọi quyết định theo đúng ngữ nghĩa hành động của nó. */
  loai: string
  bang: string
  banGhiId: string
  /** Ít nhất MỘT trường đã đổi — mảng RỖNG là lỗi gọi (không có gì để ghi), ném lỗi ngay thay vì
   * âm thầm không làm gì (fail-loud, tránh một màn hình tưởng đã lưu nhưng thực ra ghiCucBo không
   * ghi được dòng hàng chờ nào). */
  truong: TruongDaDoi[]
  /** Bản ghi ĐẦY ĐỦ sau khi áp dụng TẤT CẢ thay đổi trong `truong` — ghi vào kho hiển thị (`banGhi`,
   * Task 1) trong CÙNG giao dịch với mọi dòng hàng chờ (xem chú thích đầu file). */
  banGhi: BanGhiKho
}

/**
 * Ghi N (`>= 2`) dòng hàng chờ CÙNG `banGhi.put()` trong MỘT giao dịch IndexedDB duy nhất — bản
 * tổng quát hoá của `ghiVaXepHang` (Task 2) cho nhiều thao tác cùng lúc, dùng lại NGUYÊN VẸN khuôn
 * mẫu `voiKhoaKeTiep`/`add()` mà Task 2 đã dựng và kiểm thử (xem chú thích đầu file). CHỈ dùng nội bộ
 * — trường hợp N === 1 gọi thẳng `ghiVaXepHang` (xem `ghiCucBo` bên dưới).
 */
function ghiNhieuVaoHangCho(kho: IDBDatabase, banGhi: BanGhiKho, dsThaoTac: DongHangChoDongBo[]): Promise<void> {
  return new Promise((resolve, reject) => {
    const gd = kho.transaction([KHO_BAN_GHI, KHO_HANG_CHO], 'readwrite')
    const khoHangCho = gd.objectStore(KHO_HANG_CHO)

    gd.objectStore(KHO_BAN_GHI).put(banGhi.giaTri, banGhi.khoa)

    // Tìm khoá kế tiếp MỘT LẦN rồi tự tăng dần cho N dòng — an toàn vì các khoá `khoaKeTiep..
    // khoaKeTiep+N-1` chắc chắn CHƯA có trong kho (chưa ai khác ghi thêm được vào GIỮA một giao dịch
    // đang mở), và `add()` (không phải `put()`) vẫn ném lỗi rõ ràng nếu giả định này vì lý do gì đó
    // sai — bảo vệ dữ liệu thay vì âm thầm ghi đè mất một việc đang chờ gửi (cùng lý do Task 2 chọn
    // `add()` trong `khoDuLieu.ts`).
    voiKhoaKeTiep(
      khoHangCho,
      (khoaKeTiep) => {
        dsThaoTac.forEach((thaoTac, i) => khoHangCho.add(thaoTac, khoaKeTiep + i))
      },
      () => {},
    )

    gd.oncomplete = () => resolve()
    gd.onerror = () => reject(gd.error ?? new Error('Không ghi được bản ghi và xếp hàng chờ (nhiều trường)'))
    gd.onabort = () =>
      reject(gd.error ?? new Error('Giao dịch ghi nhiều trường vào hàng chờ bị huỷ giữa chừng — không gì được ghi'))
  })
}

/**
 * Điểm ghi cục bộ DUY NHẤT mà màn hình gọi. Xem chú thích đầu file cho toàn bộ lý do thiết kế.
 *
 * Các bước, ĐÚNG THỨ TỰ:
 * 1. Xác định `doan` (đoạn đồng hồ hiện tại) qua `phuThuoc.doan.soDoanHienTai()` — TUYỆT ĐỐI không
 *    tự đoán/mặc định 0, để hàng chờ luôn mang đúng số đoạn tại lúc ghi (ràng buộc 2, brief — cần
 *    cho `guiHangChoVaApDung` nhóm và hiệu chỉnh đúng khi gửi lại, xem `boDongBo.ts`).
 * 2. Sinh MỘT dấu đồng hồ lai (`vatLy`/`logic`) DUY NHẤT cho CẢ LẦN LƯU qua
 *    `phuThuoc.dongHoLogic.phatDau(phuThuoc.donDieu.mocHienTaiMs(), null, giaoDichId)` — dùng
 *    `donDieu.mocHienTaiMs()` (đồng hồ đơn điệu, Task 4), KHÔNG phải `Date.now()` thô (ràng buộc 2).
 *    MỌI dòng hàng chờ của lần lưu này dùng CHUNG dấu này: chúng thuộc các Ô KHÁC NHAU (khác
 *    `truong`) nên không tranh chấp LWW với nhau — coi cả lần lưu là MỘT sự kiện logic duy nhất tại
 *    một thời điểm là đúng bản chất "người dùng bấm Lưu một lần" (không có lý do gì trường thứ hai
 *    ghi "muộn hơn" trường thứ nhất trong cùng một submit).
 * 3. Dùng `giaoDichId` (một GUID mới, hoặc mã dấu ở bước 2) làm tầng phá hoà cuối cùng của DẤU — ĐÚNG
 *    quy ước mà `dauTuDongHieuLuc` (`boDongBo.ts`) dùng khi dựng lại `DauDongHo` từ một dòng hiệu lực
 *    nhận về (dùng `giaoDichId` thay cho `maThaoTac` — xem chú thích ở đó): giữ đồng hồ logic của máy
 *    này nhất quán giữa "cái mình vừa phát" và "cái mình sẽ đọc lại y hệt khi máy chủ trả về".
 * 4. Ghi bản ghi hiển thị + TẤT CẢ dòng hàng chờ trong MỘT giao dịch IndexedDB (xem
 *    `ghiNhieuVaoHangCho`/`ghiVaXepHang`).
 */
export async function ghiCucBo(kho: IDBDatabase, phuThuoc: PhuThuocBoDongBo, tso: ThamSoGhiCucBo): Promise<void> {
  if (tso.truong.length === 0) {
    throw new Error('ghiCucBo: khong co truong nao de ghi (mang truong rong) — loi goi, kiem tra lai man hinh gui len.')
  }

  const doan = await phuThuoc.doan.soDoanHienTai()
  const giaoDichId = crypto.randomUUID()
  const dau = await phuThuoc.dongHoLogic.phatDau(phuThuoc.donDieu.mocHienTaiMs(), null, giaoDichId)

  const dsThaoTac: DongHangChoDongBo[] = tso.truong.map((t) => ({
    maThaoTac: crypto.randomUUID(),
    doan,
    loai: tso.loai,
    bang: tso.bang,
    banGhiId: tso.banGhiId,
    truong: t.truong,
    giaTri: t.giaTri,
    vatLy: dau.vatLy,
    logic: dau.logic,
    giaoDichId,
  }))

  if (dsThaoTac.length === 1) {
    // Trường hợp phổ biến nhất — dùng THẲNG `ghiVaXepHang` (Task 2), đúng nghĩa đen ràng buộc 1.
    await ghiVaXepHang(kho, tso.banGhi, dsThaoTac[0])
    return
  }

  await ghiNhieuVaoHangCho(kho, tso.banGhi, dsThaoTac)
}
