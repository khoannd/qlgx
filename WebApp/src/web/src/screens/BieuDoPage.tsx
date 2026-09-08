import { useEffect, useRef, useState } from 'react'
import {
  Chart, BarController, BarElement, LineController, LineElement, PointElement,
  PieController, ArcElement, CategoryScale, LinearScale, Title, Tooltip, Legend,
} from 'chart.js'
import { api } from '../api/client'
import type { BieuDoDoTuoi, BieuDoGiaoHo } from '../api/types'

Chart.register(
  BarController, BarElement, LineController, LineElement, PointElement,
  PieController, ArcElement, CategoryScale, LinearScale, Title, Tooltip, Legend,
)

type LoaiBieuDo = 'TongGiaoDan' | 'BiTich' | 'DoTuoi' | 'GiaoHo' | 'TongHonPhoi'

const NHAN_LOAI: { loai: LoaiBieuDo; nhan: string; canNam: boolean }[] = [
  { loai: 'TongGiaoDan', nhan: 'Tổng giáo dân', canNam: true },
  { loai: 'BiTich', nhan: 'Tình hình bí tích', canNam: true },
  { loai: 'DoTuoi', nhan: 'So sánh độ tuổi (không phụ thuộc năm thống kê)', canNam: false },
  { loai: 'GiaoHo', nhan: 'So sánh Giáo họ (không phụ thuộc năm thống kê)', canNam: false },
  { loai: 'TongHonPhoi', nhan: 'Tổng hôn phối theo năm', canNam: true },
]

const MAU = ['#1d5ddb', '#e11d48', '#14b8a6', '#d97706', '#7c6bf0', '#38bdf8', '#5b4bc4']

/**
 * Màn hình "Biểu đồ" (`frmBieuDo.cs`) — bản desktop xuất Excel kèm biểu đồ dựng bằng Office
 * Interop, KHÔNG dùng được trên máy chủ Linux (ràng buộc đã chốt). Bản web vẽ trực tiếp trong
 * trình duyệt bằng Chart.js (MIT, đã kiểm giấy phép — xem
 * docs/superpowers/specs/man-hinh/thong-ke-bieu-do.md mục 8.1).
 *
 * Khác biệt cố ý: "Từ ngày/Đến ngày" của desktop chỉ dùng phần NĂM (`iDateFrom/10000`,
 * frmBieuDo.cs:88-89) — bản web dùng thẳng hai ô "Từ năm/Đến năm" thay vì hai ô ngày đầy đủ,
 * đơn giản hơn mà không mất thông tin nào (ghi ở can-review-sau.md).
 */
