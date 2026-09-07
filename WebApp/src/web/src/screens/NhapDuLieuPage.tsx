import { useEffect, useRef, useState, type CSSProperties } from 'react'
import { api } from '../api/client'
import type { BaoCaoXemTruoc, GiaoXuQuanLy, TrangThaiNhapDuLieu } from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'

const oFieldCot: CSSProperties = { flexDirection: 'column', alignItems: 'stretch' }
const MS_HOI_LAI = 1500

/**
 * Màn hình "Nhập dữ liệu Access" (VIEC-TIEP-THEO.md mục 2.4, CHỈ tài khoản "Quản trị hệ
 * thống" thấy được — xem SideNav.tsx). Đây là NỬA SAU của kiến trúc hai bước (xem
 * NhapDuLieuService.cs phía backend): máy chủ web KHÔNG đọc được file .mdb (ACE OLEDB chỉ chạy
 * Windows) nên KHÔNG nhận file .mdb ở đây — quản trị viên phải tự chạy công cụ
 * <code>Qlgx.Migration &lt;file.mdb&gt; --xuat-goi=goi.json.gz</code> tại máy Windows của mình
 * TRƯỚC, rồi tải đúng GÓI đã xuất (.json.gz) lên màn hình này.
 *
 * Ba bước bắt buộc theo đúng thứ tự: (1) chọn giáo xứ ĐÍCH + chọn gói, (2) "Chạy thử" xem báo
 * cáo đối chiếu số dòng — KHÔNG ghi gì, (3) tích xác nhận rồi "Nhập thật" — chạy NỀN phía máy
 * chủ, màn hình tự hỏi lại tiến độ (poll) mỗi {MS_HOI_LAI}ms tới khi xong.
 */
