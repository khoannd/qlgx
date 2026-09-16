import { useEffect, useRef, useState } from 'react'
import { Editor, EditorProvider, Toolbar, BtnBold, BtnItalic, BtnUnderline, BtnStrikeThrough,
  BtnBulletList, BtnNumberedList, BtnUndo, BtnRedo, BtnClearFormatting, HtmlButton } from 'react-simple-wysiwyg'
import { api, LoiXungDot } from '../api/client'
import { useAuth } from '../api/AuthContext'
import type {
  MauInDanhSachItem, MauInChiTiet, BienMauIn, CachHienThiDungSaiItem,
} from '../api/types'
import { TrangThaiTai } from '../components/TrangThaiTai'

const NHAN_CAP: Record<MauInDanhSachItem['capDangDung'], string> = {
  MacDinh: 'Mặc định gốc',
  TuyChinhHeThong: 'Đã tuỳ chỉnh (hệ thống)',
  TuyChinhGiaoXu: 'Đã tuỳ chỉnh (riêng giáo xứ)',
}

const MAU_CAP: Record<MauInDanhSachItem['capDangDung'], string> = {
  MacDinh: '#667085',
  TuyChinhHeThong: '#b7791f',
  TuyChinhGiaoXu: '#1d5ddb',
}

function HuyHieuCap({ cap }: { cap: MauInDanhSachItem['capDangDung'] }) {
  return (
    <span style={{
      display: 'inline-block', padding: '2px 8px', borderRadius: 999, fontSize: 11,
      fontWeight: 600, color: '#fff', background: MAU_CAP[cap], whiteSpace: 'nowrap',
    }}>
      {NHAN_CAP[cap]}
    </span>
  )
}

/**
 * Màn hình "Quản lý mẫu in" — NĂNG LỰC MỚI (xem
 * docs/superpowers/specs/man-hinh/quan-ly-mau-in.md), không có ở bản desktop: bản desktop sửa
 * mẫu bằng cách mở Word/Excel chỉnh trực tiếp tệp .doc/.xls trên máy (một máy một giáo xứ), mô
 * hình máy chủ tập trung nhiều giáo xứ không cho phép cách đó.
 *
 * Mọi tài khoản đã đăng nhập xem được danh sách 13 mẫu; chỉ Quản trị viên giáo xứ
 * (loaiTaiKhoan=0) sửa được mẫu RIÊNG của giáo xứ mình, chỉ Quản trị hệ thống (loaiTaiKhoan=9)
 * sửa được mẫu HỆ THỐNG — nút "Sửa" chỉ hiện cho đúng một trong hai nhóm đó, còn lại chỉ xem.
 */
