/**
 * Ba lớp "đồng hồ máy con" — spec mục 4.3. Trạng thái của cả ba lớp được lưu CHUNG trong kho
 * `conTro` (Task 1) qua `docConTro`/`ghiConTro` (merge-patch từng phần — xem `kho/conTro.ts`),
 * KHÔNG tạo kho `IndexedDB` mới.
 *
 * ## Vì sao cần BA lớp, không phải một
 *
 * 1. `DongHoDonDieu` — trả lời "giờ hiện tại ước tính là bao nhiêu", dùng `performance.now()` làm
 *    nguồn đo ĐỘ TRÔI (không dùng lại để đọc giờ hệ thống): một khi đã neo (`neoLai`) vào một mốc
 *    đáng tin (ví dụ giờ máy chủ tại lần đồng bộ gần nhất), giá trị trả về sau đó KHÔNG hề nhảy dù
 *    ai đó chỉnh đồng hồ hệ thống, vì nó không đọc lại đồng hồ hệ thống nữa.
 * 2. `DoanDongHo` — trả lời "đoạn (segment) hiện tại là số mấy", bằng cách so đồng hồ HỆ THỐNG
 *    (`Date.now()`, có thể bị chỉnh) với dự đoán của `DongHoDonDieu` (không bị chỉnh): lệch nhau
 *    quá ngưỡng nghĩa là vừa có một cú "nhảy giờ" thật (admin sửa tay, NTP kéo mạnh) chứ không phải
 *    trôi tự nhiên. Mỗi lần phát hiện, mở một đoạn MỚI. Số đoạn này được tầng gọi (Task 6/7, xem
 *    `kho/hangCho.ts`) gắn vào TỪNG DÒNG hàng chờ lúc tạo ra nó — để khi gửi lô lên máy chủ, MỖI
 *    đoạn được hiệu chỉnh theo ĐỘ LỆCH RIÊNG của chính nó, không dùng một độ lệch chung cho cả lô
 *    (xem kịch bản hỏng trong brief: máy lệch +3 ngày nhiều năm, mất mạng, admin sửa giờ, rồi mới
 *    gửi lô — nếu dùng một độ lệch đo lúc gửi, các mốc ghi TRƯỚC lúc admin sửa sẽ bị dịch SAI
 *    HƯỚNG, âm thầm đè lên dữ liệu đúng của máy khác).
 * 3. `DongHoLogicMayCon` — bọc `nangDau` (Phần A) để duy trì "dấu cuối cùng máy này đã phát" giữa
 *    các lần gọi (kể cả qua các phiên/lần tải trang khác nhau, nhờ lưu vào `conTro`) — thiếu lớp
 *    này thì mỗi lần muốn phát một dấu mới lại phải tự tay truyền đúng `dauCuoiCuaTa`, rất dễ quên
 *    và gây ra chính lỗi mà Global Constraints đã cảnh báo (bản sửa của người dùng thua chính bản
 *    ghi nó vừa đọc).
 *
 * Phạm vi Task 4 (xem `task-4-brief.md`): CHỈ cung cấp logic tính "đoạn hiện tại là bao nhiêu" và
 * hai lớp đồng hồ còn lại. Việc GẮN số đoạn vào một dòng hàng chờ cụ thể, và việc ĐỌC LẠI `doLech`
 * của một đoạn đã đóng để hiệu chỉnh khi gửi lô, là việc của tầng gọi thực tế (Task 6/7).
 */
import { docConTro, ghiConTro } from '../kho/conTro'
import { GUID_RONG, mocSangMs, nangDau, tuMocMs, type DauDongHo } from './dauDongHo'

/**
 * Ngưỡng lệch (mili giây) để coi một chênh lệch giữa đồng hồ hệ thống và dự đoán của đồng hồ đơn
 * điệu là một cú "NHẢY GIỜ" thật, thay vì trôi tự nhiên của phần cứng (thạch anh của máy tính có
 * thể trôi vài chục tới vài trăm mili giây qua NHIỀU GIỜ, đó KHÔNG phải là một cú nhảy). 2 giây đủ
 * lớn để không hoảng vì trôi bình thường, đủ nhỏ để bắt được cả nhảy "vài giây" do NTP kéo lại, chứ
 * không chỉ nhảy "vài ngày"/"vài năm" như kịch bản admin sửa tay.
 */
export const NGUONG_NHAY_GIO_MS = 2000

/** Các hàm đọc đồng hồ — tách ra thành tham số để kiểm thử được TẤT ĐỊNH (không phụ thuộc
 * `Date.now()`/`performance.now()` thật, vốn không kiểm soát được trong test). Mặc định dùng đúng
 * API trình duyệt thật. */
