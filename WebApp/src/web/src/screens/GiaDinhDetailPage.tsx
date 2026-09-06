import { useCallback, useEffect, useState } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaDinhDetail as GiaDinhDetailDuLieu, GiaoDanTimKiem, GiaoHo } from '../api/types'
import { canQuyetDinhChuyenXu, canQuyetDinhNguoiCu } from '../lib/canhBaoGiaDinh'
import { chayCayQuyetDinhNguoiCu, keHoachDoiHangLoat } from '../lib/nguoiCu'
import { useHoiDap } from '../components/GxHoiDap'
import { TrangThaiTai } from '../components/TrangThaiTai'
import { GiaDinhDetail, type YeuCauCapNhatGiaDinh } from './GiaDinhDetail'

type Props = {
  /** `null` = mở thẻ "Thêm gia đình" — component tự chuyển sang id thật ngay trên CÙNG một thẻ
   * sau khi `POST /api/gia-dinh` thành công (xem `taoMoi`), không cần đổi route/thẻ tài liệu. */
  id: string | null
  moGiaoDan?: (id: string) => void
  moDanhSachGiaDinh?: () => void
}

const tenHienThi = (nguoi: { tenThanh: string | null; hoTen: string }) =>
  `${nguoi.tenThanh ?? ''} ${nguoi.hoTen}`.trim()

/**
 * Container nối `GiaDinhDetail` với toàn bộ API ghi của gia đình: `GET`/`PUT /api/gia-dinh/{id}`,
 * tạo mới, gán/đổi Người nam-nữ (kèm cây quyết định `NguoiCu`, xem `lib/nguoiCu.ts`), thêm/xoá
 * thành viên. Mọi hộp thoại xác nhận dùng `useHoiDap` (trong ứng dụng) thay cho
 * `window.confirm`/`alert` — bắt buộc cho chuỗi hỏi nhiều bước của `NguoiCu`, xem
 * docs/superpowers/specs/man-hinh/can-review-sau.md mục 3.
 */
