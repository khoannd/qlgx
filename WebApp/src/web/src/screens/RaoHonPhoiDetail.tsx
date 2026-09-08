import { useEffect, useState } from 'react'
import { api, LoiXungDot } from '../api/client'
import type { GiaoDanTimKiem, RaoHonPhoiDetail as RaoHonPhoiDetailType } from '../api/types'
import { GxDate } from '../components/GxDate'
import { GxField, GxInline } from '../components/GxField'
import { GxPicker } from '../components/GxPicker'
import { TrangThaiTai } from '../components/TrangThaiTai'

type Props = {
  id: string | null
  onTieuDe?: (ten: string) => void
  onDaLuu?: () => void
  /** Nút "+" của `GxPicker` "Người thứ nhất"/"Người thứ hai" — mở một thẻ "Giáo dân mới" TÁCH
   * BIỆT, tạo xong tự đóng lại và điền ngược vào ô đang chọn — cùng cơ chế đã dùng ở
   * `GiaDinhDetail`, xem `GxPicker.tsx`, `App.moChiTietGiaoDan`, can-review-sau.md mục 19. */
  moGiaoDanMoiChoPicker?: (onTaoXong: (gd: GiaoDanTimKiem) => void) => void
}

type Nhap = {
  tenRaoHonPhoi: string
  giaoDan1Id: string | null; tenGiaoDan1: string
  giaoDan2Id: string | null; tenGiaoDan2: string
  ngayRaoLan1: string | null; ngayRaoLan2: string | null; ngayRaoLan3: string | null
  giaoXu1: string; giaoPhan1: string; giaoXuTruoc1: string; giaoPhanTruoc1: string
  giaoXu2: string; giaoPhan2: string; giaoXuTruoc2: string; giaoPhanTruoc2: string
  linhMucNhan: string; giaoXuNhan: string; ghiChu: string
  tam1: string; tam2: string; tam3: string
  giaoXuNQ1: string; giaoPhanNQ1: string; giaoXuNQ2: string; giaoPhanNQ2: string
}

const RONG: Nhap = {
  tenRaoHonPhoi: '', giaoDan1Id: null, tenGiaoDan1: '', giaoDan2Id: null, tenGiaoDan2: '',
  ngayRaoLan1: null, ngayRaoLan2: null, ngayRaoLan3: null,
  giaoXu1: '', giaoPhan1: '', giaoXuTruoc1: '', giaoPhanTruoc1: '',
  giaoXu2: '', giaoPhan2: '', giaoXuTruoc2: '', giaoPhanTruoc2: '',
  linhMucNhan: '', giaoXuNhan: '', ghiChu: '', tam1: '', tam2: '', tam3: '',
  giaoXuNQ1: '', giaoPhanNQ1: '', giaoXuNQ2: '', giaoPhanNQ2: '',
}

function tuChiTiet(d: RaoHonPhoiDetailType): Nhap {
  return {
    tenRaoHonPhoi: d.tenRaoHonPhoi ?? '',
    giaoDan1Id: d.giaoDan1Id, tenGiaoDan1: d.tenGiaoDan1 ?? '',
    giaoDan2Id: d.giaoDan2Id, tenGiaoDan2: d.tenGiaoDan2 ?? '',
    ngayRaoLan1: d.ngayRaoLan1, ngayRaoLan2: d.ngayRaoLan2, ngayRaoLan3: d.ngayRaoLan3,
    giaoXu1: d.giaoXu1 ?? '', giaoPhan1: d.giaoPhan1 ?? '', giaoXuTruoc1: d.giaoXuTruoc1 ?? '', giaoPhanTruoc1: d.giaoPhanTruoc1 ?? '',
    giaoXu2: d.giaoXu2 ?? '', giaoPhan2: d.giaoPhan2 ?? '', giaoXuTruoc2: d.giaoXuTruoc2 ?? '', giaoPhanTruoc2: d.giaoPhanTruoc2 ?? '',
    linhMucNhan: d.linhMucNhan ?? '', giaoXuNhan: d.giaoXuNhan ?? '', ghiChu: d.ghiChu ?? '',
    tam1: d.tam1 ?? '', tam2: d.tam2 ?? '', tam3: d.tam3 ?? '',
    giaoXuNQ1: d.giaoXuNQ1 ?? '', giaoPhanNQ1: d.giaoPhanNQ1 ?? '', giaoXuNQ2: d.giaoXuNQ2 ?? '', giaoPhanNQ2: d.giaoPhanNQ2 ?? '',
  }
}

