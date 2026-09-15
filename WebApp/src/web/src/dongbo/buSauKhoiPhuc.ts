/**
 * Bù lại dữ liệu sau khi máy chủ được khôi phục — Task 8, spec mục 4.8.5. Đây là LÝ DO TỒN TẠI
 * của cả hướng offline-first (spec 4.8 mở đầu): máy chủ hỏng, được khôi phục từ bản sao lưu cũ
 * hơn — một khoảng dữ liệu ĐÃ GỬI LÊN VÀ ĐÃ ĐƯỢC MÁY CHỦ NHẬN (loại C, spec 4.8.1) biến mất khỏi
 * máy chủ, nhưng máy con VẪN CÒN GIỮ (trong sổ đã nhận, `kho/soDaNhan.ts`, Task 3). Không làm gì
 * thì lần đồng bộ kế tiếp máy con sẽ kéo trạng thái (đã thiếu) của máy chủ về và TỰ GHI ĐÈ lên
 * bản tốt của chính nó — mất vĩnh viễn, im lặng, không một dấu hiệu nào báo trước. Module này là
 * thứ duy nhất ngăn điều đó.
 *
 * ## Vì sao máy con KHÔNG THỂ biết trước "lấy lại" hay "bỏ hẳn" (spec 4.8.3)
 *
 * Quản trị viên chọn một trong hai chế độ khi xoay `epoch` lúc khôi phục: `lay_lai` (máy chủ vừa
 * gặp sự cố, muốn lấy lại dữ liệu loại C) hoặc `bo_han` (đang cố ý quay lui, KHÔNG muốn máy con
 * đẩy dữ liệu bị bỏ trở lại). Cờ này (`ChoPhepBuLai` phía máy chủ) CHỈ được đọc và kiểm tra Ở
 * PHÍA MÁY CHỦ (`DongBoService.cs`) — không lộ ra qua bất kỳ DTO/endpoint nào cho máy con. Máy
 * con chỉ biết được kết quả SAU KHI thử gửi (`"ap"` nếu `lay_lai`, `"tu_choi"` nếu `bo_han`, xem
 * `KetQuaThaoTacDto.ketQua` — nhưng module này còn chưa gửi gì, chỉ NẠP vào hàng chờ, kết quả đó
 * sẽ tới ở chu kỳ đồng bộ bình thường kế tiếp).
 *
 * RULING (điều phối viên, xem `task-8-brief.md`): vì không biết trước, module này LUÔN sao lưu
 * vào file dự phòng (Task 9, `tepDuPhong.ts`) TRƯỚC KHI nạp vào hàng chờ — không phải "chỉ khi
 * biết là `bo_han`". Nếu sau đó máy chủ chấp nhận (`lay_lai`), file dự phòng chỉ là một bản sao
 * thừa vô hại. Nếu máy chủ từ chối (`bo_han`), file đó CHÍNH XÁC là thứ spec 4.8.3 yêu cầu:
 * "không xoá trắng" — quản trị viên vẫn còn đường lấy lại thủ công nếu quyết định quay lui là
 * sai. Kết quả `"tu_choi"` của riêng ca này KHÔNG tạo mục "cần xem lại" phía máy chủ (khác với
 * mọi `"tu_choi"` khác — `DongBoService.cs` dòng ~446-448: "đây không phải một việc cần người
 * xem lại, đây là hành vi ĐÚNG Ý theo lựa chọn của quản trị viên"), nên việc CÓ file dự phòng là
 * lớp an toàn dữ liệu duy nhất còn lại cho trường hợp `bo_han` — không phải một sơ suất của Task
 * 6 khi nó không tạo hộp cần xem lại cho ca này.
 *
 * ## Trình tự (spec 4.8.5 + đối chiếu `DongBoService.cs`/`KhoiPhucDongBoService.cs`)
 *
 * Gọi khi bắt được `LoiEpochKhongKhop` (Task 6, LỚP 1 — spec 4.8.4) từ vòng đồng bộ:
 *
 * 1. Đọc `conTro.epoch` HIỆN TẠI (`docConTro`) — đây là "epoch_cũ", PHẢI đọc TRƯỚC khi bất kỳ
 *    bước nào khác có thể ghi đè `conTro`.
 * 2. Gọi `taiToanBoVaGiaiNen()` (Task 6, đã có sẵn) lấy `{ epoch: epoch_mới, conTro: soLonNhat }`
 *    — `soLonNhat` là "số lớn nhất hiện tại của máy chủ" (server KHÔNG reset `so_thu_tu` khi xoay
 *    epoch, chỉ đổi danh tính chuỗi — số này tiếp nối đúng từ thời điểm phục hồi).
 * 3. Lọc sổ đã nhận (`docTheoKhoang(kho, epoch_cũ, soLonNhat)`, Task 3) — CHÍNH XÁC các dòng
 *    `epoch_cũ` có `so_thu_tu > soLonNhat` là phần máy chủ đã mất mà máy này còn giữ.
 * 4. Không có dòng nào: chỉ cần đưa `conTro` sang `epoch_mới`/`soLonNhat` rồi dừng — không có gì
 *    để bù (Task 6 chưa có `apDungToanBo` để tự làm việc này, xem `boDongBo.ts` cuối file — module
 *    này là nơi đầu tiên thực sự gọi `taiToanBoVaGiaiNen()`, nên phải tự cập nhật `conTro`).
 * 5. Có dòng: dựng `DongHangChoDongBo[]` từ mỗi `DongDaNhan`, GIỮ NGUYÊN `dongHoVatLy`→`vatLy` và
 *    `dongHoLogic`→`logic` (KHÔNG qua `DongHoLogicMayCon.phatDau`/`donDieu.mocHienTaiMs()` — đây
 *    là sự thật lịch sử, không phải thao tác mới), gán `nguonGocEpoch`/`nguonGocSoThuTu`, sinh
 *    `maThaoTac`/`giaoDichId` MỚI (một lần gửi MỚI về giao thức, dù nội dung là lịch sử cũ — sổ
 *    chống trùng phía máy chủ dựa vào `(NguonGocEpoch, NguonGocSoThuTu)` cho ca này, KHÔNG dựa
 *    vào `MaThaoTac`, xem `DongBoService.cs` dòng ~404-406 và ~766-768).
 * 6. LUÔN gọi `taiFileDuPhongXuong` với đúng các dòng vừa dựng ở bước 5, TRƯỚC bước 7 (xem Ruling
 *    ở trên).
 * 7. Nạp các dòng vào hàng chờ bằng `napLaiFileDuPhong` (Task 9, TÁI SỬ DỤNG NGUYÊN VẸN — nó chỉ
 *    ghi vào kho `hangCho`, không đụng `banGhi`, đúng ý cần ở đây vì bản ghi hiển thị của các
 *    thay đổi này ĐÃ CÓ SẴN từ trước, không cần ghi lại). Vòng đồng bộ bình thường (Task 6) sẽ tự
 *    gửi chúng ở chu kỳ kế tiếp — module này KHÔNG tự gọi mạng để gửi lên.
 * 8. Trả về `{ soDongDaBu }` để tầng gọi log/hiển thị "Đã gửi lại N thay đổi" (spec 4.8.6).
 *
 * ## LỚP 2 (spec 4.8.4) — lưới an toàn quan trọng HƠN việc bù lại
 *
 * Con trỏ máy con LỚN HƠN số lớn nhất máy chủ báo mà `epoch` VẪN KHỚP là chuyện không bao giờ
 * xảy ra khi vận hành bình thường — Task 6 đã tự phát hiện qua 409 "may-chu-di-lui" và DỪNG HẲN
 * vòng đồng bộ (`trangThai() === 'dung_do_may_chu_di_lui'`, xem `boDongBo.ts`). Module này TUYỆT
 * ĐỐI không được tự chạy trong tình huống đó: gửi dữ liệu lên một máy chủ đang trong tình trạng
 * chưa rõ ràng là đúng thứ LỚP 2 sinh ra để ngăn.
 *
 * QUYẾT ĐỊNH THIẾT KẾ (chưa chốt trong brief — brief chỉ đưa ra chữ ký `xuLyEpochKhongKhop(
 * phuThuoc)` một tham số): `PhuThuocBoDongBo` (Task 6) KHÔNG mang theo `trangThai()` — đó là một
 * closure cục bộ bên trong `batDauBoDongBo`, không lộ ra qua `PhuThuocBoDongBo`. Sửa lại hình
 * dạng `PhuThuocBoDongBo` (thêm một tham chiếu `DieuKhienBoDongBo`) là sửa một file đã đóng
 * (Task 6) chỉ để phục vụ một lệnh gọi hàm — không cần thiết. Thay vào đó, `xuLyEpochKhongKhop`
 * nhận thêm một tham số THỨ HAI TUỲ CHỌN `layTrangThai` (mặc định coi như "đang chạy bình
 * thường" nếu không truyền — giữ đúng chữ ký MỘT tham số mà brief mô tả cho lời gọi thường ngày),
 * để tầng tích hợp (nơi thực sự giữ `DieuKhienBoDongBo.trangThai`) truyền vào khi nối dây thật.
 * Đây là chỗ BẮT BUỘC phải có theo brief (fact test 2), nên không thể bỏ qua hoàn toàn.
 */
