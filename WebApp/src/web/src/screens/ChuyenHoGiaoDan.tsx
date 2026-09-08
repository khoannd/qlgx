import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import type { ChuyenHoGiaoDanXemTruoc, GiaoDanListItem, GiaoHo } from '../api/types'

type Props = {
  danhMucGiaoHo: GiaoHo[]
}

/**
 * "Công cụ dữ liệu" → "Chuyển họ hàng loạt" — nửa GIÁO DÂN (frmChuyenHoGiaoDan.cs, 170d, xem
 * docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 4). CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT —
 * khác desktop ở đúng chỗ an toàn bắt buộc: có bước "Xem trước" tách riêng lấy số liệu THẬT từ
 * máy chủ, hộp thoại xác nhận nêu con số cụ thể, và ghi trong một transaction (xem
 * ChuyenHoService). Desktop không có bước xem trước — lưới chọn sẵn rồi ghi thẳng.
 */
export function ChuyenHoGiaoDan({ danhMucGiaoHo }: Props) {
  const [giaoHoNguonId, setGiaoHoNguonId] = useState('')
  const [giaoHoDichId, setGiaoHoDichId] = useState('')
  const [danhSach, setDanhSach] = useState<GiaoDanListItem[]>([])
  const [daChon, setDaChon] = useState<Set<string>>(new Set())
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const [xemTruoc, setXemTruoc] = useState<ChuyenHoGiaoDanXemTruoc | null>(null)
  const [dangXemTruoc, setDangXemTruoc] = useState(false)
  const [dangGhi, setDangGhi] = useState(false)
  const [thongBaoXong, setThongBaoXong] = useState<string | null>(null)

  useEffect(() => {
    setDangTai(true)
    setDaChon(new Set())
    setXemTruoc(null)
    setThongBaoXong(null)
    api.giaoDan.danhSach(giaoHoNguonId || undefined)
      .then(setDanhSach)
      .catch((e: unknown) => { console.error('Không tải được danh sách giáo dân', e) })
      .finally(() => setDangTai(false))
  }, [giaoHoNguonId])

  const soDaChon = daChon.size

  function toggleChon(id: string) {
    setXemTruoc(null)
    setDaChon((s) => {
      const moi = new Set(s)
      if (moi.has(id)) moi.delete(id); else moi.add(id)
      return moi
    })
  }

  function chonTatCa() {
    setXemTruoc(null)
    setDaChon(new Set(danhSach.map((d) => d.id)))
  }
  function boChonTatCa() {
    setXemTruoc(null)
    setDaChon(new Set())
  }

  async function batDauXemTruoc() {
    setLoi(null)
    setThongBaoXong(null)
    if (giaoHoDichId === '') {
      setLoi('Xin vui lòng chọn giáo họ đích')
      return
    }
    if (giaoHoNguonId !== '' && giaoHoNguonId === giaoHoDichId) {
      setLoi('Xin vui lòng chọn giáo họ đích khác giáo họ nguồn')
      return
    }
    if (soDaChon === 0) {
      setLoi('Xin vui lòng chọn ít nhất 1 giáo dân để chuyển họ')
      return
    }
    setDangXemTruoc(true)
    try {
      const kq = await api.chuyenHo.xemTruocGiaoDan([...daChon], giaoHoDichId)
      setXemTruoc(kq)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Không xem trước được, thử lại sau.')
    } finally {
      setDangXemTruoc(false)
    }
  }

  async function xacNhanChuyen() {
    if (!xemTruoc) return
    setDangGhi(true)
    setLoi(null)
    try {
      const kq = await api.chuyenHo.ghiGiaoDan([...daChon], giaoHoDichId)
      setThongBaoXong(`Đã chuyển ${kq.soLuongDaChuyen} giáo dân sang giáo họ "${xemTruoc.tenGiaoHoDich}".`)
      setXemTruoc(null)
      setDaChon(new Set())
      const ds = await api.giaoDan.danhSach(giaoHoNguonId || undefined)
      setDanhSach(ds)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Chuyển họ thất bại, thử lại sau.')
    } finally {
      setDangGhi(false)
    }
  }

  const tenGiaoHo = useMemo(() => new Map(danhMucGiaoHo.map((g) => [g.id, g.tenGiaoHo])), [danhMucGiaoHo])

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Chuyển họ hàng loạt — giáo dân</h1>
        </div>

        <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 16 }}>
          <div className="field">
            <label htmlFor="chgd-nguon">Giáo họ nguồn</label>
            <select id="chgd-nguon" value={giaoHoNguonId} onChange={(e) => setGiaoHoNguonId(e.target.value)}>
              <option value="">Tất cả</option>
              {danhMucGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
            </select>
          </div>
          <div className="field">
            <label htmlFor="chgd-dich">Giáo họ đích</label>
            <select id="chgd-dich" value={giaoHoDichId} onChange={(e) => { setGiaoHoDichId(e.target.value); setXemTruoc(null) }}>
              <option value="">— Chọn —</option>
              {danhMucGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
            </select>
          </div>
          <button type="button" className="btn" onClick={chonTatCa} disabled={danhSach.length === 0}>Chọn tất cả</button>
          <button type="button" className="btn" onClick={boChonTatCa} disabled={soDaChon === 0}>Bỏ chọn tất cả</button>
          <div className="spacer" />
          <span className="count-pill"><b>{soDaChon}</b> / {danhSach.length} đã chọn</span>
        </div>
        {loi && <p className="hint" role="alert">{loi}</p>}
        {thongBaoXong && <p className="hint" style={{ color: 'var(--mint-ink)' }}>{thongBaoXong}</p>}
      </div>

      {dangTai ? (
        <p className="muted" style={{ padding: 16 }}>Đang tải danh sách giáo dân…</p>
      ) : (
        <div className="bang-chon-cuon glass">
          <table className="bang-chon">
            <thead>
              <tr>
                <th style={{ width: 40 }} />
                <th>Mã GD</th>
                <th>Họ tên</th>
                <th>Giáo họ hiện tại</th>
              </tr>
            </thead>
            <tbody>
              {danhSach.map((d) => (
                <tr key={d.id}>
                  <td>
                    <input
                      type="checkbox"
                      checked={daChon.has(d.id)}
                      onChange={() => toggleChon(d.id)}
                      aria-label={`Chọn ${d.hoTen}`}
                    />
                  </td>
                  <td>{d.maGiaoDanCu}</td>
                  <td>{[d.tenThanh, d.hoTen].filter(Boolean).join(' ')}</td>
                  <td>{d.tenGiaoHo ?? tenGiaoHo.get(giaoHoNguonId) ?? ''}</td>
                </tr>
              ))}
              {danhSach.length === 0 && (
                <tr><td colSpan={4} className="muted" style={{ textAlign: 'center', padding: 16 }}>Không có giáo dân nào.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <div className="filters-bar glass" style={{ marginTop: 12, justifyContent: 'flex-end', gap: 12 }}>
        {xemTruoc ? (
          <>
            <span>
              Sẽ chuyển <b>{xemTruoc.soLuongGiaoDan}</b> giáo dân sang giáo họ <b>"{xemTruoc.tenGiaoHoDich}"</b>.
              Xác nhận thực hiện?
            </span>
            <button type="button" className="btn" onClick={() => setXemTruoc(null)} disabled={dangGhi}>Huỷ</button>
            <button type="button" className="btn btn-primary" onClick={() => { void xacNhanChuyen() }} disabled={dangGhi}>
              {dangGhi ? 'Đang chuyển…' : 'Xác nhận chuyển'}
            </button>
          </>
        ) : (
          <button type="button" className="btn btn-primary" onClick={() => { void batDauXemTruoc() }} disabled={dangXemTruoc}>
            {dangXemTruoc ? 'Đang xem trước…' : 'Xem trước & chuyển họ'}
          </button>
        )}
      </div>
    </section>
  )
}
