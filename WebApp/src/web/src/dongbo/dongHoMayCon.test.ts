import 'fake-indexeddb/auto'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { docConTro } from '../kho/conTro'
import { moKho } from '../kho/moKho'
import { GUID_RONG, soSanh, tuMocMs } from './dauDongHo'
import { DoanDongHo, DongHoDonDieu, DongHoLogicMayCon, NGUONG_NHAY_GIO_MS, type NguonThoiGian } from './dongHoMayCon'

// Mỗi ca kiểm thử dùng một tên kho RIÊNG — cùng khuôn mẫu với moKho.test.ts (Task 1) — để không lẫn
// trạng thái đồng hồ giữa các ca.
let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-dong-ho-${demTen}`
}

/** Đồng hồ giả LẬP TRÌNH ĐƯỢC: `heThong` là biến có thể gán lại tuỳ ý (mô phỏng admin sửa giờ),
 * `donDieu` chỉ tăng khi gọi `troiQua` (mô phỏng performance.now() — không bao giờ tự nhảy). */
function dungNguonGia(heThongBanDau: number): NguonThoiGian & { heThong: number; troiQua: (ms: number) => void } {
  const trangThai = {
    heThong: heThongBanDau,
    donDieu: 0,
    gioHeThongMs: () => trangThai.heThong,
    gioDonDieuMs: () => trangThai.donDieu,
    troiQua: (ms: number) => {
      trangThai.donDieu += ms
      trangThai.heThong += ms
    },
  }
  return trangThai
}

describe('DongHoDonDieu (Task 4 Phần B, lớp 1) — không bị anh huong boi viec chinh dong ho he thong', () => {
  afterEach(() => vi.restoreAllMocks())

  it('sau khi neo, chinh dong ho HE THONG khong lam mocHienTaiMs nhay', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)

    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)

    // "Admin" chinh dong ho he thong nhay vot +3 ngay - KHONG di qua troiQua (khong phai thoi gian
    // thuc troi), chi doi truc tiep bien heThong.
    nguon.heThong += 3 * 24 * 60 * 60 * 1000

    // performance.now() (donDieu) van dung yen - chua co thoi gian thuc nao troi qua.
    expect(donDieu.mocHienTaiMs()).toBe(1_000_000)

    kho.close()
  })

  it('mocHienTaiMs tien dung theo do troi cua performance.now(), khong phai theo dong ho he thong', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)

    nguon.troiQua(5000) // 5 giay troi qua THAT (ca hai dong ho cung tien, dung kich ban binh thuong)

    expect(donDieu.mocHienTaiMs()).toBe(1_005_000)
    kho.close()
  })

  it('khoi tao lai (mo phong tai trang) doc lai mocNeo da luu trong conTro', async () => {
    const tenKho = tenKhoRieng()
    const kho1 = await moKho(tenKho)
    const nguon1 = dungNguonGia(1_000_000)
    const donDieu1 = await DongHoDonDieu.khoiTao(kho1, nguon1)
    await donDieu1.neoLai(1_000_000)
    kho1.close()

    // "Tai lai trang": mo ket noi kho MOI, dung mot nguon thoi gian MOI (performance.now() da ve
    // lai 0 o phien moi) nhung conTro van con mocNeo cu.
    const kho2 = await moKho(tenKho)
    const nguon2 = dungNguonGia(1_000_000) // gioHeThongMs khong duoc dung lai luc khoi tao neu da co mocNeo
    const donDieu2 = await DongHoDonDieu.khoiTao(kho2, nguon2)

    expect(donDieu2.mocHienTaiMs()).toBe(1_000_000)
    kho2.close()
  })
})

describe('DoanDongHo (Task 4 Phần B, lớp 2) — phat hien nhay gio, mo doan moi', () => {
  it('khong nhay thi doan khong doi', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)
    const doan = await DoanDongHo.khoiTao(kho, donDieu, nguon)

    expect(await doan.soDoanHienTai()).toBe(0)

    nguon.troiQua(500) // troi that su, ca hai dong ho cung tien - khong phai nhay
    expect(await doan.soDoanHienTai()).toBe(0)
    expect(doan.doLechDoanHienTai()).toBe(0)

    kho.close()
  })

  it('nhay vuot nguong thi mo doan moi va ghi lai do lech', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)
    const doan = await DoanDongHo.khoiTao(kho, donDieu, nguon)
    expect(await doan.soDoanHienTai()).toBe(0)

    // Dong ho he thong bi chinh nhay +1 gio, KHONG qua troiQua (khong phai thoi gian thuc).
    nguon.heThong += 60 * 60 * 1000
    expect(await doan.soDoanHienTai()).toBe(1)
    expect(doan.doLechDoanHienTai()).toBe(60 * 60 * 1000)

    // Goi lai lan nua (khong nhay them) - van giu nguyen doan 1, khong tang nua.
    expect(await doan.soDoanHienTai()).toBe(1)

    kho.close()
  })

  it('nhay duoi nguong (troi tu nhien cua phan cung) KHONG duoc coi la nhay', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)
    const doan = await DoanDongHo.khoiTao(kho, donDieu, nguon)

    nguon.heThong += NGUONG_NHAY_GIO_MS - 1
    expect(await doan.soDoanHienTai()).toBe(0)

    kho.close()
  })

  it('kich ban hong trong brief: may lech nhieu nam, mat mang, admin sua gio, roi moi gui lo — HAI doan PHAI khac nhau, khong dung MOT do lech chung', async () => {
    // Máy phòng xứ chạy lệch +3 NGÀY nhiều năm (chưa từng đồng bộ lần nào — dùng thẳng đồng hồ hệ
    // thống sai làm mốc neo ban đầu, đúng như DongHoDonDieu.khoiTao mô tả).
    const goc = Date.UTC(2026, 8, 1, 0, 0, 0) // 1/9 00:00 UTC, theo dong ho SAI (da lech +3 ngay)
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(goc)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon) // chua tung dong bo -> neo = gio he thong (sai)
    const doan = await DoanDongHo.khoiTao(kho, donDieu, nguon)

    // Ngay 1/9: mot thao tac duoc "ghi" (dai dien boi viec doc doan hien tai) - chua mat mang gi
    // ca, dong ho van sai nhu cu nhung khong AI phat hien vi khong co gi de so sanh.
    const doanNgay1 = await doan.soDoanHienTai()
    expect(doanNgay1).toBe(0)

    // Mat mang tu 1/9 den 5/9 - khong dong bo duoc, dong ho van tiep tuc troi (van sai +3 ngay,
    // nhung THOI GIAN THAT van troi qua binh thuong ca hai phia).
    nguon.troiQua(4 * 24 * 60 * 60 * 1000) // 4 ngay troi that su

    // Ngay 5/9: admin phat hien va SUA LAI dong ho he thong ve dung (tru di 3 ngay bi lech) - day
    // la mot cu NHAY GIO THAT, khong phai thoi gian troi qua.
    nguon.heThong -= 3 * 24 * 60 * 60 * 1000

    const doanNgay5 = await doan.soDoanHienTai()
    expect(doanNgay5).not.toBe(doanNgay1) // PHAI mo doan moi - day la yeu cau cot loi
    expect(doanNgay5).toBe(1)
    const doLechDoan1 = doan.doLechDoanHienTai()
    expect(doLechDoan1).toBeLessThan(0) // dong ho he thong vua bi KEO LUI so voi du kien

    // Mat mang tiep den 10/9 - lan nay dong ho DA DUNG, khong con nhay nua.
    nguon.troiQua(5 * 24 * 60 * 60 * 1000)
    const doanNgay10 = await doan.soDoanHienTai()
    expect(doanNgay10).toBe(1) // van la doan 1 - khong co nhay moi

    // Diem mau chot cua kich ban hong: cac dong hang cho ghi TRUOC ngay 5/9 duoc gan doanNgay1 (0),
    // cac dong ghi SAU duoc gan doanNgay5/doanNgay10 (1). Hai so nay KHAC NHAU, nen tang goi (Task
    // 6/7) hoan toan co the tra cuu do lech RIENG cho tung doan khi gui lo - khong bi ep dung CHUNG
    // mot do lech (do lech do luc gui, tuc luc doanNgay10) cho ca cac dong cu thuoc doanNgay1.
    expect(doanNgay1).not.toBe(doanNgay10)

    kho.close()
  })

  it('doLechDangTinCay: nhay o LAN KIEM TRA DAU TIEN (sau khi tai trang) la KHONG dang tin - co the chi la thoi gian app dong, khong phai nhay gio that', async () => {
    // Kich ban: app dong 3 NGAY (khong he co cu nhay gio nao, dong ho van dung), roi mo lai. Vi
    // performance.now() dat lai mot ve 0 moi phien, DoanDongHo.khoiTao/soDoanHienTai() KHONG THE
    // phan biet duoc "3 ngay da troi qua that su trong luc dong app" voi "dong ho vua bi nhay 3
    // ngay" — ca hai deu cho ra dung mot con so lech. Neu tang goi (Task 6/7) coi day la mot do
    // lech dang tin roi tu dong hieu chinh du lieu, no se dich sai cac moc vua ghi that di 3 ngay.
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000) // lan dong bo cuoi cung truoc khi dong app

    // App dong 3 ngay — KHONG mo phong bang troiQua (vi performance.now() se dat lai ve 0 khi mo
    // lai trang, khong con lien tuc voi phien nay nua). Thay vao do, dung mot NGUON MOI (mo phong
    // phien moi sau khi tai trang) voi gio he thong da nhay +3 ngay so voi luc dong (chinh la dieu
    // xay ra that trong doi song: thoi gian THAT da troi qua trong luc app dong).
    const baNgayMs = 3 * 24 * 60 * 60 * 1000
    const nguonPhienMoi = dungNguonGia(1_000_000 + baNgayMs) // gio he thong: da qua 3 ngay that su
    const donDieuPhienMoi = await DongHoDonDieu.khoiTao(kho, nguonPhienMoi) // doc lai mocNeo=1_000_000 tu conTro
    const doan = await DoanDongHo.khoiTao(kho, donDieuPhienMoi, nguonPhienMoi)

    const soDoan = await doan.soDoanHienTai()
    expect(soDoan).toBe(1) // van mo doan moi - dung, vi khong biet chac day khong phai nhay that
    expect(doan.doLechDangTinCay()).toBe(false) // NHUNG khong duoc coi la dang tin

    kho.close()
  })

  it('doLechDangTinCay: nhay o lan kiem tra THU HAI tro di (cung mot phien, dong ho don dieu van dang chay lien tuc) LA dang tin', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)
    const doan = await DoanDongHo.khoiTao(kho, donDieu, nguon)

    // Lan kiem tra dau tien: khong nhay gi ca (dung nhu binh thuong ngay sau khi neo).
    expect(await doan.soDoanHienTai()).toBe(0)

    // Lan kiem tra THU HAI, VAN TRONG CUNG PHIEN (dong ho don dieu lien tuc chay, khong tai lai
    // trang) — admin sua gio nhay +1 gio. Day la mot cu nhay THAT SU quan sat duoc, khong phai do
    // performance.now() vua dat lai.
    nguon.heThong += 60 * 60 * 1000
    expect(await doan.soDoanHienTai()).toBe(1)
    expect(doan.doLechDangTinCay()).toBe(true)

    kho.close()
  })
})

describe('DongHoLogicMayCon (Task 4 Phần B, lớp 3) — boc nangDau, tu quan ly dau cuoi cung', () => {
  it('lan phat dau tien luon tien (khong nhan gi)', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const logic = await DongHoLogicMayCon.khoiTao(kho, 'thiet-bi-1')

    const gioHienTaiMs = Date.UTC(2026, 8, 13, 10, 0, 0)
    const dau1 = await logic.phatDau(gioHienTaiMs, null, '00000000-0000-0000-0000-000000000001')

    expect(dau1.vatLy).toBe(tuMocMs(gioHienTaiMs))
    expect(dau1.logic).toBe(0)
    expect(dau1.thietBiId).toBe('thiet-bi-1')
    kho.close()
  })

  it('hai lan phat lien tiep CUNG mot moc gio he thong van tien (logic tang), khong hoa', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const logic = await DongHoLogicMayCon.khoiTao(kho, 'thiet-bi-1')
    const gioHienTaiMs = Date.UTC(2026, 8, 13, 10, 0, 0)

    const dau1 = await logic.phatDau(gioHienTaiMs, null, '00000000-0000-0000-0000-000000000001')
    const dau2 = await logic.phatDau(gioHienTaiMs, null, '00000000-0000-0000-0000-000000000002')

    expect(soSanh(dau2, dau1)).toBeGreaterThan(0)
  })

  it('nhan mot dau tu may chu thi lan phat KE TIEP phai xep SAU dau vua nhan (khong bi dao nhan qua)', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const logic = await DongHoLogicMayCon.khoiTao(kho, 'thiet-bi-1')
    const gioHienTaiMs = Date.UTC(2026, 8, 13, 10, 0, 0)

    const dauNhan = {
      vatLy: tuMocMs(gioHienTaiMs + 60_000), // may chu o "tuong lai gan" so voi may con
      logic: 4,
      thietBiId: null,
      maThaoTac: '00000000-0000-0000-0000-0000000000ff',
    }

    // May con "doc" du lieu do may chu vua gui, nang dau theo dau da nhan (day chinh la buoc BAT
    // BUOC theo Global Constraints - bo qua no thi ban sua sau se thua ban da doc).
    const dauSauKhiNhan = await logic.phatDau(gioHienTaiMs, dauNhan, GUID_RONG)
    expect(soSanh(dauSauKhiNhan, dauNhan)).toBeGreaterThan(0)

    // Nguoi dung ngay lap tuc sua lai chinh ban ghi do (thao tac tiep theo, khong nhan gi moi).
    const dauSua = await logic.phatDau(gioHienTaiMs, null, '00000000-0000-0000-0000-000000000010')
    expect(soSanh(dauSua, dauNhan)).toBeGreaterThan(0)
    expect(soSanh(dauSua, dauSauKhiNhan)).toBeGreaterThan(0)
  })

  it('dong ho he thong bi chinh LUI giua hai lan phat van khong lam lui dau (ben vung qua reload)', async () => {
    const tenKho = tenKhoRieng()
    const kho1 = await moKho(tenKho)
    const logic1 = await DongHoLogicMayCon.khoiTao(kho1, 'thiet-bi-1')
    const goc = Date.UTC(2026, 8, 13, 10, 0, 0)
    const dau1 = await logic1.phatDau(goc, null, '00000000-0000-0000-0000-000000000001')
    kho1.close()

    // Mo phong tai trang: khoi tao lai tu CUNG mot kho, dong ho he thong luc nay bi LUI 30 giay
    // (admin sua tay giua chung).
    const kho2 = await moKho(tenKho)
    const logic2 = await DongHoLogicMayCon.khoiTao(kho2, 'thiet-bi-1')
    const dau2 = await logic2.phatDau(goc - 30_000, null, '00000000-0000-0000-0000-000000000002')

    expect(soSanh(dau2, dau1)).toBeGreaterThan(0)
    kho2.close()
  })

  it('conTro chi bi ghi de MOT PHAN — cac truong cua DoanDongHo/DongHoDonDieu khong bi DongHoLogicMayCon xoa mat', async () => {
    const tenKho = tenKhoRieng()
    const kho = await moKho(tenKho)
    const nguon = dungNguonGia(1_000_000)
    const donDieu = await DongHoDonDieu.khoiTao(kho, nguon)
    await donDieu.neoLai(1_000_000)
    const doanDongHo = await DoanDongHo.khoiTao(kho, donDieu, nguon)
    nguon.heThong += 60 * 60 * 1000 // ep mo mot doan moi de co gia tri khac 0/mac dinh
    await doanDongHo.soDoanHienTai() // day cung NEO LAI donDieu vao gio he thong moi (xem lop 2)

    const logic = await DongHoLogicMayCon.khoiTao(kho, 'thiet-bi-1')
    await logic.phatDau(Date.UTC(2026, 8, 13, 10, 0, 0), null, '00000000-0000-0000-0000-000000000001')

    const conTro = await docConTro(kho)
    // mocNeo phai la gia tri SAU khi DoanDongHo neo lai (1_000_000 + 1 gio), khong phai gia tri
    // neo dau tien - vi phat hien nhay da kich hoat neoLai() ben trong soDoanHienTai().
    expect(conTro.mocNeo).toBe(tuMocMs(1_000_000 + 60 * 60 * 1000))
    expect(conTro.doanHienTai).toBe(1)
    expect(typeof conTro.dauCuoiVatLy).toBe('string')

    kho.close()
  })
})
