import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { BangTimThayThe } from '../api/types'

const BANG: { gt: BangTimThayThe; nhan: string }[] = [
  { gt: 1, nhan: 'Gia đình' },
  { gt: 0, nhan: 'Giáo dân' },
]

// Đúng danh sách + đúng nhãn hiển thị của combo desktop (frmReplace.cs:47-67) — TRỪ dòng
// "ThuocGiaoXu" bị lặp 2 lần trong mã gốc (dòng 61 và 68), chỉ giữ 1.
const TRUONG_GIAO_DAN: { gt: string; nhan: string }[] = [
  { gt: 'TenThanh', nhan: 'Tên thánh' },
  { gt: 'HoTen', nhan: 'Họ tên' },
  { gt: 'HoTenCha', nhan: 'Họ tên cha' },
  { gt: 'HoTenMe', nhan: 'Họ tên mẹ' },
  { gt: 'ChaRuaToi', nhan: 'Người ban bí tích rửa tội' },
  { gt: 'ChaRuocLe', nhan: 'Người ban bí tích xưng tội - rước lễ lần đầu' },
  { gt: 'ChaThemSuc', nhan: 'Người ban bí tích thêm sức' },
  { gt: 'NguoiDoDauRuaToi', nhan: 'Người đỡ đầu rửa tội' },
  { gt: 'NguoiDoDauThemSuc', nhan: 'Người ban đỡ đầu thêm sức' },
  { gt: 'NoiSinh', nhan: 'Nơi sinh' },
  { gt: 'NoiRuaToi', nhan: 'Nơi rửa tội' },
  { gt: 'NoiRuocLe', nhan: 'Nơi xưng tội - rước lễ lần đầu' },
  { gt: 'NoiThemSuc', nhan: 'Nơi thêm sức' },
  { gt: 'ThuocGiaoXu', nhan: 'Thuộc giáo xứ' },
  { gt: 'DienThoai', nhan: 'Điện thoại' },
  { gt: 'DiaChi', nhan: 'Địa chỉ' },
  { gt: 'Email', nhan: 'Email' },
  { gt: 'TrinhDoVanHoa', nhan: 'Trình độ văn hóa' },
  { gt: 'NgheNghiep', nhan: 'Nghề nghiệp' },
  { gt: 'GhiChu', nhan: 'Ghi chú' },
]

const TRUONG_GIA_DINH: { gt: string; nhan: string }[] = [
  { gt: 'TenGiaDinh', nhan: 'Tên gia đình' },
  { gt: 'DiaChi', nhan: 'Địa chỉ' },
  { gt: 'DienThoai', nhan: 'Điện thoại' },
  { gt: 'GhiChu', nhan: 'Ghi chú' },
]

/**
 * "Tìm và thay thế" — thay `frmReplace.cs` (145d, xem docs/superpowers/specs/man-hinh/tim-thay-the.md).
 * CÔNG CỤ SỬA DỮ LIỆU HÀNG LOẠT NGUY HIỂM: thay thế CHÍNH XÁC (không phải tìm-một-phần kiểu
 * "chứa") giá trị của một cột trên một bảng — MỌI bản ghi có giá trị cột đó khớp CHÍNH XÁC được
 * đổi. Bước "Xem trước" bắt buộc, không được bỏ (xem TimThayTheService) — desktop KHÔNG có
 * bước này, chỉ hỏi Yes/No rồi ghi thẳng không biết trước sẽ đổi bao nhiêu dòng.
 */