export function NhapDuLieuPage() {
  const [giaoXu, setGiaoXu] = useState<GiaoXuQuanLy[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)

  const [giaoXuDichId, setGiaoXuDichId] = useState('')
  const [tep, setTep] = useState<File | null>(null)
  const [xacNhanGhiDe, setXacNhanGhiDe] = useState(false)

  const [dangChayThu, setDangChayThu] = useState(false)
  const [baoCaoThu, setBaoCaoThu] = useState<BaoCaoXemTruoc | null>(null)
  const [loiThaoTac, setLoiThaoTac] = useState<string | null>(null)

  const [dangBatDau, setDangBatDau] = useState(false)
  const [trangThaiNhap, setTrangThaiNhap] = useState<TrangThaiNhapDuLieu | null>(null)
  const hoLaiRef = useRef<ReturnType<typeof setInterval> | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.quanTri.giaoXu.danhSach()
      .then((ds) => { setGiaoXu(ds); if (ds.length > 0 && !giaoXuDichId) setGiaoXuDichId(ds[0]!.id) })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, []) // eslint-disable-line react-hooks/exhaustive-deps

  // Dừng vòng hỏi lại tiến độ khi rời màn hình — tránh gọi API sau khi component đã unmount.
  useEffect(() => () => { if (hoLaiRef.current) clearInterval(hoLaiRef.current) }, [])

  function chonTepMoi(f: File | null) {
    setTep(f)
    setBaoCaoThu(null)
    setTrangThaiNhap(null)
    setLoiThaoTac(null)
    setXacNhanGhiDe(false)
  }

  async function chayThu() {
    if (!tep || !giaoXuDichId) return
    setDangChayThu(true); setLoiThaoTac(null); setBaoCaoThu(null); setTrangThaiNhap(null)
    try {
      setBaoCaoThu(await api.quanTri.nhapDuLieu.xemTruoc(giaoXuDichId, tep))
    } catch (err) { setLoiThaoTac(err instanceof Error ? err.message : String(err)) }
    finally { setDangChayThu(false) }
  }

  function hoiTrangThai(jobId: string) {
    hoLaiRef.current = setInterval(() => {
      api.quanTri.nhapDuLieu.trangThai(jobId)
        .then((tt) => {
          setTrangThaiNhap(tt)
          if (tt.trangThai !== 'DangChay' && hoLaiRef.current) {
            clearInterval(hoLaiRef.current); hoLaiRef.current = null
          }
        })
        .catch((e: unknown) => {
          setLoiThaoTac(e instanceof Error ? e.message : String(e))
          if (hoLaiRef.current) { clearInterval(hoLaiRef.current); hoLaiRef.current = null }
        })
    }, MS_HOI_LAI)
  }

  async function batDauNhap() {
    if (!tep || !giaoXuDichId) return
    setDangBatDau(true); setLoiThaoTac(null); setTrangThaiNhap(null)
    try {
      const { jobId } = await api.quanTri.nhapDuLieu.batDau(giaoXuDichId, tep, xacNhanGhiDe)
      setTrangThaiNhap({
        jobId, trangThai: 'DangChay', doiChieu: null, canhBao: null, loiThongBao: null,
        batDauLuc: new Date().toISOString(), ketThucLuc: null,
      })
      hoiTrangThai(jobId)
    } catch (err) { setLoiThaoTac(err instanceof Error ? err.message : String(err)) }
    finally { setDangBatDau(false) }
  }

  const giaoXuChon = (giaoXu ?? []).find((x) => x.id === giaoXuDichId) ?? null
  const dangChayNen = trangThaiNhap?.trangThai === 'DangChay'

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 20, maxWidth: 720 }}>
        <p style={{ margin: 0, fontSize: 12.5, color: 'var(--muted, #5b6a86)' }}>
          Nhập dữ liệu một giáo xứ mới từ file Access cũ (.mdb). Máy chủ KHÔNG đọc trực tiếp file
          .mdb — trước tiên hãy chạy công cụ trên máy Windows của bạn để rút ra một GÓI dữ liệu:
          {' '}
          <code>Qlgx.Migration &lt;file.mdb&gt; --xuat-goi=goi.json.gz</code>. Sau đó tải đúng gói
          <code>.json.gz</code> đó lên bên dưới.
        </p>

        <section className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 12 }}>
          <label className="field" style={oFieldCot}>Giáo xứ đích
            <select value={giaoXuDichId} onChange={(e) => { setGiaoXuDichId(e.target.value); setBaoCaoThu(null); setTrangThaiNhap(null) }}>
              {(giaoXu ?? []).map((x) => <option key={x.id} value={x.id}>{x.tenGiaoXu}{x.soTaiKhoan > 0 ? ` (đã có ${x.soTaiKhoan} tài khoản)` : ''}</option>)}
            </select>
          </label>
          {giaoXuChon && (
            <p style={{ margin: 0, fontSize: 12, color: 'var(--muted, #5b6a86)' }}>
              Chưa có giáo xứ đích? Vào "Quản lý giáo xứ" tạo giáo xứ mới trước.
            </p>
          )}

          <label className="field" style={oFieldCot}>Gói dữ liệu (.json.gz)
            <input type="file" accept=".gz,.json.gz" data-testid="chon-tep-goi"
              onChange={(e) => chonTepMoi(e.target.files?.[0] ?? null)} />
          </label>

          {loiThaoTac && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{loiThaoTac}</div>}

          <div style={{ display: 'flex', gap: 8 }}>
            <button type="button" className="btn" disabled={!tep || !giaoXuDichId || dangChayThu || dangChayNen}
              onClick={chayThu}>
              {dangChayThu ? 'Đang chạy thử…' : '1. Chạy thử (không ghi gì)'}
            </button>
          </div>
        </section>

        {baoCaoThu && (
          <section className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10 }}>
            <h3 style={{ margin: 0 }}>Báo cáo chạy thử — nguồn: {baoCaoThu.tenGiaoXuNguon}</h3>
            {baoCaoThu.giaoXuDichDaCoDuLieu && (
              <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>
                ⚠ Giáo xứ đích ĐÃ có {baoCaoThu.soGiaoDanDaCo} giáo dân. Nhập tiếp vào đây có thể
                trộn lẫn dữ liệu hai giáo xứ khác nhau — kiểm tra thật kỹ trước khi xác nhận.
              </div>
            )}
            <BangDoiChieu dong={baoCaoThu.doiChieu} />
            {baoCaoThu.canhBao.length > 0 && <CanhBaoList canhBao={baoCaoThu.canhBao} />}

            <label style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 12.5 }}>
              <input type="checkbox" checked={xacNhanGhiDe} onChange={(e) => setXacNhanGhiDe(e.target.checked)} />
              Tôi đã xem báo cáo chạy thử ở trên và xác nhận nhập THẬT vào giáo xứ
              "{giaoXuChon?.tenGiaoXu}"
              {baoCaoThu.giaoXuDichDaCoDuLieu ? ' (giáo xứ này ĐÃ có dữ liệu — vẫn muốn tiếp tục)' : ''}.
            </label>
            <div>
              <button type="button" className="btn" disabled={!xacNhanGhiDe || dangBatDau || dangChayNen}
                onClick={batDauNhap}>
                {dangBatDau ? 'Đang bắt đầu…' : '2. Nhập thật'}
              </button>
            </div>
          </section>
        )}

        {trangThaiNhap && (
          <section className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10 }}>
            <h3 style={{ margin: 0 }}>
              {trangThaiNhap.trangThai === 'DangChay' && 'Đang nhập dữ liệu… (chạy nền, đừng đóng tab)'}
              {trangThaiNhap.trangThai === 'HoanThanh' && 'Đã nhập xong'}
              {trangThaiNhap.trangThai === 'Loi' && 'Nhập thất bại'}
            </h3>
            {trangThaiNhap.trangThai === 'Loi' && (
              <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{trangThaiNhap.loiThongBao}</div>
            )}
            {trangThaiNhap.doiChieu && <BangDoiChieu dong={trangThaiNhap.doiChieu} />}
            {trangThaiNhap.canhBao && trangThaiNhap.canhBao.length > 0 && <CanhBaoList canhBao={trangThaiNhap.canhBao} />}
            {trangThaiNhap.trangThai === 'HoanThanh' && trangThaiNhap.doiChieu?.every((d) => !d.lech) && (
              <p style={{ margin: 0, fontSize: 12.5 }}>Mọi bảng đã khớp tuyệt đối số dòng nguồn/đích.</p>
            )}
          </section>
        )}
      </div>
    </TrangThaiTai>
  )
}

function BangDoiChieu({ dong }: { dong: { bang: string; soDongNguon: number; soDongDich: number; lech: boolean }[] }) {
  return (
    <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
      <thead><tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
        <th>Bảng</th><th>Nguồn</th><th>Đích</th><th /></tr></thead>
      <tbody>
        {dong.map((d) => (
          <tr key={d.bang} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
            <td>{d.bang}</td><td>{d.soDongNguon}</td><td>{d.soDongDich < 0 ? '—' : d.soDongDich}</td>
            <td>{d.lech && <span style={{ color: 'var(--rose-ink)' }}>⚠ lệch</span>}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}

function CanhBaoList({ canhBao }: { canhBao: string[] }) {
  return (
    <details>
      <summary style={{ fontSize: 12.5, cursor: 'pointer' }}>{canhBao.length} cảnh báo dữ liệu</summary>
      <ul style={{ fontSize: 11.5, margin: '6px 0 0', paddingLeft: 18 }}>
        {canhBao.slice(0, 50).map((c, i) => <li key={i}>{c}</li>)}
      </ul>
    </details>
  )
}