import { docConTro, ghiConTro } from '../kho/conTro'
import { docTheoKhoang, type DongDaNhan } from '../kho/soDaNhan'
import { taiToanBoVaGiaiNen, type DongHangChoDongBo, type PhuThuocBoDongBo, type TrangThaiBoDongBo } from './boDongBo'
import { napLaiFileDuPhong, taiFileDuPhongXuong } from './tepDuPhong'

/**
 * Dựng một dòng hàng chờ từ một dòng đã nhận (sổ đã nhận, Task 3) — dùng cho bước 5.
 *
 * `vatLy`/`logic` GIỮ NGUYÊN từ `dong.dongHoVatLy`/`dong.dongHoLogic` — KHÔNG đóng dấu lại bằng
 * giờ hiện tại: đóng dấu lại sẽ biến một sự thật lịch sử cũ thành "vừa sửa xong", khiến nó thắng
 * SAI so với những thay đổi thật đã xảy ra ở máy chủ SAU khi dữ liệu này bị mất (spec 4.8.5,
 * "Xung đột với người đã sửa sau khi khôi phục" — máy chủ còn kẹp thêm `min(mốc, giờ máy chủ)`
 * để chặn một mốc bịa ở tương lai, nhưng đó là việc của máy chủ, không phải của máy con).
 */
