import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { ChuyenHoGiaDinhXemTruoc, GiaDinhListItem, GiaoHo } from '../api/types'

type Props = {
  danhMucGiaoHo: GiaoHo[]
}

/**
 * "Công cụ dữ liệu" → "Chuyển họ hàng loạt" — nửa GIA ĐÌNH (frmChuyenHoGiaDinh.cs, 245d, xem
 * cong-cu-du-lieu.md mục 4). Chuyển một gia đình kéo theo chuyển TẤT CẢ thành viên (mọi vai
 * trò) sang giáo họ đích — đúng cảnh báo gốc "các thành viên trong các gia đình này cũng sẽ bị
 * chuyển theo" (frmChuyenHoGiaDinh.cs:143). Cùng nguyên tắc an toàn với nửa giáo dân: xem
 * trước lấy số liệu thật → xác nhận có con số cụ thể → ghi trong một transaction.
 */
export function ChuyenHoGiaDinh({ danhMucGiaoHo }: Props) {
  const [giaoHoNguonId, setGiaoHoNguonId] = useState('')
  const [giaoHoDichId, setGiaoHoDichId] = useState('')
  const [danhSach, setDanhSach] = useState<GiaDinhListItem[]>([])
  const [daChon, setDaChon] = useState<Set<string>>(new Set())
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const [xemTruoc, setXemTruoc] = useState<ChuyenHoGiaDinhXemTruoc | null>(null)
  const [dangXemTruoc, setDangXemTruoc] = useState(false)
  const [dangGhi, setDangGhi] = useState(false)
  const [thongBaoXong, setThongBaoXong] = useState<string | null>(null)

  useEffect(() => {
    setDangTai(true)
    setDaChon(new Set())
    setXemTruoc(null)
    setThongBaoXong(null)
    setLoi(null)
    api.giaDinh.danhSach(giaoHoNguonId || undefined)
      .then(setDanhSach)
      .catch((e: unknown) => {
        // Trước đây chỉ `console.error` — nuốt lỗi mạng thành "Không có gia đình nào" (bảng
        // rỗng, danh sách CŨ của giáo họ trước đó vẫn còn hiện, không có dấu hiệu gì báo lỗi).
        // Đúng lớp lỗi đã sửa ở `GxPicker.tsx` — rà lại theo yêu cầu người dùng 2026-09-08
        // (review toàn nhánh, "Cao #2"). `setDanhSach([])` để không giữ lại danh sách CŨ của
        // giáo họ nguồn trước đó (dễ hiểu nhầm là dữ liệu đúng của giáo họ đang lọc, dẫn tới
        // chuyển NHẦM gia đình của giáo họ cũ sang giáo họ đích).
        console.error('Không tải được danh sách gia đình', e)
        setDanhSach([])
        setLoi(e instanceof Error ? e.message : 'Không tải được danh sách gia đình, thử lại sau.')
      })
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
  function chonTatCa() { setXemTruoc(null); setDaChon(new Set(danhSach.map((d) => d.id))) }
  function boChonTatCa() { setXemTruoc(null); setDaChon(new Set()) }

  async function batDauXemTruoc() {
    setLoi(null)
    setThongBaoXong(null)
    if (giaoHoDichId === '') { setLoi('Xin vui lòng chọn giáo họ đích'); return }
    if (giaoHoNguonId !== '' && giaoHoNguonId === giaoHoDichId) {
      setLoi('Xin vui lòng chọn giáo họ đích khác giáo họ nguồn')
      return
    }
    if (soDaChon === 0) { setLoi('Xin vui lòng chọn ít nhất 1 gia đình để chuyển họ'); return }
    setDangXemTruoc(true)
    try {
      const kq = await api.chuyenHo.xemTruocGiaDinh([...daChon], giaoHoDichId)
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
    // QUAN TRỌNG: lệnh GHI (đã thực sự đổi dữ liệu ở máy chủ) và lệnh TẢI LẠI danh sách sau đó
    // PHẢI ở hai `try` riêng — trước đây gộp chung một `try` nên nếu `ghiGiaDinh` thành công
    // nhưng mạng đứt đúng lúc gọi lại `danhSach(...)`, lỗi rơi vào cùng một `catch` khiến
    // "Chuyển họ thất bại..." và "Đã chuyển..." hiện ĐỒNG THỜI — dễ chuyển lại lần nữa những
    // bản ghi ĐÃ chuyển xong (rà lại theo yêu cầu người dùng 2026-09-08, review toàn nhánh
    // "Cao #3"). Khuôn đúng lấy từ `GiaoDanListPage.tsx`.
    let kq
    try {
      kq = await api.chuyenHo.ghiGiaDinh([...daChon], giaoHoDichId)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Chuyển họ thất bại, thử lại sau.')
      setDangGhi(false)
      return
    }
    setThongBaoXong(
      `Đã chuyển ${kq.soLuongGiaDinhDaChuyen} gia đình (${kq.soLuongThanhVienDaChuyen} thành viên) ` +
      `sang giáo họ "${xemTruoc.tenGiaoHoDich}".`,
    )
    setXemTruoc(null)
    setDaChon(new Set())
    setDangGhi(false)
    try {
      const ds = await api.giaDinh.danhSach(giaoHoNguonId || undefined)
      setDanhSach(ds)
    } catch (e) {
      // Chuyển họ đã THÀNH CÔNG ở máy chủ — chỉ việc tải lại danh sách sau đó lỗi. Không đè
      // lên `thongBaoXong` vừa hiện ở trên.
      console.error('Không tải lại được danh sách sau khi chuyển họ (đã chuyển thành công)', e)
    }
  }

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Chuyển họ hàng loạt — gia đình</h1>
        </div>
        <p className="muted" style={{ padding: '0 4px' }}>
          Chuyển họ cho gia đình sẽ chuyển theo TẤT CẢ thành viên trong gia đình đó sang giáo họ đích.
        </p>

        <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 16 }}>
          <div className="field">
            <label htmlFor="chgd2-nguon">Giáo họ nguồn</label>
            <select id="chgd2-nguon" value={giaoHoNguonId} onChange={(e) => setGiaoHoNguonId(e.target.value)}>
              <option value="">Tất cả</option>
              {danhMucGiaoHo.map((g) => <option key={g.id} value={g.id}>{g.tenGiaoHo}</option>)}
            </select>
          </div>
          <div className="field">
            <label htmlFor="chgd2-dich">Giáo họ đích</label>
            <select id="chgd2-dich" value={giaoHoDichId} onChange={(e) => { setGiaoHoDichId(e.target.value); setXemTruoc(null) }}>
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
        <p className="muted" style={{ padding: 16 }}>Đang tải danh sách gia đình…</p>
      ) : (
        <div className="bang-chon-cuon glass">
          <table className="bang-chon">
            <thead>
              <tr>
                <th style={{ width: 40 }} />
                <th>Mã GĐ</th>
                <th>Tên gia đình</th>
                <th>Người nam</th>
                <th>Người nữ</th>
                <th>Số người</th>
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
                      aria-label={`Chọn ${d.tenGiaDinh ?? d.maGiaDinhCu}`}
                    />
                  </td>
                  <td>{d.maGiaDinhCu}</td>
                  <td>{d.tenGiaDinh}</td>
                  <td>{d.tenChong}</td>
                  <td>{d.tenVo}</td>
                  <td>{d.soLuong}</td>
                  <td>{d.tenGiaoHo}</td>
                </tr>
              ))}
              {danhSach.length === 0 && (
                <tr><td colSpan={7} className="muted" style={{ textAlign: 'center', padding: 16 }}>Không có gia đình nào.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <div className="filters-bar glass" style={{ marginTop: 12, justifyContent: 'flex-end', gap: 12 }}>
        {xemTruoc ? (
          <>
            <span>
              Sẽ chuyển <b>{xemTruoc.soLuongGiaDinh}</b> gia đình (<b>{xemTruoc.soLuongThanhVien}</b> thành
              viên) sang giáo họ <b>"{xemTruoc.tenGiaoHoDich}"</b>. Xác nhận thực hiện?
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
