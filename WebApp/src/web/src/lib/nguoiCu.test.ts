import { describe, expect, it, vi } from 'vitest'
import { apDungDoiHangLoat, chayCayQuyetDinhNguoiCu, keHoachDoiHangLoat, type ThanhVienNhe } from './nguoiCu'

/** Hộp thoại giả trả lời theo DÃY đã chuẩn bị trước — mỗi lần `hoi()` được gọi thì lấy câu trả
 * lời tiếp theo trong dãy, ném lỗi nếu hỏi nhiều hơn dự kiến (giúp bắt lỗi hỏi thừa/thiếu bước). */
function hoiTheoDay(...traLoi: boolean[]) {
  const hang = [...traLoi]
  const goi = vi.fn(async (_thongDiep: string) => {
    if (hang.length === 0) throw new Error('Cây quyết định hỏi nhiều hơn số câu trả lời chuẩn bị')
    return hang.shift()!
  })
  return goi
}

describe('chayCayQuyetDinhNguoiCu', () => {
  it('Yes cau hoi dau tien -> xoa han, khong hoi gi them', async () => {
    const hoi = hoiTheoDay(true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Giuse Nguyễn Văn A', idNguoiConLai: 'vo1', idNguoiMoi: 'moi1', thanhVien: [] },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: true } })
    expect(hoi).toHaveBeenCalledTimes(1)
    expect(hoi.mock.calls[0][0]).toContain('Bạn có muốn xóa giáo dân Giuse Nguyễn Văn A ra khỏi gia đình')
  })

  it('No -> bo chon (X), con vo, vo co con, Yes buoc 2 -> thanh Cha(4)', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'vo1', vaiTro: 1 }, { giaoDanId: 'vo1', vaiTro: 2 }]
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'vo1', idNguoiMoi: null, thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 4 } })
    expect(hoi.mock.calls[1][0]).toContain('thành cha của gia đình không?')
  })

  it('No -> bo chon (X), con vo nhung KHONG co con -> Chua ro (100) khong hoi buoc 2', async () => {
    const hoi = hoiTheoDay(false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'vo1', idNguoiMoi: null, thanhVien: [{ giaoDanId: 'vo1', vaiTro: 1 }] },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
    expect(hoi).toHaveBeenCalledTimes(1)
  })

  it('No -> khong con nguoi kia -> Chua ro, khong hoi buoc 2', async () => {
    const hoi = hoiTheoDay(false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: null, idNguoiMoi: null, thanhVien: [] },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
  })

  it('doi Nguoi nam: con vo dung vai tro, nguoi moi da tung la Con -> hoi doi hang loat sang Ong Ba', async () => {
    const thanhVien: ThanhVienNhe[] = [
      { giaoDanId: 'vo1', vaiTro: 1 },
      { giaoDanId: 'moi1', vaiTro: 2 },
      { giaoDanId: 'con1', vaiTro: 4 },
      { giaoDanId: 'con2', vaiTro: 5 },
      { giaoDanId: 'con3', vaiTro: 8 },
    ]
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'vo1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 4 }, doiHangLoat: 'ongBa' })
    expect(hoi.mock.calls[1][0]).toContain('cha mẹ thành ông bà')
  })

  it('doi Nguoi nam: con vo dung vai tro, nguoi moi CHUA tung la Con -> hoi doi hang loat sang Chua ro', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'vo1', vaiTro: 1 }]
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'vo1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 }, doiHangLoat: 'chuaRo' })
    expect(hoi.mock.calls[1][0]).toContain('các thành viên trong gia đình là chưa rõ')
  })

  it('doi Nguoi nu (Vo): dung "me"/"cua gia dinh" khi bo chon con chong co con', async () => {
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      {
        vaiTroDangDoi: 1, tenNguoiCu: 'Bà B', idNguoiConLai: 'chong1', idNguoiMoi: null,
        thanhVien: [{ giaoDanId: 'chong1', vaiTro: 0 }, { giaoDanId: 'chong1', vaiTro: 2 }],
      },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 5 } })
    expect(hoi.mock.calls[1][0]).toContain('thành mẹ của gia đình không?')
  })

  it('khong ai o vai tro doi dien, nguoi moi da tung la Con -> hoi rieng, Yes -> Cha + doi het sang chua ro', async () => {
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: null, idNguoiMoi: 'moi1', thanhVien: [{ giaoDanId: 'moi1', vaiTro: 2 }] },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 4 }, doiHangLoat: 'chuaRo' })
    expect(hoi.mock.calls[1][0]).toContain('trong gia đình và các thành viên khác là chưa rõ')
  })

  it('khong ai o vai tro doi dien, nguoi moi CHUA tung la Con -> Chua ro, khong hoi buoc 2', async () => {
    const hoi = hoiTheoDay(false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: null, idNguoiMoi: 'moi1', thanhVien: [] },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
    expect(hoi).toHaveBeenCalledTimes(1)
  })

  // --- Bổ sung: các đường trả lời "No" ở câu hỏi phụ (dòng 92/99/121) và nhánh dữ liệu bất
  // thường (dòng 103-112 — người kia KHÔNG đúng vai trò mong đợi), review-frontend mục "Cao #2"
  // chỉ ra 0% coverage. Xem báo cáo mutation-test: đảo `if(yes)` thành `if(!yes)` ở các dòng này
  // (hoặc bỏ qua nhánh 103-112) từng khiến cả 183 test cũ vẫn xanh.

  it('No o buoc 2 (con vo dung vai tro, nguoi moi tung la Con) -> Chua ro, KHONG doi hang loat', async () => {
    const thanhVien: ThanhVienNhe[] = [
      { giaoDanId: 'vo1', vaiTro: 1 },
      { giaoDanId: 'moi1', vaiTro: 2 },
    ]
    const hoi = hoiTheoDay(false, false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'vo1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
    expect(kq.doiHangLoat).toBeUndefined()
    expect(hoi).toHaveBeenCalledTimes(2)
  })

  it('No o buoc 2 (con vo dung vai tro, nguoi moi CHUA tung la Con) -> Chua ro, KHONG doi hang loat', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'vo1', vaiTro: 1 }]
    const hoi = hoiTheoDay(false, false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'vo1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
    expect(kq.doiHangLoat).toBeUndefined()
  })

  it('No o buoc 2 (khong ai vai tro doi dien, nguoi moi tung la Con) -> Chua ro, KHONG thanh Cha, KHONG doi hang loat', async () => {
    const hoi = hoiTheoDay(false, false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: null, idNguoiMoi: 'moi1', thanhVien: [{ giaoDanId: 'moi1', vaiTro: 2 }] },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
    expect(kq.doiHangLoat).toBeUndefined()
  })

  it('doi Nguoi nu (Vo), No o buoc 2 -> Chua ro, KHONG thanh Me', async () => {
    const hoi = hoiTheoDay(false, false)
    const kq = await chayCayQuyetDinhNguoiCu(
      {
        vaiTroDangDoi: 1, tenNguoiCu: 'Bà B', idNguoiConLai: 'chong1', idNguoiMoi: null,
        thanhVien: [{ giaoDanId: 'chong1', vaiTro: 0 }, { giaoDanId: 'chong1', vaiTro: 2 }],
      },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
  })

  // --- Nhánh dữ liệu bất thường (dòng 103-112): còn người kia ở vai trò đối diện NHƯNG người
  // đó KHÔNG đúng vai trò mong đợi (vd đang đổi Chồng mà "người còn lại" không có vai trò Vợ
  // trong lưới — dữ liệu di trú từ Access cũ không sạch).

  it('du lieu bat thuong: con nguoi kia nhung KHONG dung vai tro doi dien, khong ai tung la Con -> Chua ro thang, khong hoi', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'khac1', vaiTro: 8 }]
    const hoi = hoiTheoDay(false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'khac1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
    expect(hoi).toHaveBeenCalledTimes(1) // chỉ câu hỏi đầu tiên — không rơi vào câu hỏi phụ
  })

  it('du lieu bat thuong: nguoi CON LAI tung la Con -> hoi rieng, Yes -> thanh Cha', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'khac1', vaiTro: 2 }]
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'khac1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 4 } })
    expect(kq.doiHangLoat).toBeUndefined()
    expect(hoi.mock.calls[1][0]).toContain('thành cha trong gia đình không?')
  })

  it('du lieu bat thuong: nguoi MOI tung la Con -> hoi rieng, Yes -> thanh Cha', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'khac1', vaiTro: 8 }, { giaoDanId: 'moi1', vaiTro: 2 }]
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'khac1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 4 } })
  })

  it('du lieu bat thuong: nguoi CON LAI tung la Con, tra loi No -> Chua ro (khong thanh Cha)', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'khac1', vaiTro: 2 }]
    const hoi = hoiTheoDay(false, false)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 0, tenNguoiCu: 'Ông A', idNguoiConLai: 'khac1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 100 } })
  })

  it('du lieu bat thuong: doi Nguoi nu (Vo), nguoi con lai khong dung vai tro Chong, tung la Con -> thanh Me', async () => {
    const thanhVien: ThanhVienNhe[] = [{ giaoDanId: 'khac1', vaiTro: 2 }]
    const hoi = hoiTheoDay(false, true)
    const kq = await chayCayQuyetDinhNguoiCu(
      { vaiTroDangDoi: 1, tenNguoiCu: 'Bà B', idNguoiConLai: 'khac1', idNguoiMoi: 'moi1', thanhVien },
      hoi,
    )
    expect(kq).toEqual({ nguoiCu: { xoa: false, vaiTroMoi: 5 } })
    expect(hoi.mock.calls[1][0]).toContain('thành mẹ trong gia đình không?')
  })
})