export function MauInListPage() {
  const { nguoiDung } = useAuth()
  const laQuanTri = nguoiDung?.loaiTaiKhoan === 0
  const laQuanTriHeThong = nguoiDung?.loaiTaiKhoan === 9
  const coTheSua = laQuanTri || laQuanTriHeThong
  // loaiTaiKhoan chỉ nhận đúng MỘT trong hai vai trò trên cho một tài khoản — không có tài
  // khoản nào vừa sửa được mẫu riêng vừa sửa được mẫu hệ thống, nên "cấp sửa" xác định thẳng
  // theo loại tài khoản, không cần người dùng tự chọn.
  const capSua: 'rieng' | 'heThong' = laQuanTriHeThong ? 'heThong' : 'rieng'

  const [danhSach, setDanhSach] = useState<MauInDanhSachItem[] | null>(null)
  const [loi, setLoi] = useState<string | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [dangSua, setDangSua] = useState<{ tenMau: string; tenHienThi: string } | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.mauIn.danhSach()
      .then(setDanhSach)
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  return (
    <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', gap: 14 }}>
        <div>
          <h2 style={{ margin: 0 }}>Quản lý mẫu in</h2>
          <p style={{ margin: '4px 0 0', fontSize: 12.5, color: 'var(--muted, #667085)' }}>
            {coTheSua
              ? (laQuanTriHeThong
                ? 'Bạn sửa được mẫu áp dụng chung cho mọi giáo xứ chưa tự tuỳ chỉnh riêng.'
                : 'Bạn sửa được mẫu riêng của giáo xứ mình — luôn ưu tiên hơn mẫu hệ thống.')
              : 'Bạn chỉ xem được danh sách này — cần Quản trị viên giáo xứ để sửa mẫu riêng.'}
          </p>
        </div>

        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th style={{ padding: '6px 4px' }}>Mẫu in</th>
              <th style={{ padding: '6px 4px' }}>Đang dùng</th>
              <th style={{ padding: '6px 4px' }} />
            </tr>
          </thead>
          <tbody>
            {(danhSach ?? []).map((m) => (
              <tr key={m.tenMau} style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
                <td style={{ padding: '6px 4px' }}>{m.tenHienThi}</td>
                <td style={{ padding: '6px 4px' }}><HuyHieuCap cap={m.capDangDung} /></td>
                <td style={{ padding: '6px 4px', textAlign: 'right' }}>
                  {coTheSua && (
                    <button type="button" className="btn btn-quiet btn-sm"
                      onClick={() => setDangSua({ tenMau: m.tenMau, tenHienThi: m.tenHienThi })}>
                      Sửa
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {dangSua && (
          <MauInEditor
            tenMau={dangSua.tenMau}
            tenHienThi={dangSua.tenHienThi}
            capSua={capSua}
            onDong={() => setDangSua(null)}
            onDaLuu={tai}
          />
        )}

        <CachHienThiDungSai capSua={capSua} coTheSua={coTheSua} laQuanTriHeThong={laQuanTriHeThong} />
      </div>
    </TrangThaiTai>
  )
}

/**
 * Khu vực "Cách hiển thị dữ liệu đúng/sai" — cho giáo xứ tự đặt CÂU CHỮ in ra cho các mục
 * đúng/sai thay vì dấu "[x]"/"[  ]" mặc định (xem quan-ly-mau-in.md). Ví dụ người dùng nêu: mục
 * "Tân tòng" thì khi đúng in chữ "Tân tòng", khi sai để trắng.
 *
 * Đặt NGAY TRONG màn hình "Quản lý mẫu in" (không phải một màn hình riêng) vì hai việc luôn đi
 * cùng nhau: người dùng chèn biến {{TanTong}} vào mẫu ở phần trên rồi quyết định nó in ra chữ gì
 * ở phần này. Dùng lại HuyHieuCap và cùng cách phân biệt cấp sửa của phần mẫu in, để người dùng
 * thấy quen thuộc.
 *
 * TỰ TẢI DỮ LIỆU RIÊNG (không dùng chung lượt tải của danh sách mẫu in): hai phần độc lập nhau
 * hoàn toàn, lưu một câu chữ không làm danh sách mẫu in đổi và ngược lại — gộp lại chỉ khiến mỗi
 * lần lưu phải tải lại cả hai.
 */
function CachHienThiDungSai(
  { capSua, coTheSua, laQuanTriHeThong }:
  { capSua: 'rieng' | 'heThong'; coTheSua: boolean; laQuanTriHeThong: boolean },
) {
  const [danhSach, setDanhSach] = useState<CachHienThiDungSaiItem[] | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [loi, setLoi] = useState<string | null>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    api.cachHienThi.danhSach()
      .then(setDanhSach)
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [])

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginTop: 6 }}>
      <div>
        <h3 style={{ margin: 0 }}>Cách hiển thị dữ liệu đúng/sai</h3>
        <p style={{ margin: '4px 0 0', fontSize: 12.5, color: 'var(--muted, #667085)' }}>
          Các mục chỉ có hai trạng thái (có hoặc không) mặc định in ra dấu <code>[x]</code> khi có
          và <code>[  ]</code> khi không. Ở đây bạn đặt lại câu chữ muốn in cho từng mục.
          {' '}<strong>Để trống ô nào thì chỗ đó không in ra chữ gì.</strong>
        </p>
        {/* CỐ Ý viết ví dụ thành MỘT đoạn văn liền, không bọc tên mục trong <em>/<strong>: một
            thẻ chứa đúng văn bản "Tân tòng" sẽ trùng với chính ô nhãn của dòng "Tân tòng" trong
            bảng bên dưới, làm các phép tìm theo văn bản (cả của người dùng dùng Ctrl+F lẫn của
            bài test) không phân biệt được hai chỗ. */}
        <p style={{ margin: '4px 0 0', fontSize: 12.5, color: 'var(--muted, #667085)' }}>
          Ví dụ với mục tân tòng: ô “Khi có” gõ chữ cần in, ô “Khi không” để trống — bản in sẽ hiện
          chữ đó với người tân tòng và không hiện gì với những người còn lại.
        </p>
        {coTheSua && (
          <p style={{ margin: '4px 0 0', fontSize: 12.5, color: 'var(--muted, #667085)' }}>
            {laQuanTriHeThong
              ? 'Bạn đang sửa câu chữ áp dụng chung cho mọi giáo xứ chưa tự đặt riêng.'
              : 'Bạn đang sửa câu chữ riêng của giáo xứ mình — luôn ưu tiên hơn câu chữ hệ thống.'}
          </p>
        )}
      </div>

      <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 12 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid rgba(14,32,76,.13)' }}>
              <th style={{ padding: '6px 4px' }}>Mục</th>
              <th style={{ padding: '6px 4px' }}>Khi có</th>
              <th style={{ padding: '6px 4px' }}>Khi không</th>
              <th style={{ padding: '6px 4px' }}>Đang dùng</th>
              <th style={{ padding: '6px 4px' }} />
            </tr>
          </thead>
          <tbody>
            {(danhSach ?? []).map((m) => (
              <DongCachHienThi
                key={m.tenBien}
                muc={m}
                capSua={capSua}
                coTheSua={coTheSua}
                onDaLuu={tai}
              />
            ))}
          </tbody>
        </table>
      </TrangThaiTai>
    </div>
  )
}

/**
 * Một dòng (một mục đúng/sai). Tách thành component riêng để mỗi dòng tự giữ state hai ô nhập
 * của mình — một state dùng chung cho cả bảng sẽ khiến gõ vào dòng này vẽ lại mọi dòng khác.
 *
 * Hai ô nhập khởi tạo từ câu chữ của ĐÚNG CẤP đang sửa (không phải câu chữ đang áp dụng): quản
 * trị hệ thống mở màn hình phải thấy ô trống khi chính cấp hệ thống chưa đặt gì, dù giáo xứ đã đè
 * riêng — nếu không, bấm Lưu sẽ vô tình sao chép câu chữ của cấp khác sang cấp mình.
 */
function DongCachHienThi(
  { muc, capSua, coTheSua, onDaLuu }:
  { muc: CachHienThiDungSaiItem; capSua: 'rieng' | 'heThong'; coTheSua: boolean; onDaLuu: () => void },
) {
  const capDangSua = capSua === 'rieng' ? muc.rieng : muc.heThong
  const [khiDung, setKhiDung] = useState(capDangSua.khiDung ?? '')
  const [khiSai, setKhiSai] = useState(capDangSua.khiSai ?? '')
  const [dangLuu, setDangLuu] = useState(false)
  const [dangKhoiPhuc, setDangKhoiPhuc] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)

  // Đồng bộ lại hai ô khi dữ liệu máy chủ đổi (sau khi Lưu/Khôi phục làm cha tải lại) — nếu
  // không, ô nhập giữ giá trị cũ và người dùng tưởng lưu chưa ăn.
  useEffect(() => {
    setKhiDung(capDangSua.khiDung ?? '')
    setKhiSai(capDangSua.khiSai ?? '')
  }, [capDangSua.khiDung, capDangSua.khiSai, capDangSua.rowVersion])

  const nhomApi = capSua === 'rieng' ? api.cachHienThi.rieng : api.cachHienThi.heThong

  async function onLuu() {
    setDangLuu(true)
    setThongBao(null)
    try {
      await nhomApi.luu(muc.tenBien, khiDung, khiSai, capDangSua.rowVersion)
      onDaLuu()
    } catch (err) {
      setThongBao(
        err instanceof LoiXungDot ? err.message
          : err instanceof Error ? err.message : String(err),
      )
    } finally {
      setDangLuu(false)
    }
  }

  async function onKhoiPhuc() {
    if (!confirm(`Khôi phục mục "${muc.nhan}" về cách hiển thị mặc định?`)) return
    setDangKhoiPhuc(true)
    setThongBao(null)
    try {
      await nhomApi.khoiPhuc(muc.tenBien)
      onDaLuu()
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : String(err))
    } finally {
      setDangKhoiPhuc(false)
    }
  }

  return (
    <tr style={{ borderBottom: '1px solid rgba(14,32,76,.06)' }}>
      <td style={{ padding: '6px 4px' }}>{muc.nhan}</td>
      <td style={{ padding: '6px 4px' }}>
        {coTheSua ? (
          <input
            aria-label={`Khi có — ${muc.nhan}`}
            value={khiDung}
            maxLength={200}
            onChange={(e) => setKhiDung(e.target.value)}
            style={{ width: '100%', minWidth: 90, fontSize: 12 }}
          />
        ) : (
          <span>{muc.khiDungDangDung}</span>
        )}
      </td>
      <td style={{ padding: '6px 4px' }}>
        {coTheSua ? (
          <input
            aria-label={`Khi không — ${muc.nhan}`}
            value={khiSai}
            maxLength={200}
            onChange={(e) => setKhiSai(e.target.value)}
            style={{ width: '100%', minWidth: 90, fontSize: 12 }}
          />
        ) : (
          <span>{muc.khiSaiDangDung}</span>
        )}
      </td>
      <td style={{ padding: '6px 4px' }}>
        <HuyHieuCap cap={muc.capDangDung} />
        {thongBao && (
          <div style={{ color: 'var(--rose-ink)', fontSize: 11.5, marginTop: 3 }}>{thongBao}</div>
        )}
      </td>
      <td style={{ padding: '6px 4px', textAlign: 'right', whiteSpace: 'nowrap' }}>
        {coTheSua && (
          <>
            <button type="button" className="btn btn-sm" disabled={dangLuu} onClick={onLuu}>
              {dangLuu ? 'Đang lưu…' : 'Lưu'}
            </button>
            {capDangSua.daTuyChinh && (
              <button type="button" className="btn btn-quiet btn-sm" disabled={dangKhoiPhuc}
                onClick={onKhoiPhuc} style={{ marginLeft: 6 }}>
                {dangKhoiPhuc ? 'Đang khôi phục…' : 'Khôi phục mặc định'}
              </button>
            )}
          </>
        )}
      </td>
    </tr>
  )
}