export type NguonThoiGian = {
  /** Đồng hồ HỆ THỐNG — bị ảnh hưởng bởi việc admin sửa giờ, múi giờ, NTP... Mặc định `Date.now`. */
  gioHeThongMs: () => number
  /** Đồng hồ ĐƠN ĐIỆU của trình duyệt — KHÔNG bị ảnh hưởng bởi việc sửa giờ hệ thống, nhưng mốc 0
   * của nó đặt lại mỗi lần tải trang. Mặc định `performance.now`. */
  gioDonDieuMs: () => number
}

const NGUON_MAC_DINH: NguonThoiGian = {
  gioHeThongMs: () => Date.now(),
  gioDonDieuMs: () => performance.now(),
}

/**
 * Đồng hồ đơn điệu — neo một mốc "đáng tin" (`mocNeo`) vào một lần đọc `performance.now()`
 * (`msTaiNeo`), rồi từ đó tính "giờ hiện tại ước tính" bằng cách CỘNG THÊM độ trôi đo bằng
 * `performance.now()` — TUYỆT ĐỐI không đọc lại đồng hồ hệ thống sau khi đã neo. Nhờ vậy, nếu ai đó
 * (admin, NTP) chỉnh đồng hồ hệ thống SAU khi neo, giá trị đồng hồ này không hề nhảy: nó chỉ biết
 * "đã trôi bao lâu thật" qua `performance.now()`, độc lập với đồng hồ hệ thống.
 *
 * Nhược điểm CỐ HỮU (không phải lỗi, không tránh được): mốc 0 của `performance.now()` đặt lại mỗi
 * lần tải trang, nên khi mở lại app sau khi đóng, lớp này phải neo LẠI ở lần khởi tạo kế tiếp —
 * khoảng thời gian thực đã trôi qua giữa lúc đóng và mở lại app không đo được qua lớp này (đó là
 * việc `DoanDongHo` — lớp 2 — phát hiện và ghi nhận, không phải việc lớp này sửa cho đúng).
 */
export class DongHoDonDieu {
  private readonly kho: IDBDatabase
  private readonly nguon: NguonThoiGian
  private mocNeoMs: number
  private msTaiNeo: number

  private constructor(kho: IDBDatabase, nguon: NguonThoiGian, mocNeoMsBanDau: number, msTaiNeoBanDau: number) {
    this.kho = kho
    this.nguon = nguon
    this.mocNeoMs = mocNeoMsBanDau
    this.msTaiNeo = msTaiNeoBanDau
  }

  /**
   * Khởi tạo, đọc mốc neo đã lưu từ lần trước (nếu có) trong `conTro`. Nếu máy này CHƯA từng neo
   * lần nào (chưa từng đồng bộ), dùng tạm đồng hồ hệ thống làm mốc khởi điểm — mốc này có thể sai
   * (máy lệch giờ), nhưng nó sẽ được thay bằng một mốc đáng tin ngay khi `neoLai` được gọi lần đầu
   * (lần đồng bộ đầu tiên).
   *
   * LUÔN đặt `msTaiNeo` về `performance.now()` hiện tại của phiên MỚI này, dù `mocNeo` đọc được có
   * thể đến từ phiên trước — vì mốc 0 của `performance.now()` khác nhau giữa các phiên, giữ nguyên
   * `msTaiNeo` cũ (nếu có lưu) sẽ tính sai độ trôi.
   */
  static async khoiTao(kho: IDBDatabase, nguon: NguonThoiGian = NGUON_MAC_DINH): Promise<DongHoDonDieu> {
    const conTro = await docConTro(kho)
    const mocNeoLuu = conTro.mocNeo
    const mocNeoMs = typeof mocNeoLuu === 'string' ? mocSangMs(mocNeoLuu) : nguon.gioHeThongMs()
    const msTaiNeo = nguon.gioDonDieuMs()
    return new DongHoDonDieu(kho, nguon, mocNeoMs, msTaiNeo)
  }

  /** Giờ hiện tại ƯỚC TÍNH (mili giây từ epoch), tính từ mốc neo cộng độ trôi đo bằng
   * `performance.now()` — KHÔNG đọc lại đồng hồ hệ thống. */
  mocHienTaiMs(): number {
    return this.mocNeoMs + (this.nguon.gioDonDieuMs() - this.msTaiNeo)
  }

  /**
   * Neo LẠI mốc — gọi khi vừa có một nguồn giờ ĐÁNG TIN xác nhận (ví dụ: máy chủ vừa trả lời một
   * yêu cầu, biết chắc giờ máy chủ tại thời điểm đó). Ghi mốc mới xuống `conTro` để phiên sau còn
   * dùng lại được (dù `performance.now()` của phiên sau sẽ phải neo lại `msTaiNeo`, xem `khoiTao`).
   */
  async neoLai(mocMoiMs: number): Promise<void> {
    this.mocNeoMs = mocMoiMs
    this.msTaiNeo = this.nguon.gioDonDieuMs()
    await ghiConTro(this.kho, { mocNeo: tuMocMs(this.mocNeoMs) })
  }
}