export function TimThayThePage() {
  const [bang, setBang] = useState<BangTimThayThe>(1)
  const [truong, setTruong] = useState('TenGiaDinh')
  const [giaTriTim, setGiaTriTim] = useState('')
  const [giaTriThay, setGiaTriThay] = useState('')
  const [soKhop, setSoKhop] = useState<number | null>(null)
  const [dangXemTruoc, setDangXemTruoc] = useState(false)
  const [dangGhi, setDangGhi] = useState(false)
  const [loi, setLoi] = useState<string | null>(null)
  const [thongBaoXong, setThongBaoXong] = useState<string | null>(null)

  const danhSachTruong = bang === 1 ? TRUONG_GIA_DINH : TRUONG_GIAO_DAN

  // Đổi bảng thì đổi lại trường về mục đầu tiên của bảng đó (khớp cbForm_SelectedIndexChanged
  // của desktop) và huỷ kết quả xem trước cũ.
  useEffect(() => {
    setTruong(danhSachTruong[0]!.gt)
    setSoKhop(null)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [bang])

  async function batDauXemTruoc() {
    setThongBaoXong(null)
    // Nguyên văn thông báo lỗi desktop (frmReplace.cs:97-101).
    if (giaTriTim.trim() === '') { setLoi('Hãy nhập giá trị cần tìm'); return }
    setLoi(null); setDangXemTruoc(true)
    try {
      const kq = await api.timThayThe.xemTruoc({ bang, truong, giaTriTim, giaTriThay })
      setSoKhop(kq.soBanGhiKhop)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Không xem trước được, thử lại sau.')
    } finally {
      setDangXemTruoc(false)
    }
  }

  async function xacNhanThayThe() {
    setDangGhi(true); setLoi(null)
    try {
      const kq = await api.timThayThe.ghi({ bang, truong, giaTriTim, giaTriThay })
      // Nguyên văn thông báo desktop (frmReplace.cs:127): "Có {0} dữ liệu được thay thế".
      setThongBaoXong(`Có ${kq.soBanGhiDaThay} dữ liệu được thay thế`)
      setSoKhop(null)
    } catch (e) {
      setLoi(e instanceof Error ? e.message : 'Thay thế thất bại, thử lại sau.')
    } finally {
      setDangGhi(false)
    }
  }

  return (
    <section style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 12, maxWidth: 560 }}>
      <p className="muted" style={{ margin: 0, fontSize: 12.5 }}>
        Thay thế CHÍNH XÁC một giá trị bằng giá trị khác trên MỘT cột của MỘT bảng — áp dụng cho
        MỌI bản ghi có giá trị cột đó khớp chính xác với "Giá trị cần tìm" (không phải tìm gần
        đúng/chứa một phần).
      </p>

      <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Áp dụng cho
          <select value={bang} onChange={(e) => setBang(Number(e.target.value) as BangTimThayThe)}>
            {BANG.map((b) => <option key={b.gt} value={b.gt}>{b.nhan}</option>)}
          </select>
        </label>
        <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Trường dữ liệu
          <select value={truong} onChange={(e) => { setTruong(e.target.value); setSoKhop(null) }}>
            {danhSachTruong.map((t) => <option key={t.gt} value={t.gt}>{t.nhan}</option>)}
          </select>
        </label>
      </div>

      <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Giá trị cần tìm
        <input type="text" style={{ fontSize: 12 }} value={giaTriTim}
          onChange={(e) => { setGiaTriTim(e.target.value); setSoKhop(null) }} /></label>
      <label className="field" style={{ flexDirection: 'column', alignItems: 'stretch' }}>Giá trị thay thế
        <input type="text" style={{ fontSize: 12 }} value={giaTriThay}
          onChange={(e) => { setGiaTriThay(e.target.value); setSoKhop(null) }} /></label>

      {loi && <p role="alert" style={{ color: 'var(--rose-ink)', margin: 0 }}>{loi}</p>}
      {thongBaoXong && <p style={{ color: 'var(--mint-ink)', margin: 0 }}>{thongBaoXong}</p>}

      {soKhop === null ? (
        <div>
          <button type="button" className="btn btn-primary" disabled={dangXemTruoc}
            onClick={() => { void batDauXemTruoc() }}>
            {dangXemTruoc ? 'Đang xem trước…' : 'Xem trước'}
          </button>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
          <p style={{ margin: 0 }}>
            Có <b>{soKhop}</b> bản ghi khớp chính xác giá trị "{giaTriTim}". Bạn có chắc muốn
            thay thế các giá trị đã nhập?
          </p>
          <div style={{ display: 'flex', gap: 8 }}>
            <button type="button" className="btn" onClick={() => setSoKhop(null)} disabled={dangGhi}>Huỷ</button>
            <button type="button" className="btn btn-primary" disabled={dangGhi || soKhop === 0}
              onClick={() => { void xacNhanThayThe() }}>
              {dangGhi ? 'Đang thay thế…' : `Xác nhận thay thế ${soKhop} bản ghi`}
            </button>
          </div>
        </div>
      )}
    </section>
  )
}