function dongDaNhanThanhHangCho(dong: DongDaNhan, nguonGocEpoch: string): DongHangChoDongBo {
  return {
    maThaoTac: crypto.randomUUID(),
    // Mặc định của `DongHangCho` (xem `hangCho.ts`) — dòng bù lại mang GIỜ GỐC LỊCH SỬ trong
    // `vatLy`/`logic` bên dưới, không phải giờ hiện tại của máy này, nên số hiệu đoạn đồng hồ
    // (dùng để hiệu chỉnh ĐỘ LỆCH giờ hiện tại, spec 4.3) không có ý nghĩa gì với một dấu quá khứ
    // đã đúng từ trước — không cần và không nên gán số đoạn hiện tại của máy này vào đây.
    doan: 0,
    // Mọi dòng `hieu_luc` là kết quả TRIỂN KHAI mức trường phía máy chủ (spec 4.1: "tao" được
    // khai triển thành N dòng "sua" ngay khi máy chủ nhận) — nên gửi lại luôn là "sua", kể cả nếu
    // thao tác gốc từng là "tao".
    loai: 'sua',
    bang: dong.bang,
    banGhiId: dong.banGhiId,
    truong: dong.truong,
    giaTri: dong.giaTri,
    vatLy: dong.dongHoVatLy,
    logic: dong.dongHoLogic,
    // Sinh MỚI — đây LÀ một lần gửi mới về mặt giao thức, dù nội dung là lịch sử cũ. Chống trùng
    // giữa nhiều máy con cùng bù lại một dòng dựa vào `nguonGocEpoch`/`nguonGocSoThuTu` phía dưới
    // (xem `DongBoService.cs` dòng ~404-406, ~766-768), KHÔNG dựa vào `giaoDichId` này.
    giaoDichId: crypto.randomUUID(),
    nguonGocEpoch,
    nguonGocSoThuTu: dong.soThuTu,
  }
}

/**
 * Gọi khi bắt được `LoiEpochKhongKhop` từ vòng đồng bộ (Task 6) — thực hiện toàn bộ quy trình
 * spec 4.8.5. Trả về số dòng đã bù (để log/hiển thị "Đã gửi lại N thay đổi", spec 4.8.6).
 *
 * `layTrangThai`: xem "QUYẾT ĐỊNH THIẾT KẾ" ở đầu file — mặc định coi như đang chạy bình thường
 * nếu không truyền, để lời gọi với đúng MỘT tham số `phuThuoc` (brief) vẫn hợp lệ.
 */