/**
 * "Đoạn" đồng hồ — spec 4.3. Mỗi lần phát hiện đồng hồ HỆ THỐNG (`Date.now()`) lệch khỏi dự đoán
 * của `DongHoDonDieu` quá `NGUONG_NHAY_GIO_MS`, coi đó là một cú "nhảy giờ" THẬT (admin sửa tay,
 * NTP kéo mạnh, hoặc lần đầu khởi động sau khi đóng app lâu ngày) và MỞ MỘT ĐOẠN MỚI: tăng
 * `doanHienTai`, ghi lại `doLechDoanHienTai` = độ lệch đo được tại thời điểm phát hiện.
 *
 * Lớp này KHÔNG tự sửa gì cả — chỉ ĐÁNH DẤU. Việc gắn đúng số đoạn vào từng dòng hàng chờ lúc tạo
 * ra nó, và việc hiệu chỉnh MỖI đoạn theo độ lệch RIÊNG của nó khi gửi lô lên máy chủ, là việc của
 * tầng gọi (Task 6/7) — xem chú thích đầu file.
 */
export class DoanDongHo {
  private readonly kho: IDBDatabase
  private readonly donDieu: DongHoDonDieu
  private readonly nguon: NguonThoiGian
  private doanHienTaiSo: number
  private doLechHienTai: number

  private constructor(
    kho: IDBDatabase,
    donDieu: DongHoDonDieu,
    nguon: NguonThoiGian,
    doanBanDau: number,
    doLechBanDau: number,
  ) {
    this.kho = kho
    this.donDieu = donDieu
    this.nguon = nguon
    this.doanHienTaiSo = doanBanDau
    this.doLechHienTai = doLechBanDau
  }

  static async khoiTao(kho: IDBDatabase, donDieu: DongHoDonDieu, nguon: NguonThoiGian = NGUON_MAC_DINH): Promise<DoanDongHo> {
    const conTro = await docConTro(kho)
    const doan = typeof conTro.doanHienTai === 'number' ? conTro.doanHienTai : 0
    const doLech = typeof conTro.doLechDoanHienTai === 'number' ? conTro.doLechDoanHienTai : 0
    return new DoanDongHo(kho, donDieu, nguon, doan, doLech)
  }

  /**
   * Kiểm tra xem đồng hồ hệ thống có vừa nhảy so với dự đoán của đồng hồ đơn điệu hay không; nếu
   * có, MỞ ĐOẠN MỚI (tăng số đoạn, ghi lại độ lệch mới, lưu xuống `conTro`). Trả về số đoạn hiện
   * tại SAU khi đã kiểm tra (đoạn cũ nếu không có nhảy, đoạn mới nếu vừa mở).
   *
   * GỌI HÀM NÀY trước khi gắn số đoạn vào một dòng hàng chờ mới (Task 6/7) — đó là lý do hàm này
   * bất đồng bộ (ghi `conTro` khi phát hiện nhảy) dù phần lớn lần gọi (không có nhảy) chỉ đọc.
   *
   * QUAN TRỌNG: khi phát hiện nhảy, hàm này NEO LẠI `DongHoDonDieu` vào đúng giờ hệ thống vừa đọc
   * được (`thuc`). Thiếu bước này, lần gọi KẾ TIẾP sẽ so `donDieu.mocHienTaiMs()` (vẫn tính theo
   * mốc neo CŨ, chưa hề biết cú nhảy vừa xảy ra) với giờ hệ thống (đã ở mức mới) và phát hiện lại
   * ĐÚNG cú nhảy đó lần nữa — mở đoạn mới liên tục dù không có nhảy nào thêm.
   */
  async soDoanHienTai(): Promise<number> {
    const duKien = this.donDieu.mocHienTaiMs()
    const thuc = this.nguon.gioHeThongMs()
    const lech = thuc - duKien
    if (Math.abs(lech) > NGUONG_NHAY_GIO_MS) {
      this.doanHienTaiSo += 1
      this.doLechHienTai = lech
      await this.donDieu.neoLai(thuc)
      await ghiConTro(this.kho, { doanHienTai: this.doanHienTaiSo, doLechDoanHienTai: this.doLechHienTai })
    }
    return this.doanHienTaiSo
  }