export function BieuDoPage() {
  const [loai, setLoai] = useState<LoaiBieuDo>('TongGiaoDan')
  const [tuNam, setTuNam] = useState(String(new Date().getFullYear() - 5))
  const [denNam, setDenNam] = useState(String(new Date().getFullYear()))
  const [luuTru, setLuuTru] = useState(false)
  const [dangTai, setDangTai] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const chartRef = useRef<Chart | null>(null)
  const [ghiChu, setGhiChu] = useState<string | null>(null)

  useEffect(() => () => chartRef.current?.destroy(), [])

  async function xem() {
    setLoi(null)
    const nhanLoai = NHAN_LOAI.find((n) => n.loai === loai)!
    if (nhanLoai.canNam) {
      const tu = Number(tuNam), den = Number(denNam)
      if (!Number.isFinite(tu) || !Number.isFinite(den)) {
        setLoi('Xin vui lòng nhập Từ năm/Đến năm.')
        return
      }
      if (tu > den) {
        // Trước đây chép nguyên câu từ `ThongKeChungPage.tsx` ("Từ ngày...đến ngày") mà quên đổi
        // tên trường — hai ô trên màn hình này tên là "Từ năm"/"Đến năm" (không phải ô ngày thật,
        // xem chú thích ở đầu file), thông báo cũ nói sai tên khiến người dùng khó hiểu cần sửa ô
        // nào (rà lại theo yêu cầu người dùng 2026-09-08, review toàn nhánh "Trung bình #2").
        setLoi('Từ năm không thể lớn hơn đến năm')
        return
      }
    }
    setDangTai(true)
    setGhiChu(null)
    try {
      chartRef.current?.destroy()
      const ctx = canvasRef.current!.getContext('2d')!
      const tu = Number(tuNam), den = Number(denNam)

      if (loai === 'TongGiaoDan') {
        const ds = await api.thongKe.bieuDo.tongGiaoDan(tu, den, luuTru)
        chartRef.current = new Chart(ctx, {
          type: 'bar',
          data: {
            labels: ds.map((d) => String(d.nam)),
            datasets: [{ label: 'Tổng giáo dân (luỹ kế)', data: ds.map((d) => d.soLuong), backgroundColor: MAU[0] }],
          },
          options: { responsive: true, maintainAspectRatio: false, plugins: { title: { display: true, text: 'Biểu đồ tổng giáo dân' } } },
        })
        setGhiChu('Mỗi cột là tổng giáo dân TÍNH ĐẾN cuối năm đó (luỹ kế), không phải số sinh trong năm — xem thong-ke-bieu-do.md mục 4.6.')
      } else if (loai === 'TongHonPhoi') {
        const ds = await api.thongKe.bieuDo.tongHonPhoi(tu, den)
        chartRef.current = new Chart(ctx, {
          type: 'bar',
          data: {
            labels: ds.map((d) => String(d.nam)),
            datasets: [{ label: 'Số đôi hôn phối', data: ds.map((d) => d.soLuong), backgroundColor: MAU[1] }],
          },
          options: { responsive: true, maintainAspectRatio: false, plugins: { title: { display: true, text: 'Biểu đồ hôn phối' } } },
        })
      } else if (loai === 'BiTich') {
        const ds = await api.thongKe.bieuDo.biTich(tu, den)
        chartRef.current = new Chart(ctx, {
          type: 'bar',
          data: {
            labels: ds.map((d) => String(d.nam)),
            datasets: [
              { label: 'Sinh ra', data: ds.map((d) => d.sinhRa), backgroundColor: MAU[0] },
              { label: 'Rửa tội', data: ds.map((d) => d.ruaToi), backgroundColor: MAU[1] },
              { label: 'XTRL lần đầu', data: ds.map((d) => d.xtrlLanDau), backgroundColor: MAU[2] },
              { label: 'Thêm sức', data: ds.map((d) => d.themSuc), backgroundColor: MAU[3] },
            ],
          },
          options: { responsive: true, maintainAspectRatio: false, plugins: { title: { display: true, text: 'Biểu đồ theo dõi bí tích' } } },
        })
      } else if (loai === 'DoTuoi') {
        const d: BieuDoDoTuoi = await api.thongKe.bieuDo.doTuoi()
        chartRef.current = new Chart(ctx, {
          type: 'line',
          data: {
            labels: ['Dưới 7 tuổi', '7 đến 12', '13 đến 16', '17 đến 25', '26 đến 30', '31 đến 50', 'Trên 50'],
            datasets: [{
              label: 'Số giáo dân', fill: true,
              data: [d.duoi7, d.tu7Den12, d.tu13Den16, d.tu17Den25, d.tu26Den30, d.tu31Den50, d.tren50],
              backgroundColor: 'rgba(29,93,219,.25)', borderColor: MAU[0],
            }],
          },
          options: { responsive: true, maintainAspectRatio: false, plugins: { title: { display: true, text: 'Biểu đồ theo dõi độ tuổi' } } },
        })
        setGhiChu('Nhóm "Trên 50" luôn ra 0 cho tới năm 2041 — lỗi cận tuổi cố định của bản desktop (exportDoTuoi), migrate y hệt. Xem thong-ke-bieu-do.md mục 4.6.')
      } else {
        const ds: BieuDoGiaoHo[] = await api.thongKe.bieuDo.giaoHo()
        const kieuTron = ds.length <= 7
        chartRef.current = new Chart(ctx, {
          type: kieuTron ? 'pie' : 'bar',
          data: {
            labels: ds.map((d) => d.tenGiaoHo),
            datasets: [{
              label: 'Số giáo dân', data: ds.map((d) => d.soLuong),
              backgroundColor: kieuTron ? MAU : MAU[0],
            }],
          },
          options: { responsive: true, maintainAspectRatio: false, plugins: { title: { display: true, text: 'Biểu đồ so sánh giáo họ' } } },
        })
        setGhiChu('Không loại người đã qua đời (khác 4 biểu đồ kia) — đúng công thức exportGiaoHo của bản desktop.')
      }
    } catch (e) {
      setLoi(e instanceof Error ? e.message : String(e))
    } finally {
      setDangTai(false)
    }
  }

  const nhanLoaiHienTai = NHAN_LOAI.find((n) => n.loai === loai)!

  return (
    <section className="page list-page">
      <div className="list-page-head">
        <div className="page-head">
          <h1>Biểu đồ</h1>
        </div>

        <div className="filters-bar glass" style={{ flexWrap: 'wrap', gap: 12 }}>
          <div className="field">
            <label htmlFor="bd-loai">Loại biểu đồ</label>
            <select id="bd-loai" value={loai} onChange={(e) => setLoai(e.target.value as LoaiBieuDo)}>
              {NHAN_LOAI.map((n) => <option key={n.loai} value={n.loai}>{n.nhan}</option>)}
            </select>
          </div>
          {nhanLoaiHienTai.canNam && (
            <>
              <div className="field">
                <label htmlFor="bd-tunam">Từ năm</label>
                <input id="bd-tunam" type="number" value={tuNam} onChange={(e) => setTuNam(e.target.value)} style={{ width: 90 }} />
              </div>
              <div className="field">
                <label htmlFor="bd-dennam">Đến năm</label>
                <input id="bd-dennam" type="number" value={denNam} onChange={(e) => setDenNam(e.target.value)} style={{ width: 90 }} />
              </div>
            </>
          )}
          {loai === 'TongGiaoDan' && (
            <label className="toggle">
              <input type="checkbox" checked={luuTru} onChange={(e) => setLuuTru(e.target.checked)} />
              Tính cả trong hồ sơ lưu trữ
            </label>
          )}
          <button type="button" className="btn btn-primary" onClick={() => { void xem() }} disabled={dangTai}>
            {dangTai ? 'Đang tải…' : 'Xem'}
          </button>
        </div>
        {loi && <p className="hint" role="alert" style={{ padding: '0 4px' }}>{loi}</p>}
      </div>

      <div className="card glass" style={{ padding: 20, maxWidth: 900, height: 460 }}>
        <canvas ref={canvasRef} role="img" aria-label={`Biểu đồ ${nhanLoaiHienTai.nhan}`} />
      </div>
      {ghiChu && <p className="muted" style={{ padding: '8px 4px', maxWidth: 900 }}>{ghiChu}</p>}
    </section>
  )
}