export async function xuLyEpochKhongKhop(
  phuThuoc: PhuThuocBoDongBo,
  layTrangThai: () => TrangThaiBoDongBo = () => 'dang_chay',
): Promise<{ soDongDaBu: number }> {
  // LỚP 2 (spec 4.8.4) — kiểm tra TRƯỚC bất kỳ lời gọi mạng/đọc kho nào (xem chú thích đầu file).
  if (layTrangThai() === 'dung_do_may_chu_di_lui') {
    throw new Error(
      'Dong bo dang tam dung vi may chu co dau hieu vua bi dua ve ban cu (LOP 2, spec 4.8.4) — ' +
        'khong tu bu lai du lieu. Can nguoi ho tro kiem tra truoc khi tiep tuc.',
    )
  }

  // BƯỚC 1 (spec 4.8.5 bước 1): đọc epoch HIỆN TẠI trước khi bất kỳ bước nào khác có thể ghi đè
  // `conTro` — đây là "epoch_cũ", danh tính của chuỗi mà sổ đã nhận (Task 3) đang dùng để lọc.
  const conTroCu = await docConTro(phuThuoc.kho)
  const epochCu = conTroCu.epoch

  // BƯỚC 2: tải lại toàn bộ trạng thái MỚI của máy chủ (Task 6, đã có sẵn — `taiToanBoVaGiaiNen`
  // CHƯA từng được gọi ở bất kỳ đâu khác trong mã đã đóng, nên KHÔNG có ai khác ghi `conTro` xen
  // giữa bước 1 và đây — việc đọc epoch_cũ TRƯỚC vẫn là thói quen đúng, phòng khi thứ tự này bị
  // đổi sau này). `conTro` trong kết quả trả về chính là "số lớn nhất hiện tại của máy chủ".
  const ketQuaToanBo = await taiToanBoVaGiaiNen()
  const epochMoi = ketQuaToanBo.epoch
  const soLonNhat = ketQuaToanBo.conTro

  // BƯỚC 3: lọc sổ đã nhận — CHÍNH XÁC các dòng epoch_cũ có so_thu_tu > soLonNhat là phần máy chủ
  // đã mất mà máy này còn giữ. `epochCu === null` nghĩa là máy này CHƯA từng đồng bộ lần nào —
  // không có sổ đã nhận nào thuộc một epoch cụ thể để so (`docTheoKhoang` cần một epoch dạng
  // chuỗi) — xử lý an toàn bằng mảng rỗng thay vì giả định "chuyện đó không xảy ra".
  const dongCanBu: DongDaNhan[] =
    epochCu === null ? [] : await docTheoKhoang(phuThuoc.kho, epochCu, soLonNhat)

  // BƯỚC 4: không có gì để bù (hoặc chưa từng có epoch để so) — chỉ cần đưa `conTro` sang
  // epoch_mới/soLonNhat rồi dừng. Sau lệnh `if` này, TypeScript tự thu hẹp `epochCu` về `string`
  // (loại trừ `null`) cho phần còn lại của hàm — không cần ép kiểu thủ công.
  if (epochCu === null || dongCanBu.length === 0) {
    await ghiConTro(phuThuoc.kho, { epoch: epochMoi, soThuTu: soLonNhat })
    return { soDongDaBu: 0 }
  }

  // BƯỚC 5: dựng DongHangChoDongBo[] — GIỮ NGUYÊN vatLy/logic gốc, gán danh tính gốc.
  const hangChoBu = dongCanBu.map((dong) => dongDaNhanThanhHangCho(dong, epochCu))

  // BƯỚC 6: LUÔN sao lưu TRƯỚC khi thử nạp vào hàng chờ (xem Ruling ở đầu file) — máy con không
  // có cách nào biết trước cửa "cho phép bù lại" phía máy chủ đang mở hay đóng.
  taiFileDuPhongXuong(hangChoBu)

  // BƯỚC 7: nạp vào hàng chờ — TÁI SỬ DỤNG NGUYÊN VẸN `napLaiFileDuPhong` (Task 9). Vòng đồng bộ
  // bình thường (Task 6) sẽ tự gửi các dòng này ở chu kỳ kế tiếp — module này KHÔNG tự gọi mạng.
  await napLaiFileDuPhong(phuThuoc.kho, hangChoBu)

  // Cập nhật `conTro` sang epoch_mới/soLonNhat SAU KHI đã nạp an toàn vào hàng chờ, KHÔNG PHẢI
  // trước — quyết định thiết kế: nếu tiến trình bị ngắt (mất điện, đóng tab) GIỮA bước nạp và
  // bước này, `conTro` vẫn còn giữ epoch_cũ, nên lần đồng bộ sau máy con lại nhận 410 và CHẠY LẠI
  // đúng quy trình này — an toàn nhờ máy chủ chống trùng theo danh tính gốc (xem
  // `dongDaNhanThanhHangCho`), chỉ tốn thêm một lượt gửi thừa vô hại. NGƯỢC LẠI, cập nhật `conTro`
  // TRƯỚC khi nạp xong mà bị ngắt giữa chừng sẽ làm MẤT VĨNH VIỄN cơ hội bù lại: `conTro` đã sang
  // epoch_mới nên không còn 410 nào kích hoạt lại quy trình này nữa, trong khi hàng chờ có thể
  // chưa kịp nhận đủ (hoặc chưa nhận gì) — đúng loại mất dữ liệu im lặng mà toàn bộ mục 4.8 sinh
  // ra để ngăn.
  await ghiConTro(phuThuoc.kho, { epoch: epochMoi, soThuTu: soLonNhat })

  return { soDongDaBu: hangChoBu.length }
}
