import { useState } from 'react'
import { api } from '../api/client'
import type { LoaiBiTich, TaoDotBiTichXemTruoc } from '../api/types'

const TEN_LOAI: Record<LoaiBiTich, string> = { 0: 'Rửa tội', 1: 'Rước lễ (XTRL lần đầu)', 2: 'Thêm sức' }

/**
 * "Tạo danh sách bí tích tự động" — thay `frmTaoDotBiTich.cs` + `GenerateDotBiTichProcess.cs`
 * (xem docs/superpowers/specs/man-hinh/cong-cu-du-lieu.md mục 5.2). CÔNG CỤ SINH DỮ LIỆU HÀNG
 * LOẠT trên khối lớn nhất CSDL (1108 đợt bí tích, 6150 chi tiết) — chỉ THÊM MỚI, không sửa/xoá
 * bản ghi cũ, nhưng vẫn áp dụng đủ 4 nguyên tắc an toàn: xem trước tách riêng, xác nhận nêu con
 * số cụ thể, ghi trong MỘT transaction (xem TaoDotBiTichTuDongService), không mở rộng phạm vi.
 *
 * CHỈ 3 loại bí tích (Rửa tội/Rước lễ/Thêm sức) — đúng giới hạn đã có sẵn của "Danh sách sổ bí
 * tích" (`LoaiBiTich` chỉ 0|1|2 ở `api/types.ts`), desktop cũng chỉ xử lý đúng 3 loại này dù
 * combo chỉ ẩn "Hôn phối" — bản web ẩn luôn "An táng"/"Xức dầu" để tránh chọn nhầm loại không
 * được hỗ trợ (xem can-review-sau.md mục 63).
 */