/**
 * Chi tiết một đôi rao hôn phối — khớp `frmRaoHonPhoi.cs`. Không migrate "In điều tra hôn
 * phối"/"In kết quả rao hôn phối" (usePrint) ở lượt này — xem rao-hon-phoi.md mục 8.
 */
export function RaoHonPhoiDetail({ id, onTieuDe, onDaLuu, moGiaoDanMoiChoPicker }: Props) {
  const [rao, setRao] = useState<RaoHonPhoiDetailType | null>(null)
  const [nhap, setNhap] = useState<Nhap>(RONG)
  const [dangTai, setDangTai] = useState(!!id)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangLuu, setDangLuu] = useState(false)
  const [loiLuu, setLoiLuu] = useState<string | null>(null)

  useEffect(() => {
    if (!id) {
      onTieuDe?.('Đôi rao mới')
      return
    }
    setDangTai(true)
    api.raoHonPhoi.chiTiet(id)
      .then((d) => {
        setRao(d)
        setNhap(tuChiTiet(d))
        onTieuDe?.(d.tenRaoHonPhoi || `Đôi rao #${d.maRaoHonPhoiCu}`)
      })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }, [id, onTieuDe])

  function d<K extends keyof Nhap>(k: K, v: Nhap[K]) { setNhap((n) => ({ ...n, [k]: v })) }

  async function luu() {
    // Khớp gxCommand1_OnOK (frmRaoHonPhoi.cs:97-129) phần không phụ thuộc "In điều tra".
    if (!nhap.giaoDan1Id) { setLoiLuu('Xin vui lòng nhập thông tin người thứ nhất cần rao'); return }
    if (nhap.tenRaoHonPhoi.trim() === '') { setLoiLuu('Xin vui lòng nhập [đôi rao]'); return }
    if (!nhap.giaoDan2Id) { setLoiLuu('Xin vui lòng nhập thông tin người thứ hai cần rao'); return }

    setDangLuu(true)
    setLoiLuu(null)
    const than = {
      tenRaoHonPhoi: nhap.tenRaoHonPhoi, giaoDan1Id: nhap.giaoDan1Id, giaoDan2Id: nhap.giaoDan2Id,
      ngayRaoLan1: nhap.ngayRaoLan1, ngayRaoLan2: nhap.ngayRaoLan2, ngayRaoLan3: nhap.ngayRaoLan3,
      giaoXu1: nhap.giaoXu1 || null, giaoPhan1: nhap.giaoPhan1 || null,
      giaoXuTruoc1: nhap.giaoXuTruoc1 || null, giaoPhanTruoc1: nhap.giaoPhanTruoc1 || null,
      giaoXu2: nhap.giaoXu2 || null, giaoPhan2: nhap.giaoPhan2 || null,
      giaoXuTruoc2: nhap.giaoXuTruoc2 || null, giaoPhanTruoc2: nhap.giaoPhanTruoc2 || null,
      linhMucNhan: nhap.linhMucNhan || null, giaoXuNhan: nhap.giaoXuNhan || null, ghiChu: nhap.ghiChu || null,
      tam1: nhap.tam1 || null, tam2: nhap.tam2 || null, tam3: nhap.tam3 || null,
      giaoXuNQ1: nhap.giaoXuNQ1 || null, giaoPhanNQ1: nhap.giaoPhanNQ1 || null,
      giaoXuNQ2: nhap.giaoXuNQ2 || null, giaoPhanNQ2: nhap.giaoPhanNQ2 || null,
      rowVersion: rao?.rowVersion ?? null,
    }
    try {
      if (!rao) {
        const moi = await api.raoHonPhoi.tao(than)
        setRao(moi)
        setNhap(tuChiTiet(moi))
        onTieuDe?.(moi.tenRaoHonPhoi || `Đôi rao #${moi.maRaoHonPhoiCu}`)
      } else {
        await api.raoHonPhoi.capNhat(rao.id, than)
        const lai = await api.raoHonPhoi.chiTiet(rao.id)
        setRao(lai)
        setNhap(tuChiTiet(lai))
      }
      onDaLuu?.()
    } catch (e) {
      setLoiLuu(e instanceof LoiXungDot ? e.message : e instanceof Error ? e.message : 'Lưu thất bại, thử lại sau.')
    } finally {
      setDangLuu(false)
    }
  }

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={() => id && api.raoHonPhoi.chiTiet(id).then(setRao)}>
      <section className="page" style={{ overflowY: 'auto', display: 'block' }}>
        <div className="page-head">
          <h1>{rao ? (rao.tenRaoHonPhoi || `Đôi rao #${rao.maRaoHonPhoiCu}`) : 'Đôi rao mới'}</h1>
        </div>

        {/* Cùng lý do đổi `.field` (lớp của HÀNG LỌC, `.filters-bar .field` — nhãn co theo chữ,
            không có cột cố định) sang `.frow`/`GxField` như `HoiDoanDetail.tsx` (rà thêm theo
            yêu cầu người dùng 2026-09-08) — xem chú thích dài ở đó. Các `<input>` cũng thiếu
            `type="text"` nên trước đây hiện bằng kiểu mặc định của trình duyệt, không đồng bộ
            với phần còn lại của ứng dụng. */}
        <div className="card glass" style={{ marginBottom: 12 }}>
          <GxField label="Đôi rao" id="rhp-ten">
            <input id="rhp-ten" type="text" value={nhap.tenRaoHonPhoi} onChange={(e) => d('tenRaoHonPhoi', e.target.value)} />
          </GxField>

          <div className="cols-even" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginTop: 10 }}>
            <fieldset>
              <legend>Người thứ nhất</legend>
              <GxField label="Giáo dân" id="rhp-gd1">
                <GxPicker id="rhp-gd1" value={nhap.tenGiaoDan1}
                  onChon={(gd) => setNhap((n) => ({ ...n, giaoDan1Id: gd.id, tenGiaoDan1: (gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen }))}
                  onThemMoi={moGiaoDanMoiChoPicker
                    ? () => moGiaoDanMoiChoPicker((gd) => setNhap((n) => ({ ...n, giaoDan1Id: gd.id, tenGiaoDan1: (gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen })))
                    : undefined}
                  onBoChon={() => setNhap((n) => ({ ...n, giaoDan1Id: null, tenGiaoDan1: '' }))} />
              </GxField>
              <GxField label="Giáo xứ" id="rhp-gx1">
                <input id="rhp-gx1" type="text" value={nhap.giaoXu1} onChange={(e) => d('giaoXu1', e.target.value)} />
                <GxInline>Giáo phận</GxInline>
                <input aria-label="Giáo phận (người thứ nhất)" type="text" value={nhap.giaoPhan1} onChange={(e) => d('giaoPhan1', e.target.value)} />
              </GxField>
              <GxField label="Xứ trước" id="rhp-xt1">
                <input id="rhp-xt1" type="text" value={nhap.giaoXuTruoc1} onChange={(e) => d('giaoXuTruoc1', e.target.value)} />
                <GxInline>Giáo phận trước</GxInline>
                <input aria-label="Giáo phận trước (người thứ nhất)" type="text" value={nhap.giaoPhanTruoc1} onChange={(e) => d('giaoPhanTruoc1', e.target.value)} />
              </GxField>
              <GxField label="Ghi chú khác 1" id="rhp-gc1a">
                <input id="rhp-gc1a" type="text" value={nhap.giaoXuNQ1} onChange={(e) => d('giaoXuNQ1', e.target.value)} />
                <GxInline>Ghi chú khác 1b</GxInline>
                <input aria-label="Ghi chú khác 1b" type="text" value={nhap.giaoPhanNQ1} onChange={(e) => d('giaoPhanNQ1', e.target.value)} />
              </GxField>
            </fieldset>
            <fieldset>
              <legend>Người thứ hai</legend>
              <GxField label="Giáo dân" id="rhp-gd2">
                <GxPicker id="rhp-gd2" value={nhap.tenGiaoDan2}
                  onChon={(gd) => setNhap((n) => ({ ...n, giaoDan2Id: gd.id, tenGiaoDan2: (gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen }))}
                  onThemMoi={moGiaoDanMoiChoPicker
                    ? () => moGiaoDanMoiChoPicker((gd) => setNhap((n) => ({ ...n, giaoDan2Id: gd.id, tenGiaoDan2: (gd.tenThanh ? gd.tenThanh + ' ' : '') + gd.hoTen })))
                    : undefined}
                  onBoChon={() => setNhap((n) => ({ ...n, giaoDan2Id: null, tenGiaoDan2: '' }))} />
              </GxField>
              <GxField label="Giáo xứ" id="rhp-gx2">
                <input id="rhp-gx2" type="text" value={nhap.giaoXu2} onChange={(e) => d('giaoXu2', e.target.value)} />
                <GxInline>Giáo phận</GxInline>
                <input aria-label="Giáo phận (người thứ hai)" type="text" value={nhap.giaoPhan2} onChange={(e) => d('giaoPhan2', e.target.value)} />
              </GxField>
              <GxField label="Xứ trước" id="rhp-xt2">
                <input id="rhp-xt2" type="text" value={nhap.giaoXuTruoc2} onChange={(e) => d('giaoXuTruoc2', e.target.value)} />
                <GxInline>Giáo phận trước</GxInline>
                <input aria-label="Giáo phận trước (người thứ hai)" type="text" value={nhap.giaoPhanTruoc2} onChange={(e) => d('giaoPhanTruoc2', e.target.value)} />
              </GxField>
              <GxField label="Ghi chú khác 2" id="rhp-gc2a">
                <input id="rhp-gc2a" type="text" value={nhap.giaoXuNQ2} onChange={(e) => d('giaoXuNQ2', e.target.value)} />
                <GxInline>Ghi chú khác 2b</GxInline>
                <input aria-label="Ghi chú khác 2b" type="text" value={nhap.giaoPhanNQ2} onChange={(e) => d('giaoPhanNQ2', e.target.value)} />
              </GxField>
            </fieldset>
          </div>

          <fieldset style={{ marginTop: 12 }}>
            <legend>Rao</legend>
            <GxField label="Rao lần 1" id="rhp-rao1">
              <GxDate id="rhp-rao1" defaultValue={nhap.ngayRaoLan1} onIsoChange={(v) => d('ngayRaoLan1', v)} style={{ maxWidth: 170 }} />
              <GxInline>Rao lần 2</GxInline>
              <GxDate ariaLabel="Rao lần 2" defaultValue={nhap.ngayRaoLan2} onIsoChange={(v) => d('ngayRaoLan2', v)} style={{ maxWidth: 170 }} />
              <GxInline>Rao lần 3</GxInline>
              <GxDate ariaLabel="Rao lần 3" defaultValue={nhap.ngayRaoLan3} onIsoChange={(v) => d('ngayRaoLan3', v)} style={{ maxWidth: 170, marginLeft: 'auto' }} />
            </GxField>
          </fieldset>

          <fieldset style={{ marginTop: 12 }}>
            <legend>Điều tra / Ghi chú</legend>
            <GxField label="Cha nhận điều tra" id="rhp-linhmuc">
              <input id="rhp-linhmuc" type="text" value={nhap.linhMucNhan} onChange={(e) => d('linhMucNhan', e.target.value)} />
              <GxInline>Giáo xứ nhận</GxInline>
              <input aria-label="Giáo xứ nhận" type="text" value={nhap.giaoXuNhan} onChange={(e) => d('giaoXuNhan', e.target.value)} />
            </GxField>
            <GxField label="Ghi chú" id="rhp-ghichu">
              <input id="rhp-ghichu" type="text" value={nhap.ghiChu} onChange={(e) => d('ghiChu', e.target.value)} />
            </GxField>
            <GxField label="Tạm 1" id="rhp-tam1">
              <input id="rhp-tam1" type="text" value={nhap.tam1} onChange={(e) => d('tam1', e.target.value)} />
              <GxInline>Tạm 2</GxInline>
              <input aria-label="Tạm 2" type="text" value={nhap.tam2} onChange={(e) => d('tam2', e.target.value)} />
              <GxInline>Tạm 3</GxInline>
              <input aria-label="Tạm 3" type="text" value={nhap.tam3} onChange={(e) => d('tam3', e.target.value)} />
            </GxField>
          </fieldset>

          {loiLuu && <p className="hint" role="alert">{loiLuu}</p>}
          <div className="cmdbar">
            <div className="spacer" />
            <button type="button" className="btn btn-primary" disabled={dangLuu} onClick={() => { void luu() }}>
              {dangLuu ? 'Đang lưu…' : 'Cập nhật'}
            </button>
          </div>
        </div>
      </section>
    </TrangThaiTai>
  )
}
