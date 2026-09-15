// jsdom không cài IndexedDB — polyfill bằng fake-indexeddb (xem moKho.test.ts, cùng khuôn mẫu).
import 'fake-indexeddb/auto'
import { describe, expect, it } from 'vitest'
import { moKho } from './moKho'
import { ghiSoDaNhan, docTheoKhoang, donSoDaNhanCu, type DongDaNhan } from './soDaNhan'

let demTen = 0
function tenKhoRieng(): string {
  demTen += 1
  return `qlgx-test-soDaNhan-${demTen}`
}

describe('soDaNhan (Task 3 — so da nhan va don sach 30 ngay)', () => {
  it('kho rong thi doc theo khoang tra ve mang rong', async () => {
    const db = await moKho(tenKhoRieng())
    const ketQua = await docTheoKhoang(db, 'epoch-1', 0)
    expect(ketQua).toEqual([])
    db.close()
  })

  it('ghi so da nhan roi doc theo khoang tra ve dung cac dong co epoch khop va soThuTu > tuSoThuTu', async () => {
    const db = await moKho(tenKhoRieng())

    const dong1: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-1',
      truong: 'tenGiaoDan',
      giaTri: 'Nguyễn Văn A',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: 'dev-1',
      giaoDichId: 'gd-1',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    const dong2: DongDaNhan = {
      ...dong1,
      soThuTu: 20,
      banGhiId: 'id-2',
      giaoDichId: 'gd-2',
      ngayNhan: '2026-09-15T10:05:00Z',
    }

    const dong3: DongDaNhan = {
      ...dong1,
      soThuTu: 30,
      banGhiId: 'id-3',
      giaoDichId: 'gd-3',
      ngayNhan: '2026-09-15T10:10:00Z',
    }

    await ghiSoDaNhan(db, [dong1, dong2, dong3])

    // Doc theo epoch 'e1', tuSoThuTu = 15 — được dòng có soThuTu 20 và 30 (cả hai > 15)
    const ketQua = await docTheoKhoang(db, 'e1', 15)
    expect(ketQua.length).toBe(2)
    expect(ketQua[0].soThuTu).toBe(20)
    expect(ketQua[1].soThuTu).toBe(30)

    db.close()
  })

  it('docTheoKhoang chi lay dung epoch da chi, khong lay epoch khac', async () => {
    const db = await moKho(tenKhoRieng())

    const dong1: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-1',
      truong: 'tenGiaoDan',
      giaTri: 'A',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-1',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    const dong2: DongDaNhan = {
      ...dong1,
      epoch: 'e2',
      soThuTu: 5,
      banGhiId: 'id-2',
      giaoDichId: 'gd-2',
    }

    await ghiSoDaNhan(db, [dong1, dong2])

    // Doc epoch 'e1' — chi la dong1
    const ketQua1 = await docTheoKhoang(db, 'e1', 0)
    expect(ketQua1).toHaveLength(1)
    expect(ketQua1[0].epoch).toBe('e1')
    expect(ketQua1[0].soThuTu).toBe(10)

    // Doc epoch 'e2' — chi la dong2
    const ketQua2 = await docTheoKhoang(db, 'e2', 0)
    expect(ketQua2).toHaveLength(1)
    expect(ketQua2[0].epoch).toBe('e2')
    expect(ketQua2[0].soThuTu).toBe(5)

    db.close()
  })

  it('docTheoKhoang tra ve theo thu tu SO HOC cua soThuTu tang dan, khong phai thu tu khoa CHUOI', async () => {
    const db = await moKho(tenKhoRieng())

    const dong1: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 100,
      bang: 'giaoDan',
      banGhiId: 'id-100',
      truong: 'tenGiaoDan',
      giaTri: 'C',
      dongHoVatLy: '2026-09-15T10:30:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-100',
      ngayNhan: '2026-09-15T10:30:00Z',
    }

    const dong2: DongDaNhan = {
      ...dong1,
      soThuTu: 9,
      banGhiId: 'id-9',
      giaoDichId: 'gd-9',
      giaTri: 'A',
      dongHoVatLy: '2026-09-15T10:10:00Z',
      ngayNhan: '2026-09-15T10:10:00Z',
    }

    const dong3: DongDaNhan = {
      ...dong1,
      soThuTu: 10,
      banGhiId: 'id-10',
      giaoDichId: 'gd-10',
      giaTri: 'B',
      dongHoVatLy: '2026-09-15T10:20:00Z',
      ngayNhan: '2026-09-15T10:20:00Z',
    }

    // Ghi theo thứ tự 100, 9, 10 — khoá CHUỖI tương ứng "e1:100" < "e1:10" < "e1:9" (so từ điển),
    // NGƯỢC HẲN thứ tự số học 9 < 10 < 100. Nếu docTheoKhoang chỉ trả về đúng thứ tự cursor duyệt
    // (thứ tự khoá), test này phải đỏ.
    await ghiSoDaNhan(db, [dong1, dong2, dong3])

    const ketQua = await docTheoKhoang(db, 'e1', 0)
    expect(ketQua.map((d) => d.soThuTu)).toEqual([9, 10, 100])

    db.close()
  })

  it('hai dong CUNG soThuTu nhung KHAC epoch khong duoc de len nhau (khoa phai gom ca epoch)', async () => {
    // Kich ban that: may chu xoay epoch sau khi khoi phuc tu ban sao luu (spec 4.8.3/4.8.4) —
    // chuoi so_thu_tu MOI khoi dong lai tu so nho, nen rat co the trung voi mot soThuTu cua
    // epoch CU con trong so da nhan. Neu khoa luu tru chi dung soThuTu (bo epoch), dong epoch
    // CU se bi dong epoch MOI ghi de im lang — dung luc do lai la dong duy nhat Task 8 can de
    // bu lai cho may chu (spec 4.8.5).
    const db = await moKho(tenKhoRieng())

    const dongEpochCu: DongDaNhan = {
      epoch: 'epoch-cu',
      soThuTu: 4850,
      bang: 'giaoDan',
      banGhiId: 'id-cu',
      truong: 'tenGiaoDan',
      giaTri: 'Du lieu epoch cu',
      dongHoVatLy: '2026-09-10T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-epoch-cu',
      ngayNhan: '2026-09-10T10:00:00Z',
    }

    const dongEpochMoi: DongDaNhan = {
      ...dongEpochCu,
      epoch: 'epoch-moi',
      banGhiId: 'id-moi',
      giaTri: 'Du lieu epoch moi',
      giaoDichId: 'gd-epoch-moi',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    await ghiSoDaNhan(db, [dongEpochCu, dongEpochMoi])

    const conEpochCu = await docTheoKhoang(db, 'epoch-cu', 0)
    expect(conEpochCu).toHaveLength(1)
    expect(conEpochCu[0].giaTri).toBe('Du lieu epoch cu')

    const conEpochMoi = await docTheoKhoang(db, 'epoch-moi', 0)
    expect(conEpochMoi).toHaveLength(1)
    expect(conEpochMoi[0].giaTri).toBe('Du lieu epoch moi')

    db.close()
  })

  it('donSoDaNhanCu xoa chi nhung dong co ngayNhan < truocNgay, giu nguyen nhung dong >= truocNgay', async () => {
    const db = await moKho(tenKhoRieng())

    const ngayCanBao = '2026-09-15T00:00:00Z'

    const dongCu: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-cu',
      truong: 'tenGiaoDan',
      giaTri: 'Cũ',
      dongHoVatLy: '2026-09-14T23:59:59Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-cu',
      ngayNhan: '2026-09-14T23:59:59Z', // < ngayCanBao
    }

    const dongMoi: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 20,
      bang: 'giaoDan',
      banGhiId: 'id-moi',
      truong: 'tenGiaoDan',
      giaTri: 'Mới',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-moi',
      ngayNhan: '2026-09-15T10:00:00Z', // > ngayCanBao
    }

    const dongCanBao: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 15,
      bang: 'giaoDan',
      banGhiId: 'id-can-bao',
      truong: 'tenGiaoDan',
      giaTri: 'Cân bao',
      dongHoVatLy: '2026-09-15T00:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-can-bao',
      ngayNhan: '2026-09-15T00:00:00Z', // == ngayCanBao (>= nên giu)
    }

    await ghiSoDaNhan(db, [dongCu, dongMoi, dongCanBao])

    // Xoá dòng có ngayNhan < '2026-09-15T00:00:00Z'
    await donSoDaNhanCu(db, ngayCanBao)

    // Doc lại: chỉ còn dongMoi và dongCanBao (cả hai >= ngayCanBao)
    const conLai = await docTheoKhoang(db, 'e1', 0)
    expect(conLai).toHaveLength(2)
    const soThuTuConLai = conLai.map((d) => d.soThuTu).sort((a, b) => a - b)
    expect(soThuTuConLai).toEqual([15, 20])

    // dongCu đã bị xoá
    expect(conLai.every((d) => d.banGhiId !== 'id-cu')).toBe(true)

    db.close()
  })

  it('donSoDaNhanCu phai duyet HET kho, khong dung som sau dong xoa dau tien', async () => {
    // Neu con trỏ dừng ngay sau khi xoá được MỘT dòng (thiếu con.continue(), hoặc dừng vì tưởng
    // "đã tìm thấy dòng cần xoá"), các dòng cũ đứng SAU dòng đó theo thứ tự khoá sẽ không bao giờ
    // bị dọn — sổ đã nhận phình vô hạn, có thể chạm quota IndexedDB và khiến trình duyệt evict cả
    // database (kể cả hàng chờ chưa gửi). Dựng tình huống: dòng CŨ (cần xoá) đứng SAU dòng MỚI
    // (không xoá) theo thứ tự khoá chuỗi "epoch:soThuTu" — soThuTu 1 (mới) < 2 (cũ) < 3 (cũ).
    const db = await moKho(tenKhoRieng())

    const ngayCanBao = '2026-09-15T00:00:00Z'
    const taoDong = (soThuTu: number, ngayNhan: string): DongDaNhan => ({
      epoch: 'e1',
      soThuTu,
      bang: 'giaoDan',
      banGhiId: `id-${soThuTu}`,
      truong: 'tenGiaoDan',
      giaTri: `gia-tri-${soThuTu}`,
      dongHoVatLy: ngayNhan,
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: `gd-${soThuTu}`,
      ngayNhan,
    })

    const dongMoiDungTruoc = taoDong(1, '2026-09-15T10:00:00Z') // moi, khong xoa, khoa "e1:1"
    const dongCu1 = taoDong(2, '2026-09-14T00:00:00Z') // cu, phai xoa, khoa "e1:2"
    const dongCu2 = taoDong(3, '2026-09-13T00:00:00Z') // cu, phai xoa, khoa "e1:3"

    await ghiSoDaNhan(db, [dongMoiDungTruoc, dongCu1, dongCu2])

    await donSoDaNhanCu(db, ngayCanBao)

    const conLai = await docTheoKhoang(db, 'e1', 0)
    expect(conLai.map((d) => d.soThuTu)).toEqual([1])

    db.close()
  })

  it('donSoDaNhanCu voi dieu kien so sanh chuoi ISO 8601 hop le theo thu tu tu dien = thu tu thoi gian', async () => {
    const db = await moKho(tenKhoRieng())

    const dong1: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 1,
      bang: 'giaoDan',
      banGhiId: 'id-1',
      truong: 'tenGiaoDan',
      giaTri: 'A',
      dongHoVatLy: '2026-09-14T23:59:59Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-1',
      ngayNhan: '2026-09-14T23:59:59Z',
    }

    const dong2: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 2,
      bang: 'giaoDan',
      banGhiId: 'id-2',
      truong: 'tenGiaoDan',
      giaTri: 'B',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-2',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    const dong3: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 3,
      bang: 'giaoDan',
      banGhiId: 'id-3',
      truong: 'tenGiaoDan',
      giaTri: 'C',
      dongHoVatLy: '2026-09-16T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-3',
      ngayNhan: '2026-09-16T10:00:00Z',
    }

    await ghiSoDaNhan(db, [dong1, dong2, dong3])

    // Xoá các dòng < '2026-09-15T10:00:00Z' — chỉ dong1 bị xoá
    await donSoDaNhanCu(db, '2026-09-15T10:00:00Z')

    const conLai = await docTheoKhoang(db, 'e1', 0)
    expect(conLai).toHaveLength(2)
    expect(conLai[0].soThuTu).toBe(2)
    expect(conLai[1].soThuTu).toBe(3)

    db.close()
  })

  it('donSoDaNhanCu voi nguong rat xa trong qua khu thi khong xoa gi ca', async () => {
    const db = await moKho(tenKhoRieng())

    const dong: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-1',
      truong: 'tenGiaoDan',
      giaTri: 'A',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-1',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    await ghiSoDaNhan(db, [dong])

    // Don sach voi ngay rat trong qua — khong co gi bi xoa
    await expect(donSoDaNhanCu(db, '2000-01-01T00:00:00Z')).resolves.toBeUndefined()

    const conLai = await docTheoKhoang(db, 'e1', 0)
    expect(conLai).toHaveLength(1)

    db.close()
  })

  it('ghiSoDaNhan voi mang rong khong lam gi va khong nem loi', async () => {
    const db = await moKho(tenKhoRieng())

    await expect(ghiSoDaNhan(db, [])).resolves.toBeUndefined()

    const conLai = await docTheoKhoang(db, 'e1', 0)
    expect(conLai).toEqual([])

    db.close()
  })

  it('ghiSoDaNhan va docTheoKhoang bao toan tat ca truong cua DongDaNhan', async () => {
    const db = await moKho(tenKhoRieng())

    const dong: DongDaNhan = {
      epoch: 'epoch-abc-123',
      soThuTu: 42,
      bang: 'giaDinh',
      banGhiId: 'uuid-12345',
      truong: 'maTruong',
      giaTri: 'gia-tri-test',
      dongHoVatLy: '2026-09-15T12:34:56.789Z',
      dongHoLogic: 9999,
      thietBiId: 'device-xyz',
      giaoDichId: 'gd-guid-uuid',
      ngayNhan: '2026-09-15T12:34:56Z',
    }

    await ghiSoDaNhan(db, [dong])

    const ketQua = await docTheoKhoang(db, 'epoch-abc-123', 0)
    expect(ketQua).toHaveLength(1)

    const doc = ketQua[0]
    expect(doc.epoch).toBe('epoch-abc-123')
    expect(doc.soThuTu).toBe(42)
    expect(doc.bang).toBe('giaDinh')
    expect(doc.banGhiId).toBe('uuid-12345')
    expect(doc.truong).toBe('maTruong')
    expect(doc.giaTri).toBe('gia-tri-test')
    expect(doc.dongHoVatLy).toBe('2026-09-15T12:34:56.789Z')
    expect(doc.dongHoLogic).toBe(9999)
    expect(doc.thietBiId).toBe('device-xyz')
    expect(doc.giaoDichId).toBe('gd-guid-uuid')
    expect(doc.ngayNhan).toBe('2026-09-15T12:34:56Z')

    db.close()
  })

  it('ghiSoDaNhan va docTheoKhoang xu ly giaTri null dung', async () => {
    const db = await moKho(tenKhoRieng())

    const dongCoNull: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-null',
      truong: 'giaTri',
      giaTri: null, // null
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null, // null
      giaoDichId: 'gd-null',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    await ghiSoDaNhan(db, [dongCoNull])

    const ketQua = await docTheoKhoang(db, 'e1', 0)
    expect(ketQua).toHaveLength(1)
    expect(ketQua[0].giaTri).toBeNull()
    expect(ketQua[0].thietBiId).toBeNull()

    db.close()
  })

  it('ghi nhieu lan thi chi con dung cac dong cuoi cung (ghi de)', async () => {
    const db = await moKho(tenKhoRieng())

    const dong1: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-1',
      truong: 'tenGiaoDan',
      giaTri: 'A',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-1',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    const dong1Sua: DongDaNhan = {
      ...dong1,
      giaTri: 'A-sua',
      ngayNhan: '2026-09-15T11:00:00Z',
    }

    const dong2: DongDaNhan = {
      ...dong1,
      soThuTu: 20,
      banGhiId: 'id-2',
      giaoDichId: 'gd-2',
      giaTri: 'B',
    }

    // Lan 1: ghi dong1
    await ghiSoDaNhan(db, [dong1])
    let ketQua = await docTheoKhoang(db, 'e1', 0)
    expect(ketQua).toHaveLength(1)
    expect(ketQua[0].giaTri).toBe('A')

    // Lan 2: ghi lai dong1 (sua) va dong2 (moi)
    await ghiSoDaNhan(db, [dong1Sua, dong2])
    ketQua = await docTheoKhoang(db, 'e1', 0)
    expect(ketQua).toHaveLength(2)
    expect(ketQua.find((d) => d.soThuTu === 10)?.giaTri).toBe('A-sua')
    expect(ketQua.find((d) => d.soThuTu === 20)?.giaTri).toBe('B')

    db.close()
  })

  it('docTheoKhoang dung dieu kien > KHONG >=: khong lay dong co soThuTu == tuSoThuTu', async () => {
    // Ràng buộc từ spec 4.8.5 bước 2: lọc "so_thu_tu > 4900" tức so_thu_tu chính xác
    // là 4900 KHÔNG được lấy, chỉ lấy những cái > 4900.
    const db = await moKho(tenKhoRieng())

    const dong10: DongDaNhan = {
      epoch: 'e1',
      soThuTu: 10,
      bang: 'giaoDan',
      banGhiId: 'id-10',
      truong: 'tenGiaoDan',
      giaTri: 'A',
      dongHoVatLy: '2026-09-15T10:00:00Z',
      dongHoLogic: 100,
      thietBiId: null,
      giaoDichId: 'gd-10',
      ngayNhan: '2026-09-15T10:00:00Z',
    }

    const dong20: DongDaNhan = {
      ...dong10,
      soThuTu: 20,
      banGhiId: 'id-20',
      giaoDichId: 'gd-20',
    }

    const dong30: DongDaNhan = {
      ...dong10,
      soThuTu: 30,
      banGhiId: 'id-30',
      giaoDichId: 'gd-30',
    }

    await ghiSoDaNhan(db, [dong10, dong20, dong30])

    // Lọc tuSoThuTu = 20 — chỉ lấy những cái > 20, tức chỉ dong30
    const ketQua = await docTheoKhoang(db, 'e1', 20)
    expect(ketQua).toHaveLength(1)
    expect(ketQua[0].soThuTu).toBe(30)
    // dong20 (soThuTu == 20) KHÔNG được lấy
    expect(ketQua.some((d) => d.soThuTu === 20)).toBe(false)

    db.close()
  })
})