/**
 * Gom biến theo `nhom` để dựng <optgroup> — danh sách "Chèn chỗ trống" nay là toàn bộ
 * `bienKhaDung` (siêu tập của các chỗ trống mẫu gốc, xem quan-ly-mau-in.md), dài tới 80+ mục ở
 * mẫu "Lý lịch cá nhân"; liệt kê phẳng thì quý cha/quý sơ không tìm nổi biến mình cần.
 *
 * GIỮ NGUYÊN thứ tự nhóm THEO LẦN XUẤT HIỆN ĐẦU TIÊN từ máy chủ (không sắp xếp lại theo bảng
 * chữ cái): danh mục phía máy chủ đã xếp theo đúng trình tự đọc của tờ giấy (giáo xứ → giáo dân →
 * bí tích → …), sắp lại sẽ phá trình tự đó.
 */
function gomTheoNhom(bien: BienMauIn[]): [string, BienMauIn[]][] {
  const theoNhom = new Map<string, BienMauIn[]>()
  for (const b of bien) {
    const hienCo = theoNhom.get(b.nhom)
    if (hienCo) hienCo.push(b)
    else theoNhom.set(b.nhom, [b])
  }
  return [...theoNhom.entries()]
}

type EditorProps = {
  tenMau: string
  tenHienThi: string
  capSua: 'rieng' | 'heThong'
  onDong: () => void
  onDaLuu: () => void
}