export function TaoDotBiTichTuDongPage() {
  const [loaiBiTich, setLoaiBiTich] = useState<LoaiBiTich>(0)
  const [linhMuc, setLinhMuc] = useState('')
  const [noiBiTich, setNoiBiTich] = useState('')
  const [tuNgay, setTuNgay] = useState('')
  const [denNgay, setDenNgay] = useState('')
  const [xemTruoc, setXemTruoc] = useState<TaoDotBiTichXemTruoc | null>(null)
  const [dangXemTruoc, setDangXemTruoc] = useState(false)
  const [dangGhi, setDangGhi] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const [thongBaoXong, setThongBaoXong] = useState<string | null>(null)

  function than() {
    return {
      loaiBiTich, linhMuc: linhMuc.trim() || null, noiBiTich: noiBiTich.trim() || null,
      tuNgay, denNgay,
    }
  }

  // Nguyên văn 2 thông báo lỗi desktop (frmTaoDotBiTich.cs:31-42).
  function kiemTra(): string | null {
    if (tuNgay === '' || denNgay === '') return 'Xin vui lòng chọn khoảng ngày cần tạo tự động.'
    if (tuNgay > denNgay) return 'Từ ngày phải nhỏ hơn hoặc bằng đến ngày.'
    return null
  }

  async function batDauXemTruoc() {
    setThongBaoXong(null)
    const l = kiemTra()
    if (l) { setLoi(l); return }
    setLoi(null); setDangXemTruoc(true)
    try {
      setXemTruoc(await api.taoDotBiTich.xemTruoc(than()))
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Không xem trước được, thử lại sau.')
    } finally {
      setDangXemTruoc(false)
    }
  }

  async function xacNhanTao() {
    setDangGhi(true); setLoi(null)
    try {
      const kq = await api.taoDotBiTich.ghi(than())
      setThongBaoXong(`Tổng số đợt bí tích được tạo: ${kq.soDotDaTao}. Tổng số giáo dân được cho vào sổ bí tích: ${kq.soGiaoDanDaThem}.`)
      setXemTruoc(null)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Tạo thất bại, thử lại sau.')
    } finally {
      setDangGhi(false)
    }
  }

  return (
    <section style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 12, maxWidth: 720 }}>
      <p className="muted" style={{ margin: 0, fontSize: 12.5 }}>
        Tự động gộp giáo dân vào các đợt bí tích theo khoảng ngày — chỉ THÊM MỚI đợt/chi tiết còn
        thiếu, không sửa hay xoá bất kỳ bản ghi bí tích nào đã có sẵn.
      </p>

      <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Loại bí tích
          <select value={loaiBiTich} onChange={(e) => { setLoaiBiTich(Number(e.target.value) as LoaiBiTich); setXemTruoc(null) }}>
            {([0, 1, 2] as LoaiBiTich[]).map((l) => <option key={l} value={l}>{TEN_LOAI[l]}</option>)}
          </select>
        </label>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Linh mục (để trống = tất cả)
          <input type="text" style={{ fontSize: 12 }} value={linhMuc} onChange={(e) => { setLinhMuc(e.target.value); setXemTruoc(null) }} />
        </label>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Nơi bí tích (để trống = tất cả)
          <input type="text" style={{ fontSize: 12 }} value={noiBiTich} onChange={(e) => { setNoiBiTich(e.target.value); setXemTruoc(null) }} />
        </label>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Từ ngày
          <input type="date" value={tuNgay} onChange={(e) => { setTuNgay(e.target.value); setXemTruoc(null) }} />
        </label>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Đến ngày
          <input type="date" value={denNgay} onChange={(e) => { setDenNgay(e.target.value); setXemTruoc(null) }} />
        </label>
      </div>

      {loi && <p role="alert" style={{ color: 'var(--rose-ink)', margin: 0 }}>{loi}</p>}
      {thongBaoXong && <p style={{ color: 'var(--mint-ink)', margin: 0 }}>{thongBaoXong}</p>}

      {!xemTruoc && (
        <div>
          <button type="button" className="btn btn-primary" disabled={dangXemTruoc}
            onClick={() => { void batDauXemTruoc() }}>
            {dangXemTruoc ? 'Đang xem trước…' : 'Xem trước'}
          </button>
        </div>
      )}

      {xemTruoc && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <p style={{ margin: 0 }}>
            Đã kiểm tra <b>{xemTruoc.tongGiaoDanKhopDieuKien}</b> giáo dân khớp điều kiện. Sẽ tạo{' '}
            <b>{xemTruoc.soDotMoiSeTao}</b> đợt bí tích mới và thêm{' '}
            <b>{xemTruoc.soGiaoDanMoiSeThem}</b> giáo dân vào sổ bí tích.
          </p>

          {xemTruoc.mauDotMoi.length > 0 && (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ borderCollapse: 'collapse', fontSize: 12, width: '100%' }}>
                <thead>
                  <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
                    <th>Ngày</th><th>Linh mục</th><th>Nơi bí tích</th><th>Số giáo dân</th>
                  </tr>
                </thead>
                <tbody>
                  {xemTruoc.mauDotMoi.map((d, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                      <td>{d.ngay}</td><td>{d.linhMuc}</td><td>{d.noiBiTich}</td><td>{d.soGiaoDan}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {xemTruoc.soDotMoiSeTao > xemTruoc.mauDotMoi.length && (
                <p className="muted" style={{ fontSize: 12, margin: '4px 0 0' }}>
                  Chỉ hiện {xemTruoc.mauDotMoi.length} đợt mới đầu tiên trong tổng số {xemTruoc.soDotMoiSeTao} đợt sẽ tạo.
                </p>
              )}
            </div>
          )}

          <div style={{ display: 'flex', gap: 8 }}>
            <button type="button" className="btn" onClick={() => setXemTruoc(null)} disabled={dangGhi}>Huỷ</button>
            <button type="button" className="btn btn-primary" disabled={dangGhi}
              onClick={() => { void xacNhanTao() }}>
              {dangGhi ? 'Đang tạo…' : `Xác nhận tạo ${xemTruoc.soDotMoiSeTao} đợt / thêm ${xemTruoc.soGiaoDanMoiSeThem} giáo dân`}
            </button>
          </div>
        </div>
      )}
    </section>
  )
}