export function GiaDinhDetailPage({ id, moGiaoDan, moDanhSachGiaDinh }: Props) {
  const [idThat, setIdThat] = useState(id)
  const [duLieu, setDuLieu] = useState<GiaDinhDetailDuLieu | null>(null)
  const [dangTai, setDangTai] = useState(idThat !== null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [thongBaoLuu, setThongBaoLuu] = useState<string | null>(null)
  const [danhMucGiaoHo, setDanhMucGiaoHo] = useState<GiaoHo[]>([])
  const { hoi, hoi3, bao, Dialog } = useHoiDap()

  useEffect(() => {
    api.giaoHo.danhMuc()
      .then(setDanhMucGiaoHo)
      .catch((e: unknown) => { console.error('Không tải được danh mục giáo họ', e) })
  }, [])

  const tai = useCallback(() => {
    if (idThat === null) return
    setDangTai(true)
    setLoi(null)
    api.giaDinh.chiTiet(idThat)
      .then((ct) => setDuLieu(ct))
      .catch((e: unknown) => {
        console.error(`Không tải được chi tiết gia đình ${idThat}`, e)
        setLoi(e instanceof Error ? e.message : String(e))
      })
      .finally(() => setDangTai(false))
  }, [idThat])

  useEffect(tai, [tai])

  /** Nút "Tạo gia đình" khi đang thêm mới — tương đương `Memory.Instance.GetNextId` lúc mở
   * `frmGiaDinh` ở chế độ Thêm mới của bản desktop (sinh mã ngay), chỉ khác thời điểm: bản web
   * cần gọi API vì không có state phía máy khách để giữ "bản ghi nháp có mã nhưng chưa lưu".
   * Sau khi có id thật, các thao tác Người nam/nữ/thành viên bên dưới hoạt động bình thường. */
  async function taoMoi(payload: { tenGiaDinh: string | null; giaoHoId: string | null }) {
    setDangLuu(true)
    try {
      const kq = await api.giaDinh.tao(payload)
      setIdThat(kq.id)
    } catch (e) {
      await bao(e instanceof Error ? e.message : 'Tạo gia đình thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  if (idThat === null) {
    return (
      <>
        <GiaDinhDetail
          moGiaoDan={moGiaoDan}
          moDanhSachGiaDinh={moDanhSachGiaDinh}
          danhMucGiaoHo={danhMucGiaoHo}
          onTaoMoi={taoMoi}
          dangLuu={dangLuu}
        />
        {Dialog}
      </>
    )
  }

  async function luu(payload: YeuCauCapNhatGiaDinh) {
    setDangLuu(true)
    setThongBaoLuu(null)
    try {
      await api.giaDinh.capNhat(idThat as string, payload)
      setThongBaoLuu('Đã lưu thành công.')
      tai()
    } catch (e) {
      if (e instanceof LoiXungDot) {
        setThongBaoLuu(e.message)
      } else {
        console.error(`Không lưu được gia đình ${idThat}`, e)
        setThongBaoLuu(e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
      }
    } finally {
      setDangLuu(false)
    }
  }

  /** Đổi vai trò HÀNG LOẠT cho các thành viên khác (nhánh 'ongBa'/'chuaRo' của cây quyết định
   * `NguoiCu`) — không có endpoint "sửa vai trò tại chỗ" nên áp dụng bằng xoá+thêm lại từng
   * người bị ảnh hưởng, tuần tự. Lỗi một người không chặn các người còn lại (chỉ ghi log) —
   * đây là hành vi PHỤ (đoán vai trò), không phải thao tác chính đang được người dùng chờ. */
  async function apDungDoiHangLoatChoGiaDinh(
    giaDinhId: string,
    thanhVienHienTai: GiaDinhDetailDuLieu['thanhVien'],
    kieu: 'ongBa' | 'chuaRo',
  ) {
    const thanhVienKhac = thanhVienHienTai.filter((t) => t.vaiTro !== 0 && t.vaiTro !== 1)
    const ke = keHoachDoiHangLoat(thanhVienKhac, kieu)
    for (const { giaoDanId, vaiTroMoi } of ke) {
      const vaiTroCu = thanhVienKhac.find((t) => t.giaoDanId === giaoDanId)!.vaiTro
      try {
        await api.giaDinh.xoaThanhVien(giaDinhId, giaoDanId, vaiTroCu)
        await api.giaDinh.themThanhVien(giaDinhId, {
          giaoDanId, vaiTro: vaiTroMoi, boQuaCanhBao: true, muonChuyenVeXu: null,
        })
      } catch (e) {
        console.error(`Đổi vai trò hàng loạt thất bại cho giáo dân ${giaoDanId}`, e)
      }
    }
  }

  /** Chọn/đổi Người nam (vaiTro=0) hoặc Người nữ (vaiTro=1) — đúng thứ tự
   * `txtNguoiChong_OnSelecting`/`txtNguoiVo_OnSelecting` + `NguoiCu` của bản gốc: (1) cảnh báo
   * nghiệp vụ (tuổi kết hôn, đã từng kết hôn) xác nhận MỘT LƯỢT gộp (can-review-sau.md mục 20);
   * (2) nếu vai trò đang có người khác, chạy cây quyết định `NguoiCu` để tính ý định cuối; (3)
   * gửi lại kèm ý định đó; (4) áp dụng đổi vai trò hàng loạt cho các thành viên khác nếu có. */
  async function ganVoChong(vaiTro: 0 | 1, giaoDan: GiaoDanTimKiem) {
    if (!duLieu) return
    let boQuaCanhBao = false
    let xuLyNguoiCu: { xoa: boolean; vaiTroMoi: number | null } | null = null
    let doiHangLoat: 'ongBa' | 'chuaRo' | undefined

    for (;;) {
      let kq
      try {
        kq = await api.giaDinh.ganVoChong(duLieu.id, vaiTro, {
          giaoDanId: giaoDan.id, rowVersion: duLieu.rowVersion, boQuaCanhBao, xuLyNguoiCu,
        })
      } catch (e) {
        await bao(e instanceof Error ? e.message : 'Không gán được, thử lại sau.')
        return
      }
      if (kq.giaoDanId) break

      const canhBao = kq.canhBao
      if (canQuyetDinhNguoiCu(canhBao)) {
        const nguoiCuHienTai = duLieu.thanhVien.find((t) => t.vaiTro === vaiTro)
        const idConLai = duLieu.thanhVien.find((t) => t.vaiTro === (vaiTro === 0 ? 1 : 0))?.giaoDanId ?? null
        if (!nguoiCuHienTai) { await bao('Không xác định được người cũ để hỏi lại.'); return }
        const ketQua = await chayCayQuyetDinhNguoiCu(
          {
            vaiTroDangDoi: vaiTro, tenNguoiCu: tenHienThi(nguoiCuHienTai),
            idNguoiConLai: idConLai, idNguoiMoi: giaoDan.id, thanhVien: duLieu.thanhVien,
          },
          hoi,
        )
        xuLyNguoiCu = ketQua.nguoiCu.xoa
          ? { xoa: true, vaiTroMoi: null }
          : { xoa: false, vaiTroMoi: ketQua.nguoiCu.vaiTroMoi }
        doiHangLoat = ketQua.doiHangLoat
        boQuaCanhBao = true
        continue
      }

      const dongY = await hoi(canhBao.join('\n\n') + '\n\nBạn có chắc muốn tiếp tục không?')
      if (!dongY) return
      boQuaCanhBao = true
    }

    if (doiHangLoat) await apDungDoiHangLoatChoGiaDinh(duLieu.id, duLieu.thanhVien, doiHangLoat)
    tai()
  }

  /** Nút "Bỏ chọn" (X) cạnh Người nam/Người nữ — bản gốc cũng chạy qua `NguoiCu(nguoicu, null)`
   * (xem `KiemTraSuThayDoiNguoiConLai`, `frmGiaDinh.cs:393/401`) nên áp dụng ĐÚNG cây quyết
   * định với `idNguoiMoi=null`, rồi thực hiện bằng xoá (+thêm lại nếu hạ xuống thành viên) vì
   * không có `giaoDanId` mới nào để gọi `PUT vo-chong`. */
  async function boChonVoChong(vaiTro: 0 | 1) {
    if (!duLieu) return
    const nguoi = duLieu.thanhVien.find((t) => t.vaiTro === vaiTro)
    if (!nguoi) return
    const idConLai = duLieu.thanhVien.find((t) => t.vaiTro === (vaiTro === 0 ? 1 : 0))?.giaoDanId ?? null

    const ketQua = await chayCayQuyetDinhNguoiCu(
      {
        vaiTroDangDoi: vaiTro, tenNguoiCu: tenHienThi(nguoi),
        idNguoiConLai: idConLai, idNguoiMoi: null, thanhVien: duLieu.thanhVien,
      },
      hoi,
    )
    try {
      await api.giaDinh.xoaThanhVien(duLieu.id, nguoi.giaoDanId, vaiTro)
      if (!ketQua.nguoiCu.xoa) {
        await api.giaDinh.themThanhVien(duLieu.id, {
          giaoDanId: nguoi.giaoDanId, vaiTro: ketQua.nguoiCu.vaiTroMoi,
          boQuaCanhBao: true, muonChuyenVeXu: null,
        })
      }
      if (ketQua.doiHangLoat) await apDungDoiHangLoatChoGiaDinh(duLieu.id, duLieu.thanhVien, ketQua.doiHangLoat)
      tai()
    } catch (e) {
      await bao(e instanceof Error ? e.message : 'Không bỏ chọn được, thử lại sau.')
    }
  }

  /** Thêm một người vào lưới "Thành viên khác" (`addGiaoDan`, frmGiaDinh.cs:1043-1140) — đúng
   * thứ tự: cảnh báo gộp một lượt (đã thuộc gia đình khác/đã bị xoá) rồi, nếu người này đã
   * chuyển xứ, hỏi riêng 3 lựa chọn Yes/No/Cancel. */
  async function themThanhVien(giaoDan: GiaoDanTimKiem, vaiTro: number) {
    if (!duLieu) return
    let boQuaCanhBao = false
    let muonChuyenVeXu: boolean | null = null

    for (;;) {
      let kq
      try {
        kq = await api.giaDinh.themThanhVien(duLieu.id, {
          giaoDanId: giaoDan.id, vaiTro, boQuaCanhBao, muonChuyenVeXu,
        })
      } catch (e) {
        await bao(e instanceof Error ? e.message : 'Không thêm được thành viên, thử lại sau.')
        return
      }
      if (kq.giaoDanId) { tai(); return }

      const canhBao = kq.canhBao
      if (canQuyetDinhChuyenXu(canhBao)) {
        const luaChon = await hoi3(canhBao[0])
        if (luaChon === 'cancel') return
        muonChuyenVeXu = luaChon === 'yes'
        boQuaCanhBao = true
        continue
      }
      const dongY = await hoi(canhBao.join('\n\n'))
      if (!dongY) return
      boQuaCanhBao = true
    }
  }

  /** Xoá một thành viên — VĨNH VIỄN, đúng `gxAddEdit1_DeleteClick` (dòng 1198, can-review-sau.md
   * mục 5), không phải xoá mềm. */
  async function xoaThanhVien(giaoDanId: string, vaiTro: number) {
    if (!duLieu) return
    const dongY = await hoi(
      'Bạn có thực sự muốn xóa vĩnh viễn giáo dân này ra khỏi gia đình.\r\n' +
        'Chọn [Yes] để xóa.\r\nChọn [No] để thoát.',
    )
    if (!dongY) return
    try {
      await api.giaDinh.xoaThanhVien(duLieu.id, giaoDanId, vaiTro)
      tai()
    } catch (e) {
      await bao(e instanceof Error ? e.message : 'Không xoá được, thử lại sau.')
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      {duLieu && (
        <GiaDinhDetail
          duLieu={duLieu}
          moGiaoDan={moGiaoDan}
          moDanhSachGiaDinh={moDanhSachGiaDinh}
          onLuu={luu}
          dangLuu={dangLuu}
          thongBaoLuu={thongBaoLuu}
          danhMucGiaoHo={danhMucGiaoHo}
          onGanVoChong={ganVoChong}
          onBoChonVoChong={boChonVoChong}
          onThemThanhVien={themThanhVien}
          onXoaThanhVien={xoaThanhVien}
        />
      )}
      {Dialog}
    </TrangThaiTai>
  )
}