/** Trình soạn mẫu — tải chi tiết (mẫu gốc nếu chưa ai tuỳ chỉnh), cho sửa bằng trình soạn thảo
 * rich-text, "Xem thử" gọi PDF ngay từ nội dung NHÁP (chưa lưu), "Lưu" và "Khôi phục về mặc
 * định". Tách khỏi MauInListPage để danh sách không phải theo dõi toàn bộ state soạn thảo. */
function MauInEditor({ tenMau, tenHienThi, capSua, onDong, onDaLuu }: EditorProps) {
  const [chiTiet, setChiTiet] = useState<MauInChiTiet | null>(null)
  const [dangTai, setDangTai] = useState(true)
  const [loi, setLoi] = useState<string | null>(null)
  const [html, setHtml] = useState('')
  const [dangLuu, setDangLuu] = useState(false)
  const [dangXemThu, setDangXemThu] = useState(false)
  const [dangKhoiPhuc, setDangKhoiPhuc] = useState(false)
  const [thongBao, setThongBao] = useState<string | null>(null)
  const editorRef = useRef<HTMLDivElement>(null)

  function tai() {
    setDangTai(true)
    setLoi(null)
    const goi = capSua === 'rieng' ? api.mauIn.rieng.layChiTiet(tenMau) : api.mauIn.heThong.layChiTiet(tenMau)
    goi
      .then((ct) => { setChiTiet(ct); setHtml(ct.noiDungHtml) })
      .catch((e: unknown) => setLoi(e instanceof Error ? e.message : String(e)))
      .finally(() => setDangTai(false))
  }

  useEffect(tai, [tenMau, capSua])

  /** Chèn "{{Key}}" vào đúng vị trí con trỏ đang đứng trong vùng soạn thảo — dùng
   * document.execCommand đúng kiến trúc react-simple-wysiwyg đã dùng nội bộ cho mọi nút định
   * dạng (Bold/Italic...), để chèn xong tự bắn sự kiện "input" đồng bộ ngược lại state `html`
   * mà không cần tự ghép chuỗi (dễ chèn sai vị trí nếu con trỏ không ở cuối). */
  function chenChoTrong(e: React.ChangeEvent<HTMLSelectElement>) {
    const key = e.target.value
    e.target.value = ''
    if (!key || !editorRef.current) return
    editorRef.current.focus()
    document.execCommand('insertText', false, `{{${key}}}`)
  }

  async function onXemThu() {
    setDangXemThu(true)
    setThongBao(null)
    try {
      await api.mauIn.xemThu(tenMau, html)
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : String(err))
    } finally {
      setDangXemThu(false)
    }
  }

  async function onLuu() {
    if (!chiTiet) return
    setDangLuu(true)
    setThongBao(null)
    try {
      const goi = capSua === 'rieng'
        ? api.mauIn.rieng.luu(tenMau, html, chiTiet.rowVersion)
        : api.mauIn.heThong.luu(tenMau, html, chiTiet.rowVersion)
      await goi
      setThongBao('Đã lưu.')
      tai()
      onDaLuu()
    } catch (err) {
      setThongBao(
        err instanceof LoiXungDot
          ? err.message
          : err instanceof Error ? err.message : String(err),
      )
    } finally {
      setDangLuu(false)
    }
  }

  async function onKhoiPhuc() {
    if (!confirm(`Khôi phục "${tenHienThi}" về mẫu mặc định gốc? Nội dung tuỳ chỉnh hiện tại sẽ mất.`)) return
    setDangKhoiPhuc(true)
    setThongBao(null)
    try {
      const goi = capSua === 'rieng' ? api.mauIn.rieng.khoiPhuc(tenMau) : api.mauIn.heThong.khoiPhuc(tenMau)
      await goi
      tai()
      onDaLuu()
    } catch (err) {
      setThongBao(err instanceof Error ? err.message : String(err))
    } finally {
      setDangKhoiPhuc(false)
    }
  }

  return (
    <div className="glass" style={{ padding: 16, borderRadius: 'var(--r-card)', display: 'flex', flexDirection: 'column', gap: 10 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3 style={{ margin: 0 }}>Sửa mẫu: {tenHienThi}</h3>
        <button type="button" onClick={onDong}>Đóng</button>
      </div>

      <TrangThaiTai dangTai={dangTai} loi={loi} onThuLai={tai}>
        {chiTiet && (
          <>
            <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
              <label htmlFor={`chen-cho-trong-${tenMau}`} style={{ fontSize: 12 }}>Chèn chỗ trống:</label>
              <select id={`chen-cho-trong-${tenMau}`} defaultValue="" onChange={chenChoTrong} style={{ fontSize: 12 }}>
                <option value="" disabled>— chọn một chỗ trống —</option>
                {gomTheoNhom(chiTiet.bienKhaDung).map(([nhom, bien]) => (
                  <optgroup key={nhom} label={nhom}>
                    {bien.map((b) => (
                      <option key={b.key} value={b.key}>{b.nhan} ({'{{' + b.key + '}}'})</option>
                    ))}
                  </optgroup>
                ))}
              </select>
            </div>

            <EditorProvider>
              <Editor ref={editorRef} value={html}
                onChange={(e) => setHtml(e.target.value)}
                containerProps={{ style: { resize: 'vertical', minHeight: 320, maxHeight: 640, overflow: 'auto' } }}>
                <Toolbar>
                  <BtnUndo /><BtnRedo />
                  <BtnBold /><BtnItalic /><BtnUnderline /><BtnStrikeThrough />
                  <BtnBulletList /><BtnNumberedList />
                  <BtnClearFormatting />
                  <HtmlButton />
                </Toolbar>
              </Editor>
            </EditorProvider>

            {thongBao && <div style={{ color: 'var(--rose-ink)', fontSize: 12.5 }}>{thongBao}</div>}

            <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
              <button type="button" className="btn" disabled={dangXemThu} onClick={onXemThu}>
                {dangXemThu ? 'Đang dựng PDF…' : 'Xem thử'}
              </button>
              <button type="button" className="btn" disabled={dangLuu} onClick={onLuu}>
                {dangLuu ? 'Đang lưu…' : 'Lưu'}
              </button>
              {chiTiet.daTuyChinh && (
                <button type="button" className="btn btn-quiet" disabled={dangKhoiPhuc} onClick={onKhoiPhuc}>
                  {dangKhoiPhuc ? 'Đang khôi phục…' : 'Khôi phục về mặc định'}
                </button>
              )}
            </div>
          </>
        )}
      </TrangThaiTai>
    </div>
  )
}