  /** Độ lệch (giờ hệ thống trừ giờ ước tính từ đồng hồ đơn điệu, mili giây) của đoạn HIỆN TẠI —
   * dương nghĩa là đồng hồ hệ thống hiện ĐI NHANH hơn dự đoán. Dùng khi tầng gọi cần hiệu chỉnh một
   * mốc vừa ghi bằng giờ hệ thống thô về gần đúng giờ đáng tin của đoạn đó. */
  doLechDoanHienTai(): number {
    return this.doLechHienTai
  }
}

/**
 * Đồng hồ logic của máy này — bọc `nangDau` (Phần A), tự quản lý "dấu cuối cùng ta đã phát" bằng
 * cách đọc/ghi `conTro`, để việc đó không bị quên ở nơi gọi.
 *
 * GỌI `phatDau` MỖI LẦN máy này cần một dấu MỚI: khi tự ghi một thao tác của chính người dùng
 * (`nhanDuoc = null`), HOẶC — BẮT BUỘC, đúng như Global Constraints đã cảnh báo — khi vừa NHẬN một
 * dòng `hieu_luc` từ máy chủ (`nhanDuoc` = dấu của dòng đó) trước khi cho phép người dùng sửa lại
 * chính bản ghi đó. Bỏ qua vế sau là đúng lỗi đã nêu: bản sửa của người dùng thua chính bản ghi nó
 * vừa đọc.
 */
export class DongHoLogicMayCon {
  private readonly kho: IDBDatabase
  private readonly thietBiId: string
  private cuoiCung: DauDongHo

  private constructor(kho: IDBDatabase, thietBiId: string, cuoiCung: DauDongHo) {
    this.kho = kho
    this.thietBiId = thietBiId
    this.cuoiCung = cuoiCung
  }

  /** Khởi tạo, đọc "dấu cuối cùng đã phát" từ lần trước (nếu có) trong `conTro`. Nếu máy này CHƯA
   * từng phát dấu nào, dùng mốc `epoch` Unix (`tuMocMs(0)`) làm điểm khởi đầu — bất kỳ mốc thật
   * nào (luôn ở tương lai so với 1970) sẽ tự nhiên lớn hơn nó, nên lần phát đầu tiên luôn "tiến". */
  static async khoiTao(kho: IDBDatabase, thietBiId: string): Promise<DongHoLogicMayCon> {
    const conTro = await docConTro(kho)
    const cuoiCung: DauDongHo =
      typeof conTro.dauCuoiVatLy === 'string' && typeof conTro.dauCuoiLogic === 'number'
        ? {
            vatLy: conTro.dauCuoiVatLy,
            logic: conTro.dauCuoiLogic,
            thietBiId: typeof conTro.dauCuoiThietBiId === 'string' ? conTro.dauCuoiThietBiId : null,
            maThaoTac: typeof conTro.dauCuoiMaThaoTac === 'string' ? conTro.dauCuoiMaThaoTac : GUID_RONG,
          }
        : { vatLy: tuMocMs(0), logic: 0, thietBiId: null, maThaoTac: GUID_RONG }
    return new DongHoLogicMayCon(kho, thietBiId, cuoiCung)
  }

  /**
   * Sinh dấu kế tiếp mà máy này sẽ PHÁT RA, và LƯU LẠI làm "dấu cuối cùng" cho lần gọi sau (cả
   * trong phiên này lẫn phiên sau, qua `conTro`).
   *
   * @param gioHienTaiMs Giờ hiện tại (mili giây) — nên lấy từ `DongHoDonDieu.mocHienTaiMs()`, KHÔNG
   *   phải `Date.now()` thô, để không bị ảnh hưởng bởi việc chỉnh đồng hồ hệ thống.
   * @param nhanDuoc Dấu vừa đọc được (dòng `hieu_luc`) nếu có, `null` nếu đây là lần phát nội bộ.
   * @param maThaoTacMoi Mã thao tác (do máy con sinh — spec mục 5 ràng buộc 9) của chính dấu sắp
   *   phát.
   */
  async phatDau(gioHienTaiMs: number, nhanDuoc: DauDongHo | null, maThaoTacMoi: string): Promise<DauDongHo> {
    const ket = nangDau(this.cuoiCung, nhanDuoc, tuMocMs(gioHienTaiMs))
    const dauMoi: DauDongHo = { vatLy: ket.vatLy, logic: ket.logic, thietBiId: this.thietBiId, maThaoTac: maThaoTacMoi }
    this.cuoiCung = dauMoi
    await ghiConTro(this.kho, {
      dauCuoiVatLy: dauMoi.vatLy,
      dauCuoiLogic: dauMoi.logic,
      dauCuoiThietBiId: dauMoi.thietBiId,
      dauCuoiMaThaoTac: dauMoi.maThaoTac,
    })
    return dauMoi
  }
}