describe('apDungDoiHangLoat', () => {
  it('ongBa: Cha->Ong, Me->Ba, con lai (tru Ong/Ba) -> Chua ro', () => {
    expect(apDungDoiHangLoat(4, 'ongBa')).toBe(6)
    expect(apDungDoiHangLoat(5, 'ongBa')).toBe(7)
    expect(apDungDoiHangLoat(6, 'ongBa')).toBe(6)
    expect(apDungDoiHangLoat(7, 'ongBa')).toBe(7)
    expect(apDungDoiHangLoat(8, 'ongBa')).toBe(100)
    expect(apDungDoiHangLoat(2, 'ongBa')).toBe(100)
  })

  it('chuaRo: bat ky vai tro nao cung thanh 100', () => {
    expect(apDungDoiHangLoat(4, 'chuaRo')).toBe(100)
    expect(apDungDoiHangLoat(100, 'chuaRo')).toBe(100)
  })
})

describe('keHoachDoiHangLoat', () => {
  it('bo qua nguoi khong doi vai tro (da la Ong/Ba khi doi sang ongBa)', () => {
    const ke = keHoachDoiHangLoat(
      [{ giaoDanId: 'a', vaiTro: 4 }, { giaoDanId: 'b', vaiTro: 6 }, { giaoDanId: 'c', vaiTro: 8 }],
      'ongBa',
    )
    expect(ke).toEqual([{ giaoDanId: 'a', vaiTroMoi: 6 }, { giaoDanId: 'c', vaiTroMoi: 100 }])
  })
})
